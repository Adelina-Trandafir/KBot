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
