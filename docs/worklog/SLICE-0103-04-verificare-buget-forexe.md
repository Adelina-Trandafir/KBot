# SLICE-0103-04 — Check: the budget FOREXE reported == the budget K-BOT holds (operator request, 02.10.2026)

Branch: `SLICE-0100-Multithreading` (nothing committed).

## What changed and why

The operator asked for a system that checks the budget downloaded from FOREXE against Clasificatii + Rectificari as of now,
per `IdClsf`, and when they differ shows a window with every classification, both values and the difference.

- **Server** `routes/forexe/clasificatii_edit.py`: `GET /api/forexe/nomenclatoare/clasificatii/verificare-buget?data=&angajament=`.
  K-BOT side = `budget_on_day` (version in force + rectifications, cumulative to the quarter of the day: the DDF's own rule).
  FOREXE side = `FX_Indicatori.Credit_Bugetar` of the most recent indicator row (`DTQ`) of the classification (operator's choice:
  `Credit_Bugetar`). With `angajament` only that angajament's classifications and its own indicator rows. Equal = within 0.005.
  The tree route now also returns `id_unitate` and `id_clsf_acc` (used by 0103-06).
- **Client**: `INomenclatoareApi.GetBudgetCheckAsync`, `BudgetCheck` / `BudgetCheckRow` (Domain), `BudgetCheckForm` (dialog,
  designer-authored; grid: Clasificație, Denumire, SS, Buget K-BOT, Credit FOREXE, Diferență; check box «Doar diferențele»).
- **Triggers**: the new button «Verifică bugetul» in «Clasificații bugetare» (always opens the window); automatically at the end of
  `DuLaIngestieAsync` after a download (`VerificaBugetulFxAsync`): the window opens ONLY when something differs.
- FileVersions: KBot.Api 1.0.19.0, KBot.Domain 1.2.11.0.

## Files touched

`PYTHON/routes/forexe/clasificatii_edit.py`, `src/KBot.Domain/Nomenclatoare.vb`, `src/KBot.Api/INomenclatoareApi.vb`,
`src/KBot.Api/ApiClient.Nomenclatoare.vb`, `src/KBot.App/Views/Nomenclatoare/BudgetCheckForm.vb` + `.Designer.vb` (new),
`ClasificatiiForm.vb` + `.Designer.vb`, `src/KBot.App/KbotForm.Ingest.vb`, `KbotForm.Nomenclatoare.vb`, `*.vbproj` versions.

## Test results

No tests written or run (operator: no tests). `dotnet build` of KBot.Api, KBot.App → 0 warnings, 0 errors; the Python file compiles
(`py_compile`). Nothing run against a database; window not seen on screen.

## Left unverified / deferred

- Needs the 0102 DDL + the server file deployed. Until the 01.01 / approved versions exist in K-BOT (see 0103-03) most rows will
  show «Buget K-BOT» empty, so the automatic check after a download will open often at first.
- «Credit FOREXE» is the latest download of the classification across angajamente; if two angajamente carry different credits for the
  same classification the newest one wins.
- The capture `clasificatii` is stale (two new buttons) — marked `redo` in the help (0000-41).
