---
id: contabil.nomenclatoare.sabloane
title: Șabloane de operații și de salarii
part: contabil
order: 45
parent: contabil.nomenclatoare
screens: SabloaneForm, SablonOperatiiView, SablonSalariiView
keywords: șabloane salarii, articole salarii, rețineri, viramente, plăți cu numerar, șabloane, sabloane, operații contabile automate, operații diverse, operații implicite, configurare operații, șablon operație
---
<!-- slice: AV01-14, AV01-15, AV01-16, AV01-17 -->
Din ribbon, fila **Date › Șabloane** se deschide fereastra **«Șabloane»**, cu operațiile contabile pe care aplicația le face automat:
pentru fiecare operație spui care sunt conturile de la **debit** și de la **credit** și cine este partenerul. Fereastra se deschide
peste cea principală și trebuie închisă ca să lucrezi mai departe în aplicație. Bara verticală din stânga alege felul șabloanelor:

- **«Operații cu clasificație»** – operațiile legate de o clasificație (de exemplu o achiziție de tichete de vacanță);
- **«Operații diverse»** – toate celelalte operații, care pot să nu aibă nici clasificație, nici partener;
- **«Salarii»** – șabloanele de salarii, pe articole.

Fereastra arată șabloanele sectorului și sursei alese în bara de titlu a aplicației în clipa deschiderii (pe **«Toate»** le arată pe toate);
sursa apare și în titlul ferestrei. Ca să lucrezi pe altă sursă, închizi fereastra, schimbi sursa și o deschizi din nou.

În toate cele trei liste arborele din stânga ocupă **40%** din lățime, iar în dreapta partea de lucru este **blocată** până apeși
**«Modifică»** pe o operație (sau **«+»** pentru una nouă). Singurul buton al ferestrei este **«Salvează»**, care se activează
odată cu **«Modifică»** sau **«+»**.

## Operații cu clasificație
<!-- slice: AV01-14, AV01-16, AV01-17, 0010-08 -->
În stânga este arborele: **Titlu › Articol › Alineat**, cu denumirile din nomenclatorul de clasificații. Sub clasificații sunt operațiile ei:
cele de **evidențiere** cu albastru, cele de **plată** cu verde. Nodurile care au operații sub ele sunt scrise **îngroșat**.

- Pe ultimul nivel dinaintea operațiilor (alineatul sau, când alineatul este «00», articolul) apare la trecerea mouse-ului un **«+»** în dreapta:
  face o operație nouă, cu câte o linie pentru fiecare **formă** (subcapitol) a clasificației.
- Pe fiecare operație apare la trecerea mouse-ului un buton cu meniul **«Modifică»** / **«Șterge»**. **«Șterge»** întreabă înainte; operația
  dispare din listă, iar ștergerea se face definitiv la **«Salvează»**.
- Butonul din dreapta antetului arborelui deschide opțiunile listei: **«Arată doar clasificațiile cu operații»** (implicit nebifat).

În dreapta se completează operația aleasă:

- **Explicație document** – textul operației (obligatoriu);
- **Cod jurnal** – jurnalul în care se înregistrează;
- **Compartiment** – îl scrii sau îl alegi dintre cele folosite deja;
- **Tip operație** – **«Evidențiere»** sau **«Plată»**;
- **Fel factură (OP)** – textul adăugat la ordinul de plată; se completează doar la plată.

Mai jos este **tabelul liniilor**, cu **un rând pentru fiecare formă** a clasificației (nu se adaugă rânduri). Coloanele sunt:
**Forma** (subcapitolul rândului, de felul 04.02; nu se scrie aici), **Cont debit** și **Cont credit** (le alegi din listă sau le scrii), **Part. debit** și **Part. credit**
și bifele **O** (ordonanțare, doar la plată), **P/A** (propunere / angajament, doar la evidențiere) și **C** (cheltuială).

Când apeși **«Modifică»**, apar în tabel și rândurile **formelor care nu au încă operație**: le completezi ca să adaugi operația și pentru ele.
Un rând se salvează doar dacă are **și contul debit, și contul credit**; altfel este anulat la salvare (la fel și în tabelul «Operații diverse»).

**✕** de la capătul unui rând îi **golește** conținutul, dar rândul rămâne, pentru forma lui. Cu **Esc** renunți la ce scrii în căsuța curentă; un **al doilea Esc**, imediat după primul, golește tot rândul (forma rămâne). Vezi și [Arborii și tabelele](topic:contabil.liste).

Listele **Cont debit** / **Cont credit** arată conturile din **planul de conturi** al sursei, cu analitic dacă există: cele care nu depind de clasificație
și cele legate de clasificația rândului (de felul **401010002A650402200101**). Lista se deschide **mai lată** decât celula, ca să încapă denumirea.

