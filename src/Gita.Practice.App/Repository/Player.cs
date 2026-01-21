using Gita.Practice.App.Models;
using System.Windows.Controls;
using System.Linq;

namespace Gita.Practice.App.Repository;

public class Player : IPlayer
{
    private CancellationTokenSource _cts;
    private MediaElement _mediaElement;

    private bool _isPaused = false;
    private TaskCompletionSource<bool> _resumeTcs;

    public Player(IDataRepository dataRepository)
    {
        DataRepository = dataRepository ?? throw new ArgumentNullException(nameof(dataRepository));
    }

    public IDataRepository DataRepository { get; }

    private async Task WaitIfPausedAsync()
    {
        while (_isPaused)
        {
            if (_resumeTcs == null)
                _resumeTcs = new TaskCompletionSource<bool>();

            await _resumeTcs.Task;
        }
    }

    private async Task<(Chapter chapter, int startIndex, TimeSpan chapterEndTime)> InitializeAsync(PracticeInfo practiceInfo, MediaElement mediaElement)
    {
        _cts = new CancellationTokenSource();
        this._mediaElement = mediaElement ?? throw new ArgumentNullException(nameof(mediaElement));

        var chapter = await DataRepository.Get(practiceInfo.Chapter);
        var audioFilePath = await DataRepository.GetAuditFilePath(practiceInfo.Chapter);

        _mediaElement.Source = new Uri(audioFilePath);
        _mediaElement.SpeedRatio = practiceInfo.PlaybackSpeed;

        var startIndex = chapter.Shlokas.FindIndex(s => s.ShlokaNum == practiceInfo.Sloka.ToString());
        if (startIndex == -1)
            startIndex = 0;

        var chapterEndTime = TimeSpan.FromSeconds(double.Parse(chapter.Shlokas.Last().Entries.Last().EndTime));

        return (chapter, startIndex, chapterEndTime);
    }

    private async Task PlaySegment(TimeSpan start, TimeSpan end, PracticeInfo practiceInfo, CancellationToken token)
    {
        // apply current playback speed
        _mediaElement.SpeedRatio = practiceInfo.PlaybackSpeed;

        var duration = end - start;

        _mediaElement.Position = start;
        _mediaElement.Play();

        await Task.Delay(500, token);
        await WaitIfPausedAsync();

        await Task.Delay(duration / practiceInfo.PlaybackSpeed, token);
        await WaitIfPausedAsync();

        _mediaElement.Pause();
    }

    public async Task StartGroupPractice(
        PracticeInfo practiceInfo,
        MediaElement mediaElement,
        Action<OtherParticipantInfo> onOtherParticipant,
        Func<PracticeInfo, Task<PracticeInfo>> waitForYourTurnToFinish)
    {
        var (chapter, startIndex, chapterEndTime) = await InitializeAsync(practiceInfo, mediaElement);
        var token = _cts.Token;

        int participantIndex = 1;
        var currentPlayingParticipant = participantIndex;

        for (int i = startIndex; i < chapter.Shlokas.Count - 1; i++)
        {
            if (token.IsCancellationRequested)
                break;

            await WaitIfPausedAsync();

            var shloka = chapter.Shlokas[i];
            if (shloka.Entries == null || shloka.Entries.Count == 0)
                continue;

            var participantStanzaCount = practiceInfo.ParticipantStanzaCount;
            if (shloka.Entries.Count > 4)
                participantStanzaCount++;

            if (participantIndex == practiceInfo.YourTurn)
            {
                practiceInfo = await waitForYourTurnToFinish(practiceInfo);
                _mediaElement.SpeedRatio = practiceInfo.PlaybackSpeed;
                participantIndex++;
                if (participantIndex > practiceInfo.NumberOfParticipants)
                    participantIndex = 1;

                if (!practiceInfo.RepeatYourSloka)
                    continue;
            }
            else
            {
                currentPlayingParticipant = participantIndex;
                onOtherParticipant(new OtherParticipantInfo(currentPlayingParticipant, true));
                participantIndex++;
                if (participantIndex > practiceInfo.NumberOfParticipants)
                    participantIndex = 1;
            }

            var start = TimeSpan.FromSeconds(double.Parse(shloka.Entries.First().StartTime));
            var actualEntryCount = Math.Min(shloka.Entries.Count, participantStanzaCount);
            var endValue = double.Parse(shloka.Entries[actualEntryCount - 1].EndTime);
            endValue = Math.Floor(endValue);
            var end = TimeSpan.FromSeconds(endValue);

            // If this is first shloka to be played, start from 0 (prayer too)
            if (i == 1)
            {
                start = TimeSpan.Zero;
            }

            if (i == chapter.Shlokas.Count - 2)
            {
                end = chapterEndTime;
            }

            await PlaySegment(start, end, practiceInfo, token);
            await Task.Delay(500);      // give little gap for the next sloka.
            onOtherParticipant(new OtherParticipantInfo(currentPlayingParticipant, false));
        }
    }

