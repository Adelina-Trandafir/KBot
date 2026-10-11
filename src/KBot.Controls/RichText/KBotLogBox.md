# KBotLogBox

Read-only rich text surface for logs (slice ME01): per line the time in the accent colour, a short
level tag, the source dim, then the message. Errors use the theme's error colour (dark red in
Classic), warnings the warning colour, a detail (traceback) hangs dim and indented under its line.

`RichText/KBotLogBox.vb` (+ `KBotLogLine`, `KBotLogLevel`) - `RichTextBox` - Toolbox - `IThemedControl`
Conventions: [C1..C9](../CONTROLS.md). Status: not recorded (never seen on screen).

## API
`SetLines(lines, [stickToBottom = True])` replaces the content in ONE rtf assignment (thousands of
lines are fine); `ClearLines()`; `IsAtBottom()`; `ToPlainText()`; `LogLines`.
`KBotLogLine`: `Time` (formatted by the caller), `Level`, `Source`, `Text`, `Detail`.

Theme: `BackColor`/`ForeColor`/`Font` follow the scheme until the operator pins one (C4); a scheme
change re-renders the lines with the new palette.

## Limits
The scroll bars are the native ones. Selection and copy are the RichTextBox's own.

## Mouse wheel

The box is read-only and never clicked into, so it **takes the focus when the mouse enters it** (only while its form is the active one): the wheel goes to the focused control, and without this it scrolled the button next to it.
