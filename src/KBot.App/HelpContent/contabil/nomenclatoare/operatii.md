---
id: contabil.nomenclatoare.operatii
title: Operații contabile - operații diverse, facturi și plăți
part: contabil
order: 46
parent: contabil.nomenclatoare
screens: OperatiiSabloaneForm, OperatiiDiversePage, FacturiPlatiPage
keywords: operații diverse, facturi, plăți, ordin de plată, OP, factură nouă, notă contabilă, introducere operații, OP9, OPuri
---
<!-- slice: AV02-03, AV02-04, 0000-176 -->
Din ribbon, fila **Date › Operații** se deschide fereastra **«Operații contabile»**, în care înregistrezi operațiile contabile pornind de la șabloane.
Ca și **«Șabloane»**, se deschide peste fereastra principală și lucrează în **subunitatea** și **anul** alese în bara de titlu în clipa deschiderii.
Bara verticală din stânga alege pagina; dacă ai ceva necompletat și treci la cealaltă pagină sau închizi fereastra, aplicația te întreabă dacă salvezi.

## Operații diverse

Lista din stânga arată șabloanele de **operații diverse** ale subunității; scrii în căsuța de deasupra ca să le restrângi, apoi apeși pe cea dorită. În dreapta completezi:

- **Nota contabilă** – alegi una existentă sau scrii un număr nou; lăsată cu numărul propus, se folosește următorul număr liber;
- **Număr document** și **Fel document** – cum scrie pe document; felul îl poți alege din listă sau scrie;
- **Data documentului** – în anul de lucru și după ultima lună închisă;
- **Partener** – se cere doar când șablonul are linii cu «partenerul operației»;
- **Suma** – se copiază pe fiecare linie a operației, iar în tabel o poți schimba linie cu linie.

Tabelul arată jurnalul, clasificația, explicația, conturile și partenerii fiecărei linii. **«Salvează»** înregistrează liniile cu sumă (cele cu suma zero se sar): câte un document și o operație pentru fiecare linie. Conturile le ia aplicația din șablon.

## Facturi și plăți

Pagina lucrează pentru **un partener** odată: alegi partenerul și clasificația bugetară (apar cele care au șabloane de operații în subunitate), apoi:

- **Facturi noi** – cu **«Adaugă factură»** scrii de mână factura (număr, dată, fel, valoare) și cât plătești din ea acum. Se înregistrează cu **șablonul de evidențiere** ales în stânga;
- **Facturi din anul curent / anul anterior, neachitate** – facturile deja înregistrate care mai au rest de plată; în coloana **«De plată»** scrii cât plătești acum (nu poți depăși restul). Pentru anul anterior apar facturile pe care baza de date le conține;
- **Șablonul de plată** – alegi unul din stânga (apeși din nou pe el ca să-l scoți). Cu el, **«Salvează»** face **plata și ordinul de plată** (O.P.) pentru tot ce ai pus în «De plată». O plată = un ordin = o operație contabilă, chiar dacă plătește mai multe facturi ale aceluiași partener;
- **Explicația O.P.** – se compune din numerele facturilor plătite și din felul scris pe șablon; o poți schimba. Are între 1 și 75 de caractere, fără diacritice și fără virgule (ANAF le refuză);
- **Număr O.P.** – numărul de lângă butoane este **provizoriu**: arată numărul care ar urma acum, dar dacă între timp altcineva emite un ordin, numărul final poate fi altul. După salvare mesajul îți spune **numărul real**.

În tabelul **«Ordine de plată ale partenerului»** vezi plățile anului. Cu **«Șterge plata»** ștergi plata aleasă împreună cu ordinul ei, atât timp cât ordinul nu e într-un **OPME** și nu e confirmat printr-un **extras de cont**; altfel butonul e stins, iar sfatul lui spune motivul.

Dacă o dată iese din anul de lucru, un cont lipsește din planul de conturi sau o sumă depășește restul de plată, aplicația îți spune ce nu e bine și nu salvează nimic: tot ce se înregistrează la o apăsare de «Salvează» se înregistrează împreună sau deloc.
