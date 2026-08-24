# MAPARE ACCESS ▸ MARIADB — mandatory read

> **Read this file in full before writing, planning or running any migration or transfer
> code.** It is the authoritative column mapping for the DDF and ORD families. Anything
> that contradicts it — an older plan, a worklog, a comment in the code — is wrong until
> this file is updated. If something needed is not in here, **stop and ask**; do not infer
> a mapping from column names.

Last updated: 22.08.2026 · MariaDB side verified against `000_DEMO.sql` · Access side
against `FX_System_Export/TABLES/*.md` and the VBA sync modules.

---

## 0. Cheile ORD — option (A), settled 22.08

`IDORDP`, `IDORDPARTP`, `IDORDTBLP`, `IDORDDOCP` are `AUTO_INCREMENT` primary keys on
MariaDB. **The Access id is written into them explicitly.**

`AUTO_INCREMENT` fires only when a column is *omitted* from the INSERT. Supplied, the value
lands verbatim — the same mechanism `FX_DDF.IDDF` and `FX_DDF_REV.IDREV` already depend on.
So `FX_ORD_PART.IDORD` (Access) is written into `IDORDP`, and every parent link is correct
with no id map, no `lastrowid`, no second pass.

Two consequences:

* **`ON DUPLICATE KEY UPDATE` works.** `IDORDP` is the real primary key and it carries a
  value, so a second run updates instead of duplicating.
* **The counter looks after itself.** InnoDB raises the `AUTO_INCREMENT` counter whenever an
  explicit value above it is inserted, so no `ALTER TABLE ... AUTO_INCREMENT =` is needed
  after the run. Worth *verifying* once on the first DC; not worth coding around.

The Access ids are not *stored* anywhere — the legacy `'ACCESS'` columns stay empty and are
being dropped (§5). Their values live on as the MariaDB keys.

Access's own mirror columns (`IDORDPartP`, `IDORDTBLP`, `IDORDDOCP`, `IDORDRECP`) are
**never sent**, whether populated or not. In `FX_ORD_DOC` they have visibly drifted from the
Access ids — `IDORDDOC` 17, 18, 19 against `IDORDDOCP` 40, 41, 42 — which is exactly why they
are not a usable source.

---

## 1. Rules

**Rule 1 — classifications.** MariaDB has **only `IdClsf`** on the `FX_*` tables, pointing at
`Clasificatii.IDClsf`. How it is filled changed on 22.08 and the old rule is now wrong:

In the nomenclator itself:

```
Access  Clasificatii.IdClsf   ▸   MariaDB  Clasificatii.IdClsfAcc
                                  MariaDB  Clasificatii.IDClsf  = AUTO_INCREMENT, assigned
```

MariaDB assigns `IDClsf` on a fresh database, so **the `IdClsfPY` values in the Access `FX_*`
rows are stale** — they were assigned by the *old* server and mean nothing on the new one.
**Do not use `IdClsfPY`. Anywhere.**

Resolve instead, through a map built during the run:

1. Transfer `Clasificatii` first. Keep `(IdClsfAcc, IdUnitate) ▸ IDClsf` for every row written,
   reading the assigned id back from `LAST_INSERT_ID()`.
2. For each `FX_*` row, look up its **Access `IdClsf` + `IdUnitate`** in that map and write the
   result into MariaDB `IdClsf`.
3. `Clasificatii_Buget` and `Clasificatii_Rectificari` resolve through the same map, and are
   therefore written after `Clasificatii` and before any `FX_*` table.

A lookup that misses is **blocking** on `FX_DDF_REV_SA.IdClsf` and `_SB.IdClsf` (`NOT NULL`
with a foreign key) and blocking by choice on `FX_ORD_TBL.IdClsf` — nullable there, but a
silently unclassified order line is worse than a refusal.

**Column-name matching is case-insensitive, always.** Access spells this column differently per
table (`IdClsfPY`, `IdClsfPy`) and the inconsistency is not predictable. Never compare column
names with `=`; lower-case both sides. This applies to every column, not just this one.

Access's `IdClsfPY` does **not** travel. `IdClsfAcc` on the `FX_*` tables is retired; on
`Clasificatii` it stays and is the key of the resolution map.

