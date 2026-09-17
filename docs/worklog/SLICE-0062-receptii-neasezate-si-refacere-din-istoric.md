# SLICE 0062 — Anteturile fără recepție se VĂD, și refacerea din istoric a ce lipsește din `FX_Receptii_H` / `FX_Receptii`

**Data:** 15.09.2026
**Cerute de operator** (două propoziții):

1. «`ReceptiiH` își pierde `IDRH` și `IDRR` în Access; la migrare rândurile ajung pe MariaDB,
   dar `Receptii_H` n-are `IDRR` în niciun câmp, așa că serverul spune că NU sunt recepții de
   încărcat — dar sunt, doar că nu-s legate de `Receptii_R` (`IDRR IS NULL`).»
2. «Am nevoie de o cale, în vederea Recepții sau în `IstoricView`, să refac toate rândurile care
   lipsesc din `Receptii` sau `Receptii_H`, după `IDH`.»

S-a ales vederea Recepții pentru amândouă: acolo e simptomul (lista goală) și tot acolo sunt
deja editorul de legături (antet) și reîmprospătarea (subsol dreapta). `IstoricView` rămâne
read-only, cum e declarată.

---

## 1. De ce dispăreau (cererea 1)

`GET /api/forexe/receptii` pornea din `FX_Receptii_R` cu `INNER JOIN FX_Receptii_H ON
H.IDRR = R.IDRR` — portul literal al lui `qFX_MAIN_REC_TREE`. Un antet cu `IDRR NULL` nu se
potrivește cu nicio recepție, deci cădea din rezultat; când TOATE anteturile sunt așa,
răspunsul e `receptii: []` și vederea spune «Angajamentul nu are recepții». Serverul nu mințea
despre ce a găsit — mințea interogarea, prin ce alegea să nu caute.

**Schimbarea:** rădăcina interogării e acum `FX_Receptii_H`, cu `LEFT JOIN FX_Receptii_R` și
`WHERE H.CodAngajament = %s`. Un antet neașezat vine cu toate câmpurile de recepție `NULL`
(`idrr: null`, `data_r: null`, `suma_antet: 0.0`, steagurile `false`). Restul rămâne: `LEFT
JOIN FX_Receptii`, `LEFT JOIN FX_Indicatori`, subinterogările scalare pe `Clasificatii`,
aceeași ordine (`R.NRCRT, R.DataR, H.NrCrt, H.DataH, Rc.IDR` — NULL-urile lui R vin primele în
MariaDB, dar clientul oricum grupează). O recepție FĂRĂ niciun antet nu apare — nici în Access
nu apărea.

**Pe fir:** `GetReceptieRow.idrr` devine `Integer?`; `ReceptieRow.Idrr` rămâne `Integer`, cu
**0 = neașezat** — aceeași convenție pe care o are deja `AsociereInfo` (`idrr = 0` = «pe nicio
recepție»). Un `null` JSON într-un `Integer` nenulabil ar fi picat deserializarea întregului
răspuns, deci schimbarea DTO-ului nu e cosmetică.

**În arbore (`ReceptiiView.BuildTree`):** rândurile se despart în așezate (`Idrr > 0`) și
neașezate. Lunile și tooltip-urile de reconciliere se calculează DOAR peste cele așezate — un
antet fără `DataR` n-are lună și n-are ce căuta într-un cumul. Neașezatele merg într-un dosar
nou, **ultimul**, «Instantanee neașezate», cu totalul = `Sum(Total)` pe anteturi distincte și
un nod per antet (`h_{IDRH}`: `DataH`, descrierea antetului, `Total`; iconița după semnul
valorii, ca la recepții). Click pe dosar sau pe un antet umple grila cu liniile lor, ca orice
nod. Tooltip-ul dosarului spune câte sunt și că se așază din editorul de legături (iconița din
antetul arborelui), care le vede deja: `citeste_instantanee` din `asociere.py` citea DE LA
ÎNCEPUT toate anteturile angajamentului, cu `idrr = 0` pentru cele fără recepție.

## 2. Refacerea din istoric (cererea 2)

### 2.1 Ce e sursa de adevăr

