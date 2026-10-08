import { useTranslation } from "react-i18next";
import { RoleCard, type Role } from "./shared/RoleCard";
import "./App.css";

const ROLES: Role[] = ["parent", "teacher", "admin"];

export function App() {
  const { t } = useTranslation();
  return (
    <>
      <a className="skip-link" href="#main">
        {t("app.skipToContent")}
      </a>
      <header className="app-header">
        <h1>{t("app.title")}</h1>
        <p>{t("app.tagline")}</p>
      </header>
      <main id="main" className="role-grid">
        {ROLES.map((role) => (
          <RoleCard key={role} role={role} />
        ))}
      </main>
    </>
  );
}
