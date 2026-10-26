# SLICE-0024 — Gazda Adobe: extracție, profiluri și probe

Consolidare D001-14: etapele 01…03 sunt reunite cu corpurile, fișierele, probele și limitele păstrate. Comenzile/regulile sunt istorice, nu se execută în această lucrare documentară.

- [Etapa 01](#slice-0024-01)
- [Etapa 02](#slice-0024-02)
- [Etapa 03](#slice-0024-03)

## Repere pentru citirea reviziilor

- 01/02 nu aveau verdict vizual; 03 adaugă observații reale. Statusul inițial al registrului «NEVER RUN AGAINST A REAL ADOBE / NO VISUAL VERDICT» rămâne un checkpoint, nu rezumatul tuturor observațiilor ulterioare.
- La 03, cerința finală este titlul documentului obligatoriu, PID doar preferință. Pasajele inițiale despre PID strict/cădere numai fără /n și testul care respingea PID străin sunt variante corectate în §1, nu contractul final.
- Ascunderea benzilor prin ShowWindow nu le colapsează și nu reface layoutul; concluzia inițială din diff este infirmată de proba ulterioară. Clickul pe butonul Adobe a funcționat și starea a persistat în proces.
- «WakeAcroLayout nu a rulat» de la finalul 03 este un reper anterior măsurătorilor din corp: cele două probe au identificat schimbarea de dimensiune drept pasul eficient. Constatarea este păstrată; nu este extinsă la alte versiuni/mărimi.
- Rezultatele automate, randarea formularului, capturarea ferestrei și observațiile ActiveX sunt probe diferite; acceptarea AcroPDF nu certifică migrarea aplicației pe el.

## SLICE-0024 (pass 01) — one Adobe hosting implementation, shared by the bench and the app <a id="slice-0024-01"></a>

Slice **0024** («DDF, fila PDF: profiluri Adobe cu comutare la rulare»), first pass. Number taken from `KBOT_STATUS.md` («Next free slice number: 0024»), now bumped to 0025.

### §0 What this pass is

The brief's §2: *do not reimplement the harness primitives*. Clip, move, child hiding, the probe and the window plumbing existed **twice** — in `AdobeReaderHarnessForm` (KBot.DevHarness) and in `ReaderHostPreview` (KBot.App). This pass extracts them into `KBot.Controls\Adobe\`, moves the harness onto the extracted code, and adds the two measured profiles plus the setting store they read from. Pass 02 wires the shipping preview and the UI.

**A correction to the brief, reported rather than silently worked around** (CODE_WORKFLOW §4):

> §2 lists «popup watching» among the primitives that «already exist in `AdobeReaderHarnessForm`»,
> and §8 says to «reuse the harness tests» for the popup filter. **Neither exists.** `AVL_AVPopup`
> appears nowhere in the repository — not in the harness, not in its scenarios, not in the 75 KB
> `test_adobe_rhp.log`, not in any 0023 worklog. The watcher was therefore **written from scratch**
> here, and its tests are new. The class name `AVL_AVPopup` and the four filter criteria come from
> the brief (operator observation); nothing in the repo corroborates them, so that is recorded as
> unverified below.

### §1 What moved, and why it had to

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
`WS_SYSMENU` / `WS_MINIMIZEBOX` / `WS_MAXIMIZEBOX` and called `NudgeRedraw`; `ReaderHostPreview` did neither. The same PDF therefore behaved differently in the bench and in the app, and the app's copy carried the older, worse behaviour — the reparented window can stay **invisible** until an unrelated layout change without the nudge. One implementation now; the harness's stricter version won.

### §2 The two profiles are transcriptions, not proposals

`AdobeViewerProfiles.Modern` / `.Classic` carry exactly the numbers of §1 of the brief. They were cross-checked against the **actual saved bench states**, which were still on disk in `src\KBot.App\bin\Debug\net8.0-windows\Config\` — `scenariu_adobe_modern_full.json` (04.08.2026 20:06:04) and `scenariu_adobe_oldschool.json` (20:10:29). Both agree with the brief in every field.

Both files are now kept **byte-verbatim** as test fixtures under `tests\KBot.DevHarness.Tests\Fixtures\`, because they are the only evidence behind the numbers.

Carried across as written, and flagged rather than "fixed":

* **Modern has `newInstance = false`.** Adobe may hand the document to an instance the operator opened themselves. `AdobeReaderHost` logs loudly when the embedded window's PID is not the PID it started, and (pass 02) refuses to close a process it did not create. See the open threads.
* **Modern carries no open parameters at all.** The classic ones were not "helpfully" added; a test asserts the `/A` payload is empty.

### §3 What the harness now shares — and what it deliberately does not

The bench calls `AdobeWindowHosting` / `AdobeWindowProbe` / `AdobeHostGeometry` / `MoveOutcomeClassifier` / `HideOutcomeClassifier`. Its own `WalkChildren`, `ChildWindowItem`, `FindReaderWindow`, `ResolveAdobePath`, `ReadAppPath`, `KillPid`, `BuildArguments` body, `ChildRectInParent` body, `HostWindow` body, `LayoutHostedWindow` body, `NudgeRedraw` body and the
**entire 145-line Win32 interop block** are gone. `MoveOutcome.vb` and `HideOutcome.vb` were deleted
from `src\KBot.DevHarness\Internal\Adobe\`.

It does **not** use `AdobeReaderHost` (the orchestrator). That was a deliberate call: the bench drives clip and move live from spinners, runs `launch` and `waitForEmbed` as separate scenario steps, force-kills Adobe by PID and keeps a generation counter — none of which the preview wants. Folding those into the shared engine would have made the shared engine worse and the bench's behaviour harder to keep identical. The primitives are shared; the orchestration is not, and there is no duplicated primitive left to drift.

**One behaviour was added to the bench, and only one:** `ProbeChildren` now also logs
`Mod detectat: …`. It changes nothing it does — it is a log line, and it is the line that will let new Adobe builds be diagnosed on the bench.

The popup watcher is **off by default** (`AdobeReaderHost.PopupWatchEnabled = False`) precisely so the bench keeps behaving exactly as it did; pass 02 turns it on for the preview only.

### §4 The setting store — which mechanism, and why

`KBotPaths` (`<AppDir>\kbot_paths.json`, `KBot.Common`), extended with `AdobeViewerMode` and `AdobeNewInstance` plus a `Save()`.

The repo has exactly two candidates. `ThemeStore` (`%AppData%\AVACONT\theme.json`, KBot.Theming) is per-**user** and already does runtime save+load; `KBotPaths` is per-**installation** and was read-only. `KBotPaths` won because the Adobe mode describes **which Adobe is installed on this machine**, not a preference of the person: a roaming store would follow the operator to a machine with a different Adobe and be wrong there. No new file, no new format — the same JSON, loaded the same way, with the same «missing or broken → defaults + log, never throw» rule.

Two details worth keeping:

* Values are stored as **text**, not as the enum. The enums live in KBot.Controls, which references KBot.Common; the reverse would be a cycle. `KBotPaths` keeps whatever string it finds and `AdobeViewerSettings` decides what it means — which is also why an invalid value survives the load and is rejected (with a warning) exactly where the vocabulary is known.
* `Save(dir)` with an **explicit** directory does not touch the `Current` singleton. Without that, a test writing to a temp folder would silently repoint the whole process.

### §5 Detection reads the window tree, never the registry

`AdobeUiDetector` is pure and takes a list of `AdobeWindowNode`. Rule order is the brief's: `AVTaskPaneHostView` → Classic, `AV2DocumentTabView` / `AV2DockableTabStripView` → Modern, else Unknown → Classic (the conservative fallback: the classic profile neither clips nor moves, so a wrong guess shows too much chrome rather than a blank rectangle).

Two additions, both deliberate:

* markers are matched against a node's **text OR class**. Every probe the bench recorded has them as window TEXT on `AVL_AVView` children (`test_adobe_rhp.log` shows 9 occurrences each of `AV2DocumentTabView` and `AV2DockableTabStripView`); a future Adobe could make them class names, and matching both costs nothing.
* a tree carrying markers of **both** generations still resolves by first-match (Classic), but the result is flagged `Ambiguous` and the log says so. Silently picking one would be the exact kind of invisible decision slice 0023 spent four passes removing.

`bEnableAv2` is not read anywhere in the detection path.

### Files touched

**New** — `src\KBot.Controls\Adobe\{AdobeNativeMethods, AdobeHostGeometry, AdobeHideOutcome,
AdobeWindowProbe, AdobeUiDetector, AdobeViewerProfile, AdobeViewerSettings, AdobePopupFilter, AdobePopupWatcher, AdobeWindowHosting, AdobeReaderHost, AdobeHostLog}.vb`

**Deleted** — `src\KBot.DevHarness\Internal\Adobe\{MoveOutcome, HideOutcome}.vb`

**Modified** — `src\KBot.Common\KBotPaths.vb` (two settings + `Save`),
`src\KBot.DevHarness\Internal\AdobeReaderHarnessForm.vb` (refactored onto the shared code; −139 lines net), `src\KBot.DevHarness\Internal\Adobe\HarnessScenario.vb` (+`Imports KBot.Controls`), `tests\KBot.DevHarness.Tests\{MoveWindowTests, HarnessTruthTests}.vb` (+`Imports KBot.Controls`, assertions untouched), `KBot.DevHarness.Tests.vbproj` (Fixtures copy rule).

**New tests** — `tests\KBot.Controls.Tests\{AdobeViewerProfileTests, AdobeUiDetectorTests,
AdobePopupFilterTests, AdobeViewerSettingsTests}.vb`, `tests\KBot.DevHarness.Tests\BenchStateRegressionTests.vb`, `tests\KBot.DevHarness.Tests\Fixtures\bench_state_{modern_20260804_2006, classic_20260804_2010}.json`

**Versions** — `KBot.Common` 1.1.0.0 → 1.2.0.0, `KBot.Controls` 1.0.0.0 → 1.1.0.0,
`KBot.DevHarness` 1.0.6.0 → 1.0.7.0.

### Test results

* `dotnet build KBot.sln` — **0 errors, 0 BC warnings** (16 pre-existing NU1701 from iTextSharp / BouncyCastle, unchanged).
* `dotnet test KBot.sln` — **all green**. `KBot.Controls.Tests` 134 → 201 (+67), `KBot.DevHarness.Tests` 144 → 153 (+9).
* **The harness regression the brief asks for passes**: both saved bench states still parse, and feeding each through `AdobeHostGeometry.Compute` produces the same rectangle as before the extraction — `(-130,-152) 1230×952` for modern and `(0,0) 1000×800` for classic on a 1000×800 host — and the shipped profiles produce those same rectangles.

### Anything left unverified or deferred

* **Nothing here has run against a real Adobe.** The extraction is proven by tests over pure arithmetic, pure detection and pure filtering; `SetParent`, `AttachAsChild`, `NudgeRedraw` and the popup sweep have not been executed once in this pass.
* **The popup watcher has never seen a popup.** `AVL_AVPopup`, the size bounds and the intersection rule come from the brief; the repo contains no probe log, scenario or worklog mentioning that class. If the class name is wrong, the filter rejects everything and says so in the log — but that is a hypothesis, not a verified behaviour.
* **The bench has not been re-run on screen after the refactor.** Its 153 tests pass and it compiles, but the brief's «verify by re-running the saved bench states» was done **arithmetically** (the regression test), not by loading the two files into the bench with Adobe open.
* `AdobeReaderHost` is fully written but **not called by anything yet** — pass 02 mounts it. Its single «relaunch once when the detected profile needs different launch flags» path is therefore entirely untested at run time.
* `AdobeViewerProfiles.Resolve` treats an **Unknown** detection as «no mismatch» for a forced mode. That is a judgement: an Adobe we cannot classify is not evidence the operator chose wrong, and a warning there would train them to ignore warnings.

## SLICE-0024 (pass 02) — the profiles reach the DDF «Document» tab, switchable while running <a id="slice-0024-02"></a>

Continues slice **0024**. Pass 01 ([SLICE-0024-01-adobe-shared-host.md](#slice-0024-01)) stands as written.

### §0 What this pass ships

`ReaderHostPreview` rewritten onto `AdobeReaderHost`, the two runtime settings on the PDF tab of `DdfView`, and `docs\SETARI_UTILIZATOR.md`.

`DdfPreviewFactory` and its compile-time `Mode` constant are **untouched**. That switch chooses which preview *implementation* the «Vizualizare» page uses; this slice's setting chooses how the Adobe host *behaves*. They are not merged and are not related — note that the «Document» page has mounted `ReaderHostPreview` unconditionally since 0020-04, whatever the factory constant says, so this setting matters on every build.

### §1 `ReaderHostPreview` — what is left of it

Everything Win32 is gone (pass 01). What remains is UI: three states, the theme, the generate button, and a fourth element — `lblNote`, a discreet bottom strip that appears **only** when the Adobe version was not recognised, carrying `AdobeUiDetector.UnrecognisedNote` in Romanian.

Behaviour changes worth naming:

* **Embedding is asynchronous now.** It used to block the UI thread for up to 8 seconds polling `EnumWindows`. `ShowDocument` stays synchronous (the `IDdfPreview` contract), starts `EmbedAsync` fire-and-forget and guards against a stale response with `_requestedPath` — the same pattern as every view's `LoadAsync`.
* **The process is not always closed.** The old code called `CloseMainWindow()` on whatever `Process.Start` returned. Under the modern profile (`/n` off) Adobe may hand the document to the operator's own instance, and closing that would take their other documents with it. `AdobeReaderHost.Detach` now closes the process **only** when K-BOT started it *and* it owns the hosted window, and logs the refusal otherwise.
* **Every failure path produces a Romanian sentence on screen** (brief §6): Adobe not installed, launch failed, embed timeout, file missing, watcher failed to attach. Nothing is swallowed — the full detail goes to `<AppDir>\Logs\adobe_preview.log`, exceptions to `harness_errors.log`.
* **The popup watcher is on here and only here** (`PopupWatchEnabled = True`), so the bench's behaviour stays byte-identical to before the extraction.

### §2 The setting on screen

`pnlAdobe`, a `DockStyle.Top` band inside `pnlPdf`, declared in `DdfView.Designer.vb` like every other control (house rule), carrying two labelled `DropDownList` combos:

| «Mod vizualizator Adobe:» | Automat · Modern · Clasic |
| «Instanță nouă Adobe:» | Automat · Da · Nu |

The band lives on the **«Document»** page rather than in a settings dialog because that is where the effect is visible: changing the mode re-places the window hosted right there.

Both combos are backed by `AdobeModeItem` / `AdobeNewInstanceItem` wrappers instead of raw strings — reading the selection must never mean comparing UI text. A change **persists both settings** (they describe the same host; saving one and leaving the other is how a file ends up disagreeing with the panel) and then calls `ReaderHostPreview.ReapplySettings`, which re-applies the geometry to the document already on screen.

**A limit stated rather than hidden:** geometry re-applies immediately; the launch flags (`/n`,
`/s`, `/A`) cannot — you cannot change the command line of a process that is already running. They take effect on the next document. That is in the log line, in the code comment and in §2.5 of the operator document.

`_suppressAdobeComboEvent` guards the population: filling a ComboBox raises `SelectedIndexChanged`, so without it merely **opening** the view would overwrite the stored setting with whatever landed in the list first. There is a test for exactly that.

When `KBotPaths.Save` fails (read-only install folder), the setting still applies for the session and the view says so — a silent failure would be discovered next launch, when the value is mysteriously back.

### §3 The documentation (brief §7)

There was **no** user-facing settings document under `docs\` — checked: the six files there are plans, a handoff, a Redis note, a Playwright note and the forms convention. So
**`docs\SETARI_UTILIZATOR.md` is new and this is its first entry**, in Romanian with real
diacritics, covering all six required points per setting plus the limitations section: the measured geometry, the Acrobat build it was measured on (26.1.21771.0), the fact that an Adobe update can invalidate it, and that `AdobeReaderHarnessForm` is where new values get measured.

It documents a third setting too — `DdfPdfRoot`, which already existed since 0020-04 and had no operator-facing description anywhere.

### Files touched

* `src\KBot.App\Views\Ddf\ReaderHostPreview.vb` — rewritten (325 → 205 lines; all interop removed)
* `src\KBot.App\Views\Ddf\ReaderHostPreview.Designer.vb` — `lblNote`
* `src\KBot.App\Views\DdfView.vb` — `BuildAdobeCombos`, `AdobeSetting_Changed`, theme cascade, `AdobeModeItem` / `AdobeNewInstanceItem`
* `src\KBot.App\Views\DdfView.Designer.vb` — `pnlAdobe` + two labels + two combos
* `docs\SETARI_UTILIZATOR.md` — **new**
* `tests\KBot.App.Tests\DdfViewTests.vb` — 7 new headless STA tests
* `KBot.App` `FileVersion` 1.0.3.0 → 1.0.4.0

### Test results

* `dotnet build KBot.sln` — **0 errors, 0 BC warnings** (16 pre-existing NU1701).
* `dotnet test KBot.sln` — **663 passed / 0 failed / 0 skipped** (580 before slice 0024): Controls 201, DevHarness 153, App 143, Api 68, Xfa 39, Theming 27, Domain 17, Common 14, LocalStore 1.
* The 7 new App tests cover: the band is on `pnlPdf` and docked Top; both combos carry exactly the three documented Romanian labels in order; the combos start on the **stored** value; an invalid stored value starts on «Automat» without throwing; changing one combo persists both; and building the combos does not itself write the settings.

### Anything left unverified or deferred

* **NO VISUAL VERDICT, AND NO ADOBE.** Nothing in this slice has been seen on screen and no PDF has been hosted. The band's width, the combos' theming, whether the modern profile's −130/230/152 actually removes the chrome *in the app's panel* (it was measured in the bench's panel, which is a different size) — all unobserved. This is the biggest open item and it can only be closed by running the app with a real DDF PDF.
* **The popup watcher has still never hidden a popup** (see pass 01). It is enabled here, so the first real run is also its first test.
* **The «relaunch once» path is untested at run time.** With `Auto` on the first document of a session, K-BOT launches with the classic flags, detects, and relaunches if the detected profile needs different ones. That means the first modern-Adobe document of a session can start Adobe **twice**. It is bounded (once per document, `_relaunchedForProfile`) and logged, but whether it looks acceptable to the operator is unknown.
* **`newInstance = false` on the modern profile remains a live risk** (brief §1). Implemented as measured; logged loudly; the process is not closed when it was not ours; the operator can force «Da». **A decision is still owed on whether the shipped default should be forcing `/n`.**
* The setting is per **installation**, not per user (pass 01 §4). If two Windows users share one K-BOT install and have different Adobe versions, they share one value.
* `docs\SETARI_UTILIZATOR.md` describes `DdfPdfRoot` as file-only; the configuration form that would edit these values in the UI is still a later slice.

## SLICE-0024 (pass 03) — the window is caught before it is seen, and dropped instead of handed back <a id="slice-0024-03"></a>

Slice **0024**, third pass. The brief for this work was written as «slice 0023, pass 3» and asked for a new `AdobeHostController` in `KBot.Common`. **Both of those are stale**, and the reconciliation is recorded in §0 below rather than silently worked around (CODE_WORKFLOW §4).

### §0 The brief was written against a repository that no longer exists

The brief predates slices **0024-01** and **0024-02**, which landed the shared Adobe host. Read against the code as it actually stands, its sections split three ways:

| Brief section | Status |
|---|---|
| §0 — the two defects | **Still real, still unfixed.** This pass fixes them. |
| §2/§3 — build `AdobeHostController` in `KBot.Common`, flip that project to `net8.0-windows` | **Superseded.** The engine already exists as `AdobeReaderHost` + 11 helpers in `KBot.Controls\Adobe\`, driven by both consumers, with 201 passing tests. The brief's *goal* (one shared implementation, no duplication) was already met by 0024-01. Operator decision this session: leave it in `KBot.Controls`. `KBot.Common` stays `net8.0` — untouched. |
| §3.4 style mask, §3 generation counter, §6 «rewrite `ReaderHostPreview` as a thin surface» | **Already done** in 0024-01/02. `ReaderHostPreview` was already 220 lines with zero P/Invoke. |
| §5 — shipping app writes `bEnableAv2 = 0` | **Directly contradicted** by 0024-01, which decided the shipping code must never write Adobe preferences and built `AdobeUiDetector` so it does not have to. Operator decision this session: build it, but inert behind a constant. |
| §7 — «the harness switches to `AdobeHostController`» | **Not followed, deliberately.** 0024-01 §3 recorded a reasoned decision that the bench keeps its own orchestration (live spinners, separate `launch`/`waitForEmbed` scenario steps, its own generation counter) and shares only the primitives. That decision still holds; this pass shares the *new* primitives with it instead. |
| §3.6 seam, §3.3 capture, §3.5 teardown, §4 hook, §6 loading state, §7 controls, §8 AcroPDF, §9 tests | **Genuinely missing.** Built here. |

**The two defects the brief diagnosed were verified in the code before anything was written:**

* **Symptom 1 — the stray window with a taskbar button.** `AdobeReaderHost.Detach` called `AdobeWindowHosting.RestoreStandalone`, which put the original style back (`WS_CAPTION`, `WS_POPUP`, …) and re-parented the window to the desktop. That is the textbook way to *create* a top-level window, and a top-level window is a taskbar button. The close meant to follow was a `CloseMainWindow()` on a `Process` object that, for the second document onward, had already exited — Adobe is effectively single-instance.
* **Symptom 2 — the flash.** `AdobeWindowHosting.FindReaderWindow` rejected any candidate failing `IsWindowVisible`, so by construction it could only ever match a window **already drawn on screen**, caption and all.

### §1 The brief's PID rule is wrong on a real machine — and the first fix for it was ALSO wrong

Brief §3.3 says capture must `skip unless pid = ourPid`. **This shipped broken and had to be fixed after the operator ran it.** Both the mistake and the correction are recorded here because the evidence only exists in a log, not in the repository.

**First attempt (wrong).** Knowing the modern profile launches without `/n`, capture was written as
PID-first with a title fallback permitted *only when the launch profile omitted `/n`*. Under Auto or Classic — which do pass `/n` — the fallback was off, so capture searched our own PID only.

**What the operator saw:** the Adobe window never became a child of the panel and stayed on screen
as a floating, taskbar-listed window.

**Why.** From their `adobe_preview.log`, on *every single launch*:

```
ATENȚIE: fereastra încorporată (PID 25168) NU a fost creată de K-BOT (am pornit PID 27152)
```

Adobe is effectively single-instance and hands the document to an already-running copy **even when `/n` is passed — it ignores the switch**. So our launched process routinely owns no window at all, PID-strict capture found nothing, returned `WindowNotFound`, and the real window was simply left alone. Gating on what we *asked for* (`/n`) rather than what actually happens was the error.

**The fix.** The **document title is required** and the **PID is only a preference**: it decides
whether the match is labelled `ByPid` or `ByTitle` and nothing more. A foreign window is always accepted. Nothing is lost by this, because the protection against ending someone else's process lives in `AdobeWindowTeardown` and its launched-PID set — capture never needed to be strict for that to hold.

The title requirement also closes a second hole the first attempt opened: Adobe creates several top-level windows per process sharing the `Acrobat` class prefix, and `EnumWindows` returns them in Z-order, so PID + class alone could grab a **helper window** — hide it, reparent it, and leave the real frame floating. The document name in the title is what tells them apart.

**The tests were complicit and have been fixed too.** The original test #4 used a fake desktop with
exactly one Acrobat-class window per PID, so it could not distinguish the right window from the first one; and one test actively asserted that a foreign window must be *refused*. Three new tests now pin the real behaviour: a foreign window is taken (with the log evidence quoted in the test), a helper window listed first is skipped in favour of the titled frame, and a different document's window in our own process is ignored.

### §2 What changed

#### The capture (`AdobeWindowCapture`, new)

Visibility is **never** tested; identity comes from the process id; the window is hidden **inside the search loop**, the instant it matches, before the caller learns it exists. Polling dropped from 150 ms to 30 ms, because every tick of latency is a tick in which Adobe can be seen. `CaptureDelayMs` is exposed as a deliberate knob — catching the window too early may mean Adobe has not finished building its document view, which is **a risk to observe, not a settled fact**; default 0, and the bench has a spinner so the threshold can be *measured* rather than guessed.

#### The teardown (`AdobeWindowTeardown`, new)

`RestoreStandalone` is **deleted from the codebase**, not merely unused. Neither detach mode restores the style or re-parents; both drop the window.

* **Mode A (`KillProcess`, the shipped default)** — `Kill(True)` on a PID from `_launchedPids`, the set of processes this host actually started. A PID outside that set is **never** killed and the call degrades to mode B with a logged refusal, because the modern profile's window can belong to the operator's own Adobe and ending it would close their unrelated documents.
* **Mode B (`CloseWindow`)** — `PostMessage(WM_CLOSE)` (posted, never sent: a busy foreign UI thread must not block ours), then up to `CloseGraceMs` for the handle to die, then mode A **only** for a process we started. Mode B is the only path that waits, so the shipped default never blocks the UI on a document change.

#### Everything else

* `INativeWindows` / `Win32Windows`, `IAdobeLauncher` / `ProcessAdobeLauncher`, `IHostSurface` / `ControlHostSurface` — the seams that let capture, teardown and the whole orchestration run headless. `AdobeNativeMethods` stays the single P/Invoke set.
* `AdobeHostOptions` + `AdobeDetachMode`.
* `AdobeCreationHook` — `SetWinEventHook` early catch, **off by default**, delegate held in an instance field, `UnhookWinEvent` on every path including failures.
* `ReaderHostPreview` — new `pnlLoading` state («Se încarcă documentul…», declared in the Designer per the house rule) shown for the whole embed, and a compile-time `DetachMode` constant with both paths implemented.
* `AdobeUiPreference` (KBot.App) — the §5 mechanism, **`ENFORCE_LEGACY_UI = False`**. Written, reachable and documented; it reads and writes nothing while the constant is False.
* Bench: **«Închidere / captură»** (mode radios, creation-hook checkbox, capture-delay and close-grace spinners, and the **launch → embed milliseconds** label that turns A-vs-B from an impression into a number) and the `bEnableAv2 = 0` checkbox, which reuses the existing apply path including its consent dialog. New `hosting` scenario section, round-tripped.
* Bench: **«ActiveX (AcroPDF)»** — CLSID read at runtime from `HKCR\AcroPDF.PDF.1\CLSID` (**no hardcoded GUID**), a minimal `AxHost` subclass, reflection-based `LoadFile` (`Option Strict On` forbids late binding), and a second-document button because document *switching* is the whole comparison. No `aximp`, no interop assembly, no COM reference. Nothing in `KBot.App` depends on it.

#### The registry move

`IRegistryAccess`, `WinRegistryAccess`, `RegistryValueSnapshot`, `RegistrySnapshotSet`, `AdobeHiveResolver`, `AdobeRegistryConstants` moved `KBot.DevHarness` → `KBot.Controls\Adobe\` (290 lines, `git mv`, no edits to their bodies). `KBot.App` cannot reference `KBot.DevHarness`, and duplicating a snapshot helper is the exact mistake 0024-01 was written to eliminate.

### §3 The AcroPDF verdict — recorded 05.08.2026, and it is POSITIVE

**A real XFA DDF renders in the AcroPDF ActiveX control.** No «Please wait…» placeholder.

Evidence, from `test_20260805_162859_073.log` and `test_adobe_rhp.log`:

* the control **is** registered here — CLSID `{CA8A9780-280D-11CF-A24D-444553540000}`, read from `HKCR\AcroPDF.PDF.1\CLSID` at runtime as designed, never hardcoded;
* it creates and answers reflection calls — the version property returned a full Acrobat **26.1** plugin list (`AcroForm`, `EScript`, `DigSig`, `PPKLite`, …), with no interop assembly and no `aximp`;
* **two real generated DDFs rendered**, both dynamic XFA (`NeedsRendering true`): `C:\AVACONT\forexe\PDF\1264\DDF_NR_2_REV_0.PDF` and `C:\AVACONT\forexe\PDF\DDF\GENERAL\DDF_NR_2_REV_0_AAB2MBD8E3F.PDF`;
* **document switching works** — the second was loaded into the SAME control with no clear in between (16:38:40 → 16:38:57), which was the whole point of the comparison;
* zero AcroPDF entries in `harness_errors.log`.

#### The one failure, and what it actually was

`ORD_009.pdf` showed a dark grey void on both attempts. **The cause is not the file and not the path — it is that the SAME document was already open in the window-hosted Adobe.** The log shows it plainly: `Adobe încorporat — Acrobat.exe … "…\Config\ORD_009.pdf"` (lines 12 and 16) and then `LoadFile(documentul curent) … ORD_009.pdf` (lines 21 and 29). Every load of a *different* file worked; every load of the *hosted* file did not.

Adobe is effectively single-instance, and the AcroPDF control is served by that same Acrobat engine. It already had the document open in the embedded window, so the second request for it yields nothing.

Recorded because two plausible-looking theories were wrong and cost time: the failing file sits under a path containing a **space** while both working files are under `C:\AVACONT\…`, and it is a dynamic XFA form. Both facts are true, **and neither is the cause.** The same-document collision explains all four data points; the other two explain two each by coincidence.

**Consequence for any future design.** The window-hosting surface and an AcroPDF surface **cannot
show the same document at the same time**. If AcroPDF ever replaces window hosting, it replaces it entirely — the two cannot run side by side on one document, so there is no gradual migration in which both are live. That does not change §11: there is still no shipping dependency on AcroPDF.

The bench now warns when the two collide, so the next person meets a log line instead of a grey rectangle. Confirmed afterwards by the operator: **`ORD_009.pdf` renders normally in the control once it is not simultaneously hosted.** The collision was the whole of it.

#### What is still wrong with the ActiveX surface: the toolbars

The rendered document arrives **with all of Adobe's toolbars visible**. That is the same problem slice 0023 spent five passes on — clip geometry, hiding child windows by text, HKCU preferences, HKLM policies, keyboard toggles — and never solved cleanly.

The difference is that the ActiveX control has a **documented API** for it. `AcroPdfHost.ApplyChrome` now calls, by reflection, each of `setShowToolbar(False)`, `setShowScrollbars(False)`, `setPageMode("none")`, `setLayoutMode("SinglePage")` and `setView("Fit")`, **reporting each one independently** — the interesting answer is *which* of them this Adobe build honours, and a single pass/fail would hide that. A member the control does not expose is logged as «NU EXISTĂ pe acest build», which is a result, not an error.

The calls are made **before and again after** `LoadFile`, with the two passes logged separately, because the documentation and common practice disagree about which ordering takes effect on which build. The bench is the right place to settle that by measurement.

#### The chrome API was measured on 05.08.2026. It does not work.

All five calls returned **OK on every load, before and after `LoadFile`** — and the bars stayed. Accepted and ignored.

This is the **same split slice 0023 hit with the `/A` open parameters**: those methods address DOCUMENT chrome, while the tab strip and the Tools pane are APPLICATION chrome. The ActiveX route does not escape the problem; it inherits it. So the hoped-for outcome — four method calls replacing the clip / hide-children / registry apparatus — **did not happen.**

#### What DOES work: hiding three child windows by text

The operator reached a state they describe as perfect — nothing visible until the mouse moves, when the floating bar appears. Diffing that probe against a bad one shows the whole difference:

| window (by text) | bad | **perfect** |
|---|---|---|
| `AVDockableTabStripView` (left strip) | `67x297 vis=1` | **`0x297 vis=0`** |
| `AVExpandCollapseButtonView` (right, x=1872) | `27x297 vis=1` | **`27x227 vis=0`** |
| `AVSplitationPageView` (the document) | `x=67, w=1805` | **`x=0, w=1899` — the whole panel** |

**The perfect state is not a mode Adobe offers. It is three windows being invisible**, after which
the document view widens to the full panel *by itself* — nothing has to be resized. That makes it reproducible with `ShowWindow`, which is exactly the `hideChildren` lever slice 0023 already built, pointed at the ActiveX control instead of the hosted window. `btnAcroHideChrome` does that, and classifies each window BEFORE touching it so «already hidden» and «zero-sized» cannot be reported as success — pass 4's lesson, applied before the same mistake could be repeated.

#### Correction: hiding is not collapsing, and the click IS the answer

The first attempt at reproducing the state — `ShowWindow(SW_HIDE)` on those windows — **ran and did not work**, which the probe showed immediately:

```
after ShowWindow(SW_HIDE)   AVDockableTabStripView 67x697 vis=0, document x=67 w=792
the state actually wanted   AVDockableTabStripView  0x697 vis=0, document x=0  w=859
```

Hiding leaves the **width**, so Adobe never re-lays-out. Adobe's own collapse sets the width to zero
**and moves the siblings**. `vis=0` was a consequence, not the mechanism — reading the correlation as
the cause is what produced a useless button.

**Pressing Adobe's own button works.** Measured 05.08.2026 21:18:35:

```
Apăs butonul de la x=67 ... înainte: bandă 67x697 la x=0
După click: bandă 0x697, document la x=0 lățime 859 (panou 886)   REUȘIT
```

A posted `WM_LBUTTONDOWN`/`WM_LBUTTONUP` on `AVExpandCollapseButtonView` makes Adobe do its own layout, which is the only version guaranteed to be self-consistent. The button toggles, so it is only pressed when the strip is actually expanded.

**And the state persists across documents in the same Acrobat process** — at 19:22 the strip was
already `0x697` before anything was clicked, after two loads. So this has to succeed **once per session**, not per document, which is much cheaper than the hosted-window case where every embed needs its hides re-applied.

#### The grey first document: Adobe postpones its first layout

Reported symptom: the first document of a session stays grey until the operator clicks in the panel; afterwards it never recurs. The probe says exactly why — right after the first `LoadFile`, **26 of 28 child windows are zero-sized** and the tab strip is `0x0`. After a click it drops to 18 with real rectangles.

**Zero on BOTH axes is «not laid out», not «collapsed»** — a genuinely collapsed strip is `0x697`.
That distinction cost a run: the collapse button saw `0x0`, reported «deja colapsată» and refused to press. Both states have width 0 and only the height tells them apart.

`WakeAcroLayout` escalates cheapest-first — focus, then a size change, then a synthetic click into `AVDocumentMainView` — and **logs which step worked**. It runs automatically after each `LoadFile`, guarded by `IsAcroLayoutDegenerate` so a healthy layout is never touched.

**Answered on the next run: the SIZE CHANGE is what does it.** Twice, at 21:27:15 and 21:27:35 —
`REZOLVAT cu schimbarea de dimensiune`. Focus alone is not enough, and the synthetic click into the document is never reached. So the automatic wake costs one focus call and one one-pixel resize, and never has to touch the document.

#### The floating bar (Adobe's HUD)

The operator moves it higher because it is otherwise off-screen, and asked whether that position can be detected and reproduced. Two facts settle how:

* **It is not in the registry.** They moved it between two `AVGeneral` snapshots and **not one of the 106 values changed**. `iNumUserDockUndockHUD` is a dock/undock COUNTER, still 0. So the position lives in process memory and can only be reproduced by moving the window.
* **The existing probe cannot see it**, because `AdobeWindowProbe.Walk` walks the CHILDREN of the control and the bar is a TOP-LEVEL popup.

What makes this easy rather than hard: **AcroPDF is an in-process COM server**, so the popup belongs to *our own* process id. None of the cross-process caveats that dog the hosted window apply. `btnAcroHud` enumerates this process's top-level windows (excluding our own forms), logs each with class/text/rect/visibility, and remembers the smallest visible one; `btnAcroHudApply` puts it back, matching by **class + text, never by handle** — Adobe destroys and recreates the popup, the same rule slice 0023 established for the hosted window's children.

The «smallest visible window» rule is a **heuristic**, and the log prints the full candidate list precisely so it can be confirmed rather than trusted.

#### The pane state is NOT in the registry

Fifteen `AVGeneral` snapshots, 106 values each, across every pane manipulation. **The only values that changed were session counters** (`iNumAcrobatLaunches`, `iNumOfAVDocsOpened`, `uLastSessionTimeStamp`, `uWhatsNewExpContentRotation`). Nothing pane-related moved.

So the state that survives from one document to the next is held **in the live Acrobat process's memory** — which persists precisely because Adobe is single-instance. Consequences: a registry pre-set cannot steer it mid-session, and the first document of a session inherits whatever the operator's previous Acrobat session left behind. That is why the same file behaves differently depending on what was open before it.

This is also why enumerating the hive rather than guessing a key name was the right call: the answer turned out to be «no key at all», which no amount of guessing would have produced.

Per §11, nothing in `KBot.App` depends on any of this.

### Files touched

**New** — `src\KBot.Controls\Adobe\{INativeWindows, IAdobeLauncher, IHostSurface, AdobeHostOptions,
AdobeWindowCapture, AdobeWindowTeardown, AdobeCreationHook}.vb`, `src\KBot.App\Views\Ddf\AdobeUiPreference.vb`, `src\KBot.DevHarness\Internal\Adobe\AcroPdfHost.vb`, `tests\KBot.Controls.Tests\{FakeNativeWindows, AdobeWindowCaptureTests, AdobeWindowTeardownTests, AdobeReaderHostTests}.vb`, `tests\KBot.DevHarness.Tests\HarnessScenarioHostingTests.vb`

**Moved** — the six registry files listed above, `KBot.DevHarness` → `KBot.Controls`

**Modified** — `AdobeReaderHost.vb` (capture/teardown/options/launched-PID set),
`AdobeWindowHosting.vb` (`RestoreStandalone` **deleted**; `FindReaderWindow` delegates to the new capture; `BuildArguments` extras overload), `AdobeNativeMethods.vb` (`PostMessage`, `WM_CLOSE`, `SetWinEventHook`, `UnhookWinEvent`), `ReaderHostPreview.vb` + `.Designer.vb`, `AdobeReaderHarnessForm.vb` + `.Designer.vb`, `HarnessScenario.vb`, `RegistryWriteVerifier.vb`, `FakeRegistryAccess.vb`, `AdobeHarnessLayoutTests.vb`, `KBot.DevHarness.Tests.vbproj` (project-wide `<Import Include="KBot.Controls" />`)

**Modified (build hygiene)** — `KBot.Xfa.vbproj`, `KBot.App.vbproj`, `KBot.Xfa.Tests.vbproj`,
`KBot.App.Tests.vbproj`: NU1701 suppression, see the test-results section.

**Versions** — `KBot.Controls` 1.1.0.0 → 1.2.0.0, `KBot.App` 1.0.4.0 → 1.0.5.0,
`KBot.DevHarness` 1.0.7.0 → 1.0.8.0. `KBot.Common` **unchanged** — it was not touched. `KBot.Xfa` **not bumped**: only its .vbproj warning settings changed, not a single line of its code.

### Test results

* `dotnet build KBot.sln` — **0 errors, 0 warnings of any kind.** The 16 long-standing NU1701 warnings were silenced in this pass at the operator's request: `NoWarn="NU1701"` on the iTextSharp `PackageReference` in `KBot.Xfa`, plus a `<NoWarn>` property in the four projects that reach **BouncyCastle**, which arrives *transitively* through iTextSharp and therefore cannot be covered by the package-level attribute (different package id). The suppression is deliberately NOT a `Directory.Build.props` blanket — the other eight projects still warn normally, and a new .NET Framework package added tomorrow will still be reported. **What it costs:** we will no longer be told these two packages are not native to .NET 8. Acceptable because both versions are pinned (the same ones as the original XFA_WRITTER exe the engine was ported from, slice 0019) and the surface used is covered by `KBot.Xfa.Tests`. If either version ever changes, remove `NoWarn` first so the warning can be heard again — that note is in the .vbproj itself, not only here.
* `dotnet test KBot.sln` — **694 passed / 0 failed / 0 skipped**, up from 663 (+31). `KBot.Controls.Tests` 201 → 225, `KBot.DevHarness.Tests` 153 → 160.

All nine tests the brief's §9 asks for exist:

1. no Adobe → `AdobeMissing`, **nothing started** · 2. the style mask clears all six bits and sets
`WS_CHILD` · 3. capture ignores a foreign PID even when class *and* title match · 4. **capture accepts an invisible window** (the flash regression) · 5. a newer generation supersedes the older, which returns `Superseded` and kills **only its own** process · 6. mode A never kills a PID it did not launch · 7. mode B falls back to A after `CloseGraceMs` · 8. **teardown never calls `SetParent` back and never restores the style** — asserted for *both* modes, since this is the exact defect ·
9. `hosting` round-trips and a scenario file without the section still loads.

One existing test was updated rather than worked around: `AllTwelveSections_ArePresent_InOrder` became `AllFourteenSections_ArePresent_InOrder`. It is a real guard on section order and it caught the change, which is what it is for.

### Anything left unverified or deferred

* **THE CAPTURE PATH HAS NOW BEEN RUN ONCE ON A REAL MACHINE — AND IT FAILED.** See §1. The fix is in, but it too is unverified: nobody has yet confirmed on screen that the window becomes a child again. **This is the first thing to check.**
* **NOTHING ELSE HERE HAS RUN AGAINST A REAL ADOBE.** The tests prove call *order* and call *absence* against a fake desktop; teardown's `PostMessage`, the creation hook's `SetWinEventHook` and both detach modes have still not been executed once.
* **A process lesson worth keeping:** the failure was not caught by the build, by 693 passing tests, or by review — it needed one run. The tests were written from the same wrong assumption as the code, so they confirmed it instead of challenging it. The evidence that settled it was a log file in `bin\`, not anything in the repository.
* **The brief's §10 definition of done cannot be signed off from here.** «No stray window», «no taskbar button», «the window is not seen before it is inside `pnlHost`» and «the residual flash is measured» are all **on-screen observations**. The code paths that produced both defects are gone and their absence is pinned by tests — that is a different and weaker claim than «the defects are observed fixed».
* **The residual flash is narrowed much less than hoped, and now there is a number.** Every capture in the fixed build matched `după titlu` at **1465–1567 ms** — because on this machine Adobe hands the document to a running instance, so the match waits for that instance to set the document title, which it plausibly does around the moment it shows the window. Hiding on match still helps; claiming more than that would be wrong. (For contrast, the broken PID-only build matched in **42–51 ms** — because it was seizing the wrong window.)
* **`CaptureDelayMs` defaults to 0 and no threshold has been found.** Whether reparenting a half-initialised Adobe window causes trouble is the open question the spinner exists to answer.
* **The creation hook ran, and it does do something:** `Cârlig de creare eliberat (a ascuns 1 ferestre)`. First evidence it is not inert. Whether it measurably shortens the flash is still unmeasured, and it stays off by default.
* **MODE B DOES NOT WORK ON THE OPERATOR'S MACHINE.** Observed twice: `Închidere (B): fereastra nu s-a închis în 1500 ms și procesul 9828 nu e al nostru — nu îl opresc.` Adobe did not honour `WM_CLOSE` on the reparented foreign window. **And mode A cannot close it either** — correctly refusing to kill a process it did not start, it degraded to a close that also failed: `rămâne copil al panoului și dispare odată cu el`. So under the single-instance handoff **neither mode can close the window**; it only dies when the host panel does. Mode A remains the right shipped default. Whether B behaves differently against a window K-BOT actually launched (`/n` honoured) is still unverified, as is the new Acrobat UI.
* ~~The AcroPDF verdict is NOT recorded~~ — **RECORDED and POSITIVE, see §3**, and the same-document collision is confirmed as the whole explanation of the one failure.
* ~~Do the ActiveX chrome calls work?~~ **ANSWERED: NO** (all five return OK and change nothing). ~~Does hiding by text work?~~ **ANSWERED: NO** — hiding leaves the width and Adobe never reflows. ~~Does pressing Adobe's own collapse button work?~~ **ANSWERED: YES, measured.** See §3.
* **`WakeAcroLayout` has not been run.** Which of its three steps clears the grey first document — focus, size change, or synthetic click — is unknown, and that is the whole point of it reporting which one fired. Only the operator's manual click is known to work.
* **The AcroPDF route is now a genuine candidate, and that is a decision, not a code change.** It renders XFA, switches documents, reaches the wanted chrome state with one synthetic click, and that state persists for the session. Against window hosting it looks better on every axis measured so far. **Nothing in `KBot.App` depends on it (§11) and nothing should until somebody decides that deliberately** — the remaining unknowns are whether the collapse click is reliable across Adobe versions, and what happens when the host panel is resized.
* **The §5 `bEnableAv2` table is empty.** Whether the opt-out is still honoured, per UI generation, has not been observed. `AdobeUiPreference` is inert (`ENFORCE_LEGACY_UI = False`), so its snapshot/restore path — including «absent restores to deletion» — has never executed in the app; only `RegistrySnapshotSet`'s own tests cover that rule.
* **The bench has not been re-run on screen** after this refactor, same as after 0024-01.
* Carried forward from 0024-02 and untouched here: the «relaunch once» path is still untested at run time, and `newInstance = false` on the modern profile **still owes a decision** on whether the shipped default should force `/n`. This pass makes that decision *safer* (a foreign window is never killed) but does not make it.
* Keyboard and focus across the process boundary remain not-native and are still not fixed — the same documented position as slice 0020-03.

## Completări și checkpointuri din registru

- Profilurile transcrise: Modern fără /n, fără /s, fără parametri /A, clip dreapta 230/sus 152 și dx=-130; Clasic cu /n și /s, toolbar=0&navpanes=0, fără clip/move. La detecție necunoscută, registrul preciza și logarea arborelui complet înaintea fallbackului Clasic. Nu sunt recomandări noi.
- Pentru 03, registrul păstra 693 teste (Controls 224), iar worklogul a fost actualizat la 694 (Controls 225). Ambele checkpointuri sunt consemnate; nu s-a recalculat o suită acum.
- Registrul mai consemna LoadFile ca întorcând True și în cazul gri și panoul ActiveX 1899×298 în probele inițiale, apoi ridicat la 700. Aceste note nu sunt dovadă că mecanismul de ascundere dedus inițial a funcționat; corecția ulterioară este cea de mai sus.
