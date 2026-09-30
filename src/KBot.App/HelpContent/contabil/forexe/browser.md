---
id: contabil.forexe.browser
title: Vederea «Browser FOREXE»
part: contabil
order: 40
parent: contabil.forexe
screens: BrowserView, IstoricIntervalForm
keywords: browser, pagina forexe, deschide angajament
---
Cât timp ești conectat la FOREXE, bara de vederi are **Browser FOREXE**: pagina FOREXE a
robotului, direct în fereastra K-BOT.

<!-- capture: browser-forexe | caption: Vederea «Browser FOREXE» cu un angajament deschis | goto: view:browser | prepare: Conectați-vă la FOREXE și selectați un angajament în listă. -->

- Când **selectezi un angajament** în listă cu vederea deschisă, robotul îl deschide singur în
  FOREXE (pagina «Modificare» a angajamentului) și se oprește acolo.
- Când **cauți și deschizi un angajament direct în pagină**, lista din K-BOT îl selectează și ea,
  fără să pornească robotul din nou.

> Pagina este FOREXE-ul real: ce salvezi aici se salvează în FOREXE.

## Ce face K-BOT după o salvare făcută în pagină

K-BOT urmărește pagina. Când salvezi în ea **un angajament nou**, **o recepție** sau **o
rezervare**, K-BOT preia singur modificarea, fără să mai apeși iconița de descărcare:

| Ce ai salvat | Ce face K-BOT |
|--------------|---------------|
| un angajament nou | reîmprospătează lista, apoi descarcă angajamentul nou |
| o recepție (nouă sau modificată) | descarcă acea recepție și istoricul |
| o rezervare | păstrează rândurile modificate și te întreabă «Ați terminat modificarea rezervărilor?» — **DA** le preia acum, **NU** te lasă să continui și te întreabă din nou după următoarea salvare |

După preluare (în afară de rezervări), se deschide fereastra **«Istoric angajament»**: doar
rândurile de istoric scrise de FOREXE în minutele în care ai lucrat, ca să verifici ce s-a
înregistrat. **«Tot istoricul»** scoate limita de timp.

<!-- capture: istoric-interval | caption: Fereastra «Istoric angajament» după o salvare în pagina FOREXE | prepare: Salvați o recepție în pagina FOREXE din vederea «Browser FOREXE» și așteptați să se deschidă fereastra de istoric. -->

Robotul lucrează la o singură preluare odată. Dacă salvezi altceva cât încă preia, K-BOT îți spune
și nu preia a doua operațiune: descarcă angajamentul din iconița lui după ce se termină.
