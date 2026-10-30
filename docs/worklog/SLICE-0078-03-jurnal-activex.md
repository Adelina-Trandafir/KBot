# SLICE-0078-03 — ActiveX viewer: exhaustive diagnostic trace

Regrupare documentară D001-05 (25.10.2026): continuarea [0078-16](#slice-0078-16) este inclusă mai jos cu textul, probele și limitele sale istorice. Referințele «§» se citesc în intervenția care le conține.

Date: 24.09.2026. Operator request: the ActiveX (AcroPDF) viewer misbehaves on a client PC that cannot be debugged, so everything its controller and timers see and do must land in a separate file, switched on / off from «Setări».

## What changed and why

- **AcroPdfTraceLog**, new terminal sink KBot.Controls/Adobe, never throws: `<AppDir>\Logs\acropdf_trace.log`, adobe_preview.log format (timestamp/two spaces/text), 10MB ×5 rotation. Each line has managed thread id and source AcroPdfSurface/SaveTrap#n, separating DDF/ORD. Enabled reads AppSettings.Current.AcroPdfTrace per call, applying changes immediately to open documents; tree dumps are built only when on.
- **AcroPdfSurface traces:** construction clsid/panel/OS/bitness/dpi; screen events/inputs (panel visible, form, minimized); refit ticks/remaining/restart/give-up plus final tree; header candidates/siblings/rects/MoveWindow result; load size/LoadFile answer/duration/phase timings/control version/final tree; each pane attempt/tree/focus/nudge/foreground; collapse candidates/choice/tree; chrome hide decisions; OwnerPids renderer/Adobe-parent/broker/result per trap sweep; create/clear/dispose; full-stack exceptions; existing adobe_preview.log lines as REPORT:.
- **AdobeSaveTrap.Traced**, True only for AcroPdfSurface (hosted-window engine excluded). Traces: start/stop/pause/resume full state; hook handle/Win32 error/removal; ticks; sweeps with each #32770 pid/visibility/rect/owner/title; WinEvents and handling/ignore reasons; dialog classification/arbitration; filename edit/write/readback; posted/sent messages/results; pending-save state per sweep; script-noise decisions; REPORT: mirror.
- **Setting** `AppSettings.AcroPdfTrace` (JSON key `AcroPdfTrace`, default False).
- **Setări ▸ Aplicație ▸ Documente**: checkbox «ActiveX — jurnal de diagnostic detaliat (acropdf_trace.log)» under the engine combo, saved on change like the other switches.
- **Log viewer**: `acropdf_trace.log` is guessed as the `AdobeHost` format.

## Files touched

`src/KBot.Controls/Adobe/AcroPdfTraceLog.vb` (new); `src/KBot.Controls/Adobe/AcroPdfSurface.vb`; `src/KBot.Controls/Adobe/AdobeSaveTrap.vb`; `src/KBot.Common/AppSettings.vb`; `src/KBot.Common/Logging/LogFileLoader.vb`; `src/KBot.App/Setari/SetariAplicatieView.vb`, `SetariAplicatieView.Designer.vb`

## Test results

- `dotnet build src\KBot.App\KBot.App.vbproj`: 0 errors, 0 warnings.
- No tests written or run (house rule: never run tests).

## Left unverified / deferred

- Nothing seen on screen: the new checkbox row in «Documente» and the trace file itself have not been looked at. The file has never been produced on a real run.
- Volume: with a document open, `OnTick` + `Sweep` + `OwnerPids` write several lines every 200 ms (100 ms during a script-error burst). Rotation caps it at 60 MB; switch it off after collecting.
- FileVersion not bumped again: Controls / Common / App are already bumped in the uncommitted 0078 work this sits on.

---

## Pass 02 (24.09.2026) -- first client trace read; blank control fixed; trace trimmed

### What the trace showed (`temp_logs\acropdf_trace.log`)

1. **First load: Adobe had not started yet.** `LoadFile` returned True, but the control held ZERO windows for the whole 4 s the pane wait allowed. The control was then destroyed and recreated, in the middle of Acrobat's cold start. On the second control the first window («Acrobat External Window», owned by the renderer) appeared 24 ms after the load.
2. **Second load: blocked by a modal form-script alert.** 1.3 s later the DDF's script raised «Warning: JavaScript Window -- GeneralErrorOperation failed.», owned by K-BOT's main form. The trap sent BM_CLICK to its OK three times (each returned True) and the alert stayed: BM_CLICK is documented to fail when the dialog is not active, and our form held the foreground. The trap then HID the alert. It is modal to Adobe's view, so hidden it kept Adobe blocked for good, and no view was ever built.
3. The note at the bottom said «Panourile Adobe nu au putut fi colapsate» (the panes could not be collapsed), which was misleading: the collapse never ran because nothing was shown.

### What changed

- AdobeSaveTrap.PressOk uses successive OK methods: 0 = post WM_COMMAND/BN_CLICKED with button handle (no activation); 1 = SetForegroundWindow + BM_CLICK; 2 = WM_CLOSE. If all fail, an alert with OK stays VISIBLE for the operator, with log warning. Shared by both engines; new InScriptBurst.
- `AcroPdfSurface.WaitForPaneTreeAsync`: phase 1 waits up to 20 s for the control's FIRST window (cold start) before anything is recreated; phase 2 is the old 10 x 400 ms, but attempts do not count and nothing is nudged while script alerts are up; the whole wait is capped at 40 s.
- When the second load builds nothing, the result now says «Adobe nu a afișat documentul — vezi jurnalul.» and `ReaderHostPreview` shows the surface's message in the bottom note.
- **Recording sessions**, operator asks stop on open/blocking error: BeginSession at load; EndSession at header-search end (= open), blocking failure (unregistered/missing file/control creation/empty second load/stuck script alert/exception), or viewer leaving screen while opening. No writes outside sessions.
- **Noise cut:** tick/state, watched-dialog list, OwnerPids and pane-tree traces only on change; unwatched dialogs omitted, each dialog once per handle/kind. Repeated not-visible/waiting lines removed.
- **Switch reset at close**, operator forbids default-on: remove AppSettings.AcroPdfTrace; use memory-only AcroPdfTraceLog.SwitchedOn, False at each start. Old app_settings.json AcroPdfTrace key ignored.
- Rule 0 sweep: one old comment in `AcroPdfSurface.vb` quoted a Romanian log line; translated.

### Files touched

`AcroPdfTraceLog.vb`, `AcroPdfSurface.vb`, `AdobeSaveTrap.vb`, `AdobeNativeMethods.vb` (SetForegroundWindow, BN_CLICKED), `AppSettings.vb`, `ReaderHostPreview.vb`, `SetariAplicatieView.vb`, `SetariAplicatieView.Designer.vb`.

### Test results

`dotnet build src\KBot.App\KBot.App.vbproj`: 0 errors, 0 warnings. No tests run.

### Unverified

- Whether any of the three OK methods closes this alert. The trace will show which one did (`PressOk` lines), or say BLOCKING if none did.
- Whether 20 s is enough for Acrobat's cold start on the client PC.
- Whether «Acrobat External Window» fills with the AVL_* views once the alert is closed, or whether this Acrobat build renders the control some other way. The next trace answers it.

---

## Pass 03 (24.09.2026) -- second client trace; new Read Mode path (Ctrl+H)

### What the trace showed (`temp_logs\acropdf_trace (1).log`, recordings #9-#13)

1. **#9, partly signed DDF**: eight «GeneralErrorOperation failed.» alerts, each closed by the FIRST OK method (WM_COMMAND/BN_CLICKED). The clicker works. Collapse, then `AVTaskPaneHostView` hidden: it was 288 px wide at x=366 of a 654 px view, and the document stayed 339 px wide. The black band the operator saw is that hidden pane's area: hiding it does not give its width back.
2. **#10, local temp file**: same hide, same band. The header was found with height 0 and never counted as found; the search was still running when #11 started.
3. **#11, #12**: laid out as expected.
4. **#13, newly generated document**: LoadFile returned True, but the control held ZERO windows for the whole recording. The recording was ended after 4 s as «document open (header not found)» by the header search LEFT RUNNING from #12, not by anything of #13. What happened next was not recorded.

### Operator's order

Keep the old path for future problems. Add a new path: no window checkers except the script-alert clicker, and the toolbars hidden by Adobe's Read Mode (Ctrl+H) sent once the document is laid out, without timers.

### What changed

- New AdobePreviewEngine.ActiveXReadMode, persisted ActiveXCitire in kbot_paths.json (aliases activexreadmode/citire); Motor label «ActiveX (AcroPDF) — mod citire, Ctrl+H». Both old engines unchanged.
- AcroPdfSurface.ReadMode, set by ReaderHostPreview each load: after LoadFile, no pane wait/recreate-reload/collapse/chrome hiding/header timer/resize refit. Save As trap and script-alert clicker remain.
- **Ctrl+H, no timer:** one out-of-context WinEvent hook SHOW…LOCATIONCHANGE from load until send. Control/form events require viewer on screen, visible/sized AVPageView, form enabled (modal-alert release raises an event) and foreground; otherwise Form.Activated rechecks. Focus page; only with focus actually inside control, SendInput Ctrl down/H/Ctrl up, then unhook. First control event wakes Adobe once (focus/size change), never during script alerts.
- Old path fix: a new load stops the previous header search, so it can no longer end the next load's recording.
- Trace records armed/hook handle, changed waiting reason, trigger, focus before/after, pre-Ctrl+H tree and SendInput result. Ends on send, or BLOCKING if focus did not reach the control or SendInput failed.

### Files touched

`AcroPdfSurface.vb`, `AdobeNativeMethods.vb` (IsChild, IsWindowEnabled, GetFocus, SendInput + INPUT structs, STATECHANGE / LOCATIONCHANGE), `AdobeViewerProfile.vb`, `AdobeViewerSettings.vb`, `ReaderHostPreview.vb`, `SetariAplicatieView.vb`, `tests/KBot.Controls.Tests/AdobePreviewEngineSettingTests.vb` (new spellings + round trip).

### Test results

`dotnet build src\KBot.App\KBot.App.vbproj`: 0 errors, 0 warnings. Tests written, not run.

### Unverified

- Nothing run. Whether Ctrl+H reaches Read Mode inside the AcroPDF control on this Acrobat build (24.2), and whether it leaves any band.
- Ctrl+H TOGGLES. If Acrobat keeps Read Mode for the next document loaded into the same control, the next Ctrl+H turns it off. The trace's tree before Ctrl+H will show it.
- Whether the one wake is enough when Adobe postpones its first layout; if `AVPageView` never gets a size, the trace ends on «page view not laid out yet» and nothing is sent.
- Case #13 (control stays empty) is not addressed by the new path: without a timer nothing gives up on it. The recording stays open until the next load, so the trace will show it.

---

## Pass 04 (24.09.2026) -- Ctrl+H waits for the script alerts to be over; empty control replaced

### What the trace showed (`temp_logs\acropdf_trace (2).log`, recordings #1-#6 of 09:42)

- Documents WITHOUT form-script alerts (#2, #6, unsigned): Ctrl+H as soon as `AVPageView` had a size. Operator: this holds every time.
- Documents WITH alerts (#1, #5, eight «GeneralErrorOperation failed.» each, all closed by the clicker): Ctrl+H went out ~0.5 s after the last alert (trigger «form activated»). The tree at that moment was not Adobe's finished layout (no toolbar row above the page, tab strip zero-wide and hidden, collapse strip 0x0). Operator: on these it works only sometimes -- a timing issue.
- An idea from the first reading (Read Mode carrying over to the next document of the same control, «send once per control») was WRONG per the operator and was not kept.
- #3 / #4 (after «Generează documentul»): a new control never received a single window from Adobe in 17 s; destroying and recreating it (the operator's click on another revision) worked at once. Cause unknown; generation itself does not touch Adobe.

### What changed (Read Mode path only; the old path is untouched)

- Ctrl+H also waits until the alert burst is over (`AdobeSaveTrap.InScriptBurst` false, i.e. `ScriptBurstQuietMs` = 3 s without a new alert). New `AdobeSaveTrap.ScriptBurstEnded` event, raised when the trap's sweep drops back to its normal pace; the surface re-checks on it, since Adobe may raise no window event after the last alert. Without alerts nothing changes.
- One one-shot check (`DeadControlMs` = 5 s after the load, cancelled the moment Adobe puts any window in the control, paused while the viewer is off screen): a control still holding no window at all is destroyed, a new one created and the document loaded again, once. Second failure = log + recording ends BLOCKING.
- Trace: «Acrobat External Window» shown OUTSIDE the control during the wait.

### Test results

`dotnet build src\KBot.App\KBot.App.vbproj`: 0 errors, 0 warnings. No tests run.

### Unverified

- Not run. Whether 3 s of quiet is enough for Adobe to finish its layout after the alerts. The trace's tree before Ctrl+H shows whether the toolbar row / tab strip are there by then.
- On documents with alerts the toolbars are now visible for ~3 s before they go.
- Whether the empty-control replacement cures #3 as the manual click did.
---

## Pass 05 (24.09.2026) -- fourth trace; option «control Adobe nou la fiecare document»

### What the trace showed (`temp_logs\acropdf_trace (3).log`)

- #1 (signed, alerts): Ctrl+H waited for the alert burst to end, then held. Operator: much better.
- #2 (generated, loaded into the SAME control): first window 0.3 s after LoadFile, Ctrl+H at 0.9 s.
- #3 (generated, loaded into the same control again): Adobe showed its progress bar (`AVProgressBarView`) 20 ms after LoadFile, then NOTHING; the control held zero windows. The empty-control check replaced it after 5 s; the new control got its first window 3 ms after LoadFile and Ctrl+H went out 0.6 s later. The «several seconds» the operator saw are that 5 s wait, not Adobe being slow.
- So a REUSED control sometimes stays empty after LoadFile; every fresh control in the four traces built at once except one (trace 2 #3, created after a live control was destroyed).

### What changed

- `AppSettings.AcroPdfFreshControl` (JSON `AcroPdfFreshControl`, default False, saved).
- Setări ▸ Aplicație ▸ Documente: checkbox «ActiveX — control Adobe nou la fiecare document» on a new row 3; the Excel row moved to row 4.
- `ReaderHostPreview.ShowDocument` (DDF and ORD share it): with the option on and an ActiveX engine, the previous AcroPDF control is destroyed at the click (`AcroPdfSurface.Clear`), also when the new revision has no document; the load creates a new control. Both ActiveX paths.
- The 5 s empty-control replacement stays as the safety net.

### Test results

`dotnet build src\KBot.App\KBot.App.vbproj`: 0 errors, 0 warnings. No tests run.

### Unverified

- Not run, the new row not seen on screen. Whether a control per document is always as fast as the traces suggest, and whether it avoids the empty control (trace 2 #3 was a fresh control).
## Pass 06 — only listed script alerts are closed (24.09.2026)

### Why

The trap pressed OK on EVERY «Warning: JavaScript Window» box. The forms also use that box to tell the operator something («Validarea s-a terminat cu succes! Semnati formularul...», «Nu sunt erori la sectiunea A»), and the operator never saw those. The operator asked for the decision to be made on the message text (the `Static` child of the alert, Window Detective 0x00300B72) against a list they can edit in Setări, regular expressions allowed.

### What changed

- `AppSettings.AdobeTrappedAlerts` (JSON list, missing = defaults, empty = nothing trapped). Defaults = the texts the working logs show were closed as errors: `GeneralError`, `Operation failed`, `TypeError`.
- `KBot.Controls/Adobe/AdobeScriptAlertFilter.vb` (new): case-insensitive regex, matched anywhere, whitespace collapsed; broken / blank / match-everything patterns refused (`CheckPattern`) and skipped (`Compile`); 200 ms match timeout; compiled list cached per saved settings.
- `AdobeSaveTrap.DismissScriptNoise`: an alert (not the console) whose Static text matches no pattern is LEFT ON SCREEN, reported once («lăsat operatorului»), judged again on every sweep (text not readable yet). It still counts as a script burst, so Ctrl+H waits until 3 s after the operator closes it.
- `AcroPdfSurface.OnDeadCheckTick`: the 5 s empty-control check is postponed while a script alert is up (Adobe builds nothing while one is showing; the one left for the operator can stay up for long) instead of recreating the control under it.
- Setări ▸ Aplicație ▸ Documente: button «Mesaje de script Adobe…» (row 4, Excel row moved to 5) opens `AdobeMesajeForm`: one pattern per line, a live test box, «Lista implicită», save refuses broken lines with their numbers and asks before saving an empty list.
- Tests written (not run): `tests/KBot.Controls.Tests/AdobeScriptAlertFilterTests.vb`.

### Test results

`dotnet build src\KBot.App\KBot.App.vbproj`: 0 errors, 0 warnings. No tests run.

### Unverified

- Not run; the dialog and the new row not seen on screen. The alert text is taken from the `Static` children as before; an alert whose text is drawn some other way reads as empty and is left on screen.

---

## SLICE-0078-16 — Jurnalul ActiveX pe culoare (lane-uri) <a id="slice-0078-16"></a>

Operator request, 07.10.2026: in the ActiveX log viewer, a button that takes the loaded log and opens a new form with the lane control (`KBot.Controls/Lane`); no date, only time and milliseconds; each root node of the left tree becomes a lane, each leaf a change in that lane.

Slice number: **0078-16 is an assumption** (continuation of the 0078-15 viewer; the operator gave none).

### 1. What changed and why
- `ActivexLogViewerForm`: new button `btnLanes` («Pe culoare…») in the top bar. `LaneSpecsOf(tvLeft)` reads the LEFT tree as it is now (so «Fără rândurile arborilor de ferestre» and «Ascunde ce e identic» apply): root → `ActivexLaneSpec` (caption = kind name), every leaf under any element of the root → one marker. Markers are put in log order (`ActivexLogLine.Index`).
- `ActivexLaneForm` (new, DEBUG-only, `HarnessTests/`): one `KBotLaneView`, captions on, axis on, `MomentFormat = "HH:mm:ss.fff"`. The marker's moment is `2000-01-01 + time of day` parsed from the log line's stamp (`hh\:mm\:ss\.fff`); the fixed day is never shown. Markers of one element share a colour (`AutoColor`), so the stretch each marker owns reads as «this element holds until the next change». Lanes are not drop targets. The tooltip of a marker holds the whole log line (cut at 600 characters).
- Not modal, like the viewer. A line with no parseable stamp is left out and counted in the status line.

### 2. Files touched
`src/KBot.App/HarnessTests/ActivexLaneForm.vb` + `.Designer.vb` (new); `src/KBot.App/HarnessTests/ActivexLogViewerForm.vb` + `.Designer.vb`; `docs/worklog/KBOT_STATUS.md`, `docs/worklog/state/KBOT_STATUS_0070-0079.md` (0078-16 rows)

### 3. Test results
- `dotnet build src\KBot.App\KBot.App.vbproj -c Debug`: 0 warnings, 0 errors.
- No tests written or run (standing rule). Never opened on screen.

### 4. Unverified / deferred
- How it looks and behaves with real data is unseen: hundreds of markers in the Handle/Pid lanes, tooltip look, colours per element.
- A log that runs past midnight folds back onto the start of the axis (the date is deliberately not used).
- Only the left tree is used; the right side could be added as extra lanes if wanted.
- Help: none — DEBUG harness, never seen by the operator.
