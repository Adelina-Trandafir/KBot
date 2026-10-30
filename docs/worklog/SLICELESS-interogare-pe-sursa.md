# SLICELESS - KBotAdmin «Interogare pe SURSA» (27.10.2026)

Owner's tool only (KBotAdmin, never published). A one-off SQL file is run on `AVACONT_SURSA` alone, checked, shown in the schema graph, and only on confirmation carried to the units.

## How it works
Page «Interogare pe SURSA» (ribbon Baze de date > Rulare, key `sursa`):
1. **«Citește»** - the file text goes to the server (`source-read`), which lists what each statement touches: table, column, index, primary key, foreign key, or data. Nothing runs.
2. **«Vezi (nu execută)» / «Rulează pe SURSA»** - the existing runner with `--only-source`: the template only, units untouched. The text stays in the ledger `Interogari_Unice` of SURSA (nothing is kept locally).
3. **Check** - after a run, and on «Verifică în SURSA», the server replays the objects to their final state and compares them with `information_schema` of SURSA (`source-check`): exists / LIPSEȘTE / MAI EXISTĂ / cannot be checked (index or key without a name).
4. **Dump** - after a successful run the structure of SURSA (`SHOW CREATE TABLE` of every table, `source-dump`) is written over the existing `MariaDB_Schema\AVACONT_SURSA.sql`, which «Schema bazelor» reads (press «Reîncarcă» there). Only an existing file is replaced; the previous one is kept as `AVACONT_SURSA.anterior.sql`. The graph and lanes pages are unchanged.
5. **«Sincronizează unitățile»** (on the row chosen in the history) - check in SURSA, the `schema_sync` plan on all units with nothing executed, a confirmation (a second one when the plan is destructive), the run, then `--mark`: the query is written in each unit's ledger WITHOUT executing it, so «Interogări unice» never runs it there again.

**Data statements:** `schema_sync` carries structure only. A query with `INSERT` / `UPDATE` / `DELETE` is synchronized for its structure and is NOT marked; marking would hide it from «Interogări unice» on units that never got the data. Its data part is run on the units from «Interogări unice».

**«Interogări unice»** (existing page, changed): no databases grid of its own; the databases to run on are the ones ticked in the main tree on the left (`AdminContext.CheckedDatabases`). The query text is edited in a `KBotSqlBox` (coloured SQL).

## Files
- `PYTHON/routes/one_time/runner.py` - `--only-source`, `--mark`.
- `PYTHON/routes/one_time/sql_objects.py` (new) - statement reader and checker.
- `PYTHON/routes/admin_cli/admin_source.py` (new) + `admin_cli.py` - verbs `source-read`, `source-queries`, `source-check`, `source-dump`.
- `KBotAdmin/Views/SourceQueryView(.Designer).vb`, `KBotAdmin/Docs/SourceDumpFile.vb` (new); `KBotAdmin/Views/OnceQueryView(.Designer).vb`; `KBotAdmin/Server/{Dtos.vb,OnceQuery.vb}`; `KBotAdmin/{MainForm.vb,MainForm.Designer.vb,KBotAdmin.vbproj,README.md}`.
- `KBotAdmin/Views/SyncView.vb` - three regexes `Private` -> `Friend`, reused by the new page.

## Test results
`dotnet build KBotAdmin\KBotAdmin.vbproj`: 0 errors, 0 warnings. The Python files pass `py_compile`. No tests written or run (operator rule). Operator, 27.10.2026: it seems to work.

## Traps and unverified
- **The server files must be pushed by hand** (`runner.py`, `sql_objects.py`, `admin_source.py`, `admin_cli.py`); the program asks the server for the new verbs and fails until they are there.
- **The dump is structure only.** The rows the old Navicat dump held (`Interogari_Unice`, `Setari`, `AD_Lock`) are only in `AVACONT_SURSA.anterior.sql`, one generation. Views are not dumped (the server names them; the old dump had none). `AUTO_INCREMENT=` values are the current ones.
- **«Evoluția schemei» is not fed by this.** It reads `schema_diff_log` from the COMUN dump (10 MB), refreshed by hand; the sync's journal reaches it only then.
- **No separate «refă dump» button.** If the dump fails after a run (folder or file missing) the page says so; running the same text again is skipped by the ledger.
- **The SQL reader is regex-based.** A statement it does not understand shows as «Date» with an empty table, never a guess. An index or foreign key without a name cannot be checked.
- **`--mark` runs after a sync that exits 0;** it does not compare each unit's structure itself.
- The dialog opened by a double click in the history (`SqlTextForm`) still uses a plain text box.
- Not in the K-BOT help (KBotAdmin is the owner's private tool).
