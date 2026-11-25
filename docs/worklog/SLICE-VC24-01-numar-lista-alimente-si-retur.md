# SLICE-VC24-01 — Număr listă de alimente și retur din BC asociat

20.11.2026 — Implementat local. Adelina a confirmat adaptarea comenzii de la bonuri și recalcularea la salvarea returului.

## Numărul listei

- **Liste salvate → ⋮ → Modifică număr** deschide fereastra folosită la bonuri. Comanda este disponibilă pentru liste active cu dată, în orice lună a exercițiului deschis, cu drept de scriere.
- Ruta existentă verifică revizia listei, unicitatea în gestiune și exercițiu și ordinea datelor sub blocarea gestiunii. Listele din aceeași zi pot avea orice numere libere din intervalul zilei. Fiecare salvare recitește seria. Astfel, renumerotarea `3 → 2` la 02.01 eliberează `3` pentru lista din 03.01, numerotată inițial `4`.
- Schimbarea se vede imediat în arbore și în istoric. Contorul automat își păstrează maximul.

## Returul dintr-un BC asociat

- Selectorul include toate BC-urile active din context care mai au cel puțin un produs cu o cantitate de returnat, inclusiv cele asociate unei liste. Primitorii se ordonează alfabetic în română, iar bonurile lor după dată și număr.
- La salvarea, modificarea, anularea sau asocierea unui BX migrat, consumul listei se calculează din valorile BC minus valorile BX active ale acelor bonuri. Totalul net și diferența listei sursă se actualizează în tranzacția returului. Soldul cumulativ din pagină și din recapitularea raportului folosește data, apoi numărul listei din aceeași zi. Istoricul consemnează listele ale căror solduri s-au schimbat, cu documentul sursă și valorile înainte și după.
- Retururile existente intră în calcul la următoarea salvare relevantă a unui BX sau la resalvarea listei. Detaliul produselor din raport continuă să prezinte liniile bonurilor de consum. Recapitularea folosește consumul net.

## Fișiere

- Server, `PYTHON/routes/vercon/` — `doc_routes.py`, `documents.py`, `food_balance.py`, `food_list_recalculation.py`, `lista.py`, `migration_links.py`, `number_change.py`, `recalculation_audit.py`, `report_food.py`.
- Interfață, `PYTHON/static/js/vercon/` — `docs.js`, `document-history.js`, `lista.js`, `number-change.js`, `return-editor.js`. Componenta comună este în `PYTHON/static/js/utils/confirm-box.js`; ajutorul este în `PYTHON/static/help/vercon.json`.
- Documentație — acest worklog, `VERCON/VERCON_STATUS.md`, `implementare_vercon_v2.md`.

## Stare

Analiză de proiectare pe surse. Testele, rularea în aplicație și verificarea pe MariaDB rămân de făcut. Publicarea și înregistrarea pentru deployment sunt în așteptare.
