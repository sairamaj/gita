using System.Text.Json.Serialization;

namespace Gita.Practice.App.Models
{
    public class Entry
    {
        [JsonPropertyName("startTime")]
        public string StartTime { get; set; }

        [JsonPropertyName("endTime")]
        public string EndTime { get; set; }

        [JsonPropertyName("swhtsp")]
        public string Swhtsp { get; set; }

        [JsonPropertyName("shlNbr")]
        public string ShlNbr { get; set; }

        [JsonPropertyName("sty")]
        public string Sty { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }
    }

}
