---
id: tur-receptii
title: Vederea «Recepții»
part: contabil
topic: contabil.vederi.receptii
---
## Arborele recepțiilor
<!-- slice: 0015, 0015-03, 0065 -->
target: ReceptiiView.tree
goto: view:receptii
Trei niveluri: luna, recepția, antetul recepției. Dacă e gol, alege în listă un angajament care are recepții.

## Recepții și plăți, dintr-o privire
<!-- slice: 0015-02 -->
target: ReceptiiView.tree
Ține mouse-ul pe o lună sau pe o recepție: eticheta arată cât s-a recepționat până acolo, cât s-a plătit și diferența, cu roșu dacă s-a plătit mai mult decât s-a recepționat.

## Arbore › Lupa
<!-- slice: 0027, 0000-23 -->
target: ReceptiiView.tree
part: header.search
Caută în arbore: scrii și arborele arată doar ce se potrivește. Esc golește căutarea și închide banda.

## Arbore › Actualizează
<!-- slice: 0060, 0000-23, 0114-16, 0000-91 -->
target: ReceptiiView.tree
part: footer.right
Reîmprospătează recepțiile din FOREXE; întâi alegi care, în fereastra «Ce recepții reîmprospătez?». E singurul buton rămas în arbore; celelalte sunt în ribbon.

## Ribbon › Asociază recepții
<!-- slice: 0048-04, 0000-23, 0056-02, 0114-16, 0000-91 -->
Butoanele vederii sunt în ribbon, în grupul «Recepții» din dreapta (lângă grupul «Lista angajamentelor»). «Asociază recepții» deschide fereastra Asocieri, unde corectezi unde e așezat fiecare instantaneu din istoric față de recepții. Se poate deschide oricând. Legăturile unui angajament cu o ordonanțare din ziua instantaneului sau de după ea se văd, dar nu se mai pot muta.

## Ribbon › Reface din istoric...
<!-- slice: 0062, 0000-23, 0114-16, 0000-91 -->
Reface din istoric instantaneele și liniile de recepție care lipsesc. Apare doar când angajamentul are erori de reparat (o recepție al cărei lanț nu se închide, scrisă cu roșu în lista principală). Întâi numără ce lipsește și te întreabă; scrie doar după ce confirmi. Anteturile refăcute apar în Asocieri, la «Instantanee neașezate».

## Grila pe clasificații
<!-- slice: 0015 -->
target: ReceptiiView.grid
Pentru nodul ales, un rând «Toți indicatorii» și câte un rând pe clasificație, cu denumirea ei.

## Tabel › Pâlnia unei coloane
<!-- slice: 0028-03, 0030, 0000-23 -->
target: ReceptiiView.grid
part: header.filter
Deschide meniul coloanei, cu trei file: Sortare, Filtrare, Grupare. O pâlnie plină, colorată, arată că pe coloana aceea e pus un filtru.

## Tabel › TOTALURI
<!-- slice: 0017-01, 0000-23 -->
target: ReceptiiView.grid
part: footer
Rândul de jos însumează coloanele cu sume, doar pentru rândurile care se văd după filtre.
