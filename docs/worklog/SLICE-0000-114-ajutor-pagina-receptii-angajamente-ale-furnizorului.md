# SLICE-0000-114 - Help for 0121-05: the angajamente picked on the page «Recepții» stay with the supplier (19.10.2026)

## What changed and why
`contabil/efactura/index.md`, section «Pagina «Recepții»» (tag `0121-05, 0000-114`), the bullet «stânga jos, «Angajamente»»: the table shows the DDF's angajamente, the ones picked for the supplier and the ones already linked; a pick on the empty row now stays linked to the supplier (by its tax code), seen again on any invoice of the supplier and after closing the page, and used by the bulk link; a supplier with no tax code on the invoice cannot be linked (the page says so); «Elimină asociere» removes a picked angajament after a question (its indicators go too).

## Files touched
`src/KBot.App/HelpContent/contabil/efactura/index.md`; registry row in `docs/worklog/state/KBOT_STATUS_0000-0009.md`; `help-version.txt` stays 2026-10-19.

## Test results
`Check-Help.ps1 -Coverage`: nothing reported for `efactura/index.md` (the existing errors elsewhere are untouched). App build 0 warnings / 0 errors. Not run on screen.

## Left unverified or deferred
No capture changed visibly (the table looks the same), nothing marked `redo`.
