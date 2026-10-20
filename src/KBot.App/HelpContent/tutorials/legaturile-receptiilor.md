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
<!-- slice: 000T-10, 0114-16, 0000-91 -->
wait: opens:AsociereForm
În ribbon, în grupul <b>«Recepții»</b> din dreapta (lângă «Închide»), apasă butonul <b>«Asociază recepții»</b>: se deschide editorul de legături recepție › instantaneu.<BR>
<mark>Se poate deschide oricând, nu doar după o descărcare. Legăturile pe care s-a construit o ordonanțare sau peste care s-au calculat plăți rămân vizibile, dar nu se mai pot muta.</mark>

## Ce vezi în fereastră
<!-- slice: 000T-10, 000T-13 -->
target: AsociereForm.lblIntro
Textul de aici spune pe scurt ce se poate face: tragi un instantaneu peste recepția lui sau înapoi, la dreapta, ca să-l desprinzi. Apasă «Înainte» (săgeata verde cu bară, jos în dreapta, ⇥).

## Recepții și lanțurile lor
<!-- slice: 000T-10, 000T-13 -->
target: AsociereForm.treeLant
Arborele din stânga are <b>recepțiile angajamentului</b>, iar sub fiecare recepție <b>lanțul ei de instantanee</b> (starea recepției în timp).<BR>
<mark>Locul unui instantaneu în lanț îl dă ora lui; în timpul tragerii se vede unde ar cădea.</mark> Apasă «Înainte» (săgeata verde cu bară, jos în dreapta, ⇥).

## Instantanee neașezate
<!-- slice: 000T-10, 000T-13 -->
target: AsociereForm.treeLibere
Arborele din dreapta are <b>instantaneele care nu aparțin încă niciunei recepții</b>. Ele se așază tragându-le peste recepția lor. Apasă «Înainte» (săgeata verde cu bară, jos în dreapta, ⇥).

## Tabelul recepției alese
<!-- slice: 000T-10, 000T-13 -->
target: AsociereForm.gridLant
Sub arborele cu lanțuri vezi <b>liniile instantaneului ales</b>: indicatorul, SSI, creditul și valoarea. Alege un instantaneu ca să le vezi. Apasă «Înainte» (săgeata verde cu bară, jos în dreapta, ⇥).

## Tabelul instantaneului neașezat
<!-- slice: 000T-10, 000T-13 -->
target: AsociereForm.gridLibere
Sub arborele cu instantanee neașezate vezi <b>liniile instantaneului ales de acolo</b>, ca să recunoști ce recepție i se potrivește înainte să-l tragi. Apasă «Înainte» (săgeata verde cu bară, jos în dreapta, ⇥).

## Mută o asociere
<!-- slice: 000T-10, 000T-13 -->
target: AsociereForm.treeLant
<b>Trage</b> un instantaneu peste recepția lui, sau înapoi la dreapta ca să-l desprinzi. Cu <b>Ctrl</b> sau <b>Shift</b> alegi mai multe deodată și le tragi împreună.<BR>
<mark>Clic dreapta pe un instantaneu: «Desprinde de recepție», «Începe o recepție nouă», «Nu consemnează nicio schimbare» / «Consemnează o schimbare».</mark> Apasă «Înainte» (săgeata verde cu bară, jos în dreapta, ⇥) după ce ai mutat ce voiai.

## Grafice și benzi
<!-- slice: 000T-10, 000T-13 -->
target: AsociereForm.btnGrafice
optional: yes
why: Graficele sunt doar un ajutor vizual; nu schimbă nimic în date.
Butonul <b>«Grafice și benzi»</b> deschide, într-o fereastră pe care o poți mări cât ecranul, evoluția valorii și așezarea instantaneelor. Apasă «Înainte» (săgeata verde cu bară, jos în dreapta, ⇥).

## Golește așezările
<!-- slice: 000T-10, 000T-13 -->
target: AsociereForm.btnReseteaza
optional: yes
why: Folosești golirea doar dacă vrei să o iei de la capăt cu modificările nesalvate.
<b>«Golește așezările»</b> anulează tot ce ai mutat până acum și te întoarce la ce e salvat. Apasă «Înainte» (săgeata verde cu bară, jos în dreapta, ⇥).

## Salvează legăturile
<!-- slice: 000T-10 -->
target: AsociereForm.btnSalveaza
wait: click
guard: yes
optional: yes
why: Salvezi doar dacă ai mutat ceva; altfel închide fereastra cu «Renunță».
Apasă <b>«Salvează legăturile»</b>. Nimic nu pleacă spre server până la acest buton, iar după salvare recepțiile se reîncarcă.<BR>
<mark>«Renunță» închide fereastra fără să salveze.</mark>
