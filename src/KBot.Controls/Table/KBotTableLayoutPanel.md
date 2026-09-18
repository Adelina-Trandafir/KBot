# KBotTableLayoutPanel

The house `TableLayoutPanel`. Fixed rows, fixed columns and the table's own `Padding` are
authored in LOGICAL pixels (96 dpi) and computed for the screen from `AppScaling.FactorFor`
-- the same factor the tree and the grid use for their rows -- never from the platform's font
ratio. Fixed rows/columns also grow by exactly the surplus their themed content asks for
(slice 0062) and come back when it goes away. Cell lines can be painted in the theme colour.

`Table/` -- `KBotTableLayoutPanel.vb` (switches, colours, themed grid), `.Dpi.vb` (the
logical model, the triggers, the runtime API), `.Fit.vb` (the surplus rule and
`GetPreferredSize`).
`TableLayoutPanel` · partial · Toolbox · `IThemedContainer`, `IDpiScaledControl`
Conventions: [C1..C9](../CONTROLS.md).
Status: **screen-verified 2026-09-17 through `DrawToBitmap`** (slice 0066, six states on a
150% monitor with text at 110%: Classic/Modern, Automatic/Fixed100/Manual, the API moves,
and back). Never driven by hand. Harness: `TableLayoutHarnessTest` (0066),
`FormFitHarnessTest` (0062, the log-viewer filter row).

## Every TableLayoutPanel in the solution is one of these
Since 0066 no `.Designer.vb` in `src/` declares a plain `TableLayoutPanel` (35 designers,
~80 tables, in `KBot.App`, `KBot.Controls`, `KBot.DevHarness`, `KBot.Migrator`). The only
plain ones left are the `_reference/` snapshot in `KBot.Forexe` (not compiled) and the
parameter types of `ThemeTableFit`, which has no caller in `src/` any more.

## The model (C2, made concrete)
| Measure | Authored (logical) | Live (device) | Written by |
|---|---|---|---|
| Absolute `ColumnStyle.Width` | snapshot `_logicalCols(i)` | `Round(logical x scale) + surplus`, or 0 when collapsed | `Refit` |
| Absolute `RowStyle.Height` | snapshot `_logicalRows(i)` | same | `Refit` |
| `Padding` (shadowed) | `_logicalPadding` -- what the getter returns and the designer serializes | `MyBase.Padding` = `PaddingPx` | `ApplyMetricScale` |
| Percent / AutoSize styles | -- | never touched | -- |
| children's `Margin` | -- | the platform's (the child's property) | -- |

Scale = `AppScaling.FactorFor(Me)`: `DeviceDpi / 96` x text size under Automatic, 1 under
Fixed100, the operator's number under Manual; 1 at design time (C6 -- unlike the tree,
because the VS surface stamps device pixels into a table's styles). Read back through
`DpiScale`. `ScaleAbsoluteStyles = False` pins the scale of THIS table at 1.

**When the snapshot is taken:** at the first hook, whichever it is -- `ScaleControl` BEFORE
the base call (the platform's first autoscale is the first thing that touches the styles
after `InitializeComponent`), `OnHandleCreated`, `ApplyTheme`, `RefreshDpiMetrics`. A style
count that changed (rows added at runtime) is re-read: untouched entries keep their logical
value, new ones are read from the live value and unscaled.

**Why the platform is a trigger and not a source:** measured on a 150% monitor, WinForms'
font autoscale multiplies Absolute styles and Padding by 1.43 on X and 1.67 on Y, and does
it again at every font change on whatever is current. After every base `ScaleControl` the
live values are rewritten from the logical source, so its number never survives and two
passes cannot compound.

## Runtime API (all logical, all idempotent)
- `SetRowHeight(i, logical)` / `SetColumnWidth(i, logical)` -- writes the authored measure,
  makes the style Absolute, lifts a collapse. Negative is clamped to 0; bad index throws.
- `SetRowCollapsed(i, bool)` / `SetColumnCollapsed(i, bool)` + `IsRowCollapsed` /
  `IsColumnCollapsed` -- a fixed line written as 0, kept so across every scale and theme
  pass. A Percent/AutoSize line throws (C3): it has no authored measure to come back to.
- `Padding = New Padding(8)` -- logical in; `PaddingPx` is what the layout engine uses.
- `RefitToTheme()` -- the whole pass on demand (a host that changed the content before it
  measures the window, e.g. `KBotFilterPopup.AjusteazaInaltimea`).
- `ResetStyleBaseline()` -- the escape hatch: a style written directly (device pixels) is
  adopted as the new authored value; styles still at what the control last wrote are left
  alone. Prefer the methods above.
- Seams: `DebugAuthoredRow/Column(i)` (logical, -1 before any snapshot), `HasStyleBaseline`.

## The fit (slice 0062, kept)
A fixed line is `scaled authored + Max(0, surplus)`, surplus = the greediest single-cell
child's demand minus the scaled authored measure. Demand = `ThemeFormFit.ContentDemand`
plus the child's margins. Never `Width`/`Height` (the table clips a docked child to its cell,
so a 56px button in a 40px cell reports 40 -- slice 0030). `GetPreferredSize` answers from
content only, includes the device padding and skips collapsed lines.

Three measuring rules landed in 0066 because the bench showed the fit could not come BACK:
- a self-painting themed control that owns its children (tree, grid, nav list...; not a
  container, not a composite `UserControl`) demands nothing -- its scrollbars and search box
  are positioned from its own size and only echoed the cell (a 300px column with a tree grew
  to 628);
- `KBotTextField` / `KBotTextBox` report width 0 when docking stretches them (they have no
  intrinsic width; their `Width` IS the cell);
- a docked `Button` answers `GetPreferredSize` with its bounds on every `FlatStyle`
  (measured), so `ThemeFormFit` measures it by hand: padding + one line of text + borders.

## Painting
`ThemedCellBorder = True` fills the background and paints the grid in
`EffectiveCellBorderColor` (`CellBorderColor`, Empty = theme `BorderColor`) in the gap the
native `CellBorderStyle` reserves; `None` is promoted to `Single` and refused afterwards. The
gap itself (1/2/3 px) is the platform's and is not scaled.

## Where it stops
- Children's `Margin`s are the platform's: 1.43 x 1.67 at 150%, not 1.5. A cell whose
  margin must match a drawn measure exactly does not exist yet; if one appears, the margin
  belongs in the child, not here.
- Under Fixed100/Manual the table goes back to its logical pixels while the platform-scaled
  controls around it (bounds, fonts) stay large -- the documented compromise of those modes,
  visible on purpose in the bench.
- `ButtonDemand` under-asks by the few pixels of chrome WinForms adds around a button's
  text; a row authored TIGHTER than its button's text + padding will clip the chrome, not
  the text.
- Never opened in the VS designer; C6 behaviour (scale 1) is reasoned, not seen.
