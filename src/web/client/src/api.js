import { QUICK_PRACTICE_API_BASE } from "./contracts/quickPracticeContracts.js";

// Helper function to format chapter ID as two-digit string
function chapterSlug(chapterId) {
  return String(chapterId).padStart(2, '0');
}

// Fetch JSON files from the public directory
async function fetchJson(path) {
  const response = await fetch(path);
  if (!response.ok) {
    throw new Error(`Failed to load ${path}: ${response.status}`);
  }
  return response.json();
}

async function requestJson(url, options = {}) {
  const response = await fetch(url, options);
  let payload = null;

  try {
    payload = await response.json();
  } catch (_error) {
    payload = null;
  }

  if (!response.ok) {
    throw new Error(payload?.message || `Request failed: ${response.status}`);
  }

  return payload;
}

export async function fetchChapters() {
  return fetchJson('/chapters.json');
}

export async function fetchConfig() {
  return fetchJson('/config.json');
}

export async function fetchChapterMetadata(chapterId) {
  return fetchJson(`/data/${chapterSlug(chapterId)}/plain_chapter.json`);
}

export function getChapterAudioUrl(chapterId) {
  return `/data/${chapterSlug(chapterId)}/plain_chapter.m4a`;
}

export async function fetchQuickPracticeItems() {
  const payload = await requestJson(QUICK_PRACTICE_API_BASE);
  return payload.quickPracticeItems || [];
}

export async function createQuickPracticeItem(input) {
  const payload = await requestJson(QUICK_PRACTICE_API_BASE, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(input),
  });
  return payload.item;
}

export async function deleteQuickPracticeItem(id) {
  const payload = await requestJson(`${QUICK_PRACTICE_API_BASE}/${encodeURIComponent(id)}`, {
    method: "DELETE",
  });
  return payload.deletedId;
}
