---
id: tutorial-pornire
title: Primii pași în K-BOT
part: contabil
keywords: tutorial inceput pornire primii pasi ghid panglica ribbon token anaf lista angajamentelor ce este nou
starts: AvacontMain
host-key: tutorial-pornire
---
<!-- slice: 000T-13 -->
## Panglica de sus
<!-- slice: 000T-13 -->
anchor: ribbon-tab.configurari
wait: signal:ribbon-tab:configurari
Bine ai venit în K-BOT! Îți arăt în câțiva pași unde găsești lucrurile de bază. K-BOT nu apasă nimic în locul tău: urmărești chenarul de pe ecran și faci tu pasul. Poți ieși oricând cu «Mă opresc».<br><br>Începem cu panglica de sus. Apasă fila <b>Configurări</b>.

## Fila E-Factura
<!-- slice: 000T-13 -->
anchor: ribbon-tab.e-factura
wait: signal:ribbon-tab:e-factura
Fila <b>E-Factura</b> adună facturile emise, cele primite și tokenul ANAF. Apasă-o.

## Token ANAF
<!-- slice: 000T-13 -->
anchor: ribbon.ef_token
wait: opens:TokenForm
Pentru E-Factura unitatea are nevoie de un token valabil de la ANAF. Apasă <b>Token ANAF</b>: se deschide fereastra lui, unde vezi până când e valabil.

## Reînnoiește tokenul
<!-- slice: 000T-13 -->
target: TokenForm.btnAutorizeaza
wait: click
optional: yes
why: Reînnoiești tokenul doar dacă a expirat sau expiră curând; altfel treci mai departe.
Dacă tokenul a expirat sau expiră curând, îl reînnoiești de aici, cu certificatul calificat (butonul se numește «Autorizează» dacă unitatea nu are încă token). Dacă nu e nevoie, sari peste acest pas.

## Închide fereastra
<!-- slice: 000T-13 -->
target: TokenForm.btnInchide
wait: click
Apasă <b>Închide</b> ca să te întorci în fereastra principală.

## Fila Extrase
<!-- slice: 000T-13 -->
anchor: ribbon-tab.extrase
wait: signal:ribbon-tab:extrase
Fila <b>Extrase</b> deschide extrasele de cont. Apasă-o.

## Fila Angajamente
<!-- slice: 000T-13 -->
anchor: ribbon-tab.angajamente
wait: signal:ribbon-tab:angajamente
Fila <b>Angajamente</b> are tot ce ține de angajamente. Apasă-o.

## Lista angajamentelor
<!-- slice: 000T-13 -->
anchor: ribbon.pagina_angajamente
wait: signal:ribbon:pagina_angajamente
Apasă <b>Lista angajamentelor</b>: se deschide arborele cu toate angajamentele unității.

## Alege un angajament
<!-- slice: 000T-13 -->
target: KbotForm.tree
wait: select
Alege un angajament din arbore: apasă pe unul. Vederile lui (Sumar, Rezervări, Recepții, Plăți...) apar în dreapta.

## Butoanele din dreapta sus
<!-- slice: 000T-13 -->
anchor: ribbon.right
Sus, în dreapta panglicii, sunt butoanele paginii deschise: unele țin de vederea în care ești, altele de fereastra întreagă (de exemplu «Descarcă extrase» sau «Editează grupe»). Se schimbă după pagina de pe ecran. Cel mai din dreapta, <b>Închide</b>, închide pagina. Apasă «Înainte» (săgeata verde cu bară, jos în dreapta, ⇥).

## Închide pagina
<!-- slice: 000T-13 -->
anchor: ribbon.pagina_inchide
wait: signal:ribbon:pagina_inchide
Apasă <b>Închide</b> ca să părăsești lista angajamentelor și să te întorci la pagina «Acasă».

## Ce este nou
<!-- slice: 000T-13 -->
target: AcasaPage.btnNoutati
wait: click
Pe pagina «Acasă», cardul <b>«Ce este nou»</b> arată ce s-a schimbat în ultimele versiuni. Cu acest buton îl strângi sau îl deschizi la loc; K-BOT ține minte ce ai ales. Apasă-l.

## Gata
<!-- slice: 000T-13 -->
Asta a fost tot. Poți relua oricând tutorialul din butonul «?», sub «Tutoriale».<br><br>Nu vrei să mai pornească singur la deschidere? Debifează-l în Configurare K-BOT › Aplicație › Generale. Apasă «Gata».
