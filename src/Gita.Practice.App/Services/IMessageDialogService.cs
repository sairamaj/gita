namespace Gita.Practice.App.Services;

public interface IMessageDialogService
{
    void ShowError(string message, string? title = null);
    void ShowInformation(string message, string? title = null);
    void ShowWarning(string message, string? title = null);
    bool ShowQuestion(string message, string? title = null);
}

