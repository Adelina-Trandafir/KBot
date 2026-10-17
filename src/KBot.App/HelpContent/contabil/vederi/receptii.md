---
id: contabil.vederi.receptii
title: Recepții
part: contabil
order: 40
parent: contabil.vederi
screens: ReceptiiView
keywords: receptii, factura, facturi, antet, diferenta, asocieri, legaturi, instantanee
open: view:receptii
---
<!-- slice: 0015, 0015-03, 0065, 0114-16, 0000-91 -->
Recepțiile angajamentului, cu detaliul lor pe clasificații.

<!-- capture: receptii | caption: Vederea «Recepții» | goto: view:receptii | prepare: Selectați un angajament cu recepții în mai multe luni. | redo: 2026-10-17 21:00 | why: 0114-16: butoanele vederii (Asociaza receptii, Reface din istoric) sunt in ribbon; in arbore a ramas doar «Actualizeaza»; 0114-06/14: ribbonul are in dreapta grupul machetei (langa «Inchide») si grupul vederii; meniul din subsolul listei are doua randuri; E-Factura sta sub Extrase in bara vederilor; 0114-05: arborii nu mai au iconita de descarcare a extraselor (cea cu cardul din lista angajamentelor a plecat; descarcarea e butonul din ribbon) -->

- **Arborele** are trei niveluri: **luna** › **recepția** › **antetul** recepției.
- **Grila** arată, pentru nodul ales (oricare nivel), un rând «Toți indicatorii» și câte un rând
  pe clasificație, cu denumirea ei.
- **Eticheta la survolare** pe o lună sau pe o recepție arată cât s-a recepționat până acolo,
  cât s-a plătit și **diferența** (roșu dacă s-a plătit mai mult decât s-a recepționat). La o lună
  se numără plățile de până la prima recepție din luna următoare.

<!-- capture: receptii-eticheta | caption: Eticheta cu diferența recepții – plăți | goto: view:receptii | prepare: Țineți mouse-ul pe o lună din arborele de recepții până apare eticheta. | redo: 2026-10-17 20:00 | why: 0114-06/14: ribbonul are in dreapta grupul machetei (langa «Inchide») si grupul vederii; meniul din subsolul listei are doua randuri; E-Factura sta sub Extrase in bara vederilor; 0114-05: arborii nu mai au iconita de descarcare a extraselor (cea cu cardul din lista angajamentelor a plecat; descarcarea e butonul din ribbon) -->

## Asocierile, oricând
<!-- slice: 0048-04, 0114-16, 0000-91 -->

Butonul **«Asociază recepții»** din ribbon (grupul **«Recepții»**, în dreapta, cât vederea e pe ecran) deschide fereastra **Asocieri** pentru angajamentul
selectat, ca să corectezi unde e așezat fiecare instantaneu din istoric față de recepții — vezi
[Asocierile recepțiilor](topic:contabil.asocieri) și [Cum așezi instantaneele](topic:contabil.asocieri.pasi).

> Legăturile pe care se sprijină deja ordonanțări sau plăți **se văd, dar nu se pot schimba**.

După salvare, recepțiile se reîncarcă.

## Subsolul arborelui și ribbonul
<!-- slice: 0060, 0062, 0000-14, 0114-16, 0000-91 -->

- **«Actualizează»** (subsol, dreapta) — reîmprospătează recepțiile din FOREXE; întâi alegi care, în fereastra «Ce recepții
  reîmprospătez?» — [Descărcarea unui angajament](topic:contabil.forexe.descarcare). E singurul buton rămas în arbore.
- **«Reface din istoric...»** (ribbon, grupul «Recepții») — reface din istoric instantaneele și liniile de recepție care lipsesc. Apare **doar** când angajamentul are erori de reparat: o recepție al cărei lanț nu se închide (rândul lui e scris cu roșu în lista principală). K-BOT numără întâi ce lipsește și scrie doar după ce confirmi.

În capul arborelui: **lupa** caută. Butoanele comune: [Arborii și tabelele](topic:contabil.liste).
