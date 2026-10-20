# SLICE-0000-110 - Help for 0114-25 (19.10.2026)

## What changed and why
The «Repara...» button of the Rezervari view now opens a window with what was detected and what will be written. Help updated to say so.

## Files touched
- `src/KBot.App/HelpContent/contabil/vederi/rezervari.md`, section «Rezervari gresite si repararea lor»: the bullet on «Repara...» describes the window (two tables, «Repara» / «Renunta»). Tag `0114-25, 0000-110`.
- `src/KBot.App/HelpContent/tours/tur-rezervari.md`, step «Ribbon > Repara...»: same sentence. Same tag.

## Test results
`tools\HelpCheck\Check-Help.ps1`: the only message for these files was «0000-110 not in KBOT_STATUS» before the status row was added.

## Unverified / deferred
Not seen on screen. No new capture needed (the existing «grafic-rezervari» capture does not show the window).
