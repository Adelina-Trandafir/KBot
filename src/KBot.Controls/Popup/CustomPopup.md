# CustomPopup

The K-BOT context menu: looks like a system menu (rows, icon rail, underlined access letter, highlight that follows BOTH mouse and keys) but is drawn by us, so it takes the scheme colours. `ContextMenuStrip` could not: its face is drawn by `ToolStripRenderer` and stays a white strip on a dark scheme.

`Popup/` — `CustomPopup.vb` (+ `.Painting`, `.Input`, `.Slider`), `CustomPopupItem.vb` (+ collection + `EventArgs`), `PopupMnemonic.vb`, `IPopupAnchor.vb` `Form` · `IThemedControl` · not a Toolbox control (`DesignerCategory("Code")`) Conventions: [C1..C9](../CONTROLS.md). Status: covered by `CustomPopupTests`, `CustomPopupSliderTests`.

## It is a WINDOW, not a control
Built in code, shown with `ShowAt` / `ShowBelow` / `ShowAtCursor`, closes itself on a row click, Esc, or `Deactivate` (a click elsewhere). Shown modeless, so **WinForms disposes it — never wrap it in a `Using`**, or it dies before anyone sees it. Read the result from `ItemClicked`, or from `ClickedItem` in `FormClosed`; `Nothing` = dismissed.

## CustomPopupItem
- `Key` (non-empty, unique; ignored on separators), `Text`, `Image`, `Enabled = True`, `Tag` (never read by the popup), `Mnemonic` (read-only)
- `Text` carries the access letter: `"&Salvează"` → S; `"&&"` is a literal ampersand.
- `Italic = False` (0097-09) — optional italic command text, used for closed navigation-history entries. Measurement and drawing use the same row font.
- `IsSeparator = False` — thin unselectable line; key/text/image ignored.
- `IsSlider = False` + `SliderMinimum = 0` / `SliderMaximum = 100` / `SliderValue` / `SliderFraction` — a row that DRAGS instead of clicking. It does not close the menu; it raises `SliderValueChanged` while dragging and `SliderValueCommitted` on release.
- `SliderSnapPoints` (Integer(), empty = free) + `SliderSnapTolerance = 3` — values the thumb sticks to while DRAGGING when it passes within the tolerance; drawn as thin ticks on the track. Keyboard steps are not snapped. `SnapSliderValue(v)` is the pure rule.
- Factories: `CustomPopupItem.Separator()`, `CustomPopupItem.Slider(key, text, min, max, value)` and `Slider(key, text, min, max, value, snapPoints)` (points outside the range are dropped, duplicates merged, list sorted; `maximum <= minimum` → `ArgumentException`).

## CustomPopup
- Ctors: `New()`, `New(items)`, `New(items, selectedKey)` — the caller states the selection at open time.
- `Items`, `SelectedIndex`, `SelectedItem`, `SelectedKey`, `ItemByKey(key)`, `ContainsKey(key)`, `ClickedItem`, `NaturalSize`
- Events: `ItemClicked`, `SelectedItemChanged`, `SliderValueChanged`, `SliderValueCommitted`
- Show: `ShowAt(anchor, screenPoint)`, `ShowAtCursor(anchor)`, `ShowBelow(anchor[, anchorRect])`
- `ClosedJustNow` (Shared, read-only) — lets an anchor swallow the click that closed the menu instead of reopening it.
- Look: `ImageSize = 16`, `ItemHeight = 0` (0 = from the font), `MinimumPopupWidth = 120`, `MaximumPopupWidth = 420`, `CornerRadius = -1`, `ItemGradient = 14` (logical px, C2)
- `FontSizeOffset = 0` (0097-09) — extra points on the anchor's font for command rows; navigation history sets 2. Derived fonts are owned by the popup and released on font changes/disposal.
- `PreviewColumns = 0` (0097-09) — zero retains command rows; a positive value lays commands out as thumbnail tiles. Each `CustomPopupItem.PreviewImage` is a borrowed snapshot (fallback: item icon). The existing palette supplies selection and caption/icon bands; the host owns and keeps images alive until closing. `ShowCentered(anchor)` shows this mode centred on the host, clamped to its monitor. The regular history menu keeps its 180 px list; only the Ctrl+Tab selector enables tiles. Held-key and MRU behaviour belong to the application filter.
- Colours: `PopupBackColor`, `BorderColor`, `ItemForeColor`, `DisabledForeColor`, `HighlightBackColor`, `HighlightForeColor`, `SeparatorColor` — Empty = theme (C1), each with an `Effective*` read-only counterpart. `ApplyTheme(scheme)`.

## IPopupAnchor
The control that opened the menu implements `Sub SetPopupOpen(open As Boolean)` and stays lit while the menu is up. The popup calls it exactly once on open and once on close, on the single sink all three close paths go through — a host flag would leak on the path it forgot.

## Limits
- **It ACTIVATES** (unlike `KBotNavFlyout`/`TreeNodeFlyout`, which are `WS_EX_NOACTIVATE`): without activation there is no keyboard focus, and keyboard is half the requirement. The price is that the form underneath draws its title bar as inactive while the menu is open.
- Opening with zero items → `InvalidOperationException`; `ShowBelow` with an empty `anchorRect`, or an unknown/empty key, → `ArgumentException` (C3).
- One menu level per window; very long lists scroll with the mouse wheel and keep the keyboard selection visible. No multi-column layout.
- `WS_EX_TOOLWINDOW` (no taskbar button) + `CS_DROPSHADOW` (the usual menu shadow).

## Input rows: combo and text box (slice 00EF-34)
`CustomPopupItem.Combo(key, text, rows)` and `CustomPopupItem.TextInput(key, text, placeholder, maxLength, digitsOnly)`. A row is ONE row like any other (icon band on the left, the real KBot control on the rest: `KBotComboBox` not editable / `KBotTextBox`), made by `CustomPopup.Inputs.vb` (00EF-37: no caption line any more). The `text` of a combo is its pale text while nothing is picked (`KBotComboBox.PlaceholderText`, not a row of the list); the placeholder of a text row stays visible while the box is empty, also focused (`KBotTextBox.PlaceholderWhileFocused`), and goes away with the first character. Picking a row of the combo, or Enter in the box, closes the popup and raises `ItemClicked` for that item; read `Item.ComboSelectedIndex` (starts at -1: picking the first row counts) or `Item.InputText`. An input row is never highlighted, has no access letter, and while an input has the focus every key is its own (only Esc closes the popup). `DigitsOnly` keeps 0-9.

## Icon bar (slice 00EF-34, operator 16.10.2026)
The icons of the rows sit on a coloured vertical bar along the whole height of the popup, as the Windows 7 menus had (`CustomPopup.IconBar.vb`). It exists only while some row has an icon. Properties (settable in the designer file of the host or in code):
- `IconBarVisible` (default True; False = icons on the plain background as before),
- `IconBarSide` (`PopupIconBarSide.Left` default / `Right`): with **Right** the text of the rows is aligned right, the check mark and the chevron move to the left end, the combo / text box of the input rows align right too,
- `IconBarColor` (Empty = from the theme: the background with 14% of the accent; `EffectiveIconBarColor`). The geometry is shared through `BarOnRight`, `IconBarWidth`, `LeftInset` / `RightInset`, `IconRectOf`, `TextRectOf` (painting, sliders, input rows, capture redaction). `IconGutter` now includes the padding before the icon when the bar is on.
