using System.Media;
using System.Windows;
using System.Windows.Input;
using Promplet.Models;
using Promplet.Services;
using Promplet.Win32;

namespace Promplet;

public partial class MainWindow : Window
{
    private readonly ClipboardPasteService _clipboardPasteService = new();

    public MainWindow()
    {
        InitializeComponent();
        NonActivatingWindowBehavior.Attach(this);
        PromptButtons.ItemsSource = PromptCatalog.GetDefaultButtons();
        PaletteRoot.MouseLeftButtonDown += PaletteRoot_MouseLeftButtonDown;
    }

    private void PaletteRoot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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
}
