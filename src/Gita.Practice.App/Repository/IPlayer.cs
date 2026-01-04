using Gita.Practice.App.Models;
using System.Windows.Controls;

namespace Gita.Practice.App.Repository
{
    public interface IPlayer
    {
        Task Start(
            PracticeInfo practiceInfo, 
            MediaElement mediaElement,
            Action<OtherParticipantInfo> onOtherParticipant,
            Func<PracticeInfo, Task<PracticeInfo>> waitForYourTurnToFinish);
        Task StartWithRandom(PracticeInfo practiceInfo, MediaElement mediaElement, Func<PracticeInfo, Task<PracticeInfo>> waitForYourTurnToFinish);
        Task Stop();
    }
}
