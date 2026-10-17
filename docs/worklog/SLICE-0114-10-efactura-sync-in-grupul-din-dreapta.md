# SLICE-0114-10 - E-Factura: «Sincronizeaza facturi primite» and its inputs in the form group (operator item 5) (17.10.2026)

## What changed and why
`ef_primite_sync` and the two fields (days list, days box) moved from the «ANAF» group of the E-Factura tab to `ribbon.FormGroup`; shown while the received invoices window is on screen (caption «Facturi primite»). «ANAF» keeps only «Token ANAF».

## Files touched
`src/KBot.App/AvacontMain.Designer.vb`, `AvacontMain.vb`.

## Test results
`dotnet build src/KBot.App/KBot.App.vbproj`: 0 warnings, 0 errors. Nothing run, nothing seen on screen.

## Unverified / deferred
- Not seen on screen: layout of the two ribbon groups side by side, the footer popup position, the nav order.
