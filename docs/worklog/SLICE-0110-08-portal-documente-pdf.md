# SLICE-0110-08 - web area: the signed documents (DDF, ORD, notes) in a read-only PDF viewer (operator request, 05.10.2026)

Follows the verdict of `SLICE-0110-07`. View only: nothing is written, signatures are not checked here.

## What changed and why
- Server, `PYTHON/routes/portal/date.py` (same method as 0110-06: the desktop functions, undecorated through `__wrapped__`,
  the unit taken from the portal session): three more list readers `GET /api/portal/date/{ddf,ord,note}?cod=` and three byte
  routes `GET /api/portal/date/{ddf-pdf/<idrev>, ord-pdf/<idordp>, nc-pdf/<idnc>}`. The byte routes send exactly what the
  desktop route sends (raw bytes, `ETag` = the stored SHA-256, 404 with the Romanian sentence when no signed PDF is stored); the
  portal adds `Cache-Control: private, no-store`, `Content-Disposition: inline`, `nosniff`. All GET; PUT is 405.
- pdf.js is vendored: `PYTHON/static/vendor/pdfjs/` (pdfjs-dist 4.10.38, Apache-2.0, `README.md` says where each file comes from
  and how to update; the `.mjs` files are stored as `.js` so every server sends a JavaScript type). Loaded only when a document is
  opened (about 2 MB).
- `static/js/portal/pdfview.js` - `createPdfView(container).open(bytes) / clear()`: pdf.js viewer with `enableXfa: true` and
  `isEvalSupported: false`; the document is copied before pdf.js gets it; an older request never replaces a newer one; errors come
  back as sentences the operator can read. `static/css/pdfview.css` makes the form fields inert (`pointer-events: none`) and
  replaces the stock loading image (which we do not ship) with a text mark.
- Page: new tab «Documente» in `portal.html` / `app.js`: one grid for the three kinds (Document, Număr, Data, Suma, Semnături,
  PDF semnat) built from the three list answers, the viewer under it, the sentence «Vizualizare a documentului semnat.
  Semnăturile se verifică în K-BOT.», and «Descarcă PDF-ul semnat» (the exact bytes, as a file named like
  `ORD_1_<cod>.pdf`). One signed document opens by itself; with several the person chooses; a row without a stored signed PDF says
  so. Lists are cached per angajament.

## Files touched
`PYTHON/routes/portal/date.py`, `PYTHON/static/portal.html`, `PYTHON/static/js/portal/{app.js,portal.js,pdfview.js (new)}`,
`PYTHON/static/css/pdfview.css (new)`, `PYTHON/static/vendor/pdfjs/*` (new, 6 files).

## Checked
- Flask client, connection replaced by a recorder: no token 401 for the three byte routes; the unit used is the portal's; bytes
  identical to what was stored (including a NUL and high bytes), ETag equal to the stored SHA-256, the three headers present; a missing
  document 404 with the sentence; non-numeric id 404; PUT 405.
- Browser pane, served with the portal's real CSP (`script-src 'self'`, no inline script), stub data but the REAL sample PDFs of the
  repository: list of 4 documents (DDF rev. 1 and 2, ORD, note); the signed ORD (8 signatures) opens with its final values, the
  signed note opens, the unsigned DDF (pure XFA) opens through the XFA engine (4 pages); the row with no PDF shows the message and no
  viewer; the download button produces a blob of exactly the size and type of the file on the server; XFA fields report
  `pointer-events: none`; no CSP error and, after the CSS fix, no 404. Looked at on screen: the tab with list, note and the ORD.

## Not done / to do
- **A real signed DDF produced by the K-BOT signing flow was not available** (the DDF in the test is an unsigned sample); open one
  before telling users it works for every DDF. Same for documents with attachments inside the PDF, very large PDFs and phones.
- Signatures are not verified or even listed beyond the role letters the server stores (`semnatura`); the viewer says so.
- pdf.js does not run the form's scripts: a total that a script computes on opening may differ from Adobe's.
- The real database, SMTP and nginx were not involved; server not deployed. Needs `/static/vendor/pdfjs/` to be served (it is under
  the static folder Flask already serves; check that nginx does not filter `.js` under it).
- No help change (the site is not part of the in-app help).
