# SLICE-0103-02 — AvacontPush: login with the SSH key (no password) and every existing database ticked on detection (operator request, 02.10.2026)

Branch: `SLICE-0100-Multithreading` (nothing committed). Dev tooling; no help topic.

## What changed and why

1. **No password.** The operator already reaches the server from VS Code without any prompt because the key is set up in
   `~/.ssh`. AvacontPush asked for the SSH password on every run. New `SshAuth.vb` builds the `ConnectionInfo` for both
   `SshClient` and `SftpClient`: the private key when there is one, the typed password only when the key is missing or the
   operator typed one. Key lookup: `PrivateKeyPath` in `push_settings.json` → `IdentityFile` of the `~/.ssh/config` block that
   names the host (or `Host *`) → `~/.ssh/id_ed25519`, `id_ecdsa`, `id_rsa`. The Password box is optional; if typed it is also
   tried as the key's passphrase. The status bar shows which login will be used. Checked on this PC: `~/.ssh/id_ed25519` exists,
   OpenSSH format, **not encrypted** (cipher `none`), and `~/.ssh/config` has `Host 89.33.25.34 / User root / Port 27015`.
   The operator wrote «migrator»; read as AvacontPush (the SSH tool). `KBot.Migrator` has no SSH any more: its two password
   boxes are the Access file password and the MariaDB server password, which an SSH key cannot replace.
2. **Databases ticked on detection**, in both places. After «Citește bazele», the schema-sync tab ticks every database that
   really exists (the ones without the «nu există pe server» note; that one stays unticked and cannot be ticked).
   The «Interogări unice» tab got the same list and the same button; its «Vezi» / «Execută» send the ticked units as
   `--targets`, and the runner (`routes/one_time/runner.py`) takes them (`parse_targets` + `verify_targets`: a name the server
   does not have stops the run) with `AVACONT_SURSA` always first. Without `--targets` the runner still takes every CAI
   database that exists.

## Files touched

New: `PYTHON/AvacontPush/SshAuth.vb`. Changed: `AppConfig.vb`, `SshCommandService.vb`, `SftpService.vb`, `OneTimeService.vb`,
`Form1.vb`, `Form1.Designer.vb`, `README.md`, `push_settings.example.json`, `PYTHON/routes/one_time/runner.py`.

## Test results

No tests written or run (operator: no tests). `dotnet build PYTHON/AvacontPush/AvacontPush.vbproj` (forced rebuild) → 0 warnings,
0 errors; `py_compile` on `runner.py`. **No connection to the server was made**: the key login, the key parsing by SSH.NET
(OpenSSH ed25519) and the new list were not exercised.

## Left unverified / deferred

- First connection with the key not seen. If the server refuses the key (not in `authorized_keys` for this user/port) the
  error says «Autentificare eșuată … cheia SSH din ~/.ssh sau parola».
- A key only held by ssh-agent cannot be used (SSH.NET has no agent support).
- The new list's layout (40 / 60 split with the SQL box) and the longer button row were not seen on screen.
- Operator wording «migrator» read as AvacontPush; if `KBot.Migrator` was meant, its MariaDB password would need a different
  answer (e.g. an SSH tunnel), not done.
