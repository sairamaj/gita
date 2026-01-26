export default function PracticeControls({
  waitMode,
  onWaitModeChange,
  duration,
  onDurationChange,
  repeatYourShloka,
  onRepeatChange,
  playbackSpeed,
  onPlaybackSpeedChange,
  waitModes,
  speedConfig,
}) {
  return (
    <div className="card">
      <h3>Practice Settings</h3>
      <div className="form-grid">
        <label>
          Wait Mode
          <select value={waitMode} onChange={(event) => onWaitModeChange(event.target.value)}>
            {waitModes.map((mode) => (
              <option key={mode.id} value={mode.id}>
                {mode.label}
              </option>
            ))}
          </select>
        </label>
        <label>
          Duration (seconds)
          <input
            type="number"
            miny
             ="5"
            max="120"
            step="1"
            value={duration}
            onChange={(event) => onDurationChange(Number(event.target.value))}
            disabled={waitMode !== "duration"}
          />
        </label>
        <label className="checkbox">
          <input
            type="checkbox"
            checked={repeatYourShloka}
            onChange={(event) => onRepeatChange(event.target.checked)}
          />
          Repeat your shloka
        </label>
        <label>
          Playback Speed ({playbackSpeed.toFixed(1)}x)
          <input
            type="range"
            min={speedConfig.min}
            max={speedConfig.max}
            step={speedConfig.step}
            value={playbackSpeed}
            onChange={(event) => onPlaybackSpeedChange(Number(event.target.value))}
          />
        </label>
      </div>
    </div>
  );
}
