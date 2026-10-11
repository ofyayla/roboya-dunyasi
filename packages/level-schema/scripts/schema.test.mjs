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

test("validateFile_schemaVersion2_isRejected", () => {
  assert.ok(withLevel((l) => (l.schemaVersion = 2)).length > 0);
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

test("validateFile_schemaVersion1_isRejected", () => {
  assert.ok(withLevel((l) => (l.schemaVersion = 1)).length > 0);
});

// Schema v3: honey carrying, dropping and sharing (ADR 0030, PK-0001..0005).
const village = (l) => {
  l.drops = [
    { id: "petek-1", kind: "hive", x: 1, y: 0, need: 2 },
    { id: "petek-2", kind: "hive", x: 2, y: 0, need: 2 },
    { id: "sofra", kind: "table", x: 3, y: 0 },
  ];
  l.items = [{ id: "bal-1", kind: "honey", x: 0, y: 1, amount: 3 }];
  l.robot.carrying = 2;
  l.cards.palette = ["forward", "turn_right", "drop"];
  l.goal = { share: { among: ["petek-1", "petek-2"], leftoverTo: "sofra" }, carryAtEnd: 0 };
  l.options = { autoDrop: false, askRemaining: true, resetEachRun: true, clearMemoryEachRun: false, intro: true };
  l.starterProgram = [{ op: "drop" }];
};

test("validateFile_villageLevel_isAccepted", () => {
  assert.deepEqual(withLevel(village), []);
});

test("validateFile_deliverGoal_isAccepted", () => {
  assert.deepEqual(
    withLevel((l) => {
      village(l);
      l.goal = { deliver: [{ to: "petek-1", count: 2 }] };
    }),
    [],
  );
});

test("validateFile_homeDropKind_isRejected", () => {
  assert.ok(withLevel((l) => { village(l); l.drops[0].kind = "home"; }).length > 0);
});

test("validateFile_hiveNeedZero_isRejected", () => {
  assert.ok(withLevel((l) => { village(l); l.drops[0].need = 0; }).length > 0);
});

test("validateFile_shareAmongOne_isRejected", () => {
  assert.ok(withLevel((l) => { village(l); l.goal.share.among = ["petek-1"]; }).length > 0);
});

test("validateFile_carryAtEndAlone_isRejected", () => {
  assert.ok(withLevel((l) => (l.goal = { carryAtEnd: 1 })).length > 0);
});

test("validateFile_deliverCountZero_isRejected", () => {
  assert.ok(withLevel((l) => { village(l); l.goal = { deliver: [{ to: "petek-1", count: 0 }] }; }).length > 0);
});

test("validateFile_honeyAmountEight_isRejected", () => {
  assert.ok(withLevel((l) => { village(l); l.items[0].amount = 8; }).length > 0);
});

test("validateFile_dropsOverFour_isRejected", () => {
  assert.ok(
    withLevel((l) => {
      l.drops = Array.from({ length: 5 }, (_, i) => ({ id: `p-${i}`, kind: "hive", x: i, y: 0, need: 1 }));
    }).length > 0,
  );
});

test("validateFile_villageStoryProps_areAccepted", () => {
  assert.deepEqual(
    withLevel((l) => (l.story = { intro: { props: ["villager-bee", "honeycomb", "village-table"] } })),
    [],
  );
});
