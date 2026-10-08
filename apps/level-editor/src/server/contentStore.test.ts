// @vitest-environment node
import { mkdtemp, mkdir, readFile, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { ContentStore } from "./contentStore";

async function repo(): Promise<string> {
  const root = await mkdtemp(join(tmpdir(), "editor-"));
  await mkdir(join(root, "content/levels/sabir-ormani/yon-avcisi"), { recursive: true });
  await mkdir(join(root, "content/voice"), { recursive: true });
  await writeFile(join(root, "content/levels/sabir-ormani/yon-avcisi/01.json"), JSON.stringify({ id: "a.b.01", game: "yon-avcisi", order: 1 }));
  await writeFile(join(root, "content/voice/script.csv"), 'key,text,context,level_ids\na.b,"Merhaba, dünya",,\n\nc.d,x,,\n');
  return root;
}

describe("ContentStore", () => {
  it("list_returnsLevelsWithRelativePaths", async () => {
    const store = new ContentStore(await repo());
    expect(await store.list()).toEqual([{ path: "sabir-ormani/yon-avcisi/01.json", id: "a.b.01", game: "yon-avcisi", order: 1 }]);
  });

  it("write_thenRead_roundTripsWithTrailingNewline", async () => {
    const root = await repo();
    const store = new ContentStore(root);
    await store.write("sabir-ormani/kodlama-kutusu/01.json", '{"id":"x"}');
    expect(await readFile(join(root, "content/levels/sabir-ormani/kodlama-kutusu/01.json"), "utf8")).toBe('{"id":"x"}\n');
    expect(await store.read("sabir-ormani/kodlama-kutusu/01.json")).toBe('{"id":"x"}\n');
  });

  it("resolveLevel_traversalOrNonJson_isRejected", async () => {
    const store = new ContentStore(await repo());
    expect(() => store.resolveLevel("../voice/script.csv")).toThrow();
    expect(() => store.resolveLevel("../../etc/passwd.json")).toThrow();
    expect(() => store.resolveLevel("sabir-ormani/notes.txt")).toThrow();
  });

  it("write_malformedJson_isRejected", async () => {
    const store = new ContentStore(await repo());
    await expect(store.write("a/b.json", "{")).rejects.toThrow();
  });

  it("voiceKeys_readsFirstColumn", async () => {
    const store = new ContentStore(await repo());
    expect(await store.voiceKeys()).toEqual(["a.b", "c.d"]);
  });
});
