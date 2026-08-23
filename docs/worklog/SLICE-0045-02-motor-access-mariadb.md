# SLICE-0045-02 — motorul: citirea Access + descrierea tintei MariaDB

Data: 23.08.2026 · Slice **0045**, pasul 02 (implementare, prima transa).

## Ce s-a facut si de ce

Prima transa de cod a migratorului direct. Am ales sa o adaug **alaturi** de codul HTTP al
feliei 0044, nu in locul lui: asa fiecare pas ramane verde si compilabil, iar stergerea
fisierelor vechi (`ConnectForm`, `Api/MigrareApiClient`, `MigratorForm` cel vechi) se face
intr-un pas al ei, cand formularul nou e gata sa ii ia locul. Nimic din codul vechi nu a fost
atins.

Transa asta acopera **temelia**: cititul din Access si intrebatul tintei despre ea insasi.
Nu contine inca nici maparea coloanelor, nici portile de verificare, nici scrierea, nici
formularul.

### Proiectul

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

### `Access/` — patru fisiere

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

### `MariaDb/` — patru fisiere

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

## Rezultate

`dotnet build src/KBot.Migrator/KBot.Migrator.vbproj`: **succes, 0 erori, 0 avertismente.**
`dotnet build KBot.sln`: **succes, 0 erori** (5 avertismente, toate din alte proiecte, dinainte).

Nu s-au scris teste (§9 al planului: «No automated tests in this pass unless explicitly asked
for»).

## Fisiere atinse

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

## Neverificat / amanat

1. **Nimic din tot pasul asta nu a atins un server MariaDB viu.** `TargetSchema`, `WriteOrder`
   si `TargetServer` compileaza si sunt scrise pe `000_DEMO.sql`, dar niciunul nu a rulat.
2. **`AccessProvider` si `CaiRegistry` nu au fost rulate din aceasta aplicatie** — dovada ca ACE
   deschide fisierele vine din spike-ul feliei 0045-01, care e alt proces si alt proiect.
3. **Formularul nu exista inca**, deci nimic nu se vede pe ecran.
4. Portile de verificare (§5 al planului), maparea coloanelor, scrierea si dosarul SQL — pasii
   urmatori.

## Ce urmeaza

- **0045-03**: maparea coloanelor (din amandoua fisierele MAPARE), harta de rezolutie a
  clasificatiilor, portile de verificare — inclusiv cele doua noi pe care le-a scos la iveala
  schema: `Unitati` ca preconditie si `AVACONT_COMUN` pentru cele cinci chei cross-baza.
- **0045-04**: scrierea tranzactionala + dosarul de instructiuni SQL.
- **0045-05**: formularul, si stergerea codului HTTP al feliei 0044.

Raman deschise, din `docs/MAPARE_NOMENCLATOARE.md` §10: `ParteneriAng` ▸ `Parteneri_Coduri` in
scop sau nu, si cine populeaza `Unitati` (unealta sau operatorul) — aceeasi intrebare cu
utilizatorul si drepturile pentru un DC nou.
