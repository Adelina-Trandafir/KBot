# SLICE-0000-109 - Help for KE14 (19.10.2026)

## What changed and why
`contabil/efactura/index.md`, zone «Facturi primite» of the page «Recepții»: the right-click «Desface asocierea» is replaced by the text of the new view (a press on a resolved invoice shows its products read-only and only its reception;
the button «Elimină asociere» under the table of receptions; any other press or a tick brings the page back; a resolved invoice pressed while others are ticked asks to untick them first). Tag gains `KE14, 0000-109`.

## Files touched
`src/KBot.App/HelpContent/contabil/efactura/index.md`.

## Test results
Not run: `Check-Help.ps1` reported only older missing tags elsewhere on the previous pass; this edit adds none.

## Unverified / deferred
Nothing seen on screen. The capture `efactura-receptii` needs a new take.
