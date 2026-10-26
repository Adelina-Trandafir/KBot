# KBotGraphView

Owner-drawn MAP of dots and lines: one dot per thing, one line per link between two things, on a surface that zooms and moves. The K-BOT control for "what is connected to what". Written entirely in English, property-grid category names included.

`Graph/` — `KBotGraphView.vb` + `.Painting.vb`, `KBotGraphNode.vb` (holds `KBotGraphNode` and `KBotGraphEdge`), `KBotGraphLayout.vb` `Control` · sealed · partial · Toolbox · `IThemedControl` Conventions: [C1..C9](../CONTROLS.md). Status: **code-green build, never seen on screen.**

## What it is for, and how it differs from the chart and the lanes
The chart answers *how did this move*, the lanes *which one does it belong to*. This answers *what is linked to what*. Written for the KBotAdmin «Istoric md» page (markdown files and the links between them), where the picture has to stay still while a time cursor shows and hides dots.

## Model
- **KBotGraphNode** (made by `AddNode(key, text)`): `Key` (non-empty, unique), `Text` (label title + name beside the dot), `Tooltip` (label body, `KBotToolTip` markup), `Group` (nodes with the same group are laid out together), `NodeColor` (Empty = scheme accent, C1), `Radius` (logical px; 0 = the control's `NodeRadius`), `Visible = True` (False = not drawn, no hover, no line to it; the position is kept), `Highlighted` (a ring: "look here"), `X`/`Y` (WORLD coordinates, not pixels), `Tag`.
- **KBotGraphEdge** (made by `AddEdge(fromKey, toKey)`): `FromNode`, `ToNode`, `EdgeColor` (Empty = dimmed text colour), `Faint` (drawn at half strength: a link that is implied rather than written), `Tag`. No direction on the surface.
- Both are RUNTIME data (not serialized by the designer): a graph is a picture of other data, rebuilt on every load.

## Control API
- Data: `Nodes`, `Edges` (read-only lists), `AddNode`, `AddEdge`, `FindNode`, `NeighbourKeys(key)`, `ClearGraph()` (keeps zoom/pan), `BeginUpdate()`/`EndUpdate()`, `RefreshGraph()` (after the host changed a node directly), `EmptyText`
- Positions: the HOST sets `X`/`Y` — `LayoutNodes(iterations = 120)` does it on the UI thread, or call `KBotGraphLayout.Compute(groupOf, edgeFrom, edgeTo, iterations)` in the background and write the points yourself
- View: `FitToView()` (all dots, visible or not — the picture does not jump while dots appear), `SelectedKey` (setting it raises nothing)
- Look: `NodeRadius = 4`, `EdgeWidth = 1`, `EdgeOpacity = 45` (%), `LabelsVisible = False`, `PlotBackColor`, `EdgeColor`, `AutoColor(index)` (the same set the chart and the lanes hand out, never red)
- Hover: `PointTooltip: KBotToolTip`, `PointTooltipEnabled = True`, `HoverRadius = 6`
- Events: `NodeClicked(key)` (key = `Nothing` for the empty background: selection cleared), `NodeDoubleClicked(key)`, `NodeHovered(key)` (`Nothing` = left every dot)
- `ApplyTheme(scheme)`. `BackColor`/`ForeColor`/`Font` overridden with pin flags (C4).

## Interaction
Wheel = zoom around the pointer. Drag the background (or a dot) = move; a press that does not move is a click. `Home` = fit, `Esc` = clear the selection. Hovering (or selecting) a dot FADES everything except it and its neighbours, draws its own lines in the accent colour and writes the names of the neighbours.

## Layout (`KBotGraphLayout`)
Deterministic, no random numbers. Groups are packed as discs from the middle outwards (largest first); the nodes of a group start on a sunflower spiral in their disc; then Fruchterman-Reingold passes (every pair pushes apart within a cutoff found with a grid, every edge pulls together, a gentle pull to the group's own centre keeps clusters visible). A thousand nodes and 140 passes is tens of millions of cheap operations.

## Rules
- The control never moves a dot by itself; positions come from the host.
- Keys validated on add: empty/duplicate/unknown → `ArgumentException` (C3).
- `Visible = False` keeps the position, so stepping through time never reshuffles the map.

## Limits
- No dragging of single dots, no keyboard road between dots, no multi-select.
- No edge labels, no arrows, no curved edges.
- Dot hit test is a linear scan (fine to a few thousand dots).
- Never seen on screen: the force constants (`KBotGraphLayout.Spacing`, the pull to the group centre, the cutoff) are first guesses.
