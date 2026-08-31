# AdvancedTreeControl

Owner-drawn tree: nodes with expanders, checkboxes/radios, left+right icons, a rich-text
caption, an optional header band with a search bar, an optional footer band, a collapse
button with a hover flyout, and a TreeListView mode that turns rows into columns.

`Tree/` — `AdvancedTreeControl.vb` + 23 partials (`.API`, `.Properties`, `.Painting`,
`.Overrides`, `.Events`, `.Theming`, `.Header`, `.Footer`, `.Search`, `.ColFilter`,
`.ListMode`, `.Keyboard`, `.Drag`, `.Dpi`, `.Paddings`, `.ButtonHover`, `.ButtonTips`,
`.DesignerNodes`, `.NodeInspector`, `.TreeItem`, `.Popup`(+`.Table`, `.Branching`)),
plus `TreeNodeDefinition` (+ collection), `ColumnDef`, `TreeNodeFlyout`,
`TreeImageKeyConverter`, `TooltipTableModel`, `TreeLogger`, `NodeDebugInfo`, `FrmNodeDebug`.
`Control` · partial · Toolbox · `IThemedControl`, `IDpiScaledControl`
Conventions: [C1..C9](../CONTROLS.md).
Status: designer surface **screen-verified 2026-08-08** (slice 0027); the footer + collapse
pass (0027-02) is code-green and was never seen on screen. Harness:
`AdvancedTreePlaygroundTest`, `AdvancedTreeVisualTest`.

Imported from TREEVIEW_VBA with the Access bridge cut (host form, `TrimiteMesajAccess`,
the `/frm /acc /idt /log` arguments, the `SET_CHECKBOX||NodeID||State` string API). What is
left is the reusable control with native events.

## Two ways to fill it
1. **Code** — `AddItem(pKey, pCaption, [pParent], [pLeftIconClosed], [pLeftIconOpen],
   [pRightIcon], [pTag], [pExpanded], [pLazyNode]) As TreeItem`, `Clear()`,
   `SelectAndReveal(node)`.
2. **Designer** — `Nodes: TreeNodeDefinitionCollection`, a FLAT list linked by `ParentKey`,
   with `NodeImages: ImageList` resolving every image KEY (`ImageKey`, `OpenImageKey`,
   `RightImageKey`, and the header/footer/search/collapse key properties) through
   `NodeImage(key)`. A `ParentKey` that is not found promotes the node to root.
   `TreeNodeDefinition` also carries `Caption`, `Tooltip`, `Tag`, `Expanded`, `HasCheckBox`,
   `LazyNode`.

## TreeItem (runtime node — public fields, not properties)
`Key`, `Caption`, `Children`, `Parent`, `Level`, `Expanded`, `CheckState`
(`TreeCheckState`), `LeftIconClosed`, `LeftIconOpen`, `RightIcon`, `LazyNode`, `Bold`,
`Italic`, `NodeForeColor` / `NodeBackColor` (Empty = inherit / transparent),
`IsLoader`, `IsRadioSelected`, `Tooltip` (always shown when set), `HasCheckBox`,
`ShowRightIconOnHover`, `ColHeaderText` (pipe-separated column names for dynamic columns),
`Cells: Dictionary(Of String, CellData)` where `CellData` = `Value` + `BackColor` +
`ForeColor`, `Tag`, `IsLastSibling`, `SetExpanded(value, [expandParent])`.

## Caption markup
`<b> <i> <u> <color=#RRGGBB> <back=#RRGGBB>` (own parser,
`AdvancedTreeControl.ParseRichText`, `Friend Shared` — the general engine is
[KBotRichText](../ToolTip/KBotToolTip.md), which is deliberately separate). `~~~` splits the
caption into a left and a right part; `LeftTextWidth` / `RightTextWidth` (0 = dynamic) and
`PaddingSeparatorGap` control the split.

## Selection, checks, radios
`SelectedNode`, `OldSelectedNode`, `CheckBoxes = False`,
`SetItemCheckState(item, state)` with `TreeCheckState` = `Unchecked` | `Checked` |
`Indeterminate`, `RadioButtonLevel = -1` (the level that gets radios; -1 = off),
`SetRadioSelected(item)`, `HasNodeIcons = True`.

