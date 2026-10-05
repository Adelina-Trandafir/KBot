---
id: descarcare-angajamente-noi
title: Descarcă angajamentele noi din FOREXE
part: contabil
keywords: descarc descarcare angajamente noi lista forexe actualizez reimprospatez adaug nou arbore sfarsit
starts: KbotForm
host-key: descarcare-angajamente-noi
---
<!-- slice: 000T-10 -->

## Iconița din dreapta jos
<!-- slice: 000T-10 -->
target: KbotForm.tree
part: footer.right
wait: click
Apasă <b>iconița de reîmprospătare</b> din dreapta jos, sub lista de angajamente. K-BOT citește din FOREXE lista angajamentelor curente.<BR>
<mark>Cele noi se adaugă goale, doar cu antetul (fără indicatori, recepții sau plăți); cele existente rămân neatinse. Se trece prin «Coada robotului», dacă mai e o lucrare în curs.</mark>

## Urmărește progresul
<!-- slice: 000T-10 -->
target: KbotForm.forexeFooter
În banda de jos vezi progresul. La sfârșit nu apare nicio fereastră: o descărcare reușită e tăcută, iar cifrele (câte sunt în FOREXE, câte s-au adăugat) merg doar în jurnalul mesajelor.

## Unde apar cele noi (după nume)
<!-- slice: 000T-10 -->
target: KbotForm.tree
when: condition:sort-name
Arborele e sortat <b>după nume</b>: angajamentele noi apar la locul lor <b>alfabetic</b>, printre celelalte, pentru sursa-sectorul ales în bara de titlu.<BR>
<mark>Dacă sursa-sectorul ales nu e cea a angajamentului nou, nu-l vei vedea: schimbă sursa-sectorul din bara de titlu.</mark> Apasă «Înainte».

## Unde apar cele noi (după data creării)
<!-- slice: 000T-10 -->
target: KbotForm.tree
when: condition:sort-date
Arborele e sortat <b>după data creării</b>. Un angajament abia adăugat nu are încă dată de creare (ea vine odată cu descărcarea lui completă), așa că stă la <b>sfârșitul listei</b>, în ordine alfabetică, după cele cu dată.<BR>
<mark>Arborele arată toate sursele anului, deci nu trebuie schimbată nicio sursă ca să-l găsești.</mark> Apasă «Înainte».

## Descarcă-l complet
<!-- slice: 000T-10 -->
target: KbotForm.tree
wait: click
optional: yes
why: Un angajament nou poate fi descărcat complet și mai târziu.
Ca să aduci <b>tot angajamentul</b> (indicatori, recepții, plăți), selectează-l și apasă butonul din dreapta rândului lui, care apare la survolare.<BR>
<mark>Dacă descărcarea pe mai multe taburi e activată în Setări, K-BOT te întreabă după reîmprospătare dacă vrei să le descarce chiar acum pe cele noi.</mark>
