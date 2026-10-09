# SLICE-00EF-17 -- Received e-invoices, server half (parser, sync, routes)

Operator request, 09.10.2026 (answers to the five questions of the hand-over thread).

## Decisions (operator)
1. Two places show the received invoices: the «E-Factura» view of the DDF (`KbotForm`) AND a separate screen with all of them.
2. The view: tree on the left, months as groups under a root «Toate»; a horizontal bar of views (as in DDF): Linii, PDF (classic
   one embedded in the XML, or the one drawn by ANAF), Atasamente (only if the XML has some), Mesaje (only if there are some).
   Right click on an invoice: «Salveaza ca zip» / «Salveaza ca xml».
3. Sync is started from the E-Factura form (`FacturiForm`) -- a button in its bar, N days (max 60), a progress box, no count boxes on success.
4. `TrimiteMesajFactura` (message to the supplier): LATER.
5. Link to a DDF: automatic only when a DDF rev 0 has a partner with the same normalised CUI; also MANUAL, so no direct reliance on CodFiscal.
   -> new table `EF_PrimiteAsocieri` (`sql/00EF_17_primite_asocieri.sql`, NOT RUN).

## Done in this sub-slice (A: server, no screen)
- `PYTHON/routes/efactura/primite_ubl.py` -- lenient UBL reader (Invoice / CreditNote). Checked on the 3 real files in `Surse/EF_EXEMPLE`
  (9% and 19% VAT, 11 PaymentMeans, embedded PDF with 100 KB, BillingReference, line notes): numbers, dates, supplier CUI/name/address,
  totals, lines, embedded file decoded to a valid `%PDF`.
- `primite.py` -- sync (one transaction per message, `EF_Mesaje.IdSol` unique, batches with `ramase` for a progress bar), list (year, month,
  text, supplier, DDF with `legatura` auto/manual), detail, xml, zip (downloaded again from ANAF), pdf (ANAF service from the saved XML),
  embedded files, mark read, manual link.
- `primite_routes.py` -- `/api/efactura/primite...` (list at the top of the file); registered in `__init__.py`.

## Rules worth knowing
- Supplier name comes from the XML (`RegistrationName`); ANAF's company service is NOT called (it is mandatory in CIUS-RO).
- `Tip` NC / `Semn` -1 for a credit note with a positive total; `IdPrimitaRef` is set when the referred invoice is already stored.
- A message that cannot be read is skipped and reported; it is not written, so the next sync tries it again.
- `EF_Primite` keeps ONE VAT rate: the rate of the biggest taxable amount; `TVA` is the total VAT in the document currency.
- The «rev 0» condition of the automatic link is NOT applied on the server (the list takes any `iddf`); the client passes the DDF it is on. To
  confirm what «rev 0» means in `FX_DDF` (`CUAL`?) before B.

## Verified / not verified
- Verified: `py_compile`; parser on the 3 real XML files; parameter counts of the inserts against a fake cursor.
- NOT verified: any SQL against a real MariaDB (REGEXP_REPLACE normalisation of the partner code, the joins), the ANAF calls, a real sync.
  Server not deployed; the DDL (`00EF_17`) not run; no tests written (not asked).

## Next
B (client: domain, `IEFacturaApi`, the view in `KbotForm`, sync button + progress in `FacturiForm`), C (separate screen), E (help 0000-NN, NOUTATI, status).

## Correction (operator, 09.10.2026)
- «DDF rev 0» = just «a DDF exists»: the automatic link needs nothing more than a `FX_DDF_Parteneri` row with the same CUI. Nothing to add on the server.
- Several VAT rates on one invoice are real: new table `EF_PrimiteTVA` (`sql/00EF_17_primite_tva.sql`, NOT RUN): one row per `TaxSubtotal`
  (category, rate, base, VAT). The parser returns `cote`; the sync writes them; `GET /primite/<id>` answers `cote`. `EF_Primite.CotaTVA`
  stays the rate of the biggest base (a summary for the list). Checked on a made-up two-rate XML (19% + 9%) and the 3 real files.
- `00EF_17_primite_asocieri.sql` was run by the operator.

## First real run (operator's test token, unit CUI 50622085, 09.10.2026) -- found and fixed
- The message list answers `{data_creare, cif, id_solicitare, detalii, tip, id}`. ANAF downloads a message by **`id`**, NOT by
  `id_solicitare` (that is the supplier's upload number = the XML name inside the zip). 00EF-07's `descarca_mesaj` used `id_solicitare`
  and would have failed ("nu exista inregistrata nici o factura"). Sync and `zip_anaf` now use `id` (stored in `EF_Mesaje.IdIncarcare`);
  `IdSol` stays `id_solicitare` (unique key). The route `/mesaje/<id>/descarca` takes the message `id`.
- `cif_emitent` / `cif_beneficiar` are not fields of the answer: `anaf_api.lista_mesaje` now reads them from `detalii`.
- 4 received messages in 60 days read correctly (VAT 21%, one invoice with negative amounts = a reversal, kept as `FC`, `Semn` 1).
  `tools/efactura/proba_primite.py` works with `EF_TOKEN_PROBA` + `--cif-unitate` (no database).
