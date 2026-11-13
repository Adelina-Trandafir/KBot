---
id: contabil.nomenclatoare.operatii
title: Operații contabile - operații diverse, facturi și plăți
part: contabil
order: 46
parent: contabil.nomenclatoare
screens: OperatiiSabloaneForm, OperatiiDiversePage, FacturiPlatiPage
keywords: operații diverse, facturi, plăți, ordin de plată, OP, factură nouă, notă contabilă, introducere operații, OP9, OPuri
---
<!-- slice: AV02-03, AV02-04, 0000-176, AV02-05, 0000-177, 0000-178 -->
Din ribbon, fila **Introducere date › Introducere Date › Operații** se deschide fereastra **«Operații contabile»**, în care înregistrezi operațiile contabile pornind de la șabloane.
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

Pagina lucrează pentru **un partener** odată. Sus alegi **clasificația** (apar cele care au șabloane de operații în subunitate) și **partenerul**; lângă ele vezi, doar de citit, **codul client** (dacă partenerul are unul; mai multe coduri ale aceluiași cod fiscal apar despărțite prin «;») și **codul bancar** al partenerului. Apoi:

- **Documente evidențiere** (lista din stânga, sus) – alegi **șablonul de evidențiere** cu care se înregistrează facturile noi; apeși din nou pe el ca să-l scoți;
- **Facturi noi / Facturi an curent / Facturi an anterior** – bara de deasupra tabelului alege ce vezi, câte un tabel pe fiecare:
  - **Facturi noi** – scrii de mână factura (număr, dată, fel, **nota contabilă**, valoare) și cât plătești din ea acum, în **rândul gol** de la capătul tabelului; când îl completezi apare altul dedesubt, cu aceeași notă. **Nota contabilă este obligatorie la fiecare factură**: alegi una existentă sau scrii un număr nou (se deschide o notă nouă); fiecare factură se înregistrează în nota ei. Rândul gol rămas nu se salvează. Cu **✕** de la capătul rândului scoți factura nouă (încă nesalvată);
  - **Facturi an curent / an anterior** – facturile deja înregistrate care mai au rest de plată; în coloana **«De plată»** scrii cât plătești acum (nu poți depăși restul). Pentru anul anterior apar facturile pe care baza de date le conține;
- **Documente plată** (lista din stânga, jos) – alegi **șablonul de plată** (apeși din nou pe el ca să-l scoți). Cu el, **«Salvează»** face **plata și ordinul de plată** (O.P.) pentru tot ce ai pus în «De plată». O plată = un ordin = o operație contabilă, chiar dacă plătește mai multe facturi ale aceluiași partener.

Tabelul de plată din josul paginii are **pe primul rând plata pe care o faci acum**, cu:

- **Număr O.P.** – este **provizoriu**: arată numărul care ar urma acum, dar dacă între timp altcineva emite un ordin, numărul final poate fi altul. După salvare mesajul îți spune **numărul real**;
- **Data O.P.** – în anul de lucru, scrisă zz.ll.aaaa;
- **Nota contabilă** (a plății) – alegi una existentă sau scrii un număr nou; lăsată cu numărul propus, se folosește următorul număr liber;
- **Explicația O.P.** – se compune din numerele facturilor plătite și din felul scris pe șablon; o poți schimba. Are între 1 și 75 de caractere, fără diacritice și fără virgule (ANAF le refuză);
- **Valoare** – cât plătești acum, adunat din coloanele «De plată».

Sub acel rând vezi **ordinele de plată ale partenerului în anul de lucru** (doar de citit). Cu **✕** de la capătul unui ordin ștergi plata respectivă, împreună cu ordinul ei, atât timp cât ordinul nu e într-un **OPME** și nu e confirmat printr-un **extras de cont**; altfel aplicația îți spune motivul și nu șterge nimic.

Dacă o dată iese din anul de lucru, un cont lipsește din planul de conturi sau o sumă depășește restul de plată, aplicația îți spune ce nu e bine și nu salvează nimic: tot ce se înregistrează la o apăsare de «Salvează» se înregistrează împreună sau deloc.
