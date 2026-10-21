# SLICE-00EF-45 - A received invoice SENT to VERCON: the server half (EV01, 21.10.2026)

Plan: [`docs/EV_PLAN.md`](../EV_PLAN.md) (decisions D1-D12 of the operator). This is EV01 of the plan: the data model and the K-BOT routes. The tree control (EV02), the K-BOT window
(EV03) and the VERCON half (EV04, EV05) are later slices of the same family.

## What changed and why
The accountant decides in K-BOT which received invoices go to VERCON, so the Administrator can take them over in «Adaugare NIR». Nothing is copied: the invoice stays in
`EF_Primite` / `EF_PrimiteLinii`; a *sending* is a row that says "this invoice, with these proposed accounts, is ready to be taken over".

- **Three new tables** (`sql/EV_01_primite_vercon.sql`, to run by hand on `AVACONT_SURSA` and then sync):
  - `EF_PrimiteVercon`: the sending (`Stare` TRIMISA / PRELUATA / RETURNATA / RETRASA, who and when, note, return reason, who took it over). At most one ACTIVE sending
    (TRIMISA / PRELUATA) per invoice, checked in the transaction under a lock on the invoice row (MariaDB has no partial unique index).
  - `EF_PrimiteVerconLinii`: ALL the lines of the invoice (decision D10) with the account proposed by the accountant (`IdMasina`, NULL = the Administrator chooses; D9: a proposal).
  - `EF_PrimiteVerconNir`: the NIRs created from the sending (0..n; EV05 writes it). No foreign key to `VC_NIR` on purpose: the migrator empties the VC tables.
- **Server** (`PYTHON/routes/efactura/primite_vercon.py`, `primite_vercon_routes.py`, registered in `__init__.py`):
  - `GET /api/efactura/vercon/conturi`: the tree gestiuni -> accounts for the whole unit database, MF and OI included (D8). The rule "which accounts a gestiune accepts" is VERCON's own
    (`routes.vercon.receipt_products.warehouse`, imported lazily so the two packages are not tied at start-up).
  - `GET /api/efactura/vercon/stari`: one short line per invoice that was ever sent (for the tree of received invoices).
  - `GET /api/efactura/primite/<id>/vercon`: lines with values WITH VAT (the receptions' rule `primite_ke.valori_cu_tva`), the active sending, the history, the matched partner,
    possible duplicates (same supplier + number + date already in `VC_NIR`) and `poate_trimite` / `motiv`.
  - `POST` (send), `PUT .../conturi` (change the proposed accounts while it waits), `DELETE .../<idTrimitere>` (take back while it waits). Writes are for the role **Contabil** only,
    checked on the server. Audit: `EF_VC_TRIMITE`, `EF_VC_CONTURI`, `EF_VC_RETRAGE`.
  - **Effective state (D5)**: a PRELUATA sending whose NIRs are all cancelled or gone counts as TRIMISA again - decided when reading, so it does not depend on every way a NIR can be cancelled.
- **Refusals** (`NETRIMISIBILA`, 409): credit notes (D3), an invoice without lines, an invoice already sent, a supplier not found in `Parteneri` (the NIR needs a partner;
  the accountant can associate it first). A duplicate only WARNS.

## Files touched
`sql/EV_01_primite_vercon.sql` (new), `PYTHON/routes/efactura/primite_vercon.py` (new), `primite_vercon_routes.py` (new), `__init__.py` (docs + import), `docs/EV_PLAN.md` (written earlier).

## Test results
`py_compile` of the three Python files: ok. `import routes.efactura.primite_vercon`: ok. No test written or run (operator's rule), the SQL not run, nothing called.

## Unverified / deferred
- **SQL not run.** The three tables must exist (run on `AVACONT_SURSA`, then the schema sync) and `KE_01_primite_receptii.sql` must have been run (the VAT rate of a line).
- **Server not deployed** (the operator does it by hand; nothing was sent). New client + old server would fail on the new routes.
- The partner is found by the normalised CUI against `Parteneri.CodFiscal` with the expression of `primite.py` (`_PARTNER_CUI`); not run on real data. `Parteneri` is not filtered by `IdUnitate` here.
- `VC_Gestiuni.Active`, `VC_Subunitati.Active`, `VC_Masini.Activ`, `VC_NIR.Anulat/DataFc/NrFact/IdPartener` are used as the dump of 22.09.2026 shows them (read in the file, not on the live server).
- The VERCON side (list, return with a reason, take over, close the loop with `EF_PrimiteVerconNir`) is NOT in this slice (EV04, EV05); until then a sending stays TRIMISA.
- The case "one of several NIRs of a sending is cancelled" is decided in EV05 (proposal in the plan, section 4).
- No help yet: nothing the operator sees has changed (the window is EV03).
