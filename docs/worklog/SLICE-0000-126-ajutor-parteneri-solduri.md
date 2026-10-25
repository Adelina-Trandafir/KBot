# SLICE-0000-126 - Help: «Parteneri», the «Solduri inițiale» table (25.10.2026, slice AV01-08)

## What changed and why
`contabil/nomenclatoare/parteneri.md`, section «Lista și tabelul «Coduri angajament»»: new bullet «Solduri inițiale» (the table under «Coduri angajament»: account, opening balances and
cumulated turnovers of the partner for the working year; «+» / ✕; account picked from a list; saved with the partner at «Salvare»). Tag `AV01-08` added to the section.
Nothing about the Access system or the migration (help is for what the operator sees and does).

## Files touched
`KBot.App/HelpContent/contabil/nomenclatoare/parteneri.md`.

## Test results
`Check-Help.ps1` run: nothing named `parteneri.md` for this change (the other lines of its output were there before: tutorials without slice tags, tour `0000-20`). Nothing seen on screen.

## Unverified / deferred
No capture. `help-version.txt` not bumped (same date).
