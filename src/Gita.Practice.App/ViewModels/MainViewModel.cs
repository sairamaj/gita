using Gita.Practice.App.Repository;
using Gita.Practice.App.ViewModels;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace Gita.Practice.App;

public class MainViewModel : BaseViewModel
{
    public MainViewModel(IDataRepository dataRepository)
    {
        this.Chapters = dataRepository.GetAllChapters()
            .Select(tuple => new ChapterViewModel(tuple.Item1, tuple.Item2))
            .ToList();

        ChapterSelectedCommand = new RelayCommand(param => OnChapterSelected(param));
    }

    public IEnumerable<ChapterViewModel> Chapters{ get; }
    public AudioPlayerViewModel AudioPlayerViewModel { get; } = new AudioPlayerViewModel();
    private ChapterViewModel? _selectedChapter;
    public ChapterViewModel? SelectedChapter
    {
        get => _selectedChapter;
        set => SetProperty(ref _selectedChapter, value);
    }

    public ICommand ChapterSelectedCommand { get; }

    private void OnChapterSelected(object? parameter)
    {
        if (parameter is ChapterViewModel cv)
        {
            SelectedChapter = cv;
            this.AudioPlayerViewModel.SelectedChapterNumber = cv.Number;
        }
    }
}