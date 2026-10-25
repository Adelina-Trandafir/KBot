# K-BOT release notes — how they are written (slice 0067-02)

Every release (`publish-release.ps1`, `push-update.ps1`) gets a short Romanian text saying what
changed since the version before it. The text lives in [NOUTATI.md](NOUTATI.md), one section per
version, and `push-update.ps1` sends it to the clients as the `notes` of `latest.json`: it is what
the user reads in the update box before pressing «Da».

The text is written by an AI assistant — GitHub Copilot Chat in Visual Studio, or Claude — from
the worklogs. **If you are that assistant, this file is your brief.**

---

## 1. How it runs

1. The operator starts `publish-release.ps1` or `push-update.ps1`. After the version question the
   script prints `[notes] ...`, saves the request to `artifacts\release-notes-request.txt` and
   puts it **on the clipboard**.
2. The operator pastes it into Copilot Chat (agent mode) or into Claude. The build runs meanwhile.
3. The assistant adds the section to `NOUTATI.md`.
4. At the end the script looks for the section. Missing → it waits: **Enter** = look again,
   **P** = copy the request again, **S** = release without notes.
5. `push-update.ps1` sends the text. A client that jumps several versions gets the lines of every
   version above the one on the server.

Other cases:

- **The version was not bumped** (its section already exists) → nothing is asked.
- **`push-update.ps1 -Notes "..."`** → the typed text is sent as it is and stored as the
  version's section; no assistant.
- **`publish-release.ps1 -ReleaseNotes Skip`** → the step is left out.
- **Rewrite a version's notes** → delete its section and run the build again, or ask the assistant
  directly (step below).
- **Without a build**, to get the request for a version:
  `.\tools\ReleaseNotes\ReleaseNotes.ps1 -Action Request -Version 1.1.0.7`

«Since when» is the newest section below the new version that carries a
`<!-- release: utc=... -->` marker. «What changed» is every `docs/worklog/SLICE-*.md` written or
changed after that moment. A fresh clone gives every worklog today's date, so there the list is
too long: go by the `felii:` lists of the earlier sections instead.

## 2. The section

Above the first `## ` heading of `NOUTATI.md` (newest first), in exactly this shape:

```markdown
## 1.1.0.7 (01.10.2026)
<!-- release: utc=2026-10-01T09:12:00Z -->
<!-- felii: 0097-02, 0000-25 -->

- Turul ferestrei principale porneste singur la primele deschideri.
- «MENIU» are un dosar nou, «Adaugare angajamente...».
```

- The heading and the `release` line come ready-made in the request: copy them.
- `felii:` = the ids of the worklogs you used (`SLICE-0097-02-...` → `0097-02`). The next request
  marks a worklog that an earlier version already listed.
- **One change = one line starting with `- `.** No sub-lists, no headings, no blank line inside
  a line. Only these lines reach the client.

## 3. Writing rules

**Who reads it:** an accountant who uses K-BOT, in a message box, deciding whether to update now.

- **MANDATORY: no diacritics, ever.** Write `NOUTATI.md` in Romanian but with plain letters only:
  `a` for ă/â, `i` for î, `s` for ș, `t` for ț (`Clasificatii`, `lățime` → `latime`, `așteaptă` →
  `asteapta`). The Setup wizard's news page and the update box show ș/ț as `?` and ă as `a` (seen
  on 1.1.1.7), so a diacritic only spoils the line. This covers the whole file, headings included.
  «» stay. On-screen names copied from the app are written the same way: «Clasificatii bugetare»,
  not «Clasificații bugetare».
- **Romanian, plain words, short sentences.** State what is different; do not
  address the reader. If you must, use the polite form, as the update box does («Aveti»,
  «Actualizati»).
- **Only what the user sees or does differently**: a new window, button, menu row, message, a
  flow that goes another way, a fault that no longer happens.
- **Left out** (same line as the help, `docs/HELP_SYSTEM.md` §5): slice numbers, file, class,
  table and route names, how K-BOT does something inside, server-only work, refactors, tooling,
  documents, the dev harness, the capture tool, anything Debug-only. A change nobody would notice
  is not a line.
- **On-screen text as it is on screen** (minus diacritics, see above), in «». Take it from the
  worklog or the code, never from memory.
- **Plain text**: no `**bold**`, no backticks, no links — the box shows them as typed.
- **At most 12 lines, each under about 110 characters.** Most important first. Several small
  fixes in one area → one line. The help brought up to date → at most one line, last.
- **Never invent.** Every line must come from a worklog on the list. A worklog saying «built
  clean, not run» still ships: describe what the change does, do not promise it was verified.
- Describe **only what is new since the previous version**. A worklog marked «already listed
  under X» was changed after that version: read what was added to it, skip the rest.
