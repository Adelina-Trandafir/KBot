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
<!-- slice: 0014, 0061-02, 0092, 0092-01, 0000-68, 0114-15, 0000-91 -->
Rezervările de credite ale angajamentului, pe luni și zile.

<!-- capture: rezervari | caption: Vederea «Rezervări» | goto: view:rezervari | prepare: Selectați un angajament cu mai multe rezervări. | redo: 2026-10-17 21:00 | why: 0114-15: butoanele vederii (Grafice, document, Repara) sunt in ribbon, nu in arbore; 0114-06/14: ribbonul are in dreapta grupul machetei (langa «Inchide») si grupul vederii; meniul din subsolul listei are doua randuri; E-Factura sta sub Extrase in bara vederilor; 0114-05: arborii nu mai au iconita de descarcare a extraselor (cea cu cardul din lista angajamentelor a plecat; descarcarea e butonul din ribbon) -->

- **Arborele**: lunile, apoi zilele cu tipul rezervării (inițială, modificare...). Rezervarea
  inițială apare o singură dată, în ziua în care a devenit definitivă.
- **Grila**: câte un rând pe clasificație, cu creditul bugetar și valorile rezervării (inițială,
  valoare, definitivă).
- Semnul **«+»** de pe o rezervare pentru care nu există încă document de fundamentare pornește
  documentul pentru ea — vezi [Documentul de fundamentare](topic:contabil.ddf).

## Butoanele din ribbon
<!-- slice: 0060, 0061-02, 0081-02, 0092-01, 0000-68, 0113, 0113-01, 0114-15, 0000-91 -->

Arborele nu mai are iconițe în cap sau în subsol (a rămas doar «+» de pe rezervări). Cât timp vederea «Rezervări» e pe ecran, ribbonul are, în dreapta, grupul **«Rezervări»** (lângă grupul «Lista angajamentelor»), cu butoanele:

| Buton | Ce face |
|-------|---------|
| **Grafice** | **Graficul rezervărilor**: evoluția rezervărilor și totalurile pe luni, într-o fereastră a lui, mai mică decât spațiul de lucru; «Închide» te întoarce la «Rezervări». E stins cât angajamentul nu are rezervări |
| **Adaugă rezervare** (își schimbă numele) | **acțiunile documentului de fundamentare** pe acest angajament (Adaugă rezervare / Definitivează / Derulează / Generează PDF final) — vezi [Adaugă rezervare, Definitivează, Derulează](topic:contabil.ddf.rezervare); apare doar când în arbore nu mai e nicio rezervare cu «+» (de adus din FOREXE în K-BOT) și are ce să facă |
| **Repară...** | **Repară rezervările** angajamentului, din istoric, fără nicio descărcare din FOREXE; apare doar când angajamentul are rezervări greșite — vezi mai jos |

## Rezervări greșite și repararea lor
<!-- slice: 0092-01, 0000-68 -->

Dacă o lună a fost calculată de la valorile altei luni (de pildă o lună în care rezervările pornesc
de la totalul de acum două luni, ca și cum luna dinainte nu ar fi existat), valorile rezervărilor
din ea și din cele de după ea ies greșite.

- În lista principală, angajamentul cu rezervări greșite se scrie cu **roșu**. Ținând mouse-ul pe
  rând vezi, pentru fiecare rezervare greșită, ziua, indicatorul, valoarea anterioară care e acum
  și cea care ar trebui să fie. Dacă are și recepții cu lanțul neînchis, apar ambele mesaje.
- În vederea «Rezervări» apare, în ribbon, butonul **«Repară...»**; el se vede **doar** cât timp angajamentul are rezervări greșite. Apasă-l. K-BOT îți arată întâi câte rânduri și câte rezervări se schimbă, cu câteva exemple, și
  abia după ce confirmi scrie corecturile. **Nu se descarcă nimic din FOREXE.**
- Repararea **nu se face** dacă ar atinge o lună care are **revizii** de fundamentare: K-BOT spune ce lună e și nu schimbă nimic.
- După reparare, rândul din lista principală nu mai este roșu și butonul dispare.

<!-- capture: grafic-rezervari | caption: Graficul rezervărilor | goto: view:rezervari | prepare: Apăsați butonul «Grafice» din ribbon, ca să se deschidă graficul. | redo: 2026-10-17 21:00 | why: 0114-15: graficul se deschide din butonul «Grafice» din ribbon; 0114-06/14: ribbonul are in dreapta grupul machetei (langa «Inchide») si grupul vederii; meniul din subsolul listei are doua randuri; E-Factura sta sub Extrase in bara vederilor; 0114-05: arborii nu mai au iconita de descarcare a extraselor (cea cu cardul din lista angajamentelor a plecat; descarcarea e butonul din ribbon); 0113-01: graficul e o fereastră cu bara lui de titlu și cu «Închide», mai mică decât spațiul de lucru, nu o pagină a ferestrei principale -->
