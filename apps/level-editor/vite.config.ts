/// <reference types="vitest/config" />
import react from "@vitejs/plugin-react";
import { defineConfig } from "vite";
import { contentApi } from "./src/server/contentApi";

// Internal tool, dev server only: reads/writes content/levels and asks the C# solver (ADR 0002).
export default defineConfig({
  plugins: [react(), contentApi()],
  server: {
    port: 5174,
    host: "127.0.0.1",
    proxy: { "/solve": "http://127.0.0.1:5199" },
  },
  test: {
    environment: "jsdom",
    globals: true,
    setupFiles: ["./src/test-setup.ts"],
  },
});
