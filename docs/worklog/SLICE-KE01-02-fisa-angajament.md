# SLICE-KE01-02 - FX_FisaAngajament: one result table with everything about one angajament (16.10.2026)

## What changed and why
Operator request (16.10.2026): a way to hand the whole picture of ONE angajament to the assistant, to ask about its logic - first case: why the classification does not show correctly when a received invoice is linked to receptions in KE. Part of the KE family (a diagnostic of the KE01 model), not sliceless.

New file `sql/KE_01_02_fisa_angajament.sql`, three stored procedures on **AVACONT_COMUN** (one copy for all units), read-only (they write only two temporary tables):

- `FX_FisaAngajament(p_db, p_cod)` - the one to call: `CALL AVACONT_COMUN.FX_FisaAngajament('000_DEMO', '<CodAngajament>')`. `p_db` is the unit database; it is checked against `information_schema` (must exist, hold `FX_Angajamente`, be visible to the account) before it goes, in backticks, into any statement. Everything on the unit database is dynamic SQL (`#.` = the `` `db`. `` prefix).
- `FX_Fisa_Adauga` - internal: JSON of every column of a table, built from `information_schema` (follows schema drift); blobs show their length, long texts are cut at 300 characters.
- `FX_Fisa_Exec` - internal: runs one dynamic statement, catches 1146 / 1054 (missing table / column) and reports it instead of failing.

Result: ONE table `Ord | Sectiune | Tabel | Detalii`. Section `DIAG` first, mirroring `PYTHON/routes/efactura/primite_ke.py`: `indicator_clasificatie`, `linie_factura_vs_indicator`, `receptie_pe_clasificatie` (the `_SQL_VALORI_RECEPTII` rule), `legatura_ke`; then the raw rows of every table that holds the angajament (directly or through DDFs, receptions, indicators, invoices). Candidate invoices = manual DDF link + KE link + supplier CUI equals a partner of a DDF (same normalisation as `primite._PARTNER_CUI`).

## KBotAdmin (same day)
The procedure is a whitelisted command of the «Comenzi» page: `fisa_angajament` ("Fisa angajament") in `COMMANDS` of `PYTHON/routes/admin_cli/admin_cli.py`, calling `AVACONT_COMUN.FX_FisaAngajament(db, cod)`. New `COMMAND_PARAMS` (name -> label of the typed value) and a `--param` argument of the `command` verb; the code is checked against `^[A-Za-z0-9_./-]{1,60}$`. A command with a typed value runs on ONE database: the server refuses ALL, and the page refuses it first (`RequireSingleDb`) and also an empty code. The block carries `raw` so the page prints each row as one untruncated line (cells joined by ` | `), ready to copy. In the page a text box (`txtParam`, declared in the Designer) is visible only while the chosen command needs a value; another choice in the combo hides it.

## Files touched
`KBotAdmin/Views/CommandsView.vb`, `CommandsView.Designer.vb`, `KBotAdmin/Server/Dtos.vb` (`CommandInfo.Param`, `CommandBlock.Raw`), `PYTHON/routes/admin_cli/admin_cli.py`, `_Admin` (published locally with `publish-admin.ps1`). `sql/KE_01_02_fisa_angajament.sql` (new), `docs/worklog/state/KBOT_STATUS_KE.md`, `docs/worklog/KBOT_STATUS.md` (KE row).

## Test results
`dotnet build KBotAdmin`: 0 errors, 0 warnings; `py_compile` of `admin_cli.py`: ok; `publish-admin.ps1` ok (`_Admin/KBotAdmin.exe`). Nothing run against the server or seen on screen; no test written or run.

SQL: Not run: no database reachable from this PC and the operator's rule is no test runs. Read against `MariaDB_Schema/000_DEMO.sql` (dump of 22.09.2026), which does NOT yet contain `EF_PrimiteReceptii` / `EF_PrimiteLinii.IdClsf` (KE01); those come from `sql/KE_01_primite_receptii.sql`.

## Unverified / deferred
- The syntax was never executed on a server: the first run may need a fix (the dynamic SQL uses double-quoted literals, the session's ANSI_QUOTES is switched off during the call and restored at the end; the `\\1` of the CUI normalisation is the delicate spot).
- Rights not checked: the account must have CREATE ROUTINE on AVACONT_COMUN once, and EXECUTE + CREATE TEMPORARY TABLES on it + SELECT on the unit database to call.
- To use it from KBotAdmin: (1) run `sql/KE_01_02_fisa_angajament.sql` on AVACONT_COMUN; (2) push `admin_cli.py` to the server with AvacontPush (the new command and `--param` live there).
- Not in the schema sync, not for the clients. No help entry: nothing the operator sees changes.
