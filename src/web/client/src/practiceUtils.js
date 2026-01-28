export function parseTimeToSeconds(value) {
  if (value == null) {
    return null;
  }

  if (typeof value === "number") {
    return Number.isFinite(value) ? value : null;
  }

  if (typeof value !== "string") {
    return null;
  }

  const trimmed = value.trim();
  if (!trimmed) {
    return null;
  }

  if (!trimmed.includes(":")) {
    const numeric = Number(trimmed);
    return Number.isFinite(numeric) ? numeric : null;
  }

  const parts = trimmed.split(":");
  if (parts.length < 2 || parts.length > 3) {
    return null;
  }

  const [hoursPart, minutesPart, secondsPart] =
    parts.length === 3 ? parts : ["0", parts[0], parts[1]];
  const hours = Number(hoursPart);
  const minutes = Number(minutesPart);
  const seconds = Number(secondsPart);

  if (Number.isNaN(hours) || Number.isNaN(minutes) || Number.isNaN(seconds)) {
    return null;
  }

  return hours * 3600 + minutes * 60 + seconds;
}

export function extractSegments(metadata) {
  const shlokas = metadata?.shloka || metadata?.shlokas || [];
  const segments = [];

  for (const shloka of shlokas) {
    const entries = shloka?.entry || shloka?.entries || [];
    let start = null;
    let end = null;
    const texts = [];

    for (const entry of entries) {
      const entryStart = parseTimeToSeconds(entry?.startTime);
      const entryEnd = parseTimeToSeconds(entry?.endTime);
      if (entryStart == null || entryEnd == null || entryEnd <= entryStart) {
        continue;
      }
      start = start == null ? entryStart : Math.min(start, entryStart);
      end = end == null ? entryEnd : Math.max(end, entryEnd);
      if (entry?.text) {
        texts.push(entry.text);
      }
    }

    if (start != null && end != null && end > start) {
      segments.push({
        start,
        end,
        duration: end - start,
        text: texts.join(" "),
        shlokaNum: shloka?.shlokaNum || "",
      });
    }
  }

  return segments;
}

export function pickRandomSegment(segments) {
  if (!segments.length) {
    return null;
  }
  const index = Math.floor(Math.random() * segments.length);
  return segments[index];
}

export function sleep(ms) {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

export async function playSegment(audio, segment, playbackSpeed, stopRef) {
  if (!audio || !segment || stopRef?.current) {
    return;
  }

  const speed = playbackSpeed > 0 ? playbackSpeed : 1;
  audio.pause();
  audio.currentTime = segment.start;
  audio.playbackRate = speed;

  try {
    await audio.play();
  } catch (error) {
    return;
  }

  const targetEnd = segment.end;
  const checkStepMs = 100;

  while (!stopRef?.current) {
    if (audio.currentTime >= targetEnd) {
      break;
    }
    await sleep(checkStepMs);
  }
  audio.pause();
}

export async function waitForDuration(seconds, stopRef) {
  const durationMs = Math.max(0, seconds * 1000);
  const step = 200;
  let elapsed = 0;

  while (!stopRef?.current && elapsed < durationMs) {
    await sleep(step);
    elapsed += step;
  }
}

export function waitForKeyboard(setAwaitingUser, waitResolverRef) {
  return new Promise((resolve) => {
    if (setAwaitingUser) {
      setAwaitingUser(true);
    }
    waitResolverRef.current = () => {
      if (setAwaitingUser) {
        setAwaitingUser(false);
      }
      waitResolverRef.current = null;
      resolve();
    };
  });
}
