# SLICE-0000-136 - Help for «Sabloane» after AV01-16 (29.10.2026)

## What changed and why
`src/KBot.App/HelpContent/contabil/nomenclatoare/sabloane.md` rewritten for the reworked window (slice AV01-16): modal window opened over the main one, the source of the title bar fixed while it is open, trees at 40 %, the right side locked until «Modifica» / «+»,
«Salveaza» as the only button; type M: one table row per form, the partner as a tick, tree option «Arata doar clasificatiile cu operatii», bold nodes, «+» and «Modifica / Sterge» on hover; type A: free rows with «+» / «✕»; salary page: chapters in the tree, the four tables
on a bar, the classification as the first column of «Angajamente», the empty last row. Hidden slice tags now read `AV01-14, AV01-16` / `AV01-15, AV01-16` under the sections they touch.

## Files touched
`HelpContent/contabil/nomenclatoare/sabloane.md`.

## Test results
`tools\HelpCheck\Check-Help.ps1`: no message about `sabloane.md` (old messages about other topics remain). No captures.

## Unverified / deferred
Nothing seen on screen; no captures.
