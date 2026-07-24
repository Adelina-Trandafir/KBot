# SLICE-0022-01 — server + client: `GET /api/forexe/istoric`

Pass 1 of slice 0022 (vederea Istoric), per `PLAN_IstoricView.md` §2–§3. Ships the endpoint,
its host-only tests, the `KBot.Domain` POCOs and `GetIstoricAsync` on the API client. **No UI in
this pass** — `MainForm.CreateView("istoric")` still returned a `PlaceholderView` when this pass
was written; the view lands in 0022-02.

> **Worklog naming.** `CODE_WORKFLOW.md` §3.2 requires multi-pass slices to sub-number their
> worklogs (`SLICE-0007-01-…`), which is also what 0015/0017/0020 did. Followed the standing rule.

## What changed and why

One endpoint, two arrays, one round trip: `{cod, randuri[], clasificatii[]}`. The client
(`IstoricView`) shapes the grid, the three column menus and the filtering **locally** — a filter
change never issues a request. Decision inherited from 0020-01 §7; not re-litigated here.

- **`randuri`** — `SELECT … FROM FX_Istoric WHERE CodAngajament = %s ORDER BY DataFX, Clsf`.
  The `ORDER BY` is the Access `RecordSource`'s, verbatim. Nineteen wire fields (§2.1). `cod` is
  an **exact** match, parametrized (Access used `Like` — deviation, see §9 in 0022-02).
- **`data_fx` keeps its time component** (§2.3). `FX_Istoric.DataFX` is a `datetime` and istoric
  rows are timestamped events. A dedicated `_iso_dt` returns the full ISO value **with** the time
  — deliberately NOT the `_iso` from `ddf.py`, which truncates to the day (right there, wrong
  here). Pinned by `test_data_fx_carries_time_component`.
- **`Clsf` needs no join** (§2.4). `FX_Istoric.Clsf` is a stored TEXT column, written at parse
  time by `FX_Istoric_Prelucreaza_Observatii` from `FX_Indicatori`. Read it directly. The view is
  therefore **not** exposed to the `Clasificatii` duplicate problem for display — only the filter
  hierarchy touches the nomenclator.
- **The classification key points the opposite way from DDF** (§2.5). `FX_Istoric_Prelucreaza_`
  `Observatii` writes `Rs!IdClsf = rcInd!IdClsf` straight from `FX_Indicatori`, which holds the
  **Access** id. So `FX_Istoric.IdClsf` matches `Clasificatii.IdClsfAcc` (NOT `IDClsf`, as DDF's
  `FX_DDF_REV_SA` does), and needs the `IdUnitate` predicate — which `FX_Istoric` has no column
  for, so it comes through `FX_Indicatori` on `CodAngajament`, exactly as the Access filter
  (`bFilter_Click`) did. Getting this backwards yields an **empty filter menu, not an error** —
  pinned by `test_key_resolves_via_idclsfacc_not_idclsf` (a row where `IDClsf` ≠ `IdClsfAcc`).
