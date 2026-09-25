# SLICE 0080-01 — `IdClsf` = `Clasificatii.IDClsf` on seven FX_ tables, FX_Extrase.DataDoc → DATE (24.09.2026)

## Request

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

The seven: `FX_Extrase_H`, `FX_Indicatori`, `FX_Istoric`, `FX_Plati`, `FX_Receptii`,
`FX_Receptii_RHR`, `FX_Rezervari`.

**The marker.** A converted `IdClsf` carries the column comment
`Clasificatii.IDClsf (0080-01)`. Without an `IdClsfAcc` column nothing else tells a converted
table from an unconverted one, and a second pass would read MariaDB keys as Access ids. The
one-off skips marked tables; both migrators refuse an unmarked target.

## What changed

### One-off: `PYTHON/scripts/extrase_clsf_0080.py`
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

### Shared: `PYTHON/utils/clsf_pair.py`
The seven tables and where each finds its unit (own `IdUnitate`; else `FX_Indicatori` on
`CodAI`; `FX_Istoric` / `FX_Rezervari` only through `FX_Indicatori`); `problem_sql` /
`fill_sql` take the column holding the Access id; `MARKER` / `converted_tables`; `Resolver`
(row-by-row lookup for the migrator); `describe_problems` (Romanian lines).

### Python migrator (`routes/migrare`)
`execute._clsf_resolver`: on a pair table whose Access `IdClsf` travels into the target
`IdClsf` (the rename to `IdClsfAcc` no longer applies — no such target column), the value is
translated row by row before writing; unresolved rows are collected and the table is stopped
before its commit (rollback). An unmarked target is refused. `tables.py` comment updated.
`DataDoc`: `parser.py` already reads the three shapes into a DATE.

### VB migrator (`src/KBot.Migrator`) — the one the operator uses
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

### Server writers (write the MariaDB key only)
`prelucrare_unitate.find_id_clsf` returns `Clasificatii.IDClsf`; **two nomenclator rows for one
ClsfSal in a unit now raise**. `prelucrare.py` (FX_Indicatori), `prelucrare_pasi.py`
(FX_Istoric, FX_Rezervari, FX_Receptii ×2, FX_Receptii_RHR, FX_Plati), `prelucrare_asociere.py`,
`receptii_refacere.py`, `ddf_edit.py` (FX_Indicatori of a manual angajament), `extrase.py`
(FX_Extrase_H; nomenclator duplicates → first + a warning).
`ord_edit.py`: `FX_ORD_TBL.IdClsfAcc` (that table keeps its pair) is read from
`Clasificatii.IdClsfAcc` through `FX_Plati.IdClsf`. **Superseded by 0080-04** (25.09.2026):
`IdClsfAcc` was dropped from `FX_ORD_TBL`, `FX_DDF_REV_*` and `Parteneri_Coduri` too.

### Readers (join on the primary key)
`C.IDClsf = X.IdClsf` in `plati.py`, `receptii.py`, `rezervari.py`, `sumar.py`, `ord.py`,
`ord_edit.py`, `ddf_edit.py`, `prelucrare_pasi.py`, `angajament_dump.py`, `istoric.py`.

### DataDoc as DATE
`extrase.py` writes a `date`, compares dates in the dedup; the HASH keeps `dd.MM.yyyy`.
`plati.py` still sends `data_doc` as `dd.MM.yyyy` text, so `PlatiView` is unchanged.

### Other
`sql/AVACONT_SURSA.sql` (reference DDL): the marker comment on the seven `IdClsf`, `DataDoc date`.

## Files touched
PYTHON: `scripts/extrase_clsf_0080.py` (new), `utils/clsf_pair.py` (new),
`routes/migrare/{execute,tables}.py`, `routes/forexe/{prelucrare_unitate, prelucrare,
prelucrare_pasi, prelucrare_asociere, receptii_refacere, ddf_edit, extrase, plati, receptii,
rezervari, sumar, ord, ord_edit, istoric, angajament_dump}.py`; tests `test_clsf_pair.py` (new),
`test_forexe_prelucrare_{unitate,route,pasi}.py`, `test_forexe_{istoric,plati,receptii,
rezervari,sumar,extrase}.py`. `sql/AVACONT_SURSA.sql`.
src/KBot.Migrator: `Access/AccessRowCounter.vb` (new), `Transfer/{ValueConverter, ColumnMapping,
TableMap, TableMaps, TransferRunner, Verifier, OwnershipPlan, Finding}.vb`, `MigratorForm.vb`,
`KBot.Migrator.vbproj`; `tests/KBot.Migrator.Tests/ClsfPairAndDateTests.vb` (new).

## Test results
Nothing run (operator rule). `py_compile` clean on every touched Python file;
`dotnet build src\KBot.Migrator` 0 errors / 0 warnings. Tests updated/written, not run.

## Unverified / deferred
- **The one-off has never run.** `--dry-run` first on the server; it lists every problem.
- **Deploy order**: server code and the one-off in the same window (old code reads `IdClsf` as
  an Access id; new code writes the key).
- **Migrating into a database**: it must be marked first — run the one-off with `--db` on it
  (or on `AVACONT_SURSA` before creating it from there).
- Behaviour change: a ClsfSal with two rows in one unit's nomenclator STOPS the ingestion.
- `FX_Extrase_H` rows whose `IdUnitate` is not a ticked unit (D10: 77, 0) will not resolve and
  will block at «Verifică» when their `IdClsf` is non-zero — to be seen on the next run.
- The legacy seed route (`routes/forexe/seed.py`) is dead (operator) and was not touched.
