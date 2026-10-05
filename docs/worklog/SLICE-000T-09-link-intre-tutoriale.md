# SLICE-000T-09 — a link from one tutorial to another

Request (operator, 05.10.2026): a step text can contain a link to another tutorial (`<link>TUTORIAL</link>` in
`ordonantare-din-plata.md`); a click stops the running tutorial and starts the one clicked.

## What changed and why

- **Syntax:** `<link tutorial="id">text</link>` (`<a tutorial="id">` too). `KBotHtmlText` reads it: underlined, accent colour,
  the id travels on the run (`KBotRichText.RichRun.Link`, `HtmlStyle.Link`). `link` was a skipped void tag before; it is now an element.
- **Click:** `KBotRichText.LinkAt` (same geometry as `Draw`) → `KBotHtmlLabel` hand cursor + `LinkClicked(id)` on left release →
  `HelpTourBubble.TutorialLinkClicked` → `TutorialRunner.OnLinkClicked` posts `HelpService.StartTutorial(id)` on the host's queue
  (the click comes from the bubble the old tutorial closes). `TutorialRunner.Start` already ends the running tutorial first, so
  nothing else was needed for «stop this, start that».
- **Rules that fall out of the existing code:** a mandatory tutorial is not ended (it reminds); the target's `starts:` window must be
  open (its usual message otherwise); an unknown id is logged and told («Tutorialul din legătură nu a putut fi pornit…») and the running
  tutorial goes on; tours draw links but do nothing.
- **Checker:** `Check-Help.ps1` errors on a link to an unknown tutorial, warns on a link without `tutorial=` and on a self-link.
- **Docs:** `HelpContent/README.md` («Links to another tutorial»).
- The menu-row anchor (`menu.<key>`) code comments also say 000T-09; it is recorded in `SLICE-000T-07` (fourth pass).

## Second pass — the recorder and the footer / header icons of the tree (operator, 05.10.2026)

Report: pressing an icon of the tree footer (probably the header too) is not recorded as that click; the action just happens.
**Cause (read in the code, not seen running):** `TutorialPicker.SuggestWait` gave every press on a tree that was not the row button the
wait `select`, so the recorder wrote `part: footer.left` + `wait: select`; at run time `select` listens to `NodeMouseUp`, which the
footer / header icons never raise (the tree handles them in `OnMouseDown` and returns), and the old `click` on a tree listened only to the
row button. A step recorded that way could never complete. A press on a grid part was never recorded at all (`Manual`).
- **Recorder / picker:** tree: no part = `select`; `header` / `footer` bands = `manual` (nothing to wait for); any other part
  (`footer.left/right/collapse`, `header.search/right`, `columns`, `node.icon`) = `click`. Grid (`KBotDataView`): any part but `footer`
  = `click`; cells stay `manual`. Step title for a click on a part: «Apasă Type.control (part)».
- **Runner:** `wait: click` on a control that draws parts, with a `part:` (not `node.icon`, not `item:`), now watches the left
  `MouseDown` and checks the point against `HelpPartBounds(part)` (`PartContains`). The tree acts on the press itself, so the tutorial
  moves on just before the control's own action; `OnTick` re-reads the layout afterwards. `click` on a tree without part is unchanged.
- **Docs:** README `wait:` row. Build 0 warnings, 0 errors.
- **Unverified, nothing seen on screen:** recording a footer / header press and playing the step back; the caption bar's buttons
  (`help`, `options`…) and the nav list's `collapse` are still recorded as `manual` (not asked for).
- Steps already recorded with `select` on a footer part must be changed to `click` by hand.

## Files touched

`src/KBot.Controls/ToolTip/KBotRichText.vb` · `RichText/KBotHtmlText.vb` · `RichText/KBotHtmlLabel.vb` ·
`src/KBot.App/Help/HelpTourBubble.vb` · `src/KBot.App/Tutorial/TutorialRunner.vb` · `tools/HelpCheck/Check-Help.ps1` ·
`HelpContent/README.md`.

## Test results

`dotnet build src\KBot.App\KBot.App.vbproj` (scratch output folder): 0 warnings, 0 errors. `Check-Help.ps1 -Coverage`: «No errors.»
(one warning: `ordonantare-din-plata.md` has a `<link>` with no `tutorial=`). No tests written or run.

## Left unverified or deferred

- **Nothing seen on screen:** hand cursor, the click on the underlined text, the old bubble closing and the new tutorial starting.
- `ordonantare-din-plata.md` still has the bare `<link>TUTORIAL</link>`: the tutorial for downloading the statements does not exist
  yet; when it does, write `<link tutorial="its-id">…</link>`.
- Help text (`contabil/ajutor.md`) not touched: nothing a user reads there changes until a tutorial actually carries a link.
