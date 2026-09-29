# KBotComboBox

Themed drop-down, written from scratch on `Control` (slice 0094). The face is painted by us
(rounded rect, 1 px outline, GDI+ arrow), the typed text lives in a borderless `TextBox` child
placed exactly where the painted caption would be, and there is ONE list window of our own,
opened by the arrow (every item) or by typing (the matching items).

`Combo/KBotComboBox.vb` + `KBotComboBox.DropDown.vb` · `Control` · Toolbox · `IThemedControl`
Helpers: `KBotComboItemCollection` (items), `KBotComboList` (the list window), `KBotInputMask`.
Conventions: [C1..C9](../CONTROLS.md). Status: slice 0094, built, not seen on screen.

## Why it no longer inherits `ComboBox`
Until slice 0094 it did, to keep `DataSource` / `DisplayMember` binding. Every native piece had
to be fought: the EDIT child was placed by Windows (measured offsets, re-aligned after three
window messages), the height was fixed by the font, the list was a native window next to our own
find list, and the control could not be centred inside a grid cell like a text box. Nobody binds
data to it, so binding went with it. Items are added to `Items` and shown by `CaptionSelector`
or `ToString()`.

## API
- `Items: KBotComboItemCollection` — `Add` (returns the index), `AddRange(IEnumerable)`,
  `Insert`, `Remove`, `RemoveAt`, `Clear`, `IndexOf`, `Contains`, indexer, `Count`. Nothing is
  refused (`ArgumentNullException`). The selection follows its item through inserts/removals.
- `SelectedIndex` (-1 = none; out of range THROWS), `SelectedItem` (an item not in the list
  THROWS), `Text`, `SelectedIndexChanged`.
- `CaptionSelector: Func(Of Object, String)` — what an item shows; Nothing = `ToString()`.
  Replaces `DisplayMember` (LoginForm: `Function(o) DirectCast(o, UnitInfo).Display`).
- `BeginUpdate()` / `EndUpdate()`, `FindStringExact(s)` (case ignored).
- `DroppedDown` (get/set), `DropDown` / `DropDownClosed` events,
  `MaxDropDownItems: Integer = 8`, `DropDownWidth: Integer = 0` (logical px, 0 = the box width).
- `HoverColor`, `BorderColor`, `ArrowColor`, `SelectionBackColor`, `SelectionForeColor` —
  Empty = theme (C1), each with a `Effective*` read-only counterpart = what is painted.
- `CornerRadius: Integer = -1` — -1 = the scheme's radius, 0 = square (logical px).
- `BackColor` / `ForeColor` / `Font` — overridden with ShouldSerialize/Reset (C4). `Text` is never
  serialized.
- `Editable: Boolean = False`, `LimitToList: Boolean = True`, `CommitText()` — see below.
- `TextAlign: HorizontalAlignment = Left` — typed and painted text.
- `TextOffsetY: Integer = 0` — optical nudge of the text box, logical px.
- `FindAsYouType`, `FindAfterNChars`, `FindFirstGroupCount`, `InputMask`, `UnmaskedText`,
  `FindMatches(...)` (Shared, pure) — slice 0082, see below.
