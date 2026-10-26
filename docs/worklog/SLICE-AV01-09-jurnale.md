# SLICE-AV01-09 - «Jurnale»: the journals of the unit, a small MODAL window + AVC.Migrator step (operator, 25.10.2026)

## What changed and why
Decisions of the operator (25.10.2026): the list of journals is INDIVIDUAL per unit (no common nomenclator); the window is a small popup MODAL form, not a normal page.
Rules from the reading sheet `docs/access-map/avacont/jurnale.md` kept: a code cannot be empty; a journal that is used cannot be deleted (no cascade, no confirmation).

1. **DDL** `sql/AVACONT/Configurari/Jurnale/AV01_09_jurnale.sql` (on `AVACONT_SURSA`, then schema sync): `TZ_Jurnale(IdJurnal, Cod varchar(10) UNIQUE, Denumire varchar(255), DataAdaugare, DataModificare)`.
   "Per unit" = per unit DATABASE (the DC), like every `TZ_` table: no unit column. `utf8mb3_general_ci`, so the code is unique whatever the case.
2. **Server** `PYTHON/routes/avacont/configurari/jurnale/jurnale.py` (registered in `routes/avacont/__init__.py`): `GET /api/avacont/jurnale`; `POST /api/avacont/jurnale`
   `{randuri:[{idjurnal|null,cod,denumire}], sterse:[ids]}` = ONE transaction (deletes, then updates/inserts). Code trimmed + capitals, `^[A-Z0-9_]{1,10}$`, unique;
   400 invalid row, 409 duplicate code or a journal refused by a RESTRICT foreign key («Jurnalul X este folosit (operatii sau sabloane) si nu se poate sterge»).
3. **Domain / API**: `JurnalInfo`; `IJurnaleApi` (`GetJurnaleAsync`, `SaveJurnaleAsync`) + `ApiClient.Jurnale.vb`.
4. **Window** `Avacont/Configurari/Jurnale/Views/JurnaleForm` (`Inherits KBotModalForm`, opened through `ShowModalAsync` like «Grupe»; not a macheta page, not in the history).
   Small: 560 x 440 at 96 dpi (minimum 460 x 360). Caption bar, a `KBotNotice` box for warnings/errors (same system as the ord/plan windows), a `KBotDataView` (Cod, Denumire, ✕, hidden id; «+» in
   the footer; typed in place), «Renunță» left / «Salvare» right. All declared in the Designer. «Salvare» writes everything and closes; closing with unsaved changes asks. A failed read keeps the grid off.
   Opened by `KbotForm.DeschideJurnale` (`KbotForm.Jurnale.vb`); ribbon button «Jurnale» (key `jurnale`) in Date, after «Plan de conturi» (`AvacontMain.Designer.vb`, `AvacontMain.vb`).
5. **`AVC.Migrator`**: step `Jurnale` (`Configurari/Jurnale/AvcJurnaleStep.vb`): reads `Jurnale` (`CodJurnal`, `DenumireJurnal`) of every ticked unit and merges them by code (capitals) into the one `TZ_Jurnale`;
   same code with different names = ATENTIE (first name kept); a code the window would refuse = BLOCANT; upsert that keeps an existing row. New finding kind `JURNAL_INVALID`. README updated.
6. **Help**: new topic `contabil/nomenclatoare/jurnale.md` (+ line in `index.md`), tag `AV01-09`, see 0000-127.

## Files touched
`sql/AVACONT/Configurari/Jurnale/AV01_09_jurnale.sql`; `PYTHON/routes/avacont/{__init__,configurari/jurnale/__init__,configurari/jurnale/jurnale}.py`;
`src/KBot.Domain/Avacont/Configurari/Jurnale/JurnalInfo.vb`; `src/KBot.Api/Avacont/Configurari/Jurnale/{IJurnaleApi,ApiClient.Jurnale}.vb`;
`src/KBot.App/Avacont/Configurari/Jurnale/{KbotForm.Jurnale.vb,Views/JurnaleForm.vb,Views/JurnaleForm.Designer.vb}`; `src/KBot.App/{AvacontMain,AvacontMain.Designer}.vb`;
`src/AVC.Migrator/{AvcStep,AvcCatalog}.vb`, `Configurari/Jurnale/AvcJurnaleStep.vb`, `README.md`.

## Test results
`dotnet build` of `KBot.Api`, `KBot.App`, `AVC.Migrator`: 0 warnings, 0 errors. Python: `py_compile` only. NOTHING run: the DDL is not run, no window opened, no Access file read, no test.

## Unverified / deferred
- **DDL not run** (operator runs it on `AVACONT_SURSA`, then schema sync); without it the window's first read answers with a server error shown in the red box.
- The Access `Jurnale` columns (`CodJurnal`, `DenumireJurnal`) are from the reading sheet / export; no real file read. The Access `LC` journal has the name «JC» (apparent duplicate of `JC`): migrated as is.
- «A message says how many operations use the journal» (sheet, section 2) is NOT possible yet: there are no operation tables (stage 2). Now the message only says it is used. The RESTRICT foreign keys of the future tables must reference `IdJurnal`.
- Changing the CODE of a journal that has operations (sheet question 3): not blocked here. Later tables point at `IdJurnal`, so a code change breaks nothing technically; to block it, the check goes with the operations slice.
- Swapping two codes between two journals in one save is refused by the unique key (message «Exista deja un jurnal cu codul ...»): rare, left as is.
- The window's size / layout / the modal placement were never seen on screen; Designer never opened in VS.
