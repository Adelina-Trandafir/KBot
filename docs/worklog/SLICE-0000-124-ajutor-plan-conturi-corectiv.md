# SLICE-0000-124 - Help: «Plan de conturi», corrective (25.10.2026, slice AV01-06)

## What changed and why
`contabil/nomenclatoare/plan-conturi.md`: the subunit now follows the source of the title bar (with a «Subunitate» list beside it when several subunits share the source),
the subunit row and its buttons are gone from the text, «Afiseaza solduri initiale» is described as a ribbon button (right group). Tag `AV01-06` on the sections it touches.

Second pass: the inactive-accounts choice is in the popup of the tree header, «Exporta in Excel» is a ribbon button, «Cont extra» / «Cont activ» are gone from the text.

## Files touched
`KBot.App/HelpContent/contabil/nomenclatoare/plan-conturi.md`, `help-version.txt`.

## Test results
`Check-Help.ps1` not run. Nothing seen on screen.

## Unverified / deferred
No capture. The topic no longer says how a subunit is added / changed / deleted (no button for it now, see SLICE-AV01-06).
