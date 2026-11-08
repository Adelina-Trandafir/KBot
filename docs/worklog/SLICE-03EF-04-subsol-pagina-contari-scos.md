# SLICE-03EF-04 - «Contari»: the foot of the page is gone (08.11.2026)

## What changed and why
Operator (08.11.2026): take the whole foot section out of the view: the save/cancel buttons and the HTML panel with information.
- `FacturiCumparareContarePage`: `pnlJos`, `btnRenunta`, `btnSalveaza`, `lblInfo` removed (designer + code); the page rows are now 105 px / 58% / 42%. Events `RenuntaCerut` / `SalveazaCerut` gone (the ribbon calls the view).
- The information of the HTML panel is NOT lost: the supplier, every warning, the codes, the next note number and the bookings are the tooltip of the ribbon button «Salvează» (`ContareRibbonTip`; the tooltip is refreshed with the view group). The most serious warning is no longer shown on its own line.
- With nothing ticked the line that said «Clic dreapta pe o factură ...» is gone; the classification box says «Apăsați «Adaugă operații contabile»» as its grey hint (it goes back to «Alegeți clasificația bugetară» once something is ticked). Error messages of the calculation remain as before in the view's notice.

## Files touched
`KBot.EFactura/Views/FacturiCumparare/FacturiCumparareContarePage.vb`, `.Designer.vb`; `FacturiCumparareView.Contare.vb`. Help 0000-171.

## Test results
Nothing run, nothing seen on screen: the build is clean (KBot.App, Debug, 0 warnings, 0 errors), no test was run (rule), the designer was never opened in Visual Studio.

## Unverified / deferred
Whether a disabled `KBotComboBox` paints its placeholder was not seen. If it does not, the hint is not visible while nothing is ticked.
