# MAPARE NOMENCLATOARE — Access ▸ MariaDB

> Companion to `MAPARE_ACCESS_MARIADB.md` (which covers the `FX_*` families).
> This file covers the reference tables: `Clasificatii`, `Clasificatii_Buget`,
> `Clasificatii_Rectificari`, `Parteneri`. Same conventions: `=` identical, `▸` renamed,
> `✗` does not travel. Every claim is traceable to a dump listed under Evidence.
>
> **This is the deliverable of `PLAN_MigratorDirect.md` §12 (discovery pass).** It is written
> from a real read of the three `.accdb` files. Where the plan's assumption turned out to be
> wrong, the plan is named and the correction is marked ⚠.

Slice: **0045** (next free in `KBOT_STATUS.md`).
Written: 23.08.2026.

---

## Evidence

Produced by `tools/AccdbSchemaDump/` (throwaway console spike, not in `KBot.sln`), run on
this machine, 23.08.2026:

| Dump | Source file | Size | Tables |
|---|---|---:|---:|
| `artifacts/accdb-schema/cale.md` | `Avacont/cale.accdb` | 679.936 B | 11 |
| `artifacts/accdb-schema/baza2026.md` | `Avacont/Energetic ISJ/baza2026.accdb` | 3.784.704 B | 69 |
| `artifacts/accdb-schema/FX_2026.md` | `Avacont/Forexe/FX_2026.accdb` | 2.678.784 B | 27 |

⚠ **The dumps are NOT in git** — `artifacts/` is gitignored (`.gitignore:66`), and they carry
real partner names, IBANs and figures. Neither is `Avacont/` committed, for the same reason.
Regenerate both in one command from a checkout that has the files:

```bash
dotnet run --project tools/AccdbSchemaDump -c Release -- artifacts/accdb-schema "Avacont/cale.accdb" - "Avacont/Energetic ISJ/baza2026.accdb" - "Avacont/Forexe/FX_2026.accdb" -
```

(Arguments after the output directory are read in pairs: path, then password — `-` for none.)

The MariaDB side is **not** read from `information_schema` — no server was contacted in this
pass. It is read from the Flask routes that write these tables today:
`PYTHON/routes/clasificatii.py`, `PYTHON/routes/parteneri.py`,
`PYTHON/routes/nomenclatoare.py`. That is weaker evidence than a live `SHOW CREATE TABLE`,
and every claim sourced that way is marked **(from route source)**. See §8.

---

## 0. Findings that change the plan

Five, in order of how much they move the work.

### F1 ⚠ `cale.accdb` holds none of the four nomenclators

`PLAN_MigratorDirect.md` §12.2 Q1 assumed `cale.accdb` carries `CAI`, `Parteneri`,
`Clasificatii` and `Rectificari`. It carries **only the registry**. Its full table list is:

```
BurseColoane  BurseFoi  cai  K  LEGUNIT  TC  tmpBurse  tmpBurseBanci  TRACE  TRACE_S  Ver
```

There is no `Parteneri`, no `Clasificatii`, no `Rectificari` in it. `cai` is spelled
lowercase (Rule 4 applies).

### F2 ⚠ The nomenclators live in a **per-unit** file, one file per `IdUnitate`

`Clasificatii` (54 rows), `Parteneri` (75), `Rectificari` (0) are in
`Avacont/Energetic ISJ/baza2026.accdb` — the third file. `cai` points at it:
row `IdUnitate = 76` has `CaleUnitate = .\ENERGETIC ISJ` and
`FullPath = .\ENERGETIC ISJ\baza2026.accdb`.

So the transfer is not "one registry file, filter by unit". It is **one `baza<year>.accdb`
per `IdUnitate`**, opened in turn, driven by `cai`. For DC `000_DEMO` that is **13 files**.

### F3 ⚠ The nomenclator rows carry **no `IdUnitate` column**

