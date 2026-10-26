# SLICE-0105 / 0107 — Clasificații: sumar, filtre și totaluri

Istoric reunit pe tema ferestrei de buget. Etapele din 03.10.2026 păstrează probele și limitele originale. Totalul pe rând din 0107-02 și rândul separat «Buget + rectificări» descriu suprafețe diferite; regulile de credit și total anual sunt amendate ulterior în [0108](SLICE-0102-buget-pe-versiuni.md#slice-0108-01).

- [0105 — sumar și mișcare](#slice-0105)
- [0107-02 — total pe rând și subsoluri](#slice-0107-02)
- [0107 — aliniere, rând final și FOREXE](#slice-0107-buget)

<a id="slice-0105"></a>

## SLICE-0105 — Clasificatii: sumar pe noduri, filtru pe miscare, Verifica buget fara diferente zero

Operator request, 03.10.2026. Help: 0000-42 (`contabil/nomenclatoare/clasificatii.md`).

### What changed and why

1. **Non-leaf node = read-only summary.** Clicking a chapter / sub-chapter / article node of the tree shows the two grids without «Inceput», «Nr. doc.», «Data», the «✕» column, the footer «+», and without editing. They gain a first **Clsf** column that stretches. The budget grid lists every classification under the node with its LAST budget version of the year (greatest `DataInceput`); the corrections grid lists the classifications that have corrections, with the TOTAL per quarter and the Total column. Save / «Trimite in Access» stay disabled (no classification is current).
2. **Tree filter.** Default (and on every opening): only classifications with movement in the year, meaning ANY quarter of ANY budget version or ANY quarter of ANY correction is not zero. Tested quarter by quarter, never through a total (+1000 / -1000 totals 0 and still counts). The new checkbox **«Arata toate clasificatiile»** under the tree shows all of them. After «+» adds new classifications the box is ticked, otherwise the new rows (no movement yet) would be hidden.
3. **«Verifica bugetul»**: rows whose difference is 0 are never shown. `BudgetCheck.Differences` now means `Diferenta <> 0` (it was `Not Egal`, which also listed «FOREXE 0 vs no K-BOT budget»). The «Doar diferentele» checkbox is gone (both states would show the same rows). The automatic check after a download opens only when such a row exists.

Server: new `GET /api/forexe/nomenclatoare/clasificatii/sumar-buget?an=` in `routes/forexe/clasificatii_edit.py` (last version per classification, corrections totals, `activ`).

Assumptions (stated, not asked): slice number 0105; «last budget» = greatest start date of the year; the filter looks at all versions / corrections of the year, not only the last version.

### Files touched

- `PYTHON/routes/forexe/clasificatii_edit.py`
- `src/KBot.Domain/Nomenclatoare.vb` (`BudgetSummaryRow`, `BudgetCheck.Differences`)
- `src/KBot.Api/INomenclatoareApi.vb`, `ApiClient.Nomenclatoare.vb` (`GetBudgetSummaryAsync`)
- `src/KBot.App/Views/Nomenclatoare/ClasificatiiForm.vb` + `.Designer.vb`
- `src/KBot.App/Views/Nomenclatoare/BudgetCheckForm.vb` + `.Designer.vb`
- `src/KBot.App/HelpContent/contabil/nomenclatoare/clasificatii.md`

### Test results

`dotnet build src/KBot.App/KBot.App.vbproj -c Debug`: 0 warnings, 0 errors. `py_compile` of the Python route: OK. No tests written or run (project rule). Nothing run live or looked at on screen.

### Unverified / deferred

- **Server not deployed**: until it is, selecting a classification still works but the tree load fails (it now also reads `sumar-buget`) and non-leaf nodes show an error in the status line.
- Not seen on screen: the stretching «Clsf» column, hidden columns toggling, the checkbox row under the tree (new `tlyBody` row, `RowSpan 2` on the right panel).
- A classification just saved to zero stays in the tree until it is rebuilt (checkbox / reopen).
- The grid tooltips (editing hints) are not switched for the summary mode.

---

<a id="slice-0107-02"></a>

## SLICE-0107-02 - «Clasificatii bugetare»: row Total on the budget grid, no footer total where it means nothing

Operator request, 03.10.2026 (filed under slice 0107).

### What changed and why

- **Top grid (`gridBuget`)** gets a read-only **«Total»** column (key `total`, between «Trim. 4» and «✕»): the sum of the row's four quarters. Filled for a leaf (every version row), for the summary of a node (the last budget of each classification) and recomputed when a quarter is edited; a new «+» row starts at 0. The grid keeps NO footer total (no column has an aggregate), whatever node or leaf is chosen.
- **Second grid (`gridRectificari`)**: its footer sums (Trim. 1-4 and Total, `Aggregate = Sum` in the designer) are switched off with `SetCorrectionsTotal(False)` whenever no leaf is chosen: a node above the leaves (summary) and the empty «nothing chosen» state. Summing the corrections of every classification under a node means nothing. A leaf turns them back on (`SetCorrectionsTotal(True)`). The footer caption «Total» is replaced by «Rectificarile anului, pe clasificatie» in the summary and is empty when nothing is chosen, so no caption promises a total that is not there.
- Help (slice 0000-44): `contabil.nomenclatoare.clasificatii` and the tour steps «Bugetul» and «Rectificari > Total» now describe both changes; tag `0107-02` on each.

### Files touched

- `src/KBot.App/Views/Nomenclatoare/ClasificatiiForm.Designer.vb` (column `KBotDataColumn19`)
- `src/KBot.App/Views/Nomenclatoare/ClasificatiiForm.vb` (`FillGrids`, `FillSummary`, `ShowNoSelection`, `ApplyMode`, new `SetCorrectionsTotal`, `Grid_CellValueChanged`, `GridBuget_FooterRightIconClicked`)
- Help: `HelpContent/contabil/nomenclatoare/clasificatii.md`, `HelpContent/tours/tur-clasificatii.md`

### Test results

`dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors. Nothing run, nothing seen on screen (operator rule: no test runs / UI driving unless asked).

### Left unverified / deferred

- Not seen on screen: the new «Total» column (width 110, next to «Trim. 4»), the footer without sums in the summary and in the empty state, the footer sums coming back on a leaf after a summary.
- Assumption: `KBotDataColumn.Aggregate` can be changed at run time and the footer repaints (the setter calls `Owner.OnColumnAggregateChanged`); not seen.
- The «Total» of a budget row is shown for convenience only; it is not saved (the server never had one).
- The «Rectificari > Total» tour step still targets the footer; when the footer has no sums the step describes that.

---

<a id="slice-0107-buget"></a>

### Also under slice 0107 (03.10.2026): «Clasificatii bugetare», aligned grids and a final row

Same slice, operator request: the two grids of `ClasificatiiForm` keep their columns on the same vertical lines, and a row under both adds the last budget and all the corrections.

- Columns: every stretching column (`Clsf` of the budget grid, `Nr. doc.`, the `Clsf` columns) is authored at 150 (MinWidth 150); the other columns are 110 in every grid. Equal authored totals (leaf 844, node 700) make the grids shrink or stretch the same way. Before, the budget grid's `Clsf` was authored at 200 and shrank every column of that grid on a narrow panel, while the corrections grid did not.
- Leaf: the budget grid shows `Clsf` first (`Clsf | Inceput | Trim 1-4 | Total | x` over `Nr. doc. | Data | ...`).
- `KBotDataView`: `VScrollShown`, event `VScrollShownChanged` (raised after the layout pass), property `ReserveVScrollSpace`. `ClasificatiiForm.Grid_VScrollShownChanged` reserves the bar's strip on all three grids only while one of the two upper grids has its bar on.
- `gridTotal` (one row, no header / footer, read only, text «Buget + rectificari»): newest-dated budget + all corrections per quarter and Total; for a node, the last budget of every classification under it + their corrections totals. `UpdateTotalRow` follows every edit, new row and delete.
- Files: `KBotDataView.Layout.vb`, `KBotDataView.AutoSize.vb`, `ClasificatiiForm.Designer.vb`, `ClasificatiiForm.vb`; help `clasificatii.md`, `tur-clasificatii.md` (0000-46).
- `gridTotal` is not selectable (new `KBotDataView.Selectable`, default True) and its «Inceput» cell stays empty.
- New check box `chkForexe` («Arata DOAR clasificatiile folosite in FOREXE», right of «Arata toate clasificatiile», both in `tlyBife`): the tree shows only the classifications that have a row in `FX_Indicatori_Buget` (by `IdClsf`); it wins over the movement filter. Server: `GET .../clasificatii` items carry `in_forexe` (`EXISTS` on `FX_Indicatori_Buget`) - **server not deployed**; `Clasificatie.InForexe`. - «Verifica bugetul»: double click on a row closes the window and `ClasificatiiForm.GoToClassification` selects that leaf and loads it (same path as a tree click, so unsaved changes are asked about); a leaf hidden by the filters makes the tree show all classifications first. `BudgetCheckForm.RunAsync(..., onPick)`; only when opened from this window (the automatic check after a download passes no `onPick`, double click does nothing there). Row id kept in `KBotDataRow.Tag`. Not seen on screen.
- «Verifica bugetul» (`check_budget`, also the automatic check after a download): only classifications with a row in `FX_Indicatori_Buget` are checked; ones with K-BOT values but no FOREXE credit are left out (operator, 03.10.2026). The check box uses the same test. Both stay empty until a download with slice 0108 has filled `FX_Indicatori_Buget`.
- Build `KBot.App`: 0 warnings, 0 errors. Not seen on screen: the alignment at the operator's DPI with and without the bar, the strip appearing on the other grids when one starts to scroll, the height of the last row.
