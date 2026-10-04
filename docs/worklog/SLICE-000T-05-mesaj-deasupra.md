# SLICE-000T-05 — the tutorial's question box opens on top

Corrective on 000T (operator, 04.10.2026): the «Vrei să ieși din tutorial?» box (and the other tutorial boxes) opened
UNDER the step card, so the operator could not read or answer it.

## What changed and why

- Cause: the step card (`HelpTourBubble`) and the ring (`HelpTourFrame`) are top-most windows. While the box is open the
  runner's timer is paused (`_asking`), so they stay up, and an ordinary `MessageBox` (not top-most) opens below them.
- `src/KBot.Theming/KBotMessage.vb`: new `ShowOnTop` (two overloads, with and without owner). Same journal line in
  `mesaje_operator.log` as `Show`, same `DialogResult`; the box is created with the Win32 style `MB_TOPMOST` (0x40000),
  passed through `MessageBoxOptions`. The alternative (a custom box) was not needed: `MB_TOPMOST` keeps the real Windows
  box, its sounds, its keyboard behaviour and the logging path.
- `src/KBot.App/Tutorial/TutorialRunner.vb`: the three boxes of the runner use `ShowOnTop` (the exit question, «Această
  fereastră nu poate fi folosită...», «pornește dintr-o altă fereastră...»). The designer's boxes are not covered by the
  card, so they stay on `Show`.

## Second pass (same slice, same day) — three corrections from the first run on screen

1. **Space in the long-description editor moved the tutorial on.** That step waits for «Înainte» (manual), and the
   bubble's «Înainte» had the keyboard focus: a Space on a focused button is a click. (On the text-box steps the step
   moves on at the first character, so it was never noticed.) Fix, three layers:
   - `HelpTourBubble.NeverActivates` (new; set only by `TutorialRunner`): the bubble is created with `WS_EX_NOACTIVATE`
     and `ShowWithoutActivation`, so it neither takes the focus when shown / re-shown nor when clicked; its buttons still
     work; its own key handling is off. The guided tours keep the old behaviour.
   - `TutorialRunner.KeyFilter` swallows every keyboard message (0x100-0x109) aimed at a control of the bubble and hands
     the focus back to the window; whatever puts the focus there, a key can no longer click a bubble button.
   - After «Sari peste» / «Înainte» / «Mă opresc» the focus goes back to the window the operator works in (`FocusHost`).
2. **«Salvează documentul» was lit during the optional steps.** Cause: the look-ahead (`TutorialFlow.AcceptedFrom`) ran
   past all optional steps to the first mandatory one, and every step in that list gets a hole in the veil. Now an
   optional (or merged) step accepts only the NEXT step; a merged next step passes the permission on. A step further on
   stays dimmed until its turn; «Sari peste» is the way past an optional step. Consequence for the operator: to reach
   «Salvează documentul» from the middle of the optional steps, press «Sari peste» on each (it was possible to click
   «Salvează» straight away before). Help text adapted (see below). Test `AcceptedFrom_OptionalChain...` rewritten to the
   new rule.
3. **Step «Alege încă un partener»: the text was hidden.** Cause: `HelpTourBubble.FitToText` decided whether the note
   (the orange «Pas opțional: …» line) counts by `lblNota.Visible`, and `Control.Visible` is False whenever the bubble
   itself is hidden (it is hidden while the Windows file chooser of the previous step is in front). The height was
   then measured without the note (here 3 lines: the optional reason + «nu e pe ecran acum»), the labels overlapped and
   the first lines were covered. It now counts the note by its text.

Help: `HelpContent/contabil/ajutor.md` (tutorials section, `<!-- slice: 000T -->` already on it): optional steps no longer
promise a jump to «Salvează documentul»; `tutorials/revizie-din-rezervare.md` last step: «poți salva oricând» replaced by
«se pot sări cu «Sari peste»»; `HelpContent/README.md` look-ahead rule updated. `Check-Help.ps1`: «No errors.».

