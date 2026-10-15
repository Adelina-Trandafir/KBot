# SLICE-00EF-27 - E-Factura received invoices: CPV column, «Continut» view, tree padding, CPV and EFT_O at migration (15.10.2026)

Slice number: the operator asked (15.10.2026) that all E-Factura work sits in an EF slice, not sliceless. 00EF-24 is the precision
script (`00EF_24_*`), 00EF-25 / 00EF-26 are already cited in the help and in `PrimiteMesajePage` / `PrimiteObservatiiPage` without a
worklog of their own, so this work takes the next free number, 00EF-27. SQL scripts keep the slice number: `00EF_27_*`.

## What changed and why
Operator request (no slice number):
1. **Tree**: the received-invoices tree gets +4 px on its right padding (`PaddingTreeEnd` 4 -> 8, set in `PrimiteView.Designer.vb`).
2. **CPV stored**: new column `EF_PrimiteLinii.Cpv` (`sql/00EF_27_primite_linii_cpv.sql`, run on `AVACONT_SURSA` then sync to the
   unit databases). `primite_ubl._cpv` reads `Item/CommodityClassification/ItemClassificationCode` with `listID="STI"`; the sync
   writes it for every NEW invoice. `NULL` = line stored before the column (not read); `''` = invoice read, line has no STI code.
3. **View «Linii» -> «Continut»** (display text only; object names unchanged): the «Explicatie» column is replaced by «CPV»
   (132 px = 60% of the old 220), «Denumire» takes the rest (260 -> 348). A line with `Cpv = ''` is painted with the theme's
   `ErrorColor` (dark red in Classic/Modern, light red in Dark so it stays readable).
4. **Migration fills the CPV of the old invoices** (`KBot.Migrator`, E-Factura tab): after the references pass, `EfImporter.DoReceivedCpv`
   reads `EF.XML` of every selected message (an ANAF zip; a bare XML is also accepted), takes the invoice XML out of it
   (`EfCpvReader`, same rule as the server: STI), matches XML lines to `EFS` rows by invoice + line number (`EFS.ID` = XML line
   `ID`) and runs `UPDATE EF_PrimiteLinii SET Cpv = @c WHERE IdLinie = @i AND Cpv IS NULL`: a value already there stays; an
   unreadable zip, a repeated or unmatched line number leaves NULL (warnings, limited). A missing `Cpv` column is BLOCKING and
   names the script. It runs in Verify too (reads the zips, writes nothing); importing again fills lines migrated earlier.

5. **EFT_O is migrated** (operator, 15.10.2026: it tells whether a received invoice was processed). New table
   `EF_PrimiteOperatii` (`sql/00EF_27_primite_operatii.sql`, NOT yet run): `IdOperatiePrimita` (= IDEFTO), `IdPrimita` (FK, cascade),
   `IdOperatie` (Access `Oper.IdOperatie`, no FK: operations are not in MariaDB yet), `IdClsf` (= `Clasificatii.IDClsf`, the FX rule
   of slice 0080-01: NO copy of the Access id), `IdUnitate`, `Sursa`, `Suma` DECIMAL(20,4), `DataAdaugare` (= DTQ).
   `EfImporter.ReceivedOperationSpec` copies the rows of the selected invoices (not filtered by IdUnitate: 75 / 77 are the 02A / 02E
   sources of one unit). The conversion is done IN THE MIGRATOR, as for FX (`PYTHON/utils/clsf_pair.py`, `Transfer/TransferRunner`):
   Access `IdClsf` 0 / NULL -> NULL; otherwise `Clasificatii` of the target is searched on (IdClsfAcc = Access id, IdUnitate = the
   row's own IdUnitate) and must give EXACTLY ONE row, whose `IDClsf` is written. Zero or several matches (or no unit) are BLOCKING,
   one finding per pair with the row count, and nothing is written (Verify shows them too). The nomenclator must therefore be in the
   target BEFORE the E-Factura import (the main transfer does Clasificatii). No FK to Clasificatii (see the DDL comment).
   Not done: nothing in the API or the screens reads the table yet (no «prelucrata» flag in the received list).

Help: `contabil/efactura/index.md` (two sections, tag `00EF-27`).

## Files touched
`sql/00EF_27_primite_linii_cpv.sql` (new), `PYTHON/routes/efactura/primite_ubl.py`, `PYTHON/routes/efactura/primite.py`,
`src/KBot.Domain/EFacturaPrimite.vb`, `src/KBot.Api/ApiClient.EFactura.Primite.vb`,
`src/KBot.Migrator/EFactura/EfImporter.vb`, `src/KBot.Migrator/EFactura/EfCpvReader.vb` (new), `sql/00EF_27_primite_operatii.sql` (new),
`src/KBot.EFactura/Views/Primite/PrimiteView.vb`, `PrimiteView.Designer.vb`, `PrimiteLiniiPage.vb`, `PrimiteLiniiPage.Designer.vb`,
`src/KBot.App/HelpContent/contabil/efactura/index.md`.

## Test results
- `dotnet build src/KBot.EFactura` and `src/KBot.Migrator`: 0 warnings, 0 errors.
- `primite_ubl.parse` on a two-line UBL sample (one line with STI code, one without): `('1','72415000-2')`, `('2','')`.
- No tests added or run (operator rule); not seen on screen; the migrator was not run.

## Unverified / deferred
- **The DDL must be run before the new server code is used**: until `Cpv` exists, the received-invoice sync and the invoice
  detail answer «Tabelele E-Factura ale unitatii lipsesc sau sunt vechi» (503, names the script).
- The migration CPV pass never ran on a real Access file. Assumption: `EFS.ID` is the XML line `ID` (the field the server stores
  as NrLinie); the run report says how many lines did not match.
- Invoices already in MariaDB that did NOT come through the migrator keep NULL (not red) until the import is repeated.
- Only `listID="STI"` counts as CPV; a code with another or missing listID is treated as absent.
- The `Check-Help.ps1` run reports missing slice tags in `tutorials/` (not touched here); none in `efactura/index.md`.
