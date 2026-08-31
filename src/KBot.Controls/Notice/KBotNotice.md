# KBotNotice

Rounded notification box: background tinted with the state colour, a GDI+-drawn icon and
the message. Hidden by default.

`Notice/KBotNotice.vb` · `Control` · sealed · Toolbox · `IThemedControl`
Conventions: [C1..C9](../CONTROLS.md). Status: not recorded.

## API
- `NoticeKind` = `Error(0)` | `Warning(1)` | `Success(2)`
- `Show(message, kind)` — sets the text, makes it visible, measures and resizes itself.
- `Clear()` — hides it and empties the message.
- `Message: String` (read-only, `Browsable(False)`) — what is displayed now.
- `ApplyTheme(scheme)`

## Behaviour
- `Show` auto-sizes the control to the measured message.
- The three kinds pick `ErrorColor` / `WarningColor` / `SuccessColor` from the palette and
  a matching drawn glyph; no image assets involved.

## Limits
- Use `Message`, NOT `Visible`, to ask "is something shown?" — `Visible`'s getter walks up
  the parents, so on a form that was never shown it answers False right after `Show`.
- One message at a time; no queue, no stacking, no auto-dismiss timer, no close button.
- Plain text only — no rich-text markup, no per-kind icon override.
