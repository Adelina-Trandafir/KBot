# SLICE-0085-02 — KBotDataView: button cell configurable per column

Operator request, 03.10.2026: columns of type `Button` in the custom grid must expose picture,
background colour (can be transparent), inner padding, font and caption for the button, plus an
outer padding / margin to position it, and alignment inside the cell.

Slice number: **assumed** 0085-02 — 0085 is the latest slice dedicated to the grid. Say so if it
should live under another number.

## What changed and why

New properties on `KBotDataColumn` (category «K-BOT: Button»; only `Button` columns read them):

| Property | Meaning | Default |
|----------|---------|---------|
| `ButtonText` | fixed caption; empty = cell text, then header, but with an image set empty = no caption (icon-only button) | empty |
| `ButtonImage` | picture before the caption, or alone | none |
| `ButtonBackColor` | `Empty` = theme, `Transparent` = flat (no face, no border), else that colour (border stays theme) | `Empty` |
| `ButtonBorderColor` | border colour: `Empty` = theme, `Transparent` = none (pass 2) | `Empty` |
| `ButtonBorders` | which sides have a border, flags (pass 2) | All |
| `ButtonPadding` | INNER gap, face to picture / caption (logical px) | 0,0,0,0 |
| `ButtonMargin` | OUTER gap, cell edge to face (logical px) | 4,3,4,3 |
| `ButtonAlign` | where the face sits in the cell | MiddleCenter |
| `ButtonSize` | face size (logical px), 0 on an axis = fill that axis | 0,0 |
| `ButtonFont` | caption font | not set = column font, then grid font |

`ButtonSize` was NOT in the request. It is needed: with a face that always fills the cell,
alignment has nothing to move. At 0,0 the button is the full-cell button of before.

Behaviour changes worth knowing:

- **A click acts only on the face.** Before, the whole cell (including the 4/3 px margin) fired
  `ButtonClick`. With a smaller button, clicking empty cell space must not fire it. Space on the
  current cell still fires. `CellClick` still fires for any click.
- **`Transparent` also drops the border** (until pass 2: unless `ButtonBorderColor` is set).
- Picture and caption are laid out as one group, centred in the padded face; picture keeps its
  pixel size scaled by the DPI and is shrunk to fit, keeping proportions; wider than the face =
  picture left, caption cut with an ellipsis. Disabled = picture drawn at 40% opacity.
- Width measuring (`ToContent`) now needs caption + picture + gap + padding + margin; a fixed
  `ButtonSize.Width` makes it that width + margin.
- `ButtonFont` goes through `CellFontFor`, the single font source for painting and measuring.
- All metrics are logical and scaled with `ScaleDpi` at use (painter, measuring, hit-test share
  `ButtonFaceRect`).
- `ButtonText` becomes the default `Text` handed to `CellFormatting`, so a handler can still set
  a different caption per row.

## Pass 2 (operator follow-up, 03.10.2026): border colour + visible sides

Read as the BUTTON's border, per column (the request said "each cell"; assumed it meant the
button, like the rest of this slice — not the grid lines).

- `ButtonBorderColor`: `Empty` = theme, `Transparent` = no border, else that colour. Disabled
  cells keep the greyed theme border whatever colour was asked.
- `ButtonBorders` (new flags enum `KBotBorderSides`: Left, Top, Right, Bottom, `All`, `None`),
  default `All`. `All` = the rounded border of before. Any other value = straight lines on the
  chosen sides and the face drawn as a plain rectangle (rounded corners need a closed outline;
  a half-rounded corner looked worse than square ones).
- Pass 1 said `Transparent` back colour drops the border. Now: it drops the border only while
  `ButtonBorderColor` is not set; an explicit border colour brings it back, so a filled-less
  button with a border is reachable.
- Cost: no new cached objects; a `Pen` / `SolidBrush` is created and disposed per painted
  button cell ONLY when a custom border or face colour is set (the old code already created a
  brush per cell for custom backgrounds). The default path allocates nothing.

## Pass 3 (operator follow-up, 03.10.2026): the settings in the DevHarness playground

The new `Button*` properties had no switches in `DataViewPlaygroundForm`, so they could not be
tried. Added a «-- Button column (det) --» section to its left panel (declared in the
`.Designer.vb`, inserted after the column inspector). It always writes into the «det» (Button)
column, not into the column picked in the inspector. Switches: `ButtonText`, `ButtonImage` (a
16x16 test icon drawn in code), `ButtonBackColor` and `ButtonBorderColor` (theme / transparent /
two colours each), the four `ButtonBorders` sides, `ButtonAlign` (all nine), `ButtonSize` W/H,
`ButtonMargin` and `ButtonPadding` (left-right and top-bottom each) and `ButtonFont` (none / Segoe UI
14 bold). Alignment only shows once a size is set. The icon and font are released on close after
the column lets go of them. Files: `KBot.DevHarness/Internal/DataViewPlaygroundForm.vb` and
`.Designer.vb`. `dotnet build src\KBot.DevHarness\KBot.DevHarness.vbproj` — 0 warnings, 0 errors;
not opened on screen. New tab indexes (48-75) come after the «Date» section, so Tab order in the
panel is not top-to-bottom for the new section.

