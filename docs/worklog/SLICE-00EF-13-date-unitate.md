# SLICE-00EF-12…14 — Conturile și datele unității

12 introduce conturile per bază; 13 le mută, împreună cu datele emitentului, în baza comună și scoate vederea Vânzător. 14 redenumește Unitati_Detalii în Unitati_Date și înlocuiește preluarea unică ANAF cu preluare repetabilă/automată la nume gol. „Cont emitent nu citește lista” este amendat în [16](SLICE-00EF-08-ecran-facturi-emise.md#slice-00EF-16). Scripturile 00EF-06/12 sunt înlocuite, nu sunt pași de rulat în plus. [28](SLICE-00EF-28-mediu-vps.md) păstrează confirmarea ulterioară limitată a mediului. 12 și 14 nu au rânduri proprii în registru; această lipsă nu este umplută printr-o stare inventată.

- [00EF-12](#slice-00EF-12)
- [00EF-13](#slice-00EF-13)
- [00EF-14](#slice-00EF-14)

## SLICE-00EF-12 - the unit's own bank accounts for the issued invoices (code written, never run) <a id="slice-00EF-12"></a>

Slice 00EF (operator, 07.10.2026). Until now the issuing unit's account lived only as free text on each invoice (`EF_Facturi.ContPlata`); the combo «Cont emitent» offered only the accounts of earlier invoices. Operator asked for (1) a table on the source database with the unit's IBANs and the bank deduced from the account, (2) a simple window with the theme and the custom controls, a grid where the user types and deletes accounts, (3) the Migrator to carry `Factura`, `FacturaC`, `ClientiEF` (if it does not already).

These are the accounts of the CURRENT UNIT (the issuer), not of the partners it pays (those live on the partners; `EF_Clienti.Cont` is a customer's account).

### What changed and why
- **Table** `EF_FurnizorConturi` (`sql/00EF_12_efactura_conturi.sql`, for `AVACONT_SURSA`, then the schema sync to every unit): `IdCont`, `Cont` varchar(34) UNIQUE, `Banca` varchar(255) NULL, `DataAdaugare`, `DataModificare`. `Banca` is written by the server on every save from characters 5-8 of the IBAN looked up in `AVACONT_COMUN.BIC` (the rule `_bank_for` already used by the XML); NULL when the code is not in BIC. Nobody types it.
- **Server** (`PYTHON/routes/efactura/`): `GET|PUT /api/efactura/furnizor/conturi`. PUT replaces the whole list in one transaction (`{ conturi: [{ Cont }] }`): spaces out, upper case, shape `AA99...` + the ISO 13616 mod-97 check, no duplicates, at most 50, an empty list removes all. Audit `EF_FURNIZOR_CONTURI`. `_unit()` takes the SQL file name for the «tables missing» message (the accounts table asks for 00EF_12, the others keep their files).
- **Client**: `EFacturaCont` (Domain), `GetConturiAsync` / `SaveConturiAsync` on `IEFacturaApi` + `ApiClient.EFactura.Conturi.vb`.
- **Window** `ConturiForm` (KBot.EFactura/Views; KBotThemedForm, KBotCaptionBar, KBotBusyBar, KBotNotice, KBotToolTip, `KBotDataView`): columns «Cont (IBAN)» (typed), «Banca (dedusă din cont)» (read only; «— banca nu este în listă —» when unknown), «✕». Buttons «Cont nou», «Salvează», «Închide»; closing with unsaved rows asks. Opened as a dialog from the new button «Conturile unității…» in the «Vânzător» tab of `FacturiForm` (the only change there: the button, its handler, its enabled state).
- **Help** 0000-56: new topics `contabil.efactura.conturi` (this window) and `contabil.efactura.token` (getting / renewing the ANAF token, moved out of `contabil.efactura`); help only, no tutorial.
- **Migrator (item 3): nothing to change.** `KBot.Migrator/EFactura` (slice 00EF-03) already copies `ClientiEF -> EF_Clienti`, `Factura -> EF_Facturi`, `FacturaC -> EF_FacturiLinii`, every column of the three Access tables except `Factura.IdOperatie` (accounting, dropped on purpose; recorded in `sql/00EF_02_efactura_unitate.sql`). There is no Access table of the unit's accounts, so there is nothing to carry into `EF_FurnizorConturi`; no migrator slice was opened.

### Files touched
New: `sql/00EF_12_efactura_conturi.sql`, `src/KBot.Domain/EFacturaConturi.vb`, `src/KBot.Api/ApiClient.EFactura.Conturi.vb`, `src/KBot.EFactura/Views/ConturiForm.vb`, `ConturiForm.Designer.vb`, this file, [0000-56](SLICE-0000-54-ajutor-efactura.md#slice-0000-56). Edited: `PYTHON/routes/efactura/{facturi_store,facturi,factura_routes}.py`, `src/KBot.Api/IEFacturaApi.vb`, `src/KBot.EFactura/Views/FacturiForm.Designer.vb` + `FacturiForm.Furnizor.vb` (button only), `HelpContent/contabil/efactura/index.md`, status files.

### Test results
`python -m py_compile` of the three Python files: clean. (The build below was run once, before the operator said no more builds, tests or git.) The new VB files and the Api / Domain changes show no compiler error in `dotnet build src/KBot.EFactura`; the project as a whole did NOT finish building at the time because another change in progress in `FacturiForm` (`FacturiForm.Arbore.vb`, not part of this slice) calls members that do not exist yet (`SelectView`, `SendCurrentAsync`, `VerifyCurrentAsync`, `StornoCurrentAsync`, `RefreshPdfView`, `ApplyTreeLook`). Nothing was run: no test, no window opened, no SQL executed, no route called.

### Unverified / deferred
- Full clean build once the `FacturiForm.Arbore` work compiles.
- The DDL is not run; run it on `AVACONT_SURSA`, then the schema sync (the operator does it). Until then the window shows the «Tabelele E-Factura ale unității lipsesc» message naming the file.
- The window was never seen on screen (Designer written by hand, 96 dpi); the grid behaviour (typing, «✕», the empty bank cell) is unverified.
- The combo «Cont emitent» on an invoice does NOT yet read this list (it still offers the accounts of earlier invoices): left alone on request («fără să umbli la altceva»). Natural next step.
- The mod-97 check refuses an account that is not a valid IBAN; a unit with an old non-IBAN account number cannot save it here.

## SLICE-00EF-13 - «Date Unitate»: the issuer leaves the invoice window and moves to AVACONT_COMUN (code written, NOT built, never run) <a id="slice-00EF-13"></a>

Slice 00EF (operator, 07.10.2026, five points). Everything below is on code only: no build, no test, no SQL run, no window opened (standing instruction: no builds or tests unless asked).

### What changed and why
1. **Navbar icon.** The caption bar of `FacturiForm` shows a new icon: the solution icon (blue rounded square) with the letter **E** instead of the K. Made from `kbot_64.png` itself: the K pixels were painted over with the square's own colour and an E (Segoe UI Bold, same height and centre) drawn in white. New resource `kbot_e_64` (`Resources/kbot_e_64.png`, `My Project/Resources.resx` + `Resources.Designer.vb`); `FacturiForm.Designer.vb` points `capBar.IconImage` at it.
2. **«Emitent IBAN» moved to «Generale»** (`VanzareGeneralePage`): a new row «Cont emitent (IBAN) *» right under «Ref. comandă (BT-13)» (label `lblContPlataT`, combo `cmbContPlata`, same tooltip as before). `FacturiForm.Pagini.vb` binds it from `pgGenerale`; the «contul lipsește» refusal now sends the operator to «Generale».
3. **«Vânzător» is no longer a view; it is the stand-alone form `DateUnitateForm`** (`Forms/DateUnitateForm.vb` + `.Designer.vb`; `KBotThemedForm`, every control declared in the Designer, 96 dpi):
   - the account field and the button «Conturile unității…» are gone from the form body (caption «Date Unitate», icon = the K, `kbot_64`; no extra button on this form);
   - **Denumire** (Dock = Fill) and **Cod fiscal** (right aligned) on one row (panel `pnlDenCf`: name Fill, label Right, field Right); **Județ** and **Oraș** on one row; **Adresă**; **Telefon** and **E-mail** on one row; **Seria** and **Primul număr** on one row; «Contact» (`Reprezentant`) removed everywhere (form, PDF line, wire, Domain, server, Migrator, table);
   - **Telefon** has the input mask `0000.000.000` (the operator wrote `####.###.###`; `#` is `0` in the mask language of `KBotInputMask`). The mask engine existed only in `KBotComboBox`; **`KBotTextField` now has `InputMask`** (same language, same `KBotInputMask`; typing, Backspace, Delete, paste and `Text` set from code go through it; documented in `KBotTextField.md`). An incomplete number (not ten digits, not empty) is refused on save;
   - **Seria and Primul număr are read only as soon as the unit has an issued invoice**: the server answers `are_facturi` and refuses (409 `SERIE_BLOCATA`) a PUT that changes them; the window shows them blocked (`EFacturaFurnizor.AreFacturi`);
   - **«Preia de la ANAF»**: takes the name and address from ANAF by the tax code the unit has in `AVACONT_COMUN.Unitati.CF` (the existing `routes/inregistrare/anaf.py` lookup) and writes them at once; **works once**: the new column `Unitati_Detalii.AnafPreluat` (+ `DataAnafPreluat`) is set, the server answers 409 `ANAF_DEJA_PRELUAT` afterwards, the button is disabled (`EFacturaFurnizor.AnafPreluat`). A unit with no row yet gets one from it (name + address from ANAF, tax code from `Unitati`). New route `POST /api/efactura/furnizor/anaf`, client `TakeFurnizorFromAnafAsync`;
   - `flowVanzator` → panel (`pnlJos`); **«Renunță» = «Ieșire»** (Dock = Left; asks when something is unsaved), **«Salvează vânzătorul» = «Salvează»** (Dock = Right); «Preia de la ANAF» sits next to «Ieșire».
   - **Entry point** (operator's correction, 07.10.2026: NO button in the bottom bar): the caption bar of `FacturiForm` gets an extra button (`ShowOptionsButton`, image = the K `kbot_64`, untinted) that drops a custom popup `mnuUnitate` (`KBotDropDownMenu`) with two rows: **«Conturi Unitate»** (opens `ConturiForm`) and **«Date Unitate»** (opens `DateUnitateForm`; icons: the card and the K). With no issuer row the window opens by itself after the load, and «Adăugare» opens it too. What the dialog saved becomes the issuer `FacturiForm` keeps (`DateUnitateForm.Furnizor`).
   - Removed: `VanzareVanzatorPage` (+ designer + resx), the nav item «Vânzător», the edit/lock mode of the issuer in `FacturiForm.Furnizor.vb` (`Modifică`/`Renunță`, `FurnizorUnsaved`, `ConfirmFurnizorLeaveAsync`). The county list moved out of `FacturiForm.Client.vb` into `Forms/CountyList.vb` (shared by both windows).
4. **`EF_Furnizor` → `AVACONT_COMUN.Unitati_Detalii`** (one row per unit, key `DC` = `Unitati.DC`, FK → `Unitati`), no table per unit any more. 
5. **`EF_FurnizorConturi` → `AVACONT_COMUN.Unitati_Conturi`** (`IdCont`, `DC`, `Cont`, `Banca`, unique `(DC, Cont)`, FK → `Unitati`).
   - DDL: **`sql/00EF_13_unitati_detalii.sql`** (run on `AVACONT_COMUN`; includes the optional copy from a unit that already had the old tables). `00EF_02` no longer creates `EF_Furnizor` (eight tables now); `00EF_06` and `00EF_12` are reduced to «superseded, do not run».
   - Server: `facturi_store.py` names `AVACONT_COMUN.Unitati_Detalii` / `Unitati_Conturi` with a `dc` parameter (from the session) and reads/locks them through the unit connection in the same transaction as the invoice tables; `facturi.py` passes `dc` everywhere (`_need_furnizor`, numbering, storno, XML material), answers `are_facturi` + `AnafPreluat`, adds `furnizor_anaf`; `trimitere.py` passes `dc`; the «tables missing» message now names `00EF_02` + `00EF_13`.
   - Migrator (`EfImporter.DoFurnizor`): the issuer is written into `Unitati_Detalii` (`INSERT IGNORE`, key = the unit database name) through the unit connection and its transaction; `Reprezentant` no longer read.

### Files touched
New: `sql/00EF_13_unitati_detalii.sql`, `src/KBot.EFactura/Forms/DateUnitateForm.vb`, `DateUnitateForm.Designer.vb`, `Forms/CountyList.vb`, `Resources/kbot_e_64.png`, `Resources/credit_card.png`, this file, [0000-58](SLICE-0000-54-ajutor-efactura.md#slice-0000-58). Edited: `PYTHON/routes/efactura/{facturi_store,facturi,factura_routes,trimitere,__init__,ubl,validare}.py` (the last two: a docstring), `sql/00EF_02_*`, `00EF_06_*`, `00EF_12_*`, `src/KBot.Domain/EFacturaFacturi.vb`, `src/KBot.Api/{IEFacturaApi,ApiClient.EFactura.Facturi}.vb`, `src/KBot.Controls/TextField/KBotTextField.vb` + `.md`, `src/KBot.EFactura/{FacturiForm.vb, .Designer.vb, .Furnizor.vb, .Pagini.vb, .Pdf.vb, .Client.vb}`, `Views/Vanzare/VanzareGeneralePage.Designer.vb`, `Views/Vanzare/FacturaPdf.vb`, `Forms/ConturiForm.Designer.vb` (one sentence), `My Project/Resources.{resx,Designer.vb}`, `src/KBot.Migrator/EFactura/EfImporter.vb`, help (see 0000-58), status files. Deleted: `Views/Vanzare/VanzareVanzatorPage.{vb,Designer.vb,resx}`.

### Test results
`python -m py_compile` of `facturi.py`, `facturi_store.py`, `factura_routes.py`, `trimitere.py`: clean. **Nothing else was run: not built, no test written or run, no SQL executed, no route called, no window opened.** The VB was checked only by reading.

### Unverified / deferred
- **Build**: never compiled. Likely places for a complaint: `DateUnitateForm.Designer.vb` (written by hand: `capBar.ShowOptionsButton` / `OptionButtonImage`, `txtTel.InputMask`), the `Handles capBar.OptionButtonClick` (a Custom Event), `KBotTextField` (new `KeyPress` handler and `Text` setter), `EfImporter.WriteDetails`.
- **Window never seen**: the row layout of `DateUnitateForm` (96 dpi, the 55 / 45 split, the right-aligned «Cod fiscal»), the title-bar card button, the E icon at 64 px. The E was drawn programmatically; the operator may prefer another weight.
- **Server**: not deployed. **DDL not run**: `sql/00EF_13_unitati_detalii.sql` on `AVACONT_COMUN` first; until then the window shows the «tabelele … lipsesc» message. The unit connection must be allowed to read and WRITE `AVACONT_COMUN.Unitati_Detalii` / `Unitati_Conturi` (same account as the other cross-schema reads, but writing is unverified: if the grant is read only, a 1142 error comes back).
- The ANAF lookup fills only name and address (the answer of `date_generale` has no county / city split); county, city, phone, e-mail, series stay typed.
- The ANAF button needs `AVACONT_COMUN.Unitati.CF` to be the unit's real tax code.
- The combo «Cont emitent» of an invoice still offers the accounts of earlier invoices, not the unit's list `Unitati_Conturi` (unchanged; natural next step).
- Phone mask: a landline with an extension or a foreign number does not fit `0000.000.000`.
- Tests that would break: none known (no test references the removed members); none were looked at in `tests\`.

## SLICE-00EF-14 — Unitati_Date, ANAF reluabil si automat <a id="slice-00EF-14"></a>

### What changed and why
- Table `AVACONT_COMUN.Unitati_Detalii` renamed `Unitati_Date` (operator's name); `Unitati_Conturi` unchanged. Both were already moved out of the unit databases by 00EF-13; no `FX_Furnizor` / `EF_Furnizor` table is created anywhere any more (only the commented copy block at the bottom of `sql/00EF_13_unitati_detalii.sql` names the old ones).
- The ANAF take is no longer one-time: `AnafPreluat` removed (column, server, wire DTO, `EFacturaFurnizor`), `ANAF_DEJA_PRELUAT` gone; `DataAnafPreluat` -> `DataAnaf` (last take).
- `DateUnitateForm`: on opening, an empty name runs ANAF by itself (tax code from `Unitati.CF`, no question); the button «Preia de la ANAF» stays always enabled and asks before it overwrites name/county/city/address.
- The take now also fills county (ISO code) and city: `anaf._fields` reads `adresa_sediu_social` (`scod_JudetAuto`, `sdenumire_Judet`, `sdenumire_Localitate`); `routes/efactura/adresa_anaf.py` maps it, with the one-line address as fallback. `furnizor_mark_anaf` keeps the old county/city when ANAF gives none.
- Help (`contabil/efactura/index.md`, «Date unitate») updated, tag `00EF-14`.

### Files touched
`sql/00EF_02/06/12/13*.sql`, `PYTHON/routes/efactura/{facturi,facturi_store,factura_routes,validare,ubl,__init__}.py`, new `adresa_anaf.py`, `PYTHON/routes/inregistrare/anaf.py`, `src/KBot.Api/ApiClient.EFactura.Facturi.vb`, `src/KBot.Domain/EFacturaFacturi.vb`, `src/KBot.EFactura/Forms/DateUnitateForm.vb`, `src/KBot.Migrator/EFactura/EfImporter.vb`, help `index.md`.

### Test results
KBot.EFactura builds, 0 warnings. `adresa_anaf.county_and_city` checked by hand on three samples (PH/Ploiesti, B/SECTOR1, address fallback). No test suites run (not asked).

### Unverified / deferred
- The ANAF field names `scod_JudetAuto` / `sdenumire_Judet` / `sdenumire_Localitate` are from ANAF's v9 documentation, NOT seen in a live answer from this repo; the address-string fallback covers a miss. Check once against a real CUI.
- The SQL is NOT run anywhere (table never created live), so the rename needs no migration; the file name `00EF_13_unitati_detalii.sql` was kept.
- Not done: `docs/worklog/KBOT_STATUS.md` / `state/` update, commit.
