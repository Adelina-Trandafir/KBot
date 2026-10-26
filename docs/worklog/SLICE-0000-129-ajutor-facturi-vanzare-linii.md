# SLICE-0000-129 - Help: sales invoice lines (spare row, copy, no «Linie noua») (26.10.2026, slice 00EF-50)

## What changed and why
- `contabil/efactura/index.md`, section «Vederea Continut»: «Linie noua» replaced by the empty row at the end of the table; «❐» copies a line; tags `00EF-50, 0000-129`.
- `tutorials/efactura-factura-noua.md`: the step «Linie noua» (target `FacturiForm.btnLinieNoua`, which no longer exists) removed; «Completeaza linia» describes the empty row, «❐» and «✕».

## Files touched
`KBot.App/HelpContent/contabil/efactura/index.md`, `tutorials/efactura-factura-noua.md`.

## Test results
`Check-Help.ps1` not run. Nothing seen on screen.

## Unverified / deferred
No capture. The ribbon buttons now show text next to the icon; no help text named them by their look.
