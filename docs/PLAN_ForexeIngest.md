# PLAN — porting the FOREXE ingest pipeline from Access VBA to the Flask API

**Slice: 0048** (confirmed against `KBOT_STATUS.md`, which recorded *Next free slice number: 0048*).
Sub-slices follow the numbered steps of §7 (`SLICE-0048-01-…` and so on).

**What this replaces.** Today `ForexeController.DownloadNodeAsync` runs the *Prelucrare Completă*
workflow and saves the result to `<AppDir>\WorkflowResults\*.json` and nothing else. There is no
ingest. Everything the Access VBA did with that result — parsing, deduplicating, deriving
recepții / rezervări / plăți, writing them — has to exist again, this time on the server.

**Where it runs — decided by the operator: all of it in Flask.** One POST per angajament carrying
the raw FOREXE result; the server does hashing, deduplication, key allocation and all writes
inside one transaction. The client keeps the local JSON dump as an evidence trail and adds the
POST. No SQLite scratch database, no `tmpFX_*` equivalents anywhere.

**Comment style — explicit operator request.** The operator is new to Python. Every non-obvious
line of the new Python gets a comment in English saying what it does, not just why. Assume the
reader knows SQL and VB.NET but not Python idioms. Comment the idioms (list comprehensions,
`with` blocks, `%s` placeholders, `executemany`, decorators, tuples) the first time each appears
in a file. Verbosity here is wanted, not tolerated.

---

## 0. Mandatory reads before touching anything

`CODE_WORKFLOW.md` §1 applies in full. Everything below was written from project-knowledge search
and from `000_DEMO.sql`, **not** from an editor. Every "the VBA does X" claim must be re-checked
against the real file before it is ported.

**Governance**
- `docs/worklog/KBOT_STATUS.md`, `docs/worklog/CODE_WORKFLOW.md`, `docs/MAPARE_ACCESS_MARIADB.md`

**Access VBA — the source being ported** (`FX_System_Export/`)
- `MODULES/mdl_FX_Tasks_Receive_DWN.md` — the orchestrator and steps 1, 2, 4, 5
- `MODULES/mdl_FX_Istoric.md` — steps 3a–3e and `FX_Istoric_Populeaza_Receptii`
- `MODULES/mdl_FX_Rezervari.md` — `FX_Istoric_Populeaza_Rezervari`, `FX_Rezervari_Asocieaza_IDREV`
- `MODULES/mdl_FX_Receptii.md` — `FX_Receptii_Proceseaza`, `Receptii_Prelucrare`,
  `TMP_Asociaza_Receptii_Istoric`, `AsociazaFinal`, `FX_CalculeazaDIF_Receptii_Tmp`,
  `FX_Incarca_Receptii_Temporare`, `FX_Salveaza_Receptii_Temporare`
- `MODULES/mdl_FX_Plati.md` — `FX_Istoric_Populeaza_Plati_Incasari`
- `MODULES/mdl_FX_Helpers.md` — all hash and parsing helpers
- `QUERIES/qFX_ISTORIC_REZ_INIT.md`, `QUERIES/qFX_ISTORIC_REZ.md`,
  `QUERIES/qFX_ISTORIC_REZ_UPDATE_IDREV.md`

**Python — the house shape**
- `routes/forexe/__init__.py` (blueprint + import order), `routes/forexe/angajamente.py` (the only
  existing write route — copy its shape), `routes/forexe/istoric.py` and `routes/forexe/receptii.py`
  (they read what this route writes; the contract must match)
- `routes/auth/guard.py`, `routes/auth/session_store.py` — **read these to find out where a route
  gets `db_name` and `IdUnitate`. Do not invent a new parameter.**
- `utils/database.py`, `routes/admin.py` (`_validate_db_name`)

**VB.NET**
- `src/KBot.App/Forexe/ForexeController.vb`, `src/KBot.App/Forexe/WorkflowResultStore.vb`
- `src/KBot.Api/ApiClient.vb`, `IApiClient.vb`
- `src/KBot.Forexe/WorkflowCatalog.vb`
- `src/KBot.Forexe/Workflows/adlop - Prelucrare Completa.wfl` and `… Reverse.wfl`

---

## 1. What is being ported

The operator named five entry points. `FX_Angajament_Prelucrare` is a seven-step orchestrator and
the real call graph is about twenty functions. Full list, so nothing is discovered late:

| VBA function | Module | Becomes |
|---|---|---|
| `FX_Angajament_Prelucrare` | Receive_DWN | the route handler / orchestrator |
| `Reset_Are`, `FX_Angajament_Are` | Receive_DWN | the counters in the JSON response |
| `Prelucrare_Angajament` | Receive_DWN | step 1 |
| `Prelucrare_Indicatori` / `FX_Indicatori_Prelucrare` | Receive_DWN | step 2 |
| `FX_Istoric_Prelucrare` | mdl_FX_Istoric | step 3 (wrapper) |
| `FX_Istoric_Populeaza_Istoric` | mdl_FX_Istoric | step 3a |
| `FX_Istoric_Prelucreaza_Observatii` | mdl_FX_Istoric | step 3b |
| `FX_Istoric_Populeaza_Rezervari` (×2) | mdl_FX_Rezervari | step 3c, 3d |
| `FX_Rezervari_Asocieaza_IDREV` | mdl_FX_Rezervari | step 3e |
| `Prelucrare_Receptii` | Receive_DWN | step 4 (wrapper) |
| `FX_Receptii_Proceseaza` | mdl_FX_Receptii | step 4 |
| `FX_Goleste_Receptii_Temporare`, `FX_Incarca_Receptii_Temporare` | Helpers / Receptii | **dropped** — see D3 |
| `FX_Istoric_Populeaza_Receptii` | mdl_FX_Istoric | step 4a |
| `Receptii_Prelucrare` | mdl_FX_Receptii | step 4b |
| `TMP_Asociaza_Receptii_Istoric`, `AsociazaFinal` | mdl_FX_Receptii | step 4c |
| `FX_CalculeazaDIF_Receptii_Tmp` | mdl_FX_Receptii | step 4d |
| `Prelucrare_Plati_Incasari`, `FX_Istoric_Populeaza_Plati_Incasari` | Receive_DWN / Plati | step 5 |
| `FX_Salveaza_Receptii_Temporare` | mdl_FX_Receptii | **absorbed** into steps 4b–4d — see D3 |
| `FX_Istoric_Actualizeaza_Rezolvat` | mdl_FX_Istoric | step 7 |
| `FX_Indicatori_Actualizare_Extrase` | Receive_DWN | step 8 |
| `frmFX_DUBII` (+ 4 subforms) | Forms | **out of scope** — see D4 |
| `FX_Angajament_Resetare_Valori_Forexe` | Helpers | **out of scope** — the transaction rollback replaces its role here |

Helpers to port alongside: `GetHashForRow_Istoric`, `GetHashFromDict`, `GetHashForRow_Receptie`,
`FX_Receptii_H_GetHashIdent`, `FX_Receptii_Istoric_GetIndent`, `FX_Receptii_NormalizeSSI`,
`FX_Receptii_NumKey`, `FX_ExtractCodIndicator`, `ExtractObsValue`, `ExtractTextBetween`,
`ParseDataZZLLAAAA`, `ParseEnglishDate`, `ParseAmount`, `FX_DicInd_Ordine`.

---

## 2. Locked decisions

Settled by the operator or by reading the schema. Do not re-open; if one turns out to be wrong,
stop and report rather than working around it.

**D1 — everything runs in Flask.** One endpoint, one transaction. The client sends the FOREXE
result and receives counters.

> **AMENDED in slice 0048-03 (correction C3 of `docs/FUNDAMENT_Asociere_Receptii.md`).** The
> "one round trip" half is withdrawn. The ingest is **two-phase**: a *proposal* that runs the whole
> pipeline and then rolls it back unconditionally, and a *commit* that carries the operator's
> association decisions. Everything else about D1 stands — it is still one endpoint, still one
> transaction per call, still all in Flask.
>
> The reason is not a change of taste. FOREXE history never names the reception (F4), so the
> automatic pass can only place the LAST snapshot of a chain (F9); the rest — roughly
> *(snapshots − receptions)* per angajament (F10) — must be placed by a person. A wrong placement is
> silent and permanent (F12). Nothing may reach the database before the operator has answered.
> `PLAN MIGRARE` §3.2 already described this shape; D1 was the deviation.

**D2 — Access is gone.** No parallel writer. K-BOT is the only thing that writes these tables, so
the ingest does not have to stay byte-compatible with a second implementation. It still has to be
compatible with the **data already migrated out of Access** — see D9.

**D3 — no temp tables.** `tmpFX_Receptii_R/_H/_/_RHR`, `tmpFX_Plati`, `tmpFX_Receptii_Plati` do
not exist on MariaDB and are not being created. In Access the "load backend → temp → compare →
save back" round trip existed because Access was a disconnected front end editing a linked
backend. Inside one server transaction, incoming rows are compared against the live tables
directly. Consequence: `FX_Incarca_Receptii_Temporare` and `FX_Salveaza_Receptii_Temporare`
disappear as functions; their **comparison and update rules** move into steps 4b–4d and must be
ported faithfully — read them before writing step 4.

> **AMENDED in slice 0048-03 (C3).** The surviving half — "incoming rows are compared against the
> live tables inside the transaction" — is what the ported steps do, and it is unchanged. What
> changed is that the transaction may end in `rollback()` rather than `commit()`: the proposal
> phase writes to the live tables exactly as the commit phase does, and then undoes it. There are
> still no temp tables.

**D4 — `frmFX_DUBII` is a later slice.** In Access, a reception header that could not be matched
to a reception (`TmpIDRecR IS NULL`) opened a modal dialog, and refusing to resolve it wiped the
angajament. Nothing is wiped here.
*(The operator wrote "IDRH null" — `IDRH` is the primary key of `FX_Receptii_H`; the link column
that stays empty is `IDRR`.)*

> **AMENDED in slice 0048-03 (correction C4).** "The header is written with `IDRR` left NULL and
> the ingest continues" no longer describes what happens. Under the two-phase contract nothing is
> committed until the operator has resolved **every** snapshot, and a missing decision is a `400`,
> not a default — silence must not be interpretable as a choice.
>
> One narrow case still commits with `IDRR` NULL, and it is deliberate: the `ignorat` action, for a
> save that recorded no change (F17). Such a snapshot gets `Sters = 1` and stays unattached.
> Ignoring is lossless; forcing it onto a reception injects a false value into that reception's
> timeline. The `antete_neasociate` field of the response is gone with the rest of D4 — the
> proposal's `instantanee` array replaces it, and it carries *every* unplaced snapshot, not only
> the ones this run produced (D-F).
>
> The association form itself is slice **0048-04**, and D-A forbids a build where the ingest can
> produce unresolved snapshots with nowhere to resolve them — which is why 0048-03 deliberately
> does **not** wire the coordinator into `DownloadNodeAsync`.

**D5 — SPLIT in slice 0048-03. Read both halves; the original decision was half wrong.**

> **D5a — `FX_ORD_TBL_REC` is NOT a relic (correction C1).** The operator withdrew this on
> 26.08.2026: *«PLAN_ForexeIngest.md IS WRONG. The FX_ORD_TBL_REC IS NOT A RELIC. I WAS WRONG! it
> needs to travel through migration and it needs to be used.»* It links a payment to the
> ordonanțare line that consumed it. It **migrates normally** and it **will be used**. It has two
> real foreign keys (`IDORDTBLP` ▸ `FX_ORD_TBL`, `IdPlataFX` ▸ `FX_Plati`), so both parents are
> written before it. This pipeline still does not write it — ORD is a later slice — but "relic" was
> wrong and the migrator now carries it.
>
> **D5b — `FX_Receptii_Plati` IS dead, and more so than D5 said (correction C2).** *«NOT used
> anymore. it contains no data anymore. Excluded completely from migration.»* It was an early
> attempt to join a reception to a payment directly, made before the flow was understood. It is
> empty and is now excluded from migration entirely — it was previously *in* the migrator's table
> list, which was the actual mistake. `FX_ORD_TBL_REC.IDRP` pointed at it and is dead too: the
> column stays in the schema, unmapped and unwritten, carrying 0 on every sample row and no FK
> constraint on MariaDB.
>
> Steps H, I and J of `FX_Salveaza_Receptii_Temporare` are still dropped entirely — that half of
> D5 was right.

**D6 — keys become `AUTO_INCREMENT`, as a prerequisite, not as part of this work.** See §3.

**D7 — `IdClsf` on these tables stays the Access id.** `KBOT_STATUS.md` (slice 0047) settles it:
`FX_Istoric`, `FX_Indicatori`, `FX_Rezervari`, `FX_Plati`, `FX_Receptii` carry `IdClsf` =
`Clasificatii.IdClsfAcc`, not the MariaDB `IDClsf`, and the read routes depend on that. New rows
written by this pipeline **keep the same convention**. Where a classification has to be resolved
from scratch (step 2), look the row up in `Clasificatii` and write its **`IdClsfAcc`** into
`FX_Indicatori.IdClsf`. Writing `IDClsf` there would silently empty the Istoric and Recepții
filter menus — it would not raise anything.

**D8 — one database is one unit.** No `IdUnitate` in the request. The columns that still carry it
(`FX_Indicatori.IdUnitate`, `FX_Receptii.IdUnitate`, `FX_Receptii_RHR.IdUnitate`,
`FX_Plati.IdUnitate`) are filled from the same source the existing read routes use. Find that
source in `guard.py` / `session_store.py` and reuse it.

**D9 — deduplicate on the natural key, not on the hash string.** See §8. This is the single
riskiest point in the plan and the decision is deliberate.

**D10 — one transaction per angajament.** Any failure rolls the whole thing back and returns a
non-200 with a reason. Nothing is written half way. No `try: … except: pass` anywhere — house rule.

**D11 — the two workflows must return the same structure.** `TipReceptie` and `CodIndicator` are
restored to the `collectFields` list of `adlop - Prelucrare Completa.wfl` (they are commented out
there and present in the Reverse file). Confirmed by the operator. See §5.3.

**D12 — the response is counters, not data.** The client refreshes its views through the existing
read routes. The ingest does not return rows.

**D13 — re-sending the same payload is a no-op.** Idempotence is a tested property, not a hope.
See §12.

**D14 — `Prelucrare_Indicatori` is in scope.** It was not in the operator's list of five, but it
is step 2 of the orchestrator and both Istoric and Plăți resolve `CodAI` against `FX_Indicatori`,
which has a foreign key. Without it the pipeline cannot write anything.

**D15 — `AVACONT_SURSA` never gets the `ALTER`, and `schema_sync` ignores the seven columns.**
See §3.1 and §3.2. Both halves are the operator's decision and neither is open.

**D16 — `IdUnitate` and `CodProgram` are reached *through* the classification, not looked up
independently.** See §5.2a. Operator invariant, stated and relied upon: **within one database,
`(SS, ClsfSal)` identifies exactly one `IdUnitate`.**

---

## 3. Prerequisite — `AUTO_INCREMENT` on seven primary keys

**This is a separate task, done before the ingest can be finished, in `KBot.Migrator`.**

Seven primary keys are plain `int(11) NOT NULL` today, verified in `000_DEMO.sql`:

| Table | Key |
|---|---|
| `FX_Istoric` | `ID` |
| `FX_Receptii_R` | `IDRR` |
| `FX_Receptii_H` | `IDRH` |
| `FX_Receptii` | `IDR` |
| `FX_Receptii_RHR` | `IDRHR` |
| `FX_Plati` | `IdPlataFX` |
| `FX_Rezervari` | `IDRZ` |

They were not auto-increment in Access because Access needed to be able to repopulate a table after
a dropped connection, and the migration writes the Access ids verbatim — both correct reasons, both
finished once the data is in.

**Add a final step to `MigratorForm`**, run per database **after** the transfer of that database
has completed and verified:

```sql
ALTER TABLE FX_Istoric      MODIFY `ID`         INT(11) NOT NULL AUTO_INCREMENT;
ALTER TABLE FX_Receptii_R   MODIFY `IDRR`       INT(11) NOT NULL AUTO_INCREMENT;
ALTER TABLE FX_Receptii_H   MODIFY `IDRH`       INT(11) NOT NULL AUTO_INCREMENT;
ALTER TABLE FX_Receptii     MODIFY `IDR`        INT(11) NOT NULL AUTO_INCREMENT;
ALTER TABLE FX_Receptii_RHR MODIFY `IDRHR`      INT(11) NOT NULL AUTO_INCREMENT;
ALTER TABLE FX_Plati        MODIFY `IdPlataFX`  INT(11) NOT NULL AUTO_INCREMENT;
ALTER TABLE FX_Rezervari    MODIFY `IDRZ`       INT(11) NOT NULL AUTO_INCREMENT;
```

### 3.1 `AVACONT_SURSA` must NOT receive this change

**The reference schema keeps the seven columns as plain `INT NOT NULL`. Never alter them there.**

New unit databases are created from `AVACONT_SURSA` and are migrated *afterwards*. If the columns
were already `AUTO_INCREMENT` at creation time, then during that migration a row arriving with a
missing, NULL or zero id would stop being an error and become a **silently fabricated key** —
MariaDB simply assigns the next number. On a plain `INT NOT NULL` column the same row raises and
the run stops. That guard is the reason the columns are plain, and it must survive for every
database that has not yet been migrated.

The order is therefore always the same, per database: **create from `AVACONT_SURSA` → migrate →
verify → `ALTER`.** The `ALTER` is the last thing that happens to a database, once, and it never
happens to the reference.

### 3.2 `schema_sync` leaves these seven columns alone — decided

After this, every migrated database differs from `AVACONT_SURSA` on exactly these seven columns,
by design and forever.

**Operator decision (locked): `schema_sync` ignores them.** The seven columns go on a named
exemption list in `routes/schema_sync/`, and the sync neither reports a difference on them nor
rewrites them. No re-application of the `ALTER`, no tracking of which databases are already
migrated — the sync simply does not look.

This is safe because **nothing edits these columns.** They are primary keys whose only property
that ever changes is the one §3 sets, once, at the end of a migration.

Two things must be true in the code, and both belong in the worklog:

1. The exemption is a **named list of `(table, column)` pairs**, written out in full, with a
   comment pointing at this section — not a rule like "skip anything auto-increment", which would
   quietly swallow a real difference somewhere else.
2. It covers the **whole column**, not just the `AUTO_INCREMENT` attribute. The consequence is
   worth stating plainly: if anyone ever deliberately changes one of these seven columns in
   `AVACONT_SURSA` — a type change, a width change — it will **not** propagate, and the
   divergence will not be reported. The exemption list is the place someone will have to look.
   That is an accepted cost, not an oversight.

The seven pairs:

```
FX_Istoric.ID           FX_Receptii_R.IDRR      FX_Receptii_H.IDRH    FX_Receptii.IDR
FX_Receptii_RHR.IDRHR   FX_Plati.IdPlataFX      FX_Rezervari.IDRZ
```

### 3.3 Notes for the migrator's worklog

- InnoDB sets the counter from `MAX(key) + 1` when the column becomes auto-increment. **Verify it
  once**, on the first database, with `SHOW TABLE STATUS` — do not code around it, but do not
  assume it either.
- Report per table: rows, `MAX(key)` before, `AUTO_INCREMENT` value after.
- The `ALTER` must refuse to run on a database whose transfer has not completed and verified.
  Running it early is the exact failure this section exists to prevent.

**Until this lands**, the ingest cannot insert. Do not build a MAX+1 fallback into the Python — a
fallback that exists will be used, and then two ways of allocating keys have to be kept correct
forever. If the ingest has to be exercised before the migrator step exists, apply the `ALTER`
statements by hand on the test database and say so in the worklog.

`NrCrt` on `FX_Receptii_H` and `NRCRT` on `FX_Receptii_R` are **not** keys — they are ordinals
scoped to a `CodAngajament` and stay MAX+1, exactly as the VBA does with `DMax`.

---

## 4. Order of writes

Foreign keys constrain the order. Read from `000_DEMO.sql`, not assumed:

```
FX_Angajamente                         (no FK)
  └─ FX_Indicatori        FK CodAngajament
       └─ FX_Istoric      FK CodAngajament, FK CodAI  → FX_Indicatori
            ├─ FX_Receptii_R           FK CodAngajament
            │    └─ FX_Receptii_H      FK IDRR, FK IDH → FX_Istoric, FK CodAngajament
            │         └─ FX_Receptii   FK IDRH, FK IDH, FK CodAI
            ├─ FX_Receptii_RHR         FK CodAI
            ├─ FX_Plati                FK IDH, FK CodAI
            └─ FX_Rezervari            FK IDH, FK CodAI, FK IDREV → FX_DDF_REV
```

Two traps:

1. **`FX_Istoric.CodAI` has a foreign key into `FX_Indicatori`.** A history row whose indicator
   does not exist yet cannot be written with that `CodAI` set. This is why indicators come before
   history in the VBA, and the order is not negotiable. `CodAI` is nullable, so a row genuinely
   without an indicator travels with NULL — but a *missing* indicator is an error, not a NULL.
   Raise. (House rule: no silent fallback.)
2. **`FX_Rezervari.IDREV` has a foreign key into `FX_DDF_REV`.** The VBA takes `IDREV` from the
   parsed Observații (`REV:x`). If that revision does not exist in `FX_DDF_REV`, the insert fails
   with error 1452 and takes the whole transaction with it. Decide explicitly: write NULL and
   record a warning, or raise. **Recommendation: write NULL, count it, return it as a warning** —
   `FX_Rezervari_Asocieaza_IDREV` (step 3e) exists precisely to fill that link later.

---

## 5. Input contract — what actually arrives

**Verified against a real payload** (`NOVA_WATER_SC35_resend.json`, angajament `AAB37CNBK95`,
2 indicators, 6 receptions, 44 history rows) and against the executor source
(`WorkflowExecutor.Actions.vb`, `…ForEachVar.vb`). This section is fact, not proposal.

### 5.1 Scalars

| Key | Example | Notes |
|---|---|---|
| `CodAngajament` | `AAB37CNBK95` | |
| `DataAngajament` | `10/02/2026` | `dd/MM/yyyy` |
| `DataInceputDerulare` | `10/02/2026` | may be absent (guarded in the `.wfl`) |
| `UltimaModificare` | `10/02/2026 22:46:36` | may be absent |
| `DescriereAngajament` | `2026 - NOVA WATER` | |
| `StareAngajament` | `În derulare` | real diacritics |

The VBA reads `DescriereAngajament` with a fallback to `Descriere`, and — this is a **bug in the
VBA** — reads `StareAngajament` but then assigns `cTask.ExtraObject("Stare")` in both branches.
Port the intent (`StareAngajament`), not the defect. Say so in the worklog.

### 5.2 `TabelIndicatori_results` — one row per indicator

Columns: `Indicator_ang`, `Program`, `Obiectiv`, `Proiect`, `Sector_Sursa_Indicator`,
`Limita_credit_angajament`, `Total_credit_angajament`, `Credit_bugetar`, `Angajament_legal`,
`Credit_bugetar_rezervat_definitiv_an_curent`, `Credit_bugetar_rezervat_definitiv_ani_urmatori`,
`Col_12`, and a **nested** `BugetIndicator` array (the per-year budget grid).

`Sector_Sursa_Indicator` looks like `02E- 65. 03. 01. 20. 03. 01`. The sibling function
`Angajament_Incarcat_Prelucrare_Initiala` shows the rule: split on `-`, remove spaces, split on
`.`, zero-pad each part to 2 (`Format(x, "00")`), join. The `02E` prefix is the sector+sursa. On
MariaDB, `Clasificatii` has generated columns `SS` (= `Sector` + `Sursa`, e.g. `02A` / `02E`) and
`ClsfSal` (the code with the dots removed). **So the lookup is `(SS, ClsfSal)` → `Clasificatii`,
and the value written into `FX_Indicatori.IdClsf` is that row's `IdClsfAcc` (D7).**

This mapping is **inferred from a sibling function** — port `Prelucrare_Indicatori` from its own
source and use this only as a cross-check. A lookup that misses is an error; raise.

### 5.2a Resolving `IdClsf`, `IdUnitate` and `CodProgram` — one lookup, in this order (D16)

`IdUnitate` is **not** a constant for a run and must not be read from `Unitati` on its own. The
sample payload proves it: `AAB37CNBK95` carries indicators under both `02E` and `02A`, two
different `SursaSector` values inside one angajament. And `SursaSector → IdUnitate` is
many-to-one (`MAPARE_NOMENCLATOARE.md`: `cai` lists 13 `IdUnitate` for DC `000_DEMO` across three
`SURSA` values), so it is not a lookup key either.

In Access this never arose — each unit had its own `baza<year>.accdb`, so the `UNIT` query
returned one row and `globCodProgram` was unambiguous. That file boundary does not exist on
MariaDB, where one database holds a set of units.

**The order, per indicator:**

1. Build `SS` and `ClsfSal` from `Sector_Sursa_Indicator` (§5.2).
2. Look the pair up in `Clasificatii` **for this database**. This is needed for `IdClsf` anyway.
3. `FX_Indicatori.IdClsf` ← the matched row's **`IdClsfAcc`** (D7).
4. `FX_Indicatori.IdUnitate` ← the matched row's **`IdUnitate`**. The nomenclator is per-unit, so
   the row already carries the answer; nothing else has to be consulted.
5. `CodProgram` ← `Unitati.CodProgram` **keyed on that `IdUnitate`**. This is the value the VBA
   held in the global `globCodProgram` and writes into `FX_Plati.Program` (step 5).

**The invariant this rests on, stated by the operator:** within one database, `(SS, ClsfSal)`
identifies exactly one `IdUnitate`.

**So the query must assert it, not assume it.** Do **not** use `LIMIT 1` here — that is the
pattern the *read* routes use for display, where picking either duplicate is harmless. Here a
wrong pick attaches an indicator to the wrong subunit, and nothing surfaces until someone reads a
report months later. Instead:

- Select all matching rows for `(SS, ClsfSal)`.
- The nomenclator does contain real duplicates (there is no unique key on
  `(IdClsfAcc, IdUnitate)` — `MAPARE_NOMENCLATOARE.md` §3.2). Several matching rows are therefore
  **fine as long as they agree on `(IdClsfAcc, IdUnitate)`**; only `IDClsf` differs between them,
  and `IDClsf` is not written here.
- Rows that **disagree** on `IdClsfAcc` or `IdUnitate` break the invariant: raise, naming the
  angajament, the indicator, the `(SS, ClsfSal)` pair and every candidate `IdUnitate`.
- No match at all: raise, same detail.

Naming note (Rule 4): Access spells the column `SectorSursa`, MariaDB spells it `SursaSector`.
Neither is a typo; both are real.

### 5.3 `ListaReceptii_results` — one row per reception

