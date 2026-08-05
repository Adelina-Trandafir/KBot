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

## §1 One correction the brief could not have anticipated

Brief §3.3 says capture must `skip unless pid = ourPid`. Applied literally that **breaks the modern
profile**: `AdobeViewerProfiles.Modern` launches *without* `/n` — a measured value transcribed from
the bench in 0024-01, not a preference — so Adobe hands the document to an instance the operator
already had open and **our process owns no window at all**. A strict PID filter would return
`WindowNotFound` for every modern-profile document.

Implemented instead as **PID-first, with a logged title fallback permitted only when the launch
profile omitted `/n`**. The result carries `AdobeCaptureMatch.ByPid` or `.ByTitle`, and a `ByTitle`
match is what later forbids killing that process. Both halves are pinned by tests.

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

**Versions** — `KBot.Controls` 1.1.0.0 → 1.2.0.0, `KBot.App` 1.0.4.0 → 1.0.5.0,
`KBot.DevHarness` 1.0.7.0 → 1.0.8.0. `KBot.Common` **unchanged** — it was not touched.

## Test results

* `dotnet build KBot.sln` — **0 errors, 0 BC warnings** (32 NU1701 on a rebuild = the 16 pre-existing
  iTextSharp/BouncyCastle ones counted across both passes; unchanged).
* `dotnet test KBot.sln` — **693 passed / 0 failed / 0 skipped**, up from 663 (+30).
  `KBot.Controls.Tests` 201 → 224, `KBot.DevHarness.Tests` 153 → 160.

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

* **NOTHING HERE HAS RUN AGAINST A REAL ADOBE, AND NOTHING HAS BEEN SEEN ON SCREEN.** This is the
  same standing caveat as 0024-01/02 and it applies to every claim below. The tests prove call
  *order* and call *absence* against a fake desktop; `SetParent`, `ShowWindow`, `PostMessage` and
  `SetWinEventHook` have not been executed once in this pass.
* **The brief's §10 definition of done cannot be signed off from here.** «No stray window», «no
  taskbar button», «the window is not seen before it is inside `pnlHost`» and «the residual flash is
  measured» are all **on-screen observations**. The code paths that produced both defects are gone
  and their absence is pinned by tests — that is a different and weaker claim than «the defects are
  observed fixed».
* **The residual flash is narrowed, not measured.** Hiding moved from «after the window is visible»
  to «the instant it is matched», and polling from 150 ms to 30 ms. By how much that shows up on the
  operator's machine is unknown. The creation hook may narrow it further but, being an out-of-context
  WinEvent, **cannot be promised to eliminate it** — which is why it defaults to off.
* **`CaptureDelayMs` defaults to 0 and no threshold has been found.** Whether reparenting a
  half-initialised Adobe window causes trouble is the open question the spinner exists to answer.
* **Mode B has never closed a real window.** Whether Adobe honours `WM_CLOSE` on a reparented child,
  and whether the process survives when it was the last document window (in which case B degenerates
  to A), are both unverified. Behaviour on the new Acrobat UI is unverified.
* **The AcroPDF verdict is NOT recorded** — the section exists, reads the CLSID at runtime and logs,
  but nobody has run it. **The question it exists to answer is still open: does a real XFA DDF
  render, or does the Adobe «please wait…» placeholder appear?** Whether the control is even
  registered on the operator's machines is also unknown; «not registered» is a valid answer the
  section will report.
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
