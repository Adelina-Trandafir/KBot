# SLICE-0085 — KBotDataView: the editor looks like the cell, single-click editing, Up/Down through the column

Operator request, 26.09.2026, slice dedicated to the custom grid (`KBot.Controls/DataView`):

1. When a cell is edited, the text box must follow the cell's padding and size, have NO border,
   use the same font (family, size, style), and place and format the text like the cell, so the
   operator cannot tell a different control took over.
2. Clicking an editable cell starts editing immediately (not on double click).
3. Up/Down move the edit to the cell above/below in the same column when there is more than one
   row, ignoring the footer.
4. (Added mid-task) It must respect the DPI settings.

## What changed and why

**Editor placement (`PlaceEditor`, new).** The text editor is now `BorderStyle.None`,
`AutoSize = False` (Designer). On every open and on every layout pass (`UpdateLayout` → resize,
column width, theme change, DPI change via `ApplyMetricScale` → `LayoutChanged`) it is:

- dressed with the cell's *resolved* look: `ResolveCellLook` runs the same chain as the painter
  (normal / alternating / selected row colours → `RowFormatting` → `CellFormatting`) with fresh
  argument instances, so font, back colour, fore colour and horizontal alignment are exactly
  what `DrawCell` would use;
- placed in the cell's content rectangle (`CellContentRect`, i.e. the cell minus
  `CellPadding` scaled by `ScaleDpi`), full content width, one text line high;
- given edit-control margins (`EM_SETMARGINS`) equal to TextRenderer's glyph padding, measured
  on the grid's own device context (`MeasureTextPadding`: total from `MeasureText` with and
  without `NoPadding`, split left = ceil(h/6), rest right). Measured in device pixels, so it
  follows the DPI with no extra scaling;
- vertically centred like `TextFormatFlags.VerticalCenter`, rounding the spare height up (GDI
  does; plain integer division left the editor 1 px high — measured, see test results).

The painter (`DrawCell`) skips the text of the cell being edited (`IsEditingCell`), so the old
value never shows around the caret. The combo editor keeps the whole cell (a ComboBox fixes its
own height from its font) and only takes the cell's font and colours.

**Single-click editing.** `OnMouseDown`, after selecting the cell, opens the editor when the cell
is editable and puts the caret where the click landed (`PlaceCaretAt`). A click inside the cell
already being edited (its padding around the one-line editor) keeps the edit and only moves the
caret (`HandleMouseDownOnEditingCell`, checked before the grid takes focus). F2 and double click
still open the editor; double click no longer re-opens an editor already open on that cell. The
two hosts with `CellDoubleClick` handlers (`DdfEditSectiuneaAPage` «Clsf»,
`AlegereUnitateForm`) use read-only columns, so a single click never opens an editor there and
their double click still reaches the grid.

**Unchanged commit writes nothing.** With a single click opening the editor, most commits are
the operator just passing through. `CommitEdit` now closes without validating, without setting
`IsDirty` and without raising `CellValueChanged` when the editor still shows what it was opened
with (`EditorUnchanged`). Before this, even an arrow press through an untouched cell raised
`CellValueChanged` and marked the row edited.

**Up/Down.** `MoveEditVertical` + `NextEditableRow` (Friend): the edit goes to the nearest row,
in DRAWN order, whose cell in the same column is editable. Only data bands count — group
headers/footers are stepped over, rows in a collapsed group have no band, and the grid's footer
band is not a band at all. With nowhere to go (single row, first/last editable row) nothing
happens: nothing is committed, the editor stays with the caret where it was. Disabled /
non-editable rows in between are skipped (before, the edit landed on them and dropped to the
grid).

**Rule 0 sweep.** `KBotDataView.Editing.vb` (all comments, the two designer `Description`s and the
`ArgumentException` text) and `KBotDataView.Designer.vb` are now English/ASCII, plus the comments
of the `OnMouseDown` / `OnMouseDoubleClick` blocks touched in `KBotDataView.Input.vb`.

## Files touched

- `src/KBot.Controls/DataView/KBotDataView.Editing.vb` — placement, look resolution, padding
  measure, caret placement, unchanged-commit rule, `NextEditableRow` / `MoveEditVertical`; swept.
- `src/KBot.Controls/DataView/KBotDataView.Designer.vb` — `editText`: `AutoSize = False`,
  `BorderStyle.None`; swept.
