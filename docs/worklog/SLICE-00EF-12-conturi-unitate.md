# SLICE-00EF-12 - the unit's own bank accounts for the issued invoices (code written, never run)

Slice 00EF (operator, 07.10.2026). Until now the issuing unit's account lived only as free text on each invoice (`EF_Facturi.ContPlata`); the combo «Cont emitent» offered only the accounts of earlier invoices. Operator asked for (1) a table on the source database with the unit's IBANs and the bank deduced from the account, (2) a simple window with the theme and the custom controls, a grid where the user types and deletes accounts, (3) the Migrator to carry `Factura`, `FacturaC`, `ClientiEF` (if it does not already).

These are the accounts of the CURRENT UNIT (the issuer), not of the partners it pays (those live on the partners; `EF_Clienti.Cont` is a customer's account).

## What changed and why
- **Table** `EF_FurnizorConturi` (`sql/00EF_12_efactura_conturi.sql`, for `AVACONT_SURSA`, then the schema sync to every unit): `IdCont`, `Cont` varchar(34) UNIQUE, `Banca` varchar(255) NULL, `DataAdaugare`, `DataModificare`. `Banca` is written by the server on every save from characters 5-8 of the IBAN looked up in `AVACONT_COMUN.BIC` (the rule `_bank_for` already used by the XML); NULL when the code is not in BIC. Nobody types it.
- **Server** (`PYTHON/routes/efactura/`): `GET|PUT /api/efactura/furnizor/conturi`. PUT replaces the whole list in one transaction (`{ conturi: [{ Cont }] }`): spaces out, upper case, shape `AA99...` + the ISO 13616 mod-97 check, no duplicates, at most 50, an empty list removes all. Audit `EF_FURNIZOR_CONTURI`. `_unit()` takes the SQL file name for the «tables missing» message (the accounts table asks for 00EF_12, the others keep their files).
- **Client**: `EFacturaCont` (Domain), `GetConturiAsync` / `SaveConturiAsync` on `IEFacturaApi` + `ApiClient.EFactura.Conturi.vb`.
- **Window** `ConturiForm` (KBot.EFactura/Views; KBotThemedForm, KBotCaptionBar, KBotBusyBar, KBotNotice, KBotToolTip, `KBotDataView`): columns «Cont (IBAN)» (typed), «Banca (dedusă din cont)» (read only; «— banca nu este în listă —» when unknown), «✕». Buttons «Cont nou», «Salvează», «Închide»; closing with unsaved rows asks. Opened as a dialog from the new button «Conturile unității…» in the «Vânzător» tab of `FacturiForm` (the only change there: the button, its handler, its enabled state).
- **Help** 0000-56: new topics `contabil.efactura.conturi` (this window) and `contabil.efactura.token` (getting / renewing the ANAF token, moved out of `contabil.efactura`); help only, no tutorial.
- **Migrator (item 3): nothing to change.** `KBot.Migrator/EFactura` (slice 00EF-03) already copies `ClientiEF -> EF_Clienti`, `Factura -> EF_Facturi`, `FacturaC -> EF_FacturiLinii`, every column of the three Access tables except `Factura.IdOperatie` (accounting, dropped on purpose; recorded in `sql/00EF_02_efactura_unitate.sql`). There is no Access table of the unit's accounts, so there is nothing to carry into `EF_FurnizorConturi`; no migrator slice was opened.

## Files touched
New: `sql/00EF_12_efactura_conturi.sql`, `src/KBot.Domain/EFacturaConturi.vb`, `src/KBot.Api/ApiClient.EFactura.Conturi.vb`, `src/KBot.EFactura/Views/ConturiForm.vb`, `ConturiForm.Designer.vb`, this file, `SLICE-0000-56-ajutor-conturi.md`.
Edited: `PYTHON/routes/efactura/{facturi_store,facturi,factura_routes}.py`, `src/KBot.Api/IEFacturaApi.vb`, `src/KBot.EFactura/Views/FacturiForm.Designer.vb` + `FacturiForm.Furnizor.vb` (button only), `HelpContent/contabil/efactura/index.md`, status files.

## Test results
`python -m py_compile` of the three Python files: clean. (The build below was run once, before the operator said no more builds, tests or git.) The new VB files and the Api / Domain changes show no compiler error in `dotnet build src/KBot.EFactura`; the project as a whole did NOT finish building at the time because another change in progress in `FacturiForm` (`FacturiForm.Arbore.vb`, not part of this slice) calls members that do not exist yet (`SelectView`, `SendCurrentAsync`, `VerifyCurrentAsync`, `StornoCurrentAsync`, `RefreshPdfView`, `ApplyTreeLook`). Nothing was run: no test, no window opened, no SQL executed, no route called.

## Unverified / deferred
- Full clean build once the `FacturiForm.Arbore` work compiles.
- The DDL is not run; run it on `AVACONT_SURSA`, then the schema sync (the operator does it). Until then the window shows the «Tabelele E-Factura ale unității lipsesc» message naming the file.
- The window was never seen on screen (Designer written by hand, 96 dpi); the grid behaviour (typing, «✕», the empty bank cell) is unverified.
- The combo «Cont emitent» on an invoice does NOT yet read this list (it still offers the accounts of earlier invoices): left alone on request («fără să umbli la altceva»). Natural next step.
- The mod-97 check refuses an account that is not a valid IBAN; a unit with an old non-IBAN account number cannot save it here.
