# SLICE-0114-12 - KbotMain: a popup on the footer «Actualizeaza» button (operator item 6.2) (17.10.2026)

## What changed and why
The right footer icon AND the footer text (`FooterCaptionClicked`) open a `CustomPopup` above the footer (icon bar on the right, as in the received invoices tree): «Descarca angajamente NOI» (what the icon did alone: the list refresh, now `DescarcaAngajamenteNoi`) and «Actualizeaza angajamente VRAC» (`DeschideActualizareaMultipla`). The row «Actualizeaza angajamente...» left the tree options menu. Tutorial signal `footer-menu:<key>` + anchors `popup.noi` / `popup.vrac`. Footer icon tooltip reworded.

## Files touched
`src/KBot.App/KbotForm.Download.vb`, `KbotForm.TreeOptions.vb`, `KbotForm.Tutorial.vb`, `KbotForm.Designer.vb`.

## Test results
`dotnet build src/KBot.App/KBot.App.vbproj`: 0 warnings, 0 errors. Nothing run, nothing seen on screen.

## Unverified / deferred
- Not seen on screen: layout of the two ribbon groups side by side, the footer popup position, the nav order.
