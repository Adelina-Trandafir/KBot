---
id: contabil.efactura.token
title: Tokenul ANAF: obținerea și reînnoirea
part: contabil
order: 76
parent: contabil.efactura
screens: TokenForm, CertificatePickerForm, FacturiForm.btnToken
keywords: token, token anaf, autorizare, autorizeaza, reinnoire, reinnoieste tokenul, certificat, certificat calificat, card, pin, expirat, expira, valabil pana, spv, cui, ultima eroare
---
<!-- slice: 00EF-05, 0000-54, 0000-55, 0000-56 -->
Pentru a trimite facturi la ANAF, K-BOT are nevoie de un **token ANAF** al unității: o autorizare pe care o dai o singură dată, cu **certificatul calificat** al unității (cel de pe token sau card), și o reînnoiești cam o dată pe an. Tokenul rămâne pe serverul K-BOT; **nu ajunge pe calculatorul tău**.

Fereastra **«E-Factura — tokenul ANAF»** se deschide din fereastra facturilor emise, cu butonul **«Token ANAF»** din subsol (vezi [E-Factura: facturile emise](topic:contabil.efactura)).

<!-- capture: efactura-token | caption: Fereastra «E-Factura — tokenul ANAF» | prepare: Din fereastra de facturi apăsați «Token ANAF», pe o unitate care are deja un token, ca să se vadă data de expirare și certificatul. -->

## Ce arată fereastra
<!-- slice: 00EF-05, 0000-54, 0000-56 -->

Sus, o frază spune starea tokenului:

- **«Unitatea nu are încă un token ANAF…»** — nu a fost dată încă autorizarea; apasă **«Autorizează»**;
- **«Tokenul ANAF este valabil până la …»** — merge, cu data și numărul de zile rămase;
- **«Tokenul ANAF al unității a expirat sau ANAF nu l-a mai primit…»** — nu se pot face apeluri către ANAF până îl reînnoiești; apasă **«Reînnoiește tokenul»**;
- **«Serverul nu este pregătit pentru E-Factura.»** — serverul K-BOT nu are încă datele aplicației ANAF; butonul de autorizare rămâne stins. Nu ai ce face de la calculatorul tău: anunță administratorul K-BOT.

Dacă tokenul mai merge, dar expiră în **cel mult 7 zile**, apare și un chenar galben: «Tokenul expiră în … zile. Reînnoiți-l din timp…». Avertismentul se vede **în această fereastră**, când o deschizi; nu apare singur în alte părți ale K-BOT.

Dedesubt vezi: unitatea, **codul fiscal (CUI)** pentru care s-a dat tokenul, **certificatul folosit**, **cine a făcut autorizarea și când** și, dacă a fost o problemă, **ultima eroare**. Rândurile care nu au încă valoare rămân goale.

**«Reîmprospătează»** citește din nou starea. **«Închide»** (sau Esc) închide fereastra.

## Cum obții sau reînnoiești tokenul
<!-- slice: 00EF-05, 0000-54, 0000-56 -->

1. **Conectează tokenul sau cardul** cu certificatul calificat al unității și pornește programul lui, dacă îl are. Certificatul trebuie să fie valabil.
2. În fereastră apasă **«Autorizează»** (prima dată) sau **«Reînnoiește tokenul»** (după aceea).
3. Se deschide **«E-Factura — alegerea certificatului»**, cu certificatele găsite: nume, emitent, data până la care sunt valabile și dispozitivul pe care stau. Alege unul (dublu clic sau **«Folosește»**); **«Renunță»** oprește totul fără nicio schimbare.
4. Dacă programul tokenului îți cere **PIN-ul**, îl introduci acolo. K-BOT nu îl vede și nu citește cheia certificatului: aceasta rămâne pe token sau card.
5. După câteva clipe, fereastra arată starea nouă și un chenar verde: **«Tokenul ANAF a fost obținut și este păstrat pe server.»**

Orice utilizator al unității poate face autorizarea. Calculatorul de pe care o faci trebuie să aibă certificatul în Windows, adică tokenul conectat. **Reînnoiește din timp**: după expirarea completă, autorizarea se face de la zero, tot cu «Reînnoiește tokenul».

## Ce se poate întâmpla
<!-- slice: 00EF-05, 0000-54, 0000-56 -->

- **«Nu am găsit niciun certificat calificat valabil pe un token sau card conectat la acest calculator…»** — K-BOT propune doar certificate calificate, valabile azi, ale căror chei stau pe un token sau pe un card (nu cele instalate ca fișier) și care nu sunt de test. Conectează tokenul, instalează programul lui și încearcă din nou.
- **«Conexiunea cu ANAF folosind certificatul nu a reușit…»** — tokenul nu e conectat, PIN-ul a fost greșit sau anulat, sau nu ai internet. Încearcă din nou.
- **«ANAF nu a întors codul de autorizare după 3 încercări…»** — ANAF nu a răspuns cum trebuie; încearcă mai târziu.
- **«Cererea de autorizare a expirat sau nu a pornit de aici…»** — pasul a durat prea mult (peste 15 minute) sau a fost început în altă parte; apasă din nou «Autorizează».
- **Un mesaj de la ANAF despre refuz** — apare cu textul serverului; tokenul rămâne cum era. Ultimul motiv se vede și în fereastră, la «Ultima eroare».
