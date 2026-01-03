using Gita.Practice.App.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace Gita.Practice.App.ViewModels;

public class GroupPracticeProgressViewModel : BaseViewModel
{
    private ObservableCollection<ParticipantStatusViewModel> _participants = new();

    public GroupPracticeProgressViewModel()
    {
    }

    public ObservableCollection<ParticipantStatusViewModel> Participants
    {
        get => _participants;
        set => SetProperty(ref _participants, value);
    }

    public void UpdateParticipants(int numberOfParticipants, int yourTurn)
    {
        _participants.Clear();

        // Add other participants (participant-1, participant-2, etc.)
        for (int i = 1; i < yourTurn; i++)
        {
            string participantName = $"participant-{i}";
            bool isSelf = (i == yourTurn);
            var participant = new ParticipantStatusViewModel(participantName, isSelf);
            _participants.Add(participant);
        }

        // Add "you" as a separate entry
        var selfParticipant = new ParticipantStatusViewModel("you", true);
        _participants.Add(selfParticipant);

        for (int i = yourTurn; i < numberOfParticipants; i++)
        {
            string participantName = $"participant-{i}";
            bool isSelf = (i == yourTurn);
            var participant = new ParticipantStatusViewModel(participantName, isSelf);
            _participants.Add(participant);
        }

        OnPropertyChanged(nameof(Participants));
    }

    public void SetParticipantStatus(int participantNumber, ParticipantStatus status)
    {
        if (participantNumber > 0 && participantNumber <= _participants.Count)
        {
            var participant = _participants.FirstOrDefault(p => p.Name == $"participant-{participantNumber}");
            if (participant != null)
            {
                participant.Status = status;
            }
        }
    }

    public void SetSelfStatus(ParticipantStatus status)
    {
        var selfParticipant = _participants.FirstOrDefault(p => p.IsSelf && p.Name == "you");
        if (selfParticipant != null)
        {
            selfParticipant.Status = status;
        }
    }

    public void SetAllStatus(ParticipantStatus status)
    {
        foreach (var participant in _participants)
        {
            participant.Status = status;
        }
    }
}

