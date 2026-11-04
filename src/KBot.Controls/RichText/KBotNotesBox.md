# KBotNotesBox

Read-only rich text surface for a short plain text (release notes, descriptions): word wrap, Segoe UI, our own `KBotScrollBar`s. The text sibling of `KBotLogBox`.

`RichText/KBotNotesBox.vb` - `KBotRichScrollBox` (a `RichTextBox`) - Toolbox - `IThemedControl` Conventions: [C1..C9](../CONTROLS.md). Status: not recorded (never seen on screen).

## API
`ShowText(text)` replaces the text and scrolls to the top. `BackColor`/`ForeColor`/`Font` follow the scheme until the operator pins one (C4). Takes the focus when the mouse enters it (so the wheel scrolls it), like `KBotLogBox`.
