# SLICE-0080-01 / 04 — Cheile clasificațiilor și DataDoc

Reunire documentară D001-25. 01 convertește IdClsf pe cele șapte tabele și DataDoc; 04 elimină copiile Access de pe încă cinci tabele, păstrând sursa unică Clasificatii.IdClsfAcc. Reviziile migratorului și excepțiile istorice sunt păstrate; cele două scripturi și condițiile lor sunt distincte. Unificarea documentară nu execută migrări și nu certifică bazele existente.

[0080-01](#slice-0080-01) · [0080-04](#slice-0080-04)

<a id="slice-0080-01"></a>

## SLICE 0080-01 — `IdClsf` = `Clasificatii.IDClsf` on seven FX_ tables, FX_Extrase.DataDoc → DATE (24.09.2026)

### Request

Operator, 24.09.2026, while specifying the Extrase view (0080-02):

- `FX_Extrase.DataDoc` is a string and must become a DATE. Existing values are MIXED, sometimes
  in the same database: `dd.MM.yyyy`, `dd/MM/yyyy`, `yyyy-mm-dd hh:mm:ss` (time dropped). A
  Python one-off does it on every existing database; the migrator must keep working, because
  old Access data is still being brought in for some clients.
- Seven FX_ tables held the ACCESS classification id in `IdClsf` while the ORD / DDF families
  hold the MariaDB key there. "Two sets of Idclsf corresponding to two different columns in
  Clasificatii — that is wrong." `IdClsf` becomes `Clasificatii.IDClsf` on all seven.
- **No copy of the Access id is kept on those tables** ("I DON'T WANT TO KEEP IdClsfAcc in those
  tables", later the same day — the first version of this slice had added an `IdClsfAcc` to
  each; it was taken out again before anything ran). The Access id stays in
  `Clasificatii.IdClsfAcc`, reachable through `IdClsf`.
- A row whose Access id resolves to no `Clasificatii` row, or to more than one, is a PRIOR TEST:
  if it fails, nothing is done to the data until the operator has looked.

The seven: `FX_Extrase_H`, `FX_Indicatori`, `FX_Istoric`, `FX_Plati`, `FX_Receptii`, `FX_Receptii_RHR`, `FX_Rezervari`.

**The marker.** A converted `IdClsf` carries the column comment `Clasificatii.IDClsf (0080-01)`. Without an `IdClsfAcc` column nothing else tells a converted table from an unconverted one, and a second pass would read MariaDB keys as Access ids. The one-off skips marked tables; both migrators refuse an unmarked target.

### What changed

#### One-off: `PYTHON/scripts/extrase_clsf_0080.py`
- Phase 1 (read only, ALL databases — AVACONT_SURSA + every `NNN_*`): every distinct `DataDoc`
  through the migrator's own reader (`parser.parse_value`, `date` target); the classification
  check (`clsf_pair.problem_sql`) on the CURRENT `IdClsf` of every unmarked table. Any problem
  anywhere → printed, run stops, nothing touched. Ambiguous day/month readings printed.
- **Converted by hand** (operator: some databases' `FX_Plati` were already changed — they carry
  an `IdClsfAcc` column): detected by that column. Their `IdClsf` is NOT translated; phase 1
  checks every non-zero value is an existing `Clasificatii.IDClsf`; phase 2 drops `IdClsfAcc`
  and marks `IdClsf`.
- Phase 2 per database: mysqldump of the touched tables → temporaries `IdClsfAcc_0080` /
  `DataDoc_0080` → one transaction (Access id → temporary, `IdClsf` rewritten from
  `Clasificatii` on the row's unit, DataDoc converted, verified, COMMIT) → `IdClsf` marked and
  the temporary dropped; `DataDoc` replaced by the date column. An interruption between commit
  and DDL is detected (filled temporary) and only finished.
- `--dry-run` = phase 1 only; `--db NAME` = one database.

#### Shared: `PYTHON/utils/clsf_pair.py`
The seven tables and where each finds its unit (own `IdUnitate`; else `FX_Indicatori` on
`CodAI`; `FX_Istoric` / `FX_Rezervari` only through `FX_Indicatori`); `problem_sql` /
`fill_sql` take the column holding the Access id; `MARKER` / `converted_tables`; `Resolver`
(row-by-row lookup for the migrator); `describe_problems` (Romanian lines).

#### Python migrator (`routes/migrare`)
`execute._clsf_resolver`: on a pair table whose Access `IdClsf` travels into the target
`IdClsf` (the rename to `IdClsfAcc` no longer applies — no such target column), the value is
translated row by row before writing; unresolved rows are collected and the table is stopped
before its commit (rollback). An unmarked target is refused. `tables.py` comment updated.
`DataDoc`: `parser.py` already reads the three shapes into a DATE.

#### VB migrator (`src/KBot.Migrator`) — the one the operator uses
The first run on `014_SCSV` died with 1292 `Incorrect datetime value: '31.12.2025'` on
`FX_Extrase.DataDoc`: the VB migrator had been missed.
- `ValueConverter`: text going into a `date` / `datetime` / `timestamp` column is read by
  `TryReadDate`, same rules as `parser.py`. Empty text ▸ NULL; unreadable ▸ `TransferException`.
- `TableMap.WithClsfPair()` on the seven: derived mapping `ColumnSourceKind.ClasificatieByRowUnit`
  on `IdClsf` (Access `IdClsf` resolved to the assigned `Clasificatii.IDClsf`; 0 / NULL ▸ NULL;
  a miss stops the run). The unit: `OwnershipPlan.ClassificationUnit` — the row's own
  `IdUnitate`, else its ownership unit, else its indicator's (`CodAI` in Access
  `FX_Indicatori`). `Verifier.CheckClasificatiiResolution` runs the same lookup dry and names
  every miss before anything is written (the prior test); `CheckClsfConverted` refuses an
  unmarked target.
- **Angajamente follow their indicators** (after the 014_SCSV journal: one `FX_2026.accdb` holds
  TWO DCs; `FX_Istoric` / `FX_Rezervari` travelled whole — 704 / 68 rows of the other DC, links
  blanked). `FX_Angajamente.IdUnitate` is NOT usable (operator). `OwnershipPlan`:
  `Subtree.Angajament`, authority Access `FX_Indicatori` (CodAngajament ▸ IdUnitate);
  `FX_Angajamente`, `FX_Istoric`, `FX_Rezervari` travel when one indicator of the angajament is
  in a ticked unit, else «altă unitate»; an angajament with no indicator unit stays behind
  (Atenție). `FX_Istoric` / `FX_Rezervari` without `CodAngajament` = BLOCANT
  (`ANGAJAMENT_LIPSA`; operator: "a critical rule … this can never be").
- **Angajamente with no indicator unit** (operator, 25.09.2026: not fully downloaded, they
  exist only in `FX_Angajamente`): the tiebreaker is Access `FX_Angajamente.DC`. `DC` = the
  target database ▸ the angajament travels, into `FX_Angajamente` ONLY (`Shared1`); another
  DC ▸ stays behind silently as that DC's; empty `DC` ▸ stays behind with the Atenție finding.
  `FX_Istoric` / `FX_Rezervari` rows hanging from such an angajament stay behind with an
  Atenție finding. Migrator 1.12 ▸ **1.13**.
- **`FX_Extrase_H` unit and classification from the account** (operator, 25.09.2026: migrated
  headers arrived with `IdUnitate = 0` and no `IdClsf`; "when i migrate data, i don't have to
  manually do anything"). `Transfer/ExtrasHeaderRules.vb` ports the download's rules
  (`extrase.py` `_scrie_antet`): first 3 of `Cont` ▸ `DefaSSS` ▸ `DefaSS` (common db) ▸
  `Unitati.SursaSector`; with an IBAN, ClsfSal = `Cont` from position 4 (12 chars after a `6`,
  6 after a `3`) ▸ `Clasificatii` of that unit (lowest id + warning on duplicates), else
  `Clasificatii_Venituri` ▸ `IdClsfV`. The Access values of the three columns are not read.
  Lookups run on the run's transaction at the first header; the runner refuses a write order
  with `Clasificatii` / `Clasificatii_Venituri` after `FX_Extrase_H`. A miss is NULL + a logged
  warning, never a stop. A `500` prefix is read on 4 characters: `5005` = 02A, `5006` = 01A (operator) — a unit, no
  classification; same condition added to the download (`extrase.py` `SURSA_500X`). The Python
  migrator (`routes/migrare`) is NOT changed. Migrator 1.13 ▸ **1.14**; the missing-database prompt (below) ▸ **1.15**.
- Table grid «Rânduri Access» was never filled: `Access/AccessRowCounter.vb` counts every
  catalogue table in the ticked units' files on «Verifică» (raw count, before the unit filter).
- `KBot.Migrator` FileVersion 1.11.0.0 ▸ **1.12.0.0**.

#### Server writers (write the MariaDB key only)
`prelucrare_unitate.find_id_clsf` returns `Clasificatii.IDClsf`; **two nomenclator rows for one
ClsfSal in a unit now raise**. `prelucrare.py` (FX_Indicatori), `prelucrare_pasi.py`
(FX_Istoric, FX_Rezervari, FX_Receptii ×2, FX_Receptii_RHR, FX_Plati), `prelucrare_asociere.py`,
`receptii_refacere.py`, `ddf_edit.py` (FX_Indicatori of a manual angajament), `extrase.py`
(FX_Extrase_H; nomenclator duplicates → first + a warning).
`ord_edit.py`: `FX_ORD_TBL.IdClsfAcc` (that table keeps its pair) is read from
`Clasificatii.IdClsfAcc` through `FX_Plati.IdClsf`. **Superseded by 0080-04** (25.09.2026):
`IdClsfAcc` was dropped from `FX_ORD_TBL`, `FX_DDF_REV_*` and `Parteneri_Coduri` too.

#### Readers (join on the primary key)
`C.IDClsf = X.IdClsf` in `plati.py`, `receptii.py`, `rezervari.py`, `sumar.py`, `ord.py`,
`ord_edit.py`, `ddf_edit.py`, `prelucrare_pasi.py`, `angajament_dump.py`, `istoric.py`.

#### DataDoc as DATE
`extrase.py` writes a `date`, compares dates in the dedup; the HASH keeps `dd.MM.yyyy`.
`plati.py` still sends `data_doc` as `dd.MM.yyyy` text, so `PlatiView` is unchanged.

#### Other
`sql/AVACONT_SURSA.sql` (reference DDL): the marker comment on the seven `IdClsf`, `DataDoc date`.

### Files touched
PYTHON: `scripts/extrase_clsf_0080.py` (new), `utils/clsf_pair.py` (new),
`routes/migrare/{execute,tables}.py`, `routes/forexe/{prelucrare_unitate, prelucrare,
prelucrare_pasi, prelucrare_asociere, receptii_refacere, ddf_edit, extrase, plati, receptii,
rezervari, sumar, ord, ord_edit, istoric, angajament_dump}.py`; tests `test_clsf_pair.py` (new),
`test_forexe_prelucrare_{unitate,route,pasi}.py`, `test_forexe_{istoric,plati,receptii,
rezervari,sumar,extrase}.py`. `sql/AVACONT_SURSA.sql`.
src/KBot.Migrator: `Access/AccessRowCounter.vb` (new), `Transfer/{ValueConverter, ColumnMapping,
TableMap, TableMaps, TransferRunner, Verifier, OwnershipPlan, Finding}.vb`, `MigratorForm.vb`,
`KBot.Migrator.vbproj`; `tests/KBot.Migrator.Tests/ClsfPairAndDateTests.vb` (new).

### Test results
Nothing run (operator rule). `py_compile` clean on every touched Python file;
`dotnet build src\KBot.Migrator` 0 errors / 0 warnings. Tests updated/written, not run.

### Unverified / deferred
- **The one-off has never run.** `--dry-run` first on the server; it lists every problem.
- **Deploy order**: server code and the one-off in the same window (old code reads `IdClsf` as
  an Access id; new code writes the key).
- **Migrating into a database**: it must be marked first — run the one-off with `--db` on it
  (or on `AVACONT_SURSA` before creating it from there).
- Behaviour change: a ClsfSal with two rows in one unit's nomenclator STOPS the ingestion.
- `FX_Extrase_H` rows whose `IdUnitate` is not a ticked unit (D10: 77, 0) will not resolve and
  will block at «Verifică» when their `IdClsf` is non-zero — to be seen on the next run.
- The legacy seed route (`routes/forexe/seed.py`) is dead (operator) and was not touched.

---

<a id="slice-0080-04"></a>

## SLICE 0080-04 — `IdClsfAcc` only in `Clasificatii` (25.09.2026)

### Request

Operator, 25.09.2026, after 0080-01 had removed the Access copy from seven FX_ tables: "let's come back to removing the IdClsfAcc from those tables which don't need it. it should only be left in Clasificatii - that should be the source of truth."

Five tables still carried `IdClsfAcc` next to `IdClsf`:

| Table | `IdClsf` | `IdClsfAcc` |
|---|---|---|
| `FX_ORD_TBL` | FK ▸ `Clasificatii.IDClsf`, NULL default 0 | NULL |
| `FX_DDF_REV_SA` | FK, NOT NULL | NOT NULL, no default |
| `FX_DDF_REV_SB` | FK, NOT NULL | NOT NULL, no default |
| `FX_DDF_REV_PRT` | FK, NULL | NULL |
| `Parteneri_Coduri` | FK, NOT NULL default 0 | NOT NULL default 0 |

On all five `IdClsf` is already the MariaDB key, so the Access id is always `Clasificatii.IdClsfAcc` through it; the copy only duplicated it. The column is dropped, nothing is rewritten.

### Who used the copy (searched before changing)

- **K-BOT** (ORD / DDF editors): carried it back and forth, never used it. The ORD generator
  even read it FROM `Clasificatii` just to store it on the line.
- **The old Access sync (VBA, API-key routes)** `routes/ord/*`, `routes/ddf/*`,
  `routes/parteneri.py`: VBA sends it on the way in; `sync_mdb_acc` returns it to Access as
  `IdClsf`. Whether those routes are still used was NOT asked — the change keeps them working
  either way: incoming values are ignored, outgoing values are read from `Clasificatii`.
- **VB migrator**: decision D9 wrote the Access `IdClsf` into the NOT NULL `IdClsfAcc` of
  SA / SB; `Parteneri_Coduri` got the raw id too. `FX_ORD_TBL.IdClsfAcc` was never fed.
- **Python migrator**: the rename `IdClsf ▸ IdClsfAcc` (applied only when the target had it).

### What changed

#### One-off: `PYTHON/scripts/idclsfacc_0080_04.py`
- Phase 1 (read only, ALL databases): stops the whole run when a row would lose information —
  `IdClsfAcc` set but `IdClsf` empty; `IdClsf` pointing nowhere; `IdClsfAcc` different from
  `Clasificatii.IdClsfAcc` of its `IdClsf`. Samples printed with the primary key.
- Phase 2 per database: mysqldump of the tables that still have the column (the only copy of
  the dropped values), then `ALTER TABLE … DROP COLUMN IdClsfAcc`. Re-runnable (a table
  without the column is skipped). Reuses the helpers of `extrase_clsf_0080.py`.
- `--dry-run`, `--db NAME`, `--backup-dir`.

#### Server (new system)
- `routes/forexe/ord_edit.py`: generator no longer reads the Access id; `id_clsf_acc` gone from
  the draft and from `FX_ORD_TBL` INSERT / UPDATE.
- `routes/forexe/ddf_edit.py`: `id_clsf_acc` gone from the generator, draft, classification
  picker and the SA / SB INSERT / UPDATE; `_rezolva_clasificatii` reads `Clsf`, `SS`, `CodSSI`.
- Comments: `ord.py`, `ddf.py`.

#### Server (old VBA routes)
- `routes/ord/tbl.py`, `patch.py`, `sync_acc_mdb.py`: `IdClsfAcc` not written.
- `routes/ord/sync_mdb_acc.py`, `routes/ddf/sync_mdb_acc.py`: Access `IdClsf` =
  `(SELECT C.IdClsfAcc FROM Clasificatii C WHERE C.IDClsf = …IdClsf)`.
- `routes/ddf/staging.py` (commit into SA / SB), `prt.py`, `sync_acc_mdb.py`, `core.py`
  (`sa/patch`: `IdClsfAcc` no longer an allowed field), `routes/parteneri.py`
  (`upsert_coduri`): not written.
- The `stg_*` staging tables (not present in `MariaDB_Schema` at all) still receive it from VBA;
  it simply stops there.

#### Migrators
- VB (`src/KBot.Migrator`): `FX_DDF_REV_SA`, `FX_DDF_REV_SB`, `Parteneri_Coduri` no longer map
  `IdClsfAcc` (D9 gone). `Verifier.CheckClsfConverted`: a target table other than
  `Clasificatii` that still has `IdClsfAcc` is BLOCANT, naming the script. FileVersion stays
  1.13.0.0 (bumped earlier today, not yet handed out).
- Python (`routes/migrare`): `COLUMN_RENAMES` loses `IdClsf ▸ IdClsfAcc`;
  `execute._refuse_idclsfacc` refuses such a target before anything else.

#### Client
- `KBot.Api` (`ApiClient`, `DdfEditContract`, `OrdEditContract`), `KBot.Domain` (`DdfDraft`,
  `OrdDraft`), `KBot.App` (`DdfEditSectiuneaAPage`): `IdClsfAcc` / `id_clsf_acc` removed.
  Api 1.0.8 ▸ **1.0.9**, Domain 1.2.2 ▸ **1.2.3**.

#### Other
- `sql/AVACONT_SURSA.sql` (reference DDL): the five columns removed.
- Tests: `test_forexe_ddf.py` seeds without `IdClsfAcc`;
  `ClsfPairAndDateTests.Only_Clasificatii_writes_IdClsfAcc` (new).

### Test results
Nothing run (operator rule). `py_compile` clean on every touched Python file;
`dotnet build` of `KBot.App` and `KBot.Migrator`: 0 errors / 0 warnings.

### Unverified / deferred
- **The one-off has never run.** `--dry-run` first; a mismatch between a copy and the
  nomenclator stops it and needs the operator's decision.
- **Deploy order**: server code and the one-off in the same window. Old code on a converted
  database ▸ 1054 (unknown column); new code on an unconverted one ▸ 1364 on SA / SB
  (NOT NULL without default).
- A client older than this build still sends `id_clsf_acc`; the server ignores it, so that is
  harmless.

## Repere unice din Open threads 0080

Punerea în funcțiune consemna citirea integrală a dry-run, apoi rularea reală și fișierele serverului (forexe/*, migrare/execute.py, utils/clsf_pair.py, extrase_lista.py), cu restart gunicorn în aceeași fereastră. La acel checkpoint, Migratorul VB 1.12 trebuia dat operatorului, anterior reviziilor 1.13…1.15 descrise în 01. Oprirea prelucrării la ClsfSal dublat și lipsa sumelor din dreapta arborelui Extrase erau marcate de confirmat de operator; nu sunt confirmate acum.
