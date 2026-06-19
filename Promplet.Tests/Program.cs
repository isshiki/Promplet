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
            ("prompt store backs up invalid JSON and recreates defaults", PromptStoreBacksUpInvalidJson),
            ("prompt store backs up invalid JSON shape and recreates defaults", PromptStoreBacksUpInvalidJsonShape),
            ("palette view model switches visible prompt groups", PaletteViewModelSwitchesVisiblePromptGroups),
            ("palette window state clamps saved placement", PaletteWindowStateClampsSavedPlacement),
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
        AssertTrue(xaml.Descendants(wpf + "Button").Any(element => AttributeValue(element, x + "Name") == "CloseButton"), "palette has close button");
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
