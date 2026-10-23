# Proiect de date - etapa 1 (configurare cu sabloane) - AV01-02

PROPUNERE, doar document: nicio tabela creata, niciun DDL rulat, niciun cod. Bazat pe fisele din acest folder si pe
deciziile operatorului din `README.md`. Marcaj: VERIFICAT = citit intr-un fisier/schema; PROPUNERE = alegerea mea, de
confirmat; DE VERIFICAT = nu am citit inca.
Conventii (CLAUDE.md): MariaDB `utf8mb3_general_ci`, nume ASCII, DDL doar in `sql/` (il aplica operatorul), tabelele noi se
inregistreaza in `PYTHON/routes/schema_sync/schema_common.py`, stergere = `RESTRICT`, fara cascada pe date contabile.

## 1. Unde stau datele (VERIFICAT)

- Baza **unitatii** (DC, ex. `000_DEMO`): `FX_*`, `EF_*`, `VC_*`, `AD_*`, `Parteneri`, `Clasificatii*`, `Unitati` (un rand =
  un context SS+An). Aici merg toate tabelele contabile, cu prefix **`TZ_`** (decis). `An` numai pe datele care depind de an.
- `AVACONT_COMUN` (nomenclatoare comune): `DefaClsfE/F/V`, `DefaSursaSector`, `Roluri`, `Roluri_Operatii`, `Sectiuni`...
- **Lipsa:** `DefaCont` si `DefaClase` NU sunt in `AVACONT_COMUN.sql` (dump 16.10.2026). Operatorul spune ca nomenclatoarele se
  trag din COMUN: daca au fost adaugate dupa dump, refa dump-ul; altfel se creeaza (sectiunea 2). Date: `DefaClase` din
  `tabledata/DefaClase.csv` (8 randuri), `DefaCont` din `baza2026.accdb` (991 randuri; nu e in `tabledata`).

## 2. AVACONT_COMUN - tabele noi (PROPUNERE)

| Tabel | Coloane | Nota |
|---|---|---|
| `DefaClase` | `Clasa` char(1) PK, `Denumire` varchar(60) | 8 randuri |
| `DefaCont` | `Cont` varchar(7) PK, `Denumire`, `Tip` char(1) NULL, `ArePeSector`, `AreSursa`, `AreClsfF`, `AreClsfE` tinyint, `Vizibil` tinyint | din Access; cele 4 bife spun din ce se compune simbolul |
| `Roluri` (+ randuri) | `AV_STERGE_NC`, `AV_STERGE_CLSF` cu `Sectiune = 'AV'` (sectiunea `AV` exista deja) | drepturile din decizia «drept de rol»; operatia in `Roluri_Operatii` |

## 3. Baza unitatii - tabele noi (PROPUNERE, prefix `TZ_`)

**TZ_Subunitati** - `IdSubunitate` PK, `Denumire`, `Capitol` varchar(5) (`65.01`), `SursaSector` varchar(3) FK
`DefaSursaSector` (`01A`, `02E`...), `ZecimaleSalarii` tinyint (0/2: inlocuieste bifa `Zec2`), `Activ`. Unic pe `Capitol`.
Subunitate = alta unitate subordonata cu contabilitatea ei (decis); capitolul si SS vin din ea (65.01=01A, 65.02=02A,
65.10=02E, 65.11=01A). **O unitate fara subunitati are una implicita**, ca sa nu existe chei nule. DE VERIFICAT: relatia cu
`VC_Subunitati` (VERCON) - tabel separat, ramane neatins; legarea celor doua se decide mai tarziu.

**TZ_PlanCont** - `IdCont` PK, `IdSubunitate` FK, `Cont` FK `DefaCont`, `Analitic` varchar(10) NULL, `ClsfF`, `ClsfE` NULL,
`SimbolCont` varchar(50) (compus: cont + sector + sursa + [ClsfF] + [ClsfE] + [.analitic]; unic pe subunitate), `Denumire`,
`FelCont` char(1) (A/B/P), `Extra` tinyint, `Activ`. Fara `SID/SIC/RPC/RPD` (solduri separate), fara `ContLung`, fara `Expr1`/`TipCont`.
Fara `An`: planul nu depinde de exercitiu; un cont iesit din uz se marcheaza `Activ = 0`. Stergere BLOCATA daca e folosit in
linii de nota, sabloane sau solduri initiale (FK `RESTRICT` + mesaj cu locul de folosire).

