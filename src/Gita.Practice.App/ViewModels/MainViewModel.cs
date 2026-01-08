using Gita.Practice.App.Repository;
using Gita.Practice.App.Services;
using Gita.Practice.App.ViewModels;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace Gita.Practice.App;

/// <summary>
/// Main view model for the Gita Practice application.
/// Coordinates the chapter selection and audio playback functionality.
/// </summary>
public class MainViewModel : BaseViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainViewModel"/> class.
    /// </summary>
    /// <param name="player">The audio player service used for playback operations.</param>
    /// <param name="dataRepository">The data repository used to retrieve chapter information.</param>
    public MainViewModel(IPlayer player, IDataRepository dataRepository, IDownloadManager downloadManager, IMessageDialogService messageDialogService)
    {
        this.GroupPractiveViewModel = new GroupPracticeViewModel(player, messageDialogService);
        this.IndividualPracticeViewModel = new IndividualPracticeViewModel(player, messageDialogService);
        this.StatusViewModel = new StatusViewModel();
        this.Chapters = dataRepository.GetAllChapters()
            .Select(tuple => new ChapterViewModel(tuple.Item1, tuple.Item2))
            .ToList();

        ChapterSelectedCommand = new RelayCommand(param => OnChapterSelected(param));
        this.DownloadManager = downloadManager;

        this.DownloadManager.DownloadProgressChanged += (s, e) =>
        {
            this.StatusViewModel.UpdateLastMessage(e.Message);
        };
        this.DownloadManager.ChapterDownloaded += (s, e) =>
        {
            this.StatusViewModel.UpdateLastMessage(e.Message);
        };

        var firstChapter = this.Chapters.First();
        this.GroupPractiveViewModel.SelectedChapterNumber = firstChapter.Number;
        this.GroupPractiveViewModel.SelectedChapterName = firstChapter.Name;
        this.IndividualPracticeViewModel.SelectedChapterNumber = firstChapter.Number;
        this.IndividualPracticeViewModel.SelectedChapterName = firstChapter.Name;
    }

    /// <summary>
    /// Gets the collection of chapter view models representing all available chapters.
    /// </summary>
    public IEnumerable<ChapterViewModel> Chapters{ get; }

    /// <summary>
    /// Gets the audio player view model that manages playback controls and settings.
    /// </summary>
    public GroupPracticeViewModel GroupPractiveViewModel { get; }
    public IndividualPracticeViewModel IndividualPracticeViewModel { get; }
    public StatusViewModel StatusViewModel { get; }

    private ChapterViewModel? _selectedChapter;

    /// <summary>
    /// Gets or sets the currently selected chapter.
    /// When set, updates the audio player to use the selected chapter number.
    /// </summary>
    public ChapterViewModel? SelectedChapter
    {
        get => _selectedChapter;
        set => SetProperty(ref _selectedChapter, value);
    }

    /// <summary>
    /// Gets the command that is executed when a chapter is selected from the UI.
    /// </summary>
    public ICommand ChapterSelectedCommand { get; }
    public IDownloadManager DownloadManager { get; }

    /// <summary>
    /// Handles the chapter selection event.
    /// Updates the selected chapter and synchronizes the audio player with the selected chapter number.
    /// </summary>
    /// <param name="parameter">The command parameter, expected to be a <see cref="ChapterViewModel"/> instance.</param>
    private void OnChapterSelected(object? parameter)
    {
        if (parameter is ChapterViewModel cv)   
        {
            SelectedChapter = cv;
            this.GroupPractiveViewModel.SelectedChapterNumber = cv.Number;
            this.GroupPractiveViewModel.SelectedChapterName = cv.Name;
            this.IndividualPracticeViewModel.SelectedChapterNumber = cv.Number;
            this.IndividualPracticeViewModel.SelectedChapterName = cv.Name;
        }
    }
}