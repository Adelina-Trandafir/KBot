# SLICE-0047 — poarta părintelui citea Access-ul, nu rândul care pleca

Operatorul a raportat, după rularea `20260825_085520` pe `000_DEMO`: **datele ajung pe server, dar
sunt incomplete și greșite**. Cazul numit: angajamentul `AAB2EF2MCP4` are **22** de rânduri în
`FX_DDF_REV_SA` (și tot atâtea în `_SB`), iar jurnalul arată că au plecat **două**.

Jurnalul de final spunea, la virgulă, unde se duseseră celelalte:

```
Clasificatii_Buget: citite 101, scrise 12, părinte netransferat 89
Parteneri_Coduri:   citite  36, scrise  5, părinte netransferat 31
FX_DDF_REV_SA:      citite  32, scrise  2, document rămas în urmă 4, părinte netransferat 26, valori golite 28
FX_DDF_REV_SB:      citite  32, scrise  2, document rămas în urmă 4, părinte netransferat 26, valori golite 28
FX_ORD_TBL:         citite 461, scrise 461, valori golite 844
```

## Defectul — un singur rând de cod, patru tabele stricate

`TransferRunner.ParentsTravelled` verifica dacă părintele unui rând a plecat citind **coloana
Access** cu același nume ca a copilului:

```vb
Dim raw = reader.ValueOrMissing(link.AccessColumn)
...
If _written.Contains(link.ParentTable, raw) Then Continue For
```

Pe capătul celălalt, `WrittenKeys` pentru `Clasificatii` ține **id-urile atribuite de MariaDB**
(`RecordAssignedId` ▸ `_written.Add(map.TargetTable, assigned)`), adică `1..101` pe o bază goală.
Coloana Access `IdClsf` ține id-ul Access — `92, 123, 128, 138, 143, 144, 145, 146, 154, 158` pe
angajamentul din sesizare. Comparația întreba, așadar, **„id-ul Access 123 se află printre id-urile
atribuite?"** și răspundea din întâmplare: adevărat pentru orice valoare care nimerea în `1..101`,
fals pentru restul.

Se verifică pe cifre, exact:

| tabel | ce s-a întâmplat |
|---|---|
| `Clasificatii_Buget` | unitatea 75 are 7 clasificații Access cu id ≤ 101, unitatea 76 are 5. **7 + 5 = 12**, chiar numărul scris din 101 |
| `FX_DDF_REV_SA` / `_SB` | din cele zece clasificații ale angajamentului doar `92` e ≤ 101, iar `IdClsf` e `NOT NULL` acolo ▸ celelalte rânduri **cad**. Cele două rânduri rămase sunt reviziile 44 și 47 pe `IdClsf = 92` |
| `FX_ORD_TBL` | acolo `IdClsf` e nulabil ▸ rândul se scrie, dar **golit**: 383 din 461 de rânduri au ajuns pe server **fără clasificație**. 383 + 461 de `IdPartener` (`ForcedNull`, declarat) = **844**, chiar „valori golite" din jurnal |
| `Parteneri_Coduri` | `ParteneriAng.IdPartener` din Access ține `7605+`, id-uri ale serverului vechi, iar `Parteneri` are atribuite `1..291` ▸ 31 din 36 cad |

Rezoluția în sine **funcționase tot timpul**. Rândul care a trecut poartă
`IdClsfAcc = 92, IdClsf = 51`, iar 51 e într-adevăr al 51-lea `Clasificatii` scris (47 pentru
unitatea 75, apoi `79 ▸ 48`, `84 ▸ 49`, `90 ▸ 50`, `92 ▸ 51` pentru unitatea 76). `ClasificatiiMap`
și `ParteneriMap` au dat mereu răspunsul bun, iar `BuildValues` îl punea în `values`. **Numai
poarta se uita în altă parte** — și îl arunca.

Aceeași nepotrivire stătea ascunsă în spatele fiecărei redenumiri: `FX_ORD_TBL.IDORDP` e numele
ȚINTEI pentru Access `IDORD`, iar Access are **propria** coloană `IDORDP` cu id-urile serverului
vechi (117..123). Comentariul de pe `ParentLink.AccessColumn` susținea că „numele copilului e și
numele coloanei Access pentru orice legătură pe care filtrul o poate folosi, iar cititorul
răspunde «nu există coloana» pentru rest". Nu răspunde: **există**, și înseamnă altceva.

## Leacul

`ParentsTravelled` se uită acum la **valoarea care pleacă**, nu la fișierul Access:

```vb
Dim outgoing As Object = Nothing
If Not values.TryGetValue(link.ChildColumn, outgoing) Then Continue For
```

`values` E rândul așa cum va fi scris, iar `RecordPrimaryKey` umple `WrittenKeys` **din același
dicționar** — deci comparația e singura citire în care cele două capete înseamnă același lucru.
Parametrul `reader` a dispărut din semnătură: funcția nu mai are de unde să citească greșit.

O legătură a cărei coloană nu se scrie deloc nu trimite niciun id și deci **nu poate atârna în
gol** — se sare peste ea, nu se ghicește. Dacă acea coloană ar fi `NOT NULL` fără implicit, MariaDB
oprește tranzacția pe față, ceea ce e răspunsul corect.

`ParentLink.AccessColumn` s-a redenumit `ChildColumn` (era deja expusă și așa, ca proprietate
derivată). E un nume de ȚINTĂ și nimic altceva.

## Ce NU s-a atins

- **`FX_Istoric`, `FX_Indicatori`, `FX_Rezervari`, `FX_Plati`, `FX_Receptii` scriu `IdClsf` prin
  potrivire după nume, adică id-ul ACCESS — și așa trebuie.** Coloana e inversată față de familia
  DDF/ORD, iar rutele Flask o spun în clar: `routes/forexe/istoric.py` face
  `WHERE c.IdClsfAcc IN (SELECT IdClsf FROM FX_Istoric …)`, în timp ce `routes/forexe/ddf.py` și
  `ord.py` fac `WHERE c.IDClsf = sa.IdClsf`. Nu există cheie străină pe coloanele acelea, de-aia
  poarta nici nu s-a atins de ele. Constatarea `CLASIFICATIE_NECORELATA` a feliei 0045-07 le
  numește pe toate cinci; pentru acestea e **alarmă falsă**, și rămâne așa până când cineva măsoară
  fiecare tabel pe schema vie, nu pe numele coloanei.
- **`document rămas în urmă 4`** e decizia D5 (`IDDF 73/77/79/80`, secțiune A cu `IdUnitate` NULL),
  declarată în felia 0046. Angajamentul din sesizare e `IDDF 33` și nu e printre ele.
- **`FX_Rezervari: valori golite 3`** sunt trei `IDREV` care arată spre reviziile celor patru
  documente de mai sus — golire corectă pe coloană nulabilă (D14), nereparată fiindcă nu e stricată.

## Ce urmează

Nerulat pe server. `Clasificatii` e **insert-only** (D8: nu există cheie unică pe
`(IdClsfAcc, IdUnitate)`), deci reluarea cere din nou o bază **goală** — nu se poate suprascrie
peste rularea din 25.08.

La prima rulare reală, cifrele de verificat sunt fix cele de mai sus, întoarse:
`Clasificatii_Buget` **101/101**, `Parteneri_Coduri` **36/36**, `FX_DDF_REV_SA` și `_SB`
**28/32** (cele 4 rămân la D5), `FX_ORD_TBL` **461 scrise cu 461 „valori golite"**, nu 844 —
adică numai `IdPartener`, cel declarat.

`KBot.Migrator` **tot nu are proiect de teste.**
