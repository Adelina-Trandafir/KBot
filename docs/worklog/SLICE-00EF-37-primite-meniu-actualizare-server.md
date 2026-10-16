# SLICE-00EF-37 - Received invoices: «Sincronizeaza cu serverul» and icons in the footer popup (16.10.2026)

## What changed and why
The popup of the tree footer («Actualizare...», 00EF-33/34) got, under the two ANAF rows (period list, days box):
1. a **separator**, then the row **«Sincronizeaza cu serverul»** (icon `refresh`): reads the list again from the K-BOT server
   (`LoadAsync(_idDdf)`; the shown invoice stays shown; does nothing while an ANAF update runs). Read as «the list from the server»:
   the ANAF update (days) already existed, and the operator said earlier that changes on the ANAF side do not matter.
2. **Icons**: the two ANAF rows carry the `anaf` icon, the server row `refresh`. For that, `CustomPopup.DrawInputRow` now draws the
   item's image on the caption line and moves the text by the icon band (before, an input row ignored its image).

## Files touched
`src/KBot.EFactura/Views/Primite/PrimiteView.vb` (OpenSyncPopup, SyncMenuAnswered, ReloadFromServer),
`src/KBot.Controls/Popup/CustomPopup.Inputs.vb`, `contabil/efactura/index.md` (0000-82).

## Test results
`dotnet build src/KBot.App`: 0 warnings, 0 errors. Nothing run, nothing seen on screen; no test.

## Unverified / deferred
- Not seen: the icon band on the combo / box rows (the popup gets a little wider), the look of the separator before a plain row.
- The meaning of «Sincronizeaza cu serverul» (re-read the list from the K-BOT server) is my reading of the request.
