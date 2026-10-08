import { validateLevel, type LevelDto } from "@roboya/level-schema";
import { applyTool, levelPath, newLevel, resize, size, togglePaletteCard, tracePath, voiceKeysFor } from "./levelOps";

const base = () => newLevel("sabir-ormani", "yon-avcisi", 11);

describe("newLevel", () => {
  it("newLevel_defaults_areSchemaValid", () => {
    const result = validateLevel(base());
    expect(result.valid ? [] : result.errors).toEqual([]);
  });

  it("newLevel_idPathAndVoiceKeys_followConventions", () => {
    const level = base();
    expect(level.id).toBe("sabir-ormani.yon-avcisi.11");
    expect(levelPath(level)).toBe("sabir-ormani/yon-avcisi/11.json");
    expect(level.voice.intro).toBe("yon_avcisi.l11.intro");
    expect(voiceKeysFor("a.kodlama-kutusu.03").success).toBe("kodlama_kutusu.l03.success");
  });
});

describe("applyTool", () => {
  it("wall_onFloor_blocksCell_butNeverRobotOrGoal", () => {
    const level = applyTool(base(), 1, 1, "wall");
    expect(level.grid.rows[1]).toBe(".#..");
    expect(applyTool(base(), 0, 3, "wall")).toEqual(base());
    expect(applyTool(base(), 0, 0, "wall")).toEqual(base());
  });

  it("robot_onSameCell_rotatesClockwise", () => {
    let level = base();
    for (const expected of ["east", "south", "west", "north"]) {
      level = applyTool(level, 0, 3, "robot");
      expect(level.robot.facing).toBe(expected);
    }
  });

  it("robot_onWall_movesAndClearsWall", () => {
    const level = applyTool(applyTool(base(), 2, 2, "wall"), 2, 2, "robot");
    expect(level.robot).toMatchObject({ x: 2, y: 2 });
    expect(level.grid.rows[2]).toBe("....");
  });

  it("item_addsToItemsAndGoalCollect_withUniqueIds", () => {
    let level = applyTool(base(), 1, 0, "item", { kind: "fruit", color: "red" });
    level = applyTool(level, 2, 0, "item", { kind: "fruit" });
    expect(level.items?.map((i) => i.id)).toEqual(["fruit-1", "fruit-2"]);
    expect(level.goal.collect).toEqual(["fruit-1", "fruit-2"]);
    expect(level.items?.[1]).not.toHaveProperty("color");
    const result = validateLevel(level);
    expect(result.valid ? [] : result.errors).toEqual([]);
  });

  it("item_onRobotOrWithoutBrush_isIgnored", () => {
    expect(applyTool(base(), 0, 3, "item", { kind: "fruit" })).toEqual(base());
    expect(applyTool(base(), 1, 1, "item")).toEqual(base());
  });

  it("erase_removesItemAndCleansCollect", () => {
    const withItem = applyTool(base(), 1, 0, "item", { kind: "gear" });
    const erased = applyTool(withItem, 1, 0, "erase");
    expect(erased.items).toBeUndefined();
    expect(erased.goal.collect).toBeUndefined();
  });

  it("erase_goal_onlyWhenItemsRemainAsGoal", () => {
    expect(applyTool(base(), 0, 0, "erase").goal.reach).toEqual({ x: 0, y: 0 });
    const withItem = applyTool(base(), 1, 1, "item", { kind: "fruit" });
    expect(applyTool(withItem, 0, 0, "erase").goal.reach).toBeUndefined();
  });

  it("goal_movesToCell_notOntoRobot", () => {
    expect(applyTool(base(), 3, 1, "goal").goal.reach).toEqual({ x: 3, y: 1 });
    expect(applyTool(base(), 0, 3, "goal")).toEqual(base());
  });

  it("floor_clearsWall", () => {
    const level = applyTool(applyTool(base(), 1, 1, "wall"), 1, 1, "floor");
    expect(level.grid.rows[1]).toBe("....");
  });

  it("outsideGrid_isIgnored", () => {
    expect(applyTool(base(), 9, 9, "wall")).toEqual(base());
  });
});

describe("resize", () => {
  it("resize_growAndShrink_keepsInsideObjects", () => {
    const withItem = applyTool(base(), 3, 0, "item", { kind: "fruit" });
    const grown = resize(withItem, 6, 5);
    expect(size(grown)).toEqual({ width: 6, height: 5 });
    const shrunk = resize(grown, 2, 2);
    expect(shrunk.items).toBeUndefined();
    expect(shrunk.goal.collect).toBeUndefined();
    expect(shrunk.robot).toMatchObject({ x: 0, y: 1 });
  });

  it("resize_clampsToLimits", () => {
    expect(size(resize(base(), 1, 99))).toEqual({ width: 2, height: 12 });
  });

  it("resize_goalOutside_isDropped", () => {
    const moved = applyTool(base(), 3, 0, "goal");
    expect(resize(moved, 2, 4).goal.reach).toBeUndefined();
  });
});

describe("palette and path", () => {
  it("togglePaletteCard_neverEmpties_andDropsStaleIntroduction", () => {
    let level: LevelDto = { ...base(), cards: { palette: ["forward", "turn_right"], maxProgramLength: 4, introduces: "turn_right" } };
    level = togglePaletteCard(level, "turn_right");
    expect(level.cards.palette).toEqual(["forward"]);
    expect(level.cards.introduces).toBeUndefined();
    expect(togglePaletteCard(level, "forward")).toBe(level);
    expect(togglePaletteCard(level, "backward").cards.palette).toEqual(["forward", "backward"]);
  });

  it("tracePath_followsCards_andStopsAtWall", () => {
    const level = applyTool(base(), 1, 2, "wall");
    expect(tracePath(level, ["forward", "turn_right", "forward", "forward"])).toEqual([
      { x: 0, y: 3 },
      { x: 0, y: 2 },
    ]);
    expect(tracePath(base(), ["forward", "turn_left", "turn_right", "backward"])).toEqual([
      { x: 0, y: 3 },
      { x: 0, y: 2 },
      { x: 0, y: 3 },
    ]);
  });
});
