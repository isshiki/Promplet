using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Promplet.Models;
using Promplet.Services;
using WpfButton = System.Windows.Controls.Button;
using WpfCheckBox = System.Windows.Controls.CheckBox;
using WpfKeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace Promplet;

public partial class SettingsWindow : Window
{
    private readonly IReadOnlyList<GlobalHotKeyRegistrationResult> _registrationResults;
    private readonly List<HotKeyEditorRow> _hotKeyRows = [];
    private readonly DispatcherTimer _appearancePreviewTimer;
    private PromptAppSettings _settings;
    private HotKeyEditorRow? _capturingRow;
    private bool _isApplyingControls;

    public SettingsWindow(
        PromptAppSettings settings,
        IReadOnlyList<GlobalHotKeyRegistrationResult> registrationResults)
    {
        _settings = settings.Clone();
        _registrationResults = registrationResults;
        _appearancePreviewTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _appearancePreviewTimer.Tick += AppearancePreviewTimer_Tick;

        InitializeComponent();
        PreviewKeyDown += SettingsWindow_PreviewKeyDown;
        ThemeModeComboBox.ItemsSource = new[]
        {
            PromptThemeModes.System,
            PromptThemeModes.Light,
            PromptThemeModes.Dark
        };
        ThemeModeComboBox.SelectionChanged += (_, _) => ScheduleAppearancePreview();
        OpacitySlider.ValueChanged += (_, _) =>
        {
            UpdateOpacityText();
            ScheduleAppearancePreview();
        };

        ApplySettingsToControls();
        UpdateRegistrationStatus();
    }

    public event EventHandler<PromptAppSettings>? AppearancePreviewRequested;

    public PromptAppSettings Settings => _settings.Clone();

    private void ApplySettingsToControls()
    {
        _isApplyingControls = true;
        ThemeModeComboBox.SelectedItem = _settings.ThemeMode;
        OpacitySlider.Value = _settings.Opacity;
        BuildHotKeyRows();
        UpdateOpacityText();
        _isApplyingControls = false;
    }

    private void BuildHotKeyRows()
    {
        _hotKeyRows.Clear();
        HotKeyRowsPanel.Children.Clear();

        AddHotKeyRow("Show / hide palette", () => _settings.HotKeys.TogglePalette, gesture => _settings.HotKeys.TogglePalette = gesture);

        for (var index = 0; index < _settings.HotKeys.PasteButtons.Count; index++)
        {
            var capturedIndex = index;
            AddHotKeyRow(
                $"Paste button {index + 1}",
                () => _settings.HotKeys.PasteButtons[capturedIndex],
                gesture => _settings.HotKeys.PasteButtons[capturedIndex] = gesture);
        }
    }

    private void AddHotKeyRow(
        string label,
        Func<HotKeyGesture> getGesture,
        Action<HotKeyGesture> setGesture)
    {
        var grid = new Grid
        {
            Margin = new Thickness(0, 0, 0, 8)
        };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(82) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var labelText = new TextBlock
        {
            Text = label,
            VerticalAlignment = VerticalAlignment.Center
        };
        labelText.SetResourceReference(TextBlock.ForegroundProperty, "PrompletTextPrimaryBrush");
        Grid.SetColumn(labelText, 0);
        grid.Children.Add(labelText);

        var enabledCheckBox = new WpfCheckBox
        {
            Content = "On",
            IsChecked = getGesture().Enabled,
            VerticalAlignment = VerticalAlignment.Center
        };
        enabledCheckBox.SetResourceReference(ForegroundProperty, "PrompletTextPrimaryBrush");
        Grid.SetColumn(enabledCheckBox, 1);
        grid.Children.Add(enabledCheckBox);

        var captureButton = new WpfButton
        {
            Content = GlobalHotKeyDefinitions.FormatGesture(getGesture()),
            HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch
        };
        Grid.SetColumn(captureButton, 2);
        grid.Children.Add(captureButton);

        var row = new HotKeyEditorRow(getGesture, setGesture, enabledCheckBox, captureButton);
        enabledCheckBox.Checked += (_, _) => SetGestureEnabled(row, true);
        enabledCheckBox.Unchecked += (_, _) => SetGestureEnabled(row, false);
        captureButton.Click += (_, _) => StartCapture(row);

        _hotKeyRows.Add(row);
        HotKeyRowsPanel.Children.Add(grid);
    }

    private void SetGestureEnabled(HotKeyEditorRow row, bool enabled)
    {
        var gesture = row.GetGesture().Clone();
        gesture.Enabled = enabled;
        row.SetGesture(gesture);
        row.CaptureButton.Content = GlobalHotKeyDefinitions.FormatGesture(gesture);
    }

    private void StartCapture(HotKeyEditorRow row)
    {
        _capturingRow = row;
        row.CaptureButton.Content = "Press shortcut...";
        row.CaptureButton.Focus();
    }

