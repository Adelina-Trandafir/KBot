# SLICE-0027-03 — the tree's designer values in the four views (Rezervări / Recepții / Plăți / DDF)

Sub-slice of 0027 (`AdvancedTreeControl`). Operator request: «in all the views with
advancedtree, implement the designer values (like in mainform) and remove any code that is
setting the tree properties».

## What changed and why

**1. The look of the tree is now authored in the designer, in all four views.** Until now the
four views dropped a bare `AdvancedTreeControl` (dock, item height, icon sizes, right-text
width) and the header/footer bands simply did not exist there — they only existed in
`MainForm`, where 0027/0027-02 authored them. Each view's tree now carries the same band set
as `MainForm.tree`, with its own caption:

| view | `HeaderCaption` |
|---|---|
| `RezervariView` | `« REZERVĂRI»` |
| `ReceptiiView` | `« RECEPȚII»` |
| `PlatiView` | `« PLĂȚI»` |
| `DdfView` | `« REVIZII DDF»` |

Common to all four: header visible (40px, `Tahoma 9 Bold`, `GradientHorizontal` toward
`CornflowerBlue`, `folder_open` as the left icon, black caption — the same deliberate choice
`MainForm` makes), footer visible (40px) carrying ONLY the collapse button (left side,
`expand_24`/`collapse_24`), `MinimumCollapsedWidth = 120`, `RootExpander = True`.

**No end icons in the footer, and no header right icon.** `MainForm` has them because it has
handlers behind them (refresh / settings). A view has nothing to hang there yet, and an icon
that does nothing when clicked is exactly the silent no-op the house rules forbid.

**No `Font` line.** `DdfView.Designer.vb` carried `tree.Font = New Font("Segoe UI", 9F, …)` —
the tree's own default, written explicitly, which is the trap 0027-02 documented: it sets
`_fontPinned` and makes the tree deaf to `ThemeManager.ApplyBaseFont`. Deleted, not
translated. The other three never had it (0027-02 removed them there).

**2. `ConfigureTree()` is gone from all four.** It only ever set `RootExpander = True`, which
is now a designer line. `DdfView`'s copy was already commented out; the dead block was
removed rather than left as a comment.

**3. The collapse button needed its host half.** The tree is `Dock = Fill` inside
`split.Panel1` in every one of these views, so `HostOwnsWidth` is True and the control does
not write `Width` at all — it flips state and raises `CollapsedChanged`, and the HOST moves
its splitter (the 09.08.2026 correction in 0027-02). Without the handler the button would
have been a visible dead control in four views. Each view therefore gained
`tree_CollapsedChanged` + `ClampSplitter`, the same shape as `MainForm`: remember the
splitter distance, drop `Panel1MinSize` for the duration (a min-size guards the DRAG, and
collapsing is a command, not a drag), fix the splitter while collapsed, clamp the distance so
a narrow view cannot turn a button press into `InvalidOperationException`.

**4. `DdfView`'s two sample designer nodes were removed.** `TreeNodeDefinition1/2` («Ianuarie
~~~ 1.234.567,00», «01.01.2025 ~~~ 1.234.567,00») were left over from designer-surface
experiments. `RebuildFromDefinitions` materializes them at RUNTIME too, and `DdfView`'s
constructor only calls `ShowEmpty` (which does not clear the tree — `ClearAll` does), so the
running app would have shown two fake revisions until the first selection. The
`tree_image_list` binding (`NodeImages`) is kept — it is the designer's icon source, not a
node source.

## Files touched

- `src/KBot.App/Views/RezervariView.Designer.vb` — full tree band block
- `src/KBot.App/Views/ReceptiiView.Designer.vb` — idem
- `src/KBot.App/Views/PlatiView.Designer.vb` — idem
- `src/KBot.App/Views/DdfView.Designer.vb` — band block reworked (caption fixed from the
  copy-pasted «REZERVĂRI»), `Font` line dropped, the two sample node definitions removed
- `src/KBot.App/Views/RezervariView.vb`, `ReceptiiView.vb`, `PlatiView.vb`, `DdfView.vb` —
  `ConfigureTree()` removed, `tree_CollapsedChanged` + `ClampSplitter` + the two
  splitter-state fields added
- `src/KBot.App/KBot.App.vbproj` — `FileVersion` 1.0.9.0 → 1.0.10.0

## Test results

- `dotnet build src\KBot.App\KBot.App.vbproj` — **build succeeded, 0 errors**, 1 warning:
  the pre-existing `MSB3825` (BinaryFormatter) on `DdfView.resx`, from the operator's
  `tree_image_list` image stream. Not introduced here and not touched here.
- `dotnet test tests\KBot.App.Tests` — **148 passed / 1 failed / 149 total**. The failure is
  `DdfViewTests.Leaf_CaptionPadsRevisionNumberWithSpaces`, and it is **pre-existing on this
  branch**: `DdfInfo.EtichetaRevizie` returns `$"{data}"` with the `$"{numar} - {data}"` form
  commented out in the source (`src/KBot.Domain/DdfInfo.vb`, unmodified by this sub-slice —
  it is the same `EtichetaRevizie` family of failures 0027-02 recorded as pre-existing).

## Left unverified / deferred

- **Never seen on screen.** Not one of the four views has been rendered — this is the same
  standing gap as for the views themselves. The collapse behaviour is verified only by
  analogy with `MainForm`, where it WAS seen working after the 0027-02 correction.
- The four views' `split` designers do not set `Panel1MinSize` (so the default 25 already
  clears the 120px target); the min-size dance is carried anyway so a later designer change
  cannot silently veto the collapse. Untested against a non-default `Panel1MinSize` in a view.
- `MinimumCollapsedWidth = 120` and `HeaderCaption` wording are judgement calls, not operator
  choices — the captions in particular («REVIZII DDF») are open to a rename.
- `OrdonantareView` is deliberately out of scope: it is still a `PlaceholderView`.
