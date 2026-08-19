# SLICE-0042 — Migrarea tabelelor `FX_` din Access în MariaDB

**Data:** 2026-08-19
**Felia:** 0042 (numărul liber următor din registru — planul spunea „de atribuit de Adelina";
`KBOT_STATUS.md` declara 0042 ca următorul liber, deci l-am luat pe acela. Dacă vrei alt
număr, redenumește fișierul și rândul din registru.)
**Plan:** lipit în sesiune.

---

## Ce s-a schimbat și de ce

Forma cerută: **doi pași care nu se ating**. Un modul VBA exportă în fișiere JSON; un
utilitar VB.NET citește fișierele și scrie în MariaDB prin rutele Flask de seed. Din .NET
nu se referă niciun driver Access — nici OleDb, nici ACE, nici COM. Ăsta e chiar motivul
pentru care exportul e VBA: rulează înăuntrul aplicației FOREXE, unde DAO există deja, deci
nu există nici problemă de driver, nici de 32/64 de biți pe partea .NET.

### Partea A — `mdl_FX_ExportSeed` (VBA)

`Surse/VBA_MIGRARE/mdl_FX_ExportSeed.bas`. Modul NOU, separat; `mdl_FX_ExportMD` rămâne
neatins și nu e apelat de nicăieri. L-am citit pentru stilul casei și apoi l-am lăsat în
pace — pentru o migrare e descalificat pe patru puncte, fiecare dintre ele pierzând sau
stricând date: trunchiază Memo la `Left(s,5) & "..." & Right(s,5)`, colapsează NULL la `""`
prin `Nz`, scoate fiecare valoare ca șir cu ghilimele în locale românesc, și recursează câte
o interogare pe rând-părinte.

Ce face cel nou:

- **Listă fixă de 16 tabele**, în ordinea părinți-înainte-de-copii. Nicio descoperire prin
  `DB.Relations`, niciun potrivit de prefix — descoperirea e exact ce cade azi. Un tabel din
  listă absent din `TableDefs` e eroare dură, cu numele lui.
- **`[Cai]` din `cale.accdb`** → `Cai.json`, cu toate coloanele. `IdUnitate → DC` e cheia de
  rutare de care depinde toată migrarea, iar migratorul nu are voie să deschidă Access.
- **Chunk-uri**: 500 de rânduri/fișier, **50** pentru cele grele la Memo (`FX_Receptii_IMG`,
  `FX_Rezervarii_IMG`, `FX_Extrase_F`, `FX_Istoric`, `FX_Receptii`). Forma fișierului e exact
  cea pe care `POST /api/forexe/seed/rows` o acceptă deja: tablouri poziționale + o listă de
  coloane comună.
- **Conversia valorilor** — partea care contează cel mai mult. `Null → null` (niciun `Nz` în
  tot modulul); `dbBoolean → 0/1` (Access are `True = -1`, care nu are ce căuta pe fir);
  `dbDate → "yyyy-mm-dd hh:nn:ss"` cu separatorii **escapați** (`\:` — în VBA `:` e
  substituentul separatorului de oră din locale, nu un literal); numeric prin `Str$`, singurul
  formatator independent de locale (`CStr` pe o stație românească scoate virgulă și ar corupe
  fiecare sumă); text/Memo **întreg, fără trunchiere**; șirul gol rămâne șir gol, distinct de
  NULL.
- **UTF-8 real** prin `ADODB.Stream` (late-bound, deci fără referință ADO), cu BOM-ul scos
  copiind într-un al doilea stream binar de la poziția 3. Diacriticele pleacă ca ele însele,
  niciodată ca `\uXXXX`.
- **Scriere direct în stream**, niciodată acumulare într-un `String`: concatenarea VBA e
  pătratică și `FX_Istoric` are singur în jur de 24.000 de rânduri. Un `dbOpenSnapshot`
  forward-only pe tabel, `SELECT *` fără `WHERE` și fără `ORDER BY`.
- `manifest.json` se scrie **ultimul**, fiindcă abia atunci se știu numerele de rânduri.

Două lucruri prinse la scris, care ar fi produs JSON invalid:

