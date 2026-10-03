---
id: tur-asocieri
title: Fereastra Asocieri
part: contabil
topic: contabil.asocieri
---
## Pornește turul din fereastră
<!-- slice: 0000-12, 0000-20 -->
target: AsociereForm
Turul merge doar cu fereastra Asocieri deschisă. Dacă n-o vezi, închide turul, deschide Asocieri (vederea Recepții › iconița din dreapta, sus, a arborelui), apasă «?» din bara ei de titlu și pornește turul din meniu (sau F1, apoi turul din pagina de ajutor).

## Recepțiile și lanțurile lor
<!-- slice: 0048-04, 0065, 0101 -->
target: AsociereForm.treeLant
Câte un rând pe recepție: data, valoarea de azi și câte instantanee are. Sub ea, lanțul: salvările ei din istoric, în ordinea orei. Ultimul instantaneu trebuie să aibă valoarea de azi; altfel apare «Lanțul nu se închide», iar recepția și ultimul ei instantaneu se scriu cu roșu.

## Indicatorii
<!-- slice: 0048-04 -->
target: AsociereForm.gridLant
Indicatorii instantaneului ales. Un indicator poate ajunge la 0, dar nu poate dispărea din lanț: e cel mai bun indiciu că un instantaneu nu e al unei recepții.

## Coșul: instantaneele neașezate
<!-- slice: 0048-04, 0061 -->
target: AsociereForm.treeLibere
Instantaneele care încă nu au recepție. Trage-le peste recepția lor din stânga; locul în lanț îl dă ora. Cu Ctrl sau Shift iei mai multe deodată. Tras înapoi aici, un instantaneu se desprinde.

## Clic dreapta
<!-- slice: 0048-04, 0059, 0107 -->
target: AsociereForm.treeLibere
Dă întâi clic stânga pe rând; clicul dreapta merge doar pe un rând ales. Pe un instantaneu din coș: «Nu consemnează nicio schimbare» (o salvare care n-a schimbat nimic) sau «Începe o recepție nouă» (o recepție ștearsă înainte de prima descărcare). Pe unul așezat: «Desprinde de recepție» și «Este rândul de ștergere».

## Graficele și plățile
<!-- slice: 0048-05, 0048-09, 0061 -->
target: AsociereForm.pnlGrafice
Reperele verticale sunt plățile: ține mouse-ul pe unul ca să vezi totalul recepțiilor la data plății și diferența față de plăți. O diferență ciudată înseamnă, de obicei, un instantaneu pe recepția greșită.

## Grafice › Grafic
<!-- slice: 0048-05, 0000-23 -->
target: AsociereForm.navGrafice
part: item:grafic
Evoluția valorii recepțiilor în timp.

## Grafice › Distribuție
<!-- slice: 0048-09, 0000-23 -->
target: AsociereForm.navGrafice
part: item:benzi
Benzile, câte una pe recepție: se vede unde cade fiecare instantaneu.

## Mesajele
<!-- slice: 0048-04 -->
target: AsociereForm.ntfMesaj
Aici K-BOT spune cât mai e de făcut, ce a așezat singur și de ce refuză o tragere. Citește-l după fiecare pas.

## Salvează
<!-- slice: 0055, 0058 -->
target: AsociereForm.btnSalveaza
Nimic nu se scrie până aici. După o descărcare, butonul se aprinde abia când fiecare instantaneu are o hotărâre; renunțarea atunci aruncă toată descărcarea.
