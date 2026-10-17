---
id: tur-rezervari
title: Vederea «Rezervări»
part: contabil
topic: contabil.vederi.rezervari
---
## Alege un angajament
<!-- slice: 0014 -->
target: KbotForm.tree
goto: view:rezervari
Vederea arată rezervările angajamentului selectat aici. Dacă nu e niciunul selectat, alege acum unul care are rezervări, apoi apasă «Înainte».

## Arborele rezervărilor
<!-- slice: 0014, 0092 -->
target: RezervariView.tree
Rezervările, pe luni și zile. Rezervarea inițială apare o singură dată, în ziua în care a devenit definitivă. Urmează butoanele arborelui și, apoi, cele din ribbon.

## Arbore › Lupa
<!-- slice: 0027, 0000-23 -->
target: RezervariView.tree
part: header.search
Caută în arbore: scrii și arborele arată doar ce se potrivește. Esc golește căutarea și închide banda.

## Arbore › Semnul «+»
<!-- slice: 0014, 0081-02, 0000-23, 0000-30 -->
target: RezervariView.tree
part: node.icon
Stă pe rezervarea pentru care nu există încă document de fundamentare și pornește documentul pentru ea. Dacă nicio rezervare nu mai are nevoie de document, semnul nu se vede.

## Ribbon › Grafice
<!-- slice: 0061-02, 0000-23, 0114-15, 0000-91 -->
Butoanele vederii sunt în ribbon, în grupul «Rezervări» din dreapta (lângă grupul «Lista angajamentelor»). «Grafice» deschide, într-o fereastră separată, evoluția rezervărilor angajamentului și totalurile lui pe luni.

## Ribbon › Acțiunile documentului
<!-- slice: 0060, 0081-02, fara-felie, 0000-23, 0000-27, 0000-30, 0114-15, 0000-91 -->
Butonul își schimbă numele după ce se poate face acum: Adaugă rezervare, Definitivează, Derulează sau Generează PDF final (documentul de fundamentare al angajamentului). Apare doar când nu mai e nicio rezervare cu «+» în arbore; ca să apară, apeși «+» pe rezervările care îl au.

## Ribbon › Repară...
<!-- slice: 0060, 0000-23, 0092-01, 0000-68, 0114-15, 0000-91 -->
Repară rezervările acestui angajament din istoric, fără nicio descărcare din FOREXE: îți arată întâi câte rânduri se schimbă și scrie abia după ce confirmi. Butonul se vede doar când angajamentul are rezervări greșite (acelea îl fac roșu în lista principală) și repararea nu se face într-o lună cu revizii.

## Valorile pe clasificații
<!-- slice: 0014 -->
target: RezervariView.grid
Pentru rezervarea aleasă: câte un rând pe clasificație, cu creditul bugetar și valorile inițială, curentă și definitivă.

## Tabel › Pâlnia unei coloane
<!-- slice: 0028-03, 0030, 0000-23 -->
target: RezervariView.grid
part: header.filter
Deschide meniul coloanei, cu trei file: Sortare, Filtrare, Grupare. O pâlnie plină, colorată, arată că pe coloana aceea e pus un filtru.

## Tabel › TOTALURI
<!-- slice: 0017-01, 0000-23 -->
target: RezervariView.grid
part: footer
Rândul de jos însumează coloanele cu sume, doar pentru rândurile care se văd după filtre.
