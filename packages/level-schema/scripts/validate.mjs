// JSON-schema validation of every level file (structural part of `make validate-content`).
import { readdirSync, readFileSync, statSync } from "node:fs";
import { join, relative } from "node:path";
import Ajv2020 from "ajv/dist/2020.js";

const schema = JSON.parse(readFileSync(new URL("../level.schema.json", import.meta.url), "utf8"));
const ajv = new Ajv2020({ allErrors: true, strict: true, strictRequired: false, allowUnionTypes: true });
const validate = ajv.compile(schema);

export function walk(dir) {
  return readdirSync(dir).flatMap((name) => {
    const p = join(dir, name);
    return statSync(p).isDirectory() ? walk(p) : p.endsWith(".json") ? [p] : [];
  });
}

export function validateFile(path) {
  let data;
  try {
    data = JSON.parse(readFileSync(path, "utf8"));
  } catch (e) {
    return [`invalid JSON: ${e.message}`];
  }
  return validate(data) ? [] : validate.errors.map((e) => `${e.instancePath || "/"} ${e.message}`);
}

if (import.meta.url === `file://${process.argv[1]}`) {
  const root = process.argv[2];
  let failures = 0;
  const files = walk(root).sort();
  for (const file of files) {
    const errors = validateFile(file);
    if (errors.length) {
      failures += errors.length;
      console.log(`✗ ${relative(root, file)}`);
      for (const e of errors) console.log(`    şema: ${e}`);
    }
  }
  console.log(`şema: ${files.length} bölüm, ${failures} hata.`);
  process.exit(failures ? 1 : 0);
}
