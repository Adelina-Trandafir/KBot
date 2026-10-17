# SLICE-0114-21 - «Facturi primite» window: the empty strip at the bottom is gone (17.10.2026)

## What changed and why
Operator: «de ce la factura cumparare exista spatiul ala alb in footer? nu il vreau». The window «Facturi primite» (`PrimiteForm`) had a 58 px strip at the bottom (`pnlJos`) with the «Iesire» button and a state line (`lblStare`). As a page of the main window, the «Iesire» button is hidden (the ribbon's «Inchide» closes the page), and the state line only repeated what the footer of the tree already says, so the strip was empty white space. The strip, its two controls, the `CancelButton` link, the `SyncStatus` handler and the button's click handler are removed; the tree card now reaches the bottom of the page.

## Files touched
`src/KBot.EFactura/Forms/PrimiteForm.vb`, `PrimiteForm.Designer.vb`.

## Test results
`dotnet build KBot.sln`: 0 warnings, 0 errors. Nothing run, nothing seen on screen.

## Unverified / deferred
- Not seen on screen. Escape no longer closes the page through the removed cancel button (the ribbon's «Inchide» does).
- The bottom padding of the card (9 px) stays.
- The «Vanzare» window (`FacturiForm`) has a similar strip with «Iesire» and a state line; not touched, the operator asked only about «Cumparare».
- `PrimiteView.SyncStatus` still raises its event (nobody listens now).
