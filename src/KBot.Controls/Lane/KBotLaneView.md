# KBotLaneView

Owner-drawn PLACEMENT surface: one horizontal lane per thing that owns markers, one marker
per dated event, and a drag that moves a marker from one lane to another. Written entirely
in English, property-grid category names included.

`Lane/` — `KBotLaneView.vb` + `.Painting.vb` + `.Drag.vb`, `KBotLane.vb`,
`KBotLaneMarker.vb` (each + its collection), `KBotLaneEnums.vb`
`Control` · sealed · partial · Toolbox · `IThemedControl`, `ISupportInitialize`
Conventions: [C1..C9](../CONTROLS.md). Status: **code-green, never seen on screen.**

## What it is for, and how it differs from the chart
A chart answers *how did this move*. This answers *which one does it belong to*, for a
picture the operator is BUILDING rather than reading. Written for the reception ▸ snapshot
editor, where twenty lanes of twenty markers is the ordinary case and the whole picture has
to be visible at once — a placement that can only be judged one row at a time cannot be
judged at all.

**Compact by default: no text.** `LaneCaptionsVisible` and `MarkerLabelsVisible` are both
False, so the surface is markers on lanes and nothing else. Arithmetic, not minimalism: four
hundred labelled markers is not a picture, and the names are one hover away. The same
control set roomy — captions on, labels on, axis on, `LaneHeight` ≈ 26 — is the enlarged
reading of the same data.

## Enums
`KBotLaneMarkerStyle` = `Normal` · `Deletion` (cross cap — the marker that CLOSES a chain) ·
`NoChange` (hollow with an `=` — a save that recorded nothing, so it stops reading as a
duplicate) · `Locked` (padlock, **full colour, never greyed**) · `Loose` (diamond — not
placed on anything) ·
`KBotLaneEndMark` = `None` | `Ok` | `Warning`.

## Model
- **KBotLane**: `Key` (non-empty, unique), `Text` (tooltip title; painted at the left only
  when `LaneCaptionsVisible`), `Tooltip` (accepts `KBotToolTip` markup), `LaneColor` (Empty
  = derived, C1), `IsTarget = True` (False ⇒ never offered as a drop target, and the host's
  veto never sees it), `EndMark = None`, `SeparatorAbove = False` (a line drawn ABOVE this
  lane, cutting the surface in two), `Visible = True`, `Markers`, `Tag`,
  `AddMarker(moment, text)`.
