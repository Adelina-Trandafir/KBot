# SLICE-VC02-01 — alegerea DC/subunitate/gestiune/an

Istoricul aceluiași ciclu al paginii: alegere context → capabilități → draft/editare → salvare/reconciliere. Reviziile contextului și meniului rămân datate în etapele lor, iar matricea browser din VC02-05 rămâne planificată.

<a id="slice-vc02-01"></a>

Stare inițială comună VC02-01…04: **IMPLEMENTAT_LOCAL**. Verificat doar sintaxa (Python `ast`, `node --check`); nimic rulat, previzualizat sau testat; nimic validat pe server.

## Facut
- `routes/vercon/context.py`: `resolve` (functie pura) valideaza selectia pe arborele furnizat de un provider; autoselectie cand exista o singura optiune; anul implicit = cel mai recent; `version` se schimba cu selectia si cu optiunile. `EmptyProvider` in productie (pana la VC03).
- `GET /api/vercon/context` (selectie, optiuni, meniu, `type_locked`, `year_mode`); selectie invalida → 409 `SELECTION_CHANGED`.
- Pagina: trei selectoare in cascada + bara de context permanenta; raspunsurile sunt acceptate doar daca raspund ultimei cereri (numar de secventa).
- Draft: la schimbarea contextului cu modificari nesalvate se intreaba «Raman aici / Renunt»; `draft.js` retine contextul in care a pornit.
- Provider de fixture in `VERCON/tools/preview.py`; fixture extins (doua gestiuni Materiale in A1, una goala, una ocupata).

## Limite
Providerul real (tabele VC_*) vine cu VC03; ocuparea exclusiva reala cu VC04-04.

## Context și meniu comun — 18.10.2026

D001-09, istoric: **IMPLEMENTAT ÎN SURSE, NERULAT**. Numai design din surse; fără teste/build/verificări automate, browser/HTTP, MariaDB/SQL/DDL, schimbări de schemă/date, migrator, deploy/commit/push. Aspectul nu a fost probat în aplicație.

Regulile de mai jos sunt revizii ulterioare implementării inițiale: alegerea explicită a subunității în endpoint-ul de context înlocuiește aici autoselecția inițială. Disponibilitatea rapoartelor MF din 18.10 înlocuiește amânarea consemnată în jurnalul OI din 17.10.

<a id="d001-09-etapa-1"></a>

### Meniu: selecție progresivă Subunitate / Gestiune / An

La deschiderea paginii, dintre selectorii contextului se afișează doar Subunitate. Endpoint-ul context cere alegerea explicită a subunității, inclusiv când există una singură; celelalte apeluri păstrează comportamentul anterior al resolverului. După alegerea subunității, o gestiune unică se selectează automat pe server și selectorul ei rămâne ascuns. Selectorul Gestiune apare numai pentru subunități cu mai multe gestiuni. Anul apare după stabilirea gestiunii, cu ultima valoare disponibilă selectată conform regulii existente; operațiile continuă să provină din contextul serverului.

Gestiunea și anul sunt ascunse și în HTML-ul inițial, înaintea încărcării JavaScript. Controalele ascunse sunt dezactivate, iar resetarea contextului golește opțiunile vechi. Ajutorul din aplicație descrie noua succesiune. Se păstrează confirmarea modificărilor nesalvate la schimbarea contextului.

<a id="d001-09-etapa-2"></a>

### Meniu comun și Fluxuri / Mișcări pentru OI/MF

Configurări, Intrări și Rapoarte sunt comune tuturor tipurilor de gestiune. Intrări grupează NIR și Factură Stornare pentru toate tipurile, cu deschiderea paginii de consultare. Ieșiri apare la Alimente/Materiale cu Bon Consum și Bon Retur; Lista Alimente rămâne exclusiv la Alimente. OI/MF primesc Fluxuri / Mișcări cu Bon Predare, Bon Transfer și Bon Casare; MF adaugă Amortizare și Registru MF. Grupurile folosesc disponibilitatea și motivul dezactivării primite de la server; verificarea la deschiderea directă păstrează aceleași reguli de tip, drepturi și ocupare.

Rapoartele MF nu mai sunt excluse global din catalog și din formular. Pentru raportul Amortizare se reutilizează proiecția existentă depreciation_read, cu întregul istoric până la data calculului, filtre de variante și observații pe activ. Macheta implicită are coloane de durate, valori și stare, fără totalizarea implicită a activelor neeligibile. Opțiunea Amortizare din meniu deschide direct acest raport, cu un singur câmp La data și exporturile comune PDF/Excel. Din Rapoarte, calculul folosește data de sfârșit a perioadei, explicată în notele raportului. Exportul Excel păstrează modulul comun de lățimi. Rapoartele de deținere MF sunt grupate în arbore sub Mijloace fixe.

### Fișiere pe etapă

Căi istorice; numele fără prefix continuă directorul precedent.

| Etapă | Fișiere |
|---|---|
| 1 | PYTHON/routes/vercon/context.py, __init__.py; PYTHON/static/vercon.html; PYTHON/static/js/vercon/app.js; PYTHON/static/css/vercon-controls.css; implementare_vercon_v2.md. |
| 2 | PYTHON/static/js/vercon/app.js, reports.js, report-result.js; PYTHON/routes/vercon/capabilities.py, report_catalog.py, report_data.py, report_layouts.py, report_service.py, report_routes.py; implementare_vercon_v2.md. |

