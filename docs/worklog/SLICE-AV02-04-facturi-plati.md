# SLICE-AV02-04 — «Facturi si plati» (the Access form OPuri2017)

Family AV, stage 2. Asked by the operator (08.11.2026, "hai sa facem si astea"), after `SLICE-AV02-02` (tables, OP numbering, lock) and `SLICE-04EF-01` (the payment from «Contari»). **Code written and compiled (`dotnet build` KBot.App Release, 0 errors, 0 warnings); nothing was run, nothing seen on screen, no test run (operator rule).**

## Decisions taken while building (they differ from the plan where said)

- **Its own page, not the page «Contari» of FacturiCumparare.** The plan said to reuse `FacturiCumparareContarePage` without the tree. That page and its server calculation are keyed by `EF_Primite` (an invoice = an ANAF message), so reusing it would have meant rewriting both. `FacturiPlatiPage` has the same look (partner, classification and note on top, evidence / payment templates on the left, grids on the right) over a server flow of its own that shares the SERVICES: `postare.py`, `plati.py` (`posteaza_ordin`, `aloca_pe_operatii`, numbering, lock, delete) and the helpers of `primite_contari.py` (templates, classifications, codes, the text of the order). `primite_plata.posteaza` now uses the same `plati.posteaza_ordin`.
- **A typed invoice needs no new table.** It is the evidence itself: a document (`Fel = E`) and an operation (`Tip = E`) from the evidence template, with the partner on the credit side. A payment is allocated on that evidence operation in `TZ_PlatiFacturi.IdEvidentiere` (`IdPrimita` stays NULL, as the DDL of AV02-02 already says). The rest to pay = the credit lines of the partner on the operation − the sum of the allocations on it (`plati.valori_pe_operatii`). So the same list serves the invoices typed here and any evidence of the partner already in the books.
- **«Anul anterior»** lists the evidence operations of the previous year that THE DATABASE holds (notes of year − 1). The previous year before the port is not in the unit database, so for migrated history the list stays empty until the opening balances / the history of the previous year are ported (`IdEvidentiereAnterior` of the DDL is for that, unused here).
- The reception check and the tick «Protectie contari Plati/Receptii» do NOT apply here: a typed invoice has no reception link.

## What changed

- **Server** `PYTHON/routes/avacont/introducere_date/operatii/facturi_plati.py`: `GET /api/avacont/facturi-plati/context` (notes, partners, classifications with their templates), `GET .../deschise` (unpaid invoices of the partner, current and previous year; the orders of the year with their lock; the provisional number of the next order; the account), `POST .../salveaza` (typed invoices + payment with its order, ONE transaction), `DELETE .../plata/<idop>` (an unlocked order with its payment). The real order number comes back in `plata.nrop`.
- **Shared code**: `plati.py` gets `posteaza_ordin`, `aloca_pe_operatii`, `rest_pe_operatii` / `valori_pe_operatii`, `id_partener_linie`; `primite_plata.py` calls `posteaza_ordin`.
- **Client**: `FacturiPlatiPage` (+ Designer, generated) and the same window `OperatiiSabloaneForm` (page «Facturi si plati», read the first time it is shown). The class `OpExplicatie` (the ASCII text of the order, `KBot.EFactura`) became `Public` so the page can reuse it.

## Files touched
`PYTHON/routes/avacont/introducere_date/operatii/facturi_plati.py` (new), `plati.py`, `PYTHON/routes/efactura/primite_plata.py`, `PYTHON/routes/avacont/__init__.py`; `src/KBot.App/Avacont/IntroducereDate/Operatii/FacturiPlatiPage*.vb`, `OperatiiSabloaneForm*.vb`, `KbotForm.Operatii.vb` (new); `src/KBot.EFactura/Views/FacturiCumparare/OpExplicatie.vb` (Friend to Public); the domain and API files of SLICE-AV02-03.

## Test results
None run. Python: the modules import and the routes are registered. VB: build of KBot.App in Release, 0 errors, 0 warnings.

## Unverified / deferred
- Never run against a database; nothing seen on screen. The payment path now has TWO callers of `posteaza_ordin` (Contari and this page): the first one was changed after it had been written, so retest the payment from «Contari» too.
- The partner of an invoice is found by the CREDIT lines (401 / 404 side). A client-side invoice (partner on the debit) is not offered.
- No edit of a typed invoice and no delete of an evidence operation (that is the right «Stergere N.C.», its own job); only a payment can be deleted.
- A partner's account for the order: the one of `Parteneri_Coduri` for the classification, else `Parteneri.ContIBAN`. `ContUnitate` / `bBIC` stay empty like in «Contari» (see `SLICE-AV02-02`).
- Duplicate invoice numbers of the same partner are not refused.
- Help: `contabil/nomenclatoare/operatii.md`, tags `AV02-03, AV02-04, 0000-176`.
