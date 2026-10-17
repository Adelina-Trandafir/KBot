---
id: contabil.ribbon
title: Ribbonul (banda de comenzi)
part: contabil
order: 21
parent: contabil
screens: AvacontMain.ribbon
keywords: ribbon, banda, file, fila, grup, comenzi, meniu, MENIU, unde e meniul, unde a disparut meniul, strange ribbonul, ascunde banda, inchide macheta, iesire, adaugare angajamente, nomenclatoare, configurare, administrare, unitate
---
<!-- slice: 0114, 0114-02, 0114-04, 0114-05, 0000-85, 0000-88, 0000-89, 0114-06, 0114-07, 0114-09, 0114-10, 0114-11, 0000-90 -->
Sus, sub bara de titlu, fereastra principală are **ribbonul**: o bandă cu **file** — **Configurări**, **Angajamente**,
**E-Factura**, **Extrase**. La pornire e aleasă fila **Angajamente**. Fiecare filă are butoane strânse în **grupuri**, cu numele grupului dedesubt.
Ribbonul a luat locul butonului **MENIU**: tot ce era în meniu e acum aici. Ținând mouse-ul pe un buton vezi o
etichetă care spune ce face. În dreapta benzii cu file scrie utilizatorul conectat.

<!-- capture: ribbon-file | caption: Ribbonul, fila «Angajamente», cu butonul «Închide» în dreapta | prepare: Deschideți «Lista angajamentelor» din ribbon (Angajamente › Navigare), ca butonul «Închide» să se vadă. Alegeți fila «Angajamente» și prindeți toată lățimea ribbonului. | redo: 2026-10-17 21:00 | why: 0114-15/16: grupul vederii poate avea mai multe butoane (Rezervari, Receptii); 0114-06/14: ribbonul are in dreapta grupul machetei (langa «Inchide») si grupul vederii; meniul din subsolul listei are doua randuri; E-Factura sta sub Extrase in bara vederilor -->

## Filele și butoanele
<!-- slice: 0114, 0118, 0119, 0114-01, 0114-04, 0114-05, 0000-76, 0000-88, 0000-89, 0114-06, 0114-07, 0114-09, 0114-10, 0114-11, 0000-90 -->

| Fila | Grupul | Butoanele |
|------|--------|-----------|
| **Configurări** | Date | **Clasificații bugetare**, **Parteneri** — [Nomenclatoare](topic:contabil.nomenclatoare) |
| | Configurare | **Jurnal activitate** — **apare doar cât e pornit în Setări › Aplicație** — [Jurnal activitate](topic:contabil.jurnal); **Configurare K-BOT** — [Setări și aspect](topic:contabil.setari); **Istoric versiuni** — [Actualizarea K-BOT](topic:contabil.actualizare) |
| | Unitate | **Conturi Unitate** — [Conturile unității emitente](topic:contabil.efactura.conturi); **Date Unitate** — datele unității ca emitent de facturi |
| **Angajamente** | Navigare | **Lista angajamentelor** — arborele de angajamente cu vederile lor — [Fereastra principală](topic:contabil.fereastra) |
| | Adăugare | **Angajament nou** — [Angajament nou](topic:contabil.ddf.nou); **Creează în FOREXE** — [Vederea «Browser FOREXE»](topic:contabil.forexe.browser) |
| | Verificări | **Operațiuni necorelate** — **apare doar când ai operațiuni necorelate de rezolvat** — [Operațiuni necorelate](topic:contabil.notecab) |
| **E-Factura** | E-Factura | **Cumpărare** (facturile primite), **Vânzare** (facturile emise) — [E-Factura](topic:contabil.efactura) |
| | ANAF | **Token ANAF** — [Tokenul ANAF](topic:contabil.efactura.token); **Sincronizează facturi primite** a trecut în grupul din dreapta al machetei «Cumpărare» (vezi mai jos) |
| **Extrase** | Documente | **Extrase** — toate extrasele de cont ale unității — [Extrase de cont](topic:contabil.vederi.extrase) |

Un buton care lucrează pe lista angajamentelor («Angajament nou», «Creează în FOREXE», «Operațiuni necorelate») o
aduce întâi pe ecran.

<!-- capture: ribbon-configurari | caption: Fila «Configurări» a ribbonului | prepare: Alegeți fila «Configurări» a ribbonului, cu «Jurnal activitate» pornit în Setări › Aplicație. | redo: 2026-10-17 20:00 | why: 0114-06/14: ribbonul are in dreapta grupul machetei (langa «Inchide») si grupul vederii; meniul din subsolul listei are doua randuri; E-Factura sta sub Extrase in bara vederilor -->

## Strângerea ribbonului
<!-- slice: 0114, 0114-02, 0000-85 -->

Ca să ai mai mult loc pentru lucru, ribbonul se poate **strânge**: **dublu clic** pe numele unei file sau clic pe
**săgeata** din dreapta lui. Rămân doar numele filelor, iar butoanele din dreapta («Închide» și, când e cazul, «Asociază parteneri») se fac mici, lângă ele.
Un **clic pe o filă** desface ribbonul la loc. K-BOT ține minte cum l-ai lăsat și pornește data viitoare la fel.

