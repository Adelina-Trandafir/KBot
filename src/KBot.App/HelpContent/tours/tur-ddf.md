---
id: tur-ddf
title: Drumul unui document de fundamentare
part: contabil
topic: contabil.ddf
---
## De unde pornește
<!-- slice: 0081-02, 0086, 0097-02 -->
target: KbotForm.btnMeniu
Un angajament NOU pornește din MENIU › Adăugare angajamente... › Angajament nou. O rezervare nouă pe un angajament existent pornește din vederea «Rezervări».

## Adaugă rezervare
<!-- slice: 0081-02, 0081-04, 0000-23 -->
target: RezervariView.tree
part: footer.left
goto: view:rezervari
Iconița din stânga, jos, a arborelui rezervărilor deschide acțiunile documentului potrivite stării angajamentului: Adaugă rezervare, Definitivează, Derulează sau Generează PDF final.

## Reviziile documentului
<!-- slice: 0081-01, 0097, 0000-15 -->
target: DdfView.tree
goto: view:ddf
Aici sunt toate reviziile documentului, sub rădăcina «Toate reviziile», pe luni. Urmează, pe rând, butoanele arborelui.

## Arbore › Lupa
<!-- slice: 0027, 0000-23 -->
target: DdfView.tree
part: header.search
Caută o revizie. Esc golește căutarea și închide banda.

## Arbore › Starea reviziei
<!-- slice: 0081-01, 0000-15, 0000-23 -->
target: DdfView.tree
part: node.icon
Iconița de la capătul rândului arată starea reviziei: Ciornă, Semnat A (gata de trimis), Trimitere întreruptă, Trimis în FOREXE, PDF final (de semnat B), la director, Aprobat. Ține mouse-ul pe rând ca să citești starea.

## Arbore › Strânge arborele
<!-- slice: 0027-02, 0000-23 -->
target: DdfView.tree
part: footer.collapse
Îngustează arborele la o fâșie, ca documentul din dreapta să aibă tot locul; încă un clic îl desface.

## Clic dreapta pe o revizie
<!-- slice: 0081-04, 0097 -->
target: DdfView.tree
Meniul reviziei oferă, după stare: Trimite în FOREXE (sau Reia trimiterea), Modifică revizia, Șterge revizia, Șterge documentul. O revizie semnată nu se mai modifică și nu se mai șterge: îi rămâne doar trimiterea. Pe o lună sau pe rădăcină poți șterge tot ce e sub ea, dacă nimic nu e semnat.

## Paginile reviziei
<!-- slice: 0020-02, 0081-05 -->
target: DdfView.navSub
Revizia aleasă se vede pe mai multe pagini. Urmează fiecare.

## Pagini › Vizualizare
<!-- slice: 0020-02, 0000-23 -->
target: DdfView.navSub
part: item:previzualizare
Valorile reviziei, pe clasificații.

## Pagini › Document PDF
<!-- slice: 0020-02, 0078, 0000-23 -->
target: DdfView.navSub
part: item:document
Documentul reviziei; aici îl și semnezi, în Adobe.

## Ordinea pașilor
<!-- slice: 0081, 0078-06, 0000-14 -->
target: DdfView
Scrii revizia, o semnezi pe Secțiunea A, o trimiți în FOREXE, K-BOT pune Secțiunea B în documentul semnat, iar tu semnezi B. Un document semnat nu se mai generează din nou. Directorul semnează ultimul. După trimitere revizia nu se mai modifică: o schimbare de valori înseamnă o revizie nouă.
