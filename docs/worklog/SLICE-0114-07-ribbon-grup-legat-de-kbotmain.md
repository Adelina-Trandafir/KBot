# SLICE-0114-07 - Form group tied to the whole form (KbotMain), not to a view (operator item 2) (17.10.2026)

## What changed and why
`AvacontMain.UpdateFormGroup` (replaces `UpdatePrimiteSyncControls` and the `UpdateExtraseDownloadButton` of 0114-05; called from `UpdateCloseButton` on every page / shell change) decides what the form group holds from the FORM on screen: the shell (KbotMain, every view of it) -> «Editeaza grupe», «Descarca extrase»; the «Extrase de cont» window -> «Descarca extrase»; the received invoices window -> «Sincronizeaza facturi primite» + its two inputs. The caption is the form («Lista angajamentelor» / «Extrase de cont» / «Facturi primite»). The view group («Sumar» ...) is untouched. The tab «Descarcare» group of the Extrase tab is gone.

## Files touched
`src/KBot.App/AvacontMain.vb`, `AvacontMain.Designer.vb`.

## Test results
`dotnet build src/KBot.App/KBot.App.vbproj`: 0 warnings, 0 errors. Nothing run, nothing seen on screen.

## Unverified / deferred
- Not seen on screen: layout of the two ribbon groups side by side, the footer popup position, the nav order.
- Reading of «KbotMain»: the whole shell (any view). If only some views were meant, narrow `UpdateFormGroup`.
