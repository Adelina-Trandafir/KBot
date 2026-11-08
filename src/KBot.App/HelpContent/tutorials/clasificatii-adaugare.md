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

## Arborele de clasificații
<!-- slice: 000T-10, 000T-13 -->
target: ClasificatiiBugetView.tree
Clasificațiile sunt într-un arbore: <b>Capitol › Subcapitol › Titlu › Articol › Alineat</b>. Codul e în prima coloană, denumirea în a doua. Apasă «Înainte» (săgeata verde cu bară, jos în dreapta, ⇥).

## Filtrele arborelui
<!-- slice: 000T-10, 000T-13, 0126-04 -->
target: ClasificatiiBugetView.tree
part: header.right
optional: yes
why: Filtrele sunt doar vederi ale arborelui; nu schimbă nimic în date.
Iconița din dreapta capului arborelui deschide cele două filtre.<BR>
<b>«Arată toate clasificațiile»</b> bifat: arborele arată toate clasificațiile configurate; debifat: doar cele cu mișcare în an (un trimestru de buget sau de rectificare diferit de zero).<BR>
<mark>«Arată DOAR clasificațiile folosite în FOREXE» are prioritate: arată numai clasificațiile pentru care FOREXE a raportat un credit bugetar.</mark> Apasă «Înainte» (săgeata verde cu bară, jos în dreapta, ⇥).

## Adaugă din ribbon
<!-- slice: 000T-10, 0126-07 -->
anchor: ribbon.clasificatii_adauga
wait: opens:ClasificatiiAddForm
Apasă <b>«Adaugă»</b> (plusul verde) în grupul «Clasificații» din dreapta ribbonului. Se deschide fereastra de adăugare.

## 1. Sursa și sectorul
<!-- slice: 000T-10, 000T-13 -->
target: ClasificatiiAddForm.flowSurse
optional: yes
why: Dacă în bara de titlu e aleasă o sursă anume, secțiunea nu apare: clasificațiile se adaugă pe sursa din bara de titlu.
Cu <b>«Toate»</b> în bara de titlu, bifează <b>sursele-sector</b> pentru care adaugi clasificațiile. Se pot alege doar sursele unității.<BR>
<mark>Cu o sursă anume aleasă în bara de titlu, secțiunea lipsește, iar clasificațiile se adaugă pe sursa aceea.</mark> Apoi apasă «Înainte» (săgeata verde cu bară, jos în dreapta, ⇥).

## 2. Clasificația funcțională
<!-- slice: 000T-10, 000T-13 -->
target: ClasificatiiAddForm.treeF
Bifează în arbore <b>clasificațiile funcționale</b>. Arborele arată doar ce e sub capitolul unității (de obicei 65), fără capitolul ca rădăcină.<BR>
<mark>Se bifează pozițiile de pe ultimul nivel.</mark> Apoi apasă «Înainte» (săgeata verde cu bară, jos în dreapta, ⇥).

## 3. Clasificația economică
<!-- slice: 000T-10, 000T-13 -->
target: ClasificatiiAddForm.treeE
Bifează în arbore <b>clasificațiile economice</b>.<BR>
<mark>Se bifează pozițiile de pe ultimul nivel; nivelurile de deasupra nu au căsuță.</mark> Apoi apasă «Înainte» (săgeata verde cu bară, jos în dreapta, ⇥).

## Adaugă clasificațiile
<!-- slice: 000T-10 -->
target: ClasificatiiAddForm.btnAdauga
wait: click
Apasă <b>«Adaugă clasificațiile»</b>. Se adaugă câte o clasificație pentru fiecare combinație sursă × funcțională × economică bifată; cele care există deja se sar.<BR>
<mark>Dacă te răzgândești, «Renunță» închide fereastra fără să adauge nimic.</mark>

## Clasificațiile noi
<!-- slice: 000T-10 -->
Clasificațiile noi apar în arbore (K-BOT bifează singur «Arată toate clasificațiile», ca să le găsești). Încă nu au buget: îl completezi cu <link tutorial="clasificatii-buget">Adaugă sau editează bugetul unei clasificații</link>. Apasă «Gata».
