# SLICE-0067-02 — Release notes written by an AI assistant at every release (operator request, 01.10.2026)

**Operator's request:** «vreau ca la fiecare publish-release sau push-update to have the copilot
app in the vsstudio ide or you - to right in romanian what has changed in the new version compared
to the latest».

## Assumptions stated

- **Slice number: 0067-02.** The request is a pass over the update system (0067: `push-update.ps1`,
  the `notes` of `latest.json`, the update box); 0067-01 was the installer. Not a new slice.
- **Neither assistant can be started from a script.** Copilot Chat in Visual Studio has no command
  line; the `claude` CLI is not installed on this PC (`Get-Command claude` → nothing). So the script
  does not call an assistant: it prepares the request, puts it on the clipboard and waits for the
  text. If the Claude CLI is installed later, the same request can be handed to it without a paste.
- **«The latest» = the previous version recorded in the notes file**, and, at push time, the version
  on the server: a client that jumps several versions is sent the lines of all of them.
- **Where the text goes:** the existing `notes` field of `latest.json`, shown by
  `AppUpdateService.OfferText`. No new window.

## What changed and why

### 1. `docs/release-notes/NOUTATI.md` — the notes
One `## <version> (dd.MM.yyyy)` section per release, newest first, Romanian with diacritics (it is
text the user reads). Each section carries two hidden lines:
- `<!-- release: utc=... -->` — when it was released; the next version counts its changes from it;
- `<!-- felii: 0097-02, 0000-25 -->` — the worklogs the lines came from.

Only the lines starting with `- ` reach the client. Seeded with a starting point: **1.1.0.6**,
`utc=2026-09-29T13:11:47Z` — the version and `published_utc` read from
`GET https://kbot.avatarsoft.ro/api/update/latest` on 01.10.2026 (marked `baseline`: it has no list).

### 2. `docs/release-notes/README.md` — the brief for the assistant
How the step runs, the exact shape of a section, and the writing rules: for an accountant, only
what the user sees or does differently, no slice / file / class names and nothing about how K-BOT
works inside (same line as `docs/HELP_SYSTEM.md` §5), on-screen text in «», plain text (the box
shows no Markdown), at most 12 lines, never invent, only what is new since the previous version,
and the single line for «nothing visible changed». The assistant edits only `NOUTATI.md`.

### 3. `tools/ReleaseNotes/ReleaseNotes.ps1` — new
Four actions (`-Action`, `-Version`):
- **Request** — finds the newest section below the version that has a release marker, lists every
  `docs/worklog/SLICE-*.md` written or changed after it (by file time; one already named in an
  earlier `felii:` is marked «already listed under X»), adds the commit subjects since then
  (read-only `git log`), builds the request with the ready-made heading and marker, saves it to
  `artifacts\release-notes-request.txt` and copies it to the clipboard (`-NoClipboard` = file
  only). A version that already has its section asks nothing.
- **Wait** — loops until the section exists with at least one line: Enter = look again, P = copy
  the request again, S = skip. Adds the release marker if the assistant left it out. Prints the
  lines and warns (never fails) on more than 12 lines, a line over 160 characters, or Markdown.
  Without an interactive console it does not wait. Output: `$true` / `$false`.
- **Text** — the lines as plain text for `latest.json` («• » per line, CRLF). With `-Since`, every
  section above that version and up to `-Version`.
- **Record** — stores text typed by hand as the version's section (when it has none).

`git`'s exit code is put back after the call: the callers read `$LASTEXITCODE` after their own tools.

### 4. `publish-release.ps1`
- New parameter `-ReleaseNotes Ask|Skip` (default `Ask`).
- Step **1c**, right after the version question: `Request` for the version being built (the bumped
  one, or the current one). The build goes on while the assistant writes.
- Step **11**, last: `Wait`. Both steps are never fatal (a warning, as the signing).

### 5. `push-update.ps1`
- Step **4b**, after the server's version is known and before `latest.json` is built:
  - `-Notes "<text>"` → sent as typed, recorded with `Record`; `publish-release.ps1` is called with
    `-ReleaseNotes Skip` (no assistant);
  - otherwise `Text -Since <server version>`; under `-SkipBuild` (nobody asked yet) `Request` +
    `Wait` first;
  - no notes → a warning, the push goes on with an empty text; an error stops the push (nothing has
    been uploaded at that point).
- The lines are printed before `latest.json`; to change them: Ctrl+C at the password prompt, edit
  `NOUTATI.md`, run again with `-SkipBuild`.
- The call to `publish-release.ps1` now splats a hashtable (same parameters as before).
- Rule 0 sweep in the header comment: «Cauta actualizari», «Mai tarziu» (comments are ASCII).

### 6. `CLAUDE.md`
The command line of the tool and a short «Release notes (slice 0067-02)» section pointing to the
brief.

## Files touched

- `tools/ReleaseNotes/ReleaseNotes.ps1` (new)
- `docs/release-notes/NOUTATI.md`, `docs/release-notes/README.md` (new)
- `publish-release.ps1`, `push-update.ps1`
- `CLAUDE.md`
- `docs/worklog/KBOT_STATUS.md`, `docs/worklog/state/KBOT_STATUS_0060-0069.md`

No `src/` file changed: nothing to build, no version to bump.

## Test results

- The three scripts parse with **0 errors** (`Parser::ParseFile`); `ReleaseNotes.ps1` and
  `push-update.ps1` are pure ASCII, `publish-release.ps1` kept its BOM.
- `ReleaseNotes.ps1` run once by hand on this PC (Windows PowerShell 5.1), clipboard untouched:
  - `Request -Version 1.1.0.7 -NoClipboard` → 36 worklogs since 1.1.0.6 (29.09.2026 16:11), from
    `SLICE-0093` to `SLICE-0000-25`, and 14 commit subjects; the request file reads correctly;
  - `Request -Version 1.1.0.6` → «already has its section -- nothing to write»;
  - `Text -Version 1.1.0.7 -Since 1.1.0.6` → empty (no section yet);
  - `Wait -Version 1.1.0.7` without a console → `False`, no waiting; `Wait -Version 1.1.0.6` → `True`.
- **No tests written, run or built.**

## Help

No change. `contabil.actualizare` already says the box shows «versiunea ta, versiunea nouă, ce
aduce și cât are de descărcat»; «ce aduce» is this text, and the box itself is unchanged.

## Left unverified or deferred

- **`publish-release.ps1` and `push-update.ps1` were NOT run** with the new steps: no build, no
  push, no interactive wait (Enter / P / S), no clipboard copy.
- `Record` and the «marker added by the script» path of `Wait` were not run (they rewrite
  `NOUTATI.md`).
- Not seen: how a 12-line text looks in the update box on a small screen.
- The first real notes (for the version after 1.1.0.6) have 36 worklogs behind them, most of them
  the help (0000-01…25); the brief asks for at most one line about the help.
- The file times decide «changed since»: on a fresh clone every worklog looks new (said in the
  README; the `felii:` lists are the fallback).
- The notes are not inside the package and the app has no «what's new» window after an update; the
  text is seen only in the update box, before the update. Not asked for.
