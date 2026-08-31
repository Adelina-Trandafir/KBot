# Adobe viewer family — AdobeReaderHost + AcroPdfSurface

Two ways to show a PDF inside a K-BOT panel. Neither is a designer control: both take a
host `Control` (or `IHostSurface`) and a `log` callback, and both are `IDisposable`.

`Adobe/` — 27 files. Conventions: [C1..C9](../CONTROLS.md).
Status: logic unit-tested (`Adobe*Tests` + `FakeNativeWindows`); the real hosting path only
proves itself on a machine with Adobe installed. Harness: `AdobeReaderSwitchesTest`.
**Debug the real path from `bin\…\Logs\adobe_preview.log`, not from theory** — Adobe
ignores `/n` and hands documents to an already-running instance, so windows must be matched
by TITLE, not by PID.

## AdobeReaderHost — reparent the real Adobe window
Launches Adobe, finds its top-level window and makes it a child of the host panel.
- `New(hostPanel, log)` or `New(host As IHostSurface, log, …)`
- `ShowDocumentAsync(pdfPath) As Task(Of AdobeHostResult)`
- `Relayout()`, `ReapplyProfile()`, `Detach()`, `Dispose()`
- `Mode: AdobeViewerMode = Auto`, `NewInstanceMode: AdobeNewInstanceMode = Auto`,
  `PopupWatchEnabled = False`, `Options: AdobeHostOptions`
- `CurrentChoice`, `LastDetection`, `HostedWindow`, `HostedPid`, `IsHosting` (read-only)
- `AdobeReaderHost.ResolveAdobePath()` (Shared)
- `AdobeHostStatus` = `Hosted` `AdobeMissing` `LaunchFailed` `WindowNotFound` `Superseded`
  `Failed`; `AdobeHostResult` carries `Status`, `Message` (Romanian, operator-ready, empty
  on success), `Choice`, `ElapsedMs`, `Match`, `Succeeded`.
- `AdobeHostOptions`: `DetachMode` (`AdobeDetachMode`, default `KillProcess`),
  `UseCreationHook = False`, `CaptureDelayMs = 0`, `FindTimeoutMs`, `FindPollMs = 30`,
  `RedrawDelayMs`, `CloseGraceMs = 1500`, `ExtraArgs`, `Clone()`, `Describe()`.

## AcroPdfSurface — the AcroPDF ActiveX
- `New(hostPanel, log)`, `IsAvailable`, `TryReadVersion()`,
  `ShowDocumentAsync(pdfPath) As Task(Of AcroPdfResult)`, `Clear()`, `Dispose()`
- `AcroPdfStatus` = `Shown` `NotRegistered` `FileMissing` `Failed`; `AcroPdfResult` adds
  `Collapsed` (True when the document view ended up filling the panel).
- `AcroPdfDetector.ResolveClsid()` / `NormaliseClsid(clsid)`; `AcroPdfHost` is the low-level
  wrapper (`LoadFile`, `ApplyChrome`, `Clear`, `TryReadVersion`).

## Profiles and UI detection
Adobe's classic and modern UIs need different window offsets and clipping.
- `AdobeUiDetector.Detect(nodes)` → `AdobeUiDetection` (`Generation`, `Evidence`,
  `Ambiguous`, `Describe()`), fed by `AdobeWindowProbe.Walk(...)` / `RhpCandidate(...)`.
- `AdobeViewerProfile`: `NewInstance`, `NoSplash`, `OpenParameters`, `ClipEnabled`,
  `ClipRight`, `ClipTop`, `Dx/Dy/Dw/Dh`, `HidePopups`, `WithNewInstance(mode)`.
- `AdobeViewerProfiles.Modern` / `.Classic` / `.For(generation)` /
  `.Resolve(mode, detection)` → `AdobeProfileChoice` (`Profile`, `Mode`, `Detected`,
  `Mismatch`).
- `AdobeViewerMode` (Auto / forced), `AdobePreviewEngine` (which of the two surfaces),
  `AdobeNewInstanceMode`. Persisted through `AdobeViewerSettings`
  (`Parse*` / `*ToText` / `*Label` / `Current*` / `Persist`), each read returning an
  `AdobeSettingRead(Of T)` with a `Warning` for an unreadable stored value.

## Supporting pieces
- `AdobeWindowHosting` (Shared): `BuildArguments`, `FindReaderWindow`,
  `IsAdobeWindowClass`, `AdobeProcessIds`, `AttachAsChild`, `Place`, `NudgeRedraw`,
  `RectInParent`, `Hide`, `Show`, `ClickCentre`, `OwnerPid`, `IsAlive`, `IsVisible`,
  `FocusWindow`, `KillPid`.
- `AdobeWindowCapture` — `Find(launchedPid, baseName, …)` → `AdobeCaptureResult`
  (`AdobeCaptureMatch`; `ByTitle` means a FOREIGN instance was adopted), `ChildStyle`,
  `AttachAsChild`, `Reveal`.
- `AdobeHostGeometry.Compute(hostSize, profile)`, `HostedWindowGeometry.Offset/IsNeutral`,
  `MoveOutcome` + classifier; `HideOutcome` + `HideOutcomeClassifier` +
  `HideAttemptSummary`.
- `AdobePopupWatcher` + `AdobePopupFilter.Evaluate(className, ownerPid, …)` →
  `AdobePopupVerdict` — hides Adobe's own dialogs while hosted.
- `AdobeCreationHook` — WinEvent hook that hides windows as they are created
  (`Install(pid)`, `Remove`, `HiddenCount`).
- `AdobeWindowTeardown.Run(...)` → `AdobeTeardownOutcome` (`AdobeTeardownAction`).
- Registry: `IRegistryAccess` / `WinRegistryAccess`, `RegistrySnapshotSet`
  (`Capture` / `Snapshots` / `RestoreAll`), `RegistryValueSnapshot` (+ `RegPresence`),
  `AdobeRegistryConstants`, `AdobeHiveResolver.Resolve(...)` → `AdobeHiveResolution`
  (which HKCU hive: Reader vs Acrobat).
- Seams for testing: `INativeWindows` / `Win32Windows`, `IAdobeLauncher` /
  `ProcessAdobeLauncher`, `IHostSurface` / `ControlHostSurface`, `AdobeNativeMethods`,
  `AdobeHostLog.Write`.

## Limits
- Needs Adobe Reader/Acrobat installed (`AdobeMissing`) or the AcroPDF ActiveX registered
  (`NotRegistered`). No fallback renderer ships here.
- Reparenting a foreign process's window is inherently fragile: an Adobe update that changes
  window classes, titles or chrome breaks the profile offsets. Profiles exist precisely
  because two generations already needed different numbers.
- `AdobeCaptureMatch.ByTitle` = the window was adopted, not created by us. Detaching it may
  affect a document the operator opened themselves; `DetachMode` decides.
- Registry writes (chrome/pane collapsing) are per-user HKCU and are captured/restored
  through `RegistrySnapshotSet` — a crash between capture and restore leaves them changed.
- Async: `ShowDocumentAsync` can return `Superseded` when a newer document was requested
  while it waited. Do not assume the last call won.
- **Never sign a PDF while a `ReaderHostPreview` holds a window on it** (DDF slice rule).
- Nothing here is themed: the hosted surface is Adobe's own UI.
