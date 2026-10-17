# SLICE-0114-14 - KbotMain: the E-Factura view button under Extrase, aligned top (operator item 6.4) (17.10.2026)

## What changed and why
In `navViews` the `efactura` item is no longer `Align = Far` (the bottom block with Fundamentare / Ordonantare / Note corectie): it is a near (top) item placed right after «Extrase».

## Files touched
`src/KBot.App/KbotForm.Designer.vb`.

## Test results
`dotnet build src/KBot.App/KBot.App.vbproj`: 0 warnings, 0 errors. Nothing run, nothing seen on screen.

## Unverified / deferred
- Not seen on screen: layout of the two ribbon groups side by side, the footer popup position, the nav order.
