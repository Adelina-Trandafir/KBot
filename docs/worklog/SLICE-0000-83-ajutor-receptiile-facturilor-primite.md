# SLICE-0000-83 - Help: the "Receptii" view of a received invoice (slices KE01..KE07) (16.10.2026)

## What changed and why
Operator request (16.10.2026): link the received e-invoices to receptions and, later, to payments; plan in `docs/KE_PLAN.md`.

- Topic `contabil.efactura` (`HelpContent/contabil/efactura/index.md`): new section "Receptiile unei facturi primite" (what the view shows, who may write, the amounts with VAT, the classifications of the lines, the three kinds of link, the check before anything starts, what happens after the save, "Scoate legatura"); the bullet of the "Continut" view mentions the new columns; `screens:` names `KeLegaForm`, `KeAngajamentForm`, `KeClasificariForm`; new search words in `keywords:`; slice tags KE01..KE07 and 0000-83.
- `tools/HelpCheck/Check-Help.ps1` accepts the slice ids `KE` / `KEnn` (a worklog or an index row), like `00EF`.
- Watermark: `help-version.txt` = 2026-10-16.

## Files touched
`src/KBot.App/HelpContent/contabil/efactura/index.md`, `help-version.txt`, `tools/HelpCheck/Check-Help.ps1`.

## Test results
`dotnet build` of `KBot.Api`, `KBot.EFactura`, `KBot.Forexe` and `KBot.App`: 0 errors, 0 warnings; `py_compile` of the changed Python files: ok; the two new workflows parse as XML. **Nothing was run against a live database, the live site or on screen, and no test was written or run** (operator rule: no tests).

Check-Help.ps1 -Coverage: no error comes from the new section or its capture tag. 20 errors remain, all in files this slice did not touch (the tutorials efactura-factura-noua and ordonantare-din-plata, a nav item of tur-setari); coverage still lists EFacturaView, SetariAccessView and UpdateOfferForm, as before.

## Unverified / deferred
- **Captures to take:** `efactura-receptii` (new tag, in the new section). **Captures to redo:** `efactura-cumparare` (the "Continut" grid has two new columns).
- No tour or tutorial was changed.