## Third pass — typing completed the step; «Înapoi»

1. **The real cause of «anything typed moves the tutorial on» (step 9/18 «Descriere scurtă»)**: a `wait: changed` step
   was completed by `TextChanged`, i.e. by the first letter. The step after it («Element fundamentare») does not apply
   to a document made from reservations (`when: editable`), so the jump read 9 -> 11. The jump 9 -> 11 itself is by
   design; the first-letter trigger was the bug (the keyboard fix of the second pass was a different, real problem
   and stays). Now: tick box = `CheckedChanged`; `KBotComboBox` = `SelectedIndexChanged` only (a real choice, not typing
   in its search box); any other field = typed (`TextChanged` only marks it) AND the focus has left it
   (`ContainsFocus`, judged on the 150 ms tick). Step 9's text now says «apoi treci în alt câmp».
2. **«Înapoi»** in the tutorial bubble: shown on every step; enabled when `TutorialRunner.PreviousStep` finds an earlier
   step that applies (`when:`), in the same window as the current one, and neither is an `opens:` wait. It shows that
   step again as a **revisit**: nothing done by state completes it (tab already open, box already ticked would bounce
   forward at once), only a new action, or «Înainte ►» (shown on a revisited step); what the operator did meanwhile is
   not undone. Not possible: back into a window left behind (the list under the document that just opened).
3. **Designer**: `btnInapoi`, `btnInainte`, `btnInchide` were already declared in `HelpTourBubble.Designer.vb` (the
   bubble is shared with the guided tours; the three buttons are the same ones). The code only sets their Text /
   Visible / Enabled; position, size, images, fonts are the designer's. The bubble's height now takes the button row
   from the designer (`tlyCorp` row 5) instead of a fixed 44.
   Note: the form is shared, so a change there shows in the tours as well.

Files: `Tutorial/TutorialRunner.vb`, `Help/HelpTourBubble.vb`, help (`ajutor.md`, `tutorials/revizie-din-rezervare.md`,
`README.md`). Build **0 warnings, 0 errors**; `Check-Help.ps1` «No errors.». Not run on screen. Unverified: whether
`KBotTextField` raises `TextChanged` on itself and `ContainsFocus` is true while its inner box has the focus (both
expected from the pre-existing code, which hooked the same event); behaviour of the revisit on `signal:` steps (no
state completion there, «Înainte ►» is the way on).

## Fourth pass — Enter finishes a text step

Tick box unchanged (`CheckedChanged`); leaving the field still finishes a typed step; NEW: **Enter** in a single-line,
editable `TextBoxBase` (`TutorialRunner.OnEnterKey`, called from the key filter) finishes the «changed» step whose
control is, or contains, that box. Not for multi-line boxes (`Multiline` — Enter is a new line; `KBotTextBox` and the
rich editor are such), nor for tick boxes / `KBotComboBox`. Posted to the window after the box handled the key. Pressing
Enter in an empty field finishes the step too (not required that something was typed). Help (`ajutor.md`, step 9 text,
README) says so. Build 0 warnings / 0 errors, `Check-Help.ps1` «No errors.»; not run on screen.

## Fifth pass — «Sari la pasul obligatoriu»

New button `btnSariLaObligatoriu` in `HelpTourBubble.Designer.vb` (own auto-size row 5 of `tlyCorp`, so a hidden one takes
no space; the buttons row moved to row 6; anchored right, 200x32, editable in the designer like the other three).
Shown only on an OPTIONAL tutorial step (never in the guided tours). Click -> `TutorialRunner.OnSkipOptional`: goes to the
first later step that is not optional and whose `when:` holds; none left -> `Finish()` (the tutorial ends, bubble closes,
no «Vrei să ieși» question). The bubble height counts the button by its own flag. Help (`ajutor.md`) has a bullet.
Build 0 warnings / 0 errors; not run on screen. No tooltip on it (the bubble is top-most; a tooltip window might open
under it — not tried). A mandatory step whose `when:` fails at click time is skipped like any step; the jump skips the
actions of the steps it passes (nothing is marked done).