Columns today: `Tip` (`Partial`), `Data` (`11/02/2026`), `Suma` (`510,00`), `DescriereReceptie`,
and a **nested** `Detaliu` array.

`Detaliu` columns: `Cod` (= the indicator, e.g. `AAB`), `Program`, `Proiect`, `Obiectiv`,
`Sector_Sursa_Indicator`, `Credit_bugetar_rezervat_definitiv`, `Valoare_nereceptionata`,
`Valoare`. These map onto `FX_Receptii_RHR`: `CreditBugetar`, `ValoareN`, `Valoare` — confirm
against `Receptii_Prelucrare` before relying on it.

**Missing, per D11:** `TipReceptie` and `CodIndicator`. They are commented out of `collectFields`
in the forward workflow and present in the Reverse one. `AsociazaFinal` and the whole
Final/Partial demotion rule run on `TipReceptie`. **Restore both fields in the forward `.wfl` so
the two workflows return identical structures**, and make the Python raise a clear 400 if a
reception row arrives without `TipReceptie` — do not guess it from `Tip`.

Note for whoever restores them: in the payload as it stands, `TipReceptie` and `CodIndicator`
survive only as **loose positional arrays** at the top level (six values each, in iteration
order), because the executor keeps every variable assignment in a list. Do not read them that way
— positional alignment is not a contract. `collectFields` is.

### 5.4 `TabelIstoric` — one row per history entry

Columns: `Timp` (`10/02/2026 22:45:23`), `Utilizator`, `Descriere`, `Observatii`. Base name, no
`_results` suffix. 44 rows in the sample.

### 5.5 Things that are *not* in the payload

`Detaliu` and `BugetIndicator` do **not** exist as standalone top-level variables —
`CleanupCollectedVariables` removes them after collection, and they survive only nested inside
their parent rows. `TabelIndicatori` and `ListaReceptii` (without `_results`) also arrive, holding
the flat scrape *before* enrichment. The VBA reads the `_results` variants; keep that. Ignore the
`R.*` variables entirely — they are loop leftovers.

### 5.6 Formats

- Money: Romanian — `819.500,00` (thousands `.`, decimal `,`). Also bare (`210`, `3.587`). Port
  `ParseAmount`; do not use `float(x)`.
- Dates in table cells: `dd/MM/yyyy` and `dd/MM/yyyy HH:mm:ss`. Port `ParseDataZZLLAAAA`.
- **Dates inside payment Observații are English:** `Feb 16, 2026 12:00:00 AM`. Port
  `ParseEnglishDate`. Parse with an explicit month-name table, **not** with the machine locale.
- Diacritics are real UTF-8 throughout (`În derulare`, `Valoare recepţii`). Responses use
  `ensure_ascii=False`, per house rule. Note `recepţii` in the sample uses U+0163 (t-cedilla), not
  U+021B (t-comma) — string comparisons against FOREXE text must not assume one form. Where the
  VBA compared such text, keep its exact comparison.

---

## 6. Endpoint contract

New file `routes/forexe/prelucrare.py`, registered on `forexe_bp` at the end of
`routes/forexe/__init__.py` (import order matters — same pattern as the other submodules).

```
POST /api/forexe/prelucrare
@require_session
```

Request — deliberately the same shape as `PrelucrareRezultat` so the client can serialize what it
already holds:

```json
{
  "cod": "AAB37CNBK95",
  "workflow": "adlop - Prelucrare Completa.wfl",
  "moment": "2026-08-25T10:12:00",
  "scalari": { "DataAngajament": "10/02/2026", "...": "..." },
  "tabele": {
    "TabelIndicatori_results": [ { "...": "..." } ],
    "ListaReceptii_results":   [ { "...": "..." } ],
    "TabelIstoric":            [ { "...": "..." } ]
  }
}
```

`db_name` is **not** in the body. It comes from the session, like every other `routes/forexe/`
route. A token cannot target a database other than the one it logged into.

Response 200:

```json
{
  "cod": "AAB37CNBK95",
  "are": { "Indicatori": true, "Istoric": true, "Rezervari": false,
           "Receptii": true, "ReceptiiH": true, "Plati": true, "Incasari": false },
  "scrise": { "FX_Indicatori": 2, "FX_Istoric": 44, "FX_Rezervari": 0,
              "FX_Receptii_R": 6, "FX_Receptii_H": 6, "FX_Receptii": 12,
              "FX_Receptii_RHR": 12, "FX_Plati": 11 },
  "sarite": { "FX_Istoric": 0, "FX_Plati": 0 },
  "antete_neasociate": [ { "IDRH": 812, "DataH": "2026-05-29T00:00:00", "Total": 460.0 } ],
  "avertismente": [ "FX_Rezervari: IDREV 137 nu există în FX_DDF_REV — legătura rămâne goală." ]
}
```

`are` is the port of `FX_Angajament_Are` — the seven flags the Access UI used to decide what to
refresh. `antete_neasociate` is D4. `avertismente` are operator-facing, so Romanian with real
diacritics.

Failure: 4xx/5xx with `{"error": "…"}`, transaction rolled back, nothing written. Match the
error-handling shape of `routes/forexe/pdf.py` — it is the most recent and the most careful.

---

## 7. The seven steps

Each step names the VBA function it comes from. **Port from that function's real source.** Where
this plan describes behaviour, treat it as a summary that may be wrong in detail.

### Step 0 — open the transaction

```python
conn = None
try:
    # get_kbot_connection opens a connection to ONE unit database. In MariaDB terms:
    # one database = one unit, so there is no unit parameter anywhere below.
    conn = get_kbot_connection(db_name)
    # start_transaction() turns off autocommit. From here nothing is visible to anyone
    # else until conn.commit(). If we raise instead, conn.rollback() undoes everything.
    conn.start_transaction()
    ... the seven steps ...
    conn.commit()
except Exception as e:
    if conn is not None:
        conn.rollback()          # undo every write of every step
    logger.error(f"[forexe.prelucrare] {e}", exc_info=True)
    raise                        # never swallow — house rule
finally:
    if conn is not None:
        conn.close()
```

### Step 1 — `FX_Angajamente` (`Prelucrare_Angajament`)

`INSERT … ON DUPLICATE KEY UPDATE` on `CodAngajament`. Fields from the scalars: `Descriere`,
`Stare`, and the two dates.

**To verify before writing:** the VBA writes `DataAng` and `DataDer`. MariaDB `FX_Angajamente` has
`DataCreare` and `DataDefinitivare` and nothing that obviously means "start of derulare". Read the
Access `FX_Angajamente` schema (`TABLES/FX_Angajamente.md`) and the VBA's own field names, and map
them for real. If `DataInceputDerulare` has no home, say so in the worklog rather than parking it
in the nearest date column.

Do not touch `DC`, `Preluat`, `Incarcat`, `Ascuns` on update — `angajamente.py` already sets `DC`
and `Preluat` on insert and deliberately leaves them alone on update. Same rule here.

