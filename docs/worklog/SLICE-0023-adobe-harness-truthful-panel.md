# SLICE-0023 (pass 5) — Adobe harness: the panel stops lying too

Continues slice **0023**. Previous worklogs (`SLICE-0023-adobe-rhp-harness.md`,
`SLICE-0023-adobe-harness-config-layout.md`, `SLICE-0023-adobe-harness-fixes.md`) stand as written.

**No new RHP lever.** Pass 4 made the SCENARIO path honest; this pass makes the MANUAL path honest
and widens what "clean baseline" means.

## §0 Why — three findings from the first clean run

The baseline read at 10:04 on 04.08 came back clean (0 policies, 0 Adobe processes) and scenario 02
finally did what it was written to do: `bEnableAv2: 0 -> 1 (DWord) (verificat)` — the first verified
registry write in the sequence. Three things came out of that run:

1. **The panel could still not express 1.** The row was a checkbox captioned
   «bEnableAv2 = 0 (interfața clasică)». Ticked means "write 0". With the machine already on 0 it
   logged `0 -> 0`: literally correct, useless, and there was no way to ask for 1 from the panel at
   all. Pass 4 fixed this for values coming from a FILE; the manual path kept the old defect.
2. **`restoreOnClose` silently undid the experiment.** Scenario 02 carried `restoreOnClose: true`,
   so closing the bench put `bEnableAv2` back to 0. It worked exactly as specified — and it means an
   experiment that is supposed to hold across sessions cannot.
3. **The clean baseline was not clean.** The same 10:04 reading shows
   `bRHPSticky = 1 (DWord)` and `aDefaultRHPViewMode_L = Collapsed (String)` under
   `HKCU\…\Adobe Acrobat\DC\AVGeneral`. No log in this slice writes them — they are left over from
   something else, plausibly Adobe persisting its own state after the Shift+F4 on 03.08. They are
   RHP preferences and they were active during every probe, but the baseline check only looked at
   HKLM, so the machine got a green light while it already remembered wanting the pane collapsed.

## §1 One row per preference, not one checkbox

`PrefRowSelection` / `PrefRowParse` (pure, `Internal/Adobe/`) parse a row into an intent:

| row text | meaning |
|---|---|
| «nu atinge» (default), or blank | no intent at all — the value is left exactly as it is |
| «șterge» | delete the value (`UserPrefAction.Delete`) |
| any integer | `REG_DWORD` with that value — not just 0/1 |
| any other text (string row) | `REG_SZ` written literally |
| anything else on a DWORD row | **rejected**, named, and the apply refuses |

The four checkboxes are gone, replaced by `cboExpandRhp` / `cboRhpSticky` / `cboRhpViewMode` /
`cboEnableAv2` in a two-column `tlpPrefRows` (value name | editable combo). The combos are
`DropDown`, not `DropDownList`: the listed entries are suggestions, so a scenario asking for a value
nobody anticipated shows up as ITSELF instead of being rounded to the nearest switch.
`aDefaultRHPViewMode_L` gets free text with «Collapsed»/«Expanded» offered.

«nu atinge» is genuinely distinct from «0» — the same absent-versus-zero distinction the schema has
carried since pass 4 now exists in the UI. Loading a scenario writes the file's value into the row
VERBATIM (`1` stays `1`, `null` becomes «șterge», `"Docked"` stays `"Docked"`), and a scenario
silent about HKCU resets the rows to «nu atinge» rather than inheriting the previous scenario's.

An unparseable row **stops the apply** with a Romanian dialog naming the row: silently skipping it
would put panel and registry back into disagreement, which is the defect this whole sequence exists
to remove.

## §2 `chkRestoreOnClose` is unticked by default

Default off, and the caption is now explicit («Restaurează valorile HKCU la închiderea bancului»).
When it is off and values were applied, the log says so by name:

```
Restaurare la închidere DEZACTIVATĂ — valorile HKCU rămân aplicate: bEnableAv2=1.
Ele vor apărea ca stare de pornire la următoarea rulare.
```

`Config/rhp_02_interfata_moderna.json` flips to `restoreOnClose: false` for the same reason — it is
the "stay on the modern UI" experiment, and it was undoing itself every time the bench closed. Its
header comment now says how to go back to classic (row → `0` or «șterge» → apply).

