# K-BOT help system — maintainer guide (slice 0000)

Read this before touching anything under `src/KBot.App/HelpContent/` or `src/KBot.App/Help/`.
It is written for the next thread: after the operator changes or adds a feature, you should be able
to bring the help up to date from this file alone.

Status and history: [worklog/state/KBOT_STATUS_0000-0009.md](worklog/state/KBOT_STATUS_0000-0009.md)
(section «Slice 0000»). Authoring syntax in detail: [../src/KBot.App/HelpContent/README.md](../src/KBot.App/HelpContent/README.md).

---

## 1. What it is

- **F1 anywhere** (any K-BOT window) opens the help window on the topic of the control under
  focus.
- The **«?» button** on every caption bar (slice 0000-20) opens a **popup** under the button: a
  search box, «Pe ecranul acesta» (the topic F1 would open), the guided tours of the windows on
  screen (one folder per window when several have tours) and «Deschide ajutorul complet (F1)».
  The **login window has no «?»** (slice 0097-02, operator: `LoginForm.capBar.ShowHelpButton =
  False`); F1 still works there.
- The **search** (slice 0000-18) takes a typed question: filler words, diacritics and word
  endings do not matter, not every word must match, and it finds SECTIONS (a hit opens the page
  at its `## ` heading). A hit whose topic has `open:` carries «Deschide «...»» (0000-19); one
  whose topic has a tour carries «Tur ghidat». The popup and the help window use the same search
  control.
- **Questions and ratings** (slice 0000-21): every question typed there, with the hits shown,
  the hit used and a 1-5 star rating, is sent to the server WITHOUT anything about who or where
  (see §7).
- The **help window**: contents tree, the search box above it (its results take the tree's place
  while it has text), Back / Forward, «Istoric», text smaller / larger, guided tours, «Imprimă»,
  «Exportă». Slice 0000-32:
  - **one page, the whole help**: the browser holds ONE document (`HelpHtml.Book`: the start page,
    then every visible topic in contents order, each in a `<div class='sec'>` with an id). Opening
    a topic scrolls to its section; scrolling by hand walks into the next topic and the contents
    tree follows (`HelpForm.tmrScroll`, 250 ms: one hit-test at the top of the view, only after the
    page has moved). A topic reached by scrolling becomes the page on screen but gets no line in
    «Istoric». Page names are unchanged (`home`, `t:<id>`, `t:<id>#<section>`) plus `p:<part>`;
    section ids in the document are `t-<id>--<section>`. Only a topic the login may not read still
    replaces the document with a message page;
  - **«Imprimă»** prints that same document from the window (`WebBrowser.ShowPrintDialog`): its
    `@media print` rules are the paper look, and the sections outside the chosen scope get the
    class `noprint`. **«Exportă»** saves an HTML file that opens in the browser (printable to PDF
    there). Both open a menu of scopes (`HelpScope`): everything / the topic on screen / the topic
    with what is under it / its parent chapter. Printing everything, or more than
    `HelpScope.LargeTopicCount` topics, carries «(!)» and is asked about first;
  - **its own dialogs go inside `Using OwnDialog()`** (messages, «Save as», print): see «always on
    top» below -- without it the window can close under its own dialog and K-BOT freezes.

  Slice 0000-23:
  - **history per run**: Back / Forward and «Istoric» (every page seen, newest first, 25 at most)
    live in `HelpService.History` (`HelpHistory`), not in the window, so they survive closing and
    reopening it; nothing goes to disk;
  - **text size**: `AppSettings.HelpTextPercent` (80…200 %, on top of the K-BOT text size), used by
    `HelpHtml.PageHead`; the two magnifier buttons change the page on screen in place (the text
    that was at the top of the view is brought back there);
  - **always on top** (`TopMost`), and **closes itself when another window's modal dialog disables
    it** (a disabled top-most window would hide that dialog): `WM_ENABLE(false)` starts a 150 ms
    check (`HelpForm.TmrModal_Tick`); it stays open when the dialog is its own (opened inside
    `OwnDialog()`, or found by owner chain, `HelpWindowNative.OwnsEnabledWindow`) or when it is
    minimized (a tour or a capture moved it aside via `HelpService.StepAside`). Slice 0000-32: a
    check that finds NO dialog on screen yet (`HelpWindowNative.AnyEnabledWindow`) looks again
    instead of closing -- a dialog that is slow to appear is not somebody else's dialog.
