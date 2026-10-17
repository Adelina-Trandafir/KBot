# SLICE-0000-97 - Help for 00EF-41 (17.10.2026)

## What changed and why
`contabil/efactura/index.md`, the list of what the received-invoices view does: a new bullet after the one about the tree describes the list shown when «Toate facturile» is chosen (the ticks and the icon that ticks all, «Listeaza», «Salveaza XML», the «...» button of a row, what happens on a failure, how the bar of views comes back). The section tag carries `00EF-41, 0000-97`.

## Files touched
`src/KBot.App/HelpContent/contabil/efactura/index.md`.

## Test results
`tools\HelpCheck\Check-Help.ps1 -Coverage`: see the status row.

## Unverified / deferred
Nothing seen on screen. A capture of the list is not made (no `<!-- capture -->` line added); the existing captures of the received-invoices tree are unchanged.
