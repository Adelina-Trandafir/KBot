# SLICE-0097-03 — Year and Sector selectors move into the caption bar (operator request, 01.10.2026)

Follow-up to 0097 (the unit selector in the caption bar). Help recorded as 0000-33.

## What changed and why

The operator asked for three things:
1. the **year** selector («An Date») moves to the caption bar, next to the unit selector;
2. the **source / sector** selector («Sursă/Sector», the old `cboSs`) moves there too;
3. both are **removed from KbotForm** (the header band under the caption).

- `KBotCaptionBar` now has **three painted selectors**, addressed by name: `SelectorUnit`
  (0097, unchanged behaviour: drawn only with two or more units), `SelectorYear`,
  `SelectorSector` (drawn with one or more choices). They sit after the title:
  «K-BOT — [Unit ▾]   An Date [2026 ▾]   Sursă/Sector [02A ▾]»; the year and sector ones carry a dim
  text label before the box. A selector that does not fit is simply not drawn.
  - New API: `SetSelectorItems(selector, items, key)`, `ClearSelector(selector)`,
    `GetSelectorKey`, `SetSelectorKey` (no event), `SetSelectorShown` (the host hides the sector
    selector when the tree is sorted by date). The unit overloads without a name still work.
  - `SelectorChanged` now carries `Selector` + `Key` (`CaptionSelectorChangedEventArgs(selector, key)`).
  - Help parts: `year`, `ss` added next to `unit` (`HelpPartBounds`); only the unit's text is
    blurred in captures (the year / sector are not sensitive).
  - `KBotCaptionBar.UnitSelector.vb` is replaced by `KBotCaptionBar.Selectors.vb` (the code of the
    old file, generalised from one selector to a small `TitleSelector` per name).
- `KbotForm`: the controls `cboAn`, `cboSs`, `lblAn`, `lblSs` are gone from the Designer; `tlyHeader`
  is now two columns (MENIU button + operator label). `KbotForm.Periods.vb` reads / writes the bar
  (`AnulAles()`, `SsAles()`); the old `SelectedIndexChanged` handlers became one
  `CapBar_PeriodChanged` on `capBar.SelectorChanged` (the unit handler ignores the other two). The
  `_suppressPeriodEvents` flag is gone: setting a key from code never raises the event.
  `KbotForm.Tree/TreeOptions/Units/Chrome/vb` follow.
- **Behaviour kept:** highest year first; SS from `LastSS`; year change rebuilds the SSs and re-reads
  the tree; SS change is remembered on the server and re-reads the tree; unit switch clears both
  selectors then reloads them; sorted by date the sector selector hides and the tree asks for every
  source.
- **Behaviour that changed:** when the periods cannot be read (or the unit has none) the selectors
  are **absent** from the bar, where the old combos were shown disabled. The old tooltips on the two
  combos («An», «Sursă / sector») are gone: the painted selectors have no tooltip, and each carries
  its label instead.

## Files touched

- `src/KBot.Controls/CaptionBar/KBotCaptionBar.Selectors.vb` (**new**, replaces `KBotCaptionBar.UnitSelector.vb`),
  `KBotCaptionBar.vb`, `KBotCaptionBar.HelpParts.vb`
- `src/KBot.App/KbotForm.Designer.vb`, `KbotForm.vb`, `KbotForm.Periods.vb`, `KbotForm.Tree.vb`,
  `KbotForm.TreeOptions.vb`, `KbotForm.Units.vb`, `KbotForm.Chrome.vb`
- Help (0000-33): `HelpContent/tours/tur-fereastra.md`, `contabil/fereastra.md`,
  `contabil/autentificare.md`; `tools/HelpCheck/Check-Help.ps1` (caption bar parts list)

## Test results

- `dotnet build src/KBot.App/KBot.App.vbproj`: **0 errors, 0 warnings** (incl. KBot.Controls, KBot.DevHarness).
- `Check-Help.ps1 -Coverage`: the only error left is `contabil\fereastra.md: slice '0077-3' is not
  in KBOT_STATUS.md`, in a tag this task did not touch.
- **No tests run, no test code written** (operator rule). Nothing seen on screen.

## Left unverified or deferred

- Nothing run, nothing on screen: the three selectors side by side, the label spacing, the cut with
  «…» on a narrow window, the popup under each box, the DPI scaling of the labels.
- The caption bar's tour steps «Anul de lucru» / «Sursa și sectorul» now point at the bar parts
  (`year`, `ss`); the old steps targeting `cboAn` / `cboSs` are gone.
- Pictures: `fereastra-zone` shows the old header band — to re-shoot («Capturi de refăcut»).
- `KBotCaptionBar.md` (control doc) not updated with the selectors API (the unit selector was not in it either).
- FileVersion of KBot.App / KBot.Controls not bumped (`push-update.ps1` asks).
