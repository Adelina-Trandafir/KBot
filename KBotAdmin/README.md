# KBotAdmin

The owner's private administration program (slice ME01). **Not part of K-BOT's delivery: it must never be sent to a client or put in any K-BOT package, setup or update.** `publish-release.ps1` / `push-update.ps1` refuse any package that contains it. It is in `KBot.sln` for **Debug only** (so it shows in the start-up project list and F5 runs it; a Release build of the solution never builds it).

```powershell
dotnet build KBotAdmin\KBotAdmin.vbproj
dotnet run --project KBotAdmin\KBotAdmin.vbproj
.\publish-admin.ps1        # -> _Admin\KBotAdmin.exe (git-ignored; close the running program first)
```

`publish-admin.ps1` is the owner's own copy to run separately (needs the .NET 8 Desktop Runtime); it keeps the `admin_settings.json` of `_Admin` across publishes.

## Connection
- Only SSH (SSH.NET): key from `~/.ssh`, or a typed password / passphrase that is never saved. No database port is opened. Settings: `admin_settings.json` next to the exe (git-ignored; copy `admin_settings.example.json`): host, port, user, key path, `RemoteRoot`, `RemotePython`, pinned host-key fingerprint, `LocalRoot` (empty = the `PYTHON` folder next to `KBot.sln`), `PreserveMTime`, `ApiBaseUrl`, `IgnorePatterns`, `IncludeExtensions`.
- A dropped session (idle timeout, firewall) is reconnected on the next command; a keep-alive is sent every 30 s.
- Server half: `PYTHON/routes/admin_cli/` (push it with AvacontPush after every change there). The program runs `python -m routes.admin_cli.admin_cli <verb>` and reads one JSON object back; no free SQL is ever sent.

## Window

Account rights also synchronize the user's real MariaDB privileges (ME01-03). Before publishing this version, manually set up the separate privileged account using `sql/SEC03_kbotadmin_privilegii.sql` and server-side `DB_CONFIG_DREPTURI`. New personal MariaDB accounts can start with USAGE only (CREATE USER with a password, without initial ALL PRIVILEGES); assigning their application roles then grants the necessary data access. No schema changes or setup SQL are run automatically. Missing configuration refuses the rights change. Use **Aplică drepturile MariaDB** on existing accounts to remove their old broad grants without changing application roles. It applies to all assigned databases and all host variants of that username and closes the user's MariaDB connections. Accountants get data DML in their own databases, without server/schema administration. See `docs/worklog/SLICE-ME01-03-privilegii-mariadb.md` for policy and recovery details.

- Always the **Classic** theme (`ThemeManager.InitializeFixed`), whatever theme K-BOT has saved; nothing is written to the theme settings. Uses `KBot.Controls` and `KBot.Theming` by project reference.
- Icon `KBotAdmin.ico` (K-BOT's blue rounded square with an X): exe, windows, caption bar, tray.
- The X button / Alt+F4 hides the window into the **tray** (no taskbar button). The tray icon menu (left or right click): «Afișează fereastra» and «Închide» (the only way to quit; it asks when an operation is running).
- A message box left open while the operator switches away and comes back is brought back in front (`ModalGuard`).
- At start only the databases and the accounts are read; every page reads its own data when it is opened.
- Errors: `<AppDir>\Logs\harness_errors.log`; messages shown to the operator: `Logs\mesaje_operator.log`; the file push traces every step in `Logs\publicare.log`.

## Pages (ribbon, ME01-05)
The window opens maximized. The ribbon has five tabs: **Baze de date** (Comenzi, Interogări unice, Sincronizare, Clonare DEMO), **Drepturi** (the page, the ADECHIT / VERCON rights windows, add account, access to a database, K-BOT role, Cereri, Resetare Login), **Monitorizare** (Loguri, Jurnal, Vizite, Server), **Copii de siguranță** (Copii server, Copii locale), **Publicare** (Publicare, Rapoarte VERCON). The left side is ONE tree with a tick on every row; the bar above it chooses «Baze de date» (the databases and, under each, its accounts; an account linked to several databases shows under each) or «Utilizatori» (the users and, under each, their databases); «Selectează tot» under it ticks every row (ME01-06; `AdminContext.CheckedDatabases` / `CheckedAccounts`, not used by any page yet). The «Diferență» page is no longer in the window (`DiffView*.vb` stay in the project, unused). `ButtonFit` widens button columns so a caption is never cut; `KBotSqlBox` (KBot.Controls) colours the SQL of the VERCON reports. Worklog: [ME01-05](../docs/worklog/SLICE-ME01-admin-desktop.md#slice-ME01-05).

### Pages

Comenzi · Sincronizare (`schema_sync`, preview by default) · Drepturi (ADECHIT rights per account) · Loguri (all server logs, coloured: api, vba, forexe timing, asociere, schema_sync, gunicorn error and access, help feedback, one-time runner) · Jurnal · Vizite · Server · Copii (backups and schema plans; a chosen database keeps only its own; double click shows the SQL text; «Copie nouă» dumps the chosen database, or every unit database except 000_DEMO when «all databases» is chosen, and asks for a reason that is kept in the Jurnal and shown in the «Motiv» column, ME02-02) · Cereri (new-account requests: approve / reject) · **Interogări unice** (one text run once on AVACONT_SURSA and the ticked databases, with the history and the text of every query that ran) ·
**Resetare Login** (failed-login counters through the admin API) · **Publicare**, right-aligned (the file push of
AvacontPush: scan, tick, send, restart the service; grids of accepted extensions and ignore rules edited in place and saved in `admin_settings.json`). AvacontPush itself is untouched.

**Copii locale** (ME02): encrypted copies of the unit databases on THIS PC, kept in the folder chosen with «Dosar...» (`BackupRoot` in
`admin_settings.json`). Format and cryptography: `docs/ME02_FORMAT.md`; the server half is `routes/admin_cli/admin_snapshot.py` (read only). The page offers «Copie acum» (asks for a reason), «Verifică», «Export SQL» (a file for a scratch database `zz_...`), «Istoric rând», «Curăță» (retention; manual copies are never removed by it) and «Șterge» (the only way a manual copy disappears). There is NO automatic nightly copy: at start-up the program asks whether to make today's copy when none exists (`MainForm.RemindLocalBackupAsync`). The switch `KBotAdmin.exe --backup` and `BackupScheduler` stay in the code but nothing in the window starts them. The passphrase is never stored; the key file `backup_key_<id>.dat` is protected by the Windows account (DPAPI) and is git-ignored.

Status and open threads: `docs/worklog/state/KBOT_STATUS_ME01.md`, `docs/worklog/state/KBOT_STATUS_ME02.md`.
