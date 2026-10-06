# SLICE-00EF-03 - Migrator, tab «E-Factura» (code written, never run)

Slice 00EF (operator, 06.10.2026). Operator asked for a new tab in the Migrator for E-Factura (issued + received). **Correction the same day: the units of measure (EF_UM) are NOT migrated** - the operator writes `AVACONT_COMUN.EF_UM` on the server himself. The tab only READS that table, to warn about a line whose unit of measure is not in it.

## What changed and why
- `KBot.Migrator` form now has a `TabControl` (standard control, themed by the engine; chosen by the operator): the server box stays on top and shared; the existing unit list + transfer moved UNCHANGED into the tab «Transfer»; new tab «E-Factura».
- New folder `src/KBot.Migrator/EFactura/`: `EfCui` (tax code without «RO»/spaces: the one rule the server and the KbotForm view must reuse), `EfModels`, `EfTableSpec` (columns, kinds, conversion: nothing is cut or rounded silently; text too long, non-integer, bad date = BLOCKING; rounding = warning), `EfBatchWriter` (multi-row INSERT; a duplicate key leaves the target row as it is), `EfImporter` (Verify = read only; Run = ONE transaction on the unit database).
- What is copied: EF_Furnizor (UNIT first row + Scheme `DPIFV`: T3 series, C2 flag); EF_Clienti; EF_Facturi + EF_FacturiLinii (orphan lines/clients and repeated series+number are blocking); EF_Mesaje (only `cui_unit` = the unit's tax code), EF_Primite, EF_PrimiteLinii, EF_PrimiteNote, EF_PrimiteMesaje (followed by key; credit-note references set in a second step). EFT_O and everything accounting stay behind. Access keys are kept as MariaDB keys.
- The tab keeps two paths in `migrator-settings.json` (not secrets); Importă is enabled only by a clean Verify and only while nothing it looked at changed. FileVersion 1.17.1.0 -> 1.18.0.0.
- `sql/00EF_02_efactura_comun.sql`: EF_UM is defined there (one list for all units); the comment says the operator fills it.

## Files touched
New: `src/KBot.Migrator/EFactura/{EfModels,EfCui,EfTableSpec,EfBatchWriter,EfImporter}.vb`, `src/KBot.Migrator/MigratorForm.EFactura.vb`, this file.
Edited: `MigratorForm.Designer.vb` (tab + controls, anchored patch), `MigratorForm.vb` (load/save of paths, busy state, cancel button shared, DC change refreshes the tab), `MigratorSettings.vb`, `KBot.Migrator.vbproj`, the SQL scripts, the plan/status.

## Test results
`dotnet build src/KBot.Migrator` : succeeded, no warnings. NOTHING was run: no window opened, no Access file read, no SQL executed. (The UM-file reader written first was removed after the correction.)

## Unverified / deferred
- The tab and the moved groups were not seen on screen (the Designer was edited by an anchored script, not in Visual Studio): open the form once in the designer; sizes are in the 144-dpi units of the file.
- The importer was never run against real Access files or MariaDB. Assumptions to check on the first Verify: the OLE column `EF.XML` comes back as bytes; `EFT.NC` is -1/1; `Factura.NumarFactura` holds whole numbers; `UNIT.Judetul` is a county code of at most 8 characters; the received store really is one shared `ef_<an>.accdb` filtered by `cui_unit`.
- The SQL (nine unit tables + EF_Token, EF_TokenStart, EF_UM) must be run before Verify can pass.
- Years before the transfer year are not imported (one file per year).
- Not in the help: the Migrator is a developer tool.
