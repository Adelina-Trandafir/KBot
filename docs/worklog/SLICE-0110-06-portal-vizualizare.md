# SLICE-0110-06 / 08 / 10 / 12 — Vizualizarea datelor și documentelor

Istoric din 05.10.2026. 08 înlocuiește limita inițială „fără PDF” cu vizualizare numai pentru citire; semnăturile nu sunt verificate de portal. 10 înlocuiește cardul Documente cu Fundamentări/Ordonanțări și mută selecția în arbori pe calculator; 12 adaugă detalii și separă Vizualizare/Document. Recepțiile brute, totalurile arborelui și liniile grilei au roluri diferite, păstrate în etape. Probe pe exemple/date fictive și limite pe date reale rămân distincte.

- [0110-06](#slice-0110-06)
- [0110-08](#slice-0110-08)
- [0110-10](#slice-0110-10)
- [0110-12](#slice-0110-12)

## SLICE-0110-06 - web area: the read-only views (operator request, 05.10.2026) <a id="slice-0110-06"></a>

Part of the site plan (`SLICE-0110`). Uses the guard of 0110-03 and the grid of 0110-05. View only: no editing, no PDF, nothing about FOREXE.

### What changed and why
- Server, `PYTHON/routes/portal/date.py`: `GET /api/portal/date/{tree,sumar,rezervari,receptii,plati}` behind `require_portal_session`. THE QUERIES ARE NOT COPIED: each route calls the function the desktop app's route already runs (`routes/forexe/*`), the undecorated one through `__wrapped__`, after setting `g.session.db_name` from the portal session. Those functions read only the query string and `g.session.db_name`, so a portal token reaches the unit it opened and no other, and the answers have exactly the desktop shape. The module fails at import if a function loses `__wrapped__`. No route of the desktop app was touched. Only these five readers (all SELECT) are reachable; the module header says to read a function before adding one.
- Page, `static/js/portal/app.js` (+ `portal.html`, `css/portal.css`): toolbar with Unitatea / Anul / Sursa (the existing `Combobox`; sources of the chosen year plus «Toate sursele»), the list of angajamente (DataGrid: cod, descriere, stare, surse, creat, has-Rezervari/Recepții/Plăți/DDF/ORD), and for the picked angajament four tabs - Sumar (totals per indicator, footer totals), Rezervări (grouped by month, aggregates in the band), Recepții (grouped by month), Plăți (grouped by month, totals). Answers are cached per (angajament, tab) and an older answer never replaces a newer one. A 401 sends the user back to the sign-in card; any other refusal shows the server's sentence. Layout: two panes on desktop, stacked under 900 px.
- `portal.js` now only signs in and hands the account to `createApp`; the unit picker moved from the header to the toolbar.
- The event bus complained in the console about every event nobody listened to (`user-activity` on each click); the page registers a quiet listener for the session events the monitor emits.
- Grid fixes found on screen (`datagrid.js`): a group band's caption now spans the leading columns that have no aggregate (it was cut at the width of the first column); automatic widths measure the title in bold and leave more room (dates were clipped).

### Files touched
`PYTHON/routes/portal/date.py` (new), `PYTHON/main.py` (imports the module), `PYTHON/static/portal.html`, `PYTHON/static/css/portal.css`, `PYTHON/static/js/portal/{portal.js,app.js (new)}`, `PYTHON/static/js/dgv/datagrid.js`.

### Checked
- Flask client with the database connection replaced by a recorder: no token and a desktop Bearer header both 401; five readers answer with the portal's unit (`111_X`) even when the query carries `db_name=000_DEMO`; missing `cod` / `an` / `ss` 400; POST 405.
- In the browser pane (stub with made-up data): sign-in, unit picked through the Combobox, year and source filled from the periods, list of 40 angajamente, each tab with its columns, month grouping and footer totals, cache (no second request for a tab already seen), resume after a page reload with the stored token, stacked layout at phone width, no console errors. Screenshots looked at: desktop list + Rezervări with grouping.

### Not done / to do
- Real database NOT exercised: the shape of the real rows comes from the desktop functions, but nothing was read from MariaDB.
- **Recepții is the raw list** (one row per header snapshot and indicator, grouped by month). The desktop view builds a month / reception / header tree with an aggregated list (slice 0015); that logic is in the VB view and was not ported. Same for the payment tooltip and the extras detail panel of Plăți (the bank statement fields are in the answer but not shown).
- No `include_hidden` switch, no last-used source per user (default «Toate sursele»), no role-based restriction (all roles see everything their unit has, as decided).
- Dark theme and the filter popup were not looked at on screen in this page.
- Server not deployed.

## SLICE-0110-08 - web area: the signed documents (DDF, ORD, notes) in a read-only PDF viewer (operator request, 05.10.2026) <a id="slice-0110-08"></a>

Follows the verdict of `SLICE-0110-07`. View only: nothing is written, signatures are not checked here.

### What changed and why
- Server, `PYTHON/routes/portal/date.py` (same method as 0110-06: the desktop functions, undecorated through `__wrapped__`, the unit taken from the portal session): three more list readers `GET /api/portal/date/{ddf,ord,note}?cod=` and three byte routes `GET /api/portal/date/{ddf-pdf/<idrev>, ord-pdf/<idordp>, nc-pdf/<idnc>}`. The byte routes send exactly what the desktop route sends (raw bytes, `ETag` = the stored SHA-256, 404 with the Romanian sentence when no signed PDF is stored); the portal adds `Cache-Control: private, no-store`, `Content-Disposition: inline`, `nosniff`. All GET; PUT is 405.
- pdf.js is vendored: `PYTHON/static/vendor/pdfjs/` (pdfjs-dist 4.10.38, Apache-2.0, `README.md` says where each file comes from and how to update; the `.mjs` files are stored as `.js` so every server sends a JavaScript type). Loaded only when a document is opened (about 2 MB).
- `static/js/portal/pdfview.js` - `createPdfView(container).open(bytes) / clear()`: pdf.js viewer with `enableXfa: true` and `isEvalSupported: false`; the document is copied before pdf.js gets it; an older request never replaces a newer one; errors come back as sentences the operator can read. `static/css/pdfview.css` makes the form fields inert (`pointer-events: none`) and replaces the stock loading image (which we do not ship) with a text mark.
- Page: new tab «Documente» in `portal.html` / `app.js`: one grid for the three kinds (Document, Număr, Data, Suma, Semnături, PDF semnat) built from the three list answers, the viewer under it, the sentence «Vizualizare a documentului semnat. Semnăturile se verifică în K-BOT.», and «Descarcă PDF-ul semnat» (the exact bytes, as a file named like `ORD_1_<cod>.pdf`). One signed document opens by itself; with several the person chooses; a row without a stored signed PDF says so. Lists are cached per angajament.

### Files touched
`PYTHON/routes/portal/date.py`, `PYTHON/static/portal.html`, `PYTHON/static/js/portal/{app.js,portal.js,pdfview.js (new)}`, `PYTHON/static/css/pdfview.css (new)`, `PYTHON/static/vendor/pdfjs/*` (new, 6 files).

### Checked
- Flask client, connection replaced by a recorder: no token 401 for the three byte routes; the unit used is the portal's; bytes identical to what was stored (including a NUL and high bytes), ETag equal to the stored SHA-256, the three headers present; a missing document 404 with the sentence; non-numeric id 404; PUT 405.
- Browser pane, served with the portal's real CSP (`script-src 'self'`, no inline script), stub data but the REAL sample PDFs of the repository: list of 4 documents (DDF rev. 1 and 2, ORD, note); the signed ORD (8 signatures) opens with its final values, the signed note opens, the unsigned DDF (pure XFA) opens through the XFA engine (4 pages); the row with no PDF shows the message and no viewer; the download button produces a blob of exactly the size and type of the file on the server; XFA fields report `pointer-events: none`; no CSP error and, after the CSS fix, no 404. Looked at on screen: the tab with list, note and the ORD.

### Not done / to do
- **A real signed DDF produced by the K-BOT signing flow was not available** (the DDF in the test is an unsigned sample); open one before telling users it works for every DDF. Same for documents with attachments inside the PDF, very large PDFs and phones.
- Signatures are not verified or even listed beyond the role letters the server stores (`semnatura`); the viewer says so.
- pdf.js does not run the form's scripts: a total that a script computes on opening may differ from Adobe's.
- The real database, SMTP and nginx were not involved; server not deployed. Needs `/static/vendor/pdfjs/` to be served (it is under the static folder Flask already serves; check that nginx does not filter `.js` under it).
- No help change (the site is not part of the in-app help).

## SLICE-0110-10 - web area: a tree per card (computers), no date groups, Fundamentari + Ordonantari (operator request, 05.10.2026) <a id="slice-0110-10"></a>

Part of the site plan (`SLICE-0110`). The cards of one angajament in the signed-in page (`/portal`).

### What changed and why
1. **A tree per card, COMPUTERS ONLY** (`min-width: 901px`; a phone is untouched). As in the desktop views, the card has its own tree beside its grid: «Toate ...» > month > the day / the document, every node with its total or its count; a click shows the rows of the node. The trees are built in `static/js/portal/tabtrees.js` (no DOM: data in, nodes + the rows of each node out), after the desktop code:
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
2. **Date grouping removed from every card except Istoric**, on both computer and phone (the `groups` of Rezervari, Receptii, Extrase, Plati are gone; on a computer the tree does that job, on a phone the grid is flat). Istoric never had a grouping, so it is left exactly as it was.
3. **«Documente» is gone; «Fundamentari» and «Ordonantari» take its place** (both modes). Each shows the LINES of the chosen document in a grid (DDF section A lines / ORD lines: both were already in the answers of `/date/ddf` and `/date/ord`, unused until now) and its signed PDF in the viewer under it (the PDF viewer, download button and messages are the old ones). On a computer the tree chooses the document; on a phone there is no tree, so the list of documents of the old card stays (one list per card) and the lines grid follows the chosen row. The **CAB correction notes** (they were a kind of «Documente») have no card of their own: they sit in Ordonantari, in the folder «Note de corectie CAB» (tree) / as rows of the phone list. They have a PDF and no lines.
4. `DataGrid.setRows(rows, {keepWidths})`: a tree click replaces the rows without recomputing the column widths (the columns do not jump between nodes); the same grid is kept while the node changes (filters are cleared on each node).

### Files touched
`PYTHON/static/js/portal/{tabtrees.js (new), app.js, columns.js (new, shared with 0110-11)}`, `PYTHON/static/js/dgv/datagrid.js` (`keepWidths`), `PYTHON/static/portal.html` (tabs, split layout, lines grid), `PYTHON/static/css/portal.css` (`.pa__split`, `.pa__tree`, `.pa__content`, `.pa__docs-lines`), `tools/SitePreview/preview_server.py` (made-up Istoric, Extrase, DDF lines, ORD lines, receptie header fields, `data_banca` on payments).

### Checked
- SitePreview in the browser pane (made-up data, computer width 1400 and phone width 375): every card opens without console errors; on a computer the tree shows with the root open and the months closed, a month shows its rows, a leaf shows its rows; Fundamentari: a revision node shows its lines and the PDF (4 pages drawn), a folder shows the lines of all it holds and asks for a document; Ordonantari shows the notes folder; on a phone no tree, no group band in any card, the document lists show.
- `node --check` on every JS file touched. No test code was written and no test suite was run (house rule).

### Not done / to do (unverified)
- **Nothing was seen on screen**: screenshots of the pane time out here, so the look (tree width 270 px, the lines grid beside the viewer, the phone layout with nine tabs in the sliding row) was checked only through the DOM and computed sizes.
- The real database was not touched (the routes are the old ones; only the preview server fakes them).
- The right-click menu of every grid still offers «Grupeaza dupa aceasta coloana» (a general feature of the grid, not a default grouping). Say if it must go too.
- On a phone the nine tabs share one row (it slides sideways when it does not fit).
- Receptii: the grid keeps showing every version of the header (the «Versiune antet» column), as before; the tree value is the last header's total, as in the desktop.
- No help change: the site is not part of the in-app help.

## SLICE-0110-12 - web area, COMPUTERS ONLY: details under Istoric / Plati, display menu in Extrase, navbar + header in Fundamentari / Ordonantari (operator request, 05.10.2026) <a id="slice-0110-12"></a>

Part of the site plan (`SLICE-0110`). A phone is untouched (checked: no detail, no navbar, no header / footer, no Extrase card, the document cards keep their one-column layout).

### What changed and why
1. **Istoric**: under the grid, what IstoricView shows: the **Descriere** (Observatii) of the chosen row, and a small grid **Tip | Valoare** with its non-zero values (the seven values of the desktop list: rezervare initiala / definitiva / anterioara / diferenta, angajament legal, receptie, plata). New grid id `ang.istoric-valori` in `columns.js`.
2. **Plati**: under the grid, what PlatiView shows: the bank statement line of the chosen payment (Nr. document, Data banca, Data document, Referinta, Platitor, CUI, IBAN, Suma debit, Suma credit, Explicatii); «Selectati o plata.» / «Fara extras bancar asociat.» as in the desktop view.
3. **Extrase**: the card is the ExtrasePanel of the desktop (new `extrasecard.js`): the tree has a header with the icon (gear) that opens the menu **«Arata antet + operatii» / «Arata operatii + detalii»**. Antet + operatii: root / month = headers on top and the operations of the chosen header below; a day = that day's operations and the chosen one in full. Operatii + detalii: every node = operations on top, the chosen one in full below. The tree model and the 14 detail pairs are shared with the Extrase page (`extrase.js`: `buildExtraseModel`, `detailPairs`). The grids use the layouts `extrase.antete` / `extrase.operatii`.
4. **Fundamentari and Ordonantari**: a **horizontal navbar with two options, Vizualizare | Document** (DdfView / OrdView). Vizualizare = header + lines grid (+ footer for Ordonantari); Document = the signed PDF with its bar. The PDF is loaded only when the Document page is on screen (a hidden viewer has no width to draw into). Everything stretches to the bottom of the card (checked: the bottoms of the tree, the grid and the viewer are on the same line).
5. **Header information**: Fundamentari shows what DdfVizualizarePage shows (Cod angajament, Data creare, Compartimentul, CUAL, Beneficiar with the CIF, Obiect DDF; from `antet` of `/date/ddf`). Ordonantari shows what OrdVizualizarePage shows: the **«Cauta beneficiar»** filter above the grid (narrows the lines) and, under the grid, Beneficiar, Cod fiscal, Cont IBAN, Doc. justificative, Obiect DDF of the chosen line.
6. **Repair found on the way**: a tree made without its search box (the card trees, slice 0110-10) threw in `getComputedTreeHeight` when a node was opened (`searchWrapper` is null); fixed in `treeview-utils.js` (height 0 for the missing box). Before this, the trees of the cards stayed closed with empty grids after the root was opened.

### Files touched
`PYTHON/static/js/portal/{app.js, extrasecard.js (new), extrase.js, columns.js}`, `PYTHON/static/js/components/treeview/treeview-utils.js`, `PYTHON/static/portal.html`, `PYTHON/static/css/portal.css`, `tools/SitePreview/preview_server.py` (made-up DDF header, ORD line fields, payment statement fields).

### Checked
SitePreview in the browser pane (made-up data): Istoric detail (description + values of the chosen row), Plati detail (ten pairs), Extrase menu in both modes (the checked mark follows the mode; 14 detail rows after choosing an operation), Fundamentari header and navbar, Ordonantari filter / footer / Document page, bottoms aligned; phone width unchanged. No test code written, no suite run (house rule). `node --check` on every JS file touched.

### Not done / unverified
- **Nothing seen on screen** (panel screenshots time out): look, spacing and the height of the Istoric detail (190 px) are checked only through the DOM and measured sizes.
- Real data was not used (preview only); the fields of the answers were taken from the routes' code (`routes/forexe/{ddf,ord,plati,istoric}.py`).
- Plati: «Fara extras bancar» is decided by `idfxe` being empty (and no statement fields); unverified against the real database.
- Ordonantari: the CAB notes have no lines, so their node shows an empty grid and the footer stays empty.
- No help change: the site is not part of the in-app help.
