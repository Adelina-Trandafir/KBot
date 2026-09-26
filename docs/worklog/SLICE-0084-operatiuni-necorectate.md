# SLICE 0084 — «Operațiuni necorectate» after the FOREXE login

**Date:** 26.09.2026. **Request (operator):** after connecting to FOREXE, read the landing page
automatically; when it has the «Operațiuni necorectate» table, warn the operator and save the rows
into `FX_Operatiuni`. Show only Program, Sector - Sursa - Indicator, Referință TREZOR, Nr. document,
Dată Plată, Tip, Suma, and Probleme only when some cell has text.

## What changed

- **Read** (`KBot.Forexe`): `WorkflowExecutor.ReadUncorrectedOperationsAsync` (new partial
  `WorkflowExecutor.PageTables.vb`) finds the `<h4>` whose text without diacritics is
  "Operatiuni necorectate", reads the table under it (columns keyed by header, ASCII lower case),
  and counts the pages in the pagination. Exposed via `IForexeRunner.ReadUncorrectedOperationsAsync`.
- **Model** (`KBot.Domain/UncorrectedOperations.vb`): parses that JSON (date `dd/MM/yyyy`, amount in
  Romanian format), builds the operator message (max 25 lines, total, warning when the table has
  more than one page: only page 1 is read).
- **Trigger** (`ForexeController`): after BOTH `ConnectAsync` overloads succeed, the table is read;
  rows → event `UncorrectedOperationsFound`. A failed read never fails the connection.
- **Shell** (`KbotForm.UncorrectedOperations.vb`): saves first, then ONE `KBotMessage` with the list
  and a last line saying what was saved (or why not).
- **API**: `IUncorrectedOperationsApi` + `ApiClient.UncorrectedOperations.vb` →
  `POST /api/forexe/operatiuni/necorectate`.
- **Server**: `PYTHON/routes/forexe/operatiuni.py`. Inserts ONLY rows not already present
  (key: `ReferintaTrezor` + `NrDoc`; operator's choice); nothing deleted/updated.
  `IdClsf` = SS → `Unitati.SursaSector` → `Clasificatii` (IdUnitate + ClsfSal); `CodSSI` = SS + ClsfSal.
  Unknown program / unresolved classification / bad date → row skipped with a warning.
- Versions: Domain 1.2.5, Api 1.0.11 (Forexe already 1.0.16 in the uncommitted work).

## Files touched

`src/KBot.Forexe/Executor/WorkflowExecutor.PageTables.vb` (new), `IForexeRunner.vb`, `ForexeRunner.vb`,
`src/KBot.Domain/UncorrectedOperations.vb` (new), `src/KBot.Api/IUncorrectedOperationsApi.vb` (new),
`ApiClient.UncorrectedOperations.vb` (new), `src/KBot.App/Forexe/ForexeController.vb`,
`KbotForm.UncorrectedOperations.vb` (new), `KbotForm.vb`, `KbotForm.ForexeWatch.vb`,
`tests/KBot.App.Tests/ForexeControllerFailureTests.vb` (fake runner), `PYTHON/routes/forexe/operatiuni.py`
(new), `PYTHON/routes/forexe/__init__.py`, the two `.vbproj` versions.

## Test results

- `dotnet build src\KBot.App` (scratch folder): **0 errors, 0 warnings**. `py_compile` green.
- No tests written or run; the page script was NOT run against the sample HTML; nothing live.

## Left unverified / deferred

- ⚠ **`FX_Operatiuni.Suma` must exist** — the operator adds it (the 22.09 dump does not have it).
  Until then every insert fails with a 500 and the message says the save failed.
- `operatiuni.py` + `__init__.py` must be uploaded to the VPS.
- Only page 1 of the FOREXE table is read (the message says so when there are more).
- The DirectorForm login path was not wired (it does not use `ForexeController.ConnectAsync`, not checked).

## NEXT — correlation with angajamente (requested, not done)

The saved operations will next be **correlated with the angajamente in the database** (the page
shows them as «ERRRRRRRRRR» / «Neidentificat»: FOREXE could not attach them). Likely inputs:
Program + IdClsf/CodSSI + Suma + DataPlata against `FX_Angajamente` / `FX_Indicatori` / `FX_Plati`.
The rule and where the link is stored (a new column on `FX_Operatiuni`?) are still to be decided
with the operator. Because the save only ever inserts, a link written later is not lost on the
next login.
