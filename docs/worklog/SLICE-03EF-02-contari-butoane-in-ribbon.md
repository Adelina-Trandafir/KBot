# SLICE-03EF-02 - «Contari»: the buttons of the view in the ribbon (08.11.2026)

## What changed and why
Operator (08.11.2026): in the right-hand group of the ribbon, while «Contari» is on screen: «Adaugă operații contabile», a separator, «Salvează ca...» (a sub-popup with «Arhivă ZIP» / «Fișier XML») and «Ascunde...». They mirror the context menu of an invoice and work on the SHOWN invoice (the context menu stays too).
- Designer: five buttons in the right group (`ef_contare_adauga`, `_salveaza`, `_renunta`, `_salveaza_ca`, `_ascunde`), `Visible = False`; separator = `SeparatorAfter` of «Adaugă».
- `FacturiCumparareView.Contare.vb`: `ContareRibbonShown / Enabled / Text / Tip` + `RunContareRibbon` (public, `KBot.EFactura` cannot see the ribbon); the form (`FacturiCumparareForm`) forwards them; `AvacontMain.UpdateViewGroup` maps them to `ViewButtonState` and names the group «Contări».
- «Ascunde...» = the archive of 01EF-03 (the operator's «Ascunde» read as the archive: invoices archived leave the list until «Afișare facturi din arhivă»): it archives the shown invoice, becomes «Scoate din arhivă» for an archived one, and is dimmed when the invoice already has accounting operations (the server refuses it).
- «Salvează ca...» opens a `CustomPopup` under the button (`ShowBelow`) with the two existing rows (`MenuZip` / `MenuXml`).
- The view tells the window about any change through the event it already had (`ReceptiiActionChanged`), raised from `NotifyContare` (shown invoice, load, ticks, calculation).

## Files touched
`KBot.App/Avacont/AvacontMain.vb`, `AvacontMain.Designer.vb`; `KBot.EFactura/Forms/FacturiCumparareForm.vb`; `KBot.EFactura/Views/FacturiCumparare/FacturiCumparareView.vb`, `FacturiCumparareView.Contare.vb`, `FacturiCumparareView.Receptii.vb` (doc line only). Help 0000-171.

## Test results
Nothing run, nothing seen on screen: the build is clean (KBot.App, Debug, 0 warnings, 0 errors), no test was run (rule), the designer was never opened in Visual Studio.

## Unverified / deferred
- **ASSUMPTION to confirm:** «Ascunde...» = archive. If the operator meant something else (hide the tree node only, ...), only the action behind `RibbonContareAscunde` changes.
- The icons are existing ones (`plus_green`, `export`, `Umut_Pulat_Tulliana_2_File_locked_32`, `save_32`, the cancel icon): chosen without seeing them.
- «Adaugă operații contabile» starts from the invoice SHOWN in the tree (dimmed with none shown).
