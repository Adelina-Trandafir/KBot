# SLICE-0020-02 — client: `DdfView`

Pass 2 of slice 0020 (vederea DDF), per `PLAN_DdfView.md` §5. The **fifth real view** — after
Sumar (0011), Rezervări (0014), Recepții (0015), Plăți (0017) — replacing
`MainForm.CreateView("ddf")` → `PlaceholderView`.

## What changed and why

A master/detail view: an `AdvancedTreeControl` of revisions on the left, and on the right a
**horizontal** `KBotNavList` (decision 8) over three lazily-shown pages. Read-only. One
`GetDdfAsync` per `SetContext` through `WithReauth(Of DdfInfo)` with the `_requestedCod`
stale-guard; **every tree click and combo change filters data already in memory — no request**
(decision 7).

- **Tree — two levels.** Month roots (`LA_{yyyy}_{M}`, grouped on year+month of `DataRev`,
  chronological, **expanded**) → revision leaves (`RC_{IDREV}`, caption
  `{NumarRev.PadLeft(3)} - {dd.MM.yyyy}`, value `TotalRevizie`). Leaf icon: `Incarcat` → up,
  else `Preluat` → down, else neutral (Access `REV_SUS`/`REV_JOS`/`REV_NOT`). Leaf tooltip =
  `Desc_Scurta`. Each node's `Tag` carries a `DdfNodeRows` (its section-A lines + whether it is
  a root + the leaf's `RevizieRow`, which passes 03/04 need for the PDF path).
- **Pages — visibility, not a `TabControl`.** `pnlValori` / `pnlPreview` / `pnlFisiere`, exactly
  one `Visible`, the same lazy pattern `MainForm` uses for views. Preview and Fișiere are empty
  shells with a Romanian empty state; passes 03 and 04 fill them.
- **Grid.** `Clsf`, `ElementFund` (the only `AutoHide` column), `Data reviziei` (**root level
  only**), `ValPrec`, `ValCur`, `ValTot`. Totals row on with **Sum on `ValCur` only**
  (decision 5). `Clasificatii.Denumire` is not shown (decision 4). Sorting: root → `Clsf, DataRev`;
  leaf → `Clsf`.
- **Clsf combo.** Rebuilt from the clicked node's distinct `Clsf`, sorted, with
  `< Arată toate clasificațiile >` first and selected — **unconditionally on every node click**
  (decision 6), never trying to preserve the previous selection. `_suppressComboEvent` stops the
  rebuild from re-filtering mid-flight.

### Deviations from Access (deliberate, mirroring the pass-01 endpoint)

| Access | Here |
|---|---|
| month root value is the literal `0` (`"…~~~0"` to `AddTree_Root`) | the real sum of its leaves' `TotalRevizie` |
| `cRoot.foreColor = cNode.foreColor` — the root inherits **whichever leaf was processed last** | a root is red **only when its own total is negative** |
| a revision with no section-A line vanishes (`INNER JOIN`) | it stays, with total `0,00` |

Each has a test pinning it.

### Two plan inaccuracies found by reading the real files

1. **§5 says the leaf icons go "through `FxIcons`". They cannot.** `FxIcons` loads **embedded
   image resources** keyed on an angajament's `Stare` — it has no palette-tinted GDI drawing.
   The pattern §5 actually describes ("GDI-drawn and palette-tinted, as in Rezervări, no binary
   resources") lives in `RezervariIcons` / `PlatiIcons` / `ReceptiiIcons`, **one icon class per
   view**. Followed the real convention: added `DdfIcons`.
2. **§8 item 5 — `AutoHide` on a non-rightmost column is fine; the column does not need moving.**
   `PerformAutoHide` (`KBotDataView.AutoSize.vb:176`) iterates right-to-left but `Continue For`s
   past any column without `AutoHide`, so a single auto-hiding column is found wherever it sits.
   The only protected column is the fill target, which under `ColumnFillMode.LastColumn` is
   `ValTot` — not `ElementFund`. **Open item 5 is closed.**

`KBotNavList`'s real API was confirmed from the file, as §0 demanded: `Orientation As
KBotNavOrientation` with `.Horizontal` (0018 added it).

## Files touched

- `src/KBot.App/Views/DdfView.Designer.vb` — **new**. Every control declared here (house rule);
  children added in **reverse dock order** (Fill first, Top last) inside each card panel.
- `src/KBot.App/Views/DdfView.vb` — **new**. Also declares `DdfNodeRows` (the node payload POCO).
- `src/KBot.App/DdfIcons.vb` — **new**. GDI, palette-tinted, cached on (state, colour, size).
- `src/KBot.App/MainForm.vb` — `Case "ddf"` now returns `New DdfView(...)`.
- `tests/KBot.App.Tests/DdfViewTests.vb` — **new**, 19 headless STA tests.

## Test results

- **.NET** — `dotnet build KBot.sln`: **0 errors, 0 BC warnings** (only the pre-existing NU1701
  iTextSharp/BouncyCastle ones). `dotnet test KBot.sln`: **355 passed, 0 failed**
  (App 69, Api 61, Controls 134, Theming 27, Xfa 39, Domain 17, Common 7, LocalStore 1).
- Python untouched this pass (still 75 passed / 14 skipped from 0020-01).

**A test-harness bug the tests caught:** `FindControl(Of ComboBox)` walks depth-first and returned
`KBotDataView`'s own floating **editor** ComboBox instead of `cboClsf`, so two combo tests failed
against the wrong control. Fixed by looking the filter up **by name** (`FindByName`, the helper
`PlatiViewTests` already carries for this reason). Product code was not at fault.

## Anything left unverified or deferred

1. **NO VISUAL VERDICT.** Same standing caveat as every view since 0010: verified by compilation
   and headless tests only. The view has **never been seen on screen** — layout, the horizontal
   nav's measured widths, column widths, and the auto-hide behaviour under a real width are all
   unobserved. `AutoHide` is argued from reading `PerformAutoHide`, not from watching a column
   disappear.
2. **Never run against a real database** — inherits 0020-01's caveat; the view has only ever seen
   hand-built `DdfInfo` objects.
3. **`_antet` is loaded and held but not yet used.** It carries `CUAL` / `PartAng` /
   `NumePartener` for the PDF path; passes 03/04 consume it. Deliberate, not dead code.
4. **The multi-header warning path is untested.** When `Antet.Count > 1` the view logs via
   `GlobalErrorLog` and picks by `_preferredIddf`; no test exercises it because §3 item 5 has not
   yet told us whether that case exists in real data.
5. **`pnlPreview` / `pnlFisiere` are empty shells** with placeholder Romanian text. Their labels
   are worded as an empty state, but the Fișiere one still says the list "va fi disponibilă
   într-o etapă următoare" — replace that string in pass 04 rather than leaving it shipped.
6. **Sub-nav gating is not wired to anything.** All three pages are always reachable; nothing
   disables Vizualizare/Fișiere when there is no document. Passes 03/04 decide that.
