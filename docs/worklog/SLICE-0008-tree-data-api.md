# SLICE-0008 — Tree data API (`GET /api/forexe/tree`) + `MainForm.LoadTree`

Plan: `PLAN_TreeDataApi.md` (saved to this folder as part of this slice — it existed only
in chat before). Supersedes `docs/HANDOFF_TreeDataApi.md`.
Date: 2026-07-15.

## What changed and why

The tree is what unblocks every MainForm view: until now `MainForm` loaded a flat list
from `GET /api/forexe/angajamente`, which carries no `Are*` flags, so no view could know
whether it had data. This slice adds the real endpoint, the corrected POCO, and the
gating.

### Step 0 — what the mandatory reads showed

The plan said the two source queries were "in this chat's history". They were not (fresh
session), so everything below was read from the **repo** export at
`FX_System_Export/QUERIES/` — *not* from `C:\AVACONT\FX_System_Export`. Nobody has
confirmed the repo copy matches live Access; that remains unverified (see below).

The single most useful finding: **the plan's contract is a composite of two different
Access queries, and reading only one of them makes the plan look wrong.**

| Query | Carries | Lacks |
|---|---|---|
| `qFX_MAIN_TREE` (flags; source of `rcAngInd` in `frmFX_MAIN.RefreshTreeQuery`) | all nine `Are*`, `IDORD` | `Salarii`, `Surse`, `ASCUNS`, **any `WHERE` at all** |
| `qFX_MAIN_TREE_DESCRIERE` (row-source; node population via `mdl_FX_PopulareTree.Angajamente_SQL`) | `CodAngajament, IDDF, DataCreare, DataDefinitivare, Descriere, Stare, Incarcat, Preluat, Salarii, Surse` | the flags |
| `qFX_MAIN_TREE_DATA` ("sort by date" variant) | `Salarii, ASCUNS, Surse`, and the Anulat/Suspendat/Ascuns `WHERE` | `IDDF`, `DataDefinitivare`, the flags |

The plan's column list = `_DESCRIERE` ∪ `ASCUNS`; its flags = `qFX_MAIN_TREE`. Under that
reading the plan's "Corrections" hold, and the handoff is the document that was wrong.

Read-by-read:

- **`Salarii` — the plan is right, restored.** It is a real `FX_Angajamente` column
  (Boolean, `TABLES/FX_Angajamente.md:18`) *and* a real row-source column in `_DESCRIERE`.
  The POCO's old comment called it "deprecated" citing commit `22a2ec4`, but that commit
  removed it from the **angajamente list path** — a different path. Both facts are true at
  once; the list route still does not return it, and this tree route does.
- **`ArePartener` — the plan is right, `FX_DDF.PartAng` used.** `FX_DDF` has both
  `PartAng` (Boolean) and `CodPartener` (Text) (`TABLES/FX_DDF.md`). Access wrote
  `Not IsNull([CODPARTENER])`, which binds to `FX_DDF.CodPartener` (the only table in the
  join with that column). The two are near-equivalent — `frmFX_DDF` enables `CodPartener`
  only when `PartAng` is ticked (`frmFX_DDF.md:453,580`) and validation makes a partener
  mandatory when `PartAng` (`frmFX_DDF.md:701`) — but `PartAng` is the clean yes/no.
- **`db_name` needed no decision.** The session already carries it
  (`session_store.py:59`; `auth.py:359` does `dc = g.session.db_name`), so the route reads
  `g.session.db_name` and takes no base parameter. A token therefore cannot target another
  database — strictly better than the list route, which accepts `db_name` from the query.
- **`AreDDF` needs no join.** `IDDF` is a column on `FX_Angajamente` itself (both DDLs),
  so `a.IDDF IS NOT NULL` is the whole answer, exactly as the plan says.
- **`AngajamentTreeInfo` was well-documented but built on the wrong single query** — it
  had `IDORD` and no `Salarii`/`ASCUNS`/`Surse`. Rewritten.
- **`MainForm`** — `WithReauth(Of T)` preserved and reused; `LoadAngajamenteAsync` was the
  stopgap the tree replaces.

### Three corrections to the plan (recorded so nobody re-derives them)

