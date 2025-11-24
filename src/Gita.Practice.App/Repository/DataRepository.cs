using System.Text.Json;
using Gita.Practice.App.Models;

namespace Gita.Practice.App.Repository
{
    internal class DataRepository : IDataRepository
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

        public IEnumerable<Tuple<string, int>> GetAllChapters()
        {
            return new List<Tuple<string, int>>
            {
                new Tuple<string,int>("Arjuna Vishadha Yoga", 1),
                new Tuple<string,int>("Sankhya Yoga", 2),
                new Tuple<string,int>("Karma Yoga", 3),
                new Tuple<string,int>("Jnana Yoga", 4),
                new Tuple<string,int>("Karma Sanyasa Yoga", 5),
            };
        }

        public async Task<string> GetAuditFilePath(int chapter)
        {
            var chapterInfo = await new DownloadManager().DownloadChapterAsync(chapter);
            return chapterInfo.AudioFileLocation;
        }
    }
}
