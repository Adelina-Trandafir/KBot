# K-BOT — STATUS, slices 0050–0059

Moved verbatim out of `../KBOT_STATUS.md` (28.09.2026). The index there says what
each slice is; this file holds everything recorded about it: its registry row, its
«Current focus» notes and its «Open threads» notes.

---

## Slice 0050

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0050 | **Calendarul tematizat** — familia nouă `KBot.Controls/Calendar/`: `KBotCalendar` (zile/luni/ani, desenat de noi) și `KBotDatePicker` (câmpul de dată **cu înălțime liberă**) | ÎN LUCRU (cod verde: `KBot.Controls` build **0 erori / 0 avertismente**; `dotnet build KBot.sln` **0 erori / 6 avertismente**, toate `MSB3825` PREEXISTENTE pe `.resx`-urile din `KBot.App`) / **NIMIC NU S-A VĂZUT PE ECRAN**, nedeschis nici în designerul VS; **zero teste scrise și zero suite rulate** (cerut explicit); **fără git** | `SLICE-0050-calendar-tematizat.md` | Cerute de operator: un calendar care ascultă de sistemul de teme, un «textbox» care arată ca el dar **care se poate întinde pe verticală**, și zilele/lunile/anii tot tematizate. Cele două controale native se resping motivat: `MonthCalendar` se aliniază la dale întregi de lună (nu se poate andoca/întinde) și `DateTimePicker` **își rescrie propriile margini**, trăgând `Height` înapoi la înălțimea de combo a sistemului — exact defectul de eliminat; peste asta, amândouă sunt ferestre native care rămân albe pe o schemă întunecată (cazul `KBotComboBox`). `KBotDatePicker` nu atinge niciodată `SetBoundsCore`: `Height` e liber, textul rămâne centrat pe verticală, butonul crește cu câmpul. Două evenimente separate (`ValueChanged` la fiecare pas, `DateSelected` doar la alegere) ca fereastra desfășurată să nu se stingă la prima săgeată. Cultura e fixată pe `ro-RO` prin alegere, nu moștenită de la mașină. Așezarea ferestrei și garda de 250 ms se refolosesc din `CustomPopup` (`FitToWorkArea`, `ClosedJustNow`), nu se rescriu. Documentație de familie: `Calendar/KBotCalendar.md` + `Calendar/KBotDatePicker.md`, index în `CONTROLS.md`. `KBot.Controls` FileVersion 1.39 ▸ **1.40**. **Rundă corectivă** (după montarea câmpului pe `OrdEditForm`, §8-§12 în felie): (1) **aerul expus tot** — `KBotDatePicker` cu `Padding`/`TextPadding`/`ButtonPadding`/`GlyphSize`, `KBotCalendar` cu `Padding`/`HeaderPadding`/`GridPadding`/`CellPadding`/`FooterPadding`, fiecare `Padding` întreg cu `ShouldSerialize*`/`Reset*`; `GridPadding` se ia o singură dată ca să nu se decaleze capetele de coloană de coloane, iar `Padding`-ul moștenit a rămas moștenit ca să nu se rupă `ShouldSerializePadding`-ul cadrului (C4). (2) **desenul negru pe negru pe schema întunecată**: cauza era în `OrdEditForm.Designer.vb` — `dtpData.GlyphColor = SystemColors.ActiveCaptionText` (negru) fixat din grila de proprietăți, iar o culoare fixată bate tema (C1); linia (și `HoverColor`) au fost **șterse**, iar `EffectiveGlyphColor` nu mai citește un slot, ci se derivă din `ForeColor` tras o treime spre `BackColor`, deci se mișcă odată cu câmpul. (3) **valori cu oră**: `Value` nu mai e tăiat la zi, `Format` primește parte de oră, `MinDate`/`MaxDate` păstrează ora (implicitul de sus e ultima clipă a zilei), iar regula e «câmpul nu distruge partea pe care n-o arată» — cu format fără oră, alegerea unei zile păstrează ora dinainte (`FormatHasTime`, funcție pură). FileVersion **1.40 ▸ 1.41**. `Views/Ord/OrdEditForm` folosește acum `KBotDatePicker` (`dtpData`); `OrdZiuaForm` a rămas pe `DateTimePicker`-ul de sistem. **Neverificat:** aspectul pe cele trei scheme (nimic randat nici acum — vizibilitatea desenului e argumentată aritmetic: `#D2D2D2` peste `#1C1C1C` la 0,3 ⇒ `#9B9B9B`), comportamentul în designer (`ShouldSerializeValue`), zero teste |

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
| 0052 | **Calibri peste tot + comutatorul «Font din temă» + ștampilele `AutoScaleDimensions`** — fontul de bază pus într-un singur loc, cele patru scheme aliniate pe el, și măturarea celor 52 de ștampile care nu-i mai corespundeau | GATA (cod verde: `dotnet build KBot.sln --no-incremental` **0 erori / 14 avertismente**, toate `MSB3825` PREEXISTENTE pe `.resx`-uri; `KBot.Theming.Tests` **110/110**, `KBot.Controls.Tests` **964/964**, `KBot.App.Tests` 194 trecute / **13 picate — ACELEAȘI 13** dinainte, toate din alte felii) / **S-A VĂZUT PE ECRAN** — `MainForm` fotografiat pe toate patru schemele, în aplicația adevărată, cu `PrintWindow`; **`DdfEditForm` și `OrdEditForm` la lățimea nouă NU s-au văzut** (sesiunea a expirat în timpul navigării); **zero teste noi**, cerut de operator — patru teste EXISTENTE corectate | `SLICE-0052-calibri-font-sweep.md` | Pornit de la o plângere de operator: «tema face ferestrele mai mari sau mai mici, proiectez ceva și văd altceva la rulare». **Erau DOUĂ defecte, nu unul.** (1) Schema `Modern` cerea `Segoe UI Variable Text` 9 și o scria peste un formular proiectat în `Segoe UI` 9; la 150% primul se măsoară `(10, 24)`, al doilea `(10, 25)`, deci fiecare fereastră se turtea cu 4% la comutarea temei. (2) Mult mai grav: **ștampilele `AutoScaleDimensions` rămăseseră ale lui Segoe UI**, iar formularele poartă acum Calibri — la 150% `(10, 25)` față de `(9, 22)`, deci **fiecare fereastră se deschidea la 90% pe lățime și 88% pe înălțime**, pe ORICE schemă. Al doilea era cel pe care operatorul îl descria ca «Întunecat prea îngust, Modern prea lat»; nu venea din temă deloc. **Măsurat, nu dedus:** Calibri 9 = `(6F, 14F)` la 96 dpi, `(8F, 18F)` la 120, `(9F, 22F)` la 144, luate cu o sondă care rulează `PerMonitorV2` și `DpiUnaware` din același proces, cu validarea încorporată (Segoe UI 9 trebuie să dea `(7F, 15F)` la 96 — a dat). **O capcană consemnată:** `CurrentAutoScaleDimensions` se calculează și se memorează în clipa în care se atribuie `AutoScaleMode`; o schimbare ULTERIOARĂ de font nu o invalidează, deci sonda trebuie să pună fontul ÎNTÂI. Prima formă a măsurătorii dădea `(10, 25)` pentru trei fonturi diferite din cauza asta. **Un defect în planul operatorului, prins de măsurătoare și aprobat de el:** pasul 4 cerea aceeași pereche pe toate cele 52 de fișiere, ceea ce ar fi micșorat la ~2/3 cele 11 autorate la 96 dpi; s-a măturat pe grupuri. **Șapte comentarii MINȚEAU** — spuneau «coordonatele sunt scrise la 96 dpi» pe fișiere cu ștampila de 144; istoria arată de ce (commit `ab69142`, «designer re-save at 150% DPI»: Visual Studio rescrie coordonatele ȘI ștampila împreună, `pnlCard` a trecut de la `618×576` la `884×729`, adică ×10/7 pe lățime). **Urmarea de acceptat:** VS va începe să scrie `Font = New Font("Calibri", 9F)` în `.Designer.vb`, fiindcă constructorul de bază atribuie fontul; NU s-a ascuns cu un `ShouldSerializeFont`, pentru că ar fi rupt mărirea textului pe controalele de utilizator. **Ferestrele de editare:** `DdfEditForm` și `OrdEditForm` au trecut de la `ClientSize = 1300×1000` la `1641×1000`, adică fix cât `MainForm`, cerut de operator; lărgirea e sigură prin construcție (antetul are șapte coloane `Absolute` urmate de una `Percent 100`, care absoarbe tot). **Centrarea nu era stricată** — `StartPosition = CenterScreen` era deja pe toate trei și funcționa; ce lipsea era mărimea. Verificat pe ecran: `MainForm` la `(459,264)–(2100,1264)`, adică `1641×1000` și centrat exact, **identic pe toate patru schemele, pixel cu pixel**. **Descoperit pe drum și consemnat:** există `%AppData%\AVACONT\Themes\Modern.json`, care ÎNLOCUIEȘTE schema `Modern` compilată — deci editările din `BuiltInSchemes.Modern()` n-au niciun efect pe mașina asta; și `ThemeStyleOptions.ControlPadding` nu e citit de nimeni din motorul de teme (era principalul suspect pentru «Modern prea lat»; nu era el). FileVersion: `KBot.Theming` **1.11 ▸ 1.12**, `KBot.Controls` **1.44 ▸ 1.45**, `KBot.App` și `KBot.DevHarness` **1.0.23 ▸ 1.0.24**, `KBot.Forexe` **1.0.3 ▸ 1.0.4**, `KBot.Migrator` **1.6 ▸ 1.7**; `AssemblyVersion` neatins. **De hotărât de operator:** dacă rândul «Font din temă» trebuie să dispară odată cu cursorul de mărime pe cele șapte dialoguri care sting `ShowTextScaleSlider`. |

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
