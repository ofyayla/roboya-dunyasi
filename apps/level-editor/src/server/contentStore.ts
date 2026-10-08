import { mkdir, readdir, readFile, writeFile } from "node:fs/promises";
import { dirname, join, relative, resolve, sep } from "node:path";

/** File access for the editor's dev server. Every path is confined to the content root. */
export class ContentStore {
  readonly levelsRoot: string;
  readonly scriptPath: string;

  constructor(repoRoot: string) {
    this.levelsRoot = resolve(repoRoot, "content/levels");
    this.scriptPath = resolve(repoRoot, "content/voice/script.csv");
  }

  /** Resolves a relative level path, rejecting traversal and non-JSON files. */
  resolveLevel(relPath: string): string {
    const full = resolve(this.levelsRoot, relPath);
    const inside = full.startsWith(this.levelsRoot + sep);
    if (!inside || !full.endsWith(".json") || relPath.includes("\0")) {
      throw new Error(`invalid level path: ${relPath}`);
    }
    return full;
  }

  async list(): Promise<Array<{ path: string; id: string; game: string; order: number }>> {
    const files = await walk(this.levelsRoot);
    const entries = await Promise.all(
      files.map(async (file) => {
        const data = JSON.parse(await readFile(file, "utf8")) as { id?: string; game?: string; order?: number };
        return {
          path: relative(this.levelsRoot, file).split(sep).join("/"),
          id: data.id ?? "",
          game: data.game ?? "",
          order: data.order ?? 0,
        };
      }),
    );
    return entries.sort((a, b) => a.path.localeCompare(b.path));
  }

  async read(relPath: string): Promise<string> {
    return readFile(this.resolveLevel(relPath), "utf8");
  }

  async write(relPath: string, json: string): Promise<void> {
    JSON.parse(json); // refuse to write malformed JSON
    const full = this.resolveLevel(relPath);
    await mkdir(dirname(full), { recursive: true });
    await writeFile(full, json.endsWith("\n") ? json : json + "\n", "utf8");
  }

  /** Voice keys from script.csv (first column), for checking a level's narration keys. */
  async voiceKeys(): Promise<string[]> {
    const csv = await readFile(this.scriptPath, "utf8");
    return csv
      .split(/\r?\n/)
      .slice(1)
      .map((line) => line.split(",")[0]?.trim() ?? "")
      .filter((key) => key.length > 0);
  }
}

async function walk(dir: string): Promise<string[]> {
  const entries = await readdir(dir, { withFileTypes: true });
  const nested = await Promise.all(
    entries.map((e) => (e.isDirectory() ? walk(join(dir, e.name)) : Promise.resolve(e.name.endsWith(".json") ? [join(dir, e.name)] : []))),
  );
  return nested.flat();
}
