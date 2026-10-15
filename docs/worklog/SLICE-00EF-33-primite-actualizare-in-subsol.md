# SLICE-00EF-33 - Received invoices: «Actualizare…» in the footer of the tree; no «Reimprospateaza» (15.10.2026)

## What changed and why
1. **«Reimprospateaza» of the window is gone** (`PrimiteForm`: button, handler, designer). Operator: the re-read of the database is not wanted.
2. **Footer of the tree** (`PrimiteView`, so in the «Cumparare» page AND in the DDF view «E-Factura»): caption «Actualizare…», the refresh
   picture on the right (`refresh.png`, new resource), tooltip. A click on the caption or on the picture opens a popup ABOVE the footer.
3. **The popup is the standard `CustomPopup`**, with two NEW kinds of row (slice 00EF-34, see its worklog): a combo and a text box.
   (A first version with a window of its own, `KBotComboTextPopup`, was replaced the same day at the operator's request.)
4. **New tree event `AdvancedTreeControl.FooterCaptionClicked`**: the rest of the footer band (outside the collapse button and the two
   icons) used to be swallowed; now it raises this, so the caption can be a button.
5. **The update moved into the control**: `PrimiteView.SyncFromAnafAsync(days)` (the loop, the progress in the footer caption and in the
   new event `SyncStatus`, the result window, one at a time) and `PrimiteForm.SyncFromAnafAsync` only forwards (the ribbon button of 00EF-30
   still calls it). The text box: empty or outside 1-60 = no update and a warning. In the DDF view the list is read again with its DDF.
6. The ribbon button and its two fields (0114-01) are KEPT: nothing was said to remove them.

## Files touched
`src/KBot.Controls/Tree/AdvancedTreeControl.Events.vb`,
`AdvancedTreeControl.Footer.vb`, `src/KBot.EFactura/Views/Primite/PrimiteView.vb`, `PrimiteView.Designer.vb`,
`src/KBot.EFactura/Forms/PrimiteForm.vb`, `PrimiteForm.Designer.vb`, `SincronizareNoiForm.vb` (a comment),
`src/KBot.EFactura/My Project/Resources.resx`, `Resources.Designer.vb`, `Resources/refresh.png`; help `contabil/efactura/index.md` (0000-79).

## Test results
`dotnet build src/KBot.App`: 0 warnings, 0 errors. Nothing run, nothing seen on screen; no real update from ANAF.

## Unverified / deferred
- The popup: its place above the footer, the focus in the text box, that picking a row of the list closes it and starts the update
  (the list's own drop-down window is no-activate, so it should not close the popup by deactivation; untested), Enter in the box.
- The caption alignment of the footer (right, as in the Recepții tree) next to the refresh picture.
- Two entry points exist now (ribbon and footer); say if the ribbon ones should go.
