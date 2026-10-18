# SLICE-KE12 - The reception window becomes a page of the ribbon (18.10.2026)

## What changed and why
Operator request: the window built in KE11 (opened by «Leagă» / «Recepție nouă» of the «Recepții» view) becomes a stand-alone PAGE, not a modal window; it goes in the ribbon, tab E-Factura,
after «Cumpărare», with a separator after it; the footer buttons of the «Recepții» view go away, in E-Factura and in K-BOT alike (the same `PrimiteView` hosts both), for now.

- **`KeLegaForm` is a page.** Base class `KBotShellForm` (like `PrimiteForm`), borderless, hosted by `AvacontMain.ShowPage`. New constructor `New(api, gate, unitName, year)`; no modal parts left
  (`DialogResult`, «Renunță», the results `Facturi` / `Cod` / `Receptie` ...). It reads the year's invoices itself (`GetPrimiteAsync`), `ReloadAsync` forgets states and reads again.
- **Mode chosen in the page**: a combo «Recepția:» above the data of the reception (new / link to an existing / modify an existing); `CmbMod_SelectedIndexChanged` re-applies the mode.
- **The work moved in**: what `PrimiteView.LinkManyAsync` did (save classifications, the server's check of every invoice, the robot for new / modified, ONE marker, the links, the failure messages) is now
  `KeLegaForm.CommitAsync` (+ `ReserveMarkerAsync`, `MarkerRegex`, `SumRows`, `KeMarker`). While it runs the page is disabled (`SetBusy`); afterwards the page reads the invoices again (the covered ones leave the tree)
  and says how it went in the notice. `KeLegaFactura` got `Verificare`.
- **Ribbon**: button «Recepții» (key `efactura_receptii`, icon `Fatcow_Farm_Fresh_Check_boxes_32`) after «Cumpărare»; new property `KBotRibbonButton.SeparatorAfter` (a thin line after a Large button of a group when another
  button follows; `KBotRibbon.PlaceGroup` / `PaintGroup`), set on the new button, so the separator sits between «Recepții» and «Vânzare». Machete type `efactura_receptii` registered
  (`KbotForm.Machete.vb`), opener `KbotForm.DeschideEFacturaReceptii`, `AvacontMain.Ribbon_ItemClicked`.
- **«Recepții» view**: `PrimiteReceptiiPage` has no footer: the four buttons (Leagă, Nouă, Modifică, **Scoate legătură**) and their events are gone, so removing a link is not possible from anywhere for now.
  `PrimiteView.Receptii.vb`: `PgReceptii_LinkRequested`, `PgReceptii_RemoveRequested`, `LinkAsync`, the marker code removed; `PrimiteView.ReceptieMultipla.vb` deleted (its work is in the page).
- **The list's «Recepție nouă» button** (KE10, footer of the list «Toate facturile») is removed too: it opened the modal window that no longer exists (decision of mine, to confirm; the page replaces it).

## Decisions taken without the operator (to confirm)
1. «Scoate legătura» went with the other footer buttons (the operator said «butoanele din footer»): no way to remove a link until it is given a place.
2. The separator is a new ribbon feature inside a group (the ribbon only had group-edge lines and pinned-button lines); it comes after «Recepții», so «Vânzare» follows it.
3. The mode combo's three texts: «Recepție nouă», «Leagă de o recepție existentă», «Modifică o recepție existentă».
4. The year of the invoices is the year of K-BOT (as «Cumpărare»); invoices of other years are not listed.

## Files touched
`KBot.EFactura/Forms/KeLegaForm.vb`, `KeLegaForm.Designer.vb`, `KeLegaFactura.vb`; `Views/Primite/PrimiteReceptiiPage.vb` + `.Designer.vb`, `PrimiteView.Receptii.vb`, `PrimiteListaPage.vb` + `.Designer.vb`,
`PrimiteView.ReceptieMultipla.vb` (deleted); `KBot.Controls/Ribbon/KBotRibbonItems.vb`, `KBotRibbon.vb`; `KBot.App/AvacontMain.Designer.vb`, `AvacontMain.vb`, `KbotForm.EFactura.vb`, `KbotForm.Machete.vb`;
help `HelpContent/contabil/efactura/index.md` (0000-102).

## Test results
`dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors. Nothing run, nothing seen on screen, no test written or run.

## Unverified / deferred
- Nothing seen on screen: the page as a hosted page (its caption bar is hidden by the host), the mode row, the ribbon button and its separator, the view without footer.
- The page on a unit without received invoices, the role «Contabil» check (the button stays off), the robot missing (a box at the click) — written, never run.
- The page closes the combo of a group header when a layout pass runs and the combo has no focus (KE11).
- All the old KE caveats stay: SQL not run, server not deployed, the two workflows not run on the real site.
