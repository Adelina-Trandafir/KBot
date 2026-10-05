---
id: clasificatii-adaugare
title: Adaugă clasificații bugetare
part: contabil
keywords: clasificatii clasificatie bugetare adaug adaugare nomenclatoare sursa functionala economica capitol articol alineat
starts: KbotForm
host-key: clasificatii-adaugare
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

## Arborele de clasificații
<!-- slice: 000T-10 -->
target: ClasificatiiForm.tree
Clasificațiile sunt într-un arbore: <b>Capitol › Subcapitol › Articol › Alineat</b>. Codul e în prima coloană, denumirea în a doua. Apasă «Înainte».

## Bifele de sub arbore
<!-- slice: 000T-10 -->
target: ClasificatiiForm.tlyBife
optional: yes
why: Bifele sunt doar filtre ale arborelui; nu schimbă nimic în date.
<b>«Arată toate clasificațiile»</b> bifat: arborele arată toate clasificațiile configurate; debifat: doar cele cu mișcare în an (un trimestru de buget sau de rectificare diferit de zero).<BR>
<mark>«Arată DOAR clasificațiile folosite în FOREXE» are prioritate: arată numai clasificațiile pentru care FOREXE a raportat un credit bugetar.</mark> Apasă «Înainte».

## Iconița «+» din subsolul arborelui
<!-- slice: 000T-10 -->
target: ClasificatiiForm.tree
part: footer.right
wait: opens:ClasificatiiAddForm
Apasă <b>«+»</b> din subsolul arborelui, cel de lângă «Adaugă clasificații». Se deschide fereastra de adăugare.

## 1. Sursa și sectorul
<!-- slice: 000T-10 -->
target: ClasificatiiAddForm.flowSurse
Bifează <b>sursele-sector</b> pentru care adaugi clasificațiile. Se pot alege doar sursele unității. Apoi apasă «Înainte».

## 2. Clasificația funcțională
<!-- slice: 000T-10 -->
target: ClasificatiiAddForm.treeF
Bifează în arbore <b>clasificațiile funcționale</b>.<BR>
<mark>Se bifează pozițiile de pe ultimul nivel.</mark> Apoi apasă «Înainte».

## 3. Clasificația economică
<!-- slice: 000T-10 -->
target: ClasificatiiAddForm.treeE
Bifează în arbore <b>clasificațiile economice</b>.<BR>
<mark>Bifarea unui articol bifează tot ce este sub el.</mark> Apoi apasă «Înainte».

## Adaugă clasificațiile
<!-- slice: 000T-10 -->
target: ClasificatiiAddForm.btnAdauga
wait: click
Apasă <b>«Adaugă clasificațiile»</b>. Se adaugă câte o clasificație pentru fiecare combinație sursă × funcțională × economică bifată; cele care există deja se sar.<BR>
<mark>Dacă te răzgândești, «Renunță» închide fereastra fără să adauge nimic.</mark>

## Clasificațiile noi
<!-- slice: 000T-10 -->
Clasificațiile noi apar în arbore (K-BOT bifează singur «Arată toate clasificațiile», ca să le găsești). Încă nu au buget: îl completezi cu <link tutorial="clasificatii-buget">Adaugă sau editează bugetul unei clasificații</link>. Apasă «Gata».
