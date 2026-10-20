# SLICE-0121-07 - One-time query: copy the old `Parteneri_Coduri` into the new link tables (19.10.2026)

Operator (19.10.2026): only one unit is left to migrate, so the Migrator cannot carry the old data (and it would not carry what was typed in K-BOT after the migration); wanted: a run-once SQL, run from KBotAdmin like the earlier ones.

## What changed and why
New `sql/0121_07_interogare_unica.sql`, for KBotAdmin page «Interogari unice» (name `0121_07_parteneri_coduri_in_angajamente`). On AVACONT_SURSA and every ticked unit database it copies the old «Coduri angajament» into the new tables:
- `Parteneri_Coduri` JOIN `Parteneri` -> CodFiscal normalized in SQL like the DDF rule (letters and digits only, upper case, leading «RO» before a digit dropped; `REGEXP_REPLACE`), `CodAng`, `CodInd`, `ContBancar` into a temporary table;
- `Parteneri_Angajamente`: one link per `(CodFiscal, CodAng)`, inserted only when missing;
- `Parteneri_Angajamente_Indicatori`: `CodAI = CodAng-CodInd`; an existing indicator keeps what it has and is filled only where `CodInd` / `ContBancar` are empty, so data already typed in the new window is never overwritten.
- Left in the old table (listed by the check queries at the end of the file, as comments: a one-time query returns no result set): rows whose partner has no usable CodFiscal, rows with no `CodAng`. The old `IdClsf` is not copied (`CodAI` gives it). A row with `CodAng` and no `CodInd` makes the link only.
- Nothing is deleted; safe to run again; the five statements were split by the runner's own `split_statements` (checked, no database touched).

Order: `P121_01` on AVACONT_SURSA -> structure sync to the units -> the new Python files on the server -> this query. A database without the new tables fails and is left unmarked.

## Files touched
`sql/0121_07_interogare_unica.sql` (new); status + plan rows.

## Test results
Only the splitter of `routes/one_time/runner.py` was run on the file (5 statements). The SQL was not executed anywhere (no database touched).

## Left unverified or deferred
- Never run: `REGEXP_REPLACE` needs MariaDB 10.0.5+ (the server runs MariaDB; version not checked here); the `INSERT ... SELECT ... ON DUPLICATE KEY UPDATE` with `VALUES()` is the usual form but unrun.
- Normalization in SQL treats only ASCII letters and digits; the Python rule accepts any letter. For tax codes this makes no difference; a foreign id with an accented letter would be cut differently.
- Whether `CodAng` is filled in the old data is still unseen: run the first check query before the run.
- 0121-06 (dropping `Parteneri_Coduri`) waits for this query's result, for the last unit's Migrator run and for the operator's word.
