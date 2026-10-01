---
id: contabil.forexe.coada
title: Coada robotului
part: contabil
order: 60
parent: contabil.forexe
screens: RobotQueueForm, ForexeFooterView.btnCoada
keywords: coada, coada robotului, sarcini, pauza, scoate, goleste coada, opreste curenta, asteapta, ordine, mai multe descarcari
---
<!-- slice: 0098, 0000-31 -->
Robotul face **o singură lucrare pe rând**. Dacă apeși pe mai multe butoane de descărcare una după
alta (de exemplu reîmprospătarea mai multor angajamente), lucrările nu se calcă între ele: intră în
**coadă** și rulează în ordinea în care le-ai cerut.

Prin coadă trec: reîmprospătarea unui angajament (iconița de la capătul rândului), «Actualizează» din
subsolul listei, reîmprospătarea recepțiilor și a rezervărilor, descărcarea extraselor, trimiterea unui
document de fundamentare în FOREXE, «Definitivează» și «Derulează», încărcarea unei note de corecție CAB
și o operațiune salvată de tine direct în pagina FOREXE (aceasta trece **în fața** celor care așteaptă,
fiindcă ține de pagina FOREXE așa cum ai lăsat-o).

Dacă ceri ceva ce e deja în coadă sau în lucru, cererea nu se mai adaugă, iar consola scrie
«… este deja în coada robotului». Când ceva intră în coadă, consola spune ce poziție are.

## Butonul «Coadă N»
<!-- slice: 0098, 0000-31 -->

În banda FOREXE, în dreapta, butonul **«Coadă N»** (N = câte sarcini sunt: cea în lucru plus cele care
așteaptă) **se vede doar cât coada are ceva în ea**; când se golește, dispare. Dacă coada e în pauză,
pe buton apare «‖». Un clic deschide fereastra cozii.

Fereastra se deschide **și singură**, în clipa în care o sarcină trebuie să aștepte după alta, fără să-ți
ia tastatura. O găsești jos, în dreapta ferestrei principale. Se închide singură după ce coada s-a golit.
Dacă o închizi tu cât mai sunt sarcini, nu se mai redeschide singură până ce coada s-a golit o dată; butonul
«Coadă N» o deschide oricând.

<!-- capture: coada-robot | caption: Fereastra «Coada robotului» cu o sarcină în lucru și două în așteptare | prepare: Porniți reîmprospătarea a trei angajamente la rând (iconița de la capătul rândului din listă), apoi apăsați «Coadă N» din banda FOREXE. -->

## Fereastra «Coada robotului»
<!-- slice: 0098, 0000-31 -->

- **Sus** scrie ce lucrează robotul acum («În lucru: …») și, dacă e cazul, ce așteaptă («Pauză după ea.»;
  sau că coada așteaptă după tine cât e deschisă fereastra Asocieri). Fără nicio sarcină: «Nicio sarcină în
  lucru.» sau «Coada e în pauză.».
- **Lista** are sarcinile care așteaptă, în ordinea în care vor rula, cu ora la care au intrat.
- **Pauză** — sarcina în lucru se termină, cele care urmează nu mai pornesc. Butonul devine **Continuă**:
  coada pornește din nou, în aceeași ordine.
- **Scoate** — scoate din coadă sarcina aleasă în listă. Sarcina în lucru nu se poate scoate de aici.
- **Golește coada** — scoate toate sarcinile care așteaptă, după ce confirmi. Sarcina în lucru se
  termină normal.
- **Oprește curenta** — oprește robotul din sarcina în lucru, ca «Anulează» din [consolă](topic:contabil.forexe.consola).
  Merge doar cât robotul lucrează în FOREXE; ce s-a salvat deja rămâne.

## Cât lucrează robotul
<!-- slice: 0098, 0000-31 -->

- Cât robotul descarcă din FOREXE, o vedere sau o listă pe care o deschizi poate arăta bara de așteptare
  până se termină lucrarea; apoi se completează singură. Poate dura câteva minute.
- Cât e deschisă fereastra [Asocieri](topic:contabil.asocieri), robotul așteaptă după tine: nu pornește
  următoarea sarcină, deci nu poți avea două ferestre Asocieri deodată. Fereastra cozii e și ea
  dezactivată cât ține o casetă a sarcinii în lucru (Asociere sau alta).

Turul ghidat «Coada robotului» pornește din «?» din bara ferestrei cozii și merge doar cu fereastra deschisă, adică cât robotul are ceva în coadă.
