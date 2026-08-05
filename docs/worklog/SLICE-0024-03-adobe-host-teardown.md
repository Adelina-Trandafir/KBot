# SLICE-0024 (pass 03) — the window is caught before it is seen, and dropped instead of handed back

Slice **0024**, third pass. The brief for this work was written as «slice 0023, pass 3» and asked for
a new `AdobeHostController` in `KBot.Common`. **Both of those are stale**, and the reconciliation is
recorded in §0 below rather than silently worked around (CODE_WORKFLOW §4).

## §0 The brief was written against a repository that no longer exists

The brief predates slices **0024-01** and **0024-02**, which landed the shared Adobe host. Read
against the code as it actually stands, its sections split three ways:

| Brief section | Status |
|---|---|
| §0 — the two defects | **Still real, still unfixed.** This pass fixes them. |
| §2/§3 — build `AdobeHostController` in `KBot.Common`, flip that project to `net8.0-windows` | **Superseded.** The engine already exists as `AdobeReaderHost` + 11 helpers in `KBot.Controls\Adobe\`, driven by both consumers, with 201 passing tests. The brief's *goal* (one shared implementation, no duplication) was already met by 0024-01. Operator decision this session: leave it in `KBot.Controls`. `KBot.Common` stays `net8.0` — untouched. |
| §3.4 style mask, §3 generation counter, §6 «rewrite `ReaderHostPreview` as a thin surface» | **Already done** in 0024-01/02. `ReaderHostPreview` was already 220 lines with zero P/Invoke. |
| §5 — shipping app writes `bEnableAv2 = 0` | **Directly contradicted** by 0024-01, which decided the shipping code must never write Adobe preferences and built `AdobeUiDetector` so it does not have to. Operator decision this session: build it, but inert behind a constant. |
| §7 — «the harness switches to `AdobeHostController`» | **Not followed, deliberately.** 0024-01 §3 recorded a reasoned decision that the bench keeps its own orchestration (live spinners, separate `launch`/`waitForEmbed` scenario steps, its own generation counter) and shares only the primitives. That decision still holds; this pass shares the *new* primitives with it instead. |
| §3.6 seam, §3.3 capture, §3.5 teardown, §4 hook, §6 loading state, §7 controls, §8 AcroPDF, §9 tests | **Genuinely missing.** Built here. |

**The two defects the brief diagnosed were verified in the code before anything was written:**

* **Symptom 1 — the stray window with a taskbar button.** `AdobeReaderHost.Detach` called
  `AdobeWindowHosting.RestoreStandalone`, which put the original style back (`WS_CAPTION`,
  `WS_POPUP`, …) and re-parented the window to the desktop. That is the textbook way to *create* a
  top-level window, and a top-level window is a taskbar button. The close meant to follow was a
  `CloseMainWindow()` on a `Process` object that, for the second document onward, had already
  exited — Adobe is effectively single-instance.
* **Symptom 2 — the flash.** `AdobeWindowHosting.FindReaderWindow` rejected any candidate failing
  `IsWindowVisible`, so by construction it could only ever match a window **already drawn on
  screen**, caption and all.

## §1 The brief's PID rule is wrong on a real machine — and the first fix for it was ALSO wrong

Brief §3.3 says capture must `skip unless pid = ourPid`. **This shipped broken and had to be fixed
after the operator ran it.** Both the mistake and the correction are recorded here because the
evidence only exists in a log, not in the repository.

**First attempt (wrong).** Knowing the modern profile launches without `/n`, capture was written as
PID-first with a title fallback permitted *only when the launch profile omitted `/n`*. Under Auto or
Classic — which do pass `/n` — the fallback was off, so capture searched our own PID only.

**What the operator saw:** the Adobe window never became a child of the panel and stayed on screen
as a floating, taskbar-listed window.

**Why.** From their `adobe_preview.log`, on *every single launch*:

```
ATENȚIE: fereastra încorporată (PID 25168) NU a fost creată de K-BOT (am pornit PID 27152)
```

Adobe is effectively single-instance and hands the document to an already-running copy **even when
`/n` is passed — it ignores the switch**. So our launched process routinely owns no window at all,
PID-strict capture found nothing, returned `WindowNotFound`, and the real window was simply left
alone. Gating on what we *asked for* (`/n`) rather than what actually happens was the error.

**The fix.** The **document title is required** and the **PID is only a preference**: it decides
whether the match is labelled `ByPid` or `ByTitle` and nothing more. A foreign window is always
accepted. Nothing is lost by this, because the protection against ending someone else's process
lives in `AdobeWindowTeardown` and its launched-PID set — capture never needed to be strict for that
to hold.

The title requirement also closes a second hole the first attempt opened: Adobe creates several
top-level windows per process sharing the `Acrobat` class prefix, and `EnumWindows` returns them in
Z-order, so PID + class alone could grab a **helper window** — hide it, reparent it, and leave the
real frame floating. The document name in the title is what tells them apart.

**The tests were complicit and have been fixed too.** The original test #4 used a fake desktop with
exactly one Acrobat-class window per PID, so it could not distinguish the right window from the
first one; and one test actively asserted that a foreign window must be *refused*. Three new tests
now pin the real behaviour: a foreign window is taken (with the log evidence quoted in the test), a
helper window listed first is skipped in favour of the titled frame, and a different document's
window in our own process is ignored.

## §2 What changed

### The capture (`AdobeWindowCapture`, new)

Visibility is **never** tested; identity comes from the process id; the window is hidden **inside
the search loop**, the instant it matches, before the caller learns it exists. Polling dropped from
150 ms to 30 ms, because every tick of latency is a tick in which Adobe can be seen.
`CaptureDelayMs` is exposed as a deliberate knob — catching the window too early may mean Adobe has
not finished building its document view, which is **a risk to observe, not a settled fact**; default
0, and the bench has a spinner so the threshold can be *measured* rather than guessed.

### The teardown (`AdobeWindowTeardown`, new)

`RestoreStandalone` is **deleted from the codebase**, not merely unused. Neither detach mode
restores the style or re-parents; both drop the window.

* **Mode A (`KillProcess`, the shipped default)** — `Kill(True)` on a PID from `_launchedPids`, the
  set of processes this host actually started. A PID outside that set is **never** killed and the
  call degrades to mode B with a logged refusal, because the modern profile's window can belong to
  the operator's own Adobe and ending it would close their unrelated documents.
* **Mode B (`CloseWindow`)** — `PostMessage(WM_CLOSE)` (posted, never sent: a busy foreign UI thread
  must not block ours), then up to `CloseGraceMs` for the handle to die, then mode A **only** for a
  process we started. Mode B is the only path that waits, so the shipped default never blocks the UI
  on a document change.

### Everything else

* `INativeWindows` / `Win32Windows`, `IAdobeLauncher` / `ProcessAdobeLauncher`, `IHostSurface` /
  `ControlHostSurface` — the seams that let capture, teardown and the whole orchestration run
  headless. `AdobeNativeMethods` stays the single P/Invoke set.
* `AdobeHostOptions` + `AdobeDetachMode`.
* `AdobeCreationHook` — `SetWinEventHook` early catch, **off by default**, delegate held in an
  instance field, `UnhookWinEvent` on every path including failures.
* `ReaderHostPreview` — new `pnlLoading` state («Se încarcă documentul…», declared in the Designer
  per the house rule) shown for the whole embed, and a compile-time `DetachMode` constant with both
  paths implemented.
* `AdobeUiPreference` (KBot.App) — the §5 mechanism, **`ENFORCE_LEGACY_UI = False`**. Written,
  reachable and documented; it reads and writes nothing while the constant is False.
* Bench: **«Închidere / captură»** (mode radios, creation-hook checkbox, capture-delay and
  close-grace spinners, and the **launch → embed milliseconds** label that turns A-vs-B from an
  impression into a number) and the `bEnableAv2 = 0` checkbox, which reuses the existing apply path
  including its consent dialog. New `hosting` scenario section, round-tripped.
* Bench: **«ActiveX (AcroPDF)»** — CLSID read at runtime from `HKCR\AcroPDF.PDF.1\CLSID` (**no
  hardcoded GUID**), a minimal `AxHost` subclass, reflection-based `LoadFile` (`Option Strict On`
  forbids late binding), and a second-document button because document *switching* is the whole
  comparison. No `aximp`, no interop assembly, no COM reference. Nothing in `KBot.App` depends on it.

### The registry move

`IRegistryAccess`, `WinRegistryAccess`, `RegistryValueSnapshot`, `RegistrySnapshotSet`,
`AdobeHiveResolver`, `AdobeRegistryConstants` moved `KBot.DevHarness` → `KBot.Controls\Adobe\`
(290 lines, `git mv`, no edits to their bodies). `KBot.App` cannot reference `KBot.DevHarness`, and
duplicating a snapshot helper is the exact mistake 0024-01 was written to eliminate.

## §3 The AcroPDF verdict — recorded 05.08.2026, and it is POSITIVE

**A real XFA DDF renders in the AcroPDF ActiveX control.** No «Please wait…» placeholder.

Evidence, from `test_20260805_162859_073.log` and `test_adobe_rhp.log`:

* the control **is** registered here — CLSID `{CA8A9780-280D-11CF-A24D-444553540000}`, read from
  `HKCR\AcroPDF.PDF.1\CLSID` at runtime as designed, never hardcoded;
* it creates and answers reflection calls — the version property returned a full Acrobat **26.1**
  plugin list (`AcroForm`, `EScript`, `DigSig`, `PPKLite`, …), with no interop assembly and no
  `aximp`;
* **two real generated DDFs rendered**, both dynamic XFA (`NeedsRendering true`):
  `C:\AVACONT\forexe\PDF\1264\DDF_NR_2_REV_0.PDF` and
  `C:\AVACONT\forexe\PDF\DDF\GENERAL\DDF_NR_2_REV_0_AAB2MBD8E3F.PDF`;
* **document switching works** — the second was loaded into the SAME control with no clear in
  between (16:38:40 → 16:38:57), which was the whole point of the comparison;
* zero AcroPDF entries in `harness_errors.log`.

### The one failure, and what it actually was

`ORD_009.pdf` showed a dark grey void on both attempts. **The cause is not the file and not the
path — it is that the SAME document was already open in the window-hosted Adobe.** The log shows it
plainly: `Adobe încorporat — Acrobat.exe … "…\Config\ORD_009.pdf"` (lines 12 and 16) and then
`LoadFile(documentul curent) … ORD_009.pdf` (lines 21 and 29). Every load of a *different* file
worked; every load of the *hosted* file did not.

Adobe is effectively single-instance, and the AcroPDF control is served by that same Acrobat engine.
It already had the document open in the embedded window, so the second request for it yields
nothing.

Recorded because two plausible-looking theories were wrong and cost time: the failing file sits
under a path containing a **space** while both working files are under `C:\AVACONT\…`, and it is a
dynamic XFA form. Both facts are true, **and neither is the cause.** The same-document collision
explains all four data points; the other two explain two each by coincidence.

**Consequence for any future design.** The window-hosting surface and an AcroPDF surface **cannot
show the same document at the same time**. If AcroPDF ever replaces window hosting, it replaces it
entirely — the two cannot run side by side on one document, so there is no gradual migration in
which both are live. That does not change §11: there is still no shipping dependency on AcroPDF.

The bench now warns when the two collide, so the next person meets a log line instead of a grey
rectangle. Confirmed afterwards by the operator: **`ORD_009.pdf` renders normally in the control once
it is not simultaneously hosted.** The collision was the whole of it.

### What is still wrong with the ActiveX surface: the toolbars

The rendered document arrives **with all of Adobe's toolbars visible**. That is the same problem
slice 0023 spent five passes on — clip geometry, hiding child windows by text, HKCU preferences,
HKLM policies, keyboard toggles — and never solved cleanly.

The difference is that the ActiveX control has a **documented API** for it. `AcroPdfHost.ApplyChrome`
now calls, by reflection, each of `setShowToolbar(False)`, `setShowScrollbars(False)`,
`setPageMode("none")`, `setLayoutMode("SinglePage")` and `setView("Fit")`, **reporting each one
independently** — the interesting answer is *which* of them this Adobe build honours, and a single
pass/fail would hide that. A member the control does not expose is logged as «NU EXISTĂ pe acest
build», which is a result, not an error.

The calls are made **before and again after** `LoadFile`, with the two passes logged separately,
because the documentation and common practice disagree about which ordering takes effect on which
build. The bench is the right place to settle that by measurement.

### The chrome API was measured on 05.08.2026. It does not work.

All five calls returned **OK on every load, before and after `LoadFile`** — and the bars stayed.
Accepted and ignored.

This is the **same split slice 0023 hit with the `/A` open parameters**: those methods address
DOCUMENT chrome, while the tab strip and the Tools pane are APPLICATION chrome. The ActiveX route
does not escape the problem; it inherits it. So the hoped-for outcome — four method calls replacing
the clip / hide-children / registry apparatus — **did not happen.**

### What DOES work: hiding three child windows by text

The operator reached a state they describe as perfect — nothing visible until the mouse moves, when
the floating bar appears. Diffing that probe against a bad one shows the whole difference:

| window (by text) | bad | **perfect** |
|---|---|---|
| `AVDockableTabStripView` (left strip) | `67x297 vis=1` | **`0x297 vis=0`** |
| `AVExpandCollapseButtonView` (right, x=1872) | `27x297 vis=1` | **`27x227 vis=0`** |
| `AVSplitationPageView` (the document) | `x=67, w=1805` | **`x=0, w=1899` — the whole panel** |

**The perfect state is not a mode Adobe offers. It is three windows being invisible**, after which
the document view widens to the full panel *by itself* — nothing has to be resized. That makes it
reproducible with `ShowWindow`, which is exactly the `hideChildren` lever slice 0023 already built,
pointed at the ActiveX control instead of the hosted window. `btnAcroHideChrome` does that, and
classifies each window BEFORE touching it so «already hidden» and «zero-sized» cannot be reported as
success — pass 4's lesson, applied before the same mistake could be repeated.

### Correction: hiding is not collapsing, and the click IS the answer

The first attempt at reproducing the state — `ShowWindow(SW_HIDE)` on those windows — **ran and did
not work**, which the probe showed immediately:

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

A posted `WM_LBUTTONDOWN`/`WM_LBUTTONUP` on `AVExpandCollapseButtonView` makes Adobe do its own
layout, which is the only version guaranteed to be self-consistent. The button toggles, so it is only
pressed when the strip is actually expanded.

**And the state persists across documents in the same Acrobat process** — at 19:22 the strip was
already `0x697` before anything was clicked, after two loads. So this has to succeed **once per
session**, not per document, which is much cheaper than the hosted-window case where every embed
needs its hides re-applied.

### The grey first document: Adobe postpones its first layout

Reported symptom: the first document of a session stays grey until the operator clicks in the panel;
afterwards it never recurs. The probe says exactly why — right after the first `LoadFile`, **26 of 28
child windows are zero-sized** and the tab strip is `0x0`. After a click it drops to 18 with real
rectangles.

**Zero on BOTH axes is «not laid out», not «collapsed»** — a genuinely collapsed strip is `0x697`.
That distinction cost a run: the collapse button saw `0x0`, reported «deja colapsată» and refused to
press. Both states have width 0 and only the height tells them apart.

`WakeAcroLayout` now escalates cheapest-first — focus, then a size change, then a synthetic click
into `AVDocumentMainView` — and **logs which step worked**, because the cheap steps are the ones safe
to run on every load. It runs automatically after each `LoadFile`, guarded by
`IsAcroLayoutDegenerate` so a healthy layout is never touched.

### The pane state is NOT in the registry

Fifteen `AVGeneral` snapshots, 106 values each, across every pane manipulation. **The only values
that changed were session counters** (`iNumAcrobatLaunches`, `iNumOfAVDocsOpened`,
`uLastSessionTimeStamp`, `uWhatsNewExpContentRotation`). Nothing pane-related moved.

So the state that survives from one document to the next is held **in the live Acrobat process's
memory** — which persists precisely because Adobe is single-instance. Consequences: a registry
pre-set cannot steer it mid-session, and the first document of a session inherits whatever the
operator's previous Acrobat session left behind. That is why the same file behaves differently
depending on what was open before it.

This is also why enumerating the hive rather than guessing a key name was the right call: the
answer turned out to be «no key at all», which no amount of guessing would have produced.

Per §11, nothing in `KBot.App` depends on any of this.

## Files touched

**New** — `src\KBot.Controls\Adobe\{INativeWindows, IAdobeLauncher, IHostSurface, AdobeHostOptions,
AdobeWindowCapture, AdobeWindowTeardown, AdobeCreationHook}.vb`,
`src\KBot.App\Views\Ddf\AdobeUiPreference.vb`,
`src\KBot.DevHarness\Internal\Adobe\AcroPdfHost.vb`,
`tests\KBot.Controls.Tests\{FakeNativeWindows, AdobeWindowCaptureTests, AdobeWindowTeardownTests,
AdobeReaderHostTests}.vb`, `tests\KBot.DevHarness.Tests\HarnessScenarioHostingTests.vb`

**Moved** — the six registry files listed above, `KBot.DevHarness` → `KBot.Controls`

**Modified** — `AdobeReaderHost.vb` (capture/teardown/options/launched-PID set),
`AdobeWindowHosting.vb` (`RestoreStandalone` **deleted**; `FindReaderWindow` delegates to the new
capture; `BuildArguments` extras overload), `AdobeNativeMethods.vb` (`PostMessage`, `WM_CLOSE`,
`SetWinEventHook`, `UnhookWinEvent`), `ReaderHostPreview.vb` + `.Designer.vb`,
`AdobeReaderHarnessForm.vb` + `.Designer.vb`, `HarnessScenario.vb`, `RegistryWriteVerifier.vb`,
`FakeRegistryAccess.vb`, `AdobeHarnessLayoutTests.vb`, `KBot.DevHarness.Tests.vbproj`
(project-wide `<Import Include="KBot.Controls" />`)

**Modified (build hygiene)** — `KBot.Xfa.vbproj`, `KBot.App.vbproj`, `KBot.Xfa.Tests.vbproj`,
`KBot.App.Tests.vbproj`: NU1701 suppression, see the test-results section.

**Versions** — `KBot.Controls` 1.1.0.0 → 1.2.0.0, `KBot.App` 1.0.4.0 → 1.0.5.0,
`KBot.DevHarness` 1.0.7.0 → 1.0.8.0. `KBot.Common` **unchanged** — it was not touched.
`KBot.Xfa` **not bumped**: only its .vbproj warning settings changed, not a single line of its code.

## Test results

* `dotnet build KBot.sln` — **0 errors, 0 warnings of any kind.** The 16 long-standing NU1701
  warnings were silenced in this pass at the operator's request: `NoWarn="NU1701"` on the iTextSharp
  `PackageReference` in `KBot.Xfa`, plus a `<NoWarn>` property in the four projects that reach
  **BouncyCastle**, which arrives *transitively* through iTextSharp and therefore cannot be covered
  by the package-level attribute (different package id). The suppression is deliberately NOT a
  `Directory.Build.props` blanket — the other eight projects still warn normally, and a new
  .NET Framework package added tomorrow will still be reported. **What it costs:** we will no longer
  be told these two packages are not native to .NET 8. Acceptable because both versions are pinned
  (the same ones as the original XFA_WRITTER exe the engine was ported from, slice 0019) and the
  surface used is covered by `KBot.Xfa.Tests`. If either version ever changes, remove `NoWarn` first
  so the warning can be heard again — that note is in the .vbproj itself, not only here.
* `dotnet test KBot.sln` — **694 passed / 0 failed / 0 skipped**, up from 663 (+31).
  `KBot.Controls.Tests` 201 → 225, `KBot.DevHarness.Tests` 153 → 160.

All nine tests the brief's §9 asks for exist:

1. no Adobe → `AdobeMissing`, **nothing started** · 2. the style mask clears all six bits and sets
`WS_CHILD` · 3. capture ignores a foreign PID even when class *and* title match · 4. **capture
accepts an invisible window** (the flash regression) · 5. a newer generation supersedes the older,
which returns `Superseded` and kills **only its own** process · 6. mode A never kills a PID it did
not launch · 7. mode B falls back to A after `CloseGraceMs` · 8. **teardown never calls `SetParent`
back and never restores the style** — asserted for *both* modes, since this is the exact defect ·
9. `hosting` round-trips and a scenario file without the section still loads.

One existing test was updated rather than worked around: `AllTwelveSections_ArePresent_InOrder`
became `AllFourteenSections_ArePresent_InOrder`. It is a real guard on section order and it caught
the change, which is what it is for.

## Anything left unverified or deferred

* **THE CAPTURE PATH HAS NOW BEEN RUN ONCE ON A REAL MACHINE — AND IT FAILED.** See §1. The fix is
  in, but it too is unverified: nobody has yet confirmed on screen that the window becomes a child
  again. **This is the first thing to check.**
* **NOTHING ELSE HERE HAS RUN AGAINST A REAL ADOBE.** The tests prove call *order* and call
  *absence* against a fake desktop; teardown's `PostMessage`, the creation hook's `SetWinEventHook`
  and both detach modes have still not been executed once.
* **A process lesson worth keeping:** the failure was not caught by the build, by 693 passing tests,
  or by review — it needed one run. The tests were written from the same wrong assumption as the
  code, so they confirmed it instead of challenging it. The evidence that settled it was a log file
  in `bin\`, not anything in the repository.
* **The brief's §10 definition of done cannot be signed off from here.** «No stray window», «no
  taskbar button», «the window is not seen before it is inside `pnlHost`» and «the residual flash is
  measured» are all **on-screen observations**. The code paths that produced both defects are gone
  and their absence is pinned by tests — that is a different and weaker claim than «the defects are
  observed fixed».
* **The residual flash is narrowed much less than hoped, and now there is a number.** Every capture
  in the fixed build matched `după titlu` at **1465–1567 ms** — because on this machine Adobe hands
  the document to a running instance, so the match waits for that instance to set the document
  title, which it plausibly does around the moment it shows the window. Hiding on match still helps;
  claiming more than that would be wrong. (For contrast, the broken PID-only build matched in
  **42–51 ms** — because it was seizing the wrong window.)
* **`CaptureDelayMs` defaults to 0 and no threshold has been found.** Whether reparenting a
  half-initialised Adobe window causes trouble is the open question the spinner exists to answer.
* **The creation hook ran, and it does do something:** `Cârlig de creare eliberat (a ascuns 1
  ferestre)`. First evidence it is not inert. Whether it measurably shortens the flash is still
  unmeasured, and it stays off by default.
* **MODE B DOES NOT WORK ON THE OPERATOR'S MACHINE.** Observed twice: `Închidere (B): fereastra nu
  s-a închis în 1500 ms și procesul 9828 nu e al nostru — nu îl opresc.` Adobe did not honour
  `WM_CLOSE` on the reparented foreign window. **And mode A cannot close it either** — correctly
  refusing to kill a process it did not start, it degraded to a close that also failed:
  `rămâne copil al panoului și dispare odată cu el`. So under the single-instance handoff **neither
  mode can close the window**; it only dies when the host panel does. Mode A remains the right
  shipped default. Whether B behaves differently against a window K-BOT actually launched (`/n`
  honoured) is still unverified, as is the new Acrobat UI.
* ~~The AcroPDF verdict is NOT recorded~~ — **RECORDED and POSITIVE, see §3**, and the
  same-document collision is confirmed as the whole explanation of the one failure.
* ~~Do the ActiveX chrome calls work?~~ **ANSWERED: NO** (all five return OK and change nothing).
  ~~Does hiding by text work?~~ **ANSWERED: NO** — hiding leaves the width and Adobe never reflows.
  ~~Does pressing Adobe's own collapse button work?~~ **ANSWERED: YES, measured.** See §3.
* **`WakeAcroLayout` has not been run.** Which of its three steps clears the grey first document —
  focus, size change, or synthetic click — is unknown, and that is the whole point of it reporting
  which one fired. Only the operator's manual click is known to work.
* **The AcroPDF route is now a genuine candidate, and that is a decision, not a code change.** It
  renders XFA, switches documents, reaches the wanted chrome state with one synthetic click, and
  that state persists for the session. Against window hosting it looks better on every axis measured
  so far. **Nothing in `KBot.App` depends on it (§11) and nothing should until somebody decides
  that deliberately** — the remaining unknowns are whether the collapse click is reliable across
  Adobe versions, and what happens when the host panel is resized.
* **The §5 `bEnableAv2` table is empty.** Whether the opt-out is still honoured, per UI generation,
  has not been observed. `AdobeUiPreference` is inert (`ENFORCE_LEGACY_UI = False`), so its
  snapshot/restore path — including «absent restores to deletion» — has never executed in the app;
  only `RegistrySnapshotSet`'s own tests cover that rule.
* **The bench has not been re-run on screen** after this refactor, same as after 0024-01.
* Carried forward from 0024-02 and untouched here: the «relaunch once» path is still untested at run
  time, and `newInstance = false` on the modern profile **still owes a decision** on whether the
  shipped default should force `/n`. This pass makes that decision *safer* (a foreign window is never
  killed) but does not make it.
* Keyboard and focus across the process boundary remain not-native and are still not fixed — the same
  documented position as slice 0020-03.
