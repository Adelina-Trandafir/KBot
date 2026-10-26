# SLICE-00EF-52 - Sales invoice: «Cont emitent» offers the unit's accounts and saves a new one there (26.10.2026, operator request)

## What changed and why
- The combo «Cont emitent (IBAN)» of «Generale» lists only the unit's own accounts (the list «Conturi Unitate» edits); the accounts of earlier invoices are no longer mixed in (`FillContPlata`).
- An account typed there that the unit does not have yet (compared without spaces/case) is added to the unit's accounts when the invoice is SAVED (`EnsureUnitAccountAsync`, before the invoice itself): the whole list is sent through `SaveConturiAsync` (it replaces the list, so the existing accounts go along), the combo is refilled, the invoice keeps the account as the server wrote it. If the server refuses the IBAN, the message is shown and the invoice is not saved.
- Tooltip of the combo updated.

## Files touched
`KBot.EFactura/FacturiForm.Furnizor.vb`, `FacturiForm.vb`, `Views/Vanzare/VanzareGeneralePage.Designer.vb`; help `contabil/efactura/index.md`, tutorial `efactura-factura-noua.md` (0000-130); `docs/release-notes/NOUTATI.md` (also the Tab line of 00EF-51).

## Test results
`dotnet build src/KBot.EFactura/KBot.EFactura.vbproj`: succeeded. No tests run.

## Unverified / deferred
Not seen on screen, server not called. Not checked: what the server answers to an invalid IBAN. The account is added on invoice save, not when leaving the field (assumption; say if it should be earlier).
