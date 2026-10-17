# SLICE-0114-06 - Ribbon: a group for the FORM, next to the view group (operator item 1) (17.10.2026)

## What changed and why
New control property `KBotRibbon.FormGroup` (`KBotRibbonGroup`, serialized like `RightGroup`): placed at the very right of the panel, against the large pinned buttons («Inchide»). `RightGroup` stays the group of the VIEW and now stands to its LEFT. Order, right to left: X («Inchide»), group of the form, group of the view. Layout, painting, hit-test, `AllGroups` (FindButton / FindField / ValidateKeys) cover both. Both groups show only while they have a visible button or field and not while the ribbon is folded.

## Files touched
`src/KBot.Controls/Ribbon/KBotRibbon.vb`.

## Test results
`dotnet build src/KBot.App/KBot.App.vbproj`: 0 warnings, 0 errors. Nothing run, nothing seen on screen.

## Unverified / deferred
- Not seen on screen: layout of the two ribbon groups side by side, the footer popup position, the nav order.
