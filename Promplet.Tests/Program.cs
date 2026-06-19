using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;
using Promplet.Models;
using Promplet.Services;
using Promplet.ViewModels;

internal static class Program
{
    private static readonly HashSet<string?> StaticGroupNames =
    [
        "AI Chat",
        "CLI",
        "Mail",
        "Slack",
        "Terminal",
        "Dev",
        "Edit"
    ];

    [STAThread]
    private static int Main()
    {
        var tests = new (string Name, Action Body)[]
        {
            ("default catalog contains four MVP prompts in order", PromptCatalogContainsExpectedButtons),
            ("prompt store creates the default JSON file", PromptStoreCreatesDefaultJsonFile),
            ("prompt store normalizes appearance and hotkey settings", PromptStoreNormalizesAppearanceAndHotKeySettings),
            ("prompt store migrates legacy shifted numpad defaults", PromptStoreMigratesLegacyShiftedNumPadDefaults),
            ("prompt store backs up invalid JSON and recreates defaults", PromptStoreBacksUpInvalidJson),
            ("prompt store backs up invalid JSON shape and recreates defaults", PromptStoreBacksUpInvalidJsonShape),
            ("palette view model switches visible prompt groups", PaletteViewModelSwitchesVisiblePromptGroups),
            ("palette view model reloads groups from a new prompt document", PaletteViewModelReloadsPromptDocument),
            ("palette window state clamps saved placement", PaletteWindowStateClampsSavedPlacement),
            ("icon assets are packaged for Windows shell and tray use", IconAssetsArePackaged),
            ("project uses Promplet icon and Windows Forms tray support", ProjectUsesIconAndWindowsForms),
            ("tray service exposes resident app commands", TrayServiceExposesResidentAppCommands),
            ("global hotkey definitions match the approved shortcuts", GlobalHotKeyDefinitionsMatchApprovedShortcuts),
            ("global hotkey definitions can be built from user settings", GlobalHotKeyDefinitionsCanUseUserSettings),
            ("global hotkey service records registration failures", GlobalHotKeyServiceRecordsRegistrationFailures),
            ("settings dialog exposes appearance and hotkey controls", SettingsDialogExposesAppearanceAndHotkeyControls),
            ("global hotkey service exposes Win32 registration contract", GlobalHotKeyServiceExposesWin32RegistrationContract),
            ("main window wires global hotkey actions", MainWindowWiresGlobalHotKeyActions),
            ("SendInput uses the native Windows INPUT struct size", SendInputStructUsesNativeSize),
            ("main window uses the approved vertical palette contract", MainWindowUsesApprovedVerticalPaletteContract)
        };

        var failed = 0;

        foreach (var (name, body) in tests)
        {
            try
            {
                body();
                Console.WriteLine($"PASS {name}");
            }
            catch (Exception ex)
            {
                failed++;
                Console.Error.WriteLine($"FAIL {name}");
                Console.Error.WriteLine(ex);
            }
        }

        return failed;
    }

    private static void PromptCatalogContainsExpectedButtons()
    {
        var buttons = PromptCatalog.GetDefaultButtons();

        AssertEqual(4, buttons.Count, "button count");
        AssertEqual("summarize", buttons[0].Id, "first id");
        AssertEqual("要約", buttons[0].Label, "first label");
        AssertTrue(buttons[0].Text.EndsWith("\r\n\r\n", StringComparison.Ordinal), "first prompt keeps trailing blank line");
        AssertEqual("rewrite", buttons[1].Id, "second id");
        AssertEqual("review", buttons[2].Id, "third id");
        AssertEqual("explain", buttons[3].Id, "fourth id");
    }

    private static void SendInputStructUsesNativeSize()
    {
        var inputType = Type.GetType("Promplet.Win32.NativeMethods+INPUT, Promplet", throwOnError: true)!;
        var expectedSize = IntPtr.Size == 8 ? 40 : 28;
        var actualSize = Marshal.SizeOf(inputType);

        AssertEqual(expectedSize, actualSize, "Win32 INPUT struct size");
    }

