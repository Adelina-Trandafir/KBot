# KBotScrollBar

Scrollbar drawn by us — the themed sibling of `VScrollBar`/`HScrollBar`. Native bars are
windows painted by Windows, so no palette colour reaches them (`SetWindowTheme
("DarkMode_Explorer")` only buys the system's dark grey, never the scheme accent).

`Scroll/KBotScrollBar.vb` · `Control` · sealed · Toolbox · `IThemedControl`
Conventions: [C1..C9](../CONTROLS.md). Status: covered by `KBotScrollBarTests`.

## Value semantics — identical to `System.Windows.Forms.ScrollBar`
so it can replace a native bar without the host recomputing anything:
- reachable range = `Minimum .. Maximum - LargeChange + 1` (= `MaxValue`)
- thumb length = `LargeChange / (Maximum - Minimum + 1)` of the track

## API
- `Minimum = 0`, `Maximum = 100`, `Value = 0` (clamped, never throws), `SmallChange = 1`,
  `LargeChange = 10`
- `MaxValue`, `IsScrollable` (read-only)
- `SetRange(minim, maxim, pasMare, valoare)` — whole range + position in ONE invalidate.
- `Orientation = Vertical`, `ShowArrows = True`, `MinimumThumbLength = 18`,
  `ThumbCornerRadius = 3`, `ThumbPadding = 2` (logical px)
- `TrackColor` / `ThumbColor` / `ThumbHoverColor` / `ArrowColor` — Empty = theme (C1);
  hover defaults to the scheme accent.
- `TrackBounds`, `ThumbBounds` (read-only; `Rectangle.Empty` when there is nothing to scroll)
- `MutaCursorLa(pozitie)`, `Pas(ScrollEventType)` — public so tests can drive a drag/step
  without a real window.
- Events: `Scroll As ScrollEventHandler` (native signature), `ValueChanged` (any cause,
  including programmatic).
- `Const GrosimeImplicita = 12` — default thickness (logical px) the host should reserve;
  the default `Size` is `(12, 80)`, handed out via `DefaultSize` so the designer does not
  print a `Size` line into every host (C4).

## Limits
- NOT selectable: it sits next to an edit field and a Tab landing on it would surprise.
  Mouse wheel and keyboard stay with the host.
- No auto-repeat page scrolling on hold, no proportional-track click animation.
- Does not scroll anything by itself — it reports a value; the host moves the content
  (see [KBotTextBox](../TextField/KBotTextBox.md) for the wiring).
