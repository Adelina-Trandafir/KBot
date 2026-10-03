# SLICE-0078-11 - Adobe processes K-BOT started are ended when K-BOT closes (operator request, 03.10.2026)

Operator: some Adobe processes stay in the Task Manager as ghosts after the application is closed. Proposed fix: keep the
PID of every Adobe process K-BOT generates and, on closing, send WM_CLOSE (unsure it works on ghosts) or kill by id.

## What changed and why

- New `src/KBot.Controls/Adobe/AdobeProcessRegistry.vb`:
  - `Track(pid)` remembers a process with its START TIME (a pid reused later by something else is not mistaken for it).
  - `TrackFamily(parents)` / `Sweep()`: an Adobe-named process (`Acrobat`, `AcroRd32`) is "ours" only if its parent is K-BOT
    or a process already ours, repeated until nothing new (broker -> renderer chain). An Acrobat the operator opened by hand
    has another parent and is NEVER touched.
  - `Shutdown()`: final sweep, then WM_CLOSE (posted, not sent) to every top-level window of our processes, wait up to
    1.5 s for them to leave, then `Process.Kill(entireProcessTree:=True)` for the survivors - the kill is what handles a
    ghost, which has no window to answer WM_CLOSE. Never throws (terminal, runs while the application goes away).
  - Everything is written to `adobe_preview.log` (`AdobeHostLog`): the processes found, WM_CLOSE count, who left, who was
    killed, and the Adobe processes LEFT ALONE with their parent pid - the evidence for the next ghost if it has another parent.
- Where processes get remembered: `ProcessAdobeLauncher.Start` (`IAdobeLauncher.vb`, every Adobe K-BOT starts itself) and
  `AcroPdfSurface.OwnerPids` (the ActiveX viewer's broker/renderer family, on every save-trap sweep).
- Where it runs: `Program.RunShellWithLogin` - `Application.Run(shell)` is now in a `Try/Finally` whose Finally calls
  `AdobeProcessRegistry.Shutdown()`, after the shell is closed and disposed (so the ActiveX control is gone first), before
  the logout.
- FileVersion: `KBot.Controls` 1.60 -> 1.61. (`KBot.App` already bumped in the same uncommitted work.)

## Files touched

`src/KBot.Controls/Adobe/AdobeProcessRegistry.vb` (new), `IAdobeLauncher.vb`, `AcroPdfSurface.vb`,
`src/KBot.Controls/KBot.Controls.vbproj`, `src/KBot.App/Program.vb`.

## Test results

`dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors. Nothing run, no ghost provoked (operator rule: no tests).

## Left unverified / deferred

- **Whether the ghosts descend from K-BOT is an assumption.** The existing `OwnerPids` code says the broker's parent is
  K-BOT (slice 0078 evidence); if a ghost was started another way (e.g. by COM through svchost) the registry will not see it.
  The Shutdown log then lists it under «left alone» with its parent - read that line after the next ghost.
- WM_CLOSE on a hung ghost does nothing by itself; the kill after the 1.5 s wait is the part that works there.
- Only a NORMAL close is covered: a crash or a kill of K-BOT from the Task Manager skips `Shutdown` (a list of pids kept
  on disk for the next start would cover it - not built, not asked).
- The DevHarness exit path (`RunHarness`) is not covered; it has its own Adobe clean-up.
- A document Adobe is still asking about («save changes?») is killed after the wait instead of answered; at this point
  the documents are already closed by the viewers.
- No help / NOUTATI change (operator order; nothing the operator sees).
