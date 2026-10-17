# SLICE-0000-91 - Help for 0114-15 .. 0114-19 (17.10.2026)

## What changed and why
`contabil/vederi/rezervari.md`, `contabil/vederi/receptii.md`, `contabil/ribbon.md` (the view group now has Sumar / Rezervari / Receptii buttons; two rows in the Before / Now table), `contabil/ddf/rezervare.md`, `contabil/asocieri/index.md` and `fereastra.md` (capture note), `contabil/efactura/index.md` (no month list, rows not cut; the view lists only the angajament's invoices, filter; footer «Actualizeaza»; the ribbon fields look like the popup), tours `tur-rezervari`, `tur-receptii`, `tur-ddf`, `tur-asocieri` (the steps of the removed tree icons became target-less «Ribbon ›» steps), tutorial `legaturile-receptiilor`. Captures marked for redo: 10.

## Files touched
The help files above under `src/KBot.App/HelpContent/`.

## Test results
`tools\HelpCheck\Check-Help.ps1`: no new error from these files; the old ones (tur-setari, efactura-factura-noua controls, ordonantare-din-plata tags) are not from this slice.

## Unverified / deferred
Nothing seen on screen. The target-less steps of the tours were not tried in the runner. Captures to be redone by the operator.
