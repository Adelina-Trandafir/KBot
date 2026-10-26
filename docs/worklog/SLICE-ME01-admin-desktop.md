# SLICE-ME01 — KBotAdmin: programul privat și evoluția interfeței

Istoric din 10–21.10.2026, cu datele și limitele fiecărei etape. Utilizarea programului inițial din 11.10 este consemnată separat de extensiile ulterioare compilate/nerulate. Politica [privilegiilor MariaDB, ME01-03](SLICE-ME01-03-privilegii-mariadb.md) și [clonarea DEMO, ME01-04](SLICE-ME01-04-clonare-demo.md) rămân jurnale distincte. Ghidul de operare este [README KBotAdmin](../../KBotAdmin/README.md).

- [ME01 — programul și continuarea din 11.10](#slice-ME01)
- [ME01-02 — machete de drepturi](#slice-ME01-02)
- [ME01-05 — ribbon și controale](#slice-ME01-05)
- [ME01-06 — arbore, bife și diagnostic tray](#slice-ME01-06)

## SLICE-ME01 — KBotAdmin (the owner's private administration program) <a id="slice-ME01"></a>

Date: 10.10.2026. Requested by the operator. Status: done, clean build, in use by the operator since 11.10.2026 (see Follow-up).

### What changed and why

The owner wanted one place to see and manage every unit database and account. First asked as a web portal; changed to a **desktop VB.NET program, not online**, which **must never reach clients** (no publish, setup or update). Later in the same slice: the registration-requests page (approve / reject new accounts) moves from the web (`/operator`, now deleted) into the program, "too much access to be online".

#### The program (`KBotAdmin/`, outside `src/`, outside `KBot.sln`)
- WinForms, .NET 8, VB, `Option Strict On`. Uses K-BOT's own controls and theme engine through project references (`KBot.Theming`, `KBot.Controls`, `KBot.Common`); nothing references it back. All forms / pages declare their controls in `*.Designer.vb` (96 dpi, AutoScaleMode.Dpi), tooltips through `KBotToolTip`, messages through `KBotMessage`.
- **Only SSH** (SSH.NET, key from `~/.ssh` or typed password/passphrase, host-key fingerprint remembered). No database port is opened. Each call runs `python -m routes.admin_cli.admin_cli <verb>` on the server and reads one JSON object.
- Left: tree of databases (root `<TOATE BAZELE>`, one level of units, never AVACONT_SURSA / AVACONT_COMUN; red/yellow by errors and pending schema) and tree of the accounts of the chosen database. Right: pages Comenzi · Diferență schemă · Sincronizare · Drepturi pe conturi · Loguri · Jurnal activitate · Vizite · Server · Copii de siguranță · Cereri conturi noi (badge = requests waiting).
- Pages: commands = fixed whitelist chosen by the server (ping, tables, counts, processlist, journal, activity, charset check, ANALYZE); difference = read-only `build_diff` of ONE database against AVACONT_SURSA (nothing stored); sync = the `schema_sync` command line AvacontPush builds, streamed live, preview by default, destructive asks twice; rights = matrix account × ADECHIT role, a tick grants/revokes at once (journal rows `ADMIN_GRANT/REVOKE`), add / remove account on a database, change K-BOT role; logs = new control `KBotLogBox` (time in accent colour, errors dark red, tracebacks dim), filter by file / level / text / database, follow mode; the rest are grids / reports.

#### Server half (`PYTHON/routes/admin_cli/`)
`admin_cli.py` (verbs), `admin_logs.py` (log parser; reads `{u= dc=}` tags with or without `s=`), `admin_requests.py` (registration requests, same `provizionare` as before). Identifiers from the command line are checked against the server and quoted; no free SQL is accepted. Writes are journaled in `AVACONT_COMUN.Jurnal`.

#### ADECHIT (as requested)
- Cancel: **only per account** (role `AD_ANULARE`, operation `cancel`). `AllowCancelDocuments` is no longer read (`service.cancel_document`, context `settings.allowCancel`, `app.js`).
- New right `AD_EDIT_INCHISA` (operation `edit_closed`): `save_attendance` may edit the days of the **last closed month** and rewrites that child's saved situation (`rewrite_snapshot` now also stores days / contract value / total). The next open month's opening balance is derived from earlier months at calculation time, so it follows by itself. Needs `AD_PREZENTA` as well (the route guard). `sql/ME01_01_rol_editare_luna_inchisa.sql` or the KBotAdmin button creates the role.

#### Kept out of delivery
Not in `KBot.sln`; `publish-release.ps1` refuses a publish folder holding `KBotAdmin*`, `admin_settings*` or `Renci.SshNet*` (step 4c1); `push-update.ps1` refuses a package containing them. `KBotAdmin/.gitignore` hides `admin_settings.json` (server address / key path of this PC).

#### Removed
`/operator` page and API: `routes/inregistrare/operator.py`, `static/operator.html`, `static/js/operator/`, the blueprint in `main.py`; the notice mail no longer links to it (`mailer.py`). `config.OPERATORI` is now unused (left in place).

### Files touched
New: `KBotAdmin/**` (project, `AdminClient`, `AdminContext`, DTOs, SSH, `MainForm`, `ConnectionForm`, `AccountForm`, `ReasonForm`, 10 page controls with designers), `src/KBot.Controls/RichText/KBotLogBox.vb|.md`, `PYTHON/routes/admin_cli/*`, `sql/ME01_01_rol_editare_luna_inchisa.sql`, this worklog, `state/KBOT_STATUS_ME01.md`. Changed: `src/KBot.Controls/KBot.Controls.vbproj` (FileVersion 1.65.0.0), `CONTROLS.md`, `PYTHON/main.py`, `routes/auth/mailer.py`, `routes/inregistrare/README.md`, `routes/adechit/service.py|__init__.py`, `static/js/adechit/app.js`, `ADECHIT/UTILIZARE_WEB.md`, `publish-release.ps1`, `push-update.ps1`, `KBOT_STATUS.md`. Deleted: the three `/operator` files above.

### Test results
None run (project rule: no tests unless asked). Checked: `dotnet build KBotAdmin\KBotAdmin.vbproj` clean (0 warnings, 0 errors), `KBot.Controls` builds, `py_compile` on all touched Python, `node --check` on `app.js`, a scan for diacritics in new code outside string literals (none).

### Left unverified or deferred
See [Open threads](state/KBOT_STATUS_ME01.md): nothing was executed against the real server; the JSON field names between `admin_cli` and the DTOs (snake_case policy) are read from code, not from a live answer; the `LINE<TAB>` progress lines of `request-approve` and the shell quoting of arguments are untested; ADECHIT changes and tests as listed there; restore of backups, session kill and MariaDB-level password / lock were not built; 

### Follow-up 11.10.2026
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

## SLICE-ME01-02 — drepturi ADECHIT / VERCON în machetă, nu în coloane <a id="slice-ME01-02"></a>

Stare: **IMPLEMENTAT_LOCAL**, `KBotAdmin` se compilează curat (0 avertismente, 0 erori). **Nerulat**: fereastra nu a fost deschisă, nimic scris pe server. Cererea operatorului, 12.10.2026.

### Ce s-a schimbat
- Pagina «Drepturi pe conturi» (`Views/RightsView`) nu mai are coloane de bife: au rămas Cont, Baza, Rol K-BOT. Butoanele «Toate ADECHIT» și «Fără ADECHIT» au fost înlocuite cu **«Drepturi ADECHIT…»** și **«Drepturi VERCON…»**.
- Fiecare deschide `RightsForm` (nouă) pentru contul din rândul ales: un DGV cu drepturile **unul sub altul**, coloanele **Explicație** (denumirea rolului de pe server) și **Bifă**. O bifă dă/retrage dreptul pe loc; un refuz al serverului pune bifa la loc. **Click pe titlul «Bifă»** bifează tot sau, dacă tot e bifat, debifează tot, cu **un singur apel** către server (lista de coduri separată prin virgulă, pe care `grant`/`revoke` o acceptau deja).
- Serverul nu a fost modificat: rolurile `VR_*` (și operațiile lor) există deja pe server, iar `grant`/`revoke` lucrează cu orice rol din secțiunea lor.

### Fișiere
`KBotAdmin/RightsForm.vb`, `RightsForm.Designer.vb` (noi); `KBotAdmin/Views/RightsView.vb`, `RightsView.Designer.vb`.

### Neverificat
- Nimic rulat pe ecran sau pe server. Machetele noi nu au fost deschise în designerul Visual Studio.
- `AD_EDIT_INCHISA` lipsă de pe server: butonul de creare rămâne în pagină, iar rolul apare în machetă abia după ce există.
- Fără ajutor (KBotAdmin nu este pentru clienți).

## ME01-05 — KBotAdmin: ribbon și ajustări vizuale <a id="slice-ME01-05"></a>

20.10.2026. **IMPLEMENTAT ȘI COMPILAT (KBotAdmin, 0 avertismente). NERULAT: fereastra nu a fost deschisă, nici un test, nici o conexiune la server.**

### Ce s-a cerut (14 puncte ale operatorului) și ce s-a făcut

1. **Ribbon în loc de bara de navigare.** `KBotRibbon` (cel din K-BOT) cu 5 file: *Baze de date* (Comenzi, Interogări unice | Sincronizare, Clonare DEMO), *Drepturi* (Conturi și drepturi | Drepturi ADECHIT, Drepturi VERCON | Adaugă cont, Acces la o bază, Rol K-BOT | Cereri, Resetare Login), *Monitorizare* (Loguri, Jurnal, Vizite | Server), *Copii de siguranță* (Copii server | Copii locale), *Publicare* (Publicare | Rapoarte VERCON). Fiecare buton are o iconiță din stocul K-BOT (`KBot.App\Resources`, legate în `KBotAdmin.vbproj` ca resurse `icon.<nume>`; `AdminIcons.Get`). Numărul cererilor în așteptare se scrie pe butonul «Cereri» (ribbon-ul nu are insignă).
2. **Pornește maximizat** (`WindowState = Maximized` în designer; `KBotShellForm` ține maximizarea în zona de lucru).
3. **Butoanele își arată tot textul.** Cauza: `KBotButton` se desenează singur, deci `KBotTableLayoutPanel` nu cere nimic pentru el și coloana fixă rămâne cât în designer. `ButtonFit.Apply` (apelat la fiecare afișare a unei pagini) lărgește coloanele fixe până la `GetPreferredSize` al butonului, prin `SetColumnWidth` (logic, scalat cu ecranul); nu îngustează niciodată.
4. **Arborele de baze are conturile ca frunze.** Rădăcina `<TOATE BAZELE>` → bază → conturile ei; un cont legat de mai multe baze apare sub fiecare. Un clic pe un cont alege baza lui și contul (pagina «Conturi și drepturi» sare pe rândul lui). Ramurile deschise rămân deschise la reîncărcare.
5. **Arborele de conturi de jos a dispărut** (`treeAcc`, `tlyLeft`, `tlyRight`, `navViews`).
6. **Comenzi: explicație sub caseta mare.** `txtExplain` (KBotTextBox doar-citire): «Ce face», «Ce te aștepți», țintă, dacă scrie / cere o valoare. Așteptările stau în `CommandsView.Expectations` (după numele comenzii de pe server; fără modificare de server). O comandă nouă pe server primește o propoziție generală până e adăugată în listă.
7. **Pagina «Diferență» scoasă** din fereastră (nu mai e în ribbon, nici în `MainForm`). Fișierele `DiffView*.vb` au rămas în proiect, nefolosite (nu se șterg fără acordul operatorului).
8. **Sincronizarea nu mai ia în calcul și nu mai arată bazele lipsă.** Serverul le ignora deja ca țintă, dar scria avertismentul «CAI listează X, dar baza nu există». `schema_common.discover_targets` nu-l mai scrie, iar `SyncView` ascunde linia și de la un server nerepornit cu versiunea nouă. Arborele le ascundea deja.
9. **Fila «Drepturi»: butoanele care deschid o fereastră s-au mutat în ribbon**: Drepturi ADECHIT, Drepturi VERCON (RightsForm), Adaugă cont, Acces la o bază, Rol K-BOT (AccountForm). Au rămas pe pagină cele care nu deschid altă fereastră: Reîmprospătează, Scoate din bază, Aplică drepturile MariaDB, Creează rolul «Editare lună închisă». Metodele sunt acum publice în `RightsView` (`OpenAdechitAsync`, `OpenVerconAsync`, `AddAccountAsync`, `GrantDatabaseAsync`, `ChangeRoleAsync`); butonul din ribbon deschide întâi pagina.
10. **Loguri:** combo, casetă de căutare și butoane au aceeași înălțime (margini identice, 32 px).
11. **Jurnal:** la fel.
12. **Copii server + Copii locale în aceeași filă** a ribbon-ului, «Copii de siguranță».
13. **Interogări unice:** butoanele au lățime suficientă (prin `ButtonFit`, ca la punctul 3).
14. **Rapoarte VERCON: SQL-ul într-un editor.** Control nou în `KBot.Controls\RichText`: `KBotSqlBox` (cuvinte cheie, șiruri, numere, comentarii, nume între ghilimele inverse și parametrii `{{TABLE:..}}` / `%(nume)s` colorate din tema activă; lipirea e mereu text simplu). **Nu s-a folosit TiSQLEditor:** e o bibliotecă web (CodeMirror 6 + React) și cere WebView2 plus un pachet JavaScript construit cu npm, adică o dependență nouă de runtime în KBotAdmin și o integrare pe care nu o pot verifica fără să rulez fereastra. Dacă tot vrei TiSQLEditor, e o felie separată.

Cerința operatorului «toate controalele folosite vor fi cele custom»: tot ce s-a adăugat folosește controale K-BOT (`KBotRibbon`, `KBotTextBox`, `KBotSqlBox`, `KBotTableLayoutPanel`, `KBotButton`). Au rămas în desenatoarele existente `CheckBox`, `Panel` și `Label` simple: pentru `CheckBox` și `Panel` nu există control K-BOT; `KBotLabel` desenează implicit o margine de 1 px, deci schimbarea tuturor etichetelor cere o decizie (BorderWidth = 0 pe fiecare).

### Fișiere

Noi: `KBotAdmin/AdminIcons.vb`, `KBotAdmin/ButtonFit.vb`, `src/KBot.Controls/RichText/KBotSqlBox.vb`. Schimbate: `KBotAdmin/KBotAdmin.vbproj`, `MainForm.vb` + `.Designer.vb`, `Views/CommandsView.vb` + `.Designer.vb`, `Views/RightsView.vb` + `.Designer.vb`, `Views/SyncView.vb`, `Views/LogsView.Designer.vb`, `Views/AuditView.Designer.vb`, `Views/VerconReportsView.Designer.vb`, `PYTHON/routes/schema_sync/schema_common.py`, `KBotAdmin/README.md`.

### Nevalidat / de știut

- Nimic nu a fost văzut pe ecran: aspectul ribbon-ului (înălțime, încăpere pe 1000 px lățime minimă), iconițele alese, `ButtonFit` la 125 / 150 %, colorarea din `KBotSqlBox` (făcută prin selecție, cu desenarea oprită; lipirea ca text simplu) sunt gândite, nu văzute.
- `schema_common.py` (server) trebuie împins de mână de operator; până atunci filtrul din `SyncView` ascunde linia.
- Ajutorul K-BOT nu se schimbă (KBotAdmin nu ajunge la clienți). `KBot.Controls` a primit `KBotSqlBox` (FileVersion neridicat: nu s-a cerut).

## ME01-06 — KBotAdmin: arborele cu bife, două moduri, «Selectează tot» <a id="slice-ME01-06"></a>

20.10.2026. **IMPLEMENTAT ȘI COMPILAT (KBotAdmin, 0 avertismente). NERULAT: fereastra nu a fost deschisă.**

### Ce s-a cerut și ce s-a făcut

1. **Bară deasupra arborelui, lipită de el**, cu două butoane (`navMode`, `KBotNavList`): «Baze de date» (stânga) și «Utilizatori» (dreapta). În modul «Utilizatori» rădăcinile sunt utilizatorii, iar frunzele bazele fiecăruia (doar baze care există pe server). Clic pe un utilizator = contul ales, toate bazele ca context; clic pe o bază de sub el = acel cont în acea bază.
2. **Căsuță de bifat pe fiecare rând** (`CheckBoxes = True`, `HasCheckBox` pe toate rândurile). Bifa unui rând cu copii bifează și copiii; un părinte cu copii bifați doar o parte e «nedeterminat». Bifele frunzelor se păstrează la reîncărcarea arborelui și la schimbarea modului. `AdminContext.CheckedDatabases` / `CheckedAccounts` arată ce e bifat; **nicio pagină nu le folosește încă**: operatorul nu a spus ce face bifa.
3. **Titlul arborelui a dispărut** (`HeaderVisible = False`).
4. **«Selectează tot»** sub arbore (`chkAll`): bifează sau debifează toate rândurile; se aliniază singur cu arborele (e bifat doar când toate rădăcinile sunt bifate).

Adăugat în plus, din cerința «toate controalele custom»: **`KBotCheckBox`** (`KBot.Controls\CheckBox`), un `CheckBox` desenat din temă (aceeași suprafață ca `CheckBox`). Toate căsuțele simple din `KBotAdmin` au trecut pe el (Loguri, Vizite, Sincronizare, Publicare, Editorul de raport, fereastra de parolă).

Comenzi: caseta «Ce face» e acum de două ori mai înaltă (240 px) și cu fontul cu 2 pt mai mare (Segoe UI 11).

### Fișiere
Nou: `src/KBot.Controls/CheckBox/KBotCheckBox.vb`. Schimbate: `KBotAdmin/MainForm.vb` + `.Designer.vb`, `AdminContext.vb`, `Views/CommandsView.Designer.vb`, designerele cu căsuțe de bifat.

### Nevalidat
Nimic văzut pe ecran: desenarea `KBotCheckBox`, bifele din arbore (propagare, stări parțiale), comutarea modurilor și rândul «Utilizatori» la clic sunt gândite, nu văzute. Bifarea unui rând îl și alege (comportamentul arborelui), deci schimbă baza-context.

### Adăugat după prima predare

- **Arborele se redimensionează cu mausul:** un `SplitContainer` (`splMain`) între arbore și pagini (bara de 8 px; arborele minim 180 px, paginile minim 400 px; la redimensionarea ferestrei arborele își păstrează lățimea). Nu există un separator K-BOT; `ThemeManager` colorează `SplitContainer`-ul, ca în restul programului.
- **Imaginea rămasă după prima închidere în systray:** s-a observat că dispare doar la a doua închidere. Presupunere (nevăzută pe ecran): animația de ascundere a Windows lasă poza când fereastra își face din nou mânerul (`ShowInTaskbar`) imediat după `Hide()`. Animațiile ferestrei sunt oprite (`DwmSetWindowAttribute` în `OnHandleCreated`); repictarea zonei de după ascundere a rămas.
- **Jurnal de diagnostic pentru închiderea în systray** (imaginea tot rămânea): `KBotAdmin/TrayLog.vb` scrie `<AppDir>\Logs\minimizare_tray.log`. Per închidere: starea ferestrei (mâner, vizibilă, stare, dreptunghi, stil) înainte de `Hide`, după `Hide` și după `ShowInTaskbar = False`; mesajele ferestrei (afișare, ascundere, mutare, pictare, creare, distrugere) timp de 3 s; lista tuturor ferestrelor programului și a formularelor deschise imediat, la 0,4 s și la 2 s (poza ar putea fi o a doua fereastră: tooltip, fly-out). Nu schimbă comportamentul ferestrei. Cauza rămâne necunoscută până se citește jurnalul.
- **Cauza imaginii rămase, găsită cu jurnalul** (`_Admin\Logs\minimizare_tray.log`, 21.10.2026): după `Hide()`, `ShowInTaskbar = False` face un mâner nou, iar pentru fereastra maximizată noul mâner e creat VIZIBIL (WinForms `Visible=False`, sistemul `IsWindowVisible=True`, stilul cu `WS_VISIBLE`). A doua închidere îl ascundea fiindcă mânerul nu mai era refăcut. Remediu: `MainForm.HideNativeWindow` ascunde (`ShowWindow SW_HIDE`) fereastra nativă după `ShowInTaskbar = False`. Netestat pe ecran; jurnalul rămâne pornit ca să se vadă dacă `after HideNativeWindow` arată `IsWindowVisible=False`. Oprirea animațiilor DWM și repictarea zonei (încercări anterioare) au rămas.
