# SLICE 0091 — Detaliul recepției citit tăiat din FOREXE; viteza internetului și timpii de așteptare

**Data:** 29.09.2026
**Cerut de operator**, pornind de la rularea 34 (017_SCNB, AAB2DH3X6SK), respinsă de F14 și apoi de F15.
**Cerut explicit:** fără teste. Nicio suită rulată, niciun cod de test scris; doar build pe `src`.

---

## Ce s-a găsit (cu dovadă)

Pachetul `Surse/20260929_114818_871_PrelucrareCompleta_AAB2DH3X6SK.json`, `ListaReceptii_results`:
8 din 11 recepții au `Detaliu` complet (7 indicatori, fiecare cu Credit / Valoare nerecepționată /
Valoare). Trei sunt tăiate, mereu în ordinea de pe site (AA6, AA7, AA3, AA4, AA5, AAB, AA2):

| rând | recepția | ce a venit |
|---|---|---|
| 1 | 13.01.2026 · 16,00 | AA6…AA5, apoi AAB tăiat în celulă (`"01A- 65. 03. 02."`), fără valori; AA2 lipsă |
| 3 | 12.03.2026 · 286.417,00 | doar AA6, tăiat în celulă, fără valori |
| 7 | 26.06.2026 · 301,00 | AA6, AA7, AA3; lipsesc AA4, AA5, AAB, AA2 |

Robotul a citit pagina recepției în timp ce FOREXE încă o trimitea. Pasul 4b a scris ciotul în
`RHR` (care doar crește), iar F14/F15 au refuzat corect asocierile. Motivul exact pentru care
așteptarea nu a prins încărcarea NU e dovedit.

## Ce s-a schimbat

### Server (`PYTHON/routes/forexe/`)
- `prelucrare_pasi.py`: `detaliu_incomplet(detalii, suma, nr_indicatori)`. Trei semne: un rând fără
  valori; liniile nu dau `Suma`; mai puțini indicatori decât are angajamentul (presupunere, vezi mai
  jos). În pasul 4b, o recepție **existentă** cu detaliu tăiat rămâne neatinsă (antet și `RHR`);
  una **nouă** se creează, dar doar cu rândurile întregi. Fiecare ajunge în lista `incomplete` și în jurnal.
- `prelucrare.py`: lista circulă prin `_ruleaza_pasii`; fiecare recepție primește
  `detaliu_incomplet`; ambele faze întorc `receptii_incomplete` (`data_r` zz.ll.aaaa, `suma`, `motiv`).
- `prelucrare_asociere.py`: `aplica_decizii` / `valideaza_plasarile` primesc `detaliu_incomplet`
  (IDRR-uri); pe acele recepții F14 și F15 doar semnalează. F16 rămâne veto.
  `F14_PAUSED = True`: pauza pe F14 cerută de operator în aceeași zi (în ingestie și în editorul de oricând).

### Client
- `KBot.Domain/AsociereInfo.vb`: `PrelucrarePropunere.ReceptiiIncomplete`, `ReceptieIncompleta`,
  `ReceptiePropusa.DetaliuIncomplet`. `KBot.Api`: DTO-urile și citirea lor.
- `AsociereForm.vb`: vetoul F14 din client, oprit (`F14Paused`) și sărit pe recepțiile tăiate.
- `KbotForm.Ingest.vb`: `OferaReimprospatareaReceptiilorAsync`. După salvare, dacă au existat
  recepții tăiate, operatorul le vede (dată · sumă · motiv) și e întrebat «Le citesc din nou acum,
  doar pe ele?». La «Da» se citesc numai acele zile: toate celelalte zile locale intră în lista de sărit
  din felia 0060. Dacă a doua citire vine iar tăiată, întrebarea revine.
- `KBot.Forexe/Executor/WorkflowExecutor.Pacing.vb` (nou):
  - `TimeoutMultiplier` înmulțește o singură dată, pe fiecare acțiune, `Timeout` și `Wait.Seconds`,
    plus plafonul așteptării Ajax (`WaitForWicketIdleAsync`). Fișierele WFL nu se ating.
  - `ValidateReadsTwice`: fiecare pagină `ScrapeTable` se citește până când două citiri
    consecutive ies la fel (așteptare Ajax + 750 ms × multiplicator între ele, maximum 4 citiri).
- `ForexeRunner.ApplyPacing` le pune pe amândouă din setări, la conectare și înaintea fiecărei lucrări.
- `KBot.Forexe/Services/ForexeSpeedTest.vb` (nou): Chromium ascuns pe fast.com, așteaptă
  `#speed-value.succeeded`, citește valoarea și unitatea și le convertește în Mb/s.
- `AppSettings`: `ForexeTimeoutMultiplier`, `ForexeValidateTwice`, `ForexeSpeedMbps`,
  `ForexeSpeedTestedAt`; `ForexeValidateTwiceInEffect` = bifat **și** ultimul test sub 20 Mb/s.
- `SetariForexeView` (+ Designer): secțiunea nouă «Viteza internetului și așteptările», cu
  «Testează viteza», bifa «Validează de 2×» (activă doar pe o conexiune lentă) și lista «Timpii de
  așteptare din WFL» (×1 / ×1,5 / ×2 / ×3).
- `docs/SETARI_UTILIZATOR.md`: cele patru chei noi.

## Fișiere atinse
PYTHON/routes/forexe/prelucrare.py · prelucrare_pasi.py · prelucrare_asociere.py ·
src/KBot.Domain/AsociereInfo.vb · src/KBot.Api/ApiClient.vb · src/KBot.Api/UpsertAngajamenteRequest.vb ·
src/KBot.Common/AppSettings.vb · src/KBot.Forexe/ForexeRunner.vb ·
src/KBot.Forexe/Executor/WorkflowExecutor.Pacing.vb (nou) · WorkflowExecutor.Actions.vb ·
WorkflowExecutor.WicketMonitor.vb · Actions/WorkflowExecutor.Actions.ScrapeTable.vb ·
src/KBot.Forexe/Services/ForexeSpeedTest.vb (nou) · src/KBot.App/KbotForm.Ingest.vb ·
src/KBot.App/Forexe/AsociereForm.vb · src/KBot.App/Setari/SetariForexeView.vb + .Designer.vb ·
docs/SETARI_UTILIZATOR.md · docs/worklog/KBOT_STATUS.md · state/KBOT_STATUS_0090-0099.md · state/KBOT_STATUS_SLICELESS.md

## Rezultate
- `dotnet build src/KBot.App/KBot.App.vbproj`: 0 erori, 0 avertismente.
- Python: `py_compile` pe cele trei fișiere, fără erori.
- **Teste: niciunul rulat** (cererea operatorului). Nimic nu a rulat pe ecran, nimic pe serverul real.

## Neverificat / amânat
- Semnul 3 se sprijină pe un singur angajament: presupune că FOREXE listează toți indicatorii pe
  fiecare recepție. Dacă presupunerea e greșită, recepțiile acelea vor fi semnalate la fiecare rulare.
- Testul de viteză depinde de marcajul paginii fast.com.
- `test_forexe_prelucrare_pasi.py` apelează `step4b_receptii_prelucrare` cu trei argumente
  poziționale; noul parametru e opțional, deci apelurile rămân valide. Suita nu s-a rulat.
- Codul Python trebuie urcat pe VPS, iar clientul publicat.
- F14 rămâne în pauză până decide operatorul.
