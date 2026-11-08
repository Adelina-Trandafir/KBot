# SLICE-03EF-08 - «Salvează ca...» becomes «Exportă...» (08.11.2026)

## What changed and why
Operator (after trying 03EF-02): the ribbon button «Salvează ca...» is named «Exportă...». Only the text, the header and the tooltip of the button (`AvacontMain.Designer.vb`) and the comments; the key `ef_contare_salveaza_ca` and the popup («Arhivă ZIP» / «Fișier XML») are unchanged. The context menu of an invoice still says «Salvează ca ZIP» / «Salvează ca XML» (not asked).

## Files touched
`KBot.App/Avacont/AvacontMain.Designer.vb`, `KBot.EFactura/Views/FacturiCumparare/FacturiCumparareView.Contare.vb` (comments). Help 0000-172.

## Test results
The build of the projects is clean (KBot.Controls, KBot.EFactura: 0 warnings, 0 errors; KBot.App could not be copied into its output while K-BOT / Visual Studio had it open, but it reports no compile error). No test run, nothing seen on screen.

## Unverified / deferred
—
