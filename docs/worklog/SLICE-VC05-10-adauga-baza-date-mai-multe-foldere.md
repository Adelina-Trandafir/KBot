# SLICE-VC05-10 — «+ Adaugă Baza Date»: fișiere din mai multe foldere

## What changed and why
Alimente, Materiale, OI and MF usually live in different folders (different databases); the user may use all of them or none.
VRC.Migrator could read ONE folder only («Alege folder…» replaced the list). Now:

- New button **«+ Adaugă Baza Date…»** (under «Alege folder…») is a FILE selector (OpenFileDialog, multi-select, .accdb/.mdb, any folder): the picked files are APPENDED to the list.
  Files already in the list (same path) are skipped; the operator's ticks are kept (`SyncSelection`). A picked file outside the tz_ naming becomes kind «Alt nume», ticked (`VrcSourceFile.FromChosenPath`); tz.mdb is still refused.
- «Alege folder…» is unchanged in meaning: it starts the list over with one folder.
- New grid column **Folder** in the file list; `lblFolder` shows the single folder, or `N foldere: a | b | …`.
- The note «același an în două formate» now counts per folder + year (the same year in two folders is two gestiuni, not a duplicate).
- Adding a folder drops the profiles/plan (like choosing a folder): «Citește bifatele» must be pressed again.
- Nothing below changed: plan, runner and writer already identified every source by its full path
  (`VrcPlanSource.Source.Path`), so files from several folders flow through unchanged.

- **Precision (operator, 18.10.2026):** more than 4 decimals is NO LONGER a blocker. Access Double fields do not round correctly (noise like 0.1E+6);
  `VrcConvert.StoredDecimal` now rounds to 4 decimals (away from zero) and refuses only values that do not fit DECIMAL(20,4). The plan shows a WARNING per column
  (was BLOCAJ), the profile report an ATENTIE line. Reconciliation compares converted sums, so rounded values still match.

## Files touched
- `VERCON/VRC.Migrator/VrcMigratorForm.vb` (`LoadFolder`, `btnAdauga_Click`, `SyncSelection`, `ShowFiles`, `UpdateButtons`)
- `VERCON/VRC.Migrator/VrcMigratorForm.Designer.vb` (btnAdauga, Folder column, 7-row button table, tooltip)
- `VERCON/VRC.Migrator/VrcSourceFile.vb` (FromChosenPath), `VrcConvert.vb`, `VrcPlan.vb` (CheckPrecision), `VrcReport.vb`
- `VERCON/VRC.Migrator/VRC.Migrator.vbproj` (FileVersion 0.2.1.0 -> 0.2.2.0)

## Test results
`dotnet build VERCON/VRC.Migrator/VRC.Migrator.vbproj`: 0 warnings, 0 errors. Not run, not seen on screen (no tests, per standing rule).

## Unverified / deferred
- Layout of the button column (6 buttons in the 230 px group) not seen on screen.
- Not checked that the plan rules (VrcPlan.Evaluate) are right for several folders with the same year and different types
  (e.g. Alimente 2025 in folder A + Materiale 2025 in folder B). Next step, after the operator tries it.
- The ANAF partners window and the server-side checks were not touched.

## Addendum 18.10.2026 — OI_Produse.IDM
First run on tz_2100 (MF, 154 rows) was rolled back: 154 × X10 on `OI_Produse.IDM` (all value 2), because the column was `open` (no parent).
Access shows the parent (`OI_Produse INNER JOIN Masini ON OI_Produse.IDM = Masini.IdMasina`, mdl_OI_Produse and reports), so by the operator's decision
`import_map.json` now has `IDM` as `logical` -> `Masini.IdMasina` (like `NIR.IdMasina`, `Iesiri_H.IdMasina`). FileVersion 0.2.3.0.
`OI_Produse.IDBO` (the bon the object came from; operator no longer remembered it, Access code shows: a BP or a BT; the row carrying it is a BT or casare line) is now link kind `origin` (FileVersion 0.2.4.0):
`VrcRunner.ConvertOrigin` looks the value up among the IDBP labels of OI_Predare and the IDBT labels of OI_Transfer of the same source; found as BP -> number of group IDBP, as BT -> group IDBT; in both -> BLOCAJ X14; in neither -> orphan X05; 0/NULL -> NULL.
Unverified: no source base with IDBO filled was available (tz_Obiecte_Inventar.accdb has none), so the rule has never run on real data; not re-run on tz_2100 either. Masini id 2 must exist in tz_2100 (else X05).

