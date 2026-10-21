# SLICE-VC06-07 - NIR from an invoice K-BOT sent: the list and the prefilling (EV04, 21.10.2026)

Plan: [`docs/EV_PLAN.md`](../EV_PLAN.md) (slice EV04). The K-BOT half: 00EF-45 (server), 00EF-46 (combo), 00EF-47 (window). The closing of the loop is VC06-08.

## What changed and why
The Administrator takes over, in «Adaugare NIR», the received invoices the accountant sent from K-BOT. Nothing is copied: K-BOT's tables `EF_PrimiteVercon*` are in the same unit database as the `VC_*` tables, so VERCON reads them.

- **`routes/vercon/efactura_queue.py`** (new): `GET /api/vercon/doc/nir/efactura` = the sendings that wait (stored `TRIMISA`, or `PRELUATA` whose NIRs are all cancelled / gone: the rule of K-BOT's `primite_vercon.stare_efectiva`, written again here in SQL so the two packages stay apart), newest first, at most 500, per UNIT (not per gestiune: the Administrator has all of them). `GET .../<id>` = the invoice (number, date, signed total), the supplier found in `Parteneri` by normalised tax code (visible, with a tax code: what `_clean_nir` accepts), the lines with quantity, taxable unit price, VAT rate (the line's own, else the rate of the biggest taxable amount; `rate_guessed` when several rates exist), and the account proposed (`account`, NULL = none), plus `accounts` (the distinct proposals). Without the K-BOT tables: `available: false`, empty list.
- **`js/vercon/efactura-intake.js`** (new): `pick()` = a `<dialog>` with the waiting invoices (supplier, number, date, total, lines, sent by, the accountant's note; double click or Enter uses one) and «Folosește factura» / «Returnează…» / «Închide»; `apply()` fills the draft: `IdPartener`, `NrFact`, `DataFc`, `Valoare` (the invoice total), `accounts` (the proposals the context offers; the others are reported), and the lines WITHOUT a product (`Cant`, `Pret`, `IDTVA` found by percent in the current destination's VAT list, `ef_name` = the supplier's text).
- **`docs.js`** (999 lines: at the ~1000 limit; the new code lives in the new module): import, `intake`, `fromEfactura()` (opens the list, starts a NIR when none is in work, asks before replacing a started draft, applies, says what is left to do), the buttons «NIR din e-Factura…» (no draft) and «Din e-Factura…» (a new NIR in work), `efactura` in the body of the save, and the read-only column «Din factura primită» while the draft comes from an invoice.
- **CSS** `vercon-efactura.css` (+ one import line in `vercon.css`); **help** `static/help/vercon.json`, section «NIR dintr-o factură trimisă de contabil».
- The account is a PROPOSAL (decision D9): it is ticked in «Categorie / cont»; the Administrator can change it, and the product list follows the ticked accounts as always.

## Files touched
`PYTHON/routes/vercon/efactura_queue.py`, `doc_routes.py` (import + `register`), `PYTHON/static/js/vercon/efactura-intake.js`, `docs.js`, `PYTHON/static/css/vercon-efactura.css`, `vercon.css`, `PYTHON/static/help/vercon.json`, `implementare_vercon_v2.md`.

## Test results
`py_compile` of the three Python files, `import routes.vercon.efactura_queue`, `node --check` of the two JS files: ok. No test written or run, no browser, no SQL.

## Correction (21.10.2026, found on the site by the operator)
`efactura-intake.js` had a missing `}` at the end of the hint paragraph object in `pick()` (line 30): `Uncaught SyntaxError: Unexpected token ')'`, so the whole editor module failed to load. Fixed; every changed JS file under `static/js` now passes `node --check` (copied as `.mjs`, since the files are ES modules).

## Unverified / deferred
- **Nothing seen in a browser**, nothing run against MariaDB. The dialog's look, the prefilled editor (the order in which the header inputs take the draft's values, the account tree, the VAT ids, the new read-only column), and the fit of `fromEfactura` with `add()` / `allowDiscard()` are unchecked.
- The VAT id: `allocation.taxOptions({})` lists the VAT of the CURRENT destination; a line whose product ends in another destination takes that destination's tax with the same percent through the editor's own rule (`defaultTax`) when the product is chosen. A rate the destination does not have leaves the cell empty (the editor asks for it).
- The unit of measure and a new product: the Administrator picks or creates the product as in any NIR; `um` is not copied.
- The total the editor checks (the sum of the lines with VAT against the invoice total) can differ by cents because of rounding per line; the editor's «Corectează la valoarea facturii» (up to 1 leu) is the way, as for a hand-typed NIR.
- MF / OI: a NIR for them needs data the invoice does not have (classification, inventory number, holder); the Administrator completes it. Not exercised.
- Accounts of a gestiune the current context does not offer (an MF/OI context offers only itself) are not ticked; the page says so.
