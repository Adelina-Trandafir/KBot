# SLICELESS - Security plan: new system next to the legacy one (11.10.2026)

Plan: `docs/PLAN_Securitate_API.md`. Operator decision in chat: build everything, **delete nothing legacy
until the operator has tested both systems**. Help recorded as 0000-67.

## What changed and why

### S1 - mandatory change of the initial password (server)
- `sql/SEC01_conturi_securitate.sql`: table `AVACONT_COMUN.Conturi_Securitate` (UN, MustChangePassword, PasswordChangedAt, AuthVersion). **Run it before deploying.** Until then the code logs an error and treats every account as "not flagged" (it does not lock anyone out).
- `routes/auth/account_state.py`: reads/writes that state; `current_version` is cached 30 s for the guard.
- `routes/auth/password_policy.py`: reads the real policy (`simple_password_check` variables + `information_schema.PLUGINS` + `strict_password_validation`) and evaluates a password into every unmet requirement. No active plugin = only an 8-char floor and an ERROR in the log.
- `routes/auth/session_store.py`: `Session.scope` (`full` / `password_change`), `auth_version`, per-session `abs_ttl` (10 min for the restricted scope), `revoke_user(username)` in the memory and the Redis store; scope/version persist in the Redis blob.
- `routes/auth/guard.py`: `require_session` refuses a restricted session with 403 `PASSWORD_CHANGE_REQUIRED`; new `require_session_any_scope` (policy, code, change, logout); AuthVersion mismatch -> 401 `SESSION_REVOKED`; a DB failure with nothing cached -> 503 (fails closed).
- `routes/auth/auth.py`: `/units` and `/login` answer `{"password_change_required": true, "Token": <restricted>}` for a flagged account; new `GET /api/auth/password/policy`; `/password/change` judges the policy first (400 `PASSWORD_POLICY_FAILED` + `violations` + `policy`), maps MariaDB 1819 to the same answer without spending the code, then clears the flag, bumps AuthVersion, revokes every session of the account (a voluntary change from a full session gets a fresh `Token` back). Payload length limit 256.
- `routes/admin_cli/admin_cli.py`: `must-change --un U` (flag + bump + journal). Call it after every reset to an initial password.

### S1 - client
- Domain: `PasswordPolicyInfo`, `PasswordViolation`, `PasswordPolicyFailure`, `PasswordPolicyEvaluator`, `UnitsOutcome`; `PasswordChangeResult` gains `Token`/`ReloginRequired`; `LoginResult.PasswordChangeRequired`.
- Api: `IAuthApi.CheckAccessAsync`, `GetPasswordPolicyAsync`; `ApiException.PasswordFailure`; `LoginAsync` throws reason `PASSWORD_CHANGE_REQUIRED` if the account was flagged mid-session.
- App: `Auth/ForcedPasswordChangeForm` (Designer + code + resx); `LoginForm` opens it after Continue; closing it keeps the application shut. `SetariInfoView`: removed the hard-coded 8-character check, shows every unmet requirement, adopts the fresh token.

