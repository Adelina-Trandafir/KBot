---
id: contabil.liste
title: Arborii și tabelele — butoanele din cap și din subsol
part: contabil
order: 25
parent: contabil
keywords: arbore, tabel, grila, lupa, cautare, subsol, antet, iconita, strange arborele, filtru, palnie, sortare, filtrare, grupare, totaluri, bifeaza tot, butoane
---
<!-- slice: 0025, 0027, 0028, 0000-14 -->
Aproape fiecare vedere are în stânga un **arbore** (lunile, zilele, documentele) și în dreapta un
**tabel**. Amândouă au o bandă **în cap** și una **în subsol**, cu butoane mici. Aici sunt cele care
arată și se poartă la fel peste tot; butoanele proprii fiecărei vederi sunt descrise în topicul ei
(lista de la sfârșit).

Ținând mouse-ul pe orice iconiță afli ce face: fiecare are un bilet cu explicația.

## Capul arborelui
<!-- slice: 0027, 0035 -->

| Unde | Ce este |
|------|---------|
| **stânga** — dosarul | doar semnul arborelui; nu se apasă |
| **lupa** | deschide **banda de căutare** peste arbore: scrii și arborele arată doar ce se potrivește. **Esc** golește căutarea și închide banda; un nou clic pe lupă o închide și el |
| **dreapta** (unde există) | butonul propriu vederii: opțiunile listei, graficul rezervărilor, fereastra Asocieri, modul de afișare al extraselor |

## Subsolul arborelui
<!-- slice: 0025-07, 0027-02, 0034 -->

- **Textul** din subsol («Actualizează», «Adaugă», «Perioade», «Note»...) spune la ce e bun subsolul
  în vederea respectivă.
- **Iconița din dreapta** și, unde există, **cea din stânga** sunt butoane: fac acțiunea din biletul
  lor (de exemplu reîmprospătează din FOREXE sau adaugă un document).
- **Butonul de strângere** (Istoric, Fundamentare, Ordonanțare, Note corecție) îngustează arborele
  la o fâșie, ca tabelul din dreapta să aibă tot locul. Cât e strâns, ținând mouse-ul pe un rând
  vezi rândul întreg, ieșit spre dreapta; încă un clic pe buton desface arborele.

<!-- capture: liste-subsol-arbore | caption: Subsolul unui arbore, cu butonul de strângere și iconița din dreapta | goto: view:ord | prepare: Selectați un angajament cu ordonanțări; ținteți doar subsolul arborelui. -->

## Capul tabelului: pâlnia fiecărei coloane
<!-- slice: 0028-03, 0029, 0030 -->

Pe coloanele care o au, **pâlnia** din dreapta capului de coloană deschide meniul coloanei, cu trei
file:

| Fila | Ce poți face |
|------|--------------|
| **Sortare** | «Sortează Crescător», «Sortează Descrescător», «Resetează sortarea» |
| **Filtrare** | bifezi valorile de păstrat («(Selectează tot)», căutare «Caută…»); «Operatori filtru» pentru o condiție (mai mare decât, între două date...); «Șterge Filtrul» |
| **Grupare** | «Grupează după coloana aceasta», crescător / descrescător, bandă de antet și de subsol pentru fiecare grup, grupuri care se pot strânge |

**OK** aplică, **Anulează** renunță. O pâlnie **plină, colorată** înseamnă că pe coloana aceea e pus
un filtru: tabelul nu arată toate rândurile.

<!-- capture: liste-meniu-coloana | caption: Meniul unei coloane: Sortare, Filtrare, Grupare | goto: view:istoric | prepare: Selectați un angajament cu istoric și apăsați pâlnia unei coloane. -->

## Subsolul tabelului
<!-- slice: 0017-01, 0028-02 -->

- **TOTAL / TOTALURI** — rândul de jos însumează coloanele cu sume, **doar pentru rândurile care se
  văd** (după filtre).
- **«+»** în dreapta subsolului (în Nomenclatoare: rectificări, coduri de angajament) adaugă un rând
  nou, care se completează direct în tabel.

## Bifează / debifează tot
<!-- slice: 0060, 0000-07 -->

Într-un tabel cu o coloană de bife (de exemplu «Ce recepții reîmprospătez?» sau «Golește jurnale»),
iconița din **capul coloanei de bife** bifează tot; încă un clic debifează tot.

## Butoanele fiecărei vederi
<!-- slice: 0000-14 -->

| Vedere / fereastră | Unde sunt descrise |
|--------------------|--------------------|
| Lista angajamentelor | [Fereastra principală](topic:contabil.fereastra) |
| Rezervări | [Rezervări](topic:contabil.vederi.rezervari) |
| Recepții | [Recepții](topic:contabil.vederi.receptii) |
| Plăți, Istoric, Sumar | [Plăți](topic:contabil.vederi.plati), [Istoric](topic:contabil.vederi.istoric), [Sumar](topic:contabil.vederi.sumar) |
| Extrase de cont | [Extrase de cont](topic:contabil.vederi.extrase) |
| Ordonanțare | [Ordonanțare](topic:contabil.vederi.ord) |
| Fundamentare | [Documentul de fundamentare](topic:contabil.ddf) |
| Note corecție | [Note de corecție](topic:contabil.notecab) |
| Clasificații, Parteneri | [Nomenclatoare](topic:contabil.nomenclatoare) |