- **Guided tours**: a coloured ring around a control plus a bubble with Înapoi / Înainte / Închide.
  Slice 0000-23: the bubble is a **callout** (a `Region` with a triangle whose point touches the
  ring), and a step can name one **part** of a tree / grid / caption bar / nav list (`part:`,
  `IKBotHelpParts` in `KBot.Theming\KBotHelp.vb`, implemented in `KBot.Controls\*\*.HelpParts.vb`);
  a part not on screen is skipped -- unless it is only HIDDEN by state, data or a setting: then (slice
  0000-30) it is shown for the step, with a note, and its text says how to get it for real
  (`IKBotHelpReveal`, `HelpContent/README.md`). Names and rules: `HelpContent/README.md`, «Guided tours».
- **The initial tour** (slice 0097-02): the main window's tour (`HelpService.InitialTourId` =
  `tur-fereastra`) also starts by itself, once `KbotForm` has loaded its data
  (`HelpService.StartInitialTour`), at every start while `AppSettings.ShowInitialTour` is on. Its
  bubble carries the box «Nu mai arăta turul inițial». The setting goes off
  (`HelpService.InitialTourSeen`) when that tour ends past its last step -- however it was started
  -- or is closed with the box ticked; Setări › Aplicație «Arată turul ferestrei principale la
  pornirea K-BOT» turns it back on. A director never gets it (the tour's part is not visible).
  Renaming or removing `tur-fereastra` means changing `InitialTourId` too.
- **The initial tutorial** (slice 000T-07): the short tutorial `tutoriale-intro` (a message, the «?» button, the «Tutoriale» list of
  the popup) starts by itself after the initial tour is over or not owed (`HelpService.StartInitialTutorial`, called from the
  end of `StartInitialTour`), while `AppSettings.ShowInitialTutorial` is on. It is a **mandatory** tutorial (`mandatory: yes`, slice 000T-08: it cannot be closed before its last step; a press
  outside it says so); the setting goes off (`HelpService.InitialTutorialSeen`) when it ends past its last step; Setări › Aplicație ›
  Generale turns it back on. The popup's «Tutoriale» header row has `Key = tutorials` so the
  tutorial can ring that block (`KBotHelpList` part `tutorials`). Renaming `tutoriale-intro` means changing `InitialTutorialId` too.
- **Capture blur** (slice 0000-23, operator tool, NOT described in the help): the capture tool
  blurs, on the frozen screen and before the operator chooses, the TEXT (never a whole bar or
  menu) of the login's user and unit, the other units' names, anything «RO» + digits, 13 digits in
  a row (CNP), e-mail addresses, Romanian phone numbers, and a fiscal code without «RO» but only in
  a field / column whose name or header says «cod fiscal», «CUI» or «CIF» (`HelpCaptureRedaction`).
  Ordinary controls are read through `Text` (context = the control's `Name`); controls that paint
  their text report it through `IKBotCaptureRedaction` (tree, grid, caption bar, `CustomPopup`,
  `KBotMenuWindow`), with the column's key + header as context. Adobe, the FOREXE page and pictures loaded with «Încarcă» are
  not read, so not blurred. A new sensitive category is added only with the operator's OK.
- **Three parts**, and who sees them:

  | Part | `part:` | Who sees it |
  |------|---------|-------------|
  | 1 «Contabil» | `contabil` | every non-director user |
  | 2 «Opțiuni avansate» | `avansat` | non-director users, only while the advanced options are on |
  | 3 «Director» | `director` | only a login with role `Director` (and it sees ONLY this part) |

- **Screenshots** are taken with the capture tool (below), by the operator or, when the operator asks
  and has started K-BOT and logged in, by Claude driving that same tool (so the capture blur still
  applies; never a raw screen grab). Classic theme only for now; no dark variants yet.

## 2. Where everything is

| What | Where |
|------|-------|
| Topics (Markdown, one file = one topic) | `src/KBot.App/HelpContent/<part>/**/*.md` |
| Guided tours | `src/KBot.App/HelpContent/tours/*.md` |
| Pictures | `src/KBot.App/HelpContent/img/<capture-id>.png` |
| Authoring syntax (headers, captures, source tags, tours) | `src/KBot.App/HelpContent/README.md` |
| Static checker | `tools/HelpCheck/Check-Help.ps1` |
| Engine | `src/KBot.App/Help/` (below) |
| Seam used by the controls | `src/KBot.Theming/KBotHelp.vb` (`IKBotHelpProvider`: `ShowHelp` = F1, `ShowHelpMenu` = «?») |
| «?» on caption bars | `src/KBot.Controls/CaptionBar/KBotCaptionBar.HelpButton.vb` |
| The popup, the search panel, the list, the stars (slice 0000-20/21) | `src/KBot.Controls/Popup/KBotHelp*.vb` — doc `KBotHelpPopup.md` |
| The help's version (watermark date, sent with each question) | `src/KBot.App/HelpContent/help-version.txt` |
| Question log on the server (slice 0000-21) | table `AVACONT_COMUN.FX_AjutorIntrebari` (`sql/0000_21_fx_ajutor_intrebari.sql`), route `PYTHON/routes/help_feedback.py` (`POST /api/help/feedback`) |
| Capture list (`HelpCaptureForm`): red rows = pictures older than the tag's `redo:` moment (slice 0000-34) | `src/KBot.App/Help/HelpCaptureForm.vb`, `HelpCapture.vb` |
| Capture mode switch | `AppSettings.HelpCaptureMode`; Setări › Aplicație «Mod capturi pentru ajutor» (visible only with advanced options) |
| Capture menu + navigation for captures/tours | `src/KBot.App/KbotForm.HelpCapture.vb` |

The build copies `HelpContent\**` to `<AppDir>\Help\` (`KBot.App.vbproj`, `None Include ... Link=Help\...`).
The updater writes only the files in its package, so pictures shot on a client PC in
`C:\KBOT\Help\img\` survive updates.

### Engine files (`src/KBot.App/Help/`)

| File | Job |
|------|-----|
| `HelpService.vb` | installs the provider + the F1 key filter; who sees which part (`VisibleParts`); F1 → topic; `Navigate` for captures and tours; `Export` (the manual, or the topics of a `HelpScope`) |
| `HelpLibrary.vb` | loads and checks the topic files; tree, `Search` (→ `HelpSearch`), `FindByScreen`, `HelpVersion` |
| `HelpSearch.vb` | 0000-18: `HelpHit`, the section index built at load, stop words, stems, scoring, snippets, section anchors |
| `HelpSearchSession.vb` | 0000-20/21: what one search panel searches in (popup / window): hits → rows, row actions, the question being asked |
| `HelpPopupTours.vb` | 0000-20/0000-27: which tours the popup offers -- only those of the window whose «?» was pressed (itself and its views, never another window) |
| `HelpQuestionLog.vb` | 0000-21: `HelpQuestion` + the local waiting list and its batched sending |
| `HelpTopic.vb` | `HelpPart` enum + `HelpTopic` |
| `HelpHtml.vb` | Markdown → HTML (Markdig): the window's document (`Book`, 0000-32), the manual, an excerpt (`Excerpt`); screen and paper CSS; capture tags → pictures / «Imagine lipsă» |
| `HelpForm.vb` | the help window: scrolling to a place of the book, the tree that follows the scroll, print, export |
| `HelpScope.vb` | 0000-32: how much a print / an export takes (everything, a topic, a topic with what is under it, its parent chapter) and the menu row for it |
| `HelpHistory.vb` | 0000-23: Back / Forward / «Istoric» for the whole run (owned by `HelpService`) |
| `HelpWindowNative.vb` | 0000-23: whose modal dialog disabled the help window |
| `HelpCapture*.vb`, `IHelpCaptureNavigator.vb` | capture tags, the capture list («Fă poza» / «Încarcă»), the screen freeze + rectangle, saving; `HelpCaptureRedaction.vb` (0000-23) = what is blurred and the blur |
| `HelpTour*.vb` | tour parsing, runner (parts, skipping, the demo of hover-only buttons), ring, callout bubble |

## 3. How F1 finds a topic

F1 walks from the focused control up through its parents. At each step it offers
`TypeName.controlName` and `TypeName`; the first key that a VISIBLE topic lists in `screens:` wins
(parts in order). Nothing found → the help start page. The Debug build shows the keys it tried
in the help window's bar — copy the right one from there.

Inner pages (a tab page inside a window) need no key of their own when their window has one.

The «?» popup's «Pe ecranul acesta» row uses the same keys (from the focused control of the
window whose «?» was pressed). Its tours: a tour is offered on a visible, usable window where one
of its screens is visible — the tour's `screens:` if it has one, else its topic's `screens:`
(`HelpContent/README.md`, «Guided tours»).

## 4. PROCEDURE — bringing the help up to date after a change

Use this when the operator says «update the help» (or when your own task changed something the
operator sees and you are asked to cover it).

1. **Find what changed since the help was last brought up to date.** The watermark is in
   `state/KBOT_STATUS_0000-0009.md` («Ajutorul e la zi până la»). Read the `KBOT_STATUS.md` index
   rows after it, and the Open threads line «Ajutor de actualizat» (feature slices leave notes
   there). Open only those slices' worklogs.
2. **Map each change to topics.** Run
   `powershell -NoProfile -ExecutionPolicy Bypass -File tools\HelpCheck\Check-Help.ps1 -Map -Coverage`
   for the topic tree, then grep `HelpContent` for the window's type name (in `screens:`) and for
   the old captions / messages the change touched.
3. **Get the facts from the code, not from the plan.** Captions and tooltips: the window's
   `.Designer.vb` (`.Text`, `SetToolTipHeader/Text`, `HeaderText`). Messages: `KBotMessage.Show`
   strings. States: `DdfRevisionStates.Label`. Quote on-screen text exactly, in «».
4. **Edit the topics:**
   - **every section you write or change gets the slice that decided it** in its hidden
     `<!-- slice: ... -->` tag (under the `##` heading; the header tag for the text before the
     first `##`; tour steps too). Syntax: `HelpContent/README.md` «Source tags»;
   - changed behaviour → rewrite the paragraph; renamed button → rename it everywhere
     (`grep` the old caption);
   - a new window or view → a new topic (or a section in the nearest one) AND its type name in
     `screens:`; `-Coverage` must print «(none)»; if the topic explains that one screen and
     K-BOT can go there (`goto:` values), give it `open:` too (slice 0000-19);
   - a new word the operator would type for something (a synonym, an old Access name) → the
     topic's `keywords:`; endings and diacritics need no entry (slice 0000-18);
   - a renamed / removed control → fix `screens:` and tour `target:` (the checker catches it);
   - a new header-menu item, view or Setări page used by `goto:` → also add it to the lists in
     `HelpContent/README.md` (and to `KbotForm.HelpCapture.vb` if it is a new KIND of target).
5. **Pictures.**
   - A new screen worth showing → a new capture tag where the picture belongs (unique id).
   - A screen whose look changed → keep the tag and add `| redo: <yyyy-MM-dd HH:mm> | why: <slice>: <what
     changed>` to it (slice 0000-34): the capture list then paints the row RED until the picture is
     taken again. Also list its id in the worklog under «Capturi de refăcut».
   - Never delete or overwrite files in `img/` — they are the operator's.
   - A window the capture list cannot reach (director's window, dialogs that appear only in a real
     situation) → no `goto:`, and a `prepare:` that says when / where to shoot it.
6. **Tours:** if a step's target moved or its text is now wrong, fix the step. A new window or
   view that deserves a tour gets one; if its topic's `screens:` do not name the window the tour
   runs in, give the tour its own `screens:` so the «?» popup offers it there (slice 0000-20).
7. **Check + build:** `Check-Help.ps1 -Coverage` → «No errors.» and coverage «(none)», then
   `dotnet build src\KBot.App\KBot.App.vbproj` (0 warnings, 0 errors). Don't run the app to look
   at the help unless asked.
8. **Record it** as the next sub-slice `0000-NN` (never a new slice number):
   worklog `docs/worklog/SLICE-0000-NN-<slug>.md` (which topics, which capture ids are new / to
   re-shoot, what the operator must read), a row in `state/KBOT_STATUS_0000-0009.md`, move the
   watermark AND write its date (`yyyy-MM-dd`) in `src/KBot.App/HelpContent/help-version.txt`
   (it goes with every question, slice 0000-21), clear the «Ajutor de actualizat» notes you
   handled, and update the index line in `KBOT_STATUS.md` («0000-01…NN GATA»).

### What a feature slice does (when it is NOT a help task)

**Rule (operator, 30.09.2026): ANY change to the visual experience must also be recorded in the
help and must reference the slice number.** So a feature slice that changes what the operator
sees updates the help itself, as a `0000-NN`, with its own slice number in the source tags.
Only when that cannot be done in the same task: in its worklog under «left unverified or deferred», and in the 0000 Open threads line «Ajutor de
actualizat», name the topic ids (and capture ids) the change makes stale. One line is enough, e.g.
`0098: contabil.vederi.plati (noua coloana «Cont»), captura plati de refacut`.

## 5. Writing rules

- **Romanian, «tu» form, plain words**, short sentences. Write for an accountant, not a developer.
- On-screen text exactly as the operator reads it, in «» or **bold**.
- **Anything that is not always on screen (a view, button, row or page that waits for data, a
  connection or a setting) is described WITH its condition, in the topic and in the tour step**:
  what makes it appear, and where the operator does it (slice 0000-30). Never «gri» for a thing the
  app really hides; check `SetItemVisible` / `.Visible =` in the code.
- Say what to do and what happens; one fact once — link to the topic that owns it
  (`[text](topic:id)`) instead of repeating it.
- Headers ASCII (rule 0); only `title` and the body carry diacritics.
- **Left out on purpose** (operator's decisions):
  - how the custom controls work inside, the browser's internal logic;
  - **how K-BOT works inside** (operator, 30.09.2026: «the help is meant for users only»): keys
    K-BOT presses for the user (Ctrl+H, Ctrl+2...), how it places or watches the Adobe window,
    log lines, timers, internal checks. Describe only what the user sees and does;
  - **internal-only features and windows**: anything the operator did not ask for and does not
    know exists (a check window, a helper dialog, a debugging aid) stays out of the help, its
    pictures and its release notes until the operator says it is part of the product (operator,
    02.10.2026, the «Istoric angajament» window of 0101-02, removed from the help);
  - the capture mode itself (it is a tool for the operator, not for clients);
  - the clicks inside the FOREXE page — the help describes what K-BOT does, not FOREXE's own pages;
  - the ORD's next step after signing (turned into a recepție and uploaded to the FOREXE CAB
    server): K-BOT does not do it yet and the flow is not settled. The ORD signing (validation,
    the five signatures, the minimum set) IS described, from `docs/FUNDAMENT_Ordonantare_CAB.md`;
    write the sending section only when it exists;
  - anything Debug-only (the launcher, the dev harness).
- A fact you could not confirm in the code goes in the worklog under «de citit de operator», not
  into the help as if it were certain.

## 6. Engine changes (rare)

- New `goto:` kind → `KbotForm.HelpCapture.vb` `NavigateForCapture`, plus `HelpService.Navigate`
  and the `$GotoPrefix` pattern in `Check-Help.ps1`.
- New part → `HelpPart` enum, `HelpLibrary.ParsePart`, `HelpService.VisibleParts` / `ManualParts`,
  `$Parts` in `Check-Help.ps1`.
- New header key → `HelpLibrary` parser, README, `$HeaderKeys` in `Check-Help.ps1` (the last one
  added: `open:`, 0000-19, validated against `$GotoPrefix` / `HelpLibrary.GotoPattern`). New tour
  key → `HelpTour.Parse`, README, `$TourKeys` (last: `screens:`, 0000-20). A new tour STEP key
  (last: `part:`, 0000-23) → `HelpTour.Parse` preamble, README, and its check in `Check-Help.ps1`.
- New help part on a control (0000-23) → the control's `HelpPartBounds` / `SetHelpPartDemo`
  (`*.HelpParts.vb`), the README table and `$PartsByType` in `Check-Help.ps1`. A new control
  family that tours should step through → implement `IKBotHelpParts` the same way.
- A control that paints sensitive text itself → implement `IKBotCaptureRedaction` (text rectangles
  only), or the capture blur cannot see it.
- Every engine change follows the house rules in `CLAUDE.md` and is recorded as a `0000-NN` too.

## 7. Search and the question log (maintainer side)

**Search** (`HelpSearch.vb`, 0000-18). At load every topic is split into sections (the text
before the first `## `, then each `## `), folded (lower case, no diacritics) and reduced to stems
(a common Romanian ending dropped, then the first 5 letters; the first 4 letters count half).
Filler words are in ONE list, `HelpSearch.StopWords`. A section's score = words matched × 100 +
weight (title 10, heading 8, keywords 6, text 2; half for a 4-letter match); with 3+ words a
section must hold half of them; at most 3 hits per topic. Section anchors are the heading's
folded words joined by dashes; `HelpHtml` gives the page's headings the same ids. Tune the
weights only from the question log, not by feel.

**Question log** (0000-21). What a row holds: see the header of
`sql/0000_21_fx_ajutor_intrebari.sql`. Never add anything about the user, the unit, the PC or the
session — not to the row, not to a log line (the route has its own logger for that reason, see
the module docstring of `PYTHON/routes/help_feedback.py`). On the client, questions wait as one
JSON file each in `%APPDATA%\AVACONT\KBot\HelpOutbox\` and are sent in batches (every 3 minutes,
at 10 waiting, at start, at exit). A question is written when a hit is used, when it is rated,
and when the box is emptied or the popup / window closes; editing it after a click, a rating or a
3-second pause starts a new one. The queries that read the log (no click, rated ≤ 2, most
repeated, the «step 4» gate) are in `docs/worklog/SLICE-0000-21-intrebari-si-note.md`.

The help text says, in one sentence, that questions and ratings are sent without the name to
improve the help, and asks for no personal data in the box (`contabil.ajutor`, `director`). Keep
that sentence if the text is rewritten; do not describe the waiting list or the server.
