# K-BOT — STATUS, slices 0090–0099

Everything recorded about each slice: its registry row, its «Current focus» notes and its
«Open threads» notes. The index in `../KBOT_STATUS.md` says what each slice is.

---

## Slice 0090

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0090 | **`KBOT_STATUS.md` împărțit: index + fișiere de câte zece felii (cererea operatorului, 28.09.2026)** | GATA (doar documente, fără cod) | `SLICE-0090-status-split.md` | Fișierul de 658 KB → 28 KB (index, decizii blocate, note transversale) + `state/KBOT_STATUS_<zeci>.md` (rândul complet + «Current focus» + «Open threads» per felie, copiate cuvânt cu cuvânt) + `state/KBOT_STATUS_SLICELESS.md` (munca fără felie + vechiul `project_state.md`, șters din rădăcină). Verificare: toate cele 1.570 de linii ale originalelor regăsite, în afară de cele două reguli vechi «How to update» înlocuite intenționat. Regula de citire în `CLAUDE.md`, `KBOT_STATUS.md`, `CODE_WORKFLOW.md`. |

### Open threads

- **0090 — două fișiere încă mari:** `KBOT_STATUS_0020-0029.md` (~184 KB) și
  `KBOT_STATUS_0040-0049.md` (~174 KB). Operatorul a refuzat deocamdată împărțirea lor pe felii.

---

## Slice 0091

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0091 | **Detaliul recepției citit tăiat din FOREXE + viteză/așteptări în Setări → FOREXE (cererea operatorului, 29.09.2026)** | GATA pe cod (build curat; nimic rulat, nimic testat) | `SLICE-0091-detaliu-taiat-viteza-asteptari.md` | Server: `detaliu_incomplet` în pasul 4b (rând fără valori / linii ≠ Suma / mai puțini indicatori decât angajamentul) ▸ recepția existentă rămâne neatinsă, cea nouă se creează doar cu rândurile întregi; F14/F15 doar semnalează pe ele; `receptii_incomplete` în ambele faze. Client: întrebare «Le citesc din nou acum?» după salvare (doar zilele tăiate). Setări → FOREXE: «Testează viteza» (fast.com, Chromium ascuns), «Validează de 2×» (doar sub 20 Mb/s), multiplicator ×1/1,5/2/3 pentru timpii WFL + așteptarea Ajax. `F14_PAUSED` / `F14Paused` = True (pauza cerută de operator). |

### Open threads

- **0091 — nimic rulat.** Testul de viteză depinde de pagina fast.com (`#speed-value.succeeded`, `#speed-units`); dacă își schimbă pagina, butonul spune că măsurarea a eșuat.
- **0091 — presupunere:** semnul 3 («mai puțini indicatori decât angajamentul») se sprijină pe un singur angajament (017_SCNB / AAB2DH3X6SK), unde fiecare recepție completă lista toți cei 7 indicatori. Dacă FOREXE listează altundeva doar o parte, recepțiile acelea vor fi semnalate mereu și RHR-ul lor nu se va mai actualiza — atunci semnul 3 trebuie scos.
- **0091 — F14 e încă în pauză** (`F14_PAUSED` în `prelucrare_asociere.py`, `F14Paused` în `AsociereForm.vb`). Scutirea țintită pe recepțiile tăiate o face inutilă; operatorul decide când se repune.
- **0091 — RHR-ul deja stricat** (017_SCNB: recepțiile 84, 86, 90) se repară la prima recitire în care detaliul lor vine întreg: pasul 4b adaugă liniile lipsă și corectează valorile.

---

## Slice 0092

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0092-01 | **Rezervări greșite: butonul «Repară», rândul roșu în arborele principal și ștergerea ultimei luni din istoric (cererea operatorului, 12.10.2026)** | GATA pe cod (build curat KBot.App; nimic rulat pe MariaDB sau pe ecran; **serverul trebuie redeployat**) | `SLICE-0092-01-rezervari-gresite-sterge-luna.md` | August 2026 al unui utilizator pornește de la valorile lui iunie (14 rânduri; verificat pe `Rezervari.txt`). `tree.py` coloana `RezervariGresite` → rând roșu + hover; iconița dreapta-jos din «Rezervări» = «Repară» (reanalizarea existentă); Setări › FOREXE › «Istoric (avansat)» șterge ultima lună din istoric (fără DDF/ORD; cascadă + recepțiile lunii, fără orfane). Ajutor 0000-68; capturile `rezervari*.png` de refăcut. |
| 0092 | **Rezervarea inițială = un singur eveniment, pe ultima zi inițială (cererea operatorului, 29.09.2026)** | GATA pe cod (build curat; nimic rulat) | `SLICE-0092-rezervare-initiala-o-singura-zi.md` | AAB5H2CHDGD: AA2 adăugat «În definitivare» pe 27.08, AAB definitivat pe 28.08 → două rânduri `EInitiala` pe zile diferite → două frunze «Inițială». Varianta A (doar cititorii, fără rescriere în bază): `RezervariView.BuildTree` pune toate rândurile inițiale pe ultima zi inițială; `_SQL_GEN_REZERVARI` folosește aceeași zi (`ZiRez`) pentru regula «cea mai veche zi, cel mai mic tip» și ca dată a reviziei. |