1. *"Confirm the exact predicate against query 1's `WHERE`"* — **impossible as written.**
   `qFX_MAIN_TREE` has **no `WHERE` clause at all**, and `_DESCRIERE`'s `WHERE` is a unit
   filter (`Nz([FI]![IdUnitate],0)=75`) plus a `DC='000_DEMO'` orphan branch. The
   Anulat/Suspendat/Ascuns exclusion is real but lives in `qFX_MAIN_TREE_DATA.md:7` and
   `mdl_FX_PopulareTree.md:253`:
   `((InStr(1,[Stare],'Anulat')>0) Or (InStr(1,[Stare],'Suspendat')>0) Or [Ascuns]) = False`.
   Implemented from those, as `NOT LIKE '%Anulat%' AND NOT LIKE '%Suspendat%'`
   (`utf8mb4_general_ci` is case-insensitive, so `LIKE` mirrors Access `InStr` with no
   extra functions). Note Access bundles `Ascuns` into the *same* exclusion; the plan
   splits it into the separate `include_hidden` switch, which is what got built.
2. *"`IDDF` is nullable → `LEFT JOIN FX_DDF`"* — **no join was used.** `ArePartener` and
   `AreOrd` are self-contained `EXISTS` on `FX_DDF` instead. A `LEFT JOIN FX_DDF` would
   duplicate the angajament row if two DDF rows ever shared a `CodAngajament` — the plan
   asserts 1-to-at-most-1, but nothing in the schema enforces it, and a `GROUP BY`
   containing `d.IDDF` would not have collapsed the duplicate anyway. With no join,
   "one row per `CodAngajament`" follows from the primary key and needs no `GROUP BY`.
   `IDDF` itself comes from `FX_Angajamente.IDDF`, so the join bought nothing.
3. *"Wire the flags to `.Visible`"* — **`KBotNavList` has no visibility concept**, only
   `SetItemEnabled`. Per operator decision, gating uses `SetItemEnabled`: a disabled item
   is unclickable, unhoverable and skipped by keyboard nav (`KBotNavList.vb:246,270,293`),
   so an empty view is genuinely unreachable — just greyed rather than hidden. Real
   hide/show would need `SetItemVisible` in `KBot.Theming` (deferred).

### Server

`PYTHON/routes/forexe/tree.py` — new `GET /api/forexe/tree`, `@require_session`,
parameters `an`, `ss`, `include_hidden` (no `id_unitate`, no `db_name`). Nine flags as
correlated `EXISTS` per the plan (cost is a non-issue at a few hundred rows); `Surse` via
`GROUP_CONCAT(DISTINCT i.SS SEPARATOR ';')` replacing Access `ConcatRelated`. Fully
parameterized (3 placeholders). Responses go out through a local `_json_utf8` helper
(`ensure_ascii=False`) so `Descriere`/`Stare` diacritics stay literal UTF-8. A DB error
returns a reason-coded 500 body, never an empty tree.

### Client

- `AngajamentTreeInfo` rewritten: plan's columns + nine flags + `Salarii`/`Ascuns`/`Surse`;
  `IDORD` dropped (with it, the `First()`/arbitrary-pick problem disappears); `IdUnitate`
  absent. Provenance comment now names a real source per field.
- `IApiClient.GetTreeAsync` + `ApiClient.GetTreeAsync` + `GetTreeResponse`/`GetTreeRow`
  wire DTOs, mirroring `GetAngajamenteAsync` (hard-fail on non-2xx; 401 flows to
  `WithReauth`, no retry here).
- `MainForm`: `LoadTreeAsync` replaces `LoadAngajamenteAsync` on the load path;
  `PopulateTree` binds rows; `ApplyViewGating` maps each flag to its nav item; An/SS
  changes re-query; `btnOpt` toggles `include_hidden` and reloads. Four nav items added
  (`indicatori`, `istoric`, `revizii`, `partener`) as `PlaceholderView`s so all nine flags
  gate something — previously only five of nine had anywhere to land. `sumar` has no flag
  and stays always enabled; if the active view closes, selection falls back to `sumar`.

## Files touched

