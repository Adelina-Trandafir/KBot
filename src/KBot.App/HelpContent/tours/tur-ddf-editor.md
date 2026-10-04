---
id: tur-ddf-editor
title: Editorul documentului de fundamentare
part: contabil
topic: contabil.ddf.editor
screens: DdfEditForm
---
## Antetul documentului
<!-- slice: 0051, 0081-09, 0000-28 -->
target: DdfEditForm.tlyAntet
Sus sunt datele documentului: codul angajamentului, numărul CUAL, data creării, totalul, obiectul, programul, compartimentul, partenerul și datele reviziei. Totalul se recalculează singur din secțiunea A.

## Antet › Partener asociat
<!-- slice: 0051, 0094-02, 0000-28 -->
target: DdfEditForm.chkPartAng
Bifează când documentul se leagă de un partener. Partenerul principal se scrie pe toate rândurile din secțiunile A și B. Debifat, documentul nu are partener principal.

## Antet › Partenerul principal
<!-- slice: 0051, 0094-02, 0000-28 -->
target: DdfEditForm.cmbPartener
Aici alegi partenerul principal al documentului, unul singur: scrii începutul numelui sau al codului fiscal și îl alegi din listă. Majoritatea documentelor au doar acest partener. Schimbarea lui rescrie partenerul pe toate rândurile.

## Paginile documentului
<!-- slice: 0051, 0094-02, 0094-03, 0000-28 -->
target: DdfEditForm.navSub
Documentul are mai multe pagini. Urmează pe rând fiecare; cea din dreapta, «Parteneri», stă singură, pentru că nu face parte din revizia propriu-zisă, și se vede doar când «Partener asociat» e bifat.

## Pagini › Secțiunea A
<!-- slice: 0051, 0081-08, 0000-28 -->
target: DdfEditForm.navSub
part: item:sectiunea-a
Rândurile reviziei, pe clasificații: aici tastezi valorile. **Adaugă rând** deschide fereastra rândului nou; **Șterge rândul** scoate linia.

## Pagini › Secțiunea B
<!-- slice: 0051, 0081-02, 0000-28, 0000-30 -->
target: DdfEditForm.navSub
part: item:sectiunea-b
Se calculează din secțiunea A și nu se editează. Pagina se vede abia după ce revizia a fost trimisă în FOREXE (din meniul reviziei, «Trimite în FOREXE»); până atunci butonul ei lipsește.

## Pagini › Descriere
<!-- slice: 0051, 0000-28 -->
target: DdfEditForm.navSub
part: item:descriere
Descrierea scurtă și cea lungă a reviziei (starea de fapt și de drept).

## Pagini › Fișiere
<!-- slice: 0051, 0000-28 -->
target: DdfEditForm.navSub
part: item:fisiere
Atașezi imagini, documente sau tabele. Se încarcă pe server după salvarea documentului.

## Pagini › Parteneri
<!-- slice: 0094-02, 0094-03, 0084-02, 0000-28 -->
target: DdfEditForm.navSub
part: item:parteneri
Pagina din dreapta arată toți partenerii asociați documentului, nu doar pe cel din antet. Apare doar când «Partener asociat» e bifat. Apasă pe ea ca să o deschizi.

## Parteneri › Cum asociezi
<!-- slice: 0094-02, 0084-02, 0000-28 -->
target: DdfEditForm.navSub
part: item:parteneri
Pe pagina «Parteneri» alegi un partener din listă și apeși «Asociază». Partenerul principal, cel din antet, e primul în listă și se schimbă din antet. «Scoate din asociere» scoate oricare alt partener. Lista se salvează odată cu documentul.

## Salvarea documentului
<!-- slice: 0051, 0081-12, 0000-28 -->
target: DdfEditForm.btnSalveaza
Trimite tot documentul într-o singură tranzacție, inclusiv partenerii asociați. Rândurile din secțiunea A cu valoarea 0 se scot, după ce confirmi.

## Renunță
<!-- slice: 0051, 0000-28 -->
target: DdfEditForm.btnRenunta
Închide fără să salveze nimic; numerele rezervate se eliberează.
