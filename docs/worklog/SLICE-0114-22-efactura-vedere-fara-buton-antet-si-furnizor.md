# SLICE-0114-22 - E-Factura VIEW: no options button in the tree header, no supplier level (17.10.2026)

## What changed and why
Operator (second time asked for the grouping): in the VIEW of the angajament (`EFacturaView`, not the window «Facturi primite»): (1) the tree's header-right button goes; (2) the tree is «Toate facturile» / month-year / invoice, no supplier level. `PrimiteView` gets a designer property `AngajamentView` (set True in `EFacturaView.Designer.vb`): it takes the options (funnel) icon off the header (the icon the designer gave is kept and given back if the property is False again), `BuildMonthTree` puts the invoices (newest first) directly under their month, and a stray click on the header-right area does nothing. The invoice rows are built by one shared routine (`AddInvoiceNodes`) for both shapes. The window «Facturi primite» is unchanged (supplier level, options menu). With the button gone, the view has no «fara operatii» filter any more (it was in that menu).

## Files touched
`src/KBot.EFactura/Views/Primite/PrimiteView.vb`, `src/KBot.App/Views/EFactura/EFacturaView.Designer.vb`; help `contabil/efactura/index.md` (0000-93).

## Test results
`dotnet build KBot.sln`: 0 warnings, 0 errors. Nothing run, nothing seen on screen.

## Unverified / deferred
- Not seen on screen: the header with only the magnifier, the month opening on the selected invoice, the month rows without a supplier level.
- The capture `efactura-facturi`/view captures may show the old tree; not marked for redo here (the view's capture is not among those named in the help).
