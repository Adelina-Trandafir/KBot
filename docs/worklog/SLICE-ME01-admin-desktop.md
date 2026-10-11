# SLICE-ME01 — KBotAdmin (the owner's private administration program)

Date: 10.10.2026. Requested by the operator. Status: done, clean build, in use by the operator since 11.10.2026 (see Follow-up).

## What changed and why

The owner wanted one place to see and manage every unit database and account. First asked as a web portal; changed
to a **desktop VB.NET program, not online**, which **must never reach clients** (no publish, setup or update).
Later in the same slice: the registration-requests page (approve / reject new accounts) moves from the web
(`/operator`, now deleted) into the program, "too much access to be online".

### The program (`KBotAdmin/`, outside `src/`, outside `KBot.sln`)
- WinForms, .NET 8, VB, `Option Strict On`. Uses K-BOT's own controls and theme engine through project references
  (`KBot.Theming`, `KBot.Controls`, `KBot.Common`); nothing references it back. All forms / pages declare their
  controls in `*.Designer.vb` (96 dpi, AutoScaleMode.Dpi), tooltips through `KBotToolTip`, messages through `KBotMessage`.
- **Only SSH** (SSH.NET, key from `~/.ssh` or typed password/passphrase, host-key fingerprint remembered). No database
  port is opened. Each call runs `python -m routes.admin_cli.admin_cli <verb>` on the server and reads one JSON object.
- Left: tree of databases (root `<TOATE BAZELE>`, one level of units, never AVACONT_SURSA / AVACONT_COMUN; red/yellow
  by errors and pending schema) and tree of the accounts of the chosen database. Right: pages
  Comenzi · Diferență schemă · Sincronizare · Drepturi pe conturi · Loguri · Jurnal activitate · Vizite · Server ·
  Copii de siguranță · Cereri conturi noi (badge = requests waiting).
- Pages: commands = fixed whitelist chosen by the server (ping, tables, counts, processlist, journal, activity, charset
  check, ANALYZE); difference = read-only `build_diff` of ONE database against AVACONT_SURSA (nothing stored);
  sync = the `schema_sync` command line AvacontPush builds, streamed live, preview by default, destructive asks twice;
  rights = matrix account × ADECHIT role, a tick grants/revokes at once (journal rows `ADMIN_GRANT/REVOKE`), add /
  remove account on a database, change K-BOT role; logs = new control `KBotLogBox` (time in accent colour, errors dark
  red, tracebacks dim), filter by file / level / text / database, follow mode; the rest are grids / reports.

### Server half (`PYTHON/routes/admin_cli/`)
`admin_cli.py` (verbs), `admin_logs.py` (log parser; reads `{u= dc=}` tags with or without `s=`),
`admin_requests.py` (registration requests, same `provizionare` as before). Identifiers from the command line are checked
against the server and quoted; no free SQL is accepted. Writes are journaled in `AVACONT_COMUN.Jurnal`.

### ADECHIT (as requested)
- Cancel: **only per account** (role `AD_ANULARE`, operation `cancel`). `AllowCancelDocuments` is no longer read
  (`service.cancel_document`, context `settings.allowCancel`, `app.js`).
- New right `AD_EDIT_INCHISA` (operation `edit_closed`): `save_attendance` may edit the days of the **last closed month**
  and rewrites that child's saved situation (`rewrite_snapshot` now also stores days / contract value / total). The next
  open month's opening balance is derived from earlier months at calculation time, so it follows by itself. Needs
  `AD_PREZENTA` as well (the route guard). `sql/ME01_01_rol_editare_luna_inchisa.sql` or the KBotAdmin button creates the role.

### Kept out of delivery
Not in `KBot.sln`; `publish-release.ps1` refuses a publish folder holding `KBotAdmin*`, `admin_settings*` or
`Renci.SshNet*` (step 4c1); `push-update.ps1` refuses a package containing them. `KBotAdmin/.gitignore` hides
`admin_settings.json` (server address / key path of this PC).

### Removed
`/operator` page and API: `routes/inregistrare/operator.py`, `static/operator.html`, `static/js/operator/`, the blueprint in
`main.py`; the notice mail no longer links to it (`mailer.py`). `config.OPERATORI` is now unused (left in place).