**Rule 2 — `CUAL` is `INT(11)` everywhere.** Access and MariaDB, `FX_DDF` and `FX_ORD`.
Fixed 22.08. No text handling, no conversion.

**Rule 3 — Access ids are not stored.** No `'ACCESS'` legacy column receives a value. See
§0 for what happens to their *values*.

**Rule 4 — case-insensitive everywhere.** Table names, column names, rename maps, tick
lists. Access is inconsistent (`IDORDPartP`, `IdClsfPy`, `CodAi` vs `CodAI`) and the
inconsistency is not a pattern that can be predicted.

---

## 2. Tables NOT migrated

Excluded by decision, 22.08 — do not write, do not check, do not include in the order:

- `FX_DDF_REV_ATT`
- `FX_DDF_REV_PRT`
- `FX_ORD_ATT`
- `FX_ORD_TBL_REC`
- `FX_ORD_PDF` (planned table, does not exist yet)

**`Clasificatii`, `Parteneri`, `Clasificatii_Buget` and `Clasificatii_Rectificari` are now
part of the transfer** (decision 22.08, reversing the earlier read-only status). They are fed
from the registry `.accdb` per DC, for every subunit `IdUnitate` that `CAI` lists, **before**
any `FX_*` table.

Their column mapping is **not yet written** — it needs the real Access schemas, which are read
in the discovery pass (`PLAN_MigratorDirect.md` §12). Until `docs/MAPARE_NOMENCLATOARE.md`
exists, do not map them. `Clasificatii.IdClsfAcc` must stay correct per `IdUnitate`, exactly as
it is on the existing databases.

`Unitati` remains read-only — nothing in this migration writes it.

`FX_ORD_DOC` **is** in scope (confirmed 22.08) — it was the table meant by "FX_ORD_PDF",
which does not exist.

Note on `FX_ORD_TBL_REC`: it is the link between payments and order lines
(`FX_Plati` ▸ `FX_ORD_TBL_REC` ▸ `FX_ORD_TBL`) and both its parents are migrated. Leaving it
out is deliberate; the link between a payment and the order line it settled does not survive
the migration.

---

## 3. DDF

### FX_DDF
Access PK `IDDF` · MariaDB PK **(`IDDF`, `CUAL`)** composite, `IDDF` `AUTO_INCREMENT`.

| Access | MariaDB | |
|---|---|---|
| IDDF | IDDF | PK part 1 |
| Cual | CUAL | `int(11) NOT NULL`, PK part 2 (Rule 2) |
| CodAngajament | CodAngajament | `NOT NULL`, FK → FX_Angajamente |
| DataCreare | DataCreare | `NOT NULL` |
| DataDef | DataDef | |
| ObiectDDF | ObiectDDF | `NOT NULL`, varchar(500) |
| Program | Program | |
| Comp | Comp | `NOT NULL` |
| Stare | Stare | |
| PartAng | PartAng | |
| DC | DC | |
| Incarcat / Preluat / Salarii / Buget / Manual | idem | |
| CodFiscal | CodFiscal | |
| NumePartener | NumePartener | |
| IdUnitate, IdPartener, CodPartener, SS, DTQ | — | do not travel (confirmed in `staging.py`) |
| — | IdSalarii, DataAdaugare, DataModificare | MariaDB-only, server-side |

The upsert matches on **(`IDDF`, `CUAL`)**. Both must be present.

### FX_DDF_REV
Access PK `IDREV` · MariaDB PK `IDREV` (`AUTO_INCREMENT`).

| Access | MariaDB | |
|---|---|---|
| IDREV | IDREV | PK |
| IDDF | IDDF | FK → FX_DDF |
| CodAngajament, Tip, NumarRev, DataRev | idem | |
| Desc_Scurta | Desc_Scurta | |
| Desc_Lunga, Desc_Lunga_ANSI | idem | Memo ▸ longtext |
| DC, Incarcat, Preluat, Semnatura | idem | |
| ArePDFDDF, CalePDFDDF, AreDDF, CaleDDF | — | Access-only |
| — | ESpeciala | ⚠ on MariaDB and in the VBA, absent from the Access export — **verify in the live .accdb** |
| — | DataAdugare, DataModificare | MariaDB-only (spelling: `DataAdugare`) |

