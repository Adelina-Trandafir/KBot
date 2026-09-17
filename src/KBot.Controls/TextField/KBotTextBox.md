# KBotTextBox

The general text box: a borderless `TextBox` inside a frame that paints the outline (colour
AND width chosen by the operator) and, when multiline, TWO
[KBotScrollBar](../Scroll/KBotScrollBar.md)s — so the bars are themed too, dark scheme
included.

`TextField/KBotTextBox.vb` · `Control` · sealed · Toolbox · `IThemedControl`
Conventions: [C1..C9](../CONTROLS.md). Status: covered by `KBotTextBoxTests`.

Difference from [KBotTextField](KBotTextField.md): that one is the LoginForm field (one
line, fixed 1 px outline, password eye, no scrolling).

## API
Text: `Text`, `Lines`, `Multiline = True`, `WordWrap = True`, `ReadOnly = False`,
`MaxLength = 32767`, `PlaceholderText`, `TextAlign`, `UseSystemPasswordChar = False`,
`PasswordChar`, `AcceptsReturn`, `AcceptsTab`, `CharacterCasing`, `HideSelection`,
`ShortcutsEnabled` (all forwarded to the inner box, all in the F4 grid under «K-BOT»),
`AppendText(text)`, `FocusInput()`, `InnerTextBox`.

Scrolling: `ScrollBars = Vertical`, `AutoHideScrollBars = True`,
`ScrollBarThickness = KBotScrollBar.GrosimeImplicita`, `VerticalScrollBar` /
`HorizontalScrollBar` (the real `KBotScrollBar` instances), `SincronizeazaBare()`,
`ViewChanged` event.

Frame: `BorderColor`, `FocusBorderColor` (Empty = accent), `BorderWidth = 1`,
`FocusBorderWidth = 1`, `CornerRadius = 4`, `ContentBounds`, `GetPreferredSize`.

Paddings (all `Padding`, all logical px, all with ShouldSerialize/Reset — C4):
- `TextPadding = (6,6,6,6)` — outline → text, per side. Where a bar shows, the text stops at
  the bar's band on that side instead.
- `VerticalScrollBarPadding = (0,6,6,6)` — `Left` faces the text, `Right` the outline,
  `Top`/`Bottom` inset the bar from the outline's ends.
- `HorizontalScrollBarPadding = (6,0,6,6)` — `Top` faces the text, `Bottom` the outline,
  `Left`/`Right` inset the bar from the outline's sides.
- Where both bars show, each stops at the other's band; the corner stays empty.
- Inherited `Control.Padding` does nothing here and is hidden from the grid (as on
  `TextBoxBase`), so it cannot be mistaken for `TextPadding`.

The defaults reproduce the single-number geometry the control had before (6 px air, bars
inset 6 px from the outline and glued to the text).

Theme: `BackColor` / `ForeColor` / `Font` overridden with ShouldSerialize/Reset (C4),
`ApplyTheme(scheme)`. Also `FieldKeyDown` (the inner box's `KeyDown`, re-raised).

## How scrolling works without native bars
The inner `TextBox` keeps `ScrollBars = None`, so Windows draws no band. Position is read
and moved through edit-control messages: `EM_GETLINECOUNT` / `EM_GETFIRSTVISIBLELINE` /
`EM_LINESCROLL` vertically; horizontally the TRUE offset comes from `EM_POSFROMCHAR` on the
first character of the visible line — it is not cached, because the caret also moves the
view and a cached field would go stale in silence.

## Limits
- `UseSystemPasswordChar` is single-line only.
- Plain text: no rich text, no syntax colouring, no per-run formatting, no undo API beyond
  what the inner `TextBox` gives.
- Scroll positioning depends on those edit-control messages, so it needs a real window
  handle — geometry cannot be asserted before the handle exists.
- The bars are ours, but the caret, selection and context menu are still the native ones.
