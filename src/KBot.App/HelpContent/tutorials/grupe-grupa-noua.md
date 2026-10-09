---
id: grupe-grupa-noua
title: Adaugă o grupă nouă
part: contabil
keywords: grupa noua adaug adaugare grup grupe angajamente denumire culoare bifez salvez
starts: GrupeForm
host-key: grupe-grupa-noua
---
<!-- slice: 000T-12 -->

## «+ Adăugare grupă»
<!-- slice: 000T-12 -->
target: GrupeForm.tree
part: footer.left
wait: click
Apasă <b>«+ Adăugare grupă»</b>, din subsolul arborelui. În arbore apare rândul «Grupă nouă», iar în tabel apar <b>toate angajamentele, nebifate</b>.

## Denumirea grupei
<!-- slice: 000T-12 -->
target: GrupeForm.txtDenumire
wait: changed
Scrie <b>denumirea grupei</b>, apoi treci în alt câmp sau apasă Enter. Două grupe nu pot avea aceeași denumire.

## Culoarea grupei
<!-- slice: 000T-12 -->
target: GrupeForm.btnCuloare
allow: GrupeForm.btnCuloare
Butonul arată <b>culoarea grupei</b>; implicit e negru. Apasă-l ca să alegi altă culoare din paleta de culori. Cu această culoare apar grupa în meniu și angajamentele ei în lista principală.<BR>
<mark>Alegerea culorii nu e obligatorie. Când ai terminat, apasă «Înainte».</mark>

## Bifează angajamentele
<!-- slice: 000T-12 -->
target: GrupeForm.gridAng
Bifează în prima coloană <b>angajamentele care fac parte din grupă</b>. Cu mouse-ul pe coloana «Indicatori» vezi ce indicatori are fiecare. În coloana «Alias» poți scrie un nume scurt pentru un angajament; se salvează singur. Apoi apasă «Înainte».

## Salvează
<!-- slice: 000T-12 -->
target: GrupeForm.btnSalveaza
wait: click
guard: yes
Apasă <b>«Salvează»</b>. Grupa se salvează dacă are <b>denumire, culoare și cel puțin un angajament bifat</b>; altfel K-BOT îți spune ce lipsește.