## Addendum 18.10.2026 — full path of the file in reports (FileVersion 0.2.5.0)
Several folders can hold files with the same name, so a bare name no longer identifies the file.
- Migration report (Constatari): per finding group a line «Fisier: `<full path>` (N cazuri) — [deschide folderul](file:///...)» for every affected file; the 5 sample lines carry the full path too.
  `VrcIssue.SourceFullPath` (VC_MigrationIssues still stores the name only).
- «Raport HTML» view: only `file:///` links survive; a click is intercepted and opens Explorer with the file selected (`VrcShell.RevealInExplorer`; only .accdb/.mdb or a folder is ever opened).
- Main report text box: a `file:///` link is clicked through `RichTextBox.LinkClicked` (unverified that RichEdit detects `file:` URLs and where it cuts the trailing «)»; the code trims it).
- Plan check messages (blockers/warnings) name the file by full path (`VrcFileProfile.FullPath/Label`); the plan hash and the exported plan JSON keep the name.
Not run, not seen on screen. Profile report (VrcReport) headings still use the file name.

## Addendum 18.10.2026 — re-reading keeps DC and Subunitate (FileVersion 0.2.6.0)
`FillPlanGrid` (after «Citește bifatele») wrote DC and Subunitate from the files every time. Now it fills them only when the field is empty; a value already there stays.
The «numele unității diferă» warning appears only when the name was filled from the files. Not run.

