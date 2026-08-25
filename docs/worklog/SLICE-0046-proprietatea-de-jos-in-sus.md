# SLICE-0046 — proprietatea se hotărăște de JOS în SUS, și poarta părintelui reparată

O rulare pe `000_DEMO` s-a oprit cu `1452` scriind `FX_DDF_REV` (IDREV 41 ▸ IDDF 30). Datele
Access sunt corecte și relațiile din Access sunt intacte. `FX_DDF` raporta *citite 18, scrise 4,
altă unitate 14*.

Cele două cifre spun singure toată felia: fișierul viu are **nouă** rânduri `FX_DDF`, nu
optsprezece. Optsprezece = nouă citite **de două ori**, o dată pentru fiecare unitate bifată. Și
cele paisprezece „ale altei unități" erau rânduri picate pe `FX_DDF.IdUnitate`, coloană pe care
operatorul a declarat-o relicvă — IDDF 30 era printre ele, iar copilul lui nu a fost oprit fiindcă
`FX_DDF_REV` n-are `IdUnitate`, deci depindea în întregime de o poartă care era **inertă**.

## Cele două defecte, amândouă confirmate în sursă

**Defectul A — poarta părintelui nu s-a armat niciodată.** `PrimaryKeyColumn` întoarce `Nothing`
când cheia primară are mai multe coloane. `FX_DDF` era `PRIMARY KEY (IDDF, CUAL)`, deci
`RecordPrimaryKey` cădea pe `If String.IsNullOrEmpty(primaryKey) Then Return` și nu înregistra
nimic; `WrittenKeys.Tracks("FX_DDF")` rămânea `False`; iar prima linie din `ParentsTravelled` —
`If Not _written.Tracks(link.ParentTable) Then Continue For` — arunca legătura spre `FX_DDF`
pentru **fiecare** copil care arată spre el: `FX_DDF_REV`, `FX_ORD`, `tblDocFund_Revizii_Clsf`.
`FX_DDF_REV_SA` și `_SB` au scăpat doar fiindcă 0045-07 le dăduse un `OwnedVia` explicit.
**Tăcerea era defectul, nu `Nothing`-ul.**

**Defectul B — calea „coloană nulabilă" număra o golire pe care n-o făcea.** În `ParentsTravelled`:

```vb
If link.ChildColumnIsNullable Then
    outcome.ValuesNulled += 1
    Continue For
End If
```

