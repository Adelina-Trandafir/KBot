# SLICE-0023 (pass 2) — Adobe harness: scenario files + resizable docked layout

Continues slice **0023**; the number was NOT changed and the previous worklog
(`SLICE-0023-adobe-rhp-harness.md`) stays as it is. Plan: pasted in-session
("PLAN — Adobe harness: scenario files + resizable docked layout").

**No new RHP lever was added in this pass.** Everything that existed keeps working exactly as it
did; it became scriptable and resizable.

## What changed and why

### 1. Scenario files (§3)

Every test used to be a manual click sequence: the combination that worked (Shift+F4 to collapse,
then hide `AVTaskPaneHostView`) existed only as a log trace plus a memory of which buttons were
pressed in which order. It is now a file that can be repeated, sent to someone else, or tried on
another machine.

- **JSON via `System.Text.Json`** (already in .NET 8, no package reference), read with
  `ReadCommentHandling = Skip` and `AllowTrailingCommas = True` because these files are written by
  hand with comments; written with `WriteIndented = True` so saves are readable and diffable.
- **Children are addressed by window TEXT, never by HWND.** The probe log proves handles change on
  every launch (`0x5083E` → `0x20B66`) while the text `AVTaskPaneHostView` stays identical; class
  names are useless here too — nearly everything is `AVL_AVView`.
- **Absent ≠ disabled.** Every section is a reference type (Nothing when absent) and every scalar
  inside it is `Nullable`, so "no `clip` object" (leave alone) stays distinguishable from
  `"clip": { "enabled": false }` (turn off). This is the property §5 pins with tests.
- **Ten steps**, each mapping to the code path already behind the corresponding button — no step
  contains new logic. To make that literally true, three existing paths were *extracted*, not
  re-implemented: `RelaunchAsync` now = `StartFreshLaunch()` + `CompleteEmbedAsync()` (which the
  `launch` / `waitForEmbed` steps use separately), `btnApplyUser` = `ApplyUserPrefsCore()`,
  `btnApplyMachine` = `ApplyMachinePolicyCoreAsync()`. The generation counter, `NudgeRedraw` and
  the force-kill-by-PID path are untouched.
- **Unknown step name aborts the run** with a Romanian message naming the bad step *and* the valid
  list — a typo in a file sent from outside is loud, never silently skipped. Unknown *properties*
  are the opposite: captured via `JsonExtensionData` and reported as warnings, so a newer file
  still runs on an older bench.
- **Failure stops the run where it happened**, logged, with no automatic rollback — half-rolled-back
  state is harder to read than the failure.
- **Safety (§3.3):** `machinePolicy.apply` defaults to false and the `applyMachinePolicy` step is
  skipped with a log line when it is false, even if the step appears in `scenario`. Any run whose
  steps include `applyUserPrefs` or `applyMachinePolicy` shows one up-front Romanian confirmation
  listing **every path and value, current value beside the new one**; Cancel abandons the whole
  run. Loading a file never writes anything — only Run does. The HKCU snapshot rules from pass 1
  are unchanged (absent restores to deletion, snapshot once per session).
- **Re-apply after relaunch (§3.4):** because HWNDs change, every embed re-runs the probe,
  re-resolves `byText` and hides what matches, retrying `reapplyAttempts` × `reapplyIntervalMs`
  (Adobe creates the task pane host *after* the main view). Each attempt logs `n of m found` —
  `0 of 1` repeated ten times is exactly the diagnostic needed when a scenario stops working on a
  different Adobe build.
- **New "Scenariu" section (§3.5)** directly under "Diagnostic": file name label, Încarcă / Rulează
  (disabled until loaded) / Salvează, and `chkApplyOnLoad` (default checked). Save writes the
  current state of every control back out in the same schema **including the texts of the child
  windows hidden right now** — the round trip that matters. Saved files always carry
  `machinePolicy.apply = false`, so a file can never write machine-wide policy just by being run
  somewhere else.

### 2. Layout rework (§2)

The fixed-width `FlowLayoutPanel` strip is gone. Now: `splitMain` (`SplitContainer`, vertical,
`SplitterDistance` 320, `Panel1MinSize` 260, `FixedPanel = None`, draggable at runtime) → Panel1
holds `tlpOptions` (`AutoScroll`, one AutoSize row per section plus a 100% filler so sections stay
top-aligned); Panel2 holds `tlpRight` (row 0 = 100% `pnlHost`, row 1 = AutoSize `lblStatus`). Each
of the eleven sections is a `GroupBox` + inner `TableLayoutPanel`, both `Dock = Fill` +
`AutoSize`/`GrowAndShrink`; the clip section uses two columns (AutoSize label + 100% input).
`lblCmd` became **`txtCmd`** (multiline, vertical scrollbar, `MinimumSize` height 72) and
`lstChildren` got `MinimumSize` height 120, so neither collapses when the splitter goes narrow.
Pass/Fail stayed where the harness framework puts them.

The three consequences the plan said to handle rather than discover:

- Clip geometry is computed from `pnlHost.ClientSize`, which now changes with the splitter — it is
  re-applied on `pnlHost.Resize` **and** `splitMain.SplitterMoved`, not only on spinner changes.
- Both are **debounced** through a 150 ms `tmrLayout` restarted on every event; geometry and
  `NudgeRedraw` run once when it fires, never per resize tick.
