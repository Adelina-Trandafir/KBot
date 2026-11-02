# SLICE-0000-39 — help for the sliceless work of 02.10.2026 (list sources, name in the association window)

Late record (03.10.2026, within 0107): missing worklog/registry row made the existing `0000-39` help tag unknown to `Check-Help.ps1`. This records items 1–2 of `SLICELESS-lista-sursa-asociere-denumire-wicket.md`; item 3 (Wicket monitor) is not operator-visible and has no help.

## What changed and why

Text only, tag `0000-39` on the sections touched (read back from the files, 03.10.2026):

- `contabil.forexe.lista` (header section): the list refresh brings **all sources** of the unit while the tree is sorted by date, and only the source chosen in the title bar while it is sorted by name.
- `contabil.asocieri.index` «Când se deschide»: the association window title shows the **code and the denumire** of the angajament.

## Files touched

`src/KBot.App/HelpContent/contabil/forexe/lista.md`, `src/KBot.App/HelpContent/contabil/asocieri/index.md`.

## Test results

None at the time. Checked now: `Check-Help.ps1 -Coverage` no longer reports `0000-39`.

## Left unverified / deferred

- The behaviour the text describes was never run (see the sliceless worklog). No picture added or to re-shoot.