Incrementa contorul și mergea mai departe. `BuildValues` n-are ramură de orfan — `ForcedNull` e o
mapare **declarată**, nu o decizie de orfan — deci nimic nu scria vreodată `DBNull` în coloana
aceea, iar valoarea orfană pleca la server neatinsă. Decizia operatorului din 22.08 („coloană
nulabilă ▸ rândul se scrie cu acea coloană golită") **nu se întâmplase niciodată**, iar numărul
raportat era o minciună.

## Ce s-a schimbat

### `Transfer/OwnershipPlan.vb` (nou) — cine pleacă, hotărât înainte de prima scriere

Unitatea unui DDF stă în `FX_DDF_REV_SA`, iar a unui ORD în `FX_ORD_TBL` — tabele scrise **după**
capul lor, fiindcă cheia străină arată în direcția aia. `WrittenKeys` se umple pe măsură ce
rularea merge, deci nu poate răspunde la „pleacă documentul ăsta?" în clipa în care se scrie
`FX_DDF`. Așa că selecția se rezolvă **o dată**, într-o citire separată a fișierului Access, și se
refolosește neschimbată.

Săgeata proprietății s-a **întors**. 0045-07 o avea invers: SA întreba `FX_DDF` al cui e. Citirea
aia a murit cu D1 — `FX_DDF.IdUnitate` e relicvă, nu se citește niciodată, și **un IDDF poate servi
mai multe unități**, deci n-ar fi putut răspunde cinstit nici acolo unde era completată.

| ce citește | ce construiește |
|---|---|
| `FX_DDF_REV_SA` ▸ `IDDF`, `IdUnitate` | `IDDF → mulțimea de unități`, plus documentele fără nicio unitate |
| `FX_ORD_TBL` ▸ `IDORD`, `IdUnitate` | la fel, pentru ordonanțări |
| `FX_Extrase_F` ▸ `IDEXF`, `NumeFisier`; `FX_Extrase_H` ▸ `IDEXH`, `IDEXF` | fișierele care se potrivesc pe cod fiscal, și antetele lor |
| `FX_DDF` ▸ `IDDF` | documentele fără nicio linie de secțiune A (D6) |

Citirea e pe **coloane numite**, nu `SELECT *` — `AccessSchema.OpenKeyReader` (nou). `FX_Extrase_F`
ține XML-ul semnat al fiecărui extras într-un Memo, și n-are rost citit ca să afli 72 de nume.

`Decide(map, reader, passUnits)` e **singura** funcție întrebată, identic, de verificator și de
scriitor. Familiile rutate stau într-un tabel scurt și explicit (`Routes`) — care coloană numește
subarborele e un fapt despre **fișierul Access**, iar cheile țintei au fost redenumite pe dedesubt
(`IDORD` ▸ `IDORDP`), deci nu se poate deduce din cheile străine. Restul celor șaptesprezece tabele
`FX_*` cad pe regula generică.

### Bucla pe unități a dispărut din `WriteTable` (D4)

O trecere pe **fișier**, nu pe unitate. Unsprezece din treisprezece rânduri `cai` numesc ACELAȘI
`FX_2026.accdb`, deci bucla însemna citirea aceluiași fișier o dată pentru fiecare unitate bifată.
Întrebarea a devenit **apartenența la mulțimea selectată**, pusă o dată pe rând. Numărătoarea dublă
a lui `RowsWritten`, lăsată deschisă de 0045-07, dispare odată cu a doua trecere.

**Nomenclatoarele NU se grupează**, deliberat. Fiecare unitate are propriul `baza2026.accdb`
(verificat pe registrul viu: treisprezece unități, treisprezece căi distincte, niciuna comună), iar
rândurile alea n-au deloc `IdUnitate` — **fișierul E unitatea**. Dacă două rânduri `cai` ar arăta
vreodată spre UN singur fișier de nomenclatoare, gruparea le-ar lăsa fără unitate și ar opri
rularea, unde scrierea acelorași clasificații o dată per unitate e o citire apărabilă și e
comportamentul tuturor feliilor de până acum. Negruparea nu costă nimic — căile sunt oricum
distincte — și refuză să transforme o formă neprobată într-un refuz.

### Cele două porți noi

**`CHEIE_PARINTE_NEURMARITA` (Blocant, D13).** Pentru fiecare tabel din setul de scriere care e
părintele altuia din același set, cheia pe care o va înregistra trebuie să fie determinabilă. Dacă
nu e, rularea se oprește **numind amândouă tabelele și constrângerea**. Poarta asta ar fi prins
defectul A înainte de primul rând scris, și rămâne folositoare după schimbarea de schemă.

**`COD_FISCAL_LIPSA` (Blocant, D15).** Extrasele se aleg pe codul fiscal din numele fișierului
(D8), citit din registrul VBA per DC (D9). Lipsă din registry **și** casetă goală = oprire, cu
DC-ul numit. Potrivirea tăcută a nimicului ar arăta exact ca „fișierul n-are extrase pentru noi".

### `ParentsTravelled` chiar golește acum (D14)

Pune `DBNull` în dicționarul de valori al rândului înainte să meargă mai departe, deci
`ValuesNulled` nu mai minte. De asta a primit `values` ca parametru. Sub D5 niciun rând DDF sau ORD
nu ajunge pe calea asta — subarborii lor rămân în urmă întregi — deci reparația servește celelalte
familii (`FX_Rezervari.IDREV` și restul).

### Comentariile răsturnate, rescrise

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

## Fișiere atinse

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

## Rezultate

- `dotnet build src/KBot.Migrator/KBot.Migrator.vbproj` — **0 erori, 0 avertismente**.
- `dotnet build KBot.sln` — **0 erori**. Cele 5 avertismente `MSB3825` (BinaryFormatter în
  `.resx`-urile din `KBot.App/Views/`) sunt **preexistente** și în fișiere neatinse de felia asta.
- **Rulat pe datele reale ale operatorului**, nu doar compilat. O consolă de unică folosință (în
  dosarul temporar, **nu** în depozit) a legat `KBot.Migrator.dll` și a rulat `OwnershipPlan` pe
  `C:\AVACONT\forexe\FX_2026.accdb` + `C:\AVACONT\cale.accdb`.

### Ce se schimbă la `FX_DDF`, cu 75 + 76 bifate (exact rularea care a picat)

| | înainte | acum |
|---|---|---|
| citite | 18 (9 × 2 treceri) | **9** |
| scrise | 4 (IDDF 73, 77, 79, 80) | **5** (IDDF 30, 31, 32, 33, 64) |
| altă unitate | 14 | 0 |
| document rămas în urmă | — | 4, cu constatare numită |

**`1452` pe `FX_DDF_REV` nu mai poate apărea:** IDDF 30 pleacă acum, deci IDREV 41 își are părintele.

### Tabel cu tabel, 75 + 76 bifate

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

Cu **doar 76** bifată, aceleași tabele dau `FX_Angajamente` 5 / 29 altă unitate și `FX_Plati`
2477 / 267 — adică filtrarea pe unitate chiar funcționează, nu doar trece totul.

### Un defect prins CHIAR DE rulare, nu de compilare

Prima versiune a lui `Decide` trata „o singură unitate în trecere" drept „fișierul e al unității
ăleia". Cu **doar 76** bifată, asta atribuia toate cele **3.246** de rânduri `FX_Istoric` unității
76 — deși `FX_Istoric` se citește din fișierul FOREXE **partajat**. Un răspuns care se schimbă cu
lista de bife e defectul lui 0045-07 cu altă pălărie. Reparat: rezerva se aplică doar
`SourceFile.UnitFile`. Azi nimic nu consuma valoarea greșită (toate tabelele în starea aia
călătoresc pe potrivire după nume), dar în ziua în care unul ar primi un `IdClsf` rezolvat — chiar
ce cere `CLASIFICATIE_NECORELATA` — ar fi rezolvat 3.246 de rânduri pe nomenclatorul unei singure
unități, fără o vorbă.

### D8 / D9 / D15 / D16, probate una câte una

`NameCarriesCodFiscal`, nouă cazuri, toate corecte: numele real se potrivește; alt cod fiscal nu;
un segment în plus în față tot se potrivește; `03062026h1717.pdf` **nu** e segment integral numeric
deci nu poate coliziona; `28429190` (cifră în plus) și `02842919` (zero în față) **nu** se
potrivesc — e egalitate, nu „conține".

Registry, citit viu: `000_DEMO` ▸ `2842919`, `005_CEVM` ▸ `2845508`, `038_SCGP` ▸ `2843280`, DC
inexistent ▸ gol. Cele șapte cifre ale lui `000_DEMO` sunt exact ce poartă **toate cele 72** de
nume de fișier din `FX_Extrase_F`.

D15: DC fără cod fiscal + casetă goală ▸ **Blocant**, 0 extrase. D16: aceeași bază cu suprascriere
▸ 72 de extrase; bază bună suprascrisă cu `9999999` ▸ 0 extrase **plus o constatare** (vezi mai jos).

## Abateri de la plan, și de ce

1. **`OwnershipPlan` se construiește în `Verifier` și se PREDĂ lui `TransferRunner`**, care îl cere
   în constructor și refuză `Nothing`. Planul (§5.4) cere un singur loc de construcție; asta îl dă
   literal, și în plus dă garanția din §3 — „selecția se rezolvă o dată și se refolosește
   neschimbată" — fiindcă ce i s-a arătat operatorului e chiar ce se scrie. Merge fiindcă
   «Transferă» e activat doar de o verificare trecută. `MigratorForm` golește planul de fiecare
   dată când «Transferă» se dezactivează, ca unul învechit să nu supraviețuiască verificării care
   l-a justificat.
2. **`selected` = unitățile BIFATE**, nu toate unitățile DC-ului. Pseudocodul din §3 scrie „every
   IdUnitate of the DC being written", dar citirea aia intră în conflict cu decizia D2 din 0045
   („operatorul alege unitatea") și s-ar auto-sabota: rândurile `FX_ORD_TBL` sunt toate ale
   unității 76, deci cu doar 75 bifată ar călători și n-ar avea nomenclator pe care să-și rezolve
   `IdClsf` — Blocant garantat. Titlul lui D4 („o bază ține toate unitățile DC-ului") e o observație
   despre **țintă**, iar consecința cerută — colapsarea buclei — se obține întreg cu bifele.
3. **O constatare în plus, neceruta de plan:** cod fiscal setat care nu potrivește **niciun** fișier
   ▸ `COD_FISCAL_LIPSA` la **Atenție**. E chiar raționamentul lui D15 cu un pas mai departe: 0 din
   72 arată identic cu „n-avem extrase aici", iar operatorul nu poate distinge. Atenție și nu
   Blocant, fiindcă un fișier care chiar ține extrasele altcuiva e o formă reală.
4. **`OwnershipPlan.NameCarriesCodFiscal` e `Public`, nu `Friend`.** E toată decizia D8 într-o
   funcție pură, fără stare și fără fișier în spate — singura bucată verificabilă separat, și
   primul lucru pe care ar trebui să-l fixeze un proiect de teste.

## Neverificat / amânat

- ⚠ **Nu s-a scris nimic pe MariaDB.** Serverul n-a fost atins în felia asta: `Verifier` și
  `TransferRunner` compilează și jumătatea Access e probată pe date reale, dar **poarta D13 nu a
  fost rulată**, fiindcă are nevoie de `information_schema` de pe țintă.
- ⚠ **`FX_DDF` pe țintă: NEVERIFICAT.** §6 al planului cere confirmarea că operatorul chiar a scos
  `CUAL` din cheia primară. `MariaDB_Schema/000_DEMO.sql` din depozit e din **22.08** și încă arată
  `PRIMARY KEY (IDDF, CUAL)` — adică e mai vechi decât schimbarea din 24.08, nu o infirmă. Nu am
  putut întreba serverul. **Nu contează pentru corectitudine:** dacă cheia e tot compusă, noua
  poartă D13 oprește rularea numind `FX_DDF` și copiii lui, în loc să dezarmeze tăcut poarta
  părintelui. Prima rulare reală răspunde la întrebare, zgomotos, în orice caz.
- ⚠ **Cele patru documente care nu mai pleacă.** IDDF **73, 77, 79, 80** aveau `FX_DDF.IdUnitate`
  completat (76, 75, 75, 75) și **plecau** înainte. Acum, sub D1 + D2 + D5, singura lor linie de
  secțiune A are `IdUnitate` NULL, deci **rămân în Access împreună cu reviziile lor** (IDREV 129,
  133, 135, 136) și cu liniile SA/SB. Se raportează ca **Atenție**, numind IDDF-urile — nu e tăcut.
  E chiar ce spun deciziile, dar e o inversare completă a mulțimii care călătorea, și operatorul
  trebuie să știe: **leacul e completarea `IdUnitate` pe cele patru rânduri `FX_DDF_REV_SA`.**
- **§8.1 (DDF fără secțiune A ▸ Blocant) și §8.2 (toate reviziile pleacă odată cu documentul)** sunt
  implementate exact cum le-a scris planul, dar **niciuna nu se poate proba pe datele de azi**: nu
  există niciun DDF fără secțiune A, și niciun document nu servește mai mult de o unitate, deci
  cazul din §8.2 (o revizie ale cărei linii sunt toate ale altui DC) nu apare. §8.2 rămâne decizia
  cel mai probabil greșită din plan, netestată de realitate.
- **Formularul nu a fost deschis** — nici pe ecran, nici în designerul VS. `txtCodFiscal` e autorat
  în `.Designer.vb` pe rândul 3 (care era gol) al lui `TableLayoutPanel1`, cu rândul de umplutură
  mutat la sfârșit; aranjarea **nu a fost văzută**.
- **`KBot.Migrator` tot nu are proiect de teste.** Verificarea de mai sus e o rulare pe date reale,
  nu o suită de regresie. `NameCarriesCodFiscal` e prima candidată.
- Cele patru tabele din `CLASIFICATIE_NECORELATA` (`FX_Istoric`, `FX_Rezervari`, `FX_Receptii_RHR`,
  `FX_Extrase_H`) **tot nu au fost reparate**, doar semnalate — și felia asta arată exact cât costă
  (vezi defectul prins de rulare).
- **Rutele Flask (`PYTHON/routes/migrare/`) sunt neatinse și acum poartă un model de rutare care
  CONTRAZICE pe ăsta.** Nu sunt pe calea vie; divergența e consemnată aici și în `KBOT_STATUS.md`
  ca să fie găsită, nu descoperită.
