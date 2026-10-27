# SLICELESS - Security plan: new system next to the legacy one (11.10.2026)

Plan: `docs/PLAN_Securitate_API.md`. Operator decision in chat: build everything, **delete nothing legacy until the operator has tested both systems**. Help recorded as 0000-67.

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

## UPDATE 11.10.2026 (later) - RETIREMENT DONE IN CODE (not yet deployed)

Operator decision: from the Access side only the sync of classifications and partners stays, under its own key (`X-Access-Key` / `ACCESS_SYNC_KEY`); nothing FOREXE-related.

- `utils/security.py` rewritten: `require_api_key` and `config.API_KEY` are gone. Two guards left: `require_admin` (`X-Admin-Token` / `ADMIN_TOKEN`, no more `LEGACY-KEY-BRANCH`) and `require_access_sync` (`X-Access-Key` / `ACCESS_SYNC_KEY`). Constant-time compare; an unset secret refuses everything.
- `routes/auth/guard.py`: `require_session_or_api_key` deleted. `process_excel` and `forexe/angajament/dump` are `require_session` only.
- `main.py` no longer registers `nomenclatoare`, `salarii`, `wfls`, `ftp`, `ddf`, `ord`, `forexe.seed`; those modules were deleted from disk (they stay in git history).
- Trimmed: `admin.py` keeps only `users`, `login/reset`, `receptii/refacere` (so `setup_db`, which switched `strict_password_validation` OFF, is gone); `tools.py` keeps `process_excel`; `mfp.py` keeps `templates/ddf|ord`; `parteneri.py` keeps `syncdb`, `upsert`; `clasificatii.py` keeps `insert`, `clasificatii_complete_upsert`, `sync_acc_kbot` (those three + two partner routes = the Access sync, under `X-Access-Key`). Helper functions that only the removed routes used were left in place (dead code, not removed).
- `utils/logger.py`: the "old Access client" log tag also recognises `X-Access-Key`.
- Tests: new `tests/test_legacy_retired.py` (45 cases: every retired path 404 with and without the old key; Access routes refuse no key / old key / admin token / wrong key; admin routes refuse old key and Access key; unset secrets refuse everything; modern routes take the bearer only). Removed `test_ddf_global_id.py`, `test_forexe_seed_columns.py` (modules deleted) and the transition-guard tests of `test_session_store.py`; `test_admin_receptii_refacere.py` now uses the admin token. Ran: those + auth/session/ratelimit/clasificatii files, 113 passed. The rest of the suite was NOT run.
- `docs/vba/RETIRARE_API_LEGACY.md`: the Access exception, the five routes and what to change in the VBA.

### Deployment (operator, in this order)
1. Add to the server's `config.py`: `ACCESS_SYNC_KEY = "<new long random value>"` (NOT the old key). `ADMIN_TOKEN` is already there. Delete the line `API_KEY = ...` there and in any other copy of the configuration.
2. Upload `PYTHON/` (changed files, plus delete on the server: `routes/ddf/`, `routes/ord/`, `routes/nomenclatoare.py`, `routes/salarii.py`, `routes/wfls.py`, `routes/ftp.py`, `routes/forexe/seed.py`) and restart.
3. Access: change the header in the five sync functions; remove from Access the functions of the retired routes.
4. Check: an old VBA/FOREXE client must now get 404 on retired routes and 401 on the sync routes; K-BOT, KBotAdmin and the migrator keep working.
5. nginx: if there are other vhosts/ports to the same app, check them too; optional 410 for the old URLs.

### Still NOT done
S4 (the other audit fixes, the audit is not in the repo). The old key must be rotated/removed by the operator on the server. Nothing deployed or run against a live server.

## UPDATE 11.10.2026 (limits on the Access sync routes)
- `utils/access_limits.py` (`access_sync_limits(kind)`, applied under `require_access_sync` on the 5 sync routes): body at most 1 MB; at most **200 rows** per request (`data` / `clasificatii` + nested `rectificari`), over that refused whole with 413; `db_name` must be a registered unit (`AVACONT_COMUN.Unitati.DC`, else 403 - the key can no longer reach AVACONT_COMUN or any non-unit schema); **one successful call per route, unit and day** (429 the second time; a call that ends >= 400 gives the slot back); if the limits cannot be checked the route answers 503. State: `AVACONT_COMUN.Access_Sync_Limite` (`sql/SEC02_access_sync_limite.sql`, **run it BEFORE uploading**; the service account needs SELECT, INSERT, DELETE).
- Day = Europe/Bucharest calendar day (UTC if the tz database is missing on the server). The unit check reads the K-BOT server's `Unitati`; a unit that exists only on the legacy server is refused (403).
- Tests: `tests/test_access_limits.py` (8) + `test_legacy_retired.py` (45) = 53 passed offline, no DB touched. Not run against a live server.
- Access side: `docs/vba/COD_NOU_SINCRONIZARE_ACCESS.txt` (text to paste into the operator's own module; NOT a VBA module): header `X-Access-Key`, shared `KbotAccessPost` that shows the server's message (413/429/401), the 200-row check before sending, the five functions. The UPSERT functions that send a whole table stop with a message above 200 rows; narrowing them to modified rows (`Esinc = False`) is left to the operator, since it depends on `Esinc` being reset on every edit in Access.
- To let a unit sync again the same day: `DELETE FROM Access_Sync_Limite WHERE DC = '<unit>' AND Zi = CURDATE();`

## UPDATE 11.10.2026 (rectificari in the upsert)
- Operator: the classification upsert must also carry the rectifications. `routes/clasificatii.py` `clasificatii_complete_upsert`: each item may now carry `rectificari` [{Data yyyy-mm-dd, Document, Trim1..4}]; they are upserted into `Clasificatii_Rectificari` (unique IdClsf + Data + Document) in the SAME transaction as the classification (any invalid one rolls everything back, 400); the answer has a `rectificari` count. The standalone `rectificari` / `rectificari_upsert` routes stay retired.
- Access text (`docs/vba/COD_NOU_SINCRONIZARE_ACCESS.txt`): `PY_UPSERT_Clasificatii` reads the year's `Rectificari` of each classification and sends them; the 200-row cap counts classifications + rectifications; the `Stop` became a message + continue.
- Not run: no test for the new server branch (needs a database); `py_compile` only. Nothing deployed.
- (same day) `/api/clasificatii/insert` also takes `rectificari` per item. Both routes now share `_save_rectificari` (validation + upsert on the caller's cursor, same transaction); the helper was exercised by hand with a fake cursor (good entry, no list, bad date, missing document, non-list, non-object). `PY_INS_Clasificatii` in the Access text sends the year's rectifications and counts them toward the 200 rows. Not run against a database.
