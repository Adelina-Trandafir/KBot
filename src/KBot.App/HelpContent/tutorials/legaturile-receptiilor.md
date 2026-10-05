---
id: legaturile-receptiilor
title: Modifică asocierea recepțiilor cu instantaneele
part: contabil
keywords: asociere asocieri receptii receptie instantanee instantaneu legaturi legatura lant neasezate trag muta desprind istoric
starts: KbotForm
host-key: legaturile-receptiilor
---
<!-- slice: 000T-10 -->

## Alege un angajament
<!-- slice: 000T-10 -->
target: KbotForm.tree
wait: select
Alege din lista de angajamente <b>angajamentul</b> ale cărui recepții vrei să le corectezi.

## Deschide vederea «Recepții»
<!-- slice: 000T-10 -->
target: KbotForm.navViews
part: item:receptii
wait: tab:receptii
Apasă butonul <b>«Recepții»</b> din stânga ferestrei.

## Editorul de legături
<!-- slice: 000T-10 -->
target: KbotForm>ReceptiiView.tree
part: header.right
wait: opens:AsociereForm
Apasă <b>iconița din dreapta antetului arborelui</b>: se deschide editorul de legături recepție › instantaneu.<BR>
<mark>Se poate deschide oricând, nu doar după o descărcare. Legăturile pe care s-a construit o ordonanțare sau peste care s-au calculat plăți rămân vizibile, dar nu se mai pot muta.</mark>

## Ce vezi în fereastră
<!-- slice: 000T-10 -->
target: AsociereForm.lblIntro
Textul de aici spune pe scurt ce se poate face: tragi un instantaneu peste recepția lui sau înapoi, la dreapta, ca să-l desprinzi. Apasă «Înainte».

## Recepții și lanțurile lor
<!-- slice: 000T-10 -->
target: AsociereForm.treeLant
Arborele din stânga are <b>recepțiile angajamentului</b>, iar sub fiecare recepție <b>lanțul ei de instantanee</b> (starea recepției în timp).<BR>
<mark>Locul unui instantaneu în lanț îl dă ora lui; în timpul tragerii se vede unde ar cădea.</mark> Apasă «Înainte».

## Instantanee neașezate
<!-- slice: 000T-10 -->
target: AsociereForm.treeLibere
Arborele din dreapta are <b>instantaneele care nu aparțin încă niciunei recepții</b>. Ele se așază tragându-le peste recepția lor. Apasă «Înainte».

## Tabelul recepției alese
<!-- slice: 000T-10 -->
target: AsociereForm.gridLant
Sub arborele cu lanțuri vezi <b>liniile instantaneului ales</b>: indicatorul, SSI, creditul și valoarea. Alege un instantaneu ca să le vezi. Apasă «Înainte».

## Tabelul instantaneului neașezat
<!-- slice: 000T-10 -->
target: AsociereForm.gridLibere
Sub arborele cu instantanee neașezate vezi <b>liniile instantaneului ales de acolo</b>, ca să recunoști ce recepție i se potrivește înainte să-l tragi. Apasă «Înainte».

## Mută o asociere
<!-- slice: 000T-10 -->
target: AsociereForm.treeLant
<b>Trage</b> un instantaneu peste recepția lui, sau înapoi la dreapta ca să-l desprinzi. Cu <b>Ctrl</b> sau <b>Shift</b> alegi mai multe deodată și le tragi împreună.<BR>
<mark>Clic dreapta pe un instantaneu: «Desprinde de recepție», «Începe o recepție nouă», «Nu consemnează nicio schimbare» / «Consemnează o schimbare».</mark> Apasă «Înainte» după ce ai mutat ce voiai.

## Grafice și benzi
<!-- slice: 000T-10 -->
target: AsociereForm.btnGrafice
optional: yes
why: Graficele sunt doar un ajutor vizual; nu schimbă nimic în date.
Butonul <b>«Grafice și benzi»</b> deschide, într-o fereastră pe care o poți mări cât ecranul, evoluția valorii și așezarea instantaneelor. Apasă «Înainte».

## Golește așezările
<!-- slice: 000T-10 -->
target: AsociereForm.btnReseteaza
optional: yes
why: Folosești golirea doar dacă vrei să o iei de la capăt cu modificările nesalvate.
<b>«Golește așezările»</b> anulează tot ce ai mutat până acum și te întoarce la ce e salvat. Apasă «Înainte».

## Salvează legăturile
<!-- slice: 000T-10 -->
target: AsociereForm.btnSalveaza
wait: click
optional: yes
why: Salvezi doar dacă ai mutat ceva; altfel închide fereastra cu «Renunță».
Apasă <b>«Salvează legăturile»</b>. Nimic nu pleacă spre server până la acest buton, iar după salvare recepțiile se reîncarcă.<BR>
<mark>«Renunță» închide fereastra fără să salveze.</mark>