    public Task Stop()
    {
        _cts?.Cancel();
        _isPaused = false;
        _resumeTcs?.TrySetResult(true);

        if (_mediaElement != null)
        {
            _mediaElement.Stop();
            _mediaElement.Position = TimeSpan.Zero;
        }

        return Task.CompletedTask;
    }

    public void Pause()
    {
        if (_isPaused)
            return;

        _isPaused = true;
        _resumeTcs = new TaskCompletionSource<bool>();
        _mediaElement?.Pause();
    }

    public void Resume()
    {
        if (!_isPaused)
            return;

        _isPaused = false;
        _mediaElement?.Play();
        _resumeTcs?.TrySetResult(true);
    }

    public async Task StartIndividualPractice(PracticeInfo practiceInfo, MediaElement mediaElement, Func<PracticeInfo, Task<PracticeInfo>> waitForYourTurnToFinish)
    {
        var (chapter, startIndex, chapterEndTime) = await InitializeAsync(practiceInfo, mediaElement);
        var token = _cts.Token;

        // Build a list of indices to play (mirror Start: up to Count-2 inclusive)
        var lastPlayableIndex = chapter.Shlokas.Count - 2; // because Start loops while i < Count-1
        if (lastPlayableIndex < startIndex)
            return; // nothing to play

        var indices = Enumerable.Range(startIndex, lastPlayableIndex - startIndex + 1).ToList();

        // Shuffle indices
        var rnd = new Random();
        indices = indices.OrderBy(_ => rnd.Next()).ToList();

        foreach (var i in indices)
        {
            if (token.IsCancellationRequested)
                break;

            await WaitIfPausedAsync();

            var shloka = chapter.Shlokas[i];
            if (shloka.Entries == null || shloka.Entries.Count == 0)
                continue;

            // Play entire shloka for other participant first
            var start = TimeSpan.FromSeconds(double.Parse(shloka.Entries.First().StartTime));
            var rawEnd = Math.Floor(double.Parse(shloka.Entries.Last().EndTime));
            var end = TimeSpan.FromSeconds(rawEnd);

            // If this is first shloka to be played, start from 0
            if (i == 1)
            {
                start = TimeSpan.Zero;
            }

            if (i == chapter.Shlokas.Count - 2)
            {
                end = chapterEndTime;
            }


            await PlaySegment(start, end, practiceInfo, token);

            // Now give self a chance to recite the full shloka
            practiceInfo = await waitForYourTurnToFinish(practiceInfo);
            _mediaElement.SpeedRatio = practiceInfo.PlaybackSpeed;

            if (!practiceInfo.RepeatYourSloka)
                continue;

            var selfSlokaIndex = i + 1;
            if (selfSlokaIndex == chapter.Shlokas.Count-1)
            {
                selfSlokaIndex = 0;
            }

            (start, end) = GetStartEndTimes(chapter, selfSlokaIndex, practiceInfo);
            await PlaySegment(start, end, practiceInfo, token);
            await Task.Delay(500);      // give little gap for the next sloka.
        }
    }

