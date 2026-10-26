# K-BOT — STATUS, slices 0040–0049

Moved verbatim out of `../KBOT_STATUS.md` (28.09.2026). The index there says what
each slice is; this file holds everything recorded about it: its registry row, its
«Current focus» notes and its «Open threads» notes.

---

## Slice 0040

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0040 | Arborele, trecut PRIN TOT cu sistemul de scalare | DONE (code) / **nevăzut pe ecran, la nicio scalare** | [0040](../SLICE-0040-tree-scaling-sweep.md) | Lista completă a măsurilor, schimbarea ColumnDef.Width și limitele probelor sunt în jurnal. |

---

## Slice 0041

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0041 | Stocarea PDF-urilor SEMNATE pe server (`FX_DDF_PDF` / `FX_ORD_PDF`) | DONE (code) / **DDL NEVERIFICAT, niciodată rulat pe o bază vie, nevăzut pe ecran** | [0041](../SLICE-0041-pdf-storage.md) | Contractele, probele, cifrele și limitele rămân în etapa legată. |

---

## Slice 0042

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0042 | Migrarea tabelelor `FX_` din Access în MariaDB | DONE (code) / **niciodată rulat: nici pe Access, nici pe o bază vie, nevăzut pe ecran** | [0042](../SLICE-0042-migrare-fx-access.md) | Contractele, probele, cifrele și limitele rămân în etapa legată. |

### Open threads

- [Exportul JSON, designerul și ipotezele driverului](../SLICE-0042-migrare-fx-access.md); probele și limitele sunt păstrate în jurnal.

---

