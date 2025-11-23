using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Gita.Practice.App.Services
{
    public class ChapterDownloader
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        /// <summary>
        /// Downloads the chapter JSON and m4a files for the given chapter identifier (e.g. "04").
        /// Files are saved to the user's Downloads/Gita/{chapter} directory.
        /// </summary>
        /// <param name="chapter">Chapter name or number (can contain leading slash, will be sanitized).</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        public async Task DownloadChapterFilesAsync(string chapter, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(chapter))
                throw new ArgumentException("Chapter must be provided", nameof(chapter));

            // sanitize chapter (remove leading/trailing slashes)
            var cleaned = chapter.Trim().Trim('/', '\\');

            // Base URLs
            var jsonUrl = $"https://www.sgsgitafoundation.org/bg/{cleaned}/plain_chapter.json";
            var m4aUrl = $"https://www.sgsgitafoundation.org/bg/{cleaned}/plain_chapter.m4a";

            // Determine download folder: <UserProfile>/Downloads/Gita/{chapter}
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var downloadsDir = Path.Combine(userProfile, "Downloads", "Gita", cleaned);
            Directory.CreateDirectory(downloadsDir);

            var jsonPath = Path.Combine(downloadsDir, "plain_chapter.json");
            var m4aPath = Path.Combine(downloadsDir, "plain_chapter.m4a");

            // Download JSON
            await DownloadToFileAsync(jsonUrl, jsonPath, cancellationToken).ConfigureAwait(false);

            // Download m4a
            await DownloadToFileAsync(m4aUrl, m4aPath, cancellationToken).ConfigureAwait(false);
        }

        private async Task DownloadToFileAsync(string url, string destinationPath, CancellationToken cancellationToken)
        {
            using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            // Use a temp file and then move to final to avoid partial files on failure
            var tempFile = destinationPath + ".download";
            await using (var fileStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true))
            {
                await contentStream.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);
            }

            // Replace any existing file
            File.Move(tempFile, destinationPath, overwrite: true);
        }
    }
}
