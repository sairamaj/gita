namespace Gita.Practice.App.ViewModels
{
    public class ChapterViewModel
    {
        public ChapterViewModel(string Name, int number)
        {
            this.Name = Name;
            this.Number = number;
        }

        public string Name { get; }
        public int Number { get; }
    }
}
