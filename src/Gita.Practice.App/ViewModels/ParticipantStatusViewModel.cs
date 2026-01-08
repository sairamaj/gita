using Gita.Practice.App.Models;

namespace Gita.Practice.App.ViewModels;

public class ParticipantStatusViewModel : BaseViewModel
{
    private string _name = string.Empty;
    private ParticipantStatus _status = ParticipantStatus.Idle;
    private bool _isSelf = false;
    private int? _participantNumber = null;

    public ParticipantStatusViewModel(string name, bool isSelf = false, int? participantNumber = null)
    {
        _name = name;
        _isSelf = isSelf;
        _participantNumber = participantNumber;
    }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public ParticipantStatus Status
    {
        get => _status;
        set
        {
            SetProperty(ref _status, value);
            OnPropertyChanged(nameof(StatusText));
        }
    }

    public bool IsSelf
    {
        get => _isSelf;
        set => SetProperty(ref _isSelf, value);
    }

    public int? ParticipantNumber
    {
        get => _participantNumber;
        set => SetProperty(ref _participantNumber, value);
    }

    public string StatusText => Status switch
    {
        ParticipantStatus.Idle => "waiting",
        ParticipantStatus.Reciting => "reciting",
        ParticipantStatus.Waiting => "waiting",
        ParticipantStatus.Completed => "completed",
        _ => "waiting"
    };
}

