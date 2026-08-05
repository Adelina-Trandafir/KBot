# SLICE-0024 (pass 01) — one Adobe hosting implementation, shared by the bench and the app

Slice **0024** («DDF, fila PDF: profiluri Adobe cu comutare la rulare»), first pass.
Number taken from `KBOT_STATUS.md` («Next free slice number: 0024»), now bumped to 0025.

## §0 What this pass is

The brief's §2: *do not reimplement the harness primitives*. Clip, move, child hiding, the probe and
the window plumbing existed **twice** — in `AdobeReaderHarnessForm` (KBot.DevHarness) and in
`ReaderHostPreview` (KBot.App). This pass extracts them into `KBot.Controls\Adobe\`, moves the
harness onto the extracted code, and adds the two measured profiles plus the setting store they
read from. Pass 02 wires the shipping preview and the UI.

**A correction to the brief, reported rather than silently worked around** (CODE_WORKFLOW §4):

> §2 lists «popup watching» among the primitives that «already exist in `AdobeReaderHarnessForm`»,
> and §8 says to «reuse the harness tests» for the popup filter. **Neither exists.** `AVL_AVPopup`
> appears nowhere in the repository — not in the harness, not in its scenarios, not in the 75 KB
> `test_adobe_rhp.log`, not in any 0023 worklog. The watcher was therefore **written from scratch**
> here, and its tests are new. The class name `AVL_AVPopup` and the four filter criteria come from
> the brief (operator observation); nothing in the repo corroborates them, so that is recorded as
> unverified below.

## §1 What moved, and why it had to

`src\KBot.Controls\Adobe\` — new folder, nine files:

| file | what it holds |
|---|---|
| `AdobeNativeMethods.vb` | every P/Invoke, once. `Friend` — nothing outside KBot.Controls calls user32 directly |
| `AdobeHostGeometry.vb` | `HostedWindowGeometry` (**moved**, unchanged) + `AdobeHostGeometry.Compute` = the extracted body of the harness's `HostedBounds()` + `MoveOutcome` / `MoveOutcomeClassifier` (**moved**) |
| `AdobeHideOutcome.vb` | `HideOutcome` / `HideOutcomeClassifier` / `HideAttemptSummary` (**moved**, unchanged) |
| `AdobeWindowProbe.vb` | `AdobeWindowNode` + the depth-limited walk + the RHP-candidate heuristic |
| `AdobeUiDetector.vb` | `AdobeUiGeneration` + the pure detection rule |
| `AdobeViewerProfile.vb` | the two **measured** profiles + `AdobeViewerMode` / `AdobeNewInstanceMode` + `Resolve` |
| `AdobeViewerSettings.vb` | text ↔ enum, with the fallback rule |
| `AdobePopupFilter.vb` / `AdobePopupWatcher.vb` | the badge filter (pure) and the sweeper (timer) |
| `AdobeWindowHosting.vb` | Adobe lookup, command line, window find, reparent, place, nudge, hide/show |
| `AdobeReaderHost.vb` | the engine pass 02 drives; the bench does **not** use it (see §3) |
| `AdobeHostLog.vb` | `<AppDir>\Logs\adobe_preview.log` — decisions, not exceptions |

**The two copies had already drifted, which is the evidence that §2 is right.** The harness stripped
`WS_SYSMENU` / `WS_MINIMIZEBOX` / `WS_MAXIMIZEBOX` and called `NudgeRedraw`; `ReaderHostPreview` did
neither. The same PDF therefore behaved differently in the bench and in the app, and the app's copy
carried the older, worse behaviour — the reparented window can stay **invisible** until an unrelated
layout change without the nudge. One implementation now; the harness's stricter version won.

## §2 The two profiles are transcriptions, not proposals

`AdobeViewerProfiles.Modern` / `.Classic` carry exactly the numbers of §1 of the brief. They were
cross-checked against the **actual saved bench states**, which were still on disk in
`src\KBot.App\bin\Debug\net8.0-windows\Config\` — `scenariu_adobe_modern_full.json` (04.08.2026
20:06:04) and `scenariu_adobe_oldschool.json` (20:10:29). Both agree with the brief in every field.

Both files are now kept **byte-verbatim** as test fixtures under
`tests\KBot.DevHarness.Tests\Fixtures\`, because they are the only evidence behind the numbers.

Carried across as written, and flagged rather than "fixed":

* **Modern has `newInstance = false`.** Adobe may hand the document to an instance the operator
  opened themselves. `AdobeReaderHost` logs loudly when the embedded window's PID is not the PID it
  started, and (pass 02) refuses to close a process it did not create. See the open threads.
* **Modern carries no open parameters at all.** The classic ones were not "helpfully" added; a test
  asserts the `/A` payload is empty.

## §3 What the harness now shares — and what it deliberately does not

The bench calls `AdobeWindowHosting` / `AdobeWindowProbe` / `AdobeHostGeometry` /
`MoveOutcomeClassifier` / `HideOutcomeClassifier`. Its own `WalkChildren`, `ChildWindowItem`,
`FindReaderWindow`, `ResolveAdobePath`, `ReadAppPath`, `KillPid`, `BuildArguments` body,
`ChildRectInParent` body, `HostWindow` body, `LayoutHostedWindow` body, `NudgeRedraw` body and the
**entire 145-line Win32 interop block** are gone. `MoveOutcome.vb` and `HideOutcome.vb` were deleted
from `src\KBot.DevHarness\Internal\Adobe\`.

It does **not** use `AdobeReaderHost` (the orchestrator). That was a deliberate call: the bench
drives clip and move live from spinners, runs `launch` and `waitForEmbed` as separate scenario steps,
force-kills Adobe by PID and keeps a generation counter — none of which the preview wants. Folding
those into the shared engine would have made the shared engine worse and the bench's behaviour
harder to keep identical. The primitives are shared; the orchestration is not, and there is no
duplicated primitive left to drift.

**One behaviour was added to the bench, and only one:** `ProbeChildren` now also logs
`Mod detectat: …`. It changes nothing it does — it is a log line, and it is the line that will let
new Adobe builds be diagnosed on the bench.

The popup watcher is **off by default** (`AdobeReaderHost.PopupWatchEnabled = False`) precisely so
the bench keeps behaving exactly as it did; pass 02 turns it on for the preview only.

## §4 The setting store — which mechanism, and why

`KBotPaths` (`<AppDir>\kbot_paths.json`, `KBot.Common`), extended with `AdobeViewerMode` and
`AdobeNewInstance` plus a `Save()`.

The repo has exactly two candidates. `ThemeStore` (`%AppData%\AVACONT\theme.json`, KBot.Theming) is
per-**user** and already does runtime save+load; `KBotPaths` is per-**installation** and was
read-only. `KBotPaths` won because the Adobe mode describes **which Adobe is installed on this
machine**, not a preference of the person: a roaming store would follow the operator to a machine
with a different Adobe and be wrong there. No new file, no new format — the same JSON, loaded the
same way, with the same «missing or broken → defaults + log, never throw» rule.

Two details worth keeping:

* Values are stored as **text**, not as the enum. The enums live in KBot.Controls, which references
  KBot.Common; the reverse would be a cycle. `KBotPaths` keeps whatever string it finds and
  `AdobeViewerSettings` decides what it means — which is also why an invalid value survives the load
  and is rejected (with a warning) exactly where the vocabulary is known.
* `Save(dir)` with an **explicit** directory does not touch the `Current` singleton. Without that, a
  test writing to a temp folder would silently repoint the whole process.

## §5 Detection reads the window tree, never the registry

`AdobeUiDetector` is pure and takes a list of `AdobeWindowNode`. Rule order is the brief's:
`AVTaskPaneHostView` → Classic, `AV2DocumentTabView` / `AV2DockableTabStripView` → Modern, else
Unknown → Classic (the conservative fallback: the classic profile neither clips nor moves, so a
wrong guess shows too much chrome rather than a blank rectangle).

Two additions, both deliberate:

* markers are matched against a node's **text OR class**. Every probe the bench recorded has them as
  window TEXT on `AVL_AVView` children (`test_adobe_rhp.log` shows 9 occurrences each of
  `AV2DocumentTabView` and `AV2DockableTabStripView`); a future Adobe could make them class names,
  and matching both costs nothing.
* a tree carrying markers of **both** generations still resolves by first-match (Classic), but the
  result is flagged `Ambiguous` and the log says so. Silently picking one would be the exact kind of
  invisible decision slice 0023 spent four passes removing.

`bEnableAv2` is not read anywhere in the detection path.

## Files touched

**New** — `src\KBot.Controls\Adobe\{AdobeNativeMethods, AdobeHostGeometry, AdobeHideOutcome,
AdobeWindowProbe, AdobeUiDetector, AdobeViewerProfile, AdobeViewerSettings, AdobePopupFilter,
AdobePopupWatcher, AdobeWindowHosting, AdobeReaderHost, AdobeHostLog}.vb`

**Deleted** — `src\KBot.DevHarness\Internal\Adobe\{MoveOutcome, HideOutcome}.vb`

**Modified** — `src\KBot.Common\KBotPaths.vb` (two settings + `Save`),
`src\KBot.DevHarness\Internal\AdobeReaderHarnessForm.vb` (refactored onto the shared code; −139
lines net), `src\KBot.DevHarness\Internal\Adobe\HarnessScenario.vb` (+`Imports KBot.Controls`),
`tests\KBot.DevHarness.Tests\{MoveWindowTests, HarnessTruthTests}.vb` (+`Imports KBot.Controls`,
assertions untouched), `KBot.DevHarness.Tests.vbproj` (Fixtures copy rule).

**New tests** — `tests\KBot.Controls.Tests\{AdobeViewerProfileTests, AdobeUiDetectorTests,
AdobePopupFilterTests, AdobeViewerSettingsTests}.vb`,
`tests\KBot.DevHarness.Tests\BenchStateRegressionTests.vb`,
`tests\KBot.DevHarness.Tests\Fixtures\bench_state_{modern_20260804_2006, classic_20260804_2010}.json`

**Versions** — `KBot.Common` 1.1.0.0 → 1.2.0.0, `KBot.Controls` 1.0.0.0 → 1.1.0.0,
`KBot.DevHarness` 1.0.6.0 → 1.0.7.0.

## Test results

* `dotnet build KBot.sln` — **0 errors, 0 BC warnings** (16 pre-existing NU1701 from iTextSharp /
  BouncyCastle, unchanged).
* `dotnet test KBot.sln` — **all green**. `KBot.Controls.Tests` 134 → 201 (+67),
  `KBot.DevHarness.Tests` 144 → 153 (+9).
* **The harness regression the brief asks for passes**: both saved bench states still parse, and
  feeding each through `AdobeHostGeometry.Compute` produces the same rectangle as before the
  extraction — `(-130,-152) 1230×952` for modern and `(0,0) 1000×800` for classic on a 1000×800
  host — and the shipped profiles produce those same rectangles.

## Anything left unverified or deferred

* **Nothing here has run against a real Adobe.** The extraction is proven by tests over pure
  arithmetic, pure detection and pure filtering; `SetParent`, `AttachAsChild`, `NudgeRedraw` and the
  popup sweep have not been executed once in this pass.
* **The popup watcher has never seen a popup.** `AVL_AVPopup`, the size bounds and the intersection
  rule come from the brief; the repo contains no probe log, scenario or worklog mentioning that
  class. If the class name is wrong, the filter rejects everything and says so in the log — but that
  is a hypothesis, not a verified behaviour.
* **The bench has not been re-run on screen after the refactor.** Its 153 tests pass and it compiles,
  but the brief's «verify by re-running the saved bench states» was done **arithmetically** (the
  regression test), not by loading the two files into the bench with Adobe open.
* `AdobeReaderHost` is fully written but **not called by anything yet** — pass 02 mounts it. Its
  single «relaunch once when the detected profile needs different launch flags» path is therefore
  entirely untested at run time.
* `AdobeViewerProfiles.Resolve` treats an **Unknown** detection as «no mismatch» for a forced mode.
  That is a judgement: an Adobe we cannot classify is not evidence the operator chose wrong, and a
  warning there would train them to ignore warnings.
