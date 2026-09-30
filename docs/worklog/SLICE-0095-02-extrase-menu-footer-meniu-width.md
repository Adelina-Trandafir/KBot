# SLICE-0095-02 - Extrase back on the tree footer, «Extrase de cont» in the menu, btnMeniu follows navViews

Operator request, 29.09.2026 ("this is slice 95"). Slice 0095 already held the Istoric root node
(`SLICE-0095-istoric-tree-root.md`), so this pass is recorded as 0095-02.

1. The left icon of the main tree's footer downloads the SNM bank statements directly again
   (as in slice 0057), instead of opening the «Extrase de cont» window (0080-03).
2. The «Extrase de cont» window is opened from «Meniu → Extrase».
3. When `navViews` collapses, `btnMeniu` takes the same width.

## Change

- `KbotForm.Designer.vb` (operator's own uncommitted edit, kept as found): the `menuNou` entry
  «Extrase» (key `extrase`) + a separator before «Nomenclatoare».
- `KbotForm.Extrase.vb`:
  - `Tree_FooterLeftIconClicked` is `Async Sub` again: `DescarcaExtraseAsync(Me)` and, when it
    imported something, the open Extrase view reloads. Every failure is already told to the
    operator inside `DescarcaExtraseAsync`, so the handler only logs.
  - `DeschideExtrasele()`: the window standalone (modeless, `Show(Me)`), one at a time like
    Clasificatii / Parteneri (asking again activates the open one). On close the Extrase view
    reloads (`ReloadExtraseView`, shared with the footer).
- `KbotForm.Nomenclatoare.vb`:
  - `MenuNou_ItemClicked`: `Case "extrase"` -> `DeschideExtrasele()`.
  - `NavViews_CollapseStateChanged` + `NavViews_SizeChanged` -> `SyncMenuButtonWidth`: column 0
    of `tlyHeader` = `navViews.Width + btnMeniu.Margin.Horizontal`, device px divided by
    `tlyHeader.DpiScale` (the table takes logical px). Active only after the first collapse
    (`_menuFollowsNav`), so the designer width stands until the operator touches the bar;
    after that it also follows DPI / zoom changes of the bar.
  - `ApplyMenuButtonText`: while the bar is not Expanded the button shows only its icon (no
    text fits); expanded it shows «Meniu» / «Meniu (!)».
- `KbotForm.CabNotes.vb`: `MarkUncorrelated` stores the mark in `_menuMarked` and goes through
  `ApplyMenuButtonText`, so a refresh while collapsed does not put the text back.
- `Views/ExtraseForm.vb`: class summary updated (opened from the menu, modeless).

## Test results

- `dotnet build src/KBot.App/KBot.App.vbproj`: **0 warnings, 0 errors**.
- No tests written or run (operator: no tests). Not seen on screen.

## Unverified / deferred

- Not seen on screen: the collapsed button width (the bar's «Icons» state), the icon-only
  button, and the 1px alignment of the button against the bar after expanding again
  (expanded width becomes nav width + margins = 239 logical vs the designer's 240).
- After the first collapse -> expand the caption becomes «  Meniu» (the constant) instead of
  the designer's «  MENIU» - the same thing `MarkUncorrelated` already did at every refresh.
- `btnMeniu`'s tooltip still reads «Angajament nou, clasificațiile bugetare și partenerii.»
  (does not mention Extrase).
- The window is modeless now: while it is open the operator can also press the footer icon
  and start a second download; both go through `DescarcaExtraseAsync` (no re-entry guard
  there, same as before between the view and the window).
- FileVersion KBot.App not bumped (`push-update.ps1` asks).
