# K-BOT — STATUS, slices 0000–0009

Moved verbatim out of `../KBOT_STATUS.md` (28.09.2026). The index there says what
each slice is; this file holds everything recorded about it: its registry row, its
«Current focus» notes and its «Open threads» notes.

---

## Slice 0000 — HELP (ajutorul interactiv + manualul)

**Standing slice.** Operator, 30.09.2026: EVERY piece of work on the help (engine, capture tool,
topics, screenshots, tours, manual) is recorded HERE, as a sub-slice `0000-NN`, never under a new
slice number. Worklogs: `SLICE-0000-NN-<slug>.md`.

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0000 | **Ajutor interactiv + manual, în trei părți: Contabil / Opțiuni avansate / Director (cererea operatorului, 30.09.2026)** | în lucru | — | Plan: 01 motorul; 02 unealta de capturi; 03 conținut Partea 1; 04 tururi ghidate; 05 Partea 2; 06 Partea 3 + cerințele MF la un DDF nou (ORD nou încă nu există → exclus). Textul se scrie din fișierele .md ale feliilor; fără interiorul controalelor și al browserului; pagina FOREXE doar ca mod de funcționare. Imaginile le face operatorul prin unealta de capturi (0000-02), inclusiv pe PC-ul clientului pentru fluxul FOREXE. |
| 0000-01 | **Motorul de ajutor: F1 oriunde, «?» pe fiecare bară de titlu, fereastra de ajutor (cuprins, căutare, Înapoi/Înainte), export manual HTML** | GATA pe cod (build **0 avertismente, 0 erori**) / **văzut pe ecran** (login F1, «?» în fereastra principală, căutare) | `SLICE-0000-01-help-engine.md` | Subiecte Markdown în `src/KBot.App/HelpContent/` → `<AppDir>\Help\`; Markdig 0.45.0. `KBotHelp` (Theming) + `HelpService` (App). Director → doar Partea 3; ceilalți Partea 1 + Partea 2 cu opțiunile avansate active. FileVersion nebumped. (Numerotat întâi 0097-01, renumerotat la cererea operatorului.) |
| 0000-02 | **Unealta de capturi pentru ajutor** — etichete `<!-- capture: ... -->` în subiecte, fereastra «Capturi pentru ajutor» cu «Fă poza» pe fiecare rând, poziționare pe ecran + selecție cu dreptunghi, PNG salvat unde îl cere eticheta | GATA pe cod (build **0 avertismente, 0 erori**); comutatorul văzut de operator; fluxul de captură nerulat de Claude | `SLICE-0000-02-help-capture-tool.md` | Comutator în Setări › Aplicație, vizibil doar cu opțiunile avansate; nedescris în ajutor. Etichete `<!-- capture: id | caption | goto | prepare -->`; salvare în `<AppDir>Helpimg` + sursă pe build din repo. |
| 0000-03 | **Partea 1 «Contabil»: textul, cu etichete de captură** | GATA (text; build **0 avertismente, 0 erori**) / de citit de operator | `SLICE-0000-03-partea-1-contabil.md` | 31 de subiecte + 30 de etichete de captură; secțiunea «Ce cere MF la un DDF nou» din `FUNDAMENT_DocumentFundamentare_CAB.md`. Tooltipul lui `cboSs` spune «Subperioada» dar e sursă/sector — nemodificat. |
| 0000-04 | **Tururi ghidate** (+ tooltipul «Sursă / sector» corectat) | GATA pe cod (build **0 avertismente, 0 erori**), nevăzut pe ecran | `SLICE-0000-04-tururi-ghidate.md` | Fișiere `HelpContent/tours/*.md`; inel colorat în jurul controlului + bulă cu Înapoi / Înainte / Închide; pornire din pagina subiectului și din pagina de start. Patru tururi pentru Partea 1. |
| 0000-05 | **Partea 2 «Opțiuni avansate»: textul, cu etichete de captură** | GATA (text; build **0 avertismente, 0 erori**) / de citit de operator | `SLICE-0000-05-partea-2-optiuni-avansate.md` | 6 subiecte + 7 etichete: activare, Documente (Adobe / Excel), Pagina FOREXE, Temă, Căi fișiere, jurnale. Modul de capturi NU e descris (regula operatorului). |
| 0000-06 | **Partea 3 «Director»: textul, cu etichete de captură, + turul ferestrei directorului** | GATA (text; build **0 avertismente, 0 erori**) / de citit de operator | `SLICE-0000-06-partea-3-director.md` | 4 subiecte (prezentare, lista, alte unități, semnarea) + 3 etichete + `tur-director` (5 pași). Pozele se fac pe calculatorul directorului și se încarcă cu «Încarcă» (lista de capturi e doar în fereastra contabilului). |
| 0000-07 | **Acoperire F1 + finisare**: fiecare fereastră a operatorului are subiect; subiect nou «Actualizarea K-BOT»; turul Părții 2 | GATA (text; build **0 avertismente, 0 erori**) / de citit de operator | `SLICE-0000-07-acoperire-si-finisare.md` | Ferestre noi acoperite: Alegerea unității, Grafice și benzi, Istoric angajament (preluarea salvărilor din pagină), Golește jurnale, Actualizare, Informații interne. Fraza «ce modifici de mână ocolește K-BOT» înlocuită (greșită de la 0073). Editorul ORD lăsat afară — decizia operatorului. |
| 0000-08 | **Ordonanțarea în ajutor + documentația sistemului de ajutor** — vederea ORD rescrisă, subiecte noi «Ordonanțări noi (și ștergerea lor)» și «Editorul de ordonanțare»; `docs/HELP_SYSTEM.md` (ghidul de întreținere + procedura de actualizare); `tools/HelpCheck/Check-Help.ps1`; trimiteri din CLAUDE.md, CODE_WORKFLOW.md, README | GATA (text + script; build **0 avertismente, 0 erori**; verificarea **fără erori**, acoperire completă) / de citit de operator | `SLICE-0000-08-ord-si-documentatie.md` | Fluxul MF pentru trimiterea ORD în FOREXE NU e descris (neclar, K-BOT nu trimite încă ORD). Tooltipul greșit al lui «Lipește» din atașamentele ORD corectat. |
| 0000-09 | **Semnarea ORD din formularul MF** — validarea în doi pași, cele cinci semnături în ordine, minimul (1 + 2 + Ordonator), CFP înaintea ordonatorului; `docs/FUNDAMENT_Ordonantare_CAB.md` | GATA (text; build **0 avertismente, 0 erori**; verificarea fără erori) / de citit de operator | `SLICE-0000-09-semnarea-ord.md` | Din `Surse/ord_xdp.xml` + `ord_xdp_full.xml`. Pasul următor (recepție + încărcare pe serverul CAB) doar numit, nefăcut în K-BOT. 1 + 2 + 5 și cele două validări confirmate de operator; avertisment «semnează după a doua validare» + ordinea pe două persoane. |
| 0000-10 | **Semnarea ORD refăcută pe macheta A1.0.11** — validare → semnătura 1 → validare → semnătura 2 → (CFP) → ordonator; «Alte Avize», «Verificat/Avizat», «Anulare Validare»; fără ORDNT.xml la validare | GATA (text; build **0 avertismente, 0 erori**; verificarea fără erori) / de citit de operator | `SLICE-0000-10-ord-a1-0-11.md` | Înlocuiește descrierea din 0000-09 (era macheta veche A1.0.08). Din `Surse/ETAPE ORDONANTARE/`; `Etapa3_xdp.xml` = copie a lui Etapa1. Serverul servește A1.0.11 (confirmat). Etapa 1 doar cu col. 4 = fundătură (starea de după semnătura 1 nu se salvează); tabelul complet înaintea semnăturii 1. |
| 0000-11 | **Cinci tururi noi**: Ordonanțare, Recepții, Plăți, Extrase de cont, Setări (11 tururi în total) | GATA (text; build **0 avertismente, 0 erori**; verificarea fără erori) / nevăzut pe ecran | `SLICE-0000-11-tururi-noi.md` | Fără tur pentru editorul ORD și Asocieri (ferestre modale). Corectat în trecere: schimbarea parolei e pe pagina «Informații», nu «Autentificare». |
| 0000-12 | **Fereastra Asocieri, explicată pe larg** — secțiune nouă (de ce există, fereastra pe părți, pas cu pas, cazuri speciale) + `tur-asocieri` (8 pași, pornit din fereastră) | GATA (text; build **0 avertismente, 0 erori**; verificarea fără erori) / de citit de operator | `SLICE-0000-12-asocieri.md` | Din `FUNDAMENT_Asociere_Receptii.md` + `AsociereForm`. F14 (oprit) și regula datei (retrasă) lăsate afară. Turul merge doar pornit cu fereastra deschisă (fereastră modală). |

### Ajutorul e la zi până la

**30.09.2026 — codul de azi, ultima felie din index: 0096** (inclusiv 0095-02). Textul a fost scris
din codul curent, nu din planuri. Următoarea actualizare pornește de la feliile de după 0096 și de
la lista de mai jos (`docs/HELP_SYSTEM.md` §4). Mută acest reper la fiecare 0000-NN.

### Open threads

- **Ajutor de actualizat** (feliile de funcționalitate adaugă aici ce subiecte / capturi au
  învechit, ex. `0098: contabil.vederi.plati — coloana nouă «Cont»; captura plati de refăcut`):
  _nimic deocamdată._

- **0000-01** — exportul manualului neapăsat pe ecran; fereastra Director nevăzută; tema întunecată nevăzută.
- **Observat în trecere:** `000_DEMO` nu are tabela `FX_NoteCAB_Corectii` (eroare 1146 la fiecare pornire, `RefreshUncorrelatedMarkAsync`).

---

## Slice 0001

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0001 | Auth — bearer tokens, session store (Felia 1) | DONE | (pre-worklog-rule) | Static API key eliminated for K-BOT; legacy FOREXE still uses X-Api-Key |

---

## Slice 0002

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0002 | Split-brain 401 fix + reason codes | DONE | (pre-worklog-rule) | login mints via STORE; every 401 carries a reason code |

---

## Slice 0003

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0003 | Redis session backend | DONE (code) / config PENDING on VPS | — | `SESSION_BACKEND="redis"`, `SESSION_KEY_PREFIX`, `REDIS_DB=2` must be set in host config.py; verify DB 2 is free |

---

## Slice 0004

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0004 | Tier 1 hardening | IN PROGRESS | — | rate limiter DONE+pushed; remaining: ApiOptions address, retire AppConfig, verify gunicorn guard. Plan: `KBOT_Tier1_Plan.md` |

---

## Slice 0005

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0005 | Phase A cleanup (lying tests, https guard) | DONE | — | Python 75 passed / 7 skipped, 0 fail/error; .NET 80 green |

---

## Slice 0006

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0006 | MainForm scaffolding | DONE | (pre-worklog-rule) | Plan: `PLAN_MainForm_Scaffolding.md`. Built against its own 11-item checklist; `WithReauth(Of T)` + ListaAngajamente vertical preserved. Real DI signature is 5 params (`forexeRunner, session, apiClient, authApi, loginFactory`), not the 4 the plan expected |

---

## Slice 0007

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0007 | AngajamentTreeInfo POCO correction | SUPERSEDED by 0008 | `SLICE-0008-tree-data-api.md` | Was done WRONG: built against `qFX_MAIN_TREE` alone, so `Salarii` was dropped and `IDORD` kept. 0008 rewrote the POCO against the real contract (row-source `_DESCRIERE` + flags `qFX_MAIN_TREE`): `Salarii` restored, `IDORD` dropped |

---

## Slice 0008

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0008 | Tree data API + `MainForm.LoadTree` | DONE (code) / UNVERIFIED on a live DB | `SLICE-0008-tree-data-api.md`, `SLICE-0009-maintree-loadtree.md` (Part A amendment) | Plan: `PLAN_TreeDataApi.md`. `GET /api/forexe/tree` (an/ss/include_hidden, base from session), nine `EXISTS` flags, POCO rewrite, tree load + nav gating. **Amended by 0009:** the SS filter now has an orphan escape (`EXISTS SS OR NOT EXISTS any indicators`) so zero-indicator angajamente stay visible. **No part of it has touched a real database** — all route tests are host-only and skip off-host |

### Current focus

- **Now:** run Slices 0008 + 0009 on the host. The endpoint (incl. 0009's orphan escape)
  and the client are written and green offline, but nothing has hit a real database:
  `PYTHON/tests/test_forexe_tree.py` skips off-host and is the fastest way to answer
  verification items 1–5 and 8, plus the two new orphan tests (`TREEO`/`TREEX`).

### Open threads

- ~~`KBotNavList` has no `SetItemVisible` — 0008 gates views with `SetItemEnabled`
  (grey-out) instead of hiding.~~ RESOLVED in slice 0018: `SetItemVisible` added and
  `ApplyViewGating` now hides (not greys) a view whose `Are*` flag is FALSE.

---

## Slice 0009

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0009 | `MainForm.LoadTree` (client half) + tree orphan escape | DONE (code) / UNVERIFIED on a live DB | `SLICE-0009-maintree-loadtree.md` | The brief's Parts B/C/D (DTOs, `GetTreeAsync`, `LoadTreeAsync` + gating) were **already shipped by 0008**; the real deltas are Part A (orphan escape on the server, see 0008 row) + its 2 host-only tests, and the 4 `GetTreeAsync` client tests 0008 never added (Api 26 → 30). Kept 0008's choices: mapping in the client (no `BuildTreeInfo`), token from session (no param), `IDDF As Long?` throughout. LoadTree is period-driven (runs on load + every An/SS change = the `SetPeriod` precondition) |
