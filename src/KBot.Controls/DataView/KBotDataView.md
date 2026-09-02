# KBotDataView

Owner-drawn, UNBOUND, virtualized grid — the Access "continuous form" of K-BOT. Only the
visible rows are painted and ONE real editor floats over the active cell, so the handle
count stays flat no matter how many rows there are.

`DataView/` — `KBotDataView.vb` + 18 partials (`.Layout`, `.Painting`, `.Theming`,
`.Editing`, `.Filtering`, `.FilterIcon`, `.Grouping`(+`.Painting`), `.Footer`,
`.FooterCaption`, `.Collapse`, `.AutoSize`, `.WidthProbe`, `.Dpi`, `.Input`,
`.HeaderIcons`, `.Tooltip`, `.ButtonTips`), the model (`KBotDataColumn`,
`KBotDataColumnCollection`, `KBotDataRow`, `KBotGroupLevel` + collection), the enums,
`KBotFilterEngine`, `KBotColumnFilter`, `Events/`, `Filter/` (popup + condition dialog).
`Control` · Toolbox · `IThemedControl`, `ISupportInitialize`, `IDpiScaledControl`
Conventions: [C1..C9](../CONTROLS.md).
Status: heavily unit-tested (`KBotDataView*Tests`); visual harness
(`KBotDataViewPlaygroundTest`, `KBotDataViewVisualTest`) never run on screen.

## Enums
- `KBotColumnType` = `Text` `Combo` `CheckBox` `OptionButton` `Button` `ProgressBar`
- `KBotValueType` = `Text` `Number` `DateTime` `Boolean` — decides which aggregates and
  which filter operators are offered.
- `KBotAggregate` = `None Sum Count Average Min Max CountDistinct CountEmpty CountTrue
  CountFalse First Last`
- `KBotFormat` — the Access vocabulary: `GeneralNumber Currency Euro Fixed Standard Percent
  Scientific GeneralDate LongDate MediumDate ShortDate LongTime MediumTime ShortTime YesNo
  TrueFalse OnOff`
- `KBotFilterOperator` = `Equals NotEquals Contains NotContains BeginsWith NotBeginsWith
  EndsWith NotEndsWith LessThan GreaterThan Between IsEmpty IsNotEmpty`
- `KBotAutoSizeMode` = `Inherit(-1)` `None` `ToContent` · `KBotFillMode` = `None
  FirstColumn LastColumn Proportional SpecificColumn` · `KBotSortDirection` ·
  `KBotCollapseDirection` = `Horizontal|Vertical` · `KBotGroupBandKind` = `Data
  GroupHeader GroupFooter` · `KBotFooterButtonPosition` = `Right|Left` ·
  `KBotEnterKeyMode` = `NextRow|NextEditableCell`

## Data
- `Columns` (designer-authorable), `AddColumn(key, headerText, type, width)`, `Column(key)`
- `AddRow()`, `Rows`, `RowCount`, `ClearRows()`
- `Item(colKey, rowIndex)` — default indexer, read/write. Row side: `KBotDataRow.Item(colKey)`.
- Dirty tracking: `KBotDataRow.IsDirty` / `MarkClean()` / `HasValue(colKey)`,
  `GetDirtyRows()`, `ClearDirty()`
- `BeginUpdate()`/`EndUpdate()`, `InvalidateCell`, `InvalidateRow`, `EnsureVisible(rowIndex)`

## KBotDataColumn (per column)
`Key` and `ColumnType` are **frozen while the grid has rows**. Identity/size: `HeaderText`
(+ `MultiLine`, `HeaderTextAlign`, `HeaderFont`), `Width = 100`, `MinWidth = 40`,
`MaxWidth = MaxValue` (logical px, C2 — `Width` is always clamped into that pair),
`Resizable = True`, `Visible = True`, `AutoHide = False` (may be dropped, rightmost first,
rather than showing a horizontal bar), `Frozen` (metadata only — the authority is
`KBotDataView.FrozenColumnCount`), `AutoSizeMode = Inherit`.
Cells: `TextAlign`, `CellPadding = 6,0,6,0`, `ColumnFont`, `ReadOnly`, `Enabled`,
`ValueType`, `Format` or `FormatString` (never both), `DecimalPlaces = -1`,
`Aggregate` + `AggregateFormatString`, `ComboItems`, `OptionGroup`,
`ProgressMin/Max = 0/100`.
Header icons: `HeaderLeftIcon` (decorative) + `HeaderRightIcon` (raises
`HeaderRightIconClicked`) with sizes, hover colour and tooltips; `ShowColumnFilter`,
`ColumnFilterIcon`, `ColumnFilterIconSize`, `ColumnFilterHoverColor`, `FilterIconTooltip`.

