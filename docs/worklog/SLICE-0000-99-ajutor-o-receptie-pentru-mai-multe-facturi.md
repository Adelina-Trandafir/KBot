# SLICE-0000-99 - Help for KE09 / KE10 (18.10.2026)

## What changed and why
`contabil/efactura/index.md`, section «Recepțiile unei facturi primite»: a paragraph for the button «Recepție nouă» of the list (several ticked invoices, one reception)
and one for the message when FOREXE refuses a reception (CAB-0031), the date written with dots and amounts with a comma. Section tag gains `KE09, KE10, 0000-99`.

## Files touched
`src/KBot.App/HelpContent/contabil/efactura/index.md`.

## Test results
`tools\HelpCheck\Check-Help.ps1`: only the old warnings of the tutorials (no `<!-- slice -->` under their sections), nothing from this section.

## Unverified / deferred
Nothing seen on screen; the capture `efactura-receptii` is unchanged.
