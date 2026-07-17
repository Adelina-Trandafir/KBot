# K-BOT — STATUS (single source of truth)

This file is the one place that says what is done, what is in progress, and what is next.
Three people rely on it: the developer, Claude (chat), and Code (Claude Code / Codex).
Keep it current. If reality and this file disagree, fix this file in the same commit.

**Rule:** every task also produces a worklog in `docs/worklog/` (see
`docs/worklog/CODE_WORKFLOW.md`). This STATUS file is the summary; the worklog is the detail.

---

## Slice registry

Numbers are permanent once assigned. A slice done in several passes keeps one number and
gets sub-numbered worklogs (`SLICE-0007-01-…`, `SLICE-0007-02-…`). The **next free slice
number** is recorded at the bottom of this section — bump it when you assign a new one.

> The numbers below are a starting proposal. Confirm or renumber them once, then treat
> them as fixed. Felia 1 (auth) is the only one anchored by history; the rest are assigned
> here for the first time.

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0001 | Auth — bearer tokens, session store (Felia 1) | DONE | (pre-worklog-rule) | Static API key eliminated for K-BOT; legacy FOREXE still uses X-Api-Key |
| 0002 | Split-brain 401 fix + reason codes | DONE | (pre-worklog-rule) | login mints via STORE; every 401 carries a reason code |
| 0003 | Redis session backend | DONE (code) / config PENDING on VPS | — | `SESSION_BACKEND="redis"`, `SESSION_KEY_PREFIX`, `REDIS_DB=2` must be set in host config.py; verify DB 2 is free |
| 0004 | Tier 1 hardening | IN PROGRESS | — | rate limiter DONE+pushed; remaining: ApiOptions address, retire AppConfig, verify gunicorn guard. Plan: `KBOT_Tier1_Plan.md` |
| 0005 | Phase A cleanup (lying tests, https guard) | DONE | — | Python 75 passed / 7 skipped, 0 fail/error; .NET 80 green |
| 0006 | MainForm scaffolding | DONE | (pre-worklog-rule) | Plan: `PLAN_MainForm_Scaffolding.md`. Built against its own 11-item checklist; `WithReauth(Of T)` + ListaAngajamente vertical preserved. Real DI signature is 5 params (`forexeRunner, session, apiClient, authApi, loginFactory`), not the 4 the plan expected |
| 0007 | AngajamentTreeInfo POCO correction | SUPERSEDED by 0008 | `SLICE-0008-tree-data-api.md` | Was done WRONG: built against `qFX_MAIN_TREE` alone, so `Salarii` was dropped and `IDORD` kept. 0008 rewrote the POCO against the real contract (row-source `_DESCRIERE` + flags `qFX_MAIN_TREE`): `Salarii` restored, `IDORD` dropped |
| 0008 | Tree data API + `MainForm.LoadTree` | DONE (code) / UNVERIFIED on a live DB | `SLICE-0008-tree-data-api.md`, `SLICE-0009-maintree-loadtree.md` (Part A amendment) | Plan: `PLAN_TreeDataApi.md`. `GET /api/forexe/tree` (an/ss/include_hidden, base from session), nine `EXISTS` flags, POCO rewrite, tree load + nav gating. **Amended by 0009:** the SS filter now has an orphan escape (`EXISTS SS OR NOT EXISTS any indicators`) so zero-indicator angajamente stay visible. **No part of it has touched a real database** — all route tests are host-only and skip off-host |
| 0009 | `MainForm.LoadTree` (client half) + tree orphan escape | DONE (code) / UNVERIFIED on a live DB | `SLICE-0009-maintree-loadtree.md` | The brief's Parts B/C/D (DTOs, `GetTreeAsync`, `LoadTreeAsync` + gating) were **already shipped by 0008**; the real deltas are Part A (orphan escape on the server, see 0008 row) + its 2 host-only tests, and the 4 `GetTreeAsync` client tests 0008 never added (Api 26 → 30). Kept 0008's choices: mapping in the client (no `BuildTreeInfo`), token from session (no param), `IDDF As Long?` throughout. LoadTree is period-driven (runs on load + every An/SS change = the `SetPeriod` precondition) |
| 0010 | `KBotDataView` — owner-drawn unbound grid (Access continuous-form) | IN PROGRESS — pass 01 DONE | `SLICE-0010-01-kbotdataview-skeleton.md` | Plan: pasted in-session. Multi-pass. **0010-01 (skeleton):** column/row models + enum, `Control`-based double-buffered control implementing `IThemedControl`, theming cache + `ApplyTheme`, header + empty themed body painted, 4 child controls (2 editors + 2 scrollbars) declared in Designer. New `KBot.Controls.Tests` (17). **Decision:** kept in `KBot.Controls` but added a `KBot.Theming` ProjectReference (no cycle) so it self-themes via `IThemedControl`/`ThemePalette` — the plan's "`KBotTheme` constants" don't exist (that's a ~9-slot Forexe façade). Remaining passes: 02 render+virtualize, 03 column types, 04 formatting+disable, 05 input+selection, 06 editing. Nothing painted has been visually verified yet (owner-drawn; verified in the 0010-02 harness) |

**Next free slice number: 0011.**

---

## Current focus

- **Now:** run Slices 0008 + 0009 on the host. The endpoint (incl. 0009's orphan escape)
  and the client are written and green offline, but nothing has hit a real database:
  `PYTHON/tests/test_forexe_tree.py` skips off-host and is the fastest way to answer
  verification items 1–5 and 8, plus the two new orphan tests (`TREEO`/`TREEX`).
- **Next:** the real views (all nine are still `PlaceholderView`), starting with whichever
  the operator needs first; Slice 0004's remaining Tier 1 items and the Slice 0003 VPS
  config are still open and short.
- **In parallel (Slice 0010):** `KBotDataView` owner-drawn grid — pass **0010-01 (skeleton)**
  landed (models + control base + theming + header/empty body, all offline-green). Next up:
  **0010-02** (row virtualization + Text/CheckBox painting + scrollbars) with a synthetic
  5,000×20 harness to prove scroll perf. Feeds the read-only Sumar view.

## Open threads (not yet scheduled)

- `_upload_sessions` (routes/ftp.py) is still an in-process dict → blocks multi-worker.
- The rate limiter's counters are also in-process → restart clears lockouts; also blocks
  multi-worker. Both must move to Redis before `workers > 1` is possible.
- **No MariaDB DDL for `FX_DDF`, `FX_ORD`, `FX_DDF_REV_SA`, `FX_Receptii_H`** — five of
  the nine tree flags read them and `DDL_FX_ListaAngajamente.sql` creates none of them.
  The operator confirms all four exist live (2026-07-15); the endpoint hard-fails if one
  does not. Add their DDL so the repo stops disagreeing with production.
- **The two FX_Angajamente DDLs contradict each other**: `DDL_FX_ListaAngajamente.sql:42`
  has `ASCUNS` and `IdUnitate`, `docs/FX_Angajamente.sql` has neither. Operator confirms
  `ASCUNS` exists live. Pick one file as canonical and delete the other.
- The Access export used for the tree contract is the **repo copy**
  (`FX_System_Export/QUERIES/`); nobody has confirmed it matches the live Access file, or
  `C:\AVACONT\FX_System_Export`.
- `KBotNavList` has no `SetItemVisible` — 0008 gates views with `SetItemEnabled`
  (grey-out) instead of hiding. Add real visibility if the greyed items bother operators.
- `GET /api/forexe/angajamente` + `IApiClient.GetAngajamenteAsync` have **no caller left**
  after 0008 (the tree replaced them on MainForm's load path). Retire or find them a use.
- Server housekeeping: kernel reboot, systemd hardening, de-root the service user (blocked
  by AvacontPush SSH dependency), close stray ufw port 5010.
- Naming ambiguity: `Unitati` exists in both `AVACONT_COMUN` and each per-unit DB with
  different columns.

## Locked decisions (do not relitigate without a note here)

- Single-worker Gunicorn stays; `on_starting` guard refuses `workers > 1`.
- Under Redis: TTL counts down to the 30-min absolute cap (no slide); the 20-min idle
  window is enforced in-app from `expires_at`; operator password is never written to Redis;
  keys namespaced `kbot:sess:` in K-BOT's own DB; never FLUSHDB/FLUSHALL (shared Redis).
- Static API key kept ONLY for the legacy FOREXE fleet on shared routes; eliminated for
  K-BOT.

---

## How to update this file

- When a slice changes state, edit its row and (if needed) the Current focus section.
- When you assign a new slice, add a row and bump "Next free slice number".
- Do it in the same commit as the work, alongside the worklog. Never let this drift.
