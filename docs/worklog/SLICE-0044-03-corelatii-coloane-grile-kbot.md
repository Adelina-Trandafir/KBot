# SLICE-0044-03 — corelații de coloane Access ▸ MariaDB, pe o filă nouă, cu grilele K-BOT

## Ce s-a schimbat și de ce

Două cereri ale operatorului, amândouă pe ecranul de migrare al feliei 0044.

### 1. Coloanele nu se corelează întotdeauna după nume

Potrivirea Access ▸ MariaDB era, până acum, **numele**, fără litere mari/mici.
Pe perechea clasificațiilor asta scrie fiecare id în coloana celuilalt:

| | `IdClsf` | `IdClsfPY` | `IdClsfAcc` |
|---|---|---|---|
| **Access** | arată spre un tabel din **alt** `.accdb` | id-ul rândului din `Clasificatii` de pe MariaDB | — |
| **MariaDB** | id-ul de pe **MariaDB** | — | id-ul din **Access** |

Deci corelația corectă e încrucișată: Access `IdClsf` ▸ MariaDB `IdClsfAcc`, și
Access `IdClsfPY` ▸ MariaDB `IdClsf`. Regulile stau în
`tables.COLUMN_RENAMES`, iar `tables.default_rename_map()` le aplică **doar
acolo unde ținta chiar are coloana pe care o numesc** — un tabel al cărui
echivalent MariaDB n-a căpătat niciodată `IdClsfAcc` rămâne cu potrivirea
simplă după nume, exact cum era. (În exportul Access, `IdClsfPY` apare pe
`FX_DDF_REV_SA`, `FX_DDF_REV_SB`, `FX_DDF_REV_PRT` și `FX_ORD_TBL`; `IdClsf`
singur apare pe încă opt tabele migrate.)

**Operatorul poate schimba corelația oricărei coloane**, nu doar a acestor
două — asta a și cerut. Drumul complet:

* `POST /api/migrare/tabele` întoarce acum, pe fiecare tabel, `coloane_tinta`
  (numele de pe MariaDB) și, pe fiecare coloană, `tinta` — corelația propusă;
* migratorul le arată în fila nouă și le lasă schimbate rând cu rând;
* ce a aranjat operatorul se trimite la analiză în `corelatii` —
  `{tabel: {coloana_access: coloana_tinta}}`;
* `validate.column_rename_map()` compune cele trei straturi, în ordinea în care
  se bat: potrivirea după nume ‹ implicitele din `COLUMN_RENAMES` ‹ alegerea
  operatorului. `with_target_names()` și `chosen_columns_of()` citesc de acolo,
  iar `execute.run()` construiește lista de coloane a `INSERT`-ului tot de
  acolo;
* corelațiile călătoresc **pe raportul analizei** (`Report.mappings`), ca și
  bifele de coloane: scrierea folosește exact ce a MĂSURAT analiza, nu o
  alegere nouă venită între timp.

Trei lucruri refuzate explicit, în loc de tăcute:

* o țintă pe care baza n-o are e **ignorată** (analiza n-are voie să măsoare
  rândul față de o coloană care nu există), iar migratorul o refuză din
  `CellValidating` cu numele bazei în mesaj — combo-ul se poate și tasta;
* **două coloane din Access corelate cu aceeași coloană de pe MariaDB opresc
  totul**: 400 la `POST /api/migrare/analiza` (`_corelatii_cerute`),
  `ValidationError` în `column_rename_map`, `ExecuteError` la scriere. Una
  dintre valori s-ar pierde, și nu se poate spune care;
* o țintă vidă înseamnă «coloana asta nu călătorește» — același înțeles ca
  bifa stinsă din lista de coloane, nu un al doilea mecanism.

**Rutarea nu e atinsă de nimic din toate astea.** Ea citește rândul cu numele
lui din Access (`IdUnitate`, `DC`, `CodAngajament`), înainte de orice
corelație — `with_target_names` face o COPIE, rândul original rămâne al
rutării.

### 2. Fila nouă, și toate grilele trecute pe `KBotDataView`

