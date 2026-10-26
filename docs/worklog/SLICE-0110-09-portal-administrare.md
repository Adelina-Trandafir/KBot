# SLICE-0110-09 / 11 — Administrarea portalului și coloanele grilelor

Istoric din 05.10.2026. 11 extinde cele trei file inițiale cu Coloane. „Valorile implicite goale” și „următorul export” sunt checkpointuri; follow-up-ul consemnează trei exporturi primite, ultimul pentru 19 grile fără Administrare. Aceasta nu confirmă aspectul, baza reală sau publicarea. Decizia privind retenția IP și limitele geoip rămân în etapa 09.

- [0110-09](#slice-0110-09)
- [0110-11](#slice-0110-11)

## SLICE-0110-09 - web area: the «Administrare» page (databases, logs, visitors) (operator request, 05.10.2026) <a id="slice-0110-09"></a>

Part of the site plan (`SLICE-0110`). One page of the portal, visible ONLY to the administrator account(s); view only.

### What changed and why
- **Who sees it.** `PORTAL_ADMIN_EMAILS` in `config.py` (a list; default `scavatarsoft@gmail.com`), read by `portal.is_admin_email`. `GET /api/portal/me` now also returns `is_admin`; the menu shows «Administrare» only when it is true. The guard `require_portal_admin` (`routes/portal/admin.py`) sits on the portal session and answers **404** to any other account (as if the routes did not exist) and 401 without a session. No unit has to be opened.
- **Page** (`static/js/portal/admin.js`, `portal.html`, `css/portal.css`; `menu.js` got `setAdmin`): one card with three tabs, all grids are the DGV of 0110-05.
  - **Baze de date** (`GET /api/portal/admin/databases`): one row per unit database + `AVACONT_COMUN` + `AVACONT_SURSA`: exists on the server, tables, size (MB), rows (estimate from `information_schema`), the three biggest tables, last write, users, years, last login / last action / active users / logins in 30 days (from `Jurnal`), errors and warnings in 24 h / 7 days (counted from the log files, cached one minute), pending schema changes / destructive / with error (from `schema_diff_log`, a note when that table does not exist). Footer totals.
  - **Jurnale** (`GET /api/portal/admin/logs/meta`, `GET /api/portal/admin/logs?file=&db=&limit=&gens=`): the five server files (`api_server.log`, `api_server_vba.log`, `forexe_timing.log`, `asociere.log`, `schema_sync.log`) with ALL users' lines. Two filter boxes above the grid: the **file** («Toate fișierele» or one) and the **database** («Toate bazele», «(fără baza de date)» = lines with no `{s= u= dc=}` tag, or one unit). Also: how many newest entries (500 … 5000) and «Și fișierele vechi (rotite)». The grid has the DGV's own per-column filter/sort; the full text of the chosen line (traceback, timing block) shows under the grid. Same parsing rules as `routes/logs.py` (slice 0089) but not limited to the caller.
  - **Vizitatori** (`GET /api/portal/admin/visits?days=&bots=`): one row per visit: date, IP, **flag + country**, device (mobil / calculator), seconds on the page, most-read section, all sections with seconds, referrer host, screen, language, robot. «Grupat pe IP» (DGV grouping with the seconds summed), «Arată și roboții», period 7 / 30 / 90 / 180 days. Under it, a second grid with the sections (visitors, seconds, average) for the chosen visit, or for every listed visit when none is chosen.
- **Counting the visits.** `static/site/visit.js` (loaded by the landing page) sends totals to the public `POST /api/vizita` (`routes/landing/vizite.py`, always answers 204): a random id per page load, seconds the page was really read (tab visible AND the person moved / scrolled / typed in the last 30 s), seconds per `[data-screen]` section (the one crossing the middle of the window), touch device, screen size, language, referrer host; at the start, every 15 s when something changed, and on leaving (`sendBeacon`). The server adds the IP (ProxyFix), the country, the browser line, the mobile / robot judgement (from the browser line; an iPad that calls itself a Mac counts as mobile when it has touch). One row per visit (`UNIQUE VisitId`): the largest total wins, so a late message changes nothing. Limits: 4 KB per message, every field cut to a known shape, 120 messages per address per 10 minutes, rows older than 180 days (`VIZITE_RETENTIE_ZILE`) deleted one request in 200.
- **Country from the IP, offline.** `utils/geoip.py` reads the DB-IP «IP to Country Lite» MMDB file through `maxminddb`; no address leaves the server. Without the package or the file, countries are empty and the page says why in plain words. The flag is the emoji of the country code plus the country name and the code (Windows draws flag emoji as two letters, so the name always follows).
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

Part of the site plan (`SLICE-0110`). Visible only to the administrator accounts (the same gate as 0110-09: `scavatarsoft@gmail.com`, and the demo account of the SitePreview).

### What changed and why
- **A fourth tab in Administrare, «Coloane»** (`static/js/portal/colprefs.js`): for ANY grid of the web area (23 of them, listed in the picker at the top as group > title) the administrator sees a table of its columns: shown (tick) / title / position (up, down) / width in px (empty = as wide as the content) / «se extinde» (the ONE column that takes the free width when the grid is wider than its columns; a second click on it clears the choice). A preview grid under the table shows the result at once; dragging a column border in the preview sets that width. If the grid was opened earlier in the session the preview uses its rows, else only the header.
- **The grid learned to do it** (`dgv/datagrid.js`): option `layout` {order, hidden, widths, fill} (or `layoutId` + `DataGrid.layoutProvider`), the fill column (the width of the others is kept, the free width is added to it and follows the window), `onColumnResize`, `getColumnState()`.
- **One catalog of columns** (`portal/columns.js`): the column sets of EVERY grid (angajament cards, Extrase page, Clasificatii, Parteneri, Administrare, plus the new Fundamentari / Ordonantari lines and lists) moved there, with a stable id each (`ang.sumar`, `extrase.operatii`, `clsf.buget`, `admin.jurnale`, ...). Every page takes its columns from it and gives its grid the id, so the editor lists them all without opening them.
- **Where the choices live** (`portal/layouts.js`): the administrator's changes are kept at once in THIS browser (`localStorage` key `kbot.portal.gridlayouts.v1`) and apply to the real grids only while the signed-in account is an administrator, so they can be tried on the real cards. What every user gets is `portal/grid-layouts.defaults.js` (empty for now = all columns, as before).
- **For the developer** : «Descarca fisierul» (`kbot-coloane-YYYYMMDD-HHMM.json`) / «Copiaza textul» give, for every changed grid, the columns in the chosen order with titles, shown or not, the chosen width, the width the preview drew, the fill column, and the size of the window the widths were chosen on. Given to the developer, it is pasted into `grid-layouts.defaults.js` (format in the comment at its top). «Incarca din fisier» reads such a file back; «Revino la valorile din cod» and «Sterge toate modificarile» undo.

### Follow-up (same day)
- The preview of the editor reads the REAL rows of the unit that is open (first angajamente with data for the cards; the unit lists for statements, partners, classifications); no invented rows. Without a unit it says so. Administrare grids use what their own tabs read.
- The operator exported choices twice; they are the defaults of every user in grid-layouts.defaults.js: ang.sumar, ang.istoric, ang.rezervari, ang.receptii, ang.extrase (hidden columns, fill column; no fixed widths).

- Third export (19 grids: every grid except Administrare) written into grid-layouts.defaults.js. The editor preview now reads the Administrare grids too (same row mapping as their tabs, exported from admin.js), with no unit needed.
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
