# AVACONT Push

Small manual-run WinForms tool (VB.NET, `net8.0-windows`) that pushes edited files
from the local **PYTHON** folder to the remote server over SFTP, then optionally
restarts the `avacont` service and shows the journal.

## Build

```
dotnet build -c Release
```

## Deploy

Copy the published output into `PYTHON\_push\` (the app must live in a subfolder of
the PYTHON folder so it never lists its own files). Then:

1. Copy `push_settings.example.json` to `push_settings.json` next to the EXE.
2. If needed, set `LocalRoot`.
   - If `LocalRoot` is left empty, it defaults to the parent of the app folder
     (i.e. the PYTHON folder), which is correct when the EXE sits in `PYTHON\_push\`.

The password is **never stored**: it is typed in the app every run and kept only in
memory (the `Password` property is `[JsonIgnore]`, so it is never written to nor read
from `push_settings.json`). `push_settings.json` is also git-ignored.

## Use

1. **Scanează** — connects over SFTP and lists the local files as a **folder tree**
   with checkboxes. Only source/config files are considered — the `IncludeExtensions`
   allow-list (`py`, `json`, `xml`, `yaml`, `yml`, `ini`, `cfg`, `toml`, `txt`, `sql`,
   `html`, `css`, `js`, `md`) — and build/IDE output is skipped (`.git`, `.venv`,
   `venv`, `__pycache__`, `*.pyc`, `.vscode`, `.vs`, `bin`, `obj`). Each file is
   compared against the server; new/modified files are auto-checked. On the first
   successful connect the server host-key fingerprint is pinned into
   `push_settings.json`.
2. Tick/untick anything you like — **ticking a folder ticks/unticks all files under
   it**. Identical files are unchecked by default. Hover a file to see its local and
   server UTC timestamps.
3. Optionally tick **Repornește serviciul după push**.
4. **Trimite** — uploads the checked files (creating remote folders as needed). If
   restart is ticked, it then runs `systemctl daemon-reload`, `systemctl restart
   avacont`, and `journalctl -u avacont -n 30 --no-pager`, showing the output.

Every run is logged to `Logs\push_{timestamp}.log`.

## Sincronizare schemă (second tab)

The same SSH connection also runs `routes.schema_sync.schema_sync` on the server,
so the schema of a unit database can be brought to `AVACONT_SURSA` without a
terminal session. The list of databases has to come from the server: schema_sync
reads its credentials from `config.py`, which is host-only and never travels with
a push.

1. **Citește bazele** — runs `--list-targets` and fills the list with every
   database whose name starts with three digits (`000_DEMO`, `001_…`). One marked
   `(lipsă din CAI)` exists on the server but is not in the `AVACONT_COMUN.CAI`
   registry — shown rather than hidden, because that is worth knowing.
2. Tick the databases, pick **SAFE** or **FORCE**.
3. **Vezi (nu execută)** — `--view`: generates the statements, writes them to the
   server's `.sql` file and prints the summary. Executes nothing.
4. **Execută** — runs for real, after a confirmation dialog.

**Python** is the interpreter used on the server (`RemotePython`). It defaults to
`/root/AVACONT/.venv/bin/python`, because the service runs from that virtual
environment and the system `python3` cannot import `mysql.connector` — with it
every run dies on `ModuleNotFoundError: No module named 'mysql'`. If the venv
ever moves, type the new path in the box; it is saved on the next action. A
config still holding the bare `python3` is upgraded to the venv path on load,
since that value never worked here.

### How the destructive gate is answered

Commands run over this channel have **no terminal and no stdin**, so every prompt
the tool would show reads end-of-file. Three consequences, all handled:

- Runs always carry `--run`. Without it, «Executați acum?» would read EOF, take
  the last answer (`nu`) and exit 0 — a cancelled run that looks like a clean one.
- **Execută** goes out first *without* `--allow-destructive`. If the sync involves
  destructive DDL the tool refuses the whole thing (exit code 2) and **nothing is
  executed**; the app then shows the refusal and asks the operator.
- Only if the operator agrees does a second run go out with
  `--allow-destructive`, with the typed `DA` piped into it. The gate is not
  removed — it is answered in the dialog instead of at a terminal that does not
  exist here.

`PYTHONIOENCODING=utf-8` is set on every run: stdout here is a pipe, not a
terminal, so on a server under the `C` locale the first Romanian message with
diacritics would otherwise raise `UnicodeEncodeError`.

Output arrives only when the command **ends** — the channel hands over stdout and
stderr in one piece — so a long sync shows nothing until it is done.

## `.pushignore`

`IgnorePatterns` in `push_settings.json` stays what it always was: a short fixed
list of build/IDE noise. For everything else there is **`.pushignore`**, a file in
the root of the pushed tree (`PYTHON\.pushignore`) that travels with the files it
talks about and needs no rebuild or config edit.

The rules are gitignore's:

| Rule | Meaning |
|---|---|
| `# text` | comment |
| `nume` | any folder or file with that name, at any depth, and everything under it |
| `/nume` | only at the root of the pushed tree |
| `nume/` | only when it is a folder |
| `*.log` | by extension, in any folder |
| `docs/**/*.tmp` | `**` spans any number of folders |
| `!nume` | puts back something an earlier rule took out |

**The last matching rule wins**, so a negation must come *after* the rule it
undoes. `.pushignore` also has the last word over `IgnorePatterns`: a `!` line can
put back a file the fixed list would have skipped.

Note that the extension allow-list still applies on top. `.pushignore` can only
*subtract* from what `IncludeExtensions` already permits — a file whose extension
is not in that list is never pushed, ignore file or not. That also means
`.pushignore` itself is never pushed: it has no extension.

The scan writes the number of rules read into the run log.

## Notes

- Timestamps are compared in **UTC** with a 2-second tolerance.
- `PreserveMTime` sets the remote mtime to match the local file after upload, so a
  re-scan reports the file as `IDENTIC` (no phantom re-pushes).
- Host-key pinning: if the server fingerprint ever changes, the app refuses to
  connect (possible MITM). To re-pin intentionally, clear `HostKeyFingerprint` in
  `push_settings.json`.
- A schema sync run must be able to write on the server: the `.sql` file it
  produces, and the `mysqldump` backups it takes before destructive work.
- Verify the `SSH.NET` package version in `AvacontPush.vbproj` is the latest stable
  before building.
