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
| 0010 | `KBotDataView` — owner-drawn unbound grid (Access continuous-form) | DONE (code) / **NO VISUAL VERDICT YET** | `SLICE-0010-01-…skeleton.md`, `-02-…virtualizare.md`, `-03-…tipuri-coloana.md`, `-04-…formatare-disable.md`, `-05-…input-selectie.md`, `-06-…editare.md` | Plan: pasted in-session (continuation plan supersedes the original where they disagree). Multi-pass. **0010-01 (skeleton):** models + enum, double-buffered `Control` implementing `IThemedControl`, theming cache + `ApplyTheme`, header + empty body, 4 child controls in Designer. **0010-02 (render+virtualize):** split into partials (`.Theming`/`.Layout`/`.Painting`), frozen + scrolling column bands, integer virtualization math, two-pass scrollbar sizing, Text + CheckBox cell painting, `CellFormatting`/`RowFormatting` plumbing with **reused** args, full palette→role mapping. **Decision:** kept in `KBot.Controls` + a `KBot.Theming` ProjectReference (no cycle) so it self-themes — the plan's "`KBotTheme` constants" don't exist (that's a ~9-slot Forexe façade). **0010-03 (column types):** Combo / OptionButton / Button / ProgressBar painting + `OptionGroup` exclusivity via `SetOptionValue`. **0010-04 (formatting+disable):** three-level effective-enabled (`IsCellEnabled`/`IsRowEnabled`, separate "probe" args so queries can't clobber an in-flight paint), disabled rendering across all six types, conditional formatting. **0010-05 (input+selection):** current cell + `SelectionChanged`, Access-style keyboard nav, click/double-click, toggle + button activation gated on `IsCellEnabled`, header-edge column resize. **0010-06 (editing):** floating Text/Combo editor, `CellValidating` (veto **and** value coercion), Esc discard, auto-commit on move/scroll, and the corrected `IsDirty` contract — API writes are *loading*, only operator edits/toggles dirty a row (`ClearDirty` added; 2 pass-01 tests deliberately rewritten). Virtualization proven **headlessly** (same painted-row count at 5,000 and 50,000 rows). 158 tests green. **Editing caught 2 real VB case-insensitivity bugs** where a parameter shadowed a same-named property and an unqualified assignment was a silent no-op: `ProposedValue` (commit always wrote Nothing) and — live since 0010-01 — `HeaderText`, meaning **every column header was Nothing**; both fixed with `Me.` + regression tests. ⚠️ **NOBODY HAS RUN THE VISUAL HARNESS for any of the six passes.** Scroll smoothness, the WinForms key→handler path, resize drag, floating-editor placement/focus and actual colours are all unverified — the blank-header bug shows exactly why that matters. Run: DevHarness → Controls/UI → «KBotDataView — virtualizare + temă (5.000 × 20)». Next: Sumar slice consumes it read-only |

| 0011 | Sumar — endpoint + `SumarView` (prima vedere reală) | DONE (code) / UNVERIFIED on a live DB / **NO VISUAL VERDICT** | `SLICE-0011-01-sumar-endpoint.md`, `SLICE-0011-02-sumar-view.md` | Plan: pasted in-session. Port of `qFX_MAIN_SUMAR` v1 → `GET /api/forexe/sumar?cod=…` (header hoisted once + one row per indicator), plus `SumarView` replacing `PlaceholderView` for the `sumar` key — **the first of the nine views that is real**, and the first consumer of `KBotDataView` (read-only). **The `Clsf` blocker was resolved by the operator supplying the real DDL, not by guessing:** `Clasificatii.Clsf` EXISTS as a STORED generated column `concat_ws('.',Capitol,Subcapitol,Articol,Alineat)` (indexed), so Branch A — select it directly. `Titlu = left(Articol,2)` explains Access's `Mid(Clsf,13,2)`. Port decisions: no SS filter, `ClasificatiiG`→`Clasificatii` / `ParteneriG`→`Parteneri`, all `IdUnitate` join predicates dropped, **`LEFT JOIN Clasificatii`** (was INNER — an indicator with no classification must still appear), `TotalReceptii = SUM(DIF)` not `Valoare`, `'Angajament nou.'` with the trailing period. Deliberate deviations: `COALESCE(...,0)` on the five totals, `cod` pushed into every aggregate (Access full-scanned per aggregate), deterministic `ORDER BY` added, `ROUND(,2)` kept on revizii/ordonanțări only. Client: `WithReauth` passed in specialized on `SumarInfo` so the 401 policy stays in the shell; snake_case stops at the wire DTOs; **stale-response guard** discards a superseded `cod`. 164 tests green, 0 warnings. ⚠️ **Two open risks:** (a) the join key `I.IdClsf = C.IDClsf` is a documented-convention DECISION, not a verified fact — MariaDB inverts Access's naming (`IDClsf`=PY id, `IdClsfAcc`=Access id); if `clsf` comes back blank on ALL rows live, the key is `C.IdClsfAcc`; (b) nobody has run `SumarView` on screen, and `KBotDataView`'s visual harness is STILL unrun |

**Next free slice number: 0012.**

---

## Current focus

- **Now:** run Slices 0008 + 0009 on the host. The endpoint (incl. 0009's orphan escape)
  and the client are written and green offline, but nothing has hit a real database:
  `PYTHON/tests/test_forexe_tree.py` skips off-host and is the fastest way to answer
  verification items 1–5 and 8, plus the two new orphan tests (`TREEO`/`TREEX`).
- **Next:** the remaining eight views are still `PlaceholderView` (Sumar landed in 0011);
  Slice 0004's remaining Tier 1 items and the Slice 0003 VPS config are still open and short.
- **Slice 0011 (Sumar) — both halves landed, 164 tests green, 0 warnings.** Two things
  need a human: (1) run `test_forexe_sumar.py` on the host — it answers in one go
  whether the eleven touched tables exist AND whether the `Clasificatii` join key is
  right (if `clsf` is blank on every row, switch `C.IDClsf` → `C.IdClsfAcc` in
  `sumar.py`); (2) look at `SumarView` on screen — it has never been rendered.
- **Slice 0010 (`KBotDataView`) — all six passes landed, 158 tests green, 0 warnings.**
  **The one open item is a human one:** nobody has run the visual harness yet. Please run
  DevHarness (Debug start → «Nu») → Controls/UI → «KBotDataView — virtualizare + temă
  (5.000 × 20)» and give it a Pass/Fail. That probe is the only way to confirm scroll
  smoothness, keyboard/resize interaction, floating-editor placement and the actual colours —
  and pass 06 found a bug (all column headers were `Nothing` since 0010-01) that no headless
  test could have caught. After that: the **Sumar** slice, which consumes the grid read-only.

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
- **`Clasificatii` DDL is not in the repo.** Slice 0011 was blocked on it until the
  operator pasted it by hand. It documents real contracts the code now depends on
  (`Clsf`/`Titlu`/`SS` as STORED generated columns; FKs into `AVACONT_COMUN.Defa*`).
  Add it to the repo so the next slice does not have to ask again.
- **MariaDB inverts Access's classification-id naming** (`Clasificatii.IDClsf` = the
  "PY" id / PK, `IdClsfAcc` = the Access id), while the Access modules push
  `IdClsf = IdClsfPY`. Nothing in the repo confirms which of the two
  `FX_Indicatori.IdClsf` actually holds. Worth settling once and writing down — every
  future FX_ view that joins to `Clasificatii` hits the same fork.

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
