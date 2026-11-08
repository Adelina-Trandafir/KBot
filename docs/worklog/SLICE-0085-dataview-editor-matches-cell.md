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

- styled through ResolveCellLook, matching DrawCell: normal/alternating/selected row colours → RowFormatting → CellFormatting, with fresh args; resolved font/background/foreground/horizontal alignment;
- placed in the cell's content rectangle (`CellContentRect`, i.e. the cell minus `CellPadding` scaled by `ScaleDpi`), full content width, one text line high;
- EM_SETMARGINS uses grid-device-context glyph padding: MeasureTextPadding compares TextRenderer.MeasureText with/without NoPadding, left = ceil(h/6), remainder right. Device pixels already follow DPI, without extra scaling;
- vertically centred like `TextFormatFlags.VerticalCenter`, rounding the spare height up (GDI does; plain integer division left the editor 1 px high — measured, see test results).

DrawCell skips IsEditingCell text, preventing the old value around the caret. Combo keeps the full cell (native height follows font), adopting only its font/colours.

**Single click:** OnMouseDown selects/opens editable cells and PlaceCaretAt follows the click. HandleMouseDownOnEditingCell, before grid focus, keeps the edit/moves caret for padding clicks. F2/double-click still open, without reopening the same active cell. DdfEditSectiuneaAPage Clsf and AlegereUnitateForm use readonly columns: no editor on single click, existing CellDoubleClick handlers still receive double click.

**Unchanged commit:** CommitEdit closes without validation, IsDirty or CellValueChanged when EditorUnchanged finds the original text. This prevents single-click/arrow traversal alone from marking untouched cells edited.

**Up/Down:** Friend MoveEditVertical/NextEditableRow find the nearest editable cell in the same column, DRAWN order, skipping disabled/noneditable rows/group bands. Collapsed rows have no band; footer is outside bands. Boundary/single row keeps editor/caret without commit/move; previously a disabled target dropped edit to grid.

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
2. **Unselected edit colours:** ResolveCellLook now uses normal/alternating → RowFormatting/CellFormatting regardless of selection; painter fills the whole edited cell/padding with `_editBackColor`, distinct from selected row. Columns have no colour, so column default means this formatted unselected look.
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

ButtonSize was added beyond the request: alignment needs a movable face, not one filling the cell. At 0,0 it remains the former full-cell button.

Behaviour changes worth knowing:

- **A click acts only on the face.** Before, the whole cell (including the 4/3 px margin) fired `ButtonClick`. With a smaller button, clicking empty cell space must not fire it. Space on the current cell still fires. `CellClick` still fires for any click.
- **`Transparent` also drops the border** (until pass 2: unless `ButtonBorderColor` is set).
- Picture/caption form one centred group in the padded face. Picture keeps DPI-scaled pixel size, shrinking proportionally to fit; if wider than the face, left picture/ellipsized caption. Disabled picture opacity: 40%.
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
- **Default fast path**, agreed 03.10.2026 after /btw on cost: ONE DrawLine under each row. Cells with no Bottom or a custom column/CellFormatting bottom colour report spans in _rowGaps; DrawRowBottomLine skips them and each cell draws its own bottom. Defaults draw right sides only: one line/cell + one/row, no allocation. Scroll spans clamp outside the frozen band. This replaces the initial bottom line per cell.
- Non-defaults: up to four lines/cell, no per-cell allocation. Theme uses cached grid-line pen; custom colours share one last-colour pen, rebuilt only on colour change and disposed with the other pens.
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

---

## SLICE-0085-04 — KBotDataView: one header/footer look in every grid <a id="slice-0085-04"></a>

Operator request, 07.11.2026: over ALL DGVs of the solution (KBot.App and KBot.EFactura) the same header/footer height (30 px), the same header/footer font (Calibri 9pt Bold), separator colours tied to the theme (Classic = Gainsboro, Modern = default) and band back colours tied to the theme (Classic = Control, Modern = default); and this must become a RULE for every future DGV.

Slice number: **assumed** 0085-04 — 0085 is the latest slice dedicated to the standalone grid; 0085-01 is the base 0085 pass, 0085-02 and 0085-03 are taken (03 only in code comments / pass 4 above).

### What changed and why

- `KBotDataView.Theming.vb`: `ApplyTheme` — for the scheme named Classic the header/footer back is `SystemColors.Control` and the lines under the header / above the footer are `Color.Gainsboro`; every other scheme keeps the derived values (Modern = the blue-ish wash). `SetDefaultColors` (pre-theme / design time) matches Classic. `BuildBandFont` returns Calibri 9pt Bold (scaled by the text-size factor) in every scheme; the old scheme-font logic stays only as the safety net when Calibri is missing.
- Height: `HeaderHeight` already defaulted to 30 and `FooterHeight` follows it — nothing to change in the control; the overrides were the problem.
- Designers: a node script removed 294 lines from 119 `.Designer.vb` files (App, EFactura, Migrator, AVC.Migrator) that set, on a `KBotDataView`, `HeaderHeight`, `FooterHeight`, `HeaderFont`, `FooterFont`, `HeaderBackColor`, `FooterBackColor`, `HeaderSeparatorColor`, `FooterSeparatorColor`, and the `HeaderFont` of a `KBotDataColumn`. Trees (AdvancedTreeControl, same property names) and the Controls / DevHarness folders were not touched.
- The rule is written in `CLAUDE.md` (House rules) and in memory, so a new grid sets none of these.

