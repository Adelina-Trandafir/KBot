# SLICE-0045-04 — scrierea tranzacțională + dosarul de instrucțiuni SQL

Data: 23.08.2026 · Slice **0045**, pasul 04.

## Ce s-a facut si de ce

A treia transa: partea care chiar scrie. Tot alaturi de lantul HTTP al feliei 0044.
Formularul e pasul 05.

### `ValueConverter` — o singura traducere

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

### `SqlDumpWriter` — dosarul

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

### `TransferRunner` — rularea

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

## Rezultate

`dotnet build src/KBot.Migrator/KBot.Migrator.vbproj`: **succes, 0 erori, 0 avertismente.**

Compilatorul a prins doua ocurente ale capcanei VB de umbrire neinsensibila la caz — un
local `path` peste `System.IO.Path` si un local `parentLinks` peste metoda `ParentLinks`.
Amandoua redenumite.

Fara teste (§9 al planului).

## Fisiere atinse

| Fisier | Ce |
|---|---|
| `src/KBot.Migrator/Transfer/ValueConverter.vb` | nou |
| `src/KBot.Migrator/Transfer/TransferResult.vb` | nou |
| `src/KBot.Migrator/Transfer/TransferRunner.vb` | nou |
| `src/KBot.Migrator/Dump/SqlDumpWriter.vb` | nou |
| `src/KBot.Migrator/Transfer/TableMap.vb` | `Feeds` / `FeedKeyColumn` + `ResolutionTarget` |
| `src/KBot.Migrator/Transfer/TableMaps.vb` | `Clasificatii` si `Parteneri` isi declara harta |
| `src/KBot.Migrator/MariaDb/TargetServer.vb` | `Describe()`, delegat spre `TargetConnection` |

## Neverificat / amanat

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

## Ce urmeaza

**0045-05**: formularul (`MigratorForm` rescris, cu grupurile «Fișiere», «Server MariaDB»,
«Unitate», «Transfer», toate controalele declarate in `.Designer.vb`), persistenta
setarilor prin `KBot.LocalStore` **fara parole**, si stergerea fisierelor HTTP ale feliei
0044 (`ConnectForm`, `Api/MigrareApiClient`, formularul vechi) impreuna cu referinta la
`KBot.Api`.
