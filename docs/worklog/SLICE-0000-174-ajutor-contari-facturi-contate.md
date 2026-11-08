# SLICE-0000-174 - Help: booked invoices in «Contari», default tree, CPV / unit tooltips (08.11.2026)

## What changed and why
Help for 03EF-13 in `contabil/efactura/index.md`: the paragraph «Facturi deja contate» in «Contari»; the first-opening state of the tree (by supplier, only invoices without operations) in «Facturi primite»; the tooltips of CPV and Um in «Continut» (and the sentence that said the tables have no tooltips). Tags `03EF-13`, `0000-174`.

## Files touched
`src/KBot.App/HelpContent/contabil/efactura/index.md`.

## Test results
`Check-Help.ps1` not re-run. Not seen on screen.

## Unverified / deferred
Capture `efactura-cumparare` still shows an old tree (already listed by 0000-173).
