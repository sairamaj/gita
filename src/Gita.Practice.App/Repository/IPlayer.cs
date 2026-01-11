using Gita.Practice.App.Models;
using System.Windows.Controls;

namespace Gita.Practice.App.Repository
{
    public interface IPlayer
    {
        Task StartGroupPractice(
            PracticeInfo practiceInfo, 
            MediaElement mediaElement,
            Action<OtherParticipantInfo> onOtherParticipant,
            Func<PracticeInfo, Task<PracticeInfo>> waitForYourTurnToFinish);
        Task StartIndividualPractice(PracticeInfo practiceInfo, MediaElement mediaElement, Func<PracticeInfo, Task<PracticeInfo>> waitForYourTurnToFinish);
        Task Stop();
    }
}
