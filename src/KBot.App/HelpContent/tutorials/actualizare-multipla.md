---
id: actualizare-multipla
title: Actualizează mai multe angajamente deodată
part: contabil
keywords: actualizare actualizez multipla mai multe angajamente deodata descarc forexe bifez vechi neactualizate coada taburi
starts: KbotForm
host-key: actualizare-multipla
---
<!-- slice: 000T-10 -->

## Iconița de actualizare a arborelui
<!-- slice: 000T-10, 0114-12, 0000-90 -->
target: KbotForm.tree
part: footer.right
wait: click
Apasă <b>iconița de reîmprospătare</b> din dreapta jos, sub lista de angajamente (sau textul «Actualizează» de lângă ea). Se deschide un meniu mic, deasupra ei.

## «Actualizează angajamente VRAC»
<!-- slice: 000T-10, 0114-12, 0000-90 -->
anchor: popup.vrac
wait: opens:ActualizareMultiplaForm
Alege <b>«Actualizează angajamente VRAC»</b>. Se deschide fereastra cu lista angajamentelor.

## Lista angajamentelor
<!-- slice: 000T-10 -->
target: ActualizareMultiplaForm.grilaAngajamente
Lista are <b>aceeași ordine ca arborele</b>. Pe fiecare rând vezi codul, denumirea, starea și data ultimei actualizări. Apasă «Înainte».

## Bifează cele vechi
<!-- slice: 000T-10 -->
target: ActualizareMultiplaForm.btnVechi
wait: click
optional: yes
why: Poți bifa angajamentele și de mână, fără acest buton.
Butonul <b>«Bifează cele neactualizate de ... zile»</b> bifează dintr-un clic angajamentele descărcate deja, dar neactualizate de cel puțin numărul de zile din Setări › Aplicație.

## Bifează angajamentele
<!-- slice: 000T-10 -->
target: ActualizareMultiplaForm.grilaAngajamente
Bifează în prima coloană <b>angajamentele pe care vrei să le aduci la zi</b>. Sub listă vezi câte ai bifat. Apoi apasă «Înainte».

## Actualizează
<!-- slice: 000T-10 -->
target: ActualizareMultiplaForm.btnActualizeaza
wait: click
Apasă <b>«Actualizează»</b>.<BR>
<mark>Cu descărcarea pe mai multe taburi activată în Setări, angajamentele bifate se descarcă deodată, fiecare pe tabul lui; cele peste numărul de taburi așteaptă la rând. Fără ea, se descarcă una după alta, în ordinea din listă, în «Coada robotului».</mark>

## Urmărește progresul
<!-- slice: 000T-10 -->
target: KbotForm.forexeFooter
În banda de jos vezi <b>progresul descărcării</b>. Butonul «Consolă» arată detaliile, iar «Coadă» (apare când sunt lucrări la rând) deschide coada robotului. Apasă «Gata».