**Part. debit** / **Part. credit** sunt doar bife: bifat înseamnă că se folosește **partenerul operației**, care se înlocuiește când operația este
întocmită. O operație cu clasificație are partener pe **o singură parte**, debit sau credit: dacă bifezi una, cealaltă se debifează.

## Operații diverse
<!-- slice: AV01-14, AV01-16, AV01-17, 0010-08 -->
Aceeași fereastră, dar lista este simplă: primul rând este **«‹ Adaugă operație diversă ›»** (un clic face o operație nouă), sub el sunt operațiile, în
ordine alfabetică, cu același buton **«Modifică»** / **«Șterge»** la trecerea mouse-ului. Tipul operației este **«Alte operații contabile»**.

**Tabelul liniilor** ocupă aici **toată lățimea** ferestrei, sub arbore și sub câmpurile operației (arborele este de aceea mai scurt). Fiecare linie are:

- **Clasificația** – se alege din lista clasificațiilor sursei, grupate pe articole (sau se scrie); poate rămâne goală;
- **Cont debit** și **Cont credit** – din planul de conturi al sursei, cu analitic. Dacă linia are o clasificație, lista arată conturile care nu depind de
  clasificație și pe cele legate de clasificația aleasă; **fără clasificație, lista arată toate conturile**. Dacă schimbi clasificația și un cont ales nu i se mai
  potrivește, contul se șterge;
- **Partener debit** și **Partener credit** – **«(fără partener)»**, **«VARIABIL»** (partenerul operației, înlocuit când operația este întocmită) sau un partener anume;
- bifele **O**, **P/A** și **C**, ca la operațiile cu clasificație; se pot bifa doar când linia are clasificație.

Ultimul rând al tabelului este **gol**: cum scrii în el, devine o linie și apare altul gol dedesubt. **✕** de la capătul unei linii o scoate (o operație are cel puțin una). **Esc** renunță la ce scrii în căsuța curentă; un **al doilea Esc**, imediat după primul, scoate tot rândul (rândul gol de la sfârșit rămâne).

## Salarii
<!-- slice: AV01-15, AV01-16 -->
În stânga este arborele **capitolelor** (ale subunităților sursei din bara de titlu), cu **articolele de salarii** sub ele, de felul **«10.01.01 - Înregistrare
salarii de bază»**. Capitolele cu articole sunt scrise **îngroșat**. Pe un capitol apare la trecerea mouse-ului un **«+»**, care face un articol nou; pe un
articol, un buton cu meniul **«Modifică»** / **«Șterge»** (**«Șterge»** scoate articolul cu toate liniile lui, după o întrebare).

În dreapta, deasupra, este o bară cu patru tabele, fiecare cu locul lui, pe toată înălțimea: **«Angajamente»**, **«Rețineri»**, **«Viramente»** și **«Numerar»**.

- **Angajamente** – un singur rând: **clasificația articolului** (de felul **10.01.01**), explicația, contul și partenerul de la debit (**C. Debit** / **P. Debit**),
  contul și partenerul de la credit (**C. Credit** / **P. Credit**);
- **Rețineri** – sumele reținute din salariu. Debitul unei rețineri este mereu creditul articolului: îl vezi scris, dar nu îl poți schimba aici. Bifa **CM**
  (concediu medical) se poate pune la o singură reținere;
- **Viramente** – plata a ceea ce s-a reținut, către terți. La un virament nou contul de credit se completează cu **7700000**, dacă există în nomenclator;
  poți să-l schimbi. Bifa **CM** se poate pune la un singur virament;
- **Numerar** – salariile plătite în numerar.

În ultimele trei tabele ultimul rând este **gol**: cum scrii în el, devine o linie și apare altul gol dedesubt. **✕** de la capătul unui rând îl scoate.
Contul se alege din lista conturilor (sau îl tastezi), iar partenerul este **codul** lui, **«XXX»** pentru partenerul operației sau gol când nu e partener.

Un articol **nu se salvează fără cel puțin un virament**; un articol nou vine cu un rând de virament gol, pe care îl completezi. Mesajul de la salvare spune ce lipsește.

## Salvare
<!-- slice: AV01-14, AV01-16 -->
- **«Salvează»** scrie toate modificările (operațiile sau articolele schimbate, adăugate și șterse).
- Dacă încerci să închizi fereastra sau să treci la altă listă cu modificări nesalvate, aplicația întreabă dacă le salvează.
- Mesajul care apare la salvare spune ce lipsește: o operație trebuie să aibă explicație, jurnal, cel puțin o linie cu ambele conturi,
  iar un partener anume trebuie ales cu adevărat.