    private static void PromptStoreCreatesDefaultJsonFile()
    {
        using var temp = TemporaryDirectory.Create();
        var store = new PromptStore(temp.Path);

        var document = store.LoadOrCreate();

        AssertTrue(File.Exists(Path.Combine(temp.Path, "prompts.json")), "prompt JSON should be created");
        AssertEqual("ai-chat", document.App.SelectedGroupId, "default selected group");
        AssertEqual(1, document.Groups.Count, "default group count");
        AssertEqual("AI Chat", document.Groups[0].Name, "default group name");
        AssertEqual(4, document.Groups[0].Buttons.Count, "default button count");
        AssertEqual("summarize", document.Groups[0].Buttons[0].Id, "default first button id");
        AssertEqual("要約", document.Groups[0].Buttons[0].Label, "default first button label");
    }

    private static void PromptStoreNormalizesAppearanceAndHotKeySettings()
    {
        using var temp = TemporaryDirectory.Create();
        var document = PromptCatalog.CreateDefaultDocument();
        document.App.ThemeMode = "unknown";
        document.App.Opacity = 2;
        document.App.HotKeys.TogglePalette = new HotKeyGesture
        {
            Control = false,
            Alt = false,
            Shift = false,
            Windows = false,
            Key = "Missing"
        };
        document.App.HotKeys.PasteButtons =
        [
            new HotKeyGesture
            {
                Enabled = true,
                Control = true,
                Shift = true,
                Key = "F7"
            },
            new HotKeyGesture
            {
                Enabled = false,
                Key = "Missing"
            }
        ];

        var store = new PromptStore(temp.Path);
        store.Save(document);
        var reloaded = store.LoadOrCreate();

        AssertEqual("System", reloaded.App.ThemeMode, "invalid theme should normalize to System");
        AssertEqual(1d, reloaded.App.Opacity, "opacity should clamp to full opacity");
        AssertEqual("Space", reloaded.App.HotKeys.TogglePalette.Key, "invalid toggle key should reset");
        AssertTrue(reloaded.App.HotKeys.TogglePalette.Control, "default toggle should use Ctrl");
        AssertEqual(10, reloaded.App.HotKeys.PasteButtons.Count, "paste hotkey count");
        AssertEqual("F7", reloaded.App.HotKeys.PasteButtons[0].Key, "valid custom paste key should remain");
        AssertTrue(!reloaded.App.HotKeys.PasteButtons[1].Enabled, "disabled paste hotkey should stay disabled");
        AssertEqual("NumPad2", reloaded.App.HotKeys.PasteButtons[1].Key, "disabled invalid paste hotkey should keep the default key");
    }

    private static void PromptStoreMigratesLegacyShiftedNumPadDefaults()
    {
        using var temp = TemporaryDirectory.Create();
        var document = PromptCatalog.CreateDefaultDocument();

        for (var index = 0; index < document.App.HotKeys.PasteButtons.Count; index++)
        {
            document.App.HotKeys.PasteButtons[index] = new HotKeyGesture
            {
                Enabled = true,
                Control = true,
                Shift = true,
                Key = $"NumPad{(index + 1) % 10}"
            };
        }

        var store = new PromptStore(temp.Path);
        store.Save(document);
        var reloaded = store.LoadOrCreate();

        foreach (var gesture in reloaded.App.HotKeys.PasteButtons)
        {
            AssertTrue(gesture.Control, "migrated paste hotkey should keep Ctrl");
            AssertTrue(!gesture.Shift, "migrated paste hotkey should remove Shift");
            AssertTrue(!gesture.Alt, "migrated paste hotkey should not add Alt");
            AssertTrue(!gesture.Windows, "migrated paste hotkey should not add Win");
        }
    }

    private static void PromptStoreBacksUpInvalidJson()
    {
        using var temp = TemporaryDirectory.Create();
        var jsonPath = Path.Combine(temp.Path, "prompts.json");
        File.WriteAllText(jsonPath, "{ invalid json", Encoding.UTF8);

        var store = new PromptStore(temp.Path);
        var document = store.LoadOrCreate();

        AssertEqual(1, document.Groups.Count, "recreated default group count");
        AssertEqual(4, document.Groups[0].Buttons.Count, "recreated default button count");
        AssertTrue(Directory.GetFiles(temp.Path, "prompts.json.*.bak").Length == 1, "invalid JSON should be backed up");
        AssertTrue(File.ReadAllText(jsonPath, Encoding.UTF8).Contains("\"groups\"", StringComparison.Ordinal), "prompt JSON should be recreated");
    }

