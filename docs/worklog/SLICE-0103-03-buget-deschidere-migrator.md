# SLICE-0103-03 / 04 — Buget de deschidere și comparația FOREXE

Reunire documentară D001-24. Două funcții bugetare distincte, la reperul 02.10.2026: deschiderea 1/12 în Migrator și comparația creditului FOREXE. Formula trimestrială de aici este istorică; registrul 0100–0109 consemnează ulterior în 0108 schimbarea calculului. Nu se deduce un verdict live sau o rulare pe Access.

[0103-03](#slice-0103-03) · [0103-04](#slice-0103-04)

<a id="slice-0103-03"></a>

## SLICE-0103-03 — Opening budget from the previous year's Access files («Buget 1/12» in the Migrator) (operator request, 02.10.2026)

Branch: `SLICE-0100-Multithreading` (nothing committed). Slice number: the operator chose «sub-slices 0103-03..06».

### What changed and why

Until a new year's budget is adopted only a twelfth of last year's budget may be spent per month. The operator asked for a system attached to the Migrator that, for the SELECTED units, reads the PREVIOUS year's `Clasificatii.Trim1..4` plus `Rectificari.Trim1..4` per classification and writes `Clasificatii_Buget` with start date 01.01 of the transfer year.

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

### Files touched

`src/KBot.Migrator/Transfer/BudgetOpeningRunner.vb` (new), `src/KBot.Migrator/MigratorForm.vb`, `MigratorForm.Designer.vb`, `KBot.Migrator.vbproj`.

### Test results

No tests written or run (operator: no tests). `dotnet build src/KBot.Migrator/KBot.Migrator.vbproj` → 0 warnings, 0 errors. Nothing run against Access or MariaDB.

### Left unverified / deferred

- **Needs the 0102 DDL** (`Clasificatii_Buget.DataInceput`) on the target database first.
- Assumptions to confirm: the previous year's file is found by replacing the year in the registry's unit file name; the match is
  on `IdClsfAcc` + `IdUnitate` (operator); zero-sum classifications are written too
  (a 01.01 version of 0 instead of the «no version» fallback in the DDF).
- Journal: only the form's log; no SQL journal file is written for this run.
- Never run: not seen on screen, not run against real files.

---

<a id="slice-0103-04"></a>

## SLICE-0103-04 — Check: the budget FOREXE reported == the budget K-BOT holds (operator request, 02.10.2026)

Branch: `SLICE-0100-Multithreading` (nothing committed).

### What changed and why

The operator asked for a system that checks the budget downloaded from FOREXE against Clasificatii + Rectificari as of now, per `IdClsf`, and when they differ shows a window with every classification, both values and the difference.

- **Server** `routes/forexe/clasificatii_edit.py`: `GET /api/forexe/nomenclatoare/clasificatii/verificare-buget?data=&angajament=`.
  K-BOT side = `budget_on_day` (version in force + rectifications, cumulative to the quarter of the day: the DDF's own rule).
  FOREXE side = `FX_Indicatori.Credit_Bugetar` of the most recent indicator row (`DTQ`) of the classification (operator's choice:
  `Credit_Bugetar`). With `angajament` only that angajament's classifications and its own indicator rows. Equal = within 0.005.
  The tree route now also returns `id_unitate` and `id_clsf_acc` (used by 0103-06).
- **Client**: `INomenclatoareApi.GetBudgetCheckAsync`, `BudgetCheck` / `BudgetCheckRow` (Domain), `BudgetCheckForm` (dialog,
  designer-authored; grid: Clasificație, Denumire, SS, Buget K-BOT, Credit FOREXE, Diferență; check box «Doar diferențele»).
- **Triggers**: the new button «Verifică bugetul» in «Clasificații bugetare» (always opens the window); automatically at the end of
  `DuLaIngestieAsync` after a download (`VerificaBugetulFxAsync`): the window opens ONLY when something differs. **Removed 27.10.2026 (operator):** a download no longer opens the window; only «Verifica bugetul» in Clasificatii does (`VerificaBugetulFxAsync` deleted).
- FileVersions: KBot.Api 1.0.19.0, KBot.Domain 1.2.11.0.

### Files touched

`PYTHON/routes/forexe/clasificatii_edit.py`, `src/KBot.Domain/Nomenclatoare.vb`, `src/KBot.Api/INomenclatoareApi.vb`, `src/KBot.Api/ApiClient.Nomenclatoare.vb`, `src/KBot.App/Views/Nomenclatoare/BudgetCheckForm.vb` + `.Designer.vb` (new), `ClasificatiiForm.vb` + `.Designer.vb`, `src/KBot.App/KbotForm.Ingest.vb`, `KbotForm.Nomenclatoare.vb`, `*.vbproj` versions.

### Test results

No tests written or run (operator: no tests). `dotnet build` of KBot.Api, KBot.App → 0 warnings, 0 errors; the Python file compiles (`py_compile`). Nothing run against a database; window not seen on screen.

### Left unverified / deferred

- Needs the 0102 DDL + the server file deployed. Until the 01.01 / approved versions exist in K-BOT (see 0103-03) most rows will
  show «Buget K-BOT» empty, so the automatic check after a download will open often at first.
- «Credit FOREXE» is the latest download of the classification across angajamente; if two angajamente carry different credits for the
  same classification the newest one wins.
- The capture `clasificatii` is stale (two new buttons) — marked `redo` in the help (0000-41).

## Ordinea și confirmările din registru

Pentru 0103-03…06, registrul cerea: DDL-ul 0102 pe baze → clasificatii_edit.py / clasificatii.py → «Buget 1/12» în Migrator → client K-BOT; modulul VBA se importă manual. «Registru» = cale.accdb și fișierul precedent prin schimbarea anului în numele din registru rămâneau de confirmat. Ajutorul verificării și al trimiterii Access era 0000-41.