### FX_DDF_REV_SA
Access PK `ID` (does not travel) · MariaDB PK `IdSecA` (`AUTO_INCREMENT`).
Access carries `IdSecA` separately — that is the MariaDB key.

| Access | MariaDB | |
|---|---|---|
| IdSecA | IdSecA | PK |
| IDDF | IDDF | FK → FX_DDF |
| IDREV | IDREV | FK → FX_DDF_REV |
| IdClsf (Access) | IdClsf | **Rule 1** — resolved via `(IdClsfAcc, IdUnitate)`; FK → Clasificatii |
| CodPartener | CodPartener | |
| IdPartener | IdPartener | FK → Parteneri |
| IdUnitate | IdUnitate | FK → Unitati |
| CodAngajament, CodIndicator | idem | |
| Clsf, ElementFund, ParametriiFund | idem | |
| ValPrec, ValCur, ValTot, PartInd, Ramane | idem | |
| SS | SS | `varchar(3)` on MariaDB; always exactly 3 characters in Access (confirmed 22.08) |
| ID, IdClsfPY | — | do not travel |

### FX_DDF_REV_SB
Access PK `ID` (does not travel) · MariaDB PK `IdSecB` (`AUTO_INCREMENT`). Same shape as SA.

| Access | MariaDB | |
|---|---|---|
| IdSecB | IdSecB | PK |
| IDDF, IDREV | idem | FKs |
| IdClsf (Access) | IdClsf | **Rule 1** — resolved via `(IdClsfAcc, IdUnitate)`; FK → Clasificatii |
| CodPartener, IdPartener, IdUnitate | idem | last two are FKs |
| CodSSI, CodAngajament, CodIndicator | idem | |
| CA_Anterior, Inf1, CA_Curent | idem | |
| CB_Anterior, Inf2, CB_Curent | idem | |
| SS | SS | `varchar(3)`; always exactly 3 characters (confirmed 22.08) |
| ID, IdClsfPY | — | do not travel |

---

## 4. ORD

Keys follow §0: the Access id is written into the MariaDB `AUTO_INCREMENT` primary key.

### FX_ORD
Access PK `IDORD` · MariaDB PK `IDORDP` (`AUTO_INCREMENT`).

| Access | MariaDB | |
|---|---|---|
| **IDORD** | **IDORDP** | PK — the Access value written explicitly |
| IDDF | IDDF | FK → FX_DDF |
| NrORD, DataORD, Comp | idem | |
| CUAL | CUAL | `int(11)` both sides (Rule 2) |
| CodAngajament | CodAngajament | `NOT NULL` |
| Incarcat, Preluat, Semnatura | idem | |
| IDORDP (Access) | — | 0 in Access, never sent |
| IDRR, IDRH | — | **being dropped from MariaDB** — do not map |
| — | IDORD (legacy varchar) | leave empty (Rule 3) |

Note the upsert: with `IDORDP` as the real PK and a value supplied,
`ON DUPLICATE KEY UPDATE` matches correctly. Under (B) it cannot.

### FX_ORD_PART
Access PK `IDORDPART` · MariaDB PK `IDORDPARTP` (`AUTO_INCREMENT`).

| Access | MariaDB | |
|---|---|---|
| **IDORDPART** | **IDORDPARTP** | PK |
| **IDORD** | **IDORDP** | FK → FX_ORD |
| Counter, DenBene, CodFiscal, ContIBAN, Banca | idem | |
| IDORDPartP | — | Access mirror (note the mixed case), never sent |
| IDORDP (Access) | — | 0 in Access |
| IdPartener, CodPartener | — | Access-only; MariaDB `FX_ORD_PART` has neither |
| — | IDORDPART (legacy), tmpID | leave empty |

### FX_ORD_TBL
Access PK `IDORDTBL` · MariaDB PK `IDORDTBLP` (`AUTO_INCREMENT`).
Six foreign keys, three of them into tables the migration never writes.