    private static void PromptStoreBacksUpInvalidJsonShape()
    {
        using var temp = TemporaryDirectory.Create();
        var jsonPath = Path.Combine(temp.Path, "prompts.json");
        File.WriteAllText(
            jsonPath,
            """
            {
              "app": null,
              "window": null,
              "groups": [
                {
                  "id": "broken",
                  "name": "Broken",
                  "buttons": null
                }
              ]
            }
            """,
            Encoding.UTF8);

        var store = new PromptStore(temp.Path);
        var document = store.LoadOrCreate();

        AssertEqual("ai-chat", document.App.SelectedGroupId, "invalid shape should recreate selected group");
        AssertEqual(1, document.Groups.Count, "invalid shape should recreate default group count");
        AssertEqual(4, document.Groups[0].Buttons.Count, "invalid shape should recreate default buttons");
        AssertTrue(Directory.GetFiles(temp.Path, "prompts.json.*.bak").Length == 1, "invalid shape should be backed up");
    }

    private static void PaletteViewModelSwitchesVisiblePromptGroups()
    {
        var document = new PromptDocument
        {
            App = new PromptAppSettings
            {
                SelectedGroupId = "ai-chat"
            },
            Groups =
            [
                new PromptGroup
                {
                    Id = "ai-chat",
                    Name = "AI Chat",
                    Buttons =
                    [
                        new PromptButton("summarize", "要約", "AI chat prompt")
                    ]
                },
                new PromptGroup
                {
                    Id = "cli",
                    Name = "CLI",
                    Buttons =
                    [
                        new PromptButton("command", "コマンド", "CLI prompt")
                    ]
                }
            ]
        };

        var viewModel = new PaletteViewModel(document);

        AssertEqual("ai-chat", viewModel.SelectedGroup?.Id, "initial selected group");
        AssertEqual(1, viewModel.VisibleButtons.Count, "initial visible button count");
        AssertEqual("要約", viewModel.VisibleButtons[0].Label, "initial visible button label");

        viewModel.SelectGroup(viewModel.Groups.Single(group => group.Id == "cli"));

        AssertEqual("cli", viewModel.SelectedGroup?.Id, "selected group after switch");
        AssertEqual("cli", document.App.SelectedGroupId, "document selected group after switch");
        AssertEqual(1, viewModel.VisibleButtons.Count, "switched visible button count");
        AssertEqual("コマンド", viewModel.VisibleButtons[0].Label, "switched visible button label");
        AssertTrue(viewModel.Groups.Single(group => group.Id == "cli").IsSelected, "selected tab should be marked selected");
        AssertTrue(!viewModel.Groups.Single(group => group.Id == "ai-chat").IsSelected, "previous tab should be unselected");
    }

    private static void PaletteViewModelReloadsPromptDocument()
    {
        var viewModel = new PaletteViewModel(new PromptDocument
        {
            App = new PromptAppSettings
            {
                SelectedGroupId = "old"
            },
            Groups =
            [
                new PromptGroup
                {
                    Id = "old",
                    Name = "Old",
                    Buttons =
                    [
                        new PromptButton("old-button", "古い", "old prompt")
                    ]
                }
            ]
        });

        var reloaded = new PromptDocument
        {
            App = new PromptAppSettings
            {
                SelectedGroupId = "mail"
            },
            Groups =
            [
                new PromptGroup
                {
                    Id = "mail",
                    Name = "Mail",
                    Buttons =
                    [
                        new PromptButton("reply", "返信", "mail prompt")
                    ]
                }
            ]
        };

        viewModel.LoadDocument(reloaded);

        AssertEqual(1, viewModel.Groups.Count, "reloaded group count");
        AssertEqual("mail", viewModel.SelectedGroup?.Id, "reloaded selected group");
        AssertEqual("返信", viewModel.VisibleButtons.Single().Label, "reloaded visible button");
        AssertEqual("mail", reloaded.App.SelectedGroupId, "reloaded document keeps selected group");
    }

    private static void PaletteWindowStateClampsSavedPlacement()
    {
        var offScreen = PaletteWindowState.Normalize(
            new PromptWindowState
            {
                Left = -5000,
                Top = -5000,
                Width = 2000
            },
            screenLeft: 0,
            screenTop: 0,
            screenWidth: 1920,
            screenHeight: 1080);

        AssertEqual<double?>(null, offScreen.Left, "off-screen left should be cleared");
        AssertEqual<double?>(null, offScreen.Top, "off-screen top should be cleared");
        AssertEqual(1200d, offScreen.Width, "wide saved width should be clamped");

        var visible = PaletteWindowState.Normalize(
            new PromptWindowState
            {
                Left = 100,
                Top = 80,
                Width = 500
            },
            screenLeft: 0,
            screenTop: 0,
            screenWidth: 1920,
            screenHeight: 1080);

        AssertEqual<double?>(100d, visible.Left, "visible left should be retained");
        AssertEqual<double?>(80d, visible.Top, "visible top should be retained");
        AssertEqual(500d, visible.Width, "visible width should be retained");
    }

