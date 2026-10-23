# SLICE-VC08-13 — Balanță OI, generare bonuri și selecție pe primitor

Data: 2026-10-23.

Balanța OI folosește aceeași definiție de machetă ca Balanta_produse,
inclusiv coloanele configurate pentru balanța de magazie, cu titlul OI.
Configurația salvată nu este modificată. Rândurile sunt calculate pe cont
și variantă, fără subdivizare pe deținători. Sold inițial = predări minus
casări înainte de începutul perioadei. Intrări = numai predări în perioada
închisă [început, sfârșit]; Ieșiri = numai casări în aceeași perioadă.
Sold final = SI + Intrări − Ieșiri. Transferurile sunt excluse înaintea
calculului, inclusiv din istoric, și nu au coloane în balanță. Celelalte
fișe OI și rapoartele MF își păstrează calculele și istoricul transferurilor.

În cele patru rapoarte din PRIMITORI, Selectare date pornește de la primitor,
apoi cont și produs. Identitățile bifelor păstrează combinația completă;
gruparea nu combină independent persoane și produse.

Pentru eroarea HTTP 400 la P/T/C s-a identificat în surse trimiterea tuturor
identităților bifate în URL prin GET. Generarea din browser este mutată pe
POST form-url-encoded, cu contextul scurt în URL. Se aplică paginii Rapoarte,
listării directe a documentelor și regenerării la schimbarea perioadei.
Descărcarea PDF/Excel și citirea catalogului din vizualizator trimit numai
contextul. Ruta GET este păstrată pentru compatibilitate; drepturile,
rezolvarea contextului și proprietarul snapshotului se păstrează.

Limită: nu există în spațiul local jurnalul cererii HTTP 400 de pe server;
eroarea nu a fost reprodusă. Trimiterea selecțiilor în corp elimină URL-urile
foarte lungi din acest flux, dar confirmarea cauzei exacte și a rezultatului
pe server rămâne necesară după publicare. Nu s-au rulat teste sau verificări
de execuție, conform AGENTS.md.

Fișiere de aplicație:

- PYTHON/routes/vercon/report_oi.py, report_layouts.py, report_service.py,
  report_mf_selection.py, report_catalog.py, report_routes.py,
  stock_routes.py, kbot_bridge.py
- PYTHON/static/js/vercon/report-request.js (nou), reports.js,
  document-report.js, report-period-action.js
- PYTHON/static/help/vercon.json

Documentație: PYTHON/reports/vercon/README.md;
docs/worklog/VERCON-2026-10-23-rapoarte-oi-structura-si-selectie.md;
implementare_vercon_v2.md și acest jurnal.

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).
