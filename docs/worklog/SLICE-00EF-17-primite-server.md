# SLICE-00EF-17 / 23 — Facturile primite: serverul și arhivele ANAF

Istoric 09.10.2026. Corecția din 17 înlocuiește condiția reviziei 0 cu existența DDF-ului și păstrează cotele multiple; proba ulterioară pe tokenul de test citește patru mesaje reale fără bază de date. „Nimic apelat” este checkpointul anterior probei, nu starea ei finală. 23 înlocuiește stocarea XML separată cu zip-ul valid, păstrând retragerea pe XML pentru arhiva lipsă/deteriorată. 27 adaugă CPV și importul EFT_O; lipsa citirii operațiilor este checkpointul dinaintea filtrului [35](SLICE-00EF-18-primite-client.md#slice-00EF-35). Nu este confirmată sincronizarea aplicației pe MariaDB prin proba mesajelor.

- [00EF-17](#slice-00EF-17)
- [00EF-23](#slice-00EF-23)
- [00EF-27](#slice-00EF-27)

## SLICE-00EF-17 -- Received e-invoices, server half (parser, sync, routes) <a id="slice-00EF-17"></a>

Operator request, 09.10.2026 (answers to the five questions of the hand-over thread).

### Decisions (operator)
1. Two places show the received invoices: the «E-Factura» view of the DDF (`KbotForm`) AND a separate screen with all of them.
2. The view: tree on the left, months as groups under a root «Toate»; a horizontal bar of views (as in DDF): Linii, PDF (classic one embedded in the XML, or the one drawn by ANAF), Atasamente (only if the XML has some), Mesaje (only if there are some). Right click on an invoice: «Salveaza ca zip» / «Salveaza ca xml».
3. Sync is started from the E-Factura form (`FacturiForm`) -- a button in its bar, N days (max 60), a progress box, no count boxes on success.
4. `TrimiteMesajFactura` (message to the supplier): LATER.
5. Link to a DDF: automatic only when a DDF rev 0 has a partner with the same normalised CUI; also MANUAL, so no direct reliance on CodFiscal. -> new table `EF_PrimiteAsocieri` (`sql/00EF_17_primite_asocieri.sql`, NOT RUN).

### Done in this sub-slice (A: server, no screen)
- `PYTHON/routes/efactura/primite_ubl.py` -- lenient UBL reader (Invoice / CreditNote). Checked on the 3 real files in `Surse/EF_EXEMPLE` (9% and 19% VAT, 11 PaymentMeans, embedded PDF with 100 KB, BillingReference, line notes): numbers, dates, supplier CUI/name/address, totals, lines, embedded file decoded to a valid `%PDF`.
- `primite.py` -- sync (one transaction per message, `EF_Mesaje.IdSol` unique, batches with `ramase` for a progress bar), list (year, month, text, supplier, DDF with `legatura` auto/manual), detail, xml, zip (downloaded again from ANAF), pdf (ANAF service from the saved XML), embedded files, mark read, manual link.
- `primite_routes.py` -- `/api/efactura/primite...` (list at the top of the file); registered in `__init__.py`.

### Rules worth knowing
- Supplier name comes from the XML (`RegistrationName`); ANAF's company service is NOT called (it is mandatory in CIUS-RO).
- `Tip` NC / `Semn` -1 for a credit note with a positive total; `IdPrimitaRef` is set when the referred invoice is already stored.
- A message that cannot be read is skipped and reported; it is not written, so the next sync tries it again.
- `EF_Primite` keeps ONE VAT rate: the rate of the biggest taxable amount; `TVA` is the total VAT in the document currency.
- The «rev 0» condition of the automatic link is NOT applied on the server (the list takes any `iddf`); the client passes the DDF it is on. To confirm what «rev 0» means in `FX_DDF` (`CUAL`?) before B.

### Verified / not verified
- Verified: `py_compile`; parser on the 3 real XML files; parameter counts of the inserts against a fake cursor.
- NOT verified: any SQL against a real MariaDB (REGEXP_REPLACE normalisation of the partner code, the joins), the ANAF calls, a real sync. Server not deployed; the DDL (`00EF_17`) not run; no tests written (not asked).

### Next
B (client: domain, `IEFacturaApi`, the view in `KbotForm`, sync button + progress in `FacturiForm`), C (separate screen), E (help 0000-NN, NOUTATI, status).

### Correction (operator, 09.10.2026)
- «DDF rev 0» = just «a DDF exists»: the automatic link needs nothing more than a `FX_DDF_Parteneri` row with the same CUI. Nothing to add on the server.
- Several VAT rates on one invoice are real: new table `EF_PrimiteTVA` (`sql/00EF_17_primite_tva.sql`, NOT RUN): one row per `TaxSubtotal` (category, rate, base, VAT). The parser returns `cote`; the sync writes them; `GET /primite/<id>` answers `cote`. `EF_Primite.CotaTVA` stays the rate of the biggest base (a summary for the list). Checked on a made-up two-rate XML (19% + 9%) and the 3 real files.
- `00EF_17_primite_asocieri.sql` was run by the operator.

### First real run (operator's test token, unit CUI 50622085, 09.10.2026) -- found and fixed
- The message list answers `{data_creare, cif, id_solicitare, detalii, tip, id}`. ANAF downloads a message by **`id`**, NOT by `id_solicitare` (that is the supplier's upload number = the XML name inside the zip). 00EF-07's `descarca_mesaj` used `id_solicitare` and would have failed ("nu exista inregistrata nici o factura"). Sync and `zip_anaf` now use `id` (stored in `EF_Mesaje.IdIncarcare`); `IdSol` stays `id_solicitare` (unique key). The route `/mesaje/<id>/descarca` takes the message `id`.
- `cif_emitent` / `cif_beneficiar` are not fields of the answer: `anaf_api.lista_mesaje` now reads them from `detalii`.
- 4 received messages in 60 days read correctly (VAT 21%, one invoice with negative amounts = a reversal, kept as `FC`, `Semn` 1). `tools/efactura/proba_primite.py` works with `EF_TOKEN_PROBA` + `--cif-unitate` (no database).

## SLICE-00EF-23 -- the ANAF zip kept instead of the XML (Migrator + sync + server reads) <a id="slice-00EF-23"></a>

Operator request, 09.10.2026: keep the zip files of `C:\AVACONT\efactura` (`fact<EF.id>.zip`: invoice XML + ANAF signature file) on the server; the XML stays only for a record whose zip was NOT found. Same rule for the sync with ANAF.

### What changed and why
- `sql/00EF_23_mesaje_zip.sql` (new) + `00EF_02_efactura_unitate.sql`: `EF_Mesaje.ZipContinut` (longblob). Run in every unit database.
- Migrator, tab «E-Factura»: new editable field «Arhivele ANAF» (folder, default `C:\AVACONT\efactura`, saved in `migrator-settings.json`), `EfImportOptions.ZipFolder`, `EfImporter.DoReceivedArchives`: for every kept message looks for `fact<IdIncarcare>.zip`; a usable zip (opens, holds the invoice XML) is written to `ZipContinut` and `XmlContinut` is set to NULL in the same UPDATE; a missing/damaged zip leaves the XML (warning, never blocking); a zip without `semnatura_*.xml` is kept with a warning. Verify only counts.
- Migrator, Registry: `EfTokenRegistry` reads `HKCU\...\AVACONT\<DC>\Tokens` (Token, RefreshKey, TokenExpiry, Vercon) and Verify prints it masked. Read only; the token is NOT transferred (it must go through the server, encrypted).
- Server `routes/efactura/primite.py`: sync stores the zip (no separate XML); `_xml_of` reads the XML out of the zip when `XmlContinut` is empty; `zip_anaf` serves the stored zip before calling ANAF.
- Help 0000-66 (section «Facturile primite»).

### Files touched
`src/KBot.Migrator/{EFactura/EfTokenRegistry.vb (new), EFactura/EfImporter.vb, EFactura/EfModels.vb, MigratorForm.EFactura.vb, MigratorForm.vb, MigratorForm.Designer.vb, MigratorSettings.vb}`, `PYTHON/routes/efactura/primite.py`, `sql/00EF_23_mesaje_zip.sql`, `sql/00EF_02_efactura_unitate.sql`, help `contabil/efactura/index.md`.

### Test results
`dotnet build src/KBot.Migrator`: succeeded, 0 warnings. `PYTHON/routes/efactura/primite.py`: parses (`ast`). Nothing run: no Access file read, no SQL run, no window seen, the server not redeployed.

### Unverified / deferred
- New Designer controls were added by script and never opened in Visual Studio.
- `EF.id` = zip name was checked on 1 zip's contents and the folder listing (307 `fact*.zip`, 4.5 MB), not against the Access table.
- Rows synced before this slice keep their XML and have no zip (the zip route then asks ANAF as before).
- Issued invoices' zips are not handled (only `EF_Mesaje`).
- Token transfer from the registry to `EF_Token` is not done (needs a server route).

## SLICE-00EF-27 - E-Factura received invoices: CPV column, «Continut» view, tree padding, CPV and EFT_O at migration (15.10.2026) <a id="slice-00EF-27"></a>

Slice number: the operator asked (15.10.2026) that all E-Factura work sits in an EF slice, not sliceless. 00EF-24 is the precision script (`00EF_24_*`), 00EF-25 / 00EF-26 are already cited in the help and in `PrimiteMesajePage` / `PrimiteObservatiiPage` without a worklog of their own, so this work takes the next free number, 00EF-27. SQL scripts keep the slice number: `00EF_27_*`.

### What changed and why
Operator request (no slice number):
1. **Tree**: the received-invoices tree gets +4 px on its right padding (`PaddingTreeEnd` 4 -> 8, set in `PrimiteView.Designer.vb`).
2. **CPV stored**: new column `EF_PrimiteLinii.Cpv` (`sql/00EF_27_primite_linii_cpv.sql`, run on `AVACONT_SURSA` then sync to the unit databases). `primite_ubl._cpv` reads `Item/CommodityClassification/ItemClassificationCode` with `listID="STI"`; the sync writes it for every NEW invoice. `NULL` = line stored before the column (not read); `''` = invoice read, line has no STI code.
3. **View «Linii» -> «Continut»** (display text only; object names unchanged): the «Explicatie» column is replaced by «CPV» (132 px = 60% of the old 220), «Denumire» takes the rest (260 -> 348). A line with `Cpv = ''` is painted with the theme's `ErrorColor` (dark red in Classic/Modern, light red in Dark so it stays readable).
4. **Migration fills the CPV of the old invoices** (`KBot.Migrator`, E-Factura tab): after the references pass, `EfImporter.DoReceivedCpv` reads `EF.XML` of every selected message (an ANAF zip; a bare XML is also accepted), takes the invoice XML out of it (`EfCpvReader`, same rule as the server: STI), matches XML lines to `EFS` rows by invoice + line number (`EFS.ID` = XML line `ID`) and runs `UPDATE EF_PrimiteLinii SET Cpv = @c WHERE IdLinie = @i AND Cpv IS NULL`: a value already there stays; an unreadable zip, a repeated or unmatched line number leaves NULL (warnings, limited). A missing `Cpv` column is BLOCKING and names the script. It runs in Verify too (reads the zips, writes nothing); importing again fills lines migrated earlier.

5. **EFT_O is migrated** (operator, 15.10.2026: it tells whether a received invoice was processed). New table `EF_PrimiteOperatii` (`sql/00EF_27_primite_operatii.sql`, NOT yet run): `IdOperatiePrimita` (= IDEFTO), `IdPrimita` (FK, cascade), `IdOperatie` (Access `Oper.IdOperatie`, no FK: operations are not in MariaDB yet), `IdClsf` (= `Clasificatii.IDClsf`, the FX rule of slice 0080-01: NO copy of the Access id), `IdUnitate`, `Sursa`, `Suma` DECIMAL(20,4), `DataAdaugare` (= DTQ). `EfImporter.ReceivedOperationSpec` copies the rows of the selected invoices (not filtered by IdUnitate: 75 / 77 are the 02A / 02E sources of one unit). The conversion is done IN THE MIGRATOR, as for FX (`PYTHON/utils/clsf_pair.py`, `Transfer/TransferRunner`): Access `IdClsf` 0 / NULL -> NULL; otherwise `Clasificatii` of the target is searched on (IdClsfAcc = Access id, IdUnitate = the row's own IdUnitate) and must give EXACTLY ONE row, whose `IDClsf` is written. Zero or several matches (or no unit) are BLOCKING, one finding per pair with the row count, and nothing is written (Verify shows them too). The nomenclator must therefore be in the target BEFORE the E-Factura import (the main transfer does Clasificatii). No FK to Clasificatii (see the DDL comment). Not done: nothing in the API or the screens reads the table yet (no «prelucrata» flag in the received list).

Help: `contabil/efactura/index.md` (two sections, tag `00EF-27`).

### Files touched
`sql/00EF_27_primite_linii_cpv.sql` (new), `PYTHON/routes/efactura/primite_ubl.py`, `PYTHON/routes/efactura/primite.py`, `src/KBot.Domain/EFacturaPrimite.vb`, `src/KBot.Api/ApiClient.EFactura.Primite.vb`, `src/KBot.Migrator/EFactura/EfImporter.vb`, `src/KBot.Migrator/EFactura/EfCpvReader.vb` (new), `sql/00EF_27_primite_operatii.sql` (new), `src/KBot.EFactura/Views/Primite/PrimiteView.vb`, `PrimiteView.Designer.vb`, `PrimiteLiniiPage.vb`, `PrimiteLiniiPage.Designer.vb`, `src/KBot.App/HelpContent/contabil/efactura/index.md`.

### Test results
- `dotnet build src/KBot.EFactura` and `src/KBot.Migrator`: 0 warnings, 0 errors.
- `primite_ubl.parse` on a two-line UBL sample (one line with STI code, one without): `('1','72415000-2')`, `('2','')`.
- No tests added or run (operator rule); not seen on screen; the migrator was not run.

### Unverified / deferred
- **The DDL must be run before the new server code is used**: until `Cpv` exists, the received-invoice sync and the invoice detail answer «Tabelele E-Factura ale unitatii lipsesc sau sunt vechi» (503, names the script).
- The migration CPV pass never ran on a real Access file. Assumption: `EFS.ID` is the XML line `ID` (the field the server stores as NrLinie); the run report says how many lines did not match.
- Invoices already in MariaDB that did NOT come through the migrator keep NULL (not red) until the import is repeated.
- Only `listID="STI"` counts as CPV; a code with another or missing listID is treated as absent.
- The `Check-Help.ps1` run reports missing slice tags in `tutorials/` (not touched here); none in `efactura/index.md`.
