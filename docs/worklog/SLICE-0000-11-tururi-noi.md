# SLICE 0000-11 — five more guided tours

**Date:** 30.09.2026. **Standing slice:** 0000 (help). Operator chose «More guided tours».

## What changed and why
New files in `src/KBot.App/HelpContent/tours/` (11 tours in all now):
- `tur-ord.md` (5 steps) — Plăți «+» → OrdView tree → right-click menu / footer «Adaugă» →
  pages → signing order (validate + sign 1, validate + sign 2, CFP, ordonator; complete = 1, 2, 5).
- `tur-receptii.md` (4) — the three-level tree, the hover label (recepții vs plăți), the grid,
  the header / footer icons (Asocieri, refresh, rebuild).
- `tur-plati.md` (4) — tree + «+», grid, the bank-statement pane, the CAB download icon.
- `tur-extrase.md` (4) — opens «Extrase de cont» from the menu; tree + display modes, the two grids,
  «Descarcă extrasele».
- `tur-setari.md` (6) — the pages Informații, Aplicație, FOREXE, Autentificare, Jurnal.
No tour for the ORD editor or the Asocieri window: both are modal, and a modal window disables the
tour's bubble.

## Fix found while writing
`contabil/setari.md` said password change lives on «Autentificare». It is on **Informații**
(`SetariInfoView`: lblTitluParola / txtParolaActuala / ...); «Autentificare» holds what the login
window remembers («Uită datele memorate») and the server address. Table fixed (0000-03 error).

## Test results
`Check-Help.ps1`: 43 topics, 11 tours, 47 capture tags, no errors. Build App 0 warnings, 0 errors.
No tour run on screen.

## To read (operator)
- `tur-ord` step 2: if the selected angajament has no ORD, the view is disabled and the tour says the
  target is not on screen — the step text tells the operator to pick one that has.
