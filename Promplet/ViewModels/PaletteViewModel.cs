using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Promplet.Models;
using Promplet.Services;

namespace Promplet.ViewModels;

public sealed class PaletteViewModel : INotifyPropertyChanged
{
    private PromptDocument _document;
    private PaletteGroupViewModel? _selectedGroup;

    public PaletteViewModel(PromptDocument document)
    {
        _document = document;
        Groups = [];
        VisibleButtons = [];
        SelectGroupCommand = new RelayCommand(parameter =>
        {
            if (parameter is PaletteGroupViewModel group)
            {
                SelectGroup(group);
            }
        });

        LoadDocument(document);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<PaletteGroupViewModel> Groups { get; }

    public ObservableCollection<PromptButton> VisibleButtons { get; }

    public ICommand SelectGroupCommand { get; }

    public PaletteGroupViewModel? SelectedGroup
    {
        get => _selectedGroup;
        private set
        {
            if (_selectedGroup == value)
            {
                return;
            }

            _selectedGroup = value;
            OnPropertyChanged(nameof(SelectedGroup));
        }
    }

    public void LoadDocument(PromptDocument document)
    {
        _document = document;
        Groups.Clear();
        VisibleButtons.Clear();

        foreach (var group in document.Groups.Select(group => new PaletteGroupViewModel(group)))
        {
            Groups.Add(group);
        }

        var initialGroup = Groups.FirstOrDefault(group => group.Id == document.App.SelectedGroupId)
            ?? Groups.FirstOrDefault();

        if (initialGroup is not null)
        {
            SelectGroup(initialGroup);
        }
        else
        {
            SelectedGroup = null;
        }
    }

    public void SelectGroup(PaletteGroupViewModel group)
    {
        foreach (var tab in Groups)
        {
            tab.IsSelected = ReferenceEquals(tab, group);
        }

        SelectedGroup = group;
        _document.App.SelectedGroupId = group.Id;

        VisibleButtons.Clear();
        foreach (var button in group.Source.Buttons.Where(button => button.Enabled).Take(PromptStore.MaximumButtonsPerGroup))
        {
            VisibleButtons.Add(button);
        }
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public sealed class PaletteGroupViewModel : INotifyPropertyChanged
{
    private bool _isSelected;

    public PaletteGroupViewModel(PromptGroup source)
    {
        Source = source;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public PromptGroup Source { get; }

    public string Id => Source.Id;

    public string Name => Source.Name;

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value)
            {
                return;
            }

            _isSelected = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
        }
    }
}

internal sealed class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;

    public RelayCommand(Action<object?> execute)
    {
        _execute = execute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add { }
        remove { }
    }

    public bool CanExecute(object? parameter)
    {
        return true;
    }

    public void Execute(object? parameter)
    {
        _execute(parameter);
    }
}
