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
<!-- slice: 0011, 0000-14 -->
Prima privire asupra angajamentului.

<!-- capture: sumar | caption: Vederea «Sumar» | goto: view:sumar | prepare: Selectați un angajament cu mai mulți indicatori. -->

**Sus**, datele angajamentului: codul, data din FOREXE, data creării și a definitivării, starea,
dacă a fost încărcat / preluat și descrierea.

**Încărcat** = angajamentul a fost trimis în FOREXE din K-BOT (sau din vechiul program); **preluat** =
a venit în K-BOT dintr-o descărcare din FOREXE.

**Jos**, câte un rând pentru fiecare **indicator** (clasificație) al angajamentului: codul
indicatorului, creditul bugetar și totalurile de rezervări, recepții, plăți, revizii DDF și
ordonanțări.

Coloanele tabelului se pot sorta și filtra din **pâlnia** capului de coloană; rândul de jos,
**TOTALURI**, însumează ce se vede — [Arborii și tabelele](topic:contabil.liste).

## Asociază parteneri
<!-- slice: 0084-02, 0000-28, 0000-30 -->
Butonul **Asociază parteneri**, în dreapta datelor angajamentului, apare doar când angajamentul are
un document de fundamentare (DDF; îl faci din vederea «Rezervări», cu semnul «+»): partenerii se asociază documentului. Fără document, butonul nu se vede.

În fereastra care se deschide vezi partenerii deja asociați. Alegi un partener din listă (scrii
începutul numelui sau al codului fiscal) și apeși **Asociază**; apare în tabel cu rolul «De adăugat».
Poți alege câți parteneri ai nevoie. **Salvează** îi trimite pe toți odată; **Renunță** închide
fereastra fără să asocieze nimic.

- Un partener deja asociat nu mai apare în listă și nu se adaugă a doua oară.
- De aici partenerii doar se adaugă: cei deja asociați rămân. Pe unul ales și încă nesalvat îl poți
  scoate cu **Scoate din asociere**. Un partener deja salvat se scoate din editorul DDF, de pe pagina
  «Parteneri» — [Editorul DDF](topic:contabil.ddf.editor).
- Partenerul principal al documentului, cel din antetul lui, se schimbă tot din editor.
