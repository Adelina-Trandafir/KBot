# SLICE-0085 — Istoric DataView: editor, butoane și borduri

Jurnalele din 26 septembrie și 3 octombrie 2026 sunt reunite integral. Etapele descriu schimbări succesive, nu opțiuni simultane: trecerea 2 din baza 0085 înlocuiește poziționarea cursorului la clic cu selectarea întregului text și folosește culoarea nesel. a celulei. În 0085-02, trecerea 2 precizează bordura butonului, iar trecerea 4 adaugă separat bordurile tuturor celulelor. Numărul 0085-02 a fost presupus în jurnalul original; consolidarea nu atribuie o confirmare nouă. Contractul API rămâne în [documentația DataView](../../src/KBot.Controls/DataView/KBotDataView.md). Rezultatele și limitele de mai jos sunt istorice.

[0085-dataview](#slice-0085-baza) · [0085-02](#slice-0085-02)

## SLICE-0085 — KBotDataView: the editor looks like the cell, single-click editing, Up/Down through the column <a id="slice-0085-baza"></a>

Operator request, 26.09.2026, slice dedicated to the custom grid (`KBot.Controls/DataView`):

1. When a cell is edited, the text box must follow the cell's padding and size, have NO border, use the same font (family, size, style), and place and format the text like the cell, so the operator cannot tell a different control took over.
2. Clicking an editable cell starts editing immediately (not on double click).
3. Up/Down move the edit to the cell above/below in the same column when there is more than one row, ignoring the footer.
4. (Added mid-task) It must respect the DPI settings.

### What changed and why

**Editor placement (`PlaceEditor`, new).** The text editor is now `BorderStyle.None`, `AutoSize = False` (Designer). On every open and on every layout pass (`UpdateLayout` → resize, column width, theme change, DPI change via `ApplyMetricScale` → `LayoutChanged`) it is:

- dressed with the cell's *resolved* look: `ResolveCellLook` runs the same chain as the painter (normal / alternating / selected row colours → `RowFormatting` → `CellFormatting`) with fresh argument instances, so font, back colour, fore colour and horizontal alignment are exactly what `DrawCell` would use;
- placed in the cell's content rectangle (`CellContentRect`, i.e. the cell minus `CellPadding` scaled by `ScaleDpi`), full content width, one text line high;
- given edit-control margins (`EM_SETMARGINS`) equal to TextRenderer's glyph padding, measured on the grid's own device context (`MeasureTextPadding`: total from `MeasureText` with and without `NoPadding`, split left = ceil(h/6), rest right). Measured in device pixels, so it follows the DPI with no extra scaling;
- vertically centred like `TextFormatFlags.VerticalCenter`, rounding the spare height up (GDI does; plain integer division left the editor 1 px high — measured, see test results).

The painter (`DrawCell`) skips the text of the cell being edited (`IsEditingCell`), so the old value never shows around the caret. The combo editor keeps the whole cell (a ComboBox fixes its own height from its font) and only takes the cell's font and colours.

**Single-click editing.** `OnMouseDown`, after selecting the cell, opens the editor when the cell is editable and puts the caret where the click landed (`PlaceCaretAt`). A click inside the cell already being edited (its padding around the one-line editor) keeps the edit and only moves the caret (`HandleMouseDownOnEditingCell`, checked before the grid takes focus). F2 and double click still open the editor; double click no longer re-opens an editor already open on that cell. The two hosts with `CellDoubleClick` handlers (`DdfEditSectiuneaAPage` «Clsf», `AlegereUnitateForm`) use read-only columns, so a single click never opens an editor there and their double click still reaches the grid.

**Unchanged commit writes nothing.** `CommitEdit` closes without validation, `IsDirty` or `CellValueChanged` when `EditorUnchanged` finds the original text. Single-click editing otherwise made merely passing through a cell (including an arrow press) mark the row edited and raise the event.

**Up/Down.** `MoveEditVertical` / `NextEditableRow` (Friend) find the nearest editable cell in the same column, in DRAWN order. They skip disabled/non-editable rows and group bands; collapsed rows have no band, and the grid footer is outside the bands. At the boundary or on a single row, nothing commits or moves: the editor/caret stay. Previously a disabled target dropped the edit back to the grid.

**Rule 0 sweep.** `KBotDataView.Editing.vb` (all comments, the two designer `Description`s and the `ArgumentException` text) and `KBotDataView.Designer.vb` are now English/ASCII, plus the comments of the `OnMouseDown` / `OnMouseDoubleClick` blocks touched in `KBotDataView.Input.vb`.

### Files touched

`src/KBot.Controls/DataView/KBotDataView.Editing.vb` — placement, look resolution, padding measure, caret placement, unchanged-commit rule, `NextEditableRow` / `MoveEditVertical`; swept. `src/KBot.Controls/DataView/KBotDataView.Designer.vb` — `editText`: `AutoSize = False`, `BorderStyle.None`; swept. `src/KBot.Controls/DataView/KBotDataView.Input.vb` — single-click editing, click inside the edited cell, double click guard. `src/KBot.Controls/DataView/KBotDataView.Painting.vb` — skip the edited cell's text. `src/KBot.Controls/DataView/KBotDataView.Layout.vb` — `UpdateLayout` re-places the editor. `src/KBot.Controls/DataView/KBotDataView.md` — Up/Down wording + new section «The editor looks like the cell».

### Test results

- `dotnet build src\KBot.Controls\KBot.Controls.vbproj` — 0 warnings, 0 errors.
- `dotnet build src\KBot.App\KBot.App.vbproj` — 0 warnings, 0 errors.
- **Pixel check (scratch program, off-screen form, not in the repo).** For five columns — left Segoe UI 9, right-aligned numbers, centred Calibri 11 italic with padding 10/2/10/2, padding 4/3/4/0, Tahoma 10 with padding 4/0/4/1 — the grid was drawn with `DrawToBitmap` before and after opening the editor on the selected cell (selection cleared), and the two images were compared pixel by pixel:
  - 144 dpi (this machine, PerMonitorV2): **0 differing pixels in all five cells.**
  - 96 dpi (same program run DPI-unaware): 0 differing pixels in four cells; the Tahoma cell matches in position and size (best-fit offset 0,0) but its ClearType colour fringes differ. Most likely a capture artifact of DPI-unaware mode — **not confirmed**.
- **Behaviour check (same scratch program, messages sent to the controls):** single click on an editable cell → editing, focus in the editor, caret at the click point, no selection; click in the edited cell's padding → edit kept, caret moved, 0 value events; Down from row 0 with row 2 disabled → 1, 3, 4, stays on 4 (still editing); Up → 3, 1, 0, stays on 0; passing through untouched cells → 0 `CellValueChanged`.
- Unit tests: **not run and not written** (standing rule: no tests unless asked). Reading `KBotDataViewEditingTests` / `InputTests` / `FooterTests`, every commit there first types a new text, so the unchanged-commit rule should not affect them. `BeginEdit` now creates the editor's handle and a `Graphics` (margins + measuring), which the headless tests have never done — not verified.

### Left unverified or deferred

- Not seen in the real app on the operator's screen; the checks above are off-screen captures.
- The combo editor does not get the borderless/padding treatment (a native ComboBox draws its own frame and fixes its own height). It only takes the cell's font and colours.
- The rest of `KBotDataView.Input.vb`, `.Painting.vb`, `.Layout.vb` and the other partials still carry Romanian comments with diacritics (rule 0). Only the blocks touched here were swept.
- Hosts that relied on `CellValueChanged` firing for an unchanged commit (none found by search) would now get nothing.

### Pass 2 (operator follow-up, 26.09.2026)

1. **A click selects the whole text** (not a caret at the click point). `BeginEdit` already selects all; the click path no longer moves the caret, and a click in the padding of the cell already being edited also re-selects the whole text. `PlaceCaretAt` was removed.
2. **The edited cell does NOT take the selected-row colour.** `ResolveCellLook` now always resolves the UNSELECTED look (normal / alternating row colour, then `RowFormatting` / `CellFormatting`), and the painter fills the whole edited cell (padding included) with that colour (`_editBackColor`), so the cell being edited stands out from the selected row. Columns have no colour of their own, so "the column's default cell colour" = the unselected row colour after the formatting handlers.
3. **I-beam cursor** over every cell a click would edit (`CanEdit`); read-only / disabled cells keep the arrow. Set in `OnMouseMove` after the resize-edge / icon / group-band checks.

Pass 2 checks (same off-screen scratch program, 144 dpi): the edited cell's region is pixel-identical to the same cell drawn on an UNSELECTED row (0 differing pixels, five cells); its padding pixel = row colour (248,248,248) while the rest of the selected row is (203,225,242); click → `SelectionStart = 0`, `SelectionLength = TextLength`; cursor = IBeam over the editable column, Default over the read-only one. Builds: Controls and App 0 / 0.

---

## SLICE-0085-02 — KBotDataView: button cell configurable per column <a id="slice-0085-02"></a>

Operator request, 03.10.2026: columns of type `Button` in the custom grid must expose picture, background colour (can be transparent), inner padding, font and caption for the button, plus an outer padding / margin to position it, and alignment inside the cell.

Slice number: **assumed** 0085-02 — 0085 is the latest slice dedicated to the grid. Say so if it should live under another number.

### What changed and why

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

`ButtonSize` was NOT in the request. It is needed: with a face that always fills the cell, alignment has nothing to move. At 0,0 the button is the full-cell button of before.

Behaviour changes worth knowing:

- **A click acts only on the face.** Before, the whole cell (including the 4/3 px margin) fired `ButtonClick`. With a smaller button, clicking empty cell space must not fire it. Space on the current cell still fires. `CellClick` still fires for any click.
- **`Transparent` also drops the border** (until pass 2: unless `ButtonBorderColor` is set).
- Picture and caption are laid out as one group, centred in the padded face; picture keeps its pixel size scaled by the DPI and is shrunk to fit, keeping proportions; wider than the face = picture left, caption cut with an ellipsis. Disabled = picture drawn at 40% opacity.
- Width measuring (`ToContent`) now needs caption + picture + gap + padding + margin; a fixed `ButtonSize.Width` makes it that width + margin.
- `ButtonFont` goes through `CellFontFor`, the single font source for painting and measuring.
- All metrics are logical and scaled with `ScaleDpi` at use (painter, measuring, hit-test share `ButtonFaceRect`).
- `ButtonText` becomes the default `Text` handed to `CellFormatting`, so a handler can still set a different caption per row.

### Pass 2 (operator follow-up, 03.10.2026): border colour + visible sides

Read as the BUTTON's border, per column (the request said "each cell"; assumed it meant the button, like the rest of this slice — not the grid lines).

- `ButtonBorderColor`: `Empty` = theme, `Transparent` = no border, else that colour. Disabled cells keep the greyed theme border whatever colour was asked.
- `ButtonBorders` (new flags enum `KBotBorderSides`: Left, Top, Right, Bottom, `All`, `None`), default `All`. `All` = the rounded border of before. Any other value = straight lines on the chosen sides and the face drawn as a plain rectangle (rounded corners need a closed outline; a half-rounded corner looked worse than square ones).
- Pass 1 said `Transparent` back colour drops the border. Now: it drops the border only while `ButtonBorderColor` is not set; an explicit border colour brings it back, so a filled-less button with a border is reachable.
- Cost: no new cached objects; a `Pen` / `SolidBrush` is created and disposed per painted button cell ONLY when a custom border or face colour is set (the old code already created a brush per cell for custom backgrounds). The default path allocates nothing.

### Pass 3 (operator follow-up, 03.10.2026): the settings in the DevHarness playground

The new `Button*` properties had no switches in `DataViewPlaygroundForm`, so they could not be tried. Added a «-- Button column (det) --» section to its left panel (declared in the `.Designer.vb`, inserted after the column inspector). It always writes into the «det» (Button) column, not into the column picked in the inspector. Switches: `ButtonText`, `ButtonImage` (a 16x16 test icon drawn in code), `ButtonBackColor` and `ButtonBorderColor` (theme / transparent / two colours each), the four `ButtonBorders` sides, `ButtonAlign` (all nine), `ButtonSize` W/H, `ButtonMargin` and `ButtonPadding` (left-right and top-bottom each) and `ButtonFont` (none / Segoe UI 14 bold). Alignment only shows once a size is set. The icon and font are released on close after the column lets go of them. Files: `KBot.DevHarness/Internal/DataViewPlaygroundForm.vb` and `.Designer.vb`. `dotnet build src\KBot.DevHarness\KBot.DevHarness.vbproj` — 0 warnings, 0 errors; not opened on screen. New tab indexes (48-75) come after the «Date» section, so Tab order in the panel is not top-to-bottom for the new section.

### Pass 4 (operator follow-up, 03.10.2026): borders of EVERY cell, not only the button

The operator meant the borders of the cells themselves, on every element; the button border from pass 2 stays. Done per column and, through `CellFormatting`, per single cell.

- `KBotDataColumn.CellBorderColor` (`Empty` = theme grid-line colour, `Transparent` = invisible) and `CellBorders` (flags, **default Right + Bottom = the grid lines of before**).
- `KBotCellFormattingEventArgs.BorderColor` / `.Borders`, pre-filled from the column, so a handler can change the borders of one cell.
- The enum is now shared: `KBotButtonBorders` renamed `KBotBorderSides` (file `KBotBorderSides.vb`); the button keeps `ButtonBorders` with the same type.
- Drawing (`KBotDataView.CellBorders.vb`): each cell draws its chosen sides on its outermost pixels, after its content. The row keeps its single full-width line (see the fast path below); the stretch of row to the right of the last column keeps it only while the LAST column still has a Bottom side. With the defaults the picture is meant to be the same as before; not compared pixel by pixel.
- **Fast path on the default (agreed 03.10.2026, after a /btw side chat on cost).** The line under a row is again ONE `DrawLine` across the row. A cell whose bottom side is not the plain grid line (no Bottom, or a custom colour — from the column or from `CellFormatting`) reports its span in `_rowGaps` while it is painted; the row line is drawn around those spans (`DrawRowBottomLine`) and the cell draws its own bottom. Default cells draw only their right side, so the default cost is what it was before this slice: one line per cell + one per row, no allocation. Scroll-band spans are clamped so they never cut the line under the frozen band. (The first version of this pass drew a bottom line per cell; replaced by this.)
- Cost of the non-default cells: up to four lines each; no allocation per cell. The theme colour uses the cached grid-line pen; custom colours share ONE pen kept for the last colour (rebuilt only when the colour changes), disposed with the other pens.
- Two neighbours that both draw the edge they share (A's Right + B's Left, or a row's Bottom + the next row's Top) make a double line. Not prevented: it is what was asked for; pick one side per edge for a single line.
- Playground (`DataViewPlaygroundForm`): in the column inspector, `CellBorderColor`, the four `CellBorders` sides (loaded from / written to the column picked in the inspector) and a checkbox that turns on a per-cell demo (a red box on every 4th row of «Cod», through `CellFormatting`). Builds: Controls and DevHarness 0 warnings, 0 errors; nothing opened on screen.

Not verified: how the lines look on screen at any DPI, frozen columns with horizontal scroll (lines of scrolled cells are clipped under the frozen band like the cells), group bands and the footer (untouched, still use their own separators). Help: no change (no window uses it yet).

### Files touched

- `src/KBot.Controls/DataView/KBotDataColumn.vb` — the eight properties, with `ShouldSerialize*` / `Reset*` for Image, Color, Padding, Size and Font.
- `src/KBot.Controls/DataView/KBotDataView.CellButton.vb` — NEW partial: face geometry, drawing, hit-test, measuring, caption rules, border drawing. Registered in `KBot.Controls.vbproj` (`DependentUpon`).
- `src/KBot.Controls/DataView/KBotBorderSides.vb` — NEW flags enum (pass 2).
- `src/KBot.Controls/DataView/KBotDataView.Painting.vb` — `DrawCell` calls the new drawing; old `DrawButtonCell` removed.
- `src/KBot.Controls/DataView/KBotDataView.Theming.vb` — `CellFontFor` honours `ButtonFont`.
- `src/KBot.Controls/DataView/KBotDataView.AutoSize.vb` — button width need, caption fallback.
- `src/KBot.Controls/DataView/KBotDataView.Input.vb` — mouse-up acts only on the face.
- `src/KBot.Controls/DataView/KBotDataView.md` — new properties, click rule.
- `src/KBot.Controls/KBot.Controls.vbproj` — FileVersion 1.61.0.0 -> 1.62.0.0.

### Test results

- `dotnet build src\KBot.Controls\KBot.Controls.vbproj` — 0 warnings, 0 errors.
- No tests written or run (standing rule), nothing rendered, nothing opened in the designer.

### Left unverified or deferred

- Not seen on screen at any DPI; the layout maths is unchecked against a real render.
- Not checked in the Visual Studio property grid / designer serialization (a freshly dropped column should write zero `Button*` lines — the `ShouldSerialize*` pairs are there for it).
- Existing Button columns (HelpCaptureForm, RobotQueueForm, ClasificatiiForm, ParteneriForm, harness forms) were not touched; with the defaults they should paint as before, except the click now ignoring the 4/3 px margin. Not verified on screen.
- No hover / pressed look and no hand cursor over the button (never existed; not requested).
- Help: no change — no existing window uses the new properties, so the operator sees nothing new.
