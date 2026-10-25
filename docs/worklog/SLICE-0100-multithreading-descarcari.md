# SLICE-0100/0109 — Istoricul actualizării mai multor angajamente

Completare D001-45: 0109 păstrează aceeași fereastră de alegere și când multi-thread este oprit; în acel mod folosește coada robotului, nu taburi paralele. Condițiile vechi de afișare aparțin etapelor inițiale; corpurile și probele lor rămân datate.

Reunire documentară D001-24. 02 înlocuiește condiția opțiunilor avansate prin permisiunea serverului; 03 și cele opt follow-up-uri păstrează progresul, oprirea individuală, ferestrele și valorile din designer. Raportările operatorului despre fereastră sunt delimitate de reparațiile încă nevăzute. 04 tratează clicul acoperit, 05 păstrează capturile vechi fără zgomot când nu există capturi. Toate probele și limitele sunt istorice.

[0100-multithreading](#slice-0100-baza) · [0100-02](#slice-0100-02) · [0100-03](#slice-0100-03) · [0100-04](#slice-0100-04) · [0100-05](#slice-0100-05) · [0109](#slice-0109)

<a id="slice-0100-baza"></a>

## SLICE-0100 — Multi-thread downloads: several angajamente at once, one FOREXE tab each (operator request, 01.10.2026)

Branch: `SLICE-0100-Multithreading`. Help for this slice is recorded as **0000-34** (on `master`), see [0000-34](SLICE-0000-23-tururi-pe-butoane-istoric-capturi-estompate.md#slice-0000-34).

### What changed and why

**The robot (KBot.Forexe).**
- `IForexeRunner.RunJobsParallelAsync` / `ForexeRunner.RunJobsParallelAsync`: up to `min(maxThreads, 10)` jobs run
  together, each in a FRESH tab of the SAME browser context (same login). The rest wait in a FIFO
  `ConcurrentQueue`; a worker that finishes takes the next job. Every answer — failures, `<Exit>`s and timeouts
  included — is appended to ONE list when ITS tab finishes (completion order); nothing is processed meanwhile; one
  tab giving up never stops the others (each job is caught inside `RunJobOnOwnTabAsync`). A cancel stops workers
  from taking new jobs; the ones left in the queue come back as «not started».
- `WorkflowExecutor.OpenWorkerAsync` (a second executor that owns ONE tab; own variables / loop state / workflow;
  `CloseAsync` closes the tab only), `CloseOtherTabsAsync`, `AdoptTabAsync`.
- **One tab left at the end** (operator, 01.10.2026): the tab of the LAST job that finished WITHOUT an error stays
  open; if the last one failed, the one before it, and so on (`ParallelRun.Keeper`: a failed job's tab closes at
  once, a successful one replaces the previous keeper; at most N + 1 tabs at any moment). The primary executor then
  ADOPTS that tab as its page (`AdoptTabAsync`: watcher / recorder / Wicket flags cleared, the in-page menu put back
  when it was running, the old main tab closed, "browser closed" notice muted for that). No job succeeded → the
  main tab stays and the others are closed.
- One `JobHistoryManager` entry for the whole run (it keeps a single «current job»). Lines of each tab start with
  `[<cod>] ` (`RichTextBoxLogger.ScopeTag`, an `AsyncLocal`).
- The main page is veiled / input-locked for the run (as `RunJobAsync` does).

**The coordinator (KBot.App).** `ForexeController.DownloadNodesParallelAsync` (`ForexeController.Parallel.vb`): builds one job per angajament (forward / REVERSE from the local history, read BEFORE the gate closes), runs the batch gated, and when ALL downloads ended turns each answer into a saved local package, in finish order. Replay mode falls back to one-by-one. `Connected` event raised after a successful connect (after the busy flag drops).

**The shell (KbotForm.Parallel.vb).** `ActualizeazaMaiMulteAsync`: ONE robot-queue task = pre-questions (`AsocierePermiteAsync`, reception picker) → the parallel download → the ingest of the answers ONE AT A TIME (`DuLaIngestieAsync`, same two-phase road) → ONE warning listing the failures (a good run is silent). Doors: tree menu row «Actualizează angajamente...» (`ActualizareMultiplaForm`, ticks, «Bifează cele neactualizate de N zile»); on connect (`Controller_Connected` → posted with `ExecutionContext.SuppressFlow`, so the queue does not take it for part of the connecting task); after the list refresh brought new angajamente in the tree, only in multi-thread mode: «Dorești actualizarea angajamentelor noi?».

**Data.** `FX_Angajamente.DataActualizare datetime NULL` (`sql/0100_fx_angajamente_data_actualizare.sql`, with slice comments), set to `NOW()` at the commit of the ingest (`prelucrare.py`; error 1054 tolerated until the DDL is applied), returned by `GET /api/forexe/tree` (`tree.py`; literal NULL where the column is missing), carried in `AngajamentTreeInfo.DataActualizare` and shown in the tree tooltip («Actualizat: …»).

**Settings (`AppSettings`).** `MultiThreadDownloads`, `DownloadThreads` (1–10, default 3), `AutoUpdateOnConnect`, `AutoUpdateDays` (1–10, default 7), `UpdateAllReceptiiByDefault` + `…InEffect` properties (all need `AdvancedOptions`). Settings → Application → «Generale» regrouped into «Fereastra principală» / «FOREXE» / «Avansat» (behaviour unchanged) plus the advanced-only group «Descărcări multiple».

### Decisions taken (assumptions, said once)
- «Old» = was downloaded before (has history or indicators) AND (no `DataActualizare` OR older than N days).
  A header-only angajament (never downloaded) is NOT old; one downloaded before the column existed IS.
- «Actualizează implicit toate recepțiile» applies only while multi-thread is in effect (single-thread unchanged),
  and then to EVERY download that would open the reception picker. The on-connect and the new-angajamente doors
  never ask.
- Multi-thread needs the advanced options (item 3 of the request); the dependent options too.
- Default threads 3, default days 7 (the request fixed only the maxima).
- No hard per-job timeout was added: a tab ends through the workflow's own wait timeouts (× the timeout multiplier).

### Files touched
`PYTHON/routes/forexe/prelucrare.py`, `tree.py`; `sql/0100_fx_angajamente_data_actualizare.sql`;
`src/KBot.Common/AppSettings.vb`; `src/KBot.Domain/AngajamentTreeInfo.vb`; `src/KBot.Api/ApiClient.vb`,
`UpsertAngajamenteRequest.vb`; `src/KBot.Forexe/` `IForexeRunner.vb`, `ForexeRunner.vb`, `JobModels.vb`,
`Executor/WorkflowExecutor.Core.vb`, `Executor/WorkflowExecutor.Browser.vb`, `Helpers/RichTextBoxLogger.vb`;
`src/KBot.App/` `Forexe/ForexeController.vb`, `ForexeController.Parallel.vb` (new), `ActualizareMultiplaForm(.Designer).vb`
(new), `KbotForm.Parallel.vb` (new), `KbotForm.vb`, `.ForexeWatch.vb`, `.Download.vb`, `.Ingest.vb`, `.Tree.vb`,
`.TreeOptions.vb`, `Setari/SetariAplicatieView(.Designer).vb`, `HarnessTests/FakeForexeRunner.vb`;
`tests/KBot.App.Tests/ForexeControllerFailureTests.vb` (interface member only).

### Test results
`dotnet build KBot.sln`: all `src/` projects build with **0 warnings, 0 errors**. No tests written or run (operator
rule), nothing run on screen, nothing run against FOREXE or the server. `tests/KBot.App.Tests` already did not
compile before this slice (`MainForm` constructor `capturiApi`, `JobRequest` indexer in `ForexeAnswerStoreTests`);
only the new interface member was added there.

### Unverified / deferred
- **The browser side is unproven**: that a second tab of the same context reuses the FOREXE login without a new
  certificate prompt (the operator observed several tabs work); that background tabs run the workflows correctly
  (Chromium flags against throttling are already set); that the docked window keeps showing the main tab while
  workers open (`BringToFront` after each open); that `AdoptTabAsync` leaves docking and the in-page menu working.
- Server: the DDL must be applied FIRST (000_DEMO, every unit database, AVACONT_SURSA), then `prelucrare.py` and
  `tree.py` deployed. Python tests not run (`test_forexe_tree` mocks may need the new column query).
- A DDF send or other flow that creates an angajament does not ask about «new angajamente» (only the list refresh does).
- The «Actualizează angajamente» window and the settings group were not seen on screen (designer files hand-written).
- Per-job history entries are gone in a parallel run (one entry for the run).

---

<a id="slice-0100-02"></a>

## SLICE-0100-02 — `Setari` table: the server decides multi-thread (operator request, 01.10.2026)

Branch: `SLICE-0100-Multithreading`. Changes the decision of slice 0100 that multi-thread is an advanced option.

### What changed and why

The operator wants to switch multi-thread on and limit the tabs **from the server**, per database. So:

**Data.** A new table `Setari` on every database (`sql/0100_02_setari.sql` = DDL + default rows on `AVACONT_SURSA`, the template new units are cloned from; `sql/0100_02_setari_toate_bazele.sql` = a block that walks `AVACONT_COMUN.Unitati.DC` and creates the table + inserts the default rows in every existing unit database). Columns: `Cheie` (PK, ASCII), `TextVizibil` (what the operator may read), `Valoare` varchar(255) (one column for int / date / text), `Tip` enum('int','date','text'). Defaults: `Multithread = 0`, `Multithread_Max = 1`. `INSERT IGNORE`: a value changed by hand survives a second run.

**Server.** `PYTHON/routes/setari.py` (registered in `main.py`): `GET /api/setari` (bearer) returns the rows of the session's database, `Valoare` typed by `Tip` (a value that does not fit → null, logged). No table → 200 with no rows.

**Client.**
- `KBot.Common/ServerSettings.vb`: static holder, filled by `Apply`, `Changed` event; `MultithreadAllowed`
  (`Multithread <> 0`), `MultithreadMax` (≥ 1), generic `GetInt` / `GetDate` / `GetText`. No row = off.
- `KBot.Api/ISetariApi.vb` + `ApiClient.Setari.vb`: `GetServerSettingsAsync`.
- `KbotForm.Parallel.vb` `LoadServerSettingsAsync`, called after the periods at login (`KbotForm.vb`) and at a change
  of unit (`KbotForm.Units.vb`); a failure is logged and leaves everything off.
- `AppSettings.MultiThreadInEffect` = server allows AND the operator's switch (the advanced options no longer count);
  new `DownloadThreadsCeiling` = `min(10, Multithread_Max)`; `DownloadThreadsInEffect` is clamped to it.
- The «Descărcări multiple» group left «Setări › Aplicație»; new page `SetariMultithreadView` (nav key
  `multithread`, «Descărcări multiple»), shown by `SetariForm` only while `Multithread = 1` (follows
  `ServerSettings.Changed`; if it is on screen when the server turns off, hands over to «Aplicație»). Its main
  checkbox is enabled from `Multithread`, the tabs field is limited by `Multithread_Max`.

### Decisions taken (assumptions)
- The value is read from the database the operator logged in to (the unit), not from a global place.
- Read-only for K-BOT: changed on the server with an `UPDATE`, effective at the operator's next login / unit change.
- Defaults 0 / 1 as asked; the operator's saved `DownloadThreads` (default 3) is capped, not rewritten.

### Files touched
`sql/0100_02_setari.sql`, `sql/0100_02_setari_toate_bazele.sql`; `PYTHON/routes/setari.py`, `PYTHON/main.py`;
`src/KBot.Common/ServerSettings.vb` (new), `AppSettings.vb`; `src/KBot.Api/ISetariApi.vb`, `ApiClient.Setari.vb` (new);
`src/KBot.App/` `KbotForm.vb`, `KbotForm.Units.vb`, `KbotForm.Parallel.vb`, `Setari/SetariForm(.Designer).vb`,
`Setari/SetariAplicatieView(.Designer).vb`, `Setari/SetariMultithreadView(.Designer).vb` (new).

### Test results
`dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors. No tests written or run (operator rule), nothing run
on screen, SQL not run, server route not run.

### Unverified / deferred
- **Deploy order:** the DDL (both files) before `setari.py` reaches the VPS; until then the route answers «no rows» and
  multi-thread stays off everywhere. `0100_02_setari_toate_bazele.sql` uses `BEGIN NOT ATOMIC`, `PREPARE` and a cursor
  — never run; run it as root in HeidiSQL / `mysql`. Check its report line.
- Whether the provisioning job copies **rows** of `AVACONT_SURSA` into new units (it clones the tables) is unchecked;
  a new unit without the default rows reads as «off» — safe.
- The new page and the removed group were not seen on screen (designer hand-written).
- Help: done as 0000-36 (`SLICE-0000-36-ajutor-setari-multithread.md`); no screenshot taken (needs K-BOT on a unit with Multithread = 1).

---

<a id="slice-0100-03"></a>

## SLICE-0100-03 — multi-download: a row per running angajament, progress bar, X that stops one (operator request, 03.10.2026)

Branch: `SLICE-0108-credit-pe-clasificatie` (work tree shared with other slices). Extends slice 0100.

### What changed and why

The operator wants the «Coada robotului» window, when several angajamente are downloaded at once, to show **every running download on its own row**: a progress bar filled per angajament, an **X** that stops ONLY that download (and closes its tab), and at the bottom how many are still waiting for a tab. A row that ends leaves the grid and the next waiting angajament takes its place. **One angajament alone gets no grid** (operator's answer: «la un singur angajament NU arătăm fereastra»).

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

### Files touched

`src/KBot.Forexe/JobModels.vb`, `IForexeRunner.vb`, `ForexeRunner.vb`, `Executor/WorkflowExecutor.Core.vb`; `src/KBot.App/Forexe/ParallelDownloadBoard.vb` (new), `ForexeController.Parallel.vb`, `RobotQueueForm.vb`, `RobotQueueForm.Designer.vb`; `src/KBot.App/KbotForm.Parallel.vb`, `KbotForm.RobotQueue.vb`; `src/KBot.App/HarnessTests/FakeForexeRunner.vb`; `tests/KBot.App.Tests/ForexeControllerFailureTests.vb` (the signature of the fake runner only — it had to follow the interface); help (0000-48).

### Test results

`dotnet build src\KBot.App\KBot.App.vbproj -c Debug`: 0 warnings, 0 errors. No tests run, nothing run on screen (operator's rule). `tests/KBot.App.Tests` did not compile before this slice either (see 0100 open threads) — it was not rebuilt as a green check.

### Left unverified / deferred

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

### Follow-up (03.10.2026): queue window placement

Operator: the queue window opened a little to the right of the shell's right edge and, with the shell maximized, went off the screen. Cause: `ShowRobotQueue` computed the location from the window's width BEFORE `Show`, i.e. the designer's size, but the window is scaled to the screen's dpi only when its handle is created, so it ended wider than it was placed for. Fix: `KbotForm.RobotQueue.vb` — `Show` first, then the new `PlaceRobotQueue` (real size): right edge flush with the shell's client right edge, bottom edge just above the footer band, clamped to the screen's working area. Build clean (0 warnings); not seen on screen (maximized and normal shell, 100% / 150% dpi to try).

### Follow-up 2 (03.10.2026): waiting list with X, window shrinks, no false «urmează ingestia»

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

### Follow-up 3 (03.10.2026): grids too short, scroll bars

Operator's screenshot: with 3 running + 3 waiting the third row of each grid was cut and both grids showed a vertical scroll bar. Cause: `FitWindow` sized the rows with `DeviceDpi / 96`, but the grid scales by its own factor (`AppScaling.FactorFor`, which follows the operator's text size / zoom) and has a frame. Now it uses `gridDescarcari.DpiScaleY`: row = `round(RowHeight * scale)`, frame = 2 × border + 2 px slack. Build clean; not seen on screen.

### Follow-up 4 (03.10.2026): one waiting grid instead of the list box

Operator: «avem și lstCoada — nu putem folosi gridAsteapta în locul lui?». Done: `lstCoada` (ListBox) and the «Scoate» button (`btnScoate`, which worked on the selected list item) are gone from `RobotQueueForm`. `gridAsteapta` now fills the middle of the window ALWAYS and shows what waits: the angajamente of a multi-thread run still without a tab (Tag = code) and then the robot tasks of the queue («1. label   (hh:mm:ss)», Tag = id), each with an X (angajament → `ParallelDownloadBoard.RemoveWaiting`, task → `RobotQueue.Cancel`). `pnlDescarcari` is now the Fill panel holding `gridDescarcari` (running, Top, hidden outside a multi run), `lblInCoada` («În așteptare (N):», or «(nicio sarcină în așteptare)» outside a multi run when nothing waits) and `gridAsteapta`. In a multi run with nothing waiting the label and the grid are hidden and the window shrinks (as before). «Golește coada» and «Oprește curenta» stay. Help: `coada.md` (list rows have an X, «Scoate» point removed), `tur-coada.md` (list step now targets `lblInCoada` — the grid is hidden when nothing waits —, «Scoate» step removed). Build clean (0 warnings). Not seen on screen. Designer edited by hand again (controls removed, `pnlDescarcari.Dock = Fill`).

### Follow-up 5 (03.10.2026): columns editable in the designer

The columns of `gridDescarcari` (`cod`, `prog`, `opreste`) and `gridAsteapta` (`cod`, `scoate`) were already declared in `RobotQueueForm.Designer.vb` (VS re-saved them with `HeaderTextAlign`). The only thing still fixed in code was the caption of the two X buttons (`"X"` written into every row). A Button cell shows its column's `HeaderText` when the cell has no text, so the caption now lives in the designer (`HeaderText = "X"` on both button columns; the headers are hidden by `ShowHeader = False`) and the code no longer writes it. Build clean.

### Follow-up 6 (03.10.2026): icon on the X buttons (operator's new button properties)

The operator set `ButtonImage` (`minus_red`) and `ButtonMargin` (0) in the designer, but on the TEXT column `cod` of the top grid (`KBotDataColumn3`), where they do nothing. Moved to its button column `opreste` (`KBotDataColumn5`) and applied the same to the waiting grid's button column `scoate` (`KBotDataColumn2`). With a picture an empty caption stays empty, so both columns' `HeaderText` went back to `""` (no «X» anywhere, code or designer; the caption can be set with `ButtonText`). Build clean; not seen.

### Follow-up 7 (03.10.2026): playground for the queue window (DevHarness, Debug only)

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

### Follow-up 8 (03.10.2026): the operator's playground result written into the designer

The text saved by the playground (19:29) was written into `RobotQueueForm.Designer.vb`: both grids and all five columns
(top grid: `AlternatingRows = False`, `BorderWidth = 0`; columns `cod` 113, `prog` 178 (`CellBorders = None`), `opreste` 40
(`CellBorders = All`, picture + margin); bottom grid: `cod` 291, `scoate` 40 with picture + margin). The widths are the
logical values (their sums equal the grid's logical width). `FitWindow` now takes the frame from each grid's own
`BorderWidth` (the top one is 0, so no spare pixels are added any more). Build clean; not seen.

---

<a id="slice-0100-04"></a>

## SLICE-0100-04 - robot click covered by FOREXE's fixed bar; Reverse workflow timeouts (operator request, 03.10.2026)

Trigger: the operator's log of 03.10.2026 16:14, «Actualizare multiplă»: 1 of 3 angajamente failed with `Timeout 5000ms exceeded ... <footer id="footer" class="navbar navbar-fixed-bottom"> intercepts pointer events` while clicking `table.table-striped... tbody tr:nth-child(5) .glyphicon-eye-open` (row 5 of the indicators).

### What changed and why

- `src/KBot.Forexe/Workflows/adlop - Prelucrare Completa Reverse.wfl` (the workflow «Actualizare multiplă» and the reverse
  download run): every `timeout="5"` is now `timeout="15"`, the value the normal `adlop - Prelucrare Completa.wfl` already
  used for the same clicks. Two stay at 5 on purpose: the `IfExists` probes for the optional «Înapoi» button and the
  «Afișează informații complete» button (the normal workflow keeps them at 5 too; raising them would only slow the
  «button is not there» case by 10 s each). The two `timeout="1"` probes are untouched.
- `src/KBot.Forexe/Executor/Actions/WorkflowExecutor.Actions.Click.vb`: when the standard click fails with Playwright's
  «intercepts pointer events» (checked by `IsCoveredClick`) and the step is not `Force`, the element is scrolled to the
  MIDDLE of the window (`scrollIntoView({block:'center'})`) and clicked once more, with a console warning. The second
  failure, if any, is the one reported (same exception path as before). Applies to every workflow, not only Reverse.
- FileVersion: `KBot.Forexe` 1.0.19 -> 1.0.20.

### Files touched

`src/KBot.Forexe/Workflows/adlop - Prelucrare Completa Reverse.wfl`, `src/KBot.Forexe/Executor/Actions/WorkflowExecutor.Actions.Click.vb`, `src/KBot.Forexe/KBot.Forexe.vbproj`.

### Test results

`dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors. Nothing run, nothing seen on screen (operator rule: no tests / renders unless asked).

### Left unverified / deferred

- That the footer, not slowness, was the cause is a reading of the log: the real page was not seen. Playwright normally
  changes its scroll alignment between its own retries; why it did not within 5 s here is not known. The longer timeout
  and the centred retry are both cheap guards, neither is proved on that page.
- The other workflows with 5 s on the same eye click (`Creare\adlop - Definitivare Angajament.wfl`,
  `Definitivare_Derulare Angajament.wfl`) were NOT changed (outside the request); they get the centred retry.
- No help / NOUTATI change (operator order; nothing the operator sees apart from a console line).

---

<a id="slice-0100-05"></a>

## SLICE-0100-05 - no «nicio captură» noise after «Actualizare multiplă» (operator request, 03.10.2026)

Trigger: the operator's log of 03.10.2026 16:14 showed two warnings per angajament, e.g. `[MainForm.TrimiteCapturileAsync] «AAB2MA25TCB»: nicio captură de rezervare de trimis — nu există niciuna în «C:\KBOT\Capturi\AAB2MA25TCB». Pe disc: «C:\KBOT\Capturi» e gol.` (and the same for «receptie»), for every angajament that downloaded fine. Operator: «i want the noise gone».

### What changed and why

- The pictures of the FOREXE page are taken only by the operator flows (reception saved, DDF sent): the parallel robot
  (`ForexeController.Parallel.vb`) has no capture code. So after each successful parallel download
  `ActualizeazaMaiMulteCuCoadaAsync` asked `TrimiteCapturileAsync` to send two kinds of pictures that were never taken,
  and it warned about it.
- `src/KBot.App/KbotForm.Parallel.vb`: the two direct `TrimiteCapturileAsync` calls became a loop that calls it only when
  `_controller.CapturileDe(cod, fel).Count > 0`. Pictures LEFT ON DISK by an earlier session (kept with «Nu») are still
  sent exactly as before; an empty-handed run says nothing. `TrimiteCapturileAsync` itself is unchanged: the other callers
  (reception / DDF flows) keep their warning, where a missing picture is news.

### Files touched

`src/KBot.App/KbotForm.Parallel.vb`.

### Test results

`dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors. Nothing run (operator rule).

### Left unverified / deferred

- `CapturileDe` also stamps the marker number on waiting pictures as a side effect (idempotent; `TrimiteCapturileAsync`
  calls it again). Read from code, not run.
- `KBot.App` FileVersion not bumped here: the project was already bumped in the same uncommitted work (1.1.1.5 -> 1.1.1.7).
- No help / NOUTATI change (operator order).

## Repere unice din registrul 0100

Proba veche propusă pentru taburi era întâi pe 2–3 angajamente, apoi 10; nu este executată aici. Capturile enumerate în Open threads pentru 0100-02 sunt păstrate în [0000-36](SLICE-0000-36-ajutor-setari-multithread.md#captures), fără copierea lor încă o dată.

---

<a id="slice-0109"></a>

## SLICE-0109 - «Actualizează angajamente...» also in one-at-a-time (single thread) mode (operator request, 03.10.2026)

Operator: the extra row the header right icon of `KbotForm.tree` shows for multi-thread mode («Actualizează angajamente...») must also be there in single thread mode; there the ticked angajamente are added to the robot queue, exactly as when the individual refresh icon of several angajamente is pressed one after the other (slice 0098). Also to be said in the help, in the K-BOT release notes file and in the guided tour.

### What changed and why

- `KbotForm.TreeOptions.vb`: the «Actualizează angajamente...» row of the tree options menu is no longer gated by
  `MultiThreadInEffect`; it is always offered. (The caption had a typo, «Actualizăeză»; fixed on the same line.)
- `KbotForm.Parallel.vb` `DeschideActualizareaMultipla`: the `MultiThreadInEffect` guard is gone. Multi-thread on = as
  before (`ActualizeazaMaiMulteAsync`, one robot task). Multi-thread off = new `PuneNodurileInCoadaAsync(coduri)`.
- `KbotForm.Download.vb`: the body of `Tree_RightIconClicked` moved into `PuneNodulInCoadaAsync(cod)`
  (`MarcheazaDescarcareaFaraIntrebare` + `_robotQueue.RunAsync("nod|" & cod, ...)`, same duplicate / failure handling), so
  the node icon and the window share ONE path. `PuneNodurileInCoadaAsync` calls it for each ticked code, in grid (= tree)
  order, without awaiting each (every call queues synchronously up to its first wait), then awaits all. Result, identical to
  successive clicks: the first code is asked about its receptii when the queue was idle, the others skip the picker (all
  receptii); a code already waiting is refused as a duplicate by the queue.
- `ActualizareMultiplaForm.vb`: without multi-thread the header text and the «Actualizează» tooltip say «Coada robotului», one
  after the other (they said «mai multe deodată, fiecare pe un tab»). Class comment updated.

### Help, guided tour, release notes

Recorded as help sub-slice **0000-49** (0000-48 is another thread's, the queue-window board):

- `contabil.forexe.descarcare-multipla`: the menu row is always there; option table row corrected; new subsection «Cu
  descărcarea pe mai multe taburi oprită».
- `contabil.fereastra` (Rotița bullet) and `contabil.forexe.coada` (what goes through the queue): the row named.
- `tours/tur-fereastra.md` step «Lista › Rotița» (`part: header.right`): the last row of the menu and what it does in both modes.
- All carry `<!-- slice: ... 0109 -->`. Capture tag `arbore-meniu-actualizare` still says «porniți descărcarea pe mai multe
  taburi» in its `prepare:`: the row is there without it now, so the picture is not stale, but the note is longer than needed.
- **Release notes (`docs/release-notes/NOUTATI.md`): NOT written.** A section needs the version heading and the
  `release:` marker the release script puts in its request (`docs/release-notes/README.md` §2); a version that does not
  exist yet cannot be invented. The line for the next release, from this worklog:
  `- «Actualizează angajamente...» din meniul listei apare și când descărcarea pe mai multe taburi e oprită: angajamentele bifate intră în «Coada robotului», unul după altul.`

### Files touched

`src/KBot.App/KbotForm.TreeOptions.vb`, `KbotForm.Parallel.vb`, `KbotForm.Download.vb`, `Forexe/ActualizareMultiplaForm.vb`, `HelpContent/contabil/forexe/descarcare-multipla.md`, `coada.md`, `contabil/fereastra.md`, `HelpContent/tours/tur-fereastra.md`.

### Test results

`dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors. Nothing run, nothing seen on screen (operator rule: no test runs / renders unless asked). `Check-Help.ps1 -Coverage`: the only errors left are the `0000-48` tags of the other thread's help edits in `coada.md` / `descarcare-multipla.md` (their worklog and status row do not exist yet).

### Left unverified / deferred

- The queue behaviour (first code asked about receptii, the rest not; a duplicate refused) follows from reading
  `RobotQueue` / `MarcheazaDescarcareaFaraIntrebare`; it was not run. It relies on the `AsyncLocal` of the queue not leaking
  out of the first task's flow into the second `RunAsync` call of the same loop (it should not: async methods restore it).
- `NOUTATI.md` line to be added at the next release (above).

