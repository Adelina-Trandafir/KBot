# SLICE-0046 / 0047 — Proprietatea documentelor și poarta părintelui

0046 rezolvă selecția de jos în sus și golirea efectivă a valorilor; 0047 corectează comparația porții cu id-urile Access în locul rândului pregătit pentru MariaDB. Probele 0046 sunt citiri pe Access real, fără scrieri MariaDB; reluarea 0047 este încă nerulată. Convențiile IdClsf și cifrele așteptate din 0047 sunt constatări ale rulării 25.08.2026, nu o presupunere nouă despre schema actuală.

- [0046 — planul de proprietate](#slice-0046)
- [0047 — cheia care pleacă](#slice-0047)

## SLICE-0046 — proprietatea se hotărăște de JOS în SUS, și poarta părintelui reparată <a id="slice-0046"></a>

O rulare pe `000_DEMO` s-a oprit cu `1452` scriind `FX_DDF_REV` (IDREV 41 ▸ IDDF 30). Datele Access sunt corecte și relațiile din Access sunt intacte. `FX_DDF` raporta *citite 18, scrise 4, altă unitate 14*.

Fișierul viu are **9 FX_DDF**, citite de două ori pentru cele două unități bifate. Cele 14 „altă unitate” picau pe IdUnitate, relicvă declarată de operator; IDDF 30 era între ele. Copilul FX_DDF_REV, fără IdUnitate, depindea exclusiv de poarta inertă și nu fusese oprit.

### Cele două defecte, amândouă confirmate în sursă

**Defectul A — poarta părintelui inertă.** Pentru PK compus (FX_DDF: IDDF,CUAL), PrimaryKeyColumn dă Nothing; RecordPrimaryKey iese prin `If String.IsNullOrEmpty(primaryKey) Then Return`. WrittenKeys.Tracks("FX_DDF") rămâne False, iar ParentsTravelled sare legătura prin `If Not _written.Tracks(link.ParentTable) Then Continue For`: afectate FX_DDF_REV, FX_ORD, tblDocFund_Revizii_Clsf. FX_DDF_REV_SA/_SB aveau OwnedVia explicit din 0045-07. **Defectul era tăcerea, nu Nothing.**

**Defectul B — calea „coloană nulabilă" număra o golire pe care n-o făcea.** În `ParentsTravelled`:

```vb
If link.ChildColumnIsNullable Then
    outcome.ValuesNulled += 1
    Continue For
End If
```

Contorul creștea fără scriere DBNull: BuildValues nu trata orfanul, iar ForcedNull era mapare declarată, nu decizie de orfan. Valoarea pleca neatinsă, deși raportul afirma golirea coloanei nulabile cerută de operator la 22.08.

### Ce s-a schimbat

#### `Transfer/OwnershipPlan.vb` (nou) — cine pleacă, hotărât înainte de prima scriere

Unitatea DDF este în `FX_DDF_REV_SA`, iar cea ORD în `FX_ORD_TBL`, scrise după antete din cauza FK. `WrittenKeys`, umplut în timpul scrierii, nu poate decide selecția antetului; aceasta se calculează o dată, prin citire separată Access, apoi se refolosește neschimbată.

**Proprietate inversată față de 0045-07:** SA nu mai întreabă FX_DDF al cui este. D1 exclude lectura relicvei FX_DDF.IdUnitate; un IDDF poate servi mai multe unități, deci nici valoarea completată nu putea răspunde corect.

| ce citește | ce construiește |
|---|---|
| `FX_DDF_REV_SA` ▸ `IDDF`, `IdUnitate` | `IDDF → mulțimea de unități`, plus documentele fără nicio unitate |
| `FX_ORD_TBL` ▸ `IDORD`, `IdUnitate` | la fel, pentru ordonanțări |
| `FX_Extrase_F` ▸ `IDEXF`, `NumeFisier`; `FX_Extrase_H` ▸ `IDEXH`, `IDEXF` | fișierele care se potrivesc pe cod fiscal, și antetele lor |
| `FX_DDF` ▸ `IDDF` | documentele fără nicio linie de secțiune A (D6) |

Citirea folosește coloane numite prin noul `AccessSchema.OpenKeyReader`, evitând `SELECT *` și Memo-ul cu XML semnat din `FX_Extrase_F` pentru numai 72 de nume.

`Decide(map, reader, passUnits)` servește identic verificatorul/scriitorul. Tabelul explicit Routes numește subarborele prin coloana Access, fără deducere din FK țintă redenumite (IDORD → IDORDP). Celelalte 17 tabele FX_* folosesc regula generică.

#### Bucla pe unități a dispărut din `WriteTable` (D4)

**O trecere/fișier:** 11/13 rânduri cai indică același FX_2026.accdb, anterior recitit pentru fiecare unitate bifată. Apartenența la mulțimea selectată se decide o dată/rând; dispare și dubla RowsWritten rămasă din 0045-07.

**Nomenclatoarele nu se grupează:** fișierul baza2026.accdb este unitatea, fără IdUnitate pe rând. Registrul viu a confirmat 13 unități/13 căi distincte. Dacă două cai ar indica același fișier, gruparea ar pierde unitatea și ar opri rularea; citirea/scrierea clasificațiilor per unitate păstrează comportamentul anterior. Pe căile distincte nu costă suplimentar și nu refuză o configurație neprobată.

#### Cele două porți noi

**CHEIE_PARINTE_NEURMARITA (Blocant, D13):** fiecare părinte din setul de scriere trebuie să aibă cheia înregistrabilă determinabilă; altfel oprire cu ambele tabele și constrângerea numite. Ar fi prins defectul A înaintea scrierii; protecția rămâne după schimbări de schemă.

**COD_FISCAL_LIPSA (Blocant, D15):** codul fiscal pentru alegerea extraselor după nume (D8) vine din registry VBA per DC (D9). Absent și casetă goală → oprire cu DC-ul numit, evitând falsul „fișierul n-are extrase pentru noi”.

#### `ParentsTravelled` chiar golește acum (D14)

ParentsTravelled primește values și pune DBNull în dicționar înainte de continuare, făcând ValuesNulled real. Sub D5, subarborii DDF/ORD rămân integral în urmă; golirea servește celorlalte familii, inclusiv FX_Rezervari.IDREV.

#### Comentariile răsturnate, rescrise

Un comentariu care pledează pentru opusul a ce face codul e mai rău decât niciun comentariu.

| unde | ce spunea | ce spune |
|---|---|---|
| `TableMaps.FX_DDF` | (nimic despre `IdUnitate`) | D1: relicvă, nu se citește; autoritatea e `FX_DDF_REV_SA` |
| `TableMaps.FX_DDF_REV_SA` | „rândurile cu IdUnitate gol își află unitatea din FX_DDF prin IDDF" | D2/D12: tabelul ăsta **E** autoritatea; `FromUnit` și `OwnedVia` scoase |
| `TableMaps.FX_DDF_REV_SB` | „aceeași formă ca SA, inclusiv cele patru rânduri" | nu e autoritate; `IdUnitate` al rândului decide doar rândul |
| `TableMaps.FX_Extrase` | „unitatea vine din FX_Extrase_H și se scrie pe țintă" | D10: coloana pleacă din mapare cu totul |
| `TableMaps.FX_Extrase_H` | (fără notă) | D10: călătorește ca informație, nefiltrat |
| `ColumnMapping.UnitId` | „IdUnitate al fișierului citit" | unitatea **rândului**, niciodată a buclei |
| `UnitOwnership` (tot fișierul) | lanț de părinți + `Build`/`TryOwner` | doar citirea coloanei proprii; lanțul a dispărut |
| `TableMap.OwnedVia` | declarație de **părinte** | declarație de **autoritate**, cu direcția explicată |

### Fișiere atinse

| fișier | ce |
|---|---|
| `Transfer/OwnershipPlan.vb` | **nou** — `Subtree`, `RowDisposition`, `RowVerdict`, `OwnershipPlan`, `SubtreeRoute` |
| `Transfer/CodFiscalRegistry.vb` | **nou** — citește `HKCU\…\VB and VBA Program Settings\AVACONT\<DC>\CodFiscal` prin `GetSetting` |
| `Transfer/UnitOwnership.vb` | redus la `Resolve`; `UnitScope.SharedByMany` (a patra stare); `Build`/`TryOwner` șterse |
| `Transfer/TableMap.vb` | `UnitOwner*` ▸ `UnitAuthority*`; `OwnedVia` cu sens inversat |
| `Transfer/TableMaps.vb` | cele șase schimbări de catalog + notele rescrise |
| `Transfer/TransferRunner.vb` | bucla pe fișier, `FilePasses`, `RowUnit`, `ParentsTravelled` golește, antetul cu ambele coduri fiscale |
| `Transfer/Verifier.vb` | construiește planul; `CheckParentKeysTrackable`; poarta clasificațiilor pe plan; `ForexePasses` |
| `Transfer/Finding.vb` | `UNITATE_NECUNOSCUTA`, `DDF_FARA_SECTIUNE_A`, `COD_FISCAL_LIPSA`, `CHEIE_PARINTE_NEURMARITA`; `VerificationReport.Ownership` |
| `Transfer/TransferRequest.vb` | `RegistryUnits`, `KnownUnitIds`, `CodFiscalOverride`, `RegistryCodFiscal`, `ResolvedCodFiscal` |
| `Transfer/TransferResult.vb` | `RowsSubtreeSkipped`, separat de `RowsOtherUnit` |
| `Transfer/ColumnMapping.vb` | comentariul `UnitId` |
| `Access/AccessSchema.vb` | `OpenKeyReader` |
| `MigratorForm.Designer.vb` | `lblCodFiscal` + `txtCodFiscal` cu `KBotToolTip`, pe rândul 3 din `TableLayoutPanel1` |
| `MigratorForm.vb` | `_ownership` predat verificare ▸ transfer; `RegistryUnits`; nota din `SaveSettings` |
| `KBot.Migrator.vbproj` | `FileVersion` 1.5.0.0 ▸ 1.6.0.0 |

### Rezultate

- `dotnet build src/KBot.Migrator/KBot.Migrator.vbproj` — **0 erori, 0 avertismente**.
- `dotnet build KBot.sln` — **0 erori**. Cele 5 avertismente `MSB3825` (BinaryFormatter în `.resx`-urile din `KBot.App/Views/`) sunt **preexistente** și în fișiere neatinse de felia asta.
- **Rulat pe datele reale ale operatorului**, nu doar compilat. O consolă de unică folosință (în dosarul temporar, **nu** în depozit) a legat `KBot.Migrator.dll` și a rulat `OwnershipPlan` pe `C:\AVACONT\forexe\FX_2026.accdb` + `C:\AVACONT\cale.accdb`.

#### Ce se schimbă la `FX_DDF`, cu 75 + 76 bifate (exact rularea care a picat)

| | înainte | acum |
|---|---|---|
| citite | 18 (9 × 2 treceri) | **9** |
| scrise | 4 (IDDF 73, 77, 79, 80) | **5** (IDDF 30, 31, 32, 33, 64) |
| altă unitate | 14 | 0 |
| document rămas în urmă | — | 4, cu constatare numită |

**`1452` pe `FX_DDF_REV` nu mai poate apărea:** IDDF 30 pleacă acum, deci IDREV 41 își are părintele.

#### Tabel cu tabel, 75 + 76 bifate

```
FX_DDF          citite    9  pleaca    5  alta unitate   0  document ramas   4
FX_DDF_REV      citite   14  pleaca   10  alta unitate   0  document ramas   4
FX_DDF_REV_SA   citite   32  pleaca   28  alta unitate   0  document ramas   4   75=6 76=22
FX_DDF_REV_SB   citite   32  pleaca   28  alta unitate   0  document ramas   4   75=6 76=22
FX_ORD          citite   17  pleaca   17  alta unitate   0  document ramas   0
FX_ORD_TBL      citite  461  pleaca  461  alta unitate   0  document ramas   0   76=461
FX_Extrase_F    citite   72  pleaca   72  alta unitate   0  document ramas   0
FX_Extrase_H    citite  338  pleaca  338  alta unitate   0  document ramas   0
FX_Extrase      citite 3110  pleaca 3110  alta unitate   0  document ramas   0
FX_Angajamente  citite   34  pleaca   34  alta unitate   0  document ramas   0   75=29 76=5
FX_Plati        citite 2744  pleaca 2744  alta unitate   0  document ramas   0   75=267 76=2477
```

Cu **doar 76** bifată, aceleași tabele dau `FX_Angajamente` 5 / 29 altă unitate și `FX_Plati` 2477 / 267 — adică filtrarea pe unitate chiar funcționează, nu doar trece totul.

#### Un defect prins CHIAR DE rulare, nu de compilare

Prima versiune a lui `Decide` trata „o singură unitate în trecere" drept „fișierul e al unității ăleia". Cu **doar 76** bifată, asta atribuia toate cele **3.246** de rânduri `FX_Istoric` unității 76 — deși `FX_Istoric` se citește din fișierul FOREXE **partajat**. Un răspuns care se schimbă cu lista de bife e defectul lui 0045-07 cu altă pălărie. Reparat: rezerva se aplică doar `SourceFile.UnitFile`. Azi nimic nu consuma valoarea greșită (toate tabelele în starea aia călătoresc pe potrivire după nume), dar în ziua în care unul ar primi un `IdClsf` rezolvat — chiar ce cere `CLASIFICATIE_NECORELATA` — ar fi rezolvat 3.246 de rânduri pe nomenclatorul unei singure unități, fără o vorbă.

#### D8 / D9 / D15 / D16, probate una câte una

`NameCarriesCodFiscal`, nouă cazuri, toate corecte: numele real se potrivește; alt cod fiscal nu; un segment în plus în față tot se potrivește; `03062026h1717.pdf` **nu** e segment integral numeric deci nu poate coliziona; `28429190` (cifră în plus) și `02842919` (zero în față) **nu** se potrivesc — e egalitate, nu „conține".

Registry, citit viu: `000_DEMO` ▸ `2842919`, `005_CEVM` ▸ `2845508`, `038_SCGP` ▸ `2843280`, DC inexistent ▸ gol. Cele șapte cifre ale lui `000_DEMO` sunt exact ce poartă **toate cele 72** de nume de fișier din `FX_Extrase_F`.

D15: DC fără cod fiscal + casetă goală ▸ **Blocant**, 0 extrase. D16: aceeași bază cu suprascriere ▸ 72 de extrase; bază bună suprascrisă cu `9999999` ▸ 0 extrase **plus o constatare** (vezi mai jos).

### Abateri de la plan, și de ce

1. **OwnershipPlan construit în Verifier, predat TransferRunner**, obligatoriu în constructor (Nothing refuzat). Respectă construcția unică §5.4 și selecția calculată o dată/refolosită neschimbată §3: planul arătat operatorului este cel scris. Transferă se activează numai după verificare trecută; MigratorForm golește planul la dezactivare, evitând reutilizarea unuia învechit.
2. **selected = unitățile BIFATE**, potrivit D2 din 0045, deși pseudocodul §3 spune «every IdUnitate of the DC being written». Cu numai 75 selectată, interpretarea tuturor unităților ar trimite FX_ORD_TBL ale lui 76 fără nomenclator pentru IdClsf, garantând Blocant. D4 «o bază ține toate unitățile DC-ului» descrie ținta; colapsarea buclei funcționează integral cu selecția bifată.
3. **Constatare suplimentară planului:** cod fiscal completat fără niciun fișier potrivit → COD_FISCAL_LIPSA, Atenție. Zero din 72 ar părea lipsa extraselor (rațiunea D15), dar nu este Blocant: fișierul poate conține realmente extrasele altcuiva.
4. **`OwnershipPlan.NameCarriesCodFiscal` e `Public`, nu `Friend`.** E toată decizia D8 într-o funcție pură, fără stare și fără fișier în spate — singura bucată verificabilă separat, și primul lucru pe care ar trebui să-l fixeze un proiect de teste.

### Neverificat / amânat

- ⚠ **Nu s-a scris nimic pe MariaDB.** Serverul n-a fost atins în felia asta: `Verifier` și `TransferRunner` compilează și jumătatea Access e probată pe date reale, dar **poarta D13 nu a fost rulată**, fiindcă are nevoie de `information_schema` de pe țintă.
- ⚠ **`FX_DDF` pe țintă: NEVERIFICAT.** §6 al planului cere confirmarea că operatorul chiar a scos `CUAL` din cheia primară. `MariaDB_Schema/000_DEMO.sql` din depozit e din **22.08** și încă arată `PRIMARY KEY (IDDF, CUAL)` — adică e mai vechi decât schimbarea din 24.08, nu o infirmă. Nu am putut întreba serverul. **Nu contează pentru corectitudine:** dacă cheia e tot compusă, noua poartă D13 oprește rularea numind `FX_DDF` și copiii lui, în loc să dezarmeze tăcut poarta părintelui. Prima rulare reală răspunde la întrebare, zgomotos, în orice caz.
- ⚠ **Cele patru documente care nu mai pleacă.** IDDF **73, 77, 79, 80** aveau `FX_DDF.IdUnitate` completat (76, 75, 75, 75) și **plecau** înainte. Acum, sub D1 + D2 + D5, singura lor linie de secțiune A are `IdUnitate` NULL, deci **rămân în Access împreună cu reviziile lor** (IDREV 129, 133, 135, 136) și cu liniile SA/SB. Se raportează ca **Atenție**, numind IDDF-urile — nu e tăcut. E chiar ce spun deciziile, dar e o inversare completă a mulțimii care călătorea, și operatorul trebuie să știe: **leacul e completarea `IdUnitate` pe cele patru rânduri `FX_DDF_REV_SA`.**
- **§8.1 (DDF fără secțiune A ▸ Blocant) și §8.2 (toate reviziile pleacă odată cu documentul)** sunt implementate exact cum le-a scris planul, dar **niciuna nu se poate proba pe datele de azi**: nu există niciun DDF fără secțiune A, și niciun document nu servește mai mult de o unitate, deci cazul din §8.2 (o revizie ale cărei linii sunt toate ale altui DC) nu apare. §8.2 rămâne decizia cel mai probabil greșită din plan, netestată de realitate.
- **Formularul nu a fost deschis** — nici pe ecran, nici în designerul VS. `txtCodFiscal` e autorat în `.Designer.vb` pe rândul 3 (care era gol) al lui `TableLayoutPanel1`, cu rândul de umplutură mutat la sfârșit; aranjarea **nu a fost văzută**.
- **`KBot.Migrator` tot nu are proiect de teste.** Verificarea de mai sus e o rulare pe date reale, nu o suită de regresie. `NameCarriesCodFiscal` e prima candidată.
- Cele patru tabele din `CLASIFICATIE_NECORELATA` (`FX_Istoric`, `FX_Rezervari`, `FX_Receptii_RHR`, `FX_Extrase_H`) **tot nu au fost reparate**, doar semnalate — și felia asta arată exact cât costă (vezi defectul prins de rulare).
- **Rutele Flask (`PYTHON/routes/migrare/`) sunt neatinse și acum poartă un model de rutare care CONTRAZICE pe ăsta.** Nu sunt pe calea vie; divergența e consemnată aici și în `KBOT_STATUS.md` ca să fie găsită, nu descoperită.

## SLICE-0047 — poarta părintelui citea Access-ul, nu rândul care pleca <a id="slice-0047"></a>

Operatorul a raportat, după rularea `20260825_085520` pe `000_DEMO`: **datele ajung pe server, dar sunt incomplete și greșite**. Cazul numit: angajamentul `AAB2EF2MCP4` are **22** de rânduri în `FX_DDF_REV_SA` (și tot atâtea în `_SB`), iar jurnalul arată că au plecat **două**.

Jurnalul de final spunea, la virgulă, unde se duseseră celelalte:

```
Clasificatii_Buget: citite 101, scrise 12, părinte netransferat 89
Parteneri_Coduri:   citite  36, scrise  5, părinte netransferat 31
FX_DDF_REV_SA:      citite  32, scrise  2, document rămas în urmă 4, părinte netransferat 26, valori golite 28
FX_DDF_REV_SB:      citite  32, scrise  2, document rămas în urmă 4, părinte netransferat 26, valori golite 28
FX_ORD_TBL:         citite 461, scrise 461, valori golite 844
```

### Defectul — un singur rând de cod, patru tabele stricate

`TransferRunner.ParentsTravelled` verifica dacă părintele unui rând a plecat citind **coloana Access** cu același nume ca a copilului:

```vb
Dim raw = reader.ValueOrMissing(link.AccessColumn)
...
If _written.Contains(link.ParentTable, raw) Then Continue For
```

`WrittenKeys` Clasificatii conține ID-uri MariaDB atribuite prin `RecordAssignedId` → `_written.Add(map.TargetTable, assigned)`, 1..101 pe baza goală. Access IdClsf pe angajamentul sesizat: 92, 123, 128, 138, 143, 144, 145, 146, 154, 158. Poarta compara aceste două domenii: adevărat întâmplător pentru ID Access în 1..101, fals în rest.

Se verifică pe cifre, exact:

| tabel | ce s-a întâmplat |
|---|---|
| `Clasificatii_Buget` | unitatea 75 are 7 clasificații Access cu id ≤ 101, unitatea 76 are 5. **7 + 5 = 12**, chiar numărul scris din 101 |
| `FX_DDF_REV_SA` / `_SB` | din cele zece clasificații ale angajamentului doar `92` e ≤ 101, iar `IdClsf` e `NOT NULL` acolo ▸ celelalte rânduri **cad**. Cele două rânduri rămase sunt reviziile 44 și 47 pe `IdClsf = 92` |
| `FX_ORD_TBL` | acolo `IdClsf` e nulabil ▸ rândul se scrie, dar **golit**: 383 din 461 de rânduri au ajuns pe server **fără clasificație**. 383 + 461 de `IdPartener` (`ForcedNull`, declarat) = **844**, chiar „valori golite" din jurnal |
| `Parteneri_Coduri` | `ParteneriAng.IdPartener` din Access ține `7605+`, id-uri ale serverului vechi, iar `Parteneri` are atribuite `1..291` ▸ 31 din 36 cad |

**Rezoluția era corectă:** rândul trecut are IdClsfAcc = 92, IdClsf = 51, al 51-lea scris: 47 pentru unitatea 75, apoi 79→48, 84→49, 90→50, 92→51 pentru 76. ClasificatiiMap/ParteneriMap și BuildValues puneau valoarea bună în values; numai poarta o ignora și respingea rândul.

La redenumiri, FX_ORD_TBL.IDORDP țintă vine din IDORD Access; sursa are și propria IDORDP cu ID-uri server vechi 117..123. Comentariul ParentLink.AccessColumn presupunea nume copil = nume Access ori coloană absentă. Coloana exista, cu alt sens.

### Leacul

`ParentsTravelled` se uită acum la **valoarea care pleacă**, nu la fișierul Access:

```vb
Dim outgoing As Object = Nothing
If Not values.TryGetValue(link.ChildColumn, outgoing) Then Continue For
```

`values` conține rândul de scris și alimentează WrittenKeys prin RecordPrimaryKey, unificând sensul comparației. Parametrul reader dispare, eliminând lectura greșită posibilă.

Coloana nescrisă nu trimite ID, deci legătura se omite fără presupuneri. Pentru NOT NULL fără implicit, MariaDB oprește explicit tranzacția.

`ParentLink.AccessColumn` s-a redenumit `ChildColumn` (era deja expusă și așa, ca proprietate derivată). E un nume de ȚINTĂ și nimic altceva.

### Ce NU s-a atins

- **`FX_Istoric`, `FX_Indicatori`, `FX_Rezervari`, `FX_Plati`, `FX_Receptii` scriu `IdClsf` prin potrivire după nume, adică id-ul ACCESS — și așa trebuie.** Coloana e inversată față de familia DDF/ORD, iar rutele Flask o spun în clar: `routes/forexe/istoric.py` face `WHERE c.IdClsfAcc IN (SELECT IdClsf FROM FX_Istoric …)`, în timp ce `routes/forexe/ddf.py` și `ord.py` fac `WHERE c.IDClsf = sa.IdClsf`. Nu există cheie străină pe coloanele acelea, de-aia poarta nici nu s-a atins de ele. Constatarea `CLASIFICATIE_NECORELATA` a feliei 0045-07 le numește pe toate cinci; pentru acestea e **alarmă falsă**, și rămâne așa până când cineva măsoară fiecare tabel pe schema vie, nu pe numele coloanei.
- **`document rămas în urmă 4`** e decizia D5 (`IDDF 73/77/79/80`, secțiune A cu `IdUnitate` NULL), declarată în felia 0046. Angajamentul din sesizare e `IDDF 33` și nu e printre ele.
- **`FX_Rezervari: valori golite 3`** sunt trei `IDREV` care arată spre reviziile celor patru documente de mai sus — golire corectă pe coloană nulabilă (D14), nereparată fiindcă nu e stricată.

### Ce urmează

Nerulat pe server. `Clasificatii` e **insert-only** (D8: nu există cheie unică pe `(IdClsfAcc, IdUnitate)`), deci reluarea cere din nou o bază **goală** — nu se poate suprascrie peste rularea din 25.08.

La prima rulare reală, cifrele de verificat sunt fix cele de mai sus, întoarse: `Clasificatii_Buget` **101/101**, `Parteneri_Coduri` **36/36**, `FX_DDF_REV_SA` și `_SB`
**28/32** (cele 4 rămân la D5), `FX_ORD_TBL` **461 scrise cu 461 „valori golite"**, nu 844 —
adică numai `IdPartener`, cel declarat.

`KBot.Migrator` **tot nu are proiect de teste.**

Reper de compilare 0047 păstrat din registru: 0 erori / 0 avertismente; nerulat pe MariaDB.
