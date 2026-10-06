# SLICE-0078-12 - Hosted window with an OLDER Adobe: script-error monitor + its own bench (operator request, 06.10.2026)

Operator: what the hosted-window engine does today is correct for Reader 2024 and newer. On the local Acrobat Pro 2020
(Acrobat DC 19.12, cracked install) it works less well, and the JS errors that used to show only on the ActiveX engine
show there too. So the JS-error monitoring must exist on the hosted-window engine as well, and a NEW harness form is
needed to test only this old-Adobe side.

## What the real log said (read before changing anything)

`src/KBot.App/bin/Debug/net8.0-windows/Logs/adobe_preview.log`, 05.10.2026 20:05-20:58, `Acrobat.exe` 19.12.20035.332343,
engine «Fereastră găzduită», DDF 185:

- The script-window handling of `AdobeSaveTrap` was ALREADY running on the hosted window (the trap is shared by both
  engines): 12 x «GeneralErrorOperation failed.» each closed by `PressOk` method 0 (WM_COMMAND) at 20:48:06-08; the
  «JavaScript Debugger» console («The JavaScript Debugger is not enabled…») hidden at 20:43, 20:53, 20:56; the form's own
  messages («Nu sunt erori la sectiunea A», «Validarea s-a terminat cu succes!…») left to the operator; `Ctrl+H, Ctrl+2:
  aștept — mesaje de script în curs` until the burst ended.