## Addendum 18.10.2026 — Access SQL in the report (FileVersion 0.2.7.0)
Each finding group of the migration report now carries, per field, the Access SQL that shows the exact rows (`VrcAccessSql.ForField`, new file), in a ```sql block to run in each file listed above.
- X10 (link without parent, non-zero): the rows + a «value, Cazuri» count query.
- X05 (orphan): LEFT JOIN against the parent (`parentField` of the map, now parsed into `VrcImportColumn.ParentField`); for `origin` columns: not in OI_Predare.IDBP and not in OI_Transfer.IDBT. Note that the parent may live in another file of the gestiune.
- X14, X11, X12 have their own WHERE; any other BLOCAJ with whole-number keys: `WHERE key IN (...)` (first 100). CONVERSIE and warnings without a rule: none.
Not run in Access: the SQL is generated text, the syntax (Nz, Round, IN subqueries, LEFT JOIN) is standard Jet/ACE but unverified on the real file.
- No table aliases in the generated SQL (operator): the X05 LEFT JOIN uses full table names; a self-join falls back to the key list.

## Addendum 18.10.2026 — everything is remembered between runs (FileVersion 0.2.8.0)
`vrc-migrator-settings.json` (next to the exe) now keeps: server host/port/user (as before), the file list with ticks, DC, Subunitate, the plan grid rows (year, gestiune, type; keyed by file path + the type the tool proposed) and the last folder. **Never the password.**
- Load: files come back (missing ones are dropped and named in the status line), DC / Subunitate refilled, the folder dialogs open on the last folder. Files are not read again: «Citește bifatele» still does that; after it, remembered plan rows win over the tool's proposal.
- Saved after each step (folder, add files, read, server check) and on closing; DC/Subunitate also when the field is left. Plan rows of files not currently in the grid are kept.
Not run; the exact moments of saving and the restore were not seen on screen.

## Addendum 18.10.2026 — OI_Predare.IdMasinaP (FileVersion 0.2.9.0)
Run on tz_2100 stopped with 165 × X10 on `OI_Predare.IdMasinaP` (value 102): the column was `open`. Operator: in OI_Predare only, `IdMasina` = the account the objects go TO, `IdMasinaP` = the account they come FROM, both exist in Masini.
`import_map.json`: `IdMasinaP` is now `logical` -> `Masini.IdMasina` (only occurrence in the map). A value missing from Masini stops the transfer as an orphan (X05), not silently.
No change to the Access SQL generator (the grouped-query idea was rejected/not applied). Not re-run.

## Addendum 18.10.2026 — Iesiri_H.IdRetur (FileVersion 0.2.10.0)
Last `open` column of the map closed by the operator's decision: `Iesiri_H.IdRetur` is a return bon pointing to the original consumption bon (Access: `Iesiri_H INNER JOIN Iesiri_H AS I1 ON Iesiri_H.IdRetur = I1.IdBonConsum`).
`import_map.json`: `logical` -> parent `Iesiri_H`, parentField `IdBonConsum` (a reference to the same table: written as NULL first and set by `ResolveDeferred` once every row has its key; the parent is looked up in the SAME source file, so a return whose original bon is in another year's file stops as an orphan, X05).
Unverified: the same-table path has never run on real data (no base with returns yet); operator will test candidate bases.
No `open` column is left in the map.

## Addendum 18.10.2026 — «Golește lista» (FileVersion 0.2.11.0)
New button `btnGoleste` («Golește lista», between «Debifează toate» and «Salvează raportul»): empties the file list and drops the reading, the plan grid and the report on screen (`ResetAfterFileListChange`), and the empty list is remembered. DC, Subunitate, the server and the remembered plan choices stay. Nothing is deleted from disk. Disabled while busy, with an empty list, or after a confirmed COMMIT.
Layout: the button column now has 7 buttons; the sources group is 262 px high (was 230). Not seen on screen.

## Addendum 18.10.2026 — subunit per database (FileVersion 0.3.0.0)
Operator: several subunits can have the same account (Alimente...), so the subunit name is per database (file), not per plan. Decisions: ONE transaction for all subunits (all or nothing), one import marker per subunit; the global Subunitate field is gone (a «Subunitate» column per plan row, proposed from the file's own UNIT.NumeUnitate, remembered per row); ZEC per subunit; ONE DC for the whole plan.
- `VrcPlanSource.SubunitName`; `VrcPlan.SubunitNames()` / `ZecOf()`; the plan-level `SubunitName` is removed. Checks (type per gestiune, two sources for one year, OI/MF multi-year) are per (subunit, gestiune). New blocker: a file whose rows name different subunits. The plan hash and exported JSON carry the subunit per source and the ZEC per subunit.
- `VrcRunner.CreateContext`: one `VC_Subunitati` + one `VC_Imports` per subunit, gestiuni/exercises/sources under their own subunit (`GestInfo.SubunitId/ImportId`); row writing, key reading, counters, SI_LA, reconciliation, invoices fragments (`AttachHistoricalInvoices(subunitId)`), findings (`ImportOf(issue)` via the source) and `FinishImport` (summary per marker) all use the subunit of the gestiune. `_subunitId` / `_importId` no longer exist. `VrcRunResult.Subunits` lists them; the report names every subunit with its ids and ZEC; counts show «subunit / gestiune» when there are several.
- `VrcWriter.Preflight` checks every subunit name on the server; «Diagnostic» runs per subunit.
- Settings: `Subunit` removed, per plan row `Subunit` added. README updated.
**Not run, not seen on screen, and the transfer code changed: the only server-validated transfer (000_DEMO, VC05-03) is no longer the code that ran. Re-validate on 000_DEMO (single subunit first, then two).** Unverified: the layout of the server row after removing the Subunitate field and the new plan column.

## Addendum 18.10.2026 — subunit proposed from cale.accdb (FileVersion 0.3.1.0)
The subunit of a VERCON file is NOT the unit name stored in the file (UNIT.NumeUnitate = «Liceul Tehnologic Elie Radu» for every folder): it is `Detalii` of the table `cale` of VERCON (cale.accdb; `Cale` = folder relative to the VERCON folder, `AnDate` = year), e.g. «Alimente Cantina», «ISJ», «Local».
- New button «Încarcă cale…» (server row of the plan group): reads the table `cale` (or the only table) of the chosen Access file read-only (`VrcCale.Load`); the path is remembered and read again at start.
- Match (`VrcCale.Find`): the folder of the file must END with `Cale` (VERCON can be anywhere; the longest `Cale` wins; no case/slash difference). A year file prefers the row with its year; a special base (tz_2100: OI, MF) has no year: matched by folder only (a row with 2100 only breaks a tie). Several rows are accepted only if they give the same name; otherwise nothing is proposed. Files that do not match keep the unit name of the file; `Detalii` is used as it is (e.g. «Materiale» for \47).
- Priority in the plan grid: what the operator changed (remembered) > cale > file name. A remembered subunit equal to the file's own unit name counts as an old default, not a choice. Loading cale also updates the rows already on screen that were not changed by the operator.
Unverified: the real table name / column names of cale.accdb (taken from the pasted export: Cale, Detalii, AnDate; the table «cale», else the only table), the layout of the server row, the whole flow on screen.

## Addendum 18.10.2026 — folder of each file and the cale explanation in the plan grid (FileVersion 0.3.2.0)
Operator: the first row proposed «Liceul Tehnologic Elie Radu» although cale says otherwise, and the paths of the read files were not visible.
- Plan grid: new read-only column «Folder» (full folder of the file) after «Fișier».
- «Propunere / observații» ends with `| cale: …`: the Cale that matched and the name taken from it, or why it did not (`folderul fișierului nu se potrivește cu nicio Cale`, or `Cale «…» are mai multe nume (…)`). Loading cale refreshes these notes (`VrcCale.Explain`, one lookup shared with `Find`).
Unverified: the cause of the first row of the screenshot (most likely its folder does not end with a Cale: an orphan test copy); the new column will show it. Not run.

## Addendum 18.10.2026 — button column of the sources group (FileVersion 0.3.3.0)
Removed: «Bifează toate», «Debifează toate», «Salvează raportul…» (and its save dialog). Added «Șterge rândul» above «Golește lista»: the file of the current row leaves the list (nothing deleted from disk); the ticks of the other rows stay; the reading and the plan on screen are dropped (press «Citește bifatele» again). The column is now: Alege folder, + Adaugă Baza Date…, Citește bifatele, Șterge rândul, Golește lista. Not run, not seen on screen.
Note on the screenshot of the list: the Cantina file sits in `...\ALIMENTE_CANTINA\2026`, a year subfolder that cale (`\ELIE_RADU\ALIMENTE_CANTINA\`) does not have, so it does not match: the unit name of the file stays (explained in the note of the plan row).

## Addendum 18.10.2026 — transfer report rewritten in plain words (FileVersion 0.3.4.0)
Operator: the report of a rolled-back run was unreadable (six subunits with ids, an empty counts table, rule codes, the same path repeated, conversion notes drowning the one real problem).
`VrcRunResult.ToMarkdown` rewritten (the first line stays `Verdict`):
- Top: base, subunit names (ids and ZEC only after a COMMIT), duration, then ONE sentence: «N probleme de rezolvat (X cazuri în Y fișiere)» and what to do next.
- «Ce trebuie rezolvat»: one block per problem (rule + table + field): plain title, what is wrong, «Unde» (each file once, full path, folder link, up to 5 example rows as `Key = value: Field = value`), «Ce faceți», then the Access query. Texts per rule in `Describe` (X05 empty required field / origin without bon / missing parent, X10, X14, X11, X12, X03, X13, X06).
- «Doar de informare»: conversions and warnings as short lines (N11 merged: the dish types and the tables they come from).
- Counts table only when there are counts; ANAF partners listed only after a COMMIT (a rollback gets one line).
The Access SQL itself (`VrcAccessSql`) is unchanged. Not run on a real rollback.

## Addendum 18.10.2026 — Iesiri_H.IdRetur without a bon of consum no longer blocks (FileVersion 0.3.5.0)
Operator: in the old program a bon de consum could be deleted (or changed) and the returns made from it stayed in the air; their values are needed in the balance. Decision: the return is transferred with IdRetur NULL; the link with the bon de consum will be added later in the online VERCON application.
- `import_map.json`: IdRetur link gets `"orphan": "null"`; `VrcImportColumn.OrphanToNull` reads it. Generic: any other link can use it later.
- `VrcRunner`: a parent that is not found (deferred same-table link or normal link) no longer gives the blocking X05 for such a column; it gives the CONVERSIE `N12` (`AddOrphanNote`) and the row is written with the link NULL. The values are not touched, so the balance keeps them.
- Report: under «Doar de informare», N12 lists every affected row per file («IdBonConsum 49 (arăta spre 47)»), so they can be linked later.
Not run on the server (the 3 rows of tz_2026 ALIMENTE_CANTINA are the case to check).

## Addendum 18.10.2026 — IntrariStorno.IdIntrare without a NIR no longer blocks (FileVersion 0.3.6.0)
Operator: the storno lines whose NIR is gone (`tz_2100`, ID 7 and 8 pointing at 809 and 965) get the same solution as the returns: the row is transferred with `IdIntrare` NULL, noted as N12 (CONVERSIE). Only `import_map.json` changed (`"orphan": "null"` on `IntrariStorno.IdIntrare`; the target column is NULL-able) plus the wording of the N12 text.
Difference from the returns, said to the operator: a return has its own header, a storno line has none. Online, storno lines are read through their NIR (`stock_source.py`, `ledger.sql`, `receipts.sql` join on IdIntrare), so a line with NULL does not enter stock or ledger until it is linked. The N12 text for IntrariStorno says so. The text no longer claims «valorile rămân în balanță».
Not run on the server.