## Grid appearance
`RowHeight = 28`, `HeaderHeight = 30`, `ShowHeader = True`,
`AutoSizeHeaderHeight = True` + `MaxHeaderHeight = 0` (grow for multiline titles),
`AlternatingRows = True`, `ReadOnlyGrid = False`, `FrozenColumnCount = 0`,
`ScrollByColumn = False`, `FooterVisible = False`, `FooterHeight = 0` (0 = follow
`HeaderHeight`), `FooterCaption` + `FooterLeftIcon` (+ size, hover colour,
`FooterLeftIconClicked`). Bands: the `Border*`, `Header*`, `Footer*` and
`*ColumnSeparator*` colour/width properties, all Empty = theme (C1) and logical px (C2).
`ApplyTheme(scheme)`.

## Sizing
`AutoSizeColumnsMode = ToContent`, `ColumnFillMode = None` (+ `FillColumnKey` for
`SpecificColumn`), `ShrinkColumnsToFit = True`, `AutoSizeSampleRows = 200` (0 = all rows),
`AutoSizeColumns()`, `ResetColumnSizing()`.

## Sort / filter
`ApplySort(colKey, direction)`, `SortColumnKey`, `SortDirection`, `SortChanged`;
`ColumnFilter(colKey)`, `HasColumnFilter`, `SetColumnFilter(filter)`,
`ClearColumnFilter(colKey)`, `ClearAllFilters()`, `IsFiltered`, `FilteredRowCount`,
`DistinctDisplayValues(colKey)`, `FilterChanged`, `ShowColumnFilterMenu(colKey)`,
`ColumnFilterOpening` event, `FilterIconSize`.
`KBotColumnFilter` = `SelectedValues` (checklist) plus a `Condition` with `Operand1` /
`Operand2`; `Matches(rawValue, displayText, valueType)`, `Clone()`, `IsActive`.
`KBotFilterEngine` (Shared, pure): `AllowedOperators`, `IsAllowed`, `OperandCount`,
`OperatorCaption`, `Compare`, `IsBlank`, `MatchesCondition`, `CoerceOperand`.

## Grouping
`Groups: KBotGroupLevelCollection` (outermost first; empty = ungrouped), `IsGrouped`,
`GroupBy(colKey, …)`, `ClearGrouping()`, `SetColumnGroupLevel`, `GroupLevelFor(colKey)`,
`CollapseAllGroups([level])`, `ExpandAllGroups([level])`, `GroupCount([level])`,
`EnableGrouping = False` (shows the Grouping tab in the column menu; does not touch levels
authored in the designer), `GroupCollapsedChanged`, `GroupFormatting`.
`KBotGroupLevel`: `ColumnKey` (empty = inactive level, skipped), `SortDirection`
(`None` not allowed), `ShowHeader` / `ShowFooter` (+ heights, + `*CaptionFormat` where
`{0}` = column title, `{1}` = group value, `{2}` = row count), `EmptyCaption = "(goale)"`,
`Indent = 16` (cumulative; applies to the bands BELOW it), `ShowFooterAggregates = True`,
`ShowHeaderAggregates = False`, `Collapsible = True`, `CollapsedByDefault = False`, colours
and fonts.

## Collapse (the grid folds like the tree)
`CollapseButton = False` (needs `FooterVisible`), `CollapseButtonSize = 16`,
`CollapseButtonPosition = Right`, `CollapseDirection = Horizontal`,
`MinimumCollapsedWidth = 100`, `CollapseExpandedImage` / `CollapseCollapsedImage`,
`Collapsed`, `ToggleCollapse()`, `HostOwnsWidth` / `HostOwnsHeight`, `ExpandedWidth` /
`ExpandedHeight` / `CollapsedHeight`, `CollapseButtonRect`, `CollapsedChanged(collapsed)`.
When the host docks or anchors the grid it owns the size: the control flips state and raises
the event, and the host moves its own splitter.

