# SLICE-AV01-04 - Plan de conturi: subunitati si conturi (stage 1 of the AVACONT port)

Date: 24.10.2026. First CODE of the AV family. Design: `docs/access-map/avacont/proiect-de-date-etapa-1.md` (sections 3 and 7),
sheet `conturi-plan-de-conturi.md`. The operator said «hai sa incepem» after the data design; the design was not
re-confirmed table by table, so the tables below are the PROPOSAL turned into DDL (changeable).

## What changed and why
- **DDL** `sql/AVACONT/Configurari/PlanCont/AV01_04_conturi_sursa.sql` (for `AVACONT_SURSA` only; the schema sync creates the tables in the units, no
  registration is needed - `schema_diff._tables`): `TZ_Subunitati` (chapter, sector+source, salary decimals, default flag),
  `TZ_PlanCont` (per subunit; base account = FK `AVACONT_COMUN.DefaCont.Simbol`; ClsfF / ClsfE FKs; symbol unique per subunit),
  `TZ_SoldInitial` (year + account [+ partner]; created now because the account delete rule depends on it, no code writes it yet).
  Every foreign key is `RESTRICT`.
- **Server** `PYTHON/routes/avacont/` + `configurari/plan_cont/` (new blueprint `avacont_bp`, registered in `main.py`): `subunitati.py`
  (`GET/POST /api/avacont/subunitati`, `DELETE .../<id>`), `conturi.py` (`GET /api/avacont/conturi/nomenclator`,
  `GET /api/avacont/conturi?subunitate=`, `POST`, `DELETE .../<id>`), `common.py`. The symbol is composed on the server:
  `Cont + SursaSector + [ClsfF] + [ClsfE] + [.Analitic]` (checked against the Access plan: `1170000` + `01A` = `117000001A`).
  Rules kept from Access: unique symbol, analytic only digits / capitals / `_` (10), confirmation (reason `CONFIRMARE_ANALITIC`)
  for an analytic on 401* / class 6 / class 7. Rules decided: delete BLOCKED for an account used anywhere (with the place named),
  symbol of a used account cannot change, the first subunit is the default, the default cannot be deleted or unmarked directly,
  sector+source cannot change once the subunit has accounts. Classification OPTIONAL when the base account allows it (operator, 24.10.2026: «optionale dar sugerate»; the window shows the most used code of that base in the chart as a hint), refused when it does not, ClsfE never without ClsfF
  (checked on the dump: 528 none / 131 ClsfF / 159 both / 0 ClsfE only).
- **Client**: `KBot.Domain/ConturiPlan.vb`, `KBot.Api/IConturiApi.vb` + `ApiClient.Conturi.vb`.
- **Window** `KBot.App/Avacont/Configurari/PlanCont/`: `PlanContForm` (tree class > base account > full account, fields, subunit bar) and
  `SubunitateEditForm` (dialog). Glue: `KbotForm.PlanCont.vb`, machete `plan_cont`, ribbon button «Plan de conturi»
  (Configurari > Date), `AvacontMain.Ribbon_ItemClicked`. (The name `ConturiForm` was already taken by «Conturi Unitate» of E-Factura.)
- **Help** 0000-123: topic `contabil.nomenclatoare.plan-conturi`, `nomenclatoare/index.md`, `ribbon.md`.

## Second pass (operator, 24.10.2026): the window as in the Access screenshots
- **Two views** in `KBot.App/Avacont/Configurari/PlanCont/Views/`: `PlanContConturiView` (tree Clasa > Cont de baza > ClsfF > ClsfE > cont complet; fields of the account with Sector / Sursa read from the subunit; ClsfF / ClsfE as an optional tick + combo with a suggestion; multi-line name; opening balance and cumulated turnovers) and `PlanContSolduriView` (DGV, no empty row, columns Simbol / S.I. Debitor / S.I. Creditor / R.P. Creditor / R.P. Debitor as in the screenshot; an edited row is saved on row change, also when the view is left or the window closed). `PlanContForm` is only the shell (subunit bar, the «Afiseaza solduri initiale» toggle, one view at a time; in the balances view the tree AND the right-hand panel are hidden), `PlanContContext` holds what both share, `IPlanContView` is their contract.
- The search combo of Access is gone: the search is the one in the tree header (as in the main window).
- **Server / DDL**: `TZ_SoldInitial` got `RulajDebitor` / `RulajCreditor` (the operator needs the turnovers: the earlier plan to drop them is withdrawn; the DDL file has an `ALTER ... ADD COLUMN IF NOT EXISTS` for a table already created); new route file `solduri.py` (`GET/POST /api/avacont/conturi/solduri`); client `GetSolduriAsync` / `SaveSoldAsync`.
- **Not done on purpose**: «Exporta in Excel» (the Access button opened an empty Excel; there is no real export to port - to decide), the «Modifica» button of Access (the fields are edited directly). The column order R.P. Creditor before R.P. Debitor was kept as in the screenshot - REVERSED in the third pass (debit first).

