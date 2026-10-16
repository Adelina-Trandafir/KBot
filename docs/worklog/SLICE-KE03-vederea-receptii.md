# SLICE-KE03 - The "Receptii" view of a received invoice (16.10.2026)

## What changed and why
Operator request (16.10.2026): link the received e-invoices to receptions and, later, to payments; plan in `docs/KE_PLAN.md`.

- New page `PrimiteReceptiiPage` and its place in the bar of views of `PrimiteView` (after "Mesaje"), shown once an invoice is chosen, in every host of `PrimiteView` (the "Facturi primite" window and the "E-Factura" view of the DDF).
- It shows the total with VAT, what is allocated, the rest and the state, the table of links (angajament, classification, reception, kind, sum) and the buttons: "Clasificatii pe linii...", "Leaga de receptie...", "Receptie noua...", "Modifica receptia...", "Scoate legatura". For a role other than `Contabil` the buttons are hidden and a line says why.
- `PrimiteView.Receptii.vb` (partial) reads the state right after the invoice (own call `GetKeAsync`, a failure becomes a line in the view) and fills the two new columns of the lines grid.

## Files touched
`src/KBot.EFactura/Views/Primite/PrimiteReceptiiPage.vb` + designer, `PrimiteView.Receptii.vb`, `PrimiteView.vb`, `PrimiteView.Designer.vb`.

## Test results
`dotnet build` of `KBot.Api`, `KBot.EFactura`, `KBot.Forexe` and `KBot.App`: 0 errors, 0 warnings; `py_compile` of the changed Python files: ok; the two new workflows parse as XML. **Nothing was run against a live database, the live site or on screen, and no test was written or run** (operator rule: no tests).

## Unverified / deferred
- The nav item reuses the `invoice` icon (no new image was made).
- `sql/KE_01_primite_receptii.sql` is NOT run on `AVACONT_SURSA` nor synced to the unit databases; the server is NOT deployed. Until the script runs, the received-invoice routes answer "Tabelele E-Factura ale unitatii lipsesc sau sunt vechi" (the new `CotaTVA` column is part of the sync insert).
- Nothing here was seen on screen: the forms and the view are written by hand in their designer files (never opened in the Visual Studio designer).
