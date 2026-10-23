# SLICE-VC08-14 — Corecție HTTP 500 la generarea rapoartelor

Data: 2026-10-23.

Utilizatorul a raportat HTTP 500 la POST
`/api/vercon/reports/Bon_Consum/generate`, după trecerea generării pe POST.

În `report_routes.register.selection`, variabila `values` păstra inițial
`request.values`, dar era suprascrisă în bucla filtrelor cu rezultatul
`selected_ids(key)` — o listă sau None. Citirea ulterioară a opțiunilor prin
`values.get(...)` producea AttributeError; și citirea filtrelor următoare
putea pierde parametrii din corpul POST sau primi o listă ca sursă.

Corecție: `request_values` păstrează exclusiv parametrii cererii, iar `ids`
păstrează lista unui filtru. Atât filtrele individuale, cât și opțiunile
citesc aceeași sursă GET/POST pe toată durata parsării. Macheta antetului
nu este modificată de această corecție.

Fișiere: PYTHON/routes/vercon/report_routes.py; implementare_vercon_v2.md;
acest jurnal.

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).
