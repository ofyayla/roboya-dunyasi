import { render, screen, waitFor, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { App } from "./App";

const solveResponse = { valid: true, shortestLength: 3, solution: ["forward", "forward", "forward"], errors: [], warnings: [] };

function mockFetch() {
  const calls: Array<{ url: string; init?: RequestInit }> = [];
  vi.stubGlobal(
    "fetch",
    vi.fn(async (url: string, init?: RequestInit) => {
      calls.push({ url, init });
      const body =
        url === "/api/levels"
          ? [{ path: "sabir-ormani/yon-avcisi/01.json", id: "sabir-ormani.yon-avcisi.01", game: "yon-avcisi", order: 1 }]
          : url === "/api/voice-keys"
            ? ["yon_avcisi.l01.intro"]
            : url === "/solve"
              ? solveResponse
              : { saved: "sabir-ormani/yon-avcisi/02.json" };
      return new Response(JSON.stringify(body), { status: 200 });
    }),
  );
  return calls;
}

afterEach(() => vi.unstubAllGlobals());

describe("App", () => {
  it("loadsLevelList_andShowsSolverResult", async () => {
    mockFetch();
    render(<App />);
    expect(await screen.findByRole("button", { name: "sabir-ormani.yon-avcisi.01" })).toBeInTheDocument();
    expect(await screen.findByText(/en kısa çözüm 3 kart/)).toBeInTheDocument();
  });

  it("clickingCellWithWallTool_marksWall", async () => {
    mockFetch();
    render(<App />);
    const grid = screen.getByRole("grid");
    const cell = within(grid).getByRole("gridcell", { name: /^Hücre 2,1$/ });
    await userEvent.click(cell);
    expect(within(grid).getByRole("gridcell", { name: /Hücre 2,1, engel/ })).toBeInTheDocument();
  });

  it("newLevel_thenSave_putsSchemaValidJsonWithShortestLength", async () => {
    const calls = mockFetch();
    render(<App />);
    await screen.findByRole("button", { name: "sabir-ormani.yon-avcisi.01" });
    await userEvent.click(screen.getByRole("button", { name: "Yeni bölüm" }));
    const save = screen.getByRole("button", { name: "Kaydet" });
    await waitFor(() => expect(save).toBeEnabled());
    await userEvent.click(save);
    const put = await waitFor(() => {
      const c = calls.find((x) => x.init?.method === "PUT");
      if (!c) throw new Error("no PUT yet");
      return c;
    });
    expect(put.url).toBe("/api/levels/sabir-ormani%2Fyon-avcisi%2F02.json");
    const saved = JSON.parse(String(put.init?.body));
    expect(saved.id).toBe("sabir-ormani.yon-avcisi.02");
    expect(saved.solution).toEqual({ shortestLength: 3 });
    expect(await screen.findByText(/Kaydedildi/)).toBeInTheDocument();
  });
});
