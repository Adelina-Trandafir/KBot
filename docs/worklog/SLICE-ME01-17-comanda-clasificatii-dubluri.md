# SLICE-ME01-17 - KBotAdmin «Comenzi»: clasificatii dublate (03.11.2026)

Owner's tool only (KBotAdmin, never published). Same pattern as ME01-11/13: a read-only check, then one button that writes.

## What changed and why
Classifications were transferred into a database more than once, leaving several `Clasificatii` rows with the same unit and the same Capitol/Subcapitol/Articol/Alineat. New command pair, listed under «_FARA APLICATIE» (shared by FOREXE and AVACONT):

- **«Clasificatii dublate»** (`clasificatii_dubluri`, read only, also on all databases). A group = same `IdUnitate` + same four parts (compared trimmed, case-insensitive). A row is **linked** when its `IDClsf` is held by any other table of the database: every column called `IdClsf` of a base table plus every column a foreign key points from to `Clasificatii.IDClsf` (any schema), except `Clasificatii` itself and `_snap_*` copies. The detail lists each row with its links («Table (n rows)») and what would happen: `RAMANE - legat` / `RAMANE - nelegat, pastrat din grup` / `SE STERGE`. The summary column `reparabile` = rows that would be deleted.
- **«Repara clasificatii dublate»** (`clasificatii_dubluri_repara`, hidden from the list, WRITES, one database, button «Repara» after the check). One transaction (READ COMMITTED): the rows of every duplicate group locked, the links read again under the lock, linked rows never touched, every unlinked row of a group deleted except that a group with no linked row keeps one (a real `IdClsfAcc` first, then the oldest `IDClsf`). Before the commit the row counts of every linking table are compared with those from before: any difference (an unexpected cascade) aborts and rolls back. Deleted rows are written in full to the Jurnal. The result shows what was done and the check again.

## Files touched
- `PYTHON/routes/admin_cli/clasificatii_dubluri.py` (new) - `_plan`, `check`, `repair`.
- `PYTHON/routes/admin_cli/admin_cli.py` - the two commands, `SINGLE_DB_COMMANDS`.
- `KBotAdmin/Views/CommandsView.vb` - constants, `HiddenCommands`, `ConfigureRepair` and `BtnRepair_Click` cases, expectation text.
- `KBotAdmin/KBotAdmin.vbproj` - FileVersion 1.2.0.6.

## Test results
`dotnet build KBotAdmin\KBotAdmin.vbproj`: 0 errors, 0 warnings. `py_compile` clean on both Python files. No tests written or run (operator rule); nothing run against a database.

## Unverified / deferred
- Not run on the server: the SQL (`<=>` join, `information_schema` queries) was checked only by reading the real DDL in `MariaDB_Schema/000_DEMO.sql`.
- A column that points to `Clasificatii` under another name and without a foreign key is not seen as a link. Tables holding the Access id (`IdClsfAcc`) are not counted as links, because duplicates share it.
- Only `Clasificatii` (expenses); `Clasificatii_Venituri` is not covered.
- Not registered for the server (py file): to be done after Adelina's test.
- No help entry: KBotAdmin is not part of the in-app help.

## Correction 03.11.2026 - rectificari do not protect
Rule from Adelina: a duplicate linked ONLY from `Clasificatii_Rectificari` may be deleted; a link from FX_, EF_ (or the budget, any other table) still stops it. `Clasificatii_Rectificari.IdClsf` is a RESTRICT foreign key, so the rectificari of a row to delete are dealt with first, against the row that stays (the linked one, else the kept one): no rectificare with the same Data + Document on it -> moved there; the same Data + Document with the same four quarters -> a copy, dropped; same Data + Document with other quarters -> the row is NOT deleted («RAMANE - rectificari diferite de ale randului pastrat»). Moved and dropped rectificari are written to the Jurnal. The after-check of row counts now allows only the dropped copies. Build clean (0/0), `py_compile` clean; still not run on a server. The «move / drop copies» handling is my choice (she said only that they may be deleted): it keeps budget corrections instead of losing them.

## Correction 2 03.11.2026 - the budget does not protect either
First run on 045_CTER: 54 groups, 108 rows, `reparabile` 0, so no «Repara» button. Cause: every row (original and copy) holds `Clasificatii_Buget` rows and the code still counted that as a protecting link. Adelina's rule is that only FX_ / EF_ links stop a deletion, so `Clasificatii_Buget` is now handled like `Clasificatii_Rectificari` (table `SOFT_TABLES`, key An + DataInceput for the budget, Data + Document for rectificari): its rows move to the surviving row, a copy (same key, same four quarters) is dropped, same key with other quarters keeps the duplicate. The budget FK is ON DELETE CASCADE, so the rows are always dealt with before the delete, and the row-count guard still aborts if anything else disappears. Any other linking table (TZ_, Parteneri_Coduri, FX_Indicatori_Buget, ...) still protects; if you want those soft too, say which. Build clean, `py_compile` clean, not run.

## Correction 3 03.11.2026 - back to the plain rule
The move / compare / refuse-on-difference handling of the rectificari and budget rows (corrections 1 and 2) was not what Adelina asked for, and it left row 206 of 045_CTER undeleted («diferit de randul pastrat»). The rule is only: a duplicate with a link from FX_ or EF_ (or any table other than `Clasificatii_Rectificari` / `Clasificatii_Buget`) stays; one without is deleted, together with its rows in those two tables (written to the Jurnal first, then deleted explicitly because the rectificari FK is RESTRICT). The row-count guard now expects those two tables to lose exactly the rows of the deleted classifications and nothing else. Build clean, `py_compile` clean, not run. Other linking tables (TZ_Oper, Parteneri_Coduri, ...) still protect a row; not asked to change.
