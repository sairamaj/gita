import json
import os
from pathlib import Path

import requests
from flask import Flask, abort, jsonify, send_file
from flask_cors import CORS


BASE_URL = "https://www.sgsgitafoundation.org/bg"
CHAPTERS = [
    {"id": 0, "name": "Gita Dhayna Slokas"},
    {"id": 1, "name": "Arjuna Vishadha Yoga"},
    {"id": 2, "name": "Sankhya Yoga"},
    {"id": 3, "name": "Karma Yoga"},
    {"id": 4, "name": "Jnana Yoga"},
    {"id": 5, "name": "Karma Sanyasa Yoga"},
    {"id": 6, "name": "Ātma-Saṁyama Yoga"},
    {"id": 7, "name": "Jnana Vijnana Yoga"},
    {"id": 8, "name": "Aksara Brahma Yoga"},
    {"id": 9, "name": "Raja Vidya Raja Guhya Yoga"},
    {"id": 10, "name": "Vibhuti Yoga"},
    {"id": 11, "name": "Visvarupa Darsana Yoga"},
    {"id": 12, "name": "Bhakti Yoga"},
    {"id": 13, "name": "Ksetra Ksetrajna Vibhaga Yoga"},
    {"id": 14, "name": "Gunatraya Vibhaga Yoga"},
    {"id": 15, "name": "Purushottama Yoga"},
    {"id": 16, "name": "Daivasura Sampad Vibhaga Yoga"},
    {"id": 17, "name": "Sraddhatraya Vibhaga Yoga"},
    {"id": 18, "name": "Moksha Sanyasa Yoga"},
]
CHAPTER_IDS = {chapter["id"] for chapter in CHAPTERS}
CHAPTER_JSON = "plain_chapter.json"
CHAPTER_AUDIO = "plain_chapter.m4a"

DATA_DIR = Path(__file__).resolve().parent / "data"

app = Flask(__name__)
CORS(app)


def chapter_slug(chapter_id: int) -> str:
    return f"{chapter_id:02d}"


def ensure_chapter_assets(chapter_id: int) -> tuple[Path, Path]:
    if chapter_id not in CHAPTER_IDS:
        abort(404, description="Unknown chapter id")

    chapter_dir = DATA_DIR / chapter_slug(chapter_id)
    chapter_dir.mkdir(parents=True, exist_ok=True)

    json_path = chapter_dir / CHAPTER_JSON
    audio_path = chapter_dir / CHAPTER_AUDIO

    if not json_path.exists():
        url = f"{BASE_URL}/{chapter_slug(chapter_id)}/{CHAPTER_JSON}"
        response = requests.get(url, timeout=30)
        response.raise_for_status()
        json_path.write_text(response.text, encoding="utf-8")

    if not audio_path.exists():
        url = f"{BASE_URL}/{chapter_slug(chapter_id)}/{CHAPTER_AUDIO}"
        with requests.get(url, stream=True, timeout=60) as response:
            response.raise_for_status()
            with open(audio_path, "wb") as handle:
                for chunk in response.iter_content(chunk_size=1024 * 1024):
                    if chunk:
                        handle.write(chunk)

    return json_path, audio_path


@app.get("/api/health")
def health() -> tuple[dict, int]:
    return {"status": "ok"}, 200


@app.get("/api/config")
def config() -> tuple[dict, int]:
    return {
        "defaults": {
            "waitMode": "keyboard",
            "duration": 20,
            "playbackSpeed": 1.5,
            "repeatYourShloka": False,
            "participants": 4,
            "yourTurn": 2,
        },
        "waitModes": [
            {"id": "keyboard", "label": "Keyboard Hit"},
            {"id": "duration", "label": "Duration"},
        ],
        "playbackSpeed": {"min": 0.5, "max": 2.0, "step": 0.1},
    }, 200


@app.get("/api/chapters")
def chapters() -> tuple[dict, int]:
    return {"chapters": CHAPTERS}, 200


@app.get("/api/chapters/<int:chapter_id>/metadata")
def chapter_metadata(chapter_id: int):
    json_path, _ = ensure_chapter_assets(chapter_id)
    with open(json_path, "r", encoding="utf-8") as handle:
        data = json.load(handle)
    return jsonify(data)


@app.get("/api/chapters/<int:chapter_id>/audio")
def chapter_audio(chapter_id: int):
    _, audio_path = ensure_chapter_assets(chapter_id)
    return send_file(audio_path, mimetype="audio/mp4", conditional=True)


if __name__ == "__main__":
    port = int(os.environ.get("PORT", "5000"))
    app.run(host="0.0.0.0", port=port, debug=True)
