# SLICE-0000-100 - Help for 0114-24 (18.10.2026)

## What changed and why
`contabil/ribbon.md`: the section «Strangerea ribbonului» now says that a click on a tab of a folded ribbon shows its buttons for a moment (closes after use, on a click elsewhere, Esc, switching window; a click in a field keeps it open) and that double click / the arrow unfolds for good; new section «Ribbon compact». `contabil/setari.md`: the new tick in the table of the «Fereastra principala» group. Slice tags `0114-24, 0000-100`.

## Files touched
`src/KBot.App/HelpContent/contabil/ribbon.md`, `src/KBot.App/HelpContent/contabil/setari.md`.

## Test results
`tools\HelpCheck\Check-Help.ps1`: the only messages about these two files were the slice tag `0000-100` not yet being in the status (fixed by the rows added with this slice); the other errors it prints (tutorials without tags, `tur-setari` nav item `jurnal`) were there before and are not from this change.

## Unverified / deferred
Nothing seen on screen. The capture `ribbon-strans` is unchanged (the prepare text now says to unfold with a double click).
