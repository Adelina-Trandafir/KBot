# K-BOT help topics (slice 0000)

Every `*.md` file here (except this README) is one help topic. They ship as `<AppDir>\Help\`
and feed both the help window (F1 / «?») and the exported manual. Code: `src/KBot.App/Help/`.

**Maintainer guide (read first): `docs/HELP_SYSTEM.md`** — what the system is, where each piece is,
the step-by-step procedure for updating the help after a change, and the writing rules. Check the
content with `tools\HelpCheck\Check-Help.ps1 -Coverage -Map`.

## Header (required block at the top)

```
---
id: contabil.ddf.trimitere        # unique, dotted, ASCII; links use it
title: Trimiterea în FOREXE       # Romanian, shown to the operator
part: contabil                    # contabil | avansat | director
order: 30                         # position among siblings, lower first
parent: contabil.ddf              # parent topic id; empty = directly under the part
screens: DdfView, KbotForm.btnSinc  # what F1 / «?» on these opens (see below)
keywords: fundamentare, revizie   # extra search words
open: view:ddf                    # optional: the screen this topic explains (slice 0000-19)
---
```

Unknown keys are an error: the file is skipped and the reason goes to `harness_errors.log`.
Header keys and values are ASCII (rule 0); only `title` and the body are Romanian.

## screens

F1 walks from the focused control up through its parents. Each step offers
`TypeName.controlName` (a named control inside a form / user control) or `TypeName`
(the form / user control itself); the first key that a topic lists wins. Debug builds show the
keys that were tried in the help window's bar, so you can copy the right value.

## open (slice 0000-19)

The screen the topic explains, with the same values as a capture's `goto:` (below):
`view:<key>`, `menu:<key>`, `setari:<page>`. A search hit on the topic (the «?» popup, the help
window) then carries a «Deschide «...»» button that takes K-BOT there; the button's caption is
the view / menu row as the main window shows it («Setări» for a settings page). A view that is
off for the selected angajament shows the usual «nu e disponibilă» message. Give `open:` only to
topics that describe ONE real screen; a chapter page or a concept page gets none. The checker
validates the value against the `goto:` pattern.

## keywords

Search words the text does not contain: synonyms, the old Access names, what an accountant
would type («ordonantare» for ORD, «semnatura» for signing). The search already ignores
diacritics, filler words and word endings (slice 0000-18), so do not list plural / singular
forms of a word that is already in the text.

## Body

Markdown: headings, lists, **bold**, tables (`| a | b |`), `> quote` (drawn as a warning box).
- Link to a topic: `[text](topic:contabil.ddf)`. A missing target is shown in red.
- Picture: `![caption](img/ddf-trimitere-1.png)` (PNG/JPG, under `img/`). A picture not yet on
  disk shows a «Imagine lipsă» box with its name.

## Source tags: which slice decided the text (slice 0000-13)

Every explanation names the slice(s) whose decision it describes, in a line the reader never sees:

```
<!-- slice: 0072, 0097 -->
```

- right after the header block of a topic (it covers the text before the first `##`);
- right under **every** `## ` heading, in topics and in tour steps (`###`: optional, else the
  `##` above covers it);
- ids: a slice of the `KBOT_STATUS.md` index or with a worklog (`0048`, `0048-04`), a help
  sub-slice (`0000-13`) when the explanation was decided in help work, or `fara-felie` (work
  recorded only in `KBOT_STATUS_SLICELESS.md`);
- when you change a section, put the slice of the change in its tag (keep the older ids that
  still hold).

The tags are removed when a file is read (`HelpLibrary.StripSourceTags`): no page, tour bubble,
search or exported manual carries them. `Check-Help.ps1` fails on a missing or unknown tag.

## Screenshots: capture tags (slice 0000-02)

A picture the help needs is written as ONE line, where the picture should appear:

```
<!-- capture: ddf-revizii-1 | caption: Lista reviziilor DDF | goto: view:ddf | prepare: Selectați un angajament cu cel puțin două revizii. -->
```

- `capture:` the id = file name `img/<id>.png` (lower-case ASCII, digits, dashes; unique).
- `caption:` Romanian, shown under the picture / in the «Imagine lipsă» box and in the list.
- `goto:` where K-BOT goes before the shot (optional):
  `view:<navViews key>` (sumar, istoric, rezervari, receptii, plati, extrase, browser, ddf, ord,
  notecab) · `menu:<header menu key>` (angajament_nou, extrase, clasificatii, parteneri,
  operatiuni_necorelate; `jurnal` and `setari` exist too, but open the settings window -- use
  `setari:<page>` instead; `angajament_forexe` exists too but starts the robot -- do not use it
  for a capture, a tour step or `open:`) · `setari:<page>` (info, aplicatie, forexe, multithread, pagina, extrase, tema,
  autentificare, foldere, jurnal) · `help` (this topic) / `help:<topic id>`.