**TZ_SoldInitial** - `IdSold` PK, `An` int, `IdCont` FK, `IdPartener` FK `Parteneri` NULL, `SoldDebitor`, `SoldCreditor`
decimal(18,2). Inlocuieste `PlanCont.SID/SIC` si `ParteneriSI`. Unic pe (An, IdCont, IdPartener). Migrare doar `An = 2026`.
`RPC/RPD` NU se aduc (zerouri in toate exemplele; PRESUPUS rulaje precedente) - de confirmat daca vreun raport le cere.

**TZ_Jurnale** - `IdJurnal` PK, `Cod` varchar(10) unic pe unitate, `Denumire`. Pe unitate (decis). Stergere BLOCATA daca are operatii
sau e in sabloane; schimbarea `Cod` blocata daca are operatii (propunere, lipseste decizia explicita).

**TZ_Sablon** (antet) - `IdSablon` PK, `Explicatie`, `Fel` char(1) (E/P/A), `IdJurnal` FK, `Compartiment` varchar(50) NULL,
`FelOP` varchar(50) NULL, `Cheie` varchar(30) NULL (cheia clasificatiei `65.01.XX.XX.57.02.02`, doar cand sablonul e legat de o
clasificatie), `Tip` char(1) (A/C/M, **pastrat ca in Access pana se afla sensul lui M**), `Activ`.
**TZ_SablonLinie** - `IdLinie` PK, `IdSablon` FK, `Forma` varchar(5) (subcapitol), `Explicatie`, `ContDebit`, `ContCredit`
(varchar(7), FK `DefaCont`: DOAR contul de baza, decis), `PartDebitMod`/`PartCreditMod` enum(`NIMIC`,`OPERATIE`,`FIX`),
`IdPartDebit`/`IdPartCredit` FK `Parteneri` NULL (obligatoriu cand mod = `FIX`), `Ordonantare`, `PA`, `Chelt` tinyint, `Simpla`
smallint NULL. `Chelt` = «cheltuiala efectiva» (decis, ramane). Contul complet se compune la folosire din cont de baza +
sector/sursa ale subunitatii + clasificatia operatiei (regula `MakeCont`, fisa OCI A1, dupa bifele din `DefaCont`).
Fara `IdClsf` concret pe linie (se rezolva la folosire), fara `XXX`.

**TZ_SablonSalarii** (articol) - `IdArticol` PK, `IdSubunitate` FK (capitol din subunitate), `Clasificatie` varchar(12)
(`10.01.01`), `Explicatie`, `ContDebit`, `ContCredit`, `PartDebitMod/IdPartDebit`, `PartCreditMod/IdPartCredit`, `Activ`.
**TZ_SablonSalariiLinie** - `IdLinie` PK, `IdArticol` FK, `Tip` enum(`RETINERE`,`VIRAMENT`,`NUMERAR`), `IdParinte` FK (la linia
RETINERE, pentru VIRAMENT), `Explicatie`, conturi de baza + parteneri ca mai sus, `EsteCM` tinyint (concediu medical, cont 423), `Activ`.
**Regula (decisa):** un articol NU se salveaza fara cel putin o linie `VIRAMENT`; validare in serviciul de pe server, cu mesaj clar
(nu se sterge nimic la inchidere). Randurile `NUMERAR` raman in model; generarea lor automata (lantul ridicare numerar -> casa
-> plata) se decide la etapa 3. «Minim un VIRAMENT» numara si liniile `Activ = 0`? - DE CONFIRMAT.

## 4. Clasificatii Venituri (VERIFICAT pe `AVACONT_SURSA.sql` si `extrase.py`)

`Clasificatii_Venituri` (nomenclator comun, fara `IdUnitate`) ramane NEATINS: `extrase.py` citeste doar `Capitol, SubCapitol,
Paragraf, IdClsfV`, fara filtru pe unitate, deci orice schimbare de coloane o strica. Coloanele `Trim1..4 int` raman, nefolosite.
Se aduc la modelul clasificatiilor normale doar partile care lipsesc:
- **`Clasificatii_Venituri_Buget`** (nou, ca `Clasificatii_Buget`): `IdBuget` PK, `IdClsfV` FK, `IdUnitate` FK, `An`, `DataInceput`
  (o linie = o versiune), `Trim1..4` double NULL, `TOTAL` coloana generata; unic pe (`IdClsfV`, `IdUnitate`, `An`, `DataInceput`).
