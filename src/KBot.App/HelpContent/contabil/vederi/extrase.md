---
id: contabil.vederi.extrase
title: Extrase de cont
part: contabil
order: 60
parent: contabil.vederi
screens: ExtraseView, ExtraseForm, ExtrasePanel
keywords: extrase, snm, extras de cont, operatii, antet, platitor, cui
open: view:extrase
---
<!-- slice: 0080-02, 0080-03 -->
Extrasele de cont (SNM) se văd în două locuri:

- vederea **Extrase** — doar operațiunile angajamentului selectat;
- fereastra **Extrase de cont** — toate extrasele unității, cu sau fără angajament. Se deschide
  din **MENIU › Extrase** sau din iconița din stânga, jos, a listei de angajamente.

<!-- capture: extrase-fereastra | caption: Fereastra «Extrase de cont» | goto: menu:extrase | prepare: Alegeți în arbore o lună cu extrase. -->

## Arborele și cele două moduri de afișare
<!-- slice: 0096 -->

Arborele are «Toate extrasele», apoi lunile și zilele. Iconița din **capul arborelui** alege cum
se văd datele:

| Mod | Sus | Jos |
|-----|-----|-----|
| **Arată antet + operații** | anteturile extraselor perioadei alese | operațiunile antetului selectat |
| **Arată operații + detalii** | operațiunile perioadei alese | operațiunea selectată, întreagă |

În modul «operații + detalii», coloanele **Data bancă**, **Plătitor** și **CUI** se pot filtra și
grupa din meniul capului de coloană.

## Descărcarea extraselor
<!-- slice: 0057, 0080-03 -->

Iconița din **subsolul arborelui** descarcă extrasele din FOREXE (se conectează întâi, dacă nu
ești conectat). În fereastră există și butonul **«Descarcă extrasele din FOREXE»**.

## Ce coloane se văd
<!-- slice: 0080-02 -->

Coloanele fiecărei grile și ordinea lor se aleg în **Setări › Extrase**, separat pentru vedere
și pentru fereastră. «Revino la implicit» le readuce la forma inițială.
