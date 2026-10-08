import { useTranslation } from "react-i18next";

export type Role = "parent" | "teacher" | "admin";

/** One entry point per adult role; the real feature screens arrive with F1/F2 tasks. */
export function RoleCard({ role }: { role: Role }) {
  const { t } = useTranslation();
  return (
    <article className="role-card" aria-labelledby={`role-${role}`}>
      <h2 id={`role-${role}`}>{t(`roles.${role}.title`)}</h2>
      <p>{t(`roles.${role}.description`)}</p>
      <span className="badge">{t("status.comingSoon")}</span>
    </article>
  );
}
