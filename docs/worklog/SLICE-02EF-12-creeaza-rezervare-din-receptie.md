# SLICE-02EF-12 - «Creeaza rezervare» from «Recepții» (07.11.2026)

## Request
Operator: a separate ribbon button «Creeaza rezervare» that opens the SAME window as «Adauga rezervare» of the Rezervari view, with the value pre-filled from the reception's sum; the user may change it, but on this route only a protection checks that the value is enough for the reception to fit - if not, a blocking error. After a reservation exists the whole app must refresh (the page, the "not enough reservations" message, the open grids, KbotForm if open).

## Final state
- The window is the DDF editor (`DdfEditForm`) opened like «Adauga rezervare» > «Foloseste indicatorii existenti» (`AdaugaRezervareDdfAsync(cuIndicatori:=True)`), now also given `ValoriDinReceptie` (`KeRezervareRand`: indicator, reception sum, reserved now, received before the day). `PrecompleteazaIndicatoriiAsync` puts the reception's sum as the current value of each indicator it touches (total = previous + current).
- Protection (this route only): `DdfEditForm.MotiveReceptieNeacoperita`, run in `BtnSalveaza_Click` after the ordinary checks - the value added must reach `Necesar = reception sum - (reserved - received)`; a line left at 0 or deleted counts as 0. A blocking box names each indicator, what is free, what is needed and what is typed. The ordinary window is unchanged (property Nothing).
- Seam: `IKeRobot.CreeazaRezervareAsync` -> `KbotForm.CreeazaRezervareKeAsync` (`KbotForm.KeRobot.vb`); `DeschideEditorulDdfAsync`/`AdaugaRezervareDdfAsync` now return whether the editor saved.
- Ribbon: `ef_receptie_rezervare` (`AvacontMain`), shown on a new/modified reception with an angajament chosen, lit when the ticked invoices put an amount on a classification (the former reception window).
- Refresh: `KeRobot.DateAngajamentSchimbate` (static event, `AnuntaDateSchimbate(cod)`), raised by `DupaScriereaDdf` (a DDF saved) and by `IngestFlow.IngestAsync` (a download taken into K-BOT, which also reloads the tree and the open view). Every the former reception window (the page and the embedded one) clears its availability caches and asks the server again, so its message is judged again.

## Files touched
`src/KBot.Domain/EFacturaKeRobot.vb`, `src/KBot.EFactura/Views/Primite/IKeRobot.vb`, the former reception window, `src/KBot.App/DDF_EDIT/DdfEditForm.vb`, `src/KBot.App/KBotForm/Documents/{KbotForm.Ddf,KbotForm.DdfDelete}.vb`, `src/KBot.App/KBotForm/Robot/{KbotForm.KeRobot,IngestFlow}.vb`, `src/KBot.App/Avacont/AvacontMain{,.Designer}.vb`. Help 0000-168.

## Test results
`dotnet build src/KBot.App`: 0 warnings, 0 errors. Tried by the operator (07.11.2026). No automated test.

## Traps / deferred
- **A saved revision does not change `FX_Rezervari`**: the reserved figures change only after the revision is SENT to FOREXE and the angajament is read back. The page's figures and its "not enough reservations" message refresh when that download is ingested; right after the save alone they are the same (confirmed by the operator as the intended behaviour).
- The captures for the DDF are the ones the DDF send takes (nothing added here).
- A reception classification that is not an indicator of the angajament is refused with a message (a new reservation line with a new indicator is not prefilled).
