# SLICE-0050-02 - the calendar popup lines up with the RIGHT edge of a field wider than itself (07.11.2026)

Operator request: when the control that opens the custom calendar is wider than the calendar window, the window must align to the control's right edge, not the left one. Slice 0050 is the last slice that worked on the calendar directly (the later touches came from other slices' sub-passes); 0050-02 is its first free sub-number.

## What changed and why
- `KBotCalendarPopup.ShowBelow`: the popup's left edge was always the anchor's left edge, so under a wide field the calendar floated at the left. Now, when `anchorRect.Width > calendar NaturalSize.Width`, the left edge is `anchor right - calendar width`. A field narrower than the calendar keeps the old left alignment.
- The existing rules stay: flip above when there is no room below, and `CustomPopup.FitToWorkArea` still falls back to the anchor's right edge / clamps to the screen when the calendar would leave the work area.
- Callers affected (both pass a rectangle wider than the calendar only when the host is wide): `KBotDatePicker` (anchor = the whole field) and the grid date editor `KBotDataView.DateEditor` (anchor = the cell box). A wide grid cell therefore also right-aligns the calendar.

## Files touched
`src/KBot.Controls/Calendar/KBotCalendarPopup.vb`; `src/KBot.Controls/KBot.Controls.vbproj` (`FileVersion` 1.85.0.4 -> 1.85.0.5); `docs/worklog/state/KBOT_STATUS_0050-0059.md`, `docs/worklog/KBOT_STATUS.md`.

## Test results
`dotnet build src/KBot.Controls/KBot.Controls.vbproj`: 0 errors, 0 warnings. No tests written or run (project rule).

## Unverified / deferred
- NOT seen on screen: not opened in a wide `KBotDatePicker` nor in a wide grid cell.
- Help: no section describes where the calendar opens (`clasificatii.md` only says the button opens it), so nothing to tag.
- Not committed (repo rule: no git writes from here).
