# SLICE-0125-02 — Simulated FOREXE: node download, receptions refresh, reservation save with pictures (Debug only)

**Date:** 05.11.2026. Operator's request: update the simulated FOREXE (0125-01) so it also covers three roads of the
shell that were not covered: **the download of a node**, **the receptions refresh**, and **saving a reservation with
pictures** (the ingest was already covered by the simulated answers, but `CaptureFlow` was not).

## What changed and why

All in `SimulatedForexeRunner` (`#If DEBUG`) plus one new Debug control. Nothing in Release.

- **Node download** — answers `PrelucrareCompleta` and `PrelucrareCompletaReverse` from memory in the shape of
  `PYTHON/tests/fixtures/prelucrare_AAB37CNBK95.json`: the six header values, `TabelIndicatori` (scraped) and
  `TabelIndicatori_results` (collected), `ListaReceptii` (list) and `ListaReceptii_results` (detail nested), `TabelIstoric`.
  The REVERSE variant returns the history from `DATA_IESIRE` on (boundary row included; the import skips what it has).
  `InfoCompleteContract` is not answered (the server then leaves the initial credit unread, as for a page that was not opened).
- **Receptions refresh** — answers `ReceptiiAngajament` (header + the two reception tables). The days the operator left
  unticked (`RECEPTII_SARITE`) come back with an EMPTY detail, as the real flow leaves them; the controller cuts them.
- **Receptions are made up, once.** FOREXE has none for an angajament the simulator created, so the first time an angajament in
  the state «În derulare» is read, ONE sample reception is added to memory (description «SIMULARE - receptie de proba»,
  half of each row's definitive value, today's date). Before «În derulare» there is none.
- **Reservation save with pictures.** The simulated runner now docks a small made-up page (`SimulatedForexePage`, Designer
  control) into the «Browser FOREXE» view: header with the open angajament, the indicator rows, and «Salvează rezervarea»
  (indicator, new value, motive). The save follows the real page's order: at the start of a session the gate
  (`verifica-rezervare`, a server refusal stops it) and the «before» picture through the provider the controller registered
  (`SetCapturaProvider`); at every save the marker `(IDREV: n)` is asked from the server (the same `Func` the real runner
  gets, now passed to the simulated runner's constructor in `Program.vb`), appended to the motive as the history row's
  description, the row's definitive value changes, and the save is reported as the in-page watcher would
  (`OperationCaptured`: `Finished` + `Rezervare` + `{indicator, indicatorCod, buget, motiv}`). The shell then runs its own road:
  «Ați terminat?» → «after» picture → `RezervariEditate` → ingest → `CaptureFlow.SendAsync` → `IncheieSesiuneaDeCapturiAsync`
  (which resets the session through `ResetShotSessionAsync`).
- Also simulated: `DeschideAngajament` (a node picked while the Browser view is open: the page shows it, the «page» report
  follows), `CapturePaginaAsync` / `CaptureInfoCompleteAsync` (generated JPEGs marked «SIMULARE»), `ReadPageAngajamentAsync`.
- **Unknown angajament.** A code the simulator never created or received in this run is refused (node download, receptions,
  open, edited reservations) with a plain message: it has no rows to answer with, and inventing them would write into the database.

## Files touched

- `src/KBot.App/HarnessTests/SimulatedForexeRunner.vb` — workflows, receptions memory, page/capture members, constructor.
- `src/KBot.App/HarnessTests/SimulatedForexePage.vb` and `.Designer.vb` — new (Debug).
- `src/KBot.App/Program.vb` — the simulated runner gets the marker provider.

No `KBot.Controls`/`KBot.Forexe` change, so no `FileVersion` bump. Help: none (Debug only).

## Test results

`dotnet build src\KBot.App -c Debug` (output to a scratch folder): **0 errors, 0 warnings**. Nothing run on screen, no test written or run
(operator's rule).

## Left unverified or deferred

- **Nothing was run.** First things to watch: open a simulated angajament in the Browser view and save a reservation (marker
  `(IDREV: n)` in the history description, «before» and «after» pictures, upload `PUT /api/forexe/capturi/rezervare/<n>`);
  node download of a `SIM…` angajament «În derulare»; receptions refresh with a day unticked.
- **Writes real rows.** The real import writes the simulated figures, the sample reception and the saved reservation: **000_DEMO only.**
- The server's read of the nested reception detail and of the REVERSE boundary row was matched against the fixture and the code by reading, not run.
- Not simulated: saving a reception or a new angajament by hand in the page, `ListaAngajamente`, extracts, parallel downloads.
- The page is not meant for the Visual Studio designer (its constructor takes the runner).