1. `Str$` **taie zeroul din față**: `Str$(0.1)` e `" .1"`, iar `.1` e respins de orice parser
   strict. `JsonNumber` îl pune la loc, și pentru negativ.
2. `Format$(Hex$(i), "0000")` nu umple cu zerouri un cod de control: pentru 11, `Hex$` dă
   `"B"`, iar un format numeric aplicat lui `"B"` întoarce `"B"`. Umplerea se face de mână,
   `Right$("0" & Hex$(i), 2)`.

### Partea B — Flask (`PYTHON/routes/forexe/seed.py`), strict aditiv

**`mode` pe `/seed/rows`.** Ruta scria dintotdeauna `ON DUPLICATE KEY UPDATE <toate
coloanele>`, adică Access suprascrie MariaDB — exact opusul deciziei 3. Câmpul nou e opțional
și implicit `"overwrite"`, deci apelanții existenți nu se schimbă. `"insert_missing"` emite
`ON DUPLICATE KEY UPDATE <prima coloană> = <prima coloană>`, o auto-atribuire fără efect.

Auto-atribuirea, și **nu** `INSERT IGNORE`: `IGNORE` degradează la avertisment și erorile de
tip, trunchierile și violările de constrângere, adică ar înghiți eșecuri reale. Forma aleasă
suprimă exclusiv cazul cheii duplicate. Sub ea `rowcount` e 1 pe rând inserat și 0 pe duplicat
sărit, deci răspunsul poartă `inserted` și `skipped` exacte. `truncate_first` împreună cu
`insert_missing` → `400`: golești tabelul și apoi ceri să nu suprascrii nimic.

**`GET|POST /seed/ids`**, read-only. Allow-list **proprie și îngustă** —
`{("FX_DDF","IDDF"), ("FX_DDF_REV","IDREV")}` — nu `ALLOWED_TABLES`, fiindcă ruta citește
tocmai tabelele pe care seed-ul are interzis să le scrie. Valorile sunt întregi și intră în
SQL doar parametrizate; identificatorii vin exclusiv din perechea de mai sus. Varianta POST
există pentru loturi prea mari pentru un URL. Garda rămâne `@require_api_key`, mesajele
românești cu diacritice reale, `ensure_ascii=False`.

O abatere conștientă de la `/columns`: acolo un tabel inexistent întoarce `200` cu listă
goală, fiindcă lista goală e un răspuns cu sens. Aici întoarce **500 cu mesaj explicit**, nu
`200` cu «toate lipsesc» — alea două sunt diagnostice complet diferite, iar al doilea ar minți.

### Partea C — `KBot.Migrator`

Proiect WinForms nou (`net8.0-windows`, `Option Strict On`, `RootNamespace` setat, fără
blocuri `Namespace`). Referă `KBot.Common`, `KBot.Theming` și `KBot.Api`.

`KBot.Api` intră **doar** pentru `ApiOptions.DefaultBaseUrl` — adresa serverului are un singur
loc unde e scrisă — plus garda https de acolo. `ApiClient` nu se folosește: rutele de seed
sunt păzite cu `X-Api-Key`, nu cu tokenul bearer.

`KBot.Forexe` (unde stă facada `KBotTheme`) e **evitat intenționat**, deși planul îl numea: ar
târî Playwright într-un utilitar care nu deschide niciun browser. Regula din `CLAUDE.md` e
oricum cea mai nouă — culorile vin din `ThemeManager.Current.Palette`, iar formularul
moștenește `KBotThemedForm` și nu-și scrie nicio culoare.

Structura:

| Fișier | Rol |
|---|---|
| `ExportArtifacts/SeedTables.vb` | cele 16 tabele + regula de rutare a fiecăruia (`RoutingKind`) |
| `ExportArtifacts/ExportManifest.vb`, `ChunkFile.vb`, `JsonValues.vb` | POCO-uri + citiri mici de valori |
| `ExportArtifacts/ArtifactReader.vb` | manifest, verificarea fișierelor, citirea chunk-urilor |
| `Routing/RoutingMaps.vb` | hărțile Cai / A / B / C / D / E, construite toate înainte de orice scriere |
| `Routing/RowRouter.vb` | rutarea unui rând, cu indexurile rezolvate o dată per chunk |
| `Api/SeedApiClient.vb` | `/seed/columns`, `/seed/ids`, `/seed/rows` cu `insert_missing` |
| `Run/RunLog.vb`, `MigrationStats.vb`, `MigrationRunner.vb` | jurnal + contori + pașii 1–7 |
| `MigratorForm.vb` / `.Designer.vb`, `Program.vb` | ecranul cu patru regiuni + plasele globale |

