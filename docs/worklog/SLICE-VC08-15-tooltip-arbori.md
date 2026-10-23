# SLICE-VC08-15 — Tooltipuri în arbori

Data: 2026-10-23.

## Ce s-a schimbat

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

## Stare

TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

Fișiere: PYTHON/static/js/components/treeview/treeview-tooltip.js;
PYTHON/static/help/vercon.json; implementare_vercon_v2.md și acest jurnal.
