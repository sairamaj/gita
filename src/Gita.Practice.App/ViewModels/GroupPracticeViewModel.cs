using Gita.Practice.App.Models;
using Gita.Practice.App.Repository;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Gita.Practice.App.ViewModels;

public class GroupPracticeViewModel : BaseViewModel
{
    private double _playningSpeed = 1.50;
    private int _yourTurn = 2;
    private int _participantStanzaCount = 4;
    private WaitModeOption _waitMode = WaitModeOption.KeyboardHit;
    public ICommand PlayCommand { get; set; }
    public ICommand PauseCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand DownloadCommand { get; }

    public GroupPracticeViewModel(IPlayer player)
    {
        PlayCommand = new RelayCommand(_ =>
        {
            _ = Play();
        });

        PauseCommand = new RelayCommand(_ => Pause());
        StopCommand = new RelayCommand(_ => Stop());
        Player = player ?? throw new ArgumentNullException(nameof(player));
    }

    public MediaElement MediaElement { get; set; }
    public bool IsPlaying { get; set; }
    public int SelectedChapterNumber { get; set; } = 1;
    public int NumberOfParticipants { get; set; } = 2;
    public int YourTurn
    {
        get => this._yourTurn;
        set
        {
            if (value > NumberOfParticipants || value < 1)
            {
                OnPropertyChanged();
                return;
            }
            this._yourTurn = value; OnPropertyChanged();
        }
    }
    public int YourDurationInSeconds { get; set; } = 20;
    public bool RepeatYourSloka { get; set; }
    public WaitModeOption WaitMode { get => _waitMode; set { _waitMode = value; OnPropertyChanged(); } }
    public int ParticipantStanzaCount
    {
        get => _participantStanzaCount;
        set
        {
            if(value > 4 || value < 1)
            {
                OnPropertyChanged();
                return;
            }
            _participantStanzaCount = value; OnPropertyChanged();
        }
    }
    public double PlayingSpeed { get => _playningSpeed; set { _playningSpeed = value; OnPropertyChanged(); } }

    public IPlayer Player { get; }

    private async Task Play()
    {
        if (this.MediaElement == null)
        {
            MessageBox.Show("MediaElement is not set.");
        }
        try
        {
            IsPlaying = true;
            OnPropertyChanged(nameof(IsPlaying));
            await this.Player.Start(GetPracticeInfo(), this.MediaElement!, async (config) =>
            {
                if (config.WaitForKeyPress)
                {
                    MessageBox.Show("Press OK to continue to finish your turn and proceed.");
                }
                else
                {
                    await Task.Delay(TimeSpan.FromSeconds(config.YourDurationInSeconds));
                }

                return GetPracticeInfo();       // Return updated config if needed
            });
        }
        finally
        {
            IsPlaying = false;
            OnPropertyChanged(nameof(IsPlaying));
        }
    }

    private void Stop()
    {
        this.Player.Stop();
        IsPlaying = false;
        OnPropertyChanged(nameof(IsPlaying));
    }

    private static object Pause()
    {
        return Pause();
    }

    private PracticeInfo GetPracticeInfo()
    {
        return new PracticeInfo
        {
            Chapter = this.SelectedChapterNumber,
            NumberOfParticipants = NumberOfParticipants,
            YourDurationInSeconds = YourDurationInSeconds,
            RepeatYourSloka = RepeatYourSloka,
            Sloka = 1,
            YourTurn = YourTurn,
            WaitForKeyPress = WaitMode == WaitModeOption.KeyboardHit,
            ParticipantStanzaCount = ParticipantStanzaCount,
            PlaybackSpeed = PlayingSpeed,
        };
    }
}
