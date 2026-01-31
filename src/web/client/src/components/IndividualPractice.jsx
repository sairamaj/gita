import { useEffect, useMemo, useRef, useState } from "react";
import PracticeControls from "./PracticeControls.jsx";
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
  const [userSegment, setUserSegment] = useState(null);
  const audioRef = useRef(null);
  const stopRef = useRef(false);
  const waitResolverRef = useRef(null);
  const [audioError, setAudioError] = useState("");
  const repeatRef = useRef(repeatYourShloka);
  const durationRef = useRef(duration);
  const waitModeRef = useRef(waitMode);
  const playbackSpeedRef = useRef(playbackSpeed);

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

  useEffect(() => {
    repeatRef.current = repeatYourShloka;
  }, [repeatYourShloka]);

  useEffect(() => {
    durationRef.current = duration;
  }, [duration]);

  useEffect(() => {
    waitModeRef.current = waitMode;
  }, [waitMode]);

  useEffect(() => {
    playbackSpeedRef.current = playbackSpeed;
  }, [playbackSpeed]);

  useEffect(() => {
    if (audioRef.current) {
      audioRef.current.playbackRate = playbackSpeed > 0 ? playbackSpeed : 1;
    }
  }, [playbackSpeed]);

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
      const deviceSegment = pickRandomSegment(segments);
      if (!deviceSegment) {
        setStatus("No playable shlokas");
        break;
      }

      setCurrentSegment(deviceSegment);
      setStatus("Listening");
      await playSegment(
        audioRef.current,
        deviceSegment,
        playbackSpeedRef.current,
        stopRef
      );

      if (stopRef.current) {
        break;
      }

      const deviceIndex = segments.findIndex(
        (segment) =>
          segment.start === deviceSegment.start && segment.end === deviceSegment.end
      );
      const nextSegment =
        deviceIndex >= 0 ? segments[(deviceIndex + 1) % segments.length] : null;
      setUserSegment(nextSegment);

      setStatus("Your turn");
      if (waitModeRef.current === "keyboard") {
        await waitForKeyboard(setAwaitingUser, waitResolverRef);
      } else {
        await waitForDuration(durationRef.current, stopRef);
      }

      if (stopRef.current) {
        break;
      }

      if (repeatRef.current && nextSegment) {
        setStatus("Repeating your shloka");
        await playSegment(
          audioRef.current,
          nextSegment,
          playbackSpeedRef.current,
          stopRef
        );
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

  const statusClassName = (state) =>
    `participant-status status-${state.toLowerCase().replace(/[^a-z0-9]+/g, "-")}`;

  const participantStatuses = useMemo(() => {
    const deviceActive =
      status === "Listening" || status === "Repeating your shloka";
    const selfActive = awaitingUser;
    const deviceState =
      status === "Repeating your shloka"
        ? "RepeatingYourSholka"
        : deviceActive
          ? "Playing"
          : isRunning
            ? "Waiting"
            : "Idle";
    const selfState = selfActive
      ? "Reciting"
      : status === "Repeating your shloka"
        ? "Listening"
        : isRunning
          ? "Waiting"
          : "Idle";

    return [
      {
        id: "device",
        label: "Device",
        state: deviceState,
        isActive: deviceActive,
        isSelf: false,
      },
      {
        id: "self",
        label: "Self",
        state: selfState,
        isActive: selfActive,
        isSelf: true,
      },
    ];
  }, [awaitingUser, isRunning, status]);

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

      <div className="group-practice-layout">
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
            {waitMode !== "duration" ? (
              <button
                onClick={() => waitResolverRef.current?.()}
                disabled={!awaitingUser}
              >
                Finish Your turn
              </button>
            ) : null}
          </div>
        </div>

        <aside className="card group-practice-panel">
          <h3>Participants</h3>
          <ul className="participant-list">
            {participantStatuses.map((participant) => (
              <li
                key={participant.id}
                className={[
                  "participant-item",
                  participant.isActive ? "active" : "",
                  participant.isSelf ? "self" : "",
                ]
                  .filter(Boolean)
                  .join(" ")}
              >
                <span className="participant-label">{participant.label}</span>
                <span className={statusClassName(participant.state)}>{participant.state}</span>
              </li>
            ))}
          </ul>
        </aside>
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
