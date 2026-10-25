# SLICE-AV01-08 - Opening balances per partner (Access `ParteneriSI`): «Parteneri» window grid + AVC.Migrator step (operator, 25.10.2026)

## What changed and why
Operator decision (25.10.2026): the balances per partner go INTO the existing «Parteneri» window as a custom DGV (`KBotDataView`) and the migration gets a new step in `AVC.Migrator`.
Target table: `TZ_SoldInitial` with `IdPartener` set (it already existed since AV01-04: key `(An, IdCont, PartenerKey)`, FK to `Parteneri`); no DDL.

1. **Server** `PYTHON/routes/avacont/configurari/plan_cont/solduri_parteneri.py` (registered in `routes/avacont/__init__.py`):
   `GET /api/avacont/parteneri/conturi` (active accounts of every subunit, for the account list), `GET /api/avacont/parteneri/solduri?partener=&an=`,
   `POST /api/avacont/parteneri/solduri` (REPLACES the partner's list of that year in one transaction: rows sent are upserted, the others of that partner and year deleted;
   400 for a repeated account / invalid amount, 404 for an unknown partner or account). Amount rules shared with `solduri.py` (`_suma`, two places, never negative).
2. **Domain / API**: `SoldPartener`, `ContDisponibil` (`KBot.Domain/Avacont/Configurari/PlanCont/SoldPartener.vb`); `IConturiApi` +3 calls
   (`GetConturiDisponibileAsync`, `GetSolduriPartenerAsync`, `SaveSolduriPartenerAsync`), implemented in the new `ApiClient.SolduriParteneri.vb`.
3. **«Parteneri» window** (`Views/Nomenclatoare/ParteneriForm`): below «Coduri angajament», a label «Solduri inițiale — anul <An>» and `gridSolduri` (columns: Cont [combo], S.I. Debitor,
   S.I. Creditor, R.P. Debitor, R.P. Creditor, ✕, hidden id). All declared in `ParteneriForm.Designer.vb`. «+» in the footer adds a row and opens the account combo
   (label = symbol + name, with the chapter in brackets only when the unit has several subunits); an account out of use that already holds a balance still shows.
   The grid is read when a partner is picked (stale answers dropped), the balances are written by the SAME «Salvare» right after the partner (a new partner gets its id first).
   Only sent when the operator touched them, and never after a failed read (so an empty grid cannot wipe stored rows). If the balances fail after the partner was saved,
   the message says exactly that and the partner list is reloaded. `KbotForm.Nomenclatoare.vb` passes `IConturiApi` + the session (year = `SessionContext.An`);
   without them the section is hidden (its two rows get height 0).
4. **`AVC.Migrator`**: new step `ParteneriSI` (`Configurari/PlanCont/AvcParteneriSiStep.vb`, after `PlanCont` in `AvcCatalog`, `DependsOn PlanCont`). Reads `ParteneriSI`
   (`SimbolCont`, `CodPartener`, `SID`, `SIC`; RPC/RPD not migrated, like the chart step). The account is found by the COMPOSED symbol of the Access `PlanCont` row in the unit's subunit;
   the partner by `Parteneri.IdUnitate` + `CodPartener`. Rows with zero amounts are not written. Verification (ATENTIE, not blocking): account not in the Access chart / not in the
   target, partner not in `Parteneri` (KBot.Migrator not run yet) - those rows are not written, the rest are; BLOCANT: negative amounts, the same partner twice on the same account.
   Write is an upsert that keeps an existing row (what the operator typed in the window is not overwritten). New finding kinds `SOLD_PARTENER_INVALID`, `PARTENER_LIPSA`, `CONT_LIPSA`. README updated.
5. **Help**: `contabil/nomenclatoare/parteneri.md` (tag `AV01-08`), see 0000-126.

## Files touched
`PYTHON/routes/avacont/{__init__,configurari/plan_cont/solduri_parteneri}.py`; `src/KBot.Domain/Avacont/Configurari/PlanCont/SoldPartener.vb`;
`src/KBot.Api/Avacont/Configurari/PlanCont/{IConturiApi,ApiClient.SolduriParteneri}.vb`; `src/KBot.App/Views/Nomenclatoare/{ParteneriForm,ParteneriForm.Designer}.vb`;
`src/KBot.App/KbotForm.Nomenclatoare.vb`; `src/AVC.Migrator/{AvcStep,AvcCatalog}.vb`, `Configurari/PlanCont/AvcParteneriSiStep.vb`, `README.md`.

## Test results
`dotnet build` of `KBot.Api`, `KBot.App` and `AVC.Migrator`: 0 warnings, 0 errors. Python: `py_compile` only. NOTHING run: no server call, no window opened, no Access file read, no test.

## Unverified / deferred
- The Access `ParteneriSI` columns (`SimbolCont`, `CodPartener`, `SID`, `SIC`) are from the export `Surse/RawExport/tables/ParteneriSI.md`; no real file was read.
- `Parteneri` keeps one row per `(IdUnitate, CodPartener)` as migrated by KBot.Migrator; the step trusts that key (checked in `MariaDB_Schema/AVACONT_SURSA.sql`, not on data).
- The «Cont» list offers every ACTIVE account of every subunit (no rule yet for which accounts take a partner: 401, 421, ... ). To narrow it, a flag on `DefaCont` would be needed (not decided).
- Combo behaviour of `KBotDataView` with long labels, the grid height share (26 % / 26 % of the right panel) and the hidden section: never seen on screen. Designer never opened in VS.
- Turnovers (R.P.) are editable in the window but NOT migrated (same decision as the chart).
- Python route not exercised (no pytest added; operator rule: no tests unless asked).
