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
Status: **screen-verified 2026-09-18 through `DrawToBitmap`** (slice 0066-02: the 0062 and 0066
benches at 150% under Automatic, Fixed100 = the 100% look and Manual 1.25 = the 125% look, all
three the same picture at 2/3, 5/6 and 1; a 144-authored form untouched at 150% and exactly 2/3
under Fixed100). Driven by hand on 2026-09-18 by the operator at 100/125/150% BEFORE 0066-02,
which is what showed the forms themselves were not on the tree's ruler. Harness: `TableLayoutHarnessTest` (0066),
`FormFitHarnessTest` (0062, the log-viewer filter row).

## Every TableLayoutPanel in the solution is one of these
Since 0066 no `.Designer.vb` in `src/` declares a plain `TableLayoutPanel` (35 designers,
~80 tables, in `KBot.App`, `KBot.Controls`, `KBot.DevHarness`, `KBot.Migrator`). The only
plain ones left are the `_reference/` snapshot in `KBot.Forexe` (not compiled) and the
parameter types of `ThemeTableFit`, which has no caller in `src/` any more.

## The model (C2, made concrete)
| Measure | Authored (logical) | Live (device) | Written by |
|---|---|---|---|
| Absolute `ColumnStyle.Width` | snapshot `_logicalCols(i)` = authored x 96 / `DesignDpi` | `Round(logical x scale) + surplus`, or 0 when collapsed | `Refit` |
| Absolute `RowStyle.Height` | snapshot `_logicalRows(i)` | same | `Refit` |
| `Padding` (shadowed) | `_logicalPadding` -- the designer's value x 96 / `DesignDpi` after the first snapshot; what the getter returns and (at design time, unconverted) the designer serializes | `MyBase.Padding` = `PaddingPx` | `ApplyMetricScale` |
| Percent / AutoSize styles | -- | never touched | -- |
| children's `Margin` | -- | the platform's (the child's property) | -- |

Scale = `AppScaling.FactorFor(Me)`: `DeviceDpi / 96` x text size under Automatic, 1 under
Fixed100, the operator's number under Manual; 1 at design time (C6 -- unlike the tree,
because the VS surface stamps device pixels into a table's styles). Read back through
`DpiScale`. `ScaleAbsoluteStyles = False` pins the scale of THIS table at 1.

**In which pixels the designer wrote (0066-02):** those of the screen the file was saved on --
the form's `AutoScaleDimensions` says which (`(144, 144)` for a file saved at 150%), and every
Bounds in the file is in the same pixels. The first snapshot reads that stamp from the nearest
container that scales for itself (`DesignDpi`, 96 for a Font/None container, no container, or
design time) and divides, so a 48px row authored at 144 IS the 32px row authored at 96, and
both come out 48 on a 150% screen and 32 on a 100% one -- exactly like the platform treats the
Bounds around them. A table built in code writes logical values (no stamp above it).

**When the snapshot is taken:** at the first hook, whichever it is -- `ScaleControl` BEFORE
the base call (the platform's first autoscale is the first thing that touches the styles
after `InitializeComponent`), `OnHandleCreated`, `ApplyTheme`, `RefreshDpiMetrics`. A style
count that changed (rows added at runtime) is re-read: untouched entries keep their logical
value, new ones are read from the live value and unscaled.

**Why the platform is a trigger and not a source:** until 0066-02 the forms were
`AutoScaleMode.Font`, whose autoscale multiplied Absolute styles and Padding by the ratio of two
INTEGER font metrics -- 1.43 on X and 1.67 on Y on a 150% monitor -- and again at every font
change on whatever was current. The forms are `AutoScaleMode.Dpi` now (exact, uniform), but the
platform still multiplies the CURRENT value at every pass (the operator's zoom, a DPI change).
After every base `ScaleControl` the live values are rewritten from the logical source, so its
number never survives and two passes cannot compound.

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
- Seams: `DebugAuthoredRow/Column(i)` (logical, -1 before any snapshot), `HasStyleBaseline`,
  `DesignDpi` (the stamp the snapshot was read against; 96 before it).

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
- A table built in code and added to a form already on screen has no stamp of its own: its
  styles are read as logical. Write them through `SetRowHeight` / `SetColumnWidth` to be sure.
- `ButtonDemand` under-asks by the few pixels of chrome WinForms adds around a button's
  text; a row authored TIGHTER than its button's text + padding will clip the chrome, not
  the text.
- Never opened in the VS designer; C6 behaviour (scale 1) is reasoned, not seen.