    private void SettingsWindow_PreviewKeyDown(object sender, WpfKeyEventArgs e)
    {
        if (_capturingRow is null)
        {
            return;
        }

        if (e.Key == Key.Escape)
        {
            StopCapture(refreshButton: true);
            e.Handled = true;
            return;
        }

        var key = GetPressedKey(e);
        if (IsModifierKey(key))
        {
            e.Handled = true;
            return;
        }

        if (!TryCreateGestureFromKeyboard(key, Keyboard.Modifiers, out var gesture))
        {
            SystemSounds.Beep.Play();
            StopCapture(refreshButton: true);
            e.Handled = true;
            return;
        }

        _capturingRow.SetGesture(gesture);
        _capturingRow.EnabledCheckBox.IsChecked = true;
        _capturingRow.CaptureButton.Content = GlobalHotKeyDefinitions.FormatGesture(gesture);
        StopCapture(refreshButton: false);
        e.Handled = true;
    }

    private static Key GetPressedKey(WpfKeyEventArgs e)
    {
        if (e.Key == Key.System)
        {
            return e.SystemKey;
        }

        return e.Key == Key.ImeProcessed
            ? e.ImeProcessedKey
            : e.Key;
    }

    private static bool IsModifierKey(Key key)
    {
        return key is Key.LeftCtrl
            or Key.RightCtrl
            or Key.LeftShift
            or Key.RightShift
            or Key.LeftAlt
            or Key.RightAlt
            or Key.LWin
            or Key.RWin;
    }

    private static bool TryCreateGestureFromKeyboard(
        Key key,
        ModifierKeys modifiers,
        out HotKeyGesture gesture)
    {
        gesture = new HotKeyGesture
        {
            Enabled = true,
            Control = modifiers.HasFlag(ModifierKeys.Control),
            Alt = modifiers.HasFlag(ModifierKeys.Alt),
            Shift = modifiers.HasFlag(ModifierKeys.Shift),
            Windows = modifiers.HasFlag(ModifierKeys.Windows),
            Key = NormalizeWpfKeyName(key)
        };

        if (!gesture.Control && !gesture.Alt && !gesture.Shift && !gesture.Windows)
        {
            return false;
        }

        return GlobalHotKeyDefinitions.IsSupportedKey(gesture.Key);
    }

    private static string NormalizeWpfKeyName(Key key)
    {
        return key switch
        {
            Key.NumPad0 => "NumPad0",
            Key.NumPad1 => "NumPad1",
            Key.NumPad2 => "NumPad2",
            Key.NumPad3 => "NumPad3",
            Key.NumPad4 => "NumPad4",
            Key.NumPad5 => "NumPad5",
            Key.NumPad6 => "NumPad6",
            Key.NumPad7 => "NumPad7",
            Key.NumPad8 => "NumPad8",
            Key.NumPad9 => "NumPad9",
            Key.D0 => "D0",
            Key.D1 => "D1",
            Key.D2 => "D2",
            Key.D3 => "D3",
            Key.D4 => "D4",
            Key.D5 => "D5",
            Key.D6 => "D6",
            Key.D7 => "D7",
            Key.D8 => "D8",
            Key.D9 => "D9",
            Key.Space => "Space",
            _ => key.ToString()
        };
    }

    private void StopCapture(bool refreshButton)
    {
        if (_capturingRow is not null && refreshButton)
        {
            _capturingRow.CaptureButton.Content = GlobalHotKeyDefinitions.FormatGesture(_capturingRow.GetGesture());
        }

        _capturingRow = null;
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left && e.ClickCount == 1)
        {
            DragMove();
        }
    }

    private void UpdateOpacityText()
    {
        OpacityValueTextBlock.Text = $"{OpacitySlider.Value:P0}";
    }

    private void ScheduleAppearancePreview()
    {
        if (_isApplyingControls)
        {
            return;
        }

        _appearancePreviewTimer.Stop();
        _appearancePreviewTimer.Start();
    }

    private void AppearancePreviewTimer_Tick(object? sender, EventArgs e)
    {
        _appearancePreviewTimer.Stop();
        UpdateSettingsFromControls();
        AppearancePreviewRequested?.Invoke(this, Settings);
    }

    private void UpdateRegistrationStatus()
    {
        var failed = _registrationResults
            .Where(result => !result.IsRegistered)
            .Select(result => $"{result.Definition.DisplayText} failed ({result.ErrorCode})")
            .ToList();

        HotKeyStatusTextBlock.Text = failed.Count == 0
            ? "All configured hotkeys are registered."
            : string.Join("  ", failed);
    }

    private void ResetToDefaults_Click(object sender, RoutedEventArgs e)
    {
        var selectedGroupId = _settings.SelectedGroupId;
        var restoreClipboard = _settings.RestoreClipboard;

        _settings.ResetConfigurableSettings();
        _settings.SelectedGroupId = selectedGroupId;
        _settings.RestoreClipboard = restoreClipboard;
        ApplySettingsToControls();
        ScheduleAppearancePreview();
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        _appearancePreviewTimer.Stop();
        UpdateSettingsFromControls();
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        _appearancePreviewTimer.Stop();
        DialogResult = false;
    }

    private void UpdateSettingsFromControls()
    {
        _settings.ThemeMode = ThemeModeComboBox.SelectedItem as string ?? PromptThemeModes.System;
        _settings.Opacity = Math.Round(OpacitySlider.Value, 2);
    }

    private sealed record HotKeyEditorRow(
        Func<HotKeyGesture> GetGesture,
        Action<HotKeyGesture> SetGesture,
        WpfCheckBox EnabledCheckBox,
        WpfButton CaptureButton);
}
