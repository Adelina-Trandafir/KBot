# SLICE-0078-16 — Jurnalul ActiveX pe culoare (lane-uri)

Operator request, 07.10.2026: in the ActiveX log viewer, a button that takes the loaded log and opens a
new form with the lane control (`KBot.Controls/Lane`); no date, only time and milliseconds; each root
node of the left tree becomes a lane, each leaf a change in that lane.

Slice number: **0078-16 is an assumption** (continuation of the 0078-15 viewer; the operator gave none).

## 1. What changed and why
- `ActivexLogViewerForm`: new button `btnLanes` («Pe culoare…») in the top bar. `LaneSpecsOf(tvLeft)` reads
  the LEFT tree as it is now (so «Fără rândurile arborilor de ferestre» and «Ascunde ce e identic» apply):
  root → `ActivexLaneSpec` (caption = kind name), every leaf under any element of the root → one marker.
  Markers are put in log order (`ActivexLogLine.Index`).
- `ActivexLaneForm` (new, DEBUG-only, `HarnessTests/`): one `KBotLaneView`, captions on, axis on,
  `MomentFormat = "HH:mm:ss.fff"`. The marker's moment is `2000-01-01 + time of day` parsed from the log
  line's stamp (`hh\:mm\:ss\.fff`); the fixed day is never shown. Markers of one element share a colour
  (`AutoColor`), so the stretch each marker owns reads as «this element holds until the next change».
  Lanes are not drop targets. The tooltip of a marker holds the whole log line (cut at 600 characters).
- Not modal, like the viewer. A line with no parseable stamp is left out and counted in the status line.

## 2. Files touched
- `src/KBot.App/HarnessTests/ActivexLaneForm.vb` + `.Designer.vb` (new)
- `src/KBot.App/HarnessTests/ActivexLogViewerForm.vb` + `.Designer.vb`
- `docs/worklog/KBOT_STATUS.md`, `docs/worklog/state/KBOT_STATUS_0070-0079.md` (0078-16 rows)

## 3. Test results
- `dotnet build src\KBot.App\KBot.App.vbproj -c Debug`: 0 warnings, 0 errors.
- No tests written or run (standing rule). Never opened on screen.

## 4. Unverified / deferred
- How it looks and behaves with real data is unseen: hundreds of markers in the Handle/Pid lanes,
  tooltip look, colours per element.
- A log that runs past midnight folds back onto the start of the axis (the date is deliberately not used).
- Only the left tree is used; the right side could be added as extra lanes if wanted.
- Help: none — DEBUG harness, never seen by the operator.
