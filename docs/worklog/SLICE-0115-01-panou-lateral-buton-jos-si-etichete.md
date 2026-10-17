# SLICE-0115-01 - Side panel: fold button at the bottom, tooltips when folded, picture of open machete, today-only actions (17.10.2026)

Operator's request of 17.10.2026 on the panel of the main window (`KBotSidePanel` in `AvacontMain`).

## What changed and why
1. **«Actiuni recente» start over every day.** `AvacontMain.LoadActionsAsync` keeps only the actions whose day is today
   (`Moment.Date = Date.Today`), so every row shows just `HH:mm`. A one-shot timer (`_midnightTimer`, re-armed each time)
   reads the list again a few seconds after midnight, so an idle window empties itself too.
2. **Fold button moved to the bottom**, under «Erori din sesiune», bottom left, with the same icons and size as the
   button of `navViews` in the shell (`collapse_24` / `expand_24`, 14 logical px, 6 px air). The top bar of the panel is
   gone (the sections start at the very top); a band at the bottom (line + button) is reserved. New properties on the
   control: `CollapseButtonSize`, `CollapseExpandedImage`, `CollapseCollapsedImage` (same ShouldSerialize/Reset convention
   as `KBotNavList`); drawn with `KBotNavList.DrawItemImage`, a chevron when no icon is given. The button has its own tooltip.
3. **Folded / unfolded state remembered between runs**: this already existed (`AppSettings.HomeSidebarCollapsed`, read in
   `InitSidebar`, saved in `Sidebar_CollapsedChanged`); nothing to change, only documented in the help.
4. **Tooltip over the icons of the folded strip**: `KBotToolTip` (header = section title + its icon, body = one line per
   row; stacked rows as `caption: value`, closed machete in italics, tone colours kept, at most 12 lines, then «... și încă N
   mai jos»; an empty section shows its `EmptyText`). Markup in row texts is neutralised (`EscapeMarkup`).
5. **Picture of an open macheta** in the tooltip of its row in «Machete deschise»; a closed one has NO tooltip.
   - `KBotToolTipContent.BodyImage` (new): the tooltip window draws it under the body, fitted into 300 x 220 logical px,
     never enlarged, never disposed by the tooltip. `IsEmpty` and the fingerprint know it.
   - `KBotSidePanelRow.ToolTipHeader` and `ToolTipImageProvider` (new); the panel asks the provider each time the tooltip opens.
   - `AvacontMain`: `CaptureFront()` draws the macheta that is on screen into a bitmap (`Control.DrawToBitmap`, reduced to at
     most 640 x 420) right after `PutAwayFront()` and before it is hidden (`PutFrontAway`, `SwitchBase`); the macheta that is on
     screen when its row is hovered is drawn again. Bitmaps are kept by `MachetaRef.Id` and disposed when the macheta
     closes (`Page_FormClosed`, shell back to the opening page, reference change) and when the window closes.

## Files touched
`src/KBot.App/AvacontMain.vb`, `AvacontMain.Sidebar.vb`, `AvacontMain.Designer.vb` (button size + two icons);
`src/KBot.Controls/SidePanel/KBotSidePanel.vb`, `KBotSidePanelItems.vb`; `src/KBot.Controls/ToolTip/KBotToolTip.vb`
(`BodyImage`), `KBotToolTipWindow.vb` (picture); help `HelpContent/contabil/panou-lateral.md` + `help-version.txt` (0000-87).

## Test results
`dotnet build src\KBot.App\KBot.App.vbproj -c Debug`: 0 warnings, 0 errors. No tests written or run, nothing run on screen
(project rule). `Check-Help.ps1 -Coverage`: no error in `panou-lateral.md` (20 old errors in other files remain).

## Unverified / deferred
- Never seen on screen: the placement of the button and of the strip tooltip, and above all the picture - `DrawToBitmap`
  draws a Windows control faithfully, but a part that is a window of another process or ActiveX/WebView (Adobe, the FOREXE
  browser panel) can come out blank. The picture is taken after `PutAwayFront()` (the tab without a PDF), which is what
  the operator finds on return.
- Every open macheta has a picture after its first hover (if it is on screen) or after the first time it is covered.
- The midnight timer relies on `System.Windows.Forms.Timer` surviving sleep; after a long sleep the list refreshes at the
  next save, page change or midnight.
- Help captures `panou-lateral` and `panou-lateral-strans` are marked `redo:` (the button moved).

## Follow-up (17.10.2026)
The fold band is explicitly outside the scrolled area (opaque, pinned to the bottom edge; the sections scroll with the wheel in the
space above it) and, while unfolded, carries the text «Ascunde meniul lateral» beside the button (`KBotSidePanel.CollapseButtonText`,
set in `AvacontMain.Designer.vb`); the whole button + text strip is the click and hover area. Build 0/0; not seen on screen.
