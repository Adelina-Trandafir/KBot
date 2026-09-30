---
id: contabil.vederi.rezervari
title: Rezervări
part: contabil
order: 30
parent: contabil.vederi
screens: RezervariView, GraficRezervariForm
keywords: rezervari, rezervare initiala, rezervare definitiva, grafic, plus
open: view:rezervari
---
<!-- slice: 0014, 0061-02, 0092 -->
Rezervările de credite ale angajamentului, pe luni și zile.

<!-- capture: rezervari | caption: Vederea «Rezervări» | goto: view:rezervari | prepare: Selectați un angajament cu mai multe rezervări. -->

- **Arborele**: lunile, apoi zilele cu tipul rezervării (inițială, modificare...). Rezervarea
  inițială apare o singură dată, în ziua în care a devenit definitivă.
- **Grila**: câte un rând pe clasificație, cu creditul bugetar și valorile rezervării (inițială,
  valoare, definitivă).
- Semnul **«+»** de pe o rezervare pentru care nu există încă document de fundamentare pornește
  documentul pentru ea — vezi [Documentul de fundamentare](topic:contabil.ddf).

## Iconițele arborelui
<!-- slice: 0060, 0061-02, 0081-02 -->

| Unde | Ce face |
|------|---------|
| capul arborelui, dreapta | **Graficul rezervărilor**: evoluția rezervărilor și totalurile pe luni, într-o fereastră separată |
| subsol, stânga | **acțiunile documentului de fundamentare** pe acest angajament (Adaugă rezervare / Definitivează / Derulează / Generează PDF final) — vezi [Adaugă rezervare, Definitivează, Derulează](topic:contabil.ddf.rezervare) |
| subsol, dreapta | reîmprospătează din FOREXE doar rezervările (antet, indicatori, istoric) |

<!-- capture: grafic-rezervari | caption: Graficul rezervărilor | goto: view:rezervari | prepare: Apăsați iconița din capul arborelui de rezervări, ca să se deschidă graficul. -->
