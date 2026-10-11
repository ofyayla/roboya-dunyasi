import type { ItemDto } from "@roboya/level-schema";
import { useTranslation } from "react-i18next";
import { DEFAULT_NEED, MAX_AMOUNT, MAX_NEED, type ItemBrush, type Tool } from "../model/levelOps";

const TOOLS: Tool[] = ["wall", "floor", "robot", "goal", "item", "hive", "table", "erase"];
const KINDS: Array<ItemDto["kind"]> = ["fruit", "flower", "honey", "gear", "star", "ship-part"];
const COLORS: Array<NonNullable<ItemDto["color"]>> = ["red", "yellow", "blue", "green", "purple", "orange"];

interface Props {
  tool: Tool;
  brush: ItemBrush;
  onTool: (tool: Tool) => void;
  onBrush: (brush: ItemBrush) => void;
}

export function ToolBar({ tool, brush, onTool, onBrush }: Props) {
  const { t } = useTranslation();
  return (
    <fieldset className="panel">
      <legend>{t("tools.heading")}</legend>
      <div className="tool-list">
        {TOOLS.map((name) => (
          <label key={name} className={`tool ${tool === name ? "tool--active" : ""}`}>
            <input type="radio" name="tool" value={name} checked={tool === name} onChange={() => onTool(name)} />
            {t(`tools.${name}`)}
          </label>
        ))}
      </div>
      {tool === "hive" && (
        <div className="row">
          <label>
            {t("tools.need")}
            <input
              type="number"
              min={1}
              max={MAX_NEED}
              value={brush.need ?? DEFAULT_NEED}
              onChange={(e) => onBrush({ ...brush, need: Math.max(1, Math.min(MAX_NEED, Number(e.target.value))) })}
            />
          </label>
        </div>
      )}
      {tool === "item" && (
        <div className="row">
          <label>
            {t("tools.kind")}
            <select value={brush.kind} onChange={(e) => onBrush({ ...brush, kind: e.target.value as ItemDto["kind"] })}>
              {KINDS.map((k) => (
                <option key={k} value={k}>
                  {t(`kind.${k}`)}
                </option>
              ))}
            </select>
          </label>
          <label>
            {t("tools.color")}
            <select
              value={brush.color ?? ""}
              onChange={(e) => {
                const { color: _previous, ...rest } = brush;
                void _previous;
                onBrush({ ...rest, ...(e.target.value ? { color: e.target.value as ItemDto["color"] } : {}) });
              }}
            >
              <option value="">{t("tools.noColor")}</option>
              {COLORS.map((c) => (
                <option key={c} value={c}>
                  {t(`color.${c}`)}
                </option>
              ))}
            </select>
          </label>
          {brush.kind === "honey" && (
            <label>
              {t("tools.amount")}
              <input
                type="number"
                min={1}
                max={MAX_AMOUNT}
                value={brush.amount ?? 1}
                onChange={(e) => onBrush({ ...brush, amount: Math.max(1, Math.min(MAX_AMOUNT, Number(e.target.value))) })}
              />
            </label>
          )}
        </div>
      )}
    </fieldset>
  );
}
