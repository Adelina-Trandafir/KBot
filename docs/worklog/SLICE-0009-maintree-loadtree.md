# SLICE-0009 — `MainForm.LoadTree` (client half) + tree orphan-escape (0008 amendment)

Spec: the SLICE-0009 brief in chat. Date: 2026-07-17.
Supersedes nothing; amends `SLICE-0008-tree-data-api.md` (Part A below).

## The headline: most of this slice was already shipped by 0008

The 0009 brief was written against a tree state that no longer existed. It asks to add
`GetTreeResponse`/`GetTreeRow` (Part B), `ApiClient.GetTreeAsync` + `IApiClient` (Part C),
and a live `MainForm.LoadTree` with gating and period-driven reload (Part D). **Slice 0008
already built all of that** — see `SLICE-0008-tree-data-api.md` §Client. The mandatory
reads confirmed it before touching anything:

- `UpsertAngajamenteRequest.vb` already carries `GetTreeResponse` / `GetTreeRow`.
- `ApiClient.GetTreeAsync` + the `IApiClient` declaration already exist.
- `MainForm` already has `LoadTreeAsync`, `PopulateTree`, `ApplyViewGating` (all nine
  flags), An/SS re-query, and `btnOpt` → `include_hidden` reload. There is no
  `PopulateAngajamenteList`, no `BuildTreeInfo`, no `#If DEBUG` sample, and the tree does
  **not** use the `Angajament` model / `GetAngajamenteAsync` — the brief's starting state.

So the genuinely-unshipped work was: **Part A (the orphan escape, which 0008 did not have)**
and the **client-side `GetTreeAsync` tests (0008 shipped the method with no dedicated
tests — its count was "Api 26"; this slice brings it to 30)**.

Rather than churn working, committed code to match the brief's exact letter, the two
architectural choices 0008 made — which already satisfy the brief's *goals* — were kept:

1. **Mapping lives in the client, not in a `MainForm.BuildTreeInfo`.**
   `ApiClient.GetTreeAsync` returns `IReadOnlyList(Of AngajamentTreeInfo)`, mapping every
   field (incl. the `AreOrd` JSON → `AreORD` POCO bridge) itself. This keeps the wire DTO
   out of `MainForm` entirely, which is cleaner than the brief's `GetTreeResponse`-in-the-
   form shape. The mapping assertions the brief wanted on `BuildTreeInfo` are covered by
   `GetTree_Deserializes_AllFields_AndNineFlags` instead.
2. **No `token` parameter on `GetTreeAsync`.** The brief signature threads `token` through;
   the codebase reads the bearer from the `SessionContext` singleton per-request (this is
   pinned by the existing `ApiClient_ReadsTokenAtCallTime_NotAtConstruction` H1 regression
   guard). Adding a `token` param would contradict that pattern, so it was not added.
