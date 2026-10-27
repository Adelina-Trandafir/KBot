# SLICE-VC01-01 — pachetul VERCON si publicarea

Stare: **IMPLEMENTAT_LOCAL**. Nu a fost rulat, testat sau importat nimic (regula: fara teste decat la cerere); nimic validat pe server.

## Facut
- `PYTHON/routes/vercon.py` (fisier gol, 0 octeti) sters; in locul lui pachetul `PYTHON/routes/vercon/`.
- `routes/vercon/__init__.py`: `create_blueprint(authenticate=None)`, `vercon_bp`, `SECTION = "VR"`; un singur endpoint `GET /api/vercon/status` (sesiune portal obligatorie, `Cache-Control: no-store`). Fara conexiune la baza la import.
- (Revizuit la VC01-02/03: `/api/vercon/status` inlocuit de `/api/vercon/context`, cu garda de unitate si drept.)
- `main.py`: import si `register_blueprint(vercon_bp)`.
- `routes/vercon/numeric.py` (motor numeric pur, fara baza): `ZEC`/`SCARA`, `to_int` (refuz la pierdere/overflow, fara rotunjire), `display` (in sus la jumatate), `vat_to_int` (x10000), `calc_round` (HALF_EVEN, propunere), `header_total` (N01/N02, unicul calcul la migrare). Implementeaza CONTRACT_CALCULE §1–§2.3.

## Sectiunea VR
Rolurile sunt date in `AVACONT_COMUN` (`Roluri.Sectiune`), citite de `routes/portal/drepturi.py`; nu e nevoie de cod nou pentru sectiune. Rolurile VR si operatiile se definesc la VC04-01.

## Nefacut / limite
- Manifestul AvacontPush/LocalRoot nu a fost inspectat (nu este in repo; de verificat la publicare).
- Fara test: motorul numeric nu are inca etaloane executate (E01–E17 din contract).
- Nu exista pagina web, DDL sau migrator.

Starea inițială comună etapelor VC01-02/03: **IMPLEMENTAT_LOCAL**. Nimic rulat, testat sau previzualizat (regula: fara teste/randari decat la cerere); nimic validat pe server. Probele și extensiile ulterioare din sesiunea web sunt delimitate în secțiunea lor.

## SLICE-VC01-02 — pagina si ciclul de viata <a id="slice-vc01-02"></a>

### Facut
- `PYTHON/static/vercon.html`, `css/vercon.css`, `js/vercon/{gate,api,app}.js`; ruta `GET /vercon` (anteturi de securitate, no-store).
- Stari: se incarca / gol / eroare (cu cod diagnostic) / sesiune expirata (iesire la portal).
- Reutilizat: meniul de aplicatii, cronometrul de sesiune, mesajele portalului. Intrarea VERCON activata in meniurile existente si in selectorul `portal.js`.
- Curatare: un singur `AbortController` legat de `pagehide`; nu exista timere proprii.

### Limite
Fara pagina de lucru (VC02). Randarea nu a fost privita. Matricea sistem→curatare: `VERCON/INTEGRARE_SISTEME.md`.

## SLICE-VC01-03 — sesiune, log, conexiuni <a id="slice-vc01-03"></a>

### Facut
- `routes/vercon/diagnostics.py`: id de cerere (`X-Request-Id`, ecou in raspuns), email mascat, linii `VR op= outcome= unit= user= rid=`.
- `routes/vercon/connection.py`: o singura cale de conexiune (`utils.database`), inchidere garantata, fara pool/sesiune/auth paralele, fara reincercare la scriere.
- Garda: unitate (`X-Vercon-Unit`) + drept `read` per ruta; refuzurile si erorile se logheaza fara parole sau date personale complete.

### Limite
Operatiile VR din roluri se definesc la VC04-01; pana atunci nimeni nu are acces in productie (inchis, nu deschis).

### Sesiune web și probă locală — 17.10.2026 <a id="d001-12-sesiune"></a>

Preluare D001-12; cele două etape au probe diferite de starea inițială de mai sus.