- **`clasificatii` is deduped on `IdClsfAcc`** (§2.2). The nomenclator has real duplicates
  (0011-03 measured pairs at `(75,79/84/90/92/93)`). This is a **list**, not a scalar lookup, so
  `LIMIT 1` does not apply — `GROUP BY c.IdClsfAcc`, one entry per distinct value. Both predicates
  (present-in-`FX_Istoric` **and** the angajament's `IdUnitate` via `FX_Indicatori`) are from the
  Access `bFilter_Click` query and are mandatory. `IdUnitate` is **kept** — `Clasificatii` is a
  shared nomenclator, not an `FX_` table, so the drop-`IdUnitate` rule (0011-03) does not apply.

### ⚠️ Host-verification risk: the two caption tables (`DefaClsfF` / `DefaArticol`)

§2.2 assigns the two parent-menu captions to `AVACONT_COMUN.DefaClsfF.Denumire` (keyed on
`Clasificatii.ClsfF`) and `AVACONT_COMUN.DefaArticol.Denumire` (keyed on `Clasificatii.Articol`).
**The only ground truth in the repo — the Access query `qFX_DDF_SA_CLSF` — disagrees on the
names:** it reads `DefaClsfF.Explicatie` (not `.Denumire`) and `DefaTitlu2.Denumire` (there is no
`DefaArticol` table in the Access export). The MariaDB `AVACONT_COMUN` schema for these
nomenclator tables is **not in the repo** (only `CAI`/`FX_LoginLog` are in
`sql/avacont_comun_login.sql`), so I could not verify whether the migration renamed
`Explicatie→Denumire` / `DefaTitlu2→DefaArticol`.

Decision: **implemented to the plan** (it is the approved spec, and §2.2 explicitly frames these
as a decided caption source), using `LEFT JOIN` so a *missing caption* degrades to `NULL` rather
than dropping the row — but a *missing table/column* makes the whole endpoint fail loudly (500),
which is the behaviour §2.2 asks for ("fail loudly rather than return empty captions"). **This is
the #1 thing the host run must check.** If the real column is `Explicatie` / the real table is
`DefaTitlu2`, this one SQL string is the only change.

- **Alineat caption = `Clasificatii.Denumire`** (per-unit, no join) — an **operator decision**
  over `AVACONT_COMUN.DefaClsfE.Denumire` (national standard). `qFX_Clsf2026_Structura`'s
  definition is lost, so neither could be proven faithful; recorded as a decision, not a finding.

### Deliberately NOT on the wire (§2.1)

`Utilizator`, `HASH`, `Prelucrat`, `DTQ`, `Val_Receptie_T`, `Rez_Ord` — none appears on the
Access form. `Val_Receptie_T`/`Rez_Ord` exist in the DDL but on no Access control; adding them
would be inventing a view. `test_row_shape_matches_contract` asserts none of them leak.

## Client (`KBot.Domain` + `KBot.Api`)

- `KBot.Domain/IstoricInfo.vb` — `IstoricInfo` (`Cod`, `Randuri`, `Clasificatii`), `IstoricRand`
  (`DataFx As Date?` keeps the time; `Idrev As Integer?` — many rows have no revision),
  `IstoricClasificatie`. snake_case stops at the wire DTO. `FileVersion` 1.0.1.0 → 1.0.2.0.
- `KBot.Api` — wire DTOs `GetIstoricResponse` / `GetIstoricRandRow` / `GetIstoricClasificatieRow`
  (snake_case verbatim), `GetIstoricAsync(cod)` matching `GetDdfAsync`/`GetPlatiAsync` (no
  `db_name` on the wire, bearer from session, empty arrays for an unknown cod, hard-fail on
  non-2xx). The five `FakeApiClient`s in `tests/KBot.App.Tests` got a `GetIstoricAsync` stub.
  `FileVersion` 1.0.1.0 → 1.0.2.0.

## Tests

- **Python** `tests/test_forexe_istoric.py` — 12 host-only tests (skip cleanly off-host): 401
  guard, missing/blank cod → 400, unknown cod → 200 with empty arrays, row shape (every §2.1
  field present, none of the excluded ones), `ORDER BY DataFX, Clsf`, negative values keep sign,
  `data_fx` carries time, `clasificatii` scoped to present `IdClsf` and the angajament's unit,
  **no fan-out** (one entry per `IdClsfAcc` with a duplicate nomenclator row), **key direction**
  (`IdClsfAcc` not `IDClsf`), literal diacritics, and the **finding test** (§8.12) that counts
  `(IdUnitate, IdClsfAcc)` groups with >1 distinct `Clsf` — never fails, always reports, closing
  the `Status_migrare_5 §9` open item on every host run.
- **.NET** `ApiClientTests` — `GetIstoricAsync`: url/bearer/escaping, blank-cod-throws-before-
  request, both arrays + time-preserving + nullables, empty-is-empty, 401 → `ApiException` with
  reason. Api 63 → 68.
- Offline: **.NET 436 passed / 0 failed**, build 0 errors / 0 new warnings (16 pre-existing
  NU1701); **Python 75 passed / 15 skipped / 0 failed** (`test_forexe_istoric.py` = the +1 skip).

## ⚠️ Open

- The endpoint has **never run against a live database** — all 12 Python tests are host-only and
  were seen only skipping.
- The `DefaClsfF`/`DefaArticol` name risk above is unresolved until the host run.
- The finding test's answer (`Status_migrare_5 §9`) is unknown until it runs on `000_DEMO`.