`FX_Istoric` nu pierde nimic: fiecare antet și fiecare linie poartă `IDH = FX_Istoric.ID`
(verificat în `TABLES/FX_Receptii_H.md` și `TABLES/FX_Receptii.md`: coloana `IDH` și relația
`FX_IstoricFX_Receptii_H` / `FX_IstoricFX_Receptii`). Iar algoritmul care construiește H și
liniile din istoric există deja, de două ori: în Access (`mdl_FX_Istoric.FX_Istoric_Populeaza_
Receptii`, liniile 529–652 din export) și portat în `prelucrare_pasi.step4a_populeaza_receptii`
(felia 0048-03). Amândouă merg peste rândurile de istoric de recepție în ordinea `ID`, strâng
liniile (`Val_Receptie <> 0`) într-un tampon și le varsă sub primul rând cu `(activ:true)` în
`Observatii`, care e antetul (`IDH` = ID-ul lui, `DataH = DataFX`, `Total = Val_Receptie`,
`Descriere` = textul dintre «Receptie: » și virgulă).

### 2.2 Ruta nouă: `POST /api/forexe/receptii/refacere`

`routes/forexe/receptii_refacere.py`, `refa_receptii(cursor, cod, aplica)`. Aceeași plimbare
ca 4a, cu trei deosebiri, toate deliberate:

* **Peste rândurile `Prelucrat = 1`**, nu `= 0`. Cele neprelucrate aparțin următoarei
  descărcări; 4a le va construi atunci și NU verifică `IDH`-ul, deci dacă le-am construi și
  aici ar ieși două H pentru același rând de istoric.
* **Scrie numai ce lipsește.** Antet: dacă există deja un H cu acel `IDH`, i se refolosește
  `IDRH`-ul; altfel se inserează cu `_H_INSERT_SQL` din 4a (`IDRR NULL`, `NrCrt` continuă de la
  maxim, `EsteStergere` din descriere). Linie: lipsește după `IDH` ▸ `_REC_INSERT_SQL`; există
  dar `IDRH IS NULL` (orfană) ▸ `UPDATE FX_Receptii SET IDRH = %s` — relegată, nu dublată;
  există și e legată ▸ nu se atinge.
* **Nu ridică pentru un indicator lipsă.** 4a ridică `ValueError` (o ingestie nu are voie să
  continue cu o linie fără indicator). La refacere, o asemenea linie ar opri tot angajamentul;
  se sare, se numără în `linii_sarite` și se spune în `avertismente`.

Trei lucruri se **semnalează** fără să se înghită: liniile rămase fără antet la sfârșit
(tamponul nevidat), liniile sărite, și anteturile existente care au linii cu `IDH NULL` (la
ele nu se poate judeca ce lipsește, deci liniile lor nu se ating și se spune de ce).

**DIF-urile.** O linie adăugată sau relegată sub un antet care e DEJA pe o recepție îi schimbă
lanțul: `step4d_calculeaza_dif(cursor, cod, idrr)` se cheamă pentru fiecare `IDRR` atins
(`receptii_recalculate` în răspuns). Un antet nou n-are `IDRR`, deci n-are lanț — îl va avea
când operatorul îl așază, iar așezarea (`recalculeaza_final`) își face socoteala ei.

**Două moduri, un singur cod.** `aplica = false` (implicit) e o PROBĂ: numără, nu scrie
(`rollback`). `aplica = true` scrie într-o singură tranzacție. Cheile răspunsului sunt ASCII,
identice în ambele moduri: `antete_lipsa, linii_lipsa, linii_orfane, antete_scrise,
linii_scrise, linii_relegate, linii_fara_antet, linii_sarite, receptii_recalculate,
avertismente, aplicat, cod`.

### 2.2b Toată baza dintr-un foc

A doua cerere a operatorului din aceeași zi: «trebuie să le pot face pe toate deodată,
pentru o bază». Același endpoint, corp `{ "toate": true, "aplica": … }` (exclusiv cu `cod`):
`refa_toate` ia toate `CodAngajament`-urile cu rânduri de istoric de recepție prelucrate și
cheamă `refa_receptii` pentru fiecare, **fiecare în tranzacția lui** — un angajament care
cade (FK, indicator) intră în `erori` cu mesajul lui și nu trage după el nici ce s-a scris
înainte, nici ce vine după. Răspunsul: `angajamente`, `cu_lipsuri`, `totaluri` (suma pe cele
opt chei numărate) și `detalii` DOAR pentru angajamentele la care s-a găsit ceva de făcut sau
de spus. Fișier de cereri pentru VS Code (REST Client): `PYTHON/requests/receptii_refacere.http`
— login, probă pe un cod, aplicare pe un cod, probă pe toată baza, aplicare pe toată baza.
Parola se cere la prompt, nu stă în fișier.