Detalii care merită ținute minte:

- **Valorile nu se re-tipizează niciodată pe drum.** Celulele se păstrează ca `JsonElement`
  clonate și se scriu înapoi cu `WriteTo`. Un `Decimal(28,6)` trecut printr-un `Double` .NET
  s-ar rotunji tăcut, iar o migrare n-are voie să schimbe nicio valoare.
- **Două treceri de rutare, memorie mărginită.** Verificarea rutează o dată ca să numere și să
  culeagă id-urile DDF, apoi uită rândurile; transferul recitește fișierele. Altfel
  `FX_Istoric` + `FX_Extrase_F` ar trebui ținute întregi în memorie.
- **`FX_Extrase_F` se multiplică intenționat.** Un fișier de extras poate purta linii pentru
  mai multe unități, deci același rând aparține legitim mai multor baze. Contorul
  `Duplicated` ține copiile suplimentare, ca socoteala de la pasul 7 să închidă exact.
- **Cei doi părinți care nu sunt de acord opresc rularea.** La `FX_Receptii_IMG` și
  `FX_Receptii_Plati` se încearcă primul părinte cu retragere pe al doilea; dacă amândoi sunt
  prezenți și duc la DC-uri diferite, e eroare dură, nu retragere — a alege unul ar fi o
  ghiceală care mută date în baza greșită.
- **Orfanii nu se pierd.** Un rând a cărui cheie de rutare nu se rezolvă nu se scrie și nu
  dispare: pleacă în CSV-ul de respinse cu cheia primară și motivul, iar rularea continuă.
- **Folderul se cheamă `ExportArtifacts/`, nu `Artifacts/`, dintr-un motiv.** `.gitignore:66`
  are regula `artifacts/` (folderul de publish din rădăcină), iar Git pe Windows o potrivește
  fără să țină cont de majuscule — deci cele cinci fișiere din `Artifacts/` au fost ÎNGHIȚITE
  tăcut la `git add -A`, în timp ce build-ul local mergea perfect. O clonă proaspătă n-ar fi
  compilat. Redenumit, nu forțat cu `git add -f`: următorul om care adaugă un fișier acolo n-ar
  fi știut că trebuie forțat.

- **Nimic nu creează sau modifică vreun tabel.** O coloană prezentă în export și absentă pe
  țintă oprește tabelul cu numele ei; un tabel inexistent oprește tabelul. Schema se instalează
  separat, prin `AVACONT_COMUN`.

## Fișiere atinse

| Fișier | Ce |
|---|---|
| `Surse/VBA_MIGRARE/mdl_FX_ExportSeed.bas` | **NOU** — exportatorul VBA |
| `PYTHON/routes/forexe/seed.py` | câmpul `mode` pe `/rows` + ruta `/ids` + antetul actualizat (3 → 4 endpoint-uri) |
| `src/KBot.Migrator/**` | **NOU** — 12 fișiere, proiectul utilitarului |
| `KBot.sln` | proiectul nou adăugat |
| `docs/worklog/SLICE-0042-migrare-fx-access.md` | **NOU** — acest fișier |
| `docs/worklog/KBOT_STATUS.md` | rând nou în registru + «următorul număr liber» |

## Rezultate teste

- **Python**, prin `PYTHON/.venv` → `python -m pytest tests/ -q`:
  **75 passed, 15 skipped, 0 fail/error.** Identic cu înainte de modificare — suita nu
  acoperă (încă) rutele noi.
- **VB.NET**: `dotnet build src/KBot.Migrator/KBot.Migrator.vbproj -c Debug` →
  **0 avertismente, 0 erori**. `dotnet build KBot.sln -c Debug` → **0 erori**, cele 5
  avertismente sunt cele preexistente din `KBot.App` (`MSB3825`, `BinaryFormatter` în `.resx`),
  neatinse de felia asta.
