# SLICE-0000-134 - Help for the «Sabloane» window (slice AV01-14, 27.10.2026)

## What changed and why
New topic `contabil/nomenclatoare/sabloane.md` (`contabil.nomenclatoare.sabloane`, `screens: SabloaneForm, SablonOperatiiView`, label `AV01-14`): the window «Sabloane» (ribbon Date > Sabloane),
the vertical bar («Operatii cu clasificatie» / «Operatii diverse»), the tree and the right-hand editor, the partner choices, the pager over the lines, saving and deleting.
`nomenclatoare/index.md` lists it (label `AV01-14`). Nothing about the former system or the migration is in the help (operator rule).

## Files touched
`HelpContent/contabil/nomenclatoare/sabloane.md` (new), `HelpContent/contabil/nomenclatoare/index.md`, `HelpContent/help-version.txt`.

## Test results
`Check-Help.ps1 -Coverage`: no message about the new topic (the errors it prints are the ones that already existed in the tutorials). No picture, no run.

## Unverified / deferred
- Written from the Designer text and the code, nothing seen on screen. Capture to take later: the window with the tree and the editor (no capture tag yet).
- The salary templates are not in the help (they do not exist yet).
