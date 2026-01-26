import { useEffect, useMemo, useRef, useState } from "react";
import PracticeControls from "./PracticeControls.jsx";
import StatusBar from "./StatusBar.jsx";
import {
  extractSegments,
  playSegment,
  waitForDuration,
  waitForKeyboard,
} from "../practiceUtils.js";

export default function GroupPractice({
  chapter,
  audioUrl,
  metadata,
  waitModes,
  defaults,
  speedConfig,
}) {
  const [waitMode, setWaitMode] = useState(defaults.waitMode);
  const [duration, setDuration] = useState(defaults.duration);
  const [repeatYourShloka, setRepeatYourShloka] = useState(defaults.repeatYourShloka);
  const [playbackSpeed, setPlaybackSpeed] = useState(defaults.playbackSpeed);
  const [participants, setParticipants] = useState(defaults.participants);
  const [yourTurn, setYourTurn] = useState(defaults.yourTurn);
  const [stanzaCount, setStanzaCount] = useState(defaults.stanzaCount);
  const [status, setStatus] = useState("Idle");
  const [isRunning, setIsRunning] = useState(false);
  const [awaitingUser, setAwaitingUser] = useState(false);
  const [currentSegment, setCurrentSegment] = useState(null);
  const [currentParticipant, setCurrentParticipant] = useState(null);
  const audioRef = useRef(null);
  const stopRef = useRef(false);
  const waitResolverRef = useRef(null);
  const segmentIndexRef = useRef(0);
  const repeatRef = useRef(repeatYourShloka);
  const durationRef = useRef(duration);
  const waitModeRef = useRef(waitMode);

  const segments = useMemo(() => extractSegments(metadata), [metadata]);

  useEffect(() => {
    repeatRef.current = repeatYourShloka;
  }, [repeatYourShloka]);

  useEffect(() => {
    durationRef.current = duration;
  }, [duration]);

  useEffect(() => {
    waitModeRef.current = waitMode;
  }, [waitMode]);

  const nextSegment = () => {
    if (!segments.length) {
      return null;
    }
    const segment = segments[segmentIndexRef.current % segments.length];
    segmentIndexRef.current += 1;
    return segment;
  };

  const start = async () => {
    if (!audioUrl || !segments.length || isRunning) {
      return;
    }

    stopRef.current = false;
    setIsRunning(true);
    setStatus("Starting...");

    while (!stopRef.current) {
      for (let participant = 1; participant <= participants; participant += 1) {
        setCurrentParticipant(participant);
        for (let stanza = 0; stanza < stanzaCount; stanza += 1) {
          const segment = nextSegment();
          if (!segment) {
            setStatus("No playable shlokas");
            stopRef.current = true;
            break;
          }

          setCurrentSegment(segment);

          if (participant === yourTurn) {
            setStatus(`Your turn (participant ${participant})`);
            if (waitModeRef.current === "keyboard") {
              await waitForKeyboard(setAwaitingUser, waitResolverRef);
            } else {
              await waitForDuration(durationRef.current, stopRef);
            }

            if (stopRef.current) {
              break;
            }

            if (repeatRef.current) {
              setStatus(`Repeating your shloka (participant ${participant})`);
              await playSegment(audioRef.current, segment, playbackSpeed, stopRef);
            }
          } else {
            setStatus(`Participant ${participant}`);
            await playSegment(audioRef.current, segment, playbackSpeed, stopRef);
          }

          if (stopRef.current) {
            break;
          }
        }

        if (stopRef.current) {
          break;
        }
      }
    }

    setIsRunning(false);
    setAwaitingUser(false);
    setStatus("Stopped");
    setCurrentParticipant(null);
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
        <h3>Group Practice</h3>
        <p>
          Chapter: <strong>{chapter?.name || "Not selected"}</strong>
        </p>
        <div className="form-grid">
          <label>
            Participants
            <input
              type="number"
              min="1"
              max="12"
              value={participants}
              onChange={(event) => setParticipants(Number(event.target.value))}
            />
          </label>
          <label>
            Your Turn
            <input
              type="number"
              min="1"
              max={participants}
              value={yourTurn}
              onChange={(event) => setYourTurn(Number(event.target.value))}
            />
          </label>
          <label>
            Number of Stanzas
            <input
              type="number"
              min="1"
              max="4"
              value={stanzaCount}
              onChange={(event) => setStanzaCount(Number(event.target.value))}
            />
          </label>
        </div>
        <div className="controls">
          <button onClick={start} disabled={!segments.length || isRunning}>
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
              OK (Finish Reciting)
            </button>
          ) : null}
        </div>
        <StatusBar
          status={status}
          extra={
            currentSegment
              ? `Participant ${currentParticipant ?? "-"} • Shloka ${currentSegment.shlokaNum}`
              : null
          }
        />
      </div>

      <audio ref={audioRef} src={audioUrl || undefined} preload="auto" />
    </div>
  );
}