- `prepare:` Romanian, what the operator sets up by hand before «Capturează» (optional).
- `redo:` + `why:` (slice 0000-34, optional, always together): the picture is OUT OF DATE because a later
  change altered that screen. `redo: 2026-10-01 22:48` is the moment of the change (`yyyy-MM-dd HH:mm`),
  `why: 0100: ...` says what changed (Romanian, starts with the feature slice). While the saved picture
  is OLDER than that moment, the capture list paints the row in red (state «de refăcut», the reason
  at the top of the preparation box); taking the picture again, or loading one, clears it by itself.
  A picture that does not exist yet is just «lipsă». Leave the tag in place afterwards; it does no harm.

The operator takes them from «Meniu › Capturi pentru ajutor» (capture mode: Setări › Aplicație,
visible only with the advanced options). Each «Fă poza» goes to `goto`, shows `prepare` in a
floating bar, freezes the screen and saves the marked area to `<AppDir>\Help\img\` and, on a
repository build, to `src\KBot.App\HelpContent\img\` too. The capture mode itself is NOT
described in the help.

## Guided tours (slice 0000-04)

One file per tour in `tours/` (NOT a topic: the topic loader skips this folder):

```
---
id: tur-fereastra            # ASCII, unique
title: Fereastra principală  # Romanian, the bubble's caption and the link text
part: contabil               # who sees it (same parts as topics)
topic: contabil.fereastra    # the topic that shows «▶ Tur ghidat: ...»; empty = start page only
screens: KbotForm            # optional (slice 0000-20): where the «?» popup offers it; empty = the topic's screens
---
## Butonul MENIU             # one '## ' per step: the bubble's heading
target: KbotForm.btnMeniu    # optional: TypeName or TypeName.controlName to ring
part: footer.right           # optional (slice 0000-23): one painted piece of the target, see below
reveal: menu                 # optional (slice 0000-31): opens the MENIU menu first (only on KbotForm.btnMeniu)
goto: view:sumar             # optional: same values as a capture's goto
Text of the bubble. Plain text; **bold** marks are dropped; a blank line starts a paragraph,
lines starting with '- ' become bullets.
```

`target` uses the same names as `screens:` (the Debug help bar shows them on F1). A target that
is not on screen does not stop the tour: the bubble is centred and says so. Every tour is listed
on the help start page under «Tururi ghidate».

**The bubble is a callout (slice 0000-23):** its point touches the ring around the target (or the
part). It sits right of the target, else left, below, above; a target too big for any side (a
whole view) gets the bubble inside it, pointing up at its top edge.

**Parts (slice 0000-23).** Painted buttons are not controls, so `part:` names one inside the
`target` control. The tour rings just that button and points at it. The names, per control:

| Control | `part:` values |
|---------|----------------|
| tree (`AdvancedTreeControl`) | `header`, `header.search` (lupa), `header.right`, `columns`, `node.icon` (the button at the end of a row), `footer`, `footer.left`, `footer.right`, `footer.collapse` |
| grid (`KBotDataView`) | `header` (column titles), `header.filter` (the first column menu icon on screen), `rows`, `footer` (TOTAL), `footer.left`, `footer.right`, `footer.collapse` |
| caption bar (`KBotCaptionBar`) | `icon`, `title`, `unit`, `options`, `theme`, `help`, `minimize`, `maximize`, `close` |
| nav list (`KBotNavList`) | `item:<Key>` (the item's `Key` in the designer), `collapse` |

- `node.icon` that normally appears only under the mouse (the main list's refresh) is **shown for
  the step** on the selected row (else the first row on screen that has one), then hidden again.
- **Hidden by state is SHOWN for the step (slice 0000-30).** What the app hides because of data, a
  connection or a setting (a nav button the angajament has no data for, «Browser FOREXE» while not
  connected, a footer button of the FOREXE band, the «+» of a tree row, the footer-left menu icon,
  a view's own button) is shown while its step is on and put back when the step changes; the bubble
  adds the note «Il vezi acum doar pentru tur...». **The step's text MUST say what makes it appear for
  real** (the data, the connection, the setting with its caption). How it works: the runner
  (`HelpTourRunner`) tries, in order, the part's own bounds; a control whose own `Visible` is False
  under a visible parent (`target: View.control`, no `part:`) -> it is shown; else `IKBotHelpReveal`
  (`KBot.ThemingKBotHelp.vb`), asked of the target and every control above it: `KBotNavList` shows a
  hidden item (`part: item:<Key>`), `AdvancedTreeControl` shows `node.icon` / `footer.left` from the
  pictures the VIEW gives it (`HelpDemoRightIcon`, `HelpDemoFooterLeftIcon`). A new hidden button =
  nothing to do for a plain control (name it as `target:`); for a painted tree icon the view sets the
  picture. The MENIU menu: `reveal: menu` on `target: KbotForm.btnMeniu` (slice 0000-31) opens it with
  its sometimes-hidden rows and holds it open (`KBotPopupGuard`, so the bubble taking the focus does not
  close it); the ring covers button + menu (`IKBotHelpReveal.HelpReveal` returns the area). A view that
  is hidden as a whole (no data) stays unreachable, the step's note says to pick an angajament that has it.
- **A part that is still not on screen after that is skipped** (in the direction the operator is
  going): an empty tree, a hidden page. So a tour may list every button a control can have; write
  only the ones the designer really sets, though.
- The convention: one step for the control as a whole («Lista angajamentelor»), then one step per
  part, titled «Lista › Lupa», «Tabel › TOTALURI»...
- `Check-Help.ps1` checks each `part:` against the table above (the type is read from the
  control's declaration) and each `item:` key against the designer.

**In the «?» popup (slice 0000-20)** a tour is offered on each visible window (not minimized,
not behind a modal dialog) that shows one of its screens: the tour's own `screens:`, else its
topic's `screens:`. «Shows» means the form, or a control / view inside it, is visible — for the
main window that is the selected view. When several windows have tours, the popup puts them in
one folder per window. Give a tour `screens:` only when its topic's screens do not name the
window it starts on (e.g. `tur-avansat`: its topic lists one checkbox, the tour runs in Setări).

## Interactive tutorials (slice 000T)

A tour shows; a **tutorial waits for the user to do it** (the runner never clicks for them): ring on the
target, the rest of the window dimmed, a bubble with the instruction, and the next step comes by itself when
the user did the action. Anything else on the dimmed window (or a key typed outside the allowed places) asks
«Vrei să ieși din tutorial?» (Da ends it, Nu stays on the step). One file per tutorial in `tutorials/` (NOT a
topic, NOT a tour; the loaders skip it). Header keys: `id`, `title`, `part`, `keywords` (words the typed
question may use), `starts` (the window type that must be open), `host-key` (a free ASCII string handed to every
window the tutorial opens: `IKBotTutorialHost.TutorialSupports/TutorialBegin`, so one window can serve several
tutorials). One `## ` per step; its first lines are keys, then the bubble text; every step carries its
`<!-- slice: ... -->` tag like a tour step.

