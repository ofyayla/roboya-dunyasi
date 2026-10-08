"""Unit tests for the TTS planner (no network). Run: python3 -m unittest tools/tts/test_generate.py"""

import importlib.util
import tempfile
import unittest
from pathlib import Path

spec = importlib.util.spec_from_file_location("generate", Path(__file__).with_name("generate.py"))
gen = importlib.util.module_from_spec(spec)
spec.loader.exec_module(gen)


class PlanTests(unittest.TestCase):
    def setUp(self) -> None:
        self.tmp = tempfile.TemporaryDirectory()
        gen.ROOT = Path(self.tmp.name)
        (gen.ROOT / "content/voice/audio/tr").mkdir(parents=True)

    def tearDown(self) -> None:
        self.tmp.cleanup()

    def entry(self, key: str, text: str, source: str = "tts", voice: str = "v1", write: bool = True) -> dict:
        rel = f"audio/tr/{key}.mp3"
        if write:
            (gen.ROOT / "content/voice" / rel).write_bytes(b"x")
        return {"file": rel, "source": source, "textHash": gen.text_hash(text), "voiceId": voice}

    def test_plan_new_key_is_generated(self) -> None:
        self.assertEqual(gen.plan({"a.b": "Merhaba"}, {"entries": {}}, "v1", None), ["a.b"])

    def test_plan_up_to_date_key_is_skipped(self) -> None:
        manifest = {"entries": {"a.b": self.entry("a.b", "Merhaba")}}
        self.assertEqual(gen.plan({"a.b": "Merhaba"}, manifest, "v1", None), [])

    def test_plan_changed_text_or_voice_or_missing_file_regenerates(self) -> None:
        manifest = {"entries": {"a.b": self.entry("a.b", "eski"), "c.d": self.entry("c.d", "x", voice="v0"),
                                "e.f": self.entry("e.f", "y", write=False)}}
        self.assertEqual(gen.plan({"a.b": "yeni", "c.d": "x", "e.f": "y"}, manifest, "v1", None), ["a.b", "c.d", "e.f"])

    def test_plan_studio_recording_is_never_regenerated(self) -> None:
        manifest = {"entries": {"a.b": self.entry("a.b", "eski", source="studio")}}
        self.assertEqual(gen.plan({"a.b": "yeni"}, manifest, "v1", None), [])

    def test_plan_only_prefix_filters(self) -> None:
        self.assertEqual(gen.plan({"a.b": "1", "c.d": "2"}, {"entries": {}}, "v1", "c."), ["c.d"])

    def test_text_hash_is_stable_and_utf8(self) -> None:
        self.assertEqual(gen.text_hash("Ğğİı"), gen.text_hash("Ğğİı"))
        self.assertEqual(len(gen.text_hash("a")), 16)


if __name__ == "__main__":
    unittest.main()
