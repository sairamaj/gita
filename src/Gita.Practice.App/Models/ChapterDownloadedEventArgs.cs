namespace Gita.Practice.App.Models
{
    public class ChapterDownloadedEventArgs : EventArgs
    {
        public ChapterDownloadedEventArgs(string message)
        {
            Message = message;
        }

        public string Message { get; }
    }
}