| Step key | Meaning |
|----------|---------|
| `target:` | `TypeName` or `TypeName.controlName` (as a tour) — also the control the wait is attached to |
| `part:` | a painted piece of the target (as a tour; a part hidden by state is NOT revealed, the whole control is used) |
| `anchor:` | a named place only the owning window knows (`IKBotTutorialHost.TutorialAnchor`: the rows that have reservations, the row with the «+») |
| `wait:` | `manual` (default; the «Înainte» button) · `select` (tree row) · `click` (button; for a tree only its row button) · `tab:<key>` (nav item selected) · `opens:<Type>` · `closes` · `changed` · `checked` · `signal:<name>` (the window raises `TutorialSignal`) |
| `when:` | the step applies only if `enabled` / `editable` / `visible` (the target) or `checked:Type.box` / `unchecked:Type.box`; otherwise it is skipped |
| `optional:` `why:` | `yes` adds «Sari peste»; `why:` (required) is shown as «Pas opțional: ...» |
| `merge:` | `yes`: the NEXT step's action also completes this one (select a row and press its «+» are one click) |
| `dim:` | `yes` (default) or `ring` (ring only) |
| `allow:` | extra places the user may use, `Type.control, ...` |

Look-ahead: while a step is on, the user may also do the action of the next step when this one is optional or
merged, going on past optional steps up to the first mandatory one. `Check-Help.ps1` validates the files.
A window the tutorial opens should implement `IKBotTutorialHost` (the checker warns when it does not): it is
told to start / stop and builds nothing itself — the runner makes the ring, veil and bubble for it after it is
shown, so a modal window does not disable them.
