# SLICE-0023 (pass 4) — Adobe harness: make the bench tell the truth

Continues slice **0023**. Previous worklogs (`SLICE-0023-adobe-rhp-harness.md`,
`SLICE-0023-adobe-harness-config-layout.md`) stay as they are.

**No new RHP lever.** This pass is about the bench being trustworthy.

## §0 Why — four runs that reported success and tested nothing

Four scenarios were run on 04.08. All four reported success. Confirmed from
`Logs\test_adobe_rhp.log`, present in **every** probe of all four runs:

```
d=3 hwnd=0x120C96 cls=AVL_AVView text="AVTaskPaneHostView" x=0 y=1 0x0 vis=0
14 ferestre copil; niciun candidat RHP după euristic.
Ascuns copil: AVTaskPaneHostView — AVL_AVView (0x0) (hwnd=0x120C96)
hideChildren: încercarea 1/20 — 1 din 1 texte găsite.
```

The task pane host was **0×0 and already invisible before the scenario started**. `hideChildren`
hid a window that was already hidden and called it a clean success. On 03.08 the same window was
346×1440 and visible, so this was a change in machine state, not in Adobe.

Three defects, all now closed:

1. **`hideChildren` could not distinguish a real hide from a no-op** — "1 din 1 texte găsite" is
   true and useless.
2. **`userPrefs` values were clamped by the checkboxes.** Scenario 02 asked for `"bEnableAv2": 1`;
   the confirmation dialog offered `0 -> 0` and the log recorded `0 (DWord) -> 0 (DWord)`
   (`test_adobe_rhp.log`, 09:02:33). Half the schema was decorative.
3. **The machine was contaminated and nothing said so.** Traced in the run logs:
   `test_20260803_120648_861.log` applied the «Acrobat Reader» policy **and reverted it**;
   `test_20260803_120952_713.log` applied the «Adobe Acrobat» policy with **no revert line
   anywhere after it**. Read live during this pass:

   ```
   HKLM\SOFTWARE\Policies\Adobe\Adobe Acrobat\DC\FeatureLockDown\bAcroSuppressUpsell = 1
   HKLM\...\FeatureLockDown\cServices\bToggleAdobeDocumentServices = 1
   ```

   Still active — almost certainly why the pane is now zero-sized (services suppressed, nothing
   for the pane to host). **The harness did not revert it and this pass does not revert it either**
   — it is an elevated write on the operator's machine; the bench now offers one-click revert and
   refuses to pretend the state is clean.

## §1 `userPrefs` values are applied literally

- New pure `UserPrefIntent` / `UserPrefIntentFactory`: integer → `REG_DWORD`, string → `REG_SZ`,
  **`null` → delete the value**. Absent key → no intent at all, so "leave alone", "set to 0" and
  "remove it" are three distinguishable states. Value **names are open** — any value under the
  resolved `AVGeneral` hive can be driven from a file with no code change.
- `applyUserPrefs` reads from the **scenario**; the checkboxes are merged in only for names the
  file does not mention (or for all of them when no scenario is loaded) — `UserPrefIntentFactory.Merge`.
- **Write-back verification** (`RegistryWriteVerifier`): after every write the value is read back
  and compared. Match → `… (verificat)`. Mismatch → `EȘEC: <path>\<name> — cerut X, citit Y`, the
  run **stops**, and a Romanian message box names the value. Deletions verified as absence.
- Confirmation dialog shows the **true** intended value per row —
  `hive · nume · valoare curentă · valoare nouă · tip`, deletions as `(șters)`, absent current
  values as `(absent)`, never as `0`.
- New **`gridPrefs`** (read-only `DataGridView`, columns **Valoare · Cerut · Curent · Tip**) under
  the checkboxes, refreshed on load and after every write, mismatching rows in the palette's error
  colour.
- The checkbox shortcuts no longer lie: a shortcut is ticked **only when the scenario asks for
  exactly what it means**, so `"bEnableAv2": 1` leaves «bEnableAv2 = 0» unticked.

## §2 `hideChildren` reports what it actually did

`HideOutcomeClassifier` classifies each match **before** touching it (afterwards everything looks
hidden — which is how the no-op read as success): `ASCUNS` / `DEJA ASCUNS` / `DIMENSIUNE ZERO` /
`NEGĂSIT`, one log line each. Summary via `HideAttemptSummary`:

```
hideChildren: încercarea 1/10 — 1 găsit(e): 0 ascunse, 1 deja ascuns/zero. ATENȚIE: nicio schimbare reală.
```

Retries stop early **only on a real hide**; a run that only ever sees `DEJA ASCUNS`/`DIMENSIUNE
ZERO` burns through every attempt and ends with a Romanian warning in the log and `lblStatus` that
the step changed nothing and therefore proves nothing.

## §3 Machine-state diagnostic

- **`btnMachineState`** — «Starea mașinii (Adobe + registry)»: Adobe path/version/product, both
  `AVGeneral` hives with `bEnableAv2` / `bExpandRHPInViewer` / `bRHPSticky` /
  `aDefaultRHPViewMode_L` (value or `(absent)`), both HKLM policy products, and the count of
  running Adobe processes. Read-only, no elevation.
- **Automatic baseline check** before every scenario, logged under `── Bază de pornire ──`.
  `BaselineEvaluator` → Clean / Warn / Block. Warn shows the Romanian dialog naming every active
  value (Continue / Cancel); Block refuses outright when the scenario sets `requireCleanBaseline`.