3. **`GetTreeRow.IDDF` is `Long?`, not the brief's `Integer?`.** `AngajamentTreeInfo.IDDF`
   is `Long?`, so `Long?` on the wire DTO means a straight `.IDDF = r.IDDF` with no
   `CLng` conversion — one fewer place to get a cast wrong, and no overflow risk. The
   brief's `Integer?` → `Long?` conversion was mirroring `GetAngajamenteRow.IDDF As
   Integer?`; here the cleaner `Long?`-throughout was kept. `System.Text.Json` deserializes
   the JSON integer straight into `Long?`.

## Part A — server orphan escape (`PYTHON/routes/forexe/tree.py`)

The 0008 `_WHERE` filtered SS with a bare `EXISTS (… i.SS = %s)`. That hides a freshly
created angajament that has **zero** indicators (rows only in `FX_Angajamente`, not yet
downloaded from FOREXE) — it has no SS, so a strict SS `EXISTS` drops it. Legacy kept such
rows via a separate UNION ALL orphan branch in `qFX_MAIN_TREE_DESCRIERE` /
`qFX_MAIN_TREE_DATA`. Mirrored here:

```sql
AND (
    EXISTS (SELECT 1 FROM FX_Indicatori i
            WHERE i.CodAngajament = a.CodAngajament AND i.SS = %s)
    OR NOT EXISTS (SELECT 1 FROM FX_Indicatori i
                   WHERE i.CodAngajament = a.CodAngajament)
)
```

The SS filter now narrows **only** angajamente that actually have indicators; orphans
always pass. Exactly one `%s` in the block (unchanged position), so the bind tuple stays
`(an, ss, include_hidden)` — `cursor.execute(_SQL, (an, ss, include_hidden))` untouched.
`_SQL` still carries exactly 3 `%s` (year, ss, include_hidden); the `%%`-escaped `LIKE`
literals in the Anulat/Suspendat clauses are not placeholders. The rationale is recorded
in the file's `_WHERE` comment.

### Server tests (`PYTHON/tests/test_forexe_tree.py`)

`demo_rows` extended with two codes and a per-code SS map (was: one indicator on `SS` for
every code):

- **`TREEO`** — DataCreare in AN, ASCUNS=0, Stare `În derulare`, **no `FX_Indicatori`
  rows** → orphan.
- **`TREEX`** — same but its only indicator is under `99Z` (a different SS).

New tests:

- `test_orphan_no_indicators_is_shown` — `TREEO` appears (the escape works).
- `test_indicators_all_other_ss_is_hidden` — `TREEX` does **not** appear (the SS filter
  still filters; pinned so nobody later "fixes" the escape into an OR that lets everything
  through).
- `test_null_datacreare_is_always_returned` — re-checked, unaffected: `TREEN` still matches
  (indicator on `SS`, DataCreare NULL); under `an=AN-50` `TREEN` stays, `TREE1` drops
  (year filter), and the two new rows are irrelevant to its assertions.

Off-host these still skip (host-only module, no `config.py`); the module collects clean.

## Part B/C/D — verified present, nothing rebuilt

Confirmed by read that 0008 satisfies the brief's goals (all fields mapped, `Angajament`
model retired from the tree path, load triggered after a period is chosen — `LoadTreeAsync`
runs on `MainForm_Load` and on every An/SS `SelectedIndexChanged`, which is the
`SetPeriod`-precondition wiring the brief asked for). The brief's `bold = Not
IsNullOrEmpty(row.Surse)` is functionally identical to 0008's `node.Bold =
info.AreIndicatori` (a row has indicators iff `Surse` is non-empty — both derive from
`FX_Indicatori`), so it was left as-is.

### Client tests added (`tests/KBot.Api.Tests/ApiClientTests.vb`)

Filling 0008's gap — `GetTreeAsync` had no dedicated tests:

- `GetTree_BuildsUrl_SendsBearer_EscapesSs` — path `/api/forexe/tree`, `an`, `ss` with a
  space → `ss=02%20A` (proves `Uri.EscapeDataString`), `include_hidden=1`, `Bearer` header.
- `GetTree_IncludeHiddenFalse_SetsZero` — `include_hidden=0`.
- `GetTree_Deserializes_AllFields_AndNineFlags` — a full row maps to `AngajamentTreeInfo`
  with `IDDF As Long?`, `DataCreare`/`DataDefinitivare`, `Salarii`/`Ascuns`/`Surse`, and
  all nine flags 1:1 including the `AreOrd` → `AreORD` bridge.
- `GetTree_Non2xx_ThrowsWithStatus` — 500 → `ApiException(500)`.

## Files touched

| File | Change |
|---|---|
| `PYTHON/routes/forexe/tree.py` | `_WHERE` orphan escape + rationale comment |
| `PYTHON/tests/test_forexe_tree.py` | fixture (`TREEO`/`TREEX`, per-code SS) + 2 tests |
| `tests/KBot.Api.Tests/ApiClientTests.vb` | 4 `GetTreeAsync` tests |
| `docs/worklog/SLICE-0009-maintree-loadtree.md` | this worklog |
| `docs/worklog/KBOT_STATUS.md` | 0008 amended, 0009 row, next free → 0010 |

## Test results

- `dotnet test tests/KBot.Api.Tests` — **30 passed, 0 failed** (was 26; +4 new).
- `PYTHON` offline (`.venv`, `pytest -q tests/test_forexe_tree.py`) — **1 skipped**
  (host-only, expected off a Flask host; module collects clean).

## Unverified / deferred

- **Part A has not run against a real database.** Like all of 0008's tree work, the two new
  orphan tests are host-only and skip here. They answer "orphan visible / other-SS hidden"
  when run on the Flask host.
- Everything in 0008's "unverified/deferred" list still stands: no live DB run, missing
  MariaDB DDL for four flag tables, contradictory `FX_Angajamente` DDLs, `SetItemVisible`
  on `KBotNavList`, the nine real views, `btnSort`/`btnIstoric` stubs, the tree still a flat
  list (`ConfigureListMode`), and `GET /api/forexe/angajamente` + `GetAngajamenteAsync`
  remaining tree-unused (kept, no caller removed).
