# SLICE-VC02-01 — alegerea DC/subunitate/gestiune/an

Stare: **IMPLEMENTAT_LOCAL**. Verificat doar sintaxa (Python `ast`, `node --check`); nimic rulat, previzualizat sau testat; nimic validat pe server.

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
