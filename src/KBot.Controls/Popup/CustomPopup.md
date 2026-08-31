# CustomPopup

The K-BOT context menu: looks like a system menu (rows, icon rail, underlined access
letter, highlight that follows BOTH mouse and keys) but is drawn by us, so it takes the
scheme colours. `ContextMenuStrip` could not: its face is drawn by `ToolStripRenderer` and
stays a white strip on a dark scheme.

`Popup/` — `CustomPopup.vb` (+ `.Painting`, `.Input`, `.Slider`), `CustomPopupItem.vb`
(+ collection + `EventArgs`), `PopupMnemonic.vb`, `IPopupAnchor.vb`
`Form` · `IThemedControl` · not a Toolbox control (`DesignerCategory("Code")`)
Conventions: [C1..C9](../CONTROLS.md). Status: covered by `CustomPopupTests`,
`CustomPopupSliderTests`.

## It is a WINDOW, not a control
Built in code, shown with `ShowAt` / `ShowBelow` / `ShowAtCursor`, closes itself on a row
click, Esc, or `Deactivate` (a click elsewhere). Shown modeless, so **WinForms disposes it
— never wrap it in a `Using`**, or it dies before anyone sees it. Read the result from
`ItemClicked`, or from `ClickedItem` in `FormClosed`; `Nothing` = dismissed.

## CustomPopupItem
- `Key` (non-empty, unique; ignored on separators), `Text`, `Image`, `Enabled = True`,
  `Tag` (never read by the popup), `Mnemonic` (read-only)
- `Text` carries the access letter: `"&Salvează"` → S; `"&&"` is a literal ampersand.
- `IsSeparator = False` — thin unselectable line; key/text/image ignored.
- `IsSlider = False` + `SliderMinimum = 0` / `SliderMaximum = 100` / `SliderValue` /
  `SliderFraction` — a row that DRAGS instead of clicking. It does not close the menu; it
  raises `SliderValueChanged` while dragging and `SliderValueCommitted` on release.
- Factories: `CustomPopupItem.Separator()`, `CustomPopupItem.Slider(key, text, min, max, value)`
  (`maximum <= minimum` → `ArgumentException`).

## CustomPopup
- Ctors: `New()`, `New(items)`, `New(items, selectedKey)` — the caller states the selection
  at open time.
- `Items`, `SelectedIndex`, `SelectedItem`, `SelectedKey`, `ItemByKey(key)`,
  `ContainsKey(key)`, `ClickedItem`, `NaturalSize`
- Events: `ItemClicked`, `SelectedItemChanged`, `SliderValueChanged`, `SliderValueCommitted`
- Show: `ShowAt(anchor, screenPoint)`, `ShowAtCursor(anchor)`,
  `ShowBelow(anchor[, anchorRect])`
- `ClosedJustNow` (Shared, read-only) — lets an anchor swallow the click that closed the
  menu instead of reopening it.
- Look: `ImageSize = 16`, `ItemHeight = 0` (0 = from the font), `MinimumPopupWidth = 120`,
  `MaximumPopupWidth = 420`, `CornerRadius = -1`, `ItemGradient = 14` (logical px, C2)
- Colours: `PopupBackColor`, `BorderColor`, `ItemForeColor`, `DisabledForeColor`,
  `HighlightBackColor`, `HighlightForeColor`, `SeparatorColor` — Empty = theme (C1), each
  with an `Effective*` read-only counterpart. `ApplyTheme(scheme)`.

## IPopupAnchor
The control that opened the menu implements `Sub SetPopupOpen(open As Boolean)` and stays
lit while the menu is up. The popup calls it exactly once on open and once on close, on the
single sink all three close paths go through — a host flag would leak on the path it forgot.

## Limits
- **It ACTIVATES** (unlike `KBotNavFlyout`/`TreeNodeFlyout`, which are `WS_EX_NOACTIVATE`):
  without activation there is no keyboard focus, and keyboard is half the requirement. The
  price is that the form underneath draws its title bar as inactive while the menu is open.
- Opening with zero items → `InvalidOperationException`; `ShowBelow` with an empty
  `anchorRect`, or an unknown/empty key, → `ArgumentException` (C3).
- Flat menu only: no submenus, no checkable rows, no multi-column layout, no scrolling for
  very long lists.
- `WS_EX_TOOLWINDOW` (no taskbar button) + `CS_DROPSHADOW` (the usual menu shadow).
