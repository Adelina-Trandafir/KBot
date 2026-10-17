---
id: contabil.vederi.plati
title: Plăți
part: contabil
order: 50
parent: contabil.vederi
screens: PlatiView
keywords: plati, incasari, extras bancar, ordin de plata, op, banca
open: view:plati
---
<!-- slice: 0017, 0017-04, 0049-01, 0000-14, 0000-15, 0101 -->
Plățile (și încasările) angajamentului.

<!-- capture: plati | caption: Vederea «Plăți» | goto: view:plati | prepare: Selectați un angajament cu plăți în mai multe zile, apoi o plată din listă. | redo: 2026-10-17 20:00 | why: 0114-06/14: ribbonul are in dreapta grupul machetei (langa «Inchide») si grupul vederii; meniul din subsolul listei are doua randuri; E-Factura sta sub Extrase in bara vederilor; 0114-05: arborii nu mai au iconita de descarcare a extraselor (cea cu cardul din lista angajamentelor a plecat; descarcarea e butonul din ribbon) -->

- **Arborele**: lunile, apoi zilele. O zi strânge toate plățile ei. Ziua e **verde** când toate
  plățile ei sunt încasări.
- **Lista** arată exact plățile nodului ales: clasificația, plătitorul, numărul documentului,
  data și suma, cu totalul jos.
- **Jos** vezi **extrasul bancar** al plății selectate. «Fără extras bancar asociat» înseamnă că
  plata nu are extras legat de ea.
- Semnul **«+»** apare pe zilele (și pe lunile) care au plăți **neordonanțate**. Pe o zi face o
  ordonanțare din plățile ei; pe o lună face câte una pentru fiecare zi a lunii,
  după ce confirmi, fără mesaj la sfârșit — [Ordonanțări noi](topic:contabil.ord.generare).

Căutarea în arbore (lupa), pâlniile coloanelor și rândul **TOTALURI** se folosesc ca peste tot —
[Arborii și tabelele](topic:contabil.liste).
