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
<!-- slice: 0080-02, 0080-03, 0113, 0114, 0114-05, 0000-89, 0114-08, 0114-09, 0000-90 -->
Extrasele de cont (SNM) se văd în două locuri:

- vederea **Extrase** — doar operațiunile angajamentului selectat;
- macheta **Extrase de cont** — toate extrasele unității, cu sau fără angajament. Se deschide, ca pagină a
  ferestrei principale, din ribbon, **Extrase › Documente › «Extrase»**. O închizi cu «Închide» din ribbon sau cu Esc.

<!-- capture: extrase-fereastra | caption: Macheta «Extrase de cont» | goto: menu:extrase | prepare: Alegeți în arbore o lună cu extrase. | redo: 2026-10-17 20:00 | why: 0114-06/14: ribbonul are in dreapta grupul machetei (langa «Inchide») si grupul vederii; meniul din subsolul listei are doua randuri; E-Factura sta sub Extrase in bara vederilor; 0114-05: arborii nu mai au iconita de descarcare a extraselor (cea cu cardul din lista angajamentelor a plecat; descarcarea e butonul din ribbon); 0113: «Extrase de cont» e o pagină a ferestrei principale, fără bara ei de titlu și fără «Închide» -->

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
<!-- slice: 0057, 0080-03, 0114-05, 0000-89, 0114-08, 0114-09, 0114-07, 0000-90 -->

Extrasele noi se aduc din FOREXE cu butonul **«Descarcă extrase»** din ribbon, în **grupul din dreapta, lângă «Închide»**
(se conectează întâi, dacă nu ești conectat). Butonul e în grupul **macheta** de pe ecran: la **lista angajamentelor** (cu toate
vederile ei) și la **macheta «Extrase de cont»**; pe celelalte pagini nu se vede. Arborii nu mai au iconiță de descărcare,
iar macheta «Extrase de cont» nu mai are butonul ei de jos. După descărcare, extrasele deschise se reîncarcă.

## Ce coloane se văd
<!-- slice: 0080-02 -->

Coloanele fiecărei grile și ordinea lor se aleg în **Setări › Extrase**, separat pentru vedere
și pentru fereastră. «Revino la implicit» le readuce la forma inițială.
