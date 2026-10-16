# SLICE-0084-03 — «Asociază parteneri» din Sumar = popup modal

Operator request (16.10.2026): the Sumar button «Asociază parteneri» must open a modal popup,
not a view/page of its own.

## What changed and why
`SumarPartnersForm` was already built as a borderless modal dialog, but `SumarView.btnPartners_Click`
opened it through `AvacontMain.ShowMachetaAsync`, which in the shell turns the form into a docked
page of the main window (its close buttons hidden, closed from the ribbon). Now the click calls
`f.ShowDialog(FindForm())`: a real modal popup centred over the shell, with its own «Renunță»
button, Esc, and «Salvează» (which closes it). The handler is no longer `Async`.
The machete kind `sumar.parteneri` (constant + registry entry in `KbotForm.Machete.vb`) is
removed: a popup is not a page, so it has no place in the history of opened machete.

## Files touched
- `src/KBot.App/Views/SumarView.vb` (`btnPartners_Click`)
- `src/KBot.App/KbotForm.Machete.vb` (constant `MachetaSumarParteneri` + its `Register` line)
- `src/KBot.App/HelpContent/contabil/vederi/sumar.md`, `HelpContent/tours/tur-sumar.md` (help 0000-84)
- status rows in `KBOT_STATUS.md` and `state/KBOT_STATUS_0080-0089.md`

## Test results
`dotnet build src\KBot.App\KBot.App.vbproj` — 0 warnings, 0 errors. No tests written or run
(house rule); the popup was not run and not seen on screen. `Check-Help.ps1` not run.

## Left unverified / deferred
- Not seen on screen: centring over the shell and the 1100x720 size at different DPIs.
- Not committed (git writes are the operator's).
