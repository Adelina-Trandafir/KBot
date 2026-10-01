# SLICE-0000-32 — Help window: one continuous page, print, export by scope, and the freeze on «Exportă»

Operator request, 01.10.2026. Reported: pressing «Exportă» in the help window froze K-BOT (Visual
Studio: «The debugger timed out trying to pause process ... broken state»). Asked for: (1) scrolling
in the help window goes through the whole contents, and the tree selects the topic reached; (2) the
new `btnPrint` (added by the operator in `tlyBara`) prints, from a menu: everything (with «(!)» and a
confirmation that recommends against it) / the current topic / everything of the parent topic;
(3) the same menu on export. During the task the operator dropped the temporary PDF: «if we can print
straight from the help window, let's do that, as long as it keeps the formatting and images».

## What changed and why

1. **The freeze on «Exportă» (hypothesis from the code, NOT proven by a log or a dump).**
   Since 0000-23 the help window closes itself when a dialog disables it and, 150 ms later, no enabled
   window owned by it is found (`TmrModal_Tick`). The export's «Save as» is the help window's own
   dialog, but a file dialog can take longer than 150 ms to appear (first use, more so under a
   debugger). In that case the check found nothing, took the dialog for another window's and closed the
   help window -- the owner of a dialog that was just being opened, while every other K-BOT window was
   still disabled for it. That leaves a modal loop with no window: K-BOT frozen. The 0000-01 open
   thread already said the export had never been pressed on screen, and 0000-23 added the check
   afterwards, so the path was never exercised.
   Fix, two layers (`HelpForm.vb`, `HelpWindowNative.vb`):
   - every dialog the help window opens itself (messages, «Save as», print) runs inside
     `Using OwnDialog()`; no check starts or closes anything meanwhile, and for 2 s after it returns;
   - a check that finds no dialog on screen at all yet (`HelpWindowNative.AnyEnabledWindow`) looks
     again 150 ms later instead of closing. Another window's dialog still closes the help window, as
     before, once that dialog is on screen.
2. **One page, the whole help.** `HelpHtml.Book` builds ONE document: the start page, each visible
   part's heading, every topic in contents order, each in its own `<div class='sec'>` with an id
   (`home`, `p-<part>`, `t-<topic id>`; a `## ` section is `t-<id>--<section>`). `HelpForm.Render`
   now scrolls to a section instead of loading a page; the document is built once per opening (and
   again on a theme change, a new picture, a topic reload). `HelpHtml.TopicPage` / `HomePage` are gone.
   - **The tree follows the scroll**: `tmrScroll` (250 ms, in the designer) reads how far the page is
     scrolled and, only when that changed, asks the page what is at the top of the view
     (`GetElementFromPoint`) and selects that topic's node. A timer rather than the page's scroll
     event: nothing has to be wired into each loaded document. On a read failure the timer stops (one
     log line, not four a second).
   - A topic reached by scrolling becomes `HelpHistory.Current` (so «Înapoi» returns there after a
     jump) but gets no «Istoric» line until it is left by a jump.
   - Part nodes of the tree now scroll to the part's heading (`p:<part>`); they used to open the start
     page. A click on the already selected node goes back to the start of its topic.
   - Text size in place: what was at the top of the view is scrolled back there after the change.
   - A topic this login may not read, or a missing one, still shows the message page (it replaces the
     document; the next page change loads the book again).
3. **«Imprimă»** (`btnPrint`, now always visible; it had inherited the old «Capturi...» button's
   capture-mode visibility through the rename). Menu `mnuPrint` -> `HelpScope` rows:
   «Tot ajutorul», «Subiectul curent: ...», «... cu subiectele de sub el (n)» (only when the topic has
   children), «Tot capitolul «...» (n subiecte)» (the parent with everything under it; only when
   there is a parent). Printing is straight from the window: the book carries `@media print` rules
   (black on white, fixed 10.5 pt, no start page / tour links / «În acest capitol»), the sections
   outside the scope get the class `noprint`, then `WebBrowser.ShowPrintDialog()` (Windows' print
   dialog, the operator chooses the printer). No PDF, no temporary file.
   «Tot ajutorul» and any scope over `HelpScope.LargeTopicCount` (15) topics carry «(!)» and are asked
   about first (default «Nu», the text recommends printing only what is needed).
