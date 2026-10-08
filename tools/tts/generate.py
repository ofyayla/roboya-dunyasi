"""Generate narration audio from content/voice/script.csv with ElevenLabs (F0-15).

Only script text is sent to the TTS service — never user data (CLAUDE.md §10).
Audio is written as content/voice/audio/tr/<key>.mp3 and tracked in content/voice/manifest.json
with a hash of the text, so only new or changed lines are (re)generated. Studio recordings
(`"source": "studio"`) are never overwritten; replacing TTS with studio audio needs no code change.

Usage:
  python3 tools/tts/generate.py --dry-run        # show what would be generated and the credit cost
  python3 tools/tts/generate.py                  # generate missing / stale lines
  python3 tools/tts/generate.py --only yon_avcisi.l01
"""

from __future__ import annotations

import argparse
import csv
import hashlib
import json
import os
import sys
import time
import urllib.error
import urllib.request
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
SCRIPT = ROOT / "content/voice/script.csv"
MANIFEST = ROOT / "content/voice/manifest.json"
AUDIO_DIR = ROOT / "content/voice/audio/tr"
API = "https://api.elevenlabs.io/v1/text-to-speech/{voice}?output_format=mp3_44100_128"

# Child-friendly delivery: a little expressive, still stable across lines.
VOICE_SETTINGS = {"stability": 0.45, "similarity_boost": 0.75, "style": 0.35, "use_speaker_boost": True}


def text_hash(text: str) -> str:
    return hashlib.sha256(text.encode("utf-8")).hexdigest()[:16]


def load_env(path: Path) -> None:
    """Minimal .env reader so the tool has no third-party dependencies."""
    if not path.exists():
        return
    for line in path.read_text(encoding="utf-8").splitlines():
        line = line.strip()
        if line and not line.startswith("#") and "=" in line:
            key, value = line.split("=", 1)
            os.environ.setdefault(key.strip(), value.strip())


def read_script() -> dict[str, str]:
    with SCRIPT.open(encoding="utf-8", newline="") as f:
        return {row["key"]: row["text"] for row in csv.DictReader(f) if row.get("key")}


def read_manifest() -> dict:
    if MANIFEST.exists():
        return json.loads(MANIFEST.read_text(encoding="utf-8"))
    return {"version": 1, "language": "tr", "entries": {}}


def write_manifest(manifest: dict) -> None:
    manifest["entries"] = dict(sorted(manifest["entries"].items()))
    MANIFEST.write_text(json.dumps(manifest, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")


def plan(script: dict[str, str], manifest: dict, voice: str, only: str | None) -> list[str]:
    todo = []
    for key, text in script.items():
        if only and not key.startswith(only):
            continue
        entry = manifest["entries"].get(key)
        if entry and entry.get("source") == "studio":
            if entry.get("textHash") != text_hash(text):
                print(f"! {key}: studio recording is stale (text changed); re-record, not regenerating")
            continue
        file_ok = entry is not None and (ROOT / "content/voice" / entry["file"]).exists()
        if file_ok and entry["textHash"] == text_hash(text) and entry.get("voiceId") == voice:
            continue
        todo.append(key)
    return todo


def synthesize(text: str, voice: str, model: str, api_key: str) -> bytes:
    body = json.dumps(
        {"text": text, "model_id": model, "language_code": "tr", "voice_settings": VOICE_SETTINGS}
    ).encode("utf-8")
    request = urllib.request.Request(  # noqa: S310 - fixed https endpoint
        API.format(voice=voice),
        data=body,
        headers={"xi-api-key": api_key, "Content-Type": "application/json", "Accept": "audio/mpeg"},
        method="POST",
    )
    for attempt in range(3):
        try:
            with urllib.request.urlopen(request, timeout=60) as response:  # noqa: S310
                return response.read()
        except urllib.error.HTTPError as e:
            detail = e.read().decode("utf-8", "replace")[:300]
            if e.code in (429, 500, 502, 503) and attempt < 2:
                time.sleep(2 * (attempt + 1))
                continue
            raise SystemExit(f"TTS failed ({e.code}): {detail}") from e
    raise SystemExit("TTS failed after retries")


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--dry-run", action="store_true")
    parser.add_argument("--only", help="generate only keys with this prefix")
    args = parser.parse_args()

    load_env(ROOT / ".env")
    voice = os.environ.get("ELEVENLABS_VOICE_ID", "")
    model = os.environ.get("ELEVENLABS_MODEL_ID", "eleven_multilingual_v2")
    api_key = os.environ.get("ELEVENLABS_API_KEY", "")

    script = read_script()
    manifest = read_manifest()
    todo = plan(script, manifest, voice, args.only)
    chars = sum(len(script[k]) for k in todo)
    print(f"{len(todo)} line(s) to generate, {chars} characters (~{chars} credits on eleven_multilingual_v2)")
    if args.dry_run or not todo:
        for key in todo:
            print(f"  {key}")
        return 0
    if not api_key or not voice:
        print("ELEVENLABS_API_KEY and ELEVENLABS_VOICE_ID must be set in .env", file=sys.stderr)
        return 2

    AUDIO_DIR.mkdir(parents=True, exist_ok=True)
    for i, key in enumerate(todo, 1):
        audio = synthesize(script[key], voice, model, api_key)
        rel = f"audio/tr/{key}.mp3"
        (ROOT / "content/voice" / rel).write_bytes(audio)
        manifest["entries"][key] = {
            "file": rel,
            "source": "tts",
            "textHash": text_hash(script[key]),
            "voiceId": voice,
            "model": model,
        }
        write_manifest(manifest)  # after every line so an interrupted run keeps its progress
        print(f"[{i}/{len(todo)}] {key} ({len(audio) // 1024} KB)")
    return 0


if __name__ == "__main__":
    sys.exit(main())
