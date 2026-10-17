# SLICE-0114-18 - Ribbon fields of «Sincronizeaza facturi primite»: the list opens and has the rows, the second one is a text box (operator item 6) (17.10.2026)

## What changed and why
Cause: the generated designer code adds a field to the group BEFORE it sets the field's kind, rows and hint, and the ribbon made the real control the moment the field was added: both fields became an empty combo (so the first had nothing to open and the second was not a text box). `KBotRibbon` now makes the controls of the fields on the first layout (`EnsureFieldControls`), when every property is in. The look follows the popup of the invoices tree: the list shows the pale text «Perioada de adus» (no row picked at start; `KBotRibbonField.PlaceholderText` now also works for a combo) and the box «Intre 1 si 60 de zile». Nothing picked and nothing typed = 7 days (said in the tooltip). The icon bar of the popup (the ANAF picture) has no ribbon counterpart.

## Files touched
`src/KBot.Controls/Ribbon/KBotRibbon.vb`, `KBotRibbonItems.vb`, `src/KBot.App/AvacontMain.vb`, `AvacontMain.Designer.vb`.

## Test results
`dotnet build src/KBot.App/KBot.App.vbproj`: 0 warnings, 0 errors. Nothing run, nothing seen on screen.

## Unverified / deferred
- Not seen on screen: the new buttons in the view group, the popup of the document button (opens at the mouse), the fields in the ribbon, the tree rows of the issued invoices.
