using Gita.Practice.App.Models;
using Gita.Practice.App.Repository;
using Gita.Practice.App.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Gita.Practice.App.ViewModels;

public class GroupPracticeViewModel : BaseViewModel
{
    private double _playningSpeed = 1.50;
    private int _yourTurn = 2;
    private int _participantStanzaCount = 4;
    private string _selectedChapterName = string.Empty;
    private WaitModeOption _waitMode = WaitModeOption.KeyboardHit;
    private GroupPracticeProgressViewModel _progressViewModel;
    public ICommand PlayCommand { get; set; }
    public ICommand PauseCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand DownloadCommand { get; }

    public GroupPracticeViewModel(IPlayer player, IMessageDialogService messageDialogService)
    {
        PlayCommand = new RelayCommand(_ =>
        {
            _ = Play();
        });

        PauseCommand = new RelayCommand(_ => Pause());
        StopCommand = new RelayCommand(_ => Stop());
        Player = player ?? throw new ArgumentNullException(nameof(player));
        MessageDialogService = messageDialogService ?? throw new ArgumentNullException(nameof(messageDialogService));
        _progressViewModel = new GroupPracticeProgressViewModel();
        this.HelpViewModel = new HelpViewModel("group_practice_help.md");
        UpdateProgressParticipants();
    }

    public string ChapterDisplayName { get; set; }
    public MediaElement MediaElement { get; set; }
    public bool IsPlaying { get; set; }
    public int SelectedChapterNumber { get; set; } = 1;
    public string SelectedChapterName
    {
        get => this._selectedChapterName;
        set
        {
            this._selectedChapterName = value;
            OnPropertyChanged();
        }
    }
    private int _numberOfParticipants = 4   ;
    public int NumberOfParticipants
    {
        get => _numberOfParticipants;
        set
        {
            _numberOfParticipants = value;
            OnPropertyChanged();
            UpdateProgressParticipants();
            this.YourTurn = this._yourTurn; // Re-validate your turn
        }
    }

    public GroupPracticeProgressViewModel ProgressViewModel => _progressViewModel;
    public int YourTurn
    {
        get => this._yourTurn;
        set
        {
            if (value > NumberOfParticipants)
            {
                value = NumberOfParticipants;
            }
            if (value < 1)
            {
                value = 1;
            }

            this._yourTurn = value;
            OnPropertyChanged();
            UpdateProgressParticipants();
        }
    }
    public int YourDurationInSeconds { get; set; } = 20;
    public bool RepeatYourSloka { get; set; }
    public HelpViewModel HelpViewModel { get; set; }
    public WaitModeOption WaitMode { get => _waitMode; set { _waitMode = value; OnPropertyChanged(); } }
    public int ParticipantStanzaCount
    {
        get => _participantStanzaCount;
        set
        {
            if (value > 4 || value < 1)
            {
                OnPropertyChanged();
                return;
            }
            _participantStanzaCount = value; OnPropertyChanged();
        }
    }
    public double PlayingSpeed { get => _playningSpeed; set { _playningSpeed = value; OnPropertyChanged(); } }

    public IPlayer Player { get; }
    public IMessageDialogService MessageDialogService { get; }

    private void UpdateProgressParticipants()
    {
        _progressViewModel?.UpdateParticipants(NumberOfParticipants, YourTurn);
    }

    private async Task Play()
    {
        if (this.MediaElement == null)
        {
            MessageDialogService.ShowError("MediaElement is not set.", "Configuration Error");
            return;
        }
        try
        {
            IsPlaying = true;
            OnPropertyChanged(nameof(IsPlaying));
            await this.Player.Start(GetPracticeInfo(), this.MediaElement!, (otherParticipant) => 
            {
                _progressViewModel.SetParticipantStatus(otherParticipant.Number, otherParticipant.IsReciting ? ParticipantStatus.Reciting : ParticipantStatus.Idle);
            },
            async (config) =>
            {
                _progressViewModel.SetParticipantStatus(this.YourTurn, ParticipantStatus.Reciting);
                if (config.WaitForKeyPress)
                {
                    MessageDialogService.ShowInformation("Press OK to continue to finish your turn and proceed.", "Your Turn");
                }
                else
                {
                    await Task.Delay(TimeSpan.FromSeconds(config.YourDurationInSeconds));
                }

                _progressViewModel.SetParticipantStatus(this.YourTurn, ParticipantStatus.Idle);
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
