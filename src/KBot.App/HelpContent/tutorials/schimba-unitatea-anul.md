---
id: schimba-unitatea-anul
title: Schimbă unitatea, anul sau sursa/sectorul
part: contabil
keywords: schimb unitate unitatea an anul sursa sector ss perioada selector bara titlu lucrez alta unitate
starts: KbotForm
host-key: schimba-unitatea-anul
---
<!-- slice: 000T-10 -->

## Unde se schimbă
<!-- slice: 000T-10 -->
target: KbotForm.capBar
În <b>bara de titlu</b> a ferestrei principale sunt trei selectoare: <b>unitatea</b>, <b>anul</b> și <b>sursa/sectorul</b>. Fiecare apare doar când ai de ales între cel puțin două variante, așa că unele pot lipsi. Apasă «Înainte».

## Unitatea
<!-- slice: 000T-10 -->
target: KbotForm.capBar
part: unit
when: visible
wait: signal:selector-unit
optional: yes
why: Selectorul de unitate apare doar dacă ai acces la mai multe unități.
Apasă <b>numele unității</b> din bara de titlu și alege altă unitate.<BR>
<mark>La schimbarea unității sesiunea FOREXE se închide singură (nu se poate schimba cât rulează o operație FOREXE), iar lista de angajamente se citește din nou pentru unitatea nouă.</mark>

## Anul
<!-- slice: 000T-10 -->
target: KbotForm.capBar
part: year
when: visible
wait: signal:selector-year
optional: yes
why: Selectorul de an apare doar dacă unitatea are mai mulți ani configurați.
Apasă <b>anul</b> din bara de titlu și alege anul cu care lucrezi.

## Sursa/sectorul
<!-- slice: 000T-10 -->
target: KbotForm.capBar
part: ss
when: visible
wait: signal:selector-ss
optional: yes
why: Selectorul de sursă/sector apare doar dacă anul are mai multe și doar când arborele e sortat după nume.
Apasă <b>«Sursă/Sector»</b> din bara de titlu și alege sursa-sectorul cu care lucrezi.<BR>
<mark>Când arborele e sortat după data creării, selectorul dispare: arborele arată atunci toate sursele anului.</mark>

## Gata
<!-- slice: 000T-10 -->
La fiecare schimbare lista de angajamente se citește din nou pentru alegerea făcută. Apasă «Gata».
