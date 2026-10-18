# SLICE-0000-102 - Help for KE12 (18.10.2026)

## What changed and why
`contabil/efactura/index.md`, section «Recepțiile unei facturi primite», rewritten: the view «Recepții» only shows the state (no buttons now, so no «Scoate legătura» either); linking is
described as the page **«Recepții»** of the ribbon (tab E-Factura, after «Cumpărare», a separator before «Vânzare»): its four zones, the combo that chooses new / existing / modified,
the button of the footer, the three kinds of reception, several invoices in one reception (ticked in the tree), the refusal message of FOREXE. The mentions of the modal window and of the list's «Recepție nouă» button are gone.
Section tags gain `KE12, 0000-102`.

## Files touched
`src/KBot.App/HelpContent/contabil/efactura/index.md`.

## Test results
`tools\HelpCheck\Check-Help.ps1`: nothing left about this topic except the old tutorial warnings.

## Unverified / deferred
Nothing seen on screen; no capture of the page was made (the capture `efactura-receptii` shows the view).
