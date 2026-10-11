import { validateLevel, type LevelDto } from "@roboya/level-schema";
import {
  applyTool,
  applyVillagePreset,
  dropAt,
  dropMarks,
  honeyGoal,
  levelPath,
  newLevel,
  newVillageLevel,
  resize,
  setCarrying,
  setCarryAtEnd,
  setDeliver,
  setOption,
  setShare,
  size,
  togglePaletteCard,
  tracePath,
  voiceKeysFor,
} from "./levelOps";

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

const valid = (level: LevelDto) => {
  const result = validateLevel(level);
  return result.valid ? [] : result.errors;
};

/** The 6 x 3 village board without drop points: robot on the left, reach cell on the right, no honey goal. */
const noDrops = (): LevelDto => {
  const { drops: _drops, ...rest } = newVillageLevel(8);
  void _drops;
  return { ...rest, goal: { reach: { x: 5, y: 1 } }, robot: { x: 0, y: 1, facing: "east" } };
};

describe("village levels", () => {
  it("newVillageLevel_minikAndKasif_areSchemaValid", () => {
    expect(valid(newVillageLevel(2))).toEqual([]);
    expect(valid(newVillageLevel(7))).toEqual([]);
    expect(newVillageLevel(7).id).toBe("paylasim-koyu.bal-pesinde.07");
    expect(levelPath(newVillageLevel(7))).toBe("paylasim-koyu/bal-pesinde/07.json");
  });

  it("newVillageLevel_followsTheVillageDecisions", () => {
    const minik = newVillageLevel(2);
    expect(minik.meta).toMatchObject({ concepts: ["counting"], value: "sharing", ageLevels: ["minik"] });
    expect(minik.cards.palette).toEqual(["forward", "turn_right"]);
    expect(minik.options).toEqual({ resetEachRun: true, autoDrop: true, clearMemoryEachRun: true });
    const kasif = newVillageLevel(8);
    expect(kasif.cards.palette).toContain("drop");
    expect(kasif.options).toEqual({ resetEachRun: true, beeMemoryVisible: true });
    expect(newVillageLevel(7).options).toMatchObject({ intro: true, guided: true });
    expect(newVillageLevel(1).options).toMatchObject({ intro: true, guided: true, autoDrop: true });
    expect(newVillageLevel(4).options).toMatchObject({ ghostPath: true });
    expect(newVillageLevel(5).options).not.toHaveProperty("intro");
  });

  it("hive_placedOnFloor_getsUniqueIdsAndNeed", () => {
    let level = applyTool(noDrops(), 2, 0, "hive", { kind: "fruit", need: 3 });
    level = applyTool(level, 3, 0, "hive", { kind: "fruit" });
    expect(level.drops).toEqual([
      { id: "petek-1", kind: "hive", x: 2, y: 0, need: 3 },
      { id: "petek-2", kind: "hive", x: 3, y: 0, need: 2 },
    ]);
    expect(valid(level).filter((e) => e.path.startsWith("/drops"))).toEqual([]);
  });

  it("hive_onExistingHive_onlyChangesNeed", () => {
    const level = applyTool(applyTool(noDrops(), 2, 0, "hive", { kind: "fruit", need: 3 }), 2, 0, "hive", { kind: "fruit", need: 5 });
    expect(level.drops).toEqual([{ id: "petek-1", kind: "hive", x: 2, y: 0, need: 5 }]);
  });

  it("hive_onRobotIsIgnored_andReplacesAnItem", () => {
    expect(applyTool(noDrops(), 0, 1, "hive", { kind: "fruit" })).toEqual(noDrops());
    const withHoney = applyTool(noDrops(), 2, 0, "item", { kind: "honey", amount: 3 });
    const replaced = applyTool(withHoney, 2, 0, "hive", { kind: "fruit" });
    expect(replaced.items).toBeUndefined();
    expect(replaced.goal.collect).toBeUndefined();
    expect(dropAt(replaced, 2, 0)?.kind).toBe("hive");
  });

  it("hive_fifthDropPoint_isIgnored", () => {
    let level = noDrops();
    for (const x of [1, 2, 3, 4]) level = applyTool(level, x, 0, "hive", { kind: "fruit" });
    expect(level.drops).toHaveLength(4);
    expect(applyTool(level, 5, 0, "hive", { kind: "fruit" })).toBe(level);
    expect(applyTool(level, 5, 0, "table")).toBe(level);
  });

  it("table_isOnlyOne_andMovesWithItsId", () => {
    let level = applyTool(noDrops(), 4, 0, "table");
    level = applyTool(level, 5, 2, "table");
    expect(level.drops).toEqual([{ id: "sofra", kind: "table", x: 5, y: 2 }]);
    expect(applyTool(level, 5, 2, "table")).toBe(level);
  });

  it("table_onAHive_replacesIt_andFixesTheGoal", () => {
    let level = applyTool(applyTool(noDrops(), 2, 0, "hive", { kind: "fruit" }), 3, 0, "hive", { kind: "fruit" });
    level = setShare(level, true);
    level = applyTool(level, 3, 0, "table");
    expect(level.drops?.map((d) => d.id)).toEqual(["petek-1", "sofra"]);
    expect(level.goal.share).toBeUndefined();
  });

  it("wallRobotAndErase_onADropPoint_removeItAndCleanTheGoal", () => {
    const withHives = applyTool(applyTool(noDrops(), 2, 0, "hive", { kind: "fruit", need: 2 }), 3, 0, "hive", { kind: "fruit", need: 1 });
    const delivering = setDeliver(setDeliver(withHives, "petek-1", 2), "petek-2", 1);
    for (const tool of ["wall", "erase"] as const) {
      const after = applyTool(delivering, 3, 0, tool);
      expect(after.drops?.map((d) => d.id)).toEqual(["petek-1"]);
      expect(after.goal.deliver).toEqual([{ to: "petek-1", count: 2 }]);
    }
    const moved = applyTool(delivering, 2, 0, "robot");
    expect(moved.drops?.map((d) => d.id)).toEqual(["petek-2"]);
    expect(moved.goal.deliver).toEqual([{ to: "petek-2", count: 1 }]);
  });

  it("item_honeyAmount_isStoredOnlyAboveOne", () => {
    const three = applyTool(noDrops(), 1, 0, "item", { kind: "honey", amount: 3 });
    expect(three.items?.[0]).toMatchObject({ kind: "honey", amount: 3 });
    const one = applyTool(noDrops(), 1, 0, "item", { kind: "honey", amount: 1 });
    expect(one.items?.[0]).not.toHaveProperty("amount");
    const flower = applyTool(noDrops(), 1, 0, "item", { kind: "flower", amount: 4 });
    expect(flower.items?.[0]).not.toHaveProperty("amount");
  });

  it("setShare_needsTwoOrThreeHives_andCollectsAllHoney", () => {
    const oneHive = applyTool(noDrops(), 2, 0, "hive", { kind: "fruit" });
    expect(setShare(oneHive, true)).toBe(oneHive);
    let level = applyTool(applyTool(oneHive, 3, 0, "hive", { kind: "fruit" }), 4, 0, "table");
    level = applyTool(applyTool(level, 1, 2, "item", { kind: "honey", amount: 3 }), 2, 2, "item", { kind: "honey" });
    level = setShare(level, true);
    expect(level.goal.share).toEqual({ among: ["petek-1", "petek-2"], leftoverTo: "sofra" });
    expect(level.goal.collect).toEqual(expect.arrayContaining(["honey-1", "honey-2"]));
    expect(honeyGoal(level)).toBe("share");
    expect(setShare(level, false).goal.share).toBeUndefined();
  });

  it("removingAHive_fromAShareOfThree_keepsTheShare_butNotFromTwo", () => {
    let level = noDrops();
    for (const x of [1, 2, 3]) level = applyTool(level, x, 0, "hive", { kind: "fruit" });
    level = setShare(level, true);
    expect(applyTool(level, 3, 0, "erase").goal.share).toEqual({ among: ["petek-1", "petek-2"] });
    expect(applyTool(applyTool(level, 3, 0, "erase"), 2, 0, "erase").goal.share).toBeUndefined();
  });

  it("setDeliver_addsReplacesAndRemovesLines_inHiveOrder", () => {
    const hives = applyTool(applyTool(noDrops(), 2, 0, "hive", { kind: "fruit" }), 3, 0, "hive", { kind: "fruit" });
    let level = setDeliver(setDeliver(hives, "petek-2", 1), "petek-1", 2);
    expect(level.goal.deliver).toEqual([
      { to: "petek-1", count: 2 },
      { to: "petek-2", count: 1 },
    ]);
    level = setDeliver(level, "petek-1", 3);
    expect(level.goal.deliver?.[0]).toEqual({ to: "petek-1", count: 3 });
    level = setDeliver(setDeliver(level, "petek-1", null), "petek-2", 0);
    expect(level.goal.deliver).toBeUndefined();
    expect(honeyGoal(level)).toBe("none");
    expect(setDeliver(hives, "nope", 1)).toBe(hives);
  });

  it("carryingAndCarryAtEnd_areClampedAndRemovable", () => {
    const level = noDrops();
    expect(setCarrying(level, 5).robot.carrying).toBe(5);
    expect(setCarrying(level, 99).robot.carrying).toBe(7);
    expect(setCarrying(setCarrying(level, 5), 0).robot).not.toHaveProperty("carrying");
    expect(setCarryAtEnd(level, 2).goal.carryAtEnd).toBe(2);
    expect(setCarryAtEnd(setCarryAtEnd(level, 2), null).goal).not.toHaveProperty("carryAtEnd");
  });

  it("setOption_keepsEveryOtherOption", () => {
    // The old editor rewrote options and kept only ghostPath and guided: a village level lost its settings on save.
    const level: LevelDto = { ...newVillageLevel(2), options: { autoDrop: true, resetEachRun: true, predict: true, ghostPath: true } };
    const off = setOption(level, "ghostPath", false);
    expect(off.options).toEqual({ autoDrop: true, resetEachRun: true, predict: true });
    expect(setOption(off, "guided", true).options).toEqual({ autoDrop: true, resetEachRun: true, predict: true, guided: true });
    expect(setOption({ ...level, options: { ghostPath: true } }, "ghostPath", false)).not.toHaveProperty("options");
  });

  it("applyVillagePreset_switchesBetweenMinikAndKasif", () => {
    const kasif = applyVillagePreset({ ...newVillageLevel(2), options: { autoDrop: true, askRemaining: true, ghostPath: true, guided: true } }, "kasif");
    expect(kasif.meta.ageLevels).toEqual(["kasif"]);
    expect(kasif.options).toEqual({ ghostPath: true, guided: true, askRemaining: true, resetEachRun: true, beeMemoryVisible: true });
    const minik = applyVillagePreset(kasif, "minik");
    expect(minik.cards.palette).toEqual(["forward", "turn_right"]);
    expect(minik.options).not.toHaveProperty("askRemaining");
    expect(minik.options).toMatchObject({ autoDrop: true, clearMemoryEachRun: true });
    expect(minik.options).not.toHaveProperty("beeMemoryVisible");
    const introducing = applyVillagePreset({ ...kasif, cards: { ...kasif.cards, introduces: "drop" } }, "minik");
    expect(introducing.cards.introduces).toBeUndefined();
  });

  it("resize_dropPointsOutside_areRemovedWithTheirGoalLines", () => {
    let level = applyTool(applyTool(noDrops(), 1, 0, "hive", { kind: "fruit" }), 5, 0, "hive", { kind: "fruit" });
    level = setDeliver(setDeliver(level, "petek-1", 1), "petek-2", 2);
    const shrunk = resize(level, 3, 3);
    expect(shrunk.drops?.map((d) => d.id)).toEqual(["petek-1"]);
    expect(shrunk.goal.deliver).toEqual([{ to: "petek-1", count: 1 }]);
    expect(resize(applyTool(noDrops(), 5, 0, "hive", { kind: "fruit" }), 3, 3).drops).toBeUndefined();
  });

  it("anEditorBuiltSharingLevel_passesTheSchema", () => {
    let level: LevelDto = { ...noDrops(), robot: { x: 0, y: 1, facing: "east", carrying: 5 } };
    level = applyTool(applyTool(applyTool(level, 1, 1, "hive", { kind: "fruit", need: 2 }), 2, 1, "hive", { kind: "fruit", need: 2 }), 3, 1, "table");
    level = setShare(level, true);
    const { reach: _reach, ...goal } = level.goal;
    void _reach;
    level = { ...level, goal };
    expect(valid(level)).toEqual([]);
    expect(level.goal).toEqual({ share: { among: ["petek-1", "petek-2"], leftoverTo: "sofra" } });
  });
});

describe("route preview", () => {
  it("dropMarks_countsDropCardsPerCell_andTracePathIgnoresThem", () => {
    const level = applyTool(noDrops(), 2, 1, "hive", { kind: "fruit" });
    const cards = ["forward", "forward", "drop", "drop", "forward", "drop"];
    expect(dropMarks(level, cards)).toEqual(new Map([["2,1", 2]]));
    expect(tracePath(level, cards)).toEqual([
      { x: 0, y: 1 },
      { x: 1, y: 1 },
      { x: 2, y: 1 },
      { x: 3, y: 1 },
    ]);
  });
});