### Step 2 — `FX_Indicatori` (`Prelucrare_Indicatori`)

Source: `TabelIndicatori_results`. Key: `CodAI` = `CodAngajament & "-" & Indicator_ang` (this
pattern is visible in `mdl_FX_Plati` and `mdl_FX_Istoric`; confirm in `Prelucrare_Indicatori`).

Per row: resolve the classification, the unit and the program in one pass — **§5.2a, in that
order**. Then write `IdClsf` = `IdClsfAcc` (D7), `IdUnitate`, `SS`, `NrCrt`, `IndicatorFX`, and
the money columns. Upsert on `CodAI`.

A classification that does not resolve, or that resolves to more than one `IdUnitate`, is a
**blocking error** (§5.2a). So is `IdClsfPY = 0` if that value ever appears — standing house rule,
never handled silently.

Sets `are.Indicatori`.

### Step 3 — history and reservations (`FX_Istoric_Prelucrare`)

**3a — `FX_Istoric_Populeaza_Istoric`.** For each incoming history row, decide whether it already
exists (§8). New rows are inserted with `CodAngajament`, `Descriere`, `Observatii`, `Utilizator`,
`HASH`, `DataFX` (parsed from `Timp` — note the VBA splits on the space and adds `TimeValue`
separately), and `rez_ord`.

`rez_ord` is the ordering trick and it is easy to get wrong. Read the source. In summary: a
multiplier of 0 / 100 / 1000 is chosen from the current `MAX(rez_ord)` for this angajament;
`Angajament nou` → 0; `Initial ->` → 100; `definitivare ->` → 1000; a row whose Observații start
with `RAND CONTRACT:` → the indicator's ordinal (`FX_DicInd_Ordine`) plus the multiplier.

Remember which rows were newly inserted — step 7 only marks **those**.

Returns false-but-not-an-error when nothing is new; the VBA then stops the whole of step 3.
Sets `are.Istoric`.

**3b — `FX_Istoric_Prelucreaza_Observatii`.** Runs over `Prelucrat = FALSE` rows for this
angajament and fills the derived columns from the free text: `CodIndicator`, `CodAI`, `IdClsf`,
`Clsf`, `TipRand`, `IdTrezor`, `Doc`, `IDREV`, and the seven `Val_*` columns — all reset to
0/NULL first, then set. `TipRand` is what steps 3c–3e and 5 filter on (`Rez_Initiala`,
`Rez_Definitiva`, `Rez_Influenta`, `PLATA_*`, recepție rows).

This function is pure text parsing and it is the most detail-dense thing in the pipeline. Port it
line by line. Do not restructure it.

**3c / 3d — `FX_Istoric_Populeaza_Rezervari`**, called twice: `Initiala=True` then `False`.

The two Access saved queries are the source and both are in the export:

- `qFX_ISTORIC_REZ_INIT` — `TipRand = 'Rez_Definitiva'`, `EInitiala = True`,
  `R_Valoare = Val_AngLeg`
- `qFX_ISTORIC_REZ` — `TipRand = 'Rez_Influenta'`, `R_Valoare = Val_Rezervare_Dif`,
  `EMicsorare = (dif < 0)`, `EMarire = (dif > 0)`, plus `R_Anterioara`

Both are `FX_Indicatori INNER JOIN FX_Istoric ON CodAI`, both filter
`FX_Istoric.ID NOT IN (SELECT IDH FROM FX_Rezervari)` — that `NOT IN` is what makes the step
idempotent, keep it — and both order by `CDate(Format(DataFX,"Short Date"))` then `Clsf`.

Two translation notes:

- `CDate(Format([DataFX],"Short Date"))` = truncate to the day. In MariaDB: `DATE(DataFX)`.
- Both join `ClasificatiiG` (an Access query) purely for the `Clsf` sort key. On MariaDB use
  `Clasificatii` with the **scalar-subquery + `LIMIT 1`** pattern the read routes already use —
  the nomenclator has real duplicates on `(IdClsfAcc, IdUnitate)` and a join would multiply rows.
  Do not invent a new join shape; copy the one in `routes/forexe/receptii.py`.

Sets `are.Rezervari`.

**3e — `FX_Rezervari_Asocieaza_IDREV`.** Two cases: rows that already carry an explicit `IDREV`
from the parser get `AreDDF = True` (and the matching `FX_DDF_REV` gets `Incarcat`); rows with an
empty `IDREV` are matched afterwards — see `qFX_ISTORIC_REZ_UPDATE_IDREV`, which orders by
`DataFX`. Read the full function; the fragment available in the export stops mid-way.

### Step 4 — recepții (`FX_Receptii_Proceseaza`)

The largest step, and the one that changes shape most (D3).

**4a — `FX_Istoric_Populeaza_Receptii`.** Walks the unprocessed history rows in `ID` order and
builds two things from them:

- a row whose Observații contain `(activ:true)` is a **header** → `FX_Receptii_H`
  (`IDH`, `NrCrt` = MAX+1 per angajament, `DataH` = `DataFX`, `Total` = `Val_Receptie`,
  `Descriere` = the text between `Receptie: ` and `,`);
- a row with `Val_Receptie <> 0` is a **line** → accumulated into a buffer, flushed into
  `FX_Receptii` when the next header appears. Each line carries `IDH`, `IdClsf`, `CodSSI`, `Clsf`,
  `IdUnitate`, `CodAI`, `CodIndicator`, `Data`, `Valoare`, `ValoareOrig`, `HASH`
  (`FX_Receptii_Istoric_GetIndent`) and `TipIntern` = `VECHI`/`NOU` depending on whether the
  indicator was already seen.

An indicator that is not in `FX_Indicatori` raises — keep that. Sets `are.ReceptiiH`,
`are.Receptii`.

**4b — `Receptii_Prelucrare`.** Compares the incoming `ListaReceptii_results` against what is
already stored, per the header hash (`FX_Receptii_H_GetHashIdent` over `CodAngajament`, `DataH`
formatted `yyyy-mm-dd`, `TipReceptie`, `DescriereReceptie`):

- hash present and the sum matches → skip
- hash present and the sum differs → update `FX_Receptii_R` / `FX_Receptii_RHR`, and demote any
  existing `Final` on that reception
- hash absent → insert `FX_Receptii_R` + `FX_Receptii_RHR` from the nested `Detaliu`

In Access this ran against temp tables; here it runs against the live tables inside the
transaction. The **rules** do not change; only the tables they read do. Read the real function.

**4c — `TMP_Asociaza_Receptii_Istoric` + `AsociazaFinal`.** Matches each header (`FX_Receptii_H`)
to a reception (`FX_Receptii_R`) and sets `IDRH.IDRR`. `AsociazaFinal` demotes every existing
`Final` on that reception to `Partial`, rewriting its `HASH` with the new type, and promotes the
current one.

