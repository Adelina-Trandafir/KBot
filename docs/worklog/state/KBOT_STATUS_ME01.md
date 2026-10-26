# K-BOT — STATUS, slice ME01 (KBotAdmin)

Operator request, 10.10.2026: an administration program for the OWNER only. Desktop VB.NET, **not** online, and
**never part of any publish / setup / update** (it reaches no client, ever). Detail: [SLICE-ME01-admin-desktop.md](../SLICE-ME01-admin-desktop.md).

## Registry row

| Slice | What | Status | Worklog |
|---|---|---|---|
| ME01 | KBotAdmin: databases tree (minus AVACONT_SURSA / AVACONT_COMUN), accounts tree, pages (commands, schema difference, sync, rights per account, coloured logs, activity journal, visits, server, backups with SQL viewer, new-account requests, one-time queries with history, login reset, Publicare = the file push of AvacontPush); server half `PYTHON/routes/admin_cli/`; ADECHIT rights per account; web pages `/operator` and `/portal` Administrare removed; Classic theme fixed; tray; icon | DONE and in use by the operator (11.10.2026): publish, logs, backups, one-time queries, file push exercised; approval of requests, ADECHIT rights and the schema sync destructive path still unverified here. | [ME01](../SLICE-ME01-admin-desktop.md#slice-ME01) |
| ME01-02 | Machete verticale pentru drepturi ADECHIT/VERCON | 12.10.2026: compilat curat; nerulat | [ME01-02](../SLICE-ME01-admin-desktop.md#slice-ME01-02) |
| ME01-03 | Politica și sincronizarea privilegiilor MariaDB | 19.10.2026: implementat în surse; nerulat, fără build/teste/SQL | [ME01-03](../SLICE-ME01-03-privilegii-mariadb.md) |
| ME01-04 | Clonare DEMO cu identitatea destinației păstrată | 20.10.2026: compilat curat; nerulat pe server | [ME01-04](../SLICE-ME01-04-clonare-demo.md) |
| ME01-05 | Ribbon, ButtonFit, KBotSqlBox și ajustări vizuale | 20.10.2026: compilat curat; nerulat | [ME01-05](../SLICE-ME01-admin-desktop.md#slice-ME01-05) |
| ME01-06 | Arbore cu bife și două moduri, KBotCheckBox, diagnostic tray | Compilat curat la 20.10; completări până la 21.10 nerulate pe ecran; bife încă nefolosite de pagini | [ME01-06](../SLICE-ME01-admin-desktop.md#slice-ME01-06) |

## Current focus

- Written 10-11.10.2026. Builds clean (`KBotAdmin\KBotAdmin.vbproj`, `KBot.Controls`, `KBot.Theming` 1.14.0.0). The operator runs it from `_Admin\` (`publish-admin.ps1`).
- Deploy after any change to `PYTHON/routes/admin_cli/`: push `PYTHON/` with AvacontPush (or the Publicare page) and restart the service.
- Fixed on 11.10.2026 after use: a dropped SSH session ("Nu există conexiune la server") is now reconnected (+ keep-alive); untimed gunicorn log lines no longer share the file time; confirmations no longer hide behind the main window.

## Open threads

- **Role to create on the server:** `AD_EDIT_INCHISA` (operation `edit_closed`). KBotAdmin shows a button on the rights page when it is missing; or run `sql/ME01_01_rol_editare_luna_inchisa.sql`.
- **ADECHIT checks not exercised:** `save_attendance` in a closed month (last closed only, snapshot rewritten with the new days / contract value / balance), the per-account cancel right (the `AllowCancelDocuments` setting is no longer read), the web grid (`app.js` `canEditDays`, `canCancelDocument`). `PYTHON/tests` not touched and not run (may still carry the old `CANCEL_OFF` expectation). `ADECHIT/tools/preview.py` still seeds `AllowCancelDocuments` (harmless, now unused); the preview user needs the `cancel` operation to see the cancel button.
- **Approving a request** (`request-approve`) runs `provizionare.approve` from a command line under root, with the same server-wide lock as before; it needs a quiet moment (do not restart gunicorn while it runs). Never run on a real request yet.
- **Not built (deliberately):** restoring a backup into a scratch database, forced closing of sessions, password reset / account locking at MariaDB level. The portal admin page and `/operator` were removed.
- Host-key pinning: first connect asks to remember the fingerprint (`admin_settings.json`, git-ignored).
- Backups: `backups --db` filters by name (dumps) / by text (schema plans); `backup-text` returns the first 2 MB of one file. The one-time history (`once-history`) reads the `Interogari_Unice` ledger of AVACONT_SURSA and of every unit.
- Logs page: `access.log` is read only on request (last 6 MB) and is not part of «all files» (4xx would swamp the counts).
- Help: none (KBotAdmin is not for clients). `KBot.Controls` gained `KBotLogBox` (FileVersion 1.65.0.0); no K-BOT window changed.

