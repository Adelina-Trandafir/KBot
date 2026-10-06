# SLICE 0000-53 — help for 0111: correcting the value of a snapshot

**Date:** 06.10.2026. Help sub-slice of the feature `0111`
(`SLICE-0111-01-corectie-valoare-instantaneu.md`), done in the same task as the rule says: any change to
what the operator sees is recorded in the help with its slice number.

## What changed and why

The association window has a new context-menu command, «Corectează valoarea…», a new window
(`CorectieValoareForm`) and a new sign on corrected snapshots, **[corectat]**. The help now says what
they are for and how they work, in the operator's words and without any inner workings (no column
names, no server rules beyond what the operator sees).

- `contabil/asocieri/cazuri.md` (`id: contabil.asocieri.cazuri`): new section «Valoarea greșită din
  FOREXE — «Corectează valoarea…»» (when to use it, the window, the rule total = sum of the lines, the
  required reason, «Revino la valorile din FOREXE», «Salvează» is immediate and reloads the window,
  what is not possible); «Legături blocate» now says a value cannot be corrected either when an
  ordonantare freezes the snapshot; `screens:` gets `CorectieValoareForm` (F1 in the new window opens
  this topic); keywords added; slice tag `0111`.
- `contabil/asocieri/fereastra.md` (`id: contabil.asocieri.fereastra`): **[corectat]** among the signs
  of a snapshot row, with a link to the new section; slice tag `0111`.
- `contabil/asocieri/cazuri.md` also says (second pass of 0111) that the correction can be made right
  after a download, kept in the window and written together with the download.
- `tours/tur-asocieri.md`: the «Clic dreapta» step names the new command; slice tag `0111`.
- `help-version.txt`: stays `2026-10-06` (already today's date).

## Captures

- **New, not taken:** `asocieri-corectie-valoare` (tag in `cazuri.md`, no `goto:` because the window
  opens from a context menu; `prepare:` says how to reach it). It shows as «Imagine lipsă» until the
  operator takes it.
- **To redo:** none — no existing picture shows the context menu or the snapshot rows with the new
  sign in a way that makes it wrong.

## Test results

`Check-Help.ps1 -Coverage`: the only complaints about these files were that slice `0111` was not in the
registry; it is now. The remaining errors are the old ones in `tutorials\ordonantare-din-plata.md`
(sections without a slice tag), which this slice did not touch. No tests run. Help text was not looked at
in the running app.

## Left unverified or deferred

- The text was written from the code of `CorectieValoareForm` / `AsociereForm`, which has not run; the
  captions quoted («Din FOREXE», «Valoare corectă», «Motivul corecției», «Revino la valorile din FOREXE»,
  «Salvează», «Renunță») are the designer's.
- The tutorial `legaturile-receptiilor` (000T-10) was left as it is: it teaches moving snapshots, and its
  list of right-click commands is not meant to be complete.
- Whether the ordonantare freeze also applies to the value (assumed yes) is the operator's to confirm;
  if it changes, «Legături blocate» and the new section change with it.

## Files touched

`src/KBot.App/HelpContent/contabil/asocieri/cazuri.md`,
`src/KBot.App/HelpContent/contabil/asocieri/fereastra.md`,
`src/KBot.App/HelpContent/tours/tur-asocieri.md`,
`docs/worklog/state/KBOT_STATUS_0000-0009.md` (row 0000-53 + watermark note),
`docs/worklog/KBOT_STATUS.md` (index line of 0000), this file.
