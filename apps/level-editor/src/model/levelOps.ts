import type { AgeLevel, CardId, DropPointDto, Facing, GameId, ItemDto, LevelDto, RegionId, ShareDto } from "@roboya/level-schema";

/** Editing tools. Every operation returns a new level object (React state stays immutable). */
export type Tool = "wall" | "floor" | "robot" | "goal" | "item" | "hive" | "table" | "erase";

export interface ItemBrush {
  kind: ItemDto["kind"];
  color?: ItemDto["color"];
  /** Honey an item gives (1–7, honey only). */
  amount?: number;
  /** Honey a new hive asks for (1–5). */
  need?: number;
}

/** Limits of the honey fields (ADR 0030, PK-0003); the validator holds the stricter per-region rules. */
export const MAX_DROPS = 4;
export const MAX_NEED = 5;
export const MAX_AMOUNT = 7;
export const MAX_CARRYING = 7;
export const DEFAULT_NEED = 2;

export type OptionKey =
  | "ghostPath"
  | "guided"
  | "beeMemoryVisible"
  | "autoDrop"
  | "askRemaining"
  | "resetEachRun"
  | "clearMemoryEachRun"
  | "intro";

export type HoneyGoalKind = "none" | "deliver" | "share";

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
    schemaVersion: 3,
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
  const outside = (level.drops ?? []).filter((d) => !inside(d.x, d.y)).map((d) => d.id);
  const shrunk = removeDropsById({ ...level, grid: { rows: setCell(rows, robot.x, robot.y, ".") }, robot, items, goal }, outside);
  return clean(removeDropAt(shrunk, robot.x, robot.y));
}

