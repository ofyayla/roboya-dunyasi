import type { LevelDto } from "@roboya/level-schema";
import { useEffect, useState } from "react";
import { api, type SolveResponse } from "./api";

export type SolverState = { status: "loading" } | { status: "down" } | { status: "done"; result: SolveResponse };

/** Asks the C# solver after each edit (debounced), cancelling stale requests. */
export function useSolver(level: LevelDto, delayMs = 250): SolverState {
  const [state, setState] = useState<SolverState>({ status: "loading" });
  useEffect(() => {
    const controller = new AbortController();
    setState({ status: "loading" });
    const timer = setTimeout(() => {
      api
        .solve(level, controller.signal)
        .then((result) => setState({ status: "done", result }))
        .catch((e: unknown) => {
          if (!controller.signal.aborted) {
            console.warn("solver unavailable", e);
            setState({ status: "down" });
          }
        });
    }, delayMs);
    return () => {
      clearTimeout(timer);
      controller.abort();
    };
  }, [level, delayMs]);
  return state;
}
