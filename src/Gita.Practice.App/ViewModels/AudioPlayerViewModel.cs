using Gita.Practice.App.Models;
using Gita.Practice.App.Repository;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Gita.Practice.App.ViewModels;

public class AudioPlayerViewModel : BaseViewModel
{
    private Uri _audioSource;
    public Uri AudioSource
    {
        get => _audioSource;
        set { _audioSource = value; OnPropertyChanged(); }
    }

    public ICommand PlayCommand { get; }
    public ICommand PauseCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand DownloadCommand { get; }

    private readonly DispatcherTimer _timer;


    // Segment playback fields
    private DispatcherTimer? _segmentTimer;
    private TimeSpan? _segmentStart;
    private TimeSpan? _segmentEnd;
    private const int TimerIntervalMs = 200;

    private TimeSpan _fromTime = TimeSpan.FromSeconds(629.525);
    private TimeSpan _toTime = TimeSpan.FromSeconds(642.336);

    public AudioPlayerViewModel()
    {
        PlayCommand = new RelayCommand(_ =>
        {
            //PlayRange();
            Play();
        });

        //PauseCommand = new RelayCommand(_ => Pause());
        //StopCommand = new RelayCommand(_ => Stop());
        //DownloadCommand = new RelayCommand(_ => { Download(); });


    }

    public MediaElement MediaElement { get; set; }

    public int SelectedChapterNumber { get; set; } = 1;
    public int NumberOfParticipants { get; set; } = 2;
    public int YourTurn { get; set; } = 2;
    public int YourDurationInSeconds { get; set; } = 20;
    public bool RepeatYourSloka { get; set; }

    private async Task Play()
    {
        if(this.MediaElement == null)
        {
           MessageBox.Show("MediaElement is not set.");
        }
        var student = new PracticeInfo
        {
            Chapter = this.SelectedChapterNumber,
            NumberOfParticipants = NumberOfParticipants,
            YourDurationInSeconds = YourDurationInSeconds,
            RepeatYourSloka = RepeatYourSloka,
            Sloka = 1,
            YourTurn = YourTurn,
        };

        await new PlayRepository(new DataRepository()).Start(student, this.MediaElement!);
    }
}
