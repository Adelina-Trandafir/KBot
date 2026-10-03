# SLICE-0000-45 - help for 0108: credit per classification, budget without quarters

Operator rule (30.09.2026): any change the operator sees is recorded in the help with its slice.

## What changed and why

Text only, tag `0108` added to every section touched:

- `contabil.nomenclatoare.clasificatii`: «Cum se folosește bugetul în documentul de fundamentare» (the total
  of the version + corrections, no quarter), «Verificarea bugetului față de FOREXE» (the two columns, one credit
  per classification).
- `contabil.ddf.editor` (the «Buget» of the lines): total, no quarter.
- `contabil.vederi.sumar`: the credit bugetar is the classification's.
- Nothing about the Migrator or the Access system (rule: no Access in help).

## Files touched

`src/KBot.App/HelpContent/contabil/nomenclatoare/clasificatii.md`, `.../ddf/editor.md`, `.../vederi/sumar.md`.

## Test results

`Check-Help.ps1 -Coverage` run after the status rows were added (see the 0108 worklog for the code).

## Left unverified / deferred

No capture needs redoing for this change except possibly `sumar` if the credit column was framed (not checked).
