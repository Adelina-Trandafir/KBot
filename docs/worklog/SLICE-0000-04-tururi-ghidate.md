# SLICE-0000-04 — Guided tours (+ the «Sursă / sector» tooltip)

Operator, 30.09.2026: «fix the tooltip, then go ahead with 0000-04».

## What changed and why

- **Tooltip** (`KbotForm.Designer.vb`, `cboSs`): header «Sursă / sector», text «Sursa și sectorul
  (de exemplu 02A) din anul ales. / Schimbarea lor reîncarcă arborele și toate ecranele. / Ultima
  alegere se ține minte pentru data viitoare.» (was «Subperioada (SS)...», wrong — found in 0000-03).
- **Tour files**: `HelpContent/tours/*.md` — header (`id`, `title`, `part`, `topic`) + one `## ` per
  step with optional `target:` / `goto:`. Parsed by `HelpTour`; loaded by `HelpLibrary` (the topic
  loader skips the folder; bad files / duplicate ids / missing topic are logged). Format in README.
- **Engine**:
  - `HelpTourRunner` — per step: `HelpService.Navigate(goto)` (moved out of `HelpCaptureForm`,
    now shared by captures and tours), 400 ms for a lazily built view, finds the target among the
    open windows (`TypeName` / `TypeName.controlName`, first visible), rings it, places the bubble.
    One tour at a time. A missing target → centred bubble with a note; the tour goes on.
  - `HelpTourFrame` — top-most, click-through, never-activating hollow rectangle in the accent colour.
  - `HelpTourBubble` (Designer) — «Pasul n din m», title, text, note, «◄ Înapoi» / «Înainte ►»
    («Gata» on the last) / «Închide»; → / Enter / ← / Esc; sized to its text; placed right, left,
    below or above the target, clamped to the working area.
- **Entry points**: «▶ Tur ghidat: ...» at the top of the tour's topic page and a «Tururi ghidate»
  list on the help start page (`tour:` link scheme). The help window minimizes during a tour and
  comes back after. Tours are not in the exported manual.
- **Four tours** (Part 1): `tur-fereastra` (9 steps), `tur-forexe` (9), `tur-rezervari` (4),
  `tur-ddf` (6).

## Files touched

- New: `Help/HelpTour.vb`, `HelpTourRunner.vb`, `HelpTourFrame.vb`, `HelpTourBubble(.Designer).vb`,
  `HelpContent/tours/{tur-fereastra,tur-forexe,tur-rezervari,tur-ddf}.md`.
- Edited: `HelpLibrary.vb` (tours; `ParsePart` Friend), `HelpService.vb` (`Navigate`, `StartTour`),
  `HelpCaptureForm.vb` (uses `Navigate`), `HelpHtml.vb` (`TourScheme`, links, CSS), `HelpForm.vb`
  (`tour:` links), `HelpContent/README.md`, `KbotForm.Designer.vb` (tooltip).

## Test results

- Build KBot.App: 0 warnings, 0 errors; 4 tour files copied to `bin\...\Help\tours\`.
- Static check: all 21 distinct `target:` values name a real type and a control declared in its
  designer. Not run on screen (operator's rule).

## Left unverified or deferred

- On screen: ring placement at other DPIs / multi-monitor, bubble height with long texts, focus
  returning to K-BOT after «Gata».
- Painted parts (tree footer icons, caption-bar buttons, node icons) cannot be ringed on their own;
  the steps ring the whole control and say where to look.
- Tours for Part 2 and Part 3 come with those parts (0000-05 / 0000-06).