## Sixth pass — borders came back on the bubble buttons

The operator put `ButtonStyles.ApplyTrans(...)` for the four buttons in `ShowTutorial`; the borders stayed. Cause (read in
`KBotThemedForm.OnLoad` / `HandleThemeChanged` and `ThemeManager`): the form runs `ThemeManager.Apply(Me)` — which gives every
`Button` the theme's bordered / rounded look; a transparent button is not an "accent" button, so it is not spared — and only
THEN `OnThemeChanged()`. The first step is built before the first Show, so Load's Apply overwrote it, and every later theme or
scaling broadcast does the same. Moved to `HelpTourBubble.OnThemeChanged` (flag `_tutorialButtons`, set by `ShowTutorial`, so
the guided tours keep their buttons). Build 0 warnings / 0 errors; not run on screen. Seen in passing, left as the operator
wrote it: the two lines setting `btnInainte.Text` / `btnInchide.Text` in `ShowTutorial` are commented out, so those texts
(«Sari peste» / «Înainte ►» / «Gata», «Mă opresc») now come only from the designer.

## Seventh pass — «Mă opresc» ends the tutorial without a question

`TutorialRunner.OnStopRequested` now calls `Finish()` directly (was `Ask("Vrei să ieși din tutorial?")`). Esc / Alt+F4 on the
bubble raise the same event, so they also end it at once. The question stays for the other exits (a click on the dark area, a
key typed elsewhere, closing the window being worked in). Help (`ajutor.md`) adapted. Build 0 / 0, `Check-Help.ps1` «No errors.»;
not run on screen.

## Eighth pass — navigation buttons always visible

`HelpTourBubble.ShowTutorial`: «Înapoi», «Înainte/Sari peste» and «Sari la pasul obligatoriu» are all `Visible = True` on every
tutorial step and only `Enabled` changes (Înapoi: an earlier step exists; Înainte: manual / optional / revisited step; Sari la
pasul obligatoriu: optional step). `_skipShown` is now always true in a tutorial, so the bubble height always includes the
skip button's row. Guided tours are unchanged. Help bullet added. Build 0 / 0; not run on screen.

## Files touched

`src/KBot.Theming/KBotMessage.vb`, `src/KBot.App/Tutorial/{TutorialRunner,TutorialFlow}.vb`,
`src/KBot.App/Help/HelpTourBubble.vb`, `tests/KBot.App.Tests/TutorialFlowTests.vb` (not compiled: that project has older
errors), the three help files above, this worklog, `state/KBOT_STATUS_0000-0009.md`, `KBOT_STATUS.md`.

## Test results

- `dotnet build src\KBot.App\KBot.App.vbproj`: **0 warnings, 0 errors**.
- No test written or run (nothing to unit-test: it is the Windows box).
- Help: no change. The help does not describe where the box sits; the texts the operator reads are unchanged.

## Left unverified or deferred

- **Second pass not seen on screen either.** To check: type with spaces in the long-description editor (the step must
  stay); «Salvează documentul» must stay dark until the last step; step «Alege încă un partener» (right after the file
  chooser step) must show its text and the whole note; the bubble's buttons must still react to a click.
- Cause 1 is derived from the code (a focused button + Space) and from the 7th character in the editor's counter (the
  Space reached the editor too); the log or a run was not looked at. The three layers cover it whichever way the focus
  got there. If the step still moves on, the cause is something else.
- Cause 3 is derived from the code (`Visible` of a child of a hidden form), same caveat.

- **Not seen on screen.** That `MessageBox.Show` passes the extra `0x40000` bit through unchanged is known from how the
  style word is built, but it was not run here: the operator should trigger the exit question once (click outside the
  hole during a tutorial) and confirm the box is above the card.
- A top-most box stays above OTHER programs too while it is open (Alt+Tab does not put it behind). It is short-lived and
  modal, so accepted; if it bothers, the alternative is to hide the card and ring while asking.
