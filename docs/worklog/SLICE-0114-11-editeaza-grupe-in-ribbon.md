# SLICE-0114-11 - KbotMain: «Editeaza grupe» out of the popup menu, into the ribbon form group (operator item 6.1) (17.10.2026)

## What changed and why
The row «Editeaza grupe...» and its separator are gone from `menuGrupe` (`KbotForm.Grupe.vb`); new ribbon button `grupe_editeaza` (`FormGroup`, shell only) calls `KbotForm.DeschideEditorulGrupe` (now `Friend`). Tutorial anchor doc updated.

## Files touched
`src/KBot.App/KbotForm.Grupe.vb`, `AvacontMain.vb`, `AvacontMain.Designer.vb`, `KbotForm.Tutorial.vb` (comment).

## Test results
`dotnet build src/KBot.App/KBot.App.vbproj`: 0 warnings, 0 errors. Nothing run, nothing seen on screen.

## Unverified / deferred
- Not seen on screen: layout of the two ribbon groups side by side, the footer popup position, the nav order.
