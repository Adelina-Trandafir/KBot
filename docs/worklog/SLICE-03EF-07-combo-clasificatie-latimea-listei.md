# SLICE-03EF-07 - «Contari»: the classification combo is exactly as wide as «Documente evidentiere» (08.11.2026)

## What changed and why
Operator (08.11.2026, screenshot): the classification combo was 5 px wider than the grid «Documente evidențiere» below it (344 vs 339 at the designer's dpi).
- `tlySus` first column 350 → 345 (the same as the first column of `tlyEvid` / `tlyPlata`); the combo keeps its 6 px right margin (as the list below), so both end on the same x. Combo size set to 339 in the designer.
- The three boxes to its right move 5 px to the left.

## Files touched
`FacturiCumparareContarePage.Designer.vb`.

## Test results
Nothing run, nothing seen on screen: the build is clean (KBot.App, Debug, 0 warnings, 0 errors), no test was run (rule), the designer was never opened in Visual Studio.

## Unverified / deferred
Layout fix (paddings / proportions): designer + chat only, not recorded in the help.
