# SLICE-0114-28 - Rezervari grid shows the indicator code after the classification (04.11.2026)

Slice choice (operator, 04.11.2026): the slice of the last change to RezervariView is 0114-26 (commit 255573c4, 23.10.2026); sub-slice = first free one, 0114-28 (0114-27 was taken).

## What changed and why
Operator request: in the Rezervari view grid, show the indicator code right after the classification.
- `RezervariView.Designer.vb`: new read-only column `cod_indicator` («Indicator», width 80, centred, bold, filterable) added to `grid.Columns` between `clsf` and `credit_bug`. `FrozenColumnCount` stays 1 (the classification).
- `RezervariView.vb`: new `COL_INDICATOR`; `FillGrid` (row by row, a tree node is selected) fills it from `RezervareRow.CodIndicator`; `FillGridAgregat` (nothing selected / root) now groups by (Clsf, CodIndicator) instead of Clsf only, ordered by Clsf then indicator.
- Why the grouping key changed: `Credit bugetar` belongs to the indicator. With the classification alone as key, two indicators on one classification would have shown one code and merged their credits into one row. With both in the key the code shown is always the right one. When every classification has one indicator the rows are the same as before.
- No server or API change: `/api/forexe/rezervari` already returns `cod_indicator` and `ApiClient` already maps it to `RezervareRow.CodIndicator`.
- Help: `contabil/vederi/rezervari.md` (Grila bullet), tag `0114-28`.

## Files touched
`src/KBot.App/Views/RezervariView.vb`, `src/KBot.App/Views/RezervariView.Designer.vb`, `src/KBot.App/HelpContent/contabil/vederi/rezervari.md`.

## Test results
`dotnet build src/KBot.App/KBot.App.vbproj`: 0 warnings, 0 errors. No tests run (operator rule).

## Unverified / deferred
- Not seen on screen: column width (80) and its position next to the frozen column are by analogy with SumarView's «Indicator» column.
- The help capture `rezervari` was not redone (the grid has one more column); `Check-Help.ps1` not run.
- Not committed (operator rule: no git writes).
