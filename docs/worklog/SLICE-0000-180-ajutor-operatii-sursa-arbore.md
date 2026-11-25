# SLICE-0000-180 - Help: «Operatii contabile» with the source selector, the tree and the empty new-invoices grid (slice AV02-06, 19.11.2026)

## What changed and why
The topic `contabil/nomenclatoare/operatii.md` now says: the window works on the YEAR of the title bar and has its own «Sursa/Sector» selector without «Toate» (opens on the main window's source, or 02A when «Toate»; a change reads both pages again and asks to save unsaved work); «Operatii diverse» shows a tree by journal, closed at the start, with its own search (the manual search box is gone); the partner list shows only the partners of the chosen source; the table «Facturi noi» stays empty until an evidence template is chosen, and the invoices of the current and previous year fill when the partner is chosen. Slice tag `AV02-06, 0000-180` added. Users only: nothing about units, queries or keys.

## Files touched
`src/KBot.App/HelpContent/contabil/nomenclatoare/operatii.md`; `docs/worklog/state/KBOT_STATUS_0000-0009.md` (row 0000-180).

## Test results
None run. `tools\HelpCheck\Check-Help.ps1` - see the line in the slice AV02-06 worklog if run.

## Unverified / deferred
- Nothing was seen on screen; no capture exists for this topic.
- The text describes behaviour that depends on the server fix of AV02-06 (not deployed).
