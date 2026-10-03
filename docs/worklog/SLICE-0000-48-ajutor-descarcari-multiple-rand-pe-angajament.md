# SLICE-0000-48 — help for 0100-03: a row per running angajament in «Coada robotului»

Operator request, 03.10.2026 (the slice itself: `SLICE-0100-03-…`).

## What changed and why

- `contabil.forexe.coada`: new section «Mai multe angajamente deodată» (rows with code, progress bar and X; X
  stops only that download, nothing of it is saved and it is not counted as failed; a row that ends leaves and
  the next one comes; «Încă N în coadă.»; no rows for one angajament); the window now also opens by itself when
  a run of 2+ angajamente starts; «Oprește curenta» says it stops all, the X stops one.
- `contabil.forexe.descarcare-multipla`: new point 6 under «Cum decurge descărcarea» (what the window shows, the X).
- Slice tags `0100-03, 0000-48` on every touched section. Nothing about inner workings (tab closing, hooks).
- The guided tour `tur-coada` got no step: its steps point at controls that exist whenever the window is open, and
  the grid exists only during a run of 2+.

## Files touched

`src/KBot.App/HelpContent/contabil/forexe/coada.md`, `descarcare-multipla.md`.

## Test results

`tools\HelpCheck\Check-Help.ps1 -Coverage -Map`: run after the status row was added (see the worklog's end).

## Left unverified / deferred

- **New capture tag `coada-descarcari-multiple`** (operator suggestion) in `coada.md`, «Mai multe angajamente
  deodată». **The picture does not exist yet** (neither does `coada-robot`: `img/` has no file for the queue) —
  take it from «Meniu › Capturi pentru ajutor»: 4+ angajamente ticked in «Actualizează angajamente», 2 tabs, shoot
  while both rows run and «Încă N în coadă.» shows. Needs a real multi-download.
- Not seen in the help window. Number 0000-48 taken while another session was also working in the help
  (0000-47 existed); if it collides, renumber this one.
