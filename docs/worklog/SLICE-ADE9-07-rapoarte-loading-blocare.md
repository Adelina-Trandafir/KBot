# SLICE-ADE9-07 — loading imediat și o singură pregătire de raport

19.10.2026. **SCRIS LOCAL.** Numai analiză de design din surse. Nu s-au rulat
teste, verificări automate/sintactice, generări PDF/Excel, browser, HTTP,
SQL, servicii sau operații pe server, conform instrucțiunilor proiectului.

## Cerere și rezultat

Până acum, multe comenzi făceau generarea și descărcarea înainte să deschidă
viewerul, fără feedback vizibil. Un clic repetat putea porni alte rapoarte.

Componenta comună ia acum o blocare înainte de prima operație asincronă și
deschide imediat fereastra raportului. Aceasta afișează o cartelă centrală,
indicator animat și mesajul „Se pregătește raportul…”. Generarea începe după
ce browserul poate desena fereastra. Nu se aplică o temporizare arbitrară de
debounce: blocarea acoperă întreaga operație reală, inclusiv regenerarea unui
snapshot expirat. Clicurile suplimentare nu sunt puse în coadă.

La PDF, încărcarea rămâne până când PDF.js a desenat prima pagină curentă,
apoi apare raportul cu acțiunile active. La descărcare PDF/Excel, progresul
se închide după pregătirea fișierului și predarea lui către browser.
Finalizarea salvării de către utilizator nu poate fi observată de aplicație.

Erorile de generare, rețea sau afișare apar în aceeași fereastră și sunt
jurnalizate. Dacă bytes PDF există, eroarea de afișare oferă descărcarea ca
rezervă. Blocarea este eliberată în `finally`, la rezultat/eroare. Dacă
fereastra este închisă în timpul unei cereri, blocarea rămâne până la
terminarea cererii; o nouă încercare între timp afișează un mesaj explicativ.
Rezultatele întârziate ale unui context invalidat nu sunt afișate.

Indicatorul respectă tema și preferința de mișcare redusă. Stările sunt
anunțate accesibil, iar focusul este mutat de la loading la acțiunile PDF
după afișarea documentului.

## Acoperire

- ADECHIT: rapoartele din panou, dispozițiile de plată, chitanțele,
  descărcările PDF și exportul Excel cu selecția coloanelor. Ajutorul PDF
  trece prin același helper de descărcare.
- VERCON: rapoartele din panou, documentele oficiale, alternativa locală
  pentru cititorii fără dreptul separat Raport, ciornele și exporturile Excel.
- Portalul părinților: fișa PDF, imprimarea și Excel cu selecția coloanelor
  folosesc aceeași componentă. Imprimarea previzualizează PDF în aplicație.
  Modul de răspuns `columns` al rutei existente furnizează opțiunile Excel
  din modelul serverului, sub aceeași autentificare și autorizare pe copil.
  Filtrele intervalului sunt capturate la pornire și păstrate la export.

Blocarea este comună fluxurilor din aceeași pagină de browser. Nu este un
lock pe server, între taburi sau între utilizatori. Selecția și calculele
business existente nu se schimbă; nu există DDL sau seed nou.

## Fișiere de publicat împreună

Din `PYTHON`, prin AvacontPush:

- `static/js/reporting/report-progress.js` (nou), `pdf-preview.js`,
  `report-dialog.js`;
- `static/js/portal/pdfview.js` (notificare opțională după desenarea paginii);
- `static/css/report-preview.css`;
- `static/js/adechit/reports.js`, `parents.js`;
- `static/js/vercon/pdf-preview.js`, `ui.js`, `reports.js`,
  `document-report.js`, `documents.js`;
- `static/adechit-parents.html` (resursele viewerului comun);
- `routes/adechit/parent_portal.py` (opțiuni Excel și CSP pentru viewer);
- `static/help/adechit.json`.

Resurse deja necesare: `static/js/reporting/columns.js`,
`static/js/utils/confirm-box.js`, `static/css/confirm-box.css`,
`static/css/pdfview.css`, `static/vendor/pdfjs/` și fonturile rapoartelor.
Publicați întregul set înainte de reîncărcarea paginilor. Utilizatorul
repornește serviciul Python pentru modificarea rutei portalului părinților.
Nu s-au executat publicare, restart, commit, push sau acces MariaDB/server.

Documentație actualizată: `ADECHIT/UTILIZARE_WEB.md`, `ADECHIT/PORTAL_PARINTI.md`,
`PYTHON/reporting/README.md`, `PYTHON/reports/vercon/README.md`, ajutorul JSON,
`ADECHIT_STATUS.md`, starea ADE0–ADE9 și `implementare_vercon_v2.md`.
Următoarea subfelie de rapoarte: **ADE9-08**.

## Probe rămase utilizatorului

1. Clic repetat rapid, inclusiv pe două rapoarte diferite: o singură generare
   și o singură fereastră de loading. După rezultat poate porni alt raport.
2. Rețea/generare lentă și prima încărcare PDF.js: loading vizibil imediat,
   până la prima pagină desenată; zoom, paginare și imprimare după rezultat.
3. Lipsă date, HTTP 403/500, PDF invalid, resursă lipsă: eroare în raport,
   fără blocare permanentă; descărcare de rezervă la eroare de afișare.
4. Închidere în pregătire, schimbare context, expirare sesiune și navigare:
   fără rezultate vechi deschise ulterior, fără porniri simultane.
5. PDF/Excel direct, selecție/renunțare la coloane, snapshot expirat și
   regresie VERCON pentru document oficial, ciornă și alternativa locală.
6. Portal părinți: Imprimă/PDF, Excel cu coloane selectate, interval,
   schimbarea copilului, sesiune expirată și autorizarea copilului.
7. Teme luminoasă/întunecată, navigare din tastatură, mișcare redusă și mobil.

Niciuna dintre aceste probe nu este declarată executată.
