# MAPARE NOMENCLATOARE — Access ▸ MariaDB

> Companion to `MAPARE_ACCESS_MARIADB.md` (which covers the `FX_*` families).
> This file covers the reference tables: `Clasificatii`, `Clasificatii_Buget`,
> `Clasificatii_Rectificari`, `Parteneri`, `Parteneri_Coduri`. Same conventions:
> `=` identical, `▸` renamed, `✗` does not travel. Every claim is traceable to the Evidence.
>
> **Deliverable of `PLAN_MigratorDirect.md` §12 (discovery pass).** Both sides are now read
> from source: Access from the live `.accdb` files, MariaDB from the real DDL. Where the
> plan's or `MAPARE_ACCESS_MARIADB.md`'s assumption turned out wrong, it is named and
> marked ⚠.

Slice **0045**. Written 23.08.2026, revised the same day with the MariaDB schema.

---

## Evidence

**Access side** — `tools/AccdbSchemaDump/` (throwaway console spike, not in `KBot.sln`), run
on this machine 23.08.2026:

| Dump | Source file | Size | Tables |
|---|---|---:|---:|
| `artifacts/accdb-schema/cale.md` | `Avacont/cale.accdb` | 679.936 B | 11 |
| `artifacts/accdb-schema/baza2026.md` | `Avacont/Energetic ISJ/baza2026.accdb` | 3.784.704 B | 69 |
| `artifacts/accdb-schema/FX_2026.md` | `Avacont/Forexe/FX_2026.accdb` | 2.678.784 B | 27 |

**MariaDB side** — `MariaDB_Schema/000_DEMO.sql` (84.779 B, dated 22.08.2026). Real
`CREATE TABLE` DDL for 54 tables. **Schema only: zero `INSERT` statements, zero views, zero
triggers, zero stored routines.**

⚠ **None of this is in git** — `artifacts/` (`.gitignore:66`), `Avacont/` and
`/MariaDB_Schema` (`.gitignore:507`) are all ignored, and the dumps carry real partner names,
IBANs and figures. Regenerate the Access side in one command from a checkout that has the
files:

```bash
dotnet run --project tools/AccdbSchemaDump -c Release -- artifacts/accdb-schema "Avacont/cale.accdb" - "Avacont/Energetic ISJ/baza2026.accdb" - "Avacont/Forexe/FX_2026.accdb" -
```

(Arguments after the output directory are read in pairs: path, then password — `-` for none.)

---

## 0. Decisions taken (23.08.2026)

Recorded so nobody re-derives them. Operator's answers to §9 of the first revision.

