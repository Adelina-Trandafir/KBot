# SLICE-0000-103 - Help for KE12-02 (18.10.2026)

## What changed and why
`contabil/efactura/index.md`, subsection «Pagina «Recepții»», zone «Facturi de recepționat»: the supplier is only a heading without a tick, the tree starts closed, the lens searches by supplier name or invoice number
(at least 3 characters, Esc clears), invoices newest first under each supplier, and the invoices covered in full by one or several receptions, or with the value 0, are not listed. Tag gains `KE12-02, 0000-103`.

## Files touched
`src/KBot.App/HelpContent/contabil/efactura/index.md`.

## Test results
`tools\HelpCheck\Check-Help.ps1`: nothing about this topic beyond the old tutorial warnings (to be re-run).

## Unverified / deferred
Nothing seen on screen.
