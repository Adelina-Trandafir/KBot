# SLICE-0100 — Multi-thread downloads: several angajamente at once, one FOREXE tab each (operator request, 01.10.2026)

Branch: `SLICE-0100-Multithreading`. Help for this slice is recorded as **0000-34** (on `master`), see
`SLICE-0000-34-ajutor-descarcari-multiple-capturi-rosii.md`.

## What changed and why

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

**The coordinator (KBot.App).** `ForexeController.DownloadNodesParallelAsync` (`ForexeController.Parallel.vb`):
builds one job per angajament (forward / REVERSE from the local history, read BEFORE the gate closes), runs the
batch gated, and when ALL downloads ended turns each answer into a saved local package, in finish order. Replay
mode falls back to one-by-one. `Connected` event raised after a successful connect (after the busy flag drops).

**The shell (KbotForm.Parallel.vb).** `ActualizeazaMaiMulteAsync`: ONE robot-queue task = pre-questions
(`AsocierePermiteAsync`, reception picker) → the parallel download → the ingest of the answers ONE AT A TIME
(`DuLaIngestieAsync`, same two-phase road) → ONE warning listing the failures (a good run is silent).
Doors: tree menu row «Actualizează angajamente...» (`ActualizareMultiplaForm`, ticks, «Bifează cele neactualizate
de N zile»); on connect (`Controller_Connected` → posted with `ExecutionContext.SuppressFlow`, so the queue does not
take it for part of the connecting task); after the list refresh brought new angajamente in the tree, only in
multi-thread mode: «Dorești actualizarea angajamentelor noi?».

**Data.** `FX_Angajamente.DataActualizare datetime NULL` (`sql/0100_fx_angajamente_data_actualizare.sql`, with slice
comments), set to `NOW()` at the commit of the ingest (`prelucrare.py`; error 1054 tolerated until the DDL is
applied), returned by `GET /api/forexe/tree` (`tree.py`; literal NULL where the column is missing), carried in
`AngajamentTreeInfo.DataActualizare` and shown in the tree tooltip («Actualizat: …»).

**Settings (`AppSettings`).** `MultiThreadDownloads`, `DownloadThreads` (1–10, default 3), `AutoUpdateOnConnect`,
`AutoUpdateDays` (1–10, default 7), `UpdateAllReceptiiByDefault` + `…InEffect` properties (all need
`AdvancedOptions`). Settings → Application → «Generale» regrouped into «Fereastra principală» / «FOREXE» /
«Avansat» (behaviour unchanged) plus the advanced-only group «Descărcări multiple».

## Decisions taken (assumptions, said once)
- «Old» = was downloaded before (has history or indicators) AND (no `DataActualizare` OR older than N days).
  A header-only angajament (never downloaded) is NOT old; one downloaded before the column existed IS.
- «Actualizează implicit toate recepțiile» applies only while multi-thread is in effect (single-thread unchanged),
  and then to EVERY download that would open the reception picker. The on-connect and the new-angajamente doors
  never ask.
- Multi-thread needs the advanced options (item 3 of the request); the dependent options too.
- Default threads 3, default days 7 (the request fixed only the maxima).
- No hard per-job timeout was added: a tab ends through the workflow's own wait timeouts (× the timeout multiplier).

## Files touched
`PYTHON/routes/forexe/prelucrare.py`, `tree.py`; `sql/0100_fx_angajamente_data_actualizare.sql`;
`src/KBot.Common/AppSettings.vb`; `src/KBot.Domain/AngajamentTreeInfo.vb`; `src/KBot.Api/ApiClient.vb`,
`UpsertAngajamenteRequest.vb`; `src/KBot.Forexe/` `IForexeRunner.vb`, `ForexeRunner.vb`, `JobModels.vb`,
`Executor/WorkflowExecutor.Core.vb`, `Executor/WorkflowExecutor.Browser.vb`, `Helpers/RichTextBoxLogger.vb`;
`src/KBot.App/` `Forexe/ForexeController.vb`, `ForexeController.Parallel.vb` (new), `ActualizareMultiplaForm(.Designer).vb`
(new), `KbotForm.Parallel.vb` (new), `KbotForm.vb`, `.ForexeWatch.vb`, `.Download.vb`, `.Ingest.vb`, `.Tree.vb`,
`.TreeOptions.vb`, `Setari/SetariAplicatieView(.Designer).vb`, `HarnessTests/FakeForexeRunner.vb`;
`tests/KBot.App.Tests/ForexeControllerFailureTests.vb` (interface member only).

## Test results
`dotnet build KBot.sln`: all `src/` projects build with **0 warnings, 0 errors**. No tests written or run (operator
rule), nothing run on screen, nothing run against FOREXE or the server. `tests/KBot.App.Tests` already did not
compile before this slice (`MainForm` constructor `capturiApi`, `JobRequest` indexer in `ForexeAnswerStoreTests`);
only the new interface member was added there.

## Unverified / deferred
- **The browser side is unproven**: that a second tab of the same context reuses the FOREXE login without a new
  certificate prompt (the operator observed several tabs work); that background tabs run the workflows correctly
  (Chromium flags against throttling are already set); that the docked window keeps showing the main tab while
  workers open (`BringToFront` after each open); that `AdoptTabAsync` leaves docking and the in-page menu working.
- Server: the DDL must be applied FIRST (000_DEMO, every unit database, AVACONT_SURSA), then `prelucrare.py` and
  `tree.py` deployed. Python tests not run (`test_forexe_tree` mocks may need the new column query).
- A DDF send or other flow that creates an angajament does not ask about «new angajamente» (only the list refresh does).
- The «Actualizează angajamente» window and the settings group were not seen on screen (designer files hand-written).
- Per-job history entries are gone in a parallel run (one entry for the run).
