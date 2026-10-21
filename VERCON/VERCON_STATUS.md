# VERCON — status (registru separat, ca ADECHIT_STATUS)

## SLICE-VC09-05 — Reglare registru OI, produs completat la migrare — 21.10.2026

**IMPLEMENTAT ÎN SURSE, NERULAT:** migratorul 0.3.11.0 ia primirea scoaterilor din `OI_Predare` (bon + produs, ca în
Access) și completează produsul lipsă al pozițiilor de primire BP din registru numai când e sigur; restul (N17) se
reglează în VERCON din fereastra nouă «Reglare registru OI» (butonul din paginile Bon de predare / transfer / casare).
Mesajele migratorului și ale balanței OI sunt rescrise în română clară. [Worklog](../docs/worklog/SLICE-VC09-05-reglare-registru-oi-si-produs-la-migrare.md).
Doar build, `py_compile` și `node --check`; fără teste, rulări, browser sau SQL. Fereastra nu se deschide automat la
prima intrare; datele deja migrate se reglează din fereastră.

## Migrator: storno fără NIR părinte ignorat — 19.10.2026

**IMPLEMENTAT ÎN SURSE, NERULAT:** 0.3.10.0 importă IntrariStorno numai dacă
IdIntrare corespunde unui NIR cu bifa Storno din aceeași sursă/gestiune.
Restul liniilor sunt omise înaintea verificării IdProdus, NON BLOCKING,
cu raport N15 și reconciliere prin SkippedRows. Liniile valide păstrează
verificările de produs și includ variantele folosite exclusiv de storno.
Înlocuiește importarea storno-urilor orfane cu IdIntrare NULL; nu modifică
datele deja migrate sau fluxurile web pentru orfanii existenți.
[Regulă și versiune](VRC.Migrator/README.md). Fără teste/build/import,
interogări sau modificări de schemă/date. Necesită recompilare.

## Migrator: recuperare IDBO pentru bon refăcut — 19.10.2026

**IMPLEMENTAT ÎN SURSE, NERULAT:** 0.3.9.0 poate recupera un IDBO dispărut
dintr-o singură primire BP/BT cu produs/preț/deținător/cont identice, dată
document compatibilă și disponibil suficient. Snapshotul corectat este
comun remapării IDBO și contului moștenit; transferul păstrează aceeași
referință pe ambele laturi. N14 listează asocierile, X17 blochează cazurile
fără candidat unic. [Regulă și exemplul IDOI 1576](REGISTRU_OI_MF.md).
Fără teste, build sau import pentru codul nou; sursa Access a fost citită
read-only în etapa de diagnostic solicitată de utilizator. Fără modificări
de schemă, Access sau date MariaDB. Necesită recompilare.

## Registrul OI/MF și migrator 0.3.7.0 — 18.10.2026

**IMPLEMENTAT ÎN SURSE, NERULAT:** decizia privind identitatea comună P/T/C
este confirmată. `IDBC` din registru identifică bonul curent pentru toate
operațiile; `IDBP`/`IDBT` marchează predarea/transferul, iar `IDBO` păstrează
proveniența BP/BT numai la transfer/casare. Migratorul și aplicația sunt
aliniate, fără schimbare de schemă și fără repararea automată a datelor
deja migrate. [Contract, exemple și fișiere](REGISTRU_OI_MF.md).

Eșec raportat de utilizator: rollback cu `Data too long for column ToolVersion`.
Corecție în surse: `VC_Imports.ToolVersion` primește versiunea executabilului
(`0.3.7.0`), cu regulile complete păstrate separat. Necesită recompilare;
nu există încă o confirmare de import reușit după corecție. Fără teste,
build, aplicație, migrator sau interogări MariaDB rulate de asistent.

## VC06-04 — documente migrate fără sursă, 18.10.2026

**IMPLEMENTAT ÎN SURSE, NERULAT:** retururile fără bon sursă sunt marcate și se asociază manual cu un BC după validarea produselor și cantităților. Liniile de storno fără recepție au listă proprie în Factură storno, asociere manuală cu o FCS existentă sau anulare cu motiv. Scrierile folosesc context, drepturi, blocare, perioadă, versiune, audit și reluarea cererii; numai asocierea storno introduce mișcare în stoc. [Worklog și limite](../docs/worklog/SLICE-VC06-04-br-si-fcs.md). Criteriile de acceptare sunt descrise în [implementare_vercon_v2](../implementare_vercon_v2.md), fără rulare. Ajutorul web include fluxurile noi.

