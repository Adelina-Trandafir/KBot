# Status migrare 6 — Felia 0020: vederea DDF

**Data:** 2026-07-24
**Felie:** 0020 (Documentul de Fundamentare — echivalentul Access `frmFX_MAIN_DDF`)
**Stare generală:** cod complet pentru cele **cinci pași construiți** (0020-01…05), soluția
compilează curat (0 erori), toate testele verzi. **Niciun verdict vizual, niciun PDF real,
niciodată rulat pe o bază live.** Pasul 06 (semnarea) amânat la felia 0021.

Aceasta este a șasea vedere reală migrată din Access, după Sumar (0011), Rezervări (0014),
Recepții (0015) și Plăți (0017). Sursa de adevăr a stării rămâne
`docs/worklog/KBOT_STATUS.md`; acest document este un rezumat de migrare al feliei 0020.

---

## 1. Ce s-a livrat, pas cu pas

Fiecare pas are un worklog dedicat în `docs/worklog/SLICE-0020-0X-*.md` și un commit propriu.

| Pas | Commit | Ce a livrat |
|---|---|---|
| 0020-01 | `c0eab3a` | Endpoint `GET /api/forexe/ddf` + client `GetDdfAsync` + POCO-uri Domain |
| 0020-02 | `9531adc` | `DdfView` înlocuiește placeholder-ul: arbore, sub-nav orizontal, grilă, combo |
| 0020-03 | `cff715a` | Două suprafețe de previzualizare (`XfaXmlPreview` implicit / `ReaderHostPreview` backup) |
| 0020-04 | `df99591` | `KBotPaths` + browser de fișiere PDF + legarea frunză → previzualizare |
| 0020-05 | `628e80f` | Generarea PDF (părțile ne-blocate) + `DdfXmlBuilder` + extinderea endpoint-ului |
| (docs) | `40b1c40` | Refresh „Current focus" în STATUS + secțiunea de stare din CLAUDE.md |

### 0020-01 — Endpoint + client
- **Un endpoint, trei liste, un singur drum dus-întors:** `{cod, antet[], revizii[], linii[]}`.
  Clientul filtrează LOCAL (arbore/grilă/combo), deci un clic în arbore nu mai cere nimic
  serverului (decizia 7).
- **`TotalRevizie` = `SUM(ValCur)` REAL** (subinterogare scalară), nu defectul Access
  `SA.ValCur AS TotalRevizie` (valoarea unei linii purtând numele unui total).
- **`antet` e ARRAY:** PK `FX_DDF` compus `(IDDF, CUAL)`, fără constrângere unică pe
  `CodAngajament`; clientul alege explicit (`DdfInfo.AntetDeLucru`), niciodată tacit.
- **`Clsf` cheiat pe `Clasificatii.IDClsf`** (PK, invers față de `FX_Indicatori`) — fără
  fan-out, fără predicat `IdUnitate`.
- POCO-uri noi (`KBot.Domain/DdfInfo.vb`) cu regulile citite din sursă, nu ghicite:
  `FolderPdf`/`NormalizeazaNume` (partener vs `GENERAL`, `\W+`→`_`), `EtichetaRevizie`
  (`Format(NumarRev,"@@@")` = aliniere TEXT cu SPAȚII → `PadLeft(3)`, niciodată `D3`/`000`).

### 0020-02 — `DdfView`
- `SplitContainer`: arbore de revizii (stânga) | sub-nav orizontal + pagini (dreapta).
- **Arbore pe 2 niveluri:** lună (rădăcină, sumă) → revizie (frunză, `TotalRevizie`). Iconițe
  GDL tintate din paletă (`DdfIcons`, REV_SUS/REV_JOS/REV_NOT), roșu pe total negativ **doar
  pe totalul propriu** (nu se propagă spre rădăcină ca în Access — deviere documentată).
- **Sub-nav ORIZONTAL** (decizia 8) prin `KBotNavList.Orientation = Horizontal`.
- **Grilă** (read-only, rând de totaluri): `Clsf`, `ElementFund` (auto-hide), `DataRev`
  (doar la rădăcină), `ValPrec`, `ValCur` (Sum), `ValTot`.
- **Combo de clasificații:** valori distincte din rândurile nodului, prima intrare
  `< Arată toate clasificațiile >`, **resetat necondiționat la fiecare clic** pe nod.