    private static void IconAssetsArePackaged()
    {
        var svgPath = FindRepositoryFile("Promplet", "Assets", "promplet_icon.svg");
        var pngPath = FindRepositoryFile("Promplet", "Assets", "promplet_icon.png");
        var icoPath = FindRepositoryFile("Promplet", "Assets", "promplet_icon.ico");

        AssertTrue(new FileInfo(svgPath).Length > 500, "SVG icon asset should not be empty");
        AssertTrue(new FileInfo(pngPath).Length > 1000, "PNG icon asset should not be empty");

        var icoBytes = File.ReadAllBytes(icoPath);
        AssertTrue(icoBytes.Length > 1000, "ICO icon asset should not be empty");
        AssertEqual((byte)0x00, icoBytes[0], "ICO reserved byte 0");
        AssertEqual((byte)0x00, icoBytes[1], "ICO reserved byte 1");
        AssertEqual((byte)0x01, icoBytes[2], "ICO image type low byte");
        AssertEqual((byte)0x00, icoBytes[3], "ICO image type high byte");
        AssertTrue(BitConverter.ToUInt16(icoBytes, 4) >= 1, "ICO should contain at least one image");
    }

    private static void ProjectUsesIconAndWindowsForms()
    {
        var project = XDocument.Load(FindRepositoryFile("Promplet", "Promplet.csproj"));
        var properties = project.Root?.Elements("PropertyGroup").Elements().ToDictionary(element => element.Name.LocalName, element => element.Value)
            ?? throw new InvalidOperationException("Promplet.csproj has no properties.");

        AssertEqual("true", properties["UseWindowsForms"], "WinForms support for NotifyIcon");
        AssertEqual(@"Assets\promplet_icon.ico", properties["ApplicationIcon"], "application icon path");

        var iconContent = project.Descendants("Content")
            .SingleOrDefault(element => AttributeValueOrDefault(element, "Include") == @"Assets\promplet_icon.ico")
            ?? throw new InvalidOperationException("promplet_icon.ico should be copied to the build output for NotifyIcon.");
        AssertEqual("PreserveNewest", iconContent.Elements("CopyToOutputDirectory").Single().Value, "tray icon copy behavior");
    }

    private static void TrayServiceExposesResidentAppCommands()
    {
        var source = File.ReadAllText(FindRepositoryFile("Promplet", "Services", "TrayIconService.cs"), Encoding.UTF8);

        AssertTrue(source.Contains("Show / hide Promplet", StringComparison.Ordinal), "tray menu should toggle the palette");
        AssertTrue(!source.Contains("\"Show Promplet\"", StringComparison.Ordinal), "tray menu should not expose separate show");
        AssertTrue(!source.Contains("\"Hide Promplet\"", StringComparison.Ordinal), "tray menu should not expose separate hide");
        AssertTrue(source.Contains("Settings...", StringComparison.Ordinal), "tray menu should open settings");
        AssertTrue(source.Contains("Reload prompts", StringComparison.Ordinal), "tray menu should reload JSON prompts");
        AssertTrue(source.Contains("Exit", StringComparison.Ordinal), "tray menu should explicitly exit");
        AssertTrue(source.Contains("Dispose()", StringComparison.Ordinal), "tray icon should be disposable");
    }