## Slice 0043

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0043 | Pasul de deblocare a cheilor străine din `schema_sync` | DONE / **verificat pe un MariaDB 10.3.32 real** (bază de test locală, nu producție) | [0043](../SLICE-0043-fk-unblock-collation.md#slice-0043) | Starea inițială privește proba locală; etapele 01/02 păstrează raportul operatorului pe 22 de baze și reproducerile locale. |
| 0043-01 | `schema_sync`: o singură bază + integritatea legăturilor spre `AVACONT_COMUN` | DONE / **verificat pe MariaDB 10.3.32 real** | `SLICE-0043-fk-unblock-collation.md` (secțiunea 0043-01) | `--targets` sincroniza deja o singură bază; ce lipsea era refuzul unui nume greșit — `verify_targets()` oprește rularea cu cod 1, fiindcă altfel o tastare greșită arăta identic cu o rulare curată («deja sincronizate», cod 0). O bază din `CAI` care nu există rămâne simplu avertisment. Al doilea fir: `_validate_foreign_keys()`, singurul loc din diff care se uită la **rânduri** — pentru fiecare cheie de creat verifică existența tabelului referit, potrivirea seturilor de caractere (comparate cu forma **finală**: în țintă coloana referită se repară la pasul 8, înainte de pasul 11; pentru `AVACONT_COMUN` starea curentă **este** cea finală — prima variantă a comparat planificat vs. curent și a marcat trei chei sănătoase ca imposibile) și potrivirea datelor (`LEFT JOIN`+`COUNT`, cu valori vinovate în mesaj). Ce nu trece primește `error_msg` și nu se execută. Dacă însă cheia **există deja** în țintă, refuzul se extinde peste tot grupul — ștergerea, charset-ul de pe ambele capete și recrearea stau pe loc împreună, altfel cheia s-ar pierde definitiv. Măsurat: rulare cu date murdare = 11/11 reușite, 5 rânduri amânate, cheia păstrată; după curățarea datelor 5/5 și convergență la zero. |
| 0043-02 | `schema_sync`: eroarea 1054 de la prima rulare pe cele 22 de baze | DONE / **reprodusă și corectată pe MariaDB 10.3.32**; rularea reală pe cele 22 de baze NU a fost reluată | `SLICE-0043-fk-unblock-collation.md` (secțiunea 0043-02) | Verificarea de integritate din 0043-01 număra rândurile orfane pe ținta **așa cum e acum**, dar coloana cheii urma să fie adăugată de același lot la prioritatea 7 — deci cerea o coloană inexistentă: `Unknown column 'c.IdPartener'`. Reprodusă în ambele variante (coloana lipsă la copil și la părinte). Acum verificarea datelor rulează doar dacă ambele capete au deja toate coloanele; altfel întrebarea nu se pune, iar o nepotrivire de date iese la execuție cu 1452. Prezența se citește din schemele deja aflate în memorie, iar coloanele din `AVACONT_COMUN` se cache-uiesc (`_ext_columns`). În plus: `generate()` **compară întâi și golește după** — până acum golea rândurile neexecutate, apoi compara, deci o comparație căzută la mijloc lăsa `schema_diff_log` complet gol (exact ce a pățit operatorul: 693 de rânduri șterse, niciunul scris). Și eroarea unei verificări este relansată cu numele cheii și al bazei, nu goală. **NEREPARAT, de semnalat:** `PK MODIFY` stă la prioritatea 3, dar o cheie primară mutată pe o coloană **nouă** (adăugată la 7) eșuează cu 1072 — `_pk_handled_by_add_column` acoperă doar cazul `auto_increment`. |

---

## Slice 0044

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0044 | Migrarea prin fișier `.accdb` împins pe server | DONE (code) / **nimic rulat pe date reale: mdbtools nepornit niciodată, nicio rută apelată de un client viu, niciun formular văzut pe ecran** | [0044](../SLICE-0044-migrare-accdb-impins.md#slice-0044-baza) | Etapele, contractele, probele și restanțele sunt în jurnalul consolidat D001-15; Status păstrează checkpointul istoric. |
| 0044-02 | `cale.accdb` scos cu totul · selecție pe unitate · upsert · bife pe tabele | DONE (code) / **nimic rulat pe date reale; panoul nou de tabele nedeschis nici în designerul VS** | [0044-02](../SLICE-0044-migrare-accdb-impins.md#slice-0044-02) | Etapele, contractele, probele și restanțele sunt în jurnalul consolidat D001-15; Status păstrează checkpointul istoric. |
| 0044-03 | Corelații de coloane Access ▸ MariaDB · filă nouă · toate grilele pe `KBotDataView` | DONE (code) / **nimic rulat pe date reale; formularul nedeschis nici în designerul VS** | [0044-03](../SLICE-0044-migrare-accdb-impins.md#slice-0044-03) | Etapele, contractele, probele și restanțele sunt în jurnalul consolidat D001-15; Status păstrează checkpointul istoric. |
| 0044-04 | Coloane obligatorii apărate · dosarul de instrucțiuni SQL | DONE (code) / **nimic rulat pe date reale; `MIGRARE_SQL_DIR` încă nepus în `config.py` pe server, deci `POST /api/migrare/rulare` refuză să pornească până atunci** | [0044-04](../SLICE-0044-migrare-accdb-impins.md#slice-0044-04) | Etapele, contractele, probele și restanțele sunt în jurnalul consolidat D001-15; Status păstrează checkpointul istoric. |

### Current focus

Lanțul HTTP și corectivele sale sunt [istoric consolidat](../SLICE-0044-migrare-accdb-impins.md), ulterior înlocuit de 0045. Limitele parserului/ORD, Unitati și configurarea SQL au rămas datate acolo.

---

## Slice 0045

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0045-01 | Migrator direct Access ▸ MariaDB — **pasul de descoperire** (`PLAN_MigratorDirect.md` §12) | DONE / **niciun cod de transfer scris, niciun server MariaDB contactat** | [0045-01](../SLICE-0045-01-descoperire-nomenclatoare.md#slice-0045-01) + [mapare](../../MAPARE_NOMENCLATOARE.md) | Etapele, contractele, probele și restanțele sunt în jurnalul consolidat D001-15; Status păstrează checkpointul istoric. |
| 0045-02 | Migrator direct — motorul: citirea Access + descrierea țintei MariaDB | DONE (cod, 0 erori / 0 avertismente) / **niciun server MariaDB atins, niciun `.accdb` deschis DIN aplicație, nimic pe ecran** | [0045-02](../SLICE-0045-01-descoperire-nomenclatoare.md#slice-0045-02) | Etapele, contractele, probele și restanțele sunt în jurnalul consolidat D001-15; Status păstrează checkpointul istoric. |
| 0045-03 | Migrator direct — catalogul de corelații, hărțile de rezoluție, porțile de verificare | DONE (cod, 0 erori / 0 avertismente) / **niciun server atins, niciun `.accdb` deschis din aplicație, nimic pe ecran** | [0045-03](../SLICE-0045-01-descoperire-nomenclatoare.md#slice-0045-03) | Etapele, contractele, probele și restanțele sunt în jurnalul consolidat D001-15; Status păstrează checkpointul istoric. |
| 0045-04 | Migrator direct — scrierea tranzacțională + dosarul de instrucțiuni SQL | DONE (cod, 0 erori / 0 avertismente) / **nimic rulat: niciun server atins, niciun dosar de jurnal scris vreodată** | [0045-04](../SLICE-0045-01-descoperire-nomenclatoare.md#slice-0045-04) | Etapele, contractele, probele și restanțele sunt în jurnalul consolidat D001-15; Status păstrează checkpointul istoric. |
| 0045-05 | Migrator direct — formularul, ștergerea lanțului HTTP, **prima rulare pe date reale** | DONE (cod, 0 erori / 0 avertismente) / **formularul NEDESCHIS nici pe ecran, nici în designerul VS; jumătatea MariaDB tot neatinsă** | [0045-05](../SLICE-0045-01-descoperire-nomenclatoare.md#slice-0045-05) | Etapele, contractele, probele și restanțele sunt în jurnalul consolidat D001-15; Status păstrează checkpointul istoric. |
| 0045-06 | Migrator direct — `schema_sync` **pe server prin SSH** pentru o bază goală, progres la verificare, bife legate de obiect, panou de constatări | DONE (cod, 0 erori) / **nimic pe ecran, niciun `ssh` lansat, nicio conexiune la VPS deschisă; fără teste și fără pas de regresie, la cererea explicită a operatorului** | [0045-06](../SLICE-0045-01-descoperire-nomenclatoare.md#slice-0045-06) | Etapele, contractele, probele și restanțele sunt în jurnalul consolidat D001-15; Status păstrează checkpointul istoric. |
| 0045-07 | Migrator direct — **unitatea e a RÂNDULUI, nu a buclei**: `IdUnitate` NULL nu mai înseamnă «toate unitățile» | DONE (cod, 0 avertismente) / **poarta rulată pe datele reale ale operatorului, dar nimic scris pe MariaDB — serverul refuză utilizatorul `Admin`** | [0045-07](../SLICE-0045-01-descoperire-nomenclatoare.md#slice-0045-07) | Etapele, contractele, probele și restanțele sunt în jurnalul consolidat D001-15; Status păstrează checkpointul istoric. |

### Current focus

Cele șapte etape sunt în [jurnalul direct](../SLICE-0045-01-descoperire-nomenclatoare.md): descoperire/DDL, Access probat, poarta pe două unități probată, scriere MariaDB și SSH nevalidate. Formularul redesenat de operator nu echivalează cu acceptarea fluxurilor. Restanțele LastInsertedId/WrittenKeys și proveniența datelor sunt păstrate pe etapă.

---

## Slice 0046

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0046 | Migrator direct — **proprietatea se hotărăște de JOS în SUS**, și poarta părintelui reparată | DONE (cod, 0 erori / 0 avertismente) / **jumătatea Access probată pe datele reale ale operatorului; niciun server MariaDB atins, deci poarta D13 nu a fost rulată** | [0046](../SLICE-0046-proprietatea-de-jos-in-sus.md#slice-0046) | Contractele, probele, cifrele și limitele rămân în etapa legată. |

### Open threads

- [Cheia țintă, documentele rămase și cazurile încă neprobate](../SLICE-0046-proprietatea-de-jos-in-sus.md); probele și limitele sunt păstrate în jurnal.

---

## Slice 0047

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0047 | Migrator direct — **poarta părintelui citea Access-ul, nu rândul care pleca** | DONE (cod, 0 erori / 0 avertismente) / **nerulat pe MariaDB** | [0047](../SLICE-0046-proprietatea-de-jos-in-sus.md#slice-0047) | Contractele, probele, cifrele și limitele rămân în etapa legată. |

---

## Slice 0048

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0048-01 | Ingestie FOREXE — **premisele**: AUTO_INCREMENT, scutirea din schema_sync, ajutoarele pure | ÎN LUCRU (transa 01 gata: cod, 0 erori / 0 avertismente; Python 306 trecute / 14 sărite) / **ruta și clientul NU sunt scrise; nimic nu a rulat pe MariaDB** | [0048-01](../SLICE-0048-01-premise-si-ajutoare.md) | Contracte, decizii, probe și limite în jurnalul legat; starea descrie numai reperul tranșei. |
| 0048-02 | Ingestie FOREXE — **pașii 1–2** și **alegerea unității de către operator** | ÎN LUCRU (transa 02 gata: build 0 erori; Python 346 trecute / 14 sărite, +39; .NET +43 teste noi, toate verzi) / **pașii 3–8 NEPORTAȚI, nimic nu cheamă încă coordonatorul, nimic n-a rulat pe MariaDB, dialogul NEVĂZUT pe ecran** | [0048-02](../SLICE-0048-02-alegerea-unitatii.md) | Contracte, decizii, probe și limite în jurnalul legat; starea descrie numai reperul tranșei. |
| 0048-03 | Ingestie FOREXE — **pașii 3–8** și **contractul în două faze** | ÎN LUCRU (transa 03 gata: build 0 erori; Python 410 trecute / 15 sărite, +64; .NET +17 teste noi, toate verzi, 0 regresii) / **nimic n-a rulat pe MariaDB, `sql/0049` neaplicat, formularul NU există, nimic nu cheamă coordonatorul** | `SLICE-0048-03-pasii-3-8-si-contractul-in-doua-faze.md` | Contractul inițial și probele: [0048-03](../SLICE-0048-03-pasii-3-8-si-contractul-in-doua-faze.md). Evoluția adoptată: [completare](../SLICE-0048-03-pasii-3-8-si-contractul-in-doua-faze.md#slice-0048-03-completare) (pasul 8 activ, F20 retrasă, structură imbricată păstrată); F13 este retras ulterior în [0048-07](../SLICE-0048-05-graficul-evolutiei.md#slice-0048-07). Starea din coloana anterioară descrie numai momentul tranșei inițiale. |
| 0048-03-c | Ingestie FOREXE — **pasul 8**, **structura care călătorește**, **căile operatorului** | ÎN LUCRU (transa de completare gata: build 0 erori / 0 avertismente noi; Python 429 trecute / 15 sărite, `+19`; .NET `+27` teste noi, toate verzi, 0 regresii) / **nimic n-a rulat pe MariaDB și nu s-a încercat, `sql/0049` neaplicat, nimic nu s-a văzut pe ecran** | [0048-03-c](../SLICE-0048-03-pasii-3-8-si-contractul-in-doua-faze.md#slice-0048-03-completare) | Corecțiile F17/F20, D-N/D-O, F28, pasul 8, schema și căile operatorului: [completare 0048-03](../SLICE-0048-03-pasii-3-8-si-contractul-in-doua-faze.md#slice-0048-03-completare). Explicațiile și lista fișierelor se întrețin numai în jurnal; probele și limitele istorice rămân distincte. |
| 0048-04 | Ingestie FOREXE — **editorul de legături R ▸ H, disponibil ORICÂND** + regula de blocare | ÎN LUCRU (cod verde: build 0 erori / 0 avertismente; Python 460 trecute / 15 sărite, `+31`; .NET `+30` teste noi, toate verzi, 0 regresii) / **nimic n-a rulat pe MariaDB, `sql/0049` tot neaplicat, formularul NEVĂZUT pe ecran și nedeschis în designerul VS** | [0048-04](../SLICE-0048-04-asociere-oricand.md) | Contracte, decizii, probe și limite în jurnalul legat; starea descrie numai reperul tranșei. |
| 0048-05 | Ingestie FOREXE — **mesajul care nu se stingea** + **graficul evoluției** (`KBotChartView`) | ÎN LUCRU (cod verde: `KBot.Controls` build 0 erori / 0 avertismente, `KBot.App` 0 erori; `.NET +16` teste noi, toate verzi) / **`AsociereForm` tot NEVĂZUT pe ecran și nedeschis în designerul VS; controlul ÎNSUȘI a fost desenat și privit pe toate trei schemele** | [0048-05](../SLICE-0048-05-graficul-evolutiei.md#slice-0048-05) | Detaliile, motivele, versiunile, fișierele și probele sunt în secțiunea omonimă a jurnalului consolidat (D001-04). Starea și limitele din acest rând sunt istorice. |
| 0048-06 | Ingestie FOREXE — **setul de culori al graficului** + **clic pe punct ▸ rândul din arbore** | ÎN LUCRU (cod verde: `KBot.Controls` build 0 erori / 0 avertismente, `KBot.App` 0 erori, soluția întreagă 0 erori; `KBot.Controls.Tests` **953/953, ZERO roșu** — roșul străin care a însoțit 0048-05 s-a închis aici) / **`AsociereForm` tot NEVĂZUT pe ecran și nedeschis în designerul VS** | [0048-06](../SLICE-0048-05-graficul-evolutiei.md#slice-0048-06) | Detaliile, motivele, versiunile, fișierele și probele sunt în secțiunea omonimă a jurnalului consolidat (D001-04). Starea și limitele din acest rând sunt istorice. |
| 0048-07 | Ingestie FOREXE — **benzile de așezare** (`KBotLaneView`, familie nouă de control), **liniile în trepte**, **reperele plăților** și **retragerea lui F13** | ÎN LUCRU (cod verde: soluția întreagă build 0 erori — `KBot.Controls` 0 erori / 0 avertismente, iar cele 6 `MSB3825` de pe `AsociereForm.resx` sunt PREEXISTENTE; `KBot.Controls.Tests` **953/953**, exact linia de plecare; `KBot.App.Tests` 185/198 cu **13 roșii PREEXISTENTE și străine** — linie luată prin `git stash`, nu din memorie; Python **465 trecute / 15 sărite**) / **NIMIC nu s-a văzut pe ecran: `KBotLaneView` și `AsociereBenziForm` nu au fost deschise nici măcar în designerul Visual Studio** / **nimic nu a rulat pe MariaDB** / **fără git: nimic comis, nimic împins** | [0048-07](../SLICE-0048-05-graficul-evolutiei.md#slice-0048-07) | Detaliile, motivele, versiunile, fișierele și probele sunt în secțiunea omonimă a jurnalului consolidat (D001-04). Starea și limitele din acest rând sunt istorice. |
| 0048-08 | Ingestie FOREXE — **benzile citibile** (bucăți colorate, loc în dreapta, fără sume), **eticheta care rămânea în urmă**, **culorile prea deschise** și **cele două vederi din dreapta sus** | ÎN LUCRU (cod verde: `dotnet build KBot.sln` **0 erori**, singurele avertismente fiind cele **6 `MSB3825` PREEXISTENTE**; `KBot.Controls.Tests` **953/953**, exact linia de plecare; `KBot.App.Tests` 185/198 cu **aceleași 13 roșii PREEXISTENTE și străine**; Python neatins) / **S-A VĂZUT PE ECRAN** — prima felie din familia 0048 despre care se poate spune asta: `KBotLaneView` randat cu `DrawToBitmap` pe Classic / Modern / Dark, iar `AsociereForm` randat întreg în amândouă vederile; sondele au fost **temporare și șterse** / **nimic nu a rulat pe MariaDB**, nicio rută Python atinsă | [0048-08](../SLICE-0048-05-graficul-evolutiei.md#slice-0048-08) | Detaliile, motivele, versiunile, fișierele și probele sunt în secțiunea omonimă a jurnalului consolidat (D001-04). Starea și limitele din acest rând sunt istorice. |
| 0048-09 | Ingestie FOREXE — **totalul recepțiilor scris chiar pe reperul plății** (socoteala §1.3 în etichetă) | ÎN LUCRU (cod verde: `dotnet build KBot.sln` **0 erori / 0 avertismente**; `KBot.App.Tests` **188/201**, cu **aceleași 13 roșii PREEXISTENTE și străine** — linia de plecare era 185/198, deci cele **3 teste noi trec toate**; `KBot.Controls` neatins; Python neatins) / **eticheta s-a văzut randată** prin chiar motorul ei (`KBotRichText` Parse ▸ Layout ▸ Draw, sondă temporară ștearsă), dar **NU ridicată de un mouse adevărat** peste o bandă adevărată / **nimic nu a rulat pe MariaDB** | [0048-09](../SLICE-0048-05-graficul-evolutiei.md#slice-0048-09) | Detaliile, motivele, versiunile, fișierele și probele sunt în secțiunea omonimă a jurnalului consolidat (D001-04). Starea și limitele din acest rând sunt istorice. |

### Current focus

Familia vizuală este consolidată în [0048-05…09](../SLICE-0048-05-graficul-evolutiei.md): [0048-07](../SLICE-0048-05-graficul-evolutiei.md#slice-0048-07) păstrează retragerea F13 pe ambele căi și hazardul F30; [0048-08](../SLICE-0048-05-graficul-evolutiei.md#slice-0048-08) păstrează lecțiile despre conținut mutabil, contrast și randarea controlului; [0048-09](../SLICE-0048-05-graficul-evolutiei.md#slice-0048-09) păstrează calculul unic/recalculat al totalului plății și colorarea numai a diferenței negative. Explicațiile și probele se întrețin numai acolo.

### Open threads

- **0048-04:** [editorul și reviziile blocajului](../SLICE-0048-04-asociere-oricand.md) păstrează limitele SQL, mouse/designer, reconstituirea, gruparea și dosarul local. Regula pe plăți a fost retrasă la 18.09, iar corecția pe liniile ORD din 20.09 rămânea neprobată pe MariaDB. Probele vizuale ulterioare sunt în [0048-08](../SLICE-0048-05-graficul-evolutiei.md#slice-0048-08).

- **0048-03-c — limitele tranșei:** [completare, §4–5](../SLICE-0048-03-pasii-3-8-si-contractul-in-doua-faze.md#completare-4-rămas-neverificat). SQL/0049 cu patru coloane era neaplicat; citirea Recepțiilor îl cere și ea. Ordinea VB, ecranul și calea reală settings.json nu fuseseră probate; formularul de setări rămânea de construit. F20/O3 sunt retrase, iar planul absent nu se mai caută.

- **0048-03 — limitele tranșei inițiale:** [jurnal, §4–5](../SLICE-0048-03-pasii-3-8-si-contractul-in-doua-faze.md#4-rămas-neverificat-sau-amânat). MariaDB, testul de identitate a celor nouă tabele, SQL/0049, cele șapte AUTO_INCREMENT și rata hash rămâneau neprobate. F27 este limita grupării imposibil de demonstrat, iar Val_Receptie_T rămâne nescrisă. Restanțele vechi F20 și Detaliu în două forme au fost închise prin completare; nu mai constituie instrucțiuni active.

- **0048-02:** [alegerea unității și restanțele](../SLICE-0048-02-alegerea-unitatii.md#4-rămas-neverificat-sau-amânat): SQL/dialog/legare neprobate la acea etapă, alegerea pierdută la update și frecvența ambiguității rămân distincte. Aspectul este proiectat; discrepanța veche 7/5/17 teste este păstrată în jurnale.

- **0048-01:** [premisele și măsurătorile](../SLICE-0048-01-premise-si-ajutoare.md#4-rămas-neverificat-sau-amânat): AUTO_INCREMENT și rata hash neprobate, invariantul măsurat doar pe două unități, definiția ParseDataZZLLAAAA absentă. Oprirea veche D17 a fost înlocuită în 02 cu întrebarea operatorului; limitele nu certifică starea actuală.

---

## Slice 0049

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0049 | **Editorul de ordonanțare** (`frmFX_ORD` + 9 subformulare + 4 module VBA) — calea de SCRIERE | ÎN LUCRU (cod verde: `dotnet build KBot.sln --no-incremental` **0 erori / 12 avertismente**, identic cu linia de plecare luată prin `git worktree` la HEAD — toate 12 sunt `MSB3825` preexistente pe `.resx`-urile de vedere; Python **465 trecute / 15 sărite**, identic; `KBot.Api` 95/96, `KBot.App` 188/201, `KBot.Domain` 14/17 — cele **17 roșii sunt PREEXISTENTE și străine** (DDF/Istoric/XFA), numărate identic pe copia curată; `KBot.Controls` 953/953) / **NICIO rută n-a atins MariaDB și NICIUN formular nu s-a deschis, nici pe ecran, nici în designerul VS** | [0049](../SLICE-0049-ord-edit-form.md#slice-0049-baza) | Contractele, corecțiile, probele și limitele de etapă sunt în istoricul reunit. |
| 0049-01 | **«+»-ul din arborele de Plăți cheamă editorul de ordonanțare** — legarea lui `mdl_FX_ORD` | ÎN LUCRU (cod verde: `dotnet build KBot.sln --no-incremental` **0 erori / 12 avertismente**, identic cu 0049, toate `MSB3825` preexistente pe `.resx`-uri de vedere — verificat prin numărare pe tip, zero avertismente de alt fel; **o compilare incrementală raportează 0, fiindcă `.resx`-urile nu se regenerează**; `KBot.App` 188/201, `KBot.Api` 95/96, `KBot.Domain` 14/17, `KBot.Controls` 953/953 — **exact** cifrele de plecare ale feliei 0049, aceleași nume, toate străine de ORD (DDF/Istoric/XFA); Python **neatins**, diff gol) / **«+»-ul n-a fost apăsat de un mouse adevărat, nimic n-a atins MariaDB, `OrdEditForm` tot nu s-a deschis pe ecran** | [0049-01](../SLICE-0049-ord-edit-form.md#slice-0049-01) | Contractele, corecțiile, probele și limitele de etapă sunt în istoricul reunit. |
| 0049-02 | **Cele cinci corecții cerute pe editorul de ordonanțare** — câmpurile care nu se umpleau, `BIC`, rândul «< TOȚI BENEFICIARII >», butoanele netematizate | ÎN LUCRU (cod verde: `dotnet build KBot.sln --no-incremental` **0 erori / 12 avertismente**, toate `MSB3825` preexistente pe `.resx`-uri de vedere — identic cu 0049 / 0049-01; `KBot.App` **188/201**, exact aceleași 13 roșii preexistente și străine (DDF/Istoric/XFA); Python **465 trecute / 15 sărite**, identic; `KBot.Controls`/`KBot.Api`/`KBot.Domain` neatinse) / **nimic n-a atins MariaDB, `AVACONT_COMUN`.`BIC` n-a fost interogată, cele trei pagini tot nu s-au deschis pe ecran** | [0049-02](../SLICE-0049-ord-edit-form.md#slice-0049-02) | Contractele, corecțiile, probele și limitele de etapă sunt în istoricul reunit. |

### Current focus

- [0049-02](../SLICE-0049-ord-edit-form.md#slice-0049-02): trei runde de corecții, cu BIC, grupurile comune, unicitatea documentelor, vizibilitatea condiționată și numărul probabil. Diagnosticul BIC din implementarea inițială este depășit; limitele/probele rămân pe etapă.

### Open threads

- [Baza](../SLICE-0049-ord-edit-form.md#slice-0049-baza): rute/date reale, formulare, DDL/chei, atașamente, nomenclator parteneri, defectul portat și întrebările amânate; [01](../SLICE-0049-ord-edit-form.md#slice-0049-01): clicul, lotul și reîmprospătarea; [02](../SLICE-0049-ord-edit-form.md#slice-0049-02): probele celor trei runde, dreptul BIC și ștergerea duplicatelor la prima salvare a documentelor vechi. Confirmarea cerută pentru corectarea defectului portat rămâne în reperele registrului.
