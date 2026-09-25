# SLICE 0080-04 — `IdClsfAcc` only in `Clasificatii` (25.09.2026)

## Request

Operator, 25.09.2026, after 0080-01 had removed the Access copy from seven FX_ tables:
"let's come back to removing the IdClsfAcc from those tables which don't need it. it should
only be left in Clasificatii - that should be the source of truth."

Five tables still carried `IdClsfAcc` next to `IdClsf`:

| Table | `IdClsf` | `IdClsfAcc` |
|---|---|---|
| `FX_ORD_TBL` | FK ▸ `Clasificatii.IDClsf`, NULL default 0 | NULL |
| `FX_DDF_REV_SA` | FK, NOT NULL | NOT NULL, no default |
| `FX_DDF_REV_SB` | FK, NOT NULL | NOT NULL, no default |
| `FX_DDF_REV_PRT` | FK, NULL | NULL |
| `Parteneri_Coduri` | FK, NOT NULL default 0 | NOT NULL default 0 |

On all five `IdClsf` is already the MariaDB key, so the Access id is always
`Clasificatii.IdClsfAcc` through it; the copy only duplicated it. The column is dropped, nothing
is rewritten.

## Who used the copy (searched before changing)

- **K-BOT** (ORD / DDF editors): carried it back and forth, never used it. The ORD generator
  even read it FROM `Clasificatii` just to store it on the line.
- **The old Access sync (VBA, API-key routes)** `routes/ord/*`, `routes/ddf/*`,
  `routes/parteneri.py`: VBA sends it on the way in; `sync_mdb_acc` returns it to Access as
  `IdClsf`. Whether those routes are still used was NOT asked — the change keeps them working
  either way: incoming values are ignored, outgoing values are read from `Clasificatii`.
- **VB migrator**: decision D9 wrote the Access `IdClsf` into the NOT NULL `IdClsfAcc` of
  SA / SB; `Parteneri_Coduri` got the raw id too. `FX_ORD_TBL.IdClsfAcc` was never fed.
- **Python migrator**: the rename `IdClsf ▸ IdClsfAcc` (applied only when the target had it).

## What changed

### One-off: `PYTHON/scripts/idclsfacc_0080_04.py`
- Phase 1 (read only, ALL databases): stops the whole run when a row would lose information —
  `IdClsfAcc` set but `IdClsf` empty; `IdClsf` pointing nowhere; `IdClsfAcc` different from
  `Clasificatii.IdClsfAcc` of its `IdClsf`. Samples printed with the primary key.
- Phase 2 per database: mysqldump of the tables that still have the column (the only copy of
  the dropped values), then `ALTER TABLE … DROP COLUMN IdClsfAcc`. Re-runnable (a table
  without the column is skipped). Reuses the helpers of `extrase_clsf_0080.py`.
- `--dry-run`, `--db NAME`, `--backup-dir`.

### Server (new system)
- `routes/forexe/ord_edit.py`: generator no longer reads the Access id; `id_clsf_acc` gone from
  the draft and from `FX_ORD_TBL` INSERT / UPDATE.
- `routes/forexe/ddf_edit.py`: `id_clsf_acc` gone from the generator, draft, classification
  picker and the SA / SB INSERT / UPDATE; `_rezolva_clasificatii` reads `Clsf`, `SS`, `CodSSI`.
- Comments: `ord.py`, `ddf.py`.

### Server (old VBA routes)
- `routes/ord/tbl.py`, `patch.py`, `sync_acc_mdb.py`: `IdClsfAcc` not written.
- `routes/ord/sync_mdb_acc.py`, `routes/ddf/sync_mdb_acc.py`: Access `IdClsf` =
  `(SELECT C.IdClsfAcc FROM Clasificatii C WHERE C.IDClsf = …IdClsf)`.
- `routes/ddf/staging.py` (commit into SA / SB), `prt.py`, `sync_acc_mdb.py`, `core.py`
  (`sa/patch`: `IdClsfAcc` no longer an allowed field), `routes/parteneri.py`
  (`upsert_coduri`): not written.
- The `stg_*` staging tables (not present in `MariaDB_Schema` at all) still receive it from VBA;
  it simply stops there.

### Migrators
- VB (`src/KBot.Migrator`): `FX_DDF_REV_SA`, `FX_DDF_REV_SB`, `Parteneri_Coduri` no longer map
  `IdClsfAcc` (D9 gone). `Verifier.CheckClsfConverted`: a target table other than
  `Clasificatii` that still has `IdClsfAcc` is BLOCANT, naming the script. FileVersion stays
  1.13.0.0 (bumped earlier today, not yet handed out).
- Python (`routes/migrare`): `COLUMN_RENAMES` loses `IdClsf ▸ IdClsfAcc`;
  `execute._refuse_idclsfacc` refuses such a target before anything else.

### Client
- `KBot.Api` (`ApiClient`, `DdfEditContract`, `OrdEditContract`), `KBot.Domain` (`DdfDraft`,
  `OrdDraft`), `KBot.App` (`DdfEditSectiuneaAPage`): `IdClsfAcc` / `id_clsf_acc` removed.
  Api 1.0.8 ▸ **1.0.9**, Domain 1.2.2 ▸ **1.2.3**.

### Other
- `sql/AVACONT_SURSA.sql` (reference DDL): the five columns removed.
- Tests: `test_forexe_ddf.py` seeds without `IdClsfAcc`;
  `ClsfPairAndDateTests.Only_Clasificatii_writes_IdClsfAcc` (new).

## Test results
Nothing run (operator rule). `py_compile` clean on every touched Python file;
`dotnet build` of `KBot.App` and `KBot.Migrator`: 0 errors / 0 warnings.

## Unverified / deferred
- **The one-off has never run.** `--dry-run` first; a mismatch between a copy and the
  nomenclator stops it and needs the operator's decision.
- **Deploy order**: server code and the one-off in the same window. Old code on a converted
  database ▸ 1054 (unknown column); new code on an unconverted one ▸ 1364 on SA / SB
  (NOT NULL without default).
- A client older than this build still sends `id_clsf_acc`; the server ignores it, so that is
  harmless.
