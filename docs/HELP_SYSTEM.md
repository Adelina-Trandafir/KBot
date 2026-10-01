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
- The **search** (slice 0000-18) takes a typed question: filler words, diacritics and word
  endings do not matter, not every word must match, and it finds SECTIONS (a hit opens the page
  at its `## ` heading). A hit whose topic has `open:` carries «Deschide «...»» (0000-19); one
  whose topic has a tour carries «Tur ghidat». The popup and the help window use the same search
  control.
- **Questions and ratings** (slice 0000-21): every question typed there, with the hits shown,
  the hit used and a 1-5 star rating, is sent to the server WITHOUT anything about who or where
  (see §7).
- The **help window**: contents tree, the search box above it (its results take the tree's place
  while it has text), Back / Forward, «Istoric ▾», «A−» / «A+», guided tours, «Exportă
  manualul...» (one HTML file, printable to PDF). Slice 0000-23:
  - **history per run**: Back / Forward and «Istoric» (every page seen, newest first, 25 at most)
    live in `HelpService.History` (`HelpHistory`), not in the window, so they survive closing and
    reopening it; nothing goes to disk;
  - **text size**: `AppSettings.HelpTextPercent` (80…200 %, on top of the K-BOT text size), used by
    `HelpHtml.PageHead`; «A−»/«A+» change the page on screen in place (scroll kept);
  - **always on top** (`TopMost`), and **closes itself when another window's modal dialog disables
    it** (a disabled top-most window would hide that dialog): `WM_ENABLE(false)` starts a 150 ms
    check (`HelpForm.TmrModal_Tick`); it stays open when the dialog is its own (owner chain,
    `HelpWindowNative.OwnsEnabledWindow`: the manual's «Save as», the capture tool) or when it is
    minimized (a tour or a capture moved it aside via `HelpService.StepAside`).
- **Guided tours**: a coloured ring around a control plus a bubble with Înapoi / Înainte / Închide.
  Slice 0000-23: the bubble is a **callout** (a `Region` with a triangle whose point touches the
  ring), and a step can name one **part** of a tree / grid / caption bar / nav list (`part:`,
  `IKBotHelpParts` in `KBot.Theming\KBotHelp.vb`, implemented in `KBot.Controls\*\*.HelpParts.vb`);
  a part not on screen is skipped. Names and rules: `HelpContent/README.md`, «Guided tours».
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

- **Screenshots** are taken by the operator with the capture tool (below), never by Claude.

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
| Capture mode switch | `AppSettings.HelpCaptureMode`; Setări › Aplicație «Mod capturi pentru ajutor» (visible only with advanced options) |
| Capture menu + navigation for captures/tours | `src/KBot.App/KbotForm.HelpCapture.vb` |

The build copies `HelpContent\**` to `<AppDir>\Help\` (`KBot.App.vbproj`, `None Include ... Link=Help\...`).
The updater writes only the files in its package, so pictures shot on a client PC in
`C:\KBOT\Help\img\` survive updates.

### Engine files (`src/KBot.App/Help/`)

| File | Job |
|------|-----|
| `HelpService.vb` | installs the provider + the F1 key filter; who sees which part (`VisibleParts`); F1 → topic; `Navigate` for captures and tours; manual export |
| `HelpLibrary.vb` | loads and checks the topic files; tree, `Search` (→ `HelpSearch`), `FindByScreen`, `HelpVersion` |
| `HelpSearch.vb` | 0000-18: `HelpHit`, the section index built at load, stop words, stems, scoring, snippets, section anchors |
| `HelpSearchSession.vb` | 0000-20/21: what one search panel searches in (popup / window): hits → rows, row actions, the question being asked |
| `HelpPopupTours.vb` | 0000-20: which tours the popup offers, per visible window, top first |
| `HelpQuestionLog.vb` | 0000-21: `HelpQuestion` + the local waiting list and its batched sending |
| `HelpTopic.vb` | `HelpPart` enum + `HelpTopic` |
| `HelpHtml.vb` | Markdown → HTML (Markdig), pages, the manual; capture tags → pictures / «Imagine lipsă» |
| `HelpForm.vb` | the help window |
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
   - A screen whose look changed → keep the tag; list its id in the worklog under «Capturi de
     refăcut» so the operator re-shoots it («Refă» in the capture list).
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
- Say what to do and what happens; one fact once — link to the topic that owns it
  (`[text](topic:id)`) instead of repeating it.
- Headers ASCII (rule 0); only `title` and the body carry diacritics.
- **Left out on purpose** (operator's decisions):
  - how the custom controls work inside, the browser's internal logic;
  - **how K-BOT works inside** (operator, 30.09.2026: «the help is meant for users only»): keys
    K-BOT presses for the user (Ctrl+H, Ctrl+2...), how it places or watches the Adobe window,
    log lines, timers, internal checks. Describe only what the user sees and does;
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
