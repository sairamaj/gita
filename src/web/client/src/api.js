const API_BASE = import.meta.env.VITE_API_BASE || "http://localhost:5000";

async function request(path) {
  const response = await fetch(`${API_BASE}${path}`);
  if (!response.ok) {
    const message = await response.text();
    throw new Error(message || `Request failed: ${response.status}`);
  }
  return response.json();
}

export function getApiBase() {
  return API_BASE;
}

export async function fetchChapters() {
  return request("/api/chapters");
}

export async function fetchConfig() {
  return request("/api/config");
}

export async function fetchChapterMetadata(chapterId) {
  return request(`/api/chapters/${chapterId}/metadata`);
}

export function getChapterAudioUrl(chapterId) {
  return `${API_BASE}/api/chapters/${chapterId}/audio`;
}
