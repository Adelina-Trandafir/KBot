# KBotProgressBar

Determinate progress bar (0..`Maximum`), themed. The sibling of
[KBotBusyBar](../BusyBar/KBotBusyBar.md), which is the indeterminate one. Replaces
`System.Windows.Forms.ProgressBar`, which Windows draws (so it stays green on a dark scheme).

`Progress/KBotProgressBar.vb` · `Control` · sealed · Toolbox · `IThemedControl`
Conventions: [C1..C9](../CONTROLS.md). Status: not recorded.

## API
- `Value: Integer = 0` — clamped to `0..Maximum`, never throws (C3).
- `Maximum: Integer = 100`
- `Fraction: Double` (read-only) — filled ratio 0..1, for probes and hosts.
- `CornerRadius: Integer = 3` (logical px; 0 = square)
- `ShowPercentText: Boolean = False` — writes the percentage over the bar.
- `BarColor` / `TrackColor` / `PercentTextColor` — Empty = `AccentColor` / `SurfaceAltColor` /
  `TextColor` from the theme (C1).
- `Font` (+ ShouldSerialize/Reset), `ApplyTheme(scheme)`

## Limits
- Horizontal only; no orientation, no right-to-left fill.
- No `Minimum` — the low end is always 0.
- No animation, no marquee, no segmented/blocks style, no custom text template
  (`ShowPercentText` prints the percentage and nothing else).
