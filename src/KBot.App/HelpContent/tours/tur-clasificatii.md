---
id: tur-clasificatii
title: Fereastra «Clasificații bugetare»
part: contabil
topic: contabil.nomenclatoare.clasificatii
---
## Arborele clasificațiilor
<!-- slice: 0087, 0000-20 -->
target: ClasificatiiForm.tree
goto: menu:clasificatii
Capitol › Subcapitol › Articol › Alineat, cu denumirile alături.

## Arbore › Lupa
<!-- slice: 0027, 0000-23 -->
target: ClasificatiiForm.tree
part: header.search
Caută o clasificație. Esc golește căutarea și închide banda.

## Arbore › Adaugă clasificații
<!-- slice: 0087, 0000-23 -->
target: ClasificatiiForm.tree
part: footer.right
Semnul + deschide fereastra de adăugare: alegi sursa, clasificațiile funcționale și pe cele economice, ca la înregistrarea unității.

## Bugetul
<!-- slice: 0087, 0000-20, 0102, 0000-40, 0107-02 -->
target: ClasificatiiForm.gridBuget
Bugetul anului pentru clasificația aleasă, pe versiuni: fiecare rând este bugetul de la data din «Început» încolo, pe trimestre, cu totalul rândului în ultima coloană. Jos nu există total general.

## Buget › Adaugă o versiune
<!-- slice: 0102, 0000-40 -->
target: ClasificatiiForm.gridBuget
part: footer.right
Semnul + adaugă o versiune nouă de buget, cu data ei de început; «✕» de pe rând o șterge. Pentru revizii din ianuarie – martie adaugă o versiune care începe la 01.01.

## Rectificările
<!-- slice: 0087, 0000-20 -->
target: ClasificatiiForm.gridRectificari
Aceleași coloane plus numărul și data documentului. Data unei rectificări trebuie să fie în anul de lucru.

## Rectificări › Adaugă o rectificare
<!-- slice: 0087, 0000-23 -->
target: ClasificatiiForm.gridRectificari
part: footer.right
Semnul + adaugă un rând nou, pe care îl completezi direct în tabel.

## Rectificări › Total
<!-- slice: 0087, 0000-23, 0107-02 -->
target: ClasificatiiForm.gridRectificari
part: footer
Rândul de jos însumează rectificările clasificației alese. Când e ales un nod mai sus de alineat, sau nimic, rândul de total lipsește.

## Salvează
<!-- slice: 0087, 0000-20, 0102, 0000-40 -->
target: ClasificatiiForm.btnSalveaza
Scrie versiunile de buget și rectificările. Dacă treci pe alt nod sau închizi fereastra cu modificări nesalvate, K-BOT te întreabă ce faci cu ele.
