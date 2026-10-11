"""Turns the plain white background of a sprite sheet transparent, for models that cannot output transparency.

Usage: white_to_alpha.py IN.png OUT.png [--tolerance 28] [--holes 4000]
White connected to the image border is removed (flood fill), so white highlights and cream fills inside the dark
outlines stay. Enclosed pure-white areas larger than --holes pixels (under a flag garland, inside a basket handle)
are background seen through a gap and are removed too; smaller ones are highlights and stay. Pixels at the edge
of the removed area fade out over a few levels to avoid a white halo.
Then cut the sheet with slice_sheet.py (docs/art/style-guide.md).
"""

import argparse
from collections import deque

import numpy as np
from PIL import Image


def fill(mask: np.ndarray, allowed: np.ndarray, queue: deque[tuple[int, int]]) -> None:
    """Marks in mask every allowed pixel 4-connected to the queued pixels."""
    h, w = mask.shape
    while queue:
        y, x = queue.popleft()
        if mask[y, x] or not allowed[y, x]:
            continue
        mask[y, x] = True
        if y > 0:
            queue.append((y - 1, x))
        if y < h - 1:
            queue.append((y + 1, x))
        if x > 0:
            queue.append((y, x - 1))
        if x < w - 1:
            queue.append((y, x + 1))


def main() -> None:
    p = argparse.ArgumentParser()
    p.add_argument("src")
    p.add_argument("dst")
    p.add_argument("--tolerance", type=int, default=28)
    p.add_argument("--holes", type=int, default=4000)
    a = p.parse_args()
    im = np.array(Image.open(a.src).convert("RGBA"))
    h, w = im.shape[:2]
    # Distance from pure white: 0 is white, larger is darker or more colourful.
    dist = 255 - im[:, :, :3].min(axis=2).astype(np.int32)
    background = np.zeros((h, w), dtype=bool)
    queue: deque[tuple[int, int]] = deque()
    for x in range(w):
        queue.extend(((0, x), (h - 1, x)))
    for y in range(h):
        queue.extend(((y, 0), (y, w - 1)))
    fill(background, dist <= a.tolerance, queue)
    # Enclosed pure-white areas: label them one by one and drop the large ones.
    seen = background.copy()
    pure = dist <= 6
    for y, x in zip(*np.nonzero(pure & ~seen)):
        if seen[y, x]:
            continue
        region = np.zeros((h, w), dtype=bool)
        fill(region, pure, deque([(y, x)]))
        seen |= region
        if region.sum() > a.holes:
            background |= region

    alpha = im[:, :, 3].astype(np.float32)
    alpha[background] = 0
    # Soften the one-pixel rim next to the removed area: near-white rim pixels become partly transparent.
    rim = ~background & (
        np.roll(background, 1, 0) | np.roll(background, -1, 0) | np.roll(background, 1, 1) | np.roll(background, -1, 1)
    )
    alpha[rim] = np.minimum(alpha[rim], np.clip(dist[rim] * (255.0 / max(1, a.tolerance * 3)), 0, 255))
    im[:, :, 3] = alpha.astype(np.uint8)
    Image.fromarray(im).save(a.dst)


if __name__ == "__main__":
    main()
