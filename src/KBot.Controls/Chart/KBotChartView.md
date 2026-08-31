# KBotChartView

Owner-drawn TIME chart with a button band on top — the K-BOT control for "how did this
thing move". Written entirely in English, property-grid category names included.

`Chart/` — `KBotChartView.vb` + `.Painting.vb`, `KBotChartSeries.vb`, `KBotChartPoint.vb`,
`KBotChartTab.vb` (each + its collection), `KBotChartEnums.vb`
`Control` · sealed · partial · Toolbox · `IThemedControl`, `ISupportInitialize`
Conventions: [C1..C9](../CONTROLS.md). Status: covered by `KBotChartViewTests`.

## Enums
`KBotChartMarkerStyle` = `None` `Circle` `Square` `Diamond` ·
`KBotChartTabAlign` = `Left` | `Right` ·
`KBotChartValueAxisMode` = `FromZero` (magnitudes compare honestly) | `FromMinimum` (small
movements stay visible).

## Model
- **KBotChartSeries**: `Key` (non-empty, unique), `Text` (legend + default tooltip title),
  `LineColor` (Empty = derived from the palette, C1), `Visible = True`, `Emphasis = False`
  (drawn last and thicker — that is how a total is told from its parts), `FillArea = False`,
  `Points`, `Tag`, `AddPoint(moment, value)`.
- **KBotChartPoint**: `Moment` (Date — the X axis is REAL time, not a slot index), `Value`,
  `PointColor` (Empty = series colour; colours the marker and the segment LEAVING it),
  `TooltipHeader` / `TooltipText` / `TooltipFooter` (Empty = series name / moment+value /
  none; the body accepts `KBotToolTip` markup), `Tag`.
- **KBotChartTab**: `Key`, `Text`, `Icon`, `Enabled = True`, `Visible = True`, `Tooltip`.

## Control API
- Data: `Series`, `AddSeries(key, text)`, `FindSeries(key)`, `SetSeriesVisible(key, v)`,
  `ClearSeries()`, `BeginUpdate()`/`EndUpdate()`, `EmptyText` + `EmptyTextColor`
- Band: `HeaderVisible = True`, `HeaderHeight = 28`, `HeaderCaption`, `HeaderFont`,
  `HeaderBackColor`, `HeaderTextColor`, `HeaderSeparatorColor/Width = 1`, `HeaderGradient = 0`
- Tabs (SINGLE-select): `Tabs`, `AddTab`, `SelectTab(key)`, `SelectedTabKey`,
  `SetTabEnabled`, `SetTabVisible`, `ContainsTab`, `TabAlign = Right`, `TabHeight = 20`,
  `TabPadding = 12`, `TabSpacing = 4`, `TabCornerRadius = -1`, `TabGradient = 14`,
  `TabIconSize`
- Plot: `PlotMargin = 10`, `LineWidth = 2`, `EmphasisLineWidth = 3`, `MarkerSize = 6`,
  `MarkerStyle = Circle`, `AreaFillOpacity = 18`, `PlotBackColor`, `BorderVisible = True`,
  `BorderColor`, `CornerRadius = -1`
- Axes: `AxisVisible = True`, `AxisColor`, `AxisTextColor`, `GridColor`,
  `HorizontalGridLines = True`, `VerticalGridLines = False`, `ValueTickCount = 4`,
  `AxisFont`, `ValueFormat = "N0"`, `MomentFormat = "dd.MM.yy"`, `AxisLabelGap = 6`,
  `ValueAxisMode = FromZero`
- Legend: `LegendVisible = True`, `LegendHeight = 18`, `LegendSpacing = 14`, `LegendTextColor`
- Hover: `PointTooltip: KBotToolTip` (look via `PointTooltip.Style`),
  `PointTooltipEnabled = True`, `HoverRadius = 14`
- Events: `TabSelected(tabKey)`, `PointClicked(seriesKey, pointIndex)`,
  `PointHovered(seriesKey, pointIndex)`
- `AutoColor(index)` — the palette-derived colour a series gets when it has none.
- `BeginInit`/`EndInit`, `ApplyTheme(scheme)`. `BackColor`/`ForeColor`/`Font` overridden (C4).

## Rules
- Setting `SelectedTabKey` does NOT raise `TabSelected` (an assignment is the host stating a
  fact); `SelectTab(key)` DOES.
- The chart never decides what a tab MEANS — it raises the key and the host refills the series.
- Points are **not sorted for you**: a line walking backwards is an honest sign the caller's
  query is out of order.

## Limits
- `MarkerStyle = None` also removes the hit target, so no point can be named on hover.
- One shared value axis and one shared time axis — no secondary Y axis, no per-series axis.
- Line/area only: no bars, no pie, no stacking, no log scale.
- No zoom, no pan, no range selection, no crosshair.
- The band is single-select and drawn (not real buttons); no scrolling when the tabs do not
  fit, and `TabHeight` is clamped to `HeaderHeight` at layout time.
- Keys validated at `EndInit` and on add: empty/duplicate → `ArgumentException`; unknown key
  on `SelectTab`/`SetSeriesVisible`/`SetTab*` → `ArgumentException` (C3). Skipped at design
  time (C6).
