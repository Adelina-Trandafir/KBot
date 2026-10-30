---
id: contabil.notecab
title: Operațiuni necorelate și note de corecție CAB
part: contabil
order: 60
parent: contabil
screens: CabNoteForm, CabNoteReceiptForm, NoteCabView, CabNoteVizualizarePage, CabNoteDocumentPage
keywords: operatiuni necorelate, necorectate, errrrrrrrrr, nota contabila, corectie cab, f1135
open: view:notecab
---
<!-- slice: 0084, 0088 -->
Când FOREXE nu poate lega o plată de un angajament, o arată în tabelul **«Operațiuni
necorectate»** de pe pagina de start, cu angajamentul **«ERRRRRRRRRR»**. K-BOT citește tabelul
la fiecare conectare și te ajută să faci **nota contabilă de corecție CAB** (formularul F1135).

## Unde le găsești
<!-- slice: 0084, 0088, 0113, 0114, 0113-01 -->

- Imediat după conectarea la FOREXE, dacă sunt operațiuni noi, se deschide singură fereastra de
  corectare.
- Oricând, din ribbon: **Angajamente › Verificări › «Operațiuni necorelate»**. Butonul apare doar cât
  există operațiuni necorelate.
- Rândurile pe care le-ai omis nu mai apar aici; se regăsesc în **«Operațiuni omise»** (vezi mai jos).
- Fereastra de corectare se deschide ca fereastră peste fereastra principală; o închizi cu «Ieșire» sau cu Esc.

<!-- capture: operatiuni-necorelate | caption: Fereastra de corectare a operațiunilor necorelate | goto: menu:operatiuni_necorelate | prepare: E nevoie de cel puțin o operațiune necorelată în baza unității. Deschideți-o din ribbon, Angajamente › Verificări › «Operațiuni necorelate». | redo: 2026-10-16 12:00 | why: 0113-01: fereastra e o fereastră cu bara ei de titlu, mai mică decât spațiul de lucru, nu o pagină a ferestrei principale -->

## Cum faci nota
<!-- slice: 0088, 0113, 0088-06 -->

1. Pentru **fiecare** operațiune alegi **angajamentul** și **indicatorul** căruia îi aparține.
   Un rând complet primește o bifă.
2. **Salvează tot** se aprinde doar când toate rândurile rămase în notă sunt bifate. Salvează o singură notă
   cu toate corecțiile.
3. K-BOT face PDF-ul notei și te întreabă **«Vrei să încarci NOTA DE CORECȚIE în CAB?»**. La «Da»,
   robotul o încarcă în FOREXE, la transmiterea documentelor electronice.

### Un rând care nu se corectează de aici
<!-- slice: 0088-06 -->

Unele rânduri sunt operațiuni făcute de mână în FOREXE, fără documente în K-BOT. Pe ele nu poți
face nota de aici. Alege rândul în tabel și apasă **«Omite rândul»** (K-BOT te întreabă dacă ești sigur).
Rândul dispare din tabel, iar **Salvează tot** se aprinde când celelalte rânduri sunt gata.

Un rând omis **rămâne omis**: nu mai apare în fereastra de corectare, nu mai este numărat în
**«Operațiuni necorelate»** (nici în meniu, nici în ribbon) și nu mai apare la conectările următoare,
până îl reiei tu.

Ca să-l reiei: **Meniu › «Operațiuni omise»** (sau din ribbon, **Angajamente › Verificări › «Operațiuni omise»**).
Butonul apare doar cât există rânduri omise. Fereastra arată doar rândurile omise; alegi unul și apeși
**«Reia rândul»**: se întoarce în «Operațiuni necorelate».

Închiderea («Închide» din ribbon sau Esc) nu salvează nimic în plus: operațiunile rămân în K-BOT și le reiei data viitoare.

## Vederea «Note corecție»
<!-- slice: 0088, 0097, 0000-14, 0078-08, 0000-137 -->

Apare doar pe un angajament pentru care s-a făcut cel puțin o notă de corecție (altfel butonul ei nu se vede în bara vederilor).
Lupa din capul arborelui caută o notă; butonul de strângere din subsol («Note») îngustează arborele.
Arată notele care privesc angajamentul selectat: arborele cu notele, rândurile lor și pagina
**Document**, unde documentul se generează cât timp nu e semnat și se descarcă după ce e semnat.
Cât Adobe încă deschide documentul, arborele nu primește altă notă —
[Cât se deschide un document](topic:contabil.liste).

Nota se trimite în CAB cu butonul **«Trimite în CAB»** din ribbon, în grupul **«Note corecție»** (în dreapta, lângă «Închide»): se aprinde când ai ales o notă în arbore (pe o lună rămâne stins) și îți cere o confirmare înainte de încărcare. Pentru o notă deja trimisă își schimbă numele în **«Trimite din nou în CAB»**. Arborele nu mai are meniu cu clic dreapta. Când salvezi o notă nouă din fereastra de corectare, vederea apare singură în bara vederilor, fără să închizi aplicația.

<!-- capture: note-corectie | caption: Vederea «Note corecție» | goto: view:notecab | prepare: Selectați un angajament pentru care s-a făcut o notă de corecție. | redo: 2026-10-17 20:00 | why: 0114-06/14: ribbonul are in dreapta grupul machetei (langa «Inchide») si grupul vederii; meniul din subsolul listei are doua randuri; E-Factura sta sub Extrase in bara vederilor; 0114-05: arborii nu mai au iconita de descarcare a extraselor (cea cu cardul din lista angajamentelor a plecat; descarcarea e butonul din ribbon) -->
