# SLICE-AD11 — Portalul părinților: implementare și corecturi

Istoric AD11-01…05, păstrat în ordine. TESTAT, RULAT, PUBLICAT conform operatorului la 23.10.2026. Pentru rubricile de continuare/nevalidat din AD11-01, AD11-02 și AD11-05: *Rezolvat: operatorul a confirmat, la 23.10.2026, că nu mai este nimic nevalidat sau deschis.* Afirmațiile inițiale despre probe neexecutate rămân checkpointuri istorice; confirmarea ulterioară nu reprezintă probe noi în această consolidare.

AD11-05 restrânge verificarea la salvare la copiii activi și subunitatea curentă; identitatea portalului se verifică în continuare pe întregul DC, inclusiv copii plecați. ID-ul AD11-01 al anulării documentelor aparține istoricului distinct [ADE8](SLICE-ADE8-01-documente.md#slice-AD11-01-anulare).

- [AD11-01 — portal](#slice-AD11-01)
- [AD11-02 — font, totaluri și scroll](#slice-AD11-02)
- [AD11-03 — conflicte și query de date](#slice-AD11-03)
- [AD11-04 — lățimi mobile](#slice-AD11-04)
- [AD11-05 — copil+părinte și navigare](#slice-AD11-05)

## SLICE-AD11-01 — Portalul părinților <a id="slice-AD11-01"></a>

### Ce s-a schimbat și de ce

Implementarea planului aprobat: CNP+cod permanent+OTP email, părinți cu copii fără Plecat și CNP/email configurate, selector custom numai pentru mai mulți copii, dashboard lunar cu grafice și DataGrid comun readonly, fișă cont print/PDF după model, Interval implicit debifat cu două calendare custom. Istoricul continuu este pe IDP.

Registru unic CNP/email în DC, CodAccesPortal pe părinte, generare la migrare/adăugare, sincronizare între copii/subunități, buton trimitere numai cu email și validare conflict cu identificarea celuilalt părinte. Rutele parent folosesc sesiuni izolate, revalidare la fiecare cerere, OTP cu hash/expirare/încercări/unică utilizare și limitator comun.

Preview dedicat cu acreditări fictive și inbox loopback; aceeași logică de autentificare ca producția, cu delivery local și memory în loc de SMTP/Redis. Modificările WIP existente ale altor intervenții au fost păstrate; nu s-au citit/modificat sursele Access.

### Fișiere

- `sql/AD_08_portal_parinti.sql`.
- `PYTHON/routes/adechit/parent_identity.py`, `parent_portal.py`, `parent_statement.py`.
- Integrare: `main.py`, `adechit/__init__.py`, `repository.py`, `service.py`, `catalog_forms.py`, `importer.py`, `schema.json`.
- UI: `static/adechit-parents.html`, `js/adechit/parents.js`, `payers.js`, `statement-print.js`, `css/adechit-parents.css`, `adechit-statement.css`, `templates/adechit/statement.html`.
- Migrator: `ADECHIT/ADE.Migrator/AdeWriter.vb`, README.
- Local: `ADECHIT/tools/preview.py`; documentație `ADECHIT/PORTAL_PARINTI.md`, `UTILIZARE_WEB.md`, `PLAN_IMPLEMENTARE.md`, status.

### Verificări efective

- Python compileall: fără erori; JS node --check: fără erori.
- ADE.Migrator dotnet build --no-restore: 0 erori, 0 avertismente.
- ReportLab instalat în venv local, conform requirements-adechit.txt.
- Server local pornit pe localhost:5050, director SQLite separat.
- GET login parent, lista acreditărilor demo și inbox: HTTP 200 (fără probă vizuală sau autentificare executată).
- Preview-ul mai vechi a fost oprit strict după verificarea comenzii Python; a fost pornit launcherul actualizat.

### Nevalidat și continuare

Confirmarea operatorului din 23.10.2026 include login OTP, UI, grafice, print/PDF, salvările concurente și migrarea pe MariaDB, AD_08, SMTP/server. Publicarea după proba localhost: instrucțiunile și scenariile sunt în PORTAL_PARINTI.md.

## SLICE-AD11-02 — Font selector, fundal totaluri și scroll mobil <a id="slice-AD11-02"></a>

### Ce s-a schimbat și de ce

Observațiile utilizatorului: opțiunile copilului aveau font prea mic; subtotalurile și totalul final necesitau fundal gri cu contrast diferit; pe telefon pagina nu se derula. Regula globală din portal.css fixa html/body la 100dvh și overflow:hidden. Override-ul este limitat la parent-page și permite scroll document până la raport. Font 16px în dropdown, rânduri minimum 44px; gri #eeeeee subtotal / #d6d6d6 total în HTML și PDF, culori CSS în paleta comună. Print-color-adjust păstrează fundalul.

### Fișiere

adechit-parents.css, variables.css, adechit-statement.css, statement.html, parent_statement.py, PORTAL_PARINTI.md, UTILIZARE_WEB.md și statusurile ADE.

### Verificări

Citire reguli/CSS și identificarea overflow:hidden global. Compilare sintactică Python. Fără teste automate sau verificări vizuale, conform preferinței proiectului.

### Nevalidat

Proba mobil/dropdown/print/PDF aparține utilizatorului pe localhost. Niciun SQL nou. Restart local pentru generatorul PDF; autentificarea trebuie reluată după restart.

## SLICE-AD11-03 — conflicte istorice și query AvacontPush <a id="slice-AD11-03"></a>

10.10.2026. TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

La cererea utilizatorului, conflictele CNP/email existente nu opresc importul. Contactele sursă rămân intacte; identitățile ambigue au Email NULL în registrul portalului. Fiecare cerere de portal reverifică ambiguitatea în întregul DC, inclusiv sesiunile deja conectate și trimiterea codului permanent.

Plătitori afișează o avertizare separată de mesajele tranzitorii, cu lista înregistrărilor afectate din subunitatea curentă. Click selectează grupa, copilul și părintele și deschide editorul. La salvare lista se actualizează. Introducerea unor conflicte noi rămâne refuzată; un conflict existent și nemodificat nu împiedică salvarea celorlalte date ale părintelui. Corectarea emailului unui CNP sincronizează asocierile sale și reface registrul.

`sql/AD_08_portal_parinti.sql` conține numai DDL pentru AVACONT_SURSA, apoi AvacontPush sincronizează schema. `sql/AD_08_02_interogare_unica.sql` conține instrucțiuni simple pentru Interogări unice, nume AD_08_portal_parinti_date: tabel temporar de agregare, dezactivarea contactelor ambigue, coduri aleatorii numai unde lipsesc și există copil fără Plecat, propagare pe asocieri, lock 0. Fără DELIMITER/proceduri, fără trimitere email. Queryul păstrează codurile existente. Scrierile ADE se suspendă pe durata queryului. Niciun SQL remote nu a fost executat.

Migratorul VB și importerul Python aplică aceeași regulă de tolerare a conflictelor. Actualizate parent_identity/parent_portal/catalog_forms, API, payers.js, HTML/CSS, AdeWriter și ghidurile portalului, migratorului și utilizării web.

Verificări efective: py_compile pe modulele modificate, node --check payers.js, dotnet build ADE.Migrator --no-restore: 0 erori și 0 avertismente. Preview-ul local a fost repornit; GET /adechit/preview/parents răspunde HTTP 200. Fără teste automate funcționale sau verificări vizuale, conform preferinței utilizatorului. Rămân proba localhost, sincronizarea schemei, queryul, importul și loginul pe MariaDB.

Următoarea subfelie liberă a portalului: AD11-04.

## SLICE-AD11-04 — lățimi portal mobil <a id="slice-AD11-04"></a>

10.10.2026. TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

Observațiile din cele trei imagini: selectorul copilului mai îngust decât cardurile, carduri care lățesc pagina și provoacă scroll orizontal, buton Imprimă vizibil pe mobil.

adechit-parents.css: la maximum 800px, selectorul și containerul combobox ocupă 100% din lățimea disponibilă, înlocuind limita comună de 300px. Coloanele grid folosesc minmax(0,1fr), iar cardurile au min-width:0: SVG-ul cu lățime fixă nu mai forțează lățimea coloanei. Graficele ample se derulează în containerul propriu. Fieldset, labelurile calendarelor și antetul se pot comprima sau împărți pe rânduri. Imprimă este ascuns numai pe mobil; PDF-ul rămâne disponibil.

Actualizate UTILIZARE_WEB, PORTAL_PARINTI și statusurile. Fără backend sau SQL nou; nu este necesar restartul preview-ului. Utilizatorul reîncarcă pagina pentru proba mobilă. Următoarea subfelie liberă: AD11-05.

## SLICE-AD11-05 — copil + părinte într-un singur formular, linkuri în erori, email pe copii activi <a id="slice-AD11-05"></a>

11.10.2026. TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### Ce s-a schimbat și de ce

1. **Mesaj de eroare comun.** Tokeni `--err-bg/--err-ink/--err-line` în `variables.css` (cu temă întunecată) și regula `.msg[role="alert"]` în `adechit.css`: orice eroare din ADECHIT are același fundal. Alte pagini care au culorile lor proprii (portal, înregistrare) nu au fost atinse.
2. **Conflict de email cu linkuri.** `DomainError`/`require` primesc `details`; `guard` îl trimite în JSON. Mesajul „Adresa de email este deja utilizată de …” arată părintele, copilul și grupa ca linkuri (închid formularul și deschid înregistrarea); pentru altă subunitate a DC-ului, linkul comută subunitatea (`sessionStorage` `ade.pendingOpen`, reîncărcare, apoi `openForChild`). Subunitatea e afișată doar dacă DC-ul are mai mult de una accesibilă.
3. **Dublu clic.** În Prezență: rând → administrare plătitori cu grupa și copilul selectate (`openForChild`). În administrare plătitori: dublu clic pe un rând din oricare dintre cele trei liste deschide editorul.
4. **„+” din Prezență.** Un singur formular „Adaugă copil” cu secțiunea „Părinte (plătitor)”, un singur „Salvează”, nume părinte obligatoriu. Ruta nouă `POST /api/adechit/child-with-payer` (`save_child_with_payer`) salvează copilul și părintele în aceeași tranzacție.
5. **Verificarea emailului** (la salvare) se face doar pe copiii activi (nu Plecat) și doar în subunitatea curentă; la fel „acest CNP are deja alt email”. Același părinte (același CNP) la mai mulți copii nu a fost niciodată conflict.
6. **Mesaj informativ „similare”.** La părăsirea câmpurilor CNP și Email din formularul plătitorului, `GET /api/adechit/parents/similar` caută în tot DC-ul (subunitățile accesibile contului, inclusiv copii plecați) alți părinți cu același CNP / email și îi listează în același mesaj, cu aceleași linkuri; „(plecat)” și subunitatea (dacă sunt mai multe) apar în text.
7. **Copacul lunilor rămâne deschis** după salvarea unui copil (din «+» sau din administrare plătitori): `loadContext` (ramura de reîncărcare) păstra anii deschiși doar la încărcarea unui an, apoi `setData` îi uita; acum se reîncarcă toți anii deschiși și se redesenează cu `renderMonthTree`, deci luna selectată rămâne vizibilă.
8. **«Adaugă copil» din administrare plătitori** deschide același formular copil + părinte ca «+» din Prezență (fără grupă selectată: «Selectați grupa copilului.»).
9. **Portal K-BOT: meniul de aplicații pe logo.** Click pe logo, autentificat, deschide același meniu «Aplicații» ca «A» din ADECHIT (contul, K-BOT, ADECHIT, Vercon/Avacont «în curând»); nelogat, logo-ul rămâne link spre prezentare. CSS-ul meniului s-a mutat neschimbat din `adechit.css` în `css/app-menu.css` (încărcat de ADECHIT și de portal; `--avacont-blue` are valoare implicită în `:root`).
10. **Rapoartele pe ecran urmăresc tema** (întunecat/luminos) a paginii care le-a deschis: `reports.js` pune `data-theme` pe fereastra raportului, iar `adechit-report.css` are reguli doar `@media screen`. Listarea și PDF-ul rămân pe paleta Access.

### Fișiere atinse

`PYTHON/routes/adechit/{domain,__init__,parent_identity,catalog_forms}.py`, `PYTHON/static/js/adechit/{app,catalogs,payers}.js`, `PYTHON/static/js/portal/apps-menu.js` (nou), `PYTHON/static/css/{variables,adechit,adechit-report}.css`, `PYTHON/static/css/app-menu.css` (nou), `PYTHON/static/portal.html`, `PYTHON/static/adechit.html`, `ADECHIT/UTILIZARE_WEB.md`, statusuri.

### Teste

Fără teste rulate (preferința utilizatorului). `node --check` și `py_compile` curate. Eroarea `AD_PortalParents lipsă` din 000_DEMO a venit din AD_08 neaplicat; utilizatorul a rulat scripturile AD_08 / AD_08_02 și raportează „pare ok”.

### Nerezolvat / neverificat

- Linkurile din mesajul de conflict nu au fost văzute funcționând; presupunerea (părinte din altă subunitate) a fost tratată prin comutarea subunității, dar neprobată.
- Atomicitatea `child-with-payer` presupune anularea tranzacției la `DomainError` (citită în `repository.transaction`, neprobată).
- Identitatea portalului (`reconcile_contacts`, lista „Portal blocat”) rămâne pe tot DC-ul, inclusiv copii plecați și alte subunități: un email nou acceptat la salvare poate bloca portalul.
- Meniul de pe logo, mutarea CSS-ului meniului (aspectul meniurilor ADECHIT trebuie să rămână neschimbat) și copacul lunilor după salvare: doar sintaxă, neprobate.
