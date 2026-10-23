# eFactura: DGV, cantități automate și listă alimente compactă — 23.10.2026

Stare: SCRIS LOCAL, NEPUBLICAT. Numai analiză de design pe surse. Fără
teste, verificări automate/de sintaxă, browser sau MariaDB, conform
instrucțiunilor proiectului.

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

## Lista de alimente

La deschiderea unei liste nu se mai afișează automat mesajul de ordine
cronologică și nici mesajul alternativ despre modificarea porțiilor. Spanul
vc-save-status și containerul de mesaje nu sunt inserate în footer când nu
există un mesaj activ sau o perioadă închisă. Nu este ascundere CSS a textului:
elementul cu mesajul permanent nu se mai generează în documentul afișat.
Pe ecrane înguste, footerul nu rezervă nici rândul secundar de mesaje dacă
nu există containerul de mesaje.

Mesajele unei operații în curs, erorile și confirmările rămân disponibile
atunci când sunt necesare. Restricțiile existente asupra modificării și
anulării, inclusiv excepția pentru porții în ultima lună, rămân aplicate.

## SQL și livrare

Cele trei declarații din sql/EV_02_vercon_asocieri_partiale.sql au acum
CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci, conform precizării utilizatorului.
Nu este necesară schimbarea structurii pentru DGV/cantități; nu s-a executat
SQL și nu s-au modificat baze de date.

Fișiere: PYTHON/static/js/vercon/efactura-association-grid.js (nou),
efactura-association.js, lista.js; PYTHON/static/css/{vercon-efactura,vercon-lista}.css;
PYTHON/static/help/vercon.json; sql/EV_02_vercon_asocieri_partiale.sql;
implementare_vercon_v2.md; acest jurnal.

Publicare necesară: cele trei module JS și CSS/help împreună, apoi
reîncărcarea paginii. Scriptul SQL rămâne numai pentru execuție manuală de
către utilizator, la instalarea structurii.
