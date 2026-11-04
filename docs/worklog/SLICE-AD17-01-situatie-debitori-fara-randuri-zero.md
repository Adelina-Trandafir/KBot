# SLICE-AD17-01 — Situație debitori: opțiune fără rânduri zero

04.11.2026. Cerință: opțiune de ascundere a rândurilor cu zero pe linie în situația debitorilor din ADECHIT și K-BOT.ADECHIT.

## Implementare

Bifa «Ascunde rândurile cu zero» este bifată implicit în panoul Rapoarte din ADECHIT web și în pagina nativă K-BOT → ADECHIT → Rapoarte. Se transmite `hide_zero=1` exclusiv la generarea raportului `situation` (Situație lunară).

Generatorul comun filtrează după citirea calculului lunii deschise sau a situației salvate pentru luna închisă, înainte de ordonare, grupare și numerotare. Sunt eliminate numai rândurile cu zero în toate coloanele numerice din `SITUATION_COLUMNS`: ZilePrezenta, SID, SIC, ValoareContract, Plata, Plati, Compensare, Anticipat, Retur, SFD și SFC. Valorile lipsă sunt tratate ca zero. Se compară valorile numerice individual, fără însumare între coloane și fără rotunjire de afișare; valorile negative ori mișcările care se compensează rămân vizibile. Numele, CNP-ul și detaliile nu participă la criteriul numeric.

Filtrul funcționează pe o grupă, toate grupele sau grupe separate. Grupele fără rânduri rămase nu generează pagini. Dacă toate rândurile sunt eliminate, se întoarce mesajul «Nu există rânduri cu valori diferite de zero pentru selecția aleasă.». Totalurile valorice nu se schimbă prin eliminarea rândurilor zero. Ordinea existentă și poziția totalurilor sunt păstrate. PDF-ul și Excelul provin din același model filtrat; schimbarea bifei cere regenerarea raportului.

## Fișiere proprii ale taskului

Server și interfață web:

- `PYTHON/routes/adechit/reports.py`
- `PYTHON/static/js/adechit/reports.js`
- `PYTHON/static/adechit.html`
- `PYTHON/static/help/adechit.json`

Aplicație K-BOT și ajutor:

- `src/KBot.App/Views/Adechit/AdechitRapoarteForm.vb`
- `src/KBot.App/Views/Adechit/AdechitRapoarteForm.Designer.vb`
- `src/KBot.App/HelpContent/contabil/adechit.md`

Documentație:

- `ADECHIT/UTILIZARE_WEB.md`
- `docs/worklog/SLICE-AD17-01-situatie-debitori-fara-randuri-zero.md`

Motiv pentru înregistrarea tuturor fișierelor de mai sus: «Opțiune bifată implicit pentru ascunderea rândurilor cu toate valorile numerice zero în situația debitorilor, comună ADECHIT web și K-BOT, cu PDF/Excel și ajutor actualizat.»

## Stare și limite

SCRIS LOCAL. S-a făcut numai analiza codului și a machetei. Conform AGENTS.md nu s-au rulat teste, compilări, generări PDF/Excel, verificări automate sau vizuale. Nu s-au executat SQL, Git, publicare ori restart. Implementarea necesită publicarea serverului/interfeței web și distribuirea unei versiuni K-BOT compilate ulterior.

Pentru proba Adelinei: bifa activată/dezactivată, rând complet zero, sold final zero cu alte valori nenule, valori negative, o grupă/toate/separat, luni deschise/închise, selecție rămasă fără rânduri și concordanța PDF/Excel.

Adelina a confirmat explicit înregistrarea tuturor fișierelor taskului, fără o probă funcțională, și a cerut pregătirea pentru publicare normală, nu doar debugging: «NU am exact ce sa verific, dar confirm ca pot fi inregistrate. sa nu le trimit doar in debug». Confirmarea autorizează executarea `tools/Register-DeploymentTask.ps1` pentru cele nouă fișiere enumerate, inclusiv acest worklog, în SLICE-AD17-01. Starea netestată și nepublicată se păstrează; înregistrarea nu reprezintă o probă sau o publicare. Rezultatul scriptului este raportat în chat și, la succes, în registrul imuabil `docs/deployment/pending`.
