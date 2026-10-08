import tr from "./tr.json";

function leaves(obj: object, prefix = ""): string[] {
  return Object.entries(obj).flatMap(([k, v]) =>
    typeof v === "object" && v !== null ? leaves(v, `${prefix}${k}.`) : [`${prefix}${k}`],
  );
}

describe("tr.json", () => {
  it("allLeaves_areNonEmptyStrings", () => {
    const flat = leaves(tr);
    expect(flat.length).toBeGreaterThan(0);
    for (const key of flat) {
      const value = key.split(".").reduce<unknown>((o, k) => (o as Record<string, unknown>)[k], tr);
      expect(typeof value === "string" && value.length > 0, key).toBe(true);
    }
  });
});
