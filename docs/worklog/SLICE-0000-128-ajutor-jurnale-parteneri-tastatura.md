# SLICE-0000-128 - Help: journals by sector+source, «Parteneri» list menu, key navigation (26.10.2026, slice AV01-10)

## What changed and why
- `contabil/nomenclatoare/jurnale.md`: the journals follow the sector+source of the title bar (the window lists those; on «Toate» a «Sursa» column appears); the code is unique inside one sector+source. Tag `AV01-10` added.
- `contabil/nomenclatoare/parteneri.md`: the check box «Ascunde partenerii fără activitate» is replaced by the menu of the list header icon (three choices that add partners to the list) and the arrow keys move between partners like a click. Tag `AV01-10` added.
- `contabil/nomenclatoare/plan-conturi.md`: the arrows of the accounts tree open the account reached; nothing happens on a grouping node. Tag `AV01-10` added.

## Files touched
`KBot.App/HelpContent/contabil/nomenclatoare/{jurnale,parteneri,plan-conturi}.md`.

## Test results
`Check-Help.ps1` not run again. Nothing seen on screen.

## Unverified / deferred
No capture; `help-version.txt` bumped to 2026-10-26.
