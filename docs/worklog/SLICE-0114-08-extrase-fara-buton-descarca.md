# SLICE-0114-08 - «Extrase de cont» window: its own download button removed (operator item 3) (17.10.2026)

## What changed and why
`btnDescarca` is out of `ExtraseForm` (designer, handler, busy / theme lines, the `descarca` constructor argument); the footer table has two columns now. New `ExtraseForm.Reincarca`; `KbotForm.ReloadExtraseView` reloads the window too, so the ribbon download refreshes it. The empty text says the ribbon button brings the statements.

## Files touched
`src/KBot.App/Views/ExtraseForm.vb`, `ExtraseForm.Designer.vb`, `KbotForm.Extrase.vb`.

## Test results
`dotnet build src/KBot.App/KBot.App.vbproj`: 0 warnings, 0 errors. Nothing run, nothing seen on screen.

## Unverified / deferred
- Not seen on screen: layout of the two ribbon groups side by side, the footer popup position, the nav order.
- The window's own «Inchide» button was kept (not mentioned).