4. **«Exportă»** (`btnManual`): the same menu (`mnuExport`), no «(!)» (nothing goes on paper).
   `HelpService.ExportManual` -> `HelpService.Export(owner, scope)`: everything = the manual as before
   (`HelpHtml.Manual`, cover + contents, `ManualParts`); less = `HelpHtml.Excerpt` (just those topics;
   a link to a topic outside the file becomes plain text). File name `Manual_KBOT_<date>.html` or
   `Ajutor_KBOT_<topic id>_<date>.html`.
5. **The «Capturi...» button of the help window is gone** (the operator replaced it with `btnPrint`
   in the designer). The capture list still opens from the main window's capture menu
   (`KbotForm.HelpCapture.vb`).
6. Help text: `contabil.ajutor` -- «Fereastra de ajutor» rewritten (continuous page, the buttons as
   they are now: icons, «Imprimă»), new section «Tipărirea și exportul»; new keywords. The start page
   says the whole help is on one page.

Design choices (decided without asking, say if wrong):
- the extra row «... cu subiectele de sub el»: on a chapter page (e.g. «Documentul de fundamentare»)
  «the parent» is «Despre K-BOT», i.e. the whole Part 1, so without this row a chapter could not be
  printed alone;
- «(!)» + confirmation also on a parent scope over 15 topics (that same «Despre K-BOT» case = 39);
- export keeps HTML (opens in the browser, «Save as PDF» there), as before.

## Files touched

- `src/KBot.App/Help/HelpForm.vb` (rewritten in place), `HelpForm.Designer.vb` (`mnuPrint`,
  `mnuExport`, `tmrScroll`; `btnPrint.Visible = False` removed; tooltips of `btnPrint`, `btnManual`)
- `src/KBot.App/Help/HelpHtml.vb` (`Book`, `Excerpt`, anchors, screen + paper CSS; `TopicPage`,
  `HomePage` removed)
- `src/KBot.App/Help/HelpScope.vb` (new)
- `src/KBot.App/Help/HelpService.vb` (`Export`), `HelpLibrary.vb` (`Branch`),
  `HelpWindowNative.vb` (`AnyEnabledWindow`)
- `src/KBot.App/HelpContent/contabil/ajutor.md`
- `docs/HELP_SYSTEM.md` (§1, engine table)
- `docs/worklog/KBOT_STATUS.md`, `docs/worklog/state/KBOT_STATUS_0000-0009.md`, this file

## Test results

- `dotnet build src\KBot.App\KBot.App.vbproj`: **0 warnings, 0 errors**.
- `Check-Help.ps1 -Coverage`: see «left unverified» (two findings, neither from this slice).
- No tests written, run or built (operator's rule). **Nothing was run or seen on screen.**

## Capturi de refăcut

- `ajutor-fereastra-cautare` (the bar now has icons, «Imprimă» and «Exportă»; the page is continuous).

## Left unverified or deferred

- **The cause of the freeze is read from the code, not measured.** If «Exportă» still freezes after
  this build, the hypothesis is wrong and a dump / the Call Stack of the main thread at the freeze is
  needed.
- **Never seen on screen**, all of it: the continuous page (about 2.8 MB of pictures embedded in one
  document -- loading time on a slow PC unknown), the tree following the scroll, the print dialog, the
  print rules (whether the left-out sections really stay out depends on the `class` attribute being
  honoured by the page's rendering mode; both attribute names are written).
- **Windows' print adds its own header and footer** (page title, page number, «about:blank», date):
  they come from the operator's Internet Explorer page setup, K-BOT does not change that setting.
- **Print dialog timing**: `ShowPrintDialog` is assumed to return after the dialog closes; the 2 s
  grace after an own dialog covers the other case.
- `Check-Help.ps1` reports, both older than this slice and not touched: slice tag `0077-3` in
  `contabil\fereastra.md` (no such slice in the index), and three windows without a topic
  (`HelpCaptureViewForm`, `SetariIstoricView`, `UpdateOfferForm`).
- De citit de operator: the new section «Tipărirea și exportul» and the confirmation text.
- Not committed (operator: no git).
