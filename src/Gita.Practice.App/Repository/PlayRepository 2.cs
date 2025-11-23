using Gita.Practice.App.Models;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Gita.Practice.App.Repository;

internal class PlayRepository2
{
    private MediaElement _mediaElement;
    private DispatcherTimer _timer;
    private TimeSpan _fromTime = TimeSpan.FromSeconds(629.525);
    private TimeSpan _toTime = TimeSpan.FromSeconds(642.336);

    public PlayRepository2(IDataRepository dataRepository)
    {
        DataRepository = dataRepository ?? throw new ArgumentNullException(nameof(dataRepository));
    }

    public IDataRepository DataRepository { get; }


    public async Task Start(PracticeInfo practiceInfo, MediaElement mediaElement)
    {
        var chapter = await DataRepository.Get(practiceInfo.Chapter);
        var audioFilePath = await DataRepository.GetAuditFilePath(practiceInfo.Chapter);

        mediaElement.Source = new Uri(audioFilePath);
        //mediaElement.AutoPlay = false;

        // Start from the requested sloka number
        var startIndex = chapter.Shlokas
            .FindIndex(s => s.ShlokaNum == practiceInfo.Sloka.ToString());

        if (startIndex == -1)
            startIndex = 0; // fallback to beginning

        int participantIndex = 1;

        for (int i = startIndex; i < chapter.Shlokas.Count; i++)
        {
            var shloka = chapter.Shlokas[i];
            if (shloka.Entries == null || shloka.Entries.Count == 0)
                continue;

            // Pause for student turn if it's their slot
            if (participantIndex == practiceInfo.YourTurn)
            {
                await Task.Delay(TimeSpan.FromSeconds(practiceInfo.YourDurationInSeconds));
                // Cycle participant index
                participantIndex++;
                if (participantIndex > practiceInfo.NumberOfParticipants)
                    participantIndex = 1;
                continue;
            }

            participantIndex++;

            // First and last entry define the segment
            var start = TimeSpan.FromSeconds(double.Parse(shloka.Entries.First().StartTime));
            var end = TimeSpan.FromSeconds(double.Parse(shloka.Entries.Last().EndTime));
            var duration = end - start;

            // Play audio segment
            mediaElement.Position = start;
            Console.WriteLine($"Playing: {shloka.ShlokaNum}");
            mediaElement.Play();

            //await Task.Delay(500); // small delay to allow mediaElement to seek
            await Task.Delay(duration);
            mediaElement.Pause();
        }
    }
}