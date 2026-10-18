# SLICE-0000-104 - Help for KE12-03 (18.10.2026)

## What changed and why
`contabil/efactura/index.md`, zone «Produse Factura …»: every line has a tick box in the first column and the invoice row has one too (it ticks / clears all its lines); a cleared line puts 0 in «Recepționată» and locks it,
ticking it again gives back the sum of «Valoare cu TVA». Tag gains `KE12-03, 0000-104`.

## Files touched
`src/KBot.App/HelpContent/contabil/efactura/index.md`.

## Test results
`tools\HelpCheck\Check-Help.ps1` run: nothing about this topic.

## Unverified / deferred
Nothing seen on screen.
