# SLICE-VC08-15 — Tooltipuri în arbori

Data: 2026-10-23.

## Ce s-a schimbat

Politica comună de tooltip considera depășirea verticală a dreptunghiului fontului și scrollHeight drept text trunchiat, inclusiv pentru etichete care încăpeau pe lățime. Range includea și spațiile de formatare din șablon.

`treeview-tooltip.js` măsoară segmentele de text fără spațiile marginale și nodurile goale. Limitele orizontale provin din viewport și din elementele cu overflow care decupează efectiv. Pe verticală, dreptunghiurile fontului sunt limitate la caseta etichetei înaintea comparației cu panourile exterioare. Depășirea fontului peste propria casetă nu mai este criteriu de tooltip. Politica este comună arborilor inline, dropdown și ComboTree; ajutorul acțiunilor din dreapta rămâne gestionat separat.

## Corectiv 27.10.2026 — zoom de aplicație

Sub zoom de aplicație (ex. 90%), tooltipul apărea pe rânduri al căror text se vedea integral: sumele din dreapta, aflate în ultimii ~10% din lățimea ferestrei, erau declarate tăiate (60–85 px) de elementul `html`. Pe elementul rădăcină `clientWidth` este lățimea ferestrei, fără zoom, iar `innerRect` o înmulțea cu raportul `rect.width / offsetWidth` (0,9), deci zona vizibilă ieșea prea îngustă (1382 px față de 1536 px).

`textClipped` sare peste `document.documentElement` în parcurgerea părinților; fereastra rămâne acoperită de `win.innerWidth`, în aceleași coordonate de ecran ca dreptunghiurile textului. Butonul ⋮ nu a fost vinovat (rândurile de lună, fără buton, erau afectate la fel).

Capcană: pe elementul rădăcină `clientWidth` / `offsetWidth` nu se comportă ca pe celelalte elemente sub zoom; orice calcul de geometrie care îl include trebuie să-l trateze separat.

## Stare

TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026). Corectivul de zoom: confirmat de operator, 27.10.2026.

Fișiere: PYTHON/static/js/components/treeview/treeview-tooltip.js; PYTHON/static/help/vercon.json; implementare_vercon_v2.md și acest jurnal.
