import { useTranslation } from "react-i18next";
import type { LevelListItem } from "../api";

interface Props {
  levels: LevelListItem[];
  selected: string | null;
  onOpen: (path: string) => void;
  onNew: () => void;
}

export function LevelList({ levels, selected, onOpen, onNew }: Props) {
  const { t } = useTranslation();
  return (
    <nav className="level-list" aria-labelledby="levels-heading">
      <h2 id="levels-heading">{t("levels.heading")}</h2>
      <button type="button" onClick={onNew}>
        {t("levels.new")}
      </button>
      {levels.length === 0 ? (
        <p className="muted">{t("levels.empty")}</p>
      ) : (
        <ul className="plain">
          {levels.map((l) => (
            <li key={l.path}>
              <button
                type="button"
                className={`link ${selected === l.path ? "link--active" : ""}`}
                aria-current={selected === l.path ? "page" : undefined}
                onClick={() => onOpen(l.path)}
              >
                {l.id}
              </button>
            </li>
          ))}
        </ul>
      )}
    </nav>
  );
}
