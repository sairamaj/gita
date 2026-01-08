namespace Gita.Practice.App.Models
{
    public class StatusMessage
    {
        public StatusMessage(string type, string message)
        {
            this.Message = message;
            this.Type = type;
            this.DateTimeString = DateTime.Now.ToString("HH:mm");
        }

        public string Message { get; }
        public string Type { get; }
        public string DateTimeString { get; }
    }
}
