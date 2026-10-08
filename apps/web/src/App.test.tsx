import { render, screen } from "@testing-library/react";
import { App } from "./App";

describe("App", () => {
  it("renders_threeRoles_withHeadings", () => {
    render(<App />);
    expect(screen.getByRole("heading", { level: 1, name: "Roboya Dünyası" })).toBeInTheDocument();
    for (const name of ["Veli", "Öğretmen", "Kurum yöneticisi"]) {
      expect(screen.getByRole("heading", { level: 2, name })).toBeInTheDocument();
    }
  });

  it("renders_skipLink_toMainContent", () => {
    render(<App />);
    expect(screen.getByRole("link", { name: "İçeriğe geç" })).toHaveAttribute("href", "#main");
  });
});
