# eFactura: grile sursă, țintă vizibilă și memorare implicită — 23.10.2026

Stare: SCRIS LOCAL, NEPUBLICAT. Numai analiză de design pe surse; fără
teste, verificări automate/de sintaxă, browser sau MariaDB.

Eliminate complet din dialog paragraful vc-efi__hint și eticheta/bifa
vc-efi__remember. Cele două butoane de asociere/propunere sunt pe rândul
selectorului facturii, după «Deschide factura». Selectorul cedează din
lățime; bara nu se înfășoară pe mai multe rânduri la îngustarea ecranului.

Ambele liste de sus folosesc DataGrid-ul comun NIR/BC, cu denumire, CPV,
UM, cantitate și rest. Tabelul din stânga are butonul ↕️ pe rând; acesta
deschide alegerea unei poziții eFactura cu rest. Modelul provine din
ADECHIT Prezență: actionColumn/pickGroupFor și wireChildDrop din app.js.

Drag and drop folosește rowFromNode și evenimentele comune DGV. Rândul
eFactura aflat sub cursor este marcat cu is-drop-target, fundal colorat și
contur punctat. Nu se acceptă antete, zone goale, poziții fără rest sau
drag extern. Evidențierea se curăță la ieșire, drop, dragend, reîncărcare,
ocupare ori închidere. Identificarea țintei urmărește rândul real al DGV,
inclusiv după sortare, filtrare și virtualizare.

Pozițiile eFactura cu cantitatea integral asociată dispar imediat din
lista de sus. Produsele NIR cu rest zero dispar și ele. Perechile rămân în
grila asocierilor, pentru editare/eliminare. Reducerea sau eliminarea unei
alocări reintroduce automat rândurile cu rest pozitiv. Se păstrează filtrele,
lățimile, sensul de sortare ales și selecția dacă rândul mai este vizibil.

Memorarea denumirilor nu mai folosește un flag global pentru toată factura.
Fiecare pereche are în cerere learn=true numai după drag and drop ori
editare manuală a cantității. Propunerile automate, butonul de propuneri,
butonul de asociere a selecțiilor și butonul ↕️ nu marchează perechea pentru
memorie; o editare manuală ulterioară o poate marca. Serverul memorează
numai perechile marcate, în tranzacția «Salvează asocierile», și le include
în audit. Nu se introduc în memorie propuneri doar pentru că alte rânduri
au fost editate. Eliminarea unei perechi înainte de salvare îi elimină și
marcajul; nu se șterge istoricul denumirilor deja memorate.

Fișiere: PYTHON/static/js/vercon/efactura-source-grids.js (nou),
efactura-association.js, efactura-association-grid.js;
PYTHON/routes/vercon/efactura_association.py;
PYTHON/static/css/vercon-efactura.css; PYTHON/static/help/vercon.json;
docs/worklog/VERCON-2026-10-23-efactura-asociere-nir-partiala.md;
implementare_vercon_v2.md; acest jurnal.

Nu necesită DDL sau schimbări de schemă. Publicare: serverul Python și
modulele JS/CSS/help împreună, repornirea serviciului web și reîncărcarea
paginii. Structura existentă rămâne utf8mb3/utf8mb3_general_ci; nu s-a
executat SQL.
