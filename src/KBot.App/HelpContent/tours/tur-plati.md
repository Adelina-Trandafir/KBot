---
id: tur-plati
title: Vederea «Plăți»
part: contabil
topic: contabil.vederi.plati
---
## Arborele plăților
<!-- slice: 0017, 0017-04, 0049-01 -->
target: PlatiView.tree
goto: view:plati
Lunile, apoi zilele; o zi strânge toate plățile ei. O zi verde are numai încasări.

## Arbore › Lupa
<!-- slice: 0027, 0000-23 -->
target: PlatiView.tree
part: header.search
Caută în arbore o lună sau o zi. Esc golește căutarea și închide banda.

## Arbore › Semnul «+»
<!-- slice: 0049-01, 0000-23 -->
target: PlatiView.tree
part: node.icon
Stă pe zilele și pe lunile care au plăți neordonanțate. Pe o zi face o ordonanțare din plățile ei; pe o lună, câte una pentru fiecare zi a lunii. Dacă nu există plăți neordonanțate, semnul nu apare și pasul acesta se sare.

## Plățile nodului ales
<!-- slice: 0017 -->
target: PlatiView.grid
Clasificația, plătitorul, numărul documentului, data și suma fiecărei plăți a nodului ales.

## Tabel › Pâlnia unei coloane
<!-- slice: 0028-03, 0030, 0000-23 -->
target: PlatiView.grid
part: header.filter
Deschide meniul coloanei, cu trei file: Sortare, Filtrare, Grupare. O pâlnie plină, colorată, arată că pe coloana aceea e pus un filtru.

## Tabel › TOTALURI
<!-- slice: 0017-01, 0000-23 -->
target: PlatiView.grid
part: footer
Rândul de jos însumează plățile care se văd după filtre.

## Extrasul bancar
<!-- slice: 0080-02 -->
target: PlatiView.detailPane
Extrasul bancar al plății selectate. «Fără extras bancar asociat» înseamnă că plata nu are extras legat de ea.
