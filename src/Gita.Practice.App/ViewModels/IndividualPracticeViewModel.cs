using Gita.Practice.App.Models;
using Gita.Practice.App.Repository;
using Gita.Practice.App.Services;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Gita.Practice.App.ViewModels
{
    public class IndividualPracticeViewModel : BaseViewModel
    {
        private double _playningSpeed = 1.0;
        private int _yourTurn = 1;
        private int _participantStanzaCount = 1;
        private string _selectedChapterName = string.Empty;
        private WaitModeOption _waitMode = WaitModeOption.KeyboardHit;

        public ICommand PlayCommand { get; set; }
        public ICommand PauseCommand { get; }
        public ICommand StopCommand { get; }

        public IndividualPracticeViewModel(IPlayer player, IMessageDialogService messageDialogService)
        {
            PlayCommand = new RelayCommand(_ => _ = Play());
            PauseCommand = new RelayCommand(_ => Pause());
            StopCommand = new RelayCommand(_ => Stop());
            Player = player ?? throw new ArgumentNullException(nameof(player));
            MessageDialogService = messageDialogService ?? throw new ArgumentNullException(nameof(messageDialogService));
            this.HelpViewModel = new HelpViewModel("individual_practice_help.md");
        }

        public MediaElement MediaElement { get; set; }
        public bool IsPlaying { get; set; }
        public int SelectedChapterNumber { get; set; } = 1;

        public int YourDurationInSeconds { get; set; } = 20;
        public bool RepeatYourSloka { get; set; } = true;
        public string SelectedChapterName
        {
            get => this._selectedChapterName;
            set
            {
                this._selectedChapterName = value;
                OnPropertyChanged();
            }
        }

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

        public HelpViewModel HelpViewModel { get; set; }

        public IPlayer Player { get; }
        public IMessageDialogService MessageDialogService { get; }

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
                await this.Player.StartIndividualPractice(GetPracticeInfo(), this.MediaElement!, async (config) =>
                {
                    if (config.WaitForKeyPress)
                    {
                        MessageDialogService.ShowInformation("Press OK to continue to finish your turn and proceed.", "Your Turn");
                    }
                    else
                    {
                        await Task.Delay(TimeSpan.FromSeconds(config.YourDurationInSeconds));
                    }

                    return GetPracticeInfo();
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
            _ = this.Player.Stop();
            IsPlaying = false;
            OnPropertyChanged(nameof(IsPlaying));
        }

        private void Pause()
        {
            // Call Player's Pause if available
            if (this.Player is Player concretePlayer)
            {
                concretePlayer.Pause();
            }
        }

        private PracticeInfo GetPracticeInfo()
        {
            return new PracticeInfo
            {
                Chapter = this.SelectedChapterNumber,
                YourDurationInSeconds = YourDurationInSeconds,
                RepeatYourSloka = RepeatYourSloka,
                Sloka = 1,
                WaitForKeyPress = WaitMode == WaitModeOption.KeyboardHit,
                ParticipantStanzaCount = ParticipantStanzaCount,
                PlaybackSpeed = PlayingSpeed,
            };
        }
    }
}
