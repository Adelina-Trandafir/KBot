---
id: clasificatii-buget
title: Adaugă sau editează bugetul unei clasificații
part: contabil
keywords: clasificatii clasificatie buget versiuni versiune trimestre inceput adaug editez modific salvez verifica bugetul
starts: KbotForm
host-key: clasificatii-buget
---
<!-- slice: 000T-10 -->

## Deschide meniul
<!-- slice: 000T-10 -->
target: KbotForm.btnMeniu
wait: click
optional: yes
why: Dacă fereastra «Clasificații bugetare» e deja deschisă, nu mai ai nevoie de pașii de deschidere.
Apasă butonul <b>MENIU</b>.

## Nomenclatoare
<!-- slice: 000T-10 -->
anchor: menu.nomenclatoare
allow: KbotForm.btnMeniu
wait: anchor:menu.clasificatii
optional: yes
why: Dacă fereastra «Clasificații bugetare» e deja deschisă, nu mai ai nevoie de pașii de deschidere.
Treci cu mausul peste <b>«Nomenclatoare»</b> sau apasă-l: se deschide lista lui.

## Clasificații bugetare
<!-- slice: 000T-10 -->
anchor: menu.clasificatii
allow: KbotForm.btnMeniu
wait: opens:ClasificatiiForm
optional: yes
why: Dacă fereastra «Clasificații bugetare» e deja deschisă, nu mai ai nevoie de pașii de deschidere.
Alege <b>«Clasificații bugetare»</b>.

## Alege o clasificație
<!-- slice: 000T-10 -->
target: ClasificatiiForm.tree
wait: select
Alege din arbore un <b>Alineat</b> (ultimul nivel).<BR>
<mark>La un nod de mai sus bugetul se vede doar ca rezumat, fără rânduri de completat: ultimul buget al fiecărei clasificații de sub nod și totalul rectificărilor.</mark>

## Versiunile de buget
<!-- slice: 000T-10 -->
target: ClasificatiiForm.gridBuget
Fiecare rând este <b>bugetul de la data din «Început» încolo</b>, pe cele patru trimestre. Documentul de fundamentare citește bugetul de la data revizuirii. Apasă «Înainte».

## Adaugă o versiune
<!-- slice: 000T-10 -->
target: ClasificatiiForm.gridBuget
part: footer.right
wait: click
Apasă <b>«+»</b> din subsolul tabelului. Apare un rând nou, pe care îl completezi direct în tabel.<BR>
<mark>Prima versiune a anului începe la 01.01; următoarele propun data de azi, ziua în care se schimbă de obicei un buget.</mark>

## Completează rândul
<!-- slice: 000T-10 -->
target: ClasificatiiForm.gridBuget
Completează data din <b>«Început»</b> și cele <b>patru trimestre</b>, apoi apasă «Înainte».

## Editează sau șterge o versiune
<!-- slice: 000T-10 -->
target: ClasificatiiForm.gridBuget
optional: yes
why: Treci peste pas dacă nu ai de modificat o versiune existentă.
Ca să <b>modifici</b> o versiune existentă, scrie peste valorile din rând. <b>«✕»</b> de pe rând o șterge.<BR>
<mark>Nimic nu se scrie în baza de date până nu apeși «Salvează».</mark> Apoi apasă «Înainte».

## Bugetul în vigoare
<!-- slice: 000T-10 -->
target: ClasificatiiForm.gridTotal
Rândul acesta arată <b>ultimul buget</b> (cel cu data cea mai nouă) plus <b>toate rectificările</b>, pe trimestre. Se actualizează pe măsură ce completezi. Apasă «Înainte».

## Salvează
<!-- slice: 000T-10 -->
target: ClasificatiiForm.btnSalveaza
wait: click
Apasă <b>«Salvează»</b>. Se scriu în baza de date versiunile de buget și rectificările clasificației alese.

## Verifică bugetul față de FOREXE
<!-- slice: 000T-10 -->
target: ClasificatiiForm.btnVerifica
wait: click
optional: yes
why: Verificarea e doar o comparație; nu schimbă nimic în date.
Dacă vrei, apasă <b>«Verifică bugetul»</b>: compară bugetul și rectificările de azi cu creditul bugetar descărcat din FOREXE, pe fiecare clasificație, și arată diferența.
