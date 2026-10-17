# SLICE-0000-89 - Help for 0114-05: the statements are downloaded from the ribbon only (17.10.2026)

## What changed and why
`contabil/vederi/extrase.md` (section «Descarcarea extraselor» rewritten; the opening of the window only via the ribbon),
`contabil/ribbon.md` (the button appears only on the Extrase window / the list of angajamente), `contabil/fereastra.md`
(the left footer icon line replaced), `tours/tur-extrase.md` and `tours/tur-fereastra.md` (the tree-footer steps removed),
`tutorials/descarca-extrase.md` (first step is now a message about the ribbon button, no target: painted ribbon buttons cannot be ringed).
Tags `0114-05, 0000-89`.

**Captures marked `redo` (2026-10-17 18:00, why 0114-05)**: `extrase-fereastra`, `lista-actualizare`, `fereastra-zone`,
`fereastra-principala` and every capture with `goto: view:*` (the whole window shows the list footer) - 36 in all.

## Files touched
The six help files above plus the capture lines in `contabil/**`.

## Test results
`Check-Help.ps1`: no error from these files once this worklog and the status row exist; the errors left are old (efactura tutorial
controls, `ordonantare-din-plata.md` tags).

## Unverified / deferred
Nothing seen on screen; the tutorial's new step is untested in the runner. Captures are to be redone by the operator.
