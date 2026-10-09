# SLICE-ADE1-04 — ecranul Prezență după Access

## Ce s-a schimbat și de ce

Pagina generică pe cataloage a fost înlocuită cu un singur workspace operațional după
ecranul Access furnizat: arbore lună/an și arbore de grupe în stânga, situația lunară în
centru, comenzi sub grilă și tab-control financiar în footer. Sunt refolosite TreeView,
DataGrid, tema și mesajele comune.

## Fișiere atinse

- `PYTHON/static/adechit.html`
- `PYTHON/static/css/adechit.css`
- `PYTHON/static/js/adechit/app.js`
- `ADECHIT/tools/preview.py` — port configurabil pentru probe locale.

## Verificări

- `node --check PYTHON/static/js/adechit/app.js` — trecut.
- verificare vizuală în browser la 943×871: arbori, grilă, butoane și taburi afișate coerent.
- `pytest PYTHON/tests/test_adechit.py -q` cu basetemp local — 9 teste trecute.

## Neverificat sau amânat

Serverul real, autentificarea și MariaDB nu au fost testate. Transferul copilului rămâne
dezactivat până la decizia M06; rapoartele din dreapta ecranului Access rămân în ADE9.
