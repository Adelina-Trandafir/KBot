# SLICE-0000-131 - Help: the «Clasificatii» window with the pages «Cheltuieli» / «Venituri» (26.10.2026, slice AV01-11)

## What it is
- `contabil/nomenclatoare/clasificatii.md`: the window has a vertical bar with two pages, «Cheltuieli» (the default) and «Venituri». Section «Venituri» (tree Capitol > Subcapitol > Paragraf, the budget of the unit named by the title bar's source +
  year, «Toate» leaves the tables empty, the same grids and rules as «Cheltuieli», only «Salveaza», child subunits work on the parent's data and the footer says so) and «Clasificatie de venituri noua». `screens:` lists the views and the
  add form; tag `AV01-11`.
- `tours/tur-clasificatii.md`: first step on the bar (`ClasificatiiForm.navClasificatii`, part `item:venituri`); the other steps target `ClasificatiiBugetView.<control>`. The three `tutorials/clasificatii-*.md` target the same way.
- `contabil/nomenclatoare/index.md`: the line of the window says it holds the revenue page too.

## Files touched
`KBot.App/HelpContent/contabil/nomenclatoare/{clasificatii,index}.md`, `tours/tur-clasificatii.md`, `tutorials/clasificatii-{adaugare,buget,rectificare}.md`.

## Test results
`Check-Help.ps1 -Coverage`: nothing new about these files beyond the older «slice not in KBOT_STATUS.md» ones.

## Unverified / deferred
Nothing seen on screen. The capture `clasificatii` is marked `redo: 2026-10-26 17:10`; no capture of the «Venituri» page.
