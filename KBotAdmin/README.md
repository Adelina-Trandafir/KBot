# KBotAdmin

The owner's private administration program (slice ME01). **Not part of K-BOT's delivery: it must never be published,
packaged or sent to a client.** It is not in `KBot.sln`, and `publish-release.ps1` / `push-update.ps1` refuse any package
that contains it.

```powershell
dotnet build KBotAdmin\KBotAdmin.vbproj
dotnet run --project KBotAdmin\KBotAdmin.vbproj
```

- Talks to the server only over SSH (key from `~/.ssh`, or a typed password / passphrase that is never saved). No
  database port is opened. Settings: `admin_settings.json` next to the exe (git-ignored; copy `admin_settings.example.json`).
- Server half: `PYTHON/routes/admin_cli/` (push it with AvacontPush). The program runs
  `python -m routes.admin_cli.admin_cli <verb>` and reads one JSON object back.
- Uses `KBot.Controls` and `KBot.Theming` by project reference (same theme as K-BOT).
- Errors: `<AppDir>\Logs\harness_errors.log`, messages shown to the operator: `Logs\mesaje_operator.log`.

Status and open threads: `docs/worklog/state/KBOT_STATUS_ME01.md`.
