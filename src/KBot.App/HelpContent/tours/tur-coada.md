---
id: tur-coada
title: Coada robotului
part: contabil
topic: contabil.forexe.coada
screens: RobotQueueForm
---
## Pornește turul din fereastră
<!-- slice: 0098, 0000-31 -->
target: RobotQueueForm
Turul merge doar cu fereastra «Coada robotului» deschisă, adică cât robotul are ceva în coadă. Ai pornit mai multe descărcări la rând? Fereastra se deschide singură, iar dacă ai închis-o o deschizi din butonul «Coadă N» din banda FOREXE. Apoi apasă «?» din bara ei de titlu și pornește turul din meniu. Se închide singură după ce coada s-a golit, deci turul se termină odată cu ea.

## Ce lucrează robotul acum
<!-- slice: 0098, 0000-31 -->
target: RobotQueueForm.lblCurent
Sarcina în lucru. Dacă robotul așteaptă după tine (de exemplu cât e deschisă fereastra Asocieri) sau coada e în pauză, scrie și asta.

## Sarcinile care așteaptă
<!-- slice: 0098, 0100-03, 0000-31, 0000-48 -->
target: RobotQueueForm.lblInCoada
Sub acest rând e lista: sarcinile care așteaptă, în ordinea în care vor rula, cu ora la care au intrat în coadă, fiecare cu un «X» care o scoate din coadă. O sarcină nouă se adaugă la sfârșit; o operațiune salvată de tine în pagina FOREXE trece în fața lor.

## Pauză / Continuă
<!-- slice: 0098, 0000-31 -->
target: RobotQueueForm.btnPauza
Sarcina în lucru se termină, cele care urmează nu mai pornesc. Butonul devine «Continuă»: coada pornește din nou, în aceeași ordine. În pauză, pe butonul din banda FOREXE apare «‖».

## Golește coada
<!-- slice: 0098, 0000-31 -->
target: RobotQueueForm.btnGoleste
Scoate toate sarcinile care așteaptă, după ce confirmi. Se aprinde doar cât așteaptă ceva. Sarcina în lucru se termină normal.

## Oprește curenta
<!-- slice: 0098, 0000-31 -->
target: RobotQueueForm.btnOpreste
Oprește robotul din sarcina în lucru, ca «Anulează» din consolă. Se aprinde doar cât robotul lucrează în FOREXE; ce s-a salvat deja rămâne.