| File | Change |
|---|---|
| `PYTHON/routes/forexe/tree.py` | new — the endpoint |
| `PYTHON/routes/forexe/__init__.py` | register `tree` on the blueprint |
| `PYTHON/tests/test_forexe_tree.py` | new — 12 host-only tests |
| `src/KBot.Domain/AngajamentTreeInfo.vb` | rewritten to the settled shape |
| `src/KBot.Api/IApiClient.vb` | `GetTreeAsync` |
| `src/KBot.Api/ApiClient.vb` | `GetTreeAsync` implementation |
| `src/KBot.Api/UpsertAngajamenteRequest.vb` | `GetTreeResponse` / `GetTreeRow` |
| `src/KBot.App/MainForm.vb` | tree load, gating, An/SS reload, `btnOpt` |
| `docs/worklog/PLAN_TreeDataApi.md` | saved (existed only in chat) |
| `docs/worklog/KBOT_STATUS.md` | slices 0006/0007/0008 reconciled |
| `docs/HANDOFF_TreeDataApi.md` | marked superseded |
| `docs/KBOT_Tier1_Plan.md`, `docs/PLAN_MainForm_Scaffolding.md` | reduced to pointers (drifted duplicates) |
| `CLAUDE.md` | slice status refreshed |

## Test results

- `dotnet build KBot.sln` — **succeeded, 0 warnings, 0 errors**, `Option Strict On`.
- `dotnet test KBot.sln` — **80 passed, 0 failed** (Api 26, App 18, Theming 27, Common 7,
  Domain 1, LocalStore 1) — baseline held.
- `PYTHON` offline (`.venv`, `python -m pytest -q`) — **75 passed, 8 skipped, 0 fail/error**
  (was 75/7: the new host-only module is the extra skip, which is correct off-host).
- Route registration verified explicitly with a stubbed `config`: `GET /api/forexe/tree`
  is on the URL map alongside the two existing forexe rules; `_SQL` carries exactly 3
  `%s` placeholders and keeps its `%%`-escaped `LIKE` literals.

## Anything left unverified or deferred

**Unverified (no access from a dev station — needs a host run):**

1. **Nothing in this slice has touched a real database.** All 12 tree tests are host-only
   and skipped here. Verification checklist items 1–5 and 7–8 are therefore **unverified**;
   `tests/test_forexe_tree.py` is written to answer 1–5 and 8 when run on the host.
2. **The four flag tables were confirmed by the operator, not by me.** `FX_DDF`, `FX_ORD`,
   `FX_DDF_REV_SA`, `FX_Receptii_H` have **no MariaDB DDL anywhere in the repo** —
   `DDL_FX_ListaAngajamente.sql` creates only `FX_Angajamente`, `FX_Indicatori`,
   `FX_Istoric`, `FX_Rezervari`, `FX_Plati`. The operator states all four exist live; the
   endpoint hard-fails if any does not. Worth adding their DDL to the repo.
3. **`ASCUNS` likewise** — operator-confirmed. The repo contradicts itself:
   `DDL_FX_ListaAngajamente.sql:42` has the column, `docs/FX_Angajamente.sql` does not.
   (The shipped `GET /angajamente` already selects `FA.ASCUNS`, which corroborates it.)
4. **The Access export is the repo copy**, not `C:\AVACONT\FX_System_Export`, and nobody
   has confirmed it matches the live Access file.
5. **`FX_DDF.SS` exists but is unused here.** The SS filter goes through `FX_Indicatori.SS`
   per the plan; whether a DDF on another SS should affect the tree was not asked.

**Deferred:**

- `SetItemVisible` on `KBotNavList` (real hide/show instead of grey-out).
- The nine real views — all still `PlaceholderView`.
- `btnSort` tree sorting and `btnIstoric` remain placeholder `MsgBox` stubs.
- `GET /api/forexe/angajamente` and `GetAngajamenteAsync` are now **unused by MainForm**
  but kept (route, client method and their tests) — no caller was removed. Retire in a
  later slice if nothing else picks them up.
- `AdvancedTreeControl` / ComboBox theming retrofit (pre-existing).
- The tree is still a flat list (`ConfigureListMode`), not a nested tree; `ParentKey` /
  `TipNod` / `CodIndicator` / `CodAi` / `IdPartener` stay unset — Access set those from the
  node level in `mcTree_Click`, and nesting was not in this slice's contract.
