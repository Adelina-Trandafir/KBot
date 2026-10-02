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
<!-- slice: 0081-02, 0081-04, 0000-23, 0000-27, 0000-30 -->
target: RezervariView.tree
part: footer.left
goto: view:rezervari
Iconița din stânga, jos, a arborelui rezervărilor deschide acțiunile documentului potrivite stării angajamentului: Adaugă rezervare, Definitivează, Derulează sau Generează PDF final. Ea apare doar când în arbore nu mai e nicio rezervare cu «+»: un «+» înseamnă o rezervare de adus din FOREXE în K-BOT, iar până o adaugi, iconița rămâne ascunsă. Ca să apară, apeși «+» pe rezervările care îl au.

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

## Lista de tipărire
<!-- slice: 0099, 0101 -->
target: DdfView.printList
goto: view:ddf
Pe o lună sau pe rădăcina «Toate reviziile», pagina Documente arată lista reviziilor de sub rândul ales: semnăturile, data semnării, dacă a fost listat și de câte ori a fost tipărit. Lista nu se vede cât ai o revizie aleasă în arbore: dă clic pe o lună sau pe rădăcină, apoi pe pagina Documente.

## Lista › Bifele
<!-- slice: 0099 -->
target: PrintListPage.grila
part: header
Bifezi rândurile cu care lucrezi. Iconița din capul primei coloane deschide meniul «Selectează / deselectează toate» și «Selectează / deselectează doar cele nelistate». Dacă bifezi «Listat» la un document cu 0 tipăriri, K-BOT întreabă dacă îl marchezi ca listat.

## Lista › Generează și imprimă
<!-- slice: 0099 -->
target: PrintListPage.btnImprima
Trimite la imprimantă documentele bifate, fără să le deschidă. Alegi imprimanta o singură dată. Fiecare document trimis se numără ca tipărit.

## Lista › Salvează local
<!-- slice: 0099 -->
target: PrintListPage.btnSalveaza
Salvează documentele bifate într-un dosar ales de tine. Nu le numără ca tipărite.

## Paginile reviziei
<!-- slice: 0020-02, 0081-05 -->
target: DdfView.navSub
Revizia aleasă se vede pe mai multe pagini. Urmează fiecare.

## Pagini › Vizualizare
<!-- slice: 0020-02, 0000-23, 0101 -->
target: DdfView.navSub
part: item:previzualizare
Valorile reviziei, pe clasificații. Pe o lună sau pe «Toate reviziile» arată valorile tuturor reviziilor de sub rând.

## Pagini › Document PDF
<!-- slice: 0020-02, 0078, 0000-23, 0101 -->
target: DdfView.navSub
part: item:document
Documentul reviziei; aici îl și semnezi, în Adobe. Pe o lună sau pe «Toate reviziile» pagina se numește Documente și arată lista de tipărire.

## Ordinea pașilor
<!-- slice: 0081, 0078-06, 0000-14 -->
target: DdfView
Scrii revizia, o semnezi pe Secțiunea A, o trimiți în FOREXE, K-BOT pune Secțiunea B în documentul semnat, iar tu semnezi B. Un document semnat nu se mai generează din nou. Directorul semnează ultimul. După trimitere revizia nu se mai modifică: o schimbare de valori înseamnă o revizie nouă.
