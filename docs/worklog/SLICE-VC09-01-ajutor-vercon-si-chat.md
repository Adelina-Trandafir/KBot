# VC09-01 — Ajutor VERCON și documentarea chatului

Data: 2026-10-21.

VERCON folosește acum structura ajutorului ADECHIT: cuprins cu capitole și subcapitole în stânga, text formatat în dreapta, legături interne, imprimare și descărcare PDF. Acces: ? → Cum lucrez cu VERCON. Intrarea Mesaje din sesiune este păstrată. Conținutul este în `PYTHON/static/help/vercon.json`.

Ghidul acoperă contextul, drepturile și ocuparea gestiunii; meniurile, tema, zoomul și tooltipurile; consultarea, adăugarea, modificarea, numerotarea, salvarea, anularea și istoricul documentelor; NIR cu repartizare, produse noi, stoc inițial, ajustări și MF; storno, BC, retur, liste de alimente; BP/BT/BS; registru MF și amortizare; nomenclatoare și setări; stocuri; rapoarte, selecții, perioade, listare/PDF/Excel; recuperarea legăturilor după migrare; perioade închise, sesiune și situații frecvente.

Sunt descrise explicit limitele versiunii: Furnizori (Parteneri) și Închidere an au pagini în pregătire, iar Stocuri nu este o intrare separată în meniul curent. Se păstrează atenționarea existentă privind loturile OI/MF migrate.

ADECHIT și VERCON primesc același capitol DISCUȚII: audiența conversației comune, filtrele V/A/K, aplicația sursă la trimitere, pseudonim/nume, limitele textului, Enter/Ctrl+Enter, răspunsuri și citate, istoric, necitite, actualizare, păstrarea ciornei în pagină și expirarea sesiunii. ADECHIT are și o trimitere către chat în Primii pași.

Randarea JS este extrasă în `PYTHON/static/js/help/help.js`, cu intrări mici pentru fiecare aplicație. Clasele de prezentare ade-help sunt păstrate pentru compatibilitatea aspectului ADECHIT. VERCON adaugă numai cadrul său de dialog în `PYTHON/static/css/vercon-help.css`, importat din vercon.css. Rendererul PDF comun este `PYTHON/reporting/help_pdf.py`; fiecare aplicație alege propriul JSON și autor. Noua rută `/api/vercon/help/pdf` folosește guardul VERCON pentru read, fără a necesita selectarea unei gestiuni.

Fișiere:

- PYTHON/static/help/vercon.json (nou), adechit.json;
- PYTHON/static/js/help/help.js (nou), adechit/help.js, vercon/help.js (nou), vercon/app.js;
- PYTHON/static/css/vercon-help.css (nou), vercon.css; PYTHON/static/vercon.html;
- PYTHON/reporting/help_pdf.py (nou), PYTHON/routes/adechit/help_pdf.py;
- PYTHON/routes/vercon/help_pdf.py (nou), __init__.py;
- docs/worklog/SLICE-VC09-01-ajutor-vercon-si-chat.md; implementare_vercon_v2.md.

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

Publicarea necesită JSON-urile, modulele JS/CSS și fișierele Python comune și specifice, plus repornirea serviciului web pentru noua rută PDF.

Reperul din jurnalul general descria ajustările NIR de maximum 1 leu; este limita documentată în ajutorul acestei intervenții, fără extindere prin reorganizarea D001-04.
