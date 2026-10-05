# SLICE-0110-06 - web area: the read-only views (operator request, 05.10.2026)

Part of the site plan (`SLICE-0110`). Uses the guard of 0110-03 and the grid of 0110-05. View only: no editing, no PDF,
nothing about FOREXE.

## What changed and why
- Server, `PYTHON/routes/portal/date.py`: `GET /api/portal/date/{tree,sumar,rezervari,receptii,plati}` behind
  `require_portal_session`. THE QUERIES ARE NOT COPIED: each route calls the function the desktop app's route already runs
  (`routes/forexe/*`), the undecorated one through `__wrapped__`, after setting `g.session.db_name` from the portal session.
  Those functions read only the query string and `g.session.db_name`, so a portal token reaches the unit it opened and no other,
  and the answers have exactly the desktop shape. The module fails at import if a function loses `__wrapped__`. No route of the
  desktop app was touched. Only these five readers (all SELECT) are reachable; the module header says to read a function before
  adding one.
- Page, `static/js/portal/app.js` (+ `portal.html`, `css/portal.css`): toolbar with Unitatea / Anul / Sursa (the existing
  `Combobox`; sources of the chosen year plus «Toate sursele»), the list of angajamente (DataGrid: cod, descriere, stare, surse,
  creat, has-Rezervari/Recepții/Plăți/DDF/ORD), and for the picked angajament four tabs - Sumar (totals per indicator, footer
  totals), Rezervări (grouped by month, aggregates in the band), Recepții (grouped by month), Plăți (grouped by month, totals).
  Answers are cached per (angajament, tab) and an older answer never replaces a newer one. A 401 sends the user back to the
  sign-in card; any other refusal shows the server's sentence. Layout: two panes on desktop, stacked under 900 px.
- `portal.js` now only signs in and hands the account to `createApp`; the unit picker moved from the header to the toolbar.
- The event bus complained in the console about every event nobody listened to (`user-activity` on each click); the page
  registers a quiet listener for the session events the monitor emits.
- Grid fixes found on screen (`datagrid.js`): a group band's caption now spans the leading columns that have no aggregate
  (it was cut at the width of the first column); automatic widths measure the title in bold and leave more room (dates were
  clipped).

## Files touched
`PYTHON/routes/portal/date.py` (new), `PYTHON/main.py` (imports the module), `PYTHON/static/portal.html`,
`PYTHON/static/css/portal.css`, `PYTHON/static/js/portal/{portal.js,app.js (new)}`, `PYTHON/static/js/dgv/datagrid.js`.

## Checked
- Flask client with the database connection replaced by a recorder: no token and a desktop Bearer header both 401; five readers
  answer with the portal's unit (`111_X`) even when the query carries `db_name=000_DEMO`; missing `cod` / `an` / `ss` 400; POST 405.
- In the browser pane (stub with made-up data): sign-in, unit picked through the Combobox, year and source filled from the
  periods, list of 40 angajamente, each tab with its columns, month grouping and footer totals, cache (no second request for a tab
  already seen), resume after a page reload with the stored token, stacked layout at phone width, no console errors.
  Screenshots looked at: desktop list + Rezervări with grouping.

## Not done / to do
- Real database NOT exercised: the shape of the real rows comes from the desktop functions, but nothing was read from MariaDB.
- **Recepții is the raw list** (one row per header snapshot and indicator, grouped by month). The desktop view builds a
  month / reception / header tree with an aggregated list (slice 0015); that logic is in the VB view and was not ported. Same
  for the payment tooltip and the extras detail panel of Plăți (the bank statement fields are in the answer but not shown).
- No `include_hidden` switch, no last-used source per user (default «Toate sursele»), no role-based restriction (all roles see
  everything their unit has, as decided).
- Dark theme and the filter popup were not looked at on screen in this page.
- Server not deployed.
