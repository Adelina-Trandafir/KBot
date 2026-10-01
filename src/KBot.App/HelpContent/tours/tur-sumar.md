---
id: tur-sumar
title: Vederea «Sumar»
part: contabil
topic: contabil.vederi.sumar
---
## Datele angajamentului
<!-- slice: 0011, 0000-14, 0000-20 -->
target: SumarView.pnlHeader
goto: view:sumar
Sus stau datele angajamentului selectat în arbore: codul, data din FOREXE, data creării și a definitivării, starea, dacă a fost încărcat sau preluat și descrierea.

## Asociază parteneri
<!-- slice: 0084-02, 0000-28, 0000-30 -->
target: SumarView.btnPartners
Butonul apare doar când angajamentul are un document de fundamentare (se face din vederea «Rezervări», cu semnul «+»); altfel nu se vede. Deschide fereastra în care asociezi angajamentul cu unul sau mai mulți parteneri: îi alegi din listă, apeși «Asociază», apoi «Salvează».

## Indicatorii
<!-- slice: 0011, 0000-20 -->
target: SumarView.grid
Câte un rând pentru fiecare indicator (clasificație): creditul bugetar și totalurile de rezervări, recepții, plăți, revizii DDF și ordonanțări.

## Tabel › Capul coloanelor
<!-- slice: 0028, 0000-23 -->
target: SumarView.grid
part: header
Titlurile coloanelor. Tragi de marginea dintre două titluri ca să lărgești sau să îngustezi o coloană.

## Tabel › Pâlnia unei coloane
<!-- slice: 0028-03, 0030, 0000-23 -->
target: SumarView.grid
part: header.filter
Deschide meniul coloanei, cu trei file: Sortare, Filtrare, Grupare. O pâlnie plină, colorată, arată că pe coloana aceea e pus un filtru.

## Tabel › TOTALURI
<!-- slice: 0017-01, 0000-23 -->
target: SumarView.grid
part: footer
Rândul de jos însumează ce se vede după filtre.
