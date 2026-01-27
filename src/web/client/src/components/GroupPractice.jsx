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

  const participantStatuses = useMemo(() => {
    const list = [];
    for (let participant = 1; participant <= participants; participant += 1) {
      let state = "Waiting";
      if (isRunning && currentParticipant != null) {
        if (participant < currentParticipant) {
          state = "Done";
        } else if (participant === currentParticipant) {
          if (participant === yourTurn) {
            state = awaitingUser || !repeatYourShloka ? "Reciting" : "Playing";
          } else {
            state = "Playing";
          }
        }
      }

      list.push({
        id: participant,
        label: participant === yourTurn ? "Self" : `Participant ${participant}`,
        state,
        isActive: participant === currentParticipant,
        isSelf: participant === yourTurn,
      });
    }
    return list;
  }, [
    participants,
    yourTurn,
    isRunning,
    currentParticipant,
    awaitingUser,
    repeatYourShloka,
  ]);

  const nextSegment = () => {
    if (!segments.length || segmentIndexRef.current >= segments.length) {
      return null;
    }
    const segment = segments[segmentIndexRef.current];
    segmentIndexRef.current += 1;
    return segment;
  };

  const start = async () => {
    if (!audioUrl || !segments.length || isRunning) {
      return;
    }

    segmentIndexRef.current = 0;
    stopRef.current = false;
    let completed = false;
    setIsRunning(true);
    setStatus("Starting...");
    setCurrentSegment(null);
    setCurrentParticipant(null);

    while (!stopRef.current) {
      for (let participant = 1; participant <= participants; participant += 1) {
        setCurrentParticipant(participant);
        const segment = nextSegment();
        if (!segment) {
          setStatus("Completed all shlokas");
          completed = true;
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
    }

    setIsRunning(false);
    setAwaitingUser(false);
    setStatus(completed ? "Completed" : "Stopped");
    setCurrentParticipant(null);
    setCurrentSegment(null);
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

      <div className="group-practice-layout">
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
                <span className="participant-status">{participant.state}</span>
              </li>
            ))}
          </ul>
        </aside>
      </div>

      <audio ref={audioRef} src={audioUrl || undefined} preload="auto" />
    </div>
  );
}