- **Teste noi: niciunul.** Planul, §6, spune că implicit nu se scriu și că se scriu doar dacă
  le confirmi. Nu le-am scris.

## Rămâne NEVERIFICAT sau amânat

1. **Nicio linie din felia asta nu a atins o bază reală, un fișier Access real sau un ecran.**
   Modulul VBA nu a fost importat în Access și nu a rulat niciodată; utilitarul nu a fost
   pornit; rutele Flask noi nu au fost lovite de un client real. Tot ce urmează decurge din
   asta.
2. **Verificarea de round-trip în designerul VS a lui `MigratorForm` e a ta, nu a mea** — așa
   cerea planul. Toate controalele sunt declarate în `.Designer.vb`, iar ordinea de andocare e
   cea inversă din regula casei, dar formularul nu a fost deschis în designer.
3. **`Cai.json` — forma nu e specificată în plan.** Am ales aceeași formă ca un chunk
   (`table`/`columns`/`rows`), ca migratorul să-l parseze cu același cod. `[Cai]` NU e listat
   printre `manifest.tables`; are o intrare proprie, `manifest.cai`.
4. **Coloanele lui `[Cai]`** nu sunt documentate nicăieri în depozit — `cale.accdb` nu e
   exportat. Că are `IdUnitate` și `DC` e **verificat**, dar indirect: din
   `FX_System_Export/MODULES/mdl_FX_Tasks_Receive_DWN.md`, care face
   `INNER JOIN Cai ON CG.IdUnitate=Cai.IdUnitate ... AND Cai.DC='...'`. Că `IdUnitate` e cheia
   primară e presupunerea 1 din plan, confirmată de tine, nu de un fișier.
5. **Presupunerea 3 este acum verificată:** `FX_Angajamente` **are** coloană `DC` (Text 255) —
   `FX_System_Export/TABLES/FX_Angajamente.md`, iar datele-eșantion o arată completată
   (`045_CTER`). Ramura de retragere pe `IdUnitate` rămâne implementată, ca plasă.
6. **`rowcount` sub `mysql-connector-python`.** Driverul e `mysql.connector`, iar `inserted` /
   `skipped` se bazează pe faptul că `executemany` rescrie INSERT-ul cu `ON DUPLICATE KEY
   UPDATE` într-un singur enunț multi-rând și că `rowcount` iese 1/inserat și 0/duplicat
   (adică fără `CLIENT_FOUND_ROWS`). Aia e comportarea documentată, dar **nu a fost măsurată
   aici**. Dacă iese altfel, socoteala de la pasul 7 va reclama, ceea ce e exact ce trebuie să
   facă — nu va scrie greșit, va raporta greșit.
7. **Cheia API nu e în planul de ecran.** Rutele de seed sunt păzite cu `X-Api-Key` și fără ea
   utilitarul nu poate vorbi cu serverul, așa că am adăugat un câmp mascat în regiunea
   «Surse», preîncărcat din variabila de mediu `KBOT_SEED_API_KEY` dacă e pusă. Nu se scrie
   nicăieri pe disc.
8. **Respinsele nu se pot atribui unui DC** — prin definiție, un rând respins e unul care nu se
   rutează nicăieri. Planul cere fișiere per DC; ca fiecare fișier să fie complet citit singur,
   respinsele se scriu în CSV-ul **fiecărui** DC selectat, la fel ca secțiunile globale ale
   jurnalului. Dacă preferi un singur fișier de respinse pe rulare, e o schimbare mică.
9. **`FX_PRT_EXPL` și `FX_CopacAngajamente`** rămân în afara domeniului și **neverificate**, ca
   în plan. Exportatorul le caută în `TableDefs` și, dacă există, le **raportează** (în
   `manifest.unexpected_tables` și în caseta finală) fără să le exporte.
10. **Numărul feliei.** Planul îl lăsa în seama ta; l-am luat pe 0042, următorul liber declarat
    în registru. Registrul are în continuare gaura cunoscută la 0038/0039 — n-am atins-o.
