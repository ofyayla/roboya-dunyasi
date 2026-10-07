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
