# SLICE-0045-07 — unitatea e a RÂNDULUI, nu a buclei

Raportat de operator, 23.08, după prima verificare cu **două unități din același DC** bifate:

1. constatările apar «de două ori»;
2. două constatări care se contrazic pe date reale —
   `IdClsf: Unitatea 75: 1 clasificații de pe 1 rânduri nu se regăsesc … (IdClsf: 141)` și
   `IdClsf: Unitatea 76: 2 clasificații de pe 3 rânduri … (IdClsf: 97, 374)` — deși
   fiecare `IdClsf` **există** în nomenclatorul unei unități.

Operatorul a bănuit o problemă de corelație. Avea dreptate, și cele două puncte sunt **aceeași**
problemă.

## Ce se întâmpla

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

## Ce s-a schimbat

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

## Fișiere atinse

| fișier | ce |
|---|---|
| `src/KBot.Migrator/Transfer/UnitOwnership.vb` | **nou** — `UnitScope`, `Build`, `TryOwner`, `Resolve` |
| `src/KBot.Migrator/Transfer/TableMap.vb` | `UnitOwner*` + `HasUnitOwner` + `OwnedVia(...)` |
| `src/KBot.Migrator/Transfer/TableMaps.vb` | lanțurile pe SA / SB / `FX_Extrase` + `FromUnit("IdUnitate")` |
| `src/KBot.Migrator/Transfer/Verifier.vb` | poarta 7 pe rezoluția comună; garda Regula 1 în `CheckColumnPlans` |
| `src/KBot.Migrator/Transfer/TransferRunner.vb` | `BelongsToUnit` pe rezoluția comună; `UnitOwnership.Build` per unitate |
| `src/KBot.Migrator/Transfer/Finding.vb` | `UNITATE_NEDETERMINATA`, `CLASIFICATIE_NECORELATA` |
| `src/KBot.Migrator/KBot.Migrator.vbproj` | `FileVersion` 1.4.0.0 ▸ 1.5.0.0 |

## Rezultate

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

## Neverificat / amânat

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
