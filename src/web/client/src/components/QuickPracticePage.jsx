import { useEffect, useMemo, useRef, useState } from "react";
import {
  createQuickPracticeItem,
  deleteQuickPracticeItem,
  fetchQuickPracticeItems,
  fetchChapterMetadata,
  getChapterAudioUrl,
} from "../api.js";
import { parseTimeToSeconds } from "../practiceUtils.js";
import { validateQuickPracticeCreatePayload } from "../contracts/quickPracticeContracts.js";

const PRACTICE_STATE = {
  IDLE: "idle",
  PLAYING_PROMPT: "playingPrompt",
  WAITING_FOR_DONE: "waitingForDone",
  REPLAYING: "replaying",
  ADVANCING: "advancing",
  COMPLETED: "completed",
};

function normalizeSlokaNumber(value) {
  return String(value ?? "").trim();
}

function resolveSlokaSegment(metadata, targetSlokaNumber) {
  const slokas = metadata?.shloka || metadata?.shlokas || [];
  const target = normalizeSlokaNumber(targetSlokaNumber);
  const sloka = slokas.find((candidate) => normalizeSlokaNumber(candidate?.shlokaNum) === target);
  if (!sloka) {
    return null;
  }

  const entries = sloka.entry || sloka.entries || [];
  let start = null;
  let end = null;
  for (const entry of entries) {
    const entryStart = parseTimeToSeconds(entry?.startTime);
    const entryEnd = parseTimeToSeconds(entry?.endTime);
    if (entryStart == null || entryEnd == null || entryEnd <= entryStart) {
      continue;
    }
    start = start == null ? entryStart : Math.min(start, entryStart);
    end = end == null ? entryEnd : Math.max(end, entryEnd);
  }

  if (start == null || end == null || end <= start) {
    return null;
  }

  return { start, end };
}

async function playSegmentBounded(audio, segment, runIdRef, runId) {
  if (!audio || !segment || runIdRef.current !== runId) {
    return false;
  }

  audio.pause();
  audio.currentTime = segment.start;

  try {
    await audio.play();
  } catch (_error) {
    return false;
  }

  while (runIdRef.current === runId) {
    if (audio.currentTime >= segment.end) {
      break;
    }
    await new Promise((resolve) => setTimeout(resolve, 100));
  }

  audio.pause();
  return true;
}

async function ensureAudioReady(audio, runIdRef, runId) {
  if (!audio || runIdRef.current !== runId) {
    return false;
  }

  if (audio.readyState >= 2) {
    return true;
  }

  return new Promise((resolve) => {
    const onReady = () => {
      cleanup();
      resolve(runIdRef.current === runId);
    };
    const onError = () => {
      cleanup();
      resolve(false);
    };
    const cleanup = () => {
      audio.removeEventListener("canplaythrough", onReady);
      audio.removeEventListener("loadedmetadata", onReady);
      audio.removeEventListener("error", onError);
    };

    audio.addEventListener("canplaythrough", onReady);
    audio.addEventListener("loadedmetadata", onReady);
    audio.addEventListener("error", onError);
    audio.load();
  });
}

