using Gita.Practice.App.Models;
using System.Windows.Controls;
using System.Windows.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Gita.Practice.App.Repository;

internal class PlayRepository
{
    private MediaElement _mediaElement;
    private DispatcherTimer _timer;
    private TimeSpan _fromTime = TimeSpan.FromSeconds(629.525);
    private TimeSpan _toTime = TimeSpan.FromSeconds(642.336);

    // Flat list of segments built from chapter -> slokas -> entries
    private List<(TimeSpan Start, TimeSpan End, int SlokaIndex, int EntryIndex)> _segments;
    private int _currentSegmentIndex;

    public PlayRepository(IDataRepository dataRepository)
    {
        DataRepository = dataRepository ?? throw new ArgumentNullException(nameof(dataRepository));
    }

    public IDataRepository DataRepository { get; }

    public async Task Start(PracticeInfo practiceInfo, MediaElement mediaElement)
    {
        if (practiceInfo is null) throw new ArgumentNullException(nameof(practiceInfo));
            ArgumentNullException.ThrowIfNull(mediaElement);

            // stop any previous run
            StopExisting();

            // load chapter + audio
            var chapter = await DataRepository.Get(practiceInfo.Chapter);
            var audioFilePath = await DataRepository.GetAuditFilePath(practiceInfo.Chapter);

            if (chapter == null) throw new InvalidOperationException("Chapter not found.");
            if (string.IsNullOrWhiteSpace(audioFilePath)) throw new InvalidOperationException("Audio file path not found.");

            // build segments
            _segments = BuildSegmentsFromChapter(chapter);
            if (_segments == null || _segments.Count == 0)
            {
                // nothing to play
                return;
            }

            // determine start segment: practiceInfo.Sloka is 1-based sloka number
            int targetSlokaIndex = Math.Max(1, practiceInfo.Sloka) - 1;
            _currentSegmentIndex = _segments.FindIndex(s => s.SlokaIndex == targetSlokaIndex);
            if (_currentSegmentIndex < 0) _currentSegmentIndex = 0;

            // configure media element
            _mediaElement = mediaElement;
            _mediaElement.LoadedBehavior = MediaState.Manual;
            _mediaElement.UnloadedBehavior = MediaState.Manual;
            _mediaElement.Source = new Uri(audioFilePath, UriKind.RelativeOrAbsolute);

            // small safety: wait until media is opened to ensure NaturalDuration available
            void OnMediaOpened(object? s, EventArgs e)
            {
                // start timer after opened
                StartSegment(playImmediately: true, practiceInfo);
                _mediaElement.MediaOpened -= OnMediaOpened;
            }

            _mediaElement.MediaOpened += OnMediaOpened;
            // attempt to Play; MediaOpened handler will begin playing at the right pos
            try { _mediaElement.Play(); } catch { /* ignore play exceptions until opened */ }
        }

        private void StartSegment(bool playImmediately, PracticeInfo practiceInfo)
        {
            if (_segments == null || _segments.Count == 0) return;
            if (_currentSegmentIndex < 0 || _currentSegmentIndex >= _segments.Count)
            {
                CleanupPlayback();
                return;
            }

            var seg = _segments[_currentSegmentIndex];
            _fromTime = seg.Start;
            _toTime = seg.End;

            // position to fromTime and start play
            try
            {
                _mediaElement.Position = _fromTime;
            }
            catch { /* some MediaElement implementations may throw if not ready */ }

            if (playImmediately)
            {
                _mediaElement.Play();
            }

            // setup polling timer if not already
            if (_timer == null)
            {
                _timer = new DispatcherTimer(DispatcherPriority.Render)
                {
                    Interval = TimeSpan.FromMilliseconds(100)
                };
                _timer.Tick += (s, e) => OnTimerTick(practiceInfo);
                _timer.Start();
            }
        }

        private void OnTimerTick(PracticeInfo practiceInfo)
        {
            if (_mediaElement == null || _segments == null) return;

            // sometimes NaturalDuration isn't available; still use Position checks
            var pos = _mediaElement.Position;
            // small epsilon to avoid tight equality issues
            var epsilon = TimeSpan.FromMilliseconds(120);

            // if we've reached or passed the end of the current segment
            if (pos + epsilon >= _toTime)
            {
                var seg = _segments[_currentSegmentIndex];

                // check if this is the participant's turn (entry index compared 1-based)
                if (practiceInfo != null && (seg.EntryIndex + 1) == practiceInfo.YourTurn && practiceInfo.YourDurationInSeconds > 0)
                {
                    // pause and give the practitioner time
                    _mediaElement.Pause();

                    // one-shot timer for practice duration
                    var waitTimer = new DispatcherTimer
                    {
                        Interval = TimeSpan.FromSeconds(practiceInfo.YourDurationInSeconds)
                    };
                    waitTimer.Tick += (w, args) =>
                    {
                        waitTimer.Stop();
                        waitTimer.Tick -= null!;
                        // after wait, move to next segment and resume
                        AdvanceToNextSegment(practiceInfo);
                    };
                    waitTimer.Start();

                    // advance index so after wait we resume to next
                    _currentSegmentIndex++;
                    if (_currentSegmentIndex >= _segments.Count)
                    {
                        // no more segments after wait: cleanup
                        // but let the wait timer run to honor the practitioner duration
                    }
                    return;
                }

                // normal flow: advance to next segment immediately
                AdvanceToNextSegment(practiceInfo);
            }
        }

