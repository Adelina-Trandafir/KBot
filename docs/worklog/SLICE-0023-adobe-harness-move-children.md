# SLICE-0023 (pass 6) — Adobe harness: moving child windows

Continues slice **0023**. Previous worklogs stand as written.

**A new lever**, the first since the original four: the bench can now MOVE and RESIZE a child
window, not only hide one or clip the whole host.

## §1 What `normal.xml` establishes

```
class    AVL_AVView
text     AVSplitationPageView
handle   0x001C0EFC
parent   0x000E0FC6
style    0x56000000 = WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS | WS_CLIPCHILDREN
process  Acrobat.exe  PID 29136
```

`WS_CHILD`, same process, already enumerated by the probe. The black left column is an ordinary
child-window region — reachable, and therefore **movable**, not only hideable. This is the opposite
of the `AVL_AVPopup` case and needs the opposite mechanism.

## §2 Why moving, when clipping already exists

Clipping enlarges the HOSTED window so the unwanted band falls off the panel edge. A fit-width or
fit-page zoom then rescales the page to the inflated width, and the clip starts eating document
content. Moving a child changes nothing about the host size, so the page keeps its scale.

**Where moving works it is strictly better than clipping.** Whether it works is what this pass
exists to find out; the bench does not assume it.

## §3 Which window — the trap

From the 14:09 probe, in host client coordinates:

```
AV2DocumentTabView      x=11   y=63    1556x1278
  AVSplitationPageView  x=11   y=153   1466x1188
    AVSplitterView      x=131  y=153   1346x1188
```

The ~120px black gutter is **inside** `AVSplitationPageView`, to the left of `AVSplitterView`.
Moving `AVSplitationPageView` would take the gutter along with it and remove nothing. The window
that must move is **`AVSplitterView`**. Same reasoning vertically: the toolbar band between y=63 and
y=153 sits inside `AV2DocumentTabView`, above `AVSplitterView`.

**Hypothesis to be confirmed VISUALLY — not a fact, and not tested here:**

```
AVSplitterView   dx = -120   dy = -90   dw = +120   dh = +90
```

If it holds, one move replaces both the left and the top clip. Shipped as
`Config/rhp_05_muta_splitter.json` with `clip.enabled` explicitly **false**, so that whatever
appears on screen is attributable to one mechanism.

## §4 «Mută ferestre copil», under «Ferestre copil»

`txtMoveTarget` (filled by selecting an entry in `lstChildren`, still editable), `numDx` / `numDy` /
`numDw` / `numDh` (−2000…2000, default 0), `btnApplyMove`, `btnResetMoves`, `chkReapplyMoves` +
`numReapplyMs` (default 500 ms).

`SetWindowPos` with `SWP_NOZORDER | SWP_NOACTIVATE`, then the existing `NudgeRedraw`.

**Coordinates** — the one thing that would silently go wrong: `GetWindowRect` returns SCREEN
coordinates, `SetWindowPos` on a child expects the PARENT'S CLIENT coordinates. `ChildRectInParent`
converts once through `MapWindowPoints(IntPtr.Zero, parent, rect, 2)` (cPoints = 2 because a RECT is
two POINTs), and **both** rectangles are logged so a mix-up is visible rather than mysterious:

```
  MUTAT: «AVSplitterView» 131,153 1346x1188 -> 11,63 1466x1278
```

The pre-move rectangle is recorded the first time a given window TEXT is moved (`MoveOriginStore`) —
keyed by text because HWNDs change every launch, and **idempotent**, the same rule as the registry
snapshot: a second move must not overwrite the true original, or «Readu la poziția inițială» would
restore a state the operator produced rather than Adobe's own layout.

Outcome per entry, classified from the rectangles actually read before and after:
`MUTAT` / `NEGĂSIT` / `NESCHIMBAT` (zero deltas, or the window was already there) / `EȘUAT`
(`SetWindowPos` returned False). A summary that moved nothing carries
`ATENȚIE: nicio schimbare reală` — the `hideChildren` lesson from pass 4, applied before the same
mistake could be made twice.

## §5 Adobe fights back

Adobe recomputes its layout on resize, zoom change, document change and assorted repaints.
`chkReapplyMoves` re-imposes the moves on a timer. Ticks that found everything already in place log
**nothing** — at two ticks a second the log would otherwise be unreadable — and the status bar keeps
the ratio that matters:

```
Reaplicare mutări: 3 din 40 tick-uri au avut de corectat ceva.
```

**That ratio is the finding.** Occasional corrections (resize / zoom / document change) mean moving
is viable. Corrections on nearly every tick mean the approach flickers in production and clipping is
the more honest route. The bench measures it instead of assuming either way.

The timer stops on relaunch, on form close, and whenever nothing is hosted. A relaunch does not
silently resurrect a move the operator turned off — `SyncMoveReapplyTimer` only restarts it when the
checkbox is still ticked and there is something to re-impose.

## §6 Schema (still version 1)

```json
"moveChildren": [
  { "byText": "AVSplitterView", "dx": -120, "dy": -90, "dw": 120, "dh": 90 }
],
"moveOptions": { "reapply": true, "reapplyIntervalMs": 500 }
```

New step name `moveChildren`. Absent section = do nothing (and `MoveChildren` stays `Nothing`, never
an empty list — the absent/empty distinction the whole schema keeps). Entries are applied in ARRAY
ORDER, and when a text matches several windows all of them are moved and each is logged — the
duplicate-`AVUITopRightCommandCluster` case the probe already exposed for hiding. An entry with no
`byText` is a **read error**; an all-zero entry is a warning. `reapply` defaults to **false**: a
timer that writes into another process never starts because a section merely exists.

Saving the panel state writes the move only when it names a window and would do something.

## Files touched

- `src/KBot.DevHarness/Internal/Adobe/MoveOutcome.vb` — **new** (classifier, attempt, summary,
  `MoveOriginStore`)
- `src/KBot.DevHarness/Internal/Adobe/HarnessScenario.vb` — `MoveChildEntry`, `MoveOptionsConfig`,
  `moveChildren` step
- `src/KBot.DevHarness/Internal/Adobe/HarnessScenarioReader.vb` — move validation + unknown-property
  reporting for the new sections
- `src/KBot.DevHarness/Internal/AdobeReaderHarnessForm{.vb,.Designer.vb}` — `grpMove` section,
  `ChildRectInParent` / `MoveOneChild` / `ApplyMoveEntry` / `ApplyMoves`, reapply timer,
  `MapWindowPoints` + `GetParent` P/Invokes, status-block «Mutări», scenario step and binding
- `src/KBot.DevHarness/Config/rhp_05_muta_splitter.json` — **new** (the §3 hypothesis)
- `tests/KBot.DevHarness.Tests/MoveChildrenTests.vb` — **new**, 25
- `tests/KBot.DevHarness.Tests/AdobeHarnessLayoutTests.vb` — +3, twelve sections not eleven
- `tests/KBot.DevHarness.Tests/AdobeHarnessScenarioBindingTests.vb` — +2

## Test results

- `dotnet build KBot.sln` — 0 errors, 0 BC warnings.
- `dotnet test KBot.sln` — **584 passed / 0 failed / 0 skipped** (was 552).
  `KBot.DevHarness.Tests` 116 → 148.
- **Rendered and inspected on screen** with `rhp_05_muta_splitter.json` loaded: the section shows
  `AVSplitterView`, dx −120, dy −90, dw 120, dh 90, reapply ticked at 500 ms, «Decupare activă»
  unticked; «Aplică mutarea» / «Readu la poziția inițială» correctly disabled with nothing hosted.

## Anything left unverified or deferred

- **The §3 hypothesis has not been tested.** Nothing here has moved a real Adobe window. Whether
  `AVSplitterView` accepts the move, whether the gutter disappears, and whether the document pulls up
  over the toolbar band are all open — a `NESCHIMBAT` line would mean Adobe refused it or put it back
  instantly, and both are results worth recording.
- **The reapply ratio is unmeasured**, and it is the number that decides whether this approach can
  leave the bench at all (§5).
- The move is applied to the windows found by the LAST probe; the reapply tick re-probes only when
  none of the recorded handles is alive. If Adobe destroys and recreates the splitter under an
  unchanged text mid-session, one tick may act on a dead handle before the next probe catches up.
- Interaction with clipping is untested by construction — the shipped scenario turns clipping off on
  purpose so a single mechanism is attributable. Whether the two compose is a later question.