    private static void GlobalHotKeyDefinitionsMatchApprovedShortcuts()
    {
        var hotkeys = GlobalHotKeyDefinitions.CreateDefault().ToList();

        AssertEqual(11, hotkeys.Count, "default hotkey count");

        var toggle = hotkeys.Single(hotkey => hotkey.Action.Kind == GlobalHotKeyActionKind.TogglePalette);
        AssertEqual("Ctrl+Alt+Space", toggle.DisplayText, "toggle display text");
        AssertEqual(GlobalHotKeyModifiers.Control | GlobalHotKeyModifiers.Alt | GlobalHotKeyModifiers.NoRepeat, toggle.Modifiers, "toggle modifiers");
        AssertEqual(GlobalHotKeyVirtualKeys.Space, toggle.VirtualKey, "toggle virtual key");

        var pasteHotkeys = hotkeys.Where(hotkey => hotkey.Action.Kind == GlobalHotKeyActionKind.PasteVisibleButton).ToList();
        var expectedKeys = new[]
        {
            GlobalHotKeyVirtualKeys.NumPad1,
            GlobalHotKeyVirtualKeys.NumPad2,
            GlobalHotKeyVirtualKeys.NumPad3,
            GlobalHotKeyVirtualKeys.NumPad4,
            GlobalHotKeyVirtualKeys.NumPad5,
            GlobalHotKeyVirtualKeys.NumPad6,
            GlobalHotKeyVirtualKeys.NumPad7,
            GlobalHotKeyVirtualKeys.NumPad8,
            GlobalHotKeyVirtualKeys.NumPad9,
            GlobalHotKeyVirtualKeys.NumPad0
        };

        for (var index = 0; index < expectedKeys.Length; index++)
        {
            var hotkey = pasteHotkeys[index];
            AssertEqual(index, hotkey.Action.VisibleButtonIndex, $"paste hotkey {index + 1} button index");
        AssertEqual(GlobalHotKeyModifiers.Control | GlobalHotKeyModifiers.NoRepeat, hotkey.Modifiers, $"paste hotkey {index + 1} modifiers");
            AssertEqual(expectedKeys[index], hotkey.VirtualKey, $"paste hotkey {index + 1} virtual key");
        }
    }

    private static void GlobalHotKeyDefinitionsCanUseUserSettings()
    {
        var settings = HotKeySettings.CreateDefault();
        settings.TogglePalette = new HotKeyGesture
        {
            Enabled = true,
            Control = true,
            Alt = false,
            Shift = true,
            Key = "F8"
        };
        settings.PasteButtons[0] = new HotKeyGesture
        {
            Enabled = true,
            Control = true,
            Alt = true,
            Shift = false,
            Key = "D1"
        };
        settings.PasteButtons[1].Enabled = false;

        var hotkeys = GlobalHotKeyDefinitions.Create(settings).ToList();

        var toggle = hotkeys.Single(hotkey => hotkey.Action.Kind == GlobalHotKeyActionKind.TogglePalette);
        AssertEqual("Ctrl+Shift+F8", toggle.DisplayText, "custom toggle display text");
        AssertEqual(GlobalHotKeyVirtualKeys.F8, toggle.VirtualKey, "custom toggle virtual key");

        var firstPaste = hotkeys.Single(hotkey =>
            hotkey.Action.Kind == GlobalHotKeyActionKind.PasteVisibleButton
            && hotkey.Action.VisibleButtonIndex == 0);
        AssertEqual(0, firstPaste.Action.VisibleButtonIndex, "disabled paste hotkey should be skipped");
        AssertEqual("Ctrl+Alt+1", firstPaste.DisplayText, "custom paste display text");
        AssertEqual(GlobalHotKeyVirtualKeys.D1, firstPaste.VirtualKey, "custom paste virtual key");
    }

    private static void GlobalHotKeyServiceRecordsRegistrationFailures()
    {
        var serviceSource = File.ReadAllText(FindRepositoryFile("Promplet", "Services", "GlobalHotKeyService.cs"), Encoding.UTF8);

        AssertTrue(serviceSource.Contains("RegistrationResults", StringComparison.Ordinal), "GlobalHotKeyService should expose registration results");
        AssertTrue(serviceSource.Contains("Marshal.GetLastWin32Error()", StringComparison.Ordinal), "GlobalHotKeyService should record RegisterHotKey failure codes");
        AssertTrue(serviceSource.Contains("ApplyHotKeys", StringComparison.Ordinal), "GlobalHotKeyService should support applying edited hotkeys");
    }