### S2 - consumers moved off the shared key (legacy routes still there)
- `mfp.py`: `GET/HEAD /api/mfp/templates/ddf|ord` with `require_session`. `KBot.Xfa`: `TemplateDownloader` sends the session bearer (`TokenProvider`, set in `Program.RunShellWithLogin`); `API_KEY` constant deleted from `Config.vb`.
- `utils/dbname.py`: `validate_db_name` moved out of `routes.admin`; `forexe/angajamente.py` and `angajament_dump.py` import it from there (`routes.admin` re-exports it for the legacy module).
- Administration has its own credential: `utils/security.require_admin` (header `X-Admin-Token`, `ADMIN_TOKEN` in the server's `config.py`, constant-time compare). Applied to `/api/admin/users`, `/api/admin/login/reset`, `/api/admin/receptii/refacere` and all 10 `/api/migrare/*` routes. **Transition only:** it still accepts the legacy `X-Api-Key` (branch marked `LEGACY-KEY-BRANCH`). KBotAdmin (`LoginApi`), AvacontPush (`AdminApiService`) and `SchemaSyncClient` now send `X-Admin-Token`; KBotAdmin reads `ADMIN_TOKEN` over SSH.
- Deviation from the plan, on purpose: the plan prefers SSH/CLI for administration; users/login-reset must stay HTTP because the lockout counters live in the server's memory, and the migration upload is a long HTTP job. They get a separate server-only credential instead.

### Existing test fixtures touched
`tests/test_auth_login_session.py`, `tests/test_ratelimit.py`: patched `account_state.get_state` (login now reads it).

## Files touched
PYTHON: `routes/mfp.py`, `routes/admin.py`, `routes/migrare/migrare.py`, `routes/admin_cli/admin_cli.py`, `routes/auth/{auth,guard,session_store,account_state,password_policy}.py`, `routes/forexe/{angajamente,angajament_dump}.py`, `utils/{security,dbname}.py`, two test fixtures. SQL: `sql/SEC01_conturi_securitate.sql`. VB: `KBot.Domain/Auth/{PasswordPolicyInfo,UnitsOutcome,PasswordChangeInfo,LoginResult}.vb`, `KBot.Api/{AuthApi,IAuthApi,ApiException,ApiErrorBody}.vb`, `KBot.App/Auth/ForcedPasswordChangeForm*`, `KBot.App/{LoginForm,Program}.vb`, `KBot.App/Setari/SetariInfoView.vb`, `KBot.Xfa/{Config,TemplateDownloader}.vb`, `KBot.Migrator/Schema/SchemaSyncClient.vb`, `KBotAdmin/Server/LoginApi.vb`, `KBotAdmin/Views/LoginResetView.vb`, `AvacontPush/{AdminApiService.vb,Form1.Designer.vb}`. Help: `HelpContent/contabil/autentificare.md` (0000-67).

## Test results
- Builds: `KBot.App`, `KBot.DevHarness`, `KBot.Migrator`, `KBotAdmin`, `KBot.Sync` - 0 warnings, 0 errors.
- Python: `py_compile` on every touched module OK. `test_session_store`, `test_auth_login_session`, `test_redis_session_store`, `test_ratelimit` run once (the first run, before the two fixtures were patched, failed 6 on the new DB read; after the patch `test_auth_login_session` + `test_ratelimit` 15 passed, the other two 28 passed). **No test was written for the new behaviour** (operator rule: no new tests unless asked) and nothing was run against a live server, MariaDB or on screen.

## Operator steps to deploy and test (nothing here was done)
1. Run `sql/SEC01_conturi_securitate.sql`; grant the service account SELECT/INSERT/UPDATE on the table.
2. Add `ADMIN_TOKEN = "<long random>"` to the server's `config.py` (never in git). Restart the server.
3. Make sure `simple_password_check` is installed and its variables are set (comments at the end of the SQL file). Remove `SET GLOBAL strict_password_validation = OFF` from `admin.py` setup_db when that route is retired.
4. Test: flag an account (`admin_cli must-change --un x@y.ro`), log in from the new KBot.App; check the old Access/VBA clients and the old admin tools still work (the legacy key still passes `require_admin`).

## NOT done / deferred
- **Retirement (S2 items 2-6, S3, S4 of the plan)** waits for the operator's tests. When approved: delete the legacy blueprints from `main.py` and their modules (list in `docs/security/legacy-route-inventory.json`; keep `process_excel` and `angajament/dump` but under `require_session`), drop `require_session_or_api_key`, `require_api_key`, the `LEGACY-KEY-BRANCH` in `require_admin`, `config.API_KEY`; rotate the key; nginx 410/404 for old URLs; the negative tests (old key / no credentials / bearer -> 404/410).
- `mfp/get_ef_token`, `tools/*`, `ftp/*`, `forexe/seed`, `admin/{download_*,check_db,setup_db,ping_db}` have no modern consumer found in the repo; they are only removed at retirement (unverified that no external tool calls them).
- Legacy-password compatibility check (parole derivabile din numele bazei) NOT built: the formula is not in the repo; the flag set by `must-change` is the only trigger. Existing accounts must be marked explicitly.
- S3 (VBA): files already in `docs/vba/retirare_legacy`; the Access frontends are not in the repo.
- S4 (the other audit fixes: unit isolation on three FOREXE routes, KB rights, PDF crypto check, proxy headers, regex budget, upload quotas, error detail): the audit itself is not in the repo; not started.
- Mail-code rate limiting is the existing limiter; no separate limiter for the e-mail codes was added.
- `ForcedPasswordChangeForm` never opened in the designer or run; the Designer was written by hand from `ParolaAvansataForm` (144 dpi stamp).
