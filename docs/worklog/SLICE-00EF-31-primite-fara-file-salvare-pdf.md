# SLICE-00EF-31 - Received invoices: no «Atasamente» / «Observatii» views; the operator chooses where a PDF is saved (15.10.2026)

## What changed and why
1. **Views «Atasamente» and «Observatii» removed** from `PrimiteView` (the control of the «Cumparare» page and of the DDF view
   «E-Factura»): nav items, pages (`pgAtasamente`, `pgObservatii`), the double-click save of an attachment and their fill code. The files
   are still reached from the popup of «Factura PDF» (the notice for a file that cannot be shown no longer sends to the removed view);
   the notes are above the lines of «Continut» (slice 00EF-29). The page classes `PrimiteAtasamentePage` / `PrimiteObservatiiPage`
   stay in the project (`PrimiteObservatiiPage.CleanNotes` is used by the notes box).
2. **Adobe's Save is trapped, and the operator chooses**: new `AdobeSaveTrap.ChooseSavePath` (`Func(Of String, String)`): when set, a
   Save As of the document is parked off screen, the chooser is called from a posted callback (never inside a sweep) with the
   file name Adobe offered and answers a full path, or Nothing; a path -> the same save flow as before but with THAT path in
   Adobe's dialog (`_chosenPath`; the overwrite question is already answered by the operator's own dialog and Adobe's is accepted
   by the trap as before); Nothing -> Adobe's dialog is cancelled, nothing is raised. Forwarded by `AdobeReaderHost` and
   `AcroPdfViewer`; `ReaderHostPreview.UserChoosesSave` shows a `SaveFileDialog` («Salveaza documentul», PDF, overwrite prompt);
   `FacturaPdfViewer(True)` is what the received-invoice hosts pass (`PrimiteForm`, `EFacturaView`). The issued invoices and the
   DDF keep the old behaviour (a save goes back onto the document).
3. The ribbon fields overlapped (the KBot combo forces its own height): see `SLICE-0114-01-ribbon-campuri.md`, follow-up.

## Files touched
`src/KBot.EFactura/Views/Primite/PrimiteView.vb`, `PrimiteView.Designer.vb`, `src/KBot.Controls/Adobe/AdobeSaveTrap.vb`,
`AdobeReaderHost.vb`, `AcroPdfViewer.SaveTrap.vb`, `src/KBot.App/Views/Ddf/ReaderHostPreview.vb`,
`src/KBot.App/Views/EFactura/Vanzare/FacturaPdfViewer.vb`, `EFacturaView.vb`, `src/KBot.App/KbotForm.EFactura.vb`.

## Test results
`dotnet build src/KBot.App`: 0 warnings, 0 errors. No tests run. **The save trap was not exercised against a real Adobe**.

## Unverified / deferred
- Everything about the Save As flow is untested: that the chooser appears, that the file is written where chosen, that Cancel leaves
  Adobe usable, that the second Save works. Look at `adobe_preview.log` (lines «operatorul alege unde se salveaza», «Salvare anulata de operator»).
- A Save As of a DIFFERENT viewer that shares the Acrobat broker (the DDF) is still arbitrated by the existing rules.
- The temp PDF itself is not changed by the save any more when a path is chosen (Adobe now shows the chosen file).
- `Check-Help.ps1`: the tutorials that target `FacturiForm.navDetaliu` (issued invoices) are old errors, untouched.
