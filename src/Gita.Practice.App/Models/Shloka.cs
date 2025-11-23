using System.Text.Json.Serialization;

namespace Gita.Practice.App.Models
{
    public class Shloka
    {
        [JsonPropertyName("shlokaNum")]
        public string ShlokaNum { get; set; }

        [JsonPropertyName("entry")]
        public List<Entry> Entries { get; set; }
    }

}
