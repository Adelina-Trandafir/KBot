# SLICELESS — list without source filter, name in the association window, Wicket monitor after multi-thread (operator, 02.10.2026)

Branch: `SLICE-0100-Multithreading` (nothing committed). No slice number given; recorded in
`state/KBOT_STATUS_SLICELESS.md`. Help recorded as `0000-39`.

## What changed and why

1. **Association window shows the angajament name.** `AsociereForm` (both constructors) takes an optional
   `denumire`; the title is «… · COD — denumire» (`TitluAngajament`, `Friend Shared`, pure text). The two callers
   (`KbotForm.Receptii.vb`, `KbotForm.Ingest.vb`) pass `DenumireAngajament(cod)` = `AngajamentTreeInfo.Descriere` from
   `_treeInfos`; an angajament not in the tree yet shows the code alone, as before.
2. **List refresh with the tree sorted by date downloads every source of the unit.** The source selector is already
   hidden in that mode and the tree already asks the server for all sources (`ApiClient.TreeAllSources`); the robot
   still filtered by `session.SectorSursa`. `JobBuilder.BuildListaAngajamente(session, toateSursele)` now sends `SURSA`
   EMPTY (never unset: an unset variable stays as the literal `{{SURSA}}` in the XML and `IfVar` would try to select it);
   `ForexeController.DownloadListaAsync` passes `AppSettings.Current.TreeSortIsDate`. With the tree sorted by name nothing
   changes (the chosen source still filters). The program filter (`COD_PROGRAM`) is untouched.
3. **«Function "_wicketMonitorCallback" has been already registered» with multi-thread on.** Cause (read in code):
   at the end of a multi-thread run `AdoptTabAsync` makes the primary executor take over the tab of the last good worker.
   That worker had already exposed `_wicketMonitorCallback`, `_clickMonitorCallback`, `_keyMonitorCallback` in the page
   (it ran its own `WaitForWicketIdleAsync`), and `ExposeFunctionAsync` cannot be undone. `AdoptTabAsync` only reset
   `_wicketMonitoringActive`, so the first wait on the primary tried to register them again -> the error, repeated on every
   wait; the monitor stayed off, the idle waits never ended on a real «idle», and the page-by-page scrape clicked
   `a[rel='next']` while Wicket was re-rendering (`element was detached from the DOM` -> timeout on page 260).
   Fix (`WorkflowExecutor.WicketMonitor.vb`, `.ClickMonitor.vb`, `.Core.vb`): the exposed delegates now go through
   `MonitorTarget` (`_monitorForward` or `Me`); `HandleWicketPayload` / `RaiseWicketEntry` hold the logic;
   `TakeOverWicketMonitoring(worker)` (called by `AdoptTabAsync`) stops the primary's old monitor, gives the primary its own
   idle timer and the last page state, points the worker at the primary and stops the worker's own monitor. A worker that
   never started the monitors leaves the primary inactive, so the next wait installs them on the clean page. The idle
   timer is now created before the function is exposed (a callback can arrive at once) and a callback with no timer is ignored.

## Files touched

`src/KBot.Forexe/Executor/WorkflowExecutor.WicketMonitor.vb`, `.ClickMonitor.vb`, `.Core.vb`; `src/KBot.Forexe/JobBuilder.vb`;
`src/KBot.App/Forexe/ForexeController.vb`, `Forexe/AsociereForm.vb`; `src/KBot.App/KbotForm.Receptii.vb`, `KbotForm.Ingest.vb`;
help `HelpContent/contabil/forexe/lista.md`, `contabil/asocieri/index.md`.

## Test results

No tests written or run (operator rule). `dotnet build src/KBot.App` -> 0 warnings, 0 errors. The app was not run.

## Unverified / deferred

- Item 3 is a diagnosis from the code and the pasted log, not from a run: the log does not show the multi-thread download
  that ran before it. The fit is strong (3 «already registered» errors, then the next-page timeout), but confirm on a run:
  multi-thread download, then «Lista de angajamente» in the same session.
- Whether the page-260 timeout also needs a retry/wait change in `ScrapeTable` is open: it should stop once idle detection works.
- The name sits in the caption bar; a very long denumire is cut by the bar. The date-sorted list refresh was not run against
  FOREXE (the `Select` on `sursa` is simply skipped with an empty value).
- Help: `help-version.txt` / `Check-Help.ps1` not run.
