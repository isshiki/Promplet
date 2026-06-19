using System.Media;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using Promplet.Models;
using Promplet.Services;
using Promplet.ViewModels;
using Promplet.Win32;
using Drawing = System.Drawing;

namespace Promplet;

public partial class MainWindow : Window
{
    private readonly ClipboardPasteService _clipboardPasteService = new();
    private PromptDocument _promptDocument;
    private readonly PromptStore _promptStore;
    private readonly PaletteViewModel _paletteViewModel;
    private readonly TrayIconService _trayIconService;
    private readonly GlobalHotKeyService _globalHotKeyService;
    private bool _isExiting;

    public MainWindow()
        : this(new PromptStore())
    {
    }

    internal MainWindow(PromptStore promptStore)
    {
        _promptStore = promptStore;
        _promptDocument = _promptStore.LoadOrCreate();
        _paletteViewModel = new PaletteViewModel(_promptDocument);

        AppearanceService.Apply(_promptDocument.App.ThemeMode);
        InitializeComponent();
        NonActivatingWindowBehavior.Attach(this);
        DataContext = _paletteViewModel;
        ApplyAppearanceSettings();
        ApplySavedWindowState(_promptDocument.Window);
        DragHandle.MouseLeftButtonDown += DragHandle_MouseLeftButtonDown;
        _trayIconService = new TrayIconService(CreateTrayIcon(), TogglePalette, OpenPromptLibrary, OpenSettings, ReloadPrompts, ExitApplication);
        _globalHotKeyService = new GlobalHotKeyService(this, GlobalHotKeyDefinitions.Create(_promptDocument.App.HotKeys));
        _globalHotKeyService.HotKeyPressed += GlobalHotKeyService_HotKeyPressed;
        Closing += MainWindow_Closing;
        Closed += (_, _) =>
        {
            _globalHotKeyService.Dispose();
            _trayIconService.Dispose();
        };
    }

