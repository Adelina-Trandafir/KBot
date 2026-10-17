# SLICE-0000-98 - Help for 00EF-42 (17.10.2026)

## What changed and why
`contabil/efactura/index.md`, the bullet about the list shown when «Toate facturile» is chosen: the grid has the column «Recepție asociată» (date, description and value of the linked reception in one cell, several separated by «|», empty when not linked) instead of the supplier. Section tag: `00EF-42, 0000-98`.

## Files touched
`src/KBot.App/HelpContent/contabil/efactura/index.md`.

## Test results
`tools\HelpCheck\Check-Help.ps1 -Coverage`: see the status row.

## Unverified / deferred
Nothing seen on screen.
