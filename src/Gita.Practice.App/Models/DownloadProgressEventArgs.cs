namespace Gita.Practice.App.Models;

public class DownloadProgressEventArgs : EventArgs
{
    public DownloadProgressEventArgs(string message)
    {
        this.Message = message;
    }

    public string Message { get; }
}
