# SLICE-00EF-23 -- the ANAF zip kept instead of the XML (Migrator + sync + server reads)

Operator request, 09.10.2026: keep the zip files of `C:\AVACONT\efactura` (`fact<EF.id>.zip`: invoice XML + ANAF signature file) on the server; the XML stays only for a record whose zip was NOT found. Same rule for the sync with ANAF.

## What changed and why
- `sql/00EF_23_mesaje_zip.sql` (new) + `00EF_02_efactura_unitate.sql`: `EF_Mesaje.ZipContinut` (longblob). Run in every unit database.
- Migrator, tab «E-Factura»: new editable field «Arhivele ANAF» (folder, default `C:\AVACONT\efactura`, saved in `migrator-settings.json`), `EfImportOptions.ZipFolder`, `EfImporter.DoReceivedArchives`: for every kept message looks for `fact<IdIncarcare>.zip`; a usable zip (opens, holds the invoice XML) is written to `ZipContinut` and `XmlContinut` is set to NULL in the same UPDATE; a missing/damaged zip leaves the XML (warning, never blocking); a zip without `semnatura_*.xml` is kept with a warning. Verify only counts.
- Migrator, Registry: `EfTokenRegistry` reads `HKCU\...\AVACONT\<DC>\Tokens` (Token, RefreshKey, TokenExpiry, Vercon) and Verify prints it masked. Read only; the token is NOT transferred (it must go through the server, encrypted).
- Server `routes/efactura/primite.py`: sync stores the zip (no separate XML); `_xml_of` reads the XML out of the zip when `XmlContinut` is empty; `zip_anaf` serves the stored zip before calling ANAF.
- Help 0000-66 (section «Facturile primite»).

## Files touched
`src/KBot.Migrator/{EFactura/EfTokenRegistry.vb (new), EFactura/EfImporter.vb, EFactura/EfModels.vb, MigratorForm.EFactura.vb, MigratorForm.vb, MigratorForm.Designer.vb, MigratorSettings.vb}`, `PYTHON/routes/efactura/primite.py`, `sql/00EF_23_mesaje_zip.sql`, `sql/00EF_02_efactura_unitate.sql`, help `contabil/efactura/index.md`.

## Test results
`dotnet build src/KBot.Migrator`: succeeded, 0 warnings. `PYTHON/routes/efactura/primite.py`: parses (`ast`). Nothing run: no Access file read, no SQL run, no window seen, the server not redeployed.

## Unverified / deferred
- New Designer controls were added by script and never opened in Visual Studio.
- `EF.id` = zip name was checked on 1 zip's contents and the folder listing (307 `fact*.zip`, 4.5 MB), not against the Access table.
- Rows synced before this slice keep their XML and have no zip (the zip route then asks ANAF as before).
- Issued invoices' zips are not handled (only `EF_Mesaje`).
- Token transfer from the registry to `EF_Token` is not done (needs a server route).
