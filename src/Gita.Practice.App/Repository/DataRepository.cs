using System.Text.Json;
using Gita.Practice.App.Models;

namespace Gita.Practice.App.Repository
{
    internal class DataRepository
    {
        public async Task<Chapter> Get(int chapter)
        {
            // JSON sample embedded as a raw string literal to preserve unicode and formatting.
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var chapterMetaData = await new DownloadManager().GetChapterMetaData(chapter);
            return JsonSerializer.Deserialize<Chapter>(chapterMetaData, options)
                       ?? throw new InvalidOperationException("Failed to deserialize chapter JSON.");
        }

        public async Task<string> GetAuditFilePath(int chapter)
        {
            var chapterInfo = await new DownloadManager().DownloadChapterAsync(chapter);
            return chapterInfo.AudioFileLocation;
        }
    }
}
