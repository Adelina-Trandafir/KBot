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
| 0048-01 | Ingestie FOREXE — **premisele**: AUTO_INCREMENT, scutirea din schema_sync, ajutoarele pure | ÎN LUCRU (transa 01 gata: cod, 0 erori / 0 avertismente; Python 306 trecute / 14 sărite) / **ruta și clientul NU sunt scrise; nimic nu a rulat pe MariaDB** | `SLICE-0048-01-premise-si-ajutoare.md` | Prima transă din portarea conductei FOREXE (plan `docs/PLAN_ForexeIngest.md`, scris în depozit în transa asta). **Planul a fost recitit rând cu rând față de sursa Access, nu preluat — și cinci nepotriviri au ieșit, toate în pasul 2, patru dintre ele schimbând ce se scrie.** Operatorul a decis: **D17** — D16 era cheiat pe coloana greșită: `Obtine_IdUnitate_Din` rezolvă unitatea pe **`(SS, ClsfE)`** (ultimele 6 cifre, `Right(ClsfE,8)` fără puncte), nu pe `(SS, ClsfSal)`; se portează fidel, două căutări (`ClsfE` ▸ `IdUnitate`, apoi `ClsfSal` ▸ `IdClsfAcc` în acea unitate). **D18** — `BugetIndicator` nu e citit NICIODATĂ (`Exists("BugetIndicatori")` cu *i* păzește `dInd("BugetIndicator")` fără *i*) ▸ `FX_Indicatori.Receptii`/`.Plati` nu se scriu; se portează defectul, consemnat. **D19** — o clasificație negăsită nu e eroare în Access (`If Not IsNull(IdClsf)`), deci NULL + avertisment, nu `raise`. **D20** — `FX_Indicatori_Actualizare_Extrase` e VIU (două `UPDATE` pe `FX_Extrase.CodAI` din `FX_Plati`), se include în tranzacție. Plus: **fără completare cu zerouri** — planul dedusese `Format(x,"00")` dintr-o funcție-soră; `Prelucrare_Indicatori` doar scoate spații și puncte. **Două întrebări deschise s-au închis prin citire:** `DataInceputDerulare` ARE coloană (`DataCreare`/`DataDefinitivare`), iar §13.3 = D20. **Livrat:** `AutoIncrementStep.vb` (cele 7 chei, refuză fără COMMIT, refuză `AVACONT_SURSA` și `AVACONT_COMUN`, re-rulabil, raportează rânduri/`MAX` înainte/`AUTO_INCREMENT` după, tipul luat din coloana vie ca un `BIGINT` viitor să nu fie îngustat); `EXEMPT_COLUMNS` + `is_exempt_column` în `schema_common.py`, legate în `SchemaDiff._columns` — **listă numită, nu regulă**, și acoperă **coloana întreagă**, cost scris pe față în cod; `.wfl` D11 reparat și **diferența cerută de §9 făcută** (după normalizarea spațiilor rămân exact antetul de versiune, timeout-urile 15/5 și direcția de paginare a Istoricului); `prelucrare_helpers.py` + **93 de teste noi**. **Aflate scriind ajutoarele:** hash-urile stocate sunt hex cu **MAJUSCULE** (`Hex$`), deci jumătate din necunoscuta §8 e închisă — cealaltă (codificarea din `BCrypt.HASH`) rămâne, dar nu contează, D9 dedupează pe cheia naturală; `ParseLooseNumber` alege separatorul după **poziție**, de unde `"3.587"` ▸ **3587.0**; **două defecte fidele portate** (`ExtractNumberAfterLabel` iese cu 0 fără virgulă la coadă, deci ultima găleată din `ExtractRezervareDefinitiva` se pierde; a treia ramură din `FX_ExtractCodIndicator` e cod mort inaccesibil); **`ParseDataZZLLAAAA` nu există în export** — contract dedus din locurile de apel. **Măsurat pe date reale** (jurnalul `000_DEMO/20260824_092427`, 101 rânduri, unitățile 75+76): `(SS,ClsfSal)` 101 perechi distincte, `(SS,ClsfE)` 69 — deci `ClsfE` chiar pliază, dar **zero** plieri traversează o unitate. ⚠ Dovedește puțin: 2 unități din 13, ~101 rânduri dintr-un tabel cu `AUTO_INCREMENT` 1497. ⚠⚠ **Până când pasul AUTO_INCREMENT nu rulează pe baza de test, ingestia nu poate insera** — planul interzice o rezervă MAX+1 în Python. |
| 0048-02 | Ingestie FOREXE — **pașii 1–2** și **alegerea unității de către operator** | ÎN LUCRU (transa 02 gata: build 0 erori; Python 346 trecute / 14 sărite, +39; .NET +43 teste noi, toate verzi) / **pașii 3–8 NEPORTAȚI, nimic nu cheamă încă coordonatorul, nimic n-a rulat pe MariaDB, dialogul NEVĂZUT pe ecran** | `SLICE-0048-02-alegerea-unitatii.md` | **Ambiguitatea nu era un defect — era un formular.** Firul (2) al feliei 0048 spunea că, dacă o pereche `(SS, ClsfE)` dezacordă pe `IdUnitate`, D17 se oprește și raportează. Citind sursa: `Obtine_IdUnitate_Din` **deschidea `FX_Unitate` modal și ÎNTREBA operatorul, de fiecare dată**. Deci nu se oprește — se întreabă. **Decizia operatorului:** se întreabă mereu, dar dialogul are bifa «Nu mă mai întreba pentru această combinație»; bifată, răspunsul se reține **per pereche `(SS, ClsfE)`**, iar o pereche NOUĂ se întreabă din nou. **Livrat — server:** `routes/forexe/prelucrare.py` (`POST /api/forexe/prelucrare`, `@require_session`, baza din sesiune, o singură tranzacție) cu **pasul 1** (`Prelucrare_Angajament`, port rând cu rând: un câmp gol nu șterge ce e deja acolo; `DC`/`Preluat` doar la insert) și **pasul 2** (`Prelucrare_Indicatori`, `NrCrt` = contorul LOCAL din VBA, D18 respectat). `routes/forexe/prelucrare_unitate.py` traduce interogarea Access: `ClasificatiiG` ▸ `Clasificatii`, `INNER JOIN Cai ... Cai.DC = DC()` ▸ `INNER JOIN Unitati` (filtrul pe bază e implicit), `Cai.AlteDetalii` ▸ **`Unitati.Detalii`** (`AlteDetalii` nu s-a migrat) — **niciun `LIMIT 1`**, fiindcă aici alegerea greșită atașează un indicator la altă subunitate. **TOATE** perechile se rezolvă înainte de orice scriere și **toate** întrebările pleacă odată: un singur drum înapoi per angajament. **409 cu `reason = ALEGERE_UNITATE`** (nu 400 — cererea nu e greșită; serverului îi lipsește o informație pe care doar un om o are), cu `alegeri_necesare` purtând angajamentul, indicatorii, clasificația și **numele** unităților candidate; **tranzacția e derulată înapoi întâi**, deci nici angajamentul scris la pasul 1 nu rămâne. **Memoria:** `sql/0048_alegeri_unitate.sql` ▸ `FX_Alegeri_Unitate` (unic pe `(SS, ClsfE)`, cu **`UN` și `DataAlegere`** — o alegere greșită trebuie să poată fi găsită), se aplică **și pe `AVACONT_SURSA`** (spre deosebire de cele șapte `ALTER` din 0048-01: acolo scutirea păzea migrarea, aici e un tabel nou și gol). Trei reguli: alegerea se scrie **ÎN** tranzacție (pică rularea, pică și alegerea); tabela lipsă e **zgomotoasă** și numește fișierul `.sql`, dar se atinge DOAR pe ramura ambiguă, deci o bază fără ambiguități merge fără ea; o alegere reținută care nu mai e printre candidați **nu se folosește** — se avertizează și se întreabă din nou. **Livrat — client:** POCO-uri în `KBot.Domain` (+ **`PrelucrareRezultat` MUTAT** din `KBot.App`, fiindcă `KBot.Api` are nevoie de el și nu poate referi `KBot.App`); `ApiClient.TrimitePrelucrareAsync` unde 409-ul **nu aruncă**, se întoarce ca stare (tiparul `PdfDownloadStatus.NotFound`); `AlegereUnitateForm` (`KBotThemedForm`, tot în `.Designer.vb`, `KBotDataView`/`KBotNotice`/`KBotCaptionBar`/`KBotToolTip`, română cu diacritice literale); `PrelucrareCoordinator` cu întrebătorul **injectabil**, altfel bucla nu s-ar putea testa fără a deschide ferestre. Plus `KBotNotice.Message` (`Visible` urcă prin părinți, deci minte pe un formular neafișat). **⚠ De decis (portat fidel, contraintuitiv):** ramura de update din pasul 2 rescrie DOAR cele patru coloane de bani — **nu** `IdUnitate`/`IdClsf`/`SS`/`NrCrt` —, deci pe un indicator care există deja **alegerea operatorului nu ajunge în rând**. Access făcea exact la fel: întreba prin `FX_Unitate` și apoi arunca răspunsul. **⚠ `FX_Unitate` NU există în `FX_System_Export`** — aspectul dialogului e **proiectat, nu portat**. **⚠ Șapte teste roșii care NU sunt din felia asta**, toate cu o singură cauză: `DdfInfo.vb:130` are prefixul cu numărul reviziei comentat (`Return $"{data}" '$"{numar} - {data}"`), iar testele care îl pinuiau au rămas roșii în `KBot.Domain.Tests` (3), `KBot.Api.Tests` (1) și `KBot.App.Tests` (1). Arată a schimbare voită de produs cu teste nereînnoite — e o decizie a operatorului, nu s-a atins. |
| 0048-03 | Ingestie FOREXE — **pașii 3–8** și **contractul în două faze** | ÎN LUCRU (transa 03 gata: build 0 erori; Python 410 trecute / 15 sărite, +64; .NET +17 teste noi, toate verzi, 0 regresii) / **nimic n-a rulat pe MariaDB, `sql/0049` neaplicat, formularul NU există, nimic nu cheamă coordonatorul** | `SLICE-0048-03-pasii-3-8-si-contractul-in-doua-faze.md` | Contractul inițial și probele: [0048-03](../SLICE-0048-03-pasii-3-8-si-contractul-in-doua-faze.md). Evoluția adoptată: [completare](../SLICE-0048-03-completare.md) (pasul 8 activ, F20 retrasă, structură imbricată păstrată); F13 este retras ulterior în [0048-07](../SLICE-0048-05-graficul-evolutiei.md#slice-0048-07). Starea din coloana anterioară descrie numai momentul tranșei inițiale. |
| 0048-03-c | Ingestie FOREXE — **pasul 8**, **structura care călătorește**, **căile operatorului** | ÎN LUCRU (transa de completare gata: build 0 erori / 0 avertismente noi; Python 429 trecute / 15 sărite, `+19`; .NET `+27` teste noi, toate verzi, 0 regresii) / **nimic n-a rulat pe MariaDB și nu s-a încercat, `sql/0049` neaplicat, nimic nu s-a văzut pe ecran** | `SLICE-0048-03-completare.md` | Corecțiile F17/F20, D-N/D-O, F28, pasul 8, schema și căile operatorului: [completare 0048-03](../SLICE-0048-03-completare.md). Explicațiile și lista fișierelor se întrețin numai în jurnal; probele și limitele istorice rămân distincte. |
| 0048-04 | Ingestie FOREXE — **editorul de legături R ▸ H, disponibil ORICÂND** + regula de blocare | ÎN LUCRU (cod verde: build 0 erori / 0 avertismente; Python 460 trecute / 15 sărite, `+31`; .NET `+30` teste noi, toate verzi, 0 regresii) / **nimic n-a rulat pe MariaDB, `sql/0049` tot neaplicat, formularul NEVĂZUT pe ecran și nedeschis în designerul VS** | `SLICE-0048-04-asociere-oricand.md` | **Cerut de operator, 29.08.2026: legăturile trebuie corectate CÂND SE OBSERVĂ greșeala, nu doar în minutul de după o descărcare.** Formularul nu exista — nu e o regresie, e chiar împărțirea D-A a feliei 0048 (0048-03 = conducta, 0048-04 = tragerea + formularul + legarea). **⚠ ACCESS AVEA EXACT GAZDA ASTA, ȘI LIPSEȘTE DIN EXPORT.** `frmFX_DUBII_LISTA_HA.Form_Open` și `..._RH.Form_Open` se ramifică pe `isLoaded("frmFX_ASOC")`, deci Access rula DOUĂ gazde peste aceleași patru subformulare — `frmFX_DUBII` în ingestie, `frmFX_ASOC` oricând — dar `frmFX_ASOC`, `frmFX_ASOC_SUB` și `frmFX_DUBII_P_LISTA_R` NU sunt în `FX_System_Export/FORMS`. Subformularele SUNT, și ele poartă regulile; **aspectul e PROIECTAT, nu portat**, ca la `AlegereUnitateForm` în 0048-02. **⚠ REGULA DE BLOCARE ERA ÎN ACCESS, COMENTATĂ, PE UN TABEL MORT:** `btnDel_Click` avea `DCount("IDRP", "FX_Receptii_Plati", "IDRH=" & origidrh)` — cheiată pe **INSTANTANEU**, nu pe recepție — prin tabelul pe care corecția C2 îl declară GOL și scos din migrare. Regula supraviețuiește, sursa de date nu. **Re-cheiată pe tabelele vii, verificate în `000_DEMO.sql`:** `FX_ORD.IDRR`/`IDRH` există (nullable, «PK ACCESS»), iar `FX_Receptii_R.IDRR`/`FX_Receptii_H.IDRH` sunt chei primare Access păstrate, deci se leagă direct; plățile consumate vin prin `FX_ORD → FX_ORD_TBL → FX_ORD_TBL_REC → FX_Plati`, ambele salturi fiind FK reale. **⚠ `FX_ORD_TBL.IDRR` NU EXISTĂ pe MariaDB** (Access îl are, nu s-a migrat), deci legătura ordonanțare ▸ recepție trăiește doar la nivel de CAP. **⚠ În export TOATE rândurile `FX_ORD` poartă `IDRR = 0, IDRH = 0`**, deci pe date de vechimea aia jumătatea «ordonanțare» nu găsește nimic și tot blocajul stă pe jumătatea «plăți» — de-aia s-a ÎNTREBAT operatorul în loc să se ghicească. **Deciziile lui:** fereastra = **orice plată cu `Data_plata >= DataH`** (motivul e §1.3 din fundament: fiecare plată de după acea dată citește totalul recepției așa cum stătea atunci); granularitatea = **doar instantaneul atins**, restul lanțului rămâne editabil. **ASIMETRIE DELIBERATĂ:** blocajul păzește EDITAREA unei legături existente, NU atașarea uneia neașezate — la fel ca Access, unde verificarea trăia doar în `btnDel_Click`. Altfel, sub F10, formularul de ingestie s-ar bloca în prima zi, fiindcă fiecare instantaneu istoric are plăți după el. **F15 coboară de la veto la SEMN, dar NUMAI în editor** (`f15_ca_avertisment`, implicit False = calea de ingestie neatinsă): se DESPRIND legături, iar desprinderea ultimului instantaneu lasă prin definiție un lanț deschis — un veto acolo ar face imposibil tocmai lucrul pentru care există editorul, iar fundamentul §1.5 chiar așa îl descrie. F13/F14/F16 rămân vetouri pe amândouă căile. **Livrat — server:** `routes/forexe/asociere.py` (`GET`/`POST /api/forexe/asociere`), cu ancora pe **`IDRH`**, nu pe `rand_istoric` (F24 nu se aplică: nu există sarcină utilă și nu se derulează nimic înapoi), **fără obligația de acoperire** (aici tăcerea înseamnă «las-o cum e», un răspuns adevărat), a cincea acțiune **`desprins`** care există DOAR aici, și codul-motiv nou **409 `INSTANTANEU_BLOCAT`** verificat pe SERVER chiar dacă formularul știe deja blocajele. Șapte funcții refolosite NESCHIMBATE din `prelucrare_asociere.py` prin aliasul `rand_istoric = idrh`. Lanțurile rezultate se construiesc în memorie și se validează ÎNAINTE de scriere — și se validează **doar cele schimbate**: o încălcare veche, deja în bază, nu are voie să blocheze o corecție fără legătură cu ea. **Livrat — control:** `AdvancedTreeControl.Drag.vb` (D-K), implicit STINS, cu `Allow` implicit **False** (refuzul e implicitul: o gazdă care uită să răspundă nu lasă să treacă nimic). Nodul tras se ia din **obiectul de date**, nu din câmpul propriu — la tragerea între doi arbori ținta n-a pornit tragerea și `_dragSource` e gol la ea. Controlul **NU mută nodul**: arborele e o proiecție a datelor, iar o frunză mutată local ar arăta o legătură pe care nimeni n-a scris-o. **Livrat — client:** `AsociereForm` (cele patru panouri Access devin trei zone, fiindcă mutarea se face trăgând), iconița din **antetul** arborelui de Recepții (subsolul avea deja reîncărcarea și strângerea), o singură salvare la sfârșit (D-H) care trimite DOAR ce diferă de ce s-a citit. **⚠ Cele 17 teste roșii sunt PREEXISTENTE** — verificat rulând suitele pe arborele curățat înainte și după; `KBOT_STATUS` spunea «șapte», sunt **șaptesprezece**. |
| 0048-05 | Ingestie FOREXE — **mesajul care nu se stingea** + **graficul evoluției** (`KBotChartView`) | ÎN LUCRU (cod verde: `KBot.Controls` build 0 erori / 0 avertismente, `KBot.App` 0 erori; `.NET +16` teste noi, toate verzi) / **`AsociereForm` tot NEVĂZUT pe ecran și nedeschis în designerul VS; controlul ÎNSUȘI a fost desenat și privit pe toate trei schemele** | [0048-05](../SLICE-0048-05-graficul-evolutiei.md#slice-0048-05) | Detaliile, motivele, versiunile, fișierele și probele sunt în secțiunea omonimă a jurnalului consolidat (D001-04). Starea și limitele din acest rând sunt istorice. |
| 0048-06 | Ingestie FOREXE — **setul de culori al graficului** + **clic pe punct ▸ rândul din arbore** | ÎN LUCRU (cod verde: `KBot.Controls` build 0 erori / 0 avertismente, `KBot.App` 0 erori, soluția întreagă 0 erori; `KBot.Controls.Tests` **953/953, ZERO roșu** — roșul străin care a însoțit 0048-05 s-a închis aici) / **`AsociereForm` tot NEVĂZUT pe ecran și nedeschis în designerul VS** | [0048-06](../SLICE-0048-05-graficul-evolutiei.md#slice-0048-06) | Detaliile, motivele, versiunile, fișierele și probele sunt în secțiunea omonimă a jurnalului consolidat (D001-04). Starea și limitele din acest rând sunt istorice. |
| 0048-07 | Ingestie FOREXE — **benzile de așezare** (`KBotLaneView`, familie nouă de control), **liniile în trepte**, **reperele plăților** și **retragerea lui F13** | ÎN LUCRU (cod verde: soluția întreagă build 0 erori — `KBot.Controls` 0 erori / 0 avertismente, iar cele 6 `MSB3825` de pe `AsociereForm.resx` sunt PREEXISTENTE; `KBot.Controls.Tests` **953/953**, exact linia de plecare; `KBot.App.Tests` 185/198 cu **13 roșii PREEXISTENTE și străine** — linie luată prin `git stash`, nu din memorie; Python **465 trecute / 15 sărite**) / **NIMIC nu s-a văzut pe ecran: `KBotLaneView` și `AsociereBenziForm` nu au fost deschise nici măcar în designerul Visual Studio** / **nimic nu a rulat pe MariaDB** / **fără git: nimic comis, nimic împins** | [0048-07](../SLICE-0048-05-graficul-evolutiei.md#slice-0048-07) | Detaliile, motivele, versiunile, fișierele și probele sunt în secțiunea omonimă a jurnalului consolidat (D001-04). Starea și limitele din acest rând sunt istorice. |
| 0048-08 | Ingestie FOREXE — **benzile citibile** (bucăți colorate, loc în dreapta, fără sume), **eticheta care rămânea în urmă**, **culorile prea deschise** și **cele două vederi din dreapta sus** | ÎN LUCRU (cod verde: `dotnet build KBot.sln` **0 erori**, singurele avertismente fiind cele **6 `MSB3825` PREEXISTENTE**; `KBot.Controls.Tests` **953/953**, exact linia de plecare; `KBot.App.Tests` 185/198 cu **aceleași 13 roșii PREEXISTENTE și străine**; Python neatins) / **S-A VĂZUT PE ECRAN** — prima felie din familia 0048 despre care se poate spune asta: `KBotLaneView` randat cu `DrawToBitmap` pe Classic / Modern / Dark, iar `AsociereForm` randat întreg în amândouă vederile; sondele au fost **temporare și șterse** / **nimic nu a rulat pe MariaDB**, nicio rută Python atinsă | [0048-08](../SLICE-0048-05-graficul-evolutiei.md#slice-0048-08) | Detaliile, motivele, versiunile, fișierele și probele sunt în secțiunea omonimă a jurnalului consolidat (D001-04). Starea și limitele din acest rând sunt istorice. |
| 0048-09 | Ingestie FOREXE — **totalul recepțiilor scris chiar pe reperul plății** (socoteala §1.3 în etichetă) | ÎN LUCRU (cod verde: `dotnet build KBot.sln` **0 erori / 0 avertismente**; `KBot.App.Tests` **188/201**, cu **aceleași 13 roșii PREEXISTENTE și străine** — linia de plecare era 185/198, deci cele **3 teste noi trec toate**; `KBot.Controls` neatins; Python neatins) / **eticheta s-a văzut randată** prin chiar motorul ei (`KBotRichText` Parse ▸ Layout ▸ Draw, sondă temporară ștearsă), dar **NU ridicată de un mouse adevărat** peste o bandă adevărată / **nimic nu a rulat pe MariaDB** | [0048-09](../SLICE-0048-05-graficul-evolutiei.md#slice-0048-09) | Detaliile, motivele, versiunile, fișierele și probele sunt în secțiunea omonimă a jurnalului consolidat (D001-04). Starea și limitele din acest rând sunt istorice. |

### Current focus

Familia vizuală este consolidată în [0048-05…09](../SLICE-0048-05-graficul-evolutiei.md): [0048-07](../SLICE-0048-05-graficul-evolutiei.md#slice-0048-07) păstrează retragerea F13 pe ambele căi și hazardul F30; [0048-08](../SLICE-0048-05-graficul-evolutiei.md#slice-0048-08) păstrează lecțiile despre conținut mutabil, contrast și randarea controlului; [0048-09](../SLICE-0048-05-graficul-evolutiei.md#slice-0048-09) păstrează calculul unic/recalculat al totalului plății și colorarea numai a diferenței negative. Explicațiile și probele se întrețin numai acolo.

### Open threads

- **Felia 0048-04 — ce a rămas deschis după editorul de legături.** **(1)** ⚠⚠ **Formularul nu a
  fost văzut pe ecran și nici deschis în designerul Visual Studio.** Aspectul e PROIECTAT, nu
  portat — gazda Access `frmFX_ASOC` lipsește din export — deci nu există nimic cu care să fie
  comparat, și prima privire pe ecran e obligatorie. **(2)** ⚠⚠ **`_BLOCAJE_SQL` nu a rulat
  niciodată**: `GROUP_CONCAT` într-o subinterogare corelată, trei `EXISTS`-uri corelate și un
  `MIN(DataORD)`, verificate doar prin citire. **(3)** ⚠ **Jumătatea «ordonanțare» a blocajului nu
  a găsit niciodată nimic**, fiindcă în exportul Access `FX_ORD.IDRR`/`IDRH` sunt 0 peste tot; pe
  date scrise de `mdl_FX_ORD_Salvare` v5/v6 ar trebui completate, dar nu s-a putut proba.
  **18.09.2026 — regula ÎNGUSTATĂ de operator:** legăturile se editează cât timp **nu există
  ORDONANȚARE**; o plată singură **nu mai blochează** nimic. Jumătatea «plăți» a fost scoasă din
  `_BLOCAJE_SQL` și din `motive_blocare` (`routes/forexe/asociere.py`), testele rescrise
  (`test_platile_nu_mai_blocheaza`), mesajul din `AsociereForm` pe 409 `INSTANTANEU_BLOCAT` nu mai
  pomenește plata. Consecință: pe date vechi, unde `FX_ORD.IDRR`/`IDRH` sunt 0 peste tot, **nimic nu
  mai e blocat** — jumătatea rămasă e chiar cea neprobată de la (3). Nerulat.
  **20.09.2026 — confirmat de operator și CORECTAT:** exact asta se întâmpla («it's not working»);
  nimeni nu scrie `FX_ORD.IDRR` / `IDRH` (nici `ord_edit.py`). Regula lui: **cel puțin o linie
  `FX_ORD_TBL` a angajamentului cu data (a capului `FX_ORD.DataORD`) în ziua instantaneului sau
  după** — `_BLOCAJE_SQL` rescris pe `FX_ORD_TBL ⋈ FX_ORD` prin `IDORDP` + `FX_ORD.CodAngajament`,
  `DATE(DataORD) >= DATE(DataH)`; `motive_blocare` cu un singur alineat (`ord_n`/`ord_nr`/`ord_data`);
  teste rescrise; comentariul de antet din `AsociereForm.vb` adus la zi. Detalii în
  `SLICE-0048-04-asociere-oricand.md` §7. SQL-ul tot nu a atins MariaDB. **(4)**
  ⚠ **Tragerea nu s-a făcut cu un mouse adevărat** — `DoDragDrop` intră într-o buclă modală care nu
  se poate porni fără dispozitiv, deci testele acoperă pragul dinainte, contractul evenimentelor și
  proiecția locală, **nu și bucla însăși**. **(5)** **Reconstituirea (F26 / D-M) e susținută de
  server, dar nu are intrare în meniul formularului** — `ACTIUNE_RECONSTITUIRE` trece prin toată
  calea; lipsește doar gestul din interfață. **(6)** **`treeLibere` nu grupează pe lună**; pe un
  angajament cu multe instantanee neașezate lista va fi lungă. Amânat: `O4` (aspectul formularului)
  nu era specificat. **(7)** **Formularul nu folosește `AsociereStore`** — dosarul local există
  pentru contractul în două faze, unde sarcina utilă nu se mai poate reface; aici totul se recitește
  de la server oricând. De rediscutat dacă operatorul lucrează sesiuni lungi. **(8)** **Testele
  roșii sunt 17, nu 7** cum spunea acest fișier: 1 în `KBot.Api.Tests`, 13 în `KBot.App.Tests`, 3 în
  `KBot.Domain.Tests` — grupul `DdfInfo.vb:130` plus Ddf/Istoric/Xfa. Niciunul nu vine din felia
  asta (verificat prin `git stash`), dar cifra din STATUS era greșită.

- **0048-03-c — limitele tranșei:** [completare, §4–5](../SLICE-0048-03-completare.md#4-rămas-neverificat). SQL/0049 cu patru coloane era neaplicat; citirea Recepțiilor îl cere și ea. Ordinea VB, ecranul și calea reală settings.json nu fuseseră probate; formularul de setări rămânea de construit. F20/O3 sunt retrase, iar planul absent nu se mai caută.

- **0048-03 — limitele tranșei inițiale:** [jurnal, §4–5](../SLICE-0048-03-pasii-3-8-si-contractul-in-doua-faze.md#4-rămas-neverificat-sau-amânat). MariaDB, testul de identitate a celor nouă tabele, SQL/0049, cele șapte AUTO_INCREMENT și rata hash rămâneau neprobate. F27 este limita grupării imposibil de demonstrat, iar Val_Receptie_T rămâne nescrisă. Restanțele vechi F20 și Detaliu în două forme au fost închise prin completare; nu mai constituie instrucțiuni active.

- **Felia 0048-02 — ce e de privit după pașii 1–2 și alegerea unității.** **(1)** ⚠⚠
  **`sql/0048_alegeri_unitate.sql` nu a rulat niciodată** — nici pe o bază de unitate, nici pe
  `AVACONT_SURSA`. Fișierul poartă proba de rulat înainte (tipul lui `Unitati.IdUnitate` e luat
  din dump-ul din 22.08, neconfirmat pe serverul viu). Până rulează, prima ambiguitate reală
  întoarce 500 cu mesajul care numește fișierul — zgomotos, cum s-a vrut, dar de știut.
  **(2)** ⚠ **De decis: ramura de update din pasul 2 aruncă alegerea operatorului.** VBA-ul
  (`RC.Edit`) rescrie DOAR cele patru coloane de bani, nu `IdUnitate`/`IdClsf`/`SS`/`NrCrt`, deci
  pe un indicator care există deja unitatea aleasă nu ajunge în rând — se aplică abia la un
  insert. Access făcea identic (întreba prin `FX_Unitate`, apoi arunca răspunsul). S-a portat
  fidel și e comentat la locul faptei; dacă operatorul vrea altfel, e o linie, dar e o **abatere
  de la Access** și trebuie hotărâtă. **(3)** **Cât de des va fi întrebat operatorul, nu se
  știe.** Testul-constatare §10.7 din plan tot trebuie rulat pe baza vie: pe singurele date de pe
  disc (2 unități din 13, ~101 rânduri) **zero** perechi traversează o unitate, dar `ClsfE`
  (6 cifre) e mai grosier decât `ClsfSal` (12). Ambiguitatea nu mai blochează nimic — are un
  răspuns —, dar frecvența ei decide dacă bifa «nu mă mai întreba» e o comoditate sau o
  necesitate. **(4)** **Dialogul `AlegereUnitateForm` NU s-a văzut pe ecran** și **nimic nu
  cheamă încă `PrelucrareCoordinator`** — legarea în `ForexeController.DownloadNodeAsync` (§9 din
  plan) e amânată deliberat până sosesc pașii 3–8, fiindcă a posta automat o conductă pe jumătate
  la fiecare descărcare ar schimba în tăcere ce face sistemul. `KBot.DevHarness` nu poate ține un
  declanșator: nu referă `KBot.App` (ar fi ciclu). **(5)** **`FX_Unitate` nu există în
  `FX_System_Export`** — aspectul dialogului e proiectat, nu portat; dacă formularul apare
  vreodată, de comparat. **(6)** ⚠ **Șapte teste roșii care NU sunt din felia asta**, o singură
  cauză: `src/KBot.Domain/DdfInfo.vb:130` are prefixul cu numărul reviziei comentat
  (`Return $"{data}" '$"{numar} - {data}"`), iar `numar` a rămas variabilă nefolosită. Roșii:
  `KBot.Domain.Tests.DdfInfoTests.EtichetaRevizie_*` (3),
  `KBot.Api.Tests.ApiClientTests.GetDdf_FormatsRevisionLabel_*` (1),
  `KBot.App.Tests.DdfViewTests.Leaf_CaptionPadsRevisionNumberWithSpaces` (1). Arată a schimbare
  voită de produs cu teste nereînnoite: ori eticheta e doar data și testele se schimbă, ori se
  scoate comentariul. **Decizie de operator; nu s-a atins în felia asta.**

- **Felia 0048-01 — premisele; ruta există acum, dar doar pașii 1–2 (vezi 0048-02).** Ce trebuie
  privit înainte de a continua: **(1)** ⚠⚠ **Pasul AUTO_INCREMENT nu a rulat niciodată.** Până
  când cele șapte `ALTER` nu trec pe baza de test, ingestia **nu poate insera** — planul
  interzice explicit o rezervă MAX+1 în Python (o rezervă care există va fi folosită, și atunci
  două căi de alocare a cheilor trebuie ținute corecte pe veci). Dacă ruta trebuie exercitată
  mai devreme, cele șapte se aplică de mână și se scrie în worklog. La prima rulare de citit,
  per §3.3 al planului: dacă InnoDB chiar a pornit contorul de la `MAX(cheie) + 1` — pasul
  raportează exact rânduri / `MAX` înainte / `AUTO_INCREMENT` după, per tabel. **(2)**
  **Invariantul `(SS, ClsfE)` e măsurat pe prea puțin.** Pe singurele date de pe disc (jurnalul
  `000_DEMO/20260824_092427`, 101 rânduri, unitățile 75 și 76) **zero** perechi traversează o
  unitate, nici pe `ClsfE`, nici pe `ClsfSal`. Dar sunt 2 unități din 13 și ~101 rânduri dintr-un
  tabel cu `AUTO_INCREMENT` 1497, iar `ClsfE` (6 cifre) e mai grosier decât `ClsfSal` (12), deci
  **coliziunile sunt mai probabile, nu mai puțin**. Access-ului i se părea normal: ambiguitatea
  deschidea formularul `FX_Unitate` ca să întrebe operatorul. Testul-constatare §10.7 se scrie pe
  baza vie. **⚠ SUPERSEDAT de felia 0048-02:** aici scria că «D17 se oprește și se raportează»,
  dar citirea sursei a arătat că Access-ul NU se oprea — deschidea `FX_Unitate` și întreba.
  Așa face și K-BOT acum (409 + dialog). Ce rămâne de măsurat e **cât de des** se întâmplă,
  fiindcă asta decide dacă bifa «nu mă mai întreba» e comoditate sau necesitate. **(3)** **Rata de potrivire a hash-ului (§8) nu s-a măsurat** — are nevoie de baza vie.
  Nu blochează nimic (D9 dedupează pe cheia naturală), dar e singurul mod în care cineva va afla
  vreodată ce codificare folosea Access-ul; jumătatea «majuscule» e deja închisă. **(4)**
  `ParseDataZZLLAAAA` **nu are definiție în `FX_System_Export`** — contractul e dedus din cele
  două locuri de apel. Dacă modulul apare, de comparat cu `parse_data_zzllaaaa`. **(5)**
  `KBot.Migrator` **nu are proiect de teste**, deci lista celor șapte perechi din VB e ținută în
  frâu doar de `TestTheVbListAgrees`, care citește fișierul `.vb` din testul Python. Dacă apare
  vreodată un `KBot.Migrator.Tests`, pinul se mută acolo.

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
