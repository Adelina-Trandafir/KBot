# SLICE-0000-101 - Help for KE11 (18.10.2026)

## What changed and why
`contabil/efactura/index.md`, section «Recepțiile unei facturi primite»: the subsection «Clasificațiile liniilor» became **«Fereastra recepției»** and describes the four zones of the
rebuilt window (tree of invoices with ticks, «Produse Factura …» with «Recepționată» and the classification, «Angajamente» with the empty row and «Elimină asociere», «Date recepție …» with
the amounts by classification); the three bullets of «Recepția» follow the new window; the paragraph «Mai multe facturi pe o singură recepție» says the invoices are ticked in the window
(no more one window per invoice). «Ca la prima linie» and «Alt angajament…» are gone from the text. Section tag gains `KE11, 0000-101`.

## Files touched
`src/KBot.App/HelpContent/contabil/efactura/index.md`.

## Test results
`tools\HelpCheck\Check-Help.ps1`: nothing left about this topic except the old tutorial warnings.

## Unverified / deferred
Nothing seen on screen; the capture `efactura-receptii` (the view, not the window) is unchanged; no capture of the window was made.
