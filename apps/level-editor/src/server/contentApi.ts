import type { IncomingMessage, ServerResponse } from "node:http";
import { resolve } from "node:path";
import type { Plugin } from "vite";
import { ContentStore } from "./contentStore";

/** Vite dev-server middleware: /api/levels (list, read, write) and /api/voice-keys. Never part of a build. */
export function contentApi(repoRoot = resolve(__dirname, "../../../..")): Plugin {
  const store = new ContentStore(repoRoot);
  return {
    name: "roboya-content-api",
    apply: "serve",
    configureServer(server) {
      server.middlewares.use("/api", (req, res, next) => {
        handle(store, req, res).catch((e: unknown) => send(res, 400, { error: e instanceof Error ? e.message : String(e) }));
        void next;
      });
    },
  };
}

async function handle(store: ContentStore, req: IncomingMessage, res: ServerResponse): Promise<void> {
  const url = new URL(req.url ?? "/", "http://localhost");
  if (url.pathname === "/voice-keys" && req.method === "GET") {
    return send(res, 200, await store.voiceKeys());
  }
  if (url.pathname === "/levels" && req.method === "GET") {
    return send(res, 200, await store.list());
  }
  const match = /^\/levels\/(.+)$/.exec(url.pathname);
  if (match?.[1]) {
    const path = decodeURIComponent(match[1]);
    if (req.method === "GET") return send(res, 200, JSON.parse(await store.read(path)));
    if (req.method === "PUT") {
      await store.write(path, await body(req));
      return send(res, 200, { saved: path });
    }
  }
  send(res, 404, { error: "not found" });
}

function body(req: IncomingMessage): Promise<string> {
  return new Promise((ok, fail) => {
    let data = "";
    req.setEncoding("utf8");
    req.on("data", (chunk: string) => {
      data += chunk;
      if (data.length > 1_000_000) fail(new Error("body too large"));
    });
    req.on("end", () => ok(data));
    req.on("error", fail);
  });
}

function send(res: ServerResponse, status: number, payload: unknown): void {
  res.statusCode = status;
  res.setHeader("Content-Type", "application/json; charset=utf-8");
  res.end(JSON.stringify(payload));
}