| Access | MariaDB | |
|---|---|---|
| **IDORDTBL** | **IDORDTBLP** | PK |
| **IDORD** | **IDORDP** | FK → FX_ORD |
| **IDORDPART** | **IDORDPARTP** | FK → FX_ORD_PART |
| IdClsf (Access) | IdClsf | **Rule 1** — resolved via `(IdClsfAcc, IdUnitate)`; FK → Clasificatii, `DEFAULT 0` |
| CodAI | CodAI | FK → FX_Indicatori · ⚠ Access spells it `CodAi` in `tmpFX_ORD_TBL` (Rule 4) |
| CodAngajament, CodIndicator, CodSSI | idem | |
| TotalReceptii, PlatiAnt, Valoare, Ramas | idem | |
| Explicatie | Explicatie | Memo ▸ longtext |
| IDRP | IDRP | FK → FX_Receptii_Plati |
| CodPartener | CodPartener | |
| IdPartener | IdPartener | FK → Parteneri |
| IdUnitate | IdUnitate | **`NOT NULL`**, FK → Unitati — must travel, cannot be nulled |
| IDORDTBLP (Access) | — | mirror, populated in Access but not used under (A) |
| IDORDP (Access), IdClsfPY, IDRR, IDORDT, IDRD | — | do not travel |

### FX_ORD_DOC
Access PK `IDORDDOC` · MariaDB PK `IDORDDOCP` (`AUTO_INCREMENT`).
Access schema confirmed 22.08 from `TABLES/FX_ORD_DOC.md`.

| Access | MariaDB | |
|---|---|---|
| **IDORDDOC** | **IDORDDOCP** | PK (§0) |
| **IDORD** | **IDORDP** | FK → FX_ORD |
| **IDORDPART** | **IDORDPARTP** | FK → FX_ORD_PART |
| DocJust | DocJust | Memo ▸ longtext |
| NumeDoc | NumeDoc | Text 255 ▸ varchar(255) — empty in the sample data |
| TipDoc | TipDoc | Text 255 ▸ varchar(255) — `"text"` throughout the sample |
| IDORDDOCP (Access) | — | mirror, never sent (§0) — visibly drifted: 17▸40, 18▸41, 19▸42 |
| IDORDP (Access) | — | 0 in Access |
| IDORDJ | — | Access-only, no target |
| — | IDORDDOC (legacy `'ACCESS'`) | leave empty; being dropped (§5) |


---

## 5. Schema changes still owed to AVACONT_SURSA

Decided but not yet in the schema — `000_DEMO.sql` of 22.08 still shows the old state:

| Change | Current state in `000_DEMO` |
|---|---|
| Drop `IdClsfAcc` from `FX_DDF_REV_SA`, `FX_DDF_REV_SB`, `FX_ORD_TBL` | present, now **nullable** on all three (SA/SB changed 22.08) — no longer blocks a run |
| `FX_ORD.CUAL` → `int(11)` | `varchar(255)` |
| Drop `FX_ORD.IDRR`, `FX_ORD.IDRH` | present |
| Drop the legacy `'ACCESS'` columns (`FX_ORD.IDORD`, `FX_ORD_PART.IDORDPART`, `FX_ORD_TBL.IDORDTBL`, `FX_ORD_DOC.IDORDDOC`) | present |

**`IdClsfAcc` no longer blocks anything.** It was `NOT NULL` with no default on SA and SB;
set to allow NULL on 22.08, so a run that stops sending it succeeds. One thing follows while the column is still
there:

* Migrated rows will carry `IdClsfAcc = NULL`, while rows the app creates keep filling it
  (`routes/ddf/staging.py` inserts and updates it explicitly). Anything that reads, joins or
  filters on `FX_DDF_REV_SA.IdClsfAcc` / `_SB` / `FX_ORD_TBL.IdClsfAcc` will behave
  differently for migrated data than for data entered afterwards. Retire those reads before
  or with the column.

---

## 6. Still unread

1. `TABLES/FX_ORD.md` — the only remaining reconstruction; the column list in §4 comes from
   `tmpFX_ORD` plus the VBA INSERT in `mdl_FX_ORD_SYNC_FROM_MARIADB`. Everything else in this
   file is read from source on both sides.
2. `FX_DDF_REV.ESpeciala` — present on MariaDB and written by the VBA, absent from the Access
   export. Check the live `.accdb`.

## 7. Open questions

None. All four decisions of 22.08 are folded in: option (A) for the ORD keys, `FX_ORD_DOC`
in scope, `FX_ORD_TBL_REC` out of scope, `SS` always three characters.
