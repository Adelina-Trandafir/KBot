# SLICE-0045 — Istoricul migratorului direct Access–MariaDB

Consolidare D001-15: etapele 01…07 sunt reunite cu corpurile, fișierele, deciziile, probele și limitele păstrate. Istoricul de implementare, inclusiv crearea/sincronizarea schemei descrise atunci, nu autorizează operații de schemă în sesiunea curentă; lucrarea de acum este exclusiv Markdown.

Repere: 01 are descoperirea și apoi revizia pe dumpul DDL; 02–04 construiesc cod alături de HTTP; 05 înlocuiește lanțul și rulează partea Access; 06 urmează formularul redesenat de operator și înlocuiește bifele pe indice cu Tag; 07 corectează unitatea rândului și probează poarta pe două unități. «Nerulat» și «o singură unitate disponibilă» sunt checkpointurile etapelor inițiale, nu contrazic probele ulterioare limitate la Access/poartă.

## SLICE-0045-01 — pasul de descoperire (`PLAN_MigratorDirect.md` §12) <a id="slice-0045-01"></a>

Data: 23.08.2026 · Slice **0045** (primul liber in `KBOT_STATUS.md`; 0044 era ultimul).

### Ce s-a facut si de ce

Planul `PLAN_MigratorDirect.md` cere, INAINTE de orice cod de transfer (§12, "FIRST, before
any implementation"), un pas de descoperire: se citesc cele trei fisiere `.accdb` din
`Avacont/`, se documenteaza schemele, se raspunde la sase intrebari si se scrie
`docs/MAPARE_NOMENCLATOARE.md`. Nu s-a scris niciun rand din transferul propriu-zis, niciun
formular, nicio corelatie de coloane — asa cum cere §12 ("Read, record, report, stop").

Pasul acesta e si dovada tehnica pe care §1 si §3 se sprijina: ca ACE deschide fisierele, ca
un build x64 pe .NET 8 merge, si ca parola e cablata corect.

#### 1. Spike-ul

`tools/AccdbSchemaDump/` — proiect consola C#, `net8.0`, `PlatformTarget=x64`,
`System.Data.OleDb` 8.0.0. **NU e adaugat in `KBot.sln`** (cerut explicit de §12.1). Deschide
fiecare `.accdb` cu `Microsoft.ACE.OLEDB.16.0`, cu cadere pe `12.0`, prin
`OleDbConnectionStringBuilder` (nu concatenare) si `Jet OLEDB:Database Password` acolo unde e
nevoie. Scrie per fisier un `.md` cu: lista de tabele + numarul de randuri, obiectele care nu
sunt tabele, `GetSchema("Columns")`, `GetSchema("Indexes")`, `GetSchema("ForeignKeys")` si
primele 20 de randuri din fiecare tabel, cu textul taiat la 20 de caractere — aceeasi forma ca
`FX_System_Export/TABLES/*.md`.

Rezultatele sunt in `artifacts/accdb-schema/`: `cale.md`, `baza2026.md`, `FX_2026.md`.

#### 2. Livrabilul

`docs/MAPARE_NOMENCLATOARE.md` — maparea celor patru tabele de referinta, raspunsurile la
cele sase intrebari din §12.2, corectiile datorate lui `MAPARE_ACCESS_MARIADB.md`, ce nu s-a
putut stabili, si intrebarile care blocheaza pasul de implementare.

### Constatarile care schimba planul

1. **`cale.accdb` nu contine niciunul dintre cele patru nomenclatoare.** Ipoteza din §12.2 Q1
   e gresita. Contine doar registrul: `cai` (13 randuri), `LEGUNIT`, `K`, `TC`, `Ver` si patru
   tabele temporare goale.
2. **Nomenclatoarele stau intr-un fisier PE UNITATE** — `Clasificatii` (54), `Parteneri` (75),
   `Rectificari` (0) sunt in `Avacont/Energetic ISJ/baza2026.accdb`, iar `cai` arata spre el
   (`IdUnitate=76` ▸ `CaleUnitate=.\ENERGETIC ISJ`). Pentru DC `000_DEMO` inseamna 13 fisiere,
   deschise pe rand — nu un fisier filtrat pe unitate.
3. **Randurile de nomenclator NU poarta `IdUnitate`.** Exact opusul a ce presupunea §12.2 Q5.
   Harta `(IdClsfAcc, IdUnitate) ▸ IDClsf` ramane corecta, dar `IdUnitate` vine din bucla
   (randul `cai` al fisierului deschis), nu din rand. Partea `FX_*` e neatinsa — acolo coloana
   chiar exista.
4. **Legatura e dovedita pe randuri reale**, nu dedusa: `FX_DDF_REV_SA.IdUnitate=76` ▸ randul
   `cai` 76 ▸ `.\ENERGETIC ISJ\baza2026.accdb` ▸ `Clasificatii.IDClsf`. Fiecare `IdClsf` din
   esantionul `FX_DDF_REV_SA` se regaseste in `Clasificatii` al acelei unitati.
5. **`IdClsfPY` e demonstrabil invechit.** Aceeasi clasificatie (`IdClsf=123`) poarta
   `IdClsfPY=1363` in `Clasificatii` si `IdClsfPY=1309` in `FX_DDF_REV_SA` si `FX_ORD_TBL`.
   Regula 1 din `MAPARE_ACCESS_MARIADB.md` are acum dovada in spate.

### Raspunsuri la §12.2

- **Q1** — fisierul e `baza2026.accdb`, cate unul pe unitate; `cale.accdb` are doar registrul.
- **Q2** — `Clasificatii_Buget`: `trim1..trim4` devin **patru coloane ale unui rand**, nu
  patru randuri. Nu e o citire dintre doua, e stabilit: `clasificatii.py:181` scrie
  `(IdClsf, IdUnitate, An, Trim1, Trim2, Trim3, Trim4)`, ceea ce se potriveste cu cheia unica
  `(IdClsf, An)`.
- **Q3** — `Rectificari` ▸ `Clasificatii_Rectificari` mapat coloana cu coloana. Fara tinta:
  `ID`, `DTQ`, `Esinc` (toate contabilitate locala). **Tabelul e gol in fisierul disponibil**,
  deci maparea e doar de schema.
- **Q4** — unitatea e purtata de FISIER, nu de o coloana. Vezi constatarea 3.
- **Q5** — `Clasificatii` **NU** poarta `IdUnitate`. Vezi constatarea 3.
- **Q6** — numarate toate; nimic mare. Tot fisierul FX are ~3.100 de randuri in scop.

### Corectii datorate lui `MAPARE_ACCESS_MARIADB.md`

Nu am editat acel fisier (§12.3 cere sa nu fie atins speculativ). Cinci puncte, in §7 al
livrabilului:

1. **`FX_DDF_REV.ESpeciala` — verificat: ABSENT** din `.accdb`-ul viu. Inchide §6.2 al acelui
   fisier.
2. **`FX_ORD` nu mai e reconstruit** — cele 16 coloane citite din sursa. `ArePDF`, `CalePDF`,
   `DTQ` lipseau din tabelul §4. Inchide §6.1.
3. ⚠ **Regula 2 e pe jumatate gresita**: `FX_DDF.Cual` e `Integer`, dar **`FX_ORD.Cual` e
   `WChar(255)`** — text. E nevoie de conversie pe latura ORD.
4. ⚠ **`FX_ORD.IDORDP` nu e 0 in Access** — poarta 117, 118, 119… (id-urile serverului vechi,
   exact tiparul `IdClsfPY`). `FX_ORD_TBL.IDORDP` chiar e 0, cum scrie. Optiunea (A) le
   suprascrie cu `IDORD` (1..17). Semnalat, nu decis.
5. `FX_ORD_TBL` scrie **`CodAI`** — a treia grafie a aceleiasi coloane.

In plus: **ACE nu expune deloc colectia `ForeignKeys`** pe niciunul dintre cele trei fisiere.
Relatiile Access se vad doar ca nume de index. Decizia planului de a deriva ordinea de scriere
din `information_schema.KEY_COLUMN_USAGE` al TINTEI e deci si singura posibila.

### Fisierele atinse

| Fisier | Ce |
|---|---|
| `tools/AccdbSchemaDump/AccdbSchemaDump.csproj` | nou — consola x64, net8.0, System.Data.OleDb |
| `tools/AccdbSchemaDump/Program.cs` | nou — spike-ul |
| `artifacts/accdb-schema/cale.md` | nou — dump |
| `artifacts/accdb-schema/baza2026.md` | nou — dump |
| `artifacts/accdb-schema/FX_2026.md` | nou — dump |
| `docs/MAPARE_NOMENCLATOARE.md` | nou — livrabilul §12.3 |
| `docs/worklog/SLICE-0045-01-descoperire-nomenclatoare.md` | nou — acest fisier |
| `docs/worklog/KBOT_STATUS.md` | randul 0045 + Current focus |

`KBot.sln` NEATINS. `src/KBot.Migrator` NEATINS.

### Rezultate

`dotnet build -c Release` pe spike: **succes, 0 erori**, 13 avertismente `CA1416`
(platform-compatibility pe `OleDb`, care e Windows-only — proiectul tinteste `net8.0`, nu
`net8.0-windows`). Nu le-am stins: e un spike aruncabil, iar `net8.0` e ce cere §12.1.

Rulat pe toate trei fisierele: **toate trei s-au deschis**, cu `Microsoft.ACE.OLEDB.16.0`,
**fara parola**. 11 + 69 + 27 tabele citite, cu numaratori si esantioane.

Nu s-au scris teste (§9 al planului: "No automated tests in this pass unless explicitly
asked for").

### Neverificat / amanat

1. **Schema reala MariaDB nu a fost citita.** Niciun server contactat in acest pas. Toate
   listele de coloane "MariaDB" din livrabil vin din `INSERT`-urile rutelor Flask, care spun
   ce SCRIE ruta, nu ce ARE tabelul. Necunoscute pentru toate cele patru tabele tinta: lista
   completa de coloane, nulabilitatea, implicitele, latimile, si daca exista vreo coloana
   `NOT NULL` fara implicit in afara listei. **`000_DEMO.sql` nu e in depozit** — am cautat.
2. **`Rectificari` nu a fost vazut niciodata cu randuri** (0 in singurul fisier disponibil).
3. **O singura unitate** are `baza2026.accdb` disponibil (`Energetic ISJ`, IdUnitate 76,
   `SURSA=01A`). `cai` listeaza 13. Unitatile `02E`/`VENITURI` vor umple `ClasificatiiV` /
   `RectificariV`, goale aici.
4. **Un singur DC** (`000_DEMO`) in registru, deci "`cai.DC` e numele bazei verbatim" e
   confirmat, dar nu si testat pe un al doilea DC.
5. **`Clasificatii_Buget.An` nu are sursa** in randul Access. Candidati: `cai.AnDate`,
   `cai.ANNOU`, numele fisierului. Blocheaza §3 al livrabilului.
6. **Parola:** planul §1/§3 si slice 0044 spun ca fisierele sunt criptate cu `andreI`. Cele
   din `Avacont/` **nu sunt** — s-au deschis fara parola. Campurile de parola raman in plan
   (copiile operatorului pot fi protejate), dar pasul "decriptat de mana in Access" al feliei
   0044 nu se aplica fisierelor din depozit.

### Ce urmeaza — intrebari care blocheaza implementarea

Douasprezece, in §9 al livrabilului. Cele patru noi, nascute din dump:

- **Q3** de unde vine `Clasificatii_Buget.An`?
- **Q4** o unitate cu nomenclatoare dar fara fisier FX (`cai` 110 si 114 au
  `CaleForexe=NULL`) — se transfera sau se sare?
- **Q5** `ParteneriAng` ▸ `Parteneri_Coduri` (ambele capete exista si se potrivesc) — in scop?
  Si `ParteneriSI` (52 randuri, nicio tinta gasita) — confirmat afara?
- **Q6** `FX_Salarii` si `FX_Receptii_Plati` sunt in ordinea de scriere §6 dar **nu exista** in
  `FX_2026.accdb`.
- **Q7** `ClasificatiiV` / `RectificariV` — in scop pentru unitatile `02E`?

Plus cele din §11 al planului ramase deschise: biblioteca MariaDB (recomandare:
`MySqlConnector`), baza existenta cu randuri, `AVACONT_SURSA` cu vederi/rutine, crearea
utilizatorului si a drepturilor.

**Pasul se opreste aici, cum cere §12.3.**

---

## Revizia 1 — 23.08.2026, cu schema MariaDB reala

Operatorul a pus `MariaDB_Schema/000_DEMO.sql` (84.779 B, 22.08.2026) si a raspuns la
intrebarile care blocau. Golul principal semnalat mai sus — «schema reala MariaDB nu a fost
citita» — e **inchis**: ambele laturi vin acum din sursa. `docs/MAPARE_NOMENCLATOARE.md` a
fost rescris in intregime pe DDL real; nu mai contine nicio lista de coloane dedusa dintr-un
`INSERT` de ruta Flask.

### Deciziile operatorului (acum in §0 al livrabilului)

| # | Ce | Decizia |
|---|---|---|
| D1 | `Clasificatii_Buget.An` | **2026**, scris fix. Transferul e **o singura data**, tot pentru 2026 |
| D2 | Ce unitati se transfera | **operatorul alege `IdUnitate`** — selectie pe formular, nu «toate din DC» |
| D3 | `ParteneriSI` | afara |
| D4 | `FX_Salarii`, `FX_Receptii_Plati` | **afara din scopul MariaDB** — nu exista nici in `.accdb`, nici in schema. `FX_ORD_TBL.IDRP` e coloana orfana prin constructie |
| D5 | `ClasificatiiV` / `RectificariV` | nu in aceasta felie |
| D6 | `Avacont/` negitignorat | rezolvat de operator; `/MariaDB_Schema` la fel |

D2 rezolva si intrebarea despre unitatile fara fisier FX (`cai` 110 si 114, `CaleForexe=NULL`):
daca operatorul le alege, isi trec nomenclatoarele si nu au date `FX_*`.

Confirmat din schema, nu intrebat: **`AVACONT_SURSA` are doar tabele** — dump-ul nu contine
nicio vedere, niciun declansator, nicio rutina (era §11 Q5).

### Constatari noi, din schema

**F6 — `000_DEMO` NU e goala, si asta poate fi o re-sincronizare, nu o prima migrare.**
Cea mai grea constatare din tot pasul. Semnele `AUTO_INCREMENT`: `Clasificatii` 1497,
`Parteneri` 7680, `FX_ORD` 134, `FX_ORD_TBL` 891, `FX_ORD_DOC` 719, `FX_ORD_PART` 402,
`FX_DDF` 65, `FX_DDF_REV` 109. Puse langa valorile din Access: `Clasificatii.IdClsfPY`
1354–1373, `Parteneri.IdPartener` 7605–7621, `FX_ORD.IDORDP` 117–123,
`FX_ORD_TBL.IDORDTBLP` 430–444 — **toate INAUNTRUL intervalelor tintei**. Nu sunt id-uri
moarte de pe un server scos din uz; sunt id-uri VII pe acesta. Unitatea a mai fost
sincronizata in `000_DEMO`, iar fisierul Access carauseste inapoi rezultatul acelei
sincronizari in coloanele-oglinda. Doua urmari: optiunea (A) scrie `FX_ORD.IDORD` 1..17 in
cheia `IDORDP` si aterizeaza pe **randuri existente** 1..17, pe care `ON DUPLICATE KEY UPDATE`
le suprascrie (la fel `FX_DDF.IDDF`=33 si `FX_DDF_REV.IDREV`=44..47); si refuzul din §4 pasul 4
al planului se declanseaza imediat, fiindca fiecare tabel din scop are randuri.

**Cinci chei straine spre ALTA baza de date.** `Clasificatii` are sase constrangeri, iar cinci
arata spre `AVACONT_COMUN`: `ClsfE`▸`DefaClsfE`, `ClsfF`▸`DefaClsfF`, `Titlu`▸`DefaTitlu`,
`SS`▸`DefaSursaSector` (toate patru pe coloane **generate**) si `Articol`▸`DefaArticol` (pe o
coloana scrisa). Deci un rand de clasificatie e refuzat cu `1452` daca cele cinci valori nu
exista in dictionarele din `AVACONT_COMUN` — iar patru dintre ele nu sunt scrise de migrator
si nu pot fi privite inainte de INSERT, fiindca ies din `concat`/`left`/`replace` peste ce
scrie. Planul nu are aceasta poarta. Si `CREATE DATABASE` din `AVACONT_SURSA` **nu ajunge**:
`AVACONT_COMUN` e alta baza si nu e creata de bucla aceea.

**Noua coloane generate pe `Clasificatii`, una pe `Clasificatii_Buget`.** `Clsf`, `Titlu`,
`ClsfSal`, `ClsfF`, `ClsfE`, `ClsfX`, `Sector`, `Sursa`, `SS` sunt
`GENERATED ALWAYS … PERSISTENT`, la fel `Clasificatii_Buget.TOTAL`. Toate au omonim in Access.
Le nimerisem ✗ in revizia 0 fiindca ruta nu le scria; motivul adevarat e ca **nu POT fi
scrise** — un INSERT peste ele e eroare, nu ignorare.

**`Clasificatii` nu are cheie unica pe `(IdClsfAcc, IdUnitate)`.** Singurul index unic e
`PRIMARY KEY (IDClsf)`; `IdClsfAcc` si `IdUnitate` au fiecare index NEunic si nu exista
compus. Deci `ON DUPLICATE KEY UPDATE` — regula §7 a planului, «pe fiecare tabel» — **nu are
pe ce sa se potriveasca aici**, iar a doua rulare insereaza inca 54 de clasificatii cu
`IDClsf` noi, si harta de rezolvare arata tacut spre ultimele copii. E singurul tabel din set
in situatia asta: `Clasificatii_Buget` `(IdClsf, An)`, `Clasificatii_Rectificari`
`(IdClsf, Data, Document)`, `Parteneri` `(IdUnitate, CodPartener)` si `Parteneri_Coduri`
`(IdPartener, IdClsf)` isi au toate cheia.

**Ingustari de latime pe `Clasificatii`.** Access are `WChar(50)` pe Capitol / Subcapitol /
Articol / Alineat; tinta are `varchar(5)`, `varchar(5)`, `varchar(5)`, `varchar(2)`. Datele de
azi incap fix (`65.01`, `05.00`, `10.01`, `01`), dar nimic nu impune asta pe latura Access, iar
in mod strict o valoare prea lunga e `1406`. Paza §5.3 a planului prinde doar coloanele
OBLIGATORII LIPSA, nu valorile PREA LARGI. La fel `Denumire`: nulabil in Access, **`NOT NULL`**
pe tinta.

**`Unitati` e o preconditie, nu o tinta.** `IdUnitate` e cheie primara **fara**
`AUTO_INCREMENT`, iar `Clasificatii`, `Clasificatii_Buget`, `Parteneri` si `FX_ORD_TBL` au
toate cheie straina spre ea. Randul unitatii alese trebuie sa EXISTE inainte de primul INSERT
de nomenclator, altfel `1452`. Nimic din plan nu scrie `Unitati`.

**`Parteneri.Ascuns` are tinta.** In revizia 0 il pusesem ✗ fiindca ruta Flask nu-l scrie;
schema arata `Ascuns tinyint(4) NULL`. Ar trebui sa calatoreasca.

### Corectii datorate lui `MAPARE_ACCESS_MARIADB.md` — acum SAPTE

Fisierul tot nu a fost editat (§12.3). Un punct s-a schimbat fata de revizia 0 si doua sunt
noi:

- **#3 corectat fata de revizia 0.** Spusesem «e nevoie de conversie pe latura ORD», presupunand
  ca tinta e `int`. Nu e: `FX_ORD.CUAL` e `varchar(255)` pe MariaDB si `WChar(255)` in Access.
  Deci Regula 2 e gresita **pe amandoua laturile** pentru `FX_ORD`, iar text ▸ text **nu cere
  nicio conversie azi**. Schimbarea e insa tot datorata (§5 al acelui fisier), si abia atunci
  apare conversia. Conteaza: decide ce cod se scrie.
- **#6 NOU si blocant.** §5 al acelui fisier spune apasat «**`IdClsfAcc` nu mai blocheaza
  nimic** … pus sa accepte NULL pe 22.08». **Fals in schema din 22.08**:
  `FX_DDF_REV_SA.IdClsfAcc` si `FX_DDF_REV_SB.IdClsfAcc` sunt amandoua `int(11) NOT NULL`,
  fara implicit, fara `auto_increment` — deci chiar paza §5.3 a planului opreste rularea si le
  numeste. `FX_ORD_TBL.IdClsfAcc` chiar e nulabil.
- **#7 NOU.** `FX_ORD_PDF` **exista** in schema (gol); §2 al acelui fisier il numeste «planned
  table, does not exist yet».
- Neschimbate: #1 `ESpeciala` absent din Access (pe MariaDB e `tinyint(1) NULL DEFAULT 0`, deci
  pur si simplu nu calatoreste), #2 `FX_ORD` citit din sursa, #4 `FX_ORD.IDORDP` nu e 0 — si
  acum se stie si de ce conteaza (F6), #5 `CodAI`.

### Fisiere atinse in revizia 1

| Fisier | Ce |
|---|---|
| `docs/MAPARE_NOMENCLATOARE.md` | rescris integral pe DDL real; §0 decizii, F6, §3.1/§3.2/§3.3, §7 `Unitati`, §9 sapte corectii, §10 sase intrebari |
| `docs/worklog/SLICE-0045-01-descoperire-nomenclatoare.md` | aceasta sectiune |
| `docs/worklog/KBOT_STATUS.md` | randul 0045-01 + Current focus, aduse la zi |

Cod: **niciunul**. `KBot.sln`, `src/KBot.Migrator` si spike-ul, neatinse. Pasul e tot
descoperire.

### Ce a ramas deschis

Cinci, in §10 al livrabilului, plus una nedecisa:

1. **Q1 — `000_DEMO` are deja datele acestei unitati (F6).** Ce face o rulare? Se goleste
   tinta si id-urile Access sunt autoritatea; sau tinta e o baza cu adevarat noua si `000_DEMO`
   a fost doar sablonul de schema; sau optiunea (A) e gresita pentru o tinta populata si
   id-urile trebuie remapate. Decide si §11 Q4 al planului.
2. **Q2 — `Clasificatii` fara cheie unica pe `(IdClsfAcc, IdUnitate)`.** Se adauga indexul
   unic, sau se accepta ca tabelul e doar-inserare si rularea e exact o data?
3. **Q3 — `IdClsfAcc` `NOT NULL` pe SA si SB.** Se scrie `IdClsf`-ul Access in ele (ceea ce
   inseamna chiar `IdClsfAcc` pe `Clasificatii`, deci ar fi consecvent), sau se fac nulabile
   cum spune §5?
4. **Q4 — `ParteneriAng` ▸ `Parteneri_Coduri` in scop?** Ambele capete exista, se potrivesc,
   tinta are cheia unica de care upsert-ul are nevoie. 2 randuri in fisierul acestei unitati.
5. **Q5 — `Unitati`**: popularea ei e in scopul uneltei sau e o preconditie pregatita de
   operator? Aceeasi intrebare cu §11 Q6 (utilizator si drepturi pentru un DC nou).
6. **Q6 — biblioteca MariaDB**, inca nedecisa. Recomandarea ramane `MySqlConnector`.

Neverificat, in §11 al livrabilului: schema e un instantaneu fara date (semnele
`AUTO_INCREMENT` sunt maxime, nu numaratori — confirma pe serverul viu inainte de a actiona pe
Q1); **`AVACONT_COMUN` nu a fost vazuta deloc**, desi cinci chei straine arata spre ea si
continutul ei decide daca un rand de clasificatie e primit.

## SLICE-0045-02 — motorul: citirea Access + descrierea tintei MariaDB <a id="slice-0045-02"></a>

Data: 23.08.2026 · Slice **0045**, pasul 02 (implementare, prima transa).

### Ce s-a facut si de ce

Prima transa de cod a migratorului direct. Am ales sa o adaug **alaturi** de codul HTTP al
feliei 0044, nu in locul lui: asa fiecare pas ramane verde si compilabil, iar stergerea
fisierelor vechi (`ConnectForm`, `Api/MigrareApiClient`, `MigratorForm` cel vechi) se face
intr-un pas al ei, cand formularul nou e gata sa ii ia locul. Nimic din codul vechi nu a fost
atins.

Transa asta acopera **temelia**: cititul din Access si intrebatul tintei despre ea insasi.
Nu contine inca nici maparea coloanelor, nici portile de verificare, nici scrierea, nici
formularul.

#### Proiectul

`KBot.Migrator.vbproj`:

- **`<PlatformTarget>x64</PlatformTarget>`**, nu `AnyCPU`. Furnizorul ACE e pe 64 de biti pe
  acest parc, iar `AnyCPU` cu prefer-32-bit pica la `OleDbConnection.Open` cu «provider is not
  registered». Felia 0045-01 a verificat ca ACE 16.0 si 12.0 sunt amandoua inregistrate pe 64
  de biti pe masina asta.
- `System.Data.OleDb` 8.0.0 — pe .NET 8 OleDb nu mai e in BCL, e pachet NuGet.
- **`MySqlConnector` 2.6.2** (decizia D10): MIT in loc de GPL-cu-exceptie, si e drop-in peste
  interfetele ADO.NET. Nimic din solutie nu vorbea pana acum cu MariaDB, deci alegerea a fost
  libera.

Referintele de proiect raman cum erau in pasul asta (`KBot.Api` inca intra, fiindca formularul
vechi il foloseste); pleaca odata cu formularul vechi.

#### `Access/` — patru fisiere

- **`AccessProvider`** — deschide `.accdb` cu `Microsoft.ACE.OLEDB.16.0`, cu cadere pe `12.0`.
  Sirul de conexiune se construieste cu `OleDbConnectionStringBuilder`, **niciodata prin
  concatenare**, ca o parola cu `;` sau `"` sa fie scapata de builder in loc sa termine sirul.
  Parola merge in `Jet OLEDB:Database Password` — nume ramas de la Jet, desi motorul nu mai e
  Jet. Cand niciun furnizor nu deschide fisierul, mesajul e in romana cu diacritice, spune
  motivul PE FURNIZOR, listeaza furnizorii OLE DB inregistrati pe calculator si trimite la
  «Microsoft Access Database Engine» pe 64 de biti — **niciodata o eroare COM bruta**, cum cere
  §10 punctul 1 al planului. Parola nu apare in niciun mesaj.
- **`AccessSchema`** — `TableNames`, `HasTable`, `ResolveTableName` (numele real, cu grafia
  fisierului), `Columns`, `CountRows`, `OpenReader`. Toate comparatiile de nume sunt
  `OrdinalIgnoreCase`.
- **`AccessTableReader`** — invelis peste `OleDbDataReader` care **isi tine si comanda**, deci
  un singur `Using` le inchide pe amandoua. Harta de ordinale se construieste o data pe cititor,
  nu pe rand. `ValueOrMissing` deosebeste **lipsa** coloanei (intoarce `Nothing`) de un **NULL**
  real (intoarce `DBNull.Value`) — confundarea celor doua e chiar greseala consemnata in felia
  0044 impotriva CSV-ului lui `mdb-export`.
- **`CaiRegistry`** + `CaiUnit` — citeste `cale.accdb` ▸ `cai`. Rezolva caile **relative**
  (`.\ENERGETIC ISJ\baza2026.accdb`) fata de dosarul lui `cale.accdb`, accepta si cai absolute
  (`CaleForexe` chiar e absoluta), si trece peste separatorul final inconsecvent — un rand are
  `.\SC29 LOCAL\`, urmatorul `.\ENERGETIC ISJ`. `HasForexeFile` / `HasUnitFile` exista tocmai
  fiindca 2 din 13 randuri au `CaleForexe = NULL`.

Streaming peste tot: **niciun `DataTable.Load` pe un tabel intreg**, cum cere §3 al planului,
ca unealta sa nu depinda de numaratorile de azi.

#### `MariaDb/` — patru fisiere

Regulile de validare vin din **TINTA**, nu din tipurile Access. MariaDB e cea care primeste sau
refuza randul, deci MariaDB e cea intrebata; o constrangere adaugata pe server schimba raspunsul
fara nicio schimbare de cod.

- **`TargetSchemaTypes`** — `TargetColumn`, `TargetForeignKey`, `TargetUniqueKey`. Regulile stau
  **pe tip**, nu la fiecare loc de apel, fiindca aceeasi regula verificata in doua feluri e chiar
  felul in care s-au nascut defectele feliei 0044-04. `TargetColumn` calculeaza:
  `IsAutoIncrement`, **`IsGenerated`**, `IsServerFilled`, **`IsRequired`** (`NOT NULL`, fara
  implicit, necompletata de server — paza care lipsea la `1364`) si `IsWritable`.
  `TargetForeignKey` poarta **`ParentSchema`**, cu `IsCrossSchema` — parintele intr-o ALTA baza
  de date nu e ipoteza, sunt cele cinci constrangeri ale lui `Clasificatii` spre `AVACONT_COMUN`.
  `TargetUniqueKey.IsCoveredBy` raspunde daca `ON DUPLICATE KEY UPDATE` are pe ce sa se
  potriveasca.
- **`TargetSchema`** — citeste `information_schema.COLUMNS`, `KEY_COLUMN_USAGE` si `STATISTICS`
  pentru o baza. `STATISTICS` (nu `TABLE_CONSTRAINTS`) fiindca poarta ordinea coloanelor, de care
  o cheie compusa are nevoie, iar `NON_UNIQUE = 0` prinde `PRIMARY` si `UNIQUE` deodata.
  `RequiredColumns`, `WritableColumns`, `CrossSchemaForeignKeys`, `CanUpsert`. Tot ce e harta e
  `OrdinalIgnoreCase`.
- **`WriteOrder`** — sortare topologica (Kahn) peste cheile straine **vii** ale tintei. Ciclul
  opreste, cu tabelele nesituate numite. Trei lucruri sarite deliberat, fiecare cu motivul in
  cod: **auto-referintele** (un rand care arata spre propriul tabel se ordoneaza in tabel, nu
  intre tabele — tratat ca muchie ar raporta un ciclu fals), **cheile spre alta baza** (nicio
  ordine locala nu le satisface, deci sunt poarta separata) si **tabelele din afara setului
  ales** (nu e o intrebare de ordine, ci de existenta, la care raspunde poarta de orfani). O
  cheie compusa da un rand pe coloana, dar muchia e aceeasi — deduplicata.
  `Violations` verifica ARANJAREA operatorului si numeste ambele tabele plus constrangerea.
  Ordinea din §6 al planului e acolo ca sa **verifice** sortarea, nu ca sa o inlocuiasca.
- **`TargetServer`** + `TargetConnection` — conectare, `TestConnection` (versiunea serverului),
  `DatabaseNames`, `DatabaseExists`, `CharacterSetOf`, si **`CreateDatabaseFrom`**: citeste
  charset-ul si colatia sablonului din `information_schema.SCHEMATA`, creeaza baza cu ele, apoi
  copiaza fiecare tabel prin `SHOW CREATE TABLE` intre `SET FOREIGN_KEY_CHECKS = 0` si `= 1`
  (cu `Finally`, ca sa se aprinda la loc si pe eroare) — deci ordinea de creare nu conteaza,
  spre deosebire de DATE, care se scriu cu verificarile APRINSE tocmai ca o ordine gresita sa
  pice zgomotos. Fara DDL scris de mana si fara `schema_sync`: tinta e goala, nu exista ce
  compara. `CREATE DATABASE` e DDL, nu se poate derula inapoi, deci ruleaza in afara oricarei
  tranzactii. Numele de identificator se citeaza cu backtick (`Quote`), niciodata parametrizate
  — un nume de baza sau de tabel nu poate fi parametru in niciun dialect — si fiecare nume care
  ajunge acolo vine din `information_schema` sau din registrul `cai`.
  `RetargetSchema` rescrie doar referintele calificate cu numele SABLONULUI, ca sa **nu** atinga
  o referinta cross-baza reala precum `AVACONT_COMUN`.
  Comentariul metodei spune apasat ce costa D7: `SHOW CREATE TABLE` copiaza structura, **nu
  randuri**, deci `Unitati` porneste GOALA, iar patru chei straine arata spre ea.

`TargetConnection.Describe()` intoarce `utilizator@gazda:port` — forma care poate intra intr-un
jurnal sau in antetul unui dosar SQL. Parola nu iese niciodata din obiect.

### Rezultate

`dotnet build src/KBot.Migrator/KBot.Migrator.vbproj`: **succes, 0 erori, 0 avertismente.**
`dotnet build KBot.sln`: **succes, 0 erori** (5 avertismente, toate din alte proiecte, dinainte).

Nu s-au scris teste (§9 al planului: «No automated tests in this pass unless explicitly asked
for»).

### Fisiere atinse

| Fisier | Ce |
|---|---|
| `src/KBot.Migrator/KBot.Migrator.vbproj` | `PlatformTarget=x64`; pachetele `System.Data.OleDb` si `MySqlConnector` |
| `src/KBot.Migrator/Access/AccessProvider.vb` | nou |
| `src/KBot.Migrator/Access/AccessSchema.vb` | nou |
| `src/KBot.Migrator/Access/AccessTableReader.vb` | nou |
| `src/KBot.Migrator/Access/CaiRegistry.vb` | nou |
| `src/KBot.Migrator/MariaDb/TargetSchemaTypes.vb` | nou |
| `src/KBot.Migrator/MariaDb/TargetSchema.vb` | nou |
| `src/KBot.Migrator/MariaDb/WriteOrder.vb` | nou |
| `src/KBot.Migrator/MariaDb/TargetServer.vb` | nou |

Codul HTTP al feliei 0044 — NEATINS si inca legat. Se sterge in pasul formularului.

### Neverificat / amanat

1. **Nimic din tot pasul asta nu a atins un server MariaDB viu.** `TargetSchema`, `WriteOrder`
   si `TargetServer` compileaza si sunt scrise pe `000_DEMO.sql`, dar niciunul nu a rulat.
2. **`AccessProvider` si `CaiRegistry` nu au fost rulate din aceasta aplicatie** — dovada ca ACE
   deschide fisierele vine din spike-ul feliei 0045-01, care e alt proces si alt proiect.
3. **Formularul nu exista inca**, deci nimic nu se vede pe ecran.
4. Portile de verificare (§5 al planului), maparea coloanelor, scrierea si dosarul SQL — pasii
   urmatori.

### Ce urmeaza

- **0045-03**: maparea coloanelor (din amandoua fisierele MAPARE), harta de rezolutie a
  clasificatiilor, portile de verificare — inclusiv cele doua noi pe care le-a scos la iveala
  schema: `Unitati` ca preconditie si `AVACONT_COMUN` pentru cele cinci chei cross-baza.
- **0045-04**: scrierea tranzactionala + dosarul de instructiuni SQL.
- **0045-05**: formularul, si stergerea codului HTTP al feliei 0044.

Raman deschise, din `docs/MAPARE_NOMENCLATOARE.md` §10: `ParteneriAng` ▸ `Parteneri_Coduri` in
scop sau nu, si cine populeaza `Unitati` (unealta sau operatorul) — aceeasi intrebare cu
utilizatorul si drepturile pentru un DC nou.

## SLICE-0045-03 — catalogul de corelații + hărțile de rezoluție + porțile de verificare <a id="slice-0045-03"></a>

Data: 23.08.2026 · Slice **0045**, pasul 03.

### Ce s-a facut si de ce

A doua transa de cod. Tot alaturi de lantul HTTP al feliei 0044, care ramane neatins.
Aici intra tot ce trebuie DECIS inainte sa se scrie primul rand: ce coloana merge unde, ce
id se rezolva in ce, si ce anume opreste rularea.

Inca nu se scrie nimic — scrierea si dosarul SQL sunt pasul 04, formularul pasul 05.

#### Deciziile noi ale operatorului, luate in cod

- **`ParteneriAng` ▸ `Parteneri_Coduri` — IN SCOP.** Ambele capete exista si se potrivesc;
  singura redenumire e `ContBanca` ▸ **`ContBancar`**. E singura tinta gasita care tine
  `IdClsf` SI `IdClsfAcc` alaturi, deci `IdClsf`-ul Access hraneste amandoua coloanele:
  rezolvat in `IdClsf`, brut in `IdClsfAcc`.
- **`Unitati` o populeaza UNEALTA.** D7 face asta oricum inevitabil: o baza creata dupa
  sablon are structura, nu randuri, deci `Unitati` porneste goala si patru chei straine
  arata spre ea.

#### Modelul de corelare

`ColumnMapping` + `TableMap` + `ColumnPlan`.

Coloanele calatoresc **dupa nume**, neinsensibil la caz, daca harta nu spune altceva. Asta
e implicit fiindca `MAPARE_ACCESS_MARIADB.md` descrie coloana cu coloana **doar** familiile
DDF si ORD; restul setului `FX_*` e potrivire dupa nume, exact ca pe latura Python. Trei
straturi il bat, in ordinea in care se bat — **nume ‹ redenumire ‹ derivat**:

1. **potrivire dupa nume** — coloana Access al carei nume egaleaza o coloana-tinta
   **scriibila**;
2. **redenumire** — pereche explicita, si ea **ELIBEREAZA potrivirea dupa nume pe care o
   inlocuieste**; fara asta, Access `IDORD` ar ateriza si in `IDORDP` si in `IDORD`-ul
   vechi de tip varchar al tintei;
3. **derivat** — valoarea nu vine din rand deloc (unitatea, anul, un id rezolvat, un NULL
   fortat). Bate tot.

**O coloana-tinta `GENERATED` nu e niciodata candidat**, fiindca nu e scriibila — scrierea
ei e EROARE, nu ignorare. Regula asta singura acopera noua dintre potrivirile pe care le-ar
fi facut `Clasificatii` (`Clsf`, `Titlu`, `ClsfSal`, `ClsfF`, `ClsfE`, `ClsfX`, `Sector`,
`Sursa`, `SS`) plus `Clasificatii_Buget.TOTAL`, fara nicio exceptie scrisa de mana.

**O tinta revendicata de doua ori la aceeasi prioritate = OPRIRE**, nu alegere tacuta.

`ColumnPlan.Build` e chemata **identic** de verificator si (in pasul 04) de scriitor. Doua
copii ale regulii care se departeaza una de alta e chiar felul in care s-au nascut
defectele feliei 0044-04.

#### Catalogul (`TableMaps`)

Fiecare intrare se urmareste pana la `MAPARE_ACCESS_MARIADB.md` §3/§4 sau la
`docs/MAPARE_NOMENCLATOARE.md`. Nimic dedus dintr-un nume de coloana.

**Nomenclatoare, in ordine:** `Unitati` (din registru), `Clasificatii`
(`IDClsf` ▸ `IdClsfAcc`, unitatea din bucla, **doar-inserare** per D8),
`Clasificatii_Buget` (acelasi tabel-sursa, `An = 2026` fix, `IDClsf` EXCLUS fiindca ar
potrivi neinsensibil la caz peste `IdClsf`-ul tintei si ar scrie id-ul BRUT unde trebuie
cel REZOLVAT), `Clasificatii_Rectificari`, `Parteneri` (cele trei coloane cu forma de id si
doar `CodPartener` calatoreste; **`Ascuns` calatoreste** — ruta Flask nu-l scrie, dar tinta
il are), `Parteneri_Coduri`.

**FX_\*:** `FX_DDF` (exclude `IdUnitate`, `IdPartener`, `CodPartener`, `SS`, `DTQ`),
`FX_DDF_REV`, `FX_DDF_REV_SA`/`_SB` (`IdClsf` rezolvat BLOCANT, **`IdClsfAcc` primeste
`IdClsf`-ul Access per D9**, `IdPartener` NULL per §5.2), `FX_ORD` (optiunea (A):
`IDORD` ▸ `IDORDP`, iar `IDORDP`-ul Access — 117+ — NU pleaca), `FX_ORD_PART`,
`FX_ORD_TBL` (`IDRP` exclus, orfan prin constructie per D4), `FX_ORD_DOC`. Restul, potrivire
dupa nume cu `IdClsfPY` si `DTQ` scoase peste tot.

Tabelele scoase din scop sunt intr-o singura lista: cele cinci din §2 plus `FX_Salarii`,
`FX_Receptii_Plati` (D4) si `ClasificatiiV`/`RectificariV`/`ParteneriSI` (D5/D3).

#### Hartile de rezolutie

- **`ClasificatiiMap`** — `(IdClsf Access, IdUnitate)` ▸ `IDClsf` atribuit. Unitatea NU vine
  din randul de nomenclator (Access `Clasificatii` n-are coloana), ci din randul `cai` al
  fisierului deschis. Latura `FX_*` chiar poarta `IdUnitate`, deci acolo e citire de coloana.
- **`ParteneriMap`** — `(CodPartener, IdUnitate)` ▸ `IdPartener` atribuit. A aparut fiindca
  `Parteneri_Coduri` a intrat in scop. Cheia pune CODUL primul si unitatea ultima, ca
  despartirea sa se ia de la ultimul separator — un cod de partener e data de operator si ar
  putea contine el insusi `|`.
- **`WrittenKeys`** — cheile primare chiar scrise, pe tabel. **Inlocuieste hartile A–E
  facute de mana in felia 0044**: tabelele se scriu in ordine topologica, deci setul de chei
  al unui parinte e mereu complet inainte sa fie cititi copiii, si «a calatorit parintele
  randului asta?» se raspunde fara nicio trecere separata si fara a doua citire a fisierului
  Access.

#### Portile (`Verifier`)

Zece, cea mai ieftina prima, ca o rulare imposibila sa pice pe o interogare, nu pe o
tranzactie:

1. fisierele Access ale unitatilor alese exista (lipsa fisierului FOREXE e **atentionare**,
   nu blocaj — `cai` are `CaleForexe = NULL` pe doua randuri din treisprezece);
2. baza-tinta exista sau poate fi creata;
3. **`AVACONT_COMUN` exista si dictionarele ei acopera ce va calcula `Clasificatii`** —
   poarta pe care planul nu o avea;
4. `Unitati` acopera unitatile alese, ori le scrie unealta;
5. **coloanele obligatorii** sunt in lista de scriere (paza pentru `1364`), verificata pe
   REZULTAT;
6. **latimile** (paza pentru `1406`) plus `Denumire` `NOT NULL` pe tinta si nulabil in Access;
7. rezolutia clasificatiilor, rulata **uscat** peste randurile Access;
8. rezolutia partenerilor pentru `Parteneri_Coduri`;
9. un tabel doar-inserare care are deja randuri pentru o unitate aleasa (**blocant**, D8);
10. ordinea de scriere e derivabila, fara ciclu.

Poarta 3 merita explicata: patru din cele cinci valori verificate sunt coloane **generate**,
pe care migratorul nu le scrie si nu le poate vedea inainte de INSERT — ies din
`concat`/`left`/`replace` peste ce scrie. `ClasificatieDerived` le recalculeaza, ca
«Verifică» sa poata spune CARE clasificatie va fi refuzata si de ce, in loc sa lase un
`1452` care numeste o coloana pe care nimeni n-a scris-o.

`ClasificatieDerived` e o **REPLICARE a DDL-ului, nu o citire a lui** — scrie asta in
propriul comentariu. Daca o coloana generata e redefinita pe server, verificarea se
invecheste; dar pica in siguranta, fiindca cheia straina de pe server e tot cea care refuza
randul de fapt. E avertisment timpuriu, nu plasa de siguranta.

Fara clasa FORTABIL, spre deosebire de 0044: transferul e o singura data (D1) intr-o baza
noua (D7), deci «ruleaza oricum si sari randurile vinovate» ar lasa o unitate pe jumatate
populata fara nicio cale de a sti care randuri lipsesc.

### Rezultate

`dotnet build src/KBot.Migrator/KBot.Migrator.vbproj`: **succes, 0 erori, 0 avertismente.**

Fara teste (§9 al planului).

### Fisiere atinse

Toate noi, in `src/KBot.Migrator/Transfer/`: `ColumnMapping.vb`, `TableMap.vb`,
`TableMaps.vb`, `ColumnPlan.vb`, `ResolutionMaps.vb`, `ClasificatieDerived.vb`,
`Finding.vb`, `TransferRequest.vb`, `Verifier.vb`.

### Neverificat / amanat

1. **Nimic nu a atins un server MariaDB viu si niciun `.accdb` din aceasta aplicatie.**
   Tot pasul e scris pe `000_DEMO.sql` si pe dump-urile feliei 0045-01.
2. **`AVACONT_COMUN` tot nu a fost vazuta.** Poarta 3 e scrisa, dar numele coloanelor din
   dictionare (`DefaArticol.Articol`, `DefaTitlu.Titlu`, `DefaClsfF.ClsfF`,
   `DefaClsfE.ClsfE`, `DefaSursaSector.SursaSector`) vin din tintele cheilor straine din
   `000_DEMO.sql`, nu dintr-o schema citita a acelei baze. Daca vreun nume difera, poarta
   raporteaza «dictionarul nu a putut fi citit» — zgomotos, nu tacut.
3. **`ClasificatieDerived` e replicare**, vezi mai sus.
4. Cele treisprezece tabele care calatoresc pe potrivire dupa nume nu au corelatie CITITA
   dintr-un document; verificatorul le numeste intr-o constatare de tip
   `POTRIVIRE_DUPA_NUME`, ca operatorul sa vada exact care sunt.

### Ce urmeaza

- **0045-04**: scrierea tranzactionala (o singura tranzactie, `FOREIGN_KEY_CHECKS` APRINSE,
  loturi de 500, comanda pregatita pe tabel) + dosarul de instructiuni SQL.
- **0045-05**: formularul si stergerea codului HTTP al feliei 0044.

## SLICE-0045-04 — scrierea tranzacțională + dosarul de instrucțiuni SQL <a id="slice-0045-04"></a>

Data: 23.08.2026 · Slice **0045**, pasul 04.

### Ce s-a facut si de ce

A treia transa: partea care chiar scrie. Tot alaturi de lantul HTTP al feliei 0044.
Formularul e pasul 05.

#### `ValueConverter` — o singura traducere

Chemata identic de scriitor si de jurnal. Felia 0044-04 pasul 06 a consemnat alternativa:
un verificator care CONVERTEA ca sa judece, langa un scriitor care trimitea originalul, si
MariaDB raspunzand `1292`. Un convertor folosit de un singur capat lasa mereu celalalt
neaparat.

Furnizorul OLE DB de pe .NET intoarce tipuri CLR reale — `DateTime`, `Double`, `Boolean`,
`String`, `Byte()` — nu text. Asta e un castig real al cititului direct din Access si de
aceea clasa e scurta: nu mai exista parsarea de locale de care avea nevoie drumul prin
mdbtools. Ce ramane: **NULL-ul Access devine `DBNull.Value`**, niciodata sir gol si
niciodata zero (confuzia consemnata impotriva CSV-ului lui `mdb-export`); `Boolean` Access
(-1/0) devine 1/0 pentru `tinyint(1)`; iar text gol intr-o coloana **netextuala** devine
NULL, fiindca acolo Access scrie `""` cand vrea sa spuna «nimic».

`IsOrphanValue` — **un zero e orfan oriunde cheia parintelui e `AUTO_INCREMENT`**, fiindca
auto-increment nu atribuie niciodata 0. Access scrie 0 pentru «fara parinte» peste tot in
familia FX (`FX_ORD_TBL.IDORDP` e 0 pe fiecare rand), deci tratarea lui ca valoare ar
arata fiecare rand spre un rand care nu poate exista.

`ToLiteral` — redarea pentru jurnal: `NULL`, date ISO in ghilimele, `Byte()` ▸ `0x…`,
siruri scapate.

#### `SqlDumpWriter` — dosarul

`<jurnal>\<DC>\<AAAALLZZ_HHMMSS>\` cu `_00_info.txt`, un `.sql` pe tabel si
`_99_final.txt`.

- Instructiunea se scrie **INAINTE** sa se execute, iar `flush` se face pe lot, deci un
  proces omorat lasa pe disc tot pana la ultimul lot, inclusiv randul care a picat.
- Doua avertismente stau **in fisierele insesi**, nu doar aici: e o **RECONSTRUCTIE**
  (driverul trimite parametri, nu text) si **scrisul pe disc NU e in tranzactie**, deci
  fisierele `.sql` de deasupra unui `ROLLBACK` descriu munca ce nu mai exista.
- `_99_final.txt` scrie `COMMIT` sau `ROLLBACK`, totalurile, iar la esec eroarea completa
  **si ultima instructiune dinainte de oprire**.
- **Consemnarea nu poate rupe migrarea:** orice esec de scriere pe disc se logheaza cu urma
  completa, se spune O DATA in jurnalul lucrarii, si consemnarea se dezactiveaza singura.
  Singura exceptie e constructorul — dosar neconfigurat OPRESTE rularea, cu motivul spus:
  o migrare nescrisa nicaieri e mai rea decat una care nu porneste.
- **Nicio parola** nu ajunge in niciun fisier.

#### `TransferRunner` — rularea

- **O singura tranzactie** pentru tot. Orice esec deruleaza totul inapoi — o unitate
  migrata pe jumatate e mai rea decat niciuna. Oprirea de catre operator la fel.
- **`FOREIGN_KEY_CHECKS` ramane APRINS** tot timpul. Ordinea corecta e chiar scopul;
  stingerea verificarii ar ascunde exact defectele pentru care exista unealta asta. E
  opusul drumului de creare a bazei, unde se stinge tocmai fiindca ordinea de creare nu
  poarta niciun inteles.
- **`Unitati` se scrie prima**, din registru (decizia operatorului + D7). `IdUnitate` e
  cheie primara **fara** `AUTO_INCREMENT`, deci nu poate aparea altfel; `Detalii`,
  `SursaSector` si `CodProgram` vin din randul `cai` si din tabelul `UNIT` al unitatii,
  iar `An` e 2026.
- **Rand cu rand, cu o comanda pregatita refolosita pe tabel**, nu loturi multi-rand. Trei
  lucruri au nevoie de tratament pe rand si s-ar pierde intr-un lot: id-ul atribuit de
  server (`LAST_INSERT_ID` intoarce doar PRIMUL id al unui lot, iar id-urile consecutive nu
  sunt garantate sub orice `innodb_autoinc_lock_mode`), decizia de orfan, si linia de
  jurnal. Jurnalul se goleste si progresul se raporteaza la fiecare 500 de randuri.
- **`INSERT … ON DUPLICATE KEY UPDATE`** peste tot unde coloanele scrise acopera o cheie
  unica; unde nu (azi doar `Clasificatii`, D8) se emite un `INSERT` simplu, fiindca a
  pretinde altceva ar dubla tacut randurile la a doua rulare — iar verificatorul refuza a
  doua rulare tocmai ca sa nu se ajunga acolo. Cheile raman in afara listei de UPDATE.
- **Filtrarea pe unitate:** tabelele care poarta `IdUnitate` raspund direct (un fisier
  FOREXE poate purta randuri pentru mai multe unitati); restul ajung la unitatea lor prin
  parinti. Un rand al altei unitati se sare **TACUT** — e forma normala a unui fisier
  comun, nu o constatare.
- **`WrittenKeys` in loc de hartile A–E ale feliei 0044.** Ordinea topologica garanteaza
  ca setul de chei al unui parinte e complet inainte sa fie cititi copiii, deci «a
  calatorit parintele randului asta?» se raspunde fara nicio trecere separata si fara a
  doua citire a fisierului Access.
- **Orfanii se judeca dupa COLOANA, nu dupa tabel:** coloana care accepta NULL ▸ randul se
  scrie cu coloana golita si numarul intra in raport; coloana `NOT NULL` ▸ randul nu se
  scrie. O clasificatie nerezolvata pe o corelatie marcata blocanta **opreste rularea**,
  cu clasificatia, unitatea si tabelul numite.

### Rezultate

`dotnet build src/KBot.Migrator/KBot.Migrator.vbproj`: **succes, 0 erori, 0 avertismente.**

Compilatorul a prins doua ocurente ale capcanei VB de umbrire neinsensibila la caz — un
local `path` peste `System.IO.Path` si un local `parentLinks` peste metoda `ParentLinks`.
Amandoua redenumite.

Fara teste (§9 al planului).

### Fisiere atinse

| Fisier | Ce |
|---|---|
| `src/KBot.Migrator/Transfer/ValueConverter.vb` | nou |
| `src/KBot.Migrator/Transfer/TransferResult.vb` | nou |
| `src/KBot.Migrator/Transfer/TransferRunner.vb` | nou |
| `src/KBot.Migrator/Dump/SqlDumpWriter.vb` | nou |
| `src/KBot.Migrator/Transfer/TableMap.vb` | `Feeds` / `FeedKeyColumn` + `ResolutionTarget` |
| `src/KBot.Migrator/Transfer/TableMaps.vb` | `Clasificatii` si `Parteneri` isi declara harta |
| `src/KBot.Migrator/MariaDb/TargetServer.vb` | `Describe()`, delegat spre `TargetConnection` |

### Neverificat / amanat

1. **Nimic nu a rulat.** Niciun server MariaDB atins, niciun `.accdb` deschis din aceasta
   aplicatie, niciun dosar de jurnal scris vreodata. Totul e scris pe `000_DEMO.sql` si pe
   dump-urile feliei 0045-01.
2. **Comportamentul `LastInsertedId`** pe `MySqlConnector` cu o comanda pregatita
   refolosita nu a fost probat pe un server viu. Harta de rezolutie a clasificatiilor
   depinde de el.
3. **`RenderStatement` inlocuieste marcajele in ordinea descrescatoare a lungimii**, ca
   `@IdClsf` sa nu manance capul lui `@IdClsfAcc`. Corect prin constructie, dar neprobat pe
   date reale.
4. Filtrarea prin parinti foloseste numele coloanei **TINTA** pentru cautarea in randul
   Access; unde exista o redenumire de cheie (familia ORD), cititorul raspunde «nu am
   coloana asta» si legatura se sare — deci filtrarea prin parinti NU se aplica lanturilor
   ORD. Nu conteaza azi, fiindca `FX_ORD` ajunge la unitate prin `FX_DDF`, dar e o limita
   reala si trebuie stiuta.

### Ce urmeaza

**0045-05**: formularul (`MigratorForm` rescris, cu grupurile «Fișiere», «Server MariaDB»,
«Unitate», «Transfer», toate controalele declarate in `.Designer.vb`), persistenta
setarilor prin `KBot.LocalStore` **fara parole**, si stergerea fisierelor HTTP ale feliei
0044 (`ConnectForm`, `Api/MigrareApiClient`, formularul vechi) impreuna cu referinta la
`KBot.Api`.

## SLICE-0045-05 — formularul, ștergerea lanțului HTTP, și PRIMA rulare pe date reale <a id="slice-0045-05"></a>

Data: 23.08.2026 · Slice **0045**, pasul 05 (ultimul pas de implementare).

### Ce s-a facut si de ce

Ultima transa: formularul, scoaterea lantului HTTP al feliei 0044, si — pentru prima data
in toata felia — **cod de productie rulat pe fisierele reale**.

#### Formularul

`MigratorForm.vb` + `.Designer.vb`, rescrise de la zero. **Toate controalele sunt declarate
in `.Designer.vb`**, niciunul construit in cod; formularul mosteneste `KBotThemedForm`, deci
nu-si aplica singur nicio culoare. Toate grilele sunt `KBotDataView`, nu `DataGridView`.
Etichetele plutitoare sunt `KBotToolTip`, scrise in designer, in romana cu diacritice.

Patru grupuri, cum cere §2 al planului — cu **o abatere deliberata**, pe care o consemnez
fiindca planul spune altceva:

> §2 cerea o casuta «Cale FX» si una «Cale CAI». **Pasul de descoperire a schimbat asta.**
> Caile fiecarei unitati catre `baza<an>.accdb` si `FX_<an>.accdb` sunt CHIAR IN registru
> (`cai.FullPath`, `cai.CaleForexe`), deci operatorul alege doar `cale.accdb`, iar unealta
> citeste restul. A cere doua cai tastate de mana ar insemna sa i se ceara operatorului
> informatie pe care fisierul o are deja — si ar face posibila o nepotrivire intre ce alege
> el si ce spune registrul.

- **«Fișiere»** — registrul, parola fisierelor de unitate, parola fisierelor FOREXE,
  dosarul jurnalului. Ambele parole cu `UseSystemPasswordChar`.
- **«Server MariaDB»** — gazda, port, utilizator administrator, parola, «Testează
  conexiunea» + o eticheta de stare care arata versiunea serverului.
- **«Unitate»** — DC-ul intr-un combo, «Citește registrul», baza-tinta rezolvata, si o grila
  cu o bifa pe unitate. **Decizia D2: operatorul alege unitatile**, niciodata «toate din DC».
  Grila arata pe fiecare rand daca fisierul de nomenclatoare si cel FOREXE chiar exista.
- **«Transfer»** — «Verifică» / «Transferă» / «Oprește», o bara de progres, grila de tabele
  cu bife, grila de constatari si jurnalul rularii.

Reguli respectate, fiecare cu motivul ei:

- **«Transferă» porneste dezactivat** si se activeaza doar dupa o verificare fara constatari
  blocante; se dezactiveaza din nou dupa un COMMIT reusit, fiindca tabelele doar-inserare
  (D8) nu suporta o a doua rulare.
- **Cele doua operatii lungi ruleaza in afara firului de interfata** si se pot opri; oprirea
  deruleaza tranzactia inapoi. Inchiderea formularului in timpul unei operatii intreaba
  intai, apoi opreste.
- **Parolele se citesc din casute exact in clipa in care se construieste cererea** si nu se
  pastreaza nicaieri.
- Fiecare tratator de eveniment e invelit in `Try/Catch` care logheaza si inghite — o
  exceptie dintr-un tratator WinForms nu poate fi rearuncata.

#### Setarile

`MigratorSettings.vb` — un JSON mic langa executabil. **Nicio parola**: tipul nu are camp
de parola deloc, ca sa nu existe unul pe care cineva sa decida mai tarziu sa-l umple.

**Nepotrivire fata de plan, consemnata:** §2 cere persistenta «prin mecanismul existent
`KBot.LocalStore`». **Mecanismul acela nu exista.** `KBot.LocalStore` are `ITempStore` si
`SqliteTempStore` — o stocare temporara cu `Open`/`Reset`/`Dispose` si nicio interfata de
setari. Decat sa inventez una intr-o felie de migrare, am scris un JSON; daca apare o
stocare de setari reala, e o singura clasa de mutat.

#### Ce a fost sters

Lantul HTTP al feliei 0044, care nu mai era referit de nimic dupa ce formularul a fost
rescris:

- `ConnectForm.vb` / `.Designer.vb` / `.resx` — dialogul de pornire care cerea adresa
  serverului si cheia API. Nu mai are ce dovedi: nu mai exista nici adresa, nici cheie, iar
  credentialele MariaDB sunt campuri pe singurul formular, probate de «Testează conexiunea».
- `Api/MigrareApiClient.vb` — 762 de randuri de client HTTP.
- `Registry/AvacontRegistry.vb` — citea unitatile din
  `HKCU\Software\VB and VBA Program Settings\AVACONT`. Inlocuit de citirea directa a lui
  `cai`, care e sursa autoritara.
- Referinta de proiect catre `KBot.Api`.
- Comentariul din `.vbproj` care spunea «NICIO referință la Access … urcă fișierul .accdb pe
  server și vorbește HTTP» — devenise exact pe dos fata de cod, iar un comentariu care
  contrazice codul e mai rau decat niciunul.

`Program.vb` porneste acum direct `MigratorForm`.

### PRIMA rulare pe date reale

Pana acum fiecare worklog al feliei spunea «nimic rulat». Nu mai e adevarat pentru jumatatea
Access.

`tools/MigratorSmoke/` — un ham aruncabil (**nu in `KBot.sln`**) care referă chiar
`src/KBot.Migrator` si conduce **clasele de productie** peste fisierele reale. Spike-ul
feliei 0045-01 dovedise ca ACE deschide fisierele, dar era alt cod, in alt proces; asta
dovedeste ca `AccessProvider`, `AccessSchema`, `AccessTableReader`, `CaiRegistry`,
`ClasificatieDerived` si `TableMaps` fac acelasi lucru.

Rulat pe `Avacont\cale.accdb`, iesire completa, **fara parola**:

```
CaiRegistry.Read            13 unități, un singur DC (000_DEMO), toate 13 pe el
AccessSchema (ENERGETIC)    69 tabele; Clasificatii 54/30col, Parteneri 75/17col,
                            Rectificari 0/14col, ParteneriAng 2/8col, UNIT 1/21col
AccessTableReader           HasColumn("idunitate") = False
                            ValueOrMissing("capitol") → valoarea lui «Capitol»
                            ValueOrMissing(coloană inexistentă) → Nothing
TableMaps                   28 tabele în catalog, 14 pe potrivire după nume, 10 excluse
```

Patru lucruri dovedite, nu presupuse:

1. **Potrivirea neinsensibila la caz chiar functioneaza pe date reale** — `"capitol"` a citit
   `Capitol`, iar Regula 4 nu mai e doar o intentie.
2. **`ValueOrMissing` deosebeste lipsa de NULL** — coloana inexistenta a intors `Nothing`,
   nu `DBNull`.
3. **Constatarea F3 e confirmata de codul de productie:** `HasColumn("idunitate")` e `False`
   pe `Clasificatii`. Unitatea chiar NU e pe rand.
4. **`ClasificatieDerived` se potriveste cu ce calculeaza Access insusi.** Pentru
   `65.01 / 04.02 / 10.01 / 01` a dat `Clsf=65.01.04.02.10.01.01`, `Titlu=10`,
   `ClsfF=650402`, `ClsfE=100101`, `Sector=01`, `Sursa=A`, `SS=01A` — **exact** valorile pe
   care fisierul Access le poarta in propriile coloane pe acel rand (vezi
   `artifacts/accdb-schema/baza2026.md`). Replicarea DDL-ului MariaDB produce aceleasi
   valori ca formulele Access, ceea ce e o dovada serioasa ca poarta `AVACONT_COMUN` va
   verifica valorile corecte.

**Si o constatare noua, utila:** din 13 unitati, **una singura are fisierul de nomenclatoare
prezent** (unitatea 76, ENERGETIC ISJ). Restul de 12 arata `nom=NO`. Depozitul poarta doar
acel `baza2026.accdb`. Codul le detecteaza corect, iar verificatorul va da `FISIER_LIPSA`
**BLOCANT** pe oricare dintre ele daca e bifata. De asemenea, toate cele 11 unitati cu
`CaleForexe` arata catre **acelasi** `C:\AVACONT\Forexe\FX_2026.accdb` — exact forma
«un fisier, mai multe unitati» pentru care exista filtrul `BelongsToUnit`.

### Rezultate

- `dotnet build src/KBot.Migrator/KBot.Migrator.vbproj`: **0 erori, 0 avertismente.**
- `dotnet build KBot.sln`: **0 erori** (5 avertismente, toate din alte proiecte, dinainte).
- `tools/MigratorSmoke`: **0 erori, 0 avertismente**, rulat, iesire `OK`.

Fara teste automate (§9 al planului).

### Fisiere atinse

| Fisier | Ce |
|---|---|
| `src/KBot.Migrator/MigratorForm.Designer.vb` | rescris — patru grupuri, toate controalele declarate aici |
| `src/KBot.Migrator/MigratorForm.vb` | rescris — comportamentul |
| `src/KBot.Migrator/MigratorSettings.vb` | nou — JSON, fara parole |
| `src/KBot.Migrator/Program.vb` | pornire directa, fara `ConnectForm` |
| `src/KBot.Migrator/KBot.Migrator.vbproj` | fara `KBot.Api`; comentariul refacut |
| `src/KBot.Migrator/ConnectForm.*` | **STERSE** |
| `src/KBot.Migrator/Api/MigrareApiClient.vb` | **STERS** |
| `src/KBot.Migrator/Registry/AvacontRegistry.vb` | **STERS** |
| `tools/MigratorSmoke/` | nou — ham aruncabil, nu in solutie |

### Neverificat / amanat

1. **Formularul nu a fost deschis niciodata** — nici pe ecran, nici in designerul VS. Toata
   aranjarea (`TableLayoutPanel`-uri, latimi de coloana, ancorele barei de progres) e scrisa,
   nu vazuta. Asta ramane riscul cel mai mare al feliei.
2. **Jumatatea MariaDB nu a fost atinsa deloc.** `TargetSchema`, `TargetServer`,
   `WriteOrder`, `Verifier`, `TransferRunner` si `SqlDumpWriter` compileaza si sunt scrise pe
   `000_DEMO.sql`, dar **niciun server nu a fost contactat, niciun rand nu a fost scris,
   niciun dosar de jurnal nu a fost creat**.
3. **`LastInsertedId`** pe `MySqlConnector` cu o comanda pregatita refolosita ramane neprobat;
   harta de rezolutie a clasificatiilor depinde de el.
4. **`AVACONT_COMUN` tot nu a fost vazuta.** Numele coloanelor din dictionare vin din tintele
   cheilor straine din `000_DEMO.sql`, nu dintr-o schema citita.
5. **Doar o unitate din 13 are fisier de nomenclatoare** in depozit, deci o rulare peste mai
   multe unitati nu poate fi probata aici.
6. Bifele grilei de unitati se citesc **dupa indice de rand**, presupunand ca ordinea grilei
   e cea a lui `CaiRegistry.UnitsOf`. Adevarat azi (grila nu-si muta randurile singura), dar
   e o legatura implicita: daca grila capata sortare, trebuie mutata pe cheie.

### Starea feliei 0045

Cei cinci pasi sunt gata ca **cod**:

| Pas | Ce | Stare |
|---|---|---|
| 01 | descoperirea + `MAPARE_NOMENCLATOARE.md` | gata, **rulat** (spike) |
| 02 | motorul Access + descrierea tintei | cod verde; Access **rulat** (pasul 05) |
| 03 | catalogul, hartile, portile | cod verde, nerulat |
| 04 | scrierea + jurnalul SQL | cod verde, nerulat |
| 05 | formularul + stergerea lantului HTTP | cod verde, formular nedeschis |

Ce lipseste inainte de o rulare reala, in ordinea in care blocheaza:

1. **`AVACONT_COMUN`** trebuie sa existe si sa fie populata pe server — altfel niciun rand de
   clasificatie nu intra (cinci chei straine).
2. **Formularul deschis in designerul VS si apoi pe ecran**, macar o data.
3. **O baza-tinta noua**, creata dupa `AVACONT_SURSA` (D7), si o rulare de proba pe
   unitatea 76.

## SLICE-0045-06 — schema_sync, bara de progres, bifele legate de obiect, panoul de constatari <a id="slice-0045-06"></a>

Data: 23.08.2026 · Slice **0045**, pasul 06 (pas de raspuns la revizia operatorului).

### De ce exista pasul asta

Pasul 05 se incheiase. Operatorul a **redesenat el formularul** si a trimis sase observatii.
Pasul asta le implementeaza pe toate, fara sa atinga aranjarea pe care si-a facut-o.

### 1. Formularul e al operatorului — codul se muleaza pe el, nu invers

`MigratorForm.Designer.vb` a fost modificat de operator si **nu a fost atins aici**. Ce s-a
schimbat acolo si ce a insemnat pentru codul din spate:

- `txtParolaForexe` / `lblParolaForexe` au fost **sterse**. Erau doua parole in pasul 05, una
  pentru fisierele de unitate si una pentru cele FOREXE. Acum e una singura,
  `txtParolaUnitati`, reetichetata **«Parolă fișiere»**. Codul din spate are deci o singura
  metoda `AccessPassword()` si o da amandurora — ceea ce se potriveste oricum cu descoperirea
  din 0045-01, unde **toate trei fisierele s-au deschis fara nicio parola**.
- A aparut `rtbInfoRowConstatari`, un panou de detaliu sub grila de constatari (punctul 6).
- A aparut `tlpButoane`, care aranjeaza butoanele.

**Consecinta de proiectare, luata deliberat:** optiunea `schema_sync` (punctul 4) **nu a primit
un buton**. Un buton nou ar fi insemnat sa modific aranjarea operatorului. E o **intrebare care
apare exact cand se aplica** — dupa o verificare care a gasit o baza fara tabele — si nu se
vede niciodata altfel. E si mai corect ca flux: nu e un pas al migrarii, e leacul unei singure
constatari.

### 2. schema_sync pentru o baza existenta dar goala — PE SERVER

`Schema/SchemaSyncRunner.vb` — nou. Ruleaza scriptul Python care construieste structura,
**pe VPS, prin SSH**. Niciodata pe masina operatorului.

> **Corectie fata de prima scriere a acestui pas.** Prima varianta pornea venv-ul local al
> depozitului. **Gresit, si operatorul a spus-o direct:** scriptul trebuie sa ruleze pe server.
> Motivul e mai adanc decat comoditatea — proiectul Python **desfasurat pe VPS**, cu `config.py`
> al lui si interpretorul lui, e cel care are voie sa modifice bazele. Un exemplar local pornit
> peste aceleasi baze ar fi **alt cod** indreptat spre acelasi server: alta versiune, alta
> configurare, alte dependinte. Diferenta nu se vede pana nu strica ceva.

Drumul intreg: `Verifier` gaseste ca baza-tinta exista dar are **zero tabele** → constatare
noua **`BAZA_FARA_TABELE`** (Blocant) → `VerifyAsync` o vede si cheama `OfferSchemaSyncAsync`
→ intrebare in romana, care **numeste serverul** → se lanseaza `ssh.exe` cu iesirea scriptului
**linie cu linie in jurnalul rularii** (`   | ` pentru stdout, `   ! ` pentru stderr) →
**verificarea se reia singura**.

Constatarea e deliberat **distincta de `TABEL_LIPSA`**: un tabel lipsa e o schema care a
derivat, dar ZERO tabele e o baza pe care cineva a creat-o goala, si are alt leac.

Reluarea se face cu `offerSchemaSync:=False`. Daca scriptul raporteaza succes dar nu a creat
nimic, a doua trecere **spune o data si se opreste**, in loc sa ofere la nesfarsit aceeasi
rulare.

#### Comanda: ce s-a pastrat si ce s-a adaugat

Comanda din observatia operatorului era
`python3 -m routes.schema_sync.schema_sync --mode FORCE --targets [chosen_Dc]`.
**`python3` ramane exact asa** — e grafia corecta pe partea cealalta, care e Linux. Sablonul
implicit adauga doua lucruri, si amandoua sunt necesare:

1. **`--run`, care trebuie sa ramana.** Fara el `schema_sync.py` ajunge la
   `_ask("Executați acum?")` si **citeste de la stdin**. Pe un canal SSH neinteractiv citirea
   aia da EOF — scriptul ori abandoneaza, ori sta acolo la infinit in timp ce formularul
   asteapta un proces care asteapta un om. (In plus stdin se inchide imediat dupa `Start`, si
   clientul primeste `-n`, ca sa nu citeasca fluxul nostru deloc.)
2. **`cd` catre radacina proiectului de pe server.** Modulul `routes.schema_sync.schema_sync`
   se rezolva doar de acolo. Dosarul din implicit e un **substituent** — nimeni din depozit nu
   stie unde sta proiectul pe VPS.

#### Cele patru reguli ale conexiunii

- **`BatchMode=yes`** — jumatatea SSH a aceleiasi reguli ca `--run`: clientul **pica** in loc
  sa ceara o parola sau o fraza de acces. O intrebare nu poate fi raspunsa dintr-un formular,
  deci singura alternativa la esec ar fi blocarea. Inseamna si ca **autentificarea e pe CHEIE**:
  unealta nu cere, nu tasteaza, nu pastreaza si nu trimite nicio parola.
- **`StrictHostKeyChecking` ramane pe implicit**, nu pe `accept-new`. O unealta de migrare nu
  are ce cauta in rolul de a decide tacut ca are incredere intr-un server pe care nu l-a mai
  vazut. Cu `BatchMode=yes` o gazda necunoscuta pica repede, iar `HostKeyHint()` ii spune
  operatorului sa se conecteze o data de mana si sa accepte el amprenta.
- **`IdentitiesOnly=yes`** cand e data o cheie — altfel un agent poate oferi intai altele si
  serverul poate rupe conexiunea pentru prea multe incercari.
- **Codul 255 e numit explicit** in jurnal SI in caseta: vine de la CLIENTUL SSH, nu de la
  script, deci scriptul nu a pornit. Fara asta operatorul ar cauta pe server urmele unei rulari
  care nu a existat.

#### Doua lucruri de siguranta

- **Numele DC-ului e verificat cu o lista alba** (`^[A-Za-z0-9_]+$`) inainte sa intre in
  comanda de pe server. Asta e **singurul loc** in care o data citita dintr-un fisier Access
  trece intr-un interpretor de comenzi de pe alta masina; un DC vine din `cai.DC`, e data de
  operator, nu constanta, iar un nume cu accent grav, punct-si-virgula sau `$(` **ar rula ca
  o comanda pe server**. Verificat, nu escapat — orice DC real arata ca `000_DEMO`, deci nu se
  refuza nimic legitim.
- **Argumentele se dau prin `ArgumentList`, nu ca un sir concatenat**, ca sa plece comanda
  la distanta ca **un singur argument** desi are in ea spatii, punct-si-virgula si ghilimele.

`Validate` verifica clientul, tinta, cheia si portul **inainte** de pornire, ca o configurare
gresita sa fie spusa in romana, nu ca eroare Win32. Dosarul **de pe server** nu poate fi
verificat de aici — un `cd` gresit se vede ca «No such file or directory» in jurnal, imediat si
zgomotos, la prima rulare.

`PYTHONIOENCODING=utf-8` se pune **pe latura cealalta** (`export … ; comanda`), fiindca partea
Python scrie romana cu diacritice si fara el ar ajunge mojibake ori de cate ori locala
serverului nu e UTF-8. E pus de cod, nu lasat in setare, ca un operator care editeaza comanda
sa nu-l poata scapa din greseala.

Oprirea omoara **arborele de procese** al clientului. Mesajul spune «se inchide conexiunea
SSH», nu «se opreste schema_sync», fiindca **asta e adevarul**: ce face scriptul de pe server
dupa ce cade canalul e treaba serverului. O schema aplicata pe jumatate e exact ce cauta
verificarea care urmeaza.

**Setarile** (`sshExecutable`, `sshTarget`, `sshKeyFile`, `sshPort`, `schemaSyncRemoteCommand`)
stau in `migrator-settings.json`. **Se ghiceste doar CLIENTUL** — `ssh.exe` e parte fixa a
Windows-ului de la 10 incoace, deci acolo exista un raspuns corect. Serverul, cheia si dosarul
de pe server **nu se ghicesc deloc**: niciunul nu e scris nicaieri in depozit, iar o inventie
plauzibila e mai rea decat un camp gol, care macar spune ca e gol.

**Nicio parola nu ajunge la script si niciuna nu apare pe linia de comanda** — scriptul isi ia
singur configurarea din `config.py`-ul serverului. Tocmai de aceea linia de comanda **se scrie
in jurnal**: nu poarta nicio acreditare.

### 3. Bara de progres se misca si la verificare

Pana acum bara se misca doar la transfer; verificarea o lasa moarta desi are zece porti si
poate dura. `Verifier` a capatat `StepCount` (11) si un apel `Step1(eticheta)` la fiecare
poarta, plus o functie de progres in constructor. Formularul are `BeginProgress` / `ShowStep` /
`EndProgress` / `ResetProgress`, si `StepFromWorker` face trecerea pe firul de interfata.
`BeginProgress(0)` inseamna **marquee** — asa merge bara in timpul lui `schema_sync`, unde nu
exista pasi de numarat.

### 4. Bifele se citesc din obiect, nu din indicele randului

Cuplajul pe care il consemnasem in 05 — «bifele grilei de unitati se citesc dupa indicele
randului» — a fost **scos**, nu documentat. Fiecare rand isi poarta acum obiectul in
`KBotDataRow.Tag`:

```vb
For Each row In dgvUnitati.Rows
    If Not IsTicked(row) Then Continue For
    Dim unit = TryCast(row.Tag, CaiUnit)
    If unit IsNot Nothing Then request.Units.Add(unit)
Next
```

Motivul e ca modul de dinainte **presupunea tacut ca randul _n_ al grilei e unitatea _n_ a
listei**. Presupunerea tine pana cand cineva filtreaza, sorteaza sau insereaza un rand — si
atunci nu pica, ci **migreaza ALTA UNITATE**. Cu `Tag` intrebarea nu se mai pune. Acelasi tipar
pentru grila de tabele (`TableMap`) si pentru cea de constatari (`Finding`).

### 5. Panoul de detaliu al constatarilor

Clic pe un rand din `dgvConstatari` umple `rtbInfoRowConstatari`, in forma ceruta:

```
COLOANĂ - nume_coloana
MESAJ   - mesajul constatarii
```

cu eticheta ingrosata si valoarea normala. Cele doua fonturi sunt **pastrate in campuri si
eliberate in `FormClosed`** — construite la fiecare clic ar scurge cate doua descriptoare GDI
de fiecare data. Se reconstruiesc doar daca fontul grilei s-a schimbat (schimbare de tema).

Randul se ia tot din `Tag`, nu din indice.

### Ce ramane nedovedit

Nimic din pasul asta nu a fost vazut pe ecran. **Niciun `ssh` nu a fost lansat, nicio
conexiune la VPS nu a fost deschisa, `schema_sync` nu a fost rulat niciodata din formular** —
si nici nu putea fi: `sshTarget` e gol implicit, fiindca adresa serverului nu e scrisa nicaieri
in depozit. Prima rulare reala are deci de dovedit patru lucruri deodata: ca `sshTarget` si
cheia sunt bune, ca amprenta gazdei e deja acceptata (altfel cod 255), ca dosarul din
`schemaSyncRemoteCommand` chiar exista pe server, si ca `python3` de acolo vede modulul.
Esecurile sunt in ordinea asta si toate se citesc din jurnal.

`dotnet build KBot.sln` — **0 erori**. Fara teste automate si fara pas de regresie, la cererea
explicita a operatorului (si conform §9 al planului).

## SLICE-0045-07 — unitatea e a RÂNDULUI, nu a buclei <a id="slice-0045-07"></a>

Raportat de operator, 23.08, după prima verificare cu **două unități din același DC** bifate:

1. constatările apar «de două ori»;
2. două constatări care se contrazic pe date reale —
   `IdClsf: Unitatea 75: 1 clasificații de pe 1 rânduri nu se regăsesc … (IdClsf: 141)` și
   `IdClsf: Unitatea 76: 2 clasificații de pe 3 rânduri … (IdClsf: 97, 374)` — deși
   fiecare `IdClsf` **există** în nomenclatorul unei unități.

Operatorul a bănuit o problemă de corelație. Avea dreptate, și cele două puncte sunt **aceeași**
problemă.

### Ce se întâmpla

Registrul `cai` a fost citit direct (`cale.accdb`, parola `andreI` din 0045-01) și spune ceva ce
felia 0045-01 nu a consemnat: **unsprezece din cele treisprezece rânduri arată spre ACELAȘI fișier
FOREXE**, `C:\AVACONT\Forexe\FX_2026.accdb`. Nomenclatorul e pe unitate (`baza2026.accdb`), dar
datele `FX_*` sunt într-un singur fișier partajat pe tot DC-ul.

Pe fondul ăsta, atât poarta din `Verifier` cât și `TransferRunner.BelongsToUnit` citeau
`IdUnitate` cu `AsInteger(...)`, care întoarce `Nothing` **și** pentru coloană absentă, **și**
pentru NULL — exact distincția pe care `AccessTableReader.ValueOrMissing` o păstrează cu grijă cu
un comentariu deasupra. `Nothing` era tratat ca «rândul aparține unității care se procesează
acum», adică unui fișier partajat i se citea fiecare rând fără unitate de **N** ori, o dată pentru
fiecare unitate bifată.

Măsurat pe fișierul viu:

| tabel | rânduri | cu `IdUnitate` NULL |
|---|---|---|
| `FX_DDF_REV_SA` | 32 | **4** |
| `FX_DDF_REV_SB` | 32 | **4** |
| `FX_Extrase` | 3.110 | **3.110 (toate)** |

Cele patru rânduri `FX_DDF_REV_SA` sunt chiar constatările operatorului, și lanțul lor le dă pe
față: `ID 138 ▸ IDDF 73 ▸ FX_DDF.IdUnitate = 76` (`IdClsf 141`), iar `ID 146/150/152 ▸ IDDF
77/79/80 ▸ unitatea 75` (`IdClsf 97, 374, 374`). Adică **exact invers** față de ce raporta poarta:
unitatea 75 era acuzată că nu are clasificația 141, care e a lui 76, și 76 că nu le are pe 97 și
374, care sunt ale lui 75. Restul clasificațiilor nu se plângeau fiindcă există în amândouă
nomenclatoarele — observația operatorului, verificată.

«De două ori» era același lucru văzut din altă parte: aceleași rânduri, examinate o dată pentru
fiecare unitate bifată.

Consecința la scriere era mai gravă decât raportul: `TransferRunner` ar fi **scris** cele patru
rânduri o dată pentru fiecare unitate, rezolvând `IdClsf` de fiecare dată pe nomenclatorul greșit
— ori un id tăcut greșit, ori oprirea rulării pe `BlockingOnMiss`. `FX_Extrase` ar fi plecat
întreg, 3.110 rânduri, la fiecare unitate bifată.

### Ce s-a schimbat

**`Transfer/UnitOwnership.vb` (nou).** Un singur loc care răspunde la «cărei unități îi aparține
rândul ăsta», întrebat identic de verificator și de scriitor (înainte întrebau separat și
**răspundeau diferit**). Trei stări, deliberat distincte:

- `Named` — rândul își poartă `IdUnitate`, sau i-l dă părintele;
- `ParentScoped` — tabelul **nu are deloc** coloana, deci rândul își atinge unitatea prin părinți
  (`ParentsTravelled` răspunde la asta, comportament neschimbat);
- `Unattributable` — coloana e acolo și e goală, și nimic n-o rezolvă. **Nu** «toate unitățile».

**`TableMap.OwnedVia(tabelPărinte, coloanaCopil, coloanaPărinte)`.** Declarația de proprietate,
citită de `UnitOwnership.Build`. Declarate trei:

- `FX_DDF_REV_SA` și `FX_DDF_REV_SB` ▸ `FX_DDF` prin `IDDF`;
- `FX_Extrase` ▸ `FX_Extrase_H` prin `IDFXH` ▸ `IDEXH`. Legătura **nu e ghicită după nume**:
  verificată pe fișierul viu, 3.110 rânduri se potrivesc cu 338 de antete și **zero** rămân
  orfane, iar `FX_Extrase_H.IdUnitate` e completat pe toate.

**`ColumnMapping.FromUnit("IdUnitate")`** pe cele trei. Acum că rândurile sunt filtrate pe
proprietarul lor real, unitatea buclei **este** unitatea rândului, deci se scrie ea pe țintă în
locul golului din Access. (Pe `FX_Extrase` era gol pe toate cele 3.110.)

**Poarta din `Verifier`** folosește rezoluția comună; un rând neatribuibil nu mai e verificat pe
nomenclatorul unității curente, ci raportat: constatare nouă **`UNITATE_NEDETERMINATA`** (Blocant),
cu două formulări separate pentru «coloana e goală» și «tabelul n-are coloana», ridicată **o
singură dată**, la prima trecere, fiindcă rândurile sunt aceleași la fiecare.

**`TransferRunner.BelongsToUnit`** primește harta și proprietatea; un rând neatribuibil **oprește
rularea** cu `TransferException` în loc să fie scris sub o unitate aleasă la întâmplare. E
plasa de siguranță: verificatorul îl refuză oricum înainte.

**Constatare nouă `CLASIFICATIE_NECORELATA` (Atenție).** Găsită pe drum, aceeași familie:
`FX_Istoric`, `FX_Rezervari`, `FX_Receptii_RHR` și `FX_Extrase_H` poartă o coloană `IdClsf` și
călătoresc pe **potrivire după nume**, deci id-ul LOCAL din Access ar ajunge în coloana în care
ținta ține id-ul atribuit de MariaDB — chiar capcana pentru care există Regula 1 și pentru care
`FX_DDF_REV_SA/SB` și `FX_ORD_TBL` rezolvă explicit. Verificarea se uită la PLANUL construit, nu la
catalog, deci nu se plânge dacă ținta n-are coloana. Lăsată la **Atenție**, nu Blocant: leacul e o
decizie de mapare per tabel, nu ceva de ales aici.

### Fișiere atinse

| fișier | ce |
|---|---|
| `src/KBot.Migrator/Transfer/UnitOwnership.vb` | **nou** — `UnitScope`, `Build`, `TryOwner`, `Resolve` |
| `src/KBot.Migrator/Transfer/TableMap.vb` | `UnitOwner*` + `HasUnitOwner` + `OwnedVia(...)` |
| `src/KBot.Migrator/Transfer/TableMaps.vb` | lanțurile pe SA / SB / `FX_Extrase` + `FromUnit("IdUnitate")` |
| `src/KBot.Migrator/Transfer/Verifier.vb` | poarta 7 pe rezoluția comună; garda Regula 1 în `CheckColumnPlans` |
| `src/KBot.Migrator/Transfer/TransferRunner.vb` | `BelongsToUnit` pe rezoluția comună; `UnitOwnership.Build` per unitate |
| `src/KBot.Migrator/Transfer/Finding.vb` | `UNITATE_NEDETERMINATA`, `CLASIFICATIE_NECORELATA` |
| `src/KBot.Migrator/KBot.Migrator.vbproj` | `FileVersion` 1.4.0.0 ▸ 1.5.0.0 |

### Rezultate

- `dotnet build KBot.sln` — **0 avertismente, 0 erori**. La fel proiectul singur.
- **Verificat pe datele reale ale operatorului**, nu doar compilat. O consolă de unică folosință
  (în dosarul de lucru temporar, **nu** în depozit) a legat `KBot.Migrator.dll` și a rulat poarta
  pe `FX_2026.accdb` + cele două `baza2026.accdb`:
  - regula VECHE reproduce constatările operatorului **la virgulă**: unitatea 75 → `1 clasificații
    de pe 1 rânduri (141)`, unitatea 76 → `2 clasificații de pe 3 rânduri (97, 374)`;
  - regula NOUĂ, pe aceleași fișiere: **nicio constatare**, pentru `FX_DDF_REV_SA`,
    `FX_DDF_REV_SB` și `FX_ORD_TBL`, pentru amândouă unitățile;
  - atribuirea: `FX_DDF_REV_SA` 75=9 / 76=23 (era 75=6, 76=22 și 4 nicăieri), `FX_Extrase` toate
    cele 3.110 atribuite, **zero** neatribuibile.

### Neverificat / amânat

- **Nimic nu s-a scris pe MariaDB.** Serverul din `migrator-settings.json` refuză
  utilizatorul `Admin` («Access denied»), deci schema țintei nu a fost citită în acest pas:
  ce face `ColumnPlan` cu `IdUnitate` pe `FX_Extrase` depinde de coloanele reale ale țintei. Dacă
  ținta n-are coloana, maparea derivată e sărită curat («ținta nu are coloana»); dacă o are și e
  `NOT NULL`, acum primește o valoare, unde înainte primea NULL.
- `FX_Extrase_H` mai arată **unitatea 77 (195 rânduri) și unitatea 0 (13)**, iar 77 **nu e în
  registrul `cai`**. Cu lanțul pus, rândurile lor sunt sărite ca «altă unitate», ceea ce e corect,
  dar unitatea 0 rămâne o dată ciudată pe care nimeni n-a explicat-o.
- **`ParentsTravelled` e în continuare orb la unitate.** `WrittenKeys` e ținut pe tabel, nu pe
  (tabel, unitate), iar tabelele scrise înaintea copiilor lor sunt scrise pentru TOATE unitățile,
  deci în trecerea unității 75 un rând `FX_DDF_REV` al unității 76 trece poarta părintelui. Azi e
  fără urmări (upsert pe aceeași cheie primară scrie de două ori același rând), dar `RowsWritten`
  numără dublu și un tabel `ParentScoped` care ar rezolva vreodată o clasificație ar cădea în
  aceeași groapă. Verificatorul refuză acum cazul periculos; **restrângerea lui `WrittenKeys` pe
  unitate rămâne de făcut.**
- Cele patru tabele din `CLASIFICATIE_NECORELATA` **nu au fost reparate**, doar semnalate.
- `KBot.Migrator` **nu are proiect de teste**; nu s-a creat unul în acest pas. Verificarea de mai
  sus e o rulare pe date reale, nu o suită de regresie.
- Formularul **nu a fost deschis**; constatările noi nu au fost văzute în panou.

## Completări de trasabilitate din registru

- Operatorul confirmase la 23.08 existența AVACONT_COMUN pe server; documentarea ei nu fusese citită și nici conținutul verificat în acele etape. Numele dicționarelor veneau din țintele FK ale dumpului. Existența confirmată nu este validarea porții sau a datelor.
- Registrul caracteriza dumpul din 22.08 drept 54 de tabele, numai DDL, fără INSERT/view/trigger/rutine; acestea sunt metadatele consemnate atunci, fără recitirea dumpului acum. Semnele AUTO_INCREMENT sunt maxime, nu numărători, cum precizează deja revizia 1.
- Focusul preciza că operatorul văzuse/redesenase formularul în 06; aceasta nu era acceptarea fluxurilor de verificare/transfer/schema_sync. Starea privind MariaDB din focus aparține reperului 06; 07 consemnează refuzul conexiunii și păstrează lipsa scrierilor.
- Verificarea LastInsertedId la refolosirea aceleiași comenzi rămâne materială: un zero sau ID precedent ar lega greșit clasificațiile/partenerii. Probele Access nu o închid. Limitele WrittenKeys, unitățile 77/0 și cele patru clasificări necorelate sunt păstrate la 07.
- D6 din descoperire spune «Avacont/ negitignorat, rezolvat de operator», în timp ce focusul registrului folosea «Avacont/ și /MariaDB_Schema gitignorate». Formulările istorice sunt păstrate ca neconcordanță, fără presupunere despre ignorarea actuală.