This is the answer to §12.2 Q5, and it is the opposite of what that question expected
("Confirm only that Access `Clasificatii` carries `IdUnitate`, since the resolution map …
is unbuildable without it").

`Clasificatii`, `Parteneri` and `Rectificari` in `baza2026.accdb` have **no `IdUnitate`
column at all**. The unit is implied by the file.

The `(IdClsfAcc, IdUnitate) ▸ IDClsf` map of `MAPARE_ACCESS_MARIADB.md` Rule 1 is still
buildable and still correct — but `IdUnitate` is **supplied by the loop** (the `cai` row whose
file is being read), not read off the row. That distinction has to be explicit in the code, or
someone will look for the column and not find it.

The `FX_*` side is unaffected: `FX_DDF_REV_SA`, `_SB` and `FX_ORD_TBL` **do** carry
`IdUnitate` themselves, so their half of the lookup is a real column read.

### F4 The link is proven end to end on real rows

Not inferred — read:

| Step | Value | Where |
|---|---|---|
| `FX_DDF_REV_SA.IdUnitate` | `76` | `FX_2026.md` sample |
| `cai.IdUnitate = 76` ▸ `CaleUnitate` | `.\ENERGETIC ISJ` | `cale.md` sample |
| that folder's `baza2026.accdb` ▸ `Clasificatii.IDClsf` | `92, 123, 128, 138, 141, 143, 158 …` | `baza2026.md` sample |
| `FX_DDF_REV_SA.IdClsf` on those same rows | `92, 123, 128, 138, 143, 145, 146, 154, 158` | `FX_2026.md` sample |

Every `IdClsf` in the `FX_DDF_REV_SA` sample resolves to an `IDClsf` present in that unit's
`Clasificatii`. The resolution path in the plan is the right one.

### F5 `IdClsfPY` is demonstrably stale — Rule 1 confirmed on data

`MAPARE_ACCESS_MARIADB.md` says "Do not use `IdClsfPY`. Anywhere." The dumps show why, on
one classification:

| Row | `IdClsf` | `IdClsfPY` |
|---|---:|---:|
| `baza2026.Clasificatii` | 123 | **1363** |
| `FX_2026.FX_DDF_REV_SA` (three rows) | 123 | **1309** |
| `FX_2026.FX_ORD_TBL` (five rows) | 123 | **1309** |

Same classification, two different "old server" ids. Matching on `IdClsfPY` would have
mismatched or dropped these rows silently. Rule 1 stands, with evidence behind it now.

---

## 1. `cai` — the registry

`cale.accdb` ▸ table `cai`, 13 rows. No primary key; three non-unique indexes, all on
`IdUnitate`. Read-only for this tool.

| # | Column | Type | Used by the migrator |
|---:|---|---|---|
| 1 | IdUnitate | Integer | **yes** — the unit key, and the value written into `IdUnitate` on the target |
| 2 | AnDate | WChar(50) | the year, as text (`"2026"`) |
| 3 | ANNOU | WChar(4) | the year, as text (`"2026"`) — duplicate of the above in every sample row |
| 4 | DC | WChar(255) | **yes** — the target database name, see §2 |
| 5 | BLOCAT | Boolean NOT NULL | `False` on all 13 rows |
| 6 | USR | WChar(50) | NULL on all 13 |
| 7 | Cdeschid | WChar(255) | `X` or NULL |
| 8 | FullPath | WChar(255) | `.\ENERGETIC ISJ\baza2026.accdb` — relative, from the `cale.accdb` folder |
| 9 | SURSA | WChar(255) | `01A` / `02A` / `02E` — matches the `SS` values on the `FX_*` rows |
| 10 | AlteDetalii | WChar(255) | `LOCAL` / `ISJ` / `VEN` / `VENITURI` / `REPUBLICAN` |
| 11 | CaleUnitate | WChar(255) | **yes** — the unit folder, `.\ENERGETIC ISJ` |
| 12 | NumeUnitate | WChar(255) | display only |
| 13 | CaleEFACTURA | WChar(255) | out of scope |
| 14 | UnitatiSclav | WChar(255) | NULL on all 13 |
| 15 | IdUnitateParinte | Integer | NULL on all 13 |
| 16 | CaleForexe | WChar(255) | **yes** — `C:\AVACONT\Forexe\FX…`; **NULL on 2 of the 13 rows** (IdUnitate 110 and 114) |

Two things to note before coding against it.

* `FullPath` and `CaleUnitate` are **relative** (`.\…`). They resolve against the folder that
  holds `cale.accdb`, and they are inconsistently terminated — `.\SC29 LOCAL\` has a trailing
  separator, `.\ENERGETIC ISJ` does not. Normalise.
* `CaleForexe` is NULL for IdUnitate 110 and 114. Those two units have a `baza2026.accdb` but
  no FX file, so a run over "every unit in the DC" must tolerate a unit with nomenclators and
  no `FX_*` data. **Not a decision I can make** — see §9 Q4.

### §11 Q1 — is `CAI.DC` the database name verbatim?

**Yes, on the evidence available — but the evidence is single-valued.** All 13 rows carry
`DC = "000_DEMO"`, which is exactly the MariaDB database name used throughout the repo
(`MAPARE_ACCESS_MARIADB.md`, `KBOT_STATUS.md`, the Flask routes). There is no second DC in
this registry to show whether any transformation is applied. Treat as confirmed for
`000_DEMO`; re-check on the first registry that lists two DCs.

`FX_DDF.DC` and `FX_DDF_REV.DC` also read `000_DEMO`, which is a second, independent
occurrence of the same string.

---

## 2. `Clasificatii`

Access: `baza2026.accdb` ▸ `Clasificatii`, 54 rows, PK `IDClsf` (unique index `PrimaryKey`).
Four extra non-unique indexes: `C`(Capitol), `S`(Subcapitol), `R`(Articol), `L`(Alineat).

MariaDB target column list **(from route source** — `clasificatii.py:180`**)**:
`IdClsfAcc, IdUnitate, Capitol, Subcapitol, Articol, Alineat, Denumire`, with `IDClsf`
`AUTO_INCREMENT` assigned by the server (`cursor.lastrowid` is read back and returned as the
mapping).

| Access | MariaDB | |
|---|---|---|
| IDClsf | **IdClsfAcc** | ▸ Rule 1. Access PK, `Integer NOT NULL DEFAULT 0`. Key of the resolution map |
| — | IDClsf | `AUTO_INCREMENT`, assigned. **The value the `FX_*` rows must be rewritten to** |
| *(the file's unit)* | **IdUnitate** | ▸ F3 — **no source column**; comes from the `cai` row being read |
| Capitol | Capitol | = WChar(50) |
| Subcapitol | Subcapitol | = WChar(50) |
| Articol | Articol | = WChar(50) |
| Alineat | Alineat | = WChar(50) |
| Denumire | Denumire | = WChar(255) |
| Trim1, Trim2, Trim3, Trim4 | ▸ `Clasificatii_Buget` | Double. See §3 |
| TOTAL | ✗ | Double. `Trim1+…+Trim4` in the sample rows; no target column |
| TOTALFX | ✗ | Double, NULL on most rows; no target |
| IdClsfPY | ✗ | **Rule 1 — never read.** F5 shows it disagreeing with the `FX_*` side |
| Clsf | ✗ | WChar(0)/Memo, `65.01.04.02.10.01.01` — the four parts concatenated |
| Sector | ✗ | Memo, `01` |
| ClsfSal, ClsfF, ClsfE, ClsfX | ✗ | Memo, derived spellings of the same code |
| CodSSI | ✗ | Memo, `01A650402100101` = SS + the code. No target on `Clasificatii` |
| Titlu | ✗ | Memo, `10` / `20` / `59` — the title, i.e. the leading pair of `Articol` |
| Sursa | ✗ | Memo, `A` |
| SS | ✗ | Memo, `01A` |
| CodAng | ✗ | WChar(10), NULL throughout the sample |
| CodInd | ✗ | WChar(3), NULL throughout the sample |
| DTQ | ✗ | Date, `Now()` default — Access bookkeeping |
| Document, Data | ✗ | NULL throughout the sample; the rectification's document/date live on `Rectificari` |
| IdLegatura | ✗ | Integer, NULL throughout |
| Esinc | ✗ | Boolean, `True` throughout — the Access sync flag |

**Not answerable from the dumps:** whether the MariaDB `Clasificatii` has more columns than
those seven (`Clsf`, `Sector`, `Titlu`, `SS`, `CodSSI` all have plausible homes on the target
and several are read by `PYTHON/routes/`). The route's INSERT is the only evidence, and an
INSERT names what it writes, not what exists. **See §8.**

---

## 3. `Clasificatii_Buget` — §12.2 Q2 answered

**Four columns of one row, not four rows.** Settled, not a reading.

`clasificatii.py:181`:

```sql
INSERT INTO Clasificatii_Buget (IdClsf, IdUnitate, An, Trim1, Trim2, Trim3, Trim4)
VALUES (%s, %s, %s, %s, %s, %s, %s)
```

`Trim1..Trim4` are four literal columns on the target, and the row is written once per
classification per year — which is exactly what the unique key
`uq_clasificatii_buget_idclsf_an` on `(IdClsf, An)` enforces. The two readings the plan asked
me to weigh are not both open; the column list settles it.

| Access `Clasificatii` | MariaDB `Clasificatii_Buget` | |
|---|---|---|
| *(resolved id)* | IdClsf | the **assigned** `IDClsf` from §2, not the Access one |
| *(the file's unit)* | IdUnitate | ▸ F3 — from the `cai` row |
| *(the year)* | An | ⚠ **no source column on `Clasificatii`.** See §9 Q3 |
| Trim1 | Trim1 | = Double |
| Trim2 | Trim2 | = Double |
| Trim3 | Trim3 | = Double |
| Trim4 | Trim4 | = Double |
| TOTAL | ✗ | not in the INSERT; recomputable as the sum |

⚠ `An` has **no source column**. Access `Clasificatii` carries no year. The candidates are
`cai.AnDate` / `cai.ANNOU` (both `"2026"`, text) or the FX file name. This is §11 Q3 and it is
now blocking on this table, not just cosmetic. See §9.

---

## 4. `Clasificatii_Rectificari` — §12.2 Q3 answered

Access: `baza2026.accdb` ▸ `Rectificari`, **0 rows in this file**, PK `ID`, non-unique index
on `IdClsf` (plus a relationship-named index
`Clasificatii__IDClsf___Rectificari__IdClsf`, which is Access's own record of the
`Clasificatii.IDClsf ▸ Rectificari.IdClsf` link).

MariaDB target column list **(from route source** — `clasificatii.py:278` and `:331`**)**:
`IdClsf, Capitol, Subcapitol, Articol, Alineat, Data, Document, Trim1, Trim2, Trim3, Trim4`.
The upsert at `:356` updates `Capitol, Subcapitol, Articol, Alineat, Trim1..Trim4` — i.e.
`IdClsf`, `Data` and `Document` are the match, matching the stated unique key
`UK_Rectificari_IdClsf_Data_Document`.

| Access `Rectificari` | MariaDB `Clasificatii_Rectificari` | |
|---|---|---|
| IdClsf | IdClsf | ▸ **Rule 1** — the Access value is a `Clasificatii.IDClsf`, so it resolves through the map to the assigned id |
| Capitol | Capitol | ▸ WChar(255) Access ▸ target width unread |
| Subcapitol | Subcapitol | ▸ idem |
| Articol | Articol | ▸ idem |
| Alineat | Alineat | ▸ idem |
| Data | Data | = Date · part of the unique key |
| Document | Document | = WChar(255) · part of the unique key |
| Trim1..Trim4 | Trim1..Trim4 | = Double |
| ID | ✗ | Access PK. No target column in the INSERT — the target's key is the natural triple |
| DTQ | ✗ | Access bookkeeping |
| Esinc | ✗ | Access sync flag |

**Access columns with no target:** `ID`, `DTQ`, `Esinc` — all three are local bookkeeping, none
carries data.
**Target columns with no source:** none among the eleven the route writes. Whether the target
has `NOT NULL` columns *outside* that list is unknown — §8.

⚠ **`Rectificari` is empty in the one file I have.** The mapping above is schema-only. No row
has ever been through it, on either side.

⚠ `RectificariV` (0 rows) is the **venituri** sibling, keyed on `IdClsfV` ▸ `ClasificatiiV`
(also 0 rows). Neither has a target named anywhere in the plan or the routes. Out of scope
unless you say otherwise — but they exist, and on a `02E`/`VENITURI` unit they will not be
empty.

---

## 5. `Parteneri`

Access: `baza2026.accdb` ▸ `Parteneri`, 75 rows, PK **`CodPartener`** (WChar(50) NOT NULL),
non-unique index on `DenumirePartener`.

MariaDB target column list **(from route source** — `parteneri.py:145`, identical at
`nomenclatoare.py:163`**)**:
`IdUnitate, CodPartener, DenumirePartener, CodFiscal, ContIBAN, Banca, Adresa, Tip`, with
`IdPartener` the assigned primary key.

| Access | MariaDB | |
|---|---|---|
| *(the file's unit)* | IdUnitate | ▸ F3 — from the `cai` row |
| CodPartener | CodPartener | = WChar(50), Access PK. Unique per `IdUnitate` on the target |
| DenumirePartener | DenumirePartener | = WChar(255) |
| CodFiscal | CodFiscal | = WChar(255). Empty string (not NULL) on most sample rows |
| ContIBAN | ContIBAN | = WChar(255) |
| Banca | Banca | = WChar(255) |
| Adresa | Adresa | = WChar(255) |
| Tip | Tip | = WChar(255) in Access; values are `"1"` / `"2"` — **text holding a number** |
| — | IdPartener | `AUTO_INCREMENT`, assigned by the server |
| IdPartener (Access) | ✗ | ⚠ Integer, values **7605…7621** — the *old server's* ids, same trap as `IdClsfPY`. **Do not send** |
| IDPART | ✗ | Integer, `1,2,3…` — a local sequence, distinct from both of the above |
| NumePartener | ✗ | WChar(255) — the uppercased `DenumirePartener`. No target in the INSERT |
| ContPl, F8, F9 | ✗ | WChar(255), NULL/empty throughout |
| CodClient | ✗ | WChar(255), NULL/empty throughout |
| Ascuns | ✗ | Boolean, `False` throughout |
| DTQ, Esinc | ✗ | Access bookkeeping / sync flag |

⚠ Three id-shaped columns on one Access table — `IdPartener` (7605+), `IDPART` (1,2,3…) and
the real key `CodPartener` (text `"001"`). Only `CodPartener` travels. `IdPartener` is the
`IdClsfPY` mistake wearing a different name.

Per `MAPARE_ACCESS_MARIADB.md` §5.2 this does **not** revive the `IdPartener` foreign keys on
`FX_DDF_REV_SA` / `_SB` / `FX_ORD_TBL` — those still travel NULL unless you decide otherwise.

### 5.1 `ParteneriAng` ▸ `Parteneri_Coduri` — a fifth nomenclator nobody named

Not in the plan, not in `MAPARE_ACCESS_MARIADB.md`, but both ends exist and line up
column for column.

Access `ParteneriAng`, 2 rows. MariaDB `Parteneri_Coduri` **(from route source** — `parteneri.py:87`**):
`IdPartener, CodPartener, IdClsf, IdClsfAcc, CodAng, CodInd, ContBancar`.

| Access `ParteneriAng` | MariaDB `Parteneri_Coduri` | |
|---|---|---|
| IdClsf | IdClsf **and** IdClsfAcc | ▸ Rule 1 — the Access value goes to `IdClsfAcc`, the resolved id to `IdClsf`. The target carries **both** |
| CodPartener | CodPartener | = |
| CodAng | CodAng | = WChar(11) |
| CodInd | CodInd | = WChar(4) |
| ContBanca | **ContBancar** | ▸ note the rename — `ContBanca` ▸ `ContBancar` |
| — | IdPartener | the assigned `Parteneri.IdPartener` |
| Id, Clsf, DTQ | ✗ | local key / derived code / bookkeeping |

**Decision needed** — in scope or not (§9 Q5). It is the only table found where the target
keeps `IdClsf` *and* `IdClsfAcc` side by side.

`ParteneriSI` (52 rows: `SimbolCont, CodPartener, SID, SIC, RPC, RPD, DTQ, Esinc`) is opening
balances per account. **No target found** in any route. Reported, not mapped.

---

## 6. §12.2 Q6 — row counts

Per file, as read. These are the scale for batching decisions.

**`cale.accdb`** — `cai` **13**, `LEGUNIT` 7, `K` 1, `TC` 1, `Ver` 1. Everything else empty
(`BurseColoane`, `BurseFoi`, `tmpBurse`, `tmpBurseBanci`, `TRACE`, `TRACE_S`).

**`baza2026.accdb`** (69 tables) — the ones this plan touches:

| Table | Rows |
|---|---:|
| Clasificatii | **54** |
| Parteneri | **75** |
| Rectificari | **0** |
| ParteneriAng | 2 |
| ParteneriSI | 52 |
| ClasificatiiV / RectificariV | 0 / 0 |
| UNIT | 1 |
| LEGUNIT | 2 |

Largest tables in the file, none of them in scope: `DefaCont` 991, `DefaClsfE` 905, `Credit`
576, `Debit` 535, `Oper` 530, `PlanCont` 351, `SalariiPC` 214, `SalariiVA` 170, `Documente`
147.

**`FX_2026.accdb`** (27 tables):

| Table | Rows | | Table | Rows |
|---|---:|---|---|---:|
| FX_Angajamente | 1 | | FX_ORD | 17 |
| FX_DDF | 1 | | FX_ORD_PART | 144 |
| FX_DDF_REV | 4 | | FX_ORD_TBL | 461 |
| FX_DDF_REV_SA | 22 | | FX_ORD_DOC | 461 |
| FX_DDF_REV_SB | 22 | | FX_ORD_TBL_REC | 461 (out of scope) |
| FX_DDF_REV_ATT / _PRT | 0 / 0 (out of scope) | | FX_ORD_ATT | 0 (out of scope) |
| FX_Extrase | 50 | | FX_Plati | 575 |
| FX_Extrase_F | 3 | | FX_Receptii | 49 |
| FX_Extrase_H | 9 | | FX_Receptii_H | 5 |
| FX_Indicatori | 10 | | FX_Receptii_R | 5 |
| FX_Istoric | 701 | | FX_Receptii_RHR | 50 |
| FX_Rezervari | 57 | | FX_Receptii_IMG | 0 |
| FX_Rezervarii_IMG | 0 | | FX_Parteneri | 0 |

Nothing here is large. The whole FX file is ~3.100 rows in scope. The plan's figures
("`FX_Istoric` is 3246 rows and `FX_Plati` 2744 today") do not match this file — it is a
different unit or a different vintage. Batching at 500 is comfortable either way.

⚠ Two tables the plan's §6 write order names **do not exist** in `FX_2026.accdb`:
**`FX_Salarii`** (position 20) and **`FX_Receptii_Plati`** (position 21). `FX_ORD_TBL.IDRP` is
documented as a foreign key into `FX_Receptii_Plati`. Also present and unlisted: `FX_Parteneri`
(0 rows). See §9 Q6.

---

## 7. Corrections owed to `MAPARE_ACCESS_MARIADB.md`

Read from the live `.accdb`, so these close two of its open items and contradict three claims.
**I have not edited that file** — per §12.3 the plan says not to touch it speculatively. These
are for you to fold in.

| # | `MAPARE_ACCESS_MARIADB.md` says | The file says |
|---|---|---|
| 1 | §6.2 "`FX_DDF_REV.ESpeciala` … absent from the Access export — verify in the live `.accdb`" | **Verified: absent.** `FX_DDF_REV` has 17 columns, no `ESpeciala`. It is MariaDB-only |
| 2 | §6.1 "`TABLES/FX_ORD.md` — the only remaining reconstruction" | **No longer reconstructed.** Read: `IDORD, IDORDP, IDDF, IDRR, IDRH, CodAngajament, NrORD, DataORD, Comp, Cual, Incarcat, Preluat, ArePDF, CalePDF, DTQ, Semnatura` (16). `ArePDF`, `CalePDF`, `DTQ` are **not** in the §4 table |
| 3 | Rule 2: "`CUAL` is `INT(11)` everywhere. Access and MariaDB, `FX_DDF` and `FX_ORD`. No text handling, no conversion" | ⚠ **Half wrong.** `FX_DDF.Cual` is `Integer` ✓. **`FX_ORD.Cual` is `WChar(255)`** — text, holding `"4"`. A conversion is needed on the ORD side |
| 4 | §4 `FX_ORD`: "`IDORDP` (Access) — 0 in Access, never sent" | ⚠ **Not 0.** `FX_ORD.IDORDP` holds **117, 118, 119, 120, 121, 122, 123 …** — the old server's ids, exactly the `IdClsfPY` pattern. (`FX_ORD_TBL.IDORDP` *is* 0, as documented — the two tables differ.) Option (A) writes Access `IDORD` (1,2,3…) into MariaDB `IDORDP`, so anything already on the server keyed on 117+ will not match. **Flagging, not deciding** |
| 5 | §4 `FX_ORD_TBL`: "⚠ Access spells it `CodAi` in `tmpFX_ORD_TBL`" | In `FX_ORD_TBL` it is **`CodAI`**. Third spelling of the same column — Rule 4 earns its place |

Also confirmed as written: `FX_ORD_TBL.IDORDTBLP` is populated (430, 431, 432…) and
`FX_ORD_TBL.IDORDP` is 0; `FX_DDF_REV_SA.SS` is `WChar(255)` in Access but every sample value
is exactly 3 characters (`01A`); `FX_DDF_REV_SA.IdSecA` is populated (43, 44, 45…).

One more, not a contradiction: **ACE exposes no `ForeignKeys` collection** on any of the three
files ("The requested collection (ForeignKeys) is not defined"). Access relationships are
visible only as index names (`Clasificatii__IDClsf___Rectificari__IdClsf`). The plan's §6
already derives the write order from the **target's** `information_schema.KEY_COLUMN_USAGE`,
which is the right call and now the only one available.

---

## 8. What this pass could NOT establish

Stated plainly rather than guessed, per `CODE_WORKFLOW.md` §4.

1. **The real MariaDB schema.** No server was contacted. Every "MariaDB" column list above is
   read off an `INSERT` in a Flask route, which names what that route writes — not what the
   table has. Specifically unknown for all four target tables: the full column list, the
   nullability, the defaults, the text widths, and whether any `NOT NULL`-without-default
   column sits outside the route's list. The plan's own §5.3 guard is what catches this at
   runtime; it does not let me write the mapping with confidence now. **`000_DEMO.sql` is not
   in the repository** — I looked. Give me a dump or a connection and I will close this.
2. **`Rectificari` has never been seen with rows** (0 in the only file available), so its
   mapping is schema-only.
3. **Only one unit's `baza2026.accdb` is available** — `Energetic ISJ` (IdUnitate 76, `SURSA`
   `01A`). `cai` lists 13. The `02E` / `VENITURI` units will exercise `ClasificatiiV` /
   `RectificariV`, which this file leaves empty.
4. **Only one DC** (`000_DEMO`) exists in the registry, so §11 Q1 is confirmed but not
   stress-tested.
5. **`Clasificatii_Buget.An` has no source** — see §9 Q3.

---

## 9. Questions — blocking the implementation pass

The plan's §11 asked six. Three are answered above (Q1 yes/§1; Q2 and Q5 restated below).
These are what is actually open, most blocking first.

**Q3 (was §11 Q3, now blocking). Where does `Clasificatii_Buget.An` come from?**
The Access `Clasificatii` row carries no year. Candidates: `cai.AnDate` (`"2026"`, text),
`cai.ANNOU` (`"2026"`, text), or the file name. They agree in every row I have, so the choice
is only visible when they disagree. Which is authoritative?

**Q4. A unit with nomenclators but no FX file.** `cai` rows 110 and 114 have
`CaleForexe = NULL`. Do their `Clasificatii` / `Parteneri` still transfer (leaving the target
with a nomenclator and no `FX_*` data), or is the unit skipped entirely?

**Q5. `ParteneriAng` ▸ `Parteneri_Coduri` (§5.1) — in scope?** Both ends exist and map
cleanly. It is not in the plan. And `ParteneriSI` (52 rows) — no target found; confirm it is
out.

**Q6. `FX_Salarii` and `FX_Receptii_Plati` are in the plan's §6 write order but not in
`FX_2026.accdb`.** Drop them from the order, or is this file simply missing tables another
unit has? `FX_ORD_TBL.IDRP` points at `FX_Receptii_Plati`, so if it never travels, `IDRP` is
an orphan column by construction.

**Q7. `ClasificatiiV` / `RectificariV`** (the venituri pair, both empty here) — in scope for
the `02E` units, or out?

**Q8 (was §11 Q2). MariaDB client library** — `MySqlConnector` or `MySql.Data`? My
recommendation stands: `MySqlConnector`, MIT, no licence question. Nothing in the solution
talks to MariaDB today, so this is a free choice.

**Q9 (was §11 Q4).** A target database that already exists **with rows** — refuse, or offer
`Golește tabelele înainte`?

**Q10 (was §11 Q5).** Does `AVACONT_SURSA` carry views / triggers / routines, or tables only?
Note `baza2026.accdb` has four Access queries (`qBugetTransfer`, `qCLSFV`, `qCT8030`,
`qRestPlata`) — evidence that this estate does use saved queries, though those are the Access
side.

**Q11 (was §11 Q6).** Creating the MariaDB user and grants for a new DC — this tool, or by
hand?

**Q12 (from §7 above).** `FX_ORD.IDORDP` holds live old-server ids (117+), not the documented
0. Under option (A) the migration overwrites those with the Access `IDORD` (1..17). Confirm
that is intended.

---

## 10. Not a finding, but load-bearing: no password is needed

`PLAN_MigratorDirect.md` §1/§3 and `KBOT_STATUS.md` slice 0044 both state the files are
encrypted (slice 0044: "fișierele chiar sunt criptate cu `andreI` … deci niciun cititor
pur-Python nu le deschide"). **All three files in `Avacont/` opened with
`Microsoft.ACE.OLEDB.16.0` and no password at all.**

The spike still builds the connection string with `OleDbConnectionStringBuilder` and still
supports `Jet OLEDB:Database Password`, and the form must keep its password fields — the
operator's own copies may well be protected. But the "must be decrypted by hand in Access
first" step of slice 0044 does not apply to the files in this repository.

This pass therefore also discharges the proof the plan asked for in §12.1: **ACE 16.0 is
registered 64-bit on this machine, an x64 `net8.0` build opens all three `.accdb` files, and
the password property is wired.** `Microsoft.ACE.OLEDB.12.0` is registered too, so the
fallback has something to fall back to.