## Pass 4 (operator follow-up, 03.10.2026): borders of EVERY cell, not only the button

The operator meant the borders of the cells themselves, on every element; the button border from
pass 2 stays. Done per column and, through `CellFormatting`, per single cell.

- `KBotDataColumn.CellBorderColor` (`Empty` = theme grid-line colour, `Transparent` = invisible)
  and `CellBorders` (flags, **default Right + Bottom = the grid lines of before**).
- `KBotCellFormattingEventArgs.BorderColor` / `.Borders`, pre-filled from the column, so a
  handler can change the borders of one cell.
- The enum is now shared: `KBotButtonBorders` renamed `KBotBorderSides` (file
  `KBotBorderSides.vb`); the button keeps `ButtonBorders` with the same type.
- Drawing (`KBotDataView.CellBorders.vb`): each cell draws its chosen sides on its outermost
  pixels, after its content. The row keeps its single full-width line (see the fast path below);
  the stretch of row to the right of the last column keeps it only while the LAST column still has
  a Bottom side. With the defaults the picture is meant to be the same as before; not compared
  pixel by pixel.
- **Fast path on the default (agreed 03.10.2026, after a /btw side chat on cost).** The line
  under a row is again ONE `DrawLine` across the row. A cell whose bottom side is not the plain
  grid line (no Bottom, or a custom colour — from the column or from `CellFormatting`) reports
  its span in `_rowGaps` while it is painted; the row line is drawn around those spans
  (`DrawRowBottomLine`) and the cell draws its own bottom. Default cells draw only their right
  side, so the default cost is what it was before this slice: one line per cell + one per row,
  no allocation. Scroll-band spans are clamped so they never cut the line under the frozen band.
  (The first version of this pass drew a bottom line per cell; replaced by this.)
- Cost of the non-default cells: up to four lines each; no allocation per cell. The theme colour
  uses the cached grid-line pen; custom colours share ONE pen kept for the last colour (rebuilt
  only when the colour changes), disposed with the other pens.
- Two neighbours that both draw the edge they share (A's Right + B's Left, or a row's Bottom +
  the next row's Top) make a double line. Not prevented: it is what was asked for; pick one side
  per edge for a single line.
- Playground (`DataViewPlaygroundForm`): in the column inspector, `CellBorderColor`, the four
  `CellBorders` sides (loaded from / written to the column picked in the inspector) and a checkbox
  that turns on a per-cell demo (a red box on every 4th row of «Cod», through `CellFormatting`).
  Builds: Controls and DevHarness 0 warnings, 0 errors; nothing opened on screen.

Not verified: how the lines look on screen at any DPI, frozen columns with horizontal scroll
(lines of scrolled cells are clipped under the frozen band like the cells), group bands and the
footer (untouched, still use their own separators). Help: no change (no window uses it yet).

## Files touched

- `src/KBot.Controls/DataView/KBotDataColumn.vb` — the eight properties, with
  `ShouldSerialize*` / `Reset*` for Image, Color, Padding, Size and Font.
- `src/KBot.Controls/DataView/KBotDataView.CellButton.vb` — NEW partial: face geometry, drawing,
  hit-test, measuring, caption rules, border drawing. Registered in `KBot.Controls.vbproj`
  (`DependentUpon`).
- `src/KBot.Controls/DataView/KBotBorderSides.vb` — NEW flags enum (pass 2).
- `src/KBot.Controls/DataView/KBotDataView.Painting.vb` — `DrawCell` calls the new drawing; old
  `DrawButtonCell` removed.
- `src/KBot.Controls/DataView/KBotDataView.Theming.vb` — `CellFontFor` honours `ButtonFont`.
- `src/KBot.Controls/DataView/KBotDataView.AutoSize.vb` — button width need, caption fallback.
- `src/KBot.Controls/DataView/KBotDataView.Input.vb` — mouse-up acts only on the face.
- `src/KBot.Controls/DataView/KBotDataView.md` — new properties, click rule.
- `src/KBot.Controls/KBot.Controls.vbproj` — FileVersion 1.61.0.0 -> 1.62.0.0.

## Test results

- `dotnet build src\KBot.Controls\KBot.Controls.vbproj` — 0 warnings, 0 errors.
- No tests written or run (standing rule), nothing rendered, nothing opened in the designer.

## Left unverified or deferred

- Not seen on screen at any DPI; the layout maths is unchecked against a real render.
- Not checked in the Visual Studio property grid / designer serialization (a freshly dropped
  column should write zero `Button*` lines — the `ShouldSerialize*` pairs are there for it).
- Existing Button columns (HelpCaptureForm, RobotQueueForm, ClasificatiiForm, ParteneriForm,
  harness forms) were not touched; with the defaults they should paint as before, except the
  click now ignoring the 4/3 px margin. Not verified on screen.
- No hover / pressed look and no hand cursor over the button (never existed; not requested).
- Help: no change — no existing window uses the new properties, so the operator sees nothing new.
