# SLICE-ME01-35 - AVC.Migrator: signing option in the publish script (10.11.2026)

Slice given by Adelina (10.11.2026). Request: «avc.migrator trebuie sa aiba si el optiune de semnare». The script parses clean (PowerShell parser); NOT run: no publish, no SimplySign confirmation.

## What changed and why

`publish-avc-migrator.ps1` published an unsigned `AVC.Migrator.exe`. It now signs the finished exe the way `publish-release.ps1` signs K-BOT:
- `-Sign Ask|Yes|No`. Default `Ask`: one console question («Sign AVC.Migrator.exe (SimplySign confirmation)? (Y)es / (N)o [Y]», Enter = Yes; with no interactive console it signs). `No` signs nothing and the token is never asked.
- `-SignThumbprint` (same default certificate as publish-release.ps1; empty falls back to env `KBOT_SIGN_THUMBPRINT`, then no signing) and `-TimestampUrl` (`http://time.certum.pl`).
- After `dotnet publish` and the cleanup, `signtool sign /fd SHA256 /tr <url> /td SHA256 /sha1 <thumbprint>` on the exe. Never fatal: no thumbprint, no `signtool.exe`, certificate not in `CurrentUser\My` (SimplySign not running) or a signtool error give a warning and the unsigned exe stays.
- The last line of the script says `signed` / `not signed` (or the status Windows reports when the signature is not Valid).

The helper functions are a trimmed copy of publish-release.ps1's (that file is a script, not a module, so it cannot be dot-sourced without running it).

## KBotAdmin > Scripturi: signing tick boxes (ME01-35, same day)
The «publish-avc-migrator.ps1» entry of `ScriptCatalog.vb` now has the two signing ticks the other publish scripts have: «sign» (`-Sign Yes`) and «nosign» (`-Sign No`), mutually exclusive through the existing `Exclusive` group. Neither ticked = the script asks on the console. The entry's description also says the result is ONE small exe to copy and install by hand. Compiled: no; not seen on screen.

## Files touched
- `publish-avc-migrator.ps1`.
- `KBotAdmin/Scripts/ScriptCatalog.vb`.

## Unverified / deferred
- Never run: whether the signature is Valid on the produced single-file exe (a bundled single-file exe is signed as any PE, but this was not tried). The first real run prints the status.
- AVC.Migrator FileVersion not bumped (no project change).
