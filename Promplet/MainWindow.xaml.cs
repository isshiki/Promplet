using System.Media;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using Promplet.Models;
using Promplet.Services;
using Promplet.ViewModels;
using Promplet.Win32;

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

        InitializeComponent();
        NonActivatingWindowBehavior.Attach(this);
        DataContext = _paletteViewModel;
        ApplySavedWindowState(_promptDocument.Window);
        DragHandle.MouseLeftButtonDown += DragHandle_MouseLeftButtonDown;
        _trayIconService = new TrayIconService(GetTrayIconPath(), ShowPalette, HidePalette, ReloadPrompts, ExitApplication);
        _globalHotKeyService = new GlobalHotKeyService(this);
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
        }
        catch
        {
            SystemSounds.Beep.Play();
        }
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

    private static string GetTrayIconPath()
    {
        return Path.Combine(AppContext.BaseDirectory, "Assets", "promplet_icon.ico");
    }
}
