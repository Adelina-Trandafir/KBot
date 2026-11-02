# SLICE-00EF-52 - Sales invoice: «Cont emitent» offers the unit's accounts and saves a new one there (26.10.2026, operator request)

## What changed and why
- The combo «Cont emitent (IBAN)» of «Generale» lists only the unit's own accounts (the list «Conturi Unitate» edits); the accounts of earlier invoices are no longer mixed in (`FillContPlata`).
- A typed account absent from the unit's list (ignoring spaces/case) is added on invoice SAVE, before saving the invoice (`EnsureUnitAccountAsync`). `SaveConturiAsync` replaces the list, so existing accounts are sent too; the combo is refilled and the invoice keeps the server-written account. IBAN rejection displays the server message and prevents invoice saving.
- Tooltip of the combo updated.

## Files touched
`KBot.EFactura/FacturiForm.Furnizor.vb`, `FacturiForm.vb`, `Views/Vanzare/VanzareGeneralePage.Designer.vb`; help `contabil/efactura/index.md`, tutorial `efactura-factura-noua.md` (0000-130); `docs/release-notes/NOUTATI.md` (also the Tab line of 00EF-51).

## Test results
`dotnet build src/KBot.EFactura/KBot.EFactura.vbproj`: succeeded. No tests run.

## Unverified / deferred
Not seen on screen, server not called. Not checked: what the server answers to an invalid IBAN. The account is added on invoice save, not when leaving the field (assumption; say if it should be earlier).
