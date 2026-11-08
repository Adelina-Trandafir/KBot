# SLICE-02EF-10 - Page «Recepții»: the screen pictures are taken when a reception is created (07.11.2026)

## Request
Operator: when a reception is created from «Receptii <> Efactura», the screen captures used in the ordonantare must be saved too.

## Final state
The robot that writes the reception (`KbotForm.KeRobot.vb`, `SalveazaReceptieKeInCoadaAsync`) takes the pictures a reception saved by hand in the FOREXE page gets:
1. right after the site's «saved» message (before the read-back navigates away) it takes the two pictures of the guide (the Receptii tab, then «Informatii complete contract») with `CaptureFlow.TakeReceptionPicturesAsync(cod, idrh)`; the new optional `k_marcaj` puts on both the snapshot number (IDRH) K-BOT reserved for the save (parsed from the marker text by `IdrhDinMarcaj`), because the page's marker memory knows nothing about a robot save;
2. after the ingest wrote the reception, `CaptureFlow.SendAsync(cod, FelReceptie)` files them (`PUT /api/forexe/capturi/receptie/{idrh}` -> `FX_Receptii_IMG`); if the ingest did not happen they stay on disk and the console says so (`SayNotSent`);
3. no pictures when the site did not show its «saved» message (they would show an uncertain state); the console says so.

## Files touched
`src/KBot.App/KBotForm/Robot/CaptureFlow.vb`, `src/KBot.App/KBotForm/Robot/KbotForm.KeRobot.vb`. Help 0000-168.

## Test results
`dotnet build src/KBot.App`: 0 warnings, 0 errors. Tried by the operator (07.11.2026). No automated test.

## Deferred / traps
- The pictures assume the page is still on the Receptii tab when the workflow ends (the manual path assumes the same).
- A picture is taken only when the site's «saved» message was seen; a save confirmed only by the read-back gets none.
