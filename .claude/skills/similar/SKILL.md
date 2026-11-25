---
name: similar
description: Face o macheta noua sau o macheta schimbata IDENTICA cu o macheta existenta (model) si ajusteaza doar ce difera (campuri, reguli). Exemplu - "$similar br ca bc, cu campurile lui retur". Gaseste singur toate locurile unde modelul apare (JS, CSS, rute, numerotare, arbore, ajutor) si le reproduce.
---

# `$similar <noua> ca <model> [, diferente]` — copiaza un model si ajusteaza-l

Exemplu: `$similar br ca bc, cu campurile lui retur`. Adelina da numele machetei noi, modelul si, in cuvinte, **doar ce difera**. Tot restul se mosteneste de la model, nu se intreaba. Conventiile generale de macheta sunt in `$form` (butoane custom, «+» in antet, fara mesaje, Enter la urmatoarea caseta etc.) si se aplica si aici.

**Inainte de cod:** slice-ul il decide Adelina. Daca nu l-a dat, intreaba si nu edita nimic. Nu rula teste, nu te conecta la server, nu face deploy, nu scrie in Git (nici `git mv`). `$ssh` doar la cererea ei. Scripturi de editare multipla: node, in scratchpad.

## Pasul 1 — Gaseste TOATE locurile modelului (inainte sa scrii)
Cauta cheia modelului (ex. `'bc'`, `bc`, `VC_Iesiri`, `.vc-doc--bc`) in tot VERCON si fa o lista de tabel: loc → ce face pentru model → ce trebuie pentru macheta noua. Minim:
- `PYTHON/static/js/vercon/` — fisierul machetei (ex. `docs.js`): definirea coloanelor/randurilor (`lineSpecs`), antetul, `inline` vs fereastra, sursa (`config.source`), `eligible`, grupari de arbore (`bcGroupings`), sortare implicita, eticheta butonului de salvare, `layoutId`, hover/⋮ in arbore, «Modifica numar».
- `PYTHON/static/css/` — toate listele de selectori unde apare modelul (ex. `vercon-consumption.css`, `vercon-documente-panels.css`, `vercon-integration.css`, `vercon-status.css`, `vercon-responsive.css`): macheta noua se adauga in aceleasi liste, nu se copiaza CSS.
- `PYTHON/routes/vercon/` — rutele (`doc_routes.py`, `documents.py`), `number_change.py` (`TABLES`, serie, tip contor), `operations.py`/`__init__.py` (drepturi), `capabilities.py` (`FORMS`, meniu).
- Numerotare si prefix: prefixele bonurilor sunt REGULA ZERO (BC, BX, BP, BS, BT = prefix + UN spatiu + numar). Nu inventa alt prefix.
- `PYTHON/static/help/vercon.json` — sectiunea modelului.
Daca modelul are un mod aparte (modal/inline), pastreaza-l; mosteneste tot ce e comun.

## Pasul 2 — Spune diferentele inainte de editare (scurt)
Un tabel de 3 coloane: **Se mosteneste neschimbat** / **Se schimba** (campuri, sursa datelor, reguli de eligibilitate) / **Nu se stie** (intrebari pentru Adelina). Intreaba doar ce nu se poate deduce din cod, schema (`MariaDB_Schema/`) sau memorie; cate o intrebare, nu un chestionar. Nu continua pe presupuneri la campuri de baza de date.

## Pasul 3 — Implementeaza
- Reutilizeaza codul modelului: extrage in functii comune sau adauga ramuri pe cheia noua; nu dubla blocuri mari. Un fisier nou doar pentru partea specifica (ex. `return-editor.js`), cand fisierul existent trece de ~1000 de linii.
- Campurile noi: scrise ca in schita ei. Nu adauga coloane sau butoane pe care schita nu le are; fara butoane de stergere pe rand.
- Daca macheta noua depinde de alta (ex. retur → bonul consum de la care pleaca), alegerea e combobox cu tastare, cu DOAR elementele care mai au ce se returna/consuma; regulile de eligibilitate raman ca la model (ex. nu cele de pe liste de alimente).
- Server: acelasi `guard`/`scoped`, audit, `CreatedAt`/`UpdatedAt` la tabele noi, aceleasi verificari de context si blocare ca la model.
- REGULA 0: fara diacritice in cod, nume, chei, comentarii; diacritice doar in textul citit de operator.

## Pasul 4 — Verifica si inchide
- Doar sintaxa: `py_compile`, `node --check`, `JSON.parse`. Spune ce NU a fost rulat in aplicatie. Revizuieste lista din Pasul 1 si confirma ca fiecare loc a fost tratat (cate unul mai uitat = macheta pe jumatate).
- Ajutor in `vercon.json` (sectiune cu slice, doar ce vede si face utilizatorul), worklog `docs/worklog/SLICE-<...>.md`, `VERCON/VERCON_STATUS.md`, `implementare_vercon_v2.md`.
- Raport: cuvinte simple, fara jargon: ce s-a mostenit, ce s-a schimbat, ce e neverificat. Registrul de deployment: doar la `$ssh`, dupa proba ei.
