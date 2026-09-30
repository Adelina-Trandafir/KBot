---
id: tur-fereastra
title: Fereastra principală
part: contabil
topic: contabil.fereastra
---
## Bine ai venit
<!-- slice: 0000-04 -->
target: KbotForm
goto: view:sumar
Acesta este un tur scurt al ferestrei principale. Folosește «Înainte» sau săgeata dreapta ca să treci mai departe și «Închide» sau Esc ca să ieși oricând.

## Butonul MENIU
<!-- slice: 0087, 0084, 0088 -->
target: KbotForm.btnMeniu
De aici pornești un angajament nou, deschizi extrasele de cont, nomenclatoarele și, când există, operațiunile necorelate. Semnul (!) pe buton înseamnă că ai operațiuni necorelate de rezolvat.

## Anul de lucru
<!-- slice: 0001, 0086 -->
target: KbotForm.cboAn
Anul pentru care lucrezi. Schimbarea lui reîncarcă lista de angajamente și toate ecranele.

## Sursa și sectorul
<!-- slice: 0001, 0086 -->
target: KbotForm.cboSs
Sursa și sectorul (de exemplu 02A). Ultima alegere se ține minte pentru data viitoare.

## Lista angajamentelor
<!-- slice: 0009, 0777, 0034, 0080-03 -->
target: KbotForm.tree
Aici alegi angajamentul pe care lucrezi. Lupa din capul listei caută, rotița alege sortarea și coloanele.

Jos, iconița din dreapta aduce din FOREXE angajamentele noi, iar cea din stânga deschide extrasele de cont.

## Vederile
<!-- slice: 0018, 0088 -->
target: KbotForm.navViews
Fiecare vedere arată alt fel de date ale angajamentului selectat: Sumar, Istoric, Rezervări, Recepții, Plăți, Extrase, Fundamentare, Ordonanțare, Note corecție. O vedere e gri cât timp angajamentul nu are date de acel fel.

## Vederea aleasă
<!-- slice: 0006 -->
target: KbotForm.viewHost
Aici se vede vederea aleasă, pentru angajamentul selectat în listă.

## Banda FOREXE
<!-- slice: 0034 -->
target: KbotForm.forexeFooter
Conectarea la FOREXE, certificatul, progresul robotului și ultimul lui mesaj. Turul «Legătura cu FOREXE» o arată pe larg.

## Bara de titlu
<!-- slice: 0028-09, 0000-01, 0097, 0000-20 -->
target: KbotForm.capBar
Lângă «K-BOT» e unitatea pe care lucrezi; dacă ai acces la mai multe, un clic pe ea te lasă să treci pe alta, fără parolă (conexiunea FOREXE, dacă e deschisă, se închide singură). În dreapta ai rotița de setări, butonul de temă (culori și mărimea textului) și «?», care deschide un meniu de ajutor: o căsuță în care scrii o întrebare, pagina despre ce ai pe ecran și tururile ghidate. Tasta F1 deschide direct pagina de ajutor, în orice fereastră.