### Open threads

- **0092 — AAB5H2CHDGD are DDF doar pe AAB:** AA2 e legat de revizia 262 (`AreDDF = 1`). Dacă 262 conține și AAB, cauza e pasul 3e cazul 2 (exclude revizia «deja folosită», deși o revizie acoperă mai multe rânduri). Aștept rezultatul interogării pe `FX_DDF_REV` / `FX_DDF_REV_SA`.
- **0092 — revizii 0 în plus?** Două «lock NUMARREV=0» în jurnal (12:35, 12:40).

---

## Slice 0093

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0093 | **Parteneri: filtrare, ANAF, banca din IBAN, cod fiscal unic (cererea operatorului, 29.09.2026)** | GATA pe cod (build curat; nimic rulat, nimic testat) | `SLICE-0093-parteneri-anaf-cf-unic.md` | `GET /parteneri` întoarce doar Tip = '1', CodFiscal completat și ≠ `AVACONT_COMUN.Unitati.CF` (DC-ul sesiunii), `Ascuns = 0`, un singur partener pe cod fiscal (primul după IdPartener; codurile comparate doar pe cifre). Banca goală → din IBAN (car. 5-8) prin `AVACONT_COMUN.BIC`, pe server (GET + POST) și în fereastră la tastarea IBAN-ului. Rută nouă `GET /parteneri/anaf/<cf>` (refolosește `routes/inregistrare/anaf.py`, v9). Fereastra: Tip și «Arată partenerii ascunși» scoase, «Alte detalii» → «Adresa», «Cod fiscal *» obligatoriu; la ieșirea din câmp / Enter se caută la ANAF și se completează denumirea + adresa. Salvarea refuză codul unității și un cod fiscal deja folosit (verificare în Python, NU constrângere MariaDB — datele din Access au dubluri; un partener vechi cu cod dublat rămâne editabil cât timp nu-și schimbă codul). Tip scris mereu '1'. |

### Open threads

- **0093 — nimic rulat.** Nici ruta ANAF, nici fereastra nu au fost încercate pe server/ecran.
- **0093 — partenerii ascunși / cu alt Tip / fără CF nu mai apar deloc** în fereastră; un partener marcat «Ascuns» la salvare dispare din listă. Cererea operatorului, dar nu mai există cale din K-BOT de a-l reafișa.
- **0093 — cod dublat ascuns:** verificarea de unicitate pe server include și partenerii ascunși sau cu alt Tip (mesajul spune «partener ascuns»); fereastra nu-i vede, deci operatorul află doar la salvare.

---

