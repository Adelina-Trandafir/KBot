# SLICE-VC08-14 — Corecție HTTP 500 la generarea rapoartelor

Data: 2026-10-23.

Utilizatorul a raportat HTTP 500 la POST `/api/vercon/reports/Bon_Consum/generate`, după trecerea generării pe POST.

`report_routes.register.selection` suprascria în bucla filtrelor `values`, inițial `request.values`, cu `selected_ids(key)` (listă sau None). `values.get(...)` pentru opțiuni producea AttributeError; filtrele următoare puteau pierde parametrii POST ori primi lista ca sursă.

Corecție: `request_values` păstrează parametrii cererii, `ids` lista filtrului. Filtrele și opțiunile au aceeași sursă GET/POST pe toată parsarea. Macheta antetului rămâne neschimbată.

Fișiere: PYTHON/routes/vercon/report_routes.py; implementare_vercon_v2.md; acest jurnal.

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).
