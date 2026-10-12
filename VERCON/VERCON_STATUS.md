# VERCON — status (registru separat, ca ADECHIT_STATUS)

Actualizat: 12.10.2026 (corecții mockupuri/documentație UI). Stari folosite (PLAN §Reguli de implementare): PLANIFICAT, IN_LUCRU, IMPLEMENTAT_LOCAL, VERIFICAT_LOCAL, VALIDAT_SERVER, ACCEPTAT. Un document scris sau un build trecut nu este validare. **Nimic nu este VALIDAT_SERVER sau ACCEPTAT.**

## Corecții vizuale și fluxuri — 12.10.2026

Sursa: [Corectii_mockup-uri_01](MOCKUPURI/Corectii_mockup-uri_01/) și mesajul ulterior cu cinci atașamente, **revizia 2**. NIR este referința unică pentru toate grilele, fără padding mare; o listă de alimente poate avea mai multe BC-uri, în DGV cu valori per rând și total general. [FLUXURI_UI](FLUXURI_UI.md), PLAN, DECIZII, MAPARE, CONTRACT_CALCULE și PROBE_BROWSER sunt aliniate; SCHEMA_VC/DICTIONAR_DATE disting schema Access de cardinalitatea țintă solicitată. Acoperire vizuală în [indexul v2](MOCKUPURI/2026-10-12-corectate-v2/INDEX.md): consultare/editor, popup BC, predare și casare. Stocuri, NIR și Rapoarte sunt păstrate identic. Stare: **DOCUMENTAT**; PNG-urile nu certifică implementarea sau probele browser/API. DDL și generatorul sunt aliniate la 1:N (12.10.2026, `uq_LegBCLA_IdBonConsum`; în Access era 1=1, funcționalitate nouă); `VC_LegBCLA` 1:N aplicată pe AVACONT_SURSA și pe cele 20 de DC-uri (12.10.2026, confirmat de operator); regulile listă–BC sunt în `PYTHON/routes/vercon/lista_bc.py` (doar sintaxa verificată, nerulat, fără rute); restul VC07-01 rămâne PLANIFICAT. [Worklog v2](../docs/worklog/VERCON-2026-10-12-corectii-mockupuri-v2.md).

Regula numerotarii: feliile VC00-VC09, subfeliile `SLICE-VCxx-yy`; worklog in `docs/worklog/SLICE-VCxx-yy-*.md`. Documentatia: [PLAN](PLAN_IMPLEMENTARE.md) · [DETECTIE_VBA](DETECTIE_VBA.md) · [decizii](DECIZII_DESCHISE.md).

## Felia VC00 — reguli si mapare

