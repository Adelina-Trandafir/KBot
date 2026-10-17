# SLICE-0114-04 - Ribbon: tab order, "Date" folded into Configurari, new Extrase tab (17.10.2026)

## What changed and why
Operator request. Ribbon tabs are now: Configurari, Angajamente (selected at start), E-Factura, Extrase.
- "Date" tab is gone; its group (Clasificatii bugetare, Parteneri) is the first group of Configurari, now captioned "Date".
- "Extrase" button left the old "Documente" group's tab and sits in the new "Extrase" tab (group "Documente"); a second
  group "Descarcare" holds the new button `extrase_descarca` ("Descarca extrase") = the same download + import as the
  tree-footer icon, then the open Extrase view reloads.
- `ribbon.SelectedTabIndex = 1` (Angajamente) in the designer.

## Files touched
`src/KBot.App/AvacontMain.Designer.vb`, `AvacontMain.vb` (new `Case "extrase_descarca"`), `KbotForm.Extrase.vb`
(`DescarcaExtraseDinRibbon`).

## Test results
`dotnet build src/KBot.App/KBot.App.vbproj`: 0 warnings, 0 errors. No test run, nothing run on screen.

## Unverified / deferred
Not seen on screen: tab order, the preselected tab, the new button (its icon reuses the FOREXE browser icon).
Help done as 0000-88.
