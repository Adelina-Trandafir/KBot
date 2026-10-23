# SLICE-ADE6-26 — Colapsarea anului selectat anterior

12.10.2026. TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

## Ce s-a schimbat și de ce

La alegerea unei luni din alt an în arborele LUNA / ANUL, ramura anului
selectat anterior se colapsează, iar ramura anului noii luni rămâne extinsă.
Prima alegere folosește drept an anterior anul deschis automat la pornire.
Schimbarea între luni ale aceluiași an păstrează ramurile existente.
Extinderea manuală a unui an pentru consultare nu colapsează anul selectat.

Logica este în chooseMonth, înaintea sincronizării selecției și încărcării
grupelor. Starea ramurilor este păstrată de mecanismul existent la salvare
și reîncărcare; nu se schimbă componenta TreeView comună.

## Fișiere atinse

- PYTHON/static/js/adechit/app.js.
- ADECHIT/UTILIZARE_WEB.md.
- ADECHIT_STATUS.md și docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md.
- Acest worklog.

## Rezultatele testelor

Nu s-au rulat teste, verificări de sintaxă, server sau browser.
Starea este TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

## Neverificat și predare

*Rezolvat: operatorul a confirmat, la 23.10.2026, că nu mai este nimic nevalidat sau deschis.*

Utilizatorul publică PYTHON/static/js/adechit/app.js prin AvacontPush și
probează prima alegere din alt an, trecerea între ani și păstrarea ramurii
noii luni după salvare/reîncărcare. Fără SQL, publicare, commit sau push
executate din chat. Următoarea subfelie liberă: ADE6-27.
