# K-BOT — STATUS, slice ME01 (KBotAdmin)

Operator request, 10.10.2026: an administration program for the OWNER only. Desktop VB.NET, **not** online, and
**never part of any publish / setup / update** (it reaches no client, ever). Detail: [SLICE-ME01-admin-desktop.md](../SLICE-ME01-admin-desktop.md).

## Registry row

| Slice | What | Status |
|---|---|---|
| ME01 | KBotAdmin: databases tree (minus AVACONT_SURSA / AVACONT_COMUN), accounts tree, pages (commands, schema difference, sync, rights per account, coloured logs, activity journal, visits, server, backups, new-account requests); server half `PYTHON/routes/admin_cli/`; ADECHIT rights per account; web page `/operator` removed | DONE on code, clean build. **Never run against the server, never seen on screen.** |

## Current focus

- Whole slice written in one pass on 10.10.2026. Build of `KBotAdmin\KBotAdmin.vbproj` and `KBot.Controls` is clean;
  the Python files compile (`py_compile`); nothing else was run (no tests, no SSH session, no window opened).
- First real use: push `PYTHON/` with AvacontPush (new `routes/admin_cli/`, changed `routes/adechit/`, `main.py`,
  `routes/auth/mailer.py`, removed `routes/inregistrare/operator.py` + `static/operator.html` + `static/js/operator/`),
  restart gunicorn, then start `KBotAdmin` and connect.

## Open threads

- **Role to create on the server:** `AD_EDIT_INCHISA` (operation `edit_closed`). KBotAdmin shows a button on the rights
  page when it is missing; or run `sql/ME01_01_rol_editare_luna_inchisa.sql`.
- **ADECHIT checks not exercised:** `save_attendance` in a closed month (last closed only, snapshot rewritten with the new
  days / contract value / balance), the per-account cancel right (the `AllowCancelDocuments` setting is no longer read),
  the web grid (`app.js` `canEditDays`, `canCancelDocument`). `PYTHON/tests` not touched and not run (may still carry the
  old `CANCEL_OFF` expectation). `ADECHIT/tools/preview.py` still seeds `AllowCancelDocuments` (harmless, now unused);
  the preview user needs the `cancel` operation to see the cancel button.
- **Approving a request** (`request-approve`) runs `provizionare.approve` from a command line under root, with the same
  server-wide lock as before; it needs a quiet moment (do not restart gunicorn while it runs). Never run on a real request yet.
- **Not built (deliberately):** restoring a backup into a scratch database, forced closing of sessions, password reset /
  account locking at MariaDB level. The portal admin page was removed on 11.10.2026 (see the worklog follow-up).
- Host-key pinning: first connect asks to remember the fingerprint (`admin_settings.json`, git-ignored).
- Help: none (KBotAdmin is not for clients). `KBot.Controls` gained `KBotLogBox` (FileVersion 1.65.0.0); no K-BOT window changed.