    // PSEUDOCODE / PLAN (detailed):
    // - Method: GetStartEndTimes
    // - Inputs:
    //   - Chapter chapter: chapter model containing Shlokas and their Entries
    //   - int index: index of the shloka within chapter.Shlokas to compute times for
    //   - PracticeInfo practiceInfo: used only when computing participant-based end time
    //   - bool useParticipantEntryCount: when true, compute end using participant stanza count
    //   - TimeSpan? chapterEndTime: optional precomputed chapter end time; if null derive from chapter last entry
    // - Steps:
    //   1. Validate inputs: chapter not null, index in range.
    //   2. Get shloka = chapter.Shlokas[index]. If shloka or its Entries are null/empty return (TimeSpan.Zero, TimeSpan.Zero).
    //   3. Compute start:
    //      - Parse first entry StartTime to seconds.
    //      - If index == 1 then start = TimeSpan.Zero (prayer case).
    //   4. Compute end:
    //      - If useParticipantEntryCount == true:
    //          - participantEntryCount = practiceInfo.ParticipantStanzaCount
    //          - if shloka.Entries.Count > 4 then participantEntryCount++
    //          - endEntryIndex = participantEntryCount - 1
    //      - Else:
    //          - endEntryIndex = shloka.Entries.Count - 1
    //      - Clamp endEntryIndex into valid range [0, shloka.Entries.Count - 1].
    //      - Parse the selected entry's EndTime to seconds to obtain end.
    //   5. If index == chapter.Shlokas.Count - 2 then set end = chapterEndTime (derived if null).
    //   6. Return (start, end).
    //
    // - The method mirrors the logic used by Start and StartWithRandom for computing start and end times.

    private (TimeSpan start, TimeSpan end) GetStartEndTimes(Chapter chapter, int index, PracticeInfo practiceInfo, bool useParticipantEntryCount = false, TimeSpan? chapterEndTime = null)
    {
        if (chapter == null) throw new ArgumentNullException(nameof(chapter));
        if (chapter.Shlokas == null) throw new ArgumentException("Chapter has no shlokas", nameof(chapter));
        if (index < 0 || index >= chapter.Shlokas.Count) throw new ArgumentOutOfRangeException(nameof(index));

        var shloka = chapter.Shlokas[index];
        if (shloka == null || shloka.Entries == null || shloka.Entries.Count == 0)
        {
            return (TimeSpan.Zero, TimeSpan.Zero);
        }

        // Start time: first entry start (unless this is the special prayer index == 1)
        TimeSpan start;
        if (index == 1)
        {
            start = TimeSpan.Zero;
        }
        else
        {
            start = TimeSpan.FromSeconds(double.Parse(shloka.Entries.First().StartTime));
        }

        // Determine which entry's EndTime to use for end
        int endEntryIndex;
        if (useParticipantEntryCount)
        {
            if (practiceInfo == null) throw new ArgumentNullException(nameof(practiceInfo));
            var participantEntryCount = practiceInfo.ParticipantStanzaCount;
            if (shloka.Entries.Count > 4)
                participantEntryCount++;

            endEntryIndex = participantEntryCount - 1;
        }
        else
        {
            endEntryIndex = shloka.Entries.Count - 1;
        }

        // Clamp to valid range
        endEntryIndex = Math.Max(0, Math.Min(endEntryIndex, shloka.Entries.Count - 1));

        TimeSpan end = TimeSpan.FromSeconds(double.Parse(shloka.Entries[endEntryIndex].EndTime));

        // If this is the penultimate shloka (loop in Start uses i < Count-1), use chapter end time
        var penultimateIndex = chapter.Shlokas.Count - 2;
        if (index == penultimateIndex)
        {
            var ce = chapterEndTime ?? TimeSpan.FromSeconds(double.Parse(chapter.Shlokas.Last().Entries.Last().EndTime));
            end = ce;
        }

        return (start, end);
    }
}