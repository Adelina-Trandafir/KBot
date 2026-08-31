# KBotLabel

A `Label` that draws its OWN border — colour, width and corner radius chosen by the
operator.

`Label/KBotLabel.vb` · `Label` · sealed · Toolbox · `IThemedControl`
Conventions: [C1..C9](../CONTROLS.md). Status: covered by `KBotLabelTests`.

## API
- `BorderColor: Color` — Empty = `BorderColor` from the theme (C1).
- `BorderWidth: Integer = 1` (logical px; 0 = no border)
- `CornerRadius: Integer = 0` (logical px; 0 = square)
- `BackColor` / `ForeColor` / `Font` — overridden with ShouldSerialize/Reset pairs (C4).
- `GetPreferredSize` — the base label size plus the border on both sides, so text does not
  touch the line at `AutoSize = True`.
- `ApplyTheme(scheme)`

## Behaviour
- Inherits `Label`, not `Control`: `AutoSize`, `TextAlign`, `UseMnemonic` and text
  measurement are already solved there.
- With a corner radius the background is painted here (the base's rectangular fill would
  show square corners under the rounded border); without one the base paints it.

## Limits
- The inherited `BorderStyle` is hidden (`Browsable(False)`) and REFUSED — it is pinned to
  `None`. The three native values are drawn by Windows in SYSTEM colours and their width
  cannot be touched. Use `BorderColor` + `BorderWidth`.
- No background gradient, no shadow, no per-side border widths.
