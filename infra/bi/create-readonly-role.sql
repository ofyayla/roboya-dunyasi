-- Read-only role for the dashboards (F1-20, ADR 0028). Run once per environment as the database owner:
--   psql "$ROBOYA_DATABASE_ADMIN_URL" -v bi_password="'<long random password>'" -f infra/bi/create-readonly-role.sql
-- The role can read only the anonymous aggregate views in the `bi` schema, never the tables behind them.
DO $$
BEGIN
    IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'roboya_bi') THEN
        CREATE ROLE roboya_bi LOGIN;
    END IF;
END
$$;

ALTER ROLE roboya_bi PASSWORD :bi_password;
REVOKE ALL ON SCHEMA public FROM roboya_bi;
GRANT USAGE ON SCHEMA bi TO roboya_bi;
GRANT SELECT ON ALL TABLES IN SCHEMA bi TO roboya_bi;
ALTER DEFAULT PRIVILEGES IN SCHEMA bi GRANT SELECT ON TABLES TO roboya_bi;
