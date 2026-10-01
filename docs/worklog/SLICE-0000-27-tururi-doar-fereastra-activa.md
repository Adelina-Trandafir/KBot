# SLICE-0000-27 — «?» popup tours: only the window whose «?» was pressed (and its views)

Operator request, 01.10.2026: the guided tours listed in the «?» popup of a caption bar must show
only what belongs to the window whose «?» was pressed; its views may share tours, any other window
(with or without a parent) is not part of it.

## What changed and why

- `HelpPopupTours.Collect` takes the root window and looks at that window ONLY (the form and the views /
  controls inside it). No other window counts, owned by it or not. Before, it took every open form of
  the application. First pass (owned windows included) was rejected by the operator: only views are
  separate objects that may share tours; another window, with or without a parent, is not part of
  this window's «?». The per-window folders in the popup can no longer occur.
- `HelpService.ShowHelpMenu` passes `origin.FindForm()` (the form holding the pressed «?») and
  `PopupHomeRows` hands it on. A child control (a view) was already covered: a tour belongs to a
  window when its screen is visible inside it.
- Help: section «Meniul «?»» of `contabil/ajutor.md` (tag `0000-27`). `HELP_SYSTEM.md` row updated.

## Files touched

- `src/KBot.App/Help/HelpPopupTours.vb`, `src/KBot.App/Help/HelpService.vb`
- `src/KBot.App/HelpContent/contabil/ajutor.md`, `docs/HELP_SYSTEM.md`
- `docs/worklog/state/KBOT_STATUS_0000-0009.md`

## Test results

- `dotnet build src/KBot.App/KBot.App.vbproj`: **0 warnings, 0 errors**.
- `tools/HelpCheck/Check-Help.ps1 -Coverage`: «No errors.» (coverage lists only RobotQueueForm, SetariIstoricView, UpdateOfferForm, unchanged).
- Nothing seen on screen.

## Unverified / deferred

- Not seen on screen.
