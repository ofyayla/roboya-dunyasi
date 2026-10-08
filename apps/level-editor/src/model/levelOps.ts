import type { CardId, Facing, GameId, ItemDto, LevelDto, RegionId } from "@roboya/level-schema";

/** Editing tools. Every operation returns a new level object (React state stays immutable). */
export type Tool = "wall" | "floor" | "robot" | "goal" | "item" | "erase";

export interface ItemBrush {
  kind: ItemDto["kind"];
  color?: ItemDto["color"];
}

export const MIN_SIZE = 2;
export const MAX_SIZE = 12;
const FACINGS: Facing[] = ["north", "east", "south", "west"];

export function size(level: LevelDto): { width: number; height: number } {
  return { width: level.grid.rows[0]?.length ?? 0, height: level.grid.rows.length };
}

export function levelPath(level: Pick<LevelDto, "id" | "region" | "game">): string {
  const last = level.id.split(".").at(-1) ?? "00";
  return `${level.region}/${level.game}/${last}.json`;
}

export function voiceKeysFor(id: string): { intro: string; success: string } {
  const [, game = "oyun", n = "00"] = id.split(".");
  const prefix = `${game.replace(/-/g, "_")}.l${n}`;
  return { intro: `${prefix}.intro`, success: `${prefix}.success` };
}

export function newLevel(region: RegionId, game: GameId, order: number): LevelDto {
  const n = String(order).padStart(2, "0");
  const id = `${region}.${game}.${n}`;
  return {
    schemaVersion: 2,
    id,
    region,
    game,
    order,
    meta: { concepts: ["direction"], value: "patience", difficulty: 1, ageLevels: ["minik"], mebOutcomes: [] },
    grid: { rows: ["....", "....", "....", "...."] },
    robot: { x: 0, y: 3, facing: "north" },
    goal: { reach: { x: 0, y: 0 } },
    cards: { palette: ["forward", "turn_left", "turn_right"], maxProgramLength: 5 },
    voice: { ...voiceKeysFor(id), hints: ["roboya.hint.listen", "roboya.hint.check_card", "roboya.hint.show_card"] },
  };
}

export function resize(level: LevelDto, width: number, height: number): LevelDto {
  const w = clamp(width, MIN_SIZE, MAX_SIZE);
  const h = clamp(height, MIN_SIZE, MAX_SIZE);
  const rows = Array.from({ length: h }, (_, y) => {
    const old = level.grid.rows[y] ?? "";
    return (old + ".".repeat(w)).slice(0, w);
  });
  const inside = (x: number, y: number) => x < w && y < h;
  const items = (level.items ?? []).filter((i) => inside(i.x, i.y));
  const kept = new Set(items.map((i) => i.id));
  const robot = inside(level.robot.x, level.robot.y) ? level.robot : { ...level.robot, x: 0, y: h - 1 };
  const goal = { ...level.goal };
  if (goal.reach && !inside(goal.reach.x, goal.reach.y)) delete goal.reach;
  if (goal.collect) goal.collect = goal.collect.filter((id) => kept.has(id));
  return clean({ ...level, grid: { rows: setCell(rows, robot.x, robot.y, ".") }, robot, items, goal });
}

