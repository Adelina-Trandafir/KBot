# SLICE-0095 - Istoric: root node «Tot istoricul»

Operator request, 29.09.2026: the Istoric tree gets a root level «Tot istoricul», like the
other trees (Extrase has «Toate extrasele»). Clicking it does what the view does on load:
every FX_Istoric row of the angajament.

## Change

`src/KBot.App/Views/IstoricView.vb` only.

- `BuildTree`: a bold, expanded root `"all"` with caption `Tot istoricul~~~<row count>`
  (count = ALL rows, dated or not, the same number the grid shows on load). Months hang under
  it (collapsed, as before); days under months (unchanged).
- `NodPerioada.Tot()` factory + `EsteTot` flag for the root's `Tag`.
- `Tree_NodeMouseUp`: root -> `_filter.ClearAll()` + refill (the same clearing `LoadAsync` does).
  Month / day nodes unchanged.

## Notes

- On load the view still behaves as before; only an interval requested by FOREXE (slice 0073)
  narrows it. Clicking the root drops that interval too - it is «everything», like «Reset».
- Undated rows had no tree node before; the root now reaches them.

## Status

Build of KBot.App clean (0 errors). Not run, not seen on screen, no tests (operator's rule).
KBot.App FileVersion not bumped here - `push-update.ps1` asks for it.
