using Gita.Practice.App.ViewModels;
using System.ComponentModel;

namespace Gita.Practice.App;

public class MainViewModel : BaseViewModel
{
    public MainViewModel()
    {
    }

    public ChapterTreeViewModel ChapterTreeViewModel { get; } = new ChapterTreeViewModel();
}