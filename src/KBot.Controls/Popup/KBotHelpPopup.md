# KBotHelpPopup, KBotHelpSearchPanel, KBotHelpList

The help's «?» popup and the search it shares with the help window (slice 0000-20). The
controls only draw and report; every row, every text and every action comes from the
application through `IKBotHelpSearchSource` (KBot.App `HelpSearchSession`), because the controls
cannot reference KBot.App.

`Popup/` — `KBotHelpPopup.vb` (+ `.Designer.vb`), `KBotHelpSearchPanel.vb` (+ `.Designer.vb`),
`KBotHelpList.vb`, `KBotHelpStars.vb`, `KBotHelpRow.vb` (row, row kinds, actions, EventArgs,
`IKBotHelpSearchSource`)
All four `IThemedControl`, `ToolboxItem(False)`. Conventions: [C1..C9](../CONTROLS.md).
Status: builds; never rendered or clicked on screen.

## IKBotHelpSearchSource
- `HomeRows()` — rows for an empty search box (popup: the screen's topic and the tours; help
  window: none).
- `Search(query)` — rows for the query; local and quick, called while the operator types.
- `Invoke(row, action)` → `True` when the popup should close.
- `Rate(stars)` (1..5, 0 = taken back), `CurrentRating()` (the stars of the question now in the
  box), `EndQuestion()` (box emptied, popup / window closed).

## KBotHelpRow
`Kind` (`Header`, `Hit`, `Topic`, `Folder`, `Tour`, `Note`), `Title`, `Subtitle` (hit: section),
`Snippet` (hit, two lines), `OpenText` / `TourText` (hit buttons; empty = no button),
`ToolTipText`, `Children` + `Expanded` (folder), `Tag` (the application's object, never read here),
`IsSelectable` (not Header / Note).

## KBotHelpList
Painted list + its own `KBotScrollBar`. `SetRows(rows)`, `Rows`, `SelectedRow`,
`MoveSelection(delta)`, `InvokeSelected()`, event `RowInvoked(KBotHelpRowEventArgs: Row, Action)`.
A folder opens / closes itself (never raised). Hover shows `KBotToolTip.ShowAt` with the row's
text or the button's caption. Metrics are logical px scaled at layout (C2); colours only from
the palette, kept in fields (nothing for a designer to serialize, C4).
Not selectable: the search box keeps the keyboard.

## KBotHelpStars (slice 0000-21)
Painted rating line: caption «A fost util răspunsul?» + five stars. `Value` (0..5, clamped, set
by the host without an event), event `RatingChanged` (a click gives 1..5; a click on the star
already given takes it back, 0). Hover lights the stars a click would give. Colours: caption
`TextDim`, lit stars `Warning`, unlit outline `Border`.

## KBotHelpSearchPanel (UserControl)
Search box (`KBotTextField`) + list + stars (shown only while the box has text; the value comes
from `IKBotHelpSearchSource.CurrentRating` after every search, a click goes to `Rate`). `Source`, `QueryText`, `HasRows`, `CollapsedHeight`,
`FocusSearch()`, `SetQuery(text)` (no pause, handler detached while writing), `EndQuestion()`.
Events: `CloseRequested`, `EscapePressed`, `RowsVisibleChanged`. Typing searches 150 ms after the
last key; Up / Down / Enter drive the list; Enter first runs a pending search.

## KBotHelpPopup (Form)
`New(source)`, `ShowUnder(anchorScreenRect, owner)` (right edges aligned under the anchor,
above it when there is no room below, kept on the anchor's screen; placed again after `Load`,
when the theme zoom has sized it), event `FullHelpRequested` («Deschide ajutorul complet»).
Closes on Esc, `Deactivate` and `CloseRequested`, always deferred (`BeginInvoke`) so the handler
that caused it finishes on a live window. Shown modeless: WinForms disposes it, never `Using`.
`WS_EX_TOOLWINDOW` (no taskbar entry); `KBotThemedForm` with `CenterOnScreen` and
`AutoFitToTheme` off.

## Limits
- Folders open inline in the list (no cascading submenu).
- No typing-ahead in the list itself; no drag; no multi-select.
