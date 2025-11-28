using Gita.Practice.App.Models;
using Gita.Practice.App.Repository;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Gita.Practice.App.ViewModels;

public class AudioPlayerViewModel : BaseViewModel
{
    private double _playningSpeed = 1.0;
    private WaitModeOption _waitMode = WaitModeOption.KeyboardHit;
    public ICommand PlayCommand { get; }
    public ICommand PauseCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand DownloadCommand { get; }

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
    public WaitModeOption WaitMode {  get => _waitMode; set { _waitMode = value; OnPropertyChanged(); } }
    public int ParticipantEntryCount { get; set; } = 4;
    public double PlayingSpeed { get => _playningSpeed; set { _playningSpeed = value; OnPropertyChanged(); } }
    private async Task Play()
    {
        if (this.MediaElement == null)
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
            WaitForKeyPress = WaitMode == WaitModeOption.KeyboardHit,
            ParticipantEntryCount = ParticipantEntryCount,
            PlaybackSpeed = PlayingSpeed,
        };

        await new PlayRepository(new DataRepository()).Start(student, this.MediaElement!, async (waitInSeconds) =>
        {
            if (student.WaitForKeyPress)
            {
                MessageBox.Show("Press OK to continue to finish your turn and proceed.");
            }
            else
            {
                await Task.Delay(TimeSpan.FromSeconds(waitInSeconds));
            }
        });
    }
}
