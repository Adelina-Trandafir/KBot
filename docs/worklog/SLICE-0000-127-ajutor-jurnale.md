# SLICE-0000-127 - Help: «Jurnale» (25.10.2026, slice AV01-09)

## What changed and why
New topic `contabil.nomenclatoare.jurnale` (`contabil/nomenclatoare/jurnale.md`, screen `JurnaleForm`): the small window of the unit's journals (Date › Jurnale): code + name, «+» / ✕,
saved together at «Salvare», a used journal cannot be deleted, messages in the coloured box. Listed in `contabil/nomenclatoare/index.md` (keyword + tag `AV01-09` + a line saying it is a small
window over the page, not a page). Nothing about the Access system or the migration.

## Files touched
`KBot.App/HelpContent/contabil/nomenclatoare/{jurnale,index}.md`.

## Test results
`Check-Help.ps1` run: nothing named `jurnale` or `nomenclatoare` in its output (the lines it prints were there before). Nothing seen on screen.

## Unverified / deferred
No capture. `help-version.txt` not bumped (same date). The intro of `index.md` still says «trei machete» (Jurnale is the fourth entry but a window): left as is.