**Per D4: a header that finds no reception keeps `IDRR = NULL` and is reported in
`antete_neasociate`. It is not deleted, and the run is not aborted.** The Access branch that
opened `frmFX_DUBII` and then called `FX_Angajament_Resetare_Valori_Forexe` does not exist here.

**4d — `FX_CalculeazaDIF_Receptii_Tmp`**, run per reception. Recomputes `DIFH`/`DIFHC` on the
headers and `DIF`/`DIFC` on the lines. Read the function; the DIF rules feed the Recepții view's
tooltip and getting them wrong is invisible until someone reads a total.

### Step 5 — plăți and încasări (`FX_Istoric_Populeaza_Plati_Incasari`)

Source: `FX_Istoric` rows with `Prelucrat = FALSE` and `TipRand LIKE 'PLATA_%'`.

From each Observație, using `ExtractObsValue`: `Rand:` → indicator, `document:` → `NrOP`,
`data:`…`valoare:` → the date, `valoare:` → the amount, `IdTrezor:` → `Referinta_TREZOR`.
Confirmed against the real payload, e.g.
`Plata: Rand: AAB, document: 38, data: Feb 16, 2026 12:00:00 AM valoare: 819, IdTrezor: TZ52198479598`.

Rules to keep exactly:

- **Deduplicate on `Referinta_TREZOR`**, seeded from the payments already stored for this
  angajament. The VBA builds that set up front and adds to it as it goes.
- Missing `Rand:` or `IdTrezor:` → skip the row and log it (this one *is* a skip, not an error —
  it is what the VBA does).
- An unparseable date → skip.
- `Tip` = `INCASARE` when the amount is negative, otherwise `PLATA`; those two set
  `are.Incasari` / `are.Plati`.
- `Preluat = True`. `Program` came from the VBA global `globCodProgram`; here it is
  `Unitati.CodProgram` for the indicator's own `IdUnitate`, reached through the classification
  (§5.2a). Since every payment already resolves its indicator in `FX_Indicatori`, take
  `IdUnitate` from that row rather than resolving the classification a second time.
- An indicator not found in `FX_Indicatori` raises.

### Step 6 — *(absorbed)*

`FX_Salveaza_Receptii_Temporare` has no separate existence under D3. Its steps B–G2 are the writes
inside 4b–4d; its steps H–J are dropped under D5.

### Step 7 — `FX_Istoric_Actualizeaza_Rezolvat`

Sets `Prelucrat = TRUE` on **exactly the history rows this run inserted** (the ones step 3a
tracked). Rows that already existed keep whatever `Prelucrat` they had — the VBA marks only rows
whose `IDH` was set during this pass, and that distinction is what keeps a re-download from
re-processing old history.

### Step 8 — `FX_Indicatori_Actualizare_Extrase`

Called unconditionally at the end. Read it before deciding where it belongs — if it touches
`FX_Extrase`, it may need to run outside the recepții/plăți part of the transaction. If it turns
out to be dead or UI-only, say so and drop it, explicitly, in the worklog.

---

## 8. Identity and deduplication — read this twice

The Access hash is, from `mdl_FX_Helpers`:

```
key = "Timp=" & Len(v) & ":" & v & "|" & "Utilizator=" … & "|" & "Descriere=" … & "|" & "Observatii=" …
hash = hex(SHA256(key))
```

`GetHashForRow_Istoric` uses that fixed four-field order; `GetHashFromDict` and
`GetHashForRow_Receptie` use the same `name=len:value|` shape over a dictionary, in insertion
order.

**The problem.** Rows already in MariaDB carry hashes computed by the Access `BCrypt` wrapper. Two
things about that string are unknown from the export: the text encoding it hashed (ANSI vs UTF-8 —
it matters the moment an Observație contains `ă` or `ț`), and whether `BytesToHex` produced upper
or lower case. If Python's hash differs by one byte, **every previously seen history row looks new**
and the whole angajament duplicates itself on the first re-download. That failure is silent and
it corrupts data.

**D9 — deduplicate on the natural key.** The hash encodes nothing but `Timp`, `Utilizator`,
`Descriere`, `Observatii`, and all four survive in the table (`DataFX`, `Utilizator`, `Descriere`,
`Observatii`). So:

```python
# Build the identity of an incoming history row from the four fields the Access
# hash was built from. `Timp` arrives as text ("10/02/2026 22:45:23"); DataFX is
# stored as a datetime. Comparing the parsed datetime to the stored datetime is
# exact — it avoids depending on the text formatting surviving a round trip.
def cheie_istoric(rand: dict) -> tuple:
    # A tuple is an immutable list. Python can use tuples as dictionary keys and
    # can put them in a set, which is how the "have I seen this row?" test below works.
    return (
        parse_data_zzllaaaa(rand["Timp"]),   # -> datetime
        (rand.get("Utilizator") or "").strip(),
        (rand.get("Descriere")  or "").strip(),
        (rand.get("Observatii") or "").strip(),
    )

# Read the rows already stored for this angajament ONCE, and put their identities
# in a set. `set` membership is a hash-table lookup, so this is fast even for
# thousands of rows — the VBA did FindFirst per row, which we do not need here.
cursor.execute(
    "SELECT DataFX, Utilizator, Descriere, Observatii "
    "FROM FX_Istoric WHERE CodAngajament = %s",   # %s is the placeholder; the driver
    (cod,),                                       # escapes the value. Never use f-strings here.
)
existente = {
    (d, (u or "").strip(), (de or "").strip(), (o or "").strip())
    for (d, u, de, o) in cursor.fetchall()        # this is a "set comprehension"
}
```

**And still write `HASH`**, computed the Access way, so the column stays populated and a future
comparison is possible. Document the exact recipe used, in the code.

**Required verification, before this is trusted** — a Python-side check against real migrated
data: for a database that has migrated history, recompute the hash for a sample of stored rows
from their own four columns and compare against the stored `HASH`. Report the match rate in the
worklog. If it is 100%, note that hash-based dedup would also have worked. If it is not, the
natural-key decision is vindicated and the reason is now known. **Do not skip this** — it is
cheap and it is the only way anyone will ever learn which encoding Access used.

The same reasoning applies to `FX_Receptii_H.HASH` and `FX_Receptii.HASH`: match on the underlying
fields, write the hash.

---

## 9. Client side (VB.NET)

Small. `ForexeController.DownloadNodeAsync` currently ends at
`_store.SalveazaNod(cod, PrelucrareRezultat)`. After that succeeds, add the POST.

- `IApiClient` / `ApiClient`: `Function TrimitePrelucrareAsync(rezultat As PrelucrareRezultat, ct As CancellationToken) As Task(Of PrelucrareRaspuns)`
- New POCO `PrelucrareRaspuns` in `KBot.Domain` mirroring §6.
- Wrap in the shell's `WithReauth(Of T)` — this is an authenticated call like any other.
- **Keep the local JSON dump.** It is the only evidence of what was sent when something goes wrong,
  and it costs nothing.
