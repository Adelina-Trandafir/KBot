# SLICE-0000-46 - help for 0107: «Clasificatii bugetare» widths and final row

Operator rule (30.09.2026): any change the operator sees is recorded in the help with its slice.

## What changed and why

Text only, tag `0107` added to every section touched:

- `contabil.nomenclatoare.clasificatii`: «Clsf» first on the budget grid, equal-width columns, the new row
  under both grids (last budget + all corrections, also for a node).
- Tour `tur-clasificatii`: step «Bugetul» mentions «Clsf» and the widths; new step «Bugetul in vigoare»
  (target `ClasificatiiForm.gridTotal`).

## Files touched

`src/KBot.App/HelpContent/contabil/nomenclatoare/clasificatii.md`, `src/KBot.App/HelpContent/tours/tur-clasificatii.md`.

## Test results

`Check-Help.ps1 -Coverage`: no errors.

## Left unverified / deferred

- Capture `clasificatii` is stale (no «Clsf» column, no last row).
- The tour step `gridTotal` not seen running.
