"""Measures Roboya's poses and builds colour variants (ILR-03).

Writes content/rewards/robot-anchors.json (head box and antenna bulb per pose, as fractions of each sprite) and
Art/RobotParts/roboya_<pose>_<colour>.png (hue-shifted body, face and limbs unchanged). Parts are placed from these
anchors, so one catalog entry fits every pose. Run after the Roboya sprites change:

    uv run --with pillow --with numpy --with scipy python -I tools/art/robot_parts.py
"""
import json
import pathlib

import numpy as np
from PIL import Image
from scipy import ndimage

ROOT = pathlib.Path(__file__).resolve().parents[2]
POSES = ["front", "back", "side", "happy", "laughing", "curious", "surprised", "proud"]
SRC = ROOT / "apps/game/Assets/_Project/Art/Characters/Roboya"
OUT = ROOT / "apps/game/Assets/_Project/Art/RobotParts"
ANCHORS = ROOT / "content/rewards/robot-anchors.json"
COLOURS = {"blue": 205, "purple": 275}
BODY_HUE = 28


def hsv(rgb):
    mx, mn = rgb.max(2), rgb.min(2)
    s = np.where(mx > 0, (mx - mn) / np.maximum(mx, 1e-6), 0)
    r, g, b = rgb[:, :, 0], rgb[:, :, 1], rgb[:, :, 2]
    d = np.maximum(mx - mn, 1e-6)
    h = np.where(mx == r, ((g - b) / d) % 6, np.where(mx == g, (b - r) / d + 2, (r - g) / d + 4)) * 60
    return h, s, mx


def body_mask(im):
    h, s, v = hsv(im[:, :, :3])
    return (h >= 12) & (h <= 45) & (s > 0.62) & (v > 0.68) & (im[:, :, 3] > 0.2)


def anchors(im):
    height, width = im.shape[:2]
    mask = body_mask(im)
    lab, n = ndimage.label(mask)
    sizes = ndimage.sum(mask, lab, range(1, n + 1))
    main = 1 + int(np.argmax(sizes))
    ys, xs = np.where(lab == main)
    head = [xs.min() / width, ys.min() / height, xs.max() / width, ys.max() / height]
    bulb = None
    for i in range(1, n + 1):
        if i == main:
            continue
        yy, xx = np.where(lab == i)
        if len(yy) > 300 and yy.mean() < ys.min():
            bulb = [xx.mean() / width, yy.mean() / height, (xx.max() - xx.min()) / width]
    return {"head": [round(float(v), 4) for v in head], "bulb": None if bulb is None else [round(float(v), 4) for v in bulb]}


def recolour(im, target):
    rgb = im[:, :, :3]
    h, s, v = hsv(rgb)
    body = body_mask(im)
    hp = ((h + (target - BODY_HUE)) % 360) / 60
    c = v * s
    x = c * (1 - np.abs(hp % 2 - 1))
    m = v - c
    z = np.zeros_like(c)
    sec = np.floor(hp).astype(int) % 6
    pick = lambda *o: np.select([sec == i for i in range(6)], o)
    new = np.stack([pick(c, x, z, z, x, c) + m, pick(x, c, c, x, z, z) + m, pick(z, z, x, c, c, x) + m], 2)
    out = rgb.copy()
    out[body] = new[body]
    return np.concatenate([out, im[:, :, 3:]], 2)


def main():
    result = {}
    OUT.mkdir(parents=True, exist_ok=True)
    for pose in POSES:
        im = np.array(Image.open(SRC / f"roboya_{pose}.png").convert("RGBA")).astype(np.float32) / 255
        result[pose] = anchors(im)
        for name, hue in COLOURS.items():
            Image.fromarray((recolour(im, hue) * 255).clip(0, 255).astype(np.uint8)).save(OUT / f"roboya_{pose}_{name}.png")
    ANCHORS.write_text(json.dumps(result, indent=2) + "\n")
    print("anchors for", ", ".join(result))


if __name__ == "__main__":
    main()
