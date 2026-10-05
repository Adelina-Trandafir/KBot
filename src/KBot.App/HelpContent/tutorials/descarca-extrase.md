---
id: descarca-extrase
title: Descarcă extrasele de cont din FOREXE
part: contabil
keywords: extrase extras cont descarc descarcare forexe snm banca import la zi
starts: KbotForm
host-key: descarca-extrase
requires: forexe
---
<!-- slice: 000T-10 -->

## Despre descărcare
<!-- slice: 000T-10 -->
K-BOT aduce din FOREXE <b>extrasele de cont noi</b> și le importă în baza de date. Se descarcă doar ce e mai nou decât ultimul extras importat.<BR>
<mark>Acest tutorial apare doar cât ești conectat la FOREXE.</mark>

## Iconița din stânga jos
<!-- slice: 000T-10 -->
target: KbotForm.tree
part: footer.left
wait: click
Apasă <b>iconița cu cardul</b> din stânga jos, sub lista de angajamente. Descărcarea pornește imediat.<BR>
<mark>Dacă robotul FOREXE mai are o lucrare în curs, descărcarea așteaptă la rând în «Coada robotului».</mark>

## Urmărește progresul
<!-- slice: 000T-10 -->
target: KbotForm.forexeFooter
În banda de jos vezi <b>progresul descărcării</b>: bara de progres și mesajul curent. Butonul «Consolă» arată detaliile.<BR>
<mark>Dacă nu sunt extrase noi nu apare nicio fereastră și nu e o eroare. Ce s-a importat vezi în vederea «Extrase» sau la MENIU › Extrase.</mark> Apasă «Gata».
