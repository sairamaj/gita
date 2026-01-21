using System.Text.Json;
using Gita.Practice.App.Models;

namespace Gita.Practice.App.Repository
{
    internal class DataRepository : IDataRepository
    {
        public DataRepository(IDownloadManager downloadManager)
        {
            DownloadManager = downloadManager ?? throw new ArgumentNullException(nameof(downloadManager));
        }

        public IDownloadManager DownloadManager { get; }

        public async Task<Chapter> Get(int chapter)
        {
            // JSON sample embedded as a raw string literal to preserve unicode and formatting.
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var chapterMetaData = await this.DownloadManager.GetChapterMetaData(chapter);
            return JsonSerializer.Deserialize<Chapter>(chapterMetaData, options)
                       ?? throw new InvalidOperationException("Failed to deserialize chapter JSON.");
        }

        public IEnumerable<Tuple<string, int>> GetAllChapters()
        {
            return new List<Tuple<string, int>>
            {
                new Tuple<string,int>("Gita Dhayna Slokas", 0),
                new Tuple<string,int>("Arjuna Vishadha Yoga", 1),
                new Tuple<string,int>("Sankhya Yoga", 2),
                new Tuple<string,int>("Karma Yoga", 3),
                new Tuple<string,int>("Jnana Yoga", 4),
                new Tuple<string,int>("Karma Sanyasa Yoga", 5),
                new Tuple<string,int>("Ātma-Saṁyama Yoga", 6),
                new Tuple<string,int>("Jnana Vijnana Yoga", 7),
                new Tuple<string,int>("Aksara Brahma Yoga", 8),
                new Tuple<string,int>("Raja Vidya Raja Guhya Yoga", 9),
                new Tuple<string,int>("Vibhuti Yoga", 10),
                new Tuple<string,int>("Visvarupa Darsana Yoga", 11),
                new Tuple<string,int>("Bhakti Yoga", 12),
                new Tuple<string,int>("Ksetra Ksetrajna Vibhaga Yoga", 13),
                new Tuple<string,int>("Gunatraya Vibhaga Yoga", 14),
                new Tuple<string,int>("Purushottama Yoga", 15),
                new Tuple<string,int>("Daivasura Sampad Vibhaga Yoga", 16),
                new Tuple<string,int>("Sraddhatraya Vibhaga Yoga", 17),
                new Tuple<string,int>("Moksha Sanyasa Yoga", 18),
            };
        }

        public async Task<string> GetAuditFilePath(int chapter)
        {
            var chapterInfo = await this.DownloadManager.DownloadChapterAsync(chapter);
            return chapterInfo.AudioFileLocation;
        }
    }
}
