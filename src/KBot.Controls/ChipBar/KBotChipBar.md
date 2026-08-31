# KBotChipBar

A MULTI-select bar of keyed chips (filter pills) with optional count badges. Rows wrap.

`ChipBar/` — `KBotChipBar.vb`, `KBotChip.vb`, `KBotChipCollection.vb`
`Control` · sealed · Toolbox · `IThemedControl`, `ISupportInitialize`
Conventions: [C1..C9](../CONTROLS.md). Status: covered by `KBotChipBarTests`.

## KBotChip
- `Key` — non-empty, unique; the handle used by every bar method.
- `Text`, `Checked = False`, `Count = 0` (0 = no badge)
- `Enabled = True` (faded, not checkable, still takes space)
- `Visible = True` (False = no space, no paint, skipped by the keyboard)
- `AccentOverride: Color = Empty` — checked background supplied by the caller from the
  palette (e.g. `ErrorColor`); Empty = the scheme accent. Re-supply it on theme change.

## KBotChipBar
- `Chips: KBotChipCollection` — designer-authorable, display order.
- `AddChip(key, text[, checked])`, `ContainsChip(key)`, `SetChecked(key, checked)`,
  `IsChecked(key)`, `SetChipEnabled`, `SetChipVisible`, `SetBadge(key, count)`,
  `CheckAll()`, `UncheckAll()`
- `CheckedKeys: IReadOnlyList(Of String)` (read-only), `PreferredBarHeight` (read-only —
  what the host should reserve after wrapping)
- Layout: `ChipHeight = 24`, `ChipPadding = 10`, `ChipSpacing = 6`,
  `ChipCornerRadius = -1` (-1 = scheme radius, 0 = square), `ChipGradient = 14` (0..100,
  0 = flat) — all logical px (C2).
- `MinimumRequiredChecked = 0` — 1 means the last checked chip cannot be turned off with
  the mouse; the bar FLASHES that chip instead (140 ms) rather than silently ignoring the
  click.
- Events: `CheckedChanged(chipKey)`, `ChipClicked(chipKey)`
- `BeginInit`/`EndInit`, `ApplyTheme(scheme)`

## Limits
- Multi-select by design. For single-select use the tab band of
  [KBotChartView](../Chart/KBotChartView.md) or [KBotNavList](../NavList/KBotNavList.md).
- Keys are validated at `EndInit` and on every add: empty or duplicate → `ArgumentException`
  (C3); unknown key on any `Set*`/`Is*` → `ArgumentException`. Validation is skipped at
  design time (C6).
- No icons on chips, no close/remove "x" per chip, no drag reordering, no per-chip tooltip.
- `Count` only renders a number badge; there is no text badge.