export function applyTool(level: LevelDto, x: number, y: number, tool: Tool, brush?: ItemBrush): LevelDto {
  const { width, height } = size(level);
  if (x < 0 || y < 0 || x >= width || y >= height) return level;
  const isRobot = level.robot.x === x && level.robot.y === y;
  const isGoal = level.goal.reach?.x === x && level.goal.reach?.y === y;

  switch (tool) {
    case "wall":
      if (isRobot || isGoal) return level;
      return removeDropAt(removeItemAt({ ...level, grid: { rows: setCell(level.grid.rows, x, y, "#") } }, x, y), x, y);
    case "floor":
      return { ...level, grid: { rows: setCell(level.grid.rows, x, y, ".") } };
    case "robot": {
      if (isRobot) {
        const next = FACINGS[(FACINGS.indexOf(level.robot.facing) + 1) % FACINGS.length] ?? "north";
        return { ...level, robot: { ...level.robot, facing: next } };
      }
      const moved = removeDropAt(removeItemAt(level, x, y), x, y);
      return { ...moved, grid: { rows: setCell(moved.grid.rows, x, y, ".") }, robot: { ...level.robot, x, y } };
    }
    case "goal":
      if (isRobot) return level;
      return { ...level, grid: { rows: setCell(level.grid.rows, x, y, ".") }, goal: { ...level.goal, reach: { x, y } } };
    case "item": {
      if (isRobot || !brush) return level;
      const base = removeDropAt(removeItemAt(level, x, y), x, y);
      const id = uniqueId(base, brush.kind);
      const amount = brush.kind === "honey" ? clamp(brush.amount ?? 1, 1, MAX_AMOUNT) : 1;
      const item: ItemDto = { id, kind: brush.kind, x, y, ...(brush.color ? { color: brush.color } : {}), ...(amount > 1 ? { amount } : {}) };
      return {
        ...base,
        grid: { rows: setCell(base.grid.rows, x, y, ".") },
        items: [...(base.items ?? []), item],
        goal: { ...base.goal, collect: [...(base.goal.collect ?? []), id] },
      };
    }
    case "hive":
      return isRobot ? level : placeHive(level, x, y, clamp(brush?.need ?? DEFAULT_NEED, 1, MAX_NEED));
    case "table":
      return isRobot ? level : placeTable(level, x, y);
    case "erase": {
      let next = removeDropAt(removeItemAt(level, x, y), x, y);
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
  return simulate(level, cards).path;
}

/** How many "drop" cards of a card sequence run on each cell (key "x,y"), for the solver's route preview. */
export function dropMarks(level: LevelDto, cards: readonly string[]): Map<string, number> {
  return simulate(level, cards).drops;
}

function simulate(level: LevelDto, cards: readonly string[]): { path: Array<{ x: number; y: number }>; drops: Map<string, number> } {
  const { width, height } = size(level);
  let { x, y } = level.robot;
  let facing = FACINGS.indexOf(level.robot.facing);
  const path = [{ x, y }];
  const drops = new Map<string, number>();
  const step = [
    [0, -1],
    [1, 0],
    [0, 1],
    [-1, 0],
  ] as const;
  for (const card of cards) {
    if (card === "turn_left") facing = (facing + 3) % 4;
    else if (card === "turn_right") facing = (facing + 1) % 4;
    else if (card === "drop") {
      if ((level.drops ?? []).some((d) => d.x === x && d.y === y)) drops.set(`${x},${y}`, (drops.get(`${x},${y}`) ?? 0) + 1);
    } else if (card === "forward" || card === "backward") {
      const [dx, dy] = step[card === "forward" ? facing : (facing + 2) % 4] ?? [0, 0];
      const nx = x + dx;
      const ny = y + dy;
      if (nx < 0 || ny < 0 || nx >= width || ny >= height || level.grid.rows[ny]?.[nx] === "#") break;
      x = nx;
      y = ny;
      path.push({ x, y });
    }
  }
  return { path, drops };
}

/** Looks for a drop point (hive or table) on a cell. */
export function dropAt(level: LevelDto, x: number, y: number): DropPointDto | undefined {
  return (level.drops ?? []).find((d) => d.x === x && d.y === y);
}

export function honeyGoal(level: LevelDto): HoneyGoalKind {
  if (level.goal.share) return "share";
  if ((level.goal.deliver ?? []).length > 0) return "deliver";
  return "none";
}

export function setCarrying(level: LevelDto, count: number): LevelDto {
  const robot = { ...level.robot };
  const n = clamp(count, 0, MAX_CARRYING);
  if (n > 0) robot.carrying = n;
  else delete robot.carrying;
  return { ...level, robot };
}

/** Sets how many honey a hive must end with (goal.deliver); `null` removes the line. Only hives can be delivered to. */
export function setDeliver(level: LevelDto, hiveId: string, count: number | null): LevelDto {
  const drops = level.drops ?? [];
  if (!drops.some((d) => d.id === hiveId && d.kind === "hive")) return level;
  const rest = (level.goal.deliver ?? []).filter((d) => d.to !== hiveId);
  const deliver = count !== null && count >= 1 ? [...rest, { to: hiveId, count: clamp(count, 1, MAX_NEED) }] : rest;
  const order = new Map(drops.map((d, i) => [d.id, i]));
  deliver.sort((a, b) => (order.get(a.to) ?? 0) - (order.get(b.to) ?? 0));
  return clean({ ...level, goal: { ...level.goal, deliver } });
}

/**
 * Turns the equal-sharing goal on or off. On: every hive (2–3 of them) shares and a table gets the left-over. The engine
 * also wants all honey collected for a share goal, so any honey item missing from goal.collect is added.
 */
export function setShare(level: LevelDto, on: boolean): LevelDto {
  if (!on) {
    const goal = { ...level.goal };
    delete goal.share;
    return clean({ ...level, goal });
  }
  const drops = level.drops ?? [];
  const hives = drops.filter((d) => d.kind === "hive");
  if (hives.length < 2 || hives.length > 3) return level;
  const table = drops.find((d) => d.kind === "table");
  const share: ShareDto = { among: hives.map((h) => h.id), ...(table ? { leftoverTo: table.id } : {}) };
  const honey = (level.items ?? []).filter((i) => i.kind === "honey").map((i) => i.id);
  const collect = [...new Set([...(level.goal.collect ?? []), ...honey])];
  const goal = { ...level.goal, share, ...(collect.length > 0 ? { collect } : {}) };
  return clean({ ...level, goal });
}

/** Honey that must still be in the basket at the end (subtraction levels); `null` removes it. */
export function setCarryAtEnd(level: LevelDto, count: number | null): LevelDto {
  const goal = { ...level.goal };
  if (count === null) delete goal.carryAtEnd;
  else goal.carryAtEnd = clamp(count, 0, MAX_CARRYING);
  return { ...level, goal };
}

/** Switches one option on or off and keeps every other option (a level may carry options this editor has no box for). */
export function setOption(level: LevelDto, key: OptionKey, on: boolean): LevelDto {
  const kept = Object.fromEntries(Object.entries(level.options ?? {}).filter(([name]) => name !== key));
  const options: NonNullable<LevelDto["options"]> = on ? { ...kept, [key]: true } : kept;
  const { options: _previous, ...rest } = level;
  void _previous;
  return Object.keys(options).length > 0 ? { ...rest, options } : rest;
}

/** Starting values for a village level (PK-0004, PK-0005); the grid, honey and goal are the author's. */
export function applyVillagePreset(level: LevelDto, age: Extract<AgeLevel, "minik" | "kasif">): LevelDto {
  const palette: CardId[] = age === "minik" ? ["forward", "turn_right"] : ["forward", "turn_right", "turn_left", "drop"];
  const cards = { ...level.cards, palette };
  if (cards.introduces && !palette.includes(cards.introduces)) delete cards.introduces;
  let next: LevelDto = { ...level, meta: { ...level.meta, concepts: ["counting"], value: "sharing", ageLevels: [age] }, cards };
  const minik = age === "minik";
  next = setOption(next, "resetEachRun", true);
  next = setOption(next, "autoDrop", minik);
  next = setOption(next, "clearMemoryEachRun", minik);
  next = setOption(next, "beeMemoryVisible", !minik);
  if (minik) next = setOption(next, "askRemaining", false);
  return next;
}

/** A village level that is solvable as it is, so the author starts from green and changes things. */
export function newVillageLevel(order: number): LevelDto {
  const base = newLevel("paylasim-koyu", "bal-pesinde", order);
  const minik = order <= 6;
  const common: LevelDto = {
    ...base,
    grid: { rows: ["......", "......", "......"] },
    robot: { x: 0, y: 1, facing: "east" },
    drops: [{ id: "petek-1", kind: "hive", x: 2, y: 1, need: 2 }],
    cards: { palette: ["forward"], maxProgramLength: minik ? 3 : 5 },
  };
  const withGoal: LevelDto = minik
    ? {
        ...common,
        items: [{ id: "honey-1", kind: "honey", x: 1, y: 1, amount: 2 }],
        goal: { collect: ["honey-1"], deliver: [{ to: "petek-1", count: 2 }] },
      }
    : { ...common, robot: { ...common.robot, carrying: 2 }, goal: { deliver: [{ to: "petek-1", count: 2 }] } };
  let level = applyVillagePreset(withGoal, minik ? "minik" : "kasif");
  // Corner starts are guided introductions and level 4 keeps the ghost path (PK-0005).
  if ([1, 7, 13, 19].includes(order)) level = setOption(setOption(level, "intro", true), "guided", true);
  if (order === 4) level = setOption(level, "ghostPath", true);
  return level;
}

function placeHive(level: LevelDto, x: number, y: number, need: number): LevelDto {
  const here = dropAt(level, x, y);
  if (here?.kind === "hive") {
    return { ...level, drops: (level.drops ?? []).map((d) => (d === here ? { ...d, need } : d)) };
  }
  const base = removeDropAt(removeItemAt(level, x, y), x, y);
  const drops = base.drops ?? [];
  if (drops.length >= MAX_DROPS) return level;
  const hive: DropPointDto = { id: uniqueDropId(base, "petek"), kind: "hive", x, y, need };
  return { ...base, grid: { rows: setCell(base.grid.rows, x, y, ".") }, drops: [...drops, hive] };
}

function placeTable(level: LevelDto, x: number, y: number): LevelDto {
  const here = dropAt(level, x, y);
  if (here?.kind === "table") return level;
  const base = removeDropAt(removeItemAt(level, x, y), x, y);
  const drops = base.drops ?? [];
  const table = drops.find((d) => d.kind === "table");
  const grid = { rows: setCell(base.grid.rows, x, y, ".") };
  if (table) return { ...base, grid, drops: drops.map((d) => (d === table ? { ...d, x, y } : d)) };
  if (drops.length >= MAX_DROPS) return level;
  return { ...base, grid, drops: [...drops, { id: "sofra", kind: "table", x, y }] };
}

function removeDropAt(level: LevelDto, x: number, y: number): LevelDto {
  const here = dropAt(level, x, y);
  return here ? removeDropsById(level, [here.id]) : level;
}

/** Removes drop points and repairs the goal lines that pointed at them. */
function removeDropsById(level: LevelDto, ids: readonly string[]): LevelDto {
  if (ids.length === 0) return level;
  const gone = new Set(ids);
  const goal = { ...level.goal };
  const deliver = (goal.deliver ?? []).filter((d) => !gone.has(d.to));
  if (deliver.length > 0) goal.deliver = deliver;
  else delete goal.deliver;
  if (goal.share) {
    const among = goal.share.among.filter((id) => !gone.has(id));
    if (among.length < 2) {
      delete goal.share;
    } else {
      const leftover = goal.share.leftoverTo;
      goal.share = { among, ...(leftover && !gone.has(leftover) ? { leftoverTo: leftover } : {}) };
    }
  }
  return clean({ ...level, drops: (level.drops ?? []).filter((d) => !gone.has(d.id)), goal });
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
  if (next.drops && next.drops.length === 0) delete next.drops;
  if (next.goal.deliver && next.goal.deliver.length === 0) delete next.goal.deliver;
  if (next.goal.collect && next.goal.collect.length === 0) delete next.goal.collect;
  return next;
}

function uniqueDropId(level: LevelDto, prefix: string): string {
  const taken = new Set((level.drops ?? []).map((d) => d.id));
  for (let n = 1; ; n++) {
    const id = `${prefix}-${n}`;
    if (!taken.has(id)) return id;
  }
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
