# SLICE-0102 — The budget of a classification with dates: the DDF uses the budget it had on the revision's day (operator request, 02.10.2026)

Branch: `SLICE-0100-Multithreading` (nothing committed).

## What changed and why

**The problem (operator).** Generating revision 0 of a current angajament (case AAB2ES88K5F, line AA2) showed a NEGATIVE
«Valoare ramasă»: the budget fell in the meantime (rectification) and the initial reservation was measured against today's
smaller budget. «Buget» in the DDF came from `FX_Rezervari.R_CreditBug` / `FX_Indicatori.Credit_Bugetar`, i.e. what FOREXE says
**today** (`R_CreditBug` is stamped from `Credit_Bugetar` when `FX_Rezervari` is written, so a history downloaded after the
rectification gets today's value on January's reservation). The earlier budget was kept nowhere.

**The rule (operator answers, 02.10.2026).**
1. `Clasificatii_Buget` gets `DataInceput` (date, required): one row = one VERSION of the budget of a classification for a year,
   starting on that day. The existing rows get **01.04.2026** (a row of another year, if any, 01.01 of its year).
2. Rectifications (`Clasificatii_Rectificari`: `Data`, `Document`, `Trim1..4`) are NOT budget but change it, in the quarter whose
   `TrimN` they carry. A rectification counts from the version's `DataInceput` up to the revision day (older than the version =
   already inside it).
3. «Buget» on day D = version with the greatest `DataInceput <= D` (same `IdClsf`, `An = YEAR(D)`) + its rectifications,
   **cumulative to the quarter of D** (Trim1..Trimq). **No yearly total** anywhere (operator: a total there is not correct).
4. No version on D → the DDF keeps today's `Credit_Bugetar` and says which classifications fell back (notice in the editor).
5. D = the angajament's creation date for revision 0, the reservation day for the others, the revision's own date for a
   document opened for editing.

Pieces:
- **DDL**, split in two by slice 0103 (the first draft was one all-bases loop, never run, now deleted):
  `sql/0102_01_sursa.sql` runs on `AVACONT_SURSA` only (the column, the new unique key, and the ledger table of 0103); schema
  sync then carries the structure to the units. `sql/0102_02_interogare_unica.sql` is the DATA part (start date for the existing
  rows, drop the old unique key), pasted into AvacontPush's «Interogări unice» tab and run on every database
  (`SLICE-0103-interogari-unice.md`). `sql/AVACONT_SURSA.sql` (the template dump) updated the same way.
- **`PYTHON/routes/forexe/budget_on_day.py`** (new): `budget_on_day`, `BudgetByDay` (memory per (classification, day) + the
  fallback notice text).
- **`ddf_edit.py`**: `POST /api/forexe/ddf/genereaza` replaces `Buget` of every generated line (both sources: reservations and
  indicators); `GET /api/forexe/ddf/draft/...` fills `buget` (was always 0.0) from the revision's `DataRev`.
- **`clasificatii_edit.py`** (the «Clasificații bugetare» window): the budget is now a LIST of versions. GET returns
  `budgets: [{id, data_inceput, trim1..4}]` (replaces `budget`); POST takes `budgets` + `deleted_budgets` (removals first, then
  updates/inserts; start date required, inside the year, one per day). 409 text for a repeated start date.
- **Legacy Access routes** (`routes/clasificatii.py` insert + upsert, `routes/nomenclatoare.py`): the plain inserts write
  `DataInceput = MAKEDATE(An, 1)`. **The upsert no longer overwrites a budget**: a classification that already has any version
  for the year is skipped (response `buget.skipped` replaces `buget.updated`), because Access's current figures written over a
  version would erase the budget the DDF needs as it was.
- **Window** (`ClasificatiiForm` + Designer): `gridBuget` is a multi-row grid — «Început», Trim. 1–4, «✕», hidden id; footer «+»
  adds a version; no «Total» column. `KBot.Domain.Nomenclatoare`: new `BudgetVersion`, `BugetClasificatie.Budgets` (replaces
  `Budget`); `INomenclatoareApi.SaveBugetClasificatieAsync` takes the versions and the deleted version ids.
- **Migrator** (`TableMaps.vb`): `DataInceput` constant 01.04.2026 for `Clasificatii_Buget` (required on the target now).
- **Help**: 0000-40 (see `SLICE-0000-40-ajutor-buget-pe-versiuni.md`).

## Files touched

`sql/0102_01_sursa.sql` + `sql/0102_02_interogare_unica.sql` (new), `sql/AVACONT_SURSA.sql`, `PYTHON/routes/forexe/budget_on_day.py` (new),
`PYTHON/routes/forexe/ddf_edit.py`, `PYTHON/routes/forexe/clasificatii_edit.py`, `PYTHON/routes/clasificatii.py`,
`PYTHON/routes/nomenclatoare.py`, `src/KBot.Domain/Nomenclatoare.vb`, `src/KBot.Api/INomenclatoareApi.vb`,
`src/KBot.Api/ApiClient.Nomenclatoare.vb`, `src/KBot.App/Views/Nomenclatoare/ClasificatiiForm.vb` + `.Designer.vb`,
`src/KBot.Migrator/Transfer/TableMaps.vb`; help files (see the 0000-40 worklog).

## Test results

No tests written or run (operator: no tests). `dotnet build src/KBot.App/KBot.App.vbproj` and the Migrator project → 0 warnings,
0 errors. The Python files compile (`py_compile`); nothing was run against a database. The app was not run.

## Left unverified / deferred

- **Deploy order:** `0102_01_sursa.sql` on `AVACONT_SURSA`; push the server files (`budget_on_day.py`, `ddf_edit.py`,
  `clasificatii_edit.py`, `clasificatii.py`, `nomenclatoare.py`, `routes/one_time/`, `provizionare.py`) and restart; schema sync
  SAFE on the units; the one-time query `0102_buget_data_inceput`; then the client. Until the DDL runs, DDF generation and the
  draft read fail (the query names `DataInceput`), and so does the window's save.
- **The existing rows start on 01.04.2026**, so a revision dated before that (the AA2 case, 12.01.2026) finds no version and
  keeps today's credit (with the notice). To get January–March right the operator adds, per classification, a version starting
  01.01.2026 in «Clasificații bugetare» — the operator's stated next step. Nothing is added automatically.
- **The classification combo of the line dialog** (a line added by hand with «Adaugă rând») still shows today's
  `Credit_Bugetar` as «Buget». Doing it needs the revision date on `IApiClient.GetDdfClasificatiiAsync`, whose signature is
  implemented by ten test fakes (`tests/KBot.App.Tests/*`); left for a decision. The help says so.
- **Draft read**: a classification with no version on `DataRev` keeps the old 0.0 «Buget» (no fallback, no notice).
- Nothing seen on screen: the multi-row budget grid, «+» / «✕», the date cell, the save with a repeated or out-of-year date.
- The DB column `TOTAL` (generated) stays in the table; the window no longer shows or uses it.
- `PYTHON/tests/test_schema_diff_columns.py` describes a `Clasificatii_Buget` shape; not touched and not run.
- Cumulative-to-the-quarter is the operator's choice (answer «cumulat»); a rectification dated before its version's
  `DataInceput` is ignored (operator: «da»).
