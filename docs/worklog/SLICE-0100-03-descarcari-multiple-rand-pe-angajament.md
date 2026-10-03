# SLICE-0100-03 — multi-download: a row per running angajament, progress bar, X that stops one (operator request, 03.10.2026)

Branch: `SLICE-0108-credit-pe-clasificatie` (work tree shared with other slices). Extends slice 0100.

## What changed and why

The operator wants the «Coada robotului» window, when several angajamente are downloaded at once, to show
**every running download on its own row**: a progress bar filled per angajament, an **X** that stops ONLY that
download (and closes its tab), and at the bottom how many are still waiting for a tab. A row that ends leaves the
grid and the next waiting angajament takes its place. **One angajament alone gets no grid** (operator's answer:
«la un singur angajament NU arătăm fereastra»).

Decisions taken (assumptions, no question asked — confidence above 75%):
- **Progress = main-flow steps** (operator's answer: «bara după pași»). The executor already reported
  `_progressCallback(step, steps)` for the main flow; the worker tabs just never passed a callback. The bar is
  coarse (the receptions and history sections are few big steps) but never goes backwards.
- **«Fereastra» = the queue window.** The grid lives INSIDE `RobotQueueForm`, between «În lucru» and the list of
  waiting tasks, and exists only for a run of **two or more** angajamente. With one angajament nothing is added
  (and the window does not open by itself for it: the old rule, more than one FOREXE action, stays).
- **The window opens by itself** when a run of two or more starts (the task is ONE queue task, so the old rule
  «more than one action» would not have opened it).

**Robot (`KBot.Forexe`).**
- `ParallelRunHooks` (`JobModels.vb`): out — `JobStarted(job, worker)`, `JobProgress(job, step, steps)`; in —
  `StopJob(job)`. `IForexeRunner.RunJobsParallelAsync` gets a `hooks` parameter (may be Nothing).
- `ForexeRunner.RunJobOnOwnTabAsync`: a per-job stop token, linked to the run's cancel token, goes to the worker
  (`OpenWorkerAsync(token, progressCallback)`); when the stop fires the runner **closes the worker's tab** (a step
  waiting on the page is cut short; a token is only looked at between steps). A job stopped while still waiting is
  skipped when its turn comes (no tab opened). A stopped job is `ParallelJobOutcome.StoppedByOperator`, never a
  failure; even a job that was about to succeed is discarded (never becomes the kept tab).
- `FakeForexeRunner` (bench) follows the same contract.

**Shell.**
- `ParallelDownloadBoard` (new, `KBot.App/Forexe`): what the window draws — the running rows (code, percent,
  «stopping»), the waiting count; thread-safe; events `Changed`, `Started` (only for 2+). One per
  `ForexeController` (`ParallelBoard`), filled by `DownloadNodesParallelAsync` (`Begin` / `JobEnded` / `Finish`).
- `ProceseazaRaspunsul`: a stopped outcome returns `ParallelNodeResult.Stopped` before any dump / store; the shell
  (`KbotForm.Parallel.vb`) skips it silently (only a note if captures were taken) and the final «X din Y nu s-au
  actualizat» list neither lists nor counts it.
- `RobotQueueForm` (+ Designer): `pnlDescarcari` (`gridDescarcari` — a `KBotDataView`, columns `cod`, `prog`
  (ProgressBar), `opreste` (Button «X»), no header, not selectable — and `lblInCoada`). Shown only for a run of
  2+; the window grows by the panel's height while it is shown (upwards, it sits above the footer) and shrinks
  back. Only the bars are rewritten when the rows did not change (no flicker).
- `KbotForm.RobotQueue.vb`: `ParallelBoard.Started` opens the window (no steal of focus).

## Files touched

`src/KBot.Forexe/JobModels.vb`, `IForexeRunner.vb`, `ForexeRunner.vb`, `Executor/WorkflowExecutor.Core.vb`;
`src/KBot.App/Forexe/ParallelDownloadBoard.vb` (new), `ForexeController.Parallel.vb`, `RobotQueueForm.vb`,
`RobotQueueForm.Designer.vb`; `src/KBot.App/KbotForm.Parallel.vb`, `KbotForm.RobotQueue.vb`;
`src/KBot.App/HarnessTests/FakeForexeRunner.vb`; `tests/KBot.App.Tests/ForexeControllerFailureTests.vb` (the
signature of the fake runner only — it had to follow the interface); help (0000-48).

## Test results

