# SLICELESS - VERCON online: window «Parteneri dublati» (23.10.2026)

## What changed and why

The same firm existed twice under two fiscal codes (Miralics: 28022254 and 280222254), which splits its receipts and invoices. A window now lists such partners and merges them into one fiscal code.

- **Duplicates** = same name after removing spaces, accents, punctuation, a leading SC / S.C. and a trailing SRL / S.R.L. / SRL-D / SA / SNC / SCS / SCA / PFA / II (`utils/vercon_suppliers.duplicate_key`), with different fiscal codes. Partners without a fiscal code are ignored (the partner view already skips them).
- **Merge** (`POST /api/vercon/suppliers/merge`, one transaction): `VC_NIR.CodPartener` (+ `FurnizorV`) and `VC_FACTURI.CodPartener` of the whole DC move to the kept code, `Version` / `VERSION` are bumped (an editor holding an old copy gets a conflict), the merged `Parteneri` rows get `Ascuns = 1` (never deleted: FOREXE keeps its references). Audit line `supplier.merge`; idempotent through the request key. Needs the `receptie` right and the gestiune lock.
- **Window** opens once per page when an Intrari form (NIR / Storno) opens, only for users who may merge and only when duplicates exist; always from `Intrari > Parteneri dublati...`. After a merge the open form reloads.
- «Factura interna» (Codex's wording) = the common invoice `VC_FACTURI`, linked to NIRs by `VC_NIR_FACTURI`; the UI calls it «factura comuna». Both tables carry the supplier code, so the merge rewrites both. Where several invoices end up with the same number and date the answer says so (`invoice_twins`); they are NOT auto-merged (use the existing invoice merge).

## Files touched

`PYTHON/utils/vercon_suppliers.py` (duplicate_key), `PYTHON/routes/vercon/supplier_merge.py` (new), `PYTHON/routes/vercon/doc_routes.py` (register), `PYTHON/static/js/vercon/supplier-duplicates.js` (new), `PYTHON/static/js/vercon/app.js` (offer + menu entry), `PYTHON/static/css/vercon-supplier-duplicates.css` (new) and `vercon.css` (import), `PYTHON/static/help/vercon.json` (section «Parteneri dublati»).

## Test results

Only `import` of the Python modules, `node --check` of both JS files and a manual run of `duplicate_key` on sample names (S.C. MIRALICS S.R.L. / SC Miralics SRL / MIRALICS S.R.L.-D -> `miralics`). No test code, nothing run against a database or on screen (operator rule).

## Unverified / deferred

- SQL never run on a real DC database; the `UPDATE`s assume `VC_NIR.FurnizorV`, `VC_NIR.Version`, `VC_FACTURI.VERSION` (all read from existing code).
- Closed exercises are rewritten too (the point is to repair history); audited, not blocked.
- No "these are different firms" memory: the window will list a legitimate pair again until one is renamed.
- E-invoices whose CUI is the hidden code no longer match a partner (matching uses active partners only).
- Not committed (operator rule: no git writes). No slice number given - recorded as sliceless.

## Round 2 (23.10.2026, operator feedback on screenshots)

- **Scope fixed:** the window now starts from suppliers that appear in VERCON receipts, common invoices or received e-invoices (`supplier_merge.candidates`), not from the whole Parteneri list. First version only looked at ACTIVE Parteneri rows, so MIRALIS IMPEX (present in the tree, hidden/missing in Parteneri) never showed while BIN GO (0 documents) did. Name = Parteneri row, else the receipt name, else the e-invoice name. The kept code must be an active partner (else the merge is refused).
- **ANAF button** left of «Combina» (`POST /api/vercon/suppliers/anaf`, existing `routes/inregistrare/anaf.lookup`, nothing stored): result in the row tooltip. None found -> Combina disabled; one found -> it becomes «Pastreaza»; several found with different name/address -> disabled (different firms). The rule is enforced in the page only; the server does not call ANAF on merge.
- **Tree menu:** supplier branches of «Documente salvate» (grouped by supplier) get the hover ⋮ with `Afiseaza info furnizor` (`GET /api/vercon/suppliers/info`). `paintTree` now passes the full node id for non-document nodes.
- Files: `supplier_merge.py`, `supplier_lookup.py` (new), `doc_routes.py`, `supplier-duplicates.js`, `supplier-info.js` (new), `docs.js`, `receipt-tree.js`, `ui.js`, CSS, help.
- Still unverified: nothing run on a database, on screen or against ANAF; tooltip text inside a modal `<dialog>` not seen.

## Round 3 (23.10.2026)

- «Parteneri dublati...» menu entry hidden unless `GET /api/vercon/suppliers/duplicates` returns groups (fetched once per page, again after a merge) - `app.js`.
- eFactura association (NIR side): the unit of measure is no longer a criterion of the proposals (`efactura-match.js`); the message after a full association is gone (`docs.js`); the NIR-linked-to-eFactura lock message is not shown on click (`docs.js`).
- **Automatic first association** (`efactura-auto-association.js`, called first by `efactura-association.js`): exactly one candidate invoice, no allocations yet, no other receipts on the invoice and the proposals use up every line of both sides with positive quantities -> saved directly, no window, no message. Otherwise (or on any error, logged to the console) the window opens as before. "100% confidence" is defined by this exact coverage, not by a score: the code has no confidence value.
- Not run on screen or database. `learn` is false for automatic pairs, like proposals saved from the window.