- What was missing is MONITORING, not detection: those facts were only text lines among others in `adobe_preview.log`.
  No count per document, nothing structured for a bench to show, no trace (the trap's trace was ActiveX-only), no line that
  says what the document gave.

## What changed and why

- **`AdobeSaveTrap.ScriptAlertSeen`** (new event, `Action(Of AdobeScriptAlert)`): raised once per window and outcome from
  `DismissScriptNoise`: error pressed (first press), message left to the operator, error stuck after every press, error
  hidden, console hidden, console left alone. Additive: no decision of the trap changed; a listener that throws is logged
  and ignored (`NotifyScript`).
- **`AdobeScriptAlert`** (new, POCO + `AdobeScriptAlertAction`): time, handle, title, text, action, matching pattern, presses;
  `IsError` / `IsConsole` / `KindLabel` / `ActionLabel` / `Describe()`.
- **`AdobeScriptMonitor`** (new): the tally for ONE document (`ErrorCount`, `StuckCount`, `MessageCount`, `ConsoleCount`,
  `Alerts`, `Summary()`, `Recorded` event, `Reset()`). A stuck alert always follows its first press, so it is part of
  `ErrorCount`, not an addition (said in the summary: «…; 1 nu s-a închis»).
- **`AdobeReaderHost`**: owns a monitor (`ScriptMonitor`), forwards `ScriptAlertSeen`, resets it at every document, and writes
  ONE line, `Mesaje de script Adobe la deschidere: …` (always, also «niciun mesaj de script») when the document is ready and
  `… până la eliberarea documentului: …` at release only when more arrived since. Also writes `Adobe: <exe> <version> —
  ANTERIOR lui 2024 / linia 2024 sau mai nouă — <path>` at every launch (`AdobeProductInfo`) and exposes it as `AdobeProduct`.
- **Trace for the hosted window**: `AdobeReaderHost` sets `_saveTrap.Traced = True`, opens an `AcroPdfTraceLog` recording per
  document (source `AdobeReaderHost`, trap lines as `SaveTrap#n`), mirrors every working-log line as `REPORT:`, dumps the
  hosted window tree right after hosting and when the document is ready, and closes the recording at ready / failure /
  release. **All of it is a no-op unless `AcroPdfTraceLog.SwitchedOn`** (in memory, off at every start), which in the
  application is reachable only through the Setări checkbox that is hidden while the hosted window is the engine - so the
  application's behaviour on Reader 2024+ is unchanged apart from the two extra working-log lines above.
- **`AdobeProductInfo`** (new): file version of the resolved Adobe exe; `IsBefore2024` = file-version major < 24 (the operator's
  line; a number read, not behaviour probed).
- **New bench `OldAdobeHostHarnessForm`** (+ Designer, + `OldAdobeHostHarnessTest`, DevHarness ▸ «Adobe/PDF» ▸ «Fereastră
  găzduită — Adobe vechi (< 2024): monitor erori JS»; Debug only): drives `AdobeReaderHost` directly - no signing session,
  no server, no print watch. Same preface as the application before a document (classic-interface option, standard Save As
  preference, operator's «instanță nouă» and detach settings). Always on a copy (`Temp\PDF\Banc\ADOBE_VECHI_<name>`).
  Shows: the Adobe found and which side of 2024; hosted-after / ready-after milliseconds; one row per script window
  (colour: stuck = error, closed error = warning, console = dim) and the running tally; the live working log; switches for
  Ctrl+H/Ctrl+2, the trap (Save As + script alerts are one object) and the detailed trace; the REAL «Mesaje de script
  Adobe…» list dialog; «Arborele ferestrelor Adobe» (hosted tree + every top-level window of the Adobe processes, to read a
  dialog that stays on screen); «Copiază raportul» (Adobe, switches, timings, tally, each window, last 250 log lines).
- Settings window NOT touched (see deferred).
- FileVersion: `KBot.Controls` 1.62 ▸ **1.63**, `KBot.App` 1.1.1.8 ▸ **1.1.1.9**.
- Help / NOUTATI: nothing to update - nothing the operator sees changes (the bench is Debug-only, the rest is log lines).

## Files touched

`src/KBot.Controls/Adobe/`: `AdobeScriptAlert.vb` (new), `AdobeScriptMonitor.vb` (new), `AdobeProductInfo.vb` (new),
`AdobeSaveTrap.vb`, `AdobeReaderHost.vb`, `AcroPdfTraceLog.vb` (doc comment), `AdobeReaderHost.md`;
`src/KBot.App/HarnessTests/`: `OldAdobeHostHarnessForm.vb`, `OldAdobeHostHarnessForm.Designer.vb`, `OldAdobeHostHarnessTest.vb`
(all new); `tests/KBot.Controls.Tests/`: `AdobeScriptMonitorTests.vb`, `AdobeProductInfoTests.vb` (new);
`src/KBot.Controls/KBot.Controls.vbproj`, `src/KBot.App/KBot.App.vbproj` (FileVersion).

## Test results

- `dotnet build src\KBot.Controls`, `dotnet build src\KBot.App`, `dotnet build tests\KBot.Controls.Tests`: 0 errors, 0 warnings.
- Tests written (the tally, the alert kinds, the 2024 line / version parsing) and compiled, **not run** (operator rule: no tests).
- **The bench was not run**: nothing seen on screen, no PDF opened in it, the Acrobat 19.12 on this machine not driven.

## Findings from the same log, NOT fixed (outside this request)

1. **The «Replace existing file?» box is not recognised on Acrobat 19.12 (hosted window).** At 20:57:44 the trap pressed Save in
   «Save As PDF» and the box «Save As» (`butonDa=True butonNu=True`, text «…The file already exists. Replace existing file?»)
   was classified `Other` / «lăsat în pace», never answered; the trap then timed out (`ANULAT … nu s-a închis după apăsarea…`)
   and the «Save As PDF» dialog reappeared 44+ times, each cancelled («a reapărut după 3 apăsări»). `Classify` accepts a
   confirm only when `facts.OwnerWindow = awaitingConfirmFor` (the Save As we pressed): **hypothesis** - on this Acrobat the box is
   owned by another window. The `Other` log line does not print the owner; with the trace on, the bench's
   `Handle … kind=Other … owner=0x…` line does. Earlier runs (20:43, 20:44) closed the same way «fără confirmare de
   suprascriere» and the document still saved (cause unknown: operator pressed Yes?).
2. Dialogs only the older Acrobat raises, all left alone by the trap: «Appearance Integrity Report», «Token Logon», «The
   document could not be signed.» (20:43:55 after a name that could not be pinned: `ANULAT: Numele din dialog … nu a putut fi fixat`).
3. First window found after 8.7 s on a cold Acrobat 19.12 (20:43:07) - the 30 s search budget absorbs it.

## Left unverified / deferred

- Nothing run. Whether the monitor rows and the tally match what Acrobat 19.12 really shows is unproven until the bench is
  used with a DDF; the text of `Mesaje de script Adobe la deschidere:` is the first thing to compare with the 20:48 run (expect
  «12 erori de script…»).
- **Settings**: the «Mesaje de script Adobe…» button and the trace checkbox in Setări ▸ Documente are still visible only
  while an ActiveX engine is chosen, although the same list drives the hosted window's trap. Making them visible for the
  hosted window changes a screen and needs the help updated (0000-NN); asked for nothing here - the bench has both.
- Two traps that watch the SAME Adobe process (Acrobat DC ignores «/n»: the DDF and the ORD page both hosting) share the script
  windows: the trap that meets a window first reports it, so an alert can be attributed to the other page's host.
- `AdobeProductInfo.IsBefore2024` is a version number, not a behavioural test (Acrobat 2020 reports 20.x, the local 19.12 is
  a DC 2019 build).
- Not committed (operator did not ask); the worktree also holds unrelated uncommitted work.
