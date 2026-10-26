# SLICE-ADE1-02…06 — Pagina ADECHIT și preview local

Etape din 08–09.10.2026, păstrate cronologic, după [checkpointul parțial ADE1–ADE8](SLICE-ADE1-01-implementare-partiala.md). TESTAT, RULAT, PUBLICAT conform operatorului la 23.10.2026. Pentru rubricile inițiale de amânare/nevalidat din ADE1-04: *Rezolvat: operatorul a confirmat, la 23.10.2026, că nu mai este nimic nevalidat sau deschis.* Separarea preview/runtime și limitele fiecărei probe rămân explicite. Extensiile ulterioare DataGrid sunt în [istoricul ADE6](SLICE-ADE6-01-cataloage.md).

- [ADE1-02](#slice-ADE1-02)
- [ADE1-03](#slice-ADE1-03)
- [ADE1-04](#slice-ADE1-04)
- [ADE1-05](#slice-ADE1-05)
- [ADE1-06](#slice-ADE1-06)

## SLICE-ADE1-02 — pagina aplicației <a id="slice-ADE1-02"></a>

08.10.2026 — TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026). Creat `static/js/adechit/app.js`: context/unitate, selectoare, DataGrid, mesaje, temă, operații și curățare `pagehide`. AvacontPush include recursiv extensiile PYTHON folosite. Serverul și autentificarea reală nu au fost probate.

## SLICE-ADE1-03 — preview local <a id="slice-ADE1-03"></a>

08.10.2026 — TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026). `.venv` a fost retargetat la Python 3.12.14 inclus local. Preview-ul SQLite a rulat strict pe `127.0.0.1:5050` și pagina a fost deschisă în browser. Aceasta nu validează MariaDB/Linux.

## SLICE-ADE1-04 — ecranul Prezență după Access <a id="slice-ADE1-04"></a>

### Ce s-a schimbat și de ce

Pagina generică pe cataloage a fost înlocuită cu un singur workspace operațional după ecranul Access furnizat: arbore lună/an și arbore de grupe în stânga, situația lunară în centru, comenzi sub grilă și tab-control financiar în footer. Sunt refolosite TreeView, DataGrid, tema și mesajele comune.

### Fișiere atinse

- `PYTHON/static/adechit.html`
- `PYTHON/static/css/adechit.css`
- `PYTHON/static/js/adechit/app.js`
- `ADECHIT/tools/preview.py` — port configurabil pentru probe locale.

### Verificări

- `node --check PYTHON/static/js/adechit/app.js` — trecut.
- verificare vizuală în browser la 943×871: arbori, grilă, butoane și taburi afișate coerent.
- `pytest PYTHON/tests/test_adechit.py -q` cu basetemp local — 9 teste trecute.

### Neverificat sau amânat

Serverul real, autentificarea și MariaDB nu au fost testate. Transferul copilului rămâne dezactivat până la decizia M06; rapoartele din dreapta ecranului Access rămân în ADE9.

## SLICE-ADE1-05 — inițializare blueprint <a id="slice-ADE1-05"></a>

09.10.2026. TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### Schimbare

Tracebackul furnizat de utilizator arată NameError la primul decorator `@bp.get`. În create_blueprint lipsea instanțierea variabilei locale bp. Am adăugat `bp = Blueprint('adechit', __name__)` înaintea înregistrării rutelor. Fiecare apel al fabricii creează propria instanță, inclusiv pentru preview cu adaptoare locale.

### Fișiere

PYTHON/routes/adechit/__init__.py, ADECHIT_STATUS.md și statusul detaliat ADE0–ADE9.

### Verificări și predare

Cauza identificată prin citirea sursei și tracebackul utilizatorului. Nu am pornit aplicația și nu am rulat teste, conform instrucțiunii explicite a utilizatorului. Se reia aceeași comandă preview.py. Fără modificări de date/SQL, fără commit sau publicare. Fișierul runtime se publică prin AvacontPush de către utilizator. Următoarea subfelie liberă: ADE1-06.

## SLICE-ADE1-06 — pornirea preview-ului <a id="slice-ADE1-06"></a>

09.10.2026. TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

### Cauză și remediere

Utilizatorul a raportat 404 la /portal?next=%2Fadechit și a autorizat explicit testarea până la pornirea serverului și deschiderea aplicației, fără verificări vizuale. gate.js introdus pentru autentificarea portalului trimitea pagina locală fără token spre /portal; aplicația preview izolată nu avea această rută.

Răspunsul /adechit generat cu repository_factory local poartă data-ade-preview="true". Gate-ul recunoaște acest marker servit de backend și nu redirecționează preview-ul. Pagina runtime obișnuită nu are markerul și păstrează verificarea sesiunii portalului. Drepturile API și autentificarea runtime nu sunt schimbate. Preview-ul rămâne accesibil exclusiv localhost. În launcher, / și /portal redirecționează la /adechit pentru a recupera inclusiv adresa 404 deja deschisă în browser. Nu introducem un portal local fictiv.

### Fișiere

- PYTHON/routes/adechit/__init__.py: marker în răspunsul HTML local.
- PYTHON/static/js/adechit/gate.js: recunoașterea markerului.
- ADECHIT/tools/preview.py: rutele de intrare locală.
- Statusul principal/detaliat și acest worklog.

### Verificări executate

- Identificate după comanda exactă cele două procese Python ale preview-ului pe 5050; repornit exclusiv preview-ul cu codul curent. Nu au fost oprite alte aplicații.
- Launcherul cu .venv pornește pe 127.0.0.1:5050 fără traceback; instanțierea blueprintului corectată în ADE1-05 este inclusă în această probă.
- Edge headless, contexte noi fără token: /adechit, /, /portal?next=%2Fadechit și /adechit?unit=preview2 ajung în aplicație. Contextul și situația întorc 200, datele inițiale se încarcă; nicio eroare JavaScript, cerere eșuată sau HTTP >=400.
- Flask test client: pagina runtime păstrează gate.js și nu primește markerul preview; pagina preview îl primește. API-ul preview refuză adresa nonlocală cu 403.
- Context, catalog-data, situation/1 și receipt-defaults?IDL=1: HTTP 200.
- Probe reproductibile: artifacts/ade1-06-startup.cjs și ade1-06-http.py.

Nu s-au făcut capturi, evaluări de layout, probe ale editării sau salvări de business. Nu s-a rulat întreaga suită; autorizarea acestei intervenții privește numai pornirea.

### Predare

Serverul preview rămâne pornit local. Se deschide http://localhost:5050/adechit. Fără commit, push, SQL sau modificări ale serverului live. Prin AvacontPush se predau fișierele runtime din PYTHON; launcherul și probele sunt exclusiv locale. Următoarea subfelie liberă: ADE1-07.