- Nothing the user can see changed → the single line
  `- Imbunatatiri interne si corecturi mici.`

## 4. What the assistant must not do

Edit only `docs/release-notes/NOUTATI.md`. No build, no run, no tests, no git writes, no change
to the version number, to the scripts or to an earlier version's section.

## 5. Files

| What | Where |
|------|-------|
| The notes | `docs/release-notes/NOUTATI.md` |
| The request builder / reader | `tools/ReleaseNotes/ReleaseNotes.ps1` (`Request`, `Wait`, `Text`, `Record`) |
| The last request | `artifacts/release-notes-request.txt` (not in git) |
| Where the text is shown | `AppUpdateService.OfferText` (`check.Info.Notes`) |

<a id="slice-0067-02"></a>

## 6. Implementation history — 0067-02, 01.10.2026

Absorbed from the worklog in D001-29. The original request was for Romanian notes at each publish-release/push-update, written by the Visual Studio Copilot or the assistant. Number 0067-02 was a pass over the update system, after the installer 01. At this checkpoint Copilot Chat had no CLI and Get-Command claude found none on this PC: the request was pasted, not dispatched by the script; installing a CLI later could reuse the same request. These are historical observations, not a capability check today. The original “with diacritics” instruction was superseded by §3 after the observed 1.1.1.7 rendering issue.

Baseline: 1.1.0.6, utc=2026-09-29T13:11:47Z, read from GET https://kbot.avatarsoft.ro/api/update/latest on 01.10.2026; marked baseline, with no worklog list.

### Script details retained from the implementation record

| Action / caller | Contract at this checkpoint |
|---|---|
| Request (-Action / -Version) | Includes file-time worklogs and read-only git-log commit subjects; a previously used worklog is marked already listed under X. Ready-made heading/marker go into artifacts/release-notes-request.txt and clipboard; -NoClipboard writes the file only. Existing version section asks nothing. |
| Wait | Requires a section with at least one line, adds a missing release marker; Enter/P/S as §1. Prints lines and warns without failure on >12 lines, >160 characters per line or Markdown. Without interactive console it does not wait; returns true/false. |
| Text | Plain lines prefixed with • and joined with CRLF; -Since includes sections above that version through -Version. |
| Record | Stores manually supplied text only when the version has no section. |
| Git exit state | Restores git’s exit code after the call; callers read LASTEXITCODE after their own tools. |
| publish-release.ps1 | -ReleaseNotes Ask or Skip, default Ask; step 1c immediately after choosing the bumped/current version calls Request, step 11 calls Wait. Both are nonfatal, like signing warnings. |
| push-update.ps1 | Step 4b, after learning the server version and before latest.json: -Notes sends/stores the typed text and calls publish with Skip; otherwise Text -Since server version, with Request/Wait first under -SkipBuild. Missing notes warns and continues empty; an error stops before upload. Lines print before latest.json; Ctrl+C at password prompt, edit NOUTATI, rerun with -SkipBuild to change them. Publish parameters are splatted as a hashtable; header comments Cauta actualizari / Mai tarziu were swept to ASCII. |

#### 6. `CLAUDE.md`
The command line of the tool and a short «Release notes (slice 0067-02)» section pointing to the
brief.

### Files touched

- `tools/ReleaseNotes/ReleaseNotes.ps1` (new)
- `docs/release-notes/NOUTATI.md`, `docs/release-notes/README.md` (new)
- `publish-release.ps1`, `push-update.ps1`
- `CLAUDE.md`
- `docs/worklog/KBOT_STATUS.md`, `docs/worklog/state/KBOT_STATUS_0060-0069.md`

No `src/` file changed: nothing to build, no version to bump.

### Test results

- The three scripts parse with **0 errors** (`Parser::ParseFile`); `ReleaseNotes.ps1` and
  `push-update.ps1` are pure ASCII, `publish-release.ps1` kept its BOM.
- `ReleaseNotes.ps1` run once by hand on this PC (Windows PowerShell 5.1), clipboard untouched:
  - `Request -Version 1.1.0.7 -NoClipboard` → 36 worklogs since 1.1.0.6 (29.09.2026 16:11), from
    `SLICE-0093` to `SLICE-0000-25`, and 14 commit subjects; the request file reads correctly;
  - `Request -Version 1.1.0.6` → «already has its section -- nothing to write»;
  - `Text -Version 1.1.0.7 -Since 1.1.0.6` → empty (no section yet);
  - `Wait -Version 1.1.0.7` without a console → `False`, no waiting; `Wait -Version 1.1.0.6` → `True`.
- **No tests written, run or built.**

### Help

No change. `contabil.actualizare` already says the box shows «versiunea ta, versiunea nouă, ce aduce și cât are de descărcat»; «ce aduce» is this text, and the box itself is unchanged.

### Left unverified or deferred

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
