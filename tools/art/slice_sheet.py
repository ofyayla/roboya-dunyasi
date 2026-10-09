"""Cuts a transparent sprite sheet into trimmed PNGs on a regular grid (cols x rows).

Usage: slice_sheet.py SHEET.png COLS ROWS OUT_DIR name1 name2 ... [--max 512]
Names are taken row by row. Each cell's non-transparent pixels are trimmed with a small margin and
scaled down so the longest side is at most --max pixels. Sheets come from Higgsfield (docs/art/style-guide.md).
"""

import argparse
import os

import numpy as np
from PIL import Image


def main() -> None:
    p = argparse.ArgumentParser()
    p.add_argument("sheet")
    p.add_argument("cols", type=int)
    p.add_argument("rows", type=int)
    p.add_argument("out")
    p.add_argument("names", nargs="+")
    p.add_argument("--max", type=int, default=512)
    a = p.parse_args()
    im = Image.open(a.sheet).convert("RGBA")
    w, h = im.size
    cw, ch = w / a.cols, h / a.rows
    os.makedirs(a.out, exist_ok=True)
    for i, name in enumerate(a.names):
        r, c = divmod(i, a.cols)
        cell = im.crop((round(c * cw), round(r * ch), round((c + 1) * cw), round((r + 1) * ch)))
        alpha = np.array(cell)[:, :, 3]
        ys, xs = np.where(alpha > 16)
        box = (max(xs.min() - 6, 0), max(ys.min() - 6, 0), min(xs.max() + 7, cell.width), min(ys.max() + 7, cell.height))
        sprite = cell.crop(box)
        scale = a.max / max(sprite.size)
        if scale < 1:
            sprite = sprite.resize((round(sprite.width * scale), round(sprite.height * scale)), Image.LANCZOS)
        sprite.save(os.path.join(a.out, name + ".png"))
        print(name, sprite.size)


if __name__ == "__main__":
    main()