## Third pass (operator, 25.10.2026): feedback on the window
- **Classifications**: no suggestion any more. Ticking a box enables its combo and the operator fills it; unticking switches the combo off and empties it (null). ClsfE is possible only with ClsfF ticked. The tree already hangs a full account under the deepest level it has (accounts with no ClsfE, or none at all, sit directly under the ClsfF / base account).
- **«Modificare»**: the fields of the shown account are locked (read-only / disabled) until «Modificare» is pressed (a new account via «Adaugare» is editable at once). «Salvare» / «Renunta» lock them again; «Stergere» works only while not editing. Reason: accounts were once changed by accident.
- **Sector / Sursa follow the title bar (SLICE-0097-06)**: `PlanContContext` now takes the `SessionContext`; a new account takes `SectorSursa` of the title bar and the two fields only show it; when `ToateSursele` is on (or the title bar value is not a known code) they are two combos the operator fills. An existing account shows its own. The form listens to `SessionContext.PeriodChanged` (`RefreshSs`). **Model change**: the sector+source now belongs to the ACCOUNT (`TZ_PlanCont.SursaSector`, wire `sursasector`); the subunit value is only the default of a new account on «Toate», so the server no longer freezes it when the chart is not empty and the subunit dialog no longer locks it.
- **DDL to run**: `sql/AVACONT/Configurari/PlanCont/AV01_04b_plancont_sursasector.sql` on `AVACONT_SURSA` (adds the column, backfills it from the subunit, makes it NOT NULL with the foreign key), then «Sincronizare schema» on the units. `AV01_04_conturi_sursa.sql` has the column in its CREATE too. The earlier question «must I run something on MariaDB?» - yes: the DDL on `AVACONT_SURSA` (done for the first script) and the schema sync to the units.
- **DEBIT before CREDIT, always**: the balances grid is now S.I. Debitor, S.I. Creditor, R.P. Debitor, R.P. Creditor (keys `rpd` before `rpc`); the panel fields and the Excel have the same order.
- **«Exporta in Excel»** (bottom bar of `PlanContForm`): `PlanContExcel` writes a real .xlsx by hand (zip of XML parts, no library): symbol, base account, sector, source, ClsfF, ClsfE, analytic, name, type, active, S.I. D/C, R.P. D/C; amounts are numbers with the `#,##0.00` format; header bold and frozen, filter, a «Total» row with SUM formulas. Inactive accounts only when the box shows them.

## Folder layout (operator, 24.10.2026)
Rule: `Avacont/<Category>/<Module>/` in every project, with the windows in `.../<Module>/Views/`; only what several modules share stays in the root of `Avacont`. Here: `KBot.App/Avacont/Configurari/PlanCont/` (`Views/` = the window, its two views and the dialog; `PlanContContext.vb` and `KbotForm.PlanCont.vb` next to it), `KBot.Domain/Avacont/Configurari/PlanCont/`, `KBot.Api/Avacont/Configurari/PlanCont/`, `PYTHON/routes/avacont/configurari/plan_cont/` (+ `common.py` in the root), `sql/AVACONT/Configurari/PlanCont/` (the `DefaClsfV` script of AV01-03 is in `.../Configurari/ClasificatiiVenituri/`). Files moved, not renamed; no namespace blocks, so no code changed.

## Files touched
`sql/AVACONT/Configurari/PlanCont/AV01_04_conturi_sursa.sql`; `PYTHON/routes/avacont/{__init__,common}.py`, `PYTHON/routes/avacont/configurari/plan_cont/{subunitati,conturi,solduri}.py`, `PYTHON/main.py`;
`src/KBot.Domain/Avacont/Configurari/PlanCont/ConturiPlan.vb`; `src/KBot.Api/Avacont/Configurari/PlanCont/{IConturiApi,ApiClient.Conturi}.vb`;
`src/KBot.App/Avacont/Configurari/PlanCont/Views/{PlanContForm,PlanContConturiView,PlanContSolduriView,SubunitateEditForm}{,.Designer}.vb`, `Views/IPlanContView.vb`, `PlanContContext.vb`, `KbotForm.PlanCont.vb`; `src/KBot.App/{KbotForm.Machete,AvacontMain,AvacontMain.Designer}.vb`;
help files above.

## Test results
`dotnet build src/KBot.App/KBot.App.vbproj`: 0 warnings, 0 errors. Python files: `py_compile` only (syntax). **No test, no run, nothing on the
server, no DDL run, nothing seen on screen.** `Check-Help.ps1`: only older messages remain (see 0000-123 worklog).

## Unverified / deferred
- The DDL was never run: not even syntactically. Computed columns and CHECK syntax are standard MariaDB; `PartenerKey` (generated, used to make
  the unique key treat NULL partner as one value) is the least common construct.
- Every ClsfF / ClsfE of the Access plan must exist in `AVACONT_COMUN.DefaClsfF / DefaClsfE` (FKs) - the Migrator will show a miss.
- Opening balances: the grid and the panel fields exist now, never run. Balances per partner (ParteneriSI) have no window yet (the table already allows `IdPartener`).
- No right is checked on these routes (any signed-in user): the AV role rights for accounts are not decided. Only `AV_STERGE_NC` / `AV_STERGE_CLSF` are planned.
- The window is a hand-written Designer, never opened in Visual Studio; sizes copied from `ParteneriForm`. The combo `cboCont` uses `Editable` +
  `FindAsYouType` + `LimitToList` - their behaviour with 818 items is untested.
- The Access tree has 4 levels (class > account > ClsfF > ClsfE > leaf); here 3 (class > base account > full account). Export to Excel was empty in Access: not ported.
- Migration of `PlanCont` from `baza2026.accdb` into `TZ_PlanCont` (Migrator `TableMaps.vb`) is NOT written.
- Capture `plan-conturi` for the help: none taken (the topic has no `capture:` tag).