## Events
`NodeMouseDown` · `NodeMouseUp` · `NodeDoubleClicked` · `NodeChecked` ·
`NodeRadioSelected(nodeOn, nodeOff)` · `RequestLazyLoad(sender, item)` ·
`RightIconClicked` · `HeaderRightIconClicked` · `FooterRightIconClicked` ·
`FooterLeftIconClicked` · `SearchFinished(matchingItems, searchText)` ·
`CollapsedChanged(collapsed)` · drag: `NodeDragStarting` (cancellable) / `NodeDragOver`
(`Allow` + `Motiv`) / `NodeDropped`.

## Geometry (all logical px, C2)
`ItemHeight = 22`, `Indent = 10`, `ExpanderSize = 12`, `CheckBoxSize = 16`,
`LeftIconSize`, `RightIconSize`, `RootExpander = True`, `SetAutoHeight()`, and the
`Padding*` family in `.Paddings.vb`: `PaddingHeaderLeft`, `PaddingTreeStart`,
`PaddingSelectionLeft`, `PaddingTreeTop`, `PaddingTreeEnd`, `PaddingExpanderGap`,
`PaddingTreeLineHMargin`, `PaddingCheckBoxGap`, `PaddingIconGap`, `PaddingSeparatorGap`,
`PaddingTooltipIconHit`, `RightIconRightPadding`, `SearchClearButtonPadding`.

## Right icon
`ShowRightIconOnHover = False` (per control) and `TreeItem.ShowRightIconOnHover`
(per node). A hover-only icon does NOT reserve caption width — the text runs full width and
narrows under the cursor. `ReserveRightIconSpace = True` buys the fixed gutter back.

## Header band
`HeaderVisible = False`, `HeaderHeight = 32`, `HeaderCaption`, `HeaderTextAlign`,
`HeaderFont`, `HeaderLeftIcon` / `HeaderRightIcon` / `HeaderSearchIcon` (+ their
`*IconKey` designer counterparts, `HeaderIconSize`, hover colours, tooltips),
`HeaderBackColor`, `HeaderForeColor`, `HeaderBackStyle` (`Solid` or gradient) +
`HeaderGradientEndColor`, `HeaderSeparatorColor/Width`.

## Search band
`SearchShow = False` — with a `HeaderSearchIcon` present the icon opens/closes the band;
without one the band is permanent. `SearchDefaultText`, `SearchType`
(`En_Tree_SearchType`: `SearchType_Contains` | `SearchType_StartsWith`), `SearchIn`
(`En_Tree_SearchIn`: `SearchIn_Caption` | `SearchIn_Tag` | `SearchIn_Both`), `SearchMode`
(`En_Tree_SearchMode`: `SearchMode_Tree` | `SearchMode_List`), `SearchBackColor`, `SearchBoxBackColor`, `SearchBarLabelText`
(default `"Cautare: "`), `SearchBarLabelForeColor/Font`, `SearchBarFont`
(+ `SearchBarFontName` / `SearchBarFontSize`), `SearchClearButton` +
`SearchClearButtonImage` / `…ImageKey` / hover colour / padding (ESC clears),
`SearchSeparatorColor/Width`.

## Footer band + collapse
`FooterVisible = False`, `FooterHeight = 28`, `FooterCaption` (+ own font, fore/back
colour, `FooterTextAlign`), `FooterLeftIcon` / `FooterRightIcon` (+ `*IconKey`,
`FooterIconSize`, hover colours, tooltips), `FooterBackColor`, `FooterForeColor`,
`FooterBackStyle` + `FooterGradientEndColor`, `FooterSeparatorColor/Width`,
`ShowFooterLeftIcon` / `ShowFooterRightIcon` / `Footer*Rect` (read-only).
Collapse: `FooterCollapseButton = False`, `FooterCollapseButtonSize = 16`,
`FooterCollapseButtonPosition` (`Right` | `Left`), `FooterCollapseExpandedImage` /
`…CollapsedImage` (+ their `*Key`), `MinimumCollapsedWidth = 100`, `Collapsed`,
`ToggleCollapse()`, `HostOwnsWidth`, `ExpandedWidth`, `CollapseButtonTooltip` /
`ExpandButtonTooltip`.
Flyout while collapsed: `CollapsedFlyout = True`, `FlyoutSelectedNode = False`,
`FlyoutDelay = 250` ms, `FlyoutSlideDuration = 120` ms — `TreeNodeFlyout` floats the hovered
row out to the right.

