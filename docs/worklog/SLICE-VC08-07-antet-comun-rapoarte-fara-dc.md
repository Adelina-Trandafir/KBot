# 2026-10-19 — Antet comun în toate rapoartele VERCON, fără DC

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

Toate rapoartele VERCON primesc antet de tipul situației debitorilor
ADECHIT: denumirea unității, codul fiscal și adresa în stânga;
denumirea subunității active în dreapta. Datele unității provin din
aceleași surse ca în ADECHIT: `AVACONT_COMUN.Unitati` și `Unitati_Date`.
Subunitatea se citește din `VC_Subunitati`, folosind contextul autentificat.
Citirea este făcută la generarea raportului, în sesiunea existentă de citire.

`report_identity.py` construiește identitatea comună, înlocuind antetele
vechi din configurările de unitate/machetă doar în modelul din memorie.
Datele comune prevalează asupra denumirii/codului/adresei din vechile
configurări. Managerul și subsolul configurat sunt păstrate.
Configurările proprietarului de pe disc nu sunt rescrise.

Numele bazei nu mai este valoare implicită pentru numele unității.
Identificatorii bazei și tokenurile DC din textele de prezentare ale
definițiilor vechi sunt eliminate/înlocuite cu numele real al unității.
Valorile economice, cheile și identificatorii din datele raportului
nu trec prin înlocuiri text. Datele de identificare lipsă cer completare;
nu există revenire la afișarea numelui bazei.

PDF-ul generic repetă antetul pe pagini. Formularul NIR/FCS imprimă
același antet înaintea titlului pe fiecare pagină, recalculând spațiul
disponibil pentru produse. Excel exportă datele de identificare pe rânduri
reunite și le repetă la imprimare. Rezultatul în pagină include antetul.
Snapshot-urile cu versiunea veche de antet sunt refuzate la export cu
`REPORT_EXPIRED`, pentru regenerare prin fluxul existent.

Endpoint-ul de citire `/api/vercon/reports/identity` este legat de context
și dreptul de citire; permite antet și la imprimarea locală a unui document
deja încărcat, inclusiv unei ciorne. Snapshot PDF citește identitatea în
momentul generării și o repetă pe pagini. Nu reutilizează o identitate
memorată din altă subunitate. Preview-ul local primește identitate fictivă
explicită și nu se conectează la MariaDB.

Fișiere:

- `PYTHON/routes/vercon/report_identity.py`, `report_layouts.py`,
  `report_service.py`, `report_routes.py`, `report_reception.py`;
- `PYTHON/reporting/output.py`;
- `PYTHON/static/js/vercon/report-identity.js`, `app.js`, `snapshot-pdf.js`,
  `report-result.js`, `reports.js`;
- `PYTHON/static/css/vercon-rapoarte.css`;
- `tools/VerconPreview/preview_server.py`;
- `PYTHON/reports/vercon/README.md`, acest jurnal,
  `implementare_vercon_v2.md`.

Nu s-au rulat teste, build-uri, generări PDF/Excel sau verificări
automate/vizuale, conform AGENTS.md. Numai analiză de design a surselor.
Nu s-au executat interogări MariaDB și nu s-au modificat scheme.
Necesită publicarea surselor Python/JS/CSS și repornirea serviciului.
