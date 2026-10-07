import Ajv2020 from "ajv/dist/2020.js";
import schema from "../level.schema.json" with { type: "json" };
import type { LevelDto } from "../generated/level.ts";

export type * from "../generated/level.ts";
export { schema };

const ajv = new Ajv2020({ allErrors: true, strict: true, strictRequired: false, allowUnionTypes: true });
const validateFn = ajv.compile<LevelDto>(schema);

export interface SchemaError {
  path: string;
  message: string;
}

/** Structural (JSON-schema) validation. Solvability comes from the C# validator (ADR 0002). */
export function validateLevel(data: unknown): { valid: true; level: LevelDto } | { valid: false; errors: SchemaError[] } {
  if (validateFn(data)) return { valid: true, level: data };
  return {
    valid: false,
    errors: (validateFn.errors ?? []).map((e) => ({ path: e.instancePath || "/", message: e.message ?? "invalid" })),
  };
}
