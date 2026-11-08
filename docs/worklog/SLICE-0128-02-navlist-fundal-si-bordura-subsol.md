# SLICE-0128-02 - KBotNavList: pinned BackColor and footer border (08.11.2026)

Operator, 08.11.2026: `BackColor = Window` set on a `KBotNavList` in the designer did not survive to run time, and the bar needs a footer border with on/off, thickness and colour editable in the designer.

## Final state
**Back colour.** `ApplyTheme` no longer overwrites a `BackColor` the operator set. `KBotNavList` now follows the house convention: `BackColor` is overridden with its own «pinned» flag (`ShouldSerializeBackColor` / `ResetBackColor`), the theme assigns through `MyBase.BackColor` so it never marks the colour as pinned, and a freshly dropped bar writes no `BackColor` line. Not pinned = the theme's `SurfaceColor`, as before. Under a DARK scheme the theme wins over a pinned colour (same rule as the `KBotDataView` bands); the pinned value is kept and comes back when the scheme is light again.

**Footer border** (category `K-BOT: Footer`; a solid band on the bottom edge of the bar):
**A VERTICAL bar never has the footer border and reserves no space for it, whatever the three properties say (operator, 08.11.2026); only a horizontal bar draws it.**
- `FooterBorderVisible` (default True) - on/off. Off = no line and no space reserved.
- `FooterBorderWidth` (default 2, logical px at 96 dpi, scaled with DPI; 0 = no line; negatives clamp to 0).
- `FooterBorderColor` (default `Color.Empty` = from the theme, with `ShouldSerialize` / `Reset`). Empty resolves like the footer line of `KBotDataView` / `AdvancedTreeControl`: Classic = `Gainsboro`, Modern = the scheme accent, any other scheme = `SystemColors.InactiveBorder`. A colour set in the designer wins in every scheme.

The band space is reserved in the layout (`RecalcLayout` adds it to the bottom padding; the collapse button of a bottom corner sits above it), so no button is drawn over it.

## Files touched
`src/KBot.Controls/NavList/KBotNavList.vb` (fields, `BackColor` override, three footer properties, `FooterBorderPx`, `ApplyTheme`, `RecalcLayout`, `CollapseButtonRect`, `OnPaint`).

## Test results
`KBot.Controls` builds clean (0 warnings, 0 errors). No tests written or run (standing rule). Seen on screen by the operator, 08.11.2026 (OK).

## Unverified / deferred
- Only what the operator looked at is confirmed; not specifically checked: Dark, a bar with the corner button at the bottom, a horizontal bar.
- The border is ON by default, so every existing HORIZONTAL `KBotNavList` gains a 2 px line and its buttons shift up by 2 px (vertical bars: none). Not checked against the bars in the app.
- Bars whose designer already sets `BackColor` now keep that colour (before it was always replaced by the theme): not swept.
- No help change: the operator sees only a thin line on the bar; the operator decided there is nothing to write (08.11.2026).
