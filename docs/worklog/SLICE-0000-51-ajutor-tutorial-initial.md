# SLICE-0000-51 — help for the tutorial of the start (000T-07)

Procedure `docs/HELP_SYSTEM.md` §4, for the feature slice 000T-07 (the operator sees a new tutorial at start and a new switch
in Setări).

## What changed and why

- `contabil.ajutor`: new section «Tutorialul de început» (what starts, after the tour, the two ways to stop it, how to get it
  back); keywords «tutorial initial», «tutorialul de inceput», «ce sunt tutorialele», «nu mai arata tutorialul». Tag
  `000T-07, 0000-51`.
- `contabil.setari`: new bullet «Arată tutorialul de început la pornirea K-BOT» in «Pagina «Aplicație»: pornirea K-BOT»
  (default on, when it switches itself off); keywords; section tag now `0097-02, 000T-07, 0000-51`.
- The two tutorials themselves are not topics (they live in `tutorials/`, tagged `000T-07`); the popup's «Tutoriale» group
  lists them.
- `help-version.txt` = `2026-10-05`; watermark moved in `state/KBOT_STATUS_0000-0009.md`.

## Capturi de refăcut

`setari` (a new switch on «Aplicație»): its tag got `redo: 2026-10-05 08:53 | why: 000T-07: ...` (it was already red from
0100-02), so the capture list paints it red until the picture is taken again. `ajutor-meniu` (the «?» popup lists two tutorials now; 0000-50
only noted it as to be redone, with no marker) got the same `redo:` marker.

## Test results

`Check-Help.ps1 -Coverage`: «No errors.» No topic left stale by this slice.

## Left unverified

Nothing seen on screen; the text is written from the designer files and the code (`SetariAplicatieView.Designer.vb`,
`HelpService.vb`), not from the running app.