`dgvConstatari` nu mai stă andocat direct pe formular: locul din dreapta e
acum un `TabControl` cu două file — **«Constatări»** (grila de dinainte) și
**«Corelații coloane»** (grila nouă). Amândouă descriu tabelul ales în lista
din stânga, ca și lista de coloane.

Grila de corelații are patru coloane: coloana din Access, **«Se scrie în
(MariaDB)»** (combo cu coloanele țintei plus `(nu se scrie)`), **«Propus de
server»** (reperul față de care se vede ce a schimbat operatorul) și
**«Stare»** (`cheie primară` / `coloană debifată` / `fără pereche` /
`schimbată de tine` / `corelare încrucișată`).

Cele patru grile ale formularului — `dgvTabele`, `dgvColoane`,
`dgvConstatari`, `dgvCorelatii` — sunt acum `KBot.Controls.KBotDataView`, nu
`DataGridView`, **cu toate coloanele și proprietățile scrise în
`.Designer.vb`** (regula casei). Ce s-a schimbat odată cu ele:

* `KBotDataView` **nu-și mută rândurile singură** — modelul e al gazdei. Lista
  de tabele își ține deci ordinea într-un `List(Of RandTabel)` al
  formularului, iar săgețile ▲ ▼ și tragerea cu mouse-ul mută MODELUL, după
  care grila se reumple din el. Ordinea din listă e mai departe ORDINEA DE
  SCRIERE;
* pentru tragere, `KBotDataView` a căpătat **un singur membru public nou**,
  `RowIndexAt(pt)` — versiunea publică a hit-testului pe rânduri, pe care
  `RowAtPoint` o ținea `Private`. Nimic altceva din control nu s-a schimbat
  (`KBot.Controls.Tests`: 917 verde);
* «celulă inertă» nu e o proprietate de coloană în `KBotDataView`, ci vine din
  `CellFormatting`. Cele două cazuri de dinainte — un tabel care nu e în
  fișier nu se poate bifa, o cheie primară nu se poate debifa — sunt acum doi
  handleri de `CellFormatting` care coboară `e.Enabled`;
* `dgvConstatari` era legată de un `DataTable`; `KBotDataView` e NELEGATĂ, deci
  rândurile se scriu direct, în `BeginUpdate`/`EndUpdate`. `Imports
  System.Data` a plecat din formular;
* bifele nu mai au nevoie de `CurrentCellDirtyStateChanged` +
  `CommitEdit` — `KBotDataView` comută la click, pe loc, iar
  `CellValueChanged` scrie înapoi în model.

### 3. RULE 0 — diacriticele afară din cod

Corelațiile au plecat cu `coloane_țintă`, `țintă` și `corelații` drept nume de
câmpuri JSON, plus `corelații` / `_corelații_cerute` drept identificatori
Python. Regula 0 a proiectului spune altceva, iar acum e scrisă în `CLAUDE.md`:
**fără diacritice nicăieri în cod; singura excepție e textul pe care operatorul
îl citește pe ecran.**

De aici a ieșit o măturare a întregului modul, nu doar a coloanelor noi —
contractul de sârmă purta diacritice de dinainte, iar el se schimbă pe amândouă
capetele deodată (nimic n-a rulat vreodată live, deci n-a fost nimic de rupt):

| Înainte | Acum |
|---|---|
| `există` · `rânduri` · `unități` · `toate_unitățile` · `în_baza` | `exista` · `randuri` · `unitati` · `toate_unitatile` · `in_baza` |
| `analiză` · `forțat` · `înlocuiește` · `înlocuit` | `analiza` · `fortat` · `inlocuieste` · `inlocuit` |
| `bucată_maximă` · `bucăți` · `octeți` · `fișier` · `fișiere` | `bucata_maxima` · `bucati` · `octeti` · `fisier` · `fisiere` |
| `constatări` · `număr` · `sărite` · `ale_unității` · `poate_forța` | `constatari` · `numar` · `sarite` · `ale_unitatii` · `poate_forta` |
| `coloane_țintă` · `țintă` · `corelații` · `tabele_așteptate` · `acceptă_nul` | `coloane_tinta` · `tinta` · `corelatii` · `tabele_asteptate` · `accepta_nul` |