| # | Question | Decision |
|---|---|---|
| D1 | `Clasificatii_Buget.An` has no source column | **`2026`, hardcoded.** This is a **one-off** transfer; everything in it is for 2026 |
| D2 | Which units are transferred | **The operator picks the `IdUnitate`.** Not "every unit in the DC" — a selection on the form. This also disposes of the units with `CaleForexe = NULL` (`cai` 110, 114): if picked, they transfer their nomenclators and have no `FX_*` data |
| D3 | `ParteneriSI` (52 rows, no target) | **Out.** Confirmed |
| D4 | `FX_Salarii`, `FX_Receptii_Plati` | **Out of MariaDB's scope entirely** — they exist in neither the `.accdb` nor the schema. Drop both from the §6 write order. `FX_ORD_TBL.IDRP` is therefore **an orphan column by construction** and travels as NULL |
| D5 | `ClasificatiiV` / `RectificariV` (venituri) | **Not in this slice** |
| D6 | `Avacont/` untracked but not gitignored | **Done** — added to `.gitignore`, along with `/MariaDB_Schema` |
| D7 | `000_DEMO` already holds rows; where does a run write? | **A brand-new empty database**, created from `AVACONT_SURSA` per plan §4. `000_DEMO` was only ever the schema template. **Option (A) is therefore safe** — nothing pre-exists to collide with, and the Access ids stay authoritative. F6 below is retained as the reason, not as a live hazard |
| D8 | `Clasificatii` has no unique key for its upsert (§3.2) | **Insert-only, run exactly once.** No schema change. The tool **refuses to run** when the target's `Clasificatii` already holds rows for a selected unit, so a second run is impossible rather than silently duplicating |
| D9 | `IdClsfAcc` is `NOT NULL` on `FX_DDF_REV_SA`/`_SB` (§9 #6) | **Write the Access `IdClsf` into it.** That is exactly what `IdClsfAcc` means on `Clasificatii`, so it stays consistent, needs no server change, and the column is being retired anyway |
| D10 | MariaDB client library | **`MySqlConnector`** — MIT, actively maintained, genuinely async, drop-in for the ADO.NET interfaces |

Confirmed by the schema rather than asked: **`AVACONT_SURSA` carries tables only** — the
`000_DEMO` dump has no view, trigger or routine, so §4 step 3 of the plan (copy every table
via `SHOW CREATE TABLE`) is sufficient. That was §11 Q5.

### ⚠ What D7 costs: a fresh database starts with an empty `Unitati`

`CREATE DATABASE` + `SHOW CREATE TABLE` copies **structure, not rows**. So on a brand-new
database `Unitati` is empty — and `Clasificatii.IdUnitate`, `Clasificatii_Buget.IdUnitate`,
`Parteneri.IdUnitate` and `FX_ORD_TBL.IdUnitate` all carry a foreign key into it (§7).

**Nothing can be written at all until `Unitati` holds the selected units.** This is no longer
a "check it first" nicety; it is a precondition of the very first INSERT. Every value it needs
is available:

| `Unitati` column | Source |
|---|---|
| `IdUnitate` `int(11)` PK, **not** `AUTO_INCREMENT` | `cai.IdUnitate` |
| `Detalii` `varchar(255) NOT NULL` | `cai.NumeUnitate` (or `UNIT.Detalii` in the unit's own file) |
| `SursaSector` `varchar(3) NOT NULL` | `cai.SURSA` — `01A` / `02A` / `02E`, already 3 characters |
| `An` `int(4) NOT NULL` | `2026` (D1) |
| `CodProgram` `varchar(255) NULL` | `UNIT.CodProgram` |
| `Ascuns` `tinyint(4) NOT NULL DEFAULT 0` | `0` |

So the tool **can** populate it, and on D7's fresh-database path it has to. Confirm — §10 Q2.

`AVACONT_COMUN` (§3.1) is the other half of the same problem and D7 does not touch it: it is a
**separate database**, not created by the `AVACONT_SURSA` loop, and five of `Clasificatii`'s
six foreign keys point into it. A fresh database still cannot accept one classification row
until `AVACONT_COMUN` exists and is populated on that server.

---

## 1. Findings that change the plan

Six. The first four came from the `.accdb` files, the last two from the schema, and the last
one is the most serious thing in this document.

### F1 ⚠ `cale.accdb` holds none of the four nomenclators

`PLAN_MigratorDirect.md` §12.2 Q1 assumed it carries `CAI`, `Parteneri`, `Clasificatii` and
`Rectificari`. It carries **only the registry**:

```
BurseColoane  BurseFoi  cai  K  LEGUNIT  TC  tmpBurse  tmpBurseBanci  TRACE  TRACE_S  Ver
```

`cai` is spelled lowercase (Rule 4 applies).

### F2 ⚠ The nomenclators live in a **per-unit** file, one per `IdUnitate`

`Clasificatii` (54 rows), `Parteneri` (75), `Rectificari` (0) are in
`Avacont/Energetic ISJ/baza2026.accdb`. `cai` points at it: row `IdUnitate = 76` has
`CaleUnitate = .\ENERGETIC ISJ` and `FullPath = .\ENERGETIC ISJ\baza2026.accdb`.

So it is **one `baza<year>.accdb` per `IdUnitate`**, opened in turn — driven by `cai`, filtered
by the operator's selection (D2). `cai` lists 13 for DC `000_DEMO`.

### F3 ⚠ The nomenclator rows carry **no `IdUnitate` column**

The answer to §12.2 Q5, and the opposite of what it expected ("Confirm only that Access
`Clasificatii` carries `IdUnitate` …").

`Clasificatii`, `Parteneri` and `Rectificari` have **no `IdUnitate` column at all**. The unit
is implied by the file.

The `(IdClsfAcc, IdUnitate) ▸ IDClsf` map of `MAPARE_ACCESS_MARIADB.md` Rule 1 is still
correct — but `IdUnitate` is **supplied by the loop** (the `cai` row whose file is open), not
read off the row. Make that explicit in the code or someone will hunt for the column.

The `FX_*` side is unaffected: `FX_DDF_REV_SA`, `_SB` and `FX_ORD_TBL` **do** carry
`IdUnitate` themselves.

### F4 The link is proven end to end on real rows

| Step | Value | Where |
|---|---|---|
| `FX_DDF_REV_SA.IdUnitate` | `76` | `FX_2026.md` |
| `cai.IdUnitate = 76` ▸ `CaleUnitate` | `.\ENERGETIC ISJ` | `cale.md` |
| that folder's `Clasificatii.IDClsf` | `92, 123, 128, 138, 141, 143, 158 …` | `baza2026.md` |
| `FX_DDF_REV_SA.IdClsf` on those rows | `92, 123, 128, 138, 143, 145, 146, 154, 158` | `FX_2026.md` |

Every `IdClsf` in the `FX_DDF_REV_SA` sample resolves to an `IDClsf` in that unit's
`Clasificatii`. The plan's resolution path is right.

### F5 `IdClsfPY` is demonstrably stale — Rule 1 confirmed on data

| Row | `IdClsf` | `IdClsfPY` |
|---|---:|---:|
| `baza2026.Clasificatii` | 123 | **1363** |
| `FX_2026.FX_DDF_REV_SA` (3 rows) | 123 | **1309** |
| `FX_2026.FX_ORD_TBL` (5 rows) | 123 | **1309** |

Same classification, two different "old server" ids. Matching on `IdClsfPY` would have
mismatched silently.

### F6 ⚠⚠ `000_DEMO` is **not empty**, and this may be a re-sync rather than a first migration

The single most consequential fact in the schema. Every `AUTO_INCREMENT` high-water mark:

| Table | Next id | Table | Next id |
|---|---:|---|---:|
| `Clasificatii` | **1497** | `FX_ORD` | **134** |
| `Clasificatii_Buget` | **756** | `FX_ORD_PART` | **402** |
| `Clasificatii_Rectificari` | 1 (empty) | `FX_ORD_TBL` | **891** |
| `Parteneri` | **7680** | `FX_ORD_DOC` | **719** |
| `Parteneri_Coduri` | **5** | `FX_DDF` | **65** |
| `FX_DDF_REV_SA` / `_SB` | **68** / **68** | `FX_DDF_REV` | **109** |

Now line those up against the Access values:

* Access `Clasificatii.IdClsfPY` = **1354 … 1373**, inside `Clasificatii`'s 1..1496.
* Access `Parteneri.IdPartener` = **7605 … 7621**, inside `Parteneri`'s 1..7679.
* Access `FX_ORD.IDORDP` = **117 … 123**, inside `FX_ORD`'s 1..133.
* Access `FX_ORD_TBL.IDORDTBLP` = **430 … 444**, inside `FX_ORD_TBL`'s 1..890.

Those are not stale ids from a decommissioned server. **They are live ids on this one.** This
unit has already been synced into `000_DEMO`, and the Access file is carrying that sync's
result back in its mirror columns.

Two consequences, both of which need a decision before any row is written:

1. **Option (A) will collide.** Writing Access `FX_ORD.IDORD` = 1..17 into the `IDORDP`
   `AUTO_INCREMENT` key does not create new orders — it lands on **existing rows 1..17**, which
   belong to whatever was there before, and `ON DUPLICATE KEY UPDATE` then overwrites them.
   The same shape applies to `FX_DDF.IDDF` = 33 (target holds 1..64) and `FX_DDF_REV.IDREV` =
   44..47 (target holds 1..108). `MAPARE_ACCESS_MARIADB.md` §0 chose (A) on the assumption of
   a fresh database; that assumption does not hold here.
2. **The plan's §4 step 4 refusal fires immediately.** "If present — check whether any table in
   scope already holds rows. If so, refuse." On `000_DEMO`, every table in scope has rows.

This is §9 Q1 below, and nothing should be written until it is settled.

---

## 2. `cai` — the registry

`cale.accdb` ▸ `cai`, 13 rows. No primary key; three non-unique indexes, all on `IdUnitate`.
Read-only.

| # | Column | Type | Used |
|---:|---|---|---|
| 1 | IdUnitate | Integer | **yes** — the unit key, and the value written into `IdUnitate` on the target |
| 2 | AnDate | WChar(50) | `"2026"` — superseded by D1 (the year is hardcoded) |
| 3 | ANNOU | WChar(4) | `"2026"` — duplicate of the above in every row |
| 4 | DC | WChar(255) | **yes** — the target database name |
| 5 | BLOCAT | Boolean NOT NULL | `False` on all 13 |
| 6 | USR | WChar(50) | NULL on all 13 |
| 7 | Cdeschid | WChar(255) | `X` or NULL |
| 8 | FullPath | WChar(255) | `.\ENERGETIC ISJ\baza2026.accdb` — relative to the `cale.accdb` folder |
| 9 | SURSA | WChar(255) | `01A` / `02A` / `02E` — same alphabet as `SS` on the `FX_*` rows |
| 10 | AlteDetalii | WChar(255) | `LOCAL` / `ISJ` / `VEN` / `VENITURI` / `REPUBLICAN` |
| 11 | CaleUnitate | WChar(255) | **yes** — the unit folder |
| 12 | NumeUnitate | WChar(255) | **yes** — the label in the operator's unit picker (D2) |
| 13 | CaleEFACTURA | WChar(255) | out of scope |
| 14 | UnitatiSclav | WChar(255) | NULL on all 13 |
| 15 | IdUnitateParinte | Integer | NULL on all 13 |
| 16 | CaleForexe | WChar(255) | **yes** — `C:\AVACONT\Forexe\FX…`; **NULL on 2 of 13** (IdUnitate 110, 114) |

Two traps before coding against it:

* `FullPath` and `CaleUnitate` are **relative** (`.\…`), resolved against the folder holding
  `cale.accdb`, and inconsistently terminated — `.\SC29 LOCAL\` has a trailing separator,
  `.\ENERGETIC ISJ` does not. Normalise.
* `CaleForexe` is NULL for 110 and 114. Under D2 the operator may still select them; the run
  must transfer their nomenclators and find no `FX_*` file, without treating that as an error.

### §11 Q1 — is `cai.DC` the database name verbatim?

**Yes, on the evidence available, but single-valued.** All 13 rows carry `DC = "000_DEMO"`,
which is exactly the schema file's database name and the one used throughout the repo. There
is no second DC in this registry to show whether any transformation applies. Confirmed for
`000_DEMO`; re-check on the first registry listing two DCs. `FX_DDF.DC` and `FX_DDF_REV.DC`
also read `000_DEMO` — a second, independent occurrence.

---

## 3. `Clasificatii`

**Access** — `baza2026.accdb` ▸ `Clasificatii`, 54 rows, PK `IDClsf`. Non-unique indexes on
Capitol / Subcapitol / Articol / Alineat.

**MariaDB** — read from DDL. Only **seven** columns are writable; nine are
`GENERATED ALWAYS … PERSISTENT` and **cannot appear in an INSERT at all**, and two are
server-maintained timestamps.

| Access | MariaDB | |
|---|---|---|
| IDClsf | **IdClsfAcc** | ▸ `int(11) NOT NULL DEFAULT 0`. Key of the resolution map |
| — | IDClsf | `AUTO_INCREMENT` PK, assigned. **The value the `FX_*` rows are rewritten to** |
| *(the file's unit)* | **IdUnitate** | ▸ F3 — no source column; from the `cai` row. `int(11) NULL`, FK → `Unitati` ON DELETE CASCADE |
| Capitol | Capitol | ▸ WChar(50) ▸ **`varchar(5) NOT NULL`** |
| Subcapitol | Subcapitol | ▸ WChar(50) ▸ **`varchar(5) NOT NULL`** |
| Articol | Articol | ▸ WChar(50) ▸ **`varchar(5) NOT NULL`**, and **FK → `AVACONT_COMUN.DefaArticol`** |
| Alineat | Alineat | ▸ WChar(50) ▸ **`varchar(2) NOT NULL`** |
| Denumire | Denumire | ▸ WChar(255) nullable ▸ **`varchar(255) NOT NULL`** |
| Trim1..Trim4 | ▸ `Clasificatii_Buget` | Double. See §4 |
| TOTAL | ✗ | Double. On the target it is `Clasificatii_Buget.TOTAL`, **generated** |
| IdClsfPY | ✗ | **Rule 1 — never read.** F5 |
| Clsf, Titlu, ClsfSal, ClsfF, ClsfE, ClsfX, Sector, Sursa, SS | ✗ | ⚠ **All nine exist on the target as `GENERATED ALWAYS … PERSISTENT`.** Computed there from Capitol/Subcapitol/Articol/Alineat. Writing one is an error, not a no-op |
| CodSSI | ✗ | Memo, `01A650402100101`. No column on the target |
| CodAng, CodInd | ✗ | NULL throughout the sample. No column on `Clasificatii` (they live on `Parteneri_Coduri`, §6.1) |
| TOTALFX | ✗ | Double, NULL on most rows. No target |
| DTQ, Esinc | ✗ | Access bookkeeping / sync flag |
| Document, Data | ✗ | NULL throughout; the rectification's document/date live on `Rectificari` |
| IdLegatura | ✗ | NULL throughout |
| — | DataAdugare, DataModificare | server-side (note the spelling `DataAdugare`) |

### 3.1 ⚠ Five cross-database foreign keys — a gate the plan does not have

`Clasificatii` carries six constraints. Five of them point **out of the database entirely**:

| Constraint | Column | References |
|---|---|---|
| `Clasificatii_ibfk_1` | `ClsfE` *(generated)* | `AVACONT_COMUN.DefaClsfE` |
| `Clasificatii_ibfk_2` | `ClsfF` *(generated)* | `AVACONT_COMUN.DefaClsfF` |
| `Clasificatii_ibfk_3` | `Titlu` *(generated)* | `AVACONT_COMUN.DefaTitlu` |
| `Clasificatii_ibfk_4` | `SS` *(generated)* | `AVACONT_COMUN.DefaSursaSector` |
| `Clasificatii_ibfk_5` | `Articol` *(written)* | `AVACONT_COMUN.DefaArticol` |
| `Clasificatii_ibfk_6` | `IdUnitate` | `Unitati` *(same database)* |

So a classification row is rejected with `1452` unless the **computed** `ClsfE`, `ClsfF`,
`Titlu`, `SS` and the **written** `Articol` all exist in `AVACONT_COMUN`'s dictionaries. Four
of the five are values the migrator never writes and cannot inspect before the INSERT — they
fall out of `concat`/`left`/`replace` over what it does write.

Three things follow:

1. **The plan's §5 needs a third gate.** `Verifică` must compute the five values per row the
   same way the generated columns do, and check them against `AVACONT_COMUN` — otherwise the
   first failure is a `1452` mid-transaction, naming a column nobody wrote.
2. **`CREATE DATABASE` from `AVACONT_SURSA` is not enough** (plan §4 step 3). `AVACONT_COMUN`
   is a **different database** and is not created by that loop. A new DC's `Clasificatii` will
   not accept a single row until `AVACONT_COMUN` exists and is populated.
3. `Sector`/`Sursa`/`SS` are derived from `right(Capitol, 2)` with an explicit `else ''`, so a
   `Capitol` outside `00/01/02/10` computes `SS = ''` — which will not be in
   `DefaSursaSector`. The failure mode is a blank, not a wrong value.

### 3.2 ⚠ No unique key on `(IdClsfAcc, IdUnitate)` — the upsert cannot match

`Clasificatii` has exactly one unique index: `PRIMARY KEY (IDClsf)`. `IdClsfAcc` and
`IdUnitate` each carry a **non-unique** `INDEX`, and there is no composite unique key over the
pair.

So `INSERT … ON DUPLICATE KEY UPDATE` — plan §7, "on every table" — **has nothing to match on
for this table**. A second run inserts 54 more classifications with fresh `IDClsf` values, and
the resolution map silently points at the newest copies. This is the one table in the set where
the plan's upsert rule does not work as written.

`Clasificatii_Buget` `(IdClsf, An)`, `Clasificatii_Rectificari` `(IdClsf, Data, Document)`,
`Parteneri` `(IdUnitate, CodPartener)` and `Parteneri_Coduri` `(IdPartener, IdClsf)` all **do**
have the unique key their upsert needs. Only `Clasificatii` does not. See §9 Q2.

### 3.3 Width narrowing — fits today, unenforced at source

Access is `WChar(50)` on Capitol / Subcapitol / Articol and `WChar(50)` on Alineat; MariaDB is
`varchar(5)`, `varchar(5)`, `varchar(5)`, `varchar(2)`. Every sample value fits exactly
(`65.01`, `05.00`, `10.01`, `01`), but nothing on the Access side enforces it. Under strict
mode an over-long value is `1406`, not a truncation. Check the widths in `Verifică` — the
plan's §5.3 guard only covers *missing required* columns, not *too-wide* values.

Same shape on `Denumire`: nullable in Access, **`NOT NULL`** on the target.

---

## 4. `Clasificatii_Buget` — §12.2 Q2 answered

**Four columns of one row, not four rows.** Settled by the DDL, not a reading:

```sql
`IdBuget` int(11) NOT NULL AUTO_INCREMENT,
`IdClsf`  int(11) NOT NULL,          -- FK -> Clasificatii(IDClsf) ON DELETE CASCADE
`IdUnitate` int(11) NOT NULL,        -- FK -> Unitati(IdUnitate)  ON DELETE CASCADE
`TOTAL`   double GENERATED ALWAYS AS (coalesce(Trim1,0)+…+coalesce(Trim4,0)) PERSISTENT,
`Trim1`…`Trim4` double NULL,
`An`      int(4) NULL,
UNIQUE INDEX `uq_clasificatii_buget_idclsf_an`(`IdClsf`, `An`)
```

| Access `Clasificatii` | MariaDB `Clasificatii_Buget` | |
|---|---|---|
| *(resolved id)* | IdClsf | the **assigned** `IDClsf` from §3, not the Access one. `NOT NULL` |
| *(the file's unit)* | IdUnitate | ▸ F3 — from the `cai` row. **`NOT NULL`** here (nullable on `Clasificatii`) |
| *(none)* | An | **`2026`, hardcoded** (D1). `int(4)`, so an integer, not the `"2026"` text `cai` holds |
| Trim1 | Trim1 | = Double |
| Trim2 | Trim2 | = Double |
| Trim3 | Trim3 | = Double |
| Trim4 | Trim4 | = Double |
| TOTAL | ✗ | ⚠ **generated on the target** — never write it. Access's own `TOTAL` is the same sum |

Upsert matches on `(IdClsf, An)`, both always present. Works.

---

## 5. `Clasificatii_Rectificari` — §12.2 Q3 answered

**Access** — `baza2026.accdb` ▸ `Rectificari`, **0 rows**, PK `ID`, index on `IdClsf` plus
Access's own relationship index `Clasificatii__IDClsf___Rectificari__IdClsf`.

**MariaDB** — `ID` `AUTO_INCREMENT` PK, `UNIQUE (IdClsf, Data, Document)`,
`FK IdClsf → Clasificatii(IDClsf) ON DELETE RESTRICT`. **No `IdUnitate` column** — a
rectification is scoped to a unit only through its classification.

| Access `Rectificari` | MariaDB | |
|---|---|---|
| IdClsf | IdClsf | ▸ **Rule 1** — the Access value is a `Clasificatii.IDClsf`, so it resolves through the map. `int(11) NULL`, FK RESTRICT |
| Capitol | Capitol | = WChar(255) ▸ `varchar(255) NULL` — **no narrowing here**, unlike `Clasificatii` (§3.3) |
| Subcapitol | Subcapitol | = idem |
| Articol | Articol | = idem — and **no FK to `DefaArticol`** here, unlike `Clasificatii` |
| Alineat | Alineat | = idem |
| Data | Data | = Date ▸ `datetime NULL` · part of the unique key |
| Document | Document | = WChar(255) ▸ `varchar(255) NULL` · part of the unique key |
| Trim1..Trim4 | Trim1..Trim4 | = Double |
| ID | ✗ | Access PK. The target's `ID` is its own `AUTO_INCREMENT` |
| DTQ, Esinc | ✗ | Access bookkeeping / sync flag |

**Access columns with no target:** `ID`, `DTQ`, `Esinc` — all local bookkeeping.
**Target columns with no source:** none. `DataAdugare` / `DataModificare` are server-side.
**No `NOT NULL`-without-default column on the target at all** — every writable column is
nullable, so the plan's §5.3 guard has nothing to catch here.

⚠ Two cautions:

* **The table is empty on both sides** (Access 0 rows; target `AUTO_INCREMENT = 1`). The
  mapping is schema-only — no row has ever been through it.
* **The unique key contains two nullable columns.** In MySQL/MariaDB a NULL never equals a
  NULL in a unique index, so a rectification with a NULL `Data` or `Document` will **not**
  match on re-run and will duplicate. Access allows NULL in both. Harmless for a one-off
  (D1), worth knowing if this ever runs twice.

---

## 6. `Parteneri`

**Access** — `baza2026.accdb` ▸ `Parteneri`, 75 rows, PK **`CodPartener`** (WChar(50)).

**MariaDB** — `IdPartener` `AUTO_INCREMENT` PK, `UNIQUE (IdUnitate, CodPartener)`,
`FK IdUnitate → Unitati ON DELETE CASCADE`.

| Access | MariaDB | |
|---|---|---|
| *(the file's unit)* | IdUnitate | ▸ F3 — from the `cai` row. **`int(11) NOT NULL`** |
| CodPartener | CodPartener | = WChar(50) ▸ `varchar(50) NOT NULL`. Same width both sides |
| DenumirePartener | DenumirePartener | = `varchar(255) NULL` |
| CodFiscal | CodFiscal | = `varchar(255) NULL`. Empty string (not NULL) on most Access rows |
| ContIBAN | ContIBAN | = `varchar(255) NULL` |
| Banca | Banca | = `varchar(255) NULL` |
| Adresa | Adresa | = `varchar(255) NULL` |
| Tip | Tip | = `varchar(255) NULL` both sides. Values are `"1"` / `"2"` — text holding a number, on both sides, so no conversion |
| **Ascuns** | **Ascuns** | ⚠ = Access `Boolean NOT NULL DEFAULT 0` ▸ `tinyint(4) NULL`. **The target HAS this column** and the Flask route does not write it. It should travel |
| — | IdPartener | `AUTO_INCREMENT`, assigned |
| IdPartener (Access) | ✗ | ⚠ Integer, **7605…7621** — see F6: these are *live ids on this server*, not dead ones. **Do not send** |
| IDPART | ✗ | Integer `1,2,3…` — a third, purely local sequence |
| NumePartener | ✗ | The uppercased `DenumirePartener`. No target column |
| ContPl, F8, F9, CodClient | ✗ | NULL/empty throughout. No target columns |
| DTQ, Esinc | ✗ | Access bookkeeping / sync flag |

⚠ Three id-shaped columns on one Access table — `IdPartener` (7605+), `IDPART` (1,2,3…) and
the real key `CodPartener` (text `"001"`). Only `CodPartener` travels.

Per `MAPARE_ACCESS_MARIADB.md` §5.2 this does **not** revive the `IdPartener` foreign keys on
`FX_DDF_REV_SA` / `_SB` / `FX_ORD_TBL` — those still travel NULL unless you decide otherwise.
Note the schema agrees they are all nullable, so NULL is accepted.

### 6.1 `ParteneriAng` ▸ `Parteneri_Coduri` — a fifth nomenclator nobody named

Not in the plan, not in `MAPARE_ACCESS_MARIADB.md`, but both ends exist and line up.
Access `ParteneriAng` 2 rows; target `AUTO_INCREMENT = 5`, so 4 rows already there.

| Access `ParteneriAng` | MariaDB `Parteneri_Coduri` | |
|---|---|---|
| IdClsf | **IdClsfAcc** *and* **IdClsf** | ▸ Rule 1 — the Access value into `IdClsfAcc`, the resolved id into `IdClsf`. Both `int(11) NOT NULL DEFAULT 0`; `IdClsf` is FK → `Clasificatii` |
| CodPartener | CodPartener | = `varchar(255) NULL` |
| CodAng | CodAng | = WChar(11) ▸ `varchar(255) NULL` |
| CodInd | CodInd | = WChar(4) ▸ `varchar(255) NULL` |
| ContBanca | **ContBancar** | ▸ **note the rename** |
| — | IdPartener | the assigned `Parteneri.IdPartener`. `NOT NULL DEFAULT 0`, FK → `Parteneri` |
| Id, Clsf, DTQ | ✗ | local key / derived code / bookkeeping |

Upsert matches on `UNIQUE (IdPartener, IdClsf)`. It is the **only** table found where the
target keeps `IdClsf` and `IdClsfAcc` side by side — everywhere else `IdClsfAcc` is being
retired. **Still needs a scope decision** — §9 Q4.

`ParteneriSI` is **out** (D3).

---

## 7. `Unitati` — a precondition, not a target

Read-only per `MAPARE_ACCESS_MARIADB.md` §2, and the schema shows why it matters anyway:

```sql
`IdUnitate` int(11) NOT NULL,          -- PK, NOT auto_increment
`Detalii`     varchar(255) NOT NULL,
`SursaSector` varchar(3)   NOT NULL,
`An`          int(4)       NOT NULL,
`Ascuns`      tinyint(4)   NOT NULL DEFAULT 0,
```

`Clasificatii.IdUnitate`, `Clasificatii_Buget.IdUnitate`, `Parteneri.IdUnitate` and
`FX_ORD_TBL.IdUnitate` all have a foreign key into it. So **the selected unit's row must
already exist in `Unitati`** or the very first nomenclator INSERT fails with `1452`. Nothing in
the plan writes `Unitati`, and `IdUnitate` is not `AUTO_INCREMENT`, so it cannot appear by
itself.

`Verifică` should check the selected `IdUnitate` values against `Unitati` before anything else
— it is the cheapest possible gate and it fires before a transaction is even opened. This is
already flagged in `KBOT_STATUS.md` Current focus for slice 0044-04 pass 07, point (1);
it now applies to the nomenclators too, one step earlier in the order.

`SursaSector varchar(3) NOT NULL` matches `cai.SURSA` (`01A`/`02A`/`02E`) — useful if the row
ever has to be created, which is §9 Q5.

---

## 8. Row counts (§12.2 Q6)

**`cale.accdb`** — `cai` **13**, `LEGUNIT` 7, `K` 1, `TC` 1, `Ver` 1; the rest empty.

**`baza2026.accdb`** (69 tables), the ones in scope:

| Table | Rows |
|---|---:|
| Clasificatii | **54** |
| Parteneri | **75** |
| Rectificari | **0** |
| ParteneriAng | 2 |
| ParteneriSI | 52 *(out, D3)* |
| ClasificatiiV / RectificariV | 0 / 0 *(out, D5)* |
| UNIT / LEGUNIT | 1 / 2 |

Largest in the file, none in scope: `DefaCont` 991, `DefaClsfE` 905, `Credit` 576, `Debit`
535, `Oper` 530, `PlanCont` 351.

**`FX_2026.accdb`** (27 tables):

| Table | Rows | | Table | Rows |
|---|---:|---|---|---:|
| FX_Angajamente | 1 | | FX_ORD | 17 |
| FX_DDF | 1 | | FX_ORD_PART | 144 |
| FX_DDF_REV | 4 | | FX_ORD_TBL | 461 |
| FX_DDF_REV_SA | 22 | | FX_ORD_DOC | 461 |
| FX_DDF_REV_SB | 22 | | FX_ORD_TBL_REC | 461 *(out)* |
| FX_Extrase | 50 | | FX_Plati | 575 |
| FX_Extrase_F | 3 | | FX_Receptii | 49 |
| FX_Extrase_H | 9 | | FX_Receptii_H | 5 |
| FX_Indicatori | 10 | | FX_Receptii_R | 5 |
| FX_Istoric | 701 | | FX_Receptii_RHR | 50 |
| FX_Rezervari | 57 | | FX_DDF_REV_ATT/_PRT, FX_ORD_ATT, FX_Receptii_IMG, FX_Rezervarii_IMG, FX_Parteneri | 0 |

~3.100 rows in scope for one unit. Batching at 500 is comfortable. The plan's figures
(`FX_Istoric` 3246, `FX_Plati` 2744) belong to a different unit or vintage.

⚠ **`FX_Salarii` and `FX_Receptii_Plati` exist in neither the `.accdb` nor the MariaDB
schema.** Confirmed out (D4); drop positions 20 and 21 from the §6 write order.
**`FX_ORD_PDF` does exist** in the schema — `MAPARE_ACCESS_MARIADB.md` §2 calls it "planned,
does not exist yet". It is empty (`AUTO_INCREMENT = 1`) and has no Access counterpart, so it
stays out of the transfer, but the parenthetical is stale.

---

## 9. Corrections owed to `MAPARE_ACCESS_MARIADB.md`

**I have not edited that file** — §12.3 says not to touch it speculatively. Seven points.

| # | It says | Source says |
|---|---|---|
| 1 | §6.2 "`FX_DDF_REV.ESpeciala` … verify in the live `.accdb`" | **Verified: absent from Access.** `FX_DDF_REV` has 17 columns, no `ESpeciala`. On MariaDB it is `tinyint(1) NULL DEFAULT 0` — MariaDB-only, and nullable, so it simply does not travel |
| 2 | §6.1 "`TABLES/FX_ORD.md` — the only remaining reconstruction" | **Read from source.** 16 columns: `IDORD, IDORDP, IDDF, IDRR, IDRH, CodAngajament, NrORD, DataORD, Comp, Cual, Incarcat, Preluat, ArePDF, CalePDF, DTQ, Semnatura`. `ArePDF`, `CalePDF`, `DTQ` are **not** in its §4 table |
| 3 | Rule 2: "`CUAL` is `INT(11)` everywhere. Access and MariaDB, `FX_DDF` and `FX_ORD`. Fixed 22.08. No text handling, no conversion" | ⚠ **Wrong on both sides for `FX_ORD`.** Access `FX_ORD.Cual` is `WChar(255)`; MariaDB `FX_ORD.CUAL` is `varchar(255)`. (`FX_DDF.Cual`/`CUAL` really are `int` both sides ✓.) Text ▸ text needs no conversion **today** — but the change is still owed per its own §5, and *then* a conversion appears |
| 4 | §4 `FX_ORD`: "`IDORDP` (Access) — 0 in Access, never sent" | ⚠ **Not 0** — 117…123, and F6 shows those are live ids on this server. (`FX_ORD_TBL.IDORDP` *is* 0, as documented — the two tables differ) |
| 5 | §4 `FX_ORD_TBL`: "Access spells it `CodAi` in `tmpFX_ORD_TBL`" | `FX_ORD_TBL` spells it **`CodAI`**; MariaDB spells it `CodAI`. Third spelling — Rule 4 earns its place |
| 6 | §5: "**`IdClsfAcc` no longer blocks anything.** It was `NOT NULL` with no default on SA and SB; set to allow NULL on 22.08, so a run that stops sending it succeeds" | ⚠⚠ **False in this 22.08 schema.** `FX_DDF_REV_SA.IdClsfAcc` and `FX_DDF_REV_SB.IdClsfAcc` are both `int(11) NOT NULL`, **no default, not auto_increment** — so the plan's own §5.3 guard stops the run naming them. `FX_ORD_TBL.IdClsfAcc` *is* nullable ✓. Something must be written into SA and SB. See §10 Q3 |
| 7 | §2: "`FX_ORD_PDF` (planned table, does not exist yet)" | It **exists** in the schema, empty. Still out of the transfer, but the note is stale |

Also, not a contradiction: **ACE exposes no `ForeignKeys` collection** on any of the three
`.accdb` files ("The requested collection (ForeignKeys) is not defined"); Access relationships
are visible only as index names. The plan's §6 already derives the write order from the
**target's** `information_schema.KEY_COLUMN_USAGE` — now the only option, and the schema
confirms it is a rich source (`Clasificatii` alone carries six constraints, five of them
cross-database).

---

## 10. Open questions — blocking the implementation pass

Five. The first is new and outranks everything else.

**Q1 ⚠⚠ `000_DEMO` already holds this unit's data (F6). What should a run do?**
Access `FX_ORD.IDORD` = 1..17 written into `IDORDP` lands on **existing rows** 1..17, and
`ON DUPLICATE KEY UPDATE` overwrites them. Same for `FX_DDF.IDDF` = 33 and
`FX_DDF_REV.IDREV` = 44..47. Option (A) in `MAPARE_ACCESS_MARIADB.md` §0 assumed a fresh
database. Either (a) the target is emptied first and the Access ids are authoritative, (b) the
transfer targets a genuinely new database and `000_DEMO` was only ever the schema template, or
(c) option (A) is wrong for a populated target and the ids must be remapped. This also decides
§11 Q4 (a database that already exists with rows — refuse or empty), because on `000_DEMO`
the plan's §4 refusal fires on every table in scope.

**Q2 `Clasificatii` has no unique key on `(IdClsfAcc, IdUnitate)` (§3.2)** — so its upsert
cannot match and a second run duplicates all 54 rows. Add the unique index to the schema, or
accept that this table is insert-only and the run must be exactly once?

**Q3 `FX_DDF_REV_SA.IdClsfAcc` / `_SB.IdClsfAcc` are `NOT NULL` with no default (§9 #6)**,
contradicting `MAPARE_ACCESS_MARIADB.md` §5. The run cannot omit them. Write the Access
`IdClsf` into them — which is exactly what `IdClsfAcc` means on `Clasificatii`, and would be
consistent — or make the columns nullable as §5 says was already done?

**Q4 `ParteneriAng` ▸ `Parteneri_Coduri` (§6.1) — in scope?** Both ends exist, map cleanly
(`ContBanca` ▸ `ContBancar` is the only rename), and the target has the unique key its upsert
needs. Only 2 rows in this unit's file.

**Q5 `Unitati` (§7).** The selected unit's row must pre-exist or every nomenclator INSERT
fails. Is populating `Unitati` in scope for this tool, or a precondition the operator arranges?
This is the same question as §11 Q6 (creating the MariaDB user and grants for a new DC) —
both are "who prepares the database before the transfer runs".

**Q6 (unchanged) MariaDB client library** — `MySqlConnector` or `MySql.Data`? Recommendation
stands: `MySqlConnector`, MIT, no licence question, nothing in the solution talks to MariaDB
today.

**Answered and closed:** §11 Q1 (`cai.DC` verbatim, §2), Q3 (the year — D1), Q5
(`AVACONT_SURSA` carries tables only — no views, triggers or routines in the dump).

---

## 11. What is still unverified

1. **The schema file is a snapshot, not a live server.** `MariaDB_Schema/000_DEMO.sql` is
   dated 22.08 and carries no data — the `AUTO_INCREMENT` marks in F6 are the only evidence of
   how full the tables are, and they are a high-water mark, not a row count. A table could have
   been emptied since and still report `AUTO_INCREMENT = 1497`. Confirm against the live server
   before acting on Q1.
2. **`AVACONT_COMUN` has not been seen at all.** Five foreign keys point into it (§3.1) and
   its contents decide whether a classification row is accepted. Not in `MariaDB_Schema/`.
3. **`Rectificari` has never been seen with rows** — 0 in Access, 0 on the target.
4. **One unit's `baza2026.accdb` is available** (`Energetic ISJ`, IdUnitate 76, `SURSA` `01A`)
   out of the 13 `cai` lists. The `02E` units will exercise `ClasificatiiV` / `RectificariV`,
   which are out of scope for this slice (D5) but not out of existence.
5. **One DC** (`000_DEMO`) in the registry, so "`cai.DC` verbatim" is confirmed but not
   stress-tested.

---

## 12. Not a finding, but load-bearing: no password is needed

`PLAN_MigratorDirect.md` §1/§3 and `KBOT_STATUS.md` slice 0044 both state the files are
encrypted ("fișierele chiar sunt criptate cu `andreI` … deci niciun cititor pur-Python nu le
deschide"). **All three files in `Avacont/` opened with `Microsoft.ACE.OLEDB.16.0` and no
password.**

The spike still builds the connection string with `OleDbConnectionStringBuilder` and still
supports `Jet OLEDB:Database Password`, and the form must keep its password fields — the
operator's own copies may be protected. But the "decrypt by hand in Access first" step of
slice 0044 does not apply to these files.

This pass therefore also discharges the proof §12.1 asked for: **ACE 16.0 is registered 64-bit
on this machine, an x64 `net8.0` build opens all three `.accdb` files, and the password
property is wired.** `Microsoft.ACE.OLEDB.12.0` is registered too, so the fallback has
something to fall back to.