- `OfferNewItem`, `OfferNewItemText`, `NewItemRequested` — slice 0083, see below.
- `SelectionStart`, `SelectionLength`, `TextLength`, `SelectAll()` — the text box's.
- `ItemHeight` (read-only: font height + 6 logical px), `PreferredHeight` (= `ItemHeight + 6`,
  the height a native combo had), `EditBounds` (the text box's rectangle, for the DevHarness).
- `ApplyTheme(scheme)`.

## Height
Outside a grid the height follows the font, like the native control (`SetBoundsCore` clamps it
to `PreferredHeight`), so the forms laid out around the old control keep their measurements. The
grid's editor (cell editor mode) has a free height.

## Text placement
The text box is borderless, as wide as the text area (8 logical px from the left edge up to the
arrow), one line high, and vertically centred the way `TextFormatFlags.VerticalCenter` centres
(spare height rounded up). Its own margins are replaced by the glyph padding `TextRenderer` puts
around a line (measured on the control's device context). A non-editable or disabled combo paints
its caption with `TextRenderer` in the same rectangle, so typed and painted text start on the same
pixel. Same recipe as the grid's text editor (slice 0085).

## Typing: `Editable` + `LimitToList`
`Editable = True` shows the text box; off, the caption is painted, the control itself takes the
focus, Up/Down/Home/End/PageUp/PageDown move the selection and a typed letter jumps to the next
item starting with it. Disabled, the text box hides and the caption is painted in the disabled
colour.

`LimitToList` decides what happens to text that is not in the list:

| | text matches an item | text matches nothing |
|---|---|---|
| `LimitToList = True` (default) | `SelectedIndex` moves to it, spelling taken from the list | the field goes back to the last accepted value |
| `LimitToList = False` | same | the text is **kept** and `SelectedIndex` becomes -1 |

The verdict is given when the field is left (before the host's `Leave` handlers run), on Enter
(before the host's `KeyDown` handlers run), and whenever a host calls `CommitText()`. Typing does
not move `SelectedIndex`; only the verdict does.

## The list
One window, `KBotComboList`, under the box (above it when there is no room below). It **never
takes the focus** (`WS_EX_NOACTIVATE`, `MA_NOACTIVATE`): the caret stays in the box and the box
drives the list — Up/Down/PageUp/PageDown move the highlight, Enter or a click takes the row
(`SelectedIndexChanged` fires), Escape closes it. While it is open, Enter/Escape are input keys, so
a form's AcceptButton / CancelButton do not take them. It scrolls with the wheel (also over the
box), a draggable thumb, or a click on the track.

- **The arrow** (or F4 / Alt+Down, or a click anywhere on a non-editable face) opens it with every
  item, the selected one highlighted and scrolled to the middle.
- **Typing** with `FindAsYouType`, once `FindAfterNChars` characters are typed, re-fills it with
  the rows whose caption matches: rows that **start** with the text first, then rows that only
  **contain** it; case and diacritics ignored. `FindFirstGroupCount` keeps the first N items as a
  group ahead of the rest. With too little typed, a list opened by the arrow shows everything again
  (first item starting with the text highlighted); a search result closes.
- It closes on a click anywhere outside it and outside the box (an application message filter
  watches the mouse presses while it is open), when the box is left, hidden or disabled, when the
  items change, and when the host form moves, resizes or is deactivated. It follows the box when
  the box moves.

With `FindAsYouType` on, `CommitText` also accepts a text that is the **start of exactly one** row
(a full classification code picks its «code — name» row).

## «New item» row (slice 0083)
`OfferNewItem = True` with `LimitToList = True`: when the list has nothing to show (the typed text
matches no row, or the combo has no items and the arrow is clicked), ONE italic row
`OfferNewItemText` is shown. A click or Enter on it raises `NewItemRequested` (`e.Text` = the typed
text); if the handler adds an item whose caption is exactly that text, the combo selects it.

## Input mask (slice 0082)
Needs `Editable = True`. One character per position: `0` digit, `L` letter, `A` letter or digit,
`&` any non-space character, `\x` the literal `x`; anything else is a literal written by the mask.
Classification: `00.00.00.00.00.00.00` — the operator types `65020402200101`, the box shows
`65.02.04.02.20.01.01`. Backspace/Delete take one typed character (and the literal in front of
it); Ctrl+V / Shift+Insert keep only what fits the slots; any other edit (Ctrl+X, the context-menu
paste) is re-shaped after the fact. The engine is `KBotInputMask` (pure).

## Inside KBotDataView (slice 0094)
The grid's combo editor is this control in **cell editor mode** (`Friend CellEditorMode`): no
frame, no arrow, free height, text flush left. The grid places it one line high over the cell's
text rectangle (the content minus the chevron strip), exactly like its text editor, keeps painting
the chevron itself, and passes the cell as the list's anchor (`Friend DropDownAnchorProvider`) so
the list lines up with the cell and a click on the chevron toggles it.

## Limits
- The hover wash moves to the outline while the text box shows (it repaints its own rectangle).
- The text selection highlight inside the text box is the system's.
- No data binding, no multi-column list, no per-item icons, no checkbox items, no auto-complete.
- The mask does not demand a COMPLETE value; `KBotInputMask.IsComplete` is there for a host that
  needs it.
- The `NativeMethods` combo-EDIT helpers in KBot.Theming (`GetComboEditBounds`,
  `SetComboEditMargins`, `SetComboEditBounds`, `GetComboEditTextTop`, `GetComboEditLineHeight`,
  `ApplyControlColors`) are no longer called by this control.