    private void DragHandle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left && e.ClickCount == 1)
        {
            DragMove();
        }
    }

    private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (SizeToContent != System.Windows.SizeToContent.Height)
        {
            SizeToContent = System.Windows.SizeToContent.Height;
        }
    }

    private async void PromptButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { DataContext: PromptButton promptButton })
        {
            return;
        }

        await PastePromptAsync(promptButton);
    }

    private async Task PastePromptAsync(PromptButton promptButton)
    {
        try
        {
            await _clipboardPasteService.PasteTextAsync(promptButton.Text);
        }
        catch
        {
            SystemSounds.Beep.Play();
        }
    }

    private void PromptButton_RightClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is not FrameworkElement { DataContext: PromptButton promptButton })
        {
            return;
        }

        try
        {
            _clipboardPasteService.CopyText(promptButton.Text);
            e.Handled = true;
        }
        catch
        {
            SystemSounds.Beep.Play();
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        HidePalette();
    }

    private async void GlobalHotKeyService_HotKeyPressed(object? sender, GlobalHotKeyPressedEventArgs e)
    {
        switch (e.Action.Kind)
        {
            case GlobalHotKeyActionKind.TogglePalette:
                TogglePalette();
                break;
            case GlobalHotKeyActionKind.PasteVisibleButton:
                await PasteVisibleButtonByIndexAsync(e.Action.VisibleButtonIndex);
                break;
        }
    }

    private void MainWindow_Closing(object? sender, CancelEventArgs e)
    {
        if (_isExiting)
        {
            SaveWindowState();
            return;
        }

        e.Cancel = true;
        HidePalette();
    }

    private void ShowPalette()
    {
        if (!IsVisible)
        {
            Show();
        }

        if (WindowState == WindowState.Minimized)
        {
            WindowState = WindowState.Normal;
        }

        Topmost = true;
    }

    private void TogglePalette()
    {
        if (IsVisible)
        {
            HidePalette();
        }
        else
        {
            ShowPalette();
        }
    }

    private void HidePalette()
    {
        SaveWindowState();
        Hide();
    }

    private async Task PasteVisibleButtonByIndexAsync(int zeroBasedIndex)
    {
        if (zeroBasedIndex < 0 || zeroBasedIndex >= _paletteViewModel.VisibleButtons.Count)
        {
            SystemSounds.Beep.Play();
            return;
        }

        await PastePromptAsync(_paletteViewModel.VisibleButtons[zeroBasedIndex]);
    }

    private void ReloadPrompts()
    {
        try
        {
            _promptDocument = _promptStore.LoadOrCreate();
            _paletteViewModel.LoadDocument(_promptDocument);
            ApplyAppearanceSettings();
            _globalHotKeyService.ApplyHotKeys(GlobalHotKeyDefinitions.Create(_promptDocument.App.HotKeys));
        }
        catch
        {
            SystemSounds.Beep.Play();
        }
    }

    private void OpenPromptLibrary()
    {
        var dialog = new PromptLibraryWindow(_promptDocument);

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        _promptDocument = dialog.Document;
        _promptStore.Save(_promptDocument);
        _promptDocument = _promptStore.LoadOrCreate();
        _paletteViewModel.LoadDocument(_promptDocument);
        ApplyAppearanceSettings();
        _globalHotKeyService.ApplyHotKeys(GlobalHotKeyDefinitions.Create(_promptDocument.App.HotKeys));
    }

    private void OpenSettings()
    {
        var originalSettings = _promptDocument.App.Clone();
        var dialog = new SettingsWindow(_promptDocument.App, _globalHotKeyService.RegistrationResults)
        {
            Topmost = true
        };
        dialog.AppearancePreviewRequested += SettingsWindow_AppearancePreviewRequested;

        if (IsVisible)
        {
            dialog.Owner = this;
        }

        if (dialog.ShowDialog() != true)
        {
            _promptDocument.App = originalSettings;
            ApplyAppearanceSettings();
            dialog.AppearancePreviewRequested -= SettingsWindow_AppearancePreviewRequested;
            return;
        }

        dialog.AppearancePreviewRequested -= SettingsWindow_AppearancePreviewRequested;
        _promptDocument.App = dialog.Settings;
        _promptStore.Save(_promptDocument);
        ApplyAppearanceSettings();
        _globalHotKeyService.ApplyHotKeys(GlobalHotKeyDefinitions.Create(_promptDocument.App.HotKeys));
    }

    private void SettingsWindow_AppearancePreviewRequested(object? sender, PromptAppSettings settings)
    {
        AppearanceService.Apply(settings.ThemeMode);
        Opacity = settings.Opacity;
    }

    private void ExitApplication()
    {
        _isExiting = true;
        SaveWindowState();
        _globalHotKeyService.Dispose();
        _trayIconService.Dispose();
        System.Windows.Application.Current.Shutdown();
    }

    private void ApplySavedWindowState(PromptWindowState state)
    {
        var normalized = PaletteWindowState.Normalize(
            state,
            SystemParameters.VirtualScreenLeft,
            SystemParameters.VirtualScreenTop,
            SystemParameters.VirtualScreenWidth,
            SystemParameters.VirtualScreenHeight);

        Width = normalized.Width;

        if (normalized.Left is double left && normalized.Top is double top)
        {
            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = left;
            Top = top;
        }
        else
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }
    }

    private void ApplyAppearanceSettings()
    {
        AppearanceService.Apply(_promptDocument.App.ThemeMode);
        Opacity = _promptDocument.App.Opacity;
    }

    private void SaveWindowState()
    {
        _promptDocument.Window = new PromptWindowState
        {
            Left = Left,
            Top = Top,
            Width = Width
        };

        _promptStore.Save(_promptDocument);
    }

    private static Drawing.Icon CreateTrayIcon()
    {
        if (!string.IsNullOrWhiteSpace(Environment.ProcessPath))
        {
            var associatedIcon = Drawing.Icon.ExtractAssociatedIcon(Environment.ProcessPath);

            if (associatedIcon is not null)
            {
                return associatedIcon;
            }
        }

        return new Drawing.Icon(Drawing.SystemIcons.Application, Drawing.SystemIcons.Application.Size);
    }
}