        private void AdvanceToNextSegment(PracticeInfo practiceInfo)
        {
            if (_segments == null) return;

            _currentSegmentIndex++;

            if (_currentSegmentIndex >= _segments.Count)
            {
                // finished all segments
                CleanupPlayback();
                return;
            }

            // start the next segment
            var next = _segments[_currentSegmentIndex];
            _fromTime = next.Start;
            _toTime = next.End;
            try
            {
                _mediaElement.Position = _fromTime;
                _mediaElement.Play();
            }
            catch
            {
                // ignore positioning exceptions; timer will attempt again
            }
        }

        private void StopExisting()
        {
            try
            {
                _timer?.Stop();
                _timer = null;
            }
            catch { }

            try
            {
                if (_mediaElement != null)
                {
                    _mediaElement.Stop();
                    _mediaElement.Source = null;
                }
            }
            catch { }

            _segments = null;
            _currentSegmentIndex = 0;
        }

        private void CleanupPlayback()
        {
            try
            {
                _timer?.Stop();
                _timer = null;
            }
            catch { }

            try
            {
                if (_mediaElement != null)
                {
                    _mediaElement.Stop();
                    // leave Source as-is or clear depending on desired behavior
                    // _mediaElement.Source = null;
                }
            }
            catch { }

            _segments = null;
            _currentSegmentIndex = 0;
        }

        private List<(TimeSpan Start, TimeSpan End, int SlokaIndex, int EntryIndex)> BuildSegmentsFromChapter(Chapter chapter)
        {
            var list = new List<(TimeSpan, TimeSpan, int, int)>();
            if (chapter?.Shlokas == null) return list;

            for (int s = 0; s < chapter.Shlokas.Count; s++)
            {
                var sloka = chapter.Shlokas[s];
                if (sloka == null) continue;

                // try to get entries property via reflection (tolerant)
                var entriesObj = GetPropertyValue(sloka, new[] { "Entries", "Entry", "entries", "entry", "EntriesList" });
                if (entriesObj is System.Collections.IEnumerable entriesEnumerable)
                {
                    int entryIndex = 0;
                    foreach (var entry in entriesEnumerable)
                    {
                        if (entry == null) { entryIndex++; continue; }

                        // find start and end values on entry using multiple common property names
                        var startObj = GetPropertyValue(entry, new[] { "Start", "start", "From", "from", "StartTime", "StartSeconds", "startSeconds" });
                        var endObj = GetPropertyValue(entry, new[] { "End", "end", "To", "to", "EndTime", "EndSeconds", "endSeconds" });

                        var startTs = ToTimeSpan(startObj);
                        var endTs = ToTimeSpan(endObj);

                        if (startTs.HasValue && endTs.HasValue && endTs.Value > startTs.Value)
                        {
                            list.Add((startTs.Value, endTs.Value, s, entryIndex));
                        }

                        entryIndex++;
                    }
                }
                else
                {
                    // If no entries collection found, attempt to treat sloka itself as one entry with Start/End
                    var startObj = GetPropertyValue(sloka, new[] { "Start", "start", "From", "from", "StartTime", "StartSeconds", "startSeconds" });
                    var endObj = GetPropertyValue(sloka, new[] { "End", "end", "To", "to", "EndTime", "EndSeconds", "endSeconds" });
                    var startTs = ToTimeSpan(startObj);
                    var endTs = ToTimeSpan(endObj);
                    if (startTs.HasValue && endTs.HasValue && endTs.Value > startTs.Value)
                    {
                        list.Add((startTs.Value, endTs.Value, s, 0));
                    }
                }
            }

            // ensure segments sorted by start time to play in order
            //list = list.OrderBy(t => t.Start).ToList();
            return list;
        }

        private static object GetPropertyValue(object obj, string[] candidateNames)
        {
            if (obj == null) return null!;
            var type = obj.GetType();
            foreach (var name in candidateNames)
            {
                var p = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p != null)
                {
                    try
                    {
                        return p.GetValue(obj) ?? null!;
                    }
                    catch { continue; }
                }
            }

            // also support fields
            foreach (var name in candidateNames)
            {
                var f = type.GetField(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (f != null)
                {
                    try
                    {
                        return f.GetValue(obj) ?? null!;
                    }
                    catch { continue; }
                }
            }

            return null!;
        }

        private static TimeSpan? ToTimeSpan(object value)
        {
            if (value == null) return null;
            switch (value)
            {
                case TimeSpan ts:
                    return ts;
                case double d:
                    return TimeSpan.FromSeconds(d);
                case float f:
                    return TimeSpan.FromSeconds(f);
                case int i:
                    return TimeSpan.FromSeconds(i);
                case long l:
                    return TimeSpan.FromSeconds(l);
                case string s:
                    // try parse numeric seconds first
                    if (double.TryParse(s, out var dd))
                    {
                        return TimeSpan.FromSeconds(dd);
                    }
                    // try TimeSpan parse
                    if (TimeSpan.TryParse(s, out var tts))
                    {
                        return tts;
                    }
                    return null;
                default:
                    // try to find numeric property (e.g., Seconds)
                    var t = value.GetType();
                    var prop = t.GetProperty("Seconds") ?? t.GetProperty("Seconds", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (prop != null)
                    {
                        try
                        {
                            var v = prop.GetValue(value);
                            if (v is double dd2) return TimeSpan.FromSeconds(dd2);
                            if (v is int ii) return TimeSpan.FromSeconds(ii);
                        }
                        catch { }
                    }
                    break;
            }
            return null;
        }
}
