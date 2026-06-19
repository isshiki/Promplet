using System.Media;
using System.Windows;
using System.Windows.Input;
using Promplet.Models;
using Promplet.Services;

namespace Promplet;

public partial class PromptLibraryWindow : Window
{
    private readonly PromptDocument _document;
    private bool _isLoading;

    public PromptLibraryWindow(PromptDocument document)
    {
        _document = document.Clone();
        InitializeComponent();
        LoadGroups();
    }

    public PromptDocument Document => _document.Clone();

    private PromptGroup? SelectedGroup => GroupListBox.SelectedItem as PromptGroup;

    private PromptButton? SelectedPrompt => PromptListBox.SelectedItem as PromptButton;

    private void LoadGroups(PromptGroup? groupToSelect = null)
    {
        _isLoading = true;
        GroupListBox.ItemsSource = null;
        GroupListBox.ItemsSource = _document.Groups;
        GroupListBox.SelectedItem = groupToSelect ?? _document.Groups.FirstOrDefault(group => group.Id == _document.App.SelectedGroupId)
            ?? _document.Groups.FirstOrDefault();
        _isLoading = false;
        LoadSelectedGroup();
    }

    private void LoadSelectedGroup(PromptButton? promptToSelect = null)
    {
        _isLoading = true;
        var group = SelectedGroup;
        GroupNameTextBox.Text = group?.Name ?? string.Empty;
        PromptListBox.ItemsSource = null;
        PromptListBox.ItemsSource = group?.Buttons;
        PromptListBox.SelectedItem = promptToSelect ?? group?.Buttons.FirstOrDefault();
        _isLoading = false;
        LoadSelectedPrompt();
    }

