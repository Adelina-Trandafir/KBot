---
id: tur-notecab
title: Vederea «Note corecție»
part: contabil
topic: contabil.notecab
---
## Notele angajamentului
<!-- slice: 0088, 0097, 0000-20 -->
target: NoteCabView.tree
goto: view:notecab
Notele de corecție CAB care privesc angajamentul selectat. Vederea apare doar pe un angajament pentru care s-a făcut cel puțin o notă; altfel butonul ei e gri.

## Arbore › Lupa
<!-- slice: 0027, 0000-23 -->
target: NoteCabView.tree
part: header.search
Caută o notă. Esc golește căutarea și închide banda.

## Arbore › Strânge arborele
<!-- slice: 0027-02, 0000-23 -->
target: NoteCabView.tree
part: footer.collapse
Îngustează arborele la o fâșie, ca pagina din dreapta să aibă tot locul; încă un clic îl desface.

## Rândurile și documentul
<!-- slice: 0088, 0000-20 -->
target: NoteCabView.pnlPages
Nota aleasă se vede pe trei pagini. Urmează fiecare.

## Pagini › Vizualizare
<!-- slice: 0088, 0000-23 -->
target: NoteCabView.navSub
part: item:vizualizare
Rândurile notei alese.

## Pagini › Document
<!-- slice: 0088, 0000-23 -->
target: NoteCabView.navSub
part: item:document
Documentul notei: se generează cât timp nu e semnat și se descarcă după ce e semnat.

## Pagini › Recipisă
<!-- slice: 0088, 0000-23 -->
target: NoteCabView.navSub
part: item:recipisa
Recipisa primită din FOREXE pentru încărcarea notei. O notă fără recipisă arată butonul «Validează documentul».

## Unde faci o notă nouă
<!-- slice: 0084, 0088, 0000-20 -->
target: KbotForm.btnMeniu
O notă nouă se face din MENIU › (!) Operațiuni necorelate. Intrarea apare doar când există operațiuni necorelate; atunci și butonul MENIU poartă semnul (!).
