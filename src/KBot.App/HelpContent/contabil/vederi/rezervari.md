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
<!-- slice: 0014, 0061-02, 0092, 0092-01, 0000-68 -->
Rezervările de credite ale angajamentului, pe luni și zile.

<!-- capture: rezervari | caption: Vederea «Rezervări» | goto: view:rezervari | prepare: Selectați un angajament cu mai multe rezervări. -->

- **Arborele**: lunile, apoi zilele cu tipul rezervării (inițială, modificare...). Rezervarea
  inițială apare o singură dată, în ziua în care a devenit definitivă.
- **Grila**: câte un rând pe clasificație, cu creditul bugetar și valorile rezervării (inițială,
  valoare, definitivă).
- Semnul **«+»** de pe o rezervare pentru care nu există încă document de fundamentare pornește
  documentul pentru ea — vezi [Documentul de fundamentare](topic:contabil.ddf).

## Iconițele arborelui
<!-- slice: 0060, 0061-02, 0081-02, 0092-01, 0000-68, 0113, 0113-01 -->

| Unde | Ce face |
|------|---------|
| capul arborelui, dreapta | **Graficul rezervărilor**: evoluția rezervărilor și totalurile pe luni, într-o fereastră a lui, mai mică decât spațiul de lucru; «Închide» te întoarce la «Rezervări» |
| subsol, stânga | **acțiunile documentului de fundamentare** pe acest angajament (Adaugă rezervare / Definitivează / Derulează / Generează PDF final) — vezi [Adaugă rezervare, Definitivează, Derulează](topic:contabil.ddf.rezervare); iconița apare doar când în arbore nu mai e nicio rezervare cu «+» (de adus din FOREXE în K-BOT) |
| subsol, dreapta | **Repară rezervările** angajamentului, din istoric, fără nicio descărcare din FOREXE; iconița apare doar când angajamentul are rezervări greșite — vezi mai jos |

## Rezervări greșite și repararea lor
<!-- slice: 0092-01, 0000-68 -->

Dacă o lună a fost calculată de la valorile altei luni (de pildă o lună în care rezervările pornesc
de la totalul de acum două luni, ca și cum luna dinainte nu ar fi existat), valorile rezervărilor
din ea și din cele de după ea ies greșite.

- În lista principală, angajamentul cu rezervări greșite se scrie cu **roșu**. Ținând mouse-ul pe
  rând vezi, pentru fiecare rezervare greșită, ziua, indicatorul, valoarea anterioară care e acum
  și cea care ar trebui să fie. Dacă are și recepții cu lanțul neînchis, apar ambele mesaje.
- În vederea «Rezervări» apare, în dreapta-jos a arborelui, iconița cu **roata dințată** («Repară»); ea se vede **doar** cât timp angajamentul are rezervări greșite. Apas-o. K-BOT îți arată întâi câte rânduri și câte rezervări se schimbă, cu câteva exemple, și
  abia după ce confirmi scrie corecturile. **Nu se descarcă nimic din FOREXE.**
- Repararea **nu se face** dacă ar atinge o lună care are **revizii** de fundamentare: K-BOT spune ce lună e și nu schimbă nimic.
- După reparare, rândul din lista principală nu mai este roșu și iconița dispare.

<!-- capture: grafic-rezervari | caption: Graficul rezervărilor | goto: view:rezervari | prepare: Apăsați iconița din capul arborelui de rezervări, ca să se deschidă graficul. | redo: 2026-10-16 12:00 | why: 0113-01: graficul e o fereastră cu bara lui de titlu și cu «Închide», mai mică decât spațiul de lucru, nu o pagină a ferestrei principale -->
