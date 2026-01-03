using Gita.Practice.App.Models;

namespace Gita.Practice.App.ViewModels;

public class ParticipantStatusViewModel : BaseViewModel
{
    private string _name = string.Empty;
    private ParticipantStatus _status = ParticipantStatus.Idle;
    private bool _isSelf = false;

    public ParticipantStatusViewModel(string name, bool isSelf = false)
    {
        _name = name;
        _isSelf = isSelf;
    }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public ParticipantStatus Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public bool IsSelf
    {
        get => _isSelf;
        set => SetProperty(ref _isSelf, value);
    }

    public string StatusText => Status switch
    {
        ParticipantStatus.Idle => "idle",
        ParticipantStatus.Reciting => "reciting",
        ParticipantStatus.Waiting => "waiting",
        ParticipantStatus.Completed => "completed",
        _ => "idle"
    };
}

