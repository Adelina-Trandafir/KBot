# SLICE-0011-03 — fix join `Clasificatii` în Sumar (fan-out + cheie greșită)

**Data:** 2026-07-18
**Felia:** 0011 (Sumar) — pasul 3. Pașii 1–2: `SLICE-0011-01`, `SLICE-0011-02`.
**Sursa:** rulare pe date reale (`000_DEMO`) de către operator.

---

## Problema

Query-ul Sumar **multiplica rândurile-indicator**. Cifre măsurate pe `000_DEMO`:

```
SELECT COUNT(*) FROM FX_Indicatori;                               -->  29
SELECT COUNT(*) FROM FX_Indicatori WHERE IdClsf <> 0;             -->  25
JOIN Clasificatii ON I.IdClsf = C.IDClsf                          -->   0   (cheie greșită)
JOIN Clasificatii ON I.IdClsf = C.IdClsfAcc                       -->  67   (fan-out multi-unitate)
JOIN ... ON I.IdClsf = C.IdClsfAcc AND I.IdUnitate = C.IdUnitate  -->  50   (fan-out duplicate)
```

Trei defecte suprapuse, toate în cod scris în 0011-01:

1. **Cheie greșită.** `FX_Indicatori.IdClsf` ține **id-ul Access**, deci se potrivește
   cu `Clasificatii.IdClsfAcc`, nu cu `Clasificatii.IDClsf` (PK MariaDB). Zero
   potriviri ⇒ `Clsf` gol pe fiecare rând în producție.
2. **`IdUnitate` scos greșit.** `Clasificatii` din baza per-unitate conține **8
   unități** (48, 75, 76, 121, 123, 135, 136, 157). Regula „drop `IdUnitate`" se
   aplică tabelelor `FX_`, **nu nomenclatoarelor**.
3. **Duplicate în nomenclator.** Chiar cu ambele predicate corecte rămâne fan-out ×2:
   `(75,79)`, `(75,84)`, `(75,90)`, `(75,92)`, `(75,93)` — fiecare de două ori.

## Fixul

`LEFT JOIN Clasificatii` **eliminat**. `Clsf` e doar text afișat, deci se ia prin
subinterogare scalară:

```sql
(SELECT C.Clsf FROM Clasificatii C
  WHERE C.IdClsfAcc = I.IdClsf
    AND C.IdUnitate = I.IdUnitate
  LIMIT 1) AS Clsf
```

Un rând per indicator **garantat**, indiferent de duplicate; „fără clasificație → gol"
se păstrează. `ORDER BY` mutat pe aliasul de ieșire (`ORDER BY Clsf, I.CodIndicator`),
fiindcă aliasul `C` nu mai există.

### Extindere față de brief: același defect era și la `Parteneri`

Brief-ul cerea doar `Clasificatii`, dar secțiunea „de consemnat" generalizează regula
la nomenclatoarele partajate, iar `Parteneri` avea **exact aceeași structură de
defect** — `LEFT JOIN Parteneri P ON SA.CodPartener = P.CodPartener`, fără
`IdUnitate`. `FX_DDF_REV_SA` **are** `IdUnitate` (`FX_System_Export/TABLES/
FX_DDF_REV_SA.md:26`).

Acolo era **mai grav decât un defect de afișare**: join-ul stătea **înaintea lui
`GROUP BY`**, deci un partener duplicat multiplica `SUM(SA.ValCur)` și **umfla
`TotalRevizii`** — o cifră de bani greșită, nu o coloană goală. Mutat și el în
subinterogare scalară:

```sql
MIN((SELECT P.DenumirePartener FROM Parteneri P
      WHERE P.CodPartener = SA.CodPartener
        AND P.IdUnitate   = SA.IdUnitate
      LIMIT 1)) AS Partener
```

`SUM` rămâne peste rândurile `SA` reale; `MIN` peste subinterogare păstrează semantica
originală. **Nu a fost cerut explicit** — motivul extinderii e că e același defect,
descoperit prin regula pe care tocmai o primisem, iar a-l lăsa ar fi însemnat livrarea
conștientă a unei sume greșite.

## Fișiere atinse

