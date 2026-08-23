# SLICE-0045-03 — catalogul de corelații + hărțile de rezoluție + porțile de verificare

Data: 23.08.2026 · Slice **0045**, pasul 03.

## Ce s-a facut si de ce

A doua transa de cod. Tot alaturi de lantul HTTP al feliei 0044, care ramane neatins.
Aici intra tot ce trebuie DECIS inainte sa se scrie primul rand: ce coloana merge unde, ce
id se rezolva in ce, si ce anume opreste rularea.

Inca nu se scrie nimic — scrierea si dosarul SQL sunt pasul 04, formularul pasul 05.

### Deciziile noi ale operatorului, luate in cod

- **`ParteneriAng` ▸ `Parteneri_Coduri` — IN SCOP.** Ambele capete exista si se potrivesc;
  singura redenumire e `ContBanca` ▸ **`ContBancar`**. E singura tinta gasita care tine
  `IdClsf` SI `IdClsfAcc` alaturi, deci `IdClsf`-ul Access hraneste amandoua coloanele:
  rezolvat in `IdClsf`, brut in `IdClsfAcc`.
- **`Unitati` o populeaza UNEALTA.** D7 face asta oricum inevitabil: o baza creata dupa
  sablon are structura, nu randuri, deci `Unitati` porneste goala si patru chei straine
  arata spre ea.

### Modelul de corelare

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

### Catalogul (`TableMaps`)

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

### Hartile de rezolutie

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

### Portile (`Verifier`)

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

## Rezultate

`dotnet build src/KBot.Migrator/KBot.Migrator.vbproj`: **succes, 0 erori, 0 avertismente.**

Fara teste (§9 al planului).

## Fisiere atinse

Toate noi, in `src/KBot.Migrator/Transfer/`: `ColumnMapping.vb`, `TableMap.vb`,
`TableMaps.vb`, `ColumnPlan.vb`, `ResolutionMaps.vb`, `ClasificatieDerived.vb`,
`Finding.vb`, `TransferRequest.vb`, `Verifier.vb`.

## Neverificat / amanat

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

## Ce urmeaza

- **0045-04**: scrierea tranzactionala (o singura tranzactie, `FOREIGN_KEY_CHECKS` APRINSE,
  loturi de 500, comanda pregatita pe tabel) + dosarul de instructiuni SQL.
- **0045-05**: formularul si stergerea codului HTTP al feliei 0044.