## TreeListView mode
`TreeListView = False` (master switch), `DynamicColumns = True` (columns resolved per node
from `TreeItem.ColHeaderText`) or `False` + `ColumnsLevel = -1` (one static band at that
level), `ConfigureListMode(columns As IEnumerable(Of ColumnDef))`.
`ColumnDef` = `Name`, `Header`, `Width`, `ColType` (`Text|Number|Date|Boolean`), `Align`,
`Format`, plus header styling (`HeaderBackColor`, `HeaderForeColor`, `HeaderBold`,
`HeaderItalic`, `HeaderUnderline`, `HeaderAlign` with `ColAlign_Inherit`).
Column lines: `ColumnSeparatorColor/Width`, `ColumnHeaderSeparatorColor/Width`.

## Node tooltips (legacy, separate from `KBotToolTip`)
`TooltipShow = True`, `TooltipDelayMs = 600`, `TooltipBackColor` / `TooltipForeColor`
(also `TT_BackColor` / `TT_ForeColor`), `TooltipShowOnlyOnLeftIcon`,
`TooltipShowOnlyOnRightIcon`, `TooltipPopupHandle`, `TooltipTableModel` (the table layout).
The header/footer BUTTONS use `ButtonTooltip: KBotToolTip` instead (C8), with
`HeaderSearchIconTooltip`, `HeaderRightIconTooltip`, `FooterLeftIconTooltip`,
`FooterRightIconTooltip`.

## Other
- Drag: `DragEnabled = False`, `DragHighlightColor`, `DragForbiddenColor`, `DraggedItem`.
  **The control moves nothing by itself** — it raises the three events and the host does the
  restructuring.
- Colours: `BackColor`, `ForeColor`, `Font`, `BorderColor` (Transparent = none) +
  `BorderWidth`, `HoverBackColor`, `SelectedBackColor`, `SelectedBorderColor`, `LineColor`,
  the `*IconHoverColor` family, `SelectionCornerRadius`, `BandColorsFromThemeOnly`,
  `ApplyTheme(scheme)`.
- Popup mode: `IsPopupTree = False`, `PopupGraceMs = 1500` (no node double-click is raised).
- Scrollbars: `ScrollBarTheme = Explorer` (`Default` | `Explorer` | `DarkMode`) — native
  bars, uxtheme only. `AutoScrollPosition` / `AutoScrollMinSize` are shadowed.
- Diagnostics: `TreeLogger` (Init + Debug/Info/Warn/Err/Ex/Perf, `LogFilePath`),
  `NodeDebugInfo` + `frmNodeDebug.ShowForNode(...)` — a full per-node dump (bounds,
  metrics, cells, host form) for the node inspector.
- `ProcessPropertyRequest(cmd)` — string property bridge kept from the FOREXE integration,
  alongside `RightClickFunction`.

## Limits
- **One font.** `Font` sizes the row AND draws the nodes; the old `TreeFont` /
  `FontName` / `FontSize` are gone. Do not pin `"Segoe UI, 9"` — that is the default, and
  writing it defeats `ApplyBaseFont`.
- The side the collapse button sits on belongs to it: that end icon is **neither drawn nor
  clickable** (`FooterCollapseButtonPosition = Left` hides `FooterLeftIcon`).
- With `HostOwnsWidth` (a docked/anchored host) the control does **not** write `Width` —
  it flips state and raises `CollapsedChanged`, and the host moves the splitter. Writing
  `Width` against a docking parent only flickers. `MainForm.tree_CollapsedChanged` also has
  to drop `Panel1MinSize` for the duration: a min size set for dragging vetoes the
  programmatic collapse too.
- The TreeListView column band is hover-blind on purpose (`ReservedRightIconWidth`) — a
  whole-control geometry must not re-lay out under the cursor.
- Node tooltips use the legacy in-control popup, not `KBotToolTip`. Only the band buttons
  were migrated.
- Vertical scrolling only for the node area; no virtualization — every visible node is
  walked on paint.
- No built-in sorting (the header sort buttons are still a placeholder `MsgBox` at the host
  level), no multi-select, no in-place node editing, no node reordering by the control.
- `WndProc` is deliberately left unwrapped (the global `Application.ThreadException` net
  catches it) — wrapping it risks breaking the window-message contract.
