using System.Media;
using System.Windows;
using System.Windows.Input;
using Promplet.Models;
using Promplet.Services;
using Promplet.ViewModels;
using Promplet.Win32;

namespace Promplet;

public partial class MainWindow : Window
{
    private readonly ClipboardPasteService _clipboardPasteService = new();
    private readonly PromptDocument _promptDocument;
    private readonly PromptStore _promptStore;
    private readonly PaletteViewModel _paletteViewModel;

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
        Closing += (_, _) => SaveWindowState();
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
        Application.Current.Shutdown();
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
}
