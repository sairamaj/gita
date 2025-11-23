using System.Text.Json.Serialization;

namespace Gita.Practice.App.Models
{
    public class Chapter
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("chapterNum")]
        public string ChapterNum { get; set; }

        [JsonPropertyName("shloka")]
        public List<Shloka> Shlokas { get; set; }
    }
}
