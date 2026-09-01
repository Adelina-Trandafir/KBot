# KBotChartView

Owner-drawn TIME chart with a button band on top — the K-BOT control for "how did this
thing move". Written entirely in English, property-grid category names included.

`Chart/` — `KBotChartView.vb` + `.Painting.vb`, `KBotChartSeries.vb`, `KBotChartPoint.vb`,
`KBotChartTab.vb`, `KBotChartGuide.vb` (each + its collection), `KBotChartEnums.vb`,
`KBotAutoPalette.vb` (the automatic colour set, shared with `KBotLaneView`)
`Control` · sealed · partial · Toolbox · `IThemedControl`, `ISupportInitialize`
Conventions: [C1..C9](../CONTROLS.md). Status: covered by `KBotChartViewTests`.

## Enums
`KBotChartMarkerStyle` = `None` `Circle` `Square` `Diamond` ·
`KBotChartTabAlign` = `Left` | `Right` ·
`KBotChartValueAxisMode` = `FromZero` (magnitudes compare honestly) | `FromMinimum` (small
movements stay visible) ·
`KBotChartLineMode` = `Straight` (a slope from one point to the next) | `Step` (the value
HOLDS until the next point changes it, then jumps).

## Model
- **KBotChartSeries**: `Key` (non-empty, unique), `Text` (legend + default tooltip title),
  `LineColor` (Empty = derived from the palette, C1), `Visible = True`, `Emphasis = False`
  (drawn last and thicker — that is how a total is told from its parts), `FillArea = False`,
  `LineMode = Straight`, `Points`, `Tag`, `AddPoint(moment, value)`.
  `LineMode` is on the SERIES because it is a statement about what the DATA is, not about
  how the control looks: a quantity sampled by snapshots is a staircase, one measured
  continuously is not, and a chart that mixes the two must be able to say so. `FillArea`
  follows the stepped path, so the wash keeps agreeing with the line above it.
- **KBotChartGuide**: `Moment`, `Text` (tooltip title; empty ⇒ no label at all), `Tooltip`,
  `LineColor` (Empty = the dimmed text colour — **never red**, see below), `DashStyle = Dot`,
  `Visible = True`, `Tag`. A dated line drawn straight down the plot BEHIND every series — a
  moment that matters without being a measurement (a payment). Not a series: no value, no
  marker, no legend entry, no key, no click. Hovering names it, and that is all it does.
  The same type `KBotLaneView` draws, so the two surfaces can be given guides built from one
  source and cannot then disagree about a date.
- **KBotChartPoint**: `Moment` (Date — the X axis is REAL time, not a slot index), `Value`,
  `PointColor` (Empty = series colour; colours the marker and the segment LEAVING it),
  `TooltipHeader` / `TooltipText` / `TooltipFooter` (Empty = series name / moment+value /
  none; the body accepts `KBotToolTip` markup), `Tag`.
- **KBotChartTab**: `Key`, `Text`, `Icon`, `Enabled = True`, `Visible = True`, `Tooltip`.

## Control API
- Data: `Series`, `AddSeries(key, text)`, `FindSeries(key)`, `SetSeriesVisible(key, v)`,
  `ClearSeries()`, `BeginUpdate()`/`EndUpdate()`, `EmptyText` + `EmptyTextColor`
- Guides: `Guides`, `AddGuide(moment, text)`, `ClearGuides()`
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
- `AutoColor(index)` — the palette-derived colour a series gets when it has none. The set
  itself lives in `KBotAutoPalette`, shared with `KBotLaneView` so the two surfaces cannot
  disagree about what the n-th colour is. **Never red**: red is what this application spends
  on something being wrong, so the hues live strictly outside the red wedge whatever the
  scheme's accent happens to be.
- `BeginInit`/`EndInit`, `ApplyTheme(scheme)`. `BackColor`/`ForeColor`/`Font` overridden (C4).

## Rules
- Setting `SelectedTabKey` does NOT raise `TabSelected` (an assignment is the host stating a
  fact); `SelectTab(key)` DOES.
- The chart never decides what a tab MEANS — it raises the key and the host refills the series.
- Points are **not sorted for you**: a line walking backwards is an honest sign the caller's
  query is out of order.
- Guides are drawn BEHIND the series and are hit-tested on **horizontal distance only** — a
  guide is a whole column of the plot, not a spot on it. A marker always wins the pixel they
  share, because a marker can be clicked and a guide cannot. Guides do **not** stretch the
  time axis: one outside the span of the points is simply not drawn.
- In `Step` mode the corner between the flat run and the riser carries no marker: it is the
  only vertex the data does not contain, and a marker there would claim a measurement nobody
  took. Both halves take the LEFT point's colour, because both belong to the stretch it
  started.

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
