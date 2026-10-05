---
id: conectare-forexe-selectie
title: Conectare la FOREXE cu alegerea certificatului
part: contabil
keywords: conectare conectez forexe certificat alegere selectie token pin portal sesiune autentificare alt certificat
starts: KbotForm
host-key: conectare-forexe-selectie
---
<!-- slice: 000T-10 -->

## Înainte de a începe
<!-- slice: 000T-10 -->
Aici <b>alegi tu certificatul</b> cu care te conectezi la FOREXE: la prima conectare, când ai mai multe certificate sau când vrei să schimbi tokenul. Tokenul trebuie să fie introdus în PC.<BR>
<mark>Dacă folosești mereu același certificat, e mai simplu <link tutorial="conectare-forexe-implicit">Conectarea cu certificatul implicit</link>.</mark>

## Deschide alegerea certificatului
<!-- slice: 000T-10 -->
target: ForexeFooterView.btnSelectieCertificate
when: enabled
wait: opens:CertificateSelectionForm
Apasă butonul de lângă «Conectare», cel care <b>deschide fereastra de selecție a certificatelor</b>.<BR>
<mark>Butonul este stins cât timp ești deja conectat.</mark>

## Dacă lipsește certificatul
<!-- slice: 000T-10 -->
target: CertificateSelectionForm.btnRefresh
wait: click
optional: yes
why: Reîncarci lista doar dacă certificatul tău nu apare în ea.
Dacă certificatul tău nu se vede în listă (de exemplu tokenul tocmai a fost introdus), apasă <b>reîncărcarea listei</b>.

## Alege certificatul
<!-- slice: 000T-10 -->
target: CertificateSelectionForm.lstCertificates
Din listă alege <b>certificatul tău</b>, apoi apasă «Înainte» aici.<BR>
<mark>Prima poziție din listă este certificatul confirmat data trecută.</mark>

## Confirmă
<!-- slice: 000T-10 -->
target: CertificateSelectionForm.btnSelect
wait: click
Apasă <b>«Confirmă»</b>.<BR>
<mark>Nu apăsa «Fără Token» pentru o conectare obișnuită: acel buton continuă fără certificat.</mark>

## PIN-ul tokenului
<!-- slice: 000T-10 -->
Windows poate cere <b>PIN-ul tokenului</b> într-o fereastră proprie. Scrie-l acolo și confirmă.<BR>
<mark>PIN-ul se tastează doar în fereastra Windows; K-BOT nu îl vede și nu îl păstrează.</mark>

## Urmărește conectarea
<!-- slice: 000T-10 -->
target: KbotForm.forexeFooter
În banda de jos vezi cum merge conectarea: bara de progres, mesajul curent (la sfârșit <b>«Conectat.»</b>) și numele certificatului ales. Apasă «Gata».
