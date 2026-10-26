# K-BOT — STATUS, slices 0070–0079

Moved verbatim out of `../KBOT_STATUS.md` (28.09.2026). The index there says what
each slice is; this file holds everything recorded about it: its registry row, its
«Current focus» notes and its «Open threads» notes.

---

## Slice 0070

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0070 | Browser numai andocat și corecția tastaturii | GATA pe cod (`dotnet build` pe `src\KBot.Forexe`, `src\KBot.App`, `src\KBot.DevHarness`: **0 erori, 0 avertismente** în afara `MSB3825` preexistent) / **NIMIC RULAT, NIMIC PE ECRAN, NICIO SUITĂ, NIMIC COMIS** (cerere explicită: «no git. no tests!») | [0070](../SLICE-0053-recorder.md#slice-0070) | Detalii, revizii, fișiere, probe și limite în etapa legată. |

### Current focus

- [Detalii și restanțe 0070](../SLICE-0053-recorder.md#slice-0070); starea istorică a probei rămâne în registru.

---

## Slice 0071

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0071 | Consolă Release, verbozitate și corecția handle-ului | GATA pe cod (`dotnet build` pe `src\KBot.Forexe` Debug+Release, `src\KBot.Forexe.Editor`, `src\KBot.DevHarness`: **0 erori, 0 avertismente**; `KBot.App` compilează, copierea DLL-urilor blocată de aplicația care rula) / **NIMIC PE ECRAN, NICIO SUITĂ, NIMIC COMIS** (cerere explicită: «no tests. no git») | [0071](../SLICE-0071-consola-forexe-fara-zgomot.md#slice-0071) | Detalii, revizii, fișiere, probe și limite în etapa legată. |

### Current focus

- [Detalii și restanțe 0071](../SLICE-0071-consola-forexe-fara-zgomot.md#slice-0071); starea istorică a probei rămâne în registru.

---

## Slice 0072

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0072 | Fereastra Setări — pagini, comutatoare și parola în doi pași | GATA pe cod (`dotnet build` pe `src\KBot.App` Debug+Release, `src\KBot.DevHarness`, `src\KBot.Forexe.Editor`, `src\KBot.Migrator`: **0 erori, 0 avertismente**; Python `ast.parse` verde) / **văzută doar prin `DrawToBitmap` la 150 %, în afara monitorului** (toate cinci paginile, trei scheme); **serverul neatins**: `auth.py` + `config.py` sunt gitignored, rutele noi + `mailer.py` + contul SMTP trebuie duse pe VPS de mână; `SET PASSWORD` nerulat pe MariaDB; nimic comis | [0072](../SLICE-0072-fereastra-setari.md#slice-0072-baza) | Contracte, fișiere, probe și restanțe în jurnalul consolidat D001-16; Status păstrează checkpointul istoric. |
| 0072-01 | Jurnal, dialog Adobe și meniuri/cursor cu opriri | GATA pe cod (`dotnet build` pe `src\KBot.Controls`, `src\KBot.App` Debug+Release, `src\KBot.Migrator`, `src\KBot.Forexe.Editor`: **0 erori, 0 avertismente**) / **văzut doar prin `DrawToBitmap`** (pagina «Jurnal» în `LogViewerForm`, dialogul Adobe, «Temă», «Aplicație», meniul de temă cu liniuțele de oprire — trei scheme); testele scrise, **nerulate**; comis | [0072-01](../SLICE-0072-fereastra-setari.md#slice-0072-01) | Contracte, fișiere, probe și restanțe în jurnalul consolidat D001-16; Status păstrează checkpointul istoric. |
| 0072-02 | Opțiuni avansate — pagini ascunse și parola de activare | GATA pe cod (`dotnet build srcKBot.App`: **0 erori, 0 avertismente**) / nimic văzut pe ecran | [0072-02](../SLICE-0072-fereastra-setari.md#slice-0072-02) | Contracte, fișiere, probe și restanțe în jurnalul consolidat D001-16; Status păstrează checkpointul istoric. |
| 0072-03 | Corecția constructorului paginii Setări FOREXE | GATA pe cod (`dotnet build src\KBot.App`: nicio eroare de compilare; copierea DLL-urilor a eșuat doar fiindcă aplicația rula sub depanator) / nimic văzut pe ecran | [0072-03](../SLICE-0072-fereastra-setari.md#slice-0072-03) | Contracte, fișiere, probe și restanțe în jurnalul consolidat D001-16; Status păstrează checkpointul istoric. |

### Current focus

[Familia Setări](../SLICE-0072-fereastra-setari.md) păstrează baza și 01…03. Randările statice, testele nerulate și corecția FOREXE nereprobată sunt distincte; acceptarea manuală și integrarea serverului rămân la etapele lor.

### Open threads

Jurnalul detaliat este rezolvat în 0072 și nu schimbă fișierul-jurnal/istoricul. Publicarea SMTP/rutelor și proba parolei pe cele două servere sunt [restanțele bazei](../SLICE-0072-fereastra-setari.md#slice-0072-baza), fără o copie nouă a pașilor.

---

## Slice 0073

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0073 | Meniu în pagină, urmărire și stiluri timpurii | GATA pe cod (`dotnet build` pe `srcKBot.Forexe`, `srcKBot.App` `--no-incremental`, `srcKBot.DevHarness`: **0 erori, 0 avertismente**; `node --check` valid) / **văzut pe ecran** prin `DrawToBitmap`: recorderul în ambele moduri / **NIMIC nu a atins un browser adevărat sau CABWeb; nicio suită rulată** | [0073](../SLICE-0073-recorder-meniu-plutitor-urmarire.md#slice-0073) + [0073-01](../SLICE-0073-recorder-meniu-plutitor-urmarire.md#slice-0073-01) | Detalii, revizii, fișiere, probe și limite în etapa legată. |

### Current focus

- [Detalii și restanțe 0073](../SLICE-0073-recorder-meniu-plutitor-urmarire.md#slice-0073); starea istorică a probei rămâne în registru.

---

## Slice 0074

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0074 | Vederea browserului și selecția în ambele sensuri | GATA pe cod (`dotnet build` pe `src\KBot.Forexe`, `src\KBot.App` `--no-incremental`, `src\KBot.DevHarness`: **0 erori, 0 avertismente**; `node --check` valid) / **văzut pe ecran** prin `DrawToBitmap`: vederea neconectată și conectată-cu-andocare-refuzată / **NIMIC nu a atins un browser adevărat sau CABWeb; nicio suită rulată** | [0074](../SLICE-0053-recorder.md#slice-0074) | Detalii, revizii, fișiere, probe și limite în etapa legată. |

### Current focus

- [Detalii și restanțe 0074](../SLICE-0053-recorder.md#slice-0074); starea istorică a probei rămâne în registru.

---

## Slice 0075

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0075-05 | Pagina operatorului și aprobarea | GATA pe cod / **verificat în browser pe un ciot** care montează blueprint-ul și pagina adevărate, cu baza, e-mailul și jobul mimate — **nimic viu**. ⚠ **Zero teste** (decizia operatorului) | [0075-05](../SLICE-0075-01-inregistrare-preauth-anaf-cod.md#slice-0075-05) | Contractele și reviziile sunt în jurnal; reperele unice ale registrului sunt transferate la sfârșitul lui. |
| 0075-03 | Provizionare, compensări și link de parolă | GATA pe cod / **jobul n-a rulat niciodată** — niciun pas și nicio anulare n-au atins un MariaDB. Verificat fără server: `ast.parse`, importurile, Regula 0 prin `tokenize`, constructorul de rânduri prin apel direct (capitolul pentru `01A`/`02E`/`03A`/`02C`, refuzul care numește toate codurile rele, `CodProgram` D25), pagina `/parola` în browser pe un ciot (toate cele cinci căi, token-ul niciodată într-un `GET`, telefonul la 375 px). ⚠ **Zero teste** (decizia operatorului) | [0075-03](../SLICE-0075-01-inregistrare-preauth-anaf-cod.md#slice-0075-03) | Contractele și reviziile sunt în jurnal; reperele unice ale registrului sunt transferate la sfârșitul lui. |
| 0075-04 | Pagina solicitantului | GATA pe cod / **verificat în browser pe un ciot** din biblioteca standard care servește fișierele adevărate și mimează cele șapte rute — **nimic viu: nici Flask, nici MariaDB, nici ANAF**. Exersate: toate cele șase ecrane, refuzurile fiecăruia, expirarea, corpul trimis la `/cerere` (numai frunze) și telefonul la 375px. ⚠ **Zero teste** (decizia operatorului) | [0075-04](../SLICE-0075-01-inregistrare-preauth-anaf-cod.md#slice-0075-04) | Contractele și reviziile sunt în jurnal; reperele unice ale registrului sunt transferate la sfârșitul lui. |
| 0075-02 | Nomenclatoare, nume și cerere | GATA pe cod / **NIMIC nu a fost pornit** — nicio rută n-a atins un Flask viu, un MariaDB sau ANAF. `ast.parse` curat (5/5); **Regula 0 verificată prin AST** (intervalele literalilor de tip șir adunate din toate cele șapte fișiere ale feliei, căutare de diacritice în afara lor ▸ **zero**); algoritmul numelui verificat prin apel direct. ⚠ **Zero teste** (decizia operatorului) | [0075-02](../SLICE-0075-01-inregistrare-preauth-anaf-cod.md#slice-0075-02) | Contractele și reviziile sunt în jurnal; reperele unice ale registrului sunt transferate la sfârșitul lui. |
| 0075-01 | Înregistrare, ANAF și dovada e-mailului | GATA pe cod / **NIMIC nu a fost pornit**: nicio rută n-a atins un Flask viu, un MariaDB sau ANAF. `ast.parse` curat (6/6); Regula 0 verificată prin căutare (diacritice **numai** în mesajele românești). ⚠ **Zero teste** — decizia operatorului din 22.09.2026 («no pytest files, i don't use them anyway»); planul §8 a fost corectat ca să nu mai mintă, și pentru trecerile următoare | [0075-01](../SLICE-0075-01-inregistrare-preauth-anaf-cod.md#slice-0075-01) | Contractele și reviziile sunt în jurnal; reperele unice ale registrului sunt transferate la sfârșitul lui. |
| 0075-00 rev.2 | Clasificații — Sector/Sursa/SS scrise | **APLICAT PE SERVER** de operator (22.09.2026): fișierele duse pe VPS, gunicorn repornit, migrarea rulată cu `--clean-leftovers`; `dotnet build src\KBot.Migrator --no-incremental`: **0/0**. ⚠️ Cele trei coloane virtuale de pe `AVACONT_SURSA.Clasificatii` au trebuit **șterse de mână** ca să treacă rularea, motivul nenotat; de lămurit dacă și cele șapte baze de unitate au cerut intervenție. Restul scriptului (copie, instantaneu, verificare) **neexercitat pe un tabel cu rânduri** (șablonul e gol). Prima scriere adevărată într-un `Clasificatii` migrat **nevăzută**; `tests\KBot.Migrator.Tests` **necompilat** | [0075-00 rev.2](../SLICE-0075-00-clasificatii-sursa-scrisa.md) | Contractele și reviziile sunt în jurnal; reperele unice ale registrului sunt transferate la sfârșitul lui. |
| 0075-00 rev.1 | Clasificații — prima variantă, înlocuită de revizia 2 | GATA pe cod (`dotnet build src\KBot.Migrator --no-incremental`: **0 erori, 0 avertismente**; `ast.parse` verde) / **NIMIC nu a atins un MariaDB — nici `--dry-run`**; suitele nescrise pe disc dar **nerulate**; `tests\KBot.Migrator.Tests` (proiect nou) **necompilat** | [0075-00 rev.1](../SLICE-0075-00-clasificatii-sursa-scrisa.md) | Contractele și reviziile sunt în jurnal; reperele unice ale registrului sunt transferate la sfârșitul lui. |

### Current focus

- Reper 22–23.09.2026: [planul](../../PLAN_AutoProvisioning.md#00-start-here--state-on-22092026-end-of-day), [00 — clasificații](../SLICE-0075-00-clasificatii-sursa-scrisa.md) și [01…05 — înregistrare/aprobare](../SLICE-0075-01-inregistrare-preauth-anaf-cod.md). Confirmările operatorului închid publicarea paginii, verificarea 7 și 0075-06, fără detalii individuale despre drepturi; stările inițiale din tabel sunt păstrate ca repere de etapă.

### Open threads

- Limitele și întrebările sunt în fiecare jurnal, inclusiv refuzul inițial al șablonului, probele pe tabele cu rânduri, compensările, configurația/publicarea la reperul fiecărei etape și dependența definerului procedurii de GRANT. Consolidarea nu execută operațiuni pe server.

---
## Slice 0076

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0076 | Editări parțiale, memorie și marcaje de ID | GATA pe cod (`dotnet build` pe `src\KBot.Api`, `src\KBot.Forexe`, `src\KBot.App` și `src\KBot.DevHarness` `--no-incremental`: **0 erori, 0 avertismente**; 11/11 `.wfl` XML valid; `node --check` valid; Python importat) / **NIMIC nu a atins CABWeb, un browser adevărat sau MariaDB; nicio suită construită sau rulată** (cerere: «no TESTS. no GIT») | [0076](../SLICE-0073-recorder-meniu-plutitor-urmarire.md#slice-0076) | Detalii, revizii, fișiere, probe și limite în etapa legată. |

### Current focus

- [Detalii și restanțe 0076](../SLICE-0073-recorder-meniu-plutitor-urmarire.md#slice-0076); starea istorică a probei rămâne în registru.

---

## Slice 0077

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0777 | **Arborele principal: sortare + coloanele CODANGAJAMENT / SURSE; «Aplicație» primește file** — cererea operatorului din 23.09.2026 (numărul 0777 dat de operator). Iconița din dreapta antetului arborelui deschide un `CustomPopup`: «Sortare după nume» / «Sortare după data creării» (fără dată ▸ la urmă, după nume), separator, «Afișare coloana CODANGAJAMENT» / «Afișare coloana SURSE». Coloanele se țin PE SORTARE (implicit: nume = cod da / surse nu; dată = cod nu / surse da). Cinci chei noi în `AppSettings`; shell-ul urmează `AppSettings.Changed` și re-așază arborele din rândurile păstrate. `SetariAplicatieView`: bară orizontală `KBotNavList` sus cu **Generale / Documente / KBOT**. Noi în controale: `CustomPopupItem.Checked`, `AdvancedTreeControl.HeaderRightIconRect`, `AdvancedTreeControl.ColumnHeaderVisible`. A doua trecere: fără banda de titluri de coloană în arbore; lățimile coloanelor (câmpuri pe fila KBOT, 30–600 px). A treia trecere: sortarea după dată ia TOATE sursele anului (`ss=*` pe `/api/forexe/tree` — ⚠ de copiat pe VPS + repornit gunicorn). A patra trecere: un rând per angajament, sursele despărțite prin «,»; lățimi implicite 100 / 70; ordine crescătoare implicit, cu «Crescătoare / Descrescătoare» în Setări. A cincea trecere: sortarea după dată se face după `FX_Istoric.DataFX` (rândul cu `Descriere` = «Angajament nou.», unul per CodAngajament), cu tot cu oră — NU după `FX_Angajamente.DataCreare` | GATA pe cod, build curat (KBot.App, 0 avertismente) / **nimic văzut pe ecran**, ⚠ zero teste (decizia operatorului) | `SLICE-0777-optiuni-arbore-angajamente.md` | Iconițele meniului sunt provizorii (`vertical`, `calendar`, `cells`). Vechiul meniu al lui `btnSort` (implicit / dată asc / dată desc) a dispărut — butonul deschide același meniu nou |
| 0777-02 | **Arborele principal: subliniat pentru ce s-a descărcat în sesiune; fără fereastra de recepții la actualizări multiple; fereastra cozii se închide singură** (cererea operatorului, 01.10.2026) — `TreeItem.Underline`; `_descarcateInSesiune` (memorie, gol la pornire); `_descarcareFaraIntrebare` (nod / recepții puse în coadă peste altă acțiune FOREXE descarcă toate recepțiile); `CloseRobotQueueIfIdle`. Ajutor: `contabil/fereastra.md` | GATA pe cod, build curat / nimic văzut pe ecran, zero teste | `SLICE-0777-02-arbore-subliniat-coada.md` | Un nod care a afișat deja fereastra de alegere nu se schimbă |

---

## Slice 0078

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0078 | Semnarea DDF / ORD în Adobe (absoarbe 0021) | GATA pe cod (`dotnet build src\KBot.App --no-incremental`: **0 erori, 0 avertismente**; `ast.parse` verde) / **nimic văzut pe ecran, capcana n-a întâlnit un dialog Adobe real, DDL-ul 0078 nerulat, nicio suită construită sau rulată** | `SLICE-0078-semnare-pdf.md` | Contractul, versiunile, probele și publicarea la reperul inițial sunt în [jurnal](../SLICE-0078-semnare-pdf.md); corecțiile ulterioare rămân datate. |
| 0078-03 | Jurnalul de diagnostic ActiveX | GATA pe cod (`dotnet build src\KBot.App`: **0 erori, 0 avertismente**) / **nimic văzut pe ecran, fișierul nu a fost produs încă pe o rulare reală** | `SLICE-0078-03-jurnal-activex.md` | Mecanismul, probele, versiunile și limitele sunt în jurnal; starea acestei înregistrări este istorică. |
| 0078-04 | Secțiunea B după A pe DDF: diagnostic și banc | GATA pe cod (build Xfa + App: **0 erori, 0 avertismente**) / **cauza erorii = ipoteză din cod (jurnalele și textul erorii nevăzute), nimic rulat pe ecran** | `SLICE-0078-04-sectiunea-b-peste-semnat.md` | Mecanismul, probele, versiunile și limitele sunt în jurnal; starea acestei înregistrări este istorică. |
| 0078-05 | **Fereastra găzduită fără poziționare / ascundere; Ctrl+H; proba ordinii salvării** | GATA pe cod (build App: **0 erori, 0 avertismente**) / **nimic rulat, Ctrl+H nevăzut în fereastra găzduită** | [0078-05](../SLICE-0078-05-fereastra-gazduita-ctrl-h.md#slice-0078-05) | Probele cu/fără PIN și adoptarea variantei B sunt datate în jurnal; starea inițială de mai sus nu le înlocuiește. |
| 0078-06 | Secțiunea B inserată peste A în fluxul real | GATA pe cod (build App: **0 erori, 0 avertismente**) / **nimic rulat** | [0078-06](../SLICE-0078-04-sectiunea-b-peste-semnat.md#slice-0078-06) | Mecanismul, probele, versiunile și limitele sunt în jurnal; starea acestei înregistrări este istorică. |
| 0078-07 | **Fereastra Adobe găzduită care se mărește singură** | GATA pe cod (build App: **0 erori, 0 avertismente**) / **nimic rulat** | [0078-07](../SLICE-0078-05-fereastra-gazduita-ctrl-h.md#slice-0078-07) | Ipoteza ferestrei maximizate rămâne de confirmat; watcher redenumit ulterior în 08. |
| 0078-08 | **Arborele blocat cât Adobe încă deschide documentul** | GATA pe cod (build App: **0 erori, 0 avertismente**) / **nimic rulat** | [0078-08](../SLICE-0078-05-fereastra-gazduita-ctrl-h.md#slice-0078-08) | Liniștea de 250 ms și plasa de 60 s au limitele consemnate în jurnal. |
| 0078-11 | **Procesele Adobe pornite de K-BOT nu mai rămân fantome după închidere** | GATA pe cod (build `KBot.App` 0 avertismente, 0 erori; nimic rulat, nicio fantomă provocată); `KBot.Controls` 1.60 ▸ 1.61; fără ajutor / NOUTATI (ordinul operatorului) | [0078-11](../SLICE-0078-05-fereastra-gazduita-ctrl-h.md#slice-0078-11) | Doar închidere normală, nu crash sau RunHarness; procesele operatorului nu se ating. |
| 0078-09 | **Întrebarea Adobe «salvați modificările?» la închidere** | GATA pe cod (build App: **0 erori, 0 avertismente**) / **nimic rulat** | [0078-09](../SLICE-0078-05-fereastra-gazduita-ctrl-h.md#slice-0078-09) | Evoluție «Da» → «Nu»; salvările nesemnate de la închidere nu se încarcă. |
| 0078-12 | **Fereastra găzduită cu un Adobe VECHI (< 2024) — monitorul erorilor JS + banc propriu** | GATA pe cod (build `KBot.Controls`, `KBot.App`, `KBot.Controls.Tests`: **0 erori, 0 avertismente**) / **bancul nerulat și nevăzut; testele scrise, compilate, nerulate (regula operatorului)** | [0078-12](../SLICE-0078-05-fereastra-gazduita-ctrl-h.md#slice-0078-12) | Monitorizare distinctă de detectare; constatarea confirmării are continuarea 13. |
| 0078-13 | Confirmarea suprascrierii și F8 în fereastra găzduită (06.10.2026) | Cod scris; App și Controls.Tests construite 0/0; testul și ecranul nerulate | [0078-13](../SLICE-0078-05-fereastra-gazduita-ctrl-h.md#slice-0078-13) | Numai gazda; ipoteza proprietarului rămâne nedovedită, ajutorul pentru panoul ascuns neactualizat. |
| 0078-14 | **Avertisment la pornire când Adobe e mai vechi de 2025** | GATA pe cod, nerulat; build de reconfirmat | [0078-14](../SLICE-0078-05-fereastra-gazduita-ctrl-h.md#slice-0078-14) | Recomandarea >=2025 este după versiune, distinctă de monitorizarea <2024. |
| 0078-15 | Vizualizatorul ActiveX refăcut de la zero (06–07.10.2026) | GATA pe cod (build App Debug: **0 erori, 0 avertismente**) / **probat pe banca ActiveX**: deschidere fără clic, retrimiterea mărimii, Ctrl+H, «Salvare ca» pe copie, «Nu» la închidere; **Ctrl+S automat după semnătură NU a mers**; nimic în aplicație cu un DDF real | `SLICE-0078-15-vizualizator-activex-refacut.md` | Mecanismul și regulile sunt în [jurnal](../SLICE-0078-15-vizualizator-activex-refacut.md). Mențiunea inițială «ajutor neactualizat» are rezolvarea datată 07.10 în §9; versiunile rămân nemărite. |
| 0078-16 | Jurnalul ActiveX pe culoare (DEBUG) | GATA pe cod (build App: **0 erori, 0 avertismente**) / **nimic rulat pe ecran** | [0078-16](../SLICE-0078-03-jurnal-activex.md#slice-0078-16) |

### Open threads

- **0078-15 — checkpoint înainte de actualizarea cu ActiveX implicit:** probele Ctrl+S și limita lanțului A→B→Ordonator sunt în [§6](../SLICE-0078-15-vizualizator-activex-refacut.md#6-ctrls-automat-după-semnătură--nu-a-mers); restanțele, excepțiile și rezolvarea diagnosticului/ajutorului din 07.10 sunt în [§9](../SLICE-0078-15-vizualizator-activex-refacut.md#9-rămas--amânat).
- **0078-12 — bancul nerulat:** comparația cu cele 12 erori din jurnalul real, limitele atribuirii și opțiunile încă ascunse în Setări sunt în [12](../SLICE-0078-05-fereastra-gazduita-ctrl-h.md#slice-0078-12). Defectul confirmării era nereparat la acel moment; [13](../SLICE-0078-05-fereastra-gazduita-ctrl-h.md#slice-0078-13) adaugă recunoașterea prin text, fără probă pe ecran și fără confirmarea ipotezei despre proprietar.
- **0078-11 — fantome, ipoteză și nimic rulat:** [etapa 11](../SLICE-0078-05-fereastra-gazduita-ctrl-h.md#slice-0078-11) păstrează citirea Shutdown, procesul lăsat în pace și limita închiderii normale.
- **Semnarea 0078 — limitele primei etape:** [§4–5](../SLICE-0078-semnare-pdf.md#4-rămas-neverificat--amânat) păstrează ipotezele dialogului/preferinței, observațiile ActiveX și pașii de publicare/DDL nerulați atunci.
- **0078-04 — repere succesive:** [jurnalul](../SLICE-0078-04-sectiunea-b-peste-semnat.md) păstrează ipoteza inițială a sesiunii și proba pe banc din 28.09 (§1.6), cu restanțele JS/NOTAFD/regenerare de atunci; continuările [05](../SLICE-0078-05-fereastra-gazduita-ctrl-h.md#slice-0078-05) și [06](../SLICE-0078-04-sectiunea-b-peste-semnat.md#slice-0078-06) nu transformă bancul în confirmare a fluxului real.

- **0078-05 — ordine salvare/scriere/încărcare, PIN și Ctrl+H:** [etapa 05](../SLICE-0078-05-fereastra-gazduita-ctrl-h.md#slice-0078-05) păstrează proba A→B→Ordonator din 29.09, reparația semnăturii goale și necesitatea reprobării încărcării simulate.
- **0078-06 — flux real nerulat:** [etapa 06](../SLICE-0078-04-sectiunea-b-peste-semnat.md#slice-0078-06), inclusiv codul provizoriu, capturile și excepția Rev 0.
- **0078-07 — jurnalul clientului rămâne proba:** [etapa 07](../SLICE-0078-05-fereastra-gazduita-ctrl-h.md#slice-0078-07), inclusiv ipoteza maximizării și riscurile readucerii.
- **0078-08 — semnalul de deschidere rămâne de probat:** [etapa 08](../SLICE-0078-05-fereastra-gazduita-ctrl-h.md#slice-0078-08), cu presupunerea de 250 ms și plasa de 60 s.
- **0078-09 — evoluția Da→Nu și ipoteza infirmată a butoanelor:** [etapa 09](../SLICE-0078-05-fereastra-gazduita-ctrl-h.md#slice-0078-09); așteptarea ActiveX de minimum 300 ms rămâne o presupunere istorică.
- **0078-10 — de probat pe un PC cu Adobe.** Bifat «Adobe pornește în interfața clasică»: la primul PDF, `adobe_preview.log` spune «Am pus interfața clasică Adobe: … → 0» (sau «deja clasică»); Adobe se deschide cu bara din dreapta; la închiderea K-BOT «Am restaurat preferința Adobe». NEVERIFICAT: numele valorii `bEnableAv2` e din documentația Adobe, nu probat pe o mașină (vezi `AdobeRegistryConstants`); un Adobe deja pornit nu se schimbă. ADOS: fereastra găzduită închisă din Adobe (operatorul, 01.10.2026: «a rămas un cronometru în fundal») nu era observată de nimeni — cronometrul capcanei (200 ms), firul cozilor de tipărire și cronometrele tastelor/termenului mergeau până la următorul document. Acum `AdobeWindowWatcher` prinde EVENT_OBJECT_DESTROY al ferestrei găzduite (fără cronometru nou) ▸ `AdobeReaderHost.Detach` ▸ `HostedWindowClosed` ▸ previzualizarea oprește urmărirea tipăririlor și arată mesajul. De probat: închide documentul din Adobe, apoi `adobe_preview.log` trebuie să spună «Fereastra Adobe a fost închisă din Adobe» și «Capcana «Salvare ca» oprită». Comboul vechi «Mod vizualizator» (profil Auto/Modern/Clasic) rămâne scos: nimic nu-l mai citește din 0078-05.

---

## Slice 0079

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0079 | **Jurnalul semnăturilor (FX_PDF_SEMNATURI)** — cererea operatorului din 23.09.2026: pentru fiecare semnătură, momentul, IP-ul, calculatorul și contul conectat | GATA pe cod (build Xfa/Api/App: **0 erori, 0 avertismente**; `ast.parse` verde) / **DDL nerulat, nimic rulat pe server, nicio suită rulată** | `SLICE-0079-jurnal-semnaturi.md` | Un rând per semnătură NOUĂ (nu cele deja în fișier la deschidere), în aceeași tranzacție cu PDF-ul. Semnatar + dată din PDF (K-BOT, iTextSharp); `UN` + `IpPublic` de la server; calculatorul de la client (`X-Semnaturi` / `X-Statie`, base64 JSON). Doar IP, fără localitate (decizia operatorului). Tabel în fiecare bază de unitate; ruta îl sondează, deci ordinea e liberă. |