    private static void SettingsDialogExposesAppearanceAndHotkeyControls()
    {
        var xaml = XDocument.Load(FindRepositoryFile("Promplet", "SettingsWindow.xaml"));
        var source = File.ReadAllText(FindRepositoryFile("Promplet", "SettingsWindow.xaml.cs"), Encoding.UTF8);
        var xamlSource = File.ReadAllText(FindRepositoryFile("Promplet", "SettingsWindow.xaml"), Encoding.UTF8);
        XNamespace wpf = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
        XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";

        AssertEqual("None", AttributeValue(xaml.Root!, "WindowStyle"), "settings dialog should use a themed title bar");
        AssertTrue(xaml.Descendants(wpf + "Grid").Any(element => AttributeValueOrDefault(element, x + "Name") == "TitleBar"), "settings dialog should expose a custom title bar");
        AssertTrue(xamlSource.Contains("PART_Popup", StringComparison.Ordinal), "settings combo box should use a themed template");
        AssertTrue(xamlSource.Contains("PART_Track", StringComparison.Ordinal), "settings slider should use a themed template");
        AssertTrue(xaml.Descendants(wpf + "ComboBox").Any(element => AttributeValueOrDefault(element, x + "Name") == "ThemeModeComboBox"), "settings dialog should expose theme mode");
        AssertTrue(xaml.Descendants(wpf + "Slider").Any(element => AttributeValueOrDefault(element, x + "Name") == "OpacitySlider"), "settings dialog should expose opacity");
        AssertTrue(xaml.Descendants(wpf + "StackPanel").Any(element => AttributeValueOrDefault(element, x + "Name") == "HotKeyRowsPanel"), "settings dialog should expose hotkey rows");
        AssertTrue(source.Contains("DispatcherTimer", StringComparison.Ordinal), "settings dialog should debounce live appearance previews");
        AssertTrue(source.Contains("ScheduleAppearancePreview", StringComparison.Ordinal), "settings dialog should preview appearance changes before OK");
        AssertTrue(source.Contains("ResetToDefaults", StringComparison.Ordinal), "settings dialog should reset settings");
        AssertTrue(source.Contains("DialogResult = true", StringComparison.Ordinal), "settings dialog should save with OK");
        AssertTrue(source.Contains("DialogResult = false", StringComparison.Ordinal), "settings dialog should cancel changes");
    }

    private static void GlobalHotKeyServiceExposesWin32RegistrationContract()
    {
        var nativeSource = File.ReadAllText(FindRepositoryFile("Promplet", "Win32", "NativeMethods.cs"), Encoding.UTF8);
        AssertTrue(nativeSource.Contains("WM_HOTKEY", StringComparison.Ordinal), "NativeMethods should expose WM_HOTKEY");
        AssertTrue(nativeSource.Contains("RegisterHotKey", StringComparison.Ordinal), "NativeMethods should expose RegisterHotKey");
        AssertTrue(nativeSource.Contains("UnregisterHotKey", StringComparison.Ordinal), "NativeMethods should expose UnregisterHotKey");

        var serviceSource = File.ReadAllText(FindRepositoryFile("Promplet", "Services", "GlobalHotKeyService.cs"), Encoding.UTF8);
        AssertTrue(serviceSource.Contains("RegisterHotKey", StringComparison.Ordinal), "GlobalHotKeyService should register hotkeys");
        AssertTrue(serviceSource.Contains("UnregisterHotKey", StringComparison.Ordinal), "GlobalHotKeyService should unregister hotkeys");
        AssertTrue(serviceSource.Contains("WM_HOTKEY", StringComparison.Ordinal), "GlobalHotKeyService should handle WM_HOTKEY");
        AssertTrue(serviceSource.Contains("HotKeyPressed", StringComparison.Ordinal), "GlobalHotKeyService should expose hotkey events");
        AssertTrue(serviceSource.Contains("ApplyHotKeys", StringComparison.Ordinal), "GlobalHotKeyService should support updated hotkey definitions");
    }

    private static void MainWindowWiresGlobalHotKeyActions()
    {
        var source = File.ReadAllText(FindRepositoryFile("Promplet", "MainWindow.xaml.cs"), Encoding.UTF8);

        AssertTrue(source.Contains("GlobalHotKeyService", StringComparison.Ordinal), "MainWindow should own GlobalHotKeyService");
        AssertTrue(source.Contains("HotKeyPressed", StringComparison.Ordinal), "MainWindow should subscribe to hotkey events");
        AssertTrue(source.Contains("TogglePalette", StringComparison.Ordinal), "MainWindow should handle toggle hotkey");
        AssertTrue(source.Contains("PasteVisibleButtonByIndexAsync", StringComparison.Ordinal), "MainWindow should paste visible buttons by index");
        AssertTrue(source.Contains("PastePromptAsync", StringComparison.Ordinal), "MainWindow should share prompt paste behavior");
    }

