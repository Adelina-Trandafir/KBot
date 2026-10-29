# SLICE-00EF-01…03 — Analiza, modelul inițial și importul E-Factura

Istoric din 06.10.2026. 03 înlocuiește propunerea inițială de migrare EF_UM: lista este completată de operator în baza comună și fila doar o citește. Referința din 02 la un job de refresh precede [04](SLICE-00EF-04-server-token.md#slice-00EF-04), care alege reînnoirea la cerere. Modelul furnizorului și stările de instalare sunt inițiale; [28](SLICE-00EF-28-mediu-vps.md) păstrează confirmarea ulterioară pe șablon/baza comună, fără extindere la celelalte baze sau la facturi reale ANAF.

[00EF-01](#slice-00EF-01) · [00EF-02](#slice-00EF-02) · [00EF-03](#slice-00EF-03)

## SLICE-00EF-01 - analysis and plan for the new E-Factura app <a id="slice-00EF-01"></a>

Slice 00EF (operator, 06.10.2026). Documents only: no code, nothing run.

### What changed and why
- Read the raw Access export (`Surse/RawExport`: 250 forms, 357 tables, 559 queries, 116 modules, 81 classes) with a script (`tools/AccessMap/extract.js`) and wrote the relationship maps for the E-Factura area to `docs/access-map/efactura/` (overview, one page per form with every control, tables, queries with SQL, code, navigation, form x table matrix, outside dependencies). Generator: `tools/AccessMap/build-efactura.js`.
- Wrote `PLAN_00EF_EFactura.md`: the operator's decisions, how the Access side works for issued invoices (verified from the export), the target shape (UI app + Flask server that holds the ANAF token + MariaDB), the sub-slice list 00EF-02..09 and five open questions.
- Registered the slice in `KBOT_STATUS.md` and `state/KBOT_STATUS_0000-0009.md` (next to 000T, which also has a letter id).

### Files touched
New: `docs/access-map/efactura/**`, `tools/AccessMap/extract.js`, `tools/AccessMap/build-efactura.js`, `docs/worklog/PLAN_00EF_EFactura.md`, this file. Edited: `docs/worklog/KBOT_STATUS.md`, `docs/worklog/state/KBOT_STATUS_0000-0009.md`.

### Test results
None run. The generator output was read and spot-checked against the raw files (forms, handlers, writes found by the scan, the token functions in `mdl_2025`, `mdl_2026`, the upload/status code in `mdl_EFactura`).

### Unverified / deferred
- The scan reads text: SQL built from variables and forms opened by a name in a variable are not seen; "reads/writes" are leads.
- The maps were generated before the operator's scope decisions, so they still include accounting objects and `_`/`inlucru` copies; the plan says what to ignore.
- Not read: `RES\EF.EXE` (token renewal), the contents of the `Scheme` table, any row data.
- The Access screens were never seen on screen; control positions are from the export (`LayoutCached*`).
- Open questions are in `PLAN_00EF_EFactura.md`.

## SLICE-00EF-02 - E-Factura data model (DDL written, not run) <a id="slice-00EF-02"></a>

Slice 00EF (operator, 06.10.2026, fourth pass). Documents and SQL only: no Python, no VB, nothing run.

### What changed and why
- `sql/00EF_02_efactura_unitate.sql`: nine tables for the unit databases, run on `AVACONT_SURSA` and copied to every unit by the schema sync (`EF_Furnizor`, `EF_Clienti`, `EF_Facturi`, `EF_FacturiLinii`, `EF_Mesaje`, `EF_Primite`, `EF_PrimiteLinii`, `EF_PrimiteNote`, `EF_PrimiteMesaje`). Issued invoices from Access `Factura` / `FacturaC` / `ClientiEF` / `EF_UM` / `UNIT` + `Scheme` row `DPIFV`; received invoices from `EF` / `EFT` / `EFS` / `EFT_C` / `EFT_M`. Accounting columns and tables left out (decision 8 of the plan).
- `sql/00EF_02_efactura_comun.sql`: `EF_UM` (units of measure, one list for all units, like BIC) and `EF_Token` (one row per unit, tokens ENCRYPTED, expiry dates, certificate label, last error) and `EF_TokenStart` (single-use `state` of the authorise step) in `AVACONT_COMUN`.
- `tools/efactura/efactura.env.example` + `efactura.conf`: where the secrets go on the server, `/etc/avacont/efactura.env` (root, 600) read through a systemd `EnvironmentFile`. Template only, no value.
- The plan was updated with the operator's fourth-pass answers: the app never gets a token, only the expiration; warning 7 days before the refresh token runs out; the received-invoice tables and the DDF view's reading of received invoices confirmed.

### Files touched
New: `sql/00EF_02_efactura_unitate.sql`, `sql/00EF_02_efactura_comun.sql`, `tools/efactura/efactura.env.example`, `tools/efactura/efactura.conf`, this file. Edited: `docs/worklog/PLAN_00EF_EFactura.md`, `KBOT_STATUS.md`, `state/KBOT_STATUS_0000-0009.md`.

### Decisions taken here (state them, change if wrong)
- Names `EF_*`, ASCII; column names keep the Access words. `NumarFactura` unique per (series, number) instead of alone.
- Amounts are `decimal`, not `double`. The XML's received file is kept as `LONGBLOB` (file is UTF-8, the house character set is utf8mb3).
- `CuiNormalizat` is written by the server (no `RO`, spaces, dots), so the join with `FX_DDF_Parteneri.CodFiscal` is `EF_Primite.CuiNormalizat = <CodFiscal with the same stripping>`.
- The token table is in `AVACONT_COMUN`, not in the unit database (the refresh job walks one table; `Unitati(DC, CF)` is there).
- `RefreshExpiraLa` is computed by the server (authorisation + `EF_REFRESH_DAYS`, 365 assumed): ANAF's answer as `EF.EXE` reads it carries only `expires_in` for the access token.

### Test results
None. The SQL was NOT executed anywhere (no server access from here); it was written against `MariaDB_Schema/000_DEMO.sql` and `AVACONT_COMUN.sql` (types, collations, key naming) and checked by reading only. MariaDB version not known: the `CHECK` constraint on `EF_Furnizor` needs 10.2.1+ (the existing schema already uses CHECK constraints, e.g. `CK_FX_PDF_SEMNATURI_DOC`).

### Unverified / deferred
- The DDL has not been run; the service account's rights on the new tables are not checked.
- `EF_UM` has no rows: the Access rows are not in the export.
- The meaning of `Factura.Corectata` (a Double in Access) and how `IdFacturaA/SerieFacturaA/NumarFacturaA` are filled: read with the form in 00EF-07.
- The `cryptography` library is not in the project's venv; the VPS is not known to have it (needed in 00EF-03).
- Not decided: whether Access invoices already issued this year are imported or K-BOT starts fresh.

### Change after the first write (06.10.2026)
EF_UM moved from the unit script to the common script: the list (2113 UN/ECE codes) is identical for every unit, so one copy in AVACONT_COMUN, filled by the Migrator tab (00EF-03).

## SLICE-00EF-03 - Migrator, tab «E-Factura» (code written, never run) <a id="slice-00EF-03"></a>

Slice 00EF (operator, 06.10.2026). Operator asked for a new tab in the Migrator for E-Factura (issued + received). **Correction the same day: the units of measure (EF_UM) are NOT migrated** - the operator writes `AVACONT_COMUN.EF_UM` on the server himself. The tab only READS that table, to warn about a line whose unit of measure is not in it.

### What changed and why
- `KBot.Migrator` form now has a `TabControl` (standard control, themed by the engine; chosen by the operator): the server box stays on top and shared; the existing unit list + transfer moved UNCHANGED into the tab «Transfer»; new tab «E-Factura».
- New folder `src/KBot.Migrator/EFactura/`: `EfCui` (tax code without «RO»/spaces: the one rule the server and the KbotForm view must reuse), `EfModels`, `EfTableSpec` (columns, kinds, conversion: nothing is cut or rounded silently; text too long, non-integer, bad date = BLOCKING; rounding = warning), `EfBatchWriter` (multi-row INSERT; a duplicate key leaves the target row as it is), `EfImporter` (Verify = read only; Run = ONE transaction on the unit database).
- What is copied: EF_Furnizor (UNIT first row + Scheme `DPIFV`: T3 series, C2 flag); EF_Clienti; EF_Facturi + EF_FacturiLinii (orphan lines/clients and repeated series+number are blocking); EF_Mesaje (only `cui_unit` = the unit's tax code), EF_Primite, EF_PrimiteLinii, EF_PrimiteNote, EF_PrimiteMesaje (followed by key; credit-note references set in a second step). EFT_O and everything accounting stay behind. Access keys are kept as MariaDB keys.
- The tab keeps two paths in `migrator-settings.json` (not secrets); Importă is enabled only by a clean Verify and only while nothing it looked at changed. FileVersion 1.17.1.0 -> 1.18.0.0.
- `sql/00EF_02_efactura_comun.sql`: EF_UM is defined there (one list for all units); the comment says the operator fills it.

### Files touched
New: `src/KBot.Migrator/EFactura/{EfModels,EfCui,EfTableSpec,EfBatchWriter,EfImporter}.vb`, `src/KBot.Migrator/MigratorForm.EFactura.vb`, this file. Edited: `MigratorForm.Designer.vb` (tab + controls, anchored patch), `MigratorForm.vb` (load/save of paths, busy state, cancel button shared, DC change refreshes the tab), `MigratorSettings.vb`, `KBot.Migrator.vbproj`, the SQL scripts, the plan/status.

### Test results
`dotnet build src/KBot.Migrator` : succeeded, no warnings. NOTHING was run: no window opened, no Access file read, no SQL executed. (The UM-file reader written first was removed after the correction.)

### Unverified / deferred
- The tab and the moved groups were not seen on screen (the Designer was edited by an anchored script, not in Visual Studio): open the form once in the designer; sizes are in the 144-dpi units of the file.
- The importer was never run against real Access files or MariaDB. Assumptions to check on the first Verify: the OLE column `EF.XML` comes back as bytes; `EFT.NC` is -1/1; `Factura.NumarFactura` holds whole numbers; `UNIT.Judetul` is a county code of at most 8 characters; the received store really is one shared `ef_<an>.accdb` filtered by `cui_unit`.
- The SQL (nine unit tables + EF_Token, EF_TokenStart, EF_UM) must be run before Verify can pass.
- Years before the transfer year are not imported (one file per year).
- Not in the help: the Migrator is a developer tool.

Repere unice din registru: harta E-Factura generată conține 21 formulare, 68 tabele și 43 query-uri (subsetul E-Factura, distinct de totalul exportului brut din 01). Instalarea inițială a scriptului unității era cerută pe AVACONT_SURSA, apoi sincronizare SAFE; rândurile EF_UM lipseau din export.
