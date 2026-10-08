import type { SchemaError } from "@roboya/level-schema";
import { useTranslation } from "react-i18next";
import type { SolverState } from "../useSolver";

interface Props {
  solver: SolverState;
  schemaErrors: SchemaError[];
  canSave: boolean;
  saveMessage: string | null;
  onSave: () => void;
}

export function StatusPanel({ solver, schemaErrors, canSave, saveMessage, onSave }: Props) {
  const { t } = useTranslation();
  return (
    <section className="panel status" aria-live="polite" aria-labelledby="status-heading">
      <h2 id="status-heading">{t("status.heading")}</h2>
      {solver.status === "loading" && <p>{t("status.solving")}</p>}
      {solver.status === "down" && <p className="error">{t("status.solverDown")}</p>}
      {solver.status === "done" && (
        <>
          <p className={solver.result.shortestLength !== null ? "ok" : "error"}>
            {solver.result.shortestLength !== null ? `✓ ${t("status.solvable", { n: solver.result.shortestLength })}` : `✗ ${t("status.unsolvable")}`}
          </p>
          {solver.result.errors.length > 0 && (
            <ul className="error">
              {solver.result.errors.map((e) => (
                <li key={e}>{e}</li>
              ))}
            </ul>
          )}
          {solver.result.warnings.length > 0 && (
            <ul className="warn">
              {solver.result.warnings.map((w) => (
                <li key={w}>{w}</li>
              ))}
            </ul>
          )}
        </>
      )}
      {schemaErrors.length === 0 ? (
        <p className="ok">✓ {t("status.schemaOk")}</p>
      ) : (
        <>
          <p className="error">{t("status.schemaErrors")}</p>
          <ul className="error">
            {schemaErrors.map((e) => (
              <li key={e.path + e.message}>
                <code>{e.path}</code> {e.message}
              </li>
            ))}
          </ul>
        </>
      )}
      <button type="button" onClick={onSave} disabled={!canSave}>
        {t("status.save")}
      </button>
      {!canSave && <p className="muted">{t("status.saveBlocked")}</p>}
      {saveMessage && <p className="ok">{saveMessage}</p>}
    </section>
  );
}
