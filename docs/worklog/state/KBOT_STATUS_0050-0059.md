# K-BOT — STATUS, slices 0050–0059

Moved verbatim out of `../KBOT_STATUS.md` (28.09.2026). The index there says what
each slice is; this file holds everything recorded about it: its registry row, its
«Current focus» notes and its «Open threads» notes.

---

## Slice 0050

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0050 | **Calendarul tematizat** — familia nouă `KBot.Controls/Calendar/`: `KBotCalendar` (zile/luni/ani, desenat de noi) și `KBotDatePicker` (câmpul de dată **cu înălțime liberă**) | ÎN LUCRU (cod verde: `KBot.Controls` build **0 erori / 0 avertismente**; `dotnet build KBot.sln` **0 erori / 6 avertismente**, toate `MSB3825` PREEXISTENTE pe `.resx`-urile din `KBot.App`) / **NIMIC NU S-A VĂZUT PE ECRAN**, nedeschis nici în designerul VS; **zero teste scrise și zero suite rulate** (cerut explicit); **fără git** | [0050](../SLICE-0050-calendar-tematizat.md) | Familia, înălțimea liberă, cultura, paddingurile, culoarea și ora: în jurnal, cu prima etapă și runda corectivă separate. OrdEditForm folosește noul control; OrdZiuaForm rămâne pe DateTimePicker nativ. Contrastul este argumentat aritmetic, fără randare. |

---

