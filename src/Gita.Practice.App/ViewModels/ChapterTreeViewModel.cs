namespace Gita.Practice.App.ViewModels;

public class ChapterTreeViewModel
{
    public ChapterTreeViewModel()
    {
        this.Chapters = new List<ChapterViewModel>()
        {
            new ChapterViewModel("Chapter 1"),
            new ChapterViewModel("Chapter 2"),
            new ChapterViewModel("Chapter 3"),
        };
    }
    public IEnumerable<ChapterViewModel> Chapters { get; set; }
}
