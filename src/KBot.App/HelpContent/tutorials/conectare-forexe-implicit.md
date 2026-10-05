---
id: conectare-forexe-implicit
title: Conectare la FOREXE cu certificatul implicit
part: contabil
keywords: conectare conectez forexe certificat implicit token pin portal sesiune autentificare
starts: KbotForm
host-key: conectare-forexe-implicit
---
<!-- slice: 000T-10 -->

## Înainte de a începe
<!-- slice: 000T-10 -->
Te conectezi la portalul FOREXE cu <b>certificatul folosit data trecută</b>. Tokenul cu certificatul trebuie să fie introdus în PC.<BR>
<mark>Dacă nu te-ai mai conectat niciodată din K-BOT, nu există un certificat implicit și vei fi întrebat ce certificat folosești. În acest caz urmează <link tutorial="conectare-forexe-selectie">Conectare cu alegerea certificatului</link>.</mark>

## Apasă «Conectare»
<!-- slice: 000T-10 -->
target: ForexeFooterView.btnConectare
when: enabled
wait: click
Apasă butonul <b>«Conectare»</b> din banda de jos a ferestrei. K-BOT pornește browserul FOREXE și te autentifică.<BR>
<mark>Butonul este stins cât timp ești deja conectat.</mark>

## PIN-ul tokenului
<!-- slice: 000T-10 -->
Windows poate cere <b>PIN-ul tokenului</b> într-o fereastră proprie. Scrie-l acolo și confirmă.<BR>
<mark>PIN-ul se tastează doar în fereastra Windows; K-BOT nu îl vede și nu îl păstrează.</mark>

## Urmărește conectarea
<!-- slice: 000T-10 -->
target: KbotForm.forexeFooter
Aici, în banda de jos, vezi cum merge conectarea: bara de progres, mesajul curent (la sfârșit <b>«Conectat.»</b>) și numele certificatului. Când s-a terminat, butonul «Conectare» se stinge și apar butoanele browserului și ale consolei.<BR>
<mark>Dacă apare o eroare, citește mesajul din bandă și încearcă din nou; dacă nu merge, folosește conectarea cu alegerea certificatului.</mark> Apasă «Gata».
