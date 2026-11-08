# SLICE-03EF-01 - «Sincronizeaza facturi primite»: no period boxes in the ribbon, the click opens the popup (08.11.2026)

## What changed and why
Operator (08.11.2026), for the received-invoices ribbon: (1) take the box «Între 1 și 60 de zile» and the combo «Perioada de adus» out of the ribbon; (2) a click on «Sincronizează facturi primite» opens the popup of «Actualizează» (the footer of the tree of `FacturiCumparareForm`).
- `AvacontMain.Designer.vb`: `KBotRibbonField1/2` (`ef_primite_zile_combo`, `ef_primite_zile_text`) removed from the form group; tooltip of the button now says the period is chosen in the list that opens.
- `AvacontMain.vb`: the two keys, `SyncDayChoices` and `ReadSyncDays` removed; `UpdateFormGroup` no longer touches the fields; `RunPrimiteSync` asks the page on screen (`FacturiCumparareForm.OpenSyncPopup`) to open the popup under the button (`ribbon.ButtonScreenBounds`).
- `FacturiCumparareView.vb`: the popup is built by `BuildSyncMenu` (one source for both places); the footer keeps `OpenSyncPopup()` (above the footer, right-aligned), the ribbon uses the new public `OpenSyncPopup(anchor, rect)` (`CustomPopup.ShowBelow`, under the button). Same rows, same answers (preset period / days typed + Enter / «Sincronizează cu serverul»).
- `KbotForm.EFactura.vb`: `SyncPrimite(days)` removed (its only caller was the old button).

## Files touched
`KBot.App/Avacont/AvacontMain.vb`, `AvacontMain.Designer.vb`; `KBot.App/KBotForm/Integrations/KbotForm.EFactura.vb`; `KBot.EFactura/Forms/FacturiCumparareForm.vb`; `KBot.EFactura/Views/FacturiCumparare/FacturiCumparareView.vb`. Help 0000-171.

## Test results
Nothing run, nothing seen on screen: the build is clean (KBot.App, Debug, 0 warnings, 0 errors), no test was run (rule), the designer was never opened in Visual Studio.

## Unverified / deferred
Where the popup opens (just under the button, icon bar on the right as in the footer) was not seen; the popup guard `CustomPopup.ClosedJustNow` is the same as the footer's. The Access-ribbon tutorials were checked by grep: none names the two removed fields.
