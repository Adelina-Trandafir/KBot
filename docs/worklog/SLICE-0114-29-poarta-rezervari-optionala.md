# SLICE-0114-29 - The reservation gate of 0114-26 becomes optional (Setari > FOREXE) (07.11.2026)

Slice choice (operator, 07.11.2026): 0114-27 and 0114-28 were taken, so the first free sub-slice of 0114, 0114-29.

## What changed and why
Operator request: the «Rezervarea nu a inceput» message (no new FOREXE reservation while the angajament has reservations without a revision) must be optional from Setari, FOREXE page; and the FOREXE settings view must scroll vertically (too many rows).
- `AppSettings.vb`: new `ForexeReservationGate` (default True = the rule as it was), copied in `Clone`, read from the file through the DTO (`Boolean?`).
- `ForexeWatchConfig.vb`: the JSON sent to the page now carries `gate`.
- `ForexeWatch.js`: `configFrom` reads `gate` (on unless it says false); `onPoartaClick` returns at once when it is off, so the page never asks the server and the click goes through. Applied to the open page with the other page settings (`AplicaSetarilePaginiiAsync`).
- `SetariForexeView.vb` / `.Designer.vb`: new check box `chkPoartaRezervari` in the «Browserul» section (row 3, the capture combo moved to row 4), saved at once, pushed into the open page, status line in Romanian.
- Scroll: `tlyBody` already had `AutoScroll = True`; its last row was Percent 100, now Absolute 20 like `SetariAplicatieView.tlyGenerale` (same pattern, there it is the proven one). The Percent row is the suspected reason the scroll did not show; not confirmed on screen.
- Help: `contabil/setari.md` new section «Pagina «FOREXE»: rezervari fara revizie» (tag `0114-29`); `contabil/forexe/browser.md` gate paragraph points to the switch.

## Files touched
`src/KBot.Common/AppSettings.vb`, `src/KBot.Forexe/Models/ForexeWatchConfig.vb`, `src/KBot.Forexe/Services/JavaScripts/ForexeWatch.js`, `src/KBot.App/Setari/Views/SetariForexeView.vb`, `src/KBot.App/Setari/Views/SetariForexeView.Designer.vb`, `src/KBot.App/HelpContent/contabil/setari.md`, `src/KBot.App/HelpContent/contabil/forexe/browser.md`.

## Test results
`dotnet build src/KBot.App/KBot.App.vbproj`: 0 warnings, 0 errors. No tests run (operator rule). `Check-Help.ps1` not run.

## Unverified / deferred
- Nothing seen on screen: neither the new check box, nor the scroll bar, nor the gate behaviour in the real FOREXE page with the box off.
- The server route `verifica-rezervare` is untouched (the gate is skipped in the page, before any call).
- Not committed (operator rule: no git writes).
