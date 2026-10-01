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
Rezervările, pe luni și zile. Rezervarea inițială apare o singură dată, în ziua în care a devenit definitivă. Urmează, pe rând, butoanele arborelui.

## Arbore › Lupa
<!-- slice: 0027, 0000-23 -->
target: RezervariView.tree
part: header.search
Caută în arbore: scrii și arborele arată doar ce se potrivește. Esc golește căutarea și închide banda.

## Arbore › Graficul rezervărilor
<!-- slice: 0061-02, 0000-23 -->
target: RezervariView.tree
part: header.right
Deschide, într-o fereastră separată, evoluția rezervărilor angajamentului și totalurile lui pe luni.

## Arbore › Semnul «+»
<!-- slice: 0014, 0081-02, 0000-23 -->
target: RezervariView.tree
part: node.icon
Stă pe rezervarea pentru care nu există încă document de fundamentare și pornește documentul pentru ea. Dacă nicio rezervare nu are nevoie de document, semnul nu apare și pasul acesta se sare.

## Arbore › Acțiunile documentului
<!-- slice: 0060, 0081-02, fara-felie, 0000-23 -->
target: RezervariView.tree
part: footer.left
Meniul documentului de fundamentare pe acest angajament: Adaugă rezervare, Definitivează, Derulează, Generează PDF final (cele care se pot face acum), plus «Reanalizează rezervările», care reface pe server ordinea rezervărilor din istoric, fără nicio descărcare; îți arată întâi cifrele și abia apoi scrie.

## Arbore › Reîmprospătează
<!-- slice: 0060, 0000-23 -->
target: RezervariView.tree
part: footer.right
Aduce din FOREXE doar rezervările acestui angajament: antetul, indicatorii și istoricul.

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
