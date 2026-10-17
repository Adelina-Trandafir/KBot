# SLICE-0114-09 - «Descarca extrase» in the right group of the ribbon (operator item 4) (17.10.2026)

## What changed and why
The button `extrase_descarca` moved from the Extrase tab (group «Descarcare», removed) to `ribbon.FormGroup`; visible for the shell and the Extrase window (see 0114-07). It keeps the key and the handler (`DescarcaExtraseDinRibbon`).

## Files touched
`src/KBot.App/AvacontMain.Designer.vb`, `AvacontMain.vb`.

## Test results
`dotnet build src/KBot.App/KBot.App.vbproj`: 0 warnings, 0 errors. Nothing run, nothing seen on screen.

## Unverified / deferred
- Not seen on screen: layout of the two ribbon groups side by side, the footer popup position, the nav order.
