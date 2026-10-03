# SLICE-0000-47 - help topic for the «Verificare buget FOREXE» window

Operator request, 03.10.2026: note the double click (slice 0107) in the help of the check window; create that
help if it does not exist; at least one picture of the window.

## What changed and why

- The check window had no topic of its own (its text was a section of `contabil.nomenclatoare.clasificatii`,
  `BudgetCheckForm` in that topic's `screens:`). New topic **`contabil.nomenclatoare.verificare-buget`**
  (child of `clasificatii`, `screens: BudgetCheckForm`, so F1 / «?» on the window opens it): how it opens
  (the button; the automatic check after a download, only on differences), what it shows (columns, the three
  values, only classifications used in FOREXE, only differences), the double click (0107) and that the
  automatic window has no double-click pick.
- `clasificatii.md`: the old section is now two sentences and a link; `BudgetCheckForm` removed from its
  `screens:`. `nomenclatoare/index.md` links the new topic.
- Capture tag `verificare-buget` in the new topic (no `goto:`: the window is a dialog opened by a button, so
  `prepare:` says how to reach it).

## Files touched

`src/KBot.App/HelpContent/contabil/nomenclatoare/verificare-buget.md` (new), `clasificatii.md`, `index.md`.

## Test results

`Check-Help.ps1 -Coverage -Map`: see below in the status row; no build needed (text only).

## Capturi de refăcut / de făcut

- **`verificare-buget` — the PICTURE DOES NOT EXIST YET.** I did not run K-BOT. Take it from «Meniu › Capturi
  pentru ajutor» (the capture list cannot open this dialog by itself): open «Clasificații bugetare», press
  «Verifică bugetul», shoot the window with at least one row. Needs `FX_Indicatori_Buget` filled (a download
  with slice 0108) and the server with the new `check_budget`; until then the window shows no rows.
- `clasificatii` is still stale (0107: «Clsf» first on the budget grid, last row, the new check box).

## Left unverified / deferred

- Topic not seen in the help window; the F1 key `BudgetCheckForm` not tried.
