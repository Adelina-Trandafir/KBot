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
  operatiuni_necorelate) · `setari:<page>` (info, aplicatie, forexe, pagina, extrase, tema,
  autentificare, foldere, jurnal) · `help` (this topic) / `help:<topic id>`.
- `prepare:` Romanian, what the operator sets up by hand before «Capturează» (optional).

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
goto: view:sumar             # optional: same values as a capture's goto
Text of the bubble. Plain text; **bold** marks are dropped; a blank line starts a paragraph,
lines starting with '- ' become bullets.
```

`target` uses the same names as `screens:` (the Debug help bar shows them on F1). A target that
is not on screen does not stop the tour: the bubble is centred and says so. Painted parts
(tree footer icons, caption-bar buttons) are not controls: ring their control and say where.
Every tour is listed on the help start page under «Tururi ghidate».

**In the «?» popup (slice 0000-20)** a tour is offered on each visible window (not minimized,
not behind a modal dialog) that shows one of its screens: the tour's own `screens:`, else its
topic's `screens:`. «Shows» means the form, or a control / view inside it, is visible — for the
main window that is the selected view. When several windows have tours, the popup puts them in
one folder per window. Give a tour `screens:` only when its topic's screens do not name the
window it starts on (e.g. `tur-avansat`: its topic lists one checkbox, the tour runs in Setări).
