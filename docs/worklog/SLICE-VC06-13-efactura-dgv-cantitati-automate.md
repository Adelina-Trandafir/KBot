# SLICE-VC06-13 — eFactura: DGV și cantități automate la asocierea NIR — 23.10.2026

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

## Asocierea NIR/eFactura

Tabelul HTML al perechilor a fost înlocuit cu DataGrid-ul comun, prin același
adaptor grid/column folosit la NIR, BC și bonuri. Cantitățile se editează în
celule. Opțiunea de eliminare este în meniul ⋮ al rândului. Grila păstrează
sortarea aleasă și lățimile la schimbarea perechilor; ordinea inițială este
după denumirile produselor în română, cu cantități comparate numeric.
Etichetele includ UM, dar cheile de sortare ale produselor sunt denumirile.

La drag and drop sau la asociere prin buton, ambele cantități se completează
cu minimul disponibil în NIR și eFactura, inclusiv când UM diferă textual,
ca KG/KGM. Nu se deduce un factor de conversie; utilizatorul poate ajusta
separat cele două cantități. La confirmarea celulei se validează valori
pozitive, maximum patru zecimale și restul documentului corespunzător,
scăzând toate celelalte perechi. Serverul păstrează verificarea tranzacțională
a resturilor dintre toate NIR-urile.

Celula activă se confirmă înainte de salvare, asociere nouă, propuneri sau
eliminare. Un edit invalid împiedică salvarea. Grila este ocupată în timpul
cererii și este distrusă la închiderea dialogului. Stilurile butoanelor mari
ale dialogului sunt limitate la acțiunile dialogului, fără a afecta
butoanele și editorii DGV.

## SQL și livrare

Cele trei declarații din sql/EV_02_vercon_asocieri_partiale.sql au acum
CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci, conform precizării utilizatorului.
Nu este necesară schimbarea structurii pentru DGV/cantități; nu s-a executat
SQL și nu s-au modificat baze de date.

Fișiere: PYTHON/static/js/vercon/efactura-association-grid.js (nou),
efactura-association.js; PYTHON/static/css/vercon-efactura.css;
PYTHON/static/help/vercon.json; sql/EV_02_vercon_asocieri_partiale.sql;
implementare_vercon_v2.md; acest jurnal.

Publicare necesară: modulele JS și CSS/help împreună, apoi
reîncărcarea paginii. Scriptul SQL rămâne numai pentru execuție manuală de
către utilizator, la instalarea structurii.