### 2.2c Fără sesiune, cu cheia de administrare

«Nu pot cu utilizatorul admin, să dau doar numele bazei?» — ba da: `POST
/api/admin/receptii/refacere`, în `routes/admin.py`, sub `require_api_key` (`X-Api-Key` =
`API_KEY` din `config.py`), cu `db_name` în corp (validat de `_validate_db_name`), plus
`cod` sau `toate`, și `aplica`. Același nucleu (`citeste_cererea` + `executa_refacerea`, scoase
în `receptii_refacere.py` ca ambele rute să le împartă), același răspuns, plus `db_name`
ecou. **Nu trece prin `get_db_connection`** ca restul rutelor de administrare: tabelele `FX_`
sunt pe serverul K-BOT (`get_kbot_connection`). Importul e local în handler — `angajamente.py`
importă deja `_validate_db_name` din `admin.py`, un import la vârf închidea un cerc.
Regula «bearer ≠ X-Api-Key, nu se amestecă» rămâne: sunt două rute, fiecare cu o singură pază.

`PYTHON/requests/refacere.ps1` (curl.exe): `-Db 000_DEMO [-Apply] [-Cod …]` pe calea admin
(cheia la prompt mascat / `-ApiKey` / `KBOT_API_KEY`), sau `-User … -Dc …` pe calea cu login.
Scrie răspunsul brut lângă script și îl rezumă în consolă.

### 2.3 Clientul

* `IApiClient.RebuildReceptiiAsync(cod, apply, ct)` ▸ `ReceptiiRebuildResult` (POCO nou în
  `KBot.Domain`, cu `NothingToDo`). DTO-urile `PostReceptiiRefacereRequest/Response` în
  `UpsertAngajamenteRequest.vb`, lângă celelalte.
* `ReceptiiView`: al treilea callback opțional, `rebuildMissing As Action(Of String)`, legat de
  **iconița din STÂNGA subsolului** arborelui (`FooterLeftIcon = database`, slot liber până
  acum — butonul de strângere nu e pornit în vederea asta, deci nu-i ia colțul). Fără callback
  iconița se stinge, ca celelalte două.
* `MainForm.RebuildReceptiiFromIstoric(cod)`: proba ▸ dacă nu lipsește nimic, o spune și se
  oprește ▸ altfel întreabă cu cifrele reale (câte anteturi, câte linii, câte orfane) și cu
  avertizarea că anteturile refăcute vor apărea în dosarul «Instantanee neașezate» ▸ la Da,
  scrierea ▸ raportul (scrise / relegate / recepții recalculate / semnalări) ▸
  `LoadTreeAsync(pastreazaSelectia:=True)`, ca după editorul de legături: steagurile `Are*`
  ale nodului se pot schimba, iar reîncărcarea împinge singură contextul nou în vedere.
  Trăiește în shell din motivul obișnuit: plasa de re-autentificare pe o formă de răspuns
  proprie, iar `WithReauth` e privat acolo.

### 2.4 Drumul operatorului

Recepții ▸ angajamentul ▸ iconița din stânga subsolului ▸ Da ▸ anteturile apar în «Instantanee
neașezate» ▸ iconița din antetul arborelui (editorul de legături) ▸ se așază pe recepțiile lor
▸ la salvare, DIF-urile se recalculează și arborele se reîncarcă.

## Fișiere atinse

**Nou**
- `PYTHON/routes/forexe/receptii_refacere.py`
- `PYTHON/tests/test_forexe_receptii_refacere.py` (15 teste, offline, `FakeCursor`)
- `PYTHON/requests/receptii_refacere.http` (cererile pentru VS Code REST Client)
- `PYTHON/requests/refacere.ps1` (curl.exe: login sau cheie admin ▸ probă / aplicare)
- `PYTHON/tests/test_admin_receptii_refacere.py` (6 teste, offline, Flask test client + `executa_refacerea` înlocuit)
- `src/KBot.Domain/ReceptiiRebuildResult.vb`
- `docs/worklog/SLICE-0062-receptii-neasezate-si-refacere-din-istoric.md` (acest fișier)

