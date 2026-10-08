import type { LevelDto } from "@roboya/level-schema";

export interface SolveResponse {
  valid: boolean;
  shortestLength: number | null;
  solution?: string[];
  errors: string[];
  warnings: string[];
}

export interface LevelListItem {
  path: string;
  id: string;
  game: string;
  order: number;
}

async function json<T>(res: Response): Promise<T> {
  if (!res.ok) throw new Error(`${res.status} ${await res.text()}`);
  return (await res.json()) as T;
}

export const api = {
  listLevels: () => fetch("/api/levels").then((r) => json<LevelListItem[]>(r)),
  readLevel: (path: string) => fetch(`/api/levels/${encodeURIComponent(path)}`).then((r) => json<LevelDto>(r)),
  saveLevel: (path: string, level: LevelDto) =>
    fetch(`/api/levels/${encodeURIComponent(path)}`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(level, null, 2),
    }).then((r) => json<{ saved: string }>(r)),
  voiceKeys: () => fetch("/api/voice-keys").then((r) => json<string[]>(r)),
  solve: (level: LevelDto, signal?: AbortSignal) =>
    fetch("/solve", { method: "POST", body: JSON.stringify(level), signal }).then((r) => json<SolveResponse>(r)),
};
