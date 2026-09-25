# SLICE 0072-02 — «Activează opțiuni avansate» (24.09.2026)

## Request

Operator, 24.09.2026: a checkbox «Activeaza optiuni avansate» on the application page of Setări.
It shows or hides four pages: the «Documente» tab of «Aplicație», «Pagina FOREXE», «Temă» and
«Căi fișiere». Ticking it asks for a password.

## What changed

- `AppSettings.AdvancedOptions` (JSON `AdvancedOptions`, default False, saved per Windows user).
- `SetariAplicatieView`: checkbox `chkAvansate` on the «Generale» tab (row 4 of
  `tlyComutatoare`). Ticking opens `ParolaAvansataForm`; a wrong password keeps the dialog open
  with a red line; «Renunță» / Escape puts the tick back. Unticking needs no password. The
  «Documente» tab is hidden with `navPagini.SetItemVisible`; if it was the open tab, the page
  goes back to «Generale».
- `SetariForm`: keys `pagina`, `tema`, `foldere` hidden / shown from `AppSettings.AdvancedOptions`
  at Load and on every `AppSettings.Changed`; a hidden page that was on screen hands over to
  «Aplicație».
- `ParolaAvansataForm` (new, Designer + code). Only the SHA-256 of the password is in the build
  (`PasswordHash`), compared case-sensitively. It keeps an operator out of those pages; it is
  not a security boundary (the value is a plain flag in app_settings.json).
- Tests written (not run): `tests/KBot.App.Tests/ParolaAvansataFormTests.vb`.

## Test results

`dotnet build src\KBot.App\KBot.App.vbproj`: 0 errors, 0 warnings. No tests run.

## Unverified

- Not run; the checkbox and the password dialog not seen on screen.
- Choice made without asking: the switch is SAVED, so once unlocked it stays unlocked on that
  Windows user until unticked.
