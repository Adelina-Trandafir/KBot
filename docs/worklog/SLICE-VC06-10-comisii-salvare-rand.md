# SLICE-VC06-10 — Salvarea comisiilor la ieșirea de pe rând

Data: 2026-10-23.

## Ce s-a schimbat

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

TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

Fișiere: PYTHON/static/js/vercon/nomenclator-grid.js; PYTHON/static/help/vercon.json;
implementare_vercon_v2.md și acest jurnal.
