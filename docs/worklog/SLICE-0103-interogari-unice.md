# SLICE-0103 — One-time queries: run once on AVACONT_SURSA and on every unit, remembered in a ledger (operator request, 02.10.2026)

Branch: `SLICE-0100-Multithreading` (nothing committed). Dev tooling (AvacontPush + server); no operator-visible window of K-BOT,
so no help topic.

## What changed and why

Slice 0102 needed a DATA change on every database (give the existing budget rows a start date, drop an old key). schema_sync
carries structure only, and a hand-run script leaves no trace of where it was run. The operator asked for: a query for
`AVACONT_SURSA`, then a query entered in AvacontPush and run on all databases, a table on every database (the template
included) that remembers these one-time queries — «or a hash» — and the queries also run on the template, so that new units
start with them already «run».

- **Ledger table `Interogari_Unice`** on every database (`Hash` = SHA-256 of the statements, `Nume` unique, `RulatLa`,
  `RulatDe`, `Randuri`, `Interogare`). Created on the template by `sql/0102_01_sursa.sql`, brought to the units by schema sync
  (SAFE), and created by the runner wherever it is missing (`CREATE TABLE … LIKE AVACONT_SURSA.Interogari_Unice`).
  `sql/AVACONT_SURSA.sql` (the template dump) has it too.
- **`PYTHON/routes/one_time/runner.py`** (new): `--view` / `--run` / `--status`. Cuts the statements (strings, backticks,
  comments), refuses `DELIMITER` and `DROP DATABASE|SCHEMA`; targets = `AVACONT_SURSA` first, then every CAI database that
  exists (unit databases outside CAI are named and left alone); looks at every ledger first and refuses, before executing,
  a name already used with a different text; then runs per database (own connection, one transaction, ledger row only after
  every statement succeeded). A failing unit is reported, left unmarked, the run continues; a failing template stops the run.
  Exit 0 / 1 (some failed) / 2 (refused, nothing executed).
- **New units are born «run»:** `provizionare.py` copies the template's ledger rows into the cloned unit
  (`INSERT … SELECT * FROM AVACONT_SURSA.Interogari_Unice`, committed), right after the tables and views are cloned.
- **AvacontPush**, new tab «Interogări unice» (`OneTimeService.vb`, `Form1.vb`, `Form1.Designer.vb`): name box, big SQL box,
  «Din fișier…», «Ce s-a rulat», «Vezi (nu execută)», «Execută» (confirmation). The query goes as base64 on the command line
  (≤ 30,000 bytes: one SSH packet); the pane and the log show a short description instead of the blob
  (`RunRemoteAsync` got an optional display text).
- **Slice 0102 split accordingly:** the old all-in-one `sql/0102_clasificatii_buget_data_inceput.sql` (a loop over every
  database, never run) is gone; `sql/0102_01_sursa.sql` (structure, on the template only) and
  `sql/0102_02_interogare_unica.sql` (the data part, pasted into the new tab, name `0102_buget_data_inceput`) replace it.

## Files touched

New: `PYTHON/routes/one_time/__init__.py`, `PYTHON/routes/one_time/runner.py`, `PYTHON/AvacontPush/OneTimeService.vb`,
`sql/0102_01_sursa.sql`, `sql/0102_02_interogare_unica.sql`. Changed: `PYTHON/routes/inregistrare/provizionare.py`,
`PYTHON/AvacontPush/Form1.vb`, `PYTHON/AvacontPush/Form1.Designer.vb`, `PYTHON/AvacontPush/README.md`,
`sql/AVACONT_SURSA.sql`. Deleted: `sql/0102_clasificatii_buget_data_inceput.sql` (written earlier today, never run).

## Test results

No tests written or run (operator: no tests). `dotnet build PYTHON/AvacontPush/AvacontPush.vbproj` → 0 warnings, 0 errors
(forced rebuild). `py_compile` on `runner.py` and `provizionare.py`. A throwaway script in the scratchpad (not in the repo)
fed the real 0102 text and a few bad inputs to `prepare` / `split_statements`: 2 statements, comments and line ends do not
change the hash, `DELIMITER` / `DROP DATABASE` / empty / unclosed string are refused. **Nothing ran against a database or
over SSH; the tab was never opened.**

## Left unverified / deferred

- **Rights.** The runner connects with `config.DB_CONFIG_NEW` and writes to `AVACONT_SURSA` too (create the ledger if
  missing, insert the row, run the query). Not checked that this account may do that on the template; if not, the first
  run stops at the template with MariaDB's message. The provisioning account already has SELECT on the template and
  INSERT on `1__\_____`.* (what the ledger copy needs).
- **Order matters for 0102:** `0102_01_sursa.sql` → push + restart → schema sync SAFE on the units (adds `DataInceput`, the
  ledger table, the new unique key; a SAFE sync never drops the old unique key) → the one-time query. A unit that has not been
  synced yet fails the query on its first statement (unknown column), stays unmarked, and can be run again.
- **A SAFE sync adds `DataInceput date NOT NULL` without a default to tables that already have rows**: they get `0000-00-00`
  unless the server's `sql_mode` has `NO_ZERO_DATE`, in which case the ALTER errors for that unit. The one-time query fixes the
  zero dates. Check `SELECT @@sql_mode;` if a unit's sync fails on `Clasificatii_Buget`.
- **Not idempotent by itself:** a query that is not written to be re-runnable and fails half-way on a unit (DDL commits) can
  leave that unit half-done and unmarked.
- **`Setari` rows are not copied to new units** by the provisioning job (it clones structure only; this slice adds the ledger
  rows as the one exception). Not changed here; K-BOT treats a missing `Setari` row as multi-thread off.
- A unit database that exists on the server but is not in CAI is never touched (named in the output).
- Nothing seen on screen (new tab layout, the confirmation text, the pane output).
