# SLICE-AV01-10 - Journals follow the title-bar sector+source; «Parteneri» list filters; key navigation on two trees (operator, 26.10.2026)

## What changed and why
Five requests of the operator after looking at AV01-08 / AV01-09:

1. **Journals tied to the sector+source (SS) of the title bar, like the accounts.** Every journal now has a `SursaSector`; the code is unique PER SS.
   - DDL `sql/AVACONT/Configurari/Jurnale/AV01_09_jurnale.sql` REWRITTEN (the first version was not run): `TZ_Jurnale(IdJurnal, SursaSector FK DefaSursaSector, Cod, Denumire, ...)`,
     unique `(SursaSector, Cod)`. An `ALTER ... IF NOT EXISTS` block upgrades a table made by the first version, but only while it is EMPTY (the file says so).
   - Server `jurnale.py`: GET also returns `surse` (every sector+source); POST rows carry `sursasector` (checked against `DefaSursaSector`), duplicates are `(ss, cod)`, only the rows sent are touched.
   - Domain/API: `JurnalInfo.SursaSector`, new `JurnaleCatalog` (journals + sources); `IJurnaleApi.GetJurnaleAsync` returns it.
   - Window `JurnaleForm(api, gate, session)`: with a concrete SS of the title bar it lists only that SS's journals, hides the «Sursa» column and gives new rows that SS; on «Toate» (or an SS the
     list does not know: same test as `PlanContContext.SsEditable`) it lists all of them and the «Sursa» combo is typed per row. Caption says which. The grid fills on «Denumire» (`SpecificColumn`), not on the code.
   - `AVC.Migrator`: the journal step now `DependsOn Subunitati` and puts on each journal the SS of ITS unit's subunit (existing one, or the one this run creates; none = BLOCANT);
     `AvcSubunitRef` got `SursaSector`. Merge key = SS + code.
2. **«Parteneri»: the check box under the list is gone** (`chkFaraActivitate` + `flowFiltre` removed from the Designer; `tlyLeft` has one row).
3. **«Parteneri»: a right button in the tree header with a popup menu** (`CustomPopup`, like the accounts tree): «Arată partenerii fără activitate», «Arată partenerii cu sold inițial», «Arată partenerii cu angajamente».
   Meaning (my reading, to be confirmed): the list always has the partners that appear on a document (`Partener.Activ`); each choice ADDS a kind (the union). The partner on screen / just saved never vanishes.
   All three start unchecked, so the old default (everything shown) is now «only partners with documents» until a choice is ticked. «cu sold inițial» = a row in `TZ_SoldInitial` for the working year with a partner:
   new route `GET /api/avacont/parteneri/cu-solduri?an=` (`solduri_parteneri.py`) + `IConturiApi.GetParteneriCuSolduriAsync`, read with the catalog. «cu angajamente» = `Partener.Coduri.Count > 0` (already in the catalog).
4. **«Parteneri»: `tree.ClickOnKeyNavigation = True`** (every arrow move raises `NodeMouseUp` on the partner reached; the unsaved-changes question is the same as on a click).
5. **«Plan de conturi»: `tree.ClickOnKeyNavigation = True`, but only REAL accounts react**: `Tree_NodeMouseUp` now ignores any node with `Children.Count > 0` (tested on the child nodes, not on the level) and any node
   without a `ContPlan` tag.

## Files touched
`sql/AVACONT/Configurari/Jurnale/AV01_09_jurnale.sql`; `PYTHON/routes/avacont/configurari/{jurnale/jurnale,plan_cont/solduri_parteneri}.py`;
`src/KBot.Domain/Avacont/Configurari/Jurnale/JurnalInfo.vb`; `src/KBot.Api/Avacont/Configurari/{Jurnale/IJurnaleApi,Jurnale/ApiClient.Jurnale,PlanCont/IConturiApi,PlanCont/ApiClient.SolduriParteneri}.vb`;
`src/KBot.App/Avacont/Configurari/Jurnale/{KbotForm.Jurnale,Views/JurnaleForm,Views/JurnaleForm.Designer}.vb`; `src/KBot.App/Views/Nomenclatoare/{ParteneriForm,ParteneriForm.Designer}.vb`;
`src/KBot.App/Avacont/Configurari/PlanCont/Views/{PlanContConturiView,PlanContConturiView.Designer}.vb`;
`src/AVC.Migrator/{Configurari/Subunitati/AvcSubunitStore,Configurari/Jurnale/AvcJurnaleStep}.vb`, `README.md`; help `jurnale.md`, `parteneri.md`, `plan-conturi.md` (tag `AV01-10`, see 0000-128).

## Test results
`dotnet build` of `KBot.Api`, `KBot.App`, `AVC.Migrator`: 0 warnings, 0 errors. Python: `py_compile` only. Nothing run, nothing seen on screen, no test.

## Unverified / deferred
- **DDL not run** (the operator runs the rewritten `AV01_09_jurnale.sql` on `AVACONT_SURSA`, then schema sync). If the first version had been run with rows in it: drop the table and run again.
- The reading of the three popup choices (union, default unchecked) is mine; the operator may want filters that narrow instead, or a different default.
- With the new default, partners without documents are not listed until a choice is ticked (a partner added in the window stays visible while selected, then disappears from the list after another pick unless it has codes / a balance).
- Key navigation: with `ClickOnKeyNavigation` each arrow step asks the unsaved-changes question on both windows (same as a click); never tried on screen.
- The unit-has-no-subunit case of the journals step is blocking; a unit with journals but no subunit in the Access registry cannot be migrated until its subunit exists.