## Slice 0051

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0051 | Editorul DDF — implementarea inițială | ÎN LUCRU (cod verde: `dotnet build KBot.sln --no-incremental` **0 erori / 12 avertismente**, identic cu linia de bază luată dintr-un `git worktree` curat, toate `MSB3825` preexistente pe `.resx`-uri de vedere; Python **465 trecute / 15 sărite**, identic înainte și după; **ZERO teste noi la prima trecere**, cerut de plan §0.4; trecerea ulterioară pe coloanele din designer și pe blocarea secțiunii A a adăugat `DdfEditGrileTests` — 6 teste, toate trecute; §4.7 a adăugat `RichTextButtonImageLayoutTests` — 7 teste, toate trecute; §4.8 a adăugat `RichTextIconResolutionTests` — 4 teste, toate trecute, Controls 964/964) / **NIMIC N-A RULAT PE MariaDB ȘI NICI PE SERVERUL PYTHON** — niciunul nu e accesibil de pe mașina de dezvoltare; **fiecare instrucțiune SQL e scrisă din `000_DEMO.sql`**, un dump doar de structură, fără niciun rând de probă pe vreun tabel `FX_DDF*`; `sql/0051_fx_numberlock.sql` și `sql/0051_ddf_rev_att_img.sql` **NEAPLICATE**; `DdfEditForm` **NEVĂZUT pe ecran** și nedeschis în designerul VS; `KBotRichTextEditor` văzut pe ecran — banda de antet pentru §4.7 și bancul întreg pentru §4.8, ambele randate cu `DrawToBitmap`; niciodată în `DdfEditForm` și niciodată cu un document adevărat | [0051](../SLICE-0051-ddf-edit-form.md#slice-0051-baza) | Detaliile și reviziile sunt în documentul legat; starea și probele rămân cele ale etapei. |
| 0051-02 | KBotComboBox — Editable și LimitToList | GATA (cod verde: `KBot.Controls` **0 erori / 0 avertismente**; `dotnet build KBot.sln --no-incremental` **0 erori / 14 avertismente**, toate `MSB3825` PREEXISTENTE pe `.resx`-urile din `KBot.App`) / **S-A VĂZUT PE ECRAN** — `Dark` și `Classic`, la 150% DPI, cu capturi de ecran adevărate; `Modern` nefotografiată; **zero teste**, cerut explicit | [0051-02](../SLICE-0051-02-combo-editabil.md) | Detaliile și reviziile sunt în documentul legat; starea și probele rămân cele ale etapei. |
| 0051-03 | Alinierea DDF la ORD | GATA pe cod (`dotnet build src/KBot.App/KBot.App.vbproj` **0 erori / 7 avertismente**, toate `MSB3825` PREEXISTENTE pe `.resx`-uri; `dotnet test tests/KBot.App.Tests` **194 trecute / 13 picate**, aceleași 13 înainte și după, toate din alte felii — `DdfView`, `IstoricView`, `XfaXmlPreview`, parserul XFA, iconițele `MainForm`; `DdfEditGrileTests` 6/6, inclusiv unul care **pica pe ramură** înainte) / **`DdfEditForm` TOT N-A FOST VĂZUT PE ECRAN** — nici în aplicație, nici în designerul VS, nici prin `DrawToBitmap`, deci fiecare afirmație despre aspect e citită din cod, nu văzută; **nimic n-a rulat pe MariaDB sau pe serverul Python**; **zero teste noi** | [0051-03](../SLICE-0051-ddf-edit-form.md#slice-0051-03) | Detaliile și reviziile sunt în documentul legat; starea și probele rămân cele ale etapei. |

---

## Slice 0052

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0052 | **Calibri peste tot + comutatorul «Font din temă» + ștampilele `AutoScaleDimensions`** — fontul de bază pus într-un singur loc, cele patru scheme aliniate pe el, și măturarea celor 52 de ștampile care nu-i mai corespundeau | GATA (cod verde: `dotnet build KBot.sln --no-incremental` **0 erori / 14 avertismente**, toate `MSB3825` PREEXISTENTE pe `.resx`-uri; `KBot.Theming.Tests` **110/110**, `KBot.Controls.Tests` **964/964**, `KBot.App.Tests` 194 trecute / **13 picate — ACELEAȘI 13** dinainte, toate din alte felii) / **S-A VĂZUT PE ECRAN** — `MainForm` fotografiat pe toate patru schemele, în aplicația adevărată, cu `PrintWindow`; **`DdfEditForm` și `OrdEditForm` la lățimea nouă NU s-au văzut** (sesiunea a expirat în timpul navigării); **zero teste noi**, cerut de operator — patru teste EXISTENTE corectate | [0052](../SLICE-0052-calibri-font-sweep.md) | Măsurătorile, cele 52 de ștampile, versiunile, Modern.json, ControlPadding și alegerea «Font din temă»: în jurnal. Etapele oprite și probele ulterioare rămân distincte. Registrul consemna sonda PerMonitorV2/DpiUnaware rulată din același proces; această precizare rămâne istorică. |

---

## Slice 0053

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0053 | **K-BOT Recorder** — formularul din `KBot.Forexe` care andochează browserul Playwright pornit, înregistrează ce face operatorul pe CABWeb și generează un `.wfl` (+ pasul 02: cablarea la consolă și la monitorul Wicket) | GATA pe cod (`dotnet build src\KBot.Forexe\KBot.Forexe.vbproj` **0 erori / 0 avertismente**; `dotnet build KBot.sln` **0 erori**, cele 7 `MSB3825` preexistente pe `.resx`-uri din `KBot.App`; `node --check Recorder.js` valid; **round-trip compactor → writer → `WorkflowParser.Parse` verde**, 17 acțiuni recitite, niciun `LogError`, niciun selector pierdut) / **VĂZUT PE ECRAN** cu `DrawToBitmap`, Classic și Dark / **NIMIC nu a atins un browser adevărat sau CABWeb** | [Baza și 02…04](../SLICE-0053-recorder.md) | Istoric consolidat D001-17: contracte, fișiere, probe și restanțe pe etapă; andocarea simulată este înlocuită ulterior de SetParent/NoViewport. Status păstrează checkpointul inițial, randarea fiind statică. |

### Open threads

[Recorderul consolidat](../SLICE-0053-recorder.md) păstrează cablarea rezolvată, înlocuirea andocării și restanțele capturii/browserului viu, DPI/PIN/tastatură și testelor. NoViewport este implementarea adăugirii finale din 04, fără probă live; nu rămâne o propunere nouă.

---

## Slice 0054

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0054 | Cutia neagră a descărcătorului | GATA pe cod (`dotnet build src\KBot.App\KBot.App.vbproj` **0 erori**, cele 7 `MSB3825` preexistente pe `.resx`; `KBot.App.Tests` **204 trecute / 13 picate / 217**, cele 13 roșii fiind cele străine și preexistente — linia de plecare NU s-a măsurat în sesiune —, iar cele **5 teste noi** verzi) / **niciun folder produs de un robot adevărat**: rularea care a pornit felia s-a făcut pe ALT calculator, la care nu există acces de aici | [0054](../SLICE-0054-cutia-neagra-descarcarii.md#slice-0054) | Contract, diagnostic, rezultate și limite în etapa legată. |

---

## Slice 0055

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0055 | Ingestia legată de buton și prima corecție | GATA pe cod (`dotnet build KBot.sln` **0 erori**; `KBot.App.Tests` **217/230**, `KBot.Domain.Tests` **28/31**, `KBot.Api.Tests` **94/95** — toate roșiile preexistente și dovedite ca atare, iar cele **21 de teste noi** verzi) / **prima rulare adevărată (08.09.2026) a căzut cu `NullReferenceException` — `_cts.Token` citit dintr-un câmp care se naște doar în `SincronizeazaAsync`; reparat cu `CancellationToken.None`, rebuild 0 erori, teste neschimbate. Cererea tot n-a plecat, deci ruta n-a fost încă atinsă, drumul întreg n-a rulat cap-coadă, iar formularul în modul propunere rămâne NEVĂZUT pe ecran** | [0055](../SLICE-0054-cutia-neagra-descarcarii.md#slice-0055) | Contract, diagnostic, rezultate și limite în etapa legată. |

### Open threads

- [Cronologia rulărilor, confirmarea SQL și restanțele](../SLICE-0054-cutia-neagra-descarcarii.md#slice-0055); a doua rulare/refuzul sunt în [0056](../SLICE-0056-tabloul-intreg.md#slice-0056-baza). Reconstituirea era lipsa UI consemnată la acel checkpoint.

---

## Slice 0056

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0056 | Contextul complet și ancora rand_receptie | GATA pe cod (`PYTHON` **485/15 sărite**, de la 472; `dotnet build src\KBot.App` **0 erori**, cele 7 `MSB3825` preexistente pe `.resx`; `KBot.App.Tests` **222/13/235** — aceleași 13 roșii ca la 217/13/230, plus 5 teste noi verzi; `KBot.Api.Tests` **94/95** și `KBot.Domain.Tests` **28/31**, identice cu linia de plecare a feliei 0055) / **nimic n-a rulat pe calculatorul cu FOREXE: niciun `POST` real n-a purtat `rand_receptie`, iar contextul n-a fost văzut pe ecran** | [0056](../SLICE-0056-tabloul-intreg.md#slice-0056-baza) | Detaliile și reviziile sunt în documentul legat; starea și probele rămân cele ale etapei. |
| 0056-02 | Corectarea legăturilor vechi în propunere | GATA pe cod (`dotnet build src\KBot.App` **0 avertismente, 0 erori**; fișierele Python doar parsate) / **nerulat, netestat (la cererea operatorului), nevăzut; serverul nedeployat** | [0056-02](../SLICE-0056-tabloul-intreg.md#slice-0056-02) | Detaliile și reviziile sunt în documentul legat; starea și probele rămân cele ale etapei. |

---

## Slice 0057

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0057 | Listă FOREXE, opriri și portarea SNM | GATA pe cod (`dotnet build src\KBot.App` **0 erori**, 7 avertismente `MSB3825` PREEXISTENTE; `KBot.Api.Tests` 103/1 picat PREEXISTENT; `KBot.App.Tests` 227/13 picate PREEXISTENTE — cele 5 teste noi trec; `KBot.Common.Tests` 85/0; PYTHON `.venv` suita întreagă **509 trecute / 19 sărite / 0 picate**, din care 3 teste `doar_noi` rulate LIVE pe `000_DEMO`) / **NIMIC NU S-A VĂZUT PE ECRAN și nicio sesiune FOREXE n-a fost deschisă** — descărcarea SNM și ruta de import n-au rulat niciodată | [0057](../SLICE-0057-forexe-lista-oprire-extrase.md#slice-0057) | Cereri, etape, contracte, probe și limite în jurnal. |

### Open threads

- [Etape și restanțe 0057](../SLICE-0057-forexe-lista-oprire-extrase.md#slice-0057); stările/probele sunt păstrate în registru.

---

## Slice 0058

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0058 | Cache, verificări de lanț și benzi | GATA pe cod (`dotnet build KBot.sln` **0 erori, 0 avertismente**; PYTHON `.venv` suita întreagă **514 trecute / 19 sărite / 0 picate**, de la 509) / **NIMIC NU S-A VĂZUT PE ECRAN**: nici butonul nou, nici arborii care își țin derularea, nici cursivul recepțiilor noi, nici benzile după curățarea desenului. Câștigul de viteză e ARGUMENTAT din cod, **nu măsurat**. Suitele VB.NET NU s-au rulat (cerere explicită: «no tests»); nimic nu s-a comis («no git») | [0058](../SLICE-0058-asociere-cache-si-lag.md#slice-0058) | Contract, diagnostic, rezultate și limite în etapa legată. |
| 0058-02 | Jurnalul asocierii și cazul live încă deschis | GATA pe cod (PYTHON `.venv` **514 trecute / 19 sărite / 0 picate**, neschimbat; aceeași suită cu `KBOT_ASOCIERE=1` tot 514, fișierul se scrie) / **NICIO CERERE ADEVĂRATĂ N-A TRECUT PRIN EL** — serverul nu s-a pornit, baza vie nu s-a atins | [0058-02](../SLICE-0058-asociere-cache-si-lag.md#slice-0058-02) | Contract, diagnostic, rezultate și limite în etapa legată. |

### Current focus

- [Jurnalul și limitele diagnosticului live](../SLICE-0058-asociere-cache-si-lag.md#slice-0058-02). Existența instrumentului nu închide cazul operatorului.

---

## Slice 0059

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0059 | Recepție nouă din coș | GATA pe cod (`KBot.App` **0 erori**; `--filter ~Asociere` **41 trecute / 0 picate**, plus 3 teste noi pe `DeciziiDin`) / **văzut pe ecran** prin `DrawToBitmap`, în modul de oricând — modul propunere și fereastra mare de benzi NU s-au văzut, și **nu s-a salvat nimic pe o bază vie** | [0059](../SLICE-0059-asociere-receptie-noua-din-cos.md#slice-0059) | Cereri, etape, contracte, probe și limite în jurnal. |
| 0059-02 | Recepția reconstituită este ștearsă prin definiție | GATA pe cod (`KBot.App` **0 erori, 0 avertismente**; `--filter ~Asociere` **47 trecute / 0 picate**; suita `KBot.App.Tests` 241 trecute / 13 picate, exact cele preexistente) / **văzut pe ecran** prin `DrawToBitmap` în patru stări, plus fila «Tot angajamentul» — modul propunere tot NU s-a văzut, și tot **nu s-a salvat nimic pe o bază vie** | [0059-02](../SLICE-0059-asociere-receptie-noua-din-cos.md#slice-0059-02) | Cereri, etape, contracte, probe și limite în jurnal. |

### Current focus

- [Etape și restanțe 0059](../SLICE-0059-asociere-receptie-noua-din-cos.md#slice-0059); stările/probele sunt păstrate în registru.
