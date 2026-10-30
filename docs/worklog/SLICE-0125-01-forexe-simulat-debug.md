# SLICE-0125-01 — A simulated FOREXE for testing the DDF signatures (Debug only)

**Date:** 30.10.2026. Operator's request: a test "server", reachable only from the Debug side, that simulates sending a new
reservation to FOREXE and receiving the answer, so the signature system (A, B, director) can be tried without touching forexecab.
Slice number given by the operator (0125, the next free one). Interpretation confirmed by the operator: a FOREXE simulated **inside the
application** (not an HTTP site the real robot would drive).

## What changed and why

`ForexeController` talks to the robot only through `IForexeRunner`. The new `SimulatedForexeRunner` implements it with no browser:

- **Memory.** It keeps the angajamente it has "created" or "modified" (rows with code, source, classification, initial and definitive
  value, and a history with the moment of every change). What K-BOT sends is what the next read-back shows, so the check of section A against
  forexecab's table (`DdfSendInputs.Differences`) passes by itself.
- **Workflows answered:** `CreareAngajament` (real-looking code `SIM…` in `CodAng_Final`, `CodAng_<Cheie>`, the table, one capture per row),
  `IncarcaRezervare` (set or add rows; `Poza_Inainte`, `Poza_<Cheie>`, `Poza_Final`, `Poza_InfoComplete` when asked; `CodIndicator_<Cheie>` for
  new rows), `DefinitivareAngajament`, `DerulareAngajament`, and the read-back `RezervariAngajament`. Any other workflow is refused with a
  message. The dry run (`StopBeforeSave`) is honoured. A cancel is honoured.
- **The read-back has the shape of the real one** (checked against `PYTHON/tests/fixtures/prelucrare_AAB37CNBK95.json` and the server's import
  steps in `prelucrare.py` / `prelucrare_pasi.py` / `prelucrare_helpers.py`): the six header values, `TabelIndicatori` (as scraped, with the
  initial-credit column K-BOT checks), `TabelIndicatori_results` (as collected), `TabelIstoric` with `Rand contract: …` texts and the
  reason of every change as the row's description, which carries `(REV:n)`.
- **Why the import must run (and is not skipped).** For a DDF of the new flow (`DeSemnat` set) section B is written, and the option to sign B
  appears, only when `FX_Rezervari` holds a row for the revision (`DdfSectionBInsert.vb`: `InFluxulKbot AndAlso Not AreRezervari`, 0124-07).
  Those rows are written by the import from the history. So the simulated answer goes through the real import on the server.
- **Switch and entry.** Debug start window: new entry «Aplicația cu FOREXE simulat» (`StartupLauncherForm.KEY_APLICATIE_SIMULAT`) → sets
  `SimulatedForexeSwitch.Active` → `Program.ConfigureServices` (Debug only) registers `SimulatedForexeRunner` instead of `ForexeRunner` for
  `IForexeRunner`. The runner file and the registration are inside `#If DEBUG`; Release registers `ForexeRunner` exactly as before.
- **`ForexeController.ForexeSimulat`** (always False in Release) lets the send question add a line (`ModeNote`) telling the operator that FOREXE
  is simulated, that the import still writes to the database, and to use 000_DEMO only.
- `KbotForm.vb`: `DirectCast(_forexeRunner, ForexeRunner).AttachLogger` became a `TryCast` (the simulated runner has no logger).
- The captures are generated JPEGs of a made-up FOREXE page with a red band «SIMULARE — aceasta nu este o pagină FOREXE reală», so a
  simulated capture cannot be mistaken for a real one inside the signed PDF.

## Files touched

- `src/KBot.App/HarnessTests/SimulatedForexeRunner.vb` — new (`SimulatedForexeSwitch`, `SimulatedForexeRunner`), all in `#If DEBUG`.
- `src/KBot.App/Program.vb` — runner registration (Debug: switch) and the launcher case.
- `src/KBot.App/StartupLauncherForm.vb` — `KEY_APLICATIE_SIMULAT` + its list row.
- `src/KBot.App/Forexe/ForexeController.vb` — `ForexeSimulat`.
- `src/KBot.App/KbotForm.vb` — `TryCast` for `AttachLogger`.
- `src/KBot.App/KbotForm.DdfSend.vb` — `ModeNote` line for the simulation.

No `KBot.Controls`/`KBot.Forexe` change, so no `FileVersion` bump. Help: none (Debug only, nothing the operator of a Release build sees).

## Test results

`dotnet build src\KBot.App -c Debug` (output redirected to a scratch folder, the app was running and locks `bin\Debug`): **0 errors, 0 warnings**.
`-c Release`: 4 errors, all in `HarnessTests/DdfFluxServerHarnessForm.vb` (`PdfSignatureReport`, `RecordingPdfApi` not declared) — **not from this
slice**: that file (0124-01) has no `#If DEBUG` while the files it uses do. Release was not buildable before this change either; not fixed here.
Nothing run on screen, no test written or run (operator's rule).

## Left unverified / deferred

- **Nothing was run.** The import of a simulated package has been matched against the server code by reading, not against the server. First
  run to watch: that «Trimite în FOREXE» of a K-BOT-made revision reaches «PDF final / Semnat A», that `FX_Rezervari` gets a row for the revision
  (`AreRezervari`), and that section B then appears.
- **Writes real rows.** The import writes `FX_Angajamente`, `FX_Indicatori`, `FX_Istoric`, `FX_Rezervari` and `FX_Indicatori_Buget` with the
  simulated figures. The simulator does not know a classification's real budget, so it says «largest value + 100.000»; on a real database that would
  overwrite a classification's budget. **Use 000_DEMO only.** For a new angajament, `SalveazaCoduri` also writes the simulated code `SIM…` over the `!` code.
- **An angajament the simulator never saw** (a real one downloaded earlier) is created in its memory with only the rows sent; the read-back then
  carries only those rows and this run's changes. Its earlier history is not repeated (the import would skip it anyway).
- The memory is lost when K-BOT closes: a «Reia trimiterea» after a restart finds an empty simulator (a resume of a creation asks the operator, as for
  an empty FOREXE page).
- No failure injection (a send that stops half-way) — to test «Trimitere întreruptă» a switch would be needed.
- For a new angajament, «Definitivează» → «Derulează» change the simulated state and write their history, but whether the Rezervări menu then
  offers the next step depends on the import of that history (same code path as the real one); not seen.
- Not simulated: `ListaAngajamente`, `PrelucrareCompleta`, receptions, extracts, parallel downloads, the browser view (these answer «nu este simulat» or nothing).
