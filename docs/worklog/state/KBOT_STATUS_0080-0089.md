# K-BOT — STATUS, slices 0080–0089

Moved verbatim out of `../KBOT_STATUS.md` (28.09.2026). The index there says what
each slice is; this file holds everything recorded about it: its registry row, its
«Current focus» notes and its «Open threads» notes.

---

## Slice 0080

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0080-01 | Chei clasificație pe șapte tabele și DataDoc DATE | GATA pe cod (`py_compile` verde; `dotnet build src\KBot.Migrator` 0 / 0) / **scriptul unic nerulat, nimic pe server, nicio suită rulată** | [0080-01](../SLICE-0080-01-idclsf-pereche-datadoc.md#slice-0080-01) | Detaliile, probele și limitele sunt în etapa legată. |
| 0080-02 | Vederea Extrase și coloane configurabile | GATA pe cod (`dotnet build src\KBot.App --no-incremental`: **0 erori, 0 avertismente**) / **văzut doar cu `DrawToBitmap` pe date inventate; ruta nerulată; nicio suită rulată** | [0080-02](../SLICE-0080-02-vederea-extrase.md#slice-0080-02) | Detaliile, probele și limitele sunt în etapa legată. |
| 0080-03 | Fereastra tuturor extraselor | GATA pe cod (build App **0 / 0**) / **fereastra însăși nu a fost desenată; nerulată** | [0080-03](../SLICE-0080-02-vederea-extrase.md#slice-0080-03) | Detaliile, probele și limitele sunt în etapa legată. |
| 0080-04 | IdClsfAcc numai în Clasificatii | GATA pe cod (`py_compile` verde; build App + Migrator 0 / 0) / **scriptul nerulat, nimic pe server, nicio suită rulată** | [0080-04](../SLICE-0080-01-idclsf-pereche-datadoc.md#slice-0080-04) | Detaliile, probele și limitele sunt în etapa legată. |

### Current focus

- [01/04 — chei și DataDoc](../SLICE-0080-01-idclsf-pereche-datadoc.md) și [02/03 — extrase](../SLICE-0080-02-vederea-extrase.md). Probe și stare păstrate pe etapă.

### Open threads

- [Punerea în funcțiune, confirmările și restanțele](../SLICE-0080-01-idclsf-pereche-datadoc.md), plus domeniul vederii/ferestrei din 02/03; scripturile și rutele rămân nerulate la reperul registrului. Calendarul este amânat, fereastra modală nedesenată.

---
## Slice 0081

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0081 | Trimiterea și editorul DDF — plan inițial și revizii | GATA pe cod — sub-feliile 0081-01 … 0081-10 (nimic rulat live, nicio suită rulată) | [0081](../SLICE-0081-01-starea-reviziei.md) | Contractele, reviziile și limitele sunt în etapa legată; reperele unice ale registrului sunt la sfârșitul istoricului. |
| 0081-01 | Starea reviziei, G0a | GATA pe cod (build App **0 / 0**; `py_compile` verde) / **SQL nerulat, nevăzut pe ecran, nicio suită rulată** | [0081-01](../SLICE-0081-01-starea-reviziei.md#slice-0081-01) | Contractele, reviziile și limitele sunt în etapa legată; reperele unice ale registrului sunt la sfârșitul istoricului. |
| 0081-02 | Moduri editor și PDF intermediar, G6a | GATA pe cod (build App **0 / 0**; `py_compile` verde) / **nevăzut pe ecran, nerulat, nicio suită rulată** | [0081-02](../SLICE-0081-01-starea-reviziei.md#slice-0081-02) | Contractele, reviziile și limitele sunt în etapa legată; reperele unice ale registrului sunt la sfârșitul istoricului. |
| 0081-03 | Workflow-uri, G1/G2; G3 renunțat | GATA pe fișiere (XML valid; build Forexe **0 / 0**) / **NICIUN flux rulat — fiecare ramură o dată pe 000_DEMO** | [0081-03](../SLICE-0081-01-starea-reviziei.md#slice-0081-03) | Contractele, reviziile și limitele sunt în etapa legată; reperele unice ale registrului sunt la sfârșitul istoricului. |
| 0081-04 | Trimiterea și reluarea, G6b | GATA pe cod (build App **0 / 0**) / **nerulat live, nevăzut pe ecran, nicio suită rulată** | [0081-04](../SLICE-0081-01-starea-reviziei.md#slice-0081-04) | Contractele, reviziile și limitele sunt în etapa legată; reperele unice ale registrului sunt la sfârșitul istoricului. |
| 0081-05 | Capturile în Table4, G5 | GATA pe cod (build App + Xfa **0 / 0**) / **niciun PDF final deschis în Adobe, nicio suită rulată** | [0081-05](../SLICE-0081-01-starea-reviziei.md#slice-0081-05) | Contractele, reviziile și limitele sunt în etapa legată; reperele unice ale registrului sunt la sfârșitul istoricului. |
| 0081-06 | K-BOT-ul directorului, G0b | GATA pe cod (build App **0 / 0**; `py_compile` verde) / **nerulat, nevăzut pe ecran, nicio suită rulată** | [0081-06](../SLICE-0081-01-starea-reviziei.md#slice-0081-06) | Contractele, reviziile și limitele sunt în etapa legată; reperele unice ale registrului sunt la sfârșitul istoricului. |
| 0081-07 | Mod probă, reîncărcare și păstrarea răspunsurilor | GATA pe cod (build App **0 / 0**; `py_compile` verde) / **nerulat, nevăzut pe ecran, nicio suită rulată** | [0081-07](../SLICE-0081-01-starea-reviziei.md#slice-0081-07) | Contractele, reviziile și limitele sunt în etapa legată; reperele unice ale registrului sunt la sfârșitul istoricului. |
| 0081-08 | Dialogul rândului Secțiunii A | GATA pe cod (build Controls + App **0 / 0**; fereastra randată cu DrawToBitmap, tastarea `650204` → `65.02.04` + listă + Enter verificate) / **nerulat live, nicio suită rulată** | [0081-08](../SLICE-0081-01-starea-reviziei.md#slice-0081-08) | Contractele, reviziile și limitele sunt în etapa legată; reperele unice ale registrului sunt la sfârșitul istoricului. |
| 0081-09 | Antet, sursă și partener | GATA pe cod (build App **0 / 0**; fereastra rândului randată pentru ambele programe) / **teste VB scrise, nerulate; ruta Flask nerulată, fără test; nimic pe server** | [0081-09](../SLICE-0081-01-starea-reviziei.md#slice-0081-09) | Contractele, reviziile și limitele sunt în etapa legată; reperele unice ale registrului sunt la sfârșitul istoricului. |
| 0081-10 | Clasificație FOREXE și corecția antetului reviziei 0 | GATA pe cod (build App **0 / 0**; fereastra rândului randată) / **teste VB scrise, nerulate; SQL-ul partenerilor nerulat; nimic pe server** | [0081-10](../SLICE-0081-01-starea-reviziei.md#slice-0081-10) | Contractele, reviziile și limitele sunt în etapa legată; reperele unice ale registrului sunt la sfârșitul istoricului. |
| 0081-12 | Indicatori existenți, program/buget și rânduri cu zero | GATA pe cod (build App **0 / 0**) / **nevăzut pe ecran, nerulat live; niciun test scris** | [0081-12](../SLICE-0081-01-starea-reviziei.md#slice-0081-12) | Contractele, reviziile și limitele sunt în etapa legată; reperele unice ale registrului sunt la sfârșitul istoricului. |
| 0081-13 | Completarea direct în grilă și blocarea angajamentului nou | GATA pe cod (build App **0 / 0**) / **nevăzut pe ecran, nerulat live; niciun test scris** | [0081-13](../SLICE-0081-01-starea-reviziei.md#slice-0081-13) | Contractele, reviziile și limitele sunt în etapa legată; reperele unice ale registrului sunt la sfârșitul istoricului. |

### Current focus

- [13 — editarea directă în grilă](../SLICE-0081-01-starea-reviziei.md#slice-0081-13) urmează etapelor 08…12; probele și restanțele diferă. Cronologia stării, antetului și programului este explicată la începutul istoricului.

### Open threads

- [Întrebările registrului](../SLICE-0081-01-starea-reviziei.md) păstrează confirmările 0081-09/12 și limita istorică 0082; restanțele tehnice sunt în fiecare etapă: SQL și publicare server, fluxuri reale, PDF/capturi/semnare, Director, mod probă/reîncărcare și noua celulă grupată. Consolidarea nu le închide.

---
## Slice 0082

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0082 | ComboBox: căutare și mască | GATA pe cod (build Controls **0 / 0**; verificat prin randare în fereastra 0081-08) / **teste scrise, nerulate** | [0082](../SLICE-0082-combo-cautare-si-masca.md#slice-0082) | Contracte, etape, rezultate și limite în jurnalul legat. |

---

## Slice 0083

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0083 | ComboBox: oferta de element nou | GATA pe cod (build Controls **0 / 0**) / **fără verificări și fără teste, la cererea operatorului** | [0083](../SLICE-0082-combo-cautare-si-masca.md#slice-0083) | Contracte, etape, rezultate și limite în jurnalul legat. |

---

## Slice 0084

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0084 | **«Operațiuni necorectate» după conectarea FOREXE (cererea operatorului, 26.09.2026)** — după fiecare conectare reușită se citește tabelul din pagina de start; dacă are rânduri: salvare în `FX_Operatiuni` (doar cele noi, cheie ReferintaTrezor + NrDoc) și un mesaj cu Program, SSI, Ref. TREZOR, Nr. doc., Dată, Tip, Suma (+ Probleme doar dacă există) | GATA pe cod (build App **0 / 0**; `py_compile` verde) / **nerulat, scriptul paginii netestat, fără teste** | `SLICE-0084-operatiuni-necorectate.md` | ⚠ `FX_Operatiuni.Suma` o adaugă operatorul. `operatiuni.py` de urcat pe VPS. URMEAZĂ: corelarea operațiunilor cu angajamentele din bază. |
| 0084/01 | **Indicatorii la reîmprospătare + redenumirea `FX_Indicatori.Prevedere_Bugetara_Initiala` → `Credit_Bugetar` (operator, 26.09.2026)** — (1) verificat: pasul 2 al prelucrării rescrie deja cele patru coloane de bani pentru fiecare rând (fără schimbare de cod; secțiunea din fluxurile de recepții anulată la cerere). (2) Redenumirea: Python (`prelucrare.py`, `prelucrare_pasi.py`, `ddf_edit.py`), Migrator (`.Rename(...)` pe FX_Indicatori, 1.16.1.0), `sql/AVACONT_SURSA.sql` | GATA pe cod (`py_compile` verde, build Migrator **0 / 0**) / **nerulat, fără teste** | `SLICE-0084-01-indicatori-la-fiecare-reimprospatare.md` | ⚠ Redenumirea în bază o face operatorul, în AVACONT_SURSA cu `COMMENT 'rename:Prevedere_Bugetara_Initiala'` (altfel sincronizarea face DROP + ADD); cele 3 fișiere Python urcă pe VPS ODATĂ cu ea. |
| 0084/02 | Asociere parteneri DDF din Sumar — lista distinctă de antet | GATA pe cod (build curat, nerulat, nevăzut; SQL-ul și serverul nedeployate) | [0084/02](../SLICE-0084-02-asociere-parteneri-sumar.md#slice-0084-02) | Contracte, fișiere, probe și repere de ajutor în jurnalul consolidat D001-18. |
| 0084/03 | Asociază parteneri — deschidere ca popup modal | GATA pe cod (build curat, nerulat, nevăzut) | [0084/03](../SLICE-0084-02-asociere-parteneri-sumar.md#slice-0084-03) | Contracte, fișiere, probe și repere de ajutor în jurnalul consolidat D001-18. |
| 0084/04 | Asociază parteneri — comanda în ribbon, disponibilitate după DDF | GATA pe cod (build curat, nerulat, nevăzut) | [0084/04](../SLICE-0084-02-asociere-parteneri-sumar.md#slice-0084-04) | Contracte, fișiere, probe și repere de ajutor în jurnalul consolidat D001-18. |

---

## Slice 0085

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0085 | DataView: editor, clic și navigare verticală (26.09.2026) | GATA pe cod (build Controls + App **0 / 0**; verificare pe pixeli în afara ecranului: **0 pixeli diferiți** la 144 dpi pe 5 celule, 4/5 la 96 dpi — Tahoma diferă doar la franjurile ClearType) / **nevăzut în aplicație pe ecranul operatorului, nicio suită rulată** | [0085](../SLICE-0085-dataview-editor-matches-cell.md#slice-0085-baza) | Detalii, treceri, rezultate și limite în jurnalul consolidat. |

| 0085-02 | DataView: butoane și borduri (03.10.2026) | GATA pe cod (build Controls **0 / 0**, FileVersion 1.62.0.0) / **nerulat, nevazut, netestat in designer** | [0085-02](../SLICE-0085-dataview-editor-matches-cell.md#slice-0085-02) | Detalii, treceri, rezultate și limite în jurnalul consolidat. Rezumatul inițial afirma pictare identică la implicite; jurnalul păstrează explicit lipsa comparației pe pixeli și a probei pe ecran. |

### Current focus

- **0085 — DataView:** comportamentul final al editorului și proba pe ecran încă necesară sunt în [istoricul bazei](../SLICE-0085-dataview-editor-matches-cell.md#slice-0085-baza).

---

## Slice 0086

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0086 | KbotForm: parțiale și buton primar | GATA pe cod (build App + Controls **0 / 0**) / **nevăzut pe ecran și în VS, fără teste (la cererea operatorului)** | [0086](../SLICE-0086-kbotform-partials-primary-button.md#slice-0086) | Contracte, etape, rezultate și limite în jurnalul legat. |

### Current focus

- [Împărțirea și limitele probei](../SLICE-0086-kbotform-partials-primary-button.md#slice-0086); aspectul butonului și nesting-ul VS rămân nevăzute la acel checkpoint.

---

## Slice 0087

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0087 | Clasificații, Parteneri și meniul din antet | GATA pe cod (build App **0 / 0**; `py_compile` verde; randat cu `DrawToBitmap`, Classic + Dark, date false) / **nerulat pe server, SQL netestat, neapăsat pe ecran, fără teste (la cererea operatorului)** | [0087](../SLICE-0087-clasificatii-parteneri-meniu.md#slice-0087) | Contracte, etape, rezultate și limite în jurnalul legat. |

---

## Slice 0088

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0088 | Note corecție CAB și reviziile numerotării/recipisei | GATA pe cod (build Domain/Xfa/Api/Forexe/App **0 / 0**; `py_compile` verde) / **nerulat, nevăzut pe ecran, fără teste** | [0088](../SLICE-0088-note-corectie-cab.md#slice-0088) | Contracte, etape, rezultate și limite în jurnalul legat. |

---

## Slice 0089

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0089 | Jurnalele serverului pe utilizator/sesiune | done | [0089](../SLICE-0089-jurnale-server-per-utilizator.md#slice-0089-baza) | Detaliile, probele și limitele sunt în etapa legată. |
| 0089-01 | Jurnale server numai cu opțiuni avansate | GATA pe cod (build: nicio eroare de compilare) / nimic văzut pe ecran | [0089-01](../SLICE-0089-jurnale-server-per-utilizator.md#slice-0089-01) | Detaliile, probele și limitele sunt în etapa legată. |