**Modificat**
- `PYTHON/routes/forexe/receptii.py` — rădăcina H + LEFT JOIN R; nota din antet
- `PYTHON/routes/forexe/__init__.py` — înregistrarea rutei
- `PYTHON/routes/admin.py` — `POST /api/admin/receptii/refacere` (X-Api-Key + `db_name`)
- `src/KBot.Api/IApiClient.vb`, `ApiClient.vb` — `RebuildReceptiiAsync`; `Idrr = If(r.idrr, 0)`
- `src/KBot.Api/UpsertAngajamenteRequest.vb` — `idrr As Integer?`; cele două DTO-uri
- `src/KBot.Api/KBot.Api.vbproj` — FileVersion 1.0.5.0 ▸ 1.0.6.0
- `src/KBot.Domain/ReceptiiInfo.vb` — documentarea convenției `Idrr = 0`
- `src/KBot.Domain/KBot.Domain.vbproj` — FileVersion 1.2.0.0 ▸ 1.2.1.0
- `src/KBot.App/Views/ReceptiiView.vb` / `.Designer.vb` — dosarul neașezatelor, iconița și
  handler-ul din stânga subsolului, `UNPLACED_KEY`
- `src/KBot.App/KbotForm.vb` — `RebuildReceptiiFromIstoric`, `WarningsParagraph`, cablarea
- `src/KBot.App/KBot.App.vbproj` — FileVersion 1.0.29.0 ▸ 1.0.30.0
- `tests/KBot.App.Tests/ReceptiiViewTests.vb` — 5 teste noi (dosar, click, doar-neașezate,
  iconița pornită/stinsă, click ▸ shell); stub-ul `RebuildReceptiiAsync` în toate cele 9
  implementări false ale lui `IApiClient` din `tests/KBot.App.Tests`

Restul arborelui de lucru (`KbotForm.Designer.vb`, `KBot.Migrator/*`, `KBot.Theming/*`,
`FormFit*`, `Surse/SURSA_XFA_WRITTER/` etc.) era deja modificat/neurmărit la începutul
sesiunii — **nu e al acestei felii**.

## Rezultate

- `PYTHON`: `pytest tests` — **535 trecute / 26 sărite (toate host-only / env), 0 eșuate** (21 noi). **Niciun test n-a atins serverul sau vreo bază**: cursoare false și `executa_refacerea` înlocuit.
- `dotnet build KBot.sln --no-incremental` — **0 erori**; avertismentele `MSB3825` (`.resx` /
  `BinaryFormatter`) sunt PREEXISTENTE.
- `dotnet test tests/KBot.Common.Tests` — 94 trecute (vezi și felia 0063).
- `dotnet test tests/KBot.App.Tests` — vezi §«Ce NU s-a făcut» dacă rezultatul lipsește de aici.

## Ce NU s-a făcut, și trebuie spus

- **Nimic nu a rulat pe o bază vie.** Interogarea H-rădăcină, ruta de refacere și
  recalcularea DIF n-au atins MariaDB; `test_forexe_receptii.py` (host-only) nu s-a putut rula
  aici. Că `Prelucrat = 1` acoperă chiar rândurile migrate din Access e **dedus** din
  `FX_Istoric_Actualizeaza_Rezolvat` (setează `Prelucrat = True` după prelucrare), nu
  observat pe datele operatorului.
- **Nimic nu s-a văzut pe ecran.** Dosarul «Instantanee neașezate», iconița `database` din
  stânga subsolului și cele trei casete ale shell-ului sunt verificate doar prin teste
  headless / prin compilare.
- **Nu s-a atins `IstoricView`** (varianta a doua din cerere): decizie de plasare, nu lipsă.
- **Liniile legate de un H GREȘIT** (există după `IDH`, `IDRH` nenul, dar alt antet decât cel
  dedus din istoric) nu se ating și nu se semnalează. N-a fost în cerere; ar fi o judecată
  peste o legătură pe care cineva a făcut-o.
- **Anteturile duplicate pe același `IDH`** (dacă o migrare repetată le-a produs) nu se
  detectează: `h_dupa_idh` reține ultimul citit.
- **Nimic nu s-a comis** — arborele de lucru conține WIP străin de felia asta; comiterea
  selectivă e a operatorului.
