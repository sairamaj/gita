using Gita.Practice.App.Models;
using DownloadProgressEventArgs = Gita.Practice.App.Models.DownloadProgressEventArgs;

namespace Gita.Practice.App.Repository;

public interface IDownloadManager
{
    event EventHandler<DownloadProgressEventArgs> DownloadProgressChanged;
    event EventHandler<ChapterDownloadedEventArgs> ChapterDownloaded;

    Task<ChapterInfo> DownloadChapterAsync(int chapter);
    Task<string> GetChapterMetaData(int chapter);
}