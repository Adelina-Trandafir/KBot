# SLICE-0000-84 - Help for 0084-03: «Asociaza parteneri» is a modal popup (16.10.2026)

## What changed and why
The Sumar button now opens `SumarPartnersForm` as a modal popup (slice 0084-03), so the help no longer says «page of the main
window» and «Inchide from the ribbon». `contabil/vederi/sumar.md` (section «Asociaza parteneri») and `tours/tur-sumar.md`
(same step) describe the popup: it waits, «Salveaza» sends and closes, «Renunta» / Esc close without associating.
Tags `0084-03, 0000-84`.

## Files touched
`src/KBot.App/HelpContent/contabil/vederi/sumar.md`, `tours/tur-sumar.md`. (Superseded in part by 0000-85, same day: the
button moved to the ribbon.)

## Test results
`tools\HelpCheck\Check-Help.ps1` run 16.10.2026: nothing reported from these files except the slice ids until the rows existed.
No captures involved.

## Unverified / deferred
Not seen on screen.
