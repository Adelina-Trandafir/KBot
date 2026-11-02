# SLICE-0017-01 — `KBotDataView`: pinned totals row

Pass 1 of slice 0017 (Plăți). A reusable control feature in `KBot.Controls`, deliberately **not** special-cased for Plăți — Plăți (pass 03) just switches it on and tags its columns.

## What changed and why

A pinned totals band at the bottom of `KBotDataView`, painted with the header's band styling, that aggregates each column independently. Plăți's LISTA needs a Sum on the money column; the control gains the general feature (Sum / Count / Average) so any view can use it.

### Public surface

- New enum `KBotAggregate { None, Sum, Count, Average }` (`KBotAggregate.vb`).
- `KBotDataColumn.Aggregate As KBotAggregate` (default `None`) and `KBotDataColumn.AggregateFormatString As String` (optional).
- `KBotDataView.ShowTotalsRow As Boolean` (default `False`).
- `KBotDataView.TotalsRowHeight As Integer` (defaults to `HeaderHeight`; a non-positive value restores that default via a `<= 0` sentinel, so it tracks HeaderHeight until set).
- `Friend KBotDataView.DebugTotalsText(colKey)` — headless test hook, mirrors `DebugLastPaintedDataRows`.

### Behaviour (as built)

- **Pinned band** between body and horizontal scrollbar, outside `_rows`: excluded from RowCount/Rows, virtualization, selection, keyboard navigation, GetDirtyRows and conditional formatting. Hit-testing returns row -1 below ViewportHeight().
- **Header styling:** DrawTotalsRow reuses DrawHeader brushes/pens/text colour (`_bHeaderBack`, `_pHeaderSep`, `_pHeaderBaseline`, `_cHeaderText`) and semibold HeaderFont(). Rule/accent on the top edge mirror the bottom baseline of the header.
- **Frozen/H-scroll:** DrawTotalsRow uses DrawHeader layering: clipped scroll band, frozen band repainted opaquely above it. Totals stay under their columns, including ScrollByColumn.
- **Aggregates over ALL rows**, not just visible ones. `Sum`/`Average` skip `Nothing` and non-numeric cells (real numeric primitives always count; a numeric *string* parses; Boolean / non-numeric string / object are skipped). `Average` over zero countable cells renders **empty** — never `0`, never `NaN`. `Count` counts **rows whose cell `HasValue`** (state present, even a stored `Nothing`), NOT the count of non-empty numeric cells — stated and tested explicitly.
- **Formatting.** `Count` always a plain integer (ignores every format string). `Sum`/`Average` use `AggregateFormatString` if set, else the column's `FormatString`, else `ToString` — all in `CultureInfo.CurrentCulture`, matching the body's `FormatValue`.
- **Recompute:** cached formatted text per column; AddColumn/AddRow/ClearRows/EndUpdate, CommitEdit, control Item setter (dv(col,row)=), ShowTotalsRow turning on. BeginUpdate defers storms to one EndUpdate recompute. Paint never aggregates. Setter rationale and row-indexer distinction below.
- **Layout:** ViewportHeight(), UpdateScrollBars and AutoSize.WillVScrollBeVisible subtract TotalsBandHeight(), keeping predicted and actual vscroll consistent (same measurement in slice-0013).

## Decisions taken / deviations from the plan

- **Item setter added beyond the planned triggers:** AddRow recomputes before callers fill cells, so control Item (dv(col,row)=value) must recompute too. The harmless AddRow trigger remains. Direct KBotDataRow indexer writes cannot be observed per cell; Rezervări/Recepții loads use BeginUpdate/EndUpdate, which recomputes once. This preserves model-following totals. Item recomputation does not dirty rows; load/edit IsDirty semantics remain separate.
- **Auto-size includes totals**, resolving the explicit plan choice: MeasureColumnToContent measures the formatted aggregate with header font/padding, matching paint so wide totals do not ellipsize.
- **`Sum` of nothing renders `0`** (formatted), not empty — only `Average` of nothing is empty (per the plan). A money totals cell reading `0,00` on an empty grid is the natural total.

## Files touched

- `src/KBot.Controls/KBotAggregate.vb` — **new** enum.
- `src/KBot.Controls/KBotDataColumn.vb` — `Aggregate`, `AggregateFormatString`.
- `src/KBot.Controls/KBotDataView.vb` — state, `ShowTotalsRow`/`TotalsRowHeight`, recompute hooks in `AddColumn`/`AddRow`/`ClearRows`/`EndUpdate`/`Item` setter.
- `src/KBot.Controls/KBotDataView.Totals.vb` — **new** partial: compute + format + `TryNumeric`
  + `DebugTotalsText`.
- `src/KBot.Controls/KBotDataView.Layout.vb` — `TotalsBandHeight()`, `ViewportHeight()` and `UpdateScrollBars` subtract it.
- `src/KBot.Controls/KBotDataView.Painting.vb` — `DrawTotalsRow`/`DrawTotalsCell` + `OnPaint`.
- `src/KBot.Controls/KBotDataView.AutoSize.vb` — measure totals text; subtract band in `WillVScrollBeVisible`.
- `src/KBot.Controls/KBotDataView.Editing.vb` — recompute after `CommitEdit`.
- `src/KBot.DevHarness/Internal/DataViewHarnessForm{.vb,.Designer.vb}` — «Rând de totaluri» checkbox; `nr`=Count, numeric columns=Sum, first numeric=Average.
- `tests/KBot.Controls.Tests/KBotDataViewTotalsTests.vb` — **new**, 14 tests.

## Test results

- `KBotDataViewTotalsTests`: 14/14 green.
- Full `KBot.Controls.Tests`: **134** green, 0 failures.
- `KBot.Controls` + `KBot.DevHarness` build: 0 warnings, 0 errors.

## Left unverified / deferred

- **The totals band has never been rendered on screen.** It is a new painted band — the frozen-column *alignment*, the band styling, the baseline rule and the actual colours are all eyeball concerns. `KBotDataView`'s visual harness is **still unrun** (open since 0010). Run DevHarness → the `KBotDataView` probe → tick «Rând de totaluri» before trusting the paint. The headless tests cover computation, exclusion and layout math; frozen alignment is only smoke-tested (a frozen column's total computes and a full paint with H-scroll does not throw).
