---
id: contabil.notecab
title: Operațiuni necorelate și note de corecție CAB
part: contabil
order: 60
parent: contabil
screens: CabNoteForm, CabNoteReceiptForm, NoteCabView, CabNoteVizualizarePage, CabNoteDocumentPage
keywords: operatiuni necorelate, necorectate, errrrrrrrrr, nota contabila, corectie cab, f1135
---
Când FOREXE nu poate lega o plată de un angajament, o arată în tabelul **«Operațiuni
necorectate»** de pe pagina de start, cu angajamentul **«ERRRRRRRRRR»**. K-BOT citește tabelul
la fiecare conectare și te ajută să faci **nota contabilă de corecție CAB** (formularul F1135).

## Unde le găsești

- Imediat după conectarea la FOREXE, dacă sunt operațiuni noi, se deschide singură fereastra de
  corectare.
- Oricând, din **MENIU › (!) Operațiuni necorelate**. Intrarea apare doar când există
  operațiuni necorelate; atunci și butonul MENIU poartă semnul **(!)**.

<!-- capture: operatiuni-necorelate | caption: Fereastra de corectare a operațiunilor necorelate | goto: menu:operatiuni_necorelate | prepare: E nevoie de cel puțin o operațiune necorelată în baza unității. -->

## Cum faci nota

1. Pentru **fiecare** operațiune alegi **angajamentul** și **indicatorul** căruia îi aparține.
   Un rând complet primește o bifă.
2. **Salvează tot** se aprinde doar când toate rândurile sunt bifate. Salvează o singură notă
   cu toate corecțiile.
3. K-BOT face PDF-ul notei și te întreabă **«Vrei să încarci NOTA DE CORECȚIE în CAB?»**. La «Da»,
   robotul o încarcă în FOREXE, la transmiterea documentelor electronice.

«Ieșire» nu salvează nimic în plus: operațiunile rămân în K-BOT și le reiei data viitoare.

## Vederea «Note corecție»

Arată notele care privesc angajamentul selectat: arborele cu notele, rândurile lor și pagina
**Document**, unde documentul se generează cât timp nu e semnat și se descarcă după ce e semnat.

<!-- capture: note-corectie | caption: Vederea «Note corecție» | goto: view:notecab | prepare: Selectați un angajament pentru care s-a făcut o notă de corecție. -->
