# SLICE-0000-44 - help for 0107-02: «Clasificatii bugetare» totals

Operator rule (30.09.2026): any change the operator sees is recorded in the help with its slice.

## What changed and why

Text only, tag `0107-02` added to every section touched:

- `contabil.nomenclatoare.clasificatii`: the budget grid has a row Total and no footer total; the
  corrections footer sums the chosen classification's corrections; summaries and the empty state have
  no footer total.
- Tour `tur-clasificatii`: steps «Bugetul» and «Rectificari > Total».

## Files touched

`src/KBot.App/HelpContent/contabil/nomenclatoare/clasificatii.md`, `src/KBot.App/HelpContent/tours/tur-clasificatii.md`.

## Test results

`Check-Help.ps1 -Coverage`: no errors. No capture needs redoing: the captured `clasificatii` image shows a
leaf and now lacks the budget «Total» column - flagged below.

## Left unverified / deferred

- Capture `clasificatii` (leaf with budget and corrections) is stale: it shows no budget «Total» column.
