# SLICE-ADE1-06 — pornirea preview-ului

09.10.2026. TESTAT LOCAL NONVIZUAL.

## Cauză și remediere

Utilizatorul a raportat 404 la /portal?next=%2Fadechit și a autorizat explicit testarea
până la pornirea serverului și deschiderea aplicației, fără verificări vizuale.
gate.js introdus pentru autentificarea portalului trimitea pagina locală fără token
spre /portal; aplicația preview izolată nu avea această rută.

Răspunsul /adechit generat cu repository_factory local poartă data-ade-preview="true".
Gate-ul recunoaște acest marker servit de backend și nu redirecționează preview-ul.
Pagina runtime obișnuită nu are markerul și păstrează verificarea sesiunii portalului.
Drepturile API și autentificarea runtime nu sunt schimbate. Preview-ul rămâne accesibil
exclusiv localhost. În launcher, / și /portal redirecționează la /adechit pentru a
recupera inclusiv adresa 404 deja deschisă în browser. Nu introducem un portal local fictiv.

## Fișiere

- PYTHON/routes/adechit/__init__.py: marker în răspunsul HTML local.
- PYTHON/static/js/adechit/gate.js: recunoașterea markerului.
- ADECHIT/tools/preview.py: rutele de intrare locală.
- Statusul principal/detaliat și acest worklog.

## Verificări executate

- Identificate după comanda exactă cele două procese Python ale preview-ului pe 5050;
  repornit exclusiv preview-ul cu codul curent. Nu au fost oprite alte aplicații.
- Launcherul cu .venv pornește pe 127.0.0.1:5050 fără traceback; instanțierea blueprintului
  corectată în ADE1-05 este inclusă în această probă.
- Edge headless, contexte noi fără token: /adechit, /, /portal?next=%2Fadechit și
  /adechit?unit=preview2 ajung în aplicație. Contextul și situația întorc 200, datele
  inițiale se încarcă; nicio eroare JavaScript, cerere eșuată sau HTTP >=400.
- Flask test client: pagina runtime păstrează gate.js și nu primește markerul preview;
  pagina preview îl primește. API-ul preview refuză adresa nonlocală cu 403.
- Context, catalog-data, situation/1 și receipt-defaults?IDL=1: HTTP 200.
- Probe reproductibile: artifacts/ade1-06-startup.cjs și ade1-06-http.py.

Nu s-au făcut capturi, evaluări de layout, probe ale editării sau salvări de business.
Nu s-a rulat întreaga suită; autorizarea acestei intervenții privește numai pornirea.

## Predare

Serverul preview rămâne pornit local. Se deschide http://localhost:5050/adechit.
Fără commit, push, SQL sau modificări ale serverului live. Prin AvacontPush se predau
fișierele runtime din PYTHON; launcherul și probele sunt exclusiv locale.
Următoarea subfelie liberă: ADE1-07.
