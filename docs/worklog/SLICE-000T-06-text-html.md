# SLICE-000T-06/09 — Textul HTML, legăturile și clicurile tutorialelor

Consolidare documentară D002-07. 06 introduce cititorul HTML și editorul; 09 extinde același text interactiv cu legături între tutoriale și consemnează separat reparația clicurilor pe părțile desenate. Corpurile, probele și limitele rămân pe etapă. Cele 86 de verificări privesc cititorul/layoutul/controlul în proiectul-ciornă, nu formularul editorului sau clicurile nevăzute pe ecran.

[06](#slice-000t-06) · [09](#slice-000t-09)

<a id="slice-000t-06"></a>

## SLICE-000T-06 — the bubble text accepts HTML

Request (operator, 04.10.2026): the explanation of a tutorial step must support basic HTML tags (`<b>`, `<BR>`, `div`, colour...), so the text can be written in an external editor and pasted as HTML.

### What changed and why

- **Reader:** `src/KBot.Controls/RichText/KBotHtmlText.vb` (new). `LooksLikeHtml` (any of the supported tags) and `ToRuns` turn the text into the runs the tooltip engine `KBotRichText` already wraps and draws; the HTML pass is `HtmlRunBuilder` (tags, `style=` declarations, `<font>`, colours incl. theme words, lists, whitespace collapsing, breaks from `br` / blocks only, entities). Subset and limits: `RichText/KBotHtmlLabel.md`. A text with none of the tags is PLAIN and drawn as before (line breaks are breaks; a blank line keeps a full line's height with a non-breaking space), so the existing tutorials and the tours look the same.
- **Control:** `src/KBot.Controls/RichText/KBotHtmlLabel.vb` (new, `Inherits KBotLabel`): property `Html` (the base `Text` stays empty), `GetPreferredSize` = height the text needs at the proposed width, rebuilds the runs when the text, font, `ForeColor` or palette changes. `KBotLabel` lost `NotInheritable` (only change to it).
- **Bubble:** `HelpTourBubble.lblText` is a `KBotHtmlLabel` (`.Designer.vb`), `ShowStep` / `ShowTutorial` set `.Html`. `FitToText` was already calling `lblText.GetPreferredSize(New Size(w, 0))`, so the bubble grows to the HTML height with no change there.
- **Format doc:** `HelpContent/README.md`, «Interactive tutorials», paragraph «The bubble text may be HTML». Control doc + `CONTROLS.md` index row.
- **Not changed:** `TutorialFlow` (the body still goes through `HelpTour.CleanText`: lines of a paragraph are joined with a space, `**` removed, a line starting `- ` becomes a bullet — all harmless to HTML, which ignores line breaks anyway); the parser / runner.

### Second pass — the small HTML editor in the designer (operator: «do the easy one»)

`TutorialDesignerForm` (+ `.Designer.vb`): the step text box is now an editor. Under the «Textul din bulă» heading: a button row (B, I, U, a colour drop-down, «Marcaj», «↵ Rând», «• Listă», «Titlu»), then the source box (left, Consolas) and a LIVE PREVIEW (right, a `KBotHtmlLabel` with a border: the same reading as the bubble). The buttons wrap the selected text in the tag (`b i u mark h3`, `<span style="color:...">`), keep it selected, or put the caret between empty tags; «Rând» inserts `<br>`; «Listă» turns each selected line into an `<li>`. The colour list has five theme words (follow the scheme) and four fixed colours (written INTO the tutorial text). The preview updates on every change, including when a step is loaded. The whole row is disabled with no step. Pasting HTML from another editor still works in the same box.

Third pass (operator: «scoate tooltipurile din TutorialDesignerForm»): ALL tooltips of the designer are gone — the 42 `SetToolTipHeader/Text` lines, the `tips` `KBotToolTip` component and its field, the unused `ComponentResourceManager`, and the three tooltip entries in `TutorialDesignerForm.resx` (`tips.TrayLocation`, `txtText.ToolTipText`, `btnInregistreaza.ToolTipText`). The plain-vs-HTML rules that the text box tooltip explained are in `HelpContent/README.md`. The designer is an operator tool outside the help, so nothing else changes.

Not done on purpose: no WYSIWYG, no undo of its own (Ctrl+Z of the text box works), no tag completion.

### Files touched

`src/KBot.Controls/RichText/KBotHtmlText.vb` (new) · `src/KBot.Controls/RichText/KBotHtmlLabel.vb` (new) · `src/KBot.Controls/RichText/KBotHtmlLabel.md` (new) · `src/KBot.Controls/CONTROLS.md` · `src/KBot.Controls/Label/KBotLabel.vb` · `src/KBot.App/Help/HelpTourBubble.vb` · `src/KBot.App/Help/HelpTourBubble.Designer.vb` · `src/KBot.App/Tutorial/TutorialDesignerForm.vb` · `src/KBot.App/Tutorial/TutorialDesignerForm.Designer.vb` · `src/KBot.App/HelpContent/README.md` · `docs/worklog/KBOT_STATUS.md` · `docs/worklog/state/KBOT_STATUS_0000-0009.md`.

### Help

No help topic changes: nothing the operator DOES changes, and the help is for users only. The authoring format is in `HelpContent/README.md` (for whoever writes tutorials). No NOUTATI line (the operator sees no difference until a tutorial uses HTML).

### Test results

`dotnet build src\KBot.App\KBot.App.vbproj`: **0 warnings, 0 errors**.

On the operator's request («testeaza ce ai facut», after notice): a throwaway console project OUTSIDE the repo (session scratchpad, references `KBot.Controls`, opens no window) with **86 checks, all passing**: what counts as HTML; plain text unchanged (breaks, blank line, `&`); `br` / `div` / `p` / lists / headings / trailing and leading `br`; whitespace and entities, comments, `head/style` skipped, unknown tags; bold / italic / underline / strike and nesting, mis-nested closes; colours (`#rrggbb`, `#rgb`, `rgb()`, names, bad value, theme word with and without a palette); background; css weight / style / decoration / size (em, pt, keywords, `<font size>`); `code`; junk input never throws; the layout (narrow is taller, `br br` = 3 lines, paragraph gap smaller than a blank line, plain blank line = HTML blank line); the control's `GetPreferredSize` (width as asked, taller than empty, narrow taller than wide, plain, rebuild after `ForeColor`, empty). **One real bug found and fixed by it:** `a < b si c > d` was read as the `<b>` tag (whitespace allowed after `<`); both patterns now need `<` right against the name, as in a browser. The scratch project is not part of the repo, so these checks are not repeatable from it.

### Unverified / deferred

- **Never run, never seen on screen**: the layout of the bubble with HTML, the colours on each scheme, the heights at 125 / 150 % scaling, the blank-line / paragraph gaps (a small gap is `0.55 x` the line height) and the look next to the note line.
- GDI+ text (`KBotRichText`) in the bubble instead of the GDI text of a `Label`: may differ by a pixel.
- Wrapped list lines are not indented; a scheme switch with unchanged palette object and `ForeColor` keeps old theme-word colours.
- The designer editor (second pass) was only built: never opened. Its layout at the designer's 144 dpi numbers, the row of buttons at narrow widths, and the preview next to the source are unseen. The 86 checks cover the reader, not this form.

<a id="slice-000t-09"></a>

## SLICE-000T-09 — a link from one tutorial to another

Request (operator, 05.10.2026): a step text can contain a link to another tutorial (`<link>TUTORIAL</link>` in `ordonantare-din-plata.md`); a click stops the running tutorial and starts the one clicked.

### What changed and why

- **Syntax:** `<link tutorial="id">text</link>` (`<a tutorial="id">` too). `KBotHtmlText` reads it: underlined, accent colour, the id travels on the run (`KBotRichText.RichRun.Link`, `HtmlStyle.Link`). `link` was a skipped void tag before; it is now an element.
- **Click:** `KBotRichText.LinkAt` (same geometry as `Draw`) → `KBotHtmlLabel` hand cursor + `LinkClicked(id)` on left release → `HelpTourBubble.TutorialLinkClicked` → `TutorialRunner.OnLinkClicked` posts `HelpService.StartTutorial(id)` on the host's queue (the click comes from the bubble the old tutorial closes). `TutorialRunner.Start` already ends the running tutorial first, so nothing else was needed for «stop this, start that».
- **Rules that fall out of the existing code:** a mandatory tutorial is not ended (it reminds); the target's `starts:` window must be open (its usual message otherwise); an unknown id is logged and told («Tutorialul din legătură nu a putut fi pornit…») and the running tutorial goes on; tours draw links but do nothing.
- **Checker:** `Check-Help.ps1` errors on a link to an unknown tutorial, warns on a link without `tutorial=` and on a self-link.
- **Docs:** `HelpContent/README.md` («Links to another tutorial»).
- The menu-row anchor (`menu.<key>`) code comments also say 000T-09; it is recorded in `SLICE-000T-07` (fourth pass).

### Second pass — the recorder and the footer / header icons of the tree (operator, 05.10.2026)

Report: pressing an icon of the tree footer (probably the header too) is not recorded as that click; the action just happens. **Cause (read in the code, not seen running):** `TutorialPicker.SuggestWait` gave every press on a tree that was not the row button the wait `select`, so the recorder wrote `part: footer.left` + `wait: select`; at run time `select` listens to `NodeMouseUp`, which the footer / header icons never raise (the tree handles them in `OnMouseDown` and returns), and the old `click` on a tree listened only to the row button. A step recorded that way could never complete. A press on a grid part was never recorded at all (`Manual`).
- **Recorder / picker:** tree: no part = `select`; `header` / `footer` bands = `manual` (nothing to wait for); any other part (`footer.left/right/collapse`, `header.search/right`, `columns`, `node.icon`) = `click`. Grid (`KBotDataView`): any part but `footer` = `click`; cells stay `manual`. Step title for a click on a part: «Apasă Type.control (part)».
- **Runner:** `wait: click` on a control that draws parts, with a `part:` (not `node.icon`, not `item:`), now watches the left `MouseDown` and checks the point against `HelpPartBounds(part)` (`PartContains`). The tree acts on the press itself, so the tutorial moves on just before the control's own action; `OnTick` re-reads the layout afterwards. `click` on a tree without part is unchanged.
- **Docs:** README `wait:` row. Build 0 warnings, 0 errors.
- **Unverified, nothing seen on screen:** recording a footer / header press and playing the step back; the caption bar's buttons (`help`, `options`…) and the nav list's `collapse` are still recorded as `manual` (not asked for).
- Steps already recorded with `select` on a footer part must be changed to `click` by hand.

### Files touched

`src/KBot.Controls/ToolTip/KBotRichText.vb` · `RichText/KBotHtmlText.vb` · `RichText/KBotHtmlLabel.vb` · `src/KBot.App/Help/HelpTourBubble.vb` · `src/KBot.App/Tutorial/TutorialRunner.vb` · `tools/HelpCheck/Check-Help.ps1` · `HelpContent/README.md`.

### Test results

`dotnet build src\KBot.App\KBot.App.vbproj` (scratch output folder): 0 warnings, 0 errors. `Check-Help.ps1 -Coverage`: «No errors.» (one warning: `ordonantare-din-plata.md` has a `<link>` with no `tutorial=`). No tests written or run.

### Left unverified or deferred

- **Nothing seen on screen:** hand cursor, the click on the underlined text, the old bubble closing and the new tutorial starting.
- `ordonantare-din-plata.md` still has the bare `<link>TUTORIAL</link>`: the tutorial for downloading the statements does not exist yet; when it does, write `<link tutorial="its-id">…</link>`.
- Help text (`contabil/ajutor.md`) not touched: nothing a user reads there changes until a tutorial actually carries a link.
