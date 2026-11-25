# SLICE-0097-12 — Plafonul așteptării Wicket: 5 s → 10 s

Cererea operatorului (20.11.2026): timpul de așteptare Wicket să crească la 10 secunde.

## What changed and why

`WorkflowExecutor.WicketIdleTimeoutMs` (`WorkflowExecutor.WicketMonitor.vb`): 5000 → 10000 ms. E plafonul cât așteaptă robotul, după un clic, semnalul «Ajax gata» (`#animlogo` dispărut, 300 ms liniște) în `WaitForWicketIdleAsync`; se termină imediat ce semnalul vine, deci doar paginile lente (sau cele fără semnal) pot aștepta mai mult. Se înmulțește în continuare cu multiplicatorul din Setări → FOREXE (×1 … ×5): la ×2 plafonul e 20 s. Prăgul de liniște (`WicketIdleThresholdMs` = 300 ms) nu s-a schimbat. Se aplică tuturor fluxurilor care așteaptă Wicket (paginare, clicuri cu așteptare). KBot.Forexe 1.0.25.0 → 1.0.26.0.

## Files touched

`src/KBot.Forexe/Executor/WorkflowExecutor.WicketMonitor.vb`, `src/KBot.Forexe/KBot.Forexe.vbproj`. (`Surse/SURSA_FOREXE` e instantaneu, nu se construiește: neatins.)

## Test results

`dotnet build src/KBot.App/KBot.App.vbproj -c Debug`: vezi mai jos; nimic rulat pe FOREXE.

## Unverified / deferred

- Pe paginile unde semnalul Wicket nu vine (ipoteză: lista de angajamente — scriptul din pagină probabil nu e activ în documentul curent, nevăzut), fiecare așteptare durează acum 10 s, nu 5: o paginare lungă e de două ori mai lentă. Repararea semnalului nu s-a făcut (necerută).
- Nicio operațiune git. Fără schimbare vizibilă operatorului: nimic în ajutor.

## Jurnale de diagnostic pentru așteptarea Wicket (20.11.2026, cererea operatorului)

Ipoteza «semnalul Wicket nu vine pe pagina listei» (fiecare pagină citită înainte a durat exact 5 s) nu era verificată. Acum fiecare `WaitForWicketIdleAsync` scrie în consolă (nivel info, ca restul `[ScrapeTable]`):

- `[Wicket] Aștept «gata» (plafon N ms). Robot: animlogo=…, statlogo=…, semnale primite de la pagină: N, ultimul: … (acum X ms). Pagina: doc=…, #statlogo=…, #animlogo=…, script=încărcat|ABSENT, observer=armat|NU, callback=function|undefined`
- `[Wicket] SEMNAL «gata» primit | PLAFON EXPIRAT, fără semnal «gata» după X ms. Pagina acum: …`
- la pornirea monitorului: `[Wicket] Monitor pornit acum: … documentul curent: …`

Cum se citesc: `script=ABSENT` sau `observer=NU` pe documentul curent = ipoteza e adevărată (scriptul s-a instalat doar pe documente încărcate după pornire). `animlogo=none (DEJA liber -> revine imediat)` = așteptarea revine înainte ca Ajax-ul să înceapă (semnal vechi, rămas de la pagina dinainte) — celălalt fel de a greși. `#animlogo=LIPSEȘTE` = pagina nu are elementul pe care îl urmărește monitorul. `semnale primite: 0` cu `script=încărcat` = callback-ul nu ajunge la K-BOT.

Fișiere: `WorkflowExecutor.WicketMonitor.vb` (câmpuri de contor, `ProbeWicketPageAsync`, jurnalele), `Services/JavaScripts/WicketMonitor.js` (`window._kbotWicketMonLoaded`, `window._kbotWicketMonArmed`). KBot.Forexe 1.0.26.0 → 1.0.27.0. Build curat; nerulat. Comportamentul așteptării nu s-a schimbat (doar plafonul de 10 s). De scos / redus la nivel debug după ce cauza e găsită.