- **Revert tracking that survives a restart** — `Config\harness_machine_state.json`
  (`MachineStateMarker`): written on apply with the pre-apply snapshot and the revert `.reg` path,
  cleared only after a revert that is **verified** to have left no policy value behind. Warned
  about on open. A **corrupt** marker is "unknown, warn" — never "nothing outstanding".
- **`chkRevertPolicyOnClose`** (default checked): on close, an outstanding policy is reverted
  through the elevated import. UAC cancelled / import failed / values still present → marker
  **kept** and the operator told, naming the exact keys.

## §4 Probe output

- A dedicated `PANOU:` line per `hideChildren.byText` target, matched or not:
  `PANOU: AVTaskPaneHostView — 346x1440, vis=1` / `— 0x0 (posibil învechit), vis=0` / `— NEGĂSIT`.
- Rectangles read from a window with `vis=0` are marked **`(posibil învechit)`** — the log showed
  `AVExpandCollapseButtonView … 27x913` inside a 587-high host, i.e. stale geometry from an earlier
  layout.
- When the heuristic finds no candidate **and** a requested target exists with zero size, the
  summary says so explicitly instead of just «niciun candidat RHP».

## §5 Schema (still version 1, backwards compatible)

- `userPrefs.values` accepts integers, strings and `null`.
- `requireCleanBaseline` (root, default false).
- `machinePolicy.revertOnClose` (default true), applied to `chkRevertPolicyOnClose` on load.

## Files touched

- `src/KBot.DevHarness/Internal/Adobe/UserPrefIntent.vb` — **new** (intents + merge)
- `src/KBot.DevHarness/Internal/Adobe/RegistryWriteVerifier.vb` — **new**
- `src/KBot.DevHarness/Internal/Adobe/HideOutcome.vb` — **new** (classifier + summary)
- `src/KBot.DevHarness/Internal/Adobe/BaselineEvaluator.vb` — **new**
- `src/KBot.DevHarness/Internal/Adobe/MachineStateMarker.vb` — **new**
- `src/KBot.DevHarness/Internal/Adobe/HarnessScenario.vb` — `requireCleanBaseline`, `revertOnClose`
- `src/KBot.DevHarness/Internal/AdobeReaderHarnessForm{.vb,.Designer.vb}` — all of the above wired;
  new `gridPrefs`, `btnMachineState`, `chkRevertPolicyOnClose`
- `src/KBot.DevHarness/Config/rhp_0{1..4}_*.json` — the operator's four scenarios, now **versioned
  in the repo** (they only existed in `bin\`) and re-issued with `requireCleanBaseline: true`
- `src/KBot.DevHarness/Config/rhp_collapse_then_hide.json` — same flag
- `tests/KBot.DevHarness.Tests/HarnessTruthTests.vb` — **new**, 31
- `tests/KBot.DevHarness.Tests/ShippedScenarioFilesTests.vb` — **new**, 3
- `tests/KBot.DevHarness.Tests/AdobeHarnessScenarioBindingTests.vb` — +2

## Test results

- `dotnet build KBot.sln` — 0 errors, 0 BC warnings.
- `dotnet test KBot.sln` — **527 passed / 0 failed / 0 skipped** (was 491).
  `KBot.DevHarness.Tests` 55 → 91.
- **Rendered and inspected on screen** with `rhp_02_interfata_moderna.json` loaded — the scenario
  that was silently clamped. `gridPrefs` shows `bEnableAv2 | cerut 1 | curent 0 | DWord` in the
  error colour, and «bEnableAv2 = 0» is correctly **unticked**. Before this pass the same file
  produced `0 -> 0` and no visible discrepancy at all.

Tests cover §6 exactly: intent parsing (int/string/null/absent, unsupported kinds reported,
arbitrary names, merge precedence); verification (DWord match/mismatch both directions, wrong kind,
intended-write-but-absent, intended-delete-still-present, intended-delete-and-absent); the outcome
classifier (0×0 visible, non-zero invisible, not-found); the baseline evaluator (clean / warn /
block, `requireCleanBaseline` interaction, null readings); and the marker round trip including a
corrupt file treated as "unknown, warn" rather than throwing.

## Anything left unverified or deferred

- **The machine is still contaminated.** `bAcroSuppressUpsell = 1` and
  `cServices\bToggleAdobeDocumentServices = 1` under «Adobe Acrobat» are live as of this pass. I
  did not revert them — that is an elevated write on the operator's machine. Use «Politici Adobe →
  Revocă (cere elevare)»; the bench now verifies the revert landed and only then forgets it.
  Until that is done, **no conclusion about `AVTaskPaneHostView`, clipping or the document-switch
  behaviour from the 04.08 runs is worth recording** (plan §8).
- The marker file cannot retroactively know about the 03.08 apply — it did not exist then. The
  first warning the operator sees will come from `btnMachineState` / the baseline check, not from
  the marker.
- Write-back verification proves the value is in the registry immediately after writing; it cannot
  prove Adobe will not overwrite it on exit. That remains the documented community failure mode for
  `aDefaultRHPViewMode_L`.
- The four re-issued scenarios have **not been re-run** — that is the next step, on a
  verified-clean machine.
- `machinePolicy.values` is still checkbox-driven (only `userPrefs` became literal); no scenario
  currently needs a policy value the panel cannot express.
