# SLICE-0110-09 / 11 — Administrarea portalului și coloanele grilelor

Istoric din 05.10.2026. 11 extinde cele trei file inițiale cu Coloane. „Valorile implicite goale” și „următorul export” sunt checkpointuri; follow-up-ul consemnează trei exporturi primite, ultimul pentru 19 grile fără Administrare. Aceasta nu confirmă aspectul, baza reală sau publicarea. Decizia privind retenția IP și limitele geoip rămân în etapa 09.

- [0110-09](#slice-0110-09)
- [0110-11](#slice-0110-11)

## SLICE-0110-09 - web area: the «Administrare» page (databases, logs, visitors) (operator request, 05.10.2026) <a id="slice-0110-09"></a>

Part of the site plan (`SLICE-0110`). One page of the portal, visible ONLY to the administrator account(s); view only.

### What changed and why
- **Access.** `config.py` `PORTAL_ADMIN_EMAILS` (list, default `scavatarsoft@gmail.com`) feeds `portal.is_admin_email`; `GET /api/portal/me` returns `is_admin` for menu visibility. `require_portal_admin` (`routes/portal/admin.py`) uses the portal session: 404 for other accounts, 401 without session, no open unit required.
- **Page** (`static/js/portal/admin.js`, `portal.html`, `css/portal.css`; `menu.js` got `setAdmin`): one card with three tabs, all grids are the DGV of 0110-05.
  - **Baze de date** (`GET /api/portal/admin/databases`): one row per unit database + `AVACONT_COMUN` + `AVACONT_SURSA`: exists on the server, tables, size (MB), rows (estimate from `information_schema`), the three biggest tables, last write, users, years, last login / last action / active users / logins in 30 days (from `Jurnal`), errors and warnings in 24 h / 7 days (counted from the log files, cached one minute), pending schema changes / destructive / with error (from `schema_diff_log`, a note when that table does not exist). Footer totals.
  - **Jurnale** (`GET /api/portal/admin/logs/meta`, `GET /api/portal/admin/logs?file=&db=&limit=&gens=`): all users in `api_server.log`, `api_server_vba.log`, `forexe_timing.log`, `asociere.log`, `schema_sync.log`. File filter: all/one; database filter: all/one/«(fără baza de date)» (no `{s= u= dc=}` tag). Newest 500…5000 entries, optional rotated files, DGV column filters/sort; selected full traceback/timing text below. Parsing follows `routes/logs.py` (0089), without caller restriction.
  - **Vizitatori** (`GET /api/portal/admin/visits?days=&bots=`): visit date, IP, flag/country, mobile/computer, reading seconds, most-read/all sections with seconds, referrer host, screen, language, robot. Controls: Group by IP (sum seconds), show robots, 7/30/90/180 days. Second grid: sections with visitors/seconds/average for selected visit, or all listed visits if none selected.
- **Visit counting.** Landing `static/site/visit.js` posts to public `POST /api/vizita` (`routes/landing/vizite.py`, always 204): random id/page load, seconds read (visible tab AND movement/scroll/typing within 30s), seconds per `[data-screen]` crossing the window midpoint, touch device, screen size, language, referrer host. Sends initially, every 15s if changed, and by `sendBeacon` on leaving. Server adds IP (ProxyFix), country, browser line and mobile/robot judgement; touch iPad reporting Mac is mobile. `UNIQUE VisitId`, largest total wins, late messages cannot reduce it. Limits: 4 KB/message, shaped fields, 120 messages/address/10min; one request in 200 deletes rows older than `VIZITE_RETENTIE_ZILE` (180 days).
- **Offline country.** `utils/geoip.py` reads DB-IP «IP to Country Lite» MMDB through `maxminddb`, without sending IPs out. Missing package/file gives empty countries and a plain explanation. Flag emoji is followed by country name/code because Windows renders it as two letters.
- `tools/SitePreview/preview_server.py` now also fakes the admin routes: REAL log files made in a temp folder and read by the real parsers, made-up database answers, three seeded visits and every real visit to the presentation page counted in memory. Sign in as `scavatarsoft@gmail.com` (password `demo`, code `123456`).

### Files touched
`PYTHON/routes/portal/admin.py` (new), `PYTHON/routes/portal/portal.py` (`admin_emails`, `is_admin_email`, `is_admin` in `/me`), `PYTHON/routes/landing/vizite.py` (new), `PYTHON/utils/geoip.py` (new), `PYTHON/main.py` (two imports, one blueprint), `PYTHON/static/js/portal/{admin.js (new),menu.js,portal.js,app.js}`, `PYTHON/static/{portal.html,css/portal.css,site/visit.js (new)}`, `PYTHON/routes/landing/templates/landing/index.html` (one script tag), `sql/0110_09_vizite_site.sql` (new), `tools/SitePreview/preview_server.py`, `.gitignore` (`PYTHON/data/*.mmdb`).

### Checked
- `py_compile` on every Python file touched; the JS files pass `node --check`.
- Site preview in the browser pane (made-up data): the admin account sees the menu entry, a normal account does not and gets 404 on all four admin routes (401 with no token); database grid with the made-up rows and totals; the log routes with real files: all / one file / one database / «fără baza de date» / a bad file name (400), tracebacks and timing blocks come as one entry with the detail; a REAL visit to the presentation page was counted (`POST /api/vizita` -> 204) and showed in «Vizitatori» with its sections, flags and countries of the seeded rows showed, grouping by IP summed the seconds, the section names have their diacritics. No console errors after the preview's demo cursor was fixed.
- No test code was written and no test suite was run (house rule).

### Not done / to do (unverified)
- **Nothing was seen on screen**: the browser pane would not draw during the session (screenshots timed out), so the look (layout of the three tabs, the grids' height, phone width, dark theme) is checked only through the DOM text.
- The real database was not touched: the SQL of the three pages (`information_schema`, `Jurnal`, `schema_diff_log`, `Vizite_Site`) is written from `MariaDB_Schema/` but never ran on MariaDB. `UPDATE_TIME` of InnoDB tables can be empty after a server restart («Ultima scriere»).
- The seconds-per-section counting could not be timed in the pane (the page counts only while visible, and the pane was hidden); the logic is the plain 1-second tick in `visit.js`.
- Section names in `admin.js` (`SECTION_NAMES`) are my reading of the ids of the landing page's `[data-screen]` blocks; an id not listed is shown from the id itself.
- **Privacy:** the table keeps the visitor's IP address for 180 days with no consent banner, and the landing page says it has no cookies (true, but it does not mention this). IP addresses are personal data under GDPR: decide whether the site needs a line about it. Do-Not-Track is NOT honoured.
- Robots are only guessed from the browser line; they are hidden by default and a tick shows them.
- Server not deployed. **Deploy order:** (1) `sql/0110_09_vizite_site.sql` on the K-BOT server; (2) `pip install maxminddb` in the server's venv; (3) download `dbip-country-lite-YYYY-MM.mmdb` from https://db-ip.com/db/download/ip-to-country-lite, unpack, save as `PYTHON/data/dbip-country-lite.mmdb` (or point `GEOIP_DB_PATH` in `config.py` at it), refresh about monthly; DB-IP asks for a credit, which the Vizitatori note already shows; (4) optionally `PORTAL_ADMIN_EMAILS = [...]` in `config.py`; (5) restart gunicorn. Visits before step 3 have no country stored; the page fills the country in for them when it reads them (nothing is written back).
- No help change: the site is not part of the in-app help.

## SLICE-0110-11 - web area: the «Coloane» card of Administrare (which columns show, where, how wide) (operator request, 05.10.2026) <a id="slice-0110-11"></a>

Part of `SLICE-0110`, using the administrator gate of [0110-09](#slice-0110-09), including its SitePreview demo account.

### What changed and why
- **Coloane**, fourth Administrare tab (`static/js/portal/colprefs.js`): picker of all 23 web grids as group > title; columns have shown tick, title, up/down position, width px (empty = content) and «se extinde» (one fill column; second click clears). Preview updates immediately; dragging its border sets width. Uses rows if opened earlier in the session, otherwise header only.
- **DGV support** (`dgv/datagrid.js`): `layout` {order, hidden, widths, fill}, or `layoutId` + `DataGrid.layoutProvider`; fill preserves other widths and absorbs free width/resizing. API `onColumnResize`, `getColumnState()`.
- **Column catalog** (`portal/columns.js`): every web grid, including angajament cards, Extrase, Clasificatii, Parteneri, Administrare and new Fundamentari/Ordonantari lists/lines, has a stable id (`ang.sumar`, `extrase.operatii`, `clsf.buget`, `admin.jurnale`, ...). Pages read the catalog and pass the id; the editor lists grids without opening them.
- **Storage** (`portal/layouts.js`): edits persist immediately in this browser, `localStorage` `kbot.portal.gridlayouts.v1`, and apply to real grids only for signed-in administrators. Shared defaults: `portal/grid-layouts.defaults.js`, initially empty = all columns as before.
- **Handoff:** «Descarca fisierul» (`kbot-coloane-YYYYMMDD-HHMM.json`) / «Copiaza textul» export each changed grid: order, titles, shown/hidden, chosen/drawn widths, fill and window size. Developer inserts it into `grid-layouts.defaults.js` using its header format. «Incarca din fisier» imports; «Revino la valorile din cod» / «Sterge toate modificarile» undo.

### Follow-up (same day)
- Follow-up: editor preview reads real open-unit rows (first angajamente with data for cards; unit statements/partners/classifications), without invented rows; no unit gives an explanation. Administrare uses data read by its tabs.
- The operator exported choices twice; they are the defaults of every user in grid-layouts.defaults.js: ang.sumar, ang.istoric, ang.rezervari, ang.receptii, ang.extrase (hidden columns, fill column; no fixed widths).

- Third export: defaults for 19 grids, all except Administrare. Preview now also reads Administrare via the row mapping exported by admin.js for its tabs, without an open unit.
- Tree text on computers is 2 pt smaller (11.67 px); set on the tree hosts because the tree reads its container font before it adds its own class.
- The grid's fill column counts the vertical scrollbar (measured when present, estimated when the rows will bring it).

### Files touched
`PYTHON/static/js/portal/{colprefs.js, layouts.js, columns.js, grid-layouts.defaults.js (all new), admin.js, app.js, extrase.js, clasificatii.js, parteneri.js, portal.js}`, `PYTHON/static/js/dgv/datagrid.js`, `PYTHON/static/css/{portal.css, dgv.css}` (the last header grip stays inside the header), `PYTHON/static/portal.html`. No server change (the choices never leave the browser).

### Checked
- SitePreview in the browser pane (made-up data): the tab opens with the first grid; hiding a column, moving another, giving a width and choosing the fill column change the table, the preview (header widths added up to the grid width: the fill column took the free 536 px, no sideways scroll after the grip fix) and the stored value; the export text has the shape above; the real Sumar grid then showed the hidden / moved / widened columns; «Revino la valorile din cod» emptied the store. Extrase page, Administrare (all four tabs) open without console errors.
- `node --check` on every JS file touched. No test code was written and no test suite was run (house rule).

### Not done / to do (unverified)
- **Nothing was seen on screen** (pane screenshots time out): the look of the table (column widths of the editor, phone width) is checked only through the DOM.
- Clasificatii and Parteneri could not be opened in the preview (their readers are not faked, the preview has no database): their grids take the catalog the same way as the others and were only syntax-checked; the first run on the real server is their first test.
- One layout per grid for BOTH computer and phone (a hidden column is hidden on the phone too; the fill column absorbs free width on any screen). Say if the phone needs its own.
- The layout applies when a card is opened again, not to a card already on screen.
- **Next step:** the administrator sets the columns, downloads the file and gives it; the developer writes the positions into `grid-layouts.defaults.js`.
- No help change: the site is not part of the in-app help.
