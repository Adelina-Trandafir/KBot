# SLICE-VC07-06 — Numărul copiilor în orice listă din ultima lună

Data: 2026-10-23.

Numărul copiilor/porțiilor poate fi corectat în orice listă activă din ultima lună cu liste de alimente active a gestiunii. Luna se determină din data maximă a listelor active, pe toate exercițiile gestiunii, independent de luna calendaristică și de anul ales în interfață. Perioada închisă, dreptul Alimente și blocarea curentă a gestiunii rămân obligatorii.

Pentru listele mai vechi decât ultima, dar din luna eligibilă, butonul și meniul contextual oferă „Modifică numărul copiilor”. Modul permite numai Porții la Mic dejun, Dejun și Cină/Gustare. Data, numărul, comentariile, prețurile, meniurile și legăturile cu BC nu sunt editabile în acest mod. Modificarea completă și anularea păstrează regula ultimului document. Chiar și modificarea completă a ultimei liste verifică luna când se schimbă numărul porțiilor; un an vechi nu ocolește restricția.

Ruta dedicată PUT /api/vercon/doc/lista/<id>/portions primește numai cele trei numere de porții și versiunea listei, pe lângă context. Serverul verifică din nou luna și starea listei în tranzacția cu blocarea gestiunii, recitește prețurile păstrate și calculează exact cu Decimal, la ZEC: valoare masă = porții × preț; valoare listă = suma meselor; DEP = valoare listă − totalul BC asociate. Sumarul, antetul, versiunea și auditul se salvează împreună. Mesele fără porții pot avea valoare zero fără a necesita un preț istoric completat; un număr pozitiv cere preț păstrat.

Economiile și depășirile curente și cumulative se calculează deja la consultare și generarea raportului din DEP și istoricul listelor, prin food_balance.py; diferența salvată actualizează astfel și soldurile listelor următoare, fără rescrierea artificială a antetelor lor. Previzualizările deja generate trebuie regenerate. Nu sunt introduse tabele sau coloane.

Fișiere de aplicație:

- PYTHON/routes/vercon/food_list_portions.py (nou)
- PYTHON/routes/vercon/lista.py
- PYTHON/routes/vercon/doc_routes.py
- PYTHON/static/js/vercon/lista.js
- PYTHON/static/help/vercon.json

Documentație: acest jurnal și implementare_vercon_v2.md.

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).
