namespace Gita.Practice.App.Models
{
    internal class PracticeInfo
    {
        public int Chapter { get; set; }
        public int Sloka { get; set; }
        public int NumberOfParticipants { get; set; }
        public int YourTurn { get; set; }
        public int YourDurationInSeconds { get; set; }
        public bool RepeatYourSloka { get; set; }
    }
}