- The debounce and the tick both guard on `_hostedWindow <> IntPtr.Zero`.

Hidden children survive a resize but not a relaunch — handled by §3.4 above; `KillTracked` now
clears the hidden *handles* and the probe list but **keeps the hidden texts**, which are the
durable identity a scenario re-applies after the next embed.

### 3. Theming (§6) — checked, no fix needed

`ThemeManager` already handles every new container type: `SplitContainer` (its own `BackColor`
plus `Panel1`/`Panel2`, and `Traverse` recurses into both panels explicitly),
`GroupBox` (back + fore), `TableLayoutPanel` and `TextBox`
(`ThemeManager.vb:172-194`, `:218-242`, `:234`, `:321`). Verified by reading the theme engine, not
by rendering — see "unverified" below.

## Files touched

- `src/KBot.DevHarness/Internal/AdobeReaderHarnessForm.Designer.vb` — full layout rework; ~40
  controls re-declared (SplitContainer, 2 outer + 11 inner TableLayoutPanels, 11 GroupBoxes, the
  five Scenariu controls, `txtCmd`, `tmrLayout`); `lblSec*` labels and `flowLeft` removed (GroupBox
  captions replace them); `components` + `Dispose` added for the timer
- `src/KBot.DevHarness/Internal/AdobeReaderHarnessForm.vb` — scenario load/run/save + the runner,
  the `RelaunchAsync` split, debounced layout refresh, hide-by-text with retries, the up-front
  registry confirmation, `SetControlsEnabled` now toggles `tlpOptions`
- `src/KBot.DevHarness/Internal/Adobe/HarnessScenario.vb` — **new**: the config types +
  `HarnessScenarioSteps`
- `src/KBot.DevHarness/Internal/Adobe/HarnessScenarioReader.vb` — **new**: reader/writer pair +
  `HarnessScenarioReadResult` (`IsValid` / `Errors` / `Warnings`, no message boxes inside)
- `src/KBot.DevHarness/Config/rhp_collapse_then_hide.json` — **new** sample scenario (the §0
  combination), copied to `<AppDir>\Config\` with `PreserveNewest`
- `src/KBot.DevHarness/KBot.DevHarness.vbproj` — Config copy rule; FileVersion 1.0.3.0 → 1.0.4.0
- `tests/KBot.DevHarness.Tests/HarnessScenarioTests.vb` — **new**, 17 tests

Out of scope and untouched, as in pass 1: `ReaderHostPreview`, `IDdfPreview`,
`DdfPreviewFactory` (mode constant stays `XfaXml`), installer, publish, startup paths.

## Test results

- `dotnet build KBot.sln` — 0 errors, 0 BC warnings (16 pre-existing NU1701 from
  iTextSharp/BouncyCastle).
- `dotnet test KBot.sln` — **475 passed / 0 failed / 0 skipped** (was 458; +17 scenario tests).
  `KBot.DevHarness.Tests` is now 39.
- Sample scenario confirmed landing in `bin\Debug\net8.0-windows\Config\`.

Scenario tests cover exactly what §5 asked: full-field round trip
(serialize → deserialize → serialize unchanged), absent-section ≠ disabled-section (both
directions), unknown property → warning not exception, unknown step → `IsValid = False` naming the
offender, `machinePolicy.apply` absent → false, malformed JSON → Romanian error with no exception
escaping. Plus comments/trailing commas accepted, schema-version handling, and the writer omitting
absent sections so a saved file keeps its meaning.

## Anything left unverified or deferred

- **No visual verdict — nothing here has been on screen.** The layout is the most exposed part:
  AutoSize `GroupBox` + `TableLayoutPanel` nesting inside an `AutoScroll` table is exactly the
  combination that looks right in the designer and can still misbehave at a narrow splitter
  position. Wrapping of long checkbox captions, the `txtCmd`/`lstChildren` minimum sizes and the
  AutoSize row behaviour under scroll are all unconfirmed.
- **The scenario runner has never been run against a real Adobe.** The step→handler mapping is
  green offline and the model is well covered, but `launch`/`waitForEmbed`/`hideChildren` have not
  executed once against a live window.
- **Theme correctness for the new containers was verified by reading `ThemeManager`, not by
  rendering.** No theme change was needed; if a `GroupBox` border reads wrong on Dark in practice,
  the fix belongs in `KBot.Theming`, not here.
- **Registry key names are still the pass-1 ones and still unverified on a real machine** (plan
  §2 of the previous pass). Nothing in this pass changes that; the empty operator table stays in
  `SLICE-0023-adobe-rhp-harness.md`.
- **The `keys` step reuses the experimental cross-process `SendKeys` path**, documented since pass 1
  as not behaving like a native child. `ToSendKeysSyntax` translates `"Shift+F4"`/`"Ctrl+..."`
  specs; unrecognised modifiers fall through as literal text rather than being guessed at.
- **`machinePolicy.values` accepts only numeric (REG_DWORD) values**; anything else is logged and
  skipped rather than guessed. Same for `userPrefs.values` outside number/string.
- **Addition beyond the plan's letter:** the sample scenario file
  (`Config/rhp_collapse_then_hide.json`) and its copy-to-output rule. The plan specified where
  scenario files live but shipped none; this makes the §0 combination a real artifact instead of a
  memory. Its `document.path` is a placeholder that must be pointed at a real PDF.