## Files touched
New: `KBotAdmin/**` (project, `AdminClient`, `AdminContext`, DTOs, SSH, `MainForm`, `ConnectionForm`, `AccountForm`,
`ReasonForm`, 10 page controls with designers), `src/KBot.Controls/RichText/KBotLogBox.vb|.md`,
`PYTHON/routes/admin_cli/*`, `sql/ME01_01_rol_editare_luna_inchisa.sql`, this worklog, `state/KBOT_STATUS_ME01.md`.
Changed: `src/KBot.Controls/KBot.Controls.vbproj` (FileVersion 1.65.0.0), `CONTROLS.md`, `PYTHON/main.py`,
`routes/auth/mailer.py`, `routes/inregistrare/README.md`, `routes/adechit/service.py|__init__.py`,
`static/js/adechit/app.js`, `ADECHIT/UTILIZARE_WEB.md`, `publish-release.ps1`, `push-update.ps1`, `KBOT_STATUS.md`.
Deleted: the three `/operator` files above.

## Test results
None run (project rule: no tests unless asked). Checked: `dotnet build KBotAdmin\KBotAdmin.vbproj` clean (0 warnings, 0 errors),
`KBot.Controls` builds, `py_compile` on all touched Python, `node --check` on `app.js`, a scan for diacritics in new code
outside string literals (none).

## Left unverified or deferred
See [Open threads](state/KBOT_STATUS_ME01.md): nothing was executed against the real server; the JSON field names between
`admin_cli` and the DTOs (snake_case policy) are read from code, not from a live answer; the `LINE<TAB>` progress lines of
`request-approve` and the shell quoting of arguments are untested; ADECHIT changes and tests as listed there; restore of
backups, session kill and MariaDB-level password / lock were not built; 

## Follow-up 11.10.2026
- The web admin page of the portal (`/portal` > Administrare: databases, logs, visitors, Coloane) was removed: `routes/portal/admin.py`, `static/js/portal/admin.js`, `colprefs.js`, the page markup/CSS, the menu entry, the `admin.*` grid catalog, `is_admin` in `/api/portal/me`. `layouts.js` keeps its (now never switched on) administrator override code.
- KBotAdmin: tooltips on every control (labels, buttons, fields, grids, log boxes, nav bar as one list) and a header tooltip on every grid column.
- Page **Publicare** (AvacontPush inside KBotAdmin; AvacontPush itself is untouched): sub-pages Fisiere (SFTP scan/compare/push + service restart, grids of accepted/ignored extensions with counts and of ignore rules, edited in place and saved in `admin_settings.json`), Interogari unice (`routes.one_time.runner`, base64 command line), Login (admin API, key read over SSH). New: `Ssh/SftpSession.vb`, `Server/PushIgnore.vb|PushService.vb|OnceQuery.vb|LoginApi.vb`, `Views/Publish*|PushFilesView|OnceQueryView|LoginResetView|LogSink`. Never run against the server; nav labels shortened so 11 pages fit.
- `admin_logs.py`: untimed lines of gunicorn error.log were all stamped with the file time; now a traceback is one entry with the time of the next stamped line.
- KBotAdmin is in `KBot.sln` for Debug only (no Release build); KBotLogBox takes the focus on mouse enter so the wheel scrolls it.
- Menu reshaped (operator, 11.10.2026): no sub-buttons; pages Interogari unice and Resetare Login are in the main bar; Publicare (the file push) is right-aligned (KBotNavItem.Align = Far); `PublishView` removed; window 1500x760 so 13 entries fit. KBotAdmin theme is ALWAYS Classic: new `ThemeManager.InitializeFixed` (KBot.Theming 1.14.0.0) ignores the theme K-BOT saved, writes nothing to AppData, and makes `SetScheme` a no-op.
- Publicare trace: `Logs\publicare.log` (one line per step of the push).
- Icon `KBotAdmin/KBotAdmin.ico` (K-BOT window icon with an X: blue rounded square, white letter): exe icon (taskbar), embedded for every window, the caption bar and the tray. Closing (X, Alt+F4) hides the window into the tray (no taskbar button); the tray menu (left or right click) has «Afisează fereastra» and «Închide» (asks if an operation runs). `ModalGuard` + `WndProc` of MainForm/ConnectionForm bring an open message box back in front when the program is re-activated. Theme button removed from the caption bar. Not published (the operator tests first).
- Backups page (11.10.2026): a chosen database keeps only its dumps (`<db>_<stamp>.sql`) and the plans whose text names it; double click opens `SqlTextForm` (modal, read-only, first 2 MB; server verb `backup-text`, path checked against `backup/` and `schema_diff/`).
- Interogari unice: history grid (`once-history`: the ledger of AVACONT_SURSA + done / still-to-run per unit), double click shows the query text.
- Logs: `access.log`, `help_feedback.log`, `one_time.log` added (access only on request, last 6 MB).
- Start-up reads only databases and accounts (no page is opened).
- `AdminClient.LiveSession`: reconnects a dropped SSH session; `SshClient.KeepAliveInterval` = 30 s.
- Local publish: `publish-admin.ps1` -> `_Admin\` (git-ignored, outside every K-BOT package; keeps `admin_settings.json`). README rewritten.
