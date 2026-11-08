# SLICE-0000-171 - Help: «Contări» in the ribbon, the period popup of the synchronisation (08.11.2026)

## What changed and why
Help for the slices 03EF-01..07 (every visual change is recorded in the help with its slice).
- `contabil/efactura/index.md`: the intro line of the window (the synchronisation button opens the period list), the section «Contări» (the ribbon buttons, how to start, the hint in the classification box, the foot of the page gone, warnings in the tooltip of «Salvează», the «Lungime» column, «Renunță»/«Salvează» in the ribbon), «Arhiva facturilor primite» («Ascunde...») and «Sincronizarea facturilor primite» (no more boxes next to the button, the click opens the same popup as «Actualizează»).
- `contabil/ribbon.md`: the group of the window «Facturi primite» and a new «Contări» line in the view group; the table row.
- Tags: `03EF-01` .. `03EF-07`, `0000-171` on the touched sections.

## Files touched
`src/KBot.App/HelpContent/contabil/efactura/index.md`, `contabil/ribbon.md`, `help-version.txt`.

## Test results
`Check-Help.ps1 -Coverage` run: the errors it reports were there before (other topics); nothing new from these two files (see the run in the session). Not run on screen.

## Unverified / deferred
Captures to redo: the Contări page and the ribbon of the received-invoices window (no capture id is tied to them in the text; none redone). Stale after this: the tutorial `efactura-factura-noua` was checked by grep: it does not name the removed ribbon boxes.
