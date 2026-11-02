# SLICE-AV01-18 - Operation templates belong to a SUBUNIT (operator, 02.11.2026)

## What changed and why
The subunit is the application's choice (title bar, before «Sursa/Sector»; see `KbotForm.Subunit.vb`, slice 01EF-02). A template of operations used to belong only to a
SOURCE (through its journal), so two subunits of one source shared the templates. The operator's decision: **until the port is finished the templates stay as they were in Access,
where nothing is common between units** - a template belongs to ONE subunit, and a template common to several is NOT offered (the column is NULL-able only so that the rows that
exist when the DDL runs survive it).

**Data**
- DDL `sql/AVACONT/Configurari/SablonOperatii/AV01_18_sablon_operatii_subunitate.sql` (run on `AVACONT_SURSA`, then «Sincronizare schema»): `TZ_SablonOperatii.IdSubunitate`
  (NULL-able, FK to `TZ_Subunitati`, RESTRICT, index). **Nothing run yet.**
- Clean-up `sql/AVACONT/Configurari/SablonOperatii/AV01_18b_stergere_toate_sabloanele.sql` (run on EACH unit database): deletes every operation template AND every salary template
  (`TZ_SablonOperatiiLinie`, `TZ_SablonOperatii`, `TZ_SablonSalariiLinie`, `TZ_SablonSalarii`), resets the numbers; first a read-only check that no operation points at a template
  (`TZ_Oper.IdSablon`, RESTRICT), with an OPTIONAL commented `UPDATE` to forget that link. **Nothing run yet.**

**Server**
- `sablon_operatii.py`: GET returns `idsubunitate` per template; POST requires it, checks the subunit exists and that its `SursaSector` is the one of the journal, and writes it
  on INSERT and UPDATE.
- `primite_contari.py` (`_sabloane(cursor, id_subunitate)`): «Contari» offers only the templates of the subunit in force (`idSubunitate`), and so only the classifications that have
  one; without `idSubunitate` nothing is filtered (as before).

**Client**
- `SablonOperatieInfo.IdSubunitate` + the wire field in `ApiClient.SablonOperatii.vb`.
- The «Sabloane» window is modal: `KbotForm.DeschideSabloane` hands it the subunit in force (`CurrentSubunitId`) and its sector+source (new `CurrentSubunitSs`); `SabloaneForm`
  passes both to the two operation pages (`SablonOperatiiView.Bind`, optional arguments). The page shows only the templates of that subunit (`ShownTemplates`), a new template
  belongs to it, and with «Toate» in the title bar the sector+source of the subunit is the one that shows (`SsFilter`). With no subunit «+» says so and adds nothing.
  The salary page is not touched (its articles already had `IdSubunitate`).

**`AVC.Migrator`, step `SablonOperatii`**: every template is written into the subunit of the unit it comes from; the dedup key and the "already there" test include the unit /
subunit, so the same template in the files of two units is written once PER unit (it was once in total). Verify blocks, with the script name, when the column is missing.
The step `SablonSalarii` already wrote per subunit.

## Order to run (operator)
1. `AV01_18` on `AVACONT_SURSA` -> «Sincronizare schema» on every unit database.
2. `AV01_18b` on each unit database (check first: must show 0).
3. AVC.Migrator again: steps «Sabloane de operatii» and «Sabloane de salarii».

## Verification
`dotnet build` AVC.Migrator and KBot.App 0 warnings / 0 errors; Python `py_compile` on both routes. Nothing run against MariaDB, nothing seen on screen, no test.

## Open threads
- Common templates (a NULL `IdSubunitate` valid for all subunits of a source) are deliberately NOT offered; they come back only when the operator asks, after the port.
- Other Avacont windows (Jurnale, future notes / orders of payment) must still read `CurrentSubunitId`; journals stay per source.
- Salary templates: their window still picks its own subunit.

## Clean-up of the migration (02.11.2026)
`sql/AVACONT/IntroducereDate/Operatii/AV02_01b_stergere_migrare_operatii_si_sabloane.sql` deletes what AVC.Migrator wrote (operations: `TZ_Debit/Credit` of the migrated operations, `TZ_Oper` with
`IdOperatieAccess`, `TZ_Documente` with `IdDocumentAccess` once empty, `TZ_NoteC` with `UN IS NULL` once empty; templates: all salary templates and every operation template no operation points at),
in one transaction; it covers `AV01_18b`. The optional step 1b (commented) turns `EF_PrimiteOperatii.IdOperatie` back into Access numbers and is for a migration that reached COMMIT once. Not run.
The first migration run failed before COMMIT (`AvcSql.TableExists` without the transaction, fixed: optional `k_transaction`), so nothing was written.
