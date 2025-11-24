using Gita.Practice.App.Repository;
using Gita.Practice.App.ViewModels;
using System.ComponentModel;

namespace Gita.Practice.App;

public class MainViewModel : BaseViewModel
{
    public MainViewModel(IDataRepository dataRepository)
    {
        this.Chapters = dataRepository.GetAllChapters()
            .Select(tuple => new ChapterViewModel(tuple.Item1, tuple.Item2));
    }

    public IEnumerable<ChapterViewModel> Chapters{ get; }
}