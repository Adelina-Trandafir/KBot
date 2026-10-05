# SLICE-000T-10 — fourteen tutorials for the application + what they needed

Request (operator, 05.10.2026): (00) the tutorial designer / recorder only in Debug; (0) two tutorials for connecting to FOREXE
(default certificate, chosen certificate); (1) downloading the statements — active only while connected to FOREXE (or in Debug), last
step shows the FOREXE band; (2) the Extrase view (filtering, display mode); (3) the «Extrase de cont» window; (4) three for the
classifications (add, budget, correction; the tick boxes optionally); (5) partners (add, edit, CODFISCAL searched at ANAF, tick boxes);
(6) changing the receptii ↔ snapshot links (both trees and the grids under them); (7) unit / year / source-sector (optional while the
selector is shown); (8) the order of the main tree; (9) updating several angajamente; (10) downloading new angajamente (different by sort).
Style as `revizie-din-rezervare.md` (`<BR>` before `<mark>`).

## What changed and why

- **00:** the «Designer tutoriale» row of the MENIU exists only in a Debug build (`KbotForm.TutorialDesignerAvailable`, `#If DEBUG`;
  the opener refuses in Release too). **No «Tutorial nou» option was found in the «?» popup** (only that row and the designer's own
  «(tutorial nou)» entry) — if the operator meant something else it is still to be done.
- **Tutorial files (14):** `conectare-forexe-implicit`, `conectare-forexe-selectie`, `descarca-extrase`, `vederea-extrase`,
  `fereastra-extrase`, `clasificatii-adaugare`, `clasificatii-buget`, `clasificatii-rectificare`, `parteneri`, `legaturile-receptiilor`,
  `schimba-unitatea-anul`, `ordonare-arbore`, `actualizare-multipla`, `descarcare-angajamente-noi`, all in `HelpContent/tutorials/`,
  each step tagged `000T-10`. Those that work in a window opened from the MENIU start with optional «open it» steps, skipped by
  themselves when the window is already open.
- **`requires: forexe`** (header key, `TutorialFlow.Requires`): `ITutorialRequirements` (new, `Tutorial/ITutorialRequirements.vb`),
  implemented by `KbotForm` (live session via `_controller.IsConnected`, always true in Debug). `HelpService.TutorialAvailable` hides such
  a tutorial from the «?» list and from typed-question results and refuses to start it (also from a link) with
  «Acest tutorial se poate face doar cât ești conectat la FOREXE…». Used by `descarca-extrase`.
- **New step waits / conditions:** `wait: anchor:<name>` (done when the anchor is on screen: the «Nomenclatoare» submenu opening);
  `when: condition:sort-date|sort-name` (how the main tree is sorted; `descarcare-angajamente-noi` has a step for each);
  `when: visible` with a `part:` now needs the part drawn (the title bar's unit / year / ss selectors); `signal:a|b` = either.
- **New anchors / signals of the main window:** `popup.<key>` (+ `a+b` = one ring round both rows) for the tree options list
  (`CustomPopup.RowScreenBounds`, new); signals `tree-menu:<key>` (a row of that list chosen) and `selector-unit|year|ss`
  (`KbotForm.Tutorial_SelectorChanged`).
- **Window scope in a target:** `Window>Type.control` (`HelpTourRunner.FindTarget`; the checker understands it) because the Extrase
  view and the «Extrase de cont» window both hold an `ExtrasePanel`.
- **Runner:** the bubble is made again when the target appears or goes (a pop-up list that opens a moment after the press), so the note
  «nu e pe ecran acum» does not stay on a step whose ring is already there.
- **Operator text fixed:** the tooltip of the main tree's footer-left icon said it opened the statements window; the code downloads the
  statements directly (the window is MENIU › Extrase) — now says so (`KbotForm.Designer.vb`).
- **Docs:** `HelpContent/README.md` (requires, wait/when/anchors/signals, window scope, skippable opening steps); `Check-Help.ps1`
  (`requires`, `anchor` wait, `condition`, `Window>` scope).

## Files touched

`src/KBot.App/KbotForm.Tutorial.vb` · `KbotForm.HelpCapture.vb` · `KbotForm.TreeOptions.vb` · `KbotForm.Designer.vb` ·
`src/KBot.App/Tutorial/TutorialFlow.vb` · `TutorialRunner.vb` · `ITutorialRequirements.vb` (new) ·
`src/KBot.App/Help/HelpService.vb` · `HelpLibrary.vb` · `HelpTourRunner.vb` · `src/KBot.Controls/Popup/CustomPopup.vb` ·
`HelpContent/tutorials/*.md` (14 new) · `HelpContent/README.md` · `tools/HelpCheck/Check-Help.ps1` · status file.

## Test results

`dotnet build src\KBot.App\KBot.App.vbproj` (scratch output folder): 0 warnings, 0 errors. `Check-Help.ps1 -Coverage`: the 14 new tutorials
pass (only the usual «does not implement IKBotTutorialHost» warnings); the only errors are in `ordonantare-din-plata.md` (the operator's own file,
edited meanwhile: blank lines between a `##` heading and its slice tag). No tests written or run.

## Left unverified or deferred

- **Nothing was seen on screen.** All control names, parts and captions were read from the designers / code, not tried. To check by
  hand, per tutorial: the ring lands on the named thing; the pop-up / submenu rings (`popup.…`, `menu.…`) appear a moment after the press;
  the optional opening steps skip when the window is open; `descarca-extrase` disappears from the «?» list when not connected (Release).
- Steps that ask the user to drag, tick boxes in a tree or type in a grid are `manual` («Înainte»): the runner cannot see those gestures.
- Claims in the texts that come only from code comments, to be confirmed by the operator: ANAF search on leaving the CODFISCAL field;
  new angajamente sit at the end of a by-date tree until downloaded in full; the unit switch closes the FOREXE session; the «Fără Token» button.
- The Extrase view's «Filtrarea coloanelor» step needs the headers grid on screen (root or month node) — otherwise it is skipped.
- `ordonantare-din-plata.md`: the operator's intro still needs `<link tutorial="descarca-extrase">…</link>` where they want it.
- The three tutorials of the classifications / partners / extrase window use the same three opening steps; if the MENIU changes
  (keys `nomenclatoare`, `clasificatii`, `parteneri`, `extrase`) they all change.
- NOUTATI.md not touched (written when a version is published).
