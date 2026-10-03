# SLICE-0107-02 - «Clasificatii bugetare»: row Total on the budget grid, no footer total where it means nothing

Operator request, 03.10.2026 (filed under slice 0107).

## What changed and why

- **Top grid (`gridBuget`)** gets a read-only **«Total»** column (key `total`, between «Trim. 4» and «✕»):
  the sum of the row's four quarters. Filled for a leaf (every version row), for the summary of a node
  (the last budget of each classification) and recomputed when a quarter is edited; a new «+» row starts
  at 0. The grid keeps NO footer total (no column has an aggregate), whatever node or leaf is chosen.
- **Second grid (`gridRectificari`)**: its footer sums (Trim. 1-4 and Total, `Aggregate = Sum` in the
  designer) are switched off with `SetCorrectionsTotal(False)` whenever no leaf is chosen: a node above
  the leaves (summary) and the empty «nothing chosen» state. Summing the corrections of every
  classification under a node means nothing. A leaf turns them back on (`SetCorrectionsTotal(True)`).
  The footer caption «Total» is replaced by «Rectificarile anului, pe clasificatie» in the summary and
  is empty when nothing is chosen, so no caption promises a total that is not there.
- Help (slice 0000-44): `contabil.nomenclatoare.clasificatii` and the tour steps «Bugetul» and
  «Rectificari > Total» now describe both changes; tag `0107-02` on each.

## Files touched

- `src/KBot.App/Views/Nomenclatoare/ClasificatiiForm.Designer.vb` (column `KBotDataColumn19`)
- `src/KBot.App/Views/Nomenclatoare/ClasificatiiForm.vb` (`FillGrids`, `FillSummary`, `ShowNoSelection`,
  `ApplyMode`, new `SetCorrectionsTotal`, `Grid_CellValueChanged`, `GridBuget_FooterRightIconClicked`)
- Help: `HelpContent/contabil/nomenclatoare/clasificatii.md`, `HelpContent/tours/tur-clasificatii.md`

## Test results

`dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors. Nothing run, nothing seen on screen
(operator rule: no test runs / UI driving unless asked).

## Left unverified / deferred

- Not seen on screen: the new «Total» column (width 110, next to «Trim. 4»), the footer without sums in
  the summary and in the empty state, the footer sums coming back on a leaf after a summary.
- Assumption: `KBotDataColumn.Aggregate` can be changed at run time and the footer repaints (the setter
  calls `Owner.OnColumnAggregateChanged`); not seen.
- The «Total» of a budget row is shown for convenience only; it is not saved (the server never had one).
- The «Rectificari > Total» tour step still targets the footer; when the footer has no sums the step
  describes that.
