# SLICE-0000-79 - Help for 00EF-33 (15.10.2026)

## What changed and why
`contabil/efactura/index.md`: «Sincronizarea facturilor primite» (the footer «Actualizare…» and its popup: list of periods, box for
1-60 days with Enter) and the page «Cumparare» (no «Reimprospateaza»; the sync starts from the footer or the ribbon). Tags `00EF-33, 0000-79`;
`help-version.txt` stays `2026-10-15`.

## Files touched
`src/KBot.App/HelpContent/contabil/efactura/index.md`, the status files.

## Test results / unverified
`Check-Help.ps1 -Coverage`: no new error. Capture `efactura-cumparare` is already marked to redo (the footer is new in it). The text
describes code not seen working.
