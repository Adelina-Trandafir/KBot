# SLICE-0000-108 - Help for KE13 (18.10.2026)

## What changed and why
`contabil/efactura/index.md`, «Pagina «Recepții»», zone «Facturi primite» (was «Facturi de recepționat»): all invoices are listed; the covered ones are ticked and locked (grey tick, dim text); right click «Desface asocierea cu recepția»;
the options icon of the header with «Arată facturile asociate»; the total in the footer; after a reception the covered invoices stay in the tree. Tag gains `KE13, 0000-108`.

## Files touched
`src/KBot.App/HelpContent/contabil/efactura/index.md`.

## Test results
`tools\HelpCheck\Check-Help.ps1` run: reports only older missing tags in other files (`tutorials\ordonantare-din-plata.md` and others), nothing about this topic.

## Unverified / deferred
Nothing seen on screen. The capture `efactura-receptii` needs a new take.
