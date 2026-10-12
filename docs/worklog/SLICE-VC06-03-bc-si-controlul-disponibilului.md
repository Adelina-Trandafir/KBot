# SLICE-VC06-03 — BC și controlul disponibilului

Stare: **IMPLEMENTAT_LOCAL**. Doar sintaxa verificata; **nimic rulat**, nimic vazut pe ecran, nimic pe MariaDB. Cererea operatorului, 12.10.2026 («fa tot VC06»).

## Ce s-a facut
- **Bon de consum** (`create_bc/edit_bc`): primitor (`VC_Clienti`), categorie/cont, numar (automat, manual doar liber), data, explicatii; linii = varianta + cantitate. Pretul liniei este **pretul cu TVA al variantei la momentul salvarii**; linia il pastreaza apoi (si la editare, pentru o varianta deja pe bon). `ValoareBC` = suma `ValoareCUTVA`. Disponibil si pentru Alimente; nu pentru OI/MF.
- **Stoc negativ interzis** (`protocol.check_timeline`): in aceeasi tranzactie cu orice scriere, stocul fiecarei variante atinse este reparcurs pe zile (intrari la `DataFc`, retururi si iesiri la data bonului, storno la data documentului storno; documentele anulate nu conteaza; adunarile unei zile inaintea scaderilor ei). Daca vreo zi coboara sub 0 → refuz `STOCK_NEGATIVE`, rollback. Acopera si editarea/anularea unui document mai vechi.
- **Disponibil la data** (`GET /api/vercon/stock/available`): informativ in editor; documentul editat nu se numara de doua ori (`exclude`). La salvare decide verificarea de mai sus, la precizia gestiunii (1,0040 pe 1,0000 → refuz).
- **Acces exclusiv:** al doilea utilizator e blocat de `scoped()` (jeton de ocupare), inclusiv pe alt an.
- Pagina: grila BC compacta (NIR), coloana «Disponibil» cu avertizare, arbore **primitor → data → BC + total**.

## Fisiere
`PYTHON/routes/vercon/`: `protocol.py`, `documents.py`, `doc_routes.py`; `PYTHON/static/js/vercon/docs.js`, `app.js`; `PYTHON/static/css/vercon.css`.

## Presupuneri si neverificat
- Stocul se calculeaza pe exercitiu (un exercitiu are stoc initial propriu, cum le creeaza inchiderea anului la VC08); intre exercitii nu se aduna.
- Nimic rulat; nu s-a masurat timpul verificarii pe un volum real (Alimente are mii de linii: verificarea filtreaza pe variantele atinse).