- `src/KBot.Controls/DataView/KBotDataView.Input.vb` — single-click editing, click inside the
  edited cell, double click guard.
- `src/KBot.Controls/DataView/KBotDataView.Painting.vb` — skip the edited cell's text.
- `src/KBot.Controls/DataView/KBotDataView.Layout.vb` — `UpdateLayout` re-places the editor.
- `src/KBot.Controls/DataView/KBotDataView.md` — Up/Down wording + new section «The editor looks
  like the cell».

## Test results

- `dotnet build src\KBot.Controls\KBot.Controls.vbproj` — 0 warnings, 0 errors.
- `dotnet build src\KBot.App\KBot.App.vbproj` — 0 warnings, 0 errors.
- **Pixel check (scratch program, off-screen form, not in the repo).** For five columns — left
  Segoe UI 9, right-aligned numbers, centred Calibri 11 italic with padding 10/2/10/2, padding
  4/3/4/0, Tahoma 10 with padding 4/0/4/1 — the grid was drawn with `DrawToBitmap` before and
  after opening the editor on the selected cell (selection cleared), and the two images were
  compared pixel by pixel:
  - 144 dpi (this machine, PerMonitorV2): **0 differing pixels in all five cells.**
  - 96 dpi (same program run DPI-unaware): 0 differing pixels in four cells; the Tahoma cell
    matches in position and size (best-fit offset 0,0) but its ClearType colour fringes differ.
    Most likely a capture artifact of DPI-unaware mode — **not confirmed**.
- **Behaviour check (same scratch program, messages sent to the controls):** single click on
  an editable cell → editing, focus in the editor, caret at the click point, no selection;
  click in the edited cell's padding → edit kept, caret moved, 0 value events; Down from row 0
  with row 2 disabled → 1, 3, 4, stays on 4 (still editing); Up → 3, 1, 0, stays on 0; passing
  through untouched cells → 0 `CellValueChanged`.
- Unit tests: **not run and not written** (standing rule: no tests unless asked). Reading
  `KBotDataViewEditingTests` / `InputTests` / `FooterTests`, every commit there first types a new
  text, so the unchanged-commit rule should not affect them. `BeginEdit` now creates the editor's
  handle and a `Graphics` (margins + measuring), which the headless tests have never done — not
  verified.

## Left unverified or deferred

- Not seen in the real app on the operator's screen; the checks above are off-screen captures.
- The combo editor does not get the borderless/padding treatment (a native ComboBox draws its
  own frame and fixes its own height). It only takes the cell's font and colours.
- The rest of `KBotDataView.Input.vb`, `.Painting.vb`, `.Layout.vb` and the other partials still
  carry Romanian comments with diacritics (rule 0). Only the blocks touched here were swept.
- Hosts that relied on `CellValueChanged` firing for an unchanged commit (none found by search)
  would now get nothing.

## Pass 2 (operator follow-up, 26.09.2026)

1. **A click selects the whole text** (not a caret at the click point). `BeginEdit` already
   selects all; the click path no longer moves the caret, and a click in the padding of the cell
   already being edited also re-selects the whole text. `PlaceCaretAt` was removed.
2. **The edited cell does NOT take the selected-row colour.** `ResolveCellLook` now always
   resolves the UNSELECTED look (normal / alternating row colour, then `RowFormatting` /
   `CellFormatting`), and the painter fills the whole edited cell (padding included) with that
   colour (`_editBackColor`), so the cell being edited stands out from the selected row.
   Columns have no colour of their own, so "the column's default cell colour" = the unselected
   row colour after the formatting handlers.
3. **I-beam cursor** over every cell a click would edit (`CanEdit`); read-only / disabled cells
   keep the arrow. Set in `OnMouseMove` after the resize-edge / icon / group-band checks.

Pass 2 checks (same off-screen scratch program, 144 dpi): the edited cell's region is
pixel-identical to the same cell drawn on an UNSELECTED row (0 differing pixels, five cells);
its padding pixel = row colour (248,248,248) while the rest of the selected row is
(203,225,242); click → `SelectionStart = 0`, `SelectionLength = TextLength`; cursor = IBeam over
the editable column, Default over the read-only one. Builds: Controls and App 0 / 0.