---
id: contabil.forexe.coada
title: Coada robotului
part: contabil
order: 60
parent: contabil.forexe
screens: RobotQueueForm, ForexeFooterView.btnCoada
keywords: coada, coada robotului, sarcini, pauza, scoate, goleste coada, opreste curenta, asteapta, ordine, mai multe descarcari
---
<!-- slice: 0098, 0000-31, 0109 -->
Robotul face **o singură lucrare pe rând**. Dacă apeși pe mai multe butoane de descărcare una după
alta (de exemplu reîmprospătarea mai multor angajamente), lucrările nu se calcă între ele: intră în
**coadă** și rulează în ordinea în care le-ai cerut.

Prin coadă trec: reîmprospătarea unui angajament (iconița de la capătul rândului), «Actualizează
angajamente...» din meniul listei (câte o lucrare pe angajament bifat, când descărcarea pe mai multe
taburi e oprită), «Actualizează» din subsolul listei, reîmprospătarea recepțiilor și a rezervărilor, descărcarea extraselor, trimiterea unui
document de fundamentare în FOREXE, «Definitivează» și «Derulează», încărcarea unei note de corecție CAB
și o operațiune salvată de tine direct în pagina FOREXE (aceasta trece **în fața** celor care așteaptă,
fiindcă ține de pagina FOREXE așa cum ai lăsat-o).

Dacă ceri ceva ce e deja în coadă sau în lucru, cererea nu se mai adaugă, iar consola scrie
«… este deja în coada robotului». Când ceva intră în coadă, consola spune ce poziție are.

## Butonul «Coadă N»
<!-- slice: 0098, 0100-03, 0000-31, 0000-48 -->

În banda FOREXE, în dreapta, butonul **«Coadă N»** (N = câte sarcini sunt: cea în lucru plus cele care
așteaptă) **se vede doar cât coada are ceva în ea**; când se golește, dispare. Dacă coada e în pauză,
pe buton apare «‖». Un clic deschide fereastra cozii.

Fereastra se deschide **și singură**, în clipa în care o sarcină trebuie să aștepte după alta sau când pornește
o actualizare a **două sau mai multor** angajamente deodată, fără să-ți ia tastatura. O găsești jos, în dreapta ferestrei principale. Se închide singură după ce coada s-a golit.
Dacă o închizi tu cât mai sunt sarcini, nu se mai redeschide singură până ce coada s-a golit o dată; butonul
«Coadă N» o deschide oricând.

<!-- capture: coada-robot | caption: Fereastra «Coada robotului» cu o sarcină în lucru și două în așteptare | prepare: Porniți reîmprospătarea a trei angajamente la rând (iconița de la capătul rândului din listă), apoi apăsați «Coadă N» din banda FOREXE. -->

## Fereastra «Coada robotului»
<!-- slice: 0098, 0100-03, 0000-31, 0000-48 -->

- **Sus** scrie ce lucrează robotul acum («În lucru: …») și, dacă e cazul, ce așteaptă («Pauză după ea.»;
  sau că coada așteaptă după tine cât e deschisă fereastra Asocieri). Fără nicio sarcină: «Nicio sarcină în
  lucru.» sau «Coada e în pauză.».
- **Lista** are sarcinile care așteaptă, în ordinea în care vor rula, cu ora la care au intrat. Fiecare rând are un **«X»** care scoate din coadă sarcina aceea; sarcina în lucru nu se poate scoate de aici.
- **Pauză** — sarcina în lucru se termină, cele care urmează nu mai pornesc. Butonul devine **Continuă**:
  coada pornește din nou, în aceeași ordine.
- **Golește coada** — scoate toate sarcinile care așteaptă, după ce confirmi. Sarcina în lucru se
  termină normal.
- **Oprește curenta** — oprește robotul din sarcina în lucru, ca «Anulează» din [consolă](topic:contabil.forexe.consola).
  Merge doar cât robotul lucrează în FOREXE; ce s-a salvat deja rămâne. La o actualizare a mai multor
  angajamente oprește **toate** descărcările; ca să oprești una singură, apeși «X»-ul rândului ei.

## Mai multe angajamente deodată
<!-- slice: 0100-03, 0000-48 -->

Când actualizezi **două sau mai multe** angajamente deodată (vezi [Descărcări multiple](topic:contabil.forexe.descarcare-multipla)),
fereastra se deschide singură și, sub rândul «În lucru», arată **câte un rând pentru fiecare angajament
care se descarcă acum**:

- **Codul angajamentului.**
- **O bară de progres** care se umple pe măsură ce robotul trece prin pașii descărcării acelui angajament.
  Bara nu merge uniform: stă mai mult la pașii mari (recepțiile, istoricul) și apoi sare.
- **Un «X»** care oprește **doar descărcarea aceea**. Celelalte merg mai departe. Tabul ei din FOREXE se
  închide, rândul scrie o clipă «(se oprește...)» și apoi dispare. Ce s-a citit din acel angajament până
  atunci **nu se salvează**, iar K-BOT nu îl trece printre cele eșuate: l-ai oprit tu. Îl poți descărca
  din nou oricând.

<!-- capture: coada-descarcari-multiple | caption: «Coada robotului» la o actualizare multiplă: un rând cu bară de progres și «X» pentru fiecare angajament în lucru, iar dedesubt lista celor care așteaptă | prepare: Cu descărcările multiple pornite în Setări, bifați cel puțin 4 angajamente în «Actualizează angajamente» și setați 2 taburi; fotografiați fereastra cât ambele rânduri sunt în lucru, iar dedesubt se vede lista «Încă N în coadă:». -->

Când o descărcare se termină, rândul ei **dispare**, iar în locul lui apare următorul angajament din
rând. Sub rânduri, **lista** arată **angajamentele care mai așteaptă** un tab liber («Încă N în coadă:»), în ordinea în care vor porni, fiecare cu un **«X»** care îl scoate din descărcare (nu mai primește tab). Dacă nu mai așteaptă niciunul, lista dispare, iar fereastra se micșorează; rămân doar rândurile descărcărilor în lucru.

Pentru **un singur angajament** nu se arată rândurile: fereastra rămâne ca pentru orice altă sarcină.

## Cât lucrează robotul
<!-- slice: 0098, 0100, 0000-31 -->

- O [actualizare a mai multor angajamente deodată](topic:contabil.forexe.descarcare-multipla) e **o singură
  sarcină** în coadă, oricâte angajamente ar avea.
- Cât robotul descarcă din FOREXE, o vedere sau o listă pe care o deschizi poate arăta bara de așteptare
  până se termină lucrarea; apoi se completează singură. Poate dura câteva minute.
- Cât e deschisă fereastra [Asocieri](topic:contabil.asocieri), robotul așteaptă după tine: nu pornește
  următoarea sarcină, deci nu poți avea două ferestre Asocieri deodată. Fereastra cozii e și ea
  dezactivată cât ține o casetă a sarcinii în lucru (Asociere sau alta).

Turul ghidat «Coada robotului» pornește din «?» din bara ferestrei cozii și merge doar cu fereastra deschisă, adică cât robotul are ceva în coadă.