- On failure: the local file stays, the operator is told in Romanian, nothing is retried
  automatically. A resend command can come later.
- On success: refresh the affected views using the `are` flags — that is exactly what
  `FX_Angajament_Are` was for in Access.

And the `.wfl` change from D11: restore `TipReceptie,CodIndicator` to `collectFields` in
`adlop - Prelucrare Completa.wfl`. Diff the two files afterwards and confirm the only remaining
differences are the timeouts (15s vs 5s) and the history paging direction.

---

## 10. Tests

Python, host-only where they need a database (same skip pattern as `test_forexe_ddf.py`):

1. Each parsing helper against strings taken from the real payload — `ParseAmount("819.500,00")`,
   `ParseEnglishDate("Feb 16, 2026 12:00:00 AM")`, `ExtractObsValue` on the payment line quoted in
   §7 step 5, `FX_ExtractCodIndicator` on a `Rand contract:` line.
2. **Idempotence (D13):** post the sample payload twice; the second run writes zero rows and
   returns the same `are` flags. This is the single most valuable test in the set.
3. **Rollback:** force a failure in step 5 (an indicator that does not exist) and assert nothing
   from steps 1–4 survives.
4. **Unassociated header (D4):** a payload where a header cannot be matched → the header exists
   with `IDRR IS NULL`, the run returns 200, and `antete_neasociate` names it.
5. **Hash compatibility (§8):** the match-rate check against migrated data. A *finding* test — it
   reports, it does not fail.
6. `FX_Rezervari.IDREV` pointing at a non-existent revision → NULL plus a warning, not a 1452.
7. **The `(SS, ClsfSal)` invariant (§5.2a):** count the pairs in the live `Clasificatii` that match
   more than one row, and how many of those disagree on `IdUnitate`. A *finding* test — it reports,
   it does not fail. If any disagree, D16 is wrong and the run must stop and report.

.NET: the serialization of `PrelucrareRezultat` to the request body, and the parsing of the
response — the existing `ApiClient` test pattern covers both.

Report **real** counts. Do not copy numbers from another worklog.

---

## 11. Definition of done

1. `AUTO_INCREMENT` step exists in `KBot.Migrator`, runs only after a verified transfer, and is
   **not** applied to `AVACONT_SURSA` (§3.1). The seven-pair exemption list exists in
   `routes/schema_sync/` with the comment §3.2 asks for.
2. `routes/forexe/prelucrare.py` implements all seven steps in one transaction, comments
   throughout in English at the level §0 asks for.
3. `.wfl` fixed (D11) and the two files diffed.
4. Client sends and handles the response; local JSON dump retained.
5. Python suite green or cleanly skipped, 0 fail/error. .NET build 0 errors, 0 new warnings,
   `Option Strict On`. Full .NET suite green, real counts reported.
6. Worklog at `docs/worklog/SLICE-0048-…md` with the four mandatory sections: what changed and why
   · files touched · test results · anything left unverified or deferred.
7. `KBOT_STATUS.md` updated — slice row, next free number, open threads.
8. Code, worklog and STATUS committed together and pushed.
9. No swallowed exceptions introduced.

---

## 12. Explicitly out of scope

- `frmFX_DUBII` and its four subforms (D4) — the association UI is slice **0048-04**.
- `FX_Receptii_Plati` (D5b) — dead, and excluded from migration entirely.
- `FX_ORD_TBL_REC` is **no longer out of scope** (D5a / C1). It migrates; writing it belongs to the
  ORD slice, not to this one.
- `FX_Angajament_Resetare_Valori_Forexe` as an operator command.
- The upload direction — `mdl_FX_Tasks_Receive_UPL` (CreareAngajament, definitivare, încărcare
  rezervări). Different workflows, different plan.
- `FX_Extrase` / SNM — **EXCEPT the two statements of step 8** (`FX_Indicatori_Actualizare_Extrase`),
  amended 26.08.2026. Those two fill `FX_Extrase.CodAI` from `FX_Plati` on the treasury reference and
  are step 8 of this very pipeline: they were never optional, and D-G says so. They run unconditionally,
  in order, inside the same transaction as steps 1–7. Everything else about `FX_Extrase` — reading it,
  writing any other column, the SNM side — stays out of scope.
- Batch ingest of many angajamente in one call. One angajament, one POST.

## 13. Open items to carry into the worklog

1. **Settled (D16, §5.2a).** Both come through the classification. What remains is a *finding*:
   report how many `(SS, ClsfSal)` pairs matched more than one `Clasificatii` row on real data,
   and whether any of those disagreed on `IdUnitate` — i.e. whether the operator's invariant holds
   on the live nomenclator. A test that reports, not one that fails.
2. Whether `DataInceputDerulare` has a column at all (§7 step 1).
3. ~~Whether `FX_Indicatori_Actualizare_Extrase` is live or dead (§7 step 8).~~ **CLOSED
   26.08.2026.** It is live and it is ported (slice 0048-03-completare). Two statements, in order,
   unconditional, in-transaction; row counts reported under `FX_Extrase`; §12 amended above. The
   warning that used to say it had not run is deleted, along with the test that pinned it.
4. The hash match rate against migrated data (§8).
5. Whether InnoDB set the auto-increment counters correctly on the first database (§3.3).
6. `FX_Istoric.Val_Receptie_T` — present on MariaDB, not obviously written by any of the ported
   functions. Find out who writes it, or record that nobody does.

---

## Appendix — Python terms used above

For reading the code, not for the code itself.

| Term | What it is |
|---|---|
| `cursor` | the object you run SQL through, roughly a DAO `Recordset` opened for one statement |
| `%s` | the parameter placeholder. The driver escapes the value. Never build SQL with f-strings or `+` |
| `executemany` | run one INSERT with many rows in a single call — the fast path for bulk writes |
| `conn.start_transaction()` / `commit()` / `rollback()` | the same three things as DAO `BeginTrans` / `CommitTrans` / `Rollback` |
| `dict` | a `Scripting.Dictionary`. Same idea, built into the language |
| `set` | a dictionary with keys but no values — used for fast "have I seen this?" tests |
| `tuple` | an immutable list, written with parentheses. Can be a dictionary key; a list cannot |
| comprehension | `{f(x) for x in items}` builds a set (or list, or dict) in one expression — a `For Each` loop that produces a collection |
| decorator | `@require_session` above a function wraps it. Same effect as calling a guard on the first line, written above instead |
| blueprint | a group of Flask routes registered together. `forexe_bp` is one |
| `None` | `Nothing` / `Null` |
| `raise` | `Err.Raise`. A bare `raise` inside `except` re-throws the original error with its stack intact |
| f-string | `f"text {variable}"` — string interpolation. Fine for log messages, **never** for SQL |
