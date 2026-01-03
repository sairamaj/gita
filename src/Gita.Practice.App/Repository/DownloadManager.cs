using Gita.Practice.App.Models;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Gita.Practice.App.Repository
{
    internal class DownloadManager : IDownloadManager
    {
        private static readonly HttpClient httpClient = new HttpClient();

        // 🔒 Private constants for URL structure
        private const string BaseUrl = "https://www.sgsgitafoundation.org/bg/";
        private const string JsonFileName = "plain_chapter.json";
        private const string AudioFileName = "plain_chapter.m4a";

        public event EventHandler<DownloadProgressEventArgs> DownloadProgressChanged;
        public event EventHandler<ChapterDownloadedEventArgs> ChapterDownloaded;

        public string DownloadPath => Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads", "Gita");

        /// <summary>
        /// Downloads the JSON and audio files for a given chapter into
        /// Downloads/Gita/{chapterName} directory. Skips files if already downloaded.
        /// </summary>
        /// <param name="chapterName">Chapter identifier (e.g. "04")</param>
        public async Task<ChapterInfo> DownloadChapterAsync(int chapter)
        {
            var chapterName = $"{chapter:00}";
            // Build target directory path
            string downloadsPath = Path.Combine(this.DownloadPath, chapterName);

            Directory.CreateDirectory(downloadsPath);

            // Build URLs using constants
            string jsonUrl = $"{BaseUrl}{chapterName}/{JsonFileName}";
            string audioUrl = $"{BaseUrl}{chapterName}/{AudioFileName}";

            // Local file paths
            string jsonFilePath = Path.Combine(downloadsPath, JsonFileName);
            string audioFilePath = Path.Combine(downloadsPath, AudioFileName);


            // Download both files asynchronously (skip if already exists)
            var jsonFileName = await DownloadFileAsync($"metadata for chapter-{chapterName}", jsonUrl, jsonFilePath);
            var auditFileName = await DownloadFileAsync($"audio for chapter-{chapterName}", audioUrl, audioFilePath);

            return new ChapterInfo(auditFileName, jsonFileName);
        }

        public async Task<string> GetChapterMetaData(int chapter)
        {
            var chapterInfo = await this.DownloadChapterAsync(chapter);
            return File.ReadAllText(chapterInfo.DataFileLocation);
        }

        private async Task<string> DownloadFileAsync(string description, string url, string destinationPath)
        {
            if (File.Exists(destinationPath))
            {
                Console.WriteLine($"Skipping download, file already exists: {destinationPath}");
                return destinationPath;
            }

            this.DownloadProgressChanged?.Invoke(this, new DownloadProgressEventArgs($"downloading:{description}"));

            try
            {
                using (HttpResponseMessage response = await httpClient.GetAsync(url))
                {
                    response.EnsureSuccessStatusCode();
                    byte[] content = await response.Content.ReadAsByteArrayAsync();
                    await File.WriteAllBytesAsync(destinationPath, content);
                }
                this.ChapterDownloaded?.Invoke(this, new ChapterDownloadedEventArgs($"downloaded:{description}"));
            }
            catch (Exception ex)
            {
                this.DownloadProgressChanged?.Invoke(this, new DownloadProgressEventArgs($"error:{description}:{ex.Message}"));
                throw;
            }

            return destinationPath;
        }
    }
}
