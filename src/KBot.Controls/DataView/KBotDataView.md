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
  TrueFalse OnOff` + `GeneralDateMs` `LongTimeMs` (log stamps: `HH:mm:ss.fff`, not Access)
- `KBotFilterOperator` = `Equals NotEquals Contains NotContains BeginsWith NotBeginsWith
  EndsWith NotEndsWith LessThan GreaterThan Between IsEmpty IsNotEmpty`
- `KBotAutoSizeMode` = `Inherit(-1)` `None` `ToContent` · `KBotFillMode` = `None
  FirstColumn LastColumn Proportional SpecificColumn` · `KBotSortDirection` ·
  `KBotCollapseDirection` = `Horizontal|Vertical` · `KBotGroupBandKind` = `Data
  GroupHeader GroupFooter` · `KBotFooterButtonPosition` = `Right|Left` ·
  `KBotEnterKeyMode` = `NextRow|NextEditableCell`

## Data
- `Columns` (designer-authorable), `AddColumn(key, headerText, type, width)`, `Column(key)`
- `AddRow()`, `Rows`, `RowCount`, `ClearRows()`, `RemoveRowAt(index)` (slice 0087: an open
  editor is abandoned, the selection moves to the row taking its place)
- `Item(colKey, rowIndex)` — default indexer, read/write. Row side: `KBotDataRow.Item(colKey)`.
- Dirty tracking: `KBotDataRow.IsDirty` / `MarkClean()` / `HasValue(colKey)`,
  `GetDirtyRows()`, `ClearDirty()`
- `BeginUpdate()`/`EndUpdate()`, `InvalidateCell`, `InvalidateRow`, `EnsureVisible(rowIndex)`

## KBotDataColumn (per column)
`Key` and `ColumnType` are **frozen while the grid has rows**. Identity/size: `HeaderText`
(+ `MultiLine`, `HeaderTextAlign`, `HeaderFont`), `Width = 100`, `MinWidth = 40`,
`MaxWidth = MaxValue` (logical px, C2 — `Width` is always clamped into that pair),
`Resizable = True`, `Visible = KBotColumnVisibility.Visible` (`Hidden`, or `WhenRoom` = shown by
the pass only when the visible columns leave room for its `MinWidth`), `AutoHide = False`
(may be dropped, rightmost first,
rather than showing a horizontal bar), `Frozen` (metadata only — the authority is
`KBotDataView.FrozenColumnCount`), `AutoSizeMode = Inherit`.
Cell borders (slice 0085-03): `CellBorderColor` (`Empty` = theme grid-line colour) and
`CellBorders` (flags `KBotBorderSides`, default `Right | Bottom` = the grid lines); per single
cell through `CellFormatting` (`e.BorderColor`, `e.Borders`). Neighbours drawing a shared edge
make a double line.
Cells: `TextAlign`, `CellPadding = 6,0,6,0`, `ColumnFont`, `ReadOnly`, `Enabled`,
`ValueType`, `Format` or `FormatString` (never both), `DecimalPlaces = -1`,
`Aggregate` + `AggregateFormatString`, `ComboItems`, `OptionGroup`,
`ProgressMin/Max = 0/100`.
Button cells (slice 0085-02, `Button` columns only, all metrics logical px): `ButtonText`
(fixed caption; empty = cell text, then header — but with an image set, empty = no caption),
`ButtonImage`, `ButtonBackColor` (`Empty` = theme, `Transparent` = flat: no face, and no border
unless `ButtonBorderColor` is set), `ButtonBorderColor` (`Empty` = theme, `Transparent` = none),
`ButtonBorders` (flags `KBotBorderSides` Left/Top/Right/Bottom, default `All` = rounded border;
anything else = straight lines on the chosen sides over a plain rectangle),
`ButtonPadding` (inner, default 0), `ButtonMargin` (outer, default 4,3,4,3), `ButtonAlign`
(default MiddleCenter), `ButtonSize` (0 on an axis = fill the cell; alignment only shows when
the button is smaller than the cell), `ButtonFont` (not set = column font, then grid font).
A click acts only on the button face, not on the margin around it (Space still acts on the
current cell).
Header icons: `HeaderLeftIcon` (decorative) + `HeaderRightIcon` (raises
`HeaderRightIconClicked`) with sizes, hover colour and tooltips; `ShowColumnFilter`,
`ColumnFilterIcon`, `ColumnFilterIconSize`, `ColumnFilterHoverColor`, `FilterIconTooltip`.

## Grid appearance
`RowHeight = 28`, `HeaderHeight = 30`, `ShowHeader = True`,
`AutoSizeHeaderHeight = True` + `MaxHeaderHeight = 0` (grow for multiline titles),
`AlternatingRows = True`, `ReadOnlyGrid = False`, `FrozenColumnCount = 0`,
`ScrollByColumn = False`, `FooterVisible = False`, `FooterHeight = 0` (0 = follow
`HeaderHeight`), `FooterRightIcon` + `FooterRightIconTooltip` + `FooterRightIconClicked` +
`FooterRightIconRect` (slice 0087: a button in the right corner of the band, e.g. «+ add row»;
it keeps its square like the collapse button, standing left of it when both are on the right,
and the totals are cut before it), `FooterCaption` + `FooterLeftIcon` (+ size, hover colour,
`FooterLeftIconClicked`). Bands: the `Border*`, `Header*`, `Footer*` and
`*ColumnSeparator*` colour/width properties, all Empty = theme (C1) and logical px (C2).
The scrollbars sit INSIDE the border frame (inset by `BorderWidth`), so the frame stays
closed next to a visible bar; the viewport ends at the bar's edge.
`ApplyTheme(scheme)`.

## Sizing
`AutoSizeColumnsMode = ToContent`, `ColumnFillMode = None` (+ `FillColumnKey` for
`SpecificColumn`), `ShrinkColumnsToFit = True`, `ShowColumnsWhenRoom = True` (`WhenRoom`
columns are served before the fill column), `AutoSizeSampleRows = 200` (0 = all rows),
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
The condition dialog (`Filter/KBotFilterConditionDialog`) asks for the operand in a plain
text box, except on a `DateTime` column, where it is a `KBotDatePicker` (both declared in the
designer; the unused rows collapse). The field's format is
`KBotColumnFormat.DateOperandFormat(format, formatString)`: the column's `FormatString` if
set, else the culture's short date plus the time the named format shows (`.fff` for
`GeneralDateMs` / `LongTimeMs`, seconds for `LongTime`, minutes for `GeneralDate` /
`ShortTime` / `MediumTime`, none for the date-only ones) -- the grid passes it to
`KBotFilterPopup` (`dateOperandFormat`). The field writes in `CurrentCulture`, which is what
`CoerceOperand` reads back; an empty field is an empty operand (inert condition, as before).

## Grouping
`Groups: KBotGroupLevelCollection` (outermost first; empty = ungrouped), `IsGrouped`,
`GroupBy(colKey, …)`, `ClearGrouping()`, `SetColumnGroupLevel`, `GroupLevelFor(colKey)`,
`CollapseAllGroups([level])`, `ExpandAllGroups([level])`, `GroupCount([level])`, `IsRowShown(rowIndex)` (passes the filters and is not inside a collapsed group),
`EnableGrouping = False` (shows the Grouping tab in the column menu; does not touch levels
authored in the designer), `GroupCollapsedChanged`, `GroupFormatting`.
`KBotGroupLevel`: `ColumnKey` (empty = inactive level, skipped), `SortDirection`
(`None` not allowed), `KeyPattern` (regex over the DISPLAYED text; key = the capture groups
joined, or the whole match; no match = the whole text; groups are ordered by that key read in
the column's type -- e.g. `^\S+` groups a `GeneralDateMs` column by day), `HasKeyPattern`,
`ShowHeader` / `ShowFooter` (+ heights, + `*CaptionFormat` where
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
it: Up/Down commit and go to the nearest EDITABLE cell of the same column on the rows drawn
above/below (`NextEditableRow`; group bands and the footer are never stops, rows in a
collapsed group are skipped; with nowhere to go nothing happens and nothing is committed),
Left/Right commit and step to the
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

### The editor looks like the cell (slice 0085)
A single click on an editable cell starts editing with the whole text selected (a click in
the padding of the cell already being edited re-selects it). The mouse shows an I-beam over
every editable cell. The edited cell never takes the selected-row colour: it keeps its
unselected look (row colour + formatting handlers), padding included, so it stands out. F2 and double click still
work. The text editor is borderless and sits in the cell's content rectangle (`CellPadding`,
DPI-scaled), vertically centred on one line of text, with the edit control's margins zeroed
and TextRenderer's glyph padding applied instead, so its text starts on the same pixel as the
painted text. Font, colours and horizontal alignment come from the same
`RowFormatting`/`CellFormatting` chain the painter runs; the painter skips that cell's text
while it is open. It is re-placed on every layout pass (resize, column width, theme, DPI).

### The combo editor (slice 0094)
The combo editor is a `KBotComboBox` in cell editor mode (no frame, no arrow, free height), placed
like the text editor: one line high, vertically centred the same way, over the text rectangle
of the combo cell (the content minus the 16 logical px chevron strip), with the same font,
colours, alignment and glyph margins. The grid keeps painting the chevron. A click on the chevron
(starting the edit or during it) opens / closes the list, which opens under the whole cell;
elsewhere in the cell a click selects the text. Typing searches the items (`FindAsYouType`); free
text is still accepted (`LimitToList = False`) and left to `CellValidating`, as before. A commit
first lets the combo give its verdict (`CommitText`: a text that is the start of exactly one item
takes that item). Left/Right leave the cell only from the edge of the text, like the text editor.

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
  not apply to those two either (a `Button` has `ButtonPadding` / `ButtonMargin` instead).
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
