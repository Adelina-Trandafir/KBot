---
id: tur-ddf
title: Drumul unui document de fundamentare
part: contabil
topic: contabil.ddf
---
## De unde pornește
<!-- slice: 0081-02, 0086 -->
target: KbotForm.btnMeniu
Un angajament NOU pornește din MENIU › Angajament nou. O rezervare nouă pe un angajament existent pornește din vederea «Rezervări».

## Adaugă rezervare
<!-- slice: 0081-02, 0081-04 -->
target: RezervariView.tree
goto: view:rezervari
Iconița din stânga, jos, a arborelui rezervărilor arată acțiunea potrivită stării angajamentului: Adaugă rezervare, Definitivează, Derulează sau Generează PDF final. Doar una odată.

## Reviziile documentului
<!-- slice: 0081-01, 0097, 0000-15 -->
target: DdfView.tree
goto: view:ddf
Aici sunt toate reviziile documentului, sub rădăcina «Toate reviziile», pe luni, fiecare cu starea ei: Ciornă, Semnat A, Trimis, PDF final (de semnat B), la director, Aprobat.

## Clic dreapta pe o revizie
<!-- slice: 0081-04, 0097 -->
target: DdfView.tree
Meniul reviziei oferă, după stare: Trimite în FOREXE (sau Reia trimiterea), Modifică revizia, Șterge revizia, Șterge documentul. O revizie semnată nu se mai modifică și nu se mai șterge: îi rămâne doar trimiterea. Pe o lună sau pe rădăcină poți șterge tot ce e sub ea, dacă nimic nu e semnat.

## Paginile reviziei
<!-- slice: 0020-02, 0081-05 -->
target: DdfView.navSub
Vizualizare = valorile reviziei. Document PDF = documentul, unde îl și semnezi. Fișiere = atașamentele, inclusiv capturile venite din FOREXE.

## Ordinea pașilor
<!-- slice: 0081, 0078-06, 0000-14 -->
target: DdfView
Scrii revizia, o semnezi pe Secțiunea A, o trimiți în FOREXE, K-BOT pune Secțiunea B în documentul semnat, iar tu semnezi B. Un document semnat nu se mai generează din nou. Directorul semnează ultimul. După trimitere revizia nu se mai modifică: o schimbare de valori înseamnă o revizie nouă.