- Sunt eliminate apelurile native alert/confirm/prompt și beforeunload din sursele web proprii (ieșire VERCON, ADECHIT plătitori/nomenclatoare, înregistrare, eroare PDF ADECHIT). Navigarea internă păstrează confirmările custom; ADECHIT verifică și ciorna la schimbarea aplicației. Închiderea/refreshul filei nu mai întreabă prin Chrome.
- messageBox comun are un buton, text actualizabil, închidere programatică și suport pentru raport. Timerul VERCON/ADECHIT avertizează custom la 15 minute, înlocuiește mesajul cu cel serios la 5 și colorează headerul portocaliu; o singură dată pe prag/pagină, fără prelungire. Timer vizibil numai în ultimele 15 minute; în VERCON între context și cont/ajutor.
- Expirare: oprește timerele, blochează interacțiunea, închide dialogurile și ferestrele scriptate urmărite (raport/chitanță/ajutor), navighează imediat la autentificare. Logout keepalive, fără a aștepta rețeaua; VERCON anulează cererile și eliberează ocuparea. Portalul nu mai așteaptă notificarea de 3 secunde. Nu se parcurg confirmări de abandon, nici cu ciornă.
- Proba izolată tools/ComboTreePreview, /session-demo: interval pornit la click de 3 minute, praguri 120/60 secunde numai în demo, apoi /session-ended; producția păstrează 15/5 minute. Textul, formularul și fereastra de probă pot rămâne deschise.
- **Probă efectuată la cerere:** sintaxă Node pentru 13 JS, apoi încă 3 ADECHIT fără erori; căutarea nu mai găsea apeluri native/beforeunload declanșatoare. Cele 7 rute/asseturi principale HTTP 200. Parcurs real în in-app browser de 3 minute, cu text nesalvat/formular: ambele avertismente, etapa critică 01:00 cu rgb(198,91,0), fără erori de consolă; la zero navigare cu formular/avertizare deschise, fără dialog nativ raportat de getJsDialog. Readus la Pregătit/03:00 pentru operator. Server 5052 pornit numai localhost, la cerere. Fără sesiune reală/DB/SQL/scheme; logoutul de producție nu a fost probat.
- **Etapa ulterioară configurabilă, implementată fără probe de execuție:** PORTAL_SESSION_MINUTES din config.py se citește la pornire, implicit 30, întreg strict pozitiv sau eroare explicită. Se configurează manual pe server; exemplul local creat inițial a fost eliminat la cerere, configurația privată nu a fost accesată/modificată. Necesită publicarea modulelor, setarea și restart Python, neefectuate; fără DB/SQL/schemă.
- POST /api/portal/session/renew reînnoiește numai sesiunea valabilă, păstrând tokenul/identitatea/unitatea; renewed_at este distinct de issued_at. Timerul comun și monitorul cer reînnoirea la inițializare: încărcarea/refreshul pornesc o perioadă nouă. Pollingul, activitatea și revenirea în tab nu o reînnoiesc; expirata cere autentificare. Fereastra de reutilizare a codului după parolă rămâne separată și nu se prelungește la refresh.
- Înainte de logout contoarele confirmă expirarea cu serverul, respectând reînnoirea altui tab cu același token; avertismentele locale sunt retrase/reprogramate după prelungire.

Fișiere istorice (numele fără prefix continuă directorul precedent):

| Etapă generală | Fișiere |
|---|---|
| 6 | PYTHON/static/js/utils/confirm-box.js, session-exit.js (nou); PYTHON/static/js/adechit/session.js, app.js, catalogs.js, payers.js, reports.js, help.js; PYTHON/static/js/vercon/app.js, api.js; PYTHON/static/js/session/session-monitoring.js; PYTHON/static/js/inregistrare/wizard.js; PYTHON/static/vercon.html; PYTHON/static/css/confirm-box.css, vercon-base.css, adechit-layout.css; tools/ComboTreePreview/preview_server.py, session-demo.html, session-demo.js, session-demo.css, session-ended.html, README.md. |
| 7 | PYTHON/routes/portal/session_settings.py, portal.py; PYTHON/static/js/adechit/session.js; PYTHON/static/js/session/session-monitoring.js, session-monitoring-extension.js, session-monitoring-timers.js. |
