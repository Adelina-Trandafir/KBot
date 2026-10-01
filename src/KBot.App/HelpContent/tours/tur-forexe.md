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
<!-- slice: 0034, 0072 -->
target: ForexeFooterView.lblCert
După conectare, aici scrie certificatul cu care lucrează robotul.

## Ce face robotul acum
<!-- slice: 0034 -->
target: ForexeFooterView.lblStatus
Ultimul mesaj al robotului. Istoricul complet e în consolă.

## Istoricul sesiunii
<!-- slice: 0034, 0040 -->
target: ForexeFooterView.btnIstoric
Toate acțiunile duse prin FOREXE în sesiunea curentă, cu rezultatul și jurnalul fiecăreia.

## Browserul robotului
<!-- slice: 0070, 0073 -->
target: ForexeFooterView.btnBrowser
Arată browserul robotului. Atenție: acolo lucrezi pe serverul FOREXE real; ce modifici de mână ocolește K-BOT.

## Consola
<!-- slice: 0034, 0071 -->
target: ForexeFooterView.btnExtinde
Deschide consola FOREXE: progresul pas cu pas, jurnalul și butonul «Anulează».

## Descărcarea unui angajament
<!-- slice: 0055, 0060, 0048-04, 0000-23 -->
target: KbotForm.tree
part: node.icon
Fiecare angajament din listă are la capătul rândului o iconiță de reîmprospătare, care apare când ții mouse-ul pe rând (acum o vezi pe un rând ca exemplu). Apăsată, întâi alegi ce recepții se citesc din nou (fereastra «Ce recepții reîmprospătez?»), apoi robotul citește din FOREXE angajamentul. Dacă rămân modificări neașezate pe recepții, se deschide fereastra «Asocieri».