- **KBotLaneMarker**: `Moment` (Date — the X axis is REAL time, not a slot index), `Text`
  (tooltip title; painted beside the marker only when `MarkerLabelsVisible`), `Tooltip`,
  `Style = Normal`, `MarkerColor` (Empty = the lane's colour), `Visible = True`, `Tag`.
  **A marker has no value** — that is the line between this surface and the chart above it.

## Control API
- Data: `Lanes`, `AddLane(key, text)`, `FindLane(key)`, `SetLaneVisible(key, v)`,
  `ContainsLane(key)`, `ClearLanes()`, `BeginUpdate()`/`EndUpdate()`, `EmptyText` +
  `EmptyTextColor`
- Guides: `Guides`, `AddGuide(moment, text)`, `ClearGuides()` — the SAME
  [`KBotChartGuide`](../Chart/KBotChartView.md) type the chart draws, so both surfaces can
  be given guides built from one source and cannot then disagree about a date
- Band: `HeaderVisible = True`, `HeaderHeight = 28`, `HeaderCaption`, `HeaderFont`,
  `HeaderBackColor`, `HeaderTextColor`, `HeaderSeparatorColor/Width = 1`, `HeaderGradient = 0`
- Enlarge button: `EnlargeButtonVisible = True`, `EnlargeButtonImage` (Nothing ⇒ a drawn
  glyph), `EnlargeButtonSize = 16×16`, `EnlargeButtonTooltip`
- Layout: `LaneHeight = 13`, `LaneSpacing = 2`, `LaneCaptionsVisible = False`,
  `LaneCaptionWidth = 120`, `MarkerLabelsVisible = False`, `MarkerSize = 7`,
  `LaneLineWidth = 1`, `LaneLineColor`, `SegmentedRail = True`, `SegmentWidth = 0`
  (0 ⇒ `LaneLineWidth`), `LaneHoverBackColor`, `SeparatorColor`, `SeparatorWidth = 1`,
  `EndMarkSize = 9`
- Plot: `PlotMargin = 6`, `PlotBackColor`, `BorderVisible = True`, `BorderColor`,
  `BorderWidth = 1`, `CornerRadius = -1`, `TrailingSpace = 0`
- Axis: `AxisVisible = False`, `AxisTextColor`, `AxisFont`, `MomentFormat = "dd.MM.yy"`,
  `AxisLabelGap = 4`
- Range: `RangeStart` / `RangeEnd` (runtime only, `Date.MinValue` = work it out from the
  markers), `PlottedRangeStart` / `PlottedRangeEnd` (read-only)
- Hover: `MarkerTooltip: KBotToolTip` (look via `MarkerTooltip.Style`),
  `MarkerTooltipEnabled = True`, `HoverRadius = 10`
- Drag: `DragHighlightColor` (Empty = accent), `DragForbiddenColor` (Empty = error colour),
  `DraggedMarker`
- Events: `EnlargeRequested()`, `MarkerHovered(laneKey, markerIndex)`,
  `MarkerDragStarting(sender, e)`, `MarkerDragOver(sender, e)`, `MarkerDropped(sender, e)`
- `AutoColor(index)` — the palette-derived colour a lane gets when it has none. The **same**
  set `KBotChartView.AutoColor` hands out (both delegate to `KBotAutoPalette`), so a lane and
  the chart line meaning the same thing can be given the same colour.
- `BeginInit`/`EndInit`, `ApplyTheme(scheme)`. `BackColor`/`ForeColor`/`Font` overridden, and
  `AllowDrop` **shadowed** and hidden from serialization (C4).

## Rules
- **The control never moves a marker.** It raises `MarkerDropped` and stops. The lanes are a
  projection of the host's picture; a marker moved locally would show a placement nobody has
  recorded. Same rule, same reason, as `AdvancedTreeControl.NodeDropped`.
- **`Allow` on `MarkerDragOver` defaults to False** — a host that forgets to answer lets
  nothing through instead of letting everything through.
- The dragged marker is read from the **data object**, never from a private field: that is
  what makes dragging between two lane views work, and it answers the same way when the
  source and the target are one control.
- The drop outline is drawn **on refusal too**. A lane that does not react at all reads as
  "the surface did not see me".
- **Nothing is greyed out.** `Locked` is drawn in full colour with a padlock — dimming was
  tried on the chart in slice 0048-06 and, where most of a chain is locked, turned the whole
  surface grey and destroyed the colour pairing that is colour's only job here.
- **Every marker paints the stretch it owns** (`SegmentedRail`, on by default): from itself
  to the next marker along, and — for the last one — to the right-hand end. That is a
  statement about the data, the same one the chart makes with a step line: what a marker
  records holds until the next one changes it. The plain `LaneLineColor` rail is still drawn
  underneath, full width, so an empty lane stays visible as somewhere to drop and the run
  before the first marker reads as empty rather than as absent. A `Loose` marker paints
  nothing — it is placed on nothing, so it owns nothing, and a stretch out of it would draw a
  chain where there is none.
- **`TrailingSpace` is what makes the last stretch visible.** Without it the latest marker
  lands exactly on the right edge and owns zero pixels — the one stretch still open is the
  one that disappears. The room is taken out of the time AXIS, not out of the surface, so
  markers and guides keep their relative dates and the axis labels move with them. Clamped to
  a quarter of the surface.
- Markers are **not sorted for you**, and two at the same moment are both drawn — several
  saves inside one minute is the case this was built for. The stretches are ordered by X
  before they are drawn, since one running to a marker on its LEFT would run backwards.
- Guides do **not** stretch the time axis: one outside the span of the markers is simply not
  drawn.
- The caption gutter and the end-mark gutter are reserved unconditionally when switched on,
  not "only while something needs them" — a gutter appearing the moment one lane gains an
  end mark would slide the whole time axis sideways under the pointer, mid-drag.
- Keys validated at `EndInit` and on `AddLane`: empty/duplicate → `ArgumentException`;
  unknown key on `SetLaneVisible` → `ArgumentException` (C3). Skipped at design time, where a
  **red frame** reports the defect instead (C6).

## Limits
- **Vertical scrolling only.** The horizontal axis is TIME, and a time axis running off the
  edge has stopped being a comparison between lanes, which is the one thing this is for. Too
  many lanes is an ordinary scroll; too long a span is answered by the enlarged window.
- No keyboard road to dragging: choosing a marker and choosing a lane are two selections this
  control does not have, and inventing them for a gesture done with the mouse anyway would be
  a second mechanism to keep true.
- No selection, no click event on a marker, no multi-select, no bulk move.
- The padlock at the compact marker size is two or three pixels of detail and reads as
  "something is on this one" rather than as a recognisable padlock. Accepted: it only has to
  be different enough that the operator does not try to drag it. Enlarged, it is a padlock.
- `MarkerSize = 0` is clamped to 2; there is no "no marker" mode, because without markers
  there is no hit target and nothing could be dragged or named.
- One shared time axis; no per-lane axis, no zoom, no pan.
