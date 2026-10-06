# SLICE-0110-12 - web area, COMPUTERS ONLY: details under Istoric / Plati, display menu in Extrase, navbar + header in Fundamentari / Ordonantari (operator request, 05.10.2026)

Part of the site plan (`SLICE-0110`). A phone is untouched (checked: no detail, no navbar, no header / footer, no Extrase card, the document cards keep their one-column layout).

## What changed and why
1. **Istoric**: under the grid, what IstoricView shows: the **Descriere** (Observatii) of the chosen row, and a small grid **Tip | Valoare** with its non-zero values (the seven values of the desktop list: rezervare initiala / definitiva / anterioara / diferenta, angajament legal, receptie, plata). New grid id `ang.istoric-valori` in `columns.js`.
2. **Plati**: under the grid, what PlatiView shows: the bank statement line of the chosen payment (Nr. document, Data banca, Data document, Referinta, Platitor, CUI, IBAN, Suma debit, Suma credit, Explicatii); «Selectati o plata.» / «Fara extras bancar asociat.» as in the desktop view.
3. **Extrase**: the card is the ExtrasePanel of the desktop (new `extrasecard.js`): the tree has a header with the icon (gear) that opens the menu **«Arata antet + operatii» / «Arata operatii + detalii»**. Antet + operatii: root / month = headers on top and the operations of the chosen header below; a day = that day's operations and the chosen one in full. Operatii + detalii: every node = operations on top, the chosen one in full below. The tree model and the 14 detail pairs are shared with the Extrase page (`extrase.js`: `buildExtraseModel`, `detailPairs`). The grids use the layouts `extrase.antete` / `extrase.operatii`.
4. **Fundamentari and Ordonantari**: a **horizontal navbar with two options, Vizualizare | Document** (DdfView / OrdView). Vizualizare = header + lines grid (+ footer for Ordonantari); Document = the signed PDF with its bar. The PDF is loaded only when the Document page is on screen (a hidden viewer has no width to draw into). Everything stretches to the bottom of the card (checked: the bottoms of the tree, the grid and the viewer are on the same line).
5. **Header information**: Fundamentari shows what DdfVizualizarePage shows (Cod angajament, Data creare, Compartimentul, CUAL, Beneficiar with the CIF, Obiect DDF; from `antet` of `/date/ddf`). Ordonantari shows what OrdVizualizarePage shows: the **«Cauta beneficiar»** filter above the grid (narrows the lines) and, under the grid, Beneficiar, Cod fiscal, Cont IBAN, Doc. justificative, Obiect DDF of the chosen line.
6. **Repair found on the way**: a tree made without its search box (the card trees, slice 0110-10) threw in `getComputedTreeHeight` when a node was opened (`searchWrapper` is null); fixed in `treeview-utils.js` (height 0 for the missing box). Before this, the trees of the cards stayed closed with empty grids after the root was opened.

## Files touched
`PYTHON/static/js/portal/{app.js, extrasecard.js (new), extrase.js, columns.js}`, `PYTHON/static/js/components/treeview/treeview-utils.js`, `PYTHON/static/portal.html`, `PYTHON/static/css/portal.css`, `tools/SitePreview/preview_server.py` (made-up DDF header, ORD line fields, payment statement fields).

## Checked
SitePreview in the browser pane (made-up data): Istoric detail (description + values of the chosen row), Plati detail (ten pairs), Extrase menu in both modes (the checked mark follows the mode; 14 detail rows after choosing an operation), Fundamentari header and navbar, Ordonantari filter / footer / Document page, bottoms aligned; phone width unchanged. No test code written, no suite run (house rule). `node --check` on every JS file touched.

## Not done / unverified
- **Nothing seen on screen** (panel screenshots time out): look, spacing and the height of the Istoric detail (190 px) are checked only through the DOM and measured sizes.
- Real data was not used (preview only); the fields of the answers were taken from the routes' code (`routes/forexe/{ddf,ord,plati,istoric}.py`).
- Plati: «Fara extras bancar» is decided by `idfxe` being empty (and no statement fields); unverified against the real database.
- Ordonantari: the CAB notes have no lines, so their node shows an empty grid and the footer stays empty.
- No help change: the site is not part of the in-app help.
