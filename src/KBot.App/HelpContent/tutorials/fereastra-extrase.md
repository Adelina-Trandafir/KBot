---
id: fereastra-extrase
title: Fereastra «Extrase de cont» (toate extrasele)
part: contabil
keywords: extrase extras cont fereastra toate meniu descarc banca operatiuni detaliu antet
starts: KbotForm
host-key: fereastra-extrase
---
<!-- slice: 000T-10 -->

## Deschide meniul
<!-- slice: 000T-10 -->
target: KbotForm.btnMeniu
wait: click
optional: yes
why: Dacă fereastra «Extrase de cont» e deja deschisă, nu mai ai nevoie de acest pas.
Apasă butonul <b>MENIU</b>.

## Alege «Extrase»
<!-- slice: 000T-10 -->
anchor: menu.extrase
allow: KbotForm.btnMeniu
wait: opens:ExtraseForm
optional: yes
why: Dacă fereastra «Extrase de cont» e deja deschisă, nu mai ai nevoie de acest pas.
Din meniul deschis alege <b>«Extrase»</b>.

## Toate extrasele
<!-- slice: 000T-10 -->
target: ExtraseForm.panel
Fereastra arată <b>toate extrasele bazei de date</b>, nu doar ale unui angajament: cu sau fără angajament, cu cod de contract sau fără.<BR>
<mark>Vederea «Extrase» din fereastra principală arată doar extrasele angajamentului ales.</mark> Apasă «Înainte».

## Arborele
<!-- slice: 000T-10 -->
target: ExtraseForm>ExtrasePanel.tree
Arborele grupează extrasele pe <b>luni</b> și <b>zile</b>. Un antet fără nicio operațiune stă la data extrasului.

## Alege o zi
<!-- slice: 000T-10 -->
target: ExtraseForm>ExtrasePanel.tree
wait: select
Alege o <b>zi</b> din arbore. La o zi vezi operațiunile ei și, dedesubt, detaliul operațiunii alese.<BR>
<mark>La o lună sau la «Toate extrasele» vezi sus antetele extraselor și jos operațiunile antetului ales.</mark>

## Tabelele
<!-- slice: 000T-10 -->
target: ExtraseForm>ExtrasePanel.innerSplit
Aici sunt tabelele cu <b>antetele</b> și cu <b>operațiunile</b>. Alege un rând ca să-i vezi operațiunile sau detaliul. Coloanele afișate se aleg din Setări › Extrase.

## Detaliul operațiunii
<!-- slice: 000T-10 -->
target: ExtraseForm>ExtrasePanel.detailPane
Aici e <b>operațiunea aleasă, în întregime</b>: număr și date, plătitor, CUI, IBAN, sume și explicații.<BR>
<mark>În această fereastră apar și patru rânduri în plus: codul angajamentului, indicatorul, referința destinatarului și codul programului.</mark>

## Modul de afișare
<!-- slice: 000T-10 -->
target: ExtraseForm>ExtrasePanel.tree
part: header.right
Iconița din dreapta antetului arborelui alege <b>modul de afișare</b>: «antet + operații» sau «operații + detalii». Funcționează ca în vederea «Extrase». Apasă «Înainte».

## Descărcarea din FOREXE
<!-- slice: 000T-10 -->
target: ExtraseForm.btnDescarca
Butonul <b>«Descarcă extrasele din FOREXE»</b> aduce extrasele noi și le importă; lista se reîncarcă după import.<BR>
<mark>Dacă nu există o sesiune FOREXE, K-BOT se conectează întâi.</mark> Apasă «Înainte».

## Închide fereastra
<!-- slice: 000T-10 -->
target: ExtraseForm.btnInchide
wait: click
Apasă <b>«Închide»</b>. Vederea «Extrase» din fereastra principală se reîncarcă singură.