Identificatorii Python `lipsă`, `afară`, `sărite` și cei doi noi au trecut la
ASCII, iar comentariile și docstring-urile din `PYTHON/routes/migrare` și din
`src/KBot.Migrator` au fost pliate la ASCII.

Felurile de constatare și clasele au trecut și ele la ASCII, deși se scriu
literal în coloanele «Fel» și «Clasă» ale migratorului: sunt jetoane de
protocol, iar operatorul n-are nevoie de diacritice ca să le citească.

| Înainte | Acum |
|---|---|
| `TABEL_LIPSĂ` · `COLOANĂ_LIPSĂ` · `CHEIE_STRĂINĂ` · `CHEIE_DUBLĂ` · `SELECȚIE` | `TABEL_LIPSA` · `COLOANA_LIPSA` · `CHEIE_STRAINA` · `CHEIE_DUBLA` · `SELECTIE` |
| `blocant` · `forțabil` | `BLOCANT` · `FORTABIL` |

Toate trec prin constantele din `validate.py` (`F_*`, `BLOCANT`, `FORTABIL`) și
prin `CLASS_OF`, deci n-a fost niciun literal răzleț de urmărit; singura
comparație pe partea .NET, `Constatare.EsteBlocanta`, arată acum spre `BLOCANT`.

**Ce NU s-a atins:** doar șirurile pe care le vede operatorul ca PROPOZIȚII —
mesaje, etichete, sfaturi, `.resx`, liniile de jurnal, câmpul `error` al
API-ului, și valorile scrise în celule de migrator (`lipsește`, `LIPSEȘTE`,
`cheie primară`, `corelare încrucișată`…), care nu pleacă nicăieri pe sârmă.
Restul depozitului (`KBot.Controls` și celelalte proiecte) încă poartă
comentarii cu diacritice de dinainte de regulă — o măturare separată.

## Ce NU s-a făcut

* **Nimic nu s-a văzut pe ecran și nimic n-a rulat pe date reale** — ca toată
  felia 0044. Formularul compilează și nu s-a deschis nici în designerul VS.
* Fără teste noi (cerute explicit). Cele existente rămân verzi:
  `KBot.Controls.Tests` 917, suita Python 150 trecute / 15 sărite.
* Corelațiile nu se țin minte între rulări: sunt ale inventarului curent și
  se pierd odată cu el (unitate, an sau bază schimbată ⇒ `ResetInventar`).
  Dacă operatorul le vrea persistate, e o felie separată.

## Fișiere atinse

| Fișier | Ce |
|---|---|
| `PYTHON/routes/migrare/tables.py` | `COLUMN_RENAMES`, `default_rename_map()`, `default_correlations()` |
| `PYTHON/routes/migrare/validate.py` | `column_rename_map()`, `with_target_names()` și `chosen_columns_of()` pe harta nouă; `Report.mappings`; `analyze(mappings=…)` |
| `PYTHON/routes/migrare/execute.py` | lista de coloane a `INSERT`-ului din harta raportului; refuzul țintei duble |
| `PYTHON/routes/migrare/migrare.py` | `coloane_țintă` + `țintă` în inventar; `_corelații_cerute()` |
| `PYTHON/routes/migrare/README.md` | secțiunea «Corelațiile de coloane (Access ▸ MariaDB)» |
| `src/KBot.Controls/DataView/KBotDataView.Input.vb` | `RowIndexAt(pt)` (public, aditiv) |
| `src/KBot.Migrator/Api/MigrareApiClient.vb` | `ColoanaFisier.Tinta` / `.TintaImplicita`, `TabelFisier.ColoaneTinta`, `WriteCorelatii` |
| `src/KBot.Migrator/MigratorForm.Designer.vb` | patru `KBotDataView` cu coloanele lor, `TabControl` cu două file |
| `src/KBot.Migrator/MigratorForm.vb` | modelul listei de tabele, fila de corelații, handlerii noi |
| `src/KBot.Migrator/MigratorForm.resx` | eticheta grilei de corelații |