## §3 Clean baseline = no HKLM policy **and** no leftover HKCU pane preference

`BaselineOrigin` (`MachinePolicy` / `UserPreference`) on `PolicyReading`, and a new
`ReadUserRhpState()` reading `bExpandRHPInViewer`, `bRHPSticky`, `aDefaultRHPViewMode_L` from BOTH
`AVGeneral` hives. A neutral machine has all three **absent**. `ReadBaselineState()` = policies +
preferences and feeds the pre-scenario check; `ReadPolicyState()` stays HKLM-only because the revert
path uses it to verify the elevated import, and an HKCU value must not make that verification fail.

**`bEnableAv2` is deliberately NOT part of the baseline.** It selects the classic/modern viewer,
which is the thing under test — counting it as contamination would make every scenario that sets it
block itself on the next run.

Warn/block texts are origin-aware: each active value is tagged `[HKLM politică]` / `[HKCU
preferință]`, each consequence is stated only when that kind is present, and each cleanup route is
named for its own origin — elevated revert for the policy, «șterge» + «Aplică și repornește Adobe»
for the preferences. The machine-state summary counts both:
`politici HKLM active: 0 · preferințe RHP în HKCU: 2  ⚠ bază de pornire CONTAMINATĂ`.

## Files touched

- `src/KBot.DevHarness/Internal/Adobe/PrefRowSelection.vb` — **new** (row parse + round trip)
- `src/KBot.DevHarness/Internal/Adobe/BaselineEvaluator.vb` — `BaselineOrigin`, `Policies` /
  `Preferences`, origin-aware describe/warn/block text
- `src/KBot.DevHarness/Internal/AdobeReaderHarnessForm{.vb,.Designer.vb}` — `tlpPrefRows` + four
  combos + `lblPrefHint` replace the four checkboxes; `PopulatePrefRows`, `ParsePrefRows`,
  `InvalidPrefRows`, `CollectPanelUserPrefs`, `ApplyUserPrefValuesToRows`, `ReadUserRhpState`,
  `ReadBaselineState`; `chkRestoreOnClose` unticked
- `src/KBot.DevHarness/Config/rhp_02_interfata_moderna.json` — `restoreOnClose` true → false
- `tests/KBot.DevHarness.Tests/PrefRowSelectionTests.vb` — **new**, 15
- `tests/KBot.DevHarness.Tests/HarnessTruthTests.vb` — +6 (HKCU baseline)
- `tests/KBot.DevHarness.Tests/AdobeHarnessScenarioBindingTests.vb` — rewritten for rows, +4
- `tests/KBot.DevHarness.Tests/AdobeHarnessLayoutTests.vb` — new control names

## Test results

- `dotnet build KBot.sln` — 0 errors, 0 BC warnings.
- `dotnet test KBot.sln` — **552 passed / 0 failed / 0 skipped** (was 527).
  `KBot.DevHarness.Tests` 91 → 116.
- **Rendered and inspected on screen** with `rhp_02_interfata_moderna.json` loaded:
  `bExpandRHPInViewer`, `bRHPSticky`, `aDefaultRHPViewMode_L` all on «nu atinge»,
  `bEnableAv2` on `1`, `chkRestoreOnClose` unticked, `gridPrefs` showing
  `bEnableAv2 | 1 | 0 | DWord` in the error colour.

## Anything left unverified or deferred

- **The probe tree from a run on the modern UI is still missing.** «Nothing works any more» on
  `bEnableAv2 = 1` is the answer the slice was after, but whether `AVTaskPaneHostView` is absent,
  renamed, or present-but-no-longer-the-pane decides which implementation `ReaderHostPreview` gets,
  and only the `── Probă ──` block can say which. Not guessed at here.
- The two leftover HKCU values (`bRHPSticky = 1`, `aDefaultRHPViewMode_L = Collapsed`) are **still
  set** — this pass makes them visible and gives the bench a way to clear them («șterge» + apply);
  it does not clear them behind the operator's back.
- `machinePolicy.values` is still checkbox-driven. Only `userPrefs` became literal, in pass 4 for
  files and here for the panel; no scenario yet needs a policy value the panel cannot express.
- Row parsing is covered by tests; the rows have been rendered but never clicked at a different DPI.
