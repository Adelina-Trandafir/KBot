---
id: contabil.ribbon
title: Ribbonul (banda de comenzi)
part: contabil
order: 21
parent: contabil
screens: AvacontMain.ribbon
keywords: ribbon, banda, file, fila, grup, comenzi, meniu, MENIU, unde e meniul, unde a disparut meniul, strange ribbonul, ascunde banda, inchide macheta, iesire, adaugare angajamente, nomenclatoare, configurare, administrare, unitate
---
<!-- slice: 0114 -->
Sus, sub bara de titlu, fereastra principală are **ribbonul**: o bandă cu **file** — **Angajamente**, **Date**,
**E-Factura**, **Configurări**. Fiecare filă are butoane strânse în **grupuri**, cu numele grupului dedesubt.
Ribbonul a luat locul butonului **MENIU**: tot ce era în meniu e acum aici. Ținând mouse-ul pe un buton vezi o
etichetă care spune ce face. În dreapta benzii cu file scrie utilizatorul conectat.

<!-- capture: ribbon-file | caption: Ribbonul, fila «Angajamente», cu butoanele «Ajutor» și «Închide» în dreapta | prepare: Deschideți «Lista angajamentelor» din ribbon (Angajamente › Navigare), ca butonul «Închide» să se vadă. Alegeți fila «Angajamente» și prindeți toată lățimea ribbonului. -->

## Filele și butoanele
<!-- slice: 0114, 0118, 0119, 0114-01, 0000-76 -->

| Fila | Grupul | Butoanele |
|------|--------|-----------|
| **Angajamente** | Navigare | **Lista angajamentelor** — arborele de angajamente cu vederile lor — [Fereastra principală](topic:contabil.fereastra) |
| | Adăugare | **Angajament nou** — [Angajament nou](topic:contabil.ddf.nou); **Creează în FOREXE** — [Vederea «Browser FOREXE»](topic:contabil.forexe.browser) |
| | Verificări | **Operațiuni necorelate** — **apare doar când ai operațiuni necorelate de rezolvat** — [Operațiuni necorelate](topic:contabil.notecab) |
| **Date** | Nomenclatoare | **Clasificații bugetare**, **Parteneri** — [Nomenclatoare](topic:contabil.nomenclatoare) |
| | Documente | **Extrase** — toate extrasele de cont ale unității — [Extrase de cont](topic:contabil.vederi.extrase) |
| **E-Factura** | E-Factura | **Cumpărare** (facturile primite), **Vânzare** (facturile emise) — [E-Factura](topic:contabil.efactura) |
| | ANAF | **Token ANAF** — [Tokenul ANAF](topic:contabil.efactura.token); **Sincronizează facturi primite**, cu lista de zile și caseta pentru zile — **apar doar cât e deschisă macheta «Cumpărare»** |
| **Configurări** | Configurare | **Jurnal activitate** — **apare doar cât e pornit în Setări › Aplicație** — [Jurnal activitate](topic:contabil.jurnal); **Configurare K-BOT** — [Setări și aspect](topic:contabil.setari); **Istoric versiuni** — [Actualizarea K-BOT](topic:contabil.actualizare) |
| | Unitate | **Conturi Unitate** — [Conturile unității emitente](topic:contabil.efactura.conturi); **Date Unitate** — datele unității ca emitent de facturi |

Un buton care lucrează pe lista angajamentelor («Angajament nou», «Creează în FOREXE», «Operațiuni necorelate») o
aduce întâi pe ecran.

<!-- capture: ribbon-configurari | caption: Fila «Configurări» a ribbonului | prepare: Alegeți fila «Configurări» a ribbonului, cu «Jurnal activitate» pornit în Setări › Aplicație. -->

## Strângerea ribbonului
<!-- slice: 0114 -->

Ca să ai mai mult loc pentru lucru, ribbonul se poate **strânge**: **dublu clic** pe numele unei file sau clic pe
**săgeata** din dreapta lui. Rămân doar numele filelor, iar butoanele «Ajutor» și «Închide» se fac mici, lângă ele.
Un **clic pe o filă** desface ribbonul la loc. K-BOT ține minte cum l-ai lăsat și pornește data viitoare la fel.

<!-- capture: ribbon-strans | caption: Ribbonul strâns: doar numele filelor, cu «Ajutor» și «Închide» mici | prepare: Deschideți «Lista angajamentelor», apoi dați dublu clic pe numele unei file din ribbon. După captură, desfaceți ribbonul la loc cu un clic pe o filă. -->

## Butoanele din dreapta: «Ajutor» și «Închide»
<!-- slice: 0113, 0114 -->

Cele două butoane stau în dreapta ribbonului, pe **orice filă**:

- **«Ajutor»** — ajutorul pentru ce ai pe ecran — [Butonul «Ajutor»](topic:contabil.ajutor.buton).
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
| MENIU › Extrase | Date › Documente › **Extrase** |
| MENIU › Nomenclatoare › Clasificații bugetare / Parteneri | Date › Nomenclatoare |
| MENIU › E-Factura | E-Factura › E-Factura › **Vânzare** |
| MENIU › Jurnal activitate | Configurări › Configurare › **Jurnal activitate** |
| MENIU › Configurare K-BOT | Configurări › Configurare › **Configurare K-BOT** |
| Setări › pagina «Istoric versiuni» | Configurări › Configurare › **Istoric versiuni** |
| butonul din bara de titlu a ferestrei de facturi › Date Unitate / Conturi Unitate | Configurări › Unitate |
| fereastra de facturi › Token ANAF / Sincronizează facturi primite | E-Factura › ANAF |
| fereastra de facturi › Facturi primite | E-Factura › E-Factura › **Cumpărare** |
