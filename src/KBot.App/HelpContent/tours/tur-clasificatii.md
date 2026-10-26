---
id: tur-clasificatii
title: Fereastra «Clasificații cheltuieli»
part: contabil
topic: contabil.nomenclatoare.clasificatii
---
## Arborele clasificațiilor
<!-- slice: 0087, 0000-20, AV01-13 -->
target: ClasificatiiBugetView.tree
goto: menu:clasificatii
Capitol › Subcapitol › Articol › Alineat, cu denumirile alături.

## Paginile ferestrei
<!-- slice: AV01-11 -->
target: ClasificatiiForm.navClasificatii
part: item:venituri
Bara din stânga alege pagina: «Cheltuieli» (cea care se deschide) arată clasificațiile cheltuielilor, «Venituri» pe cele ale veniturilor, cu bugetul și rectificările lor. Pașii de mai jos sunt despre «Cheltuieli».

## Arbore › Lupa
<!-- slice: 0027, 0000-23 -->
target: ClasificatiiBugetView.tree
part: header.search
Caută o clasificație. Esc golește căutarea și închide banda.

## Arbore › Adaugă clasificații
<!-- slice: 0087, 0000-23 -->
target: ClasificatiiBugetView.tree
part: footer.right
Semnul + deschide fereastra de adăugare: alegi sursa, clasificațiile funcționale și pe cele economice, ca la înregistrarea unității.

## Bugetul
<!-- slice: 0087, 0000-20, 0102, 0000-40, 0107-02, 0107 -->
target: ClasificatiiBugetView.gridBuget
Bugetul anului pentru clasificația aleasă, pe versiuni: fiecare rând este bugetul de la data din «Început» încolo, pe trimestre, cu totalul rândului în ultima coloană. Jos nu există total general. Prima coloană arată clasificația; coloanele au aceeași lățime ca la rectificări.

## Buget › Adaugă o versiune
<!-- slice: 0102, 0000-40 -->
target: ClasificatiiBugetView.gridBuget
part: footer.right
Semnul + adaugă o versiune nouă de buget, cu data ei de început; «✕» de pe rând o șterge. Pentru revizii din ianuarie – martie adaugă o versiune care începe la 01.01.

## Rectificările
<!-- slice: 0087, 0000-20 -->
target: ClasificatiiBugetView.gridRectificari
Aceleași coloane plus numărul și data documentului. Data unei rectificări trebuie să fie în anul de lucru.

## Rectificări › Adaugă o rectificare
<!-- slice: 0087, 0000-23 -->
target: ClasificatiiBugetView.gridRectificari
part: footer.right
Semnul + adaugă un rând nou, pe care îl completezi direct în tabel.

## Rectificări › Total
<!-- slice: 0087, 0000-23, 0107-02 -->
target: ClasificatiiBugetView.gridRectificari
part: footer
Rândul de jos însumează rectificările clasificației alese. Când e ales un nod mai sus de alineat, sau nimic, rândul de total lipsește.

## Bugetul în vigoare
<!-- slice: 0107 -->
target: ClasificatiiBugetView.gridTotal
Ultimul buget (cel cu data de început cea mai nouă) plus toate rectificările, pe trimestre. La un nod mai sus de alineat adună toate clasificațiile de sub el.

## Salvează
<!-- slice: 0087, 0000-20, 0102, 0000-40 -->
target: ClasificatiiBugetView.btnSalveaza
Scrie versiunile de buget și rectificările. Dacă treci pe alt nod sau închizi fereastra cu modificări nesalvate, K-BOT te întreabă ce faci cu ele.