`dotnet build src\KBot.App\KBot.App.vbproj -c Debug`: 0 warnings, 0 errors. No tests run, nothing run on screen
(operator's rule). `tests/KBot.App.Tests` did not compile before this slice either (see 0100 open threads) — it was
not rebuilt as a green check.

## Left unverified / deferred

- **Nothing seen on screen and never run against FOREXE.** To prove: a run of 3+ angajamente with 2 threads (rows,
  bars moving, the waiting counter, a finished row replaced by the next); X on a running row (tab closes, the others
  go on, nothing of it ingested, no error list); X on a row that just ended (quiet); a run of ONE (no grid, window
  does not open); `Oprește curenta` still stops all.
- The grid layout in the designer (widths in the 144 dpi designer, panel height computed at run time from
  `RowHeight`) was written by hand; the window was never opened in the VS designer.
- Bar granularity: by main-flow steps only. Progress INSIDE the reception loop (`ForEach`) is not reported.
- No picture of the new grid yet: capture tag `coada-descarcari-multiple` added in the help (0000-48), picture to be taken.
- Closing a tab mid-step makes Playwright throw «target closed»; it is recognised as «stopped by operator» only
  because the stop token was already cancelled — a tab that dies by itself is still an ordinary failure.

## Follow-up (03.10.2026): queue window placement

Operator: the queue window opened a little to the right of the shell's right edge and, with the shell maximized,
went off the screen. Cause: `ShowRobotQueue` computed the location from the window's width BEFORE `Show`, i.e. the
designer's size, but the window is scaled to the screen's dpi only when its handle is created, so it ended wider
than it was placed for. Fix: `KbotForm.RobotQueue.vb` — `Show` first, then the new `PlaceRobotQueue` (real size):
right edge flush with the shell's client right edge, bottom edge just above the footer band, clamped to the
screen's working area. Build clean (0 warnings); not seen on screen (maximized and normal shell, 100% / 150% dpi to try).

## Follow-up 2 (03.10.2026): waiting list with X, window shrinks, no false «urmează ingestia»

Operator, after seeing the window (3 rows running, «Încă 3 în coadă.», an empty queue list):
- **The waiting angajamente are listed**, each with an X that takes it out of the run: new grid `gridAsteapta` (code +
  X) under the running grid, a label «Încă N în coadă:» between them; at most 5 rows at once, more scroll.
  `ParallelDownloadBoard.Snapshot.WaitingCodes` + `RemoveWaiting(cod)` (drops it from the board at once and tells the
  runner to skip it when its turn comes — it is still in the runner's FIFO).
- **No waiting angajament → the list is not shown and the window shrinks** (the running grid stays). The queue's own
  list (other robot tasks) shows only when it has tasks. `RobotQueueForm.FitWindow` sizes panel + window to what is
  on show, keeping the BOTTOM edge (above the footer); it gives up the form's minimum height while a multi run is
  on and restores it (and the old height) afterwards; `OnShown` fits again with the real scaled sizes.
- **Status line:** with 0 successful downloads the footer no longer says «…0 reușite din 6; urmează ingestia…» (there
  is nothing to ingest): it goes back to «În așteptare...». With some successes the denominator no longer counts
  the ones the operator stopped.
- Help (`coada.md`) and `NOUTATI.md` (1.1.1.7: 2 lines) updated.
Build clean (0 warnings). Not seen on screen: the heights are computed from the scaled row / label sizes by hand —
check 1, 3 and 8 waiting angajamente, the last one finishing (window shrinking), X on a waiting row, and the end of run.

## Follow-up 3 (03.10.2026): grids too short, scroll bars

Operator's screenshot: with 3 running + 3 waiting the third row of each grid was cut and both grids showed a vertical
scroll bar. Cause: `FitWindow` sized the rows with `DeviceDpi / 96`, but the grid scales by its own factor
(`AppScaling.FactorFor`, which follows the operator's text size / zoom) and has a frame. Now it uses
`gridDescarcari.DpiScaleY`: row = `round(RowHeight * scale)`, frame = 2 × border + 2 px slack. Build clean; not seen on screen.

## Follow-up 4 (03.10.2026): one waiting grid instead of the list box

Operator: «avem și lstCoada — nu putem folosi gridAsteapta în locul lui?». Done: `lstCoada` (ListBox) and the «Scoate»
button (`btnScoate`, which worked on the selected list item) are gone from `RobotQueueForm`. `gridAsteapta` now fills
the middle of the window ALWAYS and shows what waits: the angajamente of a multi-thread run still without a tab
(Tag = code) and then the robot tasks of the queue («1. label   (hh:mm:ss)», Tag = id), each with an X (angajament →
`ParallelDownloadBoard.RemoveWaiting`, task → `RobotQueue.Cancel`). `pnlDescarcari` is now the Fill panel holding
`gridDescarcari` (running, Top, hidden outside a multi run), `lblInCoada` («În așteptare (N):», or
«(nicio sarcină în așteptare)» outside a multi run when nothing waits) and `gridAsteapta`. In a multi run with nothing
waiting the label and the grid are hidden and the window shrinks (as before). «Golește coada» and «Oprește curenta» stay.
Help: `coada.md` (list rows have an X, «Scoate» point removed), `tur-coada.md` (list step now targets `lblInCoada`
— the grid is hidden when nothing waits —, «Scoate» step removed). Build clean (0 warnings). Not seen on screen.
Designer edited by hand again (controls removed, `pnlDescarcari.Dock = Fill`).

## Follow-up 5 (03.10.2026): columns editable in the designer

The columns of `gridDescarcari` (`cod`, `prog`, `opreste`) and `gridAsteapta` (`cod`, `scoate`) were already declared in
`RobotQueueForm.Designer.vb` (VS re-saved them with `HeaderTextAlign`). The only thing still fixed in code was the caption
of the two X buttons (`"X"` written into every row). A Button cell shows its column's `HeaderText` when the cell has no
text, so the caption now lives in the designer (`HeaderText = "X"` on both button columns; the headers are hidden by
`ShowHeader = False`) and the code no longer writes it. Build clean.

## Follow-up 6 (03.10.2026): icon on the X buttons (operator's new button properties)

The operator set `ButtonImage` (`minus_red`) and `ButtonMargin` (0) in the designer, but on the TEXT column `cod` of the top
grid (`KBotDataColumn3`), where they do nothing. Moved to its button column `opreste` (`KBotDataColumn5`) and applied the same to
the waiting grid's button column `scoate` (`KBotDataColumn2`). With a picture an empty caption stays empty, so both columns'
`HeaderText` went back to `""` (no «X» anywhere, code or designer; the caption can be set with `ButtonText`). Build clean; not seen.

## Follow-up 7 (03.10.2026): playground for the queue window (DevHarness, Debug only)

Operator: see the window in the harness, with a playground per column that saves what was changed so it can be written into the
designer; simulate angajamente in both grids.
- New harness test «Coada robotului — playground coloane (descărcare multiplă simulată)» (category FOREXE):
  `src/KBot.App/HarnessTests/QueuePlaygroundTest.vb` + `QueuePlaygroundForm.vb` / `.Designer.vb` (in KBot.App because the
  window, the queue and the controller are there; everything inside `#If DEBUG`).
- It opens the REAL `RobotQueueForm` (its grids are `Friend`) over a real queue + controller + `FakeForexeRunner`. Buttons:
  «Pornește descărcarea multiplă» (N angajamente with made-up codes `AAB` + 8 characters, T tabs, S seconds each — the
  running ones fill the top grid, the rest the bottom one), «+ sarcină simplă în coadă» (a plain task, shows below too),
  «Oprește tot», «(Re)deschide fereastra cozii».
- A `PropertyGrid` on the chosen grid or column (combo: both grids and each column by key) — every change shows at once
  in the real window. Extra combo for the button picture of a column (the project's resources, by name).
- «Salvează ce am modificat» → `QueueDesignerExporter` writes, per grid and per column, the lines the designer would
  serialize (only what each property's own `ShouldSerialize*` says is set: theme-resolved colours and fonts stay out), with the
  placeholders `{GRID}` / `{COL}` and the column `Key` in the block header; into the text box, the clipboard and
  `<AppDir>\Logs\playground_coada_designer.txt`. A property of a type the exporter does not write becomes a comment line.
- The controller writes its usual files for the simulated downloads (answer, package, run log) as in the queue bench.
Build clean (0 warnings); NOT run, never seen on screen.

## Follow-up 8 (03.10.2026): the operator's playground result written into the designer

The text saved by the playground (19:29) was written into `RobotQueueForm.Designer.vb`: both grids and all five columns
(top grid: `AlternatingRows = False`, `BorderWidth = 0`; columns `cod` 113, `prog` 178 (`CellBorders = None`), `opreste` 40
(`CellBorders = All`, picture + margin); bottom grid: `cod` 291, `scoate` 40 with picture + margin). The widths are the
logical values (their sums equal the grid's logical width). `FitWindow` now takes the frame from each grid's own
`BorderWidth` (the top one is 0, so no spare pixels are added any more). Build clean; not seen.
