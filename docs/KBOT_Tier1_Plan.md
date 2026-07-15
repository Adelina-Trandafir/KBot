# KBOT_Tier1_Plan.md — server-address & config hardening

> Rebuilt from the original design (the working file was lost from the tree). Content is
> reconstructed from the session where this plan was first written; treat every file path
> and field name as **to be confirmed by reading the real file**, not as verified fact.

## Status of the four Tier 1 items

1. **Brute-force rate limiter** on `/api/auth/units` + `/api/auth/login` — **DONE and
   pushed** (16/07/2026). `ratelimit.py::LIMITER` is wired in both handlers. Not part of
   this rebuild except to confirm it stays wired.
2. **gunicorn single-worker guard** — the `on_starting` hook already exists and refuses
   `workers > 1`. **Verify only**, do not rewrite.
3. **Server address consolidated to one place** (`ApiOptions`, HTTPS-only) — status
   unclear; the LoginForm work may already have hardcoded it. **Confirm, then finish.**
4. **`AppConfig` retired** — depends on #3. **Confirm, then finish.**

So the real remaining work is #3 and #4, plus a verification pass on #1 and #2.

---

## Phase 0 — read before touching (mandatory)

Open and read in full, do not edit yet:

- `src/KBot.Api/ApiOptions.vb` — is `DefaultBaseUrl` already the hardcoded
  `https://kbot.avatarsoft.ro`? Is there still a `KBOT_API_BASE_URL` environment override?
- `src/KBot.Api/ApiClient.vb`, `src/KBot.Api/IApiClient.vb` — how the base URL is read.
- `src/KBot.Common/AppConfig.vb` (or wherever `AppConfig` lives) — what is still on it
  after `ApiKey` was removed. If it is already empty, retiring it is just deleting the
  file + its call sites.
- Every caller of `AppConfig` (grep the solution) — so retiring it doesn't orphan a
  reference.
- `src/KBot.Forexe/` download / executor / `ForexeRunner` — confirm whether the
  `parseExcel` HTTP call was already routed through `ApiClient` (the `ExcelJob` bridge) or
  is still making its own request. If already done, skip that part.
- `gunicorn.conf.py` — confirm the `on_starting` guard refuses `workers > 1`.
- `routes/auth/auth.py` — confirm `LIMITER.is_blocked / record_failure / record_success`
  are still wired in both pre-auth endpoints (nothing should have removed them).

Report what each read showed **before** editing. Where an item turns out already done,
mark it done and move on — do not redo it.

---

## Phase 1 — server address written once, HTTPS-only (item #3)

The goal: the production address exists in exactly one place and can never silently become
plain `http`.

- `ApiOptions.DefaultBaseUrl = "https://kbot.avatarsoft.ro"` as a constant. The hostname
  is public and non-secret, so hardcoding is correct — this is not a credential.
- Remove the `KBOT_API_BASE_URL` environment override entirely, if it still exists.
- Add a startup check that throws if the configured address does not start with `https`.
  A plain-`http` address must stop the app at startup with a clear message, not run.
- If `DownloadAction.ApiUrl` still exists in the FOREXE action model, delete it — the XML
  parser never read it, so nothing depends on it (confirm that in Phase 0 before deleting).

Do **not** change the address itself. Production is the target.

## Phase 2 — route the FOREXE Excel call through ApiClient (item #3, if not already done)

Only if Phase 0 showed it isn't done yet:

- The FOREXE download code stops making its own web request. It fills an `ExcelJob` DTO
  (in `KBot.Common`, the shared contract) and hands it to a delegate function; `ApiClient`
  performs the actual POST (address, bearer token, reading the reply) over the shared HTTPS
  client. This closes the last plaintext hole.
- This must add **no new project reference**: the little bridge delegate lives in
  `KBot.App`, the DTO in `KBot.Common`. `KBot.Forexe` must still **not** reference
  `KBot.Api` after the edit — confirm that explicitly.
- Once this lands, the token-provider plumbing on the executor becomes dead; remove it if
  nothing else calls it (grep first).

## Phase 3 — retire AppConfig (item #4)

- If `AppConfig` is empty after `ApiKey`/base-URL removal, delete the file and every call
  site. If something still lives on it, move that to its natural home first, then delete.
- Build must be clean afterward with no dangling references.

## Phase 4 — verify #1 and #2 (no rewrite)

- `gunicorn.conf.py`: the `on_starting` hook refuses to launch with more than one worker.
  Confirm the guard is intact and leave it. This protects both the in-memory session
  store path AND `_upload_sessions` (still in-process) from silent fragmentation.
- `ratelimit.py`: `LIMITER` still called in both pre-auth endpoints. Confirm, don't touch.

Note for the record (do not fix here): the rate limiter's counters are an in-process dict,
like `_upload_sessions`. So a service restart clears all lockouts, and it is a second
thing (after `_upload_sessions`) that blocks going multi-worker even now that the session
store can be on Redis. Record it; it is not Tier 1 work.

---

## Phase 5 — tests

- Build the solution clean, `Option Strict On`, zero warnings.
- .NET tests all green (current baseline: `KBot.Api.Tests` = 16). Add a test that the
  startup HTTPS check throws on a plain-`http` address.
- Python offline suite green or cleanly skipped, zero fail/error.
- Test output to `AppDir\Logs\test_*.log`; the solution must be published before .NET
  tests run.

## Original design notes carried forward (may be guesses — confirm)

- The brute-force numbers already in `ratelimit.py` (5 failures per user, 30 per IP,
  15-minute lockout, 15-minute window) were chosen defaults for offices behind shared NAT,
  living in constants at the top of the file so they can change without hunting through
  code. Leave them unless the operator asks otherwise.

## Standing rules

- Read the real file before editing it. Never edit a file not seen verbatim this session.
- No swallowed exceptions anywhere — every `catch` / `except` surfaces or rethrows.
- VB.NET: `Option Strict On`, no `Namespace` blocks, all controls in `*.Designer.vb`,
  colours only via `KBotTheme`.
- Code and comments in English; operator-facing messages in Romanian with literal
  diacritics (ă â î ș ț), never `\uXXXX`.
- Commit each self-contained change on its own.
- Never invent a fact. Mark verified vs. assumed. Ask only below 75% confidence.