## Editing, input, formatting events
`IsEditing`, `CanEdit(colKey, rowIndex)`, `CellValidating` (`ProposedValue` + `Cancel`),
`CellValueChanged`, `CurrentRowIndex`, `CurrentColumnKey`, `CurrentRow`, `RowIndexAt(pt)`,
`SelectionChanged`, `CellClick`, `CellDoubleClick`, `ButtonClick`,
`SetOptionValue(colKey, rowIndex, value)`, `IsRowEnabled`, `IsCellEnabled`,
`CellFormatting` (per-cell text / colour / font / alignment / enabled), `RowFormatting`.

### Keyboard editing (designer-authorable)
`ArrowKeyEditing = True` — the arrows carry the EDITOR from cell to cell instead of closing
it: Up/Down commit and step one row in the same column, Left/Right commit and step to the
next EDITABLE cell of the row (`NextEditableColumn`, read-only / check / button / progress
columns skipped, no wrap). Left/Right only move from the EDGE of the text — mid-word they
stay a caret move, and an open combo keeps its own arrows — otherwise fixing one letter
would throw the operator into another cell. At the end of a row the editor reopens where it
was, so nothing is lost. Off = the editors behave like plain text boxes again.
`EnterKeyMode = NextRow` (`KBotEnterKeyMode`) — `NextRow` is the Access continuous form
(next row, same column); `NextEditableCell` walks the row field by field and drops to the
first editable field of the next row when it runs out. Enter pressed IN an editor reopens
the editor on the cell it lands on (a whole table fills in without the mouse); Enter on the
grid only moves. Tab / Shift+Tab are unchanged: next / previous ENABLED column, editable or
not.

## Tooltips
`CellTooltip: KBotCellTooltipOptions` — the label for cells whose text does not fit
(`Enabled`, `Delay = 450`, `MaxWidth = 480`, colours, `Font`, `CornerRadius = 4`,
`OverlayCell = True`).
`OverlayCell` puts the label EXACTLY over the cell — same top-left corner, same cell padding
and alignment, the row's height while one line is enough — stretched right to the grid's
usable edge (`MaxWidth` does not apply there; the grid margin is the limit). Text that still
does not fit wraps and grows DOWNWARD from the same top-left corner, so the label reads as
the cell widened rather than a balloon parked next to it. It only climbs if the grown label
would fall off the bottom of the screen. Off = the old balloon: under the cell, `MaxWidth`
wide, flipped above when there is no room.
The overflow test itself uses the column's `CellPadding` (same measurements as the overlay),
so the label appears exactly when the painted text is clipped.
`ButtonTooltip: KBotToolTip` — the label for the DRAWN header/footer buttons, plus
`FilterIconTooltip`, `CollapseButtonTooltip`, `ExpandButtonTooltip` (C8).

## Limits
- **Unbound.** No `DataSource`, no `BindingSource`, no `INotify*` plumbing — the host fills
  the rows and reads them back.
- `Key` and `ColumnType` cannot change once rows exist.
- `ShowColumnFilter` is refused on `Button` and `ProgressBar` columns; `CellPadding` does
  not apply to those two either.
- `Format` and `FormatString` are mutually exclusive.
- Aggregates are offered per `ValueType` (`KBotAggregateRules`). The footer band is NOT a
  row: it is excluded from `Rows`, `RowCount`, virtualization, selection, hit-testing and
  dirty tracking.
- A group level needs a `SortDirection` (rows of one key must sit together) and is
  collapsible only with `ShowHeader` — otherwise there is nothing to click.
- One active editor, so: no multi-cell paste, no cell merging, no row-detail panes, no
  frozen ROWS (only leading columns), no column drag-reorder.
- No row headers and no built-in multi-row / range selection.
- Between `BeginInit` and `EndInit` validation is suspended and layout deferred (C6) — a
  half-typed key must not throw out of `InitializeComponent`.