- **`Clasificatii_Venituri_Rectificari`** (exista, `Trim1..4` sunt `varchar(255)`, fara `IdUnitate`): se aduce la modelul
  `Clasificatii_Rectificari`: `Trim` double, `IdUnitate` adaugat, unic pe (`IdClsfV`, `IdUnitate`, `Data`, `Document`). DE VERIFICAT
  inainte de ALTER: tabela e goala? (e in schema din 09.09.2026, probabil nefolosita.)
- Fereastra: copie a `ClasificatiiForm` (arbore + buget pe versiuni + rectificari), cu modificare si stergere ca la cheltuieli.
- Nomenclatorul `DefaClsfV` (1404 randuri, `tabledata/DefaClsfV.csv`; coloana `CV` = `VN`: sens DE LAMURIT) exista deja in COMUN.

## 5. Setari si drepturi

- Setari Avacont: **per utilizator**, in magazinul local de setari al aplicatiei (`app_settings.json`, pagina «Avacont» sub «Aplicatie»,
  langa «Documente» si «KBOT»), fara DB. Dintre cele 4 bife ramase din ribbon, in setari ramane doar «Afiseaza cod
  partener» (`TextPart`). «Stergere N.C.» si «Stergere clasificatii» NU sunt setari: sunt drepturile `AV_STERGE_NC` / `AV_STERGE_CLSF`.
  «Protectie operatii» nu e setare (vezi mai jos). Fisa Setari mai propune 3 setari (`NDes`, `MNC_C6`, `MNC_T0`) - de confirmat inainte sa intre.
- «Protectie operatii» = regula a ecranului Note (etapa 2), nu setare.

## 6. Migrare din `baza2026.accdb` (Migrator) - PROPUNERE

`DefaClase` (CSV), `DefaCont` -> COMUN; `PlanCont` -> `TZ_PlanCont` (subunitatea implicita); `PlanCont.SID/SIC` + `ParteneriSI` ->
`TZ_SoldInitial` (An 2026); `Jurnale` -> `TZ_Jurnale`; `DefaNoteA/DefaNote` -> `TZ_Sablon*` (contul complet din Access se TAIE la contul
de baza; liniile cu `XXX` devin mod `OPERATIE`); `DefaSalarii*` -> `TZ_SablonSalarii*`; `Parteneri` fara cod fiscal -> `Parteneri` (analitice).
Extinderea `src\KBot.Migrator\Transfer\TableMaps.vb`. Numaratori de control (randuri sursa vs tinta) la fiecare tabel.
Atentie: 54 din 92 de linii de sablon au `XXX`, deci mod `OPERATIE` va fi cel mai frecvent.

## 7. Ordinea codului etapei 1 (dupa confirmare)

1. Subunitati + `DefaClase`/`DefaCont` + `TZ_PlanCont` (fereastra Conturi). 2. `TZ_Jurnale`. 3. Clasificatii Venituri (buget + rectificari).
4. `TZ_Sablon*` (OCI). 5. `TZ_SablonSalarii*` (OCS). 6. Pagina Setari «Avacont». Fiecare: DDL in `sql/`, ruta Flask, `KBot.Api` + `KBot.Domain`,
view in `KBot.App\Views\`, buton in ribbon (tab Configurari), ajutor `0000-NN`, migrare.

## 8. Ce NU stiu inca (DE VERIFICAT / DE CONFIRMAT)

- `Tip = M` la antetul de sablon; `DefaNote.Tip` (1/2/4) si `Simpla`; coloana `CV` din `DefaClsfV`.
- `ParteneriForm`/0121-0123: impun cod fiscal? (conditioneaza analiticele fara CF).
- Planul pe subunitate (propus) vs pe unitate: un cont poate apartine mai multor subunitati?
- Relatia `TZ_Subunitati` <-> `VC_Subunitati`.
- Daca `DefaCont`/`DefaClase` exista deja in COMUN dupa 16.10.2026.
