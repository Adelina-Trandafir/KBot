# SLICE-AV01-06 - «Plan de conturi»: corrective (operator, 25.10.2026)

## What changed and why
Four requests on the «Plan de conturi» page:
1. **Tree look** - the accounts tree takes the look of the «Clasificatii» tree: `Indent 14`, `LeftIconSize 16x16`, `BackColor = Window`,
   the right text in a real column (`RightTextColumn 240`, it was 0 = right-aligned and dropped when it did not fit), and bold only on the
   top level through the node flag (`TreeItem.Bold`) instead of inline `<b>` markup on the codes. The font itself is the control's ambient one in both windows.
2. **The chart follows the source of the title bar** - `PlanContForm` hears `SessionContext.PeriodChanged` (year or sector/source) and reads the
   chart again for the subunit of that source (`PlanContContext.SubunitatiForSource`: subunits whose `SursaSector` is the session's; every subunit on «Toate»).
   When two or more subunits share the source, a new painted selector **«Subunitate»** (`KBotCaptionBar.SelectorSubunit`, drawn only with 2+ choices) appears beside
   «Sursa/Sector»; picking one reloads the chart. The selector is filled and moved by the window through `IPlanContHost` (implemented by `KbotForm`); it does not
   move by itself (the pick can be refused by unsaved changes) and is shown only while the page is on screen. `PlanContContext.An` now reads the session, so the balances follow a year change.
3. **«Afiseaza solduri initiale»** is a button of the ribbon's view group (right group): `plan_cont_solduri` in `AvacontMain.Designer.vb`; `PlanContForm` implements `IViewRibbon`;
   `AvacontMain.UpdateViewGroup` now asks the page on screen when it is not on the shell, and hooks `RibbonStateChanged` of such pages.
4. **The subunit row is gone** from the window (selector, «Subunitate noua», «Modifica subunitatea», «Sterge subunitatea», the balances button).

## Second pass (operator, 25.10.2026)
- «Exporta in Excel» is a ribbon button of the view group (`plan_cont_excel`, next to «Afiseaza solduri...»); `PlanContForm` answers it through `IViewRibbon`.
- «Arata si conturile inactive» left the bottom bar: the right icon of the tree header opens a `CustomPopup` with that one row (checked = on), as on the main tree
  (`PlanContConturiView.Tree_HeaderRightIconClicked`); the view settles what is pending, flips `PlanContContext.ShowInactive` and rebinds. The balances grid reads the same flag.
- «Cont extra» / «Cont activ» check boxes removed from the account fields (rows too). Save writes the account's own `Extra` / `Activ` back (a new account: active, not extra), so
  nothing changes silently. **There is no way to mark an account inactive from the window now** (operator will think of a solution); an already inactive account still shows (italic, dim) when the popup row is on.

## Files touched
`src/KBot.Controls/CaptionBar/KBotCaptionBar.Selectors.vb`; `src/KBot.App/{AvacontMain,AvacontMain.Designer,KbotForm.Periods}.vb`;
`src/KBot.App/Avacont/Configurari/PlanCont/{IPlanContHost,PlanContContext,KbotForm.PlanCont}.vb`;
`.../PlanCont/Views/{PlanContForm,PlanContForm.Designer,PlanContConturiView,PlanContConturiView.Designer}.vb`;
help `contabil/nomenclatoare/plan-conturi.md` (tag `AV01-06`, see 0000-124), `help-version.txt`.

## Test results
`dotnet build src/KBot.App/KBot.App.vbproj`: 0 warnings, 0 errors. No test, nothing run, nothing seen on screen.

## Unverified / deferred
- **Subunit actions removed (operator, 25.10.2026).** New / change / delete subunit are no longer in `PlanContForm` (the row and the dead code are gone; `LoadAll` lost its parameters).
  `SubunitateEditForm` and the API calls stay, unused. A unit with no subunit cannot add one from this window: the interface is planned in
  `docs/access-map/avacont/plan-interfata-subunitati.md` (not critical for now).
- A change of source with unsaved account changes: the question is asked; on «Cancel» the chart on screen stays and the title bar already shows the new source (the bar cannot be vetoed).
- The match source -> subunit is `TZ_Subunitati.SursaSector` = the session's sector+source; accounts keep their own sector+source inside a subunit and are not filtered by it.
- `RightTextColumn 240` is a guess for the longest symbols; check on screen. Never opened in the VS designer.
