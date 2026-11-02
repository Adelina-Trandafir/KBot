# SLICE-0000-17 — Schimbarea unității închide singură conexiunea FOREXE (+ certificatul, la cerere)

## What changed and why

0097 initially REFUSED unit switching while FOREXE connected. Operator (30.09.2026): «nu e motiv de eroare. se deconectează și apoi se schimbă unitatea în mod automat». Now closes via `ForexeController.DisconnectAsync` (`SLICE-0097-corective.md`), refusing only during an operation. Same-day request: forget remembered certificate on switch ONLY through new, initially unticked «Setări → FOREXE» option. Help aligned:

- `contabil.fereastra` § «Unitatea de lucru» (tag `slice: 0097`): connected → K-BOT closes the connection; «Conectare» on the new unit uses the remembered certificate, unless the option is ticked, then it asks again; refused only while the robot is working, and toward a «Director» unit.
- `contabil.setari`: table row «FOREXE» + new section «Pagina «FOREXE»: certificatul memorat» (tag `slice: 0072, 0097`): «Certificatul memorat», «Uită certificatul», and the new «Uită certificatul memorat când schimb unitatea din bara de titlu» (unticked at start, what each state does); keywords `certificat memorat, uita certificatul, schimbare unitate`.
- `tours/tur-fereastra.md` step «Bara de titlu»: «(conexiunea FOREXE, dacă e deschisă, se închide singură)» instead of «(nu și cât ești conectat la FOREXE)».
- `tours/tur-setari.md` step «FOREXE» (tag + 0097): mentions the new option.

## Files touched

`src/KBot.App/HelpContent/contabil/fereastra.md`; `src/KBot.App/HelpContent/contabil/setari.md`; `src/KBot.App/HelpContent/tours/tur-fereastra.md`; `src/KBot.App/HelpContent/tours/tur-setari.md`; `docs/worklog/state/KBOT_STATUS_0000-0009.md` (row 0000-17, watermark note)

## Test results

- `tools/HelpCheck/Check-Help.ps1 -Coverage`: **No errors.** Coverage lists `RobotQueueForm` (slice 0098, another thread's window) — not part of this change.
- No tests, no app run.

## Captures

- None new. To re-shoot: `setari` only if the operator wants the FOREXE page shown (no capture of that page exists today); `unitate-selector` unchanged.

## De citit de operator

- The texts rely on `ForexeFooterView.btnConectare_Click`: with a remembered certificate «Conectare» uses it without the picker; with none it opens the picker. Not seen on screen.
