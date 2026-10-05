---
id: ordonare-arbore
title: Schimbă ordonarea angajamentelor din arbore
part: contabil
keywords: ordonare ordonez sortare sortez arbore angajamente dupa nume data crearii coloane cod surse
starts: KbotForm
host-key: ordonare-arbore
---
<!-- slice: 000T-10 -->

## Iconița de opțiuni a arborelui
<!-- slice: 000T-10 -->
target: KbotForm.tree
part: header.right
wait: click
Apasă <b>iconița din dreapta antetului</b> listei de angajamente. Se deschide lista cu opțiunile arborelui.

## Alege ordonarea
<!-- slice: 000T-10 -->
anchor: popup.sort-name+sort-date
wait: signal:tree-menu:sort-name|tree-menu:sort-date
Alege <b>«Sortare după nume»</b> sau <b>«Sortare după data creării»</b>.<BR>
<mark>După nume: ordine alfabetică după denumire, pentru sursa-sectorul ales în bara de titlu. După data creării: în ordinea în care au fost create, cu toate sursele anului; un angajament fără dată de creare stă la sfârșit, în ordine alfabetică.</mark>

## Din nou iconița
<!-- slice: 000T-10 -->
target: KbotForm.tree
part: header.right
wait: click
optional: yes
why: Coloanele afișate se schimbă separat de ordonare; treci peste pas dacă vrei să le lași cum sunt.
Dacă vrei să schimbi și <b>coloanele</b> arborelui, apasă din nou iconița.

## Coloanele
<!-- slice: 000T-10 -->
anchor: popup.col-cod+col-surse
wait: signal:tree-menu:col-cod|tree-menu:col-surse
optional: yes
why: Coloanele afișate se schimbă separat de ordonare; treci peste pas dacă vrei să le lași cum sunt.
Bifează sau debifează <b>«Afișare coloana CODANGAJAMENT»</b> și <b>«Afișare coloana SURSE»</b>.<BR>
<mark>Fiecare ordonare își ține propria alegere de coloane: după nume e implicit codul afișat și sursele ascunse, după data creării invers.</mark>

## Gata
<!-- slice: 000T-10 -->
Alegerea se păstrează și la următoarea pornire; o găsești și în Setări › Aplicație. Apasă «Gata».
