---
id: contabil.vederi.sumar
title: Sumar
part: contabil
order: 10
parent: contabil.vederi
screens: SumarView, SumarPartnersForm
keywords: sumar, indicatori, credit bugetar, totaluri, parteneri, asociere, partener
open: view:sumar
---
<!-- slice: 0011, 0000-14, 0108 -->
Prima privire asupra angajamentului.

<!-- capture: sumar | caption: Vederea «Sumar» | goto: view:sumar | prepare: Selectați un angajament cu mai mulți indicatori. | redo: 2026-10-17 20:00 | why: 0114-06/14: ribbonul are in dreapta grupul machetei (langa «Inchide») si grupul vederii; meniul din subsolul listei are doua randuri; E-Factura sta sub Extrase in bara vederilor; 0114-05: arborii nu mai au iconita de descarcare a extraselor (cea cu cardul din lista angajamentelor a plecat; descarcarea e butonul din ribbon) -->

**Sus**, datele angajamentului: codul, data din FOREXE, data creării și a definitivării, starea,
dacă a fost încărcat / preluat și descrierea.

**Încărcat** = angajamentul a fost trimis în FOREXE din K-BOT (sau din vechiul program); **preluat** =
a venit în K-BOT dintr-o descărcare din FOREXE.

**Jos**, câte un rând pentru fiecare **indicator** (clasificație) al angajamentului: codul
indicatorului, creditul bugetar și totalurile de rezervări, recepții, plăți, revizii DDF și
ordonanțări. **Creditul bugetar** este cel al clasificației, așa cum l-a raportat FOREXE: aceeași valoare
pe toate angajamentele care folosesc clasificația.

Coloanele tabelului se pot sorta și filtra din **pâlnia** capului de coloană; rândul de jos,
**TOTALURI**, însumează ce se vede — [Arborii și tabelele](topic:contabil.liste).

## Asociază parteneri
<!-- slice: 0084-02, 0084-03, 0084-04, 0114-02, 0114-03, 0000-28, 0000-30, 0000-84, 0000-85, 0113 -->
Butonul **Asociază parteneri** stă în **ribbon**, în grupul **«Sumar»** din dreapta, înaintea lui «Închide» (despărțit de el printr-o linie subțire), și se vede **doar cât e pe ecran vederea «Sumar»**. Partenerii se asociază documentului de fundamentare (DDF; îl faci din vederea «Rezervări», cu semnul «+»): fără document, butonul e stins (gri).

Se deschide o fereastră de dialog (popup) peste «Sumar», care arată partenerii deja asociați. Cât timp e deschisă,
restul aplicației așteaptă. Alegi un partener din listă (scrii
începutul numelui sau al codului fiscal) și apeși **Asociază**; apare în tabel cu rolul «De adăugat».
Poți alege câți parteneri ai nevoie. **Salvează** îi trimite pe toți odată și închide fereastra; **Renunță**
(sau Esc) o închide fără să asocieze nimic și te întoarce la «Sumar».

- Un partener deja asociat nu mai apare în listă și nu se adaugă a doua oară.
- De aici partenerii doar se adaugă: cei deja asociați rămân. Pe unul ales și încă nesalvat îl poți
  scoate cu **Scoate din asociere**. Un partener deja salvat se scoate din editorul DDF, de pe pagina
  «Parteneri» — [Editorul DDF](topic:contabil.ddf.editor).
- Partenerul principal al documentului, cel din antetul lui, se schimbă tot din editor.