    private static void MainWindowUsesApprovedVerticalPaletteContract()
    {
        var xaml = XDocument.Load(FindRepositoryFile("Promplet", "MainWindow.xaml"));
        var root = xaml.Root ?? throw new InvalidOperationException("MainWindow.xaml has no root element.");
        XNamespace wpf = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
        XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";

        AssertEqual("CanResize", AttributeValue(root, "ResizeMode"), "palette resize mode");
        AssertEqual("Height", AttributeValue(root, "SizeToContent"), "palette height sizing");
        AssertTrue(double.Parse(AttributeValue(root, "Width")) >= 600, "palette starts wider than the old compact toolbar");

        var promptButtons = xaml.Descendants(wpf + "ItemsControl")
            .SingleOrDefault(element => AttributeValue(element, x + "Name") == "PromptButtons")
            ?? throw new InvalidOperationException("Could not find PromptButtons ItemsControl.");
        AssertEqual("{Binding VisibleButtons}", AttributeValue(promptButtons, "ItemsSource"), "prompt buttons binding");

        var panel = promptButtons.Descendants(wpf + "ItemsPanelTemplate")
            .Descendants(wpf + "StackPanel")
            .SingleOrDefault()
            ?? throw new InvalidOperationException("PromptButtons does not use a StackPanel items panel.");

        AssertEqual("Vertical", AttributeValue(panel, "Orientation"), "prompt button orientation");
        var groupTabs = xaml.Descendants(wpf + "ItemsControl")
            .SingleOrDefault(element => AttributeValue(element, x + "Name") == "GroupTabs")
            ?? throw new InvalidOperationException("Could not find GroupTabs ItemsControl.");
        AssertEqual("{Binding Groups}", AttributeValue(groupTabs, "ItemsSource"), "group tabs binding");
        AssertTrue(!xaml.Descendants(wpf + "TextBlock").Any(element => StaticGroupNames.Contains(AttributeValueOrDefault(element, "Text"))), "static group tab labels should not remain in XAML");
        var closeButton = xaml.Descendants(wpf + "Button")
            .SingleOrDefault(element => AttributeValueOrDefault(element, x + "Name") == "CloseButton")
            ?? throw new InvalidOperationException("Palette should have a close button.");
        AssertEqual("Hide Promplet", AttributeValue(closeButton, "ToolTip"), "close button should hide the palette");

        var mainWindowSource = File.ReadAllText(FindRepositoryFile("Promplet", "MainWindow.xaml.cs"), Encoding.UTF8);
        AssertTrue(mainWindowSource.Contains("HidePalette();", StringComparison.Ordinal), "close button should call HidePalette");
        AssertTrue(mainWindowSource.Contains("ExitApplication", StringComparison.Ordinal), "explicit tray exit should remain available");
    }

    private static string FindRepositoryFile(params string[] segments)
    {
        var directory = new DirectoryInfo(Environment.CurrentDirectory);

        while (directory is not null)
        {
            var candidate = Path.Combine(new[] { directory.FullName }.Concat(segments).ToArray());

            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Could not find repository file: {Path.Combine(segments)}");
    }

    private static string AttributeValue(XElement element, string name)
    {
        return element.Attribute(name)?.Value
            ?? throw new InvalidOperationException($"Missing attribute '{name}' on '{element.Name.LocalName}'.");
    }

    private static string AttributeValue(XElement element, XName name)
    {
        return element.Attribute(name)?.Value
            ?? throw new InvalidOperationException($"Missing attribute '{name.LocalName}' on '{element.Name.LocalName}'.");
    }

    private static string? AttributeValueOrDefault(XElement element, string name)
    {
        return element.Attribute(name)?.Value;
    }

    private static string? AttributeValueOrDefault(XElement element, XName name)
    {
        return element.Attribute(name)?.Value;
    }

    private static void AssertEqual<T>(T expected, T actual, string context)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new InvalidOperationException($"{context}: expected '{expected}', got '{actual}'.");
        }
    }

    private static void AssertTrue(bool condition, string context)
    {
        if (!condition)
        {
            throw new InvalidOperationException(context);
        }
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        private TemporaryDirectory(string path)
        {
            Path = path;
        }

        public string Path { get; }

        public static TemporaryDirectory Create()
        {
            var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "Promplet.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return new TemporaryDirectory(path);
        }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
