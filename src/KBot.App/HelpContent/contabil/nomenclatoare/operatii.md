---
id: contabil.nomenclatoare.operatii
title: Operații contabile - operații diverse, facturi și plăți
part: contabil
order: 46
parent: contabil.nomenclatoare
screens: OperatiiSabloaneForm, OperatiiDiversePage, FacturiPlatiPage
keywords: operații diverse, facturi, plăți, ordin de plată, OP, factură nouă, notă contabilă, introducere operații, OP9, OPuri
---
<!-- slice: AV02-03, AV02-04, 0000-176, AV02-05, 0000-177, 0000-178, AV02-06, 0000-180 -->
Din ribbon, fila **Introducere date › Introducere Date › Operații** se deschide fereastra **«Operații contabile»**, în care înregistrezi operațiile contabile pornind de la șabloane.
Ca și **«Șabloane»**, se deschide peste fereastra principală și lucrează în **anul** ales în bara de titlu în clipa deschiderii.
Partenerul se alege dintr-o listă în care poți și **scrie** ca să o restrângi. În bara de titlu a ferestrei apare **«Sursă/Sector»**, fără «Toate»: aici lucrezi pe **o singură sursă** odată, ca să nu vezi aceiași parteneri de mai multe ori. Fereastra se deschide pe sursa aleasă în fereastra principală, iar dacă acolo e «Toate», pe **02A**. Când alegi altă sursă, ambele pagini se citesc din nou pentru ea (și te întreabă dacă salvezi ce ai lăsat necompletat); subunitatea este cea a sursei alese.
Bara verticală din stânga alege pagina; dacă ai ceva necompletat și treci la cealaltă pagină sau închizi fereastra, aplicația te întreabă dacă salvezi.

## Operații diverse
<!-- slice: AV02-03, AV02-06, 0000-180, AV08-04, 0000-182 -->

Arborele din stânga grupează șabloanele de **operații diverse** ale subunității pe **jurnale**; jurnalele sunt închise la început, le deschizi cu un clic. Căutarea de deasupra arborelui găsește o operație după explicație și deschide jurnalul în care se află. Apeși pe operația dorită. În dreapta completezi:

- **Nota contabilă** – alegi una existentă sau scrii un număr nou; lăsată cu numărul propus, se folosește următorul număr liber;
- **Număr document** și **Fel document** – cum scrie pe document; felul îl poți alege din listă sau scrie;
- **Data documentului** – în anul de lucru, cel mult cu un an în urmă și după ultima lună închisă;
- **Partener** – se cere doar când șablonul are linii cu «partenerul operației»; lista arată doar partenerii sursei alese;
- **Suma** – se copiază pe fiecare linie a operației, iar în tabel o poți schimba linie cu linie.

Tabelul arată clasificația, explicația, conturile și partenerii fiecărei linii. **«Salvează»** înregistrează liniile cu sumă (cele cu suma zero se sar): câte un document și o operație pentru fiecare linie. Conturile le ia aplicația din șablon. Un șablon din jurnalul de închidere **JI** nu se poate salva aici: «Operațiile diverse nu pot fi înregistrate în jurnalul de închidere (JI).».

## Facturi și plăți
<!-- slice: AV08-07, 0000-182 -->

Pagina lucrează pentru **un partener** odată. Sus alegi **clasificația** (apar cele care au șabloane de operații în subunitate) și **partenerul**; lângă ele vezi, doar de citit, **codul client** (dacă partenerul are unul; mai multe coduri ale aceluiași cod fiscal apar despărțite prin «;») și **codul bancar** al partenerului. Apoi:

- **Documente evidențiere** (lista din stânga, sus) – alegi **șablonul de evidențiere** cu care se înregistrează facturile noi; apeși din nou pe el ca să-l scoți. **Tabelul «Facturi noi» rămâne gol până alegi un șablon de evidențiere**; abia atunci apare rândul în care scrii factura. Facturile din **anul curent** și din **anul anterior** se completează când alegi partenerul;
- **Facturi noi / Facturi an curent / Facturi an anterior** – bara de deasupra tabelului alege ce vezi, câte un tabel pe fiecare:
  - **Facturi noi** – scrii de mână factura (**nota contabilă**, număr, dată, fel, valoare; nota este prima coloană) și cât plătești din ea acum, în **rândul gol** de la capătul tabelului; când îl completezi apare altul dedesubt, cu aceeași notă. **Nota contabilă este obligatorie la fiecare factură**: alegi una existentă sau scrii un număr nou (se deschide o notă nouă); fiecare factură se înregistrează în nota ei. Rândul gol rămas nu se salvează. Cu **✕** de la capătul rândului scoți factura nouă (încă nesalvată);
  - **Facturi an curent / an anterior** – facturile deja înregistrate care mai au rest de plată; în coloana **«De plată»** scrii cât plătești acum (nu poți depăși restul). Pentru anul anterior apar facturile pe care baza de date le conține;
- **Documente plată** (lista din stânga, jos) – alegi **șablonul de plată** (apeși din nou pe el ca să-l scoți). Cu el, **«Salvează»** face **plata și ordinul de plată** (O.P.) pentru tot ce ai pus în «De plată». O plată = un ordin = o operație contabilă, chiar dacă plătește mai multe facturi ale aceluiași partener.

Tabelul de plată din josul paginii este **gol** până alegi un șablon în «Documente plată». După ce l-ai ales, are **un singur rând: plata pe care o faci acum**, cu (prima coloană este **Nota contabilă**):

- **Număr O.P.** – este **provizoriu**: arată numărul care ar urma acum, dar dacă între timp altcineva emite un ordin, numărul final poate fi altul. După salvare mesajul îți spune **numărul real**;
- **Data O.P.** – în anul de lucru, scrisă zz.ll.aaaa;
- **Nota contabilă** (a plății) – alegi una existentă sau scrii un număr nou; lăsată cu numărul propus, se folosește următorul număr liber;
- **Explicația O.P.** – se compune din numerele facturilor plătite și din felul scris pe șablon; o poți schimba. Are între 1 și 75 de caractere, fără diacritice și fără virgule (ANAF le refuză);
- **Valoare** – cât plătești acum, adunat din coloanele «De plată».

Data facturii, numărul ei și data O.P. se înregistrează **exact cum le-ai scris**. O dată din afara anului de lucru sau dintr-o lună închisă (ori dintr-un an închis) **nu se schimbă singură**: mesajul îți spune care dată nu e bună, iar tu o corectezi. Dacă o factură nouă este într-o **lună de după cea mai nouă operație** a subunității, K-BOT te întreabă dacă vrei să continui: **Da** salvează, **Nu** oprește salvarea și tot ce ai completat rămâne.

Dacă o dată nu e bună, un cont lipsește din planul de conturi sau o sumă depășește restul de plată, aplicația îți spune ce nu e bine și nu salvează nimic: tot ce se înregistrează la o apăsare de «Salvează» se înregistrează împreună sau deloc.
