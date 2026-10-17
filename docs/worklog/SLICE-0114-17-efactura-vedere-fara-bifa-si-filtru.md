# SLICE-0114-17 - E-Factura view: no «show all» tick, only the «without operations» filter, footer «Actualizeaza» with the list's icon (operator item 5) (17.10.2026)

## What changed and why
`PrimiteView` with a DDF lists ONLY the invoices linked to it: the tick «Arata toate facturile primite» (control, handler, theme lines, second server read) is gone. The filter menu of the tree header no longer offers «Afisare facturi noi» when the view has a DDF (it stays in the window with all the received invoices); the «fara operatii» filter and the grouping stay. The footer of the received-invoices tree says «Actualizeaza» (was «Actualizare...») and uses the icon of the angajament list's footer (the picture was copied into the resources of KBot.EFactura); the idle text after a sync follows. The change is in the shared control, so the «Facturi primite» window gets the same footer.

## Files touched
`src/KBot.EFactura/Views/Primite/PrimiteView.vb`, `PrimiteView.Designer.vb`, `Resources/Jonas-Rask-Danish-Royalty-Free-Refresh.32.png`, `My Project/Resources.resx`, `Resources.Designer.vb`.

## Test results
`dotnet build src/KBot.App/KBot.App.vbproj`: 0 warnings, 0 errors. Nothing run, nothing seen on screen.

## Unverified / deferred
- Not seen on screen: the new buttons in the view group, the popup of the document button (opens at the mouse), the fields in the ribbon, the tree rows of the issued invoices.
- The grouping options stay in the filter menu of the view (only the «noi» filter was removed): the operator said «only the without-operations option», read as only the filter.
