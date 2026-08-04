# SLICE-0023 (pass 6) — Adobe harness: placing the hosted window (dx/dy/dw/dh)

Continues slice **0023**. Previous worklogs stand as written.

## §0 A correction: the first attempt drove the wrong window

This pass was first built as **«Mută ferestre copil»** — `SetWindowPos` on individual Adobe child
windows (`AVSplitterView`) targeted by window text, with an origin store keyed by text and a timer
re-imposing the move because Adobe recomputes its layout and puts its children back.

**That was the wrong mechanism.** The operator's correction: the position belongs on the **main PDF
window** — the one the bench hosts — *just like the right and top properties*, not on specific
child windows. Reworked accordingly; the child-move engine, the origin store, the reapply timer and
the `moveChildren` step are gone, not layered over.

The rework is also markedly smaller, which is the tell that it was the right shape all along:
one rectangle, no probing, no timer, no coordinate bookkeeping per child.

## §1 What it is

`dx` / `dy` / `dw` / `dh` on the **hosted** Adobe window, applied inside `HostedBounds()` — the same
function clip right/top already go through. The deltas are the general form of the clip:

| existing | equivalent |
|---|---|
| clip right = 200 | `dw = 200` |
| clip top = 60 | `dy = -60`, `dh = 60` |

and they **compose** with it: the clip produces the base rectangle, the deltas shift that.

Negative `dx`/`dy` pull the window left/up so the band at that edge leaves the panel's visible area;
`dw`/`dh` grow it back over the opposite edge so no empty strip is left behind. Width and height are
floored at 1 — a zero-sized window would vanish with no way back except a relaunch.

**Because everything flows through `HostedBounds()`, a delta is re-applied for free** by
`LayoutHostedWindow`, `NudgeRedraw`, the resize/splitter debounce and every relaunch. Nothing has to
re-impose it, which is exactly why the child-window version needed a timer and this one does not.
The deltas deliberately survive `ClearHostState`: they describe where the *next* hosted window
should go, like the clip settings.

## §2 The section

«Poziția ferestrei Adobe», placed directly under «Decupare» because both drive the same window:
`numDx` / `numDy` / `numDw` / `numDh` (−2000…2000, default 0), «Readu la zero», and a hint line
spelling out that this moves the **Adobe window**, not anything inside it.

The spinners apply **live**, like the clip spinners — no «Aplică» button. Reset is exact by
construction: the deltas *are* the whole displacement, so zeroing them restores the untouched
geometry and nothing needs recording beforehand.

Every placement logs what actually happened, with the rectangle asked for beside the one the window
ended up with:

```
Poziție: cerut -120,-90 1028x736 — MUTAT 0,0 908x646 -> -120,-90 1028x736
```

and, when they differ, `ATENȚIE: fereastra nu a ajuns la dreptunghiul cerut (Adobe a refuzat sau a
limitat)`. Adobe can clamp a size, and a request it ignored must not read like a success — the
`hideChildren` lesson from pass 4, applied to this lever before the same mistake could be made
twice. `ChildRectInParent` converts `GetWindowRect`'s SCREEN coordinates to pnlHost client
coordinates once, through `MapWindowPoints(0, parent, rect, 2)`; mixing the two is the one way this
produces plausible numbers that mean nothing.

## §3 Schema (still version 1)

```json
"move": { "dx": -120, "dy": -90, "dw": 120, "dh": 90 }
```

New step `applyMove`, sibling of `applyClip`. Absent section stays `Nothing` ("leave the window
where the panel puts it") and remains distinguishable from a present section full of zeros, which is
a warning. `moveChildren` / `moveOptions` and the `moveChildren` step are **removed** — a file
written against them now fails loudly rather than silently doing nothing.

Shipped as `Config/rhp_05_muta_fereastra.json` with `clip.enabled` explicitly **false**: the two
compose, so with both active nothing on screen would be attributable to one mechanism.

## Files touched

- `src/KBot.DevHarness/Internal/Adobe/MoveOutcome.vb` — rewritten: `HostedWindowGeometry` (pure
  arithmetic) + outcome classifier. `MoveChildEntry`, `MoveOptionsConfig`, `MoveAttempt`,
  `MoveAttemptSummary` and `MoveOriginStore` deleted.
- `src/KBot.DevHarness/Internal/Adobe/HarnessScenario.vb` — `MoveConfig`, `applyMove`
- `src/KBot.DevHarness/Internal/Adobe/HarnessScenarioReader.vb` — all-zero `move` warning
- `src/KBot.DevHarness/Internal/AdobeReaderHarnessForm{.vb,.Designer.vb}` — `grpMove` under
  `grpClip`, deltas inside `HostedBounds`, `ApplyHostedGeometry`, `ChildRectInParent`, status-block
  «Poziție». Child-move engine and reapply timer removed.
- `src/KBot.DevHarness/Config/rhp_05_muta_fereastra.json` — **new** (replaces
  `rhp_05_muta_splitter.json`)
- `tests/KBot.DevHarness.Tests/MoveWindowTests.vb` — **new**, 20 (was `MoveChildrenTests.vb`)
- `tests/KBot.DevHarness.Tests/AdobeHarnessLayoutTests.vb`, `AdobeHarnessScenarioBindingTests.vb`

## Test results

- `dotnet build KBot.sln` — 0 errors, 0 BC warnings.
- `dotnet test KBot.sln` — **579 passed / 0 failed / 0 skipped** (552 before this pass).
  `KBot.DevHarness.Tests` 116 → 143.
- **Rendered and inspected on screen** with `rhp_05_muta_fereastra.json` loaded: the section sits
  under «Decupare», shows −120 / −90 / 120 / 90, and the status block reads
  `Poziție: dx=-120 dy=-90 dw=120 dh=90`.
- **A defect the render caught:** «Readu la zero» stayed disabled after loading a non-zero scenario —
  `ApplyScenarioToControls` never called `UpdateActionStates`, so a delta arriving from a file did
  not enable the button a typed one did. Fixed, and both directions are now pinned by tests.

## Anything left unverified or deferred

- **Nothing here has been applied to a running Adobe.** Whether pulling the window left actually
  takes the gutter out of view, and whether Adobe clamps the size, are open.
- **The zoom limitation is shared with clipping and untested:** the hosted window really is larger
  than the panel, so a fit-width/fit-page zoom rescales the page to the inflated width. Moving
  cannot avoid that — the earlier claim that it could was tied to moving a CHILD window, which is
  not what this does.
- Composing `clip` with `move` is arithmetically covered by a test but has never been looked at on
  screen; the shipped scenario deliberately turns clipping off.
