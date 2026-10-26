# SLICE-AV01-13 - «Clasificatii»: delete, extrase by source, Access migration, names (operator, 26.10.2026)

## What changed and why
1. **Delete a revenue classification.** `DELETE /api/avacont/clasificatii-venituri/<id>` (`clasificatii_venituri.py`): one transaction, refused with a 409 that names the places when anything depends on the classification.
   Who depends is decided by `PYTHON/utils/dependente.py` (`Dependencies`): every table with a foreign key to `Clasificatii_Venituri.IdClsfV` is found by itself (information_schema), and a table without a foreign key
   subscribes with one line in `clasificatii_venituri/dependente.py` (`CLASIFICATII_VENITURI.register(table, column, label)`; today `FX_Extrase_H`, plus the labels of the budget and the corrections). A subscribed table
   missing in a database counts as not used there. Client: `IClasificatiiVenituriApi.DeleteClasificatieVenitAsync`. UI: button «Sterge clasificatia» (`btnSterge`, in the Designer) in the footer of `ClasificatiiVenituriView`, left of
   «Salveaza»; enabled for a chosen leaf with nothing unsaved. Editing a classification does not exist (operator: no point).
2. **Extrase by source.** `extrase.py` (`clsf_venituri_pentru(clsf_sal, sursa)`) and `KBot.Migrator/Transfer/ExtrasHeaderRules.vb` find `IdClsfV` on (SS of the account, code).
3. **Access migration (`KBot.Migrator/Transfer/TableMaps.vb`).** `ClasificatiiV` ▸ `Clasificatii_Venituri` writes `SS` = the sector+source of the unit whose file the row comes from (new `ColumnSourceKind.UnitSursaSector`,
   `ColumnMapping.FromUnitSursaSector`, resolved in `TransferRunner.BuildValues` from `cai.SURSA`, the value `Unitati.SursaSector` gets). The same Access table feeds a second target, `Clasificatii_Venituri_Buget`
   (`IdUnitate` of the file, `An` 2026, `DataInceput` 01.01.2026, `Trim1..4`; the Access `IdClsfV` stays the key; every row is written, like the expenses budget). `RectificariV` ▸ `Clasificatii_Venituri_Rectificari`
   gets `IdUnitate`. Note in `docs/MAPARE_NOMENCLATOARE.md` (D5).
4. **Names.** The ribbon button, which opens both pages, is «Clasificatii». Everywhere else the expenses side is «Clasificatii cheltuieli»: the MENU item (MENIU > Nomenclatoare) and its error text, the machete name, the menu-button
   tooltip, the window title, the ribbon tooltip text («de cheltuieli si de venituri»). The vertical bar of the window has «Cheltuieli» and «Venituri», and the window title follows the page shown: «K-BOT — Clasificatii cheltuieli {an}» /
   «K-BOT — Clasificatii venituri {an}». «Rectificari bugetare», «Credit bugetar» and the like are other things and keep their names.
5. **Help** (0000-133): see `SLICE-0000-133-ajutor-clasificatii-stergere-ribbon.md`.

## Files touched
`clasificatii_venituri.py`, `clasificatii_venituri/dependente.py` (new), `PYTHON/utils/dependente.py` (new), `extrase.py`; `IClasificatiiVenituriApi.vb`, `ApiClient.ClasificatiiVenituri.vb`; `ClasificatiiVenituriView.vb` + `.Designer.vb`;
`ClasificatiiForm.vb` + `.Designer.vb`, `ClasificatiiBugetView.vb`, `BudgetCheckForm.Designer.vb`; `AvacontMain.Designer.vb`, `KbotForm.Designer.vb`, `KbotForm.Nomenclatoare.vb`, `KbotForm.Machete.vb`, `KbotForm.CabNotes.vb`;
`KBot.Migrator/Transfer/ExtrasHeaderRules.vb`, `TableMaps.vb`, `ColumnMapping.vb`, `TransferRunner.vb`; `docs/MAPARE_NOMENCLATOARE.md`; help pages (see 0000-133).

## Test results
`dotnet build` of `KBot.App`, `KBot.Migrator`, `AVC.Migrator`: 0 warnings, 0 errors. `py_compile` of the Python files: ok; the dependency registry was smoke-checked with a fake cursor (a table with a foreign key is found without being
subscribed). No tests run (operator rule); the migrator maps were never run against an Access file, the delete never against a server; nothing seen on screen.

## Unverified / deferred
- The Access `ClasificatiiV` / `RectificariV` column names come from the Access code read for AV01-01 (`IdClsfV`, `Capitol`, `SubCapitol`, `Paragraf`, `Denumire`, `Trim1..4`; `Document`, `Data`), not from an exported file; the budget
  target excludes `Capitol`, `Subcapitol`, `Paragraf`, `Denumire`.
- The budget start date 01.01.2026 is a choice (the window starts the first version of a year on 01.01); expenses use 01.04.2026.
- The role right «Stergere clasificatii» of the Access settings is not implemented: any user of the window can delete an unused classification.
