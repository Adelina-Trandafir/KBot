# SLICE-0000-22 — Help content, docs and record for the help assistant

Plan: `docs/PLAN_help_assistant.md` § 0000-22. Closes 0000-18…0000-21.

## What changed and why

### Help text (Romanian, «tu», users only)

- `contabil/ajutor.md` («Cum folosești ajutorul») rewritten: F1 vs «?»; **Meniul «?»** (search
  box, «Pe ecranul acesta», «Tururi ghidate» with one folder per window, «Deschide ajutorul
  complet (F1)», how it closes); **Cum pui o întrebare** (own words, filler words / diacritics /
  endings do not matter, results as you type, page › section + a few words, click / arrows +
  Enter open the section, what to do when nothing is found) + the one honest sentence: questions
  and ratings are sent without the name to improve the help, no personal data in the box;
  **Butoanele unui rezultat** («Deschide «...»», what happens when the view is not available,
  «Tur ghidat»); **A fost util răspunsul?** (stars 1-5, same star takes it back, rating in the help
  window after reading); **Tururile ghidate** (where they start); **Fereastra de ajutor** (the
  same search box, results in place of the contents, Esc). Each section tagged with the sub-slice
  that decided it (`0000-18` … `0000-21`, plus the older `0000-01` / `0000-04`). `screens:` gains
  `KBotHelpPopup` (F1 inside the popup opens this page); keywords for the typed words
  (intrebare, meniul ajutor, tur ghidat, stele, nota, util).
- `director/index.md`: the note about «?» / F1 says what the popup holds, that the director can
  rate an answer, and the same sentence about sending without the name (tag `0081-06, 0000-20,
  0000-21`); `screens:` + `KBotHelpPopup`.
- `tours/tur-fereastra.md` step «Bara de titlu»: «?» opens a help menu (question box, the
  screen's page, the tours); F1 opens the page directly (tag + `0000-20`).
- `tours/tur-asocieri.md` first step: start the tour from «?» in the Asocieri window's caption bar
  (or F1, then the page's tour link) (tag + `0000-20`).
- Nothing about the inner workings (waiting list, batches, server, weights) is in the help.

### Docs

- `docs/HELP_SYSTEM.md`: §1 (F1 vs the «?» popup, the search, the question log), §2 (popup
  controls, `help-version.txt`, table + route; engine files `HelpSearch`, `HelpSearchSession`,
  `HelpPopupTours`, `HelpQuestionLog`), §3 (the popup's topic row and tours), §4 procedure
  (`open:` for a new screen, `keywords:` for new words, tours with their own `screens:`, the
  watermark date also in `help-version.txt`), §6 (last header / tour keys added), new §7
  «Search and the question log (maintainer side)».
- `src/KBot.App/HelpContent/README.md`: `open:` (0000-19), `keywords`, tour `screens:` and how the
  popup picks tours (0000-20) — written in those sub-slices, checked here.
- `tools/HelpCheck/Check-Help.ps1`: `open` and tour `screens` (0000-19 / 0000-20), checked here.
- `docs/PLAN_help_assistant.md`: header says it is built and where the differences are written.

### Record

Worklogs `SLICE-0000-18…22`, rows in `state/KBOT_STATUS_0000-0009.md`, index line
«0000-01…22 GATA», watermark moved («… plus asistentul de ajutor (0000-18…0000-22)»,
`help-version.txt` = `2026-09-30`), open threads for the VPS order and the screen check.

## Captures

New capture tags (the operator shoots them, never Claude):

- `ajutor-meniu` — the «?» popup on the main window (Rezervări selected), empty box.
- `ajutor-cautare` — the popup with «cum trimit un DDF»: results, buttons, stars.
- `ajutor-fereastra-cautare` — the help window with «cum semnez» in the search box (`goto: help`).

The popup closes when the focus leaves it, so the capture tool's own «Fă poza» cannot freeze it:
the `prepare:` text says to take it with Win+Shift+S and load it with «Încarcă».
To re-shoot: none (no existing picture shows the old search box).

## Files touched

- `src/KBot.App/HelpContent/contabil/ajutor.md`, `director/index.md`, `tours/tur-fereastra.md`,
  `tours/tur-asocieri.md`
- `docs/HELP_SYSTEM.md`, `docs/PLAN_help_assistant.md`
- `docs/worklog/state/KBOT_STATUS_0000-0009.md`, `docs/worklog/KBOT_STATUS.md`

## Test results

- `dotnet build src\KBot.App\KBot.App.vbproj`: **0 warnings, 0 errors**.
- `tools\HelpCheck\Check-Help.ps1 -Coverage -Map`: **No errors.** 48 topics, 17 tours, 57 capture
  tags. Coverage lists `RobotQueueForm` (slice 0098, another thread; already in «Ajutor de
  actualizat»).
- No tests, no app run.

## De citit de operator

- The whole `contabil.ajutor` page and the director's note: the popup, the stars and the sending
  have not been seen on screen; the text follows the code.
- «Dacă vederea nu există pentru angajamentul selectat, K-BOT îți spune» relies on the existing
  message of `NavigateForCapture` («Vederea «…» nu e disponibilă pentru angajamentul selectat…»).
- The sentence on sending without the name is in two places (`contabil.ajutor`, `director`) and
  in the search box's tooltip.

## Left open (not part of this plan)

- `RobotQueueForm` (0098) still has no topic; the 0098 and «Reanalizează rezervările» notes in
  «Ajutor de actualizat» are untouched.
- The DDL + route must be on the VPS before a client with 0000-21 is published (0000-21 worklog).
