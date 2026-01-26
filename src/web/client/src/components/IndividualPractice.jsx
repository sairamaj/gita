import { useEffect, useMemo, useRef, useState } from "react";
import PracticeControls from "./PracticeControls.jsx";
import StatusBar from "./StatusBar.jsx";
import {
  extractSegments,
  pickRandomSegment,
  playSegment,
  waitForDuration,
  waitForKeyboard,
} from "../practiceUtils.js";

export default function IndividualPractice({
  chapter,
  audioUrl,
  metadata,
  isMetadataLoading,
  waitModes,
  defaults,
  speedConfig,
}) {
  const [waitMode, setWaitMode] = useState(defaults.waitMode);
  const [duration, setDuration] = useState(defaults.duration);
  const [repeatYourShloka, setRepeatYourShloka] = useState(defaults.repeatYourShloka);
  const [playbackSpeed, setPlaybackSpeed] = useState(defaults.playbackSpeed);
  const [status, setStatus] = useState("Idle");
  const [isRunning, setIsRunning] = useState(false);
  const [awaitingUser, setAwaitingUser] = useState(false);
  const [currentSegment, setCurrentSegment] = useState(null);
  const audioRef = useRef(null);
  const stopRef = useRef(false);
  const waitResolverRef = useRef(null);
  const [audioError, setAudioError] = useState("");

  const segments = useMemo(() => extractSegments(metadata), [metadata]);
  const isPlayDisabled = isRunning || isMetadataLoading || !audioUrl || !segments.length;
  const playDisabledReason = (() => {
    if (isRunning) {
      return "Practice is already running.";
    }
    if (isMetadataLoading) {
      return "Loading chapter metadata...";
    }
    if (!audioUrl) {
      return "Select a chapter to begin.";
    }
    if (!segments.length) {
      return "No shlokas loaded for this chapter.";
    }
    return "";
  })();

  useEffect(() => {
    setAudioError("");
  }, [audioUrl]);

  const ensureAudioReady = async () => {
    const audio = audioRef.current;
    if (!audio || !audioUrl) {
      return false;
    }

    if (audio.readyState >= 3) {
      return true;
    }

    setStatus("Loading audio...");

    return new Promise((resolve) => {
      const onReady = () => {
        cleanup();
        resolve(true);
      };
      const onError = () => {
        cleanup();
        setAudioError("Audio failed to load.");
        setStatus("Audio failed to load.");
        resolve(false);
      };
      const cleanup = () => {
        audio.removeEventListener("canplaythrough", onReady);
        audio.removeEventListener("error", onError);
      };

      audio.addEventListener("canplaythrough", onReady);
      audio.addEventListener("error", onError);
      audio.load();
    });
  };

  const start = async () => {
    if (!audioUrl || !segments.length || isRunning) {
      return;
    }

    stopRef.current = false;
    setIsRunning(true);
    setStatus("Starting...");

    const ready = await ensureAudioReady();
    if (!ready || stopRef.current) {
      setIsRunning(false);
      return;
    }

    while (!stopRef.current) {
      const segment = pickRandomSegment(segments);
      if (!segment) {
        setStatus("No playable shlokas");
        break;
      }

      setCurrentSegment(segment);
      setStatus("Listening");
      await playSegment(audioRef.current, segment, playbackSpeed, stopRef);

      if (stopRef.current) {
        break;
      }

      setStatus("Your turn");
      if (waitMode === "keyboard") {
        await waitForKeyboard(setAwaitingUser, waitResolverRef);
      } else {
        await waitForDuration(duration, stopRef);
      }

      if (stopRef.current) {
        break;
      }

      if (repeatYourShloka) {
        setStatus("Repeating your shloka");
        await playSegment(audioRef.current, segment, playbackSpeed, stopRef);
      }
    }

    setIsRunning(false);
    setAwaitingUser(false);
    setStatus("Stopped");
  };

  const stop = () => {
    stopRef.current = true;
    if (waitResolverRef.current) {
      waitResolverRef.current();
    }
    if (audioRef.current) {
      audioRef.current.pause();
    }
  };

  return (
    <div className="stack">
      <PracticeControls
        waitMode={waitMode}
        onWaitModeChange={setWaitMode}
        duration={duration}
        onDurationChange={setDuration}
        repeatYourShloka={repeatYourShloka}
        onRepeatChange={setRepeatYourShloka}
        playbackSpeed={playbackSpeed}
        onPlaybackSpeedChange={setPlaybackSpeed}
        waitModes={waitModes}
        speedConfig={speedConfig}
      />

      <div className="card">
        <h3>Individual Practice</h3>
        <p>
          Chapter: <strong>{chapter?.name || "Not selected"}</strong>
        </p>
        {audioError ? <p className="error">{audioError}</p> : null}
        {playDisabledReason ? <p className="hint">{playDisabledReason}</p> : null}
        <div className="controls">
          <button onClick={start} disabled={isPlayDisabled}>
            Play
          </button>
          <button onClick={stop} disabled={!isRunning && !awaitingUser}>
            Stop
          </button>
          <button
            onClick={() => waitResolverRef.current?.()}
            disabled={!awaitingUser}
          >
            OK (Finish Reciting)
          </button>
        </div>
        <StatusBar
          status={status}
          extra={currentSegment ? `Shloka ${currentSegment.shlokaNum}` : null}
        />
      </div>

      <audio
        ref={audioRef}
        src={audioUrl || undefined}
        preload="auto"
        crossOrigin="anonymous"
      />
    </div>
  );
}
