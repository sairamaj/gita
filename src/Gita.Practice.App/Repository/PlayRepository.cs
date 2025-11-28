using Gita.Practice.App.Models;
using System.Diagnostics;
using System.Windows.Controls;

namespace Gita.Practice.App.Repository;

internal class PlayRepository
{
    public PlayRepository(IDataRepository dataRepository)
    {
        DataRepository = dataRepository ?? throw new ArgumentNullException(nameof(dataRepository));
    }

    public IDataRepository DataRepository { get; }


    public async Task Start(PracticeInfo practiceInfo, MediaElement mediaElement, Func<int, Task> waitForYourTurnToFinish)
    {

        var chapter = await DataRepository.Get(practiceInfo.Chapter);
        var audioFilePath = await DataRepository.GetAuditFilePath(practiceInfo.Chapter);

        mediaElement.Source = new Uri(audioFilePath);
        mediaElement.SpeedRatio = practiceInfo.PlaybackSpeed;

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

            var participantEntryCount = practiceInfo.ParticipantEntryCount;
            if (shloka.Entries.Count > 4)
            {
                participantEntryCount++;
            }

            // Pause for student turn if it's their slot
            if (participantIndex == practiceInfo.YourTurn)
            {
                await waitForYourTurnToFinish(practiceInfo.YourDurationInSeconds); 
                // Cycle participant index
                participantIndex++;
                if (participantIndex > practiceInfo.NumberOfParticipants)
                    participantIndex = 1;
                if (!practiceInfo.RepeatYourSloka)
                {
                    continue;       // if no repeat for self turn , then skip it.
                }
            }
            else
            {
                participantIndex++;
                if (participantIndex > practiceInfo.NumberOfParticipants)
                    participantIndex = 1;
            }

            // Start = first entry
            var start = TimeSpan.FromSeconds(
                double.Parse(shloka.Entries.First().StartTime)
            );

            // End = entry at ParticipantEntryCount index (1-based)
            var end = TimeSpan.FromSeconds(
                double.Parse(shloka.Entries[participantEntryCount - 1].EndTime)
            );

            // Duration
            var duration = end - start;

            // Play audio segment
            mediaElement.Position = start;
            Console.WriteLine($"Playing: {shloka.ShlokaNum}");
            mediaElement.Play();

            await Task.Delay(500); // small delay to allow mediaElement to seek
            //duration = TimeSpan.FromSeconds(1);
            await Task.Delay(duration / practiceInfo.PlaybackSpeed);
            mediaElement.Pause();
        }
    }

    public async Task StartForDebug(PracticeInfo practiceInfo, MediaElement mediaElement)
    {
        var chapter = await DataRepository.Get(practiceInfo.Chapter);
        var audioFilePath = await DataRepository.GetAuditFilePath(practiceInfo.Chapter);

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
                Trace.WriteLine($"Your turn:{shloka.ShlokaNum}");
                await Task.Delay(TimeSpan.FromSeconds(practiceInfo.YourDurationInSeconds));
                // Cycle participant index
                participantIndex++;
                if (participantIndex > practiceInfo.NumberOfParticipants)
                    participantIndex = 1;
                continue;
            }

            participantIndex++;
            if (participantIndex > practiceInfo.NumberOfParticipants)
                participantIndex = 1;

            // First and last entry define the segment
            var start = TimeSpan.FromSeconds(double.Parse(shloka.Entries.First().StartTime));
            var end = TimeSpan.FromSeconds(double.Parse(shloka.Entries.Last().EndTime));
            var duration = end - start;

            // Play audio segment
            mediaElement.Position = start;
            Trace.WriteLine($"Playing: {shloka.ShlokaNum}");
            duration = TimeSpan.FromSeconds(1); // debug duration
            await Task.Delay(duration);
        }
    }
}