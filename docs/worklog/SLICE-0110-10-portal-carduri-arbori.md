# SLICE-0110-10 - web area: a tree per card (computers), no date groups, Fundamentari + Ordonantari (operator request, 05.10.2026)

Part of the site plan (`SLICE-0110`). The cards of one angajament in the signed-in page (`/portal`).

## What changed and why
1. **A tree per card, COMPUTERS ONLY** (`min-width: 901px`; a phone is untouched). As in the desktop views, the card has its own tree beside
   its grid: «Toate ...» > month > the day / the document, every node with its total or its count; a click shows the rows of the node.
   The trees are built in `static/js/portal/tabtrees.js` (no DOM: data in, nodes + the rows of each node out), after the desktop code:
   | card | tree (desktop view it copies) |
   |---|---|
   | Istoric | Tot istoricul (rows) > month > day (IstoricView) |
   | Rezervari | Toate rezervarile (sum R_Valoare) > month > (day, kind) leaf with the sum of ValoareOperatie; the initial lines all sit on the LAST initial day; negative leaf red (RezervariView) |
   | Receptii | Toate receptiile > month > receptie, value = total of the LAST header (not deleted); «Instantanee neasezate» folder for headers with no receptie (ReceptiiView) |
   | Extrase (of the angajament) | Toate extrasele > month > day by DataBanca, count |
   | Plati | Toate platile (sum) > month > day, the day = the bank date, else the payment date (PlatiView) |
   | Fundamentari | Toate reviziile > month > revision `Rev. N - date` (DdfView) |
   | Ordonantari | Toate ordonantarile > month > `nr - date` (OrdView), plus the folder «Note de corectie CAB» |
   **Sumar has no tree**: the desktop SumarView has none either.
2. **Date grouping removed from every card except Istoric**, on both computer and phone (the `groups` of Rezervari, Receptii, Extrase, Plati are gone;
   on a computer the tree does that job, on a phone the grid is flat). Istoric never had a grouping, so it is left exactly as it was.
3. **«Documente» is gone; «Fundamentari» and «Ordonantari» take its place** (both modes). Each shows the LINES of the chosen document in a grid
   (DDF section A lines / ORD lines: both were already in the answers of `/date/ddf` and `/date/ord`, unused until now) and its signed PDF in the
   viewer under it (the PDF viewer, download button and messages are the old ones). On a computer the tree chooses the document; on a phone there is no
   tree, so the list of documents of the old card stays (one list per card) and the lines grid follows the chosen row.
   The **CAB correction notes** (they were a kind of «Documente») have no card of their own: they sit in Ordonantari, in the folder «Note de corectie CAB»
   (tree) / as rows of the phone list. They have a PDF and no lines.
4. `DataGrid.setRows(rows, {keepWidths})`: a tree click replaces the rows without recomputing the column widths (the columns do not jump between nodes);
   the same grid is kept while the node changes (filters are cleared on each node).

## Files touched
`PYTHON/static/js/portal/{tabtrees.js (new), app.js, columns.js (new, shared with 0110-11)}`, `PYTHON/static/js/dgv/datagrid.js` (`keepWidths`),
`PYTHON/static/portal.html` (tabs, split layout, lines grid), `PYTHON/static/css/portal.css` (`.pa__split`, `.pa__tree`, `.pa__content`, `.pa__docs-lines`),
`tools/SitePreview/preview_server.py` (made-up Istoric, Extrase, DDF lines, ORD lines, receptie header fields, `data_banca` on payments).

## Checked
- SitePreview in the browser pane (made-up data, computer width 1400 and phone width 375): every card opens without console errors; on a computer the tree
  shows with the root open and the months closed, a month shows its rows, a leaf shows its rows; Fundamentari: a revision node shows its lines and the PDF
  (4 pages drawn), a folder shows the lines of all it holds and asks for a document; Ordonantari shows the notes folder; on a phone no tree, no group band
  in any card, the document lists show.
- `node --check` on every JS file touched. No test code was written and no test suite was run (house rule).

## Not done / to do (unverified)
- **Nothing was seen on screen**: screenshots of the pane time out here, so the look (tree width 270 px, the lines grid beside the viewer, the phone layout
  with nine tabs in the sliding row) was checked only through the DOM and computed sizes.
- The real database was not touched (the routes are the old ones; only the preview server fakes them).
- The right-click menu of every grid still offers «Grupeaza dupa aceasta coloana» (a general feature of the grid, not a default grouping). Say if it must go too.
- On a phone the nine tabs share one row (it slides sideways when it does not fit).
- Receptii: the grid keeps showing every version of the header (the «Versiune antet» column), as before; the tree value is the last header's total, as in the desktop.
- No help change: the site is not part of the in-app help.