| Fișier | Ce |
|---|---|
| `PYTHON/routes/forexe/sumar.py` | join `Clasificatii` → subinterogare scalară; join `Parteneri` → subinterogare scalară; `ORDER BY` pe alias; antetul fișierului corectat |
| `PYTHON/tests/test_forexe_sumar.py` | fixtură „murdară" intenționat + 3 teste noi |

## Teste

Fixtura **reproduce defectul**, nu îl presupune:

- aceeași `(IdClsfAcc, IdUnitate)` inserată de **două ori** → duplicat real;
- același `IdClsfAcc` sub **altă** `IdUnitate` → vecin de altă unitate;
- `IdUnitate` real citit din `Unitati` (FK `Clasificatii_ibfk_6`), nu `0`.

⚠️ **Schimbare importantă de fixtură:** `FX_Indicatori.IdClsf` se umple acum cu
`CLSF_ACC` (id-ul Access), **nu** cu `cur.lastrowid` (PK-ul MariaDB). Numele coloanei
minte — exact defectul 1.

Trei teste noi:

| Test | Ce prinde |
|---|---|
| `test_duplicate_classification_does_not_fan_out` | IND-A apare **o singură dată** deși nomenclatorul are duplicatul, cu `Clsf` populat |
| `test_other_unit_classification_does_not_add_a_row` | exact 2 rânduri; clasificația altei unități nu adaugă niciunul |
| `test_row_count_equals_indicator_count` | plasă generală: `len(rows)` = numărul de indicatori — prinde **orice** join viitor care multiplică |

Ultimul e deliberat mai larg decât brief-ul: defectul a apărut de trei ori pe felia
asta, deci merită o santinelă care nu depinde de cunoașterea cauzei.

**Rezultat local:** `75 passed, 9 skipped, 0 fail/error`. Cele 18 teste din
`test_forexe_sumar.py` (15 vechi + 3 noi) sunt host-only și **s-au sărit aici** —
`config.py` lipsește pe stație. **Verificarea care contează e tot pe gazdă.**

## De consemnat (intrat în `KBOT_STATUS.md`)

**Regula «drop `IdUnitate`» se limitează la tabelele `FX_`.** Nomenclatoarele
partajate (`Clasificatii`, `Parteneri` și altele de acest fel) **păstrează** predicatul
`IdUnitate` — baza per-unitate le conține pentru mai multe unități.

**Convenții de denumire, ambele confirmate pe schema vie:**

- `Clasificatii`: `IDClsf` = PK MariaDB, `IdClsfAcc` = id Access păstrat.
  **DAR `FX_Indicatori.IdClsf` ține id-ul Access — nu urmează convenția.**
  Numele coloanei nu e o dovadă.
- Familia `FX_ORD`: sufix „P" = PK MariaDB (`IDORDTBLP`), fără „P" = id Access
  (`IDORDTBL`).

**Concluzie operațională, mai utilă decât ambele tabele:** nu deduce cheia din numele
coloanei — **numără rândurile înainte și după join**. Un join care întoarce `0` sau
mai multe rânduri decât tabelul din stânga e un defect, nu o particularitate a
datelor. Cele trei rulări pierdute pe felia asta ar fi fost prinse de un singur
`COUNT(*)`.

## Rămâne NEVERIFICAT / fire deschise

1. **Nu am rulat pe date reale** — testele host se sar pe stație. Cifrele din acest
   worklog sunt măsurate de operator, nu de mine.
2. **Duplicatele din `Clasificatii`** (fir deschis, nu blochează felia): de verificat
   dacă rândurile diferă prin `Sursa`/`Sector` — caz în care **lipsește o dimensiune
   din join** și `LIMIT 1` alege arbitrar între două clasificații diferite — sau sunt
   identice, caz în care sunt date de curățat. `LIMIT 1` e corect pentru al doilea caz
   și doar *acceptabil* pentru primul.
3. `MIN(...)` peste subinterogarea `Parteneri` păstrează semantica Access, dar dacă un
   `CodPartener` are mai mulți parteneri pe aceeași unitate, `LIMIT 1` alege arbitrar.
   Nu s-a observat așa ceva; nu a fost verificat.