export function applyTool(level: LevelDto, x: number, y: number, tool: Tool, brush?: ItemBrush): LevelDto {
  const { width, height } = size(level);
  if (x < 0 || y < 0 || x >= width || y >= height) return level;
  const isRobot = level.robot.x === x && level.robot.y === y;
  const isGoal = level.goal.reach?.x === x && level.goal.reach?.y === y;

  switch (tool) {
    case "wall":
      if (isRobot || isGoal) return level;
      return removeItemAt({ ...level, grid: { rows: setCell(level.grid.rows, x, y, "#") } }, x, y);
    case "floor":
      return { ...level, grid: { rows: setCell(level.grid.rows, x, y, ".") } };
    case "robot": {
      if (isRobot) {
        const next = FACINGS[(FACINGS.indexOf(level.robot.facing) + 1) % FACINGS.length] ?? "north";
        return { ...level, robot: { ...level.robot, facing: next } };
      }
      const moved = removeItemAt(level, x, y);
      return { ...moved, grid: { rows: setCell(moved.grid.rows, x, y, ".") }, robot: { ...level.robot, x, y } };
    }
    case "goal":
      if (isRobot) return level;
      return { ...level, grid: { rows: setCell(level.grid.rows, x, y, ".") }, goal: { ...level.goal, reach: { x, y } } };
    case "item": {
      if (isRobot || !brush) return level;
      const base = removeItemAt(level, x, y);
      const id = uniqueId(base, brush.kind);
      const item: ItemDto = { id, kind: brush.kind, x, y, ...(brush.color ? { color: brush.color } : {}) };
      return {
        ...base,
        grid: { rows: setCell(base.grid.rows, x, y, ".") },
        items: [...(base.items ?? []), item],
        goal: { ...base.goal, collect: [...(base.goal.collect ?? []), id] },
      };
    }
    case "erase": {
      let next = removeItemAt(level, x, y);
      if (isGoal && (next.goal.collect?.length ?? 0) > 0) {
        const goal = { ...next.goal };
        delete goal.reach;
        next = { ...next, goal };
      }
      return { ...next, grid: { rows: setCell(next.grid.rows, x, y, ".") } };
    }
  }
}

export function togglePaletteCard(level: LevelDto, card: CardId): LevelDto {
  const has = level.cards.palette.includes(card);
  const palette = has ? level.cards.palette.filter((c) => c !== card) : [...level.cards.palette, card];
  if (palette.length === 0) return level;
  const cards = { ...level.cards, palette };
  if (cards.introduces && !palette.includes(cards.introduces)) delete cards.introduces;
  return { ...level, cards };
}

/** Cells visited by a card sequence, for drawing the solver's route. Stops at the first bump. */
export function tracePath(level: LevelDto, cards: readonly string[]): Array<{ x: number; y: number }> {
  const { width, height } = size(level);
  let { x, y } = level.robot;
  let facing = FACINGS.indexOf(level.robot.facing);
  const path = [{ x, y }];
  const step = [
    [0, -1],
    [1, 0],
    [0, 1],
    [-1, 0],
  ] as const;
  for (const card of cards) {
    if (card === "turn_left") facing = (facing + 3) % 4;
    else if (card === "turn_right") facing = (facing + 1) % 4;
    else if (card === "forward" || card === "backward") {
      const [dx, dy] = step[card === "forward" ? facing : (facing + 2) % 4] ?? [0, 0];
      const nx = x + dx;
      const ny = y + dy;
      if (nx < 0 || ny < 0 || nx >= width || ny >= height || level.grid.rows[ny]?.[nx] === "#") break;
      x = nx;
      y = ny;
      path.push({ x, y });
    }
  }
  return path;
}

function removeItemAt(level: LevelDto, x: number, y: number): LevelDto {
  const removed = (level.items ?? []).filter((i) => i.x === x && i.y === y).map((i) => i.id);
  if (removed.length === 0) return level;
  return clean({
    ...level,
    items: (level.items ?? []).filter((i) => !removed.includes(i.id)),
    goal: { ...level.goal, collect: (level.goal.collect ?? []).filter((id) => !removed.includes(id)) },
  });
}

/** Drops empty optional arrays so saved files stay minimal and schema-valid (collect needs ≥ 1). */
function clean(level: LevelDto): LevelDto {
  const next = { ...level, goal: { ...level.goal } };
  if (next.items && next.items.length === 0) delete next.items;
  if (next.goal.collect && next.goal.collect.length === 0) delete next.goal.collect;
  return next;
}

function uniqueId(level: LevelDto, kind: string): string {
  const taken = new Set((level.items ?? []).map((i) => i.id));
  for (let n = 1; ; n++) {
    const id = `${kind}-${n}`;
    if (!taken.has(id)) return id;
  }
}

function setCell(rows: readonly string[], x: number, y: number, ch: "." | "#"): string[] {
  return rows.map((row, ry) => (ry === y ? row.slice(0, x) + ch + row.slice(x + 1) : row));
}

function clamp(v: number, min: number, max: number): number {
  return Math.min(max, Math.max(min, Math.round(v)));
}