## Aspectul selectorilor — 15.10.2026 <a id="d001-13-context"></a>

Preluare D001-13: în PYTHON/static/css/vercon-controls.css, Subunitate/Gestiune/An au 28px înălțime și rază 7,5px, ca ADECHIT Subunitate. Tranziția all .2s ease este dezactivată numai pe acestea, eliminând animația fundalului la ieșirea din hover; evidențierea hover rămâne. CSS modificat, fără teste/browser.

<a id="slice-vc02-02"></a>

## SLICE-VC02-02 — meniuri, capabilitati, coloane

### Facut
- `routes/vercon/capabilities.py`: un singur tabel (formular → tipuri permise, drept necesar) pentru meniu SI pentru refuzul apelului direct (`GET /api/vercon/forms/<form>/open`: 404/403 `FORM_NOT_AVAILABLE`/403/409 ocupat). Sursa: ribbonul din MAPARE_ACCESS_WEB + regulile din plan.
- BC/BR doar Materiale+Alimente; Lista alimente doar Alimente; BP/BT/BS doar OI+MF; Inchidere an nu pentru OI/MF; coloanele NIR de MF (nr. inventar, data intrarii, durata) doar pentru MF; preferinta de formular salvata cu cheia tipului.
- Tip editabil doar pe gestiune goala: `POST /api/vercon/gestiune/type` (drept `configure`) → 409 `TYPE_LOCKED` dupa populare; selectorul din pagina se dezactiveaza.

### Limite
Schitele se folosesc ca ghid de campuri, nu ca machete pixel cu pixel (formularele reale vin la VC06/VC07). Operatiile `edit`/`configure` se definesc oficial la VC04-01. Formularele non-document (produse, categorii etc.) sunt doar intrari de meniu cu text de pregatire.

### Gruparea istorică Intrări/Ieșiri <a id="d001-13-meniu"></a>

Preluare D001-13; etapă în surse, fără teste/browser, anterioară meniului comun OI/MF din 18.10 descris în [context](SLICE-VC02-01-alegerea-contextului.md#d001-09-etapa-2).

- app.js grupează ALIMENTE/MATERIALE în INTRARI: Factură cumpărare (NIR), Factură Stornare (fără FCS în etichetă), separator/Listare. Adăugare dispare din acest meniu; factura deschide consultarea, fără adăugare automată. Listare rămâne acțiunea goală existentă, fără funcție nouă.
- IESIRI: Bon Consum, Bon Retur, separator/Lista Alimente numai dacă serverul o oferă, exclusiv ALIMENTE. Butoanele separate de storno/retur/listă devin submeniuri.
- Fiecare copil păstrează cheia/deschiderea/enabled/reason server. Părintele este disponibil dacă măcar un copil e disponibil, activul urmărește cheia copilului. Minime INTRARI 260px/IESIRI 224px. Configurări, Rapoarte și OI/MF păstrau atunci structura anterioară. Fișier: PYTHON/static/js/vercon/app.js.

<a id="slice-vc02-03"></a>

## SLICE-VC02-03 — editare

### Facut
- `static/js/vercon/fields.js`: contract per coloana (text/int/decimal/date): parse, validare, format. Virgula zecimala, `1.234,56`, data `zz.ll.aaaa`, gol = NULL diferit de 0, text pastrat exact (apostrof, diacritice).
- `draft.js`: draft in afara DOM, randuri dupa cheie stabila (`n1`, `n2`…), `collect` valideaza tot si da erori pe camp.
- `editor.js`: antet + linii pe fixture; repictarea liniilor din draft nu pierde valorile.

### Limite
Nu s-a folosit `DataGrid` comun (nu exista inca date mari / virtualizare); decizia de a-l adopta se ia la G5 din PROBE_BROWSER. Combobox/date picker custom nu sunt legate in editor. Totalurile read-only nu exista inca (VC06).

<a id="slice-vc02-04"></a>

## SLICE-VC02-04 — salvare, conflict, sesiune expirata

### Facut
- `static/js/vercon/save.js`: o salvare = un RequestKey; rezultate saved / error / conflict / unknown (+ expired, stale). La unknown cheia NU se schimba; urmeaza `reconcile` (GET dupa cheie). Al doilea apel in timpul unei salvari returneaza aceeasi promisiune (dublu Enter).
- Raspunsul este folosit doar daca versiunea de context e cea din momentul pornirii; eroarea si sesiunea expirata pastreaza datele in formular.
- Stand-in de server doar in preview: `POST /api/vercon/preview/save` (revizie + cheie idempotenta, `X-Preview-Simulate: lose-response`) si `GET …/save/<cheie>`.

### Limite
Endpoint-urile reale de salvare vin la VC06; in productie butonul spune ca salvarea vine cu VC06. Diagnosticul pe linie/camp exista doar pentru validarea din pagina.

<a id="slice-vc02-05"></a>

## SLICE-VC02-05 — probe browser si ajutor

Stare: **PLANIFICAT** — matricea de probe este scrisa, nimic nu a fost executat; ajutorul nu este scris.

- Matricea inițială `VERCON/PROBE_BROWSER.md`, absorbită prin D001-04 în [catalog — browser](../../VERCON/PROBE_ACCES.md#probe-browser): scenarii C/M/E/S/G cu coloana „Rezultat" goala.
- Nu exista masuratori de performanta. Acceptarea acestei subfelii cere rularea reala a matricei si ajutorul pe flux/gestiune.