Decizia OI/MF semnalată inițial aici a fost ulterior confirmată și implementată
în surse; vezi revizia OI/MF de mai sus. Fără teste, build, verificări automate,
browser, HTTP, SQL ori schimbări de schemă pentru acea corecție.

## VC06-06 — implementarea noului editor NIR, 16.10.2026

**IMPLEMENTAT ÎN SURSE, NERULAT:** [VC06-06-D01–D11](DECIZII_DESCHISE.md#g-decizii-nir-și-contul-produsului--16102026-slice-vc06-06). Eliminată macheta separată și butonul «Factură comună». Adăugare NIR folosește arbore de conturi din întregul DC și ComboTree pentru produse, cu căutare și poziționare în viewport. Mai multe conturi din aceeași destinație rămân în același NIR; numai destinații diferite impun «auto». Salvarea întregii repartizări este atomică, cu rezervarea temporară a destinațiilor suplimentare și alocarea numerelor la salvare. Contul liniei se deduce prin produs; schimbarea contului unui produs cu mișcări este blocată în UI și API. Importul Access nu se schimbă pentru acest flux. [Worklog slice](../docs/worklog/SLICE-VC06-06-facturi-comune-corectii-nir-si-schema-unica.md).

Fără teste, build, browser sau SQL executate. Nu se declară validare pe server ori acceptare vizuală. Noile rădăcini de interfață (Alimente/Materiale/OI/MF) folosesc contextele tehnice SubunitId/GestiuneId existente. Detaliile de activare și comisiile pe destinații sunt în [documentul implementării](FACTURI_NIR_IMPLEMENTARE.md).

## Revizie infrastructură schemă — 16.10.2026

VRC.Migrator reutilizează `KBot.Migrator.TargetSchema.Load` și `WriteOrder.Derive` pentru schema reală și ordinea FK. Maparea Access este `import_map.json`, generată de `tools/import_map.js`, fără tipuri SQL/nulabilitate pentru țintă. Sursa structurii este AVACONT_SURSA de pe server; DDL-urile noi sunt incrementale, aplicate manual și sincronizate separat. Eliminat scriptul complet generat `sql/VC_AVACONT_SURSA.sql`, generatorul său `tools/ddl.js` și fragmentele de creare. Referințele la acestea și la `target_schema.json` din registrul istoric de mai jos descriu livrările vechi. `VC_Golire.sql` rămâne un script istoric cu listă fixă, fără regenerare și fără rol de descriere a schemei actuale.

Stare revizie: **IMPLEMENTAT ÎN SURSE**, fără build, teste, verificări automate sau rulare MariaDB. Validarea pe server din 12.10 nu certifică această revizie.

Actualizat: 12.10.2026 (corecții mockupuri/documentație UI). Stari folosite (PLAN §Reguli de implementare): PLANIFICAT, IN_LUCRU, IMPLEMENTAT_LOCAL, VERIFICAT_LOCAL, VALIDAT_SERVER, ACCEPTAT. Un document scris sau un build trecut nu este validare. **Singurul lucru validat pe server este transferul VC05 pe 000_DEMO (12.10.2026); nimic nu este ACCEPTAT.**

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
| SLICE-VC03-06 | `sql/VC_Golire.sql` (generat de `tools/ddl.js`); [worklog](../docs/worklog/SLICE-VC03-06-golire-tabele-vc.md) | **IMPLEMENTAT_LOCAL** | Golește toate tabelele `VC_*` ca să se poată reface o migrare; nerulat de mine; nu atinge `Parteneri` |
| SLICE-VC04-01 | `operations.py`, `sql/VC04_01_roluri_vr.sql`; [worklog](../docs/worklog/SLICE-VC04-01-matricea-operatiilor-vr.md) | **IMPLEMENTAT_LOCAL** | Doar sintaxa verificata; nerulat |
| SLICE-VC04-02 | `scoped()` + `repository.py` + `tables.py`; [worklog](../docs/worklog/SLICE-VC04-02-guard-si-repository-cu-context.md) | **IMPLEMENTAT_LOCAL** | Doar sintaxa verificata; nerulat |
| SLICE-VC04-03 | `sql/VC04_03_privilegii_migrator.sql` (sablon); [worklog](../docs/worklog/SLICE-VC04-03-acl-locale-si-contul-migratorului.md) | **IMPLEMENTAT_LOCAL** | Doar sintaxa verificata; nerulat |
| SLICE-VC04-04 | `lock.py`, `js/vercon/lock.js`, rute `/lock/*`; [worklog](../docs/worklog/SLICE-VC04-04-ocupare-exclusiva-cache-drafturi.md) | **IMPLEMENTAT_LOCAL** | Doar sintaxa verificata; nerulat |
| SLICE-VC04-05 | [PROBE_ACCES](PROBE_ACCES.md) (parțial executat); [worklog](../docs/worklog/SLICE-VC04-05-probe-de-acces-si-audit.md) | **IN_LUCRU** | Probele de apel direct rulate pe preview (peste 40 de apeluri, 2 defecte gasite si reparate); restul neexecutat; nimic pe MariaDB/roluri reale |
| SLICE-VC05-01 | `VERCON/VRC.Migrator` (reader read-only + profilare + raport), `tools/expected.js`; [worklog](../docs/worklog/SLICE-VC05-01-reader-si-profilare.md) | **IMPLEMENTAT_LOCAL** | Compilat curat; **rulat de operator pe tz_2025 si tz_2026 (047_GR47) la 12.10.2026**: hash stabil, schema conforma; raman MF/OI si baza fara Ver |
| SLICE-VC05-02 | `VrcPlan.vb`, grila de asociere din formular; [worklog](../docs/worklog/SLICE-VC05-02-planul-si-formularul-de-asociere.md) | **VALIDAT_SERVER** (000_DEMO) | Plan complet pe tz_2025 + tz_2026 (047_GR47), despărțire pe conturi Alimente/Materiale; rulat de operator |
| SLICE-VC05-03 | `VrcRunner.vb`, `VrcConvert.vb`, `target_schema.json` (generat de `tools/ddl.js`); [worklog](../docs/worklog/SLICE-VC05-03-writer-atomic-si-maparea-cheilor.md) | **VALIDAT_SERVER** (000_DEMO) | COMMIT confirmat 12.10.2026, ImportId 2, ~5 s: 719/556 NIR, 3.080/2.480 intrări, 580/525 bonuri, 6.642/5.146 ieșiri. Neprobat: OI/MF, grupurile OI, alte DC-uri |
| SLICE-VC05-04 | stări, oprire, jurnal, Diagnostic; [worklog](../docs/worklog/SLICE-VC05-04-oprire-jurnal-si-diagnostic.md) | **IMPLEMENTAT_LOCAL** | Rollback după blocaj verificat în rulări reale; **neprobate, ignorate la cererea operatorului (12.10.2026)**: Oprește în timpul transferului, COMMIT fără răspuns, refuzul după marker |
| SLICE-VC05-05 | reconciliere înainte de COMMIT, raport; [worklog](../docs/worklog/SLICE-VC05-05-reconciliere-si-raport.md) | **VALIDAT_SERVER** (000_DEMO) | Numărători și sume egale pe toate cele 42 de tabele/context-uri; **neprobat, ignorat la cererea operatorului (12.10.2026)**: diferența introdusă deliberat |
| SLICE-VC05-06 | `VrcAnaf.vb`, `VrcPartnersForm.vb`, `KBotDataView.HeaderClick`; [worklog](../docs/worklog/SLICE-VC05-06-parteneri-din-anaf.md) | **VALIDAT_SERVER** (000_DEMO) | 13 parteneri din ANAF adăugați și confirmați; bifarea SS per furnizor (click pe titlu = toți). Fereastra nevăzută de mine pe ecran |
| SLICE-VC05-07 | `VrcPartnerStore.AddInTransaction`, `VrcPlan.NewPartners`; [worklog](../docs/worklog/SLICE-VC05-07-parteneri-in-tranzactia-transferului.md) | **VALIDAT_SERVER** (000_DEMO) | Partenerii se scriu în aceeași tranzacție cu transferul (cad odată cu el); **neprobat, ignorat la cererea operatorului (12.10.2026)**: eșec provocat după inserare |
| SLICE-VC05-08 | `Parteneri.Provenienta` (`sql/Parteneri_Provenienta.sql`), rutele Parteneri; [worklog](../docs/worklog/SLICE-VC05-08-coloana-parteneri-provenienta.md) | **IMPLEMENTAT_LOCAL** | Compilat; **ALTER + sincronizare rulate de operator (12.10.2026)**; rutele Python deployate (confirmat de operator, 12.10.2026). Valori: MIGRARE_VERCON, manual, avacont; gol = migrare_kbot |
| SLICE-VC05-09 | `VrcMigratorForm`, `VrcRunResult.Verdict`; [worklog](../docs/worklog/SLICE-VC05-09-rezultat-colorat-si-ordinea-butoanelor.md) | **IMPLEMENTAT_LOCAL** | Rezultat verde / portocaliu / roșu + propoziție la începutul raportului; «Citește» sub «Alege folder»; verificat pe ecran de operator (12.10.2026) |
| SLICE-VC05-10 | `VrcMigratorForm` («+ Adaugă Baza Date…» selector de fișiere, coloana Folder, rotunjire la 4 zecimale în loc de blocaj); [worklog](../docs/worklog/SLICE-VC05-10-adauga-baza-date-mai-multe-foldere.md) | **IMPLEMENTAT_LOCAL** | Fișiere din mai multe foldere (Alimente / Materiale / OI / MF); compilat curat, nerulat, nevăzut. Regulile planului pe mai multe foldere neverificate |
| SLICE-VC06-01 | `provider.py` (provider real VC_*), `nomenclatoare.py`, `nom_routes.py`, `js/vercon/nomenclatoare.js`; [worklog](../docs/worklog/SLICE-VC06-01-nomenclatoare-si-configurarea-gestiunii.md) | **IMPLEMENTAT_LOCAL** | Categorii, produse + variante de pret, clienti/gestionari, TVA, comisii, setari (zecimale/scara); inactivare, revizie, RequestKey, audit. Doar sintaxa verificata; nerulat, nevazut pe ecran, nimic pe MariaDB |
| SLICE-VC06-02 | `protocol.py`, `documents.py`, `doc_routes.py`, `js/vercon/docs.js`; [worklog](../docs/worklog/SLICE-VC06-02-fc-nir-si-extensia-mf.md) | **IMPLEMENTAT_LOCAL** | NIR cu calcul pe server, N01/N02, numerotare, extensie MF (activ = varianta + ProduseM), arbore furnizor → data → factura. Doar sintaxa verificata; nerulat |
| SLICE-VC06-03 | BC + `protocol.check_timeline`, disponibil la data; [worklog](../docs/worklog/SLICE-VC06-03-bc-si-controlul-disponibilului.md) | **IMPLEMENTAT_LOCAL** | BC, stoc negativ interzis pe zile in aceeasi tranzactie, disponibil informativ. Doar sintaxa verificata; nerulat |
| SLICE-VC06-04 | BR (retur) si FCS (storno); [worklog](../docs/worklog/SLICE-VC06-04-br-si-fcs.md) | **IMPLEMENTAT_LOCAL** | Retur/storno partial si multiplu, cantitati eligibile, pret din documentul sursa. Doar sintaxa verificata; nerulat |
| SLICE-VC06-05 | editare/anulare/istoric; [worklog](../docs/worklog/SLICE-VC06-05-editare-anulare-si-traseul-documentului.md) | **IMPLEMENTAT_LOCAL** | Revizie, perioada inchisa, motiv la anulare, dependente, audit. Doar sintaxa verificata; nerulat |
| SLICE-VC06-06 | Facturi comune, corecții AUTO, validări NIR și schemă unică; [worklog](../docs/worklog/SLICE-VC06-06-facturi-comune-corectii-nir-si-schema-unica.md) | **IMPLEMENTAT_LOCAL** | Extensie 16.10.2026; cod și DDL manual pregătite. Fără build, teste, verificări automate, browser sau SQL. VRC folosește infrastructura KBot.Migrator; schema nu se creează automat |
| SLICE-VC06-07 / 08 | NIR din facturile primite prin e-Factura: `efactura_queue.py`, `efactura-intake.js`, `docs.js`, `vercon-efactura.css`; [worklog 07](../docs/worklog/SLICE-VC06-07-nir-din-efactura-lista-si-completare.md), [worklog 08](../docs/worklog/SLICE-VC06-08-nir-din-efactura-inchiderea-trimiterii.md); plan [EV_PLAN](../docs/EV_PLAN.md) | **IMPLEMENTAT_LOCAL** | 21.10.2026: lista facturilor trimise de contabil din K-BOT, completarea ciornei (furnizor, factură, valoare, conturi propuse, linii fără produs), returnarea cu motiv, închiderea trimiterii în tranzacția NIR-ului. Doar sintaxa verificată (py_compile, node --check); nerulat, fără browser/SQL. Cere `sql/EV_01_primite_vercon.sql` rulat și serverul K-BOT 00EF-45 |
| SLICE-VC07-01 | `lista.py`, `js/vercon/lista.js`, nomenclator Feluri; [worklog](../docs/worklog/SLICE-VC07-01-lista-alimente.md) | **IMPLEMENTAT_LOCAL** | Lista cu mese, feluri si mai multe BC-uri (1:N), popup Alege B.C., salvare atomica, anulare care elibereaza bonurile. Retetarul neimplementat. Doar sintaxa verificata; nerulat |
| SLICE-VC07-02 | `oi.py` (BP), registrul OI, `docs.js`; [worklog](../docs/worklog/SLICE-VC07-02-oi-predare-si-registrul-detinatorului.md) | **IMPLEMENTAT_LOCAL** | Predare = BC creat (stoc) + OI_Predare + registru (detinere), fara dublare; id-uri noi dintr-un contor. Doar sintaxa verificata; nerulat |
| SLICE-VC07-03 | `oi.py` (BT, BS); [worklog](../docs/worklog/SLICE-VC07-03-oi-transfer-si-casare.md) | **IMPLEMENTAT_LOCAL** | Transfer net zero si casare pe loturi, cantitate eligibila, pret pozitiv. Doar sintaxa verificata; nerulat |
| SLICE-VC07-04 | `mf.py`, `js/vercon/mf.js`; [worklog](../docs/worklog/SLICE-VC07-04-evidenta-mf.md) | **IMPLEMENTAT_LOCAL** | Registru MF: active, detinator, casat, editare DNF/DF cu revizie, valoare lunara recalculata. Doar sintaxa verificata; nerulat |
| SLICE-VC07-05 | [PROBE_VC07](PROBE_VC07.md); [worklog](../docs/worklog/SLICE-VC07-05-verificare-end-to-end-pe-tipuri.md) | **PLANIFICAT** | Matricea de 18 scenarii scrisa, nimic executat; scenariile OI/MF sunt sintetice |
| SLICE-VC08-01 | `stock_source.py`, `stock.py`, `stock_routes.py`, `js/vercon/stock.js`; [worklog](../docs/worklog/SLICE-VC08-01-motor-stoc-si-balante.md) | **IMPLEMENTAT_LOCAL** | SI/rulaje/SF/fișă pe variantă/deținător, disponibil draft, N07–N09 în motor; 10 probe sintetice și fixture Chromium trecute. Fără MariaDB/deploy. Furnizor = proveniență explicită, fără FIFO |
| SLICE-VC08-02 | `depreciation.py`, `depreciation_source.py`, `depreciation_read.py`, `js/vercon/depreciation.js`; [worklog](../docs/worklog/SLICE-VC08-02-motor-amortizare.md) | **IMPLEMENTAT_LOCAL** | Lunile calendaristice/DF, rest ultima lună, sursele valorii lunare, transfer/casare, diagnostice și dialog MF. 11 probe sintetice pregătite, nerulate. Proiecție cu parametri curenți; cantitățile ambigue diagnosticate. Fără teste/browser/SQL/MariaDB |

Restul (9 subfelii) PLANIFICAT, plus VC02-05; VC04-05 este in lucru. VC05-04/05/07: cazurile neprobate (Oprește, eșec după parteneri, diferență la reconciliere) sunt ignorate la cererea operatorului (12.10.2026). VC05-01 a fost rulat pe tz_2025/tz_2026 (047_GR47, profilare); VC05-02, 03, 05, 06 si 07 au fost rulate de operator pe 000_DEMO (COMMIT confirmat); VC05-04, 08 si 09 sunt scrise si compilate, **neprobate**.

## Urmatorul pas

Actualizare 13.10.2026: **VC08-03 Materiale/Alimente IMPLEMENTAT_LOCAL, NERULAT**; vezi [worklog](../docs/worklog/SLICE-VC08-03-rapoarte-magazie-alimente.md). Generare ecran/PDF/Excel, SQL și antet/subsol per unitate în KbotAdmin, editor vizual de bază și infrastructură Excel comună în `PYTHON/reporting/excel_widths.py`. Rapoarte OI/MF amânate. Fără teste/build/browser/SQL sau paritate validată. VC08-04/05 rămân separat, iar VC08 nu este declarat încheiat. Referirea de mai jos la VC08-03 ca pas curent precedă această actualizare.

Continuarea curentă după VC08-02: **VC08-03 — catalogul și generarea rapoartelor incluse**. CSS-ul modular și regula de ~1000 linii/modul sunt în `PYTHON/AGENTS.md`. Verificările locale trecute aparțin CSS/VC08-01; VC08-02 este nerulat. Integrarea MariaDB rămâne nevalidată.

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

**Corectie 12.10.2026 — stocul pe cont:** un produs apartine unui cont si pe un document stau numai produsele contului lui (NIR, BC, BP); pagina ofera doar acele produse, serverul le refuza pe celelalte; BP are etichetele conturilor corectate (Cont de intrare / Cont primitor). Vezi worklog VC06-03 si VC07-02. Doar sintaxa verificata; nerulat.
