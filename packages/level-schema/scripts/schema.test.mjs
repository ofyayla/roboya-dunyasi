import { test } from "node:test";
import assert from "node:assert/strict";
import { mkdtempSync, writeFileSync, readFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { validateFile, walk } from "./validate.mjs";

const sample = new URL("../../../content/levels/sabir-ormani/yon-avcisi/01.json", import.meta.url);

function withLevel(mutate) {
  const level = JSON.parse(readFileSync(sample, "utf8"));
  mutate(level);
  const dir = mkdtempSync(join(tmpdir(), "lvl-"));
  const file = join(dir, "x.json");
  writeFileSync(file, JSON.stringify(level));
  return validateFile(file);
}

test("validateFile_prototypeLevel_hasNoErrors", () => {
  assert.deepEqual(withLevel(() => {}), []);
});

test("validateFile_unknownProperty_isRejected", () => {
  assert.ok(withLevel((l) => (l.childName = "Ali")).length > 0);
});

test("validateFile_badGridCharacter_isRejected", () => {
  assert.ok(withLevel((l) => (l.grid.rows[0] = "..x.")).length > 0);
});

test("validateFile_goalWithoutReachOrCollect_isRejected", () => {
  assert.ok(withLevel((l) => (l.goal = {})).length > 0);
});

test("validateFile_repeatWithoutTimes_isRejected", () => {
  assert.ok(withLevel((l) => (l.starterProgram = [{ op: "repeat", body: [] }])).length > 0);
});

test("validateFile_nestedCommands_areAccepted", () => {
  const errors = withLevel((l) => {
    l.starterProgram = [
      { op: "repeat", times: 2, body: [{ op: "forward" }] },
      { op: "if", condition: { type: "not", inner: { type: "path_blocked" } }, then: [{ op: "forward" }] },
      { op: "call", name: "dans" },
      { op: "action", id: "zipla" },
    ];
  });
  assert.deepEqual(errors, []);
});

test("validateFile_wrongSchemaVersion_isRejected", () => {
  assert.ok(withLevel((l) => (l.schemaVersion = 4)).length > 0);
});

test("validateFile_invalidJson_reportsParseError", () => {
  const dir = mkdtempSync(join(tmpdir(), "lvl-"));
  const file = join(dir, "bad.json");
  writeFileSync(file, "{");
  assert.match(validateFile(file)[0], /invalid JSON/);
});

test("walk_contentTree_findsAllLevels", () => {
  assert.ok(walk(new URL("../../../content/levels", import.meta.url).pathname).length >= 10);
});

test("validateFile_storyScenes_areAccepted", () => {
  const errors = withLevel((l) => {
    l.story = {
      intro: { roboya: "surprised", friend: "explaining", props: ["log", "apple"] },
      outro: { roboya: "proud", friend: "thanks" },
    };
  });
  assert.deepEqual(errors, []);
});

test("validateFile_storyUnknownPose_isRejected", () => {
  assert.ok(withLevel((l) => (l.story = { intro: { roboya: "angry" } })).length > 0);
});

test("validateFile_storyTooManyProps_isRejected", () => {
  assert.ok(withLevel((l) => (l.story = { intro: { props: ["apple", "pear", "gear", "log"] } })).length > 0);
});

test("validateFile_storyUnknownField_isRejected", () => {
  assert.ok(withLevel((l) => (l.story = { intro: { childName: "Ali" } })).length > 0);
});

test("validateFile_schemaVersion2_isRejected", () => {
  assert.ok(withLevel((l) => (l.schemaVersion = 2)).length > 0);
});

test("validateFile_schemaVersion1_isRejected", () => {
  assert.ok(withLevel((l) => (l.schemaVersion = 1)).length > 0);
});

// Village square (koy-meydani, ADR 0031): no grid, a `sharing` block instead.
const village = () => ({
  schemaVersion: 3,
  id: "paylasim-koyu.koy-meydani.01",
  region: "paylasim-koyu",
  game: "koy-meydani",
  order: 1,
  meta: { concepts: ["counting"], value: "sharing", difficulty: 1, ageLevels: ["minik"] },
  sharing: {
    hives: [{ need: 1 }, { need: 1 }],
    basket: { start: 2 },
    goal: "deliver",
    limits: { maxCards: 3 },
  },
  voice: { intro: "koy_meydani.l01.intro" },
});

const withVillage = (mutate) => {
  const level = village();
  mutate(level);
  const dir = mkdtempSync(join(tmpdir(), "village-"));
  const file = join(dir, "01.json");
  writeFileSync(file, JSON.stringify(level));
  return validateFile(file);
};

test("validateFile_villageLevel_isAccepted", () => {
  assert.deepEqual(withVillage(() => {}), []);
});

test("validateFile_villageRepeatBoxLevel_isAccepted", () => {
  assert.deepEqual(
    withVillage((l) => {
      l.sharing = { hives: [{}, {}, {}], basket: { start: 6 }, goal: "share", limits: { maxCards: 3 }, repeatBox: { slots: 3, maxTurns: 3 } };
    }),
    [],
  );
});

test("validateFile_villageWithoutSharing_isRejected", () => {
  assert.ok(withVillage((l) => delete l.sharing).length > 0);
});

test("validateFile_villageWithAGrid_isRejected", () => {
  assert.ok(withVillage((l) => (l.grid = { rows: ["..."] })).length > 0);
});

test("validateFile_gridGameWithSharing_isRejected", () => {
  assert.ok(withLevel((l) => (l.sharing = village().sharing)).length > 0);
});

test("validateFile_gridGameWithoutAGrid_isRejected", () => {
  assert.ok(withLevel((l) => delete l.grid).length > 0);
});

test("validateFile_villageFiveHives_isRejected", () => {
  assert.ok(withVillage((l) => (l.sharing.hives = Array.from({ length: 5 }, () => ({ need: 1 })))).length > 0);
});

test("validateFile_villageUnknownGoal_isRejected", () => {
  assert.ok(withVillage((l) => (l.sharing.goal = "swap")).length > 0);
});

test("validateFile_villageWithoutLimits_isRejected", () => {
  assert.ok(withVillage((l) => delete l.sharing.limits).length > 0);
});

test("validateFile_villageRepeatBoxTurnsOverThree_isRejected", () => {
  assert.ok(withVillage((l) => (l.sharing.repeatBox = { slots: 2, maxTurns: 4 })).length > 0);
});
