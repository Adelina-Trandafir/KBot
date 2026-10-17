# SLICE-00EF-38 - Migrator: received invoices linked to existing receptions, only when certain (17.10.2026)

## What changed and why
Operator request: when the E-Factura tab of `KBot.Migrator` imports the received invoices, associate them with the angajamente that
already exist (the manual system of slices KE01..KE04), but only when the match is certain: first by the TOTAL value, and when
several receptions have the same value, by the MONTH of the invoice.

What the manual system writes, and so what the migration now writes for a certain match (read from `PYTHON/routes/efactura/primite_ke.py`
`leaga` / `_verifica` and `sql/KE_01_primite_receptii.sql`):
- `EF_PrimiteLinii.IdClsf` = the classification of the reception, on every line of the invoice that has none;
- one row of `EF_PrimiteReceptii` (`Tip = 'existenta'`, `UN = 'migrare'`): angajament, classification, `IDRR`, `Suma` = the invoice total WITH VAT
  and with the invoice sign (a credit note is negative, linked to a negative reception).

The step is the fourth pass over the received invoices (after references and CPV), `EfImporter.DoReceptionLinks`; the rules are in the new
`EfReceptionLinker` (`Propose` reads and decides, writes nothing; `Write` runs only in the import, inside its transaction).

Rules (the order the operator gave):
1. **Which angajamente**: those of the DDFs whose partner has the supplier's tax code (`FX_DDF_Parteneri.CodFiscal`, same rule as the manual
   screen's candidate list), plus the header partner `FX_DDF.CodFiscal` (the DDF editor keeps it as a row of `FX_DDF_Parteneri` too, but the migration does
   not carry that table, so migrated DDFs have only the header). Both compared by `EfCui.Normalize`. A supplier with no DDF is never linked.
2. **Total**: a live reception of those angajamente whose uncovered amount equals the invoice total (value of its LAST counting snapshot per
   classification, the rule of the receptions view and of the manual linking, minus what existing `EF_PrimiteReceptii` rows allocate; same sign;
   tolerance 0.005). Exactly one = linked.
3. **Month**: several receptions with that amount -> only the ones in the invoice's year-month count; exactly one = linked.
4. **Left for the accountant** (a warning in the report, never a block): no single reception after the month, a reception with more than one
   classification with uncovered amount (the invoice lines carry no classification, a split would be a guess), or two invoices that want the same reception
   (both left alone). No reception of the same value = an information line only.
5. **Only invoices this run adds** (ids not in `EF_Primite` before the pass): an invoice already in the target is never touched, so a repeated
   import changes nothing and a link the accountant removed by hand does not come back.

Missing tables or columns (`KE_01_primite_receptii.sql` not applied, receptions not transferred yet): one warning, the step is skipped, the rest
of the import is unaffected. An invoice that did not reach `EF_Primite` (the writer skips a row that collides on another unique key) is skipped by `Write`, so the foreign key
cannot fail the whole import.

## Files touched
- `src/KBot.Migrator/EFactura/EfReceptionLinker.vb` (new: `EfLinkInvoice`, `EfLinkProposal`, `EfReceptionLinker`)
- `src/KBot.Migrator/EFactura/EfImporter.vb` (fields, `ResetState`, step count 6 -> 7, `ReceivedSpec.AfterRow` collects the invoices, `DoReceptionLinks`, `LoadTargetIntKeys`)
- `src/KBot.Migrator/KBot.Migrator.vbproj` (FileVersion 1.19.0.0 -> 1.20.0.0)

No Python, no SQL, no change to anything the K-BOT operator sees: nothing in the help or in `NOUTATI.md` (the Migrator is the owner's tool).

## Test results
`dotnet build src/KBot.Migrator/KBot.Migrator.vbproj`: 0 warnings, 0 errors. Nothing run against a database (no tests, per the operator's rule), the SQL was not tried.

## Unverified / deferred
- The SQL was copied from the manual route (`_SQL_VALORI_RECEPTII`, `_SQL_ALOCAT_RECEPTII`) minus the angajament filter; it has not been run on a migrated unit.
  On a big unit it reads every live reception once (correlated sub-select per reception); not measured.
- Assumed that the main transfer leaves `FX_Receptii_H.IDRR`, `EsteStergere`, `Sters` and `FX_Indicatori.IdClsf` filled so that the "last counting snapshot" rule works on migrated
  receptions. If a migrated reception has no counting snapshot, it simply has no value and is never matched.
- The E-Factura import must run AFTER the main transfer (receptions, indicators, DDFs), like the classification pairs; before it the step reports the missing data and links nothing.
- `Suma` is the invoice total from the Access `TOTAL` times the sign (`NC`), the way the K-BOT screen reads `EF_Primite.Total * Semn`; if Access already stores credit notes negative, that
  existing convention applies to the link too.
- The line values on the manual screen are computed from the XML VAT rates, which the migration does not store (`CotaTVA` on lines, `EF_PrimiteTVA`); the server fills the line rates when the
  invoice is first opened. A few cents between the sum of lines and the total can show as «rest» per classification there; the link amount itself is the total.