| Subfelie | Livrabile | Stare | Dovada / limita |
|---|---|---|---|
| SLICE-VC00-01 | [MANIFEST_SURSE](MANIFEST_SURSE.md), [INVENTAR_OBIECTE](INVENTAR_OBIECTE.md) (377 obiecte, o singura dispozitie fiecare) | **VERIFICAT_LOCAL** | Hash-uri SHA-256 identice la inceput si sfarsit (runda 2: `tz_Alimente` nu s'a putut recalcula, deschis de MSACCESS al utilizatorului); schema citita read-only. Clasificare statica, completata cu referentii fiecarui obiect nelamurit (`tools/referenti.js`), ribbonul si meniul SFP primite: dupa runda 3: 0 DE-CLARIFICAT, 13 INLOCUIT-PARTENERI-KBOT, 8 NEREFERIT-NEPORTAT; lipseste frontendul (3 rapoarte MF; `.tmpl` nu se portează) |
| SLICE-VC00-02 | [DICTIONAR_DATE](DICTIONAR_DATE.md) (tabele, 26 relatii business, abateri de schema, legaturi logice) | **VERIFICAT_LOCAL** | Generat din TZ_SCHEMA + dump DAO read-only al celor 3 baze; tipurile tinta sunt propuneri (DDL = VC03-02) |
| SLICE-VC00-03 | [CONTRACT_CALCULE](CONTRACT_CALCULE.md): `ZEC`/`SCARA` pe gestiune, linie/total, N01/N02, servicii, etaloane | **VERIFICAT_LOCAL** (partea cu dovezi) / IMPLEMENTAT_LOCAL | Probe agregate pe bazele reale (4 scripturi). Etaloanele E01, E04-E10, E13-E17 sunt sintetice; rotunjirea calculelor documentelor noi (HALF_EVEN) este propunere de verificat la VC08-05; afisarea rotunjeste in sus (decis) |
| SLICE-VC00-04 | 101 schite PNG + [INDEX](SCHITE/INDEX.md) + trasabilitate pe control, [MAPARE_ACCESS_WEB](MAPARE_ACCESS_WEB.md) (ribbon, reguli din cod, intrarea VaerCon), [RAPOARTE](RAPOARTE.md) (meniul SFP) | **VERIFICAT_LOCAL** (vizual + automat) | Toate cele 101 imagini privite individual; un defect de randare (dimensiuni lipsa) corectat; dupa corectie doar 26 reprivite una cate una, restul acoperite de verificare automata (23 controale de dimensiune 0 in export). Nu reprezinta functionalitate implementata |
| SLICE-VC00-05 | [DECIZII_DESCHISE](DECIZII_DESCHISE.md), acest status, worklogs | **IMPLEMENTAT_LOCAL** | Runda 3 (Q1-Q12) integrata: nu mai exista intrebari deschise cerute utilizatorului; ramase nesolicitate: D12, `Cant=0`, `AnDate=2100`, baze `tz_aaaa` lipsa, atribut DAO 8192 |

## Felii VC01-VC09

| Subfelie | Livrabile | Stare | Dovada / limita |
|---|---|---|---|
| SLICE-VC01-01 | pachetul `PYTHON/routes/vercon/` (blueprint, `/api/vercon/context`), inregistrare in `main.py`, motor numeric pur `numeric.py` (endpoint-ul `/api/vercon/status` a devenit `/api/vercon/context`); [worklog](../docs/worklog/SLICE-VC01-01-pachetul-vercon.md) | **IMPLEMENTAT_LOCAL** | Nerulat si netestat (fara teste la cerere); manifestul de publicare neinspectat |
| SLICE-VC01-02 | `static/vercon.html`, `js/vercon/*`, ruta `/vercon`, meniuri activate; [worklog](../docs/worklog/SLICE-VC01-02-pagina-si-ciclul-de-viata.md) | **IMPLEMENTAT_LOCAL** | Nerulat, nepreviazut; pagina si ciclul de viata |
| SLICE-VC01-03 | `diagnostics.py`, `connection.py`, garda cu unitate + drept; [worklog](../docs/worklog/SLICE-VC01-03-sesiune-log-conexiuni.md) | **IMPLEMENTAT_LOCAL** | Nerulat, nepreviazut; sesiune, log, conexiuni |
| SLICE-VC01-04 | `VERCON/tools/preview.py` + fixture, intrare `vercon-preview`; [worklog](../docs/worklog/SLICE-VC01-04-preview-reproductibil.md) | **IMPLEMENTAT_LOCAL** | Nerulat, nepreviazut; preview |
| SLICE-VC01-05 | `js/vercon/documents.js`, [INTEGRARE_SISTEME](INTEGRARE_SISTEME.md); [worklog](../docs/worklog/SLICE-VC01-05-integrarea-documentelor-si-schemelor.md) | **IMPLEMENTAT_LOCAL** | Nerulat, nepreviazut; documente si scheme |
| SLICE-VC02-01 | `routes/vercon/context.py`, `/api/vercon/context`, selectoare + bara de context; [worklog](../docs/worklog/SLICE-VC02-01-alegerea-contextului.md) | **IMPLEMENTAT_LOCAL** | Doar sintaxa verificata; nerulat |
| SLICE-VC02-02 | `capabilities.py`, `/forms/<f>/open`, `/gestiune/type`, meniu; [worklog](../docs/worklog/SLICE-VC02-02-meniuri-capabilitati-coloane.md) | **IMPLEMENTAT_LOCAL** | Doar sintaxa verificata; nerulat |
| SLICE-VC02-03 | `fields.js`, `draft.js`, `editor.js`; [worklog](../docs/worklog/SLICE-VC02-03-editare-in-grila.md) | **IMPLEMENTAT_LOCAL** | Doar sintaxa verificata; nerulat |
| SLICE-VC02-04 | `save.js` + stand-in de salvare in preview; [worklog](../docs/worklog/SLICE-VC02-04-salvare-conflict-sesiune.md) | **IMPLEMENTAT_LOCAL** | Doar sintaxa verificata; nerulat |
| SLICE-VC02-05 | [PROBE_BROWSER](PROBE_BROWSER.md) (neexecutat); ajutor nescris; [worklog](../docs/worklog/SLICE-VC02-05-probe-browser-si-ajutor.md) | **PLANIFICAT** | Nicio proba rulata |
| SLICE-VC03-01 | `sql/VC_AVACONT_SURSA.sql`; [worklog](../docs/worklog/SLICE-VC03-01-ierarhia-si-politica-anuala.md) | **IMPLEMENTAT_LOCAL** | Scripturi scrise; nimic aplicat pe MariaDB, nicio proba rulata |
| SLICE-VC03-02 | `sql/VC_AVACONT_SURSA.sql` (generat de `tools/ddl.js`), [SCHEMA_VC](SCHEMA_VC.md); [worklog](../docs/worklog/SLICE-VC03-02-tabelele-business-tipuri-fk.md) | **IMPLEMENTAT_LOCAL** | Scripturi scrise; nimic aplicat pe MariaDB, nicio proba rulata |
| SLICE-VC03-03 | `sql/VC_AVACONT_SURSA.sql`; [worklog](../docs/worklog/SLICE-VC03-03-importuri-audit-idempotency.md) | **IMPLEMENTAT_LOCAL** | Scripturi scrise; nimic aplicat pe MariaDB, nicio proba rulata |
| SLICE-VC03-04 | `VC_Settings`, `VC_Numbering` (in VC_03); [worklog](../docs/worklog/SLICE-VC03-04-configurare-numerotare-date-comune.md) | **IMPLEMENTAT_LOCAL** | Scripturi scrise; nimic aplicat pe MariaDB, nicio proba rulata |
| SLICE-VC03-05 | `sql/VC_00_preflight.sql`, `sql/VC_99_probe.sql`, `sql/VC_Reguli_DC.sql`; [worklog](../docs/worklog/SLICE-VC03-05-aplicare-sincronizare-proba.md) | **IMPLEMENTAT_LOCAL** | Scripturi scrise; nimic aplicat pe MariaDB, nicio proba rulata |
| SLICE-VC04-01 | `operations.py`, `sql/VC04_01_roluri_vr.sql`; [worklog](../docs/worklog/SLICE-VC04-01-matricea-operatiilor-vr.md) | **IMPLEMENTAT_LOCAL** | Doar sintaxa verificata; nerulat |
| SLICE-VC04-02 | `scoped()` + `repository.py` + `tables.py`; [worklog](../docs/worklog/SLICE-VC04-02-guard-si-repository-cu-context.md) | **IMPLEMENTAT_LOCAL** | Doar sintaxa verificata; nerulat |
| SLICE-VC04-03 | `sql/VC04_03_privilegii_migrator.sql` (sablon); [worklog](../docs/worklog/SLICE-VC04-03-acl-locale-si-contul-migratorului.md) | **IMPLEMENTAT_LOCAL** | Doar sintaxa verificata; nerulat |
| SLICE-VC04-04 | `lock.py`, `js/vercon/lock.js`, rute `/lock/*`; [worklog](../docs/worklog/SLICE-VC04-04-ocupare-exclusiva-cache-drafturi.md) | **IMPLEMENTAT_LOCAL** | Doar sintaxa verificata; nerulat |
| SLICE-VC04-05 | [PROBE_ACCES](PROBE_ACCES.md) (parțial executat); [worklog](../docs/worklog/SLICE-VC04-05-probe-de-acces-si-audit.md) | **IN_LUCRU** | Probele de apel direct rulate pe preview (peste 40 de apeluri, 2 defecte gasite si reparate); restul neexecutat; nimic pe MariaDB/roluri reale |
| SLICE-VC05-01 | `VERCON/VRC.Migrator` (reader read-only + profilare + raport), `tools/expected.js`; [worklog](../docs/worklog/SLICE-VC05-01-reader-si-profilare.md) | **IMPLEMENTAT_LOCAL** | Compilat curat; **rulat de operator pe tz_2025 si tz_2026 (047_GR47) la 12.10.2026**: hash stabil, schema conforma; raman MF/OI si baza fara Ver |
| SLICE-VC05-02 | `VrcPlan.vb`, grila de asociere din formular; [worklog](../docs/worklog/SLICE-VC05-02-planul-si-formularul-de-asociere.md) | **IMPLEMENTAT_LOCAL** | Compilat curat; nepornit |
| SLICE-VC05-03 | `VrcRunner.vb`, `VrcConvert.vb`, `target_schema.json` (generat de `tools/ddl.js`); [worklog](../docs/worklog/SLICE-VC05-03-writer-atomic-si-maparea-cheilor.md) | **IMPLEMENTAT_LOCAL** | Compilat curat; **nicio linie scrisă vreodată în MariaDB**, nerulat |
| SLICE-VC05-04 | stări, oprire, jurnal, Diagnostic; [worklog](../docs/worklog/SLICE-VC05-04-oprire-jurnal-si-diagnostic.md) | **IMPLEMENTAT_LOCAL** | Compilat; nerulat |
| SLICE-VC05-05 | reconciliere înainte de COMMIT, raport; [worklog](../docs/worklog/SLICE-VC05-05-reconciliere-si-raport.md) | **IMPLEMENTAT_LOCAL** | Compilat; nicio reconciliere reală |

Restul (20 subfelii) PLANIFICAT, plus VC02-05; VC04-05 este in lucru. VC05-01 a fost rulat pe tz_2025/tz_2026 (047_GR47, profilare); VC05-02…05 sunt scrise si compilate, **neprobate**.

## Urmatorul pas

1. Nu exista raspunsuri cerute. Atribuirea fisierelor (folder, subunitate, tip, DC) o face operatorul in migrator, la import.
2. Fara dependente deschise: rularea preflight + probe pe o baza de test MariaDB (VC03-05), reader/profilare VC05-01 (doar citire), DDL VC03-01.

## Reproducere

```powershell
node VERCON\tools\inventar.js
powershell -NoProfile -ExecutionPolicy Bypass -File VERCON\tools\dump_schema.ps1
node VERCON\tools\dictionar.js
node VERCON\tools\schite.js
powershell -NoProfile -ExecutionPolicy Bypass -File VERCON\tools\schite_render.ps1
node VERCON\tools\mapare.js
```

Scripturile citesc doar `VERCON_ACCESS` (care este ignorat de Git); iesirile intermediare sunt in `VERCON/tools/out/`.
