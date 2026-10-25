# SLICE-0000-123 - Help: «Plan de conturi» (24.10.2026, slice AV01-04)

## What changed and why
New window «Plan de conturi» (slice AV01-04) is in the help: new topic `contabil.nomenclatoare.plan-conturi` (screens `PlanContForm`, `PlanContConturiView`, `PlanContSolduriView`,
`SubunitateEditForm`; sections: the subunit, the accounts), the list in `contabil/nomenclatoare/index.md` (three windows now) and the ribbon
table in `contabil/ribbon.md`. Source tag `AV01-04` everywhere.

## Files touched
`KBot.App/HelpContent/contabil/nomenclatoare/plan-conturi.md` (new), `.../nomenclatoare/index.md`, `.../contabil/ribbon.md`, `help-version.txt`.

## Test results
`tools\HelpCheck\Check-Help.ps1 -Coverage`: no message about the new topic after the worklog exists; the remaining messages are older ones
(tutorial `efactura-factura-noua` controls, `ordonantare-din-plata` tags, `KE12-0x` tags). Not run in the app.

## Unverified / deferred
- Nothing seen on screen; no capture and no tour for this window. Description taken from the Designer files and the form code, not from a running window.
- Not in the help on purpose: opening balances (no window yet), and anything about the server's inner rules beyond what the operator sees.
