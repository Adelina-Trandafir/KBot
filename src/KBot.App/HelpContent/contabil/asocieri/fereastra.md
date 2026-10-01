---
id: contabil.asocieri.fereastra
title: Fereastra Asocieri, pe părți
part: contabil
order: 10
parent: contabil.asocieri
screens: AsociereForm.treeLant, AsociereForm.gridLant, AsociereForm.pnlGrafice, AsociereForm.grafic, AsociereForm.benzi, AsociereForm.navGrafice, AsociereForm.btnGrafice, GraficeAsociereForm, AsociereBenziForm
keywords: fereastra asocieri, receptii si lanturile lor, instantanee neasezate, grafic, distributie, benzi, evolutia valorii, plata, total receptii la data platii
---
<!-- slice: 0048-04, 0000-12 -->
<!-- capture: asocieri-fereastra | caption: Fereastra Asocieri, cu recepțiile în stânga și instantaneele neașezate în dreapta | goto: view:receptii | prepare: Selectați un angajament cu mai multe recepții modificate, apoi apăsați iconița din dreapta, sus, a arborelui de recepții. -->

## Stânga — «RECEPȚII ȘI LANȚURILE LOR»
<!-- slice: 0048-04, 0056, 0056-02, 0059, 0062, 0065 -->

Câte un rând pe **recepție**: data recepției, valoarea de azi și, în paranteză, câte instantanee are
lanțul ei. Sub fiecare recepție, **lanțul**: instantaneele ei, în ordinea orei.

Semnele de pe rândul recepției:

| Semn | Înseamnă |
|------|----------|
| **[ștearsă]** (și fond stins) | recepția a fost ștearsă pe site; rămâne o recepție întreagă pentru plățile de dinaintea ștergerii |
| **[reconstituită]** | recepția n-a mai existat în FOREXE la prima descărcare și a fost refăcută din instantaneele ei |
| **[nouă]** / **[nouă, ștearsă]** | recepție pornită de tine chiar acum, din coș, încă nescrisă |

Ținând mouse-ul pe o recepție afli ultimul instantaneu și, dacă e cazul,
**«⚠ Lanțul nu se închide: ultimul instantaneu nu are valoarea de acum»** — semn că lipsește ceva din
lanț sau că e ceva în plus.

Semnele de pe rândul unui instantaneu: data și ora salvării, totalul, plus **[ștergere]** (rândul în
care recepția a fost ștearsă, scris înclinat) sau **[fără schimbare]**. Un instantaneu scris stins,
cu lacăt, este **blocat**: angajamentul are o ordonanțare din ziua lui sau de după ea; se vede, dar
nu se mai poate muta. Toate celelalte se pot muta — și după o descărcare, inclusiv cele care erau
așezate dinainte ([Legături blocate](topic:contabil.asocieri.cazuri)).

**Grila de sub arbore** arată indicatorii instantaneului (sau ai recepției) aleși: indicator, cod SSI,
credit bugetar, valoare.

## Dreapta — «INSTANTANEE NEAȘEZATE» (coșul)
<!-- slice: 0048-04, 0061 -->

Instantaneele care încă nu au recepție. Grila de sub ele arată indicatorii instantaneului ales.
Aici tragi înapoi un instantaneu ca să-l desprinzi de recepția lui.

## Jos — graficele
<!-- slice: 0048-05, 0048-06, 0048-08, 0048-09, 0061 -->

Două vederi, alese din dreapta:

- **Grafic** — «EVOLUȚIA VALORII»: fila **Recepția** arată lanțul recepției alese, fila **Tot
  angajamentul** câte o linie pe recepție, fiecare în culoarea ei (aceeași culoare o are și rândul
  recepției din stânga). Un clic pe un punct alege instantaneul lui în arbore.
- **Distribuție** — «AȘEZAREA INSTANTANEELOR»: câte o **bandă** pe recepție, cu un marcaj pe fiecare
  instantaneu, iar jos banda celor neașezate. Marcajele se pot trage și ele.

<!-- capture: asocieri-grafic | caption: Graficul «Tot angajamentul», cu reperele plăților | goto: view:receptii | prepare: Deschideți Asocieri, alegeți «Grafic» și fila «Tot angajamentul», apoi țineți mouse-ul pe un reper de plată. -->

**Reperele verticale sunt plățile.** Ținând mouse-ul pe unul vezi socoteala ordonanțării de atunci:
plata (data, suma), **«Total recepții la data plății»**, **«Plăți anterioare»** și **«Diferență
(recepții - plăți)»**. Dacă până la acea dată mai sunt instantanee neașezate, eticheta o spune:
totalul nu le cuprinde. E cea mai rapidă verificare: **dacă diferența iese negativă sau ciudată, un
instantaneu stă, probabil, pe recepția greșită.**

**«Grafice și benzi»** deschide graficul și benzile într-o fereastră separată, pe care o poți mări cât
ecranul — bună la angajamentele cu multe recepții.

<!-- capture: asocieri-grafice | caption: Fereastra «Grafice și benzi» | goto: view:receptii | prepare: Deschideți Asocieri, apoi apăsați «Grafice și benzi». -->

## Butoanele
<!-- slice: 0048-04, 0058, 0056-02 -->

| Buton | Ce face |
|-------|---------|
| **Salvează legăturile** | scrie ce ai hotărât. După o descărcare se aprinde abia când **fiecare** instantaneu adus de descărcare are o hotărâre; legăturile vechi se scriu doar dacă le-ai mutat. |
| **Golește așezările** | doar după o descărcare: pune înapoi în coș tot ce a adus descărcarea și anulează marcajele tale; legăturile vechi pe care le-ai mutat revin la cum sunt pe server |
| **Renunță** | închide fără să scrie nimic (vezi mai jos ce înseamnă după o descărcare) |

Banda de mesaje de sus spune mereu cât mai e de făcut, de exemplu «Mai sunt 3 instantanee
neașezate...» sau «Toate instantaneele au primit o hotărâre — poți salva descărcarea.»
