---
id: tur-ord
title: Ordonanțările
part: contabil
topic: contabil.vederi.ord
---
## Plățile neordonanțate
<!-- slice: 0049-01, 0000-23, 0000-30 -->
target: PlatiView.tree
part: node.icon
goto: view:plati
O ordonanțare se face din plăți. În vederea Plăți, semnul + apare pe zilele (și lunile) cu plăți neordonanțate; fără plăți neordonanțate nu se vede. Pe o zi face o ordonanțare din plățile ei și o deschide în editor; pe o lună face câte una pentru fiecare zi și le salvează direct.

## Arborele ordonanțărilor
<!-- slice: 0033, 0097 -->
target: OrdView.tree
goto: view:ord
Ordonanțările angajamentului, pe luni, sub rădăcina «Toate ordonanțările», cu data și totalul fiecăreia. Dacă vederea nu se deschide, angajamentul ales nu are încă ordonanțări: alege unul care are.

## Arbore › Lupa
<!-- slice: 0027, 0000-23 -->
target: OrdView.tree
part: header.search
Caută o ordonanțare. Esc golește căutarea și închide banda.

## Arbore › Adaugă
<!-- slice: 0049, 0000-23 -->
target: OrdView.tree
part: footer.right
Face același lucru ca «Adaugă ordonanțare…» din meniul de clic dreapta: o ordonanțare nouă, din plățile unei zile.

## Arbore › Strânge arborele
<!-- slice: 0027-02, 0000-23 -->
target: OrdView.tree
part: footer.collapse
Îngustează arborele la o fâșie, ca documentul din dreapta să aibă tot locul; încă un clic îl desface.

## Clic dreapta pe arbore
<!-- slice: 0049, 0049-01, 0097 -->
target: OrdView.tree
Meniul are: Adaugă ordonanțare (întreabă ziua plăților), Modifică ordonanțarea, Șterge ordonanțarea (plățile ei redevin neordonanțate) și Generare în lot (câte una pentru fiecare zi cu plăți neordonanțate; se oprește la prima eroare). Pe o lună sau pe rădăcina «Toate ordonanțările» poți șterge toate ordonanțările de sub ea, dacă niciuna nu e semnată. Pe o ordonanțare semnată nu apare niciun meniu: nu se mai modifică și nu se mai șterge.

## Paginile ordonanțării
<!-- slice: 0033, 0041, 0000-14 -->
target: OrdView.navSub
Ordonanțarea aleasă se vede pe două pagini. Urmează fiecare.

## Pagini › Vizualizare
<!-- slice: 0033, 0000-23 -->
target: OrdView.navSub
part: item:vizualizare
Rândurile ordonanțării și, pe fiecare beneficiar, codul fiscal, contul IBAN și documentele justificative.

## Pagini › Document
<!-- slice: 0041, 0000-14, 0000-23 -->
target: OrdView.navSub
part: item:document
PDF-ul ordonanțării; dacă lipsește, butonul Generează îl face din datele salvate. Un document semnat nu se mai generează din nou.

## Semnarea
<!-- slice: 0078, 0000-10 -->
target: OrdView.pnlPages
Pe pagina Document semnezi în Adobe, pe rând: Validare formular, semnătura 1 (compartimentul de specialitate); apoi din nou Validare formular, semnătura 2 (persoana cu acces la sistemul de control al angajamentelor); CFP dacă e cazul; la sfârșit ordonatorul. Documentul e complet cu semnăturile 1, 2 și 5.
