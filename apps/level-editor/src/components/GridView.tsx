import type { LevelDto } from "@roboya/level-schema";
import { useTranslation } from "react-i18next";
import { dropAt, size } from "../model/levelOps";

const ARROW: Record<string, string> = { north: "▲", east: "▶", south: "▼", west: "◀" };
const KIND_SYMBOL: Record<string, string> = { flower: "✿", honey: "⬢", gear: "⚙", star: "★", fruit: "●", "ship-part": "⚙" };

interface Props {
  level: LevelDto;
  path: Array<{ x: number; y: number }>;
  /** Drop cards of the solver's route per cell ("x,y"), shown as "↓n". */
  drops?: ReadonlyMap<string, number>;
  onCell: (x: number, y: number) => void;
}

/** Grid as a table of buttons: keyboard and screen-reader friendly; symbols plus text, never colour alone. */
export function GridView({ level, path, drops, onCell }: Props) {
  const { t } = useTranslation();
  const { width, height } = size(level);
  const steps = new Map(path.slice(1).map((p, i) => [`${p.x},${p.y}`, i + 1]));
  return (
    <div className="grid" role="grid" aria-label={t("grid.heading")} style={{ gridTemplateColumns: `repeat(${width}, 1fr)` }}>
      {Array.from({ length: height }, (_, y) =>
        Array.from({ length: width }, (_, x) => {
          const wall = level.grid.rows[y]?.[x] === "#";
          const robot = level.robot.x === x && level.robot.y === y;
          const goal = level.goal.reach?.x === x && level.goal.reach?.y === y;
          const item = level.items?.find((i) => i.x === x && i.y === y);
          const step = steps.get(`${x},${y}`);
          const drop = dropAt(level, x, y);
          const given = drops?.get(`${x},${y}`);
          const parts = [
            t("grid.cell", { x, y }),
            wall && t("grid.wall"),
            robot && t("grid.robot", { facing: t(`facing.${level.robot.facing}`) }),
            goal && t("grid.goal"),
            item && t("grid.item", { id: item.id }),
            item?.amount && item.amount > 1 && t("grid.amount", { n: item.amount }),
            drop?.kind === "hive" && t("grid.hive", { id: drop.id, need: drop.need ?? 0 }),
            drop?.kind === "table" && t("grid.table"),
            given !== undefined && t("grid.drops", { n: given }),
            step !== undefined && t("grid.step", { n: step }),
          ].filter(Boolean);
          return (
            <button
              key={`${x},${y}`}
              type="button"
              role="gridcell"
              className={["cell", wall && "cell--wall", step !== undefined && "cell--path"].filter(Boolean).join(" ")}
              aria-label={parts.join(", ")}
              onClick={() => onCell(x, y)}
            >
              {drop && (
                <span className="piece piece--drop">
                  {drop.kind === "hive" ? `⬣${drop.need ?? 0}` : "▭"}
                </span>
              )}
              {goal && <span className="piece piece--goal">🐢</span>}
              {item && (
                <span className={`piece piece--item color-${item.color ?? "none"}`}>
                  {KIND_SYMBOL[item.kind] ?? "?"}
                  {item.amount && item.amount > 1 ? item.amount : ""}
                </span>
              )}
              {robot && <span className="piece piece--robot">{ARROW[level.robot.facing]}</span>}
              {step !== undefined && !robot && <span className="step">{step}</span>}
              {given !== undefined && <span className="drops-mark">↓{given}</span>}
            </button>
          );
        }),
      )}
    </div>
  );
}