### Files touched

- `src/KBot.Controls/DataView/KBotDataView.Theming.vb`
- 119 `.Designer.vb` files (list: re-run `git status`; many are untracked/new so `git diff` does not show them)
- `CLAUDE.md`

### Test results

Build `KBot.sln`: 0 errors, 0 warnings. No tests run (not asked).

### Unverified / deferred

- Not seen on screen: Classic vs Modern look of the bands, 30 px in grids that were 24/26/40, `XfaXmlPreview` header now Calibri instead of Consolas.
- Dark and Colorful keep their derived colours (the request named only Classic and Modern).
- `AutoSizeHeaderHeight` is unchanged: a header with multi-line titles still grows above 30 px.
- The properties still exist on the control; the rule is enforced by convention, not by code.
- Help: no section was edited; grid screenshots may differ slightly (see Open threads «Ajutor de actualizat»).

### Pass 2 (operator follow-up, 07.11.2026): AdvancedTreeControl gets the same bands

The tree's header and footer bands must look exactly like the DGV's: font, colours, height, separators.

- `AdvancedTreeControl.Properties.vb`: `HeaderHeight` 32 -> 30 and `FooterHeight` 28 -> 30 (`BAND_HEIGHT`); `HeaderSeparatorWidth` / `FooterSeparatorWidth` default 1 -> 2 (`BAND_SEPARATOR_WIDTH`, same as the DGV); `HeaderFont` / `FooterCaptionFont` unset = Calibri 9pt Bold (shared base font, scaled by `AppScaling.ScaledFont`) instead of the tree font; `HeaderSeparatorColor` / `FooterSeparatorColor` unset = theme colour (before: the text colour at alpha 60).
- `AdvancedTreeControl.Theming.vb`: Classic = Control back + Gainsboro lines; other schemes = DGV defaults (header back = surface-alt, footer back = surface-alt washed 10% toward the accent, both lines = accent). Pre-theme fields match Classic.
- Designers: 162 lines removed from the `AdvancedTreeControl` designers (HeaderHeight, FooterHeight, HeaderFont, FooterCaptionFont, HeaderBackColor, FooterBackColor, HeaderSeparatorColor, FooterSeparatorColor, HeaderSeparatorWidth, FooterSeparatorWidth). Only the header and footer BANDS; the TreeListView column-title band (24 px, `ColumnHeader*`) and the search bar were not touched.
- Build `KBot.sln`: 0 errors, 0 warnings. Not run, not seen on screen. Dark/Colorful take the same non-Classic colours.
- CLAUDE.md rule extended to trees.

### Pass 3 (operator follow-up, 07.11.2026): the tree caption is part of its icon's button

Operator request: the text in the tree footer must have a transparent back colour (a light box showed behind «Actualizeaza»), and the text tied to a button must be part of that button -- clickable like the icon and lit by the same hover colour -- in the footer and in the header.

- Transparent: the three designers that pinned `FooterCaptionBackColor = Control` (KbotForm, the former reception window, VerconForm) lost that line. The property stays (empty = none); CLAUDE.md says a tree never sets it.
- Footer (`AdvancedTreeControl.Footer.vb`): the caption belongs to the right icon (to the left one when only it exists, or when both exist and the text is left-aligned). Button area = icon + caption rectangle (`FooterRightButtonRect` / `FooterLeftButtonRect`), used for hover and click; the hover plate is drawn under both. `DrawFooter` now computes icon and caption rectangles first, then paints plates, icons, text.
- Header (`AdvancedTreeControl.Header.vb`): a caption is a label only when it is aligned to the RIGHT, next to the icons (nearest icon: search, else right icon); left/centre captions stay titles and are not clickable. Same union rectangles (`HeaderSearchButtonRect` / `HeaderRightButtonRect`) for hover and click.
- Removed the `FooterCaptionClicked` event (it fired for any band click outside the icons) and its two host handlers (`KbotForm.Download.vb`, `PrimiteView.vb`): they did the same as `FooterRightIconClicked`, which now also fires on the caption, so keeping both would double-fire.
- Build `KBot.sln`: 0 errors, 0 warnings. Not run, not seen on screen; the caption rectangle comes from the last paint, so hit testing before the first paint finds no label.
