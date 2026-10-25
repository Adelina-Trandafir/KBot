# SLICE-0097-06 — Sursa «Toate» în bara de titlu și mesaj de perioadă pe tot aplicația

Cererea operatorului, 25.10.2026. Slice-ul 0097-03 a mutat anul și sursa/sectorul în bara de titlu; numărul
0097-06 este ultimul sub-slice liber al lui 0097 (cerut de operator).

## What changed and why

Până acum selectorul Sursă/Sector era legat de arborele din fereastra principală: la sortarea după dată se
ascundea, iar arborele aducea toate sursele. Operatorul vrea altfel (mai multe machete vor răspunde la sursă,
nu doar fereastra principală):

- **Alegerea «Toate»** (cheia `*`, aceeași pe care o trimite cererea de arbore pentru toate sursele) este primul
  rând al selectorului.
- **Mesaj pe tot aplicația:** `SessionContext.PeriodChanged` (cu `PeriodChangedEventArgs`: an, sursă, program,
  «Toate»). Se ridică din `SetPeriod` doar când ceva s-a schimbat, pe firul UI. Orice machetă se abonează la el.
- **Sesiunea păstrează mereu o sursă și un program concrete** (robotul FOREXE și documentele au nevoie de ele);
  `SessionContext.ToateSursele` spune doar că lista nu e filtrată. «Toate» nu se salvează ca `LastSS`.
- **Arborele:** selectorul nu se mai ascunde la sortare (doar cât e aleasă o grupă). La fiecare schimbare a
  sortării (după nume sau după dată) selectorul trece pe «Toate» și arborele se reîncarcă. O sursă aleasă după
  aceea aduce doar angajamentele ei, în ordinea sortării.
- **Descărcarea listei din FOREXE** (`ForexeController`) întreabă `ToateSursele`, nu sortarea după dată.
- **Schimbarea anului:** «Toate» rămâne «Toate»; la prima citire, «Toate» este alegerea când sortarea e după dată.
- Parametrii noi sau atinși din `SessionContext.SetPeriod` / `PeriodChangedEventArgs` au prefix `k_`.
- Ajutor: `contabil/fereastra.md` (secțiunile «Lista angajamentelor» și «Anul și sursa/sectorul»),
  `tours/tur-fereastra.md` (pasul «Sursa și sectorul»), `contabil/forexe/lista.md` — toate cu tag `0097-06`.

Interpretare (de confirmat): „la fel și la ordonare nume" = și la trecerea pe sortarea după nume selectorul
trece pe «Toate».

## Files touched

- `src/KBot.Common/SessionContext.vb`
- `src/KBot.App/KbotForm.Periods.vb`
- `src/KBot.App/KbotForm.Tree.vb`
- `src/KBot.App/KbotForm.TreeOptions.vb`
- `src/KBot.App/Forexe/ForexeController.vb`
- `src/KBot.App/HelpContent/contabil/fereastra.md`, `tours/tur-fereastra.md`, `contabil/forexe/lista.md`

## Test results

`dotnet build src/KBot.App/KBot.App.vbproj`: 0 warnings, 0 errors. Netestat, nevăzut pe ecran.

## Unverified / deferred

- Nerulat pe ecran: nu s-a văzut «Toate» în selector, trecerea pe «Toate» la sortare, nici filtrarea pe o sursă.
- Nicio altă machetă nu e abonată încă la `PeriodChanged`; evenimentul e pregătit.
- Starea de pe server a `LastSS` nu se schimbă la «Toate» (rămâne ultima sursă concretă).
- Rândul grupei active: cât e aleasă o grupă selectorul rămâne ascuns, ca înainte.
- `Check-Help.ps1` nerulat; nu s-au scris teste (regula proiectului).
- Nu s-a făcut nicio operațiune git (regula proiectului).