## Slice 0094

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0094 | **`KBotComboBox` rescris pe `Control` (nu mai moștenește `ComboBox`) și folosit ca editor de combo în `KBotDataView` (cererea operatorului, 29.09.2026)** — fără `DataSource`/`DisplayMember` (elementele în `Items`, afișate prin `CaptionSelector` sau `ToString()`); textul tastat într-o casetă fără chenar așezată ca textul pictat (rețeta 0085); O SINGURĂ listă: săgeata o deschide cu toate elementele, tastarea (`FindAsYouType`) cu cele potrivite; se închide la un clic în afara ei; bară de derulare trasă cu mouse-ul. În grilă: editorul de combo stă pe o linie, centrat ca editorul de text, peste textul celulei; săgeata celulei (pictată de grilă) deschide lista sub celulă; textul liber rămâne la `CellValidating` | GATA pe cod (build Controls/App/Migrator/Theming/Updater **0 / 0**) / **nevăzut pe ecran, fără teste** | `SLICE-0094-combo-own-control-grid-editor.md` | 14 fișiere Designer curățate de liniile ComboBox (`DrawMode`, `DropDownStyle`, `FlatStyle`, `IntegralHeight`, `ItemHeight`, `FormattingEnabled`). An/SS și unitatea de la login: `Items` în loc de `DataSource`. Controls 1.57.0.0, DevHarness 1.0.29.0. |
| 0094-02 | Editor DDF — partener principal și listă asociată | GATA pe cod (build curat, nerulat, nevăzut; SQL-ul și serverul nedeployate) | [0094-02](../SLICE-0094-02-ddfedit-pagina-parteneri.md#slice-0094-02) | Tabela/rutele/controlul: 0084-02; ajutor 0000-28. Contracte și ordine în jurnal. |
| 0094-03 | Editor DDF — fila Parteneri urmează bifa | GATA pe cod (build curat, nevăzut) | [0094-03](../SLICE-0094-02-ddfedit-pagina-parteneri.md#slice-0094-03) | Partenerii non-principali ascunși rămân salvați; decizia este deschisă. |

### Current focus

- **Slice 0094 — combo-ul propriu (29.09.2026).** `KBotComboBox` e un `Control` cu casetă de text,
  listă proprie și colecție de elemente proprie; grila îl folosește ca editor de combo, așezat ca
  editorul de text. **Next:** operatorul îl vede în aplicație (grila DDF Secțiunea A, Parteneri,
  combo-urile din Setări / login / MainForm).

### Open threads

- **0094 — nimic văzut pe ecran.** Alinierea la pixel a editorului de combo cu textul celulei se
  sprijină pe rețeta măsurată la 0085 pentru editorul de text; pentru combo nu s-a măsurat.
- **0094 — `AssemblyVersion` a KBot.Controls a rămas 1.0.0.0** deși tipul de bază al
  `KBotComboBox` s-a schimbat; merge doar pentru că actualizarea livrează tot setul.
- **0094 — cod rămas fără folosință:** ajutoarele pentru EDIT-ul nativ din
  `KBot.Theming/Interop/NativeMethods.vb` (`GetComboEditBounds`, `SetComboEditMargins`,
  `SetComboEditBounds`, `GetComboEditTextTop`, `GetComboEditLineHeight`, `ApplyControlColors`).
- **0094-02:** [ordine de instalare și probe UI](../SLICE-0094-02-ddfedit-pagina-parteneri.md#slice-0094-02); nimic văzut/deployat la checkpoint.
- **0094 — diferențe de comportament:** rotița peste un combo ÎNCHIS nu mai schimbă selecția;
  `Items.Clear()` pe un combo editabil păstrează textul tastat.

---

## Slice 0095

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0095 | **Istoric: nod rădăcină «Tot istoricul» (cererea operatorului, 29.09.2026)** — arborele Istoric are acum un nivel rădăcină, ca celelalte arbori (Extrase: «Toate extrasele»); lunile stau sub el, zilele sub luni. Clic pe rădăcină = ce face vederea la încărcare: toate filtrele golite, toate rândurile FX_Istoric ale angajamentului | GATA pe cod (build KBot.App **0 erori**) / **nevăzut pe ecran, fără teste** | [0095](../SLICE-0022-01-istoric-endpoint.md#slice-0095) | Doar `IstoricView.vb`: `NodPerioada.Tot()` + `EsteTot`. Numărul din dreapta rădăcinii = toate rândurile, inclusiv cele fără dată (până acum fără nod în arbore). Clicul pe rădăcină renunță și la intervalul cerut de FOREXE (0073). FileVersion KBot.App nebumped (îl cere `push-update.ps1`). |
| 0095-02 | Extrase — subsolul, fereastra și lățimea meniului | GATA pe cod (build KBot.App **0 avertismente, 0 erori**) / **nevăzut pe ecran, fără teste** | [0095-02](../SLICE-0096-extrase-display-menu.md#slice-0095-02) | Subpas separat de Istoric 0095; ipoteze, limite și FileVersion în jurnal. |

### Open threads

- **0095-02:** [butonul strâns, alinierea și tooltipul](../SLICE-0096-extrase-display-menu.md#slice-0095-02); nevăzute.

---

## Slice 0096

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0096 | Extrase — antet/operații sau operații/detalii | GATA pe cod (build KBot.App **0 avertismente, 0 erori**) / **nevăzut pe ecran, fără teste** | [0096](../SLICE-0096-extrase-display-menu.md#slice-0096) | Ipotezele privind data, gruparea și păstrarea alegerii sunt în etapă. |

### Open threads

- **0096:** [ipotezele de afișare și alegere a modului](../SLICE-0096-extrase-display-menu.md#slice-0096); de confirmat cu operatorul.

---

## Slice 0097

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0097 | Corecturi inițiale: meniuri, unități și autentificare | GATA pe cod (build KBot.App **0 avertismente, 0 erori**) / **nevăzut pe ecran, fără teste; server nedeployat** | [0097](../SLICE-0097-corective.md#slice-0097) | Regulile inițiale pentru avansate sunt amendate în 0097-04/05; probele și limitele rămân pe etapă. |
| 0097-02 | A doua trecere: tur, browser și recepție pe aceeași dată | GATA pe cod (build KBot.App **0 avertismente, 0 erori**; `node --check` pe script) / **nerulat, nevăzut pe ecran, fără teste** | [0097-02](../SLICE-0097-corective.md#slice-0097-02) | Contracte, ajutor, probe și limite în etapa legată. |
| 0097-03 | Anul și Sursă/Sector în bara de titlu | GATA pe cod (build **0 avertismente, 0 erori**) / **nevăzut pe ecran**, nimic rulat | [0097-03](../SLICE-0097-corective.md#slice-0097-03) | Regula de ascundere la sortare este amendată în 0097-06; ajutor 0000-33, captura restantă în jurnal. |
| 0097-04 | Parola memorată: aplicare independentă de avansate | GATA pe cod (build curat, netestat, nevăzut) | [0097-04](../SLICE-0097-corective.md#slice-0097-04) | Contracte, ajutor, probe și limite în etapa legată. |
| 0097-05 | Avansatele permit schimbarea setărilor salvate | GATA pe cod (build curat, netestat, nevăzut) | [0097-05](../SLICE-0097-corective.md#slice-0097-05) | Contracte, ajutor, probe și limite în etapa legată. |
| 0097-06 | Sursa «Toate» și evenimentul global de perioadă | GATA pe cod (build curat, netestat, nevăzut) | [0097-06](../SLICE-0097-corective.md#slice-0097-06) | Contracte, ajutor, probe și limite în etapa legată. |

### Open threads

- **0097:** [server, testul vechi, eticheta DPI și schimbarea unității](../SLICE-0097-corective.md#slice-0097); încă neîncercate la checkpoint.
- **0097-02:** [probele vizuale, confirmările și recepția pe aceeași dată](../SLICE-0097-corective.md#slice-0097-02); workflow-ul nou nerulat și apelul direct din afara cozii sunt consemnate acolo.

---

## Slice 0098

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0098 | Coada robotului și poarta serverului — inițial | GATA pe cod (build KBot.App **0 avertismente, 0 erori**) / **nevăzut pe ecran, fără teste** | [0098](../SLICE-0098-robot-queue-server-gate.md#slice-0098) | Blocarea citirilor este înlocuită în 0098-02; ajutorul și versiunile la checkpoint sunt în jurnal. |

### Open threads

- **0098/02:** [poarta, ordinea, fereastra, modalele și apelanții direcți](../SLICE-0098-robot-queue-server-gate.md); nimic rulat/văzut la checkpoint. Bancul 02 rămâne nerulat; numai scrierile sunt ținute după amendament.

### 0098-02

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0098-02 | Citirile trec, banc simulat și deschidere peste o acțiune | GATA pe cod (build Debug + Release **0 avertismente, 0 erori**) / **bancul nerulat** | [0098-02](../SLICE-0098-robot-queue-server-gate.md#slice-0098-02) | Bancul scrie fișiere PROBA-* și mesaje [SIMULARE]; nu a fost rulat. |

---

## Slice 0099

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0099 | **`PrintCount`: de câte ori a fost trimis la imprimantă un document (cererea operatorului, 01.10.2026)** — coloană `PrintCount int NOT NULL DEFAULT 0` pe `FX_DDF_PDF`, `FX_ORD_PDF`, `FX_NoteCAB_PDF`, `FX_NoteCAB_Recipisa` și, pentru documentele NESEMNATE (fără rând de PDF), pe `FX_DDF_REV` și `FX_ORD` (`sql/0099_print_count.sql`). K-BOT nu are comandă de tipărire: operatorul tipărește din Adobe, iar K-BOT vede lucrarea în coada de tipărire Windows (`AdobePrintWatcher` + `PrintSpooler`, un singur fir pentru tot procesul, doar cât e un document afișat); fereastra de tipărire Adobe, văzută de capcana «Salvare ca», doar îndesește citirea cozii. O lucrare nouă care poartă numele fișierului afișat = o tipărire → `POST …/print` (`routes/forexe/print_count.py`); serverul alege rândul (PDF-ul păstrat, altfel rândul documentului). «Microsoft Print to PDF» se numără. Legat în `DdfView` / `OrdView` / `NoteCabView` prin `PdfPrintTarget` → `ReaderHostPreview`, și în bancul de semnare (`PdfSigningHarnessForm`). **Partea 2 (aceeași zi, tot 0099): lista de tipărire** — pe o lună sau pe rădăcina «Toate…» din ORD și DDF, dreapta arată `PrintListPage` (grilă: bifă cu meniu în antet «toate» / «doar nelistate», Document, Semnături, Semnat la, Listat, Nr. tipăriri) cu «Generează și imprimă» (documentele pregătite și trimise la imprimantă prin verbul `printto`, fără să fie deschise) și «Salvează local» (dosar ales); «Listat» bifat la 0 tipăriri întreabă o dată, «Da» = o tipărire numărată, fără mesaj după. Rutele de listă `GET /api/forexe/ddf` / `/ord` întorc `print_count` (PDF + document; 0 unde SQL-ul nu a rulat) | GATA pe cod (build KBot.App Debug + Release **0 avertismente, 0 erori**) / **nimic rulat: nicio tipărire, nicio rută chemată, SQL neaplicat; fără teste** | `SLICE-0099-print-count.md` | Partea 1 nu are nimic vizibil; partea 2 e vizibilă și e în ajutor (`contabil.vederi.ord`, `contabil.ddf`, etichetă 0099). KBot.Controls 1.59.0.0, KBot.Api 1.0.17.0, KBot.Domain 1.2.9.0; KBot.App la `push-update.ps1`. Ordinea de deploy: SQL-ul pe fiecare bază + `AVACONT_SURSA`, apoi `print_count.py` + `__init__.py` pe VPS, apoi clientul. `PrintCount` nu e afișat nicăieri. |

### Open threads

- **0099 — nimic rulat.** De probat pe banc (DevHarness → semnarea PDF): Ctrl+P în Adobe, apoi
  liniile «[Adobe] Tipărire…» din jurnal.
- **0099 — numele lucrării Adobe e o presupunere** (numele fișierului). Dacă e altul, nu se
  numără nimic; jurnalul arată lucrarea cu «nu poartă numele unui document deschis în K-BOT».
- **0099 — titlul / clasa ferestrei de tipărire Adobe sunt presupuneri**; dacă sunt greșite se
  pierde doar citirea deasă (250 ms), numărarea rămâne.
- **0099 — «Microsoft Print to PDF» și capcana «Salvare ca» (risc nemăsurat):** fereastra în care
  acea imprimantă cere locul fișierului e o fereastră standard de fișier; dacă aparține
  procesului Adobe, capcana feliei 0078 îi scrie calea documentului și apasă «Salvare». Capcana
  NU a fost schimbată aici.
- **0099 — imprimante de rețea:** Windows anunță doar cozile calculatorului; pe cele de rețea
  lucrarea se găsește la citirea periodică (2 s / 250 ms).
- **0099 — sesiune expirată:** apelul merge direct la client (fără reautentificare); tipărirea
  se scrie în jurnal ca nenumărată. O notă CAB fără PDF păstrat nu se numără.
- **0099 (lista de tipărire) — nimic rulat, nimic văzut pe ecran.** De probat: clic pe o lună / «Toate» în ORD și DDF; meniul din antetul bifelor; «Listat» la 0 tipăriri; «Salvează local»; «Generează și imprimă» pe o imprimantă reală.
- **0099 (lista de tipărire) — tipărirea fără fereastră merge prin verbul `printto` al programului de PDF** (Adobe Reader îl are). Fără un program de PDF care îl înregistrează, fiecare document se raportează cu mesajul shell-ului. Copiile cerute în fereastra imprimantei se fac prin repetarea trimiterii; numărătoarea crește cu 1 pe document. Nu se verifică dacă lucrarea a ajuns pe hârtie — doar că programul de PDF a terminat.
- **0099 (lista de tipărire) — documentul semnat se tipărește așa cum e pe server** (pentru o revizie DDF semnată doar pe A și trimisă, fără Secțiunea B pusă în fișier). Cele nesemnate se generează în `TempPdf`, ca «Generează».
- **0099 — de deployat:** `sql/0099_print_count.sql` (în `AVACONT_SURSA` întâi `sql/0088_04`,
  tabela `FX_NoteCAB_Recipisa` lipsește acolo în dump-ul din 22.09.2026), `print_count.py`,
  `routes/forexe/__init__.py`. `sql/AVACONT_SURSA.sql` și `MariaDB_Schema/` neactualizate.
