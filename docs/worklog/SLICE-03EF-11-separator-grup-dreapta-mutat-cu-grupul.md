# SLICE-03EF-11 - Ribbon: the separator of the right group travels with the group (08.11.2026)

## What changed and why
Operator (screenshot): a stray vertical line appeared over the buttons of the left («Cumpărare» / «Vânzare») since 03EF-02/03. Cause, in the control: `KBotRibbon.PlaceGroup` places the form group and the right group at x = 0 and `ShiftGroup` then moves them to the right edge; it shifted the buttons, the fields and the caption but NOT `SeparatorX` (the line after a large button, `SeparatorAfter`, slice KE12), which stayed at its local x near the left edge. Groups of the tabs are placed at their real x, so KE12 never showed it. `ShiftGroup` now moves `SeparatorX` too.

## Files touched
`KBot.Controls/Ribbon/KBotRibbon.vb`.

## Test results
The build of the projects is clean (KBot.Controls, KBot.EFactura: 0 warnings, 0 errors; KBot.App could not be copied into its output while K-BOT / Visual Studio had it open, but it reports no compile error). No test run, nothing seen on screen.

## Unverified / deferred
Not seen on screen after the fix.
