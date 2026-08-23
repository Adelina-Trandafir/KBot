# SLICE-0045-01 — pasul de descoperire (`PLAN_MigratorDirect.md` §12)

Data: 23.08.2026 · Slice **0045** (primul liber in `KBOT_STATUS.md`; 0044 era ultimul).

## Ce s-a facut si de ce

Planul `PLAN_MigratorDirect.md` cere, INAINTE de orice cod de transfer (§12, "FIRST, before
any implementation"), un pas de descoperire: se citesc cele trei fisiere `.accdb` din
`Avacont/`, se documenteaza schemele, se raspunde la sase intrebari si se scrie
`docs/MAPARE_NOMENCLATOARE.md`. Nu s-a scris niciun rand din transferul propriu-zis, niciun
formular, nicio corelatie de coloane — asa cum cere §12 ("Read, record, report, stop").

Pasul acesta e si dovada tehnica pe care §1 si §3 se sprijina: ca ACE deschide fisierele, ca
un build x64 pe .NET 8 merge, si ca parola e cablata corect.

### 1. Spike-ul

`tools/AccdbSchemaDump/` — proiect consola C#, `net8.0`, `PlatformTarget=x64`,
`System.Data.OleDb` 8.0.0. **NU e adaugat in `KBot.sln`** (cerut explicit de §12.1). Deschide
fiecare `.accdb` cu `Microsoft.ACE.OLEDB.16.0`, cu cadere pe `12.0`, prin
`OleDbConnectionStringBuilder` (nu concatenare) si `Jet OLEDB:Database Password` acolo unde e
nevoie. Scrie per fisier un `.md` cu: lista de tabele + numarul de randuri, obiectele care nu
sunt tabele, `GetSchema("Columns")`, `GetSchema("Indexes")`, `GetSchema("ForeignKeys")` si
primele 20 de randuri din fiecare tabel, cu textul taiat la 20 de caractere — aceeasi forma ca
`FX_System_Export/TABLES/*.md`.

Rezultatele sunt in `artifacts/accdb-schema/`: `cale.md`, `baza2026.md`, `FX_2026.md`.

### 2. Livrabilul

`docs/MAPARE_NOMENCLATOARE.md` — maparea celor patru tabele de referinta, raspunsurile la
cele sase intrebari din §12.2, corectiile datorate lui `MAPARE_ACCESS_MARIADB.md`, ce nu s-a
putut stabili, si intrebarile care blocheaza pasul de implementare.

## Constatarile care schimba planul

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

## Raspunsuri la §12.2

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

## Corectii datorate lui `MAPARE_ACCESS_MARIADB.md`

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

## Fisierele atinse

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

## Rezultate

`dotnet build -c Release` pe spike: **succes, 0 erori**, 13 avertismente `CA1416`
(platform-compatibility pe `OleDb`, care e Windows-only — proiectul tinteste `net8.0`, nu
`net8.0-windows`). Nu le-am stins: e un spike aruncabil, iar `net8.0` e ce cere §12.1.

Rulat pe toate trei fisierele: **toate trei s-au deschis**, cu `Microsoft.ACE.OLEDB.16.0`,
**fara parola**. 11 + 69 + 27 tabele citite, cu numaratori si esantioane.

Nu s-au scris teste (§9 al planului: "No automated tests in this pass unless explicitly
asked for").

## Neverificat / amanat

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

## Ce urmeaza — intrebari care blocheaza implementarea

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

# Revizia 1 — 23.08.2026, cu schema MariaDB reala

Operatorul a pus `MariaDB_Schema/000_DEMO.sql` (84.779 B, 22.08.2026) si a raspuns la
intrebarile care blocau. Golul principal semnalat mai sus — «schema reala MariaDB nu a fost
citita» — e **inchis**: ambele laturi vin acum din sursa. `docs/MAPARE_NOMENCLATOARE.md` a
fost rescris in intregime pe DDL real; nu mai contine nicio lista de coloane dedusa dintr-un
`INSERT` de ruta Flask.

## Deciziile operatorului (acum in §0 al livrabilului)

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

## Constatari noi, din schema

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

## Corectii datorate lui `MAPARE_ACCESS_MARIADB.md` — acum SAPTE

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

## Fisiere atinse in revizia 1

| Fisier | Ce |
|---|---|
| `docs/MAPARE_NOMENCLATOARE.md` | rescris integral pe DDL real; §0 decizii, F6, §3.1/§3.2/§3.3, §7 `Unitati`, §9 sapte corectii, §10 sase intrebari |
| `docs/worklog/SLICE-0045-01-descoperire-nomenclatoare.md` | aceasta sectiune |
| `docs/worklog/KBOT_STATUS.md` | randul 0045-01 + Current focus, aduse la zi |

Cod: **niciunul**. `KBot.sln`, `src/KBot.Migrator` si spike-ul, neatinse. Pasul e tot
descoperire.

## Ce a ramas deschis

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
