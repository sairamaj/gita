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
