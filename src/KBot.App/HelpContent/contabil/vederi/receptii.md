---
id: contabil.vederi.receptii
title: Recepții
part: contabil
order: 40
parent: contabil.vederi
screens: ReceptiiView
keywords: receptii, antet, diferenta, asocieri, legaturi, instantanee
---
<!-- slice: 0015, 0015-03, 0065 -->
Recepțiile angajamentului, cu detaliul lor pe clasificații.

<!-- capture: receptii | caption: Vederea «Recepții» | goto: view:receptii | prepare: Selectați un angajament cu recepții în mai multe luni. -->

- **Arborele** are trei niveluri: **luna** › **recepția** › **antetul** recepției.
- **Grila** arată, pentru nodul ales (oricare nivel), un rând «Toți indicatorii» și câte un rând
  pe clasificație, cu denumirea ei.
- **Eticheta la survolare** pe o lună sau pe o recepție arată cât s-a recepționat până acolo,
  cât s-a plătit și **diferența** (roșu dacă s-a plătit mai mult decât s-a recepționat). La o lună
  se numără plățile de până la prima recepție din luna următoare.

<!-- capture: receptii-eticheta | caption: Eticheta cu diferența recepții – plăți | goto: view:receptii | prepare: Țineți mouse-ul pe o lună din arborele de recepții până apare eticheta. -->

## Asocierile, oricând
<!-- slice: 0048-04 -->

Iconița din **dreapta, sus**, a arborelui deschide fereastra **Asocieri** pentru angajamentul
selectat, ca să corectezi unde e așezat fiecare instantaneu din istoric față de recepții — vezi
[Asocierile recepțiilor](topic:contabil.asocieri) și [Cum așezi instantaneele](topic:contabil.asocieri.pasi).

> Legăturile pe care se sprijină deja ordonanțări sau plăți **se văd, dar nu se pot schimba**.

După salvare, recepțiile se reîncarcă.

## Subsolul arborelui
<!-- slice: 0060, 0062, 0000-14 -->

- **dreapta** — reîmprospătează recepțiile din FOREXE; întâi alegi care, în fereastra «Ce recepții
  reîmprospătez?» — [Descărcarea unui angajament](topic:contabil.forexe.descarcare).
- **stânga** — reface din istoric instantaneele și liniile de recepție care lipsesc.

În capul arborelui: **lupa** caută, iar iconița din dreapta deschide fereastra Asocieri (mai sus).
Butoanele comune: [Arborii și tabelele](topic:contabil.liste).
