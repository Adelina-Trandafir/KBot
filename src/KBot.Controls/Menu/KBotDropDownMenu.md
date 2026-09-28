# KBotDropDownMenu

The K-BOT drop-down menu (slice 0087): the classic Windows 10 desktop menu -- a coloured bar
down the whole left side holding the row icons, rows with formatted text, separators that start
after the bar, a highlight that follows mouse and keys, and cascading submenus.

`Menu/` — `KBotDropDownMenu.vb` (+ `.Input`), `KBotMenuItem.vb` (item + collection + click
`EventArgs`), `KBotMenuWindow.vb` (one level on screen, Friend)
`Component` · Toolbox (sits in the designer tray) · not `IThemedControl` (resolves the theme
every time it opens)
Conventions: [C1..C9](../CONTROLS.md). Status: rendered off-screen (Classic + Dark) with
`DrawToBitmap`, slice 0087; never clicked through on screen.

## KBotMenuItem
Edited in the property grid (stock collection editor), like `KBotNavItem`.
- `Key` (non-empty, unique in the WHOLE tree; ignored on separators), `Text` (accepts the
  `KBotRichText` markup `<b> <i> <u> <color=#RRGGBB> <back=#RRGGBB>`), `ShortcutText` (dim,
  right-aligned; not on submenu rows), `Image` (left bar), `Height` (logical px, 0 = menu's
  `ItemHeight`), `Font` (Nothing = menu's), `ForeColor` (Empty = theme), `Enabled = True`,
  `Visible = True`, `IsSeparator = False`, `Tag`.
- `Items` — the submenu (nested collection editor). A row with at least one visible sub-row
  opens a submenu instead of raising `ItemClicked`; `HasSubmenu` says so.
- `KBotMenuItem.Separator()`, `New KBotMenuItem(key, text[, image])`.
- `KBotMenuItemCollection.AddRange`, `.Find(key)` (searches submenus too).

## KBotDropDownMenu
- `Items`, `DropDownButton` (a click on it opens the menu below it; a click while open closes
  it), `IsOpen`, `ShowBelow(anchor)`, `ShowAt(anchor, screenPoint)`, `Close()`.
- Events: `ItemClicked(KBotMenuItemClickedEventArgs: Item, Key)` (raised after the menu closed),
  `Opening` (cancellable), `Closed`.
- Look (logical px, C2): `ItemHeight = 30`, `ImageSize = 20`, `IconBarWidth = 38`,
  `MinimumWidth = 200`, `MaximumWidth = 480`, `Font` (Nothing = the host form's).
- Behaviour: `SubmenuDelay = 300` ms (mouse rest before a submenu opens or the open one closes).
- Colours (Empty = theme, C1): `BackColor`, `IconBarColor`, `ForeColor`, `BorderColor`,
  `HighlightBackColor`, `HighlightBorderColor`, `DisabledForeColor`, `SeparatorColor`.
- Keyboard: Up/Down (wrap, skip separators and disabled rows), Home/End, Right/Enter/Space open
  a submenu on its first row, Left/Esc close one level (Esc on the root closes all), Enter/Space
  on a command chooses it, Alt/Tab close.

## How it works
- The windows NEVER activate (`WS_EX_NOACTIVATE`, `MA_NOACTIVATE`, shown without activation,
  owned by the host form): several levels can be open at once and the host keeps its active
  title bar. That is the difference from `CustomPopup`, which activates (it needs the focus for
  its keyboard) and so cannot cascade.
- While open, an application message filter routes the keys to the deepest window and closes
  the menu on a press anywhere outside its windows (a press on `DropDownButton` only closes it,
  never reopens). The host form's `Deactivate` closes it too. An `IPopupAnchor` button is told
  when the menu opens / closes.
- Keys are validated when the menu opens (empty / duplicate → `ArgumentException`, C3); no
  visible row → `InvalidOperationException`.

## Limits
- No check marks, no radio rows, no scrolling for menus taller than the screen.
- No mnemonics (access letters) yet.
- Text is one line; `MaximumWidth` cuts longer text at the edge (no ellipsis for markup).
