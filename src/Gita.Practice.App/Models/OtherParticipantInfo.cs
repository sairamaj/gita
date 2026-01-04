namespace Gita.Practice.App.Models
{
    public class OtherParticipantInfo
    {
        public OtherParticipantInfo(int number, bool isReciting)
        {
            this.Number = number;
            this.IsReciting = isReciting;
        }

        public int Number { get; }
        public bool IsReciting { get; }
    }
}
