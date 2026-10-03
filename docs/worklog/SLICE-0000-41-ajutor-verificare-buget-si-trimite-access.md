# SLICE-0000-41 — Help for 0103-04 / 0103-06: «Verifică bugetul» and «Trimite în Access» (operator request, 02.10.2026)

## What changed and why

`contabil.nomenclatoare.clasificatii` (`src/KBot.App/HelpContent/contabil/nomenclatoare/clasificatii.md`): two bullets for the new
buttons in the window's list; new sections «Verificarea bugetului față de FOREXE» (`<!-- slice: 0103-04 -->`, the three values, «Doar
diferențele», the automatic check after a download) and «Trimite în Access» (`<!-- slice: 0103-06 -->`); header tag now
`0087, 0075-00, 0102, 0103-04, 0103-06`; `screens:` gets `BudgetCheckForm`; new keywords. Only what the operator sees and does.

## Files touched

`src/KBot.App/HelpContent/contabil/nomenclatoare/clasificatii.md`.

## Test results

`Check-Help.ps1 -Coverage`: `BudgetCheckForm` is covered; 3 errors remain that are NOT from this change
(`contabil\fereastra.md` slice `0077-3`; `contabil\asocieri\index.md` and `contabil\forexe\lista.md` slice `0000-39`, which has no
registry row). `dotnet build src\KBot.App` → 0 warnings, 0 errors.

## Capturi de refăcut

- `clasificatii` (tag already marked `redo: 2026-10-02 18:00`, `why: 0103-04 / 0103-06`): two new buttons in the footer.
- No capture exists yet for the new window «Verificare buget FOREXE» (`BudgetCheckForm`; it opens only through the button or after a download
  with differences — no `goto:`).

## Left unverified / deferred

Nothing run, nothing seen on screen. `help-version.txt` already reads 2026-10-02.
