# SLICE-AV01-17 - «Sabloane»: classification per line, accounts from the chart of accounts, «Operatii diverse» table (operator, 29.10.2026)

## What changed and why
Operator's list for the two operation pages of the «Sabloane» window, plus the correction that **`TZ_SablonOperatiiLinie` must hold `IdClsf`** (the link to the
classification): the classification of `TZ_SablonOperatii` (`Cheie`) is for **type M only**; type A has none on the header but may have one **per line**.

**Data**
- New columns on `TZ_SablonOperatiiLinie` (DDL `sql/AVACONT/Configurari/SablonOperatii/AV01_17_sablon_linie_clasificatie_analitic.sql`): `IdClsf` (FK to `Clasificatii.IDClsf`,
  `ON DELETE RESTRICT`), `AnaliticDebit`, `AnaliticCredit`. The account of a line stays the BASE account; base + analytic picks the account in `TZ_PlanCont`.
- Server (`sablon_operatii.py`): GET returns `idclsf` / `analiticdebit` / `analiticcredit` per line, `conturi_plan` (the unit's chart of accounts, one row per symbol and
  sector+source) and `clsf` (every concrete classification). POST: type A sends `idclsf` per line; **type M gets `IdClsf` from the server** (key + form of the line + sector+source of the
  journal, NULL when no row matches). Flags O / P / C on type A need a classification on the line; PA / Ordonantare vs the kind (E / P) are checked on type M only. The analytic must exist
  in `TZ_PlanCont` for that base account.
- Domain/Api: `SablonLinieInfo.IdClsf/AnaliticDebit/AnaliticCredit`, `SablonClsfInfo`, `SablonContPlanInfo`, catalog `ClasificatiiLinii` / `ConturiPlan`.

**Grid (`KBot.Controls`)**: `KBotDataColumn.ComboDropDownWidth` (logical px, min width of the opened list; 0 = the cell) and the event `KBotDataView.ComboItemsRequested` (a per-cell list; the
accounts depend on the row). Both are generic.

**«Operatii cu clasificatie» (type M)**
1. The «Clasificatia» column is gone from this table: «Forma» is shown instead.
2. On «Modifica» the forms of the classification that have no line yet show as empty rows (nothing can be added by hand).
3. Account / partner lists open wider than the cell (`ComboDropDownWidth` 460 / 400, set in the Designer).
4. Account combos list the unit's chart of accounts for the sector+source of the journal, with analytic: the accounts not tied to a classification and the ones tied to the row's classification
   (ClsfF = chapter + form, ClsfE = article + paragraph, e.g. 401010002A650402200101); the ones tied to another classification are left out.

**«Operatii diverse» (type A)**
1. The table takes the whole width under the tree and the header fields (the tree is shorter by as much): `ArrangeForType` moves it at `Bind` (percent rows 70.3 / 29.7).
2. 25 % shorter than the M table (312 -> 234 at the 144-dpi designer = 29.7 % of the body).
3. A «Clasificatia» column, editable, a tree combo (`KBotComboGroupedItem`: article as header, paragraphs under it; face text = the concrete key).
4. The «+» under the table and its footer are gone (`FooterVisible = False` for both types).
5. An empty row at the end: typing in it makes a line and a new empty row follows.
6. The O, P/A, C ticks, as on type M (a tick needs a classification on the line; clearing the classification clears them).
7. Partner combos have «VARIABIL» (the partner of the operation, `OPERATIE` mode); «XXX» is not shown any more (also the type-A text constant).
8. Account combos as above, but with a classification on the line only its accounts show; **without a classification every account of the source shows** (those tied to a classification once, as the base symbol).
   Changing the classification clears the accounts that no longer fit.

**`AVC.Migrator`, step `SablonOperatii`** (the Access `DefaNote` -> the new columns):
- `DefaNote.Clsf` (the Access `IDClsf` of the line) -> `IdClsf` through `Clasificatii.IdClsfAcc` + `IdUnitate`; type M and type A both keep it. Type A keeps no form, and keeps the three flags only when the line has a classification;
  a type-A line whose classification is not in `Clasificatii` leaves the template out (named in the verification).
- The analytic is what follows the dot of the Access symbol (`421000002A.01` -> base 4210000 + `01`) -> `AnaliticDebit` / `AnaliticCredit`; it must exist in `TZ_PlanCont`. At Verify a missing one is only noted
  (the `PlanCont` step of the run writes it first; the step now depends on `PlanCont`), at the write it leaves the template out.
- Verify blocks when `TZ_SablonOperatiiLinie` lacks the three columns (names `AV01_17`) or `TZ_PlanCont` does not exist.
- Templates already written by an earlier run are not touched (as before), so their lines keep `IdClsf` NULL until the template is saved from the window.

**Both tables - minimum to save**: a row needs BOTH accounts (debit and credit); any row missing either one is cancelled before the save (not sent, whatever else was typed in it) and the table is drawn again from the lines that are left (also after a failed save). The server already refused a line without both accounts.

## Files touched
`PYTHON/routes/avacont/configurari/sablon_operatii/sablon_operatii.py`; `sql/AVACONT/Configurari/SablonOperatii/AV01_17_*.sql` (new);
`KBot.Domain/.../SablonOperatiiInfo.vb`; `KBot.Api/.../ApiClient.SablonOperatii.vb`;
`AVC.Migrator/Configurari/SablonOperatii/AvcSablonOperatiiStep.vb`;
`KBot.Controls/DataView/KBotDataColumn.vb`, `KBotDataView.Editing.vb`, `Events/KBotComboItemsEventArgs.vb` (new);
`KBot.App/Avacont/Configurari/SablonOperatii/Views/SablonOperatiiView.vb` + `.Designer.vb`; help `HelpContent/contabil/nomenclatoare/sabloane.md` (0000-138).

## Test results
`dotnet build` KBot.Api, KBot.App and AVC.Migrator: 0 warnings, 0 errors. `py_compile` of `sablon_operatii.py`: OK. `Check-Help.ps1`: nothing reported for `sabloane.md`. No tests run (operator rule); nothing seen on screen.

## Unverified / deferred
- **DDL `AV01_17` NOT RUN.** Until the operator runs it and syncs the schema, the GET of the templates fails (it reads the new columns). Server not touched.
- **Nothing was seen on screen**: the full-width table under the tree, the 70 / 30 split, the tree combo, the wider lists, the per-row account lists.
- «Cu dublu» for the list width was read as «twice the cell»: the lists are 400-480 logical px (the cells are 200). Change `ComboDropDownWidth` in the Designer if another width is wanted.
- The accounts are matched by base account + analytic. A plan row tied to a classification is shown with its full symbol only when the row's classification is known; the stored value stays base + analytic,
  the classification part is composed when the template is used (stage 2).
- Class 8 accounts are NOT left out (the Access list excluded them; not asked here).
- The account lists use the sector+source of the operation's **journal**; changing the journal of an operation does not re-check the accounts already chosen.
- The migrator step was NOT run (needs the Access files, the DDL `AV01_17` and the steps `PlanCont` / `Jurnale`); the `.`-analytic rule is read from the symbol format in the schema comments (`421000002A.01`), not seen in real Access data.
- Existing type-M lines have `IdClsf` NULL until their template is saved again.