<!-- capture: ribbon-strans | caption: Ribbonul strâns: doar numele filelor, cu «Închide» mic | prepare: Deschideți «Lista angajamentelor», apoi dați dublu clic pe numele unei file din ribbon. După captură, desfaceți ribbonul la loc cu un clic pe o filă. | redo: 2026-10-17 21:00 | why: 0114-15/16: grupul vederii poate avea mai multe butoane (Rezervari, Receptii); 0114-06/14: ribbonul are in dreapta grupul machetei (langa «Inchide») si grupul vederii; meniul din subsolul listei are doua randuri; E-Factura sta sub Extrase in bara vederilor -->

## Butoanele din dreapta: «Închide» și cele ale paginii
<!-- slice: 0113, 0114, 0114-02, 0114-03, 0084-04, 0000-85, 0114-15, 0114-16, 0000-91 -->

Butoanele din dreapta ribbonului stau pe **orice filă**, și cu ribbonul strâns. Ajutorul nu mai are buton aici: îl deschizi cu **F1** sau cu **?** din bara de titlu — [Ajutorul](topic:contabil.ajutor).

- **Grupul machetei** — în dreapta panoului, chiar lângă «Închide», pe orice filă; numele lui spune macheta de pe ecran și are doar butoanele ei: la **lista angajamentelor** (cu toate vederile ei) — «Editează grupe» și «Descarcă extrase»; la **«Extrase de cont»** — «Descarcă extrase»; la **«Facturi primite»** (E-Factura › Cumpărare) — «Sincronizează facturi primite», cu lista de zile și caseta pentru zile. Dispare când macheta se schimbă.
- **Grupul vederii** — un grup cu nume (numele vederii), în stânga grupului machetei, despărțit de el printr-o linie subțire; pe orice filă. Are butoane doar pentru **vederea de pe ecran** a listei angajamentelor; grupul dispare când treci la altă vedere fără butoane sau la altă machetă:
  - **Sumar** — **«Asociază parteneri»** (stins, gri, când angajamentul n-are document de fundamentare) — [Vederea «Sumar»](topic:contabil.vederi.sumar);
  - **Rezervări** — **«Grafice»**, butonul documentului de fundamentare (își schimbă numele: «Adaugă rezervare», «Definitivează», «Derulează» sau «Generează PDF final»; apare doar când are ce face) și **«Repară...»** (apare doar când angajamentul are rezervări greșite) — [Vederea «Rezervări»](topic:contabil.vederi.rezervari);
  - **Recepții** — **«Asociază recepții»** și **«Reface din istoric...»** (apare doar când angajamentul are recepții cu lanțul neînchis) — [Vederea «Recepții»](topic:contabil.vederi.receptii).
- **«Închide»** — închide **macheta de pe ecran** (Extrase de cont, Clasificații, un editor, Setări…). Dacă ai
  modificări nesalvate, macheta te întreabă, ca înainte. Din **lista angajamentelor** te întoarce la
  [«Acasă»](topic:contabil.acasa), fără să piardă angajamentul și vederea alese. Pe pagina «Acasă» butonul nu
  există: **K-BOT se închide doar din X-ul ferestrei**, din dreapta sus.

Tasta **Esc** închide macheta, la fel ca înainte.

## Unde găsești ce era în MENIU
<!-- slice: 0114, 0118, 0119 -->

| Înainte | Acum, în ribbon |
|---------|-----------------|
| MENIU › Adăugare angajamente... › Angajament nou | Angajamente › Adăugare › **Angajament nou** |
| MENIU › Adăugare angajamente... › Creează angajament în FOREXE | Angajamente › Adăugare › **Creează în FOREXE** |
| MENIU › (!) Operațiuni necorelate | Angajamente › Verificări › **Operațiuni necorelate** |
| MENIU › Extrase | Extrase › Documente › **Extrase** |
| MENIU › Nomenclatoare › Clasificații bugetare / Parteneri | Configurări › Date |
| pictograma din stânga a arborelui › descarcă extrasele | grupul din dreapta al ribbonului › **Descarcă extrase** (cât e pe ecran lista angajamentelor sau «Extrase de cont») |
| MENIU › E-Factura | E-Factura › E-Factura › **Vânzare** |
| MENIU › Jurnal activitate | Configurări › Configurare › **Jurnal activitate** |
| MENIU › Configurare K-BOT | Configurări › Configurare › **Configurare K-BOT** |
| Setări › pagina «Istoric versiuni» | Configurări › Configurare › **Istoric versiuni** |
| butonul din bara de titlu a ferestrei de facturi › Date Unitate / Conturi Unitate | Configurări › Unitate |
| fereastra de facturi › Token ANAF | E-Factura › ANAF |
| fereastra de facturi › Sincronizează facturi primite | grupul din dreapta al machetei «Cumpărare» |
| rotița listei › Editează grupe... | grupul din dreapta al listei angajamentelor › **Editează grupe** |
| pictogramele din capul și din subsolul arborelui de rezervări (grafic, document de fundamentare, repară) | grupul **Rezervări** din dreapta ribbonului › **Grafice**, butonul documentului, **Repară...** |
| pictogramele din capul și din stânga subsolului arborelui de recepții (legături, refacere din istoric) | grupul **Recepții** din dreapta ribbonului › **Asociază recepții**, **Reface din istoric...** |
| fereastra de facturi › Facturi primite | E-Factura › E-Factura › **Cumpărare** |
