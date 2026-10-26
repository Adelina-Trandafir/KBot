# SLICELESS - KBotAdmin «Istoric md» page + KBotGraphView (26.10.2026)

## What changed and why
The operator wanted to see how the project evolved ONLY through its markdown files, per slice and per sub-slice, with the files and how they link to each other (owner's tool only, never for clients). Built in KBotAdmin (never published):

- **`KBotAdmin/Docs/MdCorpus.vb`** - `MdScanner.Scan(root)`: every .md (git `ls-files --cached --others`, else the disk), the days each was touched (one `git log --name-only --no-renames`, plus the write day of files modified and not committed), title, the written `](...md)` links, slice / sub-slice / family / folder from the file name and path, implied link worklog -> registry file (`StateResolver`: the index of `KBOT_STATUS.md` names «AV» where worklogs say «AV01», so state files are also matched by their own names, e.g. `KBOT_STATUS_0120-0129.md`, `ADECHIT_STATUS_ADE0-ADE9.md`).
- **`KBotAdmin/Views/MdHistoryView(.Designer).vb`** - ribbon Monitorizare > Proiect > «Istoric md» (key `istoric-md`, icon `md_history` = Brick-link). Views: «Cronologie pe felii» (`KBotLaneView`, a lane per slice then per folder, a dot per file at the day it appeared, optional hollow dots for later modifications) and «Graf în timp» (`KBotGraphView`, a time cursor = `KBotScrollBar`, day by day; shows only files that existed on that date, rings those touched in the 7 days up to it). Family filter, «Citește din nou», «Deschide fișierul».
- **`KBot.Controls/Graph/`** (new family): `KBotGraphView` (+ `.Painting`), `KBotGraphNode.vb` (node + edge), `KBotGraphLayout` (deterministic cluster packing + Fruchterman-Reingold on a grid). Doc `KBotGraphView.md`, row in `CONTROLS.md`, folder row in `README.md`. KBot.Controls FileVersion 1.72.0.0 -> 1.73.0.0; KBotAdmin 1.0.0.0 -> 1.1.0.0.
- `MainForm.vb` / `MainForm.Designer.vb`: page registered (`_views("istoric-md")`, button, group).

## Corrective (26.10.2026, operator)
- The general tooltips of the two surfaces (lanes, graph) are gone; only the per-point labels remain.
- The per-point label now carries a small formatted preview of the file: `KBotAdmin/Docs/MdPreview.vb` turns the start of the md (max 14 lines / ~1200 chars, first `# ` title left out) into `KBotToolTip` markup: headings and **bold** bold, `code` italic, bullets with a dot, table rows as «a · b · c», links reduced to their text, HTML comments and code-fence lines dropped. No colours written. `MdFile.Preview` is filled during the scan.
- The preview label no longer times out (`AutoPopDelay = 0` on `laneView.MarkerTooltip` and `graphView.PointTooltip`, set in the view constructor); it still closes when the pointer leaves the dot.

## Second page: «Schema bazelor» (26.10.2026, operator)
- `KBotAdmin/Docs/SchemaCorpus.vb` (`SchemaScanner`): reads the table structures out of `MariaDB_SchemaAVACONT_SURSA.sql` and `AVACONT_COMUN.sql` (Navicat dumps; a CREATE TABLE counts only right after the DROP of the same name, so SQL text inside data rows is ignored): columns, primary key, index count, foreign keys; links one per pair, never self. Expected from the dumps: SURSA 156 tables / 265 FK, COMUN 50 tables / 18 FK.
- `KBotAdmin/Views/SchemaGraphView(.Designer).vb`, ribbon Monitorizare > Proiect > «Schema bazelor» (key `schema-graf`, icon `database`): a dot per table (clustered and coloured by name prefix inside each schema, bigger with more links), a line per foreign key; filter Ambele / SURSA / COMUN, «Doar tabele cu legături», «Citește din nou»; the preview label lists the columns (key columns bold, foreign-key columns with the table they point at) and does not time out; the bottom line gives the columns and neighbours of the table under the pointer.
- Limits: SURSA and COMUN are separate schemas, so NO line joins the two (no foreign key can; guessing by column names was left out). The dumps are from 22.09.2026 and refreshed by hand (folder git-ignored): the page says the date of each dump in the status line. Never seen on screen.

## Third page: «Evoluția schemei» (26.10.2026, operator)
- Source chosen by the operator: the server's own journal `schema_diff_log` (table in AVACONT_COMUN; 14,736 rows in the dump, the last of 26.10.2026 18:52), with 000_DEMO selected by default (1,051 rows, 20.08.2026 to now, ~300 tables). It records every DDL statement the schema sync planned with `created_at` / `executed_at` / `error_msg`, so the dates are the REAL run dates (not file dates). Scripts run by hand from `sql/` do NOT appear there; the loose .sql files of the project were NOT read (a possible second source, dates would be the file/git date).
- `KBotAdmin/Docs/SchemaLog.vb` (`SchemaLogScanner`): reads only the `INSERT INTO schema_diff_log` lines of `AVACONT_COMUN.sql` (one per row, 13 values, escapes undone; a row of another shape is skipped and counted).
- `KBotAdmin/Views/SchemaHistoryView(.Designer).vb`, ribbon Monitorizare > Proiect > «Evoluția schemei» (key `schema-istoric`): a lane per table (ordered by first change), a dot per statement at `executed_at` (else `created_at`): full = ran, hollow = not run or failed, cross = a DROP that ran. Filters: database (with counts) and «Arată» (all / tables / columns / indexes and keys / other). The label shows the statement (first 10 lines), the state and any error, and never times out.
- Limit: the page reads the DUMP, so it is only as fresh as the last dump (the status line says when it was written). The first sync of a database puts all its tables on one day.

## Corrective (26.10.2026, operator): a «Familie» combo on both schema pages
- Same look as the one of «Istoric md» (`Toate (n)`, then each family with its count, biggest first). Here the family is the PREFIX of the table name (AD, FX, TZ, CHAT ...; `SchemaScanner.PrefixOf`, no prefix = «(fără prefix)»). «Schema bazelor» counts tables of the chosen schema; «Evoluția schemei» counts statements of the chosen database. Changing the schema / database refills the list and keeps the family when it still exists.

## Files touched
`src/KBot.Controls/Graph/*` (new), `src/KBot.Controls/{CONTROLS.md,README.md,KBot.Controls.vbproj}`, `KBotAdmin/Docs/MdCorpus.vb` + `KBotAdmin/Docs/MdPreview.vb` (new), `KBotAdmin/Docs/SchemaCorpus.vb` + `KBotAdmin/Docs/SchemaLog.vb` + `KBotAdmin/Views/SchemaHistoryView*.vb` + `KBotAdmin/Views/SchemaGraphView*.vb` (new), `KBotAdmin/Views/MdHistoryView*.vb` (new), `KBotAdmin/{MainForm.vb,MainForm.Designer.vb,KBotAdmin.vbproj,README.md}`.

## Test results
`dotnet build` of `KBot.Controls` and `KBotAdmin`: 0 errors, 0 warnings. No tests written or run (operator rule). Checked by reading only: `git log` output shape, 119 index rows of `KBOT_STATUS.md` match the index regex.

## Unverified / deferred
- NEVER seen on screen: the layout constants of `KBotGraphLayout` (Spacing 26, pull to the group centre 0.25, 140 passes), the look of the graph, the lane captions at LaneHeight 18, the page layout.
- Dates are only as good as the commits: 583 md files were touched on 26.10.2026 and 399 on 25.10.2026 (documentation commits), so the last week of the cursor rings many files. A first-add date of a file renamed in git is the date of the rename (`--no-renames`).
- The scan runs on every «Citește din nou» (git + ~800 files, a few seconds), not cached.
- Not in the K-BOT help (KBotAdmin is the owner's private tool).
