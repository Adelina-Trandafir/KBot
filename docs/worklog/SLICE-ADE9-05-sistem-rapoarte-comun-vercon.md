# SLICE-ADE9-05 — sistem de rapoarte comun cu VERCON

19.10.2026. TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026). Cerere: rapoartele ADECHIT să folosească sistemul VERCON.

## Comportamentul rezultat

- Rapoartele din panoul ADECHIT și dispozițiile de plată generează un model o
  singură dată. Previzualizarea PDF și exportul Excel folosesc acel model,
  păstrat 10 minute într-un snapshot legat de email, DC și subunitate.
- Previzualizarea PDF este aceeași componentă folosită de VERCON, în aplicație:
  tipărire, alegerea imprimantei, descărcare PDF, zoom, potrivire la lățime,
  100%, navigare între pagini și închidere. Chitanțele folosesc același viewer;
  imprimarea lor se pornește din bara raportului.
- Excel păstrează selecția coloanelor ADECHIT, cheile existente, valorile
  numerice și citirea lățimilor configurate la fiecare export. Selectarea unui
  subset de coloane adaptează și celulele reunite ale subtotalurilor.
- Motorul generic PDF/Excel este extras din VERCON în infrastructura comună.
  Adaptorul ADECHIT traduce antetul, subtitlurile, culorile și formulele fără
  schimbarea calculelor de business. Nota HTML a Situației de plată rămâne un
  bloc formatat în PDF și text simplu în Excel.
- PDF-ul fișei din portalul părinților folosește motorul comun. Formularul
  dedicat chitanței rămâne specializat, similar formularului NIR din VERCON.
- VERCON folosește modulele extrase prin adaptoare compatibile, păstrând
  identitatea, cheile Excel duplicate, machetele și rendererul dedicat NIR.
- Previzualizările sunt eliberate la închidere, navigare, schimbarea contextului
  ADECHIT sau expirarea sesiunii. Rutele vechi de ieșire directă/HTML rămân
  disponibile integrărilor existente.

Configurarea SQL/machetelor VERCON din KBotAdmin nu este modificată. Sursele
datelor ADECHIT rămân constructorii existenți, cu situația salvată pentru lunile
închise și calculul existent pentru luna deschisă. Nu există DDL sau seed nou.

## Fișiere de publicat împreună

Din `PYTHON`, prin AvacontPush:

- `reporting/output.py`, `errors.py`, `snapshots.py`, `richtext.py` (noi).
- `routes/adechit/__init__.py`, `report_routes.py` (nou), `report_model.py` (nou),
  `report_render.py`, `report_excel.py`, `report_richtext.py`, `parent_statement.py`.
- `routes/vercon/report_output.py`, `routes/vercon/report_service.py`.
- `static/js/reporting/pdf-preview.js`, `report-dialog.js`, `columns.js` (noi).
- `static/js/adechit/reports.js`, `app.js`, `static/js/vercon/pdf-preview.js`.
- `static/css/report-preview.css` (nou), `adechit.css`, `vercon.css`,
  `vercon-preview.css`, `static/adechit.html`, `static/help/adechit.json`.
- `.pushignore` actualizat: exclude și `instance/adechit-reports/`.

Resurse deja necesare, care trebuie să existe pe server:
`static/fonts/DejaVuSans.ttf`, fonturile NIR existente, `static/js/portal/pdfview.js`,
`static/css/pdfview.css`, `static/vendor/pdfjs/` și `static/css/confirm-box.css`.
Dependențele rămân cele din `requirements-reporting.txt`.

După copierea întregului set se repornește serviciul Python de către utilizator.
Nu publicați separat adaptoarele fără modulele comune. Configurările VERCON
existente din `instance/vercon-reports/` se păstrează. Snapshot-urile ADECHIT se
scriu de aplicație în `instance/adechit-reports/snapshots/`, sau sub directorul
`ADECHIT_REPORT_CONFIG_DIR` dacă este configurat; procesul serviciului are nevoie
de drept de scriere acolo. Acesta este un cache pe disc, fără tabele MariaDB.
`.gitignore` exclude cache-ul ADECHIT local.

Documentație actualizată: `ADECHIT/UTILIZARE_WEB.md`, `PYTHON/static/help/adechit.json`,
`PYTHON/reporting/README.md`, `ADECHIT_STATUS.md`, starea ADE0–ADE9 și
`implementare_vercon_v2.md`. Următoarea subfelie de rapoarte: **ADE9-06**.

## Probe rămase utilizatorului

1. Situație lunară pe grupă/toate grupele, Situație de plată cu text formatat,
   registru casă, bancă, fișă cont, documente anulate și dispoziție de plată.
2. PDF cu mai multe pagini, zoom, imprimare și alegerea imprimantei, descărcare;
   Excel integral și cu un subset de coloane, totaluri reunite, CNP ca text,
   sume numerice și lățimi configurate.
3. Schimbarea datelor după generare: exportul păstrează datele raportului
   afișat; un raport nou citește datele actualizate. Expirarea după 10 minute
   cere regenerare și nu trece la alte date în mod automat.
4. Accesul între utilizatori/DC/subunități și expirarea sesiunii; închiderea
   repetată a viewerului și a selectorului de coloane.
5. Chitanța cu două exemplare și PDF-ul din portalul părinților; regresie VERCON
   pe raport generic, NIR și raport cu chei Excel duplicate.

Aspectul și paginarea după trecerea la motorul comun nu sunt validate vizual.
Nu s-a executat publicare, restart, commit, push sau acces la MariaDB/server.
