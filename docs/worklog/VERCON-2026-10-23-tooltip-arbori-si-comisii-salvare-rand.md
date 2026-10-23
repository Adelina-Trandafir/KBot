# VERCON — tooltipuri arbori și salvarea comisiilor

Data: 2026-10-23.

## Arbori

Politica comună de tooltip considera depășirea verticală a dreptunghiului
fontului și scrollHeight drept text trunchiat, inclusiv pentru etichete care
încăpeau pe lățime. Range includea și spațiile de formatare din șablon.

`treeview-tooltip.js` măsoară segmentele de text fără spațiile marginale și
nodurile goale. Limitele orizontale provin din viewport și din elementele cu
overflow care decupează efectiv. Pe verticală, dreptunghiurile fontului sunt
limitate la caseta etichetei înaintea comparației cu panourile exterioare.
Depășirea fontului peste propria casetă nu mai este criteriu de tooltip.
Politica este comună arborilor inline, dropdown și ComboTree; ajutorul
acțiunilor din dreapta rămâne gestionat separat.

## Comisii

Ambele nomenclatoare, comisie_nir și comisie_inventar, activează mecanismul
existent rowSave. Confirmarea unei celule păstrează modificarea în rândul
local; persistarea rândului este făcută prin onRowLeave, când se trece pe
alt rând sau se părăsește tabelul. Comanda explicită Salvează rândul rămâne
disponibilă pentru finalizare/reîncercare. Validarea câmpurilor obligatorii
este făcută la nivel de rând și folosește mesaje specifice comisiei.

Finalizarea unui rând nou permite trecerea la rândul următor din prima;
blocarea navigării după adăugarea explicită rămâne specifică produselor.
Helpul precizează că trecerea între celulele aceleiași comisii nu salvează.

## Stare

SCRIS LOCAL, NEPUBLICAT. Numai analiză de design pe surse. Nu au fost rulate
teste, verificări automate/de sintaxă sau browser, conform instrucțiunilor
proiectului. Fără acces MariaDB, SQL executat sau modificări de schemă.

Fișiere: PYTHON/static/js/components/treeview/treeview-tooltip.js;
PYTHON/static/js/vercon/nomenclator-grid.js; PYTHON/static/help/vercon.json;
implementare_vercon_v2.md și acest jurnal.
