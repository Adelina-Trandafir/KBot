# SLICE-ADE9 — Rapoarte, motor comun, încărcare și ajutor

01 definește rapoartele și abaterile Access, 04 adaugă Situația de plată cu text HTML, 05 trece la model/snapshot și motor comun, 06 aliniază butoanele, 07 adaugă loading și blocarea unei singure pregătiri per pagină, iar 09 reduce numai textele cerute cu 2pt. 10 păstrează distinct inventarul capturilor mascate și ajutorul extraselor din 21.10. [08](SLICE-ADE9-08-resetare-eroare-la-actiune.md) rămâne separat, fiind notificarea generală a paginii. Nota Rezolvat repetată în 01 și 04: *Rezolvat: operatorul a confirmat, la 23.10.2026, că nu mai este nimic nevalidat sau deschis.* Predarea/capturile din 10 nu primesc o confirmare de publicare inventată; probele și datele rămân în etapa lor.

- [ADE9-01](#slice-ADE9-01)
- [ADE9-04](#slice-ADE9-04)
- [ADE9-05](#slice-ADE9-05)
- [ADE9-06](#slice-ADE9-06)
- [ADE9-07](#slice-ADE9-07)
- [ADE9-09](#slice-ADE9-09)
- [ADE9-10](#slice-ADE9-10)

## SLICE-ADE9-01 — rapoartele ADECHIT <a id="slice-ADE9-01"></a>

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### Ce s-a schimbat și de ce

Panoul «Rapoarte» din dreapta era dezactivat. Acum fiecare buton cere serverului un raport (pagină de listare sau PDF). Serverul doar citește: nu scrie rânduri și nu consumă numere de chitanță. Cifrele vin din aceleași rânduri ca ecranul (luna deschisă = `calculate`, luna închisă = `AD_SS_Buget`), nu dintr-un al doilea calcul.

| Raport Access | În web | Panou |
|---|---|---|
| SituatieDebitori_buget / _total / _total_grp | `situation` (o grupă / toate grupele / grupe separate) | Situație lunară + «Toate grupele?» + «Separă grupe?» + «Generează PDF?» |
| RegistruCasa | `cash` (interval de date) | Registru casă |
| RaportBanca | `bank` (interval de date) | Raport bancă |
| DocumenteAnulate | `cancelled` (luna aleasă) | Documente anulate |
| FisaCont + FisaCont_s | `account` (copilul ales sau toți din grupă) | Fișă cont |
| FisaDebitor (+_L, _C) | `debtor` | Fișă debitor (buton nou) |
| rpt_SitFin | `financial` | Situație financiară (buton nou) |
| Dispozitie plata | `payment-order` (o restituire) | butonul 📥 de pe rândul restituirii |
| Chitanta | deja făcută (ADE8) | neschimbată |

### Abateri față de Access (de confirmat)


- **Registru/Bancă/Anulate**: sume cu 2 zecimale (Access arăta 0 zecimale; ar fi ascuns bani).
- **Documente anulate**: Access filtrează după luna prezenței, deși antetul arată un interval; antetul arată acum luna. Plățile fără chitanță și fără alt document apar ca «Bon fiscal». Totalul «TOTAL DOCUMENTE ANULATE» din Access folosea două casete inexistente în raport (`[si]-[SP]`) și nu a fost reprodus; rămâne rândul TOTAL.
- **Fișă cont**: rândurile «Transfer» vin din jurnalul `Platitori_Istoric` (MutaCopil nu se portează, M06). «Explicație» a documentelor alte plăți = textul din `AlteDoc.Explicatie` + număr (FelDoc nu mai există, AD_07). Ordinea rândurilor cu aceeași dată: sold inițial, prezență, plăți, restituiri, transferuri (Access nu definea o ordine).
- **Fișă debitor**: lipsesc calendarul pe zile (`Prezenta_sub`), bonurile fiscale și coloanele Hrană / %Frate / Avans (date neportate). Absența = zile lucrătoare ale lunii − zile prezență. Lunile sunt în ordine cronologică (Access sorta textul `LA`, deci greșit între ani). Plătitorii activi se listează toți pe pagina copilului.
- **Dispoziție de plată**: beneficiar = copilul (ca în Access), nu plătitorul. Seria/numărul actului de identitate rămân de completat de mână.
- **Situație financiară (rpt_SitFin)**: sursa Access (`Situatii salvate`) nu e exportată (M09). Coloanele sunt mapate pe nume: Valoare achitată = total chitanțe, Achitat anticipat = Anticipat, Plăți = alte plăți, Număr chitanță = chitanțele lunii. **Maparea e presupusă, nu verificată.**
- **Fonturi PDF**: DejaVu Sans normal (nu există variantă bold în proiect); evidențierea se face prin fundal.
- Mail la Fișă cont: bifa rămâne dezactivată (mailul nu trimite, ADE8-05).

### Neconstruite

- **SituatieDebitori** (varianta veche): exclus de utilizator, 10.10.2026.
- **rpt_Factura, Factura_mail**: necesită tabelul `Facturi` (emitere, numerotare, delegat) — modul exclus în inventar («fără facturi»); nu există în schema web.
- «Încarcă chitanțe» (buton): nu e raport; rămâne dezactivat.

### Fișiere

Noi: `PYTHON/routes/adechit/reports.py`, `report_render.py`, `static/js/adechit/reports.js`, `report-print.js`, `static/css/adechit-report.css`, `adechit-report-landscape.css`. Modificate: `routes/adechit/__init__.py` (ruta `GET /api/adechit/reports/<kind>/<print|pdf>`), `receipt_output.py` (`issuer_data` extras și reutilizat), `static/adechit.html` (panou), `static/js/adechit/app.js` (legare panou, butonul dispoziției), `tests/adechit_lazy.test.mjs` (doar importurile simulate noi).

### De probat de utilizator


Fiecare raport pe date reale, listare + PDF; interogările SQL nu au fost rulate pe MariaDB (nici pe SQLite); `reportlab` lipsește din `.venv` local (PDF-ul se probează doar pe server); comparație cifre cu Access pe aceeași lună.

## SLICE-ADE9-04 — Situație de plată și text HTML configurabil <a id="slice-ADE9-04"></a>

13.10.2026. TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### Comportament

Raport A4 portret după imagine: grupă, copii în ordine alfabetică, SID/SIC, taxa pentru zilele configurate ale lunii și suma SID + taxa − SIC. Utilizatorul a confirmat explicit soldurile inițiale și taxa lunară integrală. Datele provin din situația calculată/salvată existentă; taxa și zilele sunt cele ale lunii selectate. Sumele negative se păstrează. Nu se schimbă calculele contabile existente.

Butonul Situație de plată este sub Situație lunară și folosește Toate grupele. Toate grupele introduce mereu salt de pagină între grupe, independent de Separă grupe. HTML/PDF repetă antetul tabelului la continuare și nota sub fiecare grupă. Grupele numeroase sau notele lungi pot ocupa mai multe pagini.

Meniul Text situație de plată deschide editor vizual și sursă HTML. Configurare per subunitate, drept Catalog, versiune pentru conflicte, cheie de idempotentă păstrată la reîncercare și blocare a schimbării subunității cât timp dialogul este deschis. Notele acceptă liste, paragrafe, titluri, aldine/cursive/subliniere, aliniere, șapte culori, trei familii de font și șapte mărimi. Randarea trece printr-o listă explicită de formatare acceptată, fără atribute sau CSS arbitrar. Editorul primește numai HTML filtrat; lipirea este text simplu, sursa permite HTML. Variabile: [UNITATE], [CONT], [COD_FISCAL], [GRUPA], [LUNA], [AN]. Primul cont este implicit, dar utilizatorul poate scrie alt IBAN în text. Datele instituției din imagine nu sunt introduse ca valori reale în aplicație.

PDF folosește Helvetica/Times/Courier, cu DejaVu pentru caractere Unicode. Excel exportă nota ca text simplu și introduce salturi între grupe; lățimile se citesc la fiecare export prin reporting/excel_widths.py cu chei stabile.

### Fișiere de publicat

- Python: `routes/adechit/__init__.py`, `reports.py`, `report_render.py`, `report_excel.py`, modulele noi `payment_notice.py` și `report_richtext.py`.
- Pagina: `static/adechit.html`; JS: `static/js/adechit/app.js`, `reports.js`, `payment-notice-config.js`.
- CSS: `static/css/adechit.css`, `adechit-payment-notice.css`, `adechit-richtext.css`, `adechit-payment-notice-config.css`.
- Ajutor: `static/help/adechit.json`; ghidul UTILIZARE_WEB și statusurile au fost actualizate în aceeași intervenție.

### Instalare manuală

1. După schema de subunități AD_05, executați `sql/AD_10_raport_situatie_plata.sql` în fiecare bază ADECHIT. Schema de referință citită: MariaDB_Schema/000_DEMO.sql, AD_Subunits, AD_LunaD, AD_ValoriTaxe și AD_Operations.
2. Opțional, după AD_08_rapoarte_excel_coloane.sql, executați `sql/AD_10_raport_situatie_plata_excel.sql` pentru lățimile din AVACONT_COMUN.
3. Publicați fișierele prin AvacontPush și configurați textul în subunitatea dorită.

Tabela nouă este AD_REPORT_CONFIG (SUBUNIT_ID, REPORT_KEY, FOOTER_HTML, VERSION), cu FK către AD_Subunits. Absența unei înregistrări folosește modelul implicit. Tabela trebuie să existe înainte de folosirea raportului/configurării. Nu există DDL automat în aplicație. Revenire: restaurați fișierele anterioare; tabela nouă poate rămâne fără să afecteze versiunea veche. Nu sunt necesare ștergeri de date.

### Rămâne de probat de utilizator


Lună deschisă/închisă, SID/SIC și taxa integrală, totaluri, o grupă/toate grupele, separarea paginilor, nota cu liste/culori/font/mărime și variabile, conflictele de salvare și reîncercarea după o eroare HTTP, drepturile Catalog, izolarea subunităților, imprimarea/PDF/Excel și regresia rapoartelor existente.

## SLICE-ADE9-05 — sistem de rapoarte comun cu VERCON <a id="slice-ADE9-05"></a>

19.10.2026. TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026). Cerere: rapoartele ADECHIT să folosească sistemul VERCON.

### Comportamentul rezultat

- Rapoartele din panoul ADECHIT și dispozițiile de plată generează un model o singură dată. Previzualizarea PDF și exportul Excel folosesc acel model, păstrat 10 minute într-un snapshot legat de email, DC și subunitate.
- Previzualizarea PDF este aceeași componentă folosită de VERCON, în aplicație: tipărire, alegerea imprimantei, descărcare PDF, zoom, potrivire la lățime, 100%, navigare între pagini și închidere. Chitanțele folosesc același viewer; imprimarea lor se pornește din bara raportului.
- Excel păstrează selecția coloanelor ADECHIT, cheile existente, valorile numerice și citirea lățimilor configurate la fiecare export. Selectarea unui subset de coloane adaptează și celulele reunite ale subtotalurilor.
- Motorul generic PDF/Excel este extras din VERCON în infrastructura comună. Adaptorul ADECHIT traduce antetul, subtitlurile, culorile și formulele fără schimbarea calculelor de business. Nota HTML a Situației de plată rămâne un bloc formatat în PDF și text simplu în Excel.
- PDF-ul fișei din portalul părinților folosește motorul comun. Formularul dedicat chitanței rămâne specializat, similar formularului NIR din VERCON.
- VERCON folosește modulele extrase prin adaptoare compatibile, păstrând identitatea, cheile Excel duplicate, machetele și rendererul dedicat NIR.
- Previzualizările sunt eliberate la închidere, navigare, schimbarea contextului ADECHIT sau expirarea sesiunii. Rutele vechi de ieșire directă/HTML rămân disponibile integrărilor existente.

Antetul și meniul de aplicații VERCON sunt furnizate de adaptorul existent. Configurarea SQL/machetelor VERCON din KBotAdmin nu este modificată. Sursele datelor ADECHIT rămân constructorii existenți, cu situația salvată pentru lunile închise și calculul existent pentru luna deschisă. Nu există DDL sau seed nou.

### Fișiere de publicat împreună

Din `PYTHON`, prin AvacontPush:

- `reporting/output.py`, `errors.py`, `snapshots.py`, `richtext.py` (noi).
- `routes/adechit/__init__.py`, `report_routes.py` (nou), `report_model.py` (nou), `report_render.py`, `report_excel.py`, `report_richtext.py`, `parent_statement.py`.
- `routes/vercon/report_output.py`, `routes/vercon/report_service.py`.
- `static/js/reporting/pdf-preview.js`, `report-dialog.js`, `columns.js` (noi).
- `static/js/adechit/reports.js`, `app.js`, `static/js/vercon/pdf-preview.js`.
- `static/css/report-preview.css` (nou), `adechit.css`, `vercon.css`, `vercon-preview.css`, `static/adechit.html`, `static/help/adechit.json`.
- `.pushignore` actualizat: exclude și `instance/adechit-reports/`.

Resurse deja necesare, care trebuie să existe pe server: `static/fonts/DejaVuSans.ttf`, fonturile NIR existente, `static/js/portal/pdfview.js`, `static/css/pdfview.css`, `static/vendor/pdfjs/` și `static/css/confirm-box.css`. Dependențele rămân cele din `requirements-reporting.txt`.

După copierea întregului set se repornește serviciul Python de către utilizator. Nu publicați separat adaptoarele fără modulele comune. Configurările VERCON existente din `instance/vercon-reports/` se păstrează. Snapshot-urile ADECHIT se scriu de aplicație în `instance/adechit-reports/snapshots/`, sau sub directorul `ADECHIT_REPORT_CONFIG_DIR` dacă este configurat; procesul serviciului are nevoie de drept de scriere acolo. Acesta este un cache pe disc, fără tabele MariaDB. `.gitignore` exclude cache-ul ADECHIT local.

Documentație actualizată: `ADECHIT/UTILIZARE_WEB.md`, `PYTHON/static/help/adechit.json`, `PYTHON/reporting/README.md`, `ADECHIT_STATUS.md`, starea ADE0–ADE9 și `implementare_vercon_v2.md`. Următoarea subfelie de rapoarte: **ADE9-06**.

### Probe rămase utilizatorului

1. Situație lunară pe grupă/toate grupele, Situație de plată cu text formatat, registru casă, bancă, fișă cont, documente anulate și dispoziție de plată.
2. PDF cu mai multe pagini, zoom, imprimare și alegerea imprimantei, descărcare; Excel integral și cu un subset de coloane, totaluri reunite, CNP ca text, sume numerice și lățimi configurate.
3. Schimbarea datelor după generare: exportul păstrează datele raportului afișat; un raport nou citește datele actualizate. Expirarea după 10 minute cere regenerare și nu trece la alte date în mod automat.
4. Accesul între utilizatori/DC/subunități și expirarea sesiunii; închiderea repetată a viewerului și a selectorului de coloane.
5. Chitanța cu două exemplare și PDF-ul din portalul părinților; regresie VERCON pe raport generic, NIR și raport cu chei Excel duplicate.

Aspectul și paginarea după trecerea la motorul comun nu sunt validate vizual. Nu s-a executat publicare, restart, commit, push sau acces la MariaDB/server.

Reviziile comune din 19.10 privind Export Raport, integrarea totalului general, aspectul/omiterea lui și subtotalurile sunt păstrate o singură dată în [istoricul rapoartelor, etapele 8–10 și 17](SLICE-VC08-03-rapoarte-magazie-alimente.md#d001-11-rapoarte); acestea consemnau Confirmat 23.10. Limitele celorlalte probe din acest jurnal rămân distincte.

### Antetul și meniul Export Raport — 19.10.2026 <a id="d001-12-antet-export"></a>

Două revizii preluate prin D001-12, ambele **Confirmat 23.10**. Comenzile PDF ADECHIT/VERCON sunt în același antet cu informațiile, la dreapta, cu trecere pe rândul următor pe ecrane înguste; bara separată de deasupra PDF dispare. Meniul Export Raport se aliniază la stânga butonului și folosește rândurile drepte ale meniului comun, fără colțuri/borduri moștenite din dialog. PDF = 📕, Excel = 📗.

Fișiere istorice (numele fără prefix continuă directorul precedent):

| Etapă generală | Fișiere |
|---|---|
| 13 | PYTHON/static/js/reporting/report-dialog.js, pdf-preview.js; PYTHON/static/css/report-preview.css; implementare_vercon_v2.md. |
| 14 | PYTHON/static/js/reporting/pdf-preview.js; PYTHON/static/css/report-preview.css; implementare_vercon_v2.md. |

## SLICE-ADE9-06 — butoane ADECHIT în stilul VERCON <a id="slice-ADE9-06"></a>

19.10.2026. TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### Schimbări

Butoanele de comandă din panoul Rapoarte renunță la fontul monospațiat gros și fundalul în degrade. Folosesc fontul interfeței, greutate normală, fundal transparent, bordură simplă și colțuri de 5px, ca acțiunile VERCON. Lățimea panoului și înălțimea minimă a comenzilor rămân cele existente.

Butoanele de acțiune `.btn` din ADECHIT folosesc aceeași bordură, rotunjire și greutate a fontului. Comenzile folosesc feedback-ul comun extras din VERCON: contur albastru, umbră și ridicare de 1px la hover/focus; apăsarea revine la poziția normală cu umbră interioară. Butoanele dezactivate nu primesc aceste efecte. Preferința de mișcare redusă elimină deplasarea și tranzițiile.

Feedback-ul este într-un singur stylesheet pentru VERCON și ADECHIT. Importul istoric VERCON rămâne un adaptor CSS. Meniurile și acțiunile integrate în controale păstrează stilurile lor; în ADECHIT sunt incluse aici și comenzile din celulele încasărilor, taburile, arborele de ani și editorul de text. Modul activ al asocierii bancare își păstrează fundalul de selecție.

### Fișiere și publicare

Prin AvacontPush, publicați împreună din `PYTHON/static/css/`:

- `application-button-effects.css` (nou, comun);
- `vercon-button-effects.css` (import compatibil către cel comun);
- `adechit-buttons.css` (nou, adaptări ADECHIT);
- `adechit-reports.css`;
- `adechit.css`.

Ajutorul actualizat: `PYTHON/static/help/adechit.json`. Documentație și stare: `ADECHIT/UTILIZARE_WEB.md`, `ADECHIT_STATUS.md`, starea ADE0–ADE9 și `implementare_vercon_v2.md`. Următoarea subfelie de rapoarte: **ADE9-07**.

Nu există modificări Python/JS, DDL sau seed. Nu s-au executat publicare, restart, commit sau push. Utilizatorul probează aspectul în temele deschisă și închisă, hover/focus/apăsare/dezactivare, panoul Rapoarte extins și restrâns, formularele și comenzile încorporate. Aspectul nu este declarat validat.

## SLICE-ADE9-07 — loading imediat și o singură pregătire de raport <a id="slice-ADE9-07"></a>

19.10.2026. TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### Cerere și rezultat

Până acum, multe comenzi făceau generarea și descărcarea înainte să deschidă viewerul, fără feedback vizibil. Un clic repetat putea porni alte rapoarte.

Componenta comună ia acum o blocare înainte de prima operație asincronă și deschide imediat fereastra raportului. Aceasta afișează o cartelă centrală, indicator animat și mesajul „Se pregătește raportul…”. Generarea începe după ce browserul poate desena fereastra. Nu se aplică o temporizare arbitrară de debounce: blocarea acoperă întreaga operație reală, inclusiv regenerarea unui snapshot expirat. Clicurile suplimentare nu sunt puse în coadă.

La PDF, încărcarea rămâne până când PDF.js a desenat prima pagină curentă, apoi apare raportul cu acțiunile active. La descărcare PDF/Excel, progresul se închide după pregătirea fișierului și predarea lui către browser. Finalizarea salvării de către utilizator nu poate fi observată de aplicație.

Erorile de generare, rețea sau afișare apar în aceeași fereastră și sunt jurnalizate. Dacă bytes PDF există, eroarea de afișare oferă descărcarea ca rezervă. Blocarea este eliberată în `finally`, la rezultat/eroare. Dacă fereastra este închisă în timpul unei cereri, blocarea rămâne până la terminarea cererii; o nouă încercare între timp afișează un mesaj explicativ. Rezultatele întârziate ale unui context invalidat nu sunt afișate.

Indicatorul respectă tema și preferința de mișcare redusă. Stările sunt anunțate accesibil, iar focusul este mutat de la loading la acțiunile PDF după afișarea documentului.

### Acoperire

- ADECHIT: rapoartele din panou, dispozițiile de plată, chitanțele, descărcările PDF și exportul Excel cu selecția coloanelor. Ajutorul PDF trece prin același helper de descărcare.
- VERCON: rapoartele din panou, documentele oficiale, alternativa locală pentru cititorii fără dreptul separat Raport, ciornele și exporturile Excel.
- Portalul părinților: fișa PDF, imprimarea și Excel cu selecția coloanelor folosesc aceeași componentă. Imprimarea previzualizează PDF în aplicație. Modul de răspuns `columns` al rutei existente furnizează opțiunile Excel din modelul serverului, sub aceeași autentificare și autorizare pe copil. Filtrele intervalului sunt capturate la pornire și păstrate la export.

Blocarea este comună fluxurilor din aceeași pagină de browser. Nu este un lock pe server, între taburi sau între utilizatori. Selecția și calculele business existente nu se schimbă; nu există DDL sau seed nou.

### Fișiere de publicat împreună

Din `PYTHON`, prin AvacontPush:

- `static/js/reporting/report-progress.js` (nou), `pdf-preview.js`, `report-dialog.js`;
- `static/js/portal/pdfview.js` (notificare opțională după desenarea paginii);
- `static/css/report-preview.css`;
- `static/js/adechit/reports.js`, `parents.js`;
- `static/js/vercon/pdf-preview.js`, `ui.js`, `reports.js`, `document-report.js`, `documents.js`;
- `static/adechit-parents.html` (resursele viewerului comun);
- `routes/adechit/parent_portal.py` (opțiuni Excel și CSP pentru viewer);
- `static/help/adechit.json`.

Resurse deja necesare: `static/js/reporting/columns.js`, `static/js/utils/confirm-box.js`, `static/css/confirm-box.css`, `static/css/pdfview.css`, `static/vendor/pdfjs/` și fonturile rapoartelor. Publicați întregul set înainte de reîncărcarea paginilor. Utilizatorul repornește serviciul Python pentru modificarea rutei portalului părinților. Nu s-au executat publicare, restart, commit, push sau acces MariaDB/server.

Documentație actualizată: `ADECHIT/UTILIZARE_WEB.md`, `ADECHIT/PORTAL_PARINTI.md`, `PYTHON/reporting/README.md`, `PYTHON/reports/vercon/README.md`, ajutorul JSON, `ADECHIT_STATUS.md`, starea ADE0–ADE9 și `implementare_vercon_v2.md`. Următoarea subfelie de rapoarte: **ADE9-08**.

### Probe rămase utilizatorului

1. Clic repetat rapid, inclusiv pe două rapoarte diferite: o singură generare și o singură fereastră de loading. După rezultat poate porni alt raport.
2. Rețea/generare lentă și prima încărcare PDF.js: loading vizibil imediat, până la prima pagină desenată; zoom, paginare și imprimare după rezultat.
3. Lipsă date, HTTP 403/500, PDF invalid, resursă lipsă: eroare în raport, fără blocare permanentă; descărcare de rezervă la eroare de afișare.
4. Închidere în pregătire, schimbare context, expirare sesiune și navigare: fără rezultate vechi deschise ulterior, fără porniri simultane.
5. PDF/Excel direct, selecție/renunțare la coloane, snapshot expirat și regresie VERCON pentru document oficial, ciornă și alternativa locală.
6. Portal părinți: Imprimă/PDF, Excel cu coloane selectate, interval, schimbarea copilului, sesiune expirată și autorizarea copilului.
7. Teme luminoasă/întunecată, navigare din tastatură, mișcare redusă și mobil.

Niciuna dintre aceste probe nu este declarată executată.

## SLICE-ADE9-09 — textul din Rapoarte redus cu 2pt <a id="slice-ADE9-09"></a>

19.10.2026. TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

În panoul ADECHIT Rapoarte, titlul, etichetele bifelor, etichetele datelor și valorile din câmpurile calendarului folosesc dimensiunea anterioară minus 2pt. Reducerea este exprimată în puncte, nu în pixeli. Textul îngroșat „Trimite Mail?” moștenește aceeași reducere ca eticheta sa.

Textul comenzilor de raport, butonul de restrângere și butonul calendarului își păstrează dimensiunile. Pentru calendar, dimensiunea butonului este exprimată explicit față de tokenul original, evitând micșorarea prin moștenirea noii dimensiuni a etichetei. Căsuțele de bifare și geometria butoanelor nu sunt modificate.

De publicat prin AvacontPush:

- `PYTHON/static/css/adechit-reports.css`;
- `PYTHON/static/help/adechit.json`.

Documentație: `ADECHIT/UTILIZARE_WEB.md`, statusul principal și starea ADE0–ADE9. Aspectul în teme luminoasă/întunecată, panoul extins/restrâns și butoanele calendarului: TESTAT, RULAT, PUBLICAT. Următoarea subfelie de rapoarte: **ADE9-10**.

## ADECHIT — help cu capturi și încasări din extrase <a id="slice-ADE9-10"></a>

Data: 21 octombrie 2026

Am extins helpul existent din `PYTHON/static/help/adechit.json` pe structura lui actuală. Acoperă ecranul principal, tabelul de prezență, grupurile și plătitorii, taxele, zilele lucrătoare, chitanțele, alte plăți, restituirile, rapoartele, exportul, drepturile, DISCUȚII și durata sesiunii. Am descris și cele șase comenzi din Meniu de sus.

Secțiunea nouă despre extrase se aplică numai unităților care încasează contribuțiile prin bancă. Explică selectarea încasărilor neasociate, asocierea manuală, previzualizarea automată, potrivirea după cod fiscal și cont, verificarea și generarea documentelor în Alte plăți, cazurile ambigue, accesul necesar și tratarea dublelor asocieri.

Cele 23 de capturi publicabile din `PYTHON/static/help/images/adechit/` provin din interfața DEMO. Captura tabelului principal are ambele panouri laterale restrânse. Valorile personale, financiare, de prezență și conținutul discuțiilor sunt mascate cu blur puternic. Inventarul public se află în `manifest.json`. Originalele nemascate au fost șterse din spațiul temporar după prelucrare. Captura pregătită inițial pentru filtrare a fost exclusă deoarece nu arăta meniul de filtre; instrucțiunile pentru filtrare rămân în help.

Am adăugat în `AGENTS.md` regula de blur puternic pentru viitoarele capturi de help. Capturile VERCON rămân în forma deja acceptată.

Nu au fost modificate datele din ADECHIT sau structura bazei de date. Nu au fost rulate teste, conform instrucțiunilor proiectului. Publicarea fișierului de help și a imaginilor rămâne pasul de deployment.
