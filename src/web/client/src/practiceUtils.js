export function parseTimeToSeconds(value) {
  if (!value || typeof value !== "string") {
    return null;
  }

  const parts = value.split(":");
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
    for (const entry of entries) {
      const start = parseTimeToSeconds(entry?.startTime);
      const end = parseTimeToSeconds(entry?.endTime);
      if (start == null || end == null || end <= start) {
        continue;
      }
      segments.push({
        start,
        end,
        duration: end - start,
        text: entry?.text || "",
        shlokaNum: shloka?.shlokaNum || entry?.shlNbr || "",
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

  const durationMs = Math.max(0, (segment.duration * 1000) / speed);
  await sleep(durationMs);
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
