# SLICE-03EF-10 - «Salvează» only when every cell of the grids is filled (08.11.2026)

## What changed and why
Operator: «Salvează» must check that the data is complete: all the cells of the grids that hold data are filled, and that is the condition.
- `FacturiCumparareContarePage.UpdateSave`: besides the old conditions (role, classification, evidence template, no blocking warning, something left to book, a valid explanation when paying) the button needs `GridFilled` for `gridEvidenta` and `gridPlata`: no cell of any row is empty (`Nothing`, `DBNull`, blank text); a number, even 0, counts as filled; the progress column «Lungime» is skipped.
- The state is re-evaluated when the note (`nc`) is shared, when the order number (`nrop`) is committed, and when the explanation is typed.
- **Consequence:** the column «NC» (note) and, with a payment template, «Număr OP» must now be typed — an empty note no longer means «the next number». When the only thing missing is cells, the tooltip of «Salvează» says so first.

## Files touched
`KBot.EFactura/Views/FacturiCumparare/FacturiCumparareContarePage.vb`. Help 0000-172.

## Test results
The build of the projects is clean (KBot.Controls, KBot.EFactura: 0 warnings, 0 errors; KBot.App could not be copied into its output while K-BOT / Visual Studio had it open, but it reports no compile error). No test run, nothing seen on screen.

## Unverified / deferred
- A document without a date (`DataDocument` empty) also blocks the save (its «Data» cell is empty).
- Whether to fill «NC» with the next note number by default (it would be shown, but another user could take the number meanwhile) is left to the operator.