    private void LoadSelectedPrompt()
    {
        _isLoading = true;
        var prompt = SelectedPrompt;
        PromptNameTextBox.Text = prompt?.Label ?? string.Empty;
        PromptTextTextBox.Text = prompt?.Text ?? string.Empty;
        PromptEnabledCheckBox.IsChecked = prompt?.Enabled ?? false;
        var hasPrompt = prompt is not null;
        PromptNameTextBox.IsEnabled = hasPrompt;
        PromptTextTextBox.IsEnabled = hasPrompt;
        PromptEnabledCheckBox.IsEnabled = hasPrompt;
        _isLoading = false;
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left && e.ClickCount == 1)
        {
            DragMove();
        }
    }

    private void GroupListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (_isLoading)
        {
            return;
        }

        if (SelectedGroup is PromptGroup group)
        {
            _document.App.SelectedGroupId = group.Id;
        }

        LoadSelectedGroup();
    }

    private void PromptListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (!_isLoading)
        {
            LoadSelectedPrompt();
        }
    }

    private void AddGroup_Click(object sender, RoutedEventArgs e)
    {
        AddGroup();
    }

    private void DeleteGroup_Click(object sender, RoutedEventArgs e)
    {
        DeleteGroup();
    }

    private void AddPrompt_Click(object sender, RoutedEventArgs e)
    {
        AddPrompt();
    }

    private void DeletePrompt_Click(object sender, RoutedEventArgs e)
    {
        DeletePrompt();
    }

    private void MovePromptUp_Click(object sender, RoutedEventArgs e)
    {
        MovePrompt(-1);
    }

    private void MovePromptDown_Click(object sender, RoutedEventArgs e)
    {
        MovePrompt(1);
    }

    private void AddGroup()
    {
        if (_document.Groups.Count >= PromptStore.MaximumGroups)
        {
            SystemSounds.Beep.Play();
            return;
        }

        var group = new PromptGroup
        {
            Id = CreateUniqueId(_document.Groups.Select(existing => existing.Id), "group"),
            Name = CreateUniqueName(_document.Groups.Select(existing => existing.Name), "New Group"),
            Buttons = []
        };

        _document.Groups.Add(group);
        _document.App.SelectedGroupId = group.Id;
        LoadGroups(group);
    }

    private void DeleteGroup()
    {
        var group = SelectedGroup;
        if (group is null || _document.Groups.Count <= 1)
        {
            SystemSounds.Beep.Play();
            return;
        }

        var index = _document.Groups.IndexOf(group);
        _document.Groups.Remove(group);
        var nextGroup = _document.Groups[Math.Clamp(index, 0, _document.Groups.Count - 1)];
        _document.App.SelectedGroupId = nextGroup.Id;
        LoadGroups(nextGroup);
    }

    private void AddPrompt()
    {
        var group = SelectedGroup;
        if (group is null || group.Buttons.Count >= PromptStore.MaximumButtonsPerGroup)
        {
            SystemSounds.Beep.Play();
            return;
        }

        var prompt = new PromptButton(
            CreateUniqueId(group.Buttons.Select(existing => existing.Id), "prompt"),
            CreateUniqueName(group.Buttons.Select(existing => existing.Label), "New Prompt"),
            string.Empty);

        group.Buttons.Add(prompt);
        LoadSelectedGroup(prompt);
    }

    private void DeletePrompt()
    {
        var group = SelectedGroup;
        var prompt = SelectedPrompt;
        if (group is null || prompt is null)
        {
            SystemSounds.Beep.Play();
            return;
        }

        var index = group.Buttons.IndexOf(prompt);
        group.Buttons.Remove(prompt);
        var nextPrompt = group.Buttons.Count == 0
            ? null
            : group.Buttons[Math.Clamp(index, 0, group.Buttons.Count - 1)];
        LoadSelectedGroup(nextPrompt);
    }

    private void MovePrompt(int offset)
    {
        var group = SelectedGroup;
        var prompt = SelectedPrompt;
        if (group is null || prompt is null)
        {
            SystemSounds.Beep.Play();
            return;
        }

        var oldIndex = group.Buttons.IndexOf(prompt);
        var newIndex = oldIndex + offset;
        if (newIndex < 0 || newIndex >= group.Buttons.Count)
        {
            SystemSounds.Beep.Play();
            return;
        }

        group.Buttons.RemoveAt(oldIndex);
        group.Buttons.Insert(newIndex, prompt);
        LoadSelectedGroup(prompt);
    }

    private void GroupNameTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (_isLoading || SelectedGroup is not PromptGroup group)
        {
            return;
        }

        group.Name = GroupNameTextBox.Text;
        GroupListBox.Items.Refresh();
    }

    private void PromptNameTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        UpdateSelectedPrompt(prompt => prompt with { Label = PromptNameTextBox.Text });
    }

    private void PromptTextTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        UpdateSelectedPrompt(prompt => prompt with { Text = PromptTextTextBox.Text });
    }

    private void PromptEnabledCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        UpdateSelectedPrompt(prompt => prompt with { Enabled = PromptEnabledCheckBox.IsChecked == true });
    }

    private void UpdateSelectedPrompt(Func<PromptButton, PromptButton> update)
    {
        if (_isLoading || SelectedGroup is not PromptGroup group || SelectedPrompt is not PromptButton prompt)
        {
            return;
        }

        var index = group.Buttons.IndexOf(prompt);
        if (index < 0)
        {
            return;
        }

        var updated = update(prompt);
        group.Buttons[index] = updated;
        _isLoading = true;
        PromptListBox.Items.Refresh();
        PromptListBox.SelectedItem = updated;
        _isLoading = false;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        NormalizeBeforeSave();
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private void NormalizeBeforeSave()
    {
        foreach (var group in _document.Groups)
        {
            group.Name = string.IsNullOrWhiteSpace(group.Name)
                ? "Group"
                : group.Name.Trim();

            for (var index = 0; index < group.Buttons.Count; index++)
            {
                var prompt = group.Buttons[index];
                group.Buttons[index] = prompt with
                {
                    Label = string.IsNullOrWhiteSpace(prompt.Label)
                        ? "Prompt"
                        : prompt.Label.Trim(),
                    Text = prompt.Text ?? string.Empty
                };
            }
        }

        if (!_document.Groups.Any(group => group.Id == _document.App.SelectedGroupId))
        {
            _document.App.SelectedGroupId = _document.Groups.FirstOrDefault()?.Id ?? "ai-chat";
        }
    }

    private static string CreateUniqueId(IEnumerable<string> existingIds, string prefix)
    {
        var existing = existingIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
        for (var index = 1; index < 1000; index++)
        {
            var candidate = $"{prefix}-{index}";
            if (!existing.Contains(candidate))
            {
                return candidate;
            }
        }

        return $"{prefix}-{Guid.NewGuid():N}";
    }

    private static string CreateUniqueName(IEnumerable<string> existingNames, string baseName)
    {
        var existing = existingNames.ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (!existing.Contains(baseName))
        {
            return baseName;
        }

        for (var index = 2; index < 1000; index++)
        {
            var candidate = $"{baseName} {index}";
            if (!existing.Contains(candidate))
            {
                return candidate;
            }
        }

        return baseName;
    }
}
