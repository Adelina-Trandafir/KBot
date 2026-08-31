# KBotNavList

Owner-drawn navigation bar: keyed buttons and separators, vertical (sidebar) or horizontal
(toolbar), with badges, a three-state collapse and a flyout label while collapsed.

`NavList/` — `KBotNavList.vb`, `KBotNavItem.vb` (+ collection), `KBotNavFlyout.vb`
`Control` · sealed · Toolbox · `IThemedControl`, `ISupportInitialize`
Conventions: [C1..C9](../CONTROLS.md).
Status: covered by `KBotNavList*Tests` (selection, shape, collapse, flyout).

## Enums
- `KBotNavOrientation` = `Vertical` | `Horizontal`
- `KBotNavAlign` = `Near` (top/left) | `Far` (bottom/right) — `Far` detaches a group.
- `KBotNavCorner` = `TopLeft` | `TopRight` | `BottomLeft` | `BottomRight`
- `KBotNavCollapseState` = `Expanded` → `Icons` → `Complete` → `Expanded` (cycled by the
  corner button; `Icons` is skipped when `IconsCollapseAvailable` is False).

## KBotNavItem
`Key` (non-empty, unique; ignored on separators), `Text`, `Image`, `Badge = 0` (0 = none),
`Enabled = True`, `Visible = True`, `AutoSize = False` (fit to text/icon, ignoring
`ItemWidth`), `IsSeparator = False`, `Align = Near`.

## KBotNavList
- `Items`, `AddItem(key, text[, align])`, `AddSeparator([align])`
- `SelectedKey` (unknown key → `ArgumentException`; a disabled/separator key is refused),
  `ClearSelection()`, `SelectionChanged(key)` event
- `SetBadge`, `SetItemEnabled`, `SetItemVisible`
- Layout: `Orientation = Vertical`, `IconSize = 20`, `ItemWidth = 0` (0 = auto),
  `ItemCornerRadius = -1`, `ItemGradient = 14`, `ItemPadding = 6,6,6,6` (logical px, C2).
  `Padding` is shadowed.
- Collapse: `Collapsible = False`, `CollapseCorner = TopRight`, `CollapseButtonSize = 18`
  (0 = no button, code-only), `CollapseExpandedImage` / `CollapseCollapsedImage`,
  `CollapseState`, `IconsCollapseAvailable`, `ToggleCollapse()`,
  `CollapseStateChanged(state)` event
- Flyout: `CollapsedFlyout = True`, `FlyoutDelay = 250` ms (0 = instant),
  `FlyoutSlideDuration = 120` ms (0 = no animation)
- `BeginInit`/`EndInit`, `ApplyTheme(scheme)`

## Behaviour
- `KBotNavFlyout` is a `WS_EX_NOACTIVATE` window that slides the full button out to the
  right while the bar is collapsed; the BAR computes its colours/fonts/scaled metrics
  (`KBotNavFlyoutStyle`) and the window only paints them — theme logic lives in one place.
  The flyout borrows fonts and images and never disposes them.

## Limits
- Keys validated at `EndInit` and on add: empty or duplicate → `ArgumentException` (C3).
  Skipped at design time (C6).
- `CollapseState` setter throws `InvalidOperationException` when the bar is not
  `Collapsible`, or on a state that is unavailable.
- Single-select only; no checkbox items, no sub-items/tree, no drag reordering.
- No per-item tooltips yet (known gap, together with `KBotCaptionBar` buttons).
- Badge is a number only.
