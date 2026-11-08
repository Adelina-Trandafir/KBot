# SLICE-0129-03 - Every container takes the same background, PanelBack() (08.11.2026)

Request (operator, 08.11.2026): the gap between the vertical nav list and the tree, between the main tree and the Istoric tree and the other trees had no visible separation, because the controls that HOST other controls were painted by different sources (Control / Window / Surface / SurfaceAlt). Operator: all container backgrounds must be the same everywhere, whether the scheme slot is Surface or SurfaceAlt. `KBotNavList` is left alone.

## What changed and why
One rule, `ThemeScheme.PanelBack()` (Classic = Window, Dark = SurfaceAlt, any other scheme = Surface), now paints every control that hosts other controls:
- `ThemeManager` (`StylePalette` + `StyleSystem`): `SplitContainer` and its two panels, `Panel`, `Tag = "Card"` panels, plain `TableLayoutPanel`, `GroupBox`, `TabPage` (palette path). Classic: `SplitContainer`, Card, `TableLayoutPanel`, `GroupBox` go from Control / White to Window. The `Form` itself is NOT changed (stays Control, as 0129).
- `KBotTableLayoutPanel.ApplyTheme`: Card or not, `PanelBack()`.
- Per-view `ApplyTheme` / `OnThemeChanged` overrides in `KBot.App` and `KBot.EFactura` (about 280 lines, 100+ files): the view / page / form self (`BackColor = ...`), `split` + `Panel1/Panel2`, `tly*` / `tbl*` / `pnl*`, the container loops (`For Each k_c As Control In {...}`) and the empty-state / status labels that sit on those containers now use `scheme.PanelBack()` instead of `Palette.SurfaceAltColor` / `SurfaceColor`.
- Not touched: `KBotNavList`; inputs (text boxes, trees, list boxes, `PropertyGrid`, picture boxes); `KBotChatBubble`, popups, `KBotFilterPopup*`, ribbon, side panel, chart / graph / lane (they paint themselves); `KBot.Tray`; DevHarness / HarnessTests; explicit `KBotBackgroundRoles` pins; forms whose back is the 1px outline (`BackColor = p.BorderColor`).

## Files touched
`KBot.Theming/ThemeManager.vb`, `KBot.Controls/Table/KBotTableLayoutPanel.vb`, and the view / page / form `.vb` files in `KBot.App` and `KBot.EFactura` that carried container `SurfaceAlt` / `Surface` lines.

## Test results
`KBot.EFactura` and `KBot.App` build: 0 warnings, 0 errors. No tests written or run. **Seen on screen and approved by the operator, 08.11.2026** (the nav-to-tree, tree-to-tree and tree-to-grid gaps in KBotForm now read correctly).

## Unverified / deferred
Nothing open. Kept out on purpose: `KBotNavList`, inputs, self-painting controls, `KBot.Tray`, the harnesses, the `Form` itself (Control), explicit `KBotBackgroundRoles` pins. No help text: colour-only change.
