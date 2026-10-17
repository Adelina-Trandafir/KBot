# SLICE-0114-19 - E-Factura Vanzare: the month list removed, the tree rows no longer cut (operator item 7.1, 7.2) (17.10.2026)

## What changed and why
The month list above the tree of the issued invoices (`cmbLuna`, its fill, handler and filter) is removed: the tree shows every invoice of the year. The rows were cut after a few letters because the designer pinned the left text of the tree to 100 px (`tree.LeftTextWidth = 100`); that line is gone (0 = dynamic, as in the received-invoices tree), so the text takes the room between the icon and the amount. **Item 7.3 (the id used to download the invoice as ANAF draws it) was NOT changed**: the client sends only the invoice id and the server downloads with the `id_descarcare` stored on the invoice (`trimitere.descarca` -> `anaf_api.descarca`); nothing in the code picks another number, so if the stored value is not the one ANAF calls the download id (the 4xxx one) it was written wrong earlier. It needs the stored value or the error the operator sees.

## Files touched
`src/KBot.EFactura/FacturiForm.vb`, `FacturiForm.Arbore.vb`, `FacturiForm.Designer.vb`.

## Test results
`dotnet build src/KBot.App/KBot.App.vbproj`: 0 warnings, 0 errors. Nothing run, nothing seen on screen.

## Unverified / deferred
- Not seen on screen: the new buttons in the view group, the popup of the document button (opens at the mouse), the fields in the ribbon, the tree rows of the issued invoices.
- Item 7.3 is open: needs the id stored in `EF_Facturi.id_descarcare` of an invoice that fails, and the error text of the «Factura ANAF» view.
