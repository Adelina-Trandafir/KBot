# SLICE-0105 — Clasificatii: sumar pe noduri, filtru pe miscare, Verifica buget fara diferente zero

Operator request, 03.10.2026. Help: 0000-42 (`contabil/nomenclatoare/clasificatii.md`).

## What changed and why

1. **Non-leaf node = read-only summary.** Clicking a chapter / sub-chapter / article node of the tree
   shows the two grids without «Inceput», «Nr. doc.», «Data», the «✕» column, the footer «+», and
   without editing. They gain a first **Clsf** column that stretches. The budget grid lists every
   classification under the node with its LAST budget version of the year (greatest `DataInceput`);
   the corrections grid lists the classifications that have corrections, with the TOTAL per quarter
   and the Total column. Save / «Trimite in Access» stay disabled (no classification is current).
2. **Tree filter.** Default (and on every opening): only classifications with movement in the year,
   meaning ANY quarter of ANY budget version or ANY quarter of ANY correction is not zero. Tested
   quarter by quarter, never through a total (+1000 / -1000 totals 0 and still counts). The new
   checkbox **«Arata toate clasificatiile»** under the tree shows all of them. After «+» adds new
   classifications the box is ticked, otherwise the new rows (no movement yet) would be hidden.
3. **«Verifica bugetul»**: rows whose difference is 0 are never shown. `BudgetCheck.Differences` now
   means `Diferenta <> 0` (it was `Not Egal`, which also listed «FOREXE 0 vs no K-BOT budget»). The
   «Doar diferentele» checkbox is gone (both states would show the same rows). The automatic check
   after a download opens only when such a row exists.

Server: new `GET /api/forexe/nomenclatoare/clasificatii/sumar-buget?an=` in
`routes/forexe/clasificatii_edit.py` (last version per classification, corrections totals, `activ`).

Assumptions (stated, not asked): slice number 0105; «last budget» = greatest start date of the year;
the filter looks at all versions / corrections of the year, not only the last version.

## Files touched

- `PYTHON/routes/forexe/clasificatii_edit.py`
- `src/KBot.Domain/Nomenclatoare.vb` (`BudgetSummaryRow`, `BudgetCheck.Differences`)
- `src/KBot.Api/INomenclatoareApi.vb`, `ApiClient.Nomenclatoare.vb` (`GetBudgetSummaryAsync`)
- `src/KBot.App/Views/Nomenclatoare/ClasificatiiForm.vb` + `.Designer.vb`
- `src/KBot.App/Views/Nomenclatoare/BudgetCheckForm.vb` + `.Designer.vb`
- `src/KBot.App/HelpContent/contabil/nomenclatoare/clasificatii.md`

## Test results

`dotnet build src/KBot.App/KBot.App.vbproj -c Debug`: 0 warnings, 0 errors. `py_compile` of the
Python route: OK. No tests written or run (project rule). Nothing run live or looked at on screen.

## Unverified / deferred

- **Server not deployed**: until it is, selecting a classification still works but the tree load
  fails (it now also reads `sumar-buget`) and non-leaf nodes show an error in the status line.
- Not seen on screen: the stretching «Clsf» column, hidden columns toggling, the checkbox row under
  the tree (new `tlyBody` row, `RowSpan 2` on the right panel).
- A classification just saved to zero stays in the tree until it is rebuilt (checkbox / reopen).
- The grid tooltips (editing hints) are not switched for the summary mode.
