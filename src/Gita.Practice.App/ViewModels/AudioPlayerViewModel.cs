using Gita.Practice.App.Repository;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Gita.Practice.App.ViewModels;

public class AudioPlayerViewModel : BaseViewModel
{
    private Uri _audioSource;
    public Uri AudioSource
    {
        get => _audioSource;
        set { _audioSource = value; OnPropertyChanged(); }
    }

    public ICommand PlayCommand { get; }
    public ICommand PauseCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand DownloadCommand { get; }

    private MediaElement _mediaElement;
    private readonly DispatcherTimer _timer;


    // Segment playback fields
    private DispatcherTimer? _segmentTimer;
    private TimeSpan? _segmentStart;
    private TimeSpan? _segmentEnd;
    private const int TimerIntervalMs = 200;

    private TimeSpan _fromTime = TimeSpan.FromSeconds(629.525);
    private TimeSpan _toTime = TimeSpan.FromSeconds(642.336);

    public AudioPlayerViewModel(MediaElement mediaElement)
    {
        _mediaElement = mediaElement;

        PlayCommand = new RelayCommand(_ =>
        {
            PlayRange();
        });

        PauseCommand = new RelayCommand(_ => Pause());
        StopCommand = new RelayCommand(_ => Stop());
        DownloadCommand = new RelayCommand(_ => { Download(); });

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(20)
        };
        _timer.Tick += Timer_Tick;

    }
    private void PlayRange()
    {
        _mediaElement.Position = _fromTime;
        _mediaElement.Play();
        _timer.Start();
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
        if (_mediaElement.Position >= _toTime)
        {
            _mediaElement.Stop();
            _timer.Stop();
        }
    }

    private async void Download()
    {
        var dataRepository = new DataRepository();
        var chapter = await dataRepository.Get(7);

        AudioSource = new Uri(await dataRepository.GetAuditFilePath(7), UriKind.Relative);
        _mediaElement.Source = AudioSource;
        var sloka = chapter.Shlokas.Skip(2).FirstOrDefault();
        this._fromTime = TimeSpan.FromSeconds(Convert.ToDouble( sloka.Entries.First().StartTime));
        this._toTime = TimeSpan.FromSeconds(Convert.ToDouble(sloka.Entries.Last().EndTime));

        this.PlayRange();
        Console.WriteLine(chapter.Name);
    }

    // Play current media (normal play). If a segment is active, ensure timer runs so end is enforced.
    private void Play()
    {
        if (_mediaElement == null) return;

        // If a segment is requested and the media is already opened, start the segment playback immediately.
        if (_segmentStart.HasValue && _segmentEnd.HasValue && _mediaElement.NaturalDuration.HasTimeSpan)
        {
            try
            {
                StartPlaybackAt(_segmentStart.Value, _segmentEnd.Value);
            }
            catch
            {
                // Swallow exceptions to preserve original behavior (best-effort seeking/playback).
            }
            return;
        }

        // Ensure the MediaElement has its source so Play triggers loading/opening on some builds.
        if (_mediaElement.Source == null && AudioSource != null)
            _mediaElement.Source = AudioSource;

        // Normal play (resume or start full playback). If a segment was requested but the media
        // is not yet opened, we rely on the MediaOpened handler attached when PlayRange was called
        // to call StartPlaybackAt once the media finishes opening.
        _mediaElement.Play();

        // If a segment end is set and we are before the end, ensure the timer enforces the end.
        try
        {
            if (_segmentEnd.HasValue && _mediaElement.Position < _segmentEnd.Value)
            {
                EnsureSegmentTimer();
                _segmentTimer?.Start();
            }
        }
        catch
        {
            // Swallow exceptions from reading Position on rare occasions.
        }
    }

    private void Pause()
    {
        _mediaElement?.Pause();
        _segmentTimer?.Stop();
    }

    private void Stop()
    {
        _mediaElement?.Stop();
        StopSegmentTimer();
        // Optionally clear requested segment so next Play is full playback
        _segmentStart = null;
        _segmentEnd = null;
    }

    // Public helper to play a range [from, to). Will seek to 'from' and stop when reaching 'to'.
    public void PlayRange(TimeSpan from, TimeSpan to)
    {
        if (_mediaElement == null) return;
        if (to <= from) return; // invalid range

        _segmentStart = from;
        _segmentEnd = to;

        // Ensure the MediaElement has the source assigned
        if (_mediaElement.Source == null && AudioSource != null)
            _mediaElement.Source = AudioSource;

        // If media is already opened, we can start immediately.
        if (_mediaElement.NaturalDuration.HasTimeSpan)
        {
            StartPlaybackAt(from, to);
            return;
        }

        // Otherwise wait for MediaOpened
        RoutedEventHandler? openedHandler = null;
        openedHandler = (s, e) =>
        {
            _mediaElement.MediaOpened -= openedHandler;
            // Start after the media is opened
            StartPlaybackAt(from, to);
        };
        _mediaElement.MediaOpened += openedHandler;

        // If the media was not loaded, calling Play triggers loading on some builds.
        // Do not call Play here unconditionally; StartPlaybackAt will call Play when ready.
    }

    private void StartPlaybackAt(TimeSpan from, TimeSpan to)
    {
        if (_mediaElement == null) return;

        // Clamp `to` to duration if available
        if (_mediaElement.NaturalDuration.HasTimeSpan)
        {
            var duration = _mediaElement.NaturalDuration.TimeSpan;
            if (from > duration) return;
            if (to > duration) to = duration;
            _segmentEnd = to;
        }

        // Seek to `from`. Seeking must happen on UI thread.
        try
        {
            _mediaElement.Dispatcher.Invoke(() => _mediaElement.Position = from);
        }
        catch
        {
            // Some seeks may fail if element not ready; best-effort only.
        }

        // Start playback and timer
        _mediaElement.Dispatcher.Invoke(() =>
        {
            _mediaElement.Play();
            EnsureSegmentTimer();
            _segmentTimer?.Start();
        });
    }

    private void EnsureSegmentTimer()
    {
        if (_mediaElement == null) return;

        if (_segmentTimer == null)
        {
            _segmentTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(TimerIntervalMs), DispatcherPriority.Normal, SegmentTimer_Tick, _mediaElement.Dispatcher);
        }
    }

    private void SegmentTimer_Tick(object? sender, EventArgs e)
    {
        if (_mediaElement == null || !_segmentEnd.HasValue) return;

        try
        {
            var pos = _mediaElement.Position;
            // If current position reached or exceeded end, stop playback and timer.
            if (pos >= _segmentEnd.Value)
            {
                _mediaElement.Stop();
                StopSegmentTimer();
                // Optionally reset position to start or leave at end
                // _mediaElement.Position = _segmentStart ?? TimeSpan.Zero;
            }
        }
        catch
        {
            // Swallow exceptions from reading Position on rare occasions.
        }
    }

    private void StopSegmentTimer()
    {
        if (_segmentTimer == null) return;
        _segmentTimer.Stop();
        _segmentTimer.Tick -= SegmentTimer_Tick;
        _segmentTimer = null;
    }
}
