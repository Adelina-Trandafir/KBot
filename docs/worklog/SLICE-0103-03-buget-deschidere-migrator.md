# SLICE-0103-03 — Opening budget from the previous year's Access files («Buget 1/12» in the Migrator) (operator request, 02.10.2026)

Branch: `SLICE-0100-Multithreading` (nothing committed). Slice number: the operator chose «sub-slices 0103-03..06».

## What changed and why

Until a new year's budget is adopted only a twelfth of last year's budget may be spent per month. The operator asked
for a system attached to the Migrator that, for the SELECTED units, reads the PREVIOUS year's `Clasificatii.Trim1..4` plus
`Rectificari.Trim1..4` per classification and writes `Clasificatii_Buget` with start date 01.01 of the transfer year.

- **`Transfer/BudgetOpeningRunner.vb`** (new). Per unit: previous-year file = the registry's unit file with the year stepped
  back (`baza2026.accdb` → `baza2025.accdb`, same folder); sum per classification INSIDE that file (Rectificari joined to
  Clasificatii on its own `IDClsf`); match to MariaDB on `Access.IdClsf = Clasificatii.IdClsfAcc AND Access unit = Clasificatii.IdUnitate` (operator's rule, 02.10.2026;
  the unit is the one whose file is read); no row or several rows → reported as unmatched, never guessed.
  Value written: **`Trim1 = CEILING(sum / 12)`, `Trim2..4 = 0`** (operator's answer: the budget is read cumulatively to the
  quarter of the day, so for January only Trim1 counts). Two phases: `Plan` only reads; `Write` inserts in ONE transaction.
  A classification that already has the 01.01 version is left untouched and counted.
- **`MigratorForm`**: new button «Buget 1/12» (`btnBugetDeschidere`, designer, 5th column of `tlpButoane`), handler shows the plan
  (rows to write / already there / unmatched / skipped units) and asks before writing.
- `KBot.Migrator` FileVersion 1.16.1.0 → 1.17.0.0.

## Files touched

`src/KBot.Migrator/Transfer/BudgetOpeningRunner.vb` (new), `src/KBot.Migrator/MigratorForm.vb`, `MigratorForm.Designer.vb`,
`KBot.Migrator.vbproj`.

## Test results

No tests written or run (operator: no tests). `dotnet build src/KBot.Migrator/KBot.Migrator.vbproj` → 0 warnings, 0 errors.
Nothing run against Access or MariaDB.

## Left unverified / deferred

- **Needs the 0102 DDL** (`Clasificatii_Buget.DataInceput`) on the target database first.
- Assumptions to confirm: the previous year's file is found by replacing the year in the registry's unit file name; the match is
  on `IdClsfAcc` + `IdUnitate` (operator); zero-sum classifications are written too
  (a 01.01 version of 0 instead of the «no version» fallback in the DDF).
- Journal: only the form's log; no SQL journal file is written for this run.
- Never run: not seen on screen, not run against real files.
