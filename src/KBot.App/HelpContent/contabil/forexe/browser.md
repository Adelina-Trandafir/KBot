---
id: contabil.forexe.browser
title: Vederea «Browser FOREXE»
part: contabil
order: 40
parent: contabil.forexe
screens: BrowserView, IstoricIntervalForm
keywords: browser, pagina forexe, deschide angajament, creeaza angajament in forexe, angajament nou in forexe, esti sigur, sunteti sigur, confirmare, receptie pe aceeasi data, doua receptii, mini-meniu, marire pagina, zoom
open: view:browser
---
<!-- slice: 0070, 0074 -->
Cât timp ești conectat la FOREXE, bara de vederi are **Browser FOREXE**: pagina FOREXE a
robotului, direct în fereastra K-BOT.

<!-- capture: browser-forexe | caption: Vederea «Browser FOREXE» cu un angajament deschis | goto: view:browser | prepare: Conectați-vă la FOREXE și selectați un angajament în listă. -->

- Când **selectezi un angajament** în listă cu vederea deschisă, robotul îl deschide singur în
  FOREXE (pagina «Modificare» a angajamentului) și se oprește acolo.
- Când **cauți și deschizi un angajament direct în pagină**, lista din K-BOT îl selectează și ea,
  fără să pornească robotul din nou.

> Pagina este FOREXE-ul real: ce salvezi aici se salvează în FOREXE.

## Ce face K-BOT după o salvare făcută în pagină
<!-- slice: 0073, 0076 -->

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

## Un angajament nou făcut direct în FOREXE
<!-- slice: 0097-02 -->

**MENIU › Adăugare angajamente... › Creează angajament în FOREXE** te duce la formularul
«Angajament nou» din pagina FOREXE:

1. în lista de angajamente nu mai rămâne nimic selectat;
2. se deschide vederea **Browser FOREXE**;
3. robotul duce pagina la formularul «Angajament nou» și se oprește acolo.

De aici completezi și salvezi tu, ca în FOREXE. După salvare, K-BOT preia singur angajamentul nou
(vezi tabelul de mai sus).

Dacă nu ești conectat la FOREXE, K-BOT îți cere întâi conectarea. Cât robotul lucrează la altceva,
aștepți să termine și alegi din nou.

## Întrebările «Sunteți sigur...?» din pagină
<!-- slice: 0097-02 -->

Când lucrezi tu în pagină și FOREXE te întreabă dacă ești sigur (de exemplu la «Renunță»), K-BOT
răspunde singur **«Da»**, iar fereastra cu întrebarea se închide.

Ferestrele în care ai **ceva de completat** (de exemplu motivul unei rezervări) rămân deschise:
le completezi și le confirmi tu.

## Două recepții pe aceeași dată
<!-- slice: 0097-02 -->

Când adaugi o **recepție nouă** în pagină și alegi o dată pe care angajamentul are deja o
recepție, K-BOT te întreabă — după ce ieși din câmpul datei sau, cel târziu, când apeși
«Salvează»:

> «Angajamentul are deja o recepție cu data ... NU este recomandat să aveți mai multe recepții
> pe aceeași dată. Continuați cu această dată?»

- **«Nu, schimb data»** te lasă în formular, cu cursorul în câmpul datei;
- **«Da, continui»** păstrează data; pentru data aceea nu mai ești întrebat a doua oară.

Poți salva și așa, dar e mai bine să alegi altă dată.

## Mini-meniul K-BOT din pagină
<!-- slice: 0073, 0097-02 -->

În colțul paginii FOREXE stă un mic meniu **K-BOT**: **−**, **+** și **100%** micșorează și
măresc pagina, iar rândul de sub ele spune dacă K-BOT urmărește o operațiune începută de tine.
Îl poți muta trăgându-l de titlu și îl poți strânge din săgeata de lângă titlu.

Dacă te încurcă, îl ascunzi din **Setări › FOREXE**: «Arată mini-meniul K-BOT în pagina
FOREXE...» — [Setări și aspect](topic:contabil.setari). K-BOT urmărește în continuare ce salvezi.
