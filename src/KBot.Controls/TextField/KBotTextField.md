# KBotTextField

Single-line form field: a borderless `TextBox` inside a frame that paints a 1 px rounded
outline (accent while focused) with inner padding. For passwords it also draws a GDI+ reveal
eye on the right. This is the LoginForm field.

`TextField/KBotTextField.vb` · `Control` · sealed · **not** in the Toolbox · `IThemedControl`
Conventions: [C1..C9](../CONTROLS.md). Status: not recorded.

## API
- `Text`, `PlaceholderText`, `MaxLength`, `UseSystemPasswordChar`
- `InnerTextBox: TextBox` (read-only) — the real edit control.
- `FocusInput()`, `GetPreferredSize(...)`, `ApplyTheme(scheme)`
- `FieldKeyDown As KeyEventHandler` — the inner box's `KeyDown`, re-raised on the frame.

## Behaviour
- The frame is NOT selectable: Tab lands straight on the inner `TextBox`. Because the frame
  never gets focus, a `KeyDown` on it would never fire — hence `FieldKeyDown`.
- `UseSystemPasswordChar = True` adds the reveal eye; clicking it toggles plain text.

## Limits
- Single line only, no scrolling. For a multiline box with adjustable border and drawn
  scrollbars use [KBotTextBox](KBotTextBox.md).
- Border colour/width and corner radius are not settable here — they come from the theme.
- Kept out of the Toolbox (`ToolboxItem(False)`); it is a form-specific field, not a
  general-purpose control.
