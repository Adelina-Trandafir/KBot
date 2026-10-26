# SLICE-VC08-12 — Antet comun pentru bonuri P/T/C și Consum

Data: 2026-10-23.

Antetul bonurilor de predare, transfer, casare și consum este construit după imaginea furnizată: gestiune în celula superioară, Număr document, Data împărțită în Ziua/Luna/Anul, tipul gestiunii, Predător, Primitor, Nr. comandă și titlul bonului în celula mare din dreapta, unită vertical. Returul de consum folosește aceeași structură, cu titlu și participanți corespunzători returului.

Antetul este comun pentru PDF, Excel și previzualizarea locală a ciornelor sau a documentelor listate fără dreptul de rapoarte. Bonurile MF salvate folosesc acum raportul oficial MF atunci când există dreptul de rapoarte; fallbackul local rămâne disponibil la lipsa acestui drept. Documentele anulate și ciornele păstrează fluxul local existent.

Fiecare document primește antetul, inclusiv când macheta salvată avea alte grupări; adaptarea se face numai în modelul din memorie. Titlul și perioada nu se mai repetă deasupra antetului. Explicațiile sunt păstrate separat. Nu se inventează un număr de comandă dacă sursa nu îl furnizează. Numele și numărul/datele din exemplu nu sunt copiate în aplicație.

Celulele PDF și canvas permit încadrarea denumirilor lungi pe rânduri. Excel folosește celule unite și înălțimi calculate după lățimile finale, prin contractul comun ColumnWidths/configured_widths, inclusiv cheile noi stabile ANTET_BON_*. Nu există scrieri de configurare sau de schemă.

Fișiere:

- PYTHON/routes/vercon/report_voucher.py (nou)
- PYTHON/routes/vercon/report_body.py, report_service.py, report_routes.py
- PYTHON/reporting/form_grid.py (nou), output.py
- PYTHON/static/js/vercon/voucher-header.js (nou)
- PYTHON/static/js/vercon/document-report.js, docs.js, snapshot-pdf.js, report-result.js
- PYTHON/static/css/vercon-rapoarte.css
- PYTHON/reports/vercon/README.md
- implementare_vercon_v2.md și acest jurnal

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).
