using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Gita.Practice.App.ViewModels;

public class ChapterViewModel : BaseViewModel
{
    public ChapterViewModel(string Name, int number)
    {
        this.Name = Name;
        this.Number = number;
    }

    public string Name { get; }
    public int Number { get; }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    private bool _isExpanded;
    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetProperty(ref _isExpanded, value);
    }
}
