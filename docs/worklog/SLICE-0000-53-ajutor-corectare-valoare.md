# SLICE 0000-53 — help for 0111: correcting the value of a snapshot

**Date:** 06.10.2026. Help for `0111` (`SLICE-0111-01-corectie-valoare-instantaneu.md`), in the same task, under the rule to document operator-visible changes with their slice number.

## What changed and why

Help covers «Corectează valoarea…», `CorectieValoareForm` and **[corectat]**: purpose/use in the operator's words, omitting internal columns/server rules.

- `contabil/asocieri/cazuri.md` (`id: contabil.asocieri.cazuri`): new section «Valoarea greșită din FOREXE — «Corectează valoarea…»» (use/window, total = sum of lines, required reason, «Revino la valorile din FOREXE», immediate «Salvează» and reload, restrictions); «Legături blocate» also forbids value correction when an ordonantare freezes the snapshot; `screens: CorectieValoareForm` gives the window F1 access; keywords and tag `0111` added.
- `contabil/asocieri/fereastra.md` (`id: contabil.asocieri.fereastra`): **[corectat]** among the signs of a snapshot row, with a link to the new section; slice tag `0111`.
- The same section records 0111's second pass: correction immediately after download, kept in the window and written with the download.
- `tours/tur-asocieri.md`: the «Clic dreapta» step names the new command; slice tag `0111`.
- `help-version.txt`: stays `2026-10-06` (already today's date).

## Captures

- **New, not taken:** `asocieri-corectie-valoare` (tag in `cazuri.md`, no `goto:` because the window opens from a context menu; `prepare:` says how to reach it). It shows as «Imagine lipsă» until the operator takes it.
- **To redo:** none — no existing picture shows the context menu or the snapshot rows with the new sign in a way that makes it wrong.

## Test results

`Check-Help.ps1 -Coverage`: the only complaints about these files were that slice `0111` was not in the registry; it is now. The remaining errors are the old ones in `tutorials\ordonantare-din-plata.md` (sections without a slice tag), which this slice did not touch. No tests run. Help text was not looked at in the running app.

## Left unverified or deferred

- The text was written from the code of `CorectieValoareForm` / `AsociereForm`, which has not run; the captions quoted («Din FOREXE», «Valoare corectă», «Motivul corecției», «Revino la valorile din FOREXE», «Salvează», «Renunță») are the designer's.
- The tutorial `legaturile-receptiilor` (000T-10) was left as it is: it teaches moving snapshots, and its list of right-click commands is not meant to be complete.
- Whether the ordonantare freeze also applies to the value (assumed yes) is the operator's to confirm; if it changes, «Legături blocate» and the new section change with it.

## Files touched

`src/KBot.App/HelpContent/contabil/asocieri/cazuri.md`, `src/KBot.App/HelpContent/contabil/asocieri/fereastra.md`, `src/KBot.App/HelpContent/tours/tur-asocieri.md`, `docs/worklog/state/KBOT_STATUS_0000-0009.md` (row 0000-53 + watermark note), `docs/worklog/KBOT_STATUS.md` (index line of 0000), this file.
