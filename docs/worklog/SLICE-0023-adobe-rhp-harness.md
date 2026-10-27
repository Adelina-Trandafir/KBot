# SLICE-0023 — Istoricul bancului Adobe RHP

Consolidare D001-10: cinci workloguri, etape 1, 2, 4, 5 și 6. Numerele originale și probele rămân pe etapă; nu există un jurnal separat pentru pasul 3 în această familie. Instrucțiunile istorice de mai jos nu autorizează operații în sesiunea documentară curentă.

Important pentru lectură: pasul 2 are resturi ale descrierii inițiale (`chkApplyOnLoad`, `document.path`), deși începutul aceluiași pas consemnează eliminarea lor; acele resturi sunt istorice. Pasul 4 schimbă prioritatea userPrefs, pasul 5 introduce rândurile literale și restaurarea HKCU implicit oprită. Verdictul de bază curată este corectat ulterior; randarea formularului nu dovedește efectul asupra Adobe.

- [Pasul 1](#slice-0023-pas-1)
- [Pasul 2](#slice-0023-pas-2)
- [Pasul 4](#slice-0023-pas-4)
- [Pasul 5](#slice-0023-pas-5)
- [Pasul 6](#slice-0023-pas-6)

## SLICE-0023 — Adobe right-hand pane (RHP): every removal option, in the harness only <a id="slice-0023-pas-1"></a>

Slice number confirmed against `KBOT_STATUS.md` ("Next free slice number: 0023" — unchanged when this pass started). Plan: pasted in-session ("PLAN — Adobe right-hand pane (RHP)").

### What changed and why

The operator cannot remove Adobe's right-hand Tools pane ("Export PDF" / "Start Free Trial") from the embedded Reader window. The `/A` open parameters already in the harness address *document* chrome; the RHP is *application* chrome and has no open parameter. Which of the four candidate levers works depends on the Adobe build and cannot be settled from the repo — so this pass builds **all four** into the existing `AdobeReaderHarnessForm` (DevHarness → categoria "Adobe/PDF") for the operator to judge on screen, on both the classic Reader DC UI and the modern ("new Acrobat") UI:

- **§3.A Diagnostic** — `btnProbe` walks the hosted window's children (GW_CHILD/GW_HWNDNEXT recursion, depth ≤ 4) and logs per node: depth, HWND, class, text (truncated 40), rect in `pnlHost` client coordinates, size, visibility, style/exstyle. Output goes to the harness log AND `AppDir\Logs\test_adobe_rhp.log` (timestamped, appended). The status line names the RHP *candidate* — visible, flush against the host's right edge (±8 px) and narrower than half the host width, widest wins — explicitly labelled a HEURISTIC in the log. This probe decides everything else: discrete HWND ⇒ §3.C wins; no HWND ⇒ §3.B is the only deterministic option.
- **§3.B Decupare** — geometry clipping, live (no relaunch, no kill): with `chkClip` on, the hosted window is positioned at `(0, -sus)` sized `(lățime gazdă + dreapta, înălțime + sus)`, so the clipped bands fall outside `pnlHost`. `btnClipAuto` copies the probe candidate's width into `numClipRight`. Implemented as a single `HostedBounds()` used by both `LayoutHostedWindow` and `NudgeRedraw` (the existing redraw-nudge sequence is reused — Adobe lays out late and will not repaint on its own).
- **§3.C Ferestre copil** — `lstChildren` (populated by the probe; class + size, `Tag`=HWND) + hide/show/restore-all via `ShowWindow`. Hidden handles tracked for exact restore; every call guarded with `IsWindow`, dead handles dropped with a log line, `NudgeRedraw` after each change.
- **§3.D Scurtături** — Shift+F4 / F4 sent to the hosted window (`SetFocus` + `SendKeys`), logged as EXPERIMENTAL and version-dependent; "nothing happened" is a valid recorded result (no retries, per plan).
- **§3.E Preferințe Adobe (HKCU)** — the three community-sourced RHP values (`bExpandRHPInViewer=0`, `bRHPSticky=1`, `aDefaultRHPViewMode_L="Collapsed"`) plus the Adobe-documented viewer-generation switch (`bEnableAv2=0`), written to the auto-detected AVGeneral hive (Reader DC vs Adobe Acrobat; `cboHive` can override). Apply order per plan: once-per-session snapshot → kill our Adobe by PID + ask (Romanian, never silent) before killing foreign Adobe → write with old→new logging → relaunch via the existing `RelaunchAsync`. Restore (button and on-close, `chkRestoreOnClose` default checked) replays the snapshot exactly: **absent restores to DELETION, never to 0**; wrong-type originals are written back verbatim.
- **§3.F Politici Adobe (HKLM)** — `bAcroSuppressUpsell=1` + `cServices\bToggleAdobeDocumentServices=1` under `HKLM\SOFTWARE\Policies\Adobe\<product>\DC\FeatureLockDown`, applied by generating `adobe_policy_apply.reg` / `adobe_policy_revert.reg` into `AppDir\Logs\` (UTF-16; revert built from pre-apply reads, with `"name"=-` deletion lines for absent values) and importing via `reg.exe` with `Verb=runas` — K-BOT itself is never elevated. UAC cancel (Win32Exception
  1223) shows "Operația a fost anulată de utilizator." with full detail only in the log.
- **§3.G** — `lblStatus` always shows one state block: hive in use, clip values, hidden-child count, HKCU values applied, HKLM applied this session, plus the last action message.

Pure logic extracted behind an I/O seam so tests never touch the real registry (`Internal/Adobe/`): `AdobeRegistryConstants` (the §2 names, transcribed — never from memory), `RegistryValueSnapshot` + `RegistrySnapshotSet` (three-state snapshot: absent / present-with-value / wrong-type-as-present-verbatim; capture idempotent per session), `RegFileBuilder` (header, `dword:` hex, deletion lines, escaping), `AdobeHiveResolver` (hive/product from key existence + exe name), `IRegistryAccess` (seam) and `WinRegistryAccess` (the real HKCU/HKLM-read boundary; logs + rethrows per house rule).

### Files touched

- `src/KBot.DevHarness/Internal/AdobeReaderHarnessForm.vb` — all §3 sections (see above)
- `src/KBot.DevHarness/Internal/AdobeReaderHarnessForm.Designer.vb` — ~30 new controls, all declared here (house rule); flowLeft widened 320→360
- `src/KBot.DevHarness/Internal/Adobe/AdobeRegistryConstants.vb` — new
- `src/KBot.DevHarness/Internal/Adobe/RegistryValueSnapshot.vb` — new
- `src/KBot.DevHarness/Internal/Adobe/RegistrySnapshotSet.vb` — new
- `src/KBot.DevHarness/Internal/Adobe/RegFileBuilder.vb` — new
- `src/KBot.DevHarness/Internal/Adobe/AdobeHiveResolver.vb` — new
- `src/KBot.DevHarness/Internal/Adobe/IRegistryAccess.vb` — new
- `src/KBot.DevHarness/Internal/Adobe/WinRegistryAccess.vb` — new
- `tests/KBot.DevHarness.Tests/` — **new test project** (`KBot.DevHarness` had none; the pure helpers live in `KBot.DevHarness/Internal/Adobe/` and are covered from here): `RegistrySnapshotSetTests` (6), `RegFileBuilderTests` (8), `AdobeHiveResolverTests` (8), `FakeRegistryAccess` (in-memory seam)
- `KBot.sln` — KBot.DevHarness.Tests added under the Tests folder
- `src/KBot.DevHarness/KBot.DevHarness.vbproj` — FileVersion 1.0.2.0 → 1.0.3.0

Out-of-scope files NOT touched, per plan: `ReaderHostPreview.vb`, `IDdfPreview.vb` / `DdfPreviewFactory` (mode constant stays `XfaXml`), installer/publish/startup. No registry write happens outside an explicit button press in this harness.

### Test results

- `dotnet build KBot.sln` — 0 errors, 0 BC warnings (the 16 NU1701 iTextSharp/BouncyCastle warnings pre-date this slice).
- `dotnet test KBot.sln` — **458 passed / 0 failed / 0 skipped** across 9 projects (was 436; +22 new: DevHarness.Tests 22).

### What the operator will record (empty — fill in on screen)

For each of: classic Reader DC, and the modern ("new Acrobat") UI —

| # | Question | Classic DC | Modern UI |
|--:|----------|------------|-----------|
| 1 | Is the RHP a discrete child HWND in the probe output? Class name and width. | | |
| 2 | Does hiding that child remove the pane, and does it stay hidden when the document changes? | | |
| 3 | Does clipping work, and at what `numClipRight` value? | | |
| 4 | Does Shift+F4 do anything? | | |
| 5 | Do the three HKCU RHP values collapse the pane, and does the collapse survive opening a document? | | |
| 6 | Does `bEnableAv2 = 0` change the answer to 5? | | |
| 7 | Does the HKLM policy pair remove the pane, or only its contents (Export PDF, trial upsell)? | | |

### Anything left unverified or deferred

- **Nothing in plan §2 has been tested on a real machine.** Every registry key/value name is transcribed from the plan (Adobe ETK + community threads). If a name is absent on the operator's build, that absence is the result to record here — no alternative names were invented (plan §8). The known community-reported failure (Adobe overwriting `aDefaultRHPViewMode_L` with a binary zero on document open; no effect on Acrobat Pro 2023.003) is exactly what the bench exists to observe.
- **No visual verdict** — the new sections have never been rendered on screen (like the rest of the harness form). The whole slice is a human-verdict bench; code green only.
- **`AdobeUtils.GetAdobeReaderPath` / `IsAdobeViewer` (KBot.Xfa) were NOT reused** although the plan suggested it: both are `Private` in `AdobeUtils`, and DevHarness does not reference KBot.Xfa — making them reusable means touching KBot.Xfa, which this pass does not do. The harness keeps its own App-Paths-based resolver (documented in code). If reuse is wanted, a later pass should make them `Public Shared` in KBot.Xfa and swap the harness over.
- **Deliberate deviation (on-close restore):** foreign Adobe processes are NOT killed during the on-close restore — there is no consent dialog at that point and the plan's own rule ("never kill the operator's own open documents silently") wins; instead their presence is logged with the warning that the restore may be overwritten when they exit. On restore failure the operator gets a MessageBox naming the exact keys (lblStatus alone would never be seen on a closing form) — the plan asked for lblStatus; both are done.
- **Exotic HKLM originals:** a pre-apply HKLM value that is neither DWORD nor REG_SZ cannot be represented in the generated revert `.reg`; it is logged as needing manual revert instead of being guessed at.
- The keyboard shortcuts (§3.D) are labelled experimental in code and log; no retries by design.

## SLICE-0023 (pass 2) — Adobe harness: scenario files + resizable docked layout <a id="slice-0023-pas-2"></a>

Continues slice **0023**; the number was NOT changed and the previous worklog ([SLICE-0023-adobe-rhp-harness.md](#slice-0023-pas-1)) stays as it is. Plan: pasted in-session ("PLAN — Adobe harness: scenario files + resizable docked layout").

**No new RHP lever was added in this pass.** Everything that existed keeps working exactly as it did; it became scriptable and resizable.

### What changed and why

#### 1. Scenario files (§3)

Every test used to be a manual click sequence: the combination that worked (Shift+F4 to collapse, then hide `AVTaskPaneHostView`) existed only as a log trace plus a memory of which buttons were pressed in which order. It is now a file that can be repeated, sent to someone else, or tried on another machine.

- **A scenario is a SETTINGS recipe, never a document** (operator clarification, applied after the first cut): loading one **pre-sets the controls in the left panel**, and the PDF chosen afterwards with «Deschide PDF…» opens under those settings. The `document` section is gone from the schema; saving no longer writes a path. From load onward the **panel is the single source of truth** — the scenario is never consulted again while launching, so what is ticked is what runs and the operator can still adjust before opening a file. Files written against the earlier schema still load: `document` lands in the unknown-property bucket and is reported as a warning. The `chkApplyOnLoad` switch was removed with it — loading always applies now, so unticking it would have meant "loading does nothing". Picking a file launches with those switches and re-applies the hidden child windows; the rest of the steps (Shift+F4 and friends) run when «Rulează scenariul» is pressed.
- **JSON via `System.Text.Json`** (already in .NET 8, no package reference), read with `ReadCommentHandling = Skip` and `AllowTrailingCommas = True` because these files are written by hand with comments; written with `WriteIndented = True` so saves are readable and diffable.
- **Children are addressed by window TEXT, never by HWND.** The probe log proves handles change on every launch (`0x5083E` → `0x20B66`) while the text `AVTaskPaneHostView` stays identical; class names are useless here too — nearly everything is `AVL_AVView`.
- **Absent ≠ disabled.** Every section is a reference type (Nothing when absent) and every scalar inside it is `Nullable`, so "no `clip` object" (leave alone) stays distinguishable from `"clip": { "enabled": false }` (turn off). This is the property §5 pins with tests.
- **Ten steps**, each mapping to the code path already behind the corresponding button — no step contains new logic. To make that literally true, three existing paths were *extracted*, not re-implemented: `RelaunchAsync` now = `StartFreshLaunch()` + `CompleteEmbedAsync()` (which the `launch` / `waitForEmbed` steps use separately), `btnApplyUser` = `ApplyUserPrefsCore()`, `btnApplyMachine` = `ApplyMachinePolicyCoreAsync()`. The generation counter, `NudgeRedraw` and the force-kill-by-PID path are untouched.
- **Unknown step name aborts the run** with a Romanian message naming the bad step *and* the valid list — a typo in a file sent from outside is loud, never silently skipped. Unknown *properties* are the opposite: captured via `JsonExtensionData` and reported as warnings, so a newer file still runs on an older bench.
- **Failure stops the run where it happened**, logged, with no automatic rollback — half-rolled-back state is harder to read than the failure.
- **Safety (§3.3):** `machinePolicy.apply` defaults to false and the `applyMachinePolicy` step is skipped with a log line when it is false, even if the step appears in `scenario`. Any run whose steps include `applyUserPrefs` or `applyMachinePolicy` shows one up-front Romanian confirmation listing **every path and value, current value beside the new one**; Cancel abandons the whole run. Loading a file never writes anything — only Run does. The HKCU snapshot rules from pass 1 are unchanged (absent restores to deletion, snapshot once per session).
- **Re-apply after relaunch (§3.4):** because HWNDs change, every embed re-runs the probe, re-resolves `byText` and hides what matches, retrying `reapplyAttempts` × `reapplyIntervalMs` (Adobe creates the task pane host *after* the main view). Each attempt logs `n of m found` — `0 of 1` repeated ten times is exactly the diagnostic needed when a scenario stops working on a different Adobe build.
- **New "Scenariu" section (§3.5)** directly under "Diagnostic": file name label, Încarcă / Rulează (disabled until loaded) / Salvează, and `chkApplyOnLoad` (default checked). Save writes the current state of every control back out in the same schema **including the texts of the child windows hidden right now** — the round trip that matters. Saved files always carry `machinePolicy.apply = false`, so a file can never write machine-wide policy just by being run somewhere else.

#### 2. Layout rework (§2)

> **This shipped broken and was fixed afterwards — see “Defect found on screen” below.**
> The description here is the corrected end state.

The fixed-width strip is replaced by: `splitMain` (`SplitContainer`, vertical, `SplitterDistance` 470, `Panel1MinSize` 300, `FixedPanel = None`, draggable at runtime) → Panel1 holds **`flowOptions`** (a `FlowLayoutPanel`: `AutoScroll`, `FlowDirection = TopDown`, `WrapContents = False`); Panel2 holds `tlpRight` (row 0 = 100% `pnlHost`, row 1 = AutoSize `lblStatus`). Each of the eleven sections is a `GroupBox` + inner `TableLayoutPanel`; the clip section uses two columns (AutoSize label + 100% input). `lblCmd` became **`txtCmd`** (multiline, vertical scrollbar, `MinimumSize` height 72) and `lstChildren` got `MinimumSize` height 120, so neither collapses when the splitter goes narrow. Pass/Fail stayed where the harness framework puts them.

Sections do **not** dock (a `FlowLayoutPanel` ignores `Dock` on its children). Their width is tracked to the panel in `SizeSections` (`AdobeReaderHarnessForm.vb`), called from the ctor and from `flowOptions.SizeChanged` — so also on every splitter drag.

The three consequences the plan said to handle rather than discover:

- Clip geometry is computed from `pnlHost.ClientSize`, which now changes with the splitter — it is re-applied on `pnlHost.Resize` **and** `splitMain.SplitterMoved`, not only on spinner changes.
- Both are **debounced** through a 150 ms `tmrLayout` restarted on every event; geometry and `NudgeRedraw` run once when it fires, never per resize tick.
- The debounce and the tick both guard on `_hostedWindow <> IntPtr.Zero`.

Hidden children survive a resize but not a relaunch — handled by §3.4 above; `KillTracked` now clears the hidden *handles* and the probe list but **keeps the hidden texts**, which are the durable identity a scenario re-applies after the next embed.

#### 2b. Defect found on screen — the options panel did not scroll

The layout above was first shipped with `tlpOptions` as a **`TableLayoutPanel`** with `AutoScroll = True` and twelve rows: eleven AutoSize plus a final **`Percent 100` filler**. A percent row absorbs whatever space is left, so the table always reported that its content fitted its client area, **never showed a scrollbar, and silently clipped every section past the fold**. The eleven sections stack to ~1470px in a ~740px panel, so the operator could not reach «Preferințe Adobe (HKCU)» or «Politici Adobe (HKLM)» **at all** — the two registry sections this whole slice exists for. `SplitterDistance` was also set to 320 when the operator's own designer-regenerated file measured `chkNewInstance` at 427 and `chkDisableServices` at 412, so captions were truncated as well.

Neither the build nor the 475 tests noticed, because nothing exercised the layout. It was found by the operator, on screen, after I twice reported the pass as landed.

Fix: `tlpOptions` → `flowOptions` (`FlowLayoutPanel`, TopDown, AutoScroll, no wrapping — the container that already scrolled this same content before the rework), the percent filler row gone with the table, `SplitterDistance` 320 → 470, `Panel1MinSize` 260 → 300, and the GroupBoxes' width pinned in `SizeSections` via `MinimumSize`/`MaximumSize` rather than `.Width` (setting `.Width` on an `AutoSize` control is futile — the layout engine recomputes it from the content and the section overflows the panel sideways; pinning min = max leaves AutoSize governing height only).

Verified by rendering the real form: panel client 453×740, **vertical scrollbar visible** (`Maximum` 1467), **no** horizontal scrollbar, all eleven sections at a uniform 426px, `grpUser` at top=979 and `grpMachine` at top=1241 — both past the fold and both fully reachable by scrolling, with every checkbox and button inside them visible and correctly captioned.

#### 3. Theming (§6) — checked, no fix needed

`ThemeManager` already handles every new container type: `SplitContainer` (its own `BackColor` plus `Panel1`/`Panel2`, and `Traverse` recurses into both panels explicitly), `GroupBox` (back + fore), `TableLayoutPanel` and `TextBox` (`ThemeManager.vb:172-194`, `:218-242`, `:234`, `:321`). Verified by reading the theme engine, not by rendering — see "unverified" below.

### Files touched

- `src/KBot.DevHarness/Internal/AdobeReaderHarnessForm.Designer.vb` — full layout rework; ~40 controls re-declared (SplitContainer, 2 outer + 11 inner TableLayoutPanels, 11 GroupBoxes, the five Scenariu controls, `txtCmd`, `tmrLayout`); `lblSec*` labels and `flowLeft` removed (GroupBox captions replace them); `components` + `Dispose` added for the timer
- `src/KBot.DevHarness/Internal/AdobeReaderHarnessForm.vb` — scenario load/run/save + the runner, the `RelaunchAsync` split, debounced layout refresh, hide-by-text with retries, the up-front registry confirmation, `SetControlsEnabled` now toggles `tlpOptions`
- `src/KBot.DevHarness/Internal/Adobe/HarnessScenario.vb` — **new**: the config types + `HarnessScenarioSteps`
- `src/KBot.DevHarness/Internal/Adobe/HarnessScenarioReader.vb` — **new**: reader/writer pair + `HarnessScenarioReadResult` (`IsValid` / `Errors` / `Warnings`, no message boxes inside)
- `src/KBot.DevHarness/Config/rhp_collapse_then_hide.json` — **new** sample scenario (the §0 combination), copied to `<AppDir>\Config\` with `PreserveNewest`
- `src/KBot.DevHarness/KBot.DevHarness.vbproj` — Config copy rule; FileVersion 1.0.3.0 → 1.0.4.0
- `tests/KBot.DevHarness.Tests/HarnessScenarioTests.vb` — **new**, 17 tests

Out of scope and untouched, as in pass 1: `ReaderHostPreview`, `IDdfPreview`, `DdfPreviewFactory` (mode constant stays `XfaXml`), installer, publish, startup paths.

### Test results

- `dotnet build KBot.sln` — 0 errors, 0 BC warnings (16 pre-existing NU1701 from iTextSharp/BouncyCastle).
- `dotnet test KBot.sln` — **491 passed / 0 failed / 0 skipped** (was 458; +19 scenario model, +8 layout regression, +6 scenario→panel binding). `KBot.DevHarness.Tests` is now 55.
- Sample scenario confirmed landing in `bin\Debug\net8.0-windows\Config\`.
- **The form was rendered and looked at** (see §2b) — the step that was missing the first time.

`AdobeHarnessScenarioBindingTests` (6, STA) pin the corrected contract by driving the form's real load path: loading sets the launch/chrome switches, sets the clip spinners, ticks **only** the registry values the file names, **leaves the document untouched** (the command preview still says `<alege un PDF>`), the preview reflects the scenario's switches, and an operator adjustment made after loading wins over the file.

`AdobeHarnessLayoutTests` (8, STA — same `RunSta` pattern as `SumarViewTests`) is the guard the clipping defect deserved: the options panel must be a scrolling FlowLayoutPanel and not a TableLayoutPanel; all eleven sections present in order; content taller than the panel **must** produce a vertical scrollbar; both registry sections must scroll fully into view; all thirteen registry controls must exist with non-zero size; sections must track the panel width without forcing a horizontal scrollbar; and the splitter must be at least as wide as the widest caption.

Scenario tests cover exactly what §5 asked: full-field round trip (serialize → deserialize → serialize unchanged), absent-section ≠ disabled-section (both directions), unknown property → warning not exception, unknown step → `IsValid = False` naming the offender, `machinePolicy.apply` absent → false, malformed JSON → Romanian error with no exception escaping. Plus comments/trailing commas accepted, schema-version handling, and the writer omitting absent sections so a saved file keeps its meaning.

### Anything left unverified or deferred

- **Process failure, recorded deliberately:** this pass was reported as landed twice while the layout had never been rendered. The panel did not scroll and both registry sections were unreachable (§2b). "Builds clean and the tests are green" said nothing about it, because no test touched the layout — that gap is now closed by `AdobeHarnessLayoutTests`. A UI change is not done until it has been on screen.
- **Rendered, but not click-tested.** The screenshots prove the sections are laid out, scrollable and correctly captioned; they do not prove that dragging the splitter to its narrowest, or running at a different DPI, behaves well. The regression tests pin the width/scroll invariants, not the feel.
- **The scenario runner has never been run against a real Adobe.** The step→handler mapping is green offline and the model is well covered, but `launch`/`waitForEmbed`/`hideChildren` have not executed once against a live window.
- **Theme correctness for the new containers was verified by reading `ThemeManager`, not by rendering.** No theme change was needed; if a `GroupBox` border reads wrong on Dark in practice, the fix belongs in `KBot.Theming`, not here.
- **Registry key names are still the pass-1 ones and still unverified on a real machine** (plan §2 of the previous pass). Nothing in this pass changes that; the empty operator table stays in [SLICE-0023-adobe-rhp-harness.md](#slice-0023-pas-1).
- **The `keys` step reuses the experimental cross-process `SendKeys` path**, documented since pass 1 as not behaving like a native child. `ToSendKeysSyntax` translates `"Shift+F4"`/`"Ctrl+..."` specs; unrecognised modifiers fall through as literal text rather than being guessed at.
- **`machinePolicy.values` accepts only numeric (REG_DWORD) values**; anything else is logged and skipped rather than guessed. Same for `userPrefs.values` outside number/string.
- **Addition beyond the plan's letter:** the sample scenario file (`Config/rhp_collapse_then_hide.json`) and its copy-to-output rule. The plan specified where scenario files live but shipped none; this makes the §0 combination a real artifact instead of a memory. Its `document.path` is a placeholder that must be pointed at a real PDF.

## SLICE-0023 (pass 4) — Adobe harness: make the bench tell the truth <a id="slice-0023-pas-4"></a>

Continues slice **0023**. Previous worklogs ([SLICE-0023-adobe-rhp-harness.md](#slice-0023-pas-1), [SLICE-0023-adobe-harness-config-layout.md](#slice-0023-pas-2)) stay as they are.

**No new RHP lever.** This pass is about the bench being trustworthy.

### §0 Why — four runs that reported success and tested nothing

Four scenarios were run on 04.08. All four reported success. Confirmed from `Logs\test_adobe_rhp.log`, present in **every** probe of all four runs:

```
d=3 hwnd=0x120C96 cls=AVL_AVView text="AVTaskPaneHostView" x=0 y=1 0x0 vis=0
14 ferestre copil; niciun candidat RHP după euristic.
Ascuns copil: AVTaskPaneHostView — AVL_AVView (0x0) (hwnd=0x120C96)
hideChildren: încercarea 1/20 — 1 din 1 texte găsite.
```

The task pane host was **0×0 and already invisible before the scenario started**. `hideChildren` hid a window that was already hidden and called it a clean success. On 03.08 the same window was 346×1440 and visible, so this was a change in machine state, not in Adobe.

Three defects, all now closed:

1. **`hideChildren` could not distinguish a real hide from a no-op** — "1 din 1 texte găsite" is true and useless.
2. **`userPrefs` values were clamped by the checkboxes.** Scenario 02 asked for `"bEnableAv2": 1`; the confirmation dialog offered `0 -> 0` and the log recorded `0 (DWord) -> 0 (DWord)` (`test_adobe_rhp.log`, 09:02:33). Half the schema was decorative.
3. **The machine was contaminated and nothing said so.** Traced in the run logs: `test_20260803_120648_861.log` applied the «Acrobat Reader» policy **and reverted it**; `test_20260803_120952_713.log` applied the «Adobe Acrobat» policy with **no revert line anywhere after it**. Read live during this pass:

   ```
   HKLM\SOFTWARE\Policies\Adobe\Adobe Acrobat\DC\FeatureLockDown\bAcroSuppressUpsell = 1
   HKLM\...\FeatureLockDown\cServices\bToggleAdobeDocumentServices = 1
   ```

   Still active — almost certainly why the pane is now zero-sized (services suppressed, nothing for the pane to host). **The harness did not revert it and this pass does not revert it either** — it is an elevated write on the operator's machine; the bench now offers one-click revert and refuses to pretend the state is clean.

### §1 `userPrefs` values are applied literally

- New pure `UserPrefIntent` / `UserPrefIntentFactory`: integer → `REG_DWORD`, string → `REG_SZ`, **`null` → delete the value**. Absent key → no intent at all, so "leave alone", "set to 0" and "remove it" are three distinguishable states. Value **names are open** — any value under the resolved `AVGeneral` hive can be driven from a file with no code change.
- `applyUserPrefs` reads from the **scenario**; the checkboxes are merged in only for names the file does not mention (or for all of them when no scenario is loaded) — `UserPrefIntentFactory.Merge`.
- **Write-back verification** (`RegistryWriteVerifier`): after every write the value is read back and compared. Match → `… (verificat)`. Mismatch → `EȘEC: <path>\<name> — cerut X, citit Y`, the run **stops**, and a Romanian message box names the value. Deletions verified as absence.
- Confirmation dialog shows the **true** intended value per row — `hive · nume · valoare curentă · valoare nouă · tip`, deletions as `(șters)`, absent current values as `(absent)`, never as `0`.
- New **`gridPrefs`** (read-only `DataGridView`, columns **Valoare · Cerut · Curent · Tip**) under the checkboxes, refreshed on load and after every write, mismatching rows in the palette's error colour.
- The checkbox shortcuts no longer lie: a shortcut is ticked **only when the scenario asks for exactly what it means**, so `"bEnableAv2": 1` leaves «bEnableAv2 = 0» unticked.

### §2 `hideChildren` reports what it actually did

`HideOutcomeClassifier` classifies each match **before** touching it (afterwards everything looks hidden — which is how the no-op read as success): `ASCUNS` / `DEJA ASCUNS` / `DIMENSIUNE ZERO` / `NEGĂSIT`, one log line each. Summary via `HideAttemptSummary`:

```
hideChildren: încercarea 1/10 — 1 găsit(e): 0 ascunse, 1 deja ascuns/zero. ATENȚIE: nicio schimbare reală.
```

Retries stop early **only on a real hide**; a run that only ever sees `DEJA ASCUNS`/`DIMENSIUNE ZERO` burns through every attempt and ends with a Romanian warning in the log and `lblStatus` that the step changed nothing and therefore proves nothing.

### §3 Machine-state diagnostic

- **`btnMachineState`** — «Starea mașinii (Adobe + registry)»: Adobe path/version/product, both `AVGeneral` hives with `bEnableAv2` / `bExpandRHPInViewer` / `bRHPSticky` / `aDefaultRHPViewMode_L` (value or `(absent)`), both HKLM policy products, and the count of running Adobe processes. Read-only, no elevation.
- **Automatic baseline check** before every scenario, logged under `── Bază de pornire ──`. `BaselineEvaluator` → Clean / Warn / Block. Warn shows the Romanian dialog naming every active value (Continue / Cancel); Block refuses outright when the scenario sets `requireCleanBaseline`.
- **Revert tracking that survives a restart** — `Config\harness_machine_state.json` (`MachineStateMarker`): written on apply with the pre-apply snapshot and the revert `.reg` path, cleared only after a revert that is **verified** to have left no policy value behind. Warned about on open. A **corrupt** marker is "unknown, warn" — never "nothing outstanding".
- **`chkRevertPolicyOnClose`** (default checked): on close, an outstanding policy is reverted through the elevated import. UAC cancelled / import failed / values still present → marker **kept** and the operator told, naming the exact keys.

### §4 Probe output

- A dedicated `PANOU:` line per `hideChildren.byText` target, matched or not: `PANOU: AVTaskPaneHostView — 346x1440, vis=1` / `— 0x0 (posibil învechit), vis=0` / `— NEGĂSIT`.
- Rectangles read from a window with `vis=0` are marked **`(posibil învechit)`** — the log showed `AVExpandCollapseButtonView … 27x913` inside a 587-high host, i.e. stale geometry from an earlier layout.
- When the heuristic finds no candidate **and** a requested target exists with zero size, the summary says so explicitly instead of just «niciun candidat RHP».

### §5 Schema (still version 1, backwards compatible)

- `userPrefs.values` accepts integers, strings and `null`.
- `requireCleanBaseline` (root, default false).
- `machinePolicy.revertOnClose` (default true), applied to `chkRevertPolicyOnClose` on load.

### Files touched

- `src/KBot.DevHarness/Internal/Adobe/UserPrefIntent.vb` — **new** (intents + merge)
- `src/KBot.DevHarness/Internal/Adobe/RegistryWriteVerifier.vb` — **new**
- `src/KBot.DevHarness/Internal/Adobe/HideOutcome.vb` — **new** (classifier + summary)
- `src/KBot.DevHarness/Internal/Adobe/BaselineEvaluator.vb` — **new**
- `src/KBot.DevHarness/Internal/Adobe/MachineStateMarker.vb` — **new**
- `src/KBot.DevHarness/Internal/Adobe/HarnessScenario.vb` — `requireCleanBaseline`, `revertOnClose`
- `src/KBot.DevHarness/Internal/AdobeReaderHarnessForm{.vb,.Designer.vb}` — all of the above wired; new `gridPrefs`, `btnMachineState`, `chkRevertPolicyOnClose`
- `src/KBot.DevHarness/Config/rhp_0{1..4}_*.json` — the operator's four scenarios, now **versioned in the repo** (they only existed in `bin\`) and re-issued with `requireCleanBaseline: true`
- `src/KBot.DevHarness/Config/rhp_collapse_then_hide.json` — same flag
- `tests/KBot.DevHarness.Tests/HarnessTruthTests.vb` — **new**, 31
- `tests/KBot.DevHarness.Tests/ShippedScenarioFilesTests.vb` — **new**, 3
- `tests/KBot.DevHarness.Tests/AdobeHarnessScenarioBindingTests.vb` — +2

### Test results

- `dotnet build KBot.sln` — 0 errors, 0 BC warnings.
- `dotnet test KBot.sln` — **527 passed / 0 failed / 0 skipped** (was 491). `KBot.DevHarness.Tests` 55 → 91.
- **Rendered and inspected on screen** with `rhp_02_interfata_moderna.json` loaded — the scenario that was silently clamped. `gridPrefs` shows `bEnableAv2 | cerut 1 | curent 0 | DWord` in the error colour, and «bEnableAv2 = 0» is correctly **unticked**. Before this pass the same file produced `0 -> 0` and no visible discrepancy at all.

Tests cover §6 exactly: intent parsing (int/string/null/absent, unsupported kinds reported, arbitrary names, merge precedence); verification (DWord match/mismatch both directions, wrong kind, intended-write-but-absent, intended-delete-still-present, intended-delete-and-absent); the outcome classifier (0×0 visible, non-zero invisible, not-found); the baseline evaluator (clean / warn / block, `requireCleanBaseline` interaction, null readings); and the marker round trip including a corrupt file treated as "unknown, warn" rather than throwing.

### Anything left unverified or deferred

- **The machine is still contaminated.** `bAcroSuppressUpsell = 1` and `cServices\bToggleAdobeDocumentServices = 1` under «Adobe Acrobat» are live as of this pass. I did not revert them — that is an elevated write on the operator's machine. Use «Politici Adobe → Revocă (cere elevare)»; the bench now verifies the revert landed and only then forgets it. Until that is done, **no conclusion about `AVTaskPaneHostView`, clipping or the document-switch behaviour from the 04.08 runs is worth recording** (plan §8).
- The marker file cannot retroactively know about the 03.08 apply — it did not exist then. The first warning the operator sees will come from `btnMachineState` / the baseline check, not from the marker.
- Write-back verification proves the value is in the registry immediately after writing; it cannot prove Adobe will not overwrite it on exit. That remains the documented community failure mode for `aDefaultRHPViewMode_L`.
- The four re-issued scenarios have **not been re-run** — that is the next step, on a verified-clean machine.
- `machinePolicy.values` is still checkbox-driven (only `userPrefs` became literal); no scenario currently needs a policy value the panel cannot express.

## SLICE-0023 (pass 5) — Adobe harness: the panel stops lying too <a id="slice-0023-pas-5"></a>

Continues slice **0023**. Previous worklogs ([SLICE-0023-adobe-rhp-harness.md](#slice-0023-pas-1), [SLICE-0023-adobe-harness-config-layout.md](#slice-0023-pas-2), [SLICE-0023-adobe-harness-fixes.md](#slice-0023-pas-4)) stand as written.

**No new RHP lever.** Pass 4 made the SCENARIO path honest; this pass makes the MANUAL path honest and widens what "clean baseline" means.

### §0 Why — three findings from the first clean run

The baseline read at 10:04 on 04.08 came back clean (0 policies, 0 Adobe processes) and scenario 02 finally did what it was written to do: `bEnableAv2: 0 -> 1 (DWord) (verificat)` — the first verified registry write in the sequence. Three things came out of that run:

1. **The panel could still not express 1.** The row was a checkbox captioned «bEnableAv2 = 0 (interfața clasică)». Ticked means "write 0". With the machine already on 0 it logged `0 -> 0`: literally correct, useless, and there was no way to ask for 1 from the panel at all. Pass 4 fixed this for values coming from a FILE; the manual path kept the old defect.
2. **`restoreOnClose` silently undid the experiment.** Scenario 02 carried `restoreOnClose: true`, so closing the bench put `bEnableAv2` back to 0. It worked exactly as specified — and it means an experiment that is supposed to hold across sessions cannot.
3. **The clean baseline was not clean.** The same 10:04 reading shows `bRHPSticky = 1 (DWord)` and `aDefaultRHPViewMode_L = Collapsed (String)` under `HKCU\…\Adobe Acrobat\DC\AVGeneral`. No log in this slice writes them — they are left over from something else, plausibly Adobe persisting its own state after the Shift+F4 on 03.08. They are RHP preferences and they were active during every probe, but the baseline check only looked at HKLM, so the machine got a green light while it already remembered wanting the pane collapsed.

### §1 One row per preference, not one checkbox

`PrefRowSelection` / `PrefRowParse` (pure, `Internal/Adobe/`) parse a row into an intent:

| row text | meaning |
|---|---|
| «nu atinge» (default), or blank | no intent at all — the value is left exactly as it is |
| «șterge» | delete the value (`UserPrefAction.Delete`) |
| any integer | `REG_DWORD` with that value — not just 0/1 |
| any other text (string row) | `REG_SZ` written literally |
| anything else on a DWORD row | **rejected**, named, and the apply refuses |

The four checkboxes are gone, replaced by `cboExpandRhp` / `cboRhpSticky` / `cboRhpViewMode` / `cboEnableAv2` in a two-column `tlpPrefRows` (value name | editable combo). The combos are `DropDown`, not `DropDownList`: the listed entries are suggestions, so a scenario asking for a value nobody anticipated shows up as ITSELF instead of being rounded to the nearest switch. `aDefaultRHPViewMode_L` gets free text with «Collapsed»/«Expanded» offered.

«nu atinge» is genuinely distinct from «0» — the same absent-versus-zero distinction the schema has carried since pass 4 now exists in the UI. Loading a scenario writes the file's value into the row VERBATIM (`1` stays `1`, `null` becomes «șterge», `"Docked"` stays `"Docked"`), and a scenario silent about HKCU resets the rows to «nu atinge» rather than inheriting the previous scenario's.

An unparseable row **stops the apply** with a Romanian dialog naming the row: silently skipping it would put panel and registry back into disagreement, which is the defect this whole sequence exists to remove.

### §2 `chkRestoreOnClose` is unticked by default

Default off, and the caption is now explicit («Restaurează valorile HKCU la închiderea bancului»). When it is off and values were applied, the log says so by name:

```
Restaurare la închidere DEZACTIVATĂ — valorile HKCU rămân aplicate: bEnableAv2=1.
Ele vor apărea ca stare de pornire la următoarea rulare.
```

`Config/rhp_02_interfata_moderna.json` flips to `restoreOnClose: false` for the same reason — it is the "stay on the modern UI" experiment, and it was undoing itself every time the bench closed. Its header comment now says how to go back to classic (row → `0` or «șterge» → apply).

### §3 Clean baseline = no HKLM policy **and** no leftover HKCU pane preference

`BaselineOrigin` (`MachinePolicy` / `UserPreference`) on `PolicyReading`, and a new `ReadUserRhpState()` reading `bExpandRHPInViewer`, `bRHPSticky`, `aDefaultRHPViewMode_L` from BOTH `AVGeneral` hives. A neutral machine has all three **absent**. `ReadBaselineState()` = policies + preferences and feeds the pre-scenario check; `ReadPolicyState()` stays HKLM-only because the revert path uses it to verify the elevated import, and an HKCU value must not make that verification fail.

**`bEnableAv2` is deliberately NOT part of the baseline.** It selects the classic/modern viewer, which is the thing under test — counting it as contamination would make every scenario that sets it block itself on the next run.

Warn/block texts are origin-aware: each active value is tagged `[HKLM politică]` / `[HKCU preferință]`, each consequence is stated only when that kind is present, and each cleanup route is named for its own origin — elevated revert for the policy, «șterge» + «Aplică și repornește Adobe» for the preferences. The machine-state summary counts both: `politici HKLM active: 0 · preferințe RHP în HKCU: 2  ⚠ bază de pornire CONTAMINATĂ`.

### Files touched

- `src/KBot.DevHarness/Internal/Adobe/PrefRowSelection.vb` — **new** (row parse + round trip)
- `src/KBot.DevHarness/Internal/Adobe/BaselineEvaluator.vb` — `BaselineOrigin`, `Policies` / `Preferences`, origin-aware describe/warn/block text
- `src/KBot.DevHarness/Internal/AdobeReaderHarnessForm{.vb,.Designer.vb}` — `tlpPrefRows` + four combos + `lblPrefHint` replace the four checkboxes; `PopulatePrefRows`, `ParsePrefRows`, `InvalidPrefRows`, `CollectPanelUserPrefs`, `ApplyUserPrefValuesToRows`, `ReadUserRhpState`, `ReadBaselineState`; `chkRestoreOnClose` unticked
- `src/KBot.DevHarness/Config/rhp_02_interfata_moderna.json` — `restoreOnClose` true → false
- `tests/KBot.DevHarness.Tests/PrefRowSelectionTests.vb` — **new**, 15
- `tests/KBot.DevHarness.Tests/HarnessTruthTests.vb` — +6 (HKCU baseline)
- `tests/KBot.DevHarness.Tests/AdobeHarnessScenarioBindingTests.vb` — rewritten for rows, +4
- `tests/KBot.DevHarness.Tests/AdobeHarnessLayoutTests.vb` — new control names

### Test results

- `dotnet build KBot.sln` — 0 errors, 0 BC warnings.
- `dotnet test KBot.sln` — **552 passed / 0 failed / 0 skipped** (was 527). `KBot.DevHarness.Tests` 91 → 116.
- **Rendered and inspected on screen** with `rhp_02_interfata_moderna.json` loaded: `bExpandRHPInViewer`, `bRHPSticky`, `aDefaultRHPViewMode_L` all on «nu atinge», `bEnableAv2` on `1`, `chkRestoreOnClose` unticked, `gridPrefs` showing `bEnableAv2 | 1 | 0 | DWord` in the error colour.

### Anything left unverified or deferred

- **The probe tree from a run on the modern UI is still missing.** «Nothing works any more» on `bEnableAv2 = 1` is the answer the slice was after, but whether `AVTaskPaneHostView` is absent, renamed, or present-but-no-longer-the-pane decides which implementation `ReaderHostPreview` gets, and only the `── Probă ──` block can say which. Not guessed at here.
- The two leftover HKCU values (`bRHPSticky = 1`, `aDefaultRHPViewMode_L = Collapsed`) are **still set** — this pass makes them visible and gives the bench a way to clear them («șterge» + apply); it does not clear them behind the operator's back.
- `machinePolicy.values` is still checkbox-driven. Only `userPrefs` became literal, in pass 4 for files and here for the panel; no scenario yet needs a policy value the panel cannot express.
- Row parsing is covered by tests; the rows have been rendered but never clicked at a different DPI.

## SLICE-0023 (pass 6) — Adobe harness: placing the hosted window (dx/dy/dw/dh) <a id="slice-0023-pas-6"></a>

Continues slice **0023**. Previous worklogs stand as written.

### §0 A correction: the first attempt drove the wrong window

This pass was first built as **«Mută ferestre copil»** — `SetWindowPos` on individual Adobe child windows (`AVSplitterView`) targeted by window text, with an origin store keyed by text and a timer re-imposing the move because Adobe recomputes its layout and puts its children back.

**That was the wrong mechanism.** The operator's correction: the position belongs on the **main PDF window** — the one the bench hosts — *just like the right and top properties*, not on specific child windows. Reworked accordingly; the child-move engine, the origin store, the reapply timer and the `moveChildren` step are gone, not layered over.

The rework is also markedly smaller, which is the tell that it was the right shape all along: one rectangle, no probing, no timer, no coordinate bookkeeping per child.

### §1 What it is

`dx` / `dy` / `dw` / `dh` on the **hosted** Adobe window, applied inside `HostedBounds()` — the same function clip right/top already go through. The deltas are the general form of the clip:

| existing | equivalent |
|---|---|
| clip right = 200 | `dw = 200` |
| clip top = 60 | `dy = -60`, `dh = 60` |

and they **compose** with it: the clip produces the base rectangle, the deltas shift that.

Negative `dx`/`dy` pull the window left/up so the band at that edge leaves the panel's visible area; `dw`/`dh` grow it back over the opposite edge so no empty strip is left behind. Width and height are floored at 1 — a zero-sized window would vanish with no way back except a relaunch.

**Because everything flows through `HostedBounds()`, a delta is re-applied for free** by `LayoutHostedWindow`, `NudgeRedraw`, the resize/splitter debounce and every relaunch. Nothing has to re-impose it, which is exactly why the child-window version needed a timer and this one does not. The deltas deliberately survive `ClearHostState`: they describe where the *next* hosted window should go, like the clip settings.

### §2 The section

«Poziția ferestrei Adobe», placed directly under «Decupare» because both drive the same window: `numDx` / `numDy` / `numDw` / `numDh` (−2000…2000, default 0), «Readu la zero», and a hint line spelling out that this moves the **Adobe window**, not anything inside it.

The spinners apply **live**, like the clip spinners — no «Aplică» button. Reset is exact by construction: the deltas *are* the whole displacement, so zeroing them restores the untouched geometry and nothing needs recording beforehand.

Every placement logs what actually happened, with the rectangle asked for beside the one the window ended up with:

```
Poziție: cerut -120,-90 1028x736 — MUTAT 0,0 908x646 -> -120,-90 1028x736
```

and, when they differ, `ATENȚIE: fereastra nu a ajuns la dreptunghiul cerut (Adobe a refuzat sau a limitat)`. Adobe can clamp a size, and a request it ignored must not read like a success — the `hideChildren` lesson from pass 4, applied to this lever before the same mistake could be made twice. `ChildRectInParent` converts `GetWindowRect`'s SCREEN coordinates to pnlHost client coordinates once, through `MapWindowPoints(0, parent, rect, 2)`; mixing the two is the one way this produces plausible numbers that mean nothing.

### §3 Schema (still version 1)

```json
"move": { "dx": -120, "dy": -90, "dw": 120, "dh": 90 }
```

New step `applyMove`, sibling of `applyClip`. Absent section stays `Nothing` ("leave the window where the panel puts it") and remains distinguishable from a present section full of zeros, which is a warning. `moveChildren` / `moveOptions` and the `moveChildren` step are **removed** — a file written against them now fails loudly rather than silently doing nothing.

Shipped as `Config/rhp_05_muta_fereastra.json` with `clip.enabled` explicitly **false**: the two compose, so with both active nothing on screen would be attributable to one mechanism.

### Files touched

- `src/KBot.DevHarness/Internal/Adobe/MoveOutcome.vb` — rewritten: `HostedWindowGeometry` (pure arithmetic) + outcome classifier. `MoveChildEntry`, `MoveOptionsConfig`, `MoveAttempt`, `MoveAttemptSummary` and `MoveOriginStore` deleted.
- `src/KBot.DevHarness/Internal/Adobe/HarnessScenario.vb` — `MoveConfig`, `applyMove`
- `src/KBot.DevHarness/Internal/Adobe/HarnessScenarioReader.vb` — all-zero `move` warning
- `src/KBot.DevHarness/Internal/AdobeReaderHarnessForm{.vb,.Designer.vb}` — `grpMove` under `grpClip`, deltas inside `HostedBounds`, `ApplyHostedGeometry`, `ChildRectInParent`, status-block «Poziție». Child-move engine and reapply timer removed.
- `src/KBot.DevHarness/Config/rhp_05_muta_fereastra.json` — **new** (replaces `rhp_05_muta_splitter.json`)
- `tests/KBot.DevHarness.Tests/MoveWindowTests.vb` — **new**, 20 (was `MoveChildrenTests.vb`)
- `tests/KBot.DevHarness.Tests/AdobeHarnessLayoutTests.vb`, `AdobeHarnessScenarioBindingTests.vb`

### Test results

- `dotnet build KBot.sln` — 0 errors, 0 BC warnings.
- `dotnet test KBot.sln` — **579 passed / 0 failed / 0 skipped** (552 before this pass). `KBot.DevHarness.Tests` 116 → 143.
- **Rendered and inspected on screen** with `rhp_05_muta_fereastra.json` loaded: the section sits under «Decupare», shows −120 / −90 / 120 / 90, and the status block reads `Poziție: dx=-120 dy=-90 dw=120 dh=90`.
- **A defect the render caught:** «Readu la zero» stayed disabled after loading a non-zero scenario — `ApplyScenarioToControls` never called `UpdateActionStates`, so a delta arriving from a file did not enable the button a typed one did. Fixed, and both directions are now pinned by tests.

### Anything left unverified or deferred

- **Nothing here has been applied to a running Adobe.** Whether pulling the window left actually takes the gutter out of view, and whether Adobe clamps the size, are open.
- **The zoom limitation is shared with clipping and untested:** the hosted window really is larger than the panel, so a fit-width/fit-page zoom rescales the page to the inflated width. Moving cannot avoid that — the earlier claim that it could was tied to moving a CHILD window, which is not what this does.
- Composing `clip` with `move` is arithmetically covered by a test but has never been looked at on screen; the shipped scenario deliberately turns clipping off.

## Completări din registrul istoric

Registrul 0023 consemna suplimentar versiunea DevHarness **1.0.5.0 → 1.0.6.0** la pasul 6. Verdictul său rămâne «DONE (code) / rulările din 04.08 INVALIDE (bază contaminată)»; nu este certificarea unei stări actuale a mașinii. Tabelul operatorului din pasul 1 rămâne necompletat.
