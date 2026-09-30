---
id: tur-ord
title: Ordonanțările
part: contabil
topic: contabil.vederi.ord
---
## Plățile neordonanțate
target: PlatiView.tree
goto: view:plati
O ordonanțare se face din plăți. În vederea Plăți, semnul + apare pe zilele (și lunile) cu plăți neordonanțate. Pe o zi face o ordonanțare din plățile ei și o deschide în editor; pe o lună face câte una pentru fiecare zi și le salvează direct.

## Arborele ordonanțărilor
target: OrdView.tree
goto: view:ord
Ordonanțările angajamentului, pe luni, cu data și totalul fiecăreia. Dacă vederea nu se deschide, angajamentul ales nu are încă ordonanțări: alege unul care are.

## Clic dreapta pe arbore
target: OrdView.tree
Meniul are: Adaugă ordonanțare (întreabă ziua plăților), Modifică ordonanțarea, Șterge ordonanțarea (plățile ei redevin neordonanțate) și Generare în lot (câte una pentru fiecare zi cu plăți neordonanțate; se oprește la prima eroare). Iconița Adaugă din subsolul arborelui face același lucru ca Adaugă ordonanțare.

## Paginile ordonanțării
target: OrdView.navSub
Vizualizare = rândurile ordonanțării și beneficiarii ei. Document = PDF-ul; dacă lipsește, butonul Generează îl face din datele salvate.

## Semnarea
target: OrdView.pnlPages
Pe pagina Document semnezi în Adobe, pe rând: Validare formular, semnătura 1 (compartimentul de specialitate); apoi din nou Validare formular, semnătura 2 (persoana cu acces la sistemul de control al angajamentelor); CFP dacă e cazul; la sfârșit ordonatorul. Documentul e complet cu semnăturile 1, 2 și 5.
