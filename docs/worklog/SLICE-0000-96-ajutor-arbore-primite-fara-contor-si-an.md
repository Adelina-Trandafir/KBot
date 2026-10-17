# SLICE-0000-96 - Help for 00EF-40 (17.10.2026)

## What changed and why
`contabil/efactura/index.md`, the bullet about the tree on the left: the root «Toate facturile» has no count of invoices (only the total), the months show only the month's name, and the year appears only when the list has invoices of more than one year. Section tag: `00EF-40, 0000-96`.

## Files touched
`src/KBot.App/HelpContent/contabil/efactura/index.md`.

## Test results
`tools\HelpCheck\Check-Help.ps1 -Coverage`: see the status row.

## Unverified / deferred
Nothing seen on screen. The captures of the received-invoices tree (`efactura-cumparare` and any other showing «Toate facturile (n)» or «Luna AAAA») may need a redo.
