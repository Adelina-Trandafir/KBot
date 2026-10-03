# SLICE-0109 - «Actualizează angajamente...» also in one-at-a-time (single thread) mode (operator request, 03.10.2026)

Operator: the extra row the header right icon of `KbotForm.tree` shows for multi-thread mode («Actualizează
angajamente...») must also be there in single thread mode; there the ticked angajamente are added to the robot queue,
exactly as when the individual refresh icon of several angajamente is pressed one after the other (slice 0098).
Also to be said in the help, in the K-BOT release notes file and in the guided tour.

## What changed and why

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

## Help, guided tour, release notes

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

## Files touched

`src/KBot.App/KbotForm.TreeOptions.vb`, `KbotForm.Parallel.vb`, `KbotForm.Download.vb`, `Forexe/ActualizareMultiplaForm.vb`,
`HelpContent/contabil/forexe/descarcare-multipla.md`, `coada.md`, `contabil/fereastra.md`, `HelpContent/tours/tur-fereastra.md`.

## Test results

`dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors. Nothing run, nothing seen on screen (operator rule: no
test runs / renders unless asked). `Check-Help.ps1 -Coverage`: the only errors left are the `0000-48` tags of the other
thread's help edits in `coada.md` / `descarcare-multipla.md` (their worklog and status row do not exist yet).

## Left unverified / deferred

- The queue behaviour (first code asked about receptii, the rest not; a duplicate refused) follows from reading
  `RobotQueue` / `MarcheazaDescarcareaFaraIntrebare`; it was not run. It relies on the `AsyncLocal` of the queue not leaking
  out of the first task's flow into the second `RunAsync` call of the same loop (it should not: async methods restore it).
- `NOUTATI.md` line to be added at the next release (above).
