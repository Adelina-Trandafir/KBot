---
id: clasificatii-rectificare
title: Adaugă sau editează o rectificare bugetară
part: contabil
keywords: clasificatii clasificatie rectificare rectificari bugetare numar document data trimestre adaug editez modific salvez
starts: KbotForm
host-key: clasificatii-rectificare
---
<!-- slice: 000T-10 -->

## Deschide meniul
<!-- slice: 000T-10, AV01-13 -->
target: KbotForm.btnMeniu
wait: click
optional: yes
why: Dacă fereastra «Clasificații cheltuieli» e deja deschisă, nu mai ai nevoie de pașii de deschidere.
Apasă butonul <b>MENIU</b>.

## Nomenclatoare
<!-- slice: 000T-10, AV01-13 -->
anchor: menu.nomenclatoare
allow: KbotForm.btnMeniu
wait: anchor:menu.clasificatii
optional: yes
why: Dacă fereastra «Clasificații cheltuieli» e deja deschisă, nu mai ai nevoie de pașii de deschidere.
Treci cu mausul peste <b>«Nomenclatoare»</b> sau apasă-l: se deschide lista lui.

## Clasificații cheltuieli
<!-- slice: 000T-10, AV01-13 -->
anchor: menu.clasificatii
allow: KbotForm.btnMeniu
wait: opens:ClasificatiiForm
optional: yes
why: Dacă fereastra «Clasificații cheltuieli» e deja deschisă, nu mai ai nevoie de pașii de deschidere.
Alege <b>«Clasificații cheltuieli»</b>.

## Alege o clasificație
<!-- slice: 000T-10 -->
target: ClasificatiiBugetView.tree
wait: select
Alege din arbore un <b>Alineat</b> (ultimul nivel). Rectificările se completează pentru o clasificație anume.

## Rectificările anului
<!-- slice: 000T-10, 000T-13 -->
target: ClasificatiiBugetView.gridRectificari
Aici sunt <b>rectificările bugetare ale anului</b> pentru clasificația aleasă: numărul documentului, data și cele patru trimestre. În subsol vezi totalurile. Apasă «Înainte» (săgeata verde cu bară, jos în dreapta, ⇥).

## Adaugă o rectificare
<!-- slice: 000T-10 -->
target: ClasificatiiBugetView.gridRectificari
part: footer.right
wait: click
Apasă <b>«+»</b> din subsolul tabelului. Apare un rând nou, pe care îl completezi direct în tabel.

## Completează rândul
<!-- slice: 000T-10, 000T-13 -->
target: ClasificatiiBugetView.gridRectificari
Scrie <b>numărul documentului</b>, <b>data</b> și valorile pe trimestre, apoi apasă «Înainte» (săgeata verde cu bară, jos în dreapta, ⇥).<BR>
<mark>«Nr. doc.» și data sunt obligatorii, iar data trebuie să fie în anul de lucru.</mark>

## Editează sau șterge o rectificare
<!-- slice: 000T-10, 000T-13 -->
target: ClasificatiiBugetView.gridRectificari
optional: yes
why: Treci peste pas dacă nu ai de modificat o rectificare existentă.
Ca să <b>modifici</b> o rectificare existentă, scrie peste valorile din rând. <b>«✕»</b> de pe rând o șterge.<BR>
<mark>Nimic nu se scrie în baza de date până nu apeși «Salvează».</mark> Apoi apasă «Înainte» (săgeata verde cu bară, jos în dreapta, ⇥).

## Bugetul în vigoare
<!-- slice: 000T-10, 000T-13 -->
target: ClasificatiiBugetView.gridTotal
Rândul acesta adună <b>ultimul buget</b> și <b>toate rectificările</b>, pe trimestre, ca să vezi efectul rectificării. Apasă «Înainte» (săgeata verde cu bară, jos în dreapta, ⇥).

## Salvează
<!-- slice: 000T-10 -->
target: ClasificatiiBugetView.btnSalveaza
wait: click
guard: yes
Apasă <b>«Salvează»</b>. Se scriu în baza de date rectificările și versiunile de buget ale clasificației alese.
