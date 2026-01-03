using System;

namespace Gita.Practice.App.Models
{
    public class ChapterInfo
    {
        public ChapterInfo(string audioFileLocation, string dataFileLocation)
        {
            AudioFileLocation = audioFileLocation ?? throw new ArgumentNullException(nameof(audioFileLocation));
            DataFileLocation = dataFileLocation ?? throw new ArgumentNullException(nameof(dataFileLocation));
        }
        public string AudioFileLocation { get; private set; }
        public string DataFileLocation { get; private set; }
    }
}
