import * as matchers from "@testing-library/jest-dom/matchers";
import { expect } from "vitest";
import "./i18n";

// Register matchers directly: the "/vitest" entry cannot resolve vitest when npm hoists jest-dom to the root.
expect.extend(matchers);
