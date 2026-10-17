# SLICE-0000-87 - Help for 0115-01: side panel (17.10.2026)

## What changed and why
`contabil/panou-lateral.md` (tag `0115-01` on the page and on «Machete deschise» / «Actiuni recente»): the fold button is
at the bottom of the panel, the state is kept between runs, a tooltip lists the section when the panel is folded, an open
macheta shows a picture of itself on hover (a closed one shows nothing), «Actiuni recente» shows only today and empties at
midnight. Captures `panou-lateral` and `panou-lateral-strans` marked `redo:`. `help-version.txt` = `2026-10-17`.

## Files touched
`src/KBot.App/HelpContent/contabil/panou-lateral.md`, `src/KBot.App/HelpContent/help-version.txt`.

## Test results
`Check-Help.ps1 -Coverage` run 17.10.2026: no error from `panou-lateral.md` (the 20 remaining ones are old: tutorials, `tur-setari`).

## Unverified / deferred
Nothing seen on screen. The two captures above must be redone.
