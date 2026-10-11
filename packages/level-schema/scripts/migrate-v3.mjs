// Migrates level files from schemaVersion 2 to 3. v3 only adds optional honey fields (ADR 0030),
// so the migration bumps the version and keeps key order (schemaVersion stays second).
// Usage: node packages/level-schema/scripts/migrate-v3.mjs content/levels
import { readFileSync, writeFileSync } from "node:fs";
import { walk } from "./validate.mjs";

const root = process.argv[2] ?? "content/levels";
let changed = 0;
for (const file of walk(root)) {
  const level = JSON.parse(readFileSync(file, "utf8"));
  if (level.schemaVersion !== 2) continue;
  level.schemaVersion = 3;
  writeFileSync(file, JSON.stringify(level, null, 2) + "\n");
  changed++;
}
console.log(`migrated ${changed} level(s) to schemaVersion 3`);
