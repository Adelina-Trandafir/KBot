# SLICE-0000-24 — tour bubble without a caption bar, section titles in the «?» menu

Operator request, 30.09.2026, with two screenshots:

1. The guided-tour bubble should have **no caption bar at all**, a **slightly larger font**, and
   the **hole on its left side** must go.
2. In the «?» menu, **«Tururi ghidate»** sat too high (glued to the row above it) and did not read
   as the title of the tours listed under it.

## What changed and why

### Tour bubble (`HelpTourBubble`)

- **Caption bar removed** (`capBar` gone from the designer). The tour's name, which the bar used to
  show, now leads the step line: «Fereastra principală  ·  Pasul 4 din 32». Closing stays on
  «Închide», Esc and Alt+F4 (all raise `CloseRequested`).
- **Larger text**: step line 9.75 pt, title 12.5 pt semibold (was 11), body 10.5 pt, note 10 pt,
  buttons 10 pt (explicit fonts in the designer). Width 400 → 440 logical, button columns
  100/100/100, button row 40 → 44, inner padding 14/8/14/10 → 16/12/16/12. `FitToText` no longer
  counts the bar and uses the 44 px row.
- **The hole on the left**: the callout window is the body plus a strip for the triangle; its
  Region cuts the strip down to the triangle. On Windows 11 DWM still rounds the WHOLE window
  rectangle and draws its grey 1 px border around it (the theme asks for rounded corners on every
  borderless form), so the transparent strip beside the triangle was framed and read as a hole.
  This is read from the screenshot (a rounded grey outline around the full rectangle, triangle
  included) — not measured on the machine. New `HelpWindowNative.PlainFrame(hWnd)` sets
  `DWMWA_WINDOW_CORNER_PREFERENCE = DWMWCP_DONOTROUND` and `DWMWA_BORDER_COLOR = DWMWA_COLOR_NONE`
  (Windows 10 does not know them: the calls return an HRESULT and change nothing). The bubble calls
  it after every placement, in `OnShown`, and after each theme change (`BeginInvoke`, because
  `ThemeManager.Apply` rounds borderless forms after `OnThemeChanged`).

### «?» menu headers (`KBotHelpList`, shared with the search results of the help window)

- A header row now has air above it (16 px logical; 4 px for the first row, right under the
  search box) and 6 px below; its title is drawn in the **accent colour**, bold, standing on a thin
  rule (button-border colour) across the list. Before: dim grey, `VerticalCenter Or Bottom` in a
  row only 12 px taller than the text, so it looked glued to the row above.

## Files touched

- `src/KBot.App/Help/HelpTourBubble.Designer.vb` — capBar removed, fonts, sizes
- `src/KBot.App/Help/HelpTourBubble.vb` — tour name in `lblPas`, `FitToText`, `PlainFrame` calls, comments
- `src/KBot.App/Help/HelpWindowNative.vb` — `PlainFrame` (DWM corner + border)
- `src/KBot.Controls/Popup/KBotHelpList.vb` — header air, accent title, rule
- `src/KBot.App/HelpContent/contabil/ajutor.md` — «Meniul «?»» (titled parts) and «Tururile
  ghidate» (tour name + step at the top of the bubble), tags `0000-24`

KBot.Controls FileVersion was already bumped in the uncommitted work (1.57 → 1.58); not bumped again.

## Test results

- No tests (operator: no tests).
- `dotnet build src\KBot.App\KBot.App.vbproj`: **0 warnings, 0 errors**.
- `Check-Help.ps1 -Coverage`: **No errors.** Coverage lists only `RobotQueueForm` (pre-existing,
  already in the 0000 Open threads under 0098).

## Left unverified or deferred

- **Not seen on screen.** The DWM diagnosis of the «hole» comes from the screenshot only; if the
  outline is still there on the operator's PC, the next suspect is the theme re-rounding after a
  path not covered here.
- The bubble still inherits `KBotShellForm` (8 px resize band on its edges); left as it was.
- Captures to re-shoot: `ajutor-meniu` (the headers look different now).
- Pre-existing, not from this slice: `RobotQueueForm` has no help topic (`-Coverage`).