export default function QuickPracticePage() {
  const [chapterNumber, setChapterNumber] = useState(0);
  const [slokaNumber, setSlokaNumber] = useState(1);
  const [isLoading, setIsLoading] = useState(true);
  const [isAdding, setIsAdding] = useState(false);
  const [isRemovingId, setIsRemovingId] = useState("");
  const [quickPracticeItems, setQuickPracticeItems] = useState([]);
  const [formError, setFormError] = useState("");
  const [apiError, setApiError] = useState("");
  const [practiceError, setPracticeError] = useState("");
  const [practiceState, setPracticeState] = useState(PRACTICE_STATE.IDLE);
  const [currentIndex, setCurrentIndex] = useState(-1);
  const [isBusy, setIsBusy] = useState(false);
  const [activeAudioUrl, setActiveAudioUrl] = useState("");
  const runIdRef = useRef(0);
  const audioRef = useRef(null);
  const metadataCacheRef = useRef({});

  useEffect(() => {
    const load = async () => {
      setApiError("");
      setIsLoading(true);
      try {
        const items = await fetchQuickPracticeItems();
        setQuickPracticeItems(items);
      } catch (error) {
        setApiError(error.message || "Failed to load quick-practice items.");
      } finally {
        setIsLoading(false);
      }
    };

    load();
  }, []);

  const currentItem = currentIndex >= 0 && currentIndex < quickPracticeItems.length
    ? quickPracticeItems[currentIndex]
    : null;

  const isPlayingActive =
    practiceState === PRACTICE_STATE.PLAYING_PROMPT || practiceState === PRACTICE_STATE.REPLAYING;
  const canStart =
    quickPracticeItems.length > 0 &&
    (practiceState === PRACTICE_STATE.IDLE || practiceState === PRACTICE_STATE.COMPLETED);
  const canDone = practiceState === PRACTICE_STATE.WAITING_FOR_DONE && !isBusy;

  const onAdd = async () => {
    setFormError("");
    setApiError("");

    const payload = { chapterNumber, slokaNumber };
    const validation = validateQuickPracticeCreatePayload(payload);
    if (!validation.valid) {
      setFormError(validation.message);
      return;
    }

    setIsAdding(true);
    try {
      const item = await createQuickPracticeItem(payload);
      setQuickPracticeItems((prev) => [...prev, item]);
    } catch (error) {
      setApiError(error.message || "Failed to add quick-practice item.");
    } finally {
      setIsAdding(false);
    }
  };

  const onRemove = async (id) => {
    setApiError("");
    setIsRemovingId(id);
    try {
      await deleteQuickPracticeItem(id);
      setQuickPracticeItems((prev) => prev.filter((item) => item.id !== id));
    } catch (error) {
      setApiError(error.message || "Failed to remove quick-practice item.");
    } finally {
      setIsRemovingId("");
    }
  };

  const stopActivePlayback = () => {
    runIdRef.current += 1;
    audioRef.current?.pause();
    setIsBusy(false);
    setCurrentIndex(-1);
    setPracticeState(PRACTICE_STATE.IDLE);
  };

  const getMetadataForChapter = async (chapter) => {
    if (metadataCacheRef.current[chapter]) {
      return metadataCacheRef.current[chapter];
    }
    const metadata = await fetchChapterMetadata(chapter);
    metadataCacheRef.current[chapter] = metadata;
    return metadata;
  };

  const playCurrentItemPrompt = async (index, activeRunId) => {
    if (runIdRef.current !== activeRunId) {
      return false;
    }

    const item = quickPracticeItems[index];
    setCurrentIndex(index);
    setIsBusy(true);
    setPracticeState(PRACTICE_STATE.PLAYING_PROMPT);
    setPracticeError("");

    try {
      const metadata = await getMetadataForChapter(item.chapterNumber);
      if (runIdRef.current !== activeRunId) {
        return false;
      }

      const segment = resolveSlokaSegment(metadata, item.slokaNumber);
      if (!segment) {
        throw new Error(
          `Missing metadata for Chapter ${item.chapterNumber}, Sloka ${item.slokaNumber}. Skipping item.`
        );
      }

      const nextAudioUrl = getChapterAudioUrl(item.chapterNumber);
      setActiveAudioUrl(nextAudioUrl);
      const audio = audioRef.current;
      if (!audio) {
        throw new Error("Audio player is unavailable.");
      }

      audio.src = nextAudioUrl;
      const ready = await ensureAudioReady(audio, runIdRef, activeRunId);
      if (!ready) {
        throw new Error(
          `Audio failed for Chapter ${item.chapterNumber}, Sloka ${item.slokaNumber}. Skipping item.`
        );
      }
      const played = await playSegmentBounded(audio, segment, runIdRef, activeRunId);
      if (!played) {
        throw new Error(
          `Audio failed for Chapter ${item.chapterNumber}, Sloka ${item.slokaNumber}. Skipping item.`
        );
      }

      if (runIdRef.current === activeRunId) {
        setPracticeState(PRACTICE_STATE.WAITING_FOR_DONE);
      }
      return true;
    } catch (error) {
      if (runIdRef.current === activeRunId) {
        setPracticeError(error.message || "Failed to play quick-practice item.");
      }
      return false;
    } finally {
      if (runIdRef.current === activeRunId) {
        setIsBusy(false);
      }
    }
  };

  const moveToNextItem = async (nextIndex, activeRunId) => {
    if (runIdRef.current !== activeRunId) {
      return;
    }

    if (nextIndex >= quickPracticeItems.length) {
      setCurrentIndex(-1);
      setPracticeState(PRACTICE_STATE.COMPLETED);
      setActiveAudioUrl("");
      return;
    }

    const played = await playCurrentItemPrompt(nextIndex, activeRunId);
    if (!played && runIdRef.current === activeRunId) {
      setPracticeState(PRACTICE_STATE.ADVANCING);
      await moveToNextItem(nextIndex + 1, activeRunId);
    }
  };

  const onPlay = async () => {
    const nextRunId = runIdRef.current + 1;
    runIdRef.current = nextRunId;
    setPracticeError("");
    setPracticeState(PRACTICE_STATE.ADVANCING);
    await moveToNextItem(0, nextRunId);
  };

  const onDone = async () => {
    if (!currentItem) {
      return;
    }
    const activeRunId = runIdRef.current;
    setIsBusy(true);
    setPracticeState(PRACTICE_STATE.REPLAYING);
    setPracticeError("");

    try {
      const metadata = await getMetadataForChapter(currentItem.chapterNumber);
      const segment = resolveSlokaSegment(metadata, currentItem.slokaNumber);
      if (!segment) {
        throw new Error(
          `Missing metadata for Chapter ${currentItem.chapterNumber}, Sloka ${currentItem.slokaNumber}.`
        );
      }

      const audio = audioRef.current;
      if (!audio) {
        throw new Error("Audio player is unavailable.");
      }
      const replayAudioUrl = getChapterAudioUrl(currentItem.chapterNumber);
      setActiveAudioUrl(replayAudioUrl);
      audio.src = replayAudioUrl;
      const ready = await ensureAudioReady(audio, runIdRef, activeRunId);
      if (!ready) {
        throw new Error(
          `Audio failed for Chapter ${currentItem.chapterNumber}, Sloka ${currentItem.slokaNumber}.`
        );
      }
      const played = await playSegmentBounded(audio, segment, runIdRef, activeRunId);
      if (!played) {
        throw new Error(
          `Audio failed for Chapter ${currentItem.chapterNumber}, Sloka ${currentItem.slokaNumber}.`
        );
      }
    } catch (error) {
      setPracticeError(error.message || "Failed to replay current quick-practice item.");
    } finally {
      if (runIdRef.current !== activeRunId) {
        setIsBusy(false);
        return;
      }
      setPracticeState(PRACTICE_STATE.ADVANCING);
      setIsBusy(false);
      await moveToNextItem(currentIndex + 1, activeRunId);
    }
  };

  const playDisabledReason = useMemo(() => {
    if (isLoading) {
      return "Loading saved quick-practice items...";
    }
    if (!quickPracticeItems.length) {
      return "Add at least one item to start Quick Practice.";
    }
    return "";
  }, [isLoading, quickPracticeItems.length]);

  return (
    <div className="stack quick-practice-layout">
      <section className="card">
        <h3>Quick Practice</h3>
        <p className="hint">Add chapter and sloka pairs for fast repetition practice.</p>
        <div className="form-grid">
          <label>
            Chapter Number (0..18)
            <input
              type="number"
              min="0"
              max="18"
              value={chapterNumber}
              onChange={(event) => setChapterNumber(Number(event.target.value))}
            />
          </label>
          <label>
            Sloka Number (&gt;= 1)
            <input
              type="number"
              min="1"
              value={slokaNumber}
              onChange={(event) => setSlokaNumber(Number(event.target.value))}
            />
          </label>
        </div>
        {formError ? <p className="error">{formError}</p> : null}
        {apiError ? <p className="error">{apiError}</p> : null}
        <div className="controls">
          <button type="button" onClick={onAdd} disabled={isAdding || isPlayingActive}>
            Add
          </button>
        </div>
      </section>

      <section className="card">
        <h3>Saved Items</h3>
        {isLoading ? (
          <p className="hint">Loading quick-practice items...</p>
        ) : quickPracticeItems.length ? (
          <ul className="quick-practice-list">
            {quickPracticeItems.map((item) => (
              <li key={item.id}>
                <span>
                  Chapter {item.chapterNumber} - Sloka {item.slokaNumber}
                </span>
                <button
                  type="button"
                  className="quiet-button"
                  onClick={() => onRemove(item.id)}
                  disabled={isRemovingId === item.id || isPlayingActive}
                >
                  Remove
                </button>
              </li>
            ))}
          </ul>
        ) : (
          <p className="hint">No saved items yet. Add your first chapter/sloka pair above.</p>
        )}
      </section>

      <section className="card">
        <h3>Practice</h3>
        <div className="controls">
          <button type="button" onClick={onPlay} disabled={!canStart || isBusy}>
            {practiceState === PRACTICE_STATE.COMPLETED ? "Restart" : "Play"}
          </button>
          <button type="button" onClick={onDone} disabled={!canDone}>
            Done
          </button>
          <button
            type="button"
            className="quiet-button"
            onClick={stopActivePlayback}
            disabled={!isPlayingActive && practiceState !== PRACTICE_STATE.WAITING_FOR_DONE}
          >
            Stop
          </button>
        </div>
        {playDisabledReason && practiceState === PRACTICE_STATE.IDLE ? (
          <p className="hint">{playDisabledReason}</p>
        ) : null}
        {practiceError ? <p className="error">{practiceError}</p> : null}
        <p className="hint">State: {practiceState}</p>
        <div className="quick-practice-current-item">
          <strong>Current Item:</strong>{" "}
          {currentItem
            ? `Chapter ${currentItem.chapterNumber} - Sloka ${currentItem.slokaNumber}`
            : practiceState === PRACTICE_STATE.COMPLETED
              ? "Completed"
              : "Not started"}
        </div>
        <audio ref={audioRef} src={activeAudioUrl || undefined} preload="auto" />
      </section>
    </div>
  );
}