- Încărcare prin `WithReauth(Of DdfInfo)` cu stale-guard `_requestedCod`.

### 0020-03 — Cele două previzualizări
- `IDdfPreview` + `DdfPreviewFactory` cu **constantă de compilare** (`Mode`), nu comutare
  la runtime. Implicit = `XfaXmlPreview`; backup = `ReaderHostPreview`.
- **`XfaXmlPreview`** (implicit): randează din XFA-XML — `.xml` fratern dacă există, altfel
  `PdfXmlExtractor` pe PDF — antet + tabelul secțiunii A + notă, suprafață WinForms temată.
- **`ReaderHostPreview`** (backup): lansează PDF-ul, găsește fereastra Adobe prin `EnumWindows`
  (potrivire pe clasă + nume fișier, timeout mărginit), o reparentează cu `SetParent`, curăță
  stilurile de fereastră. Scris defensiv la fiecare eșec din plan.
- Stare „document lipsă" cu buton **«Generează documentul»** ce ridică `GenerateRequested`.

### 0020-04 — `KBotPaths` + browser de fișiere
- **`KBotPaths`** în `KBot.Common`: JSON lângă executabil, o singură proprietate azi —
  `DdfPdfRoot` (implicit `C:\AVACONT\FOREXE\PDF\DDF\`). Fișier lipsă/corupt → default + log,
  nu aruncă la pornire. **NU e o reînviere a `AppConfig`** (adresa serverului rămâne în
  `ApiOptions`); `KBotPaths` ține doar locații de fișiere locale.
- **Browser** (`KBotDataView`): enumerare recursivă `*.pdf`, păstrează doar
  `*_{CodAngajament}.PDF` — prinde `GENERAL\` și folderele de partener fără să hardcodeze
  niciunul; fără `.xml`, fără `PDF\ORD\`. Coloane: Folder, Fișier, `CUAL`, `NumarRev`, Mărime,
  Modificat.
- **Legare încrucișată:** clic pe o frunză calculează calea așteptată (§2.5, din `CUAL` +
  `PartAng`/`NumePartener` + `NumarRev`) și o dă previzualizării cu flag-ul de existență; clic
  pe rădăcină golește previzualizarea.

### 0020-05 — Generarea PDF (părțile ne-blocate)
- **Pas 05-00 (client):** `KBot.Xfa/Config.vb` `BASE_URL` mutat pe
  `https://kbot.avatarsoft.ro/api/`, comentariul învechit corectat (macheta NU trăiește pe un
  „server vechi" separat — `mfp_bp` e în ACELAȘI app Flask, `main.py:52`), `FileVersion`
  1.1.0.0 → 1.2.0.0. `X-API-KEY` rămâne — amânare deliberată, logată ca fir deschis.
- **Endpoint extins** (o singură rută): `linii` primește `ss`
  (`COALESCE(NULLIF(SA.SS,''), Clasificatii.SS)`); array-urile `sectiuneb` (FX_DDF_REV_SB) și
  `atasamente` (FX_DDF_REV_ATT) **opt-in printr-un singur `?pentru_generare=1`** — date
  numai-pentru-generare, ca vederea să nu poarte ce nu afișează (§2.8).
- **`DdfXmlBuilder`** (pur — piesa de rezistență): port fidel al celor trei funcții din
  `mdl_FX_DDF_PDF` — `BuildFormXml` (`form1`: `SubformAntet` + secțiunea A cu **`Row1` dummy** +
  `SubformSectiuneaB/Table3` cu `Cell7` sărit), `BuildNotafdXml` (NOTAFD cu sume trunchiate
  `Int()`), `BuildAttachments` (nodul `<Attachments>`: NOTAFD base64 + rândurile din
  `FX_DDF_REV_ATT.DateFisier`).
- **Apel `XfaWriter.Genereaza`** pe thread de fundal (`Await Task.Run`), cu gardă anti-reintrare;
  la succes K-BOT **NU scrie nimic în baza** (§2.4 — cele patru coloane de evidență PDF nu
  există în MariaDB), existența = scanarea de disc.

---

## 2. Devieri deliberate față de plan (toate documentate în worklog-uri)

1. **`revizii`/`linii` filtrează prin `IN (SELECT …)`, nu prin join pe antet** (0020-01). Cu
   PK-ul compus, un `INNER JOIN … ON IDDF` ar multiplica fiecare revizie cu numărul de rânduri
   `CUAL`. Pin de regresie: `test_second_cual_row_does_not_fan_out_revisions`.
2. **Roșul pe total negativ rămâne pe nodul propriu**, nu se propagă spre rădăcină (Access
   colora rădăcina cu culoarea ultimei frunze procesate — accidental).
3. **Un singur flag `?pentru_generare=1`** pentru SB + atașamente (planul propunea `?atasamente=1`
   doar pentru atașamente) — evită un payload de generare încărcat pe jumătate.
4. **Worklog-uri sub-numerotate** (`SLICE-0020-01…05`) în loc de fișierul unic din plan —
   conform `CODE_WORKFLOW.md` §3.2 și practicii feliilor 0015/0017.
5. **`FxIcons` NU se folosește** (planul îl numea): acela încarcă iconițe embedded pentru starea
   angajamentului; s-a scris `DdfIcons` (forme GDI tintate din paletă), tiparul real per-vedere.

---

## 3. Teste

| | Rezultat |
|---|---|
| **.NET** `dotnet build KBot.sln` | 0 erori, 0 warning-uri BC (doar NU1701 preexistente: iTextSharp/BouncyCastle pe net8.0) |
| **.NET** `dotnet test KBot.sln` | **415 passed / 0 failed** (App 120, Api 63, Controls 134, Theming 27, Xfa 39, Domain 17, Common 14, LocalStore 1) |
| **Python** `pytest tests/` (offline) | **75 passed / 14 skipped / 0 failed** — cele 17 teste DDF sunt host-only, sar corect off-host |

Fișiere de test noi: `test_forexe_ddf.py` (17), `DdfInfoTests.vb` (13), `DdfViewTests.vb`,
`XfaXmlPreviewTests.vb`, `DdfPdfLocatorTests.vb`, `DdfFileBrowserTests.vb`, `KBotPathsTests.vb`,
`DdfXmlBuilderTests.vb` (20 structurale), `DdfXfaParserTests.vb`.

> Câteva teste au prins bug-uri **în harness-ul de test**, nu în produs: `FindControl(Of ComboBox)`
> întorcea editorul flotant al grilei în loc de `cboClsf` (rezolvat prin căutare după nume);
> `Control.Visible` întoarce False când un strămoș e ascuns (asertarea de pagină funcționează
> doar după ce vederea are date). Codul de produs nu a fost de vină în niciunul.

---

## 4. Ce a rămas de făcut

### 4.1 Verificări pe host (blocante pentru încredere, nu pentru cod)
1. **Pas 05-00 — verificarea live a machetei.** `GET` **și** `HEAD` pe
   `https://kbot.avatarsoft.ro/api/mfp/template_ddf` cu `X-API-KEY`, peste 443. Ruta și suportul
   HEAD există în app-ul Flask (confirmat offline, `routes/mfp.py:177`), dar **dacă nginx rutează
   `/api/mfp/*` pe vhost-ul de 443 și dacă macheta e în cache-ul serverului — neverificat.** Dacă
   macheta nu se poate aduce, `XfaWriter.Genereaza` cade la `TemplateDownloader`. **Rulează asta
   înainte de orice altceva din generare.**
2. **Rulează `test_forexe_ddf.py` pe host** (17 teste). Confirmă tabelele/coloanele și răspunde
   la **§3 punctul 5**: câte `CodAngajament` au >1 rând `FX_DDF` (test de CONSTATARE, nu pică
   niciodată). Până atunci, dacă §2.7 e teoretic sau real rămâne **nedecis**.

### 4.2 Verdict vizual și PDF real (niciun ochi nu le-a văzut)
3. **Niciun verdict vizual** pe nicio suprafață — layout, lățimile măsurate ale nav-ului
   orizontal, auto-hide-ul lui `ElementFund`, stilul antetului `XfaXmlPreview`: toate verificate
   doar prin compilare + teste headless.
4. **`ReaderHostPreview` — calea Win32 de găzduire de fereastră nu a rulat niciodată** și nu poate
   fi exercitată headless sau fără Adobe instalat. Cea mai puțin verificată piesă din toată felia.
5. **Niciun PDF real produs sau deschis.** `XfaWriter.Genereaza` e cablat și apelat pe thread de
   fundal, dar are nevoie de machetă + iTextSharp pe un template real. Verdictul „Adobe Reader
   deschide corect PDF-ul generat" (deschide și firele 0019 a/c) e **încă deschis**.
6. **Niciun diff structural contra unui `.xml` Access bun** — imposibil din repo: singurul sample
   DDF (`Surse/SURSA_XFA_WRITTER/XSD_XML/ddf_demo.xml`) e de fapt ORD. `DdfXmlBuilder` e pinuit de
   20 de teste structurale contra **sursei modulului**, nu contra unui artefact real. Capturează
   un `.xml` DDF scris de Access pe host și fă diff-ul.
7. **Fidelitatea base64 ANSI-vs-UTF-8 în NOTAFD** — risc neverificat.

### 4.3 Funcționalitate amânată explicit
8. **Pasul 06 (semnarea) → felia 0021** (decizia operatorului). Rămân: `XfaWriter.GenereazaSiSemneaza`
   pe thread de fundal, ruta de write-back `UPDATE FX_DDF_REV.Semnatura`, și regula „nu semna
   niciodată cât timp `ReaderHostPreview` ține o fereastră". `Semnatura` se poartă azi dar nu se
   folosește.
9. **UI-ul de generare nu are bară de progres.** `DdfView` nu are mâner la bara de ocupare a
   shell-ului, deci doar se apără de reintrare (`_generating`). O indicație vizuală de progres e
   un follow-up.
10. **`X-API-KEY` hardcodat în `Config.vb`** — fir deschis logat în STATUS. Scoaterea lui cere
    `@require_session` pe cele două rute `mfp` + token bearer în `TemplateDownloader`, ceea ce
    atinge și ORD. De decis când, nu dacă.

### 4.4 Fire deschise mai mici
11. **Calea de avertizare la antet multiplu netestată** — când `Antet.Count > 1`, vederea
    loghează și alege pe `_preferredIddf`; niciun test nu o exercită fiindcă §3 punctul 5 nu ne-a
    spus încă dacă acel caz există în date reale.
12. **`DataDef`/`DataCreare` trunchiate la zi** — dacă un pas ulterior are nevoie de oră, `_iso`
    trebuie schimbat.
13. **Ambiguitatea `Program`** (§2.9): `FX_DDF.Program` vs codul de program al sesiunii — de
    lămurit la generare pe date reale.

---

## 5. Definiția de gata — bifat vs rămas

- [x] Endpoint `GET /api/forexe/ddf` + 17 teste host-only + `GetDdfAsync` + 4+ teste client.
- [x] `DdfView` înlocuiește placeholder-ul; arbore, sub-nav orizontal, grilă, combo, rând de
      totaluri — toate cu teste headless.
- [x] Ambele previzualizări există; constanta de mod comută între ele la recompilare.
- [x] Browser de fișiere `*_{CodAngajament}.PDF` sub rădăcina configurată; fără `.xml`, fără ORD.
- [x] `KBotPaths` citește JSON, cade curat pe default, documentat ca ne-`AppConfig`.
- [x] `Config.BASE_URL` pe HTTPS; `FileVersion` 1.2.0.0; amânarea API-key logată.
- [x] Endpoint extins cu `sectiuneb` + `atasamente` opt-in + `ss` pe `linii`.
- [x] XML-ul generat poartă ambele blocuri de secțiune B.
- [x] Suita .NET verde (inclusiv cele 39 `KBot.Xfa.Tests`); suita Python verde offline.
- [x] Fără referință la `XFA_WRITTER.exe` în codul nou.
- [ ] **Pas 05-00 verificat live** (GET+HEAD peste 443). — RĂMAS
- [ ] **PDF real generat, deschis corect în Adobe Reader** (închide 0019 a/c). — RĂMAS
- [ ] **Diff structural contra unui `.xml` Access cunoscut-bun.** — RĂMAS (imposibil din repo)
- [ ] **Verdict vizual pe orice suprafață.** — RĂMAS
- [ ] Pasul 06 (semnarea). — AMÂNAT la felia 0021
