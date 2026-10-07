# SLICE-0000-57 - Ajutor pentru 00EF-09: arborele facturilor, meniul facturii, vederile cu document

Help sub-slice (CLAUDE.md: any change to what the operator sees goes into the help, with its slice number). 07.10.2026.

## What changed and why
- `contabil/efactura/index.md`:
  - «Lista facturilor» → **«Arborele facturilor»** (clients and invoices, the dot colours, the three-dots sign, the search magnifier);
  - new sections **«Meniul unei facturi»** (table: state → items), **«Trimiterea la ANAF»**, **«Stornarea unei facturi»**, **«Factură PDF, Factură ANAF și Eroare ANAF»**;
  - the five «Fila X» sections became **«Vederea X»** (the window has a bar of views, not tabs); «O factură nouă» and the Generale section state the date rule (new invoice or the last of the series, never before the previous invoice);
  - the sentence «trimiterea … nu se fac încă» is gone (it is done now);
  - slice tags `00EF-09, 0000-57` on every section touched; keywords widened (arbore, trimitere, validare, stornare, storno, factura clasica, pdf, eroare anaf).
- `help-version.txt` unchanged (`2026-10-07`, already that day).

## Files touched
`src/KBot.App/HelpContent/contabil/efactura/index.md`; status files.

## Test results
`tools\HelpCheck\Check-Help.ps1 -Coverage`: no error about `contabil/efactura/index.md` or about 0000-57; the only errors left are the old ones of `tutorials\ordonantare-din-plata.md` (not touched). The help was not opened in the app.

## Capturi
- **New, missing:** `efactura-meniu-factura` (the menu of an accepted invoice; `goto: menu:efactura`, prepare: hover an accepted invoice, press the three dots).
- **Changed:** `efactura-facturi` — the window is now the tree + the views bar; the picture never existed, only its prepare text changed.
- No capture for the PDF views (they need Adobe and an accepted invoice at ANAF).

## Unverified / deferred
- All wording comes from the code of `FacturiForm` (captions, menu items, messages), not from the window on screen: it was never run. Read the new sections against it. In particular the colour words (grey / orange / green / red come from the theme palette) and the sentence about «Factură ANAF» needing a token and a connection.
- The sections of 0000-56 (accounts) were not touched.
