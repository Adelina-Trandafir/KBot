---
id: tur-forexe
title: Legătura cu FOREXE
part: contabil
topic: contabil.forexe
---
## Banda FOREXE
<!-- slice: 0034, 0040 -->
target: KbotForm.forexeFooter
Tot ce ține de FOREXE se vede în banda de jos a ferestrei principale. Robotul K-BOT lucrează în FOREXE cu certificatul tău digital.

## Conectare
<!-- slice: 0034, 0084 -->
target: ForexeFooterView.btnConectare
Introdu tokenul cu certificatul în calculator, apoi apasă «Conectare». Fără token, conectarea nu se poate face.

## Alt certificat
<!-- slice: 0034, 0072 -->
target: ForexeFooterView.btnSelectieCertificate
Dacă ai mai multe certificate, de aici îl alegi pe cel cu care te conectezi.

## Certificatul folosit
<!-- slice: 0034, 0072, 0000-30 -->
target: ForexeFooterView.lblCert
Aici scrie certificatul cu care lucrează robotul. Eticheta apare doar după ce s-a ales un certificat (cu «Conectare» sau din iconița de alegere); până atunci nu se vede.

## Ce face robotul acum
<!-- slice: 0034 -->
target: ForexeFooterView.lblStatus
Ultimul mesaj al robotului. Istoricul complet e în consolă.

## Istoricul sesiunii
<!-- slice: 0034, 0040, 0000-30 -->
target: ForexeFooterView.btnIstoric
Toate acțiunile duse prin FOREXE în sesiunea curentă, cu rezultatul și jurnalul fiecăreia. În K-BOT-ul de acum butonul e ascuns în bandă și nicio setare nu-l arată; îl vezi aici doar pentru tur.

## Browserul robotului
<!-- slice: 0070, 0073, 0000-30 -->
target: ForexeFooterView.btnBrowser
Arată browserul robotului. Apare doar cât ești conectat la FOREXE și cât e bifat în Setări › Aplicație «Butonul «Arată browserul» în banda FOREXE». Atenție: acolo lucrezi pe serverul FOREXE real; ce modifici de mână ocolește K-BOT.

## Consola
<!-- slice: 0034, 0071, 0000-30 -->
target: ForexeFooterView.btnExtinde
Deschide consola FOREXE: progresul pas cu pas, jurnalul și butonul «Anulează». Apare doar cât ești conectat la FOREXE.

## Coada robotului
<!-- slice: 0098, 0000-31 -->
target: ForexeFooterView.btnCoada
Robotul face o singură lucrare pe rând; ce ceri între timp așteaptă în coadă, în ordine. Butonul «Coadă N» (N = sarcina în lucru plus cele care așteaptă) apare doar cât coada are ceva în ea (de exemplu după ce ai pornit mai multe reîmprospătări la rând) și dispare când se golește. «‖» pe el înseamnă pauză. Un clic deschide fereastra cozii; turul «Coada robotului» o arată pe larg.

## Descărcarea unui angajament
<!-- slice: 0055, 0060, 0048-04, 0000-23 -->
target: KbotForm.tree
part: node.icon
Fiecare angajament din listă are la capătul rândului o iconiță de reîmprospătare, care apare când ții mouse-ul pe rând (acum o vezi pe un rând ca exemplu). Apăsată, întâi alegi ce recepții se citesc din nou (fereastra «Ce recepții reîmprospătez?»), apoi robotul citește din FOREXE angajamentul. Dacă rămân modificări neașezate pe recepții, se deschide fereastra «Asocieri».
