# SLICE-ME01-34 - E-Factura import: unresolved classifications say which they are and what to do (10.11.2026)

Slice given by Adelina (10.11.2026, ME + last used family; this is the first AVC.Migrator task under the ME scheme). Built clean (`AVC.Migrator.vbproj`, 0 errors; the one warning, BC42108 in `AvcOperatiiStep.vb`, was there before). Nothing run against a database or Access file.

## What changed and why

Run on a unit database, the E-Factura step (`AvcEFacturaStep` -> `EfImporter`) stopped with 7 blocking lines of the kind «clasificatia Access 313, fara unitate: 0 potriviri in Clasificatii (IdClsfAcc + IdUnitate)». The lines only gave the Access id, so nobody could tell which classification it was. (An 8th line, Access 326 on unit 48 with 2 matches, was a duplicate in `Clasificatii`; it went away once the duplicates were cleaned.)

`EfImporter.ReportClassificationProblems` now adds to every such line:
- **Which classification it is**: the code `Capitol.Subcapitol.Articol.Alineat` (built like the generated `Clasificatii.Clsf`) and the name, read from the Access `Clasificatii` table of the unit file (`IssuedFile`, the first ticked unit's `baza<year>.accdb`). That table has no unit column, so its ids belong to that file (the same reading `BudgetOpeningRunner` does).
- **A proposal from MariaDB**, by `Clsf`: exactly one row with that code (on the row's own unit when it has one) -> «Propunere: ... este IDClsf n (unitatea u, IdClsfAcc a)»; several -> the list («Nu exista o propunere unica ...», first 6); none -> «se creeaza din Access prin transferul nomenclatorului Clasificatii din KBot.Migrator, apoi se ruleaza din nou».
- If the unit file cannot be read or has no `Clasificatii` table, a **warning** says so and the lines stay as before (the read is only for information, so a failure is logged in `GlobalErrorLog` and shown, it does not hide the blockers).

Nothing is applied from the proposal. A row with no unit stays BLOCKING, so an invoice is never tied to another unit's classification by guess (rule of 00EF-27 kept: 0 or more than one match = nothing is written). The offer to create is text only: the creation itself is the existing Clasificatii transfer of KBot.Migrator, which sets Sursa/Sector/SS.

## Correction, same day - the single match by code is now APPLIED, and every ticked unit file is read

First run (Adelina): 316, 317, 318, 320 and 326 were blocked although the proposal named exactly one row (IDClsf 19..29, unit 46, same IdClsfAcc); 313 and 314 were «not found» because only the FIRST unit file was read. She asked why they still blocked. They blocked because the report only proposed. Now:
- `ResolveClassification`: when (IdClsfAcc, IdUnitate) gives no single row, `ResolveByCode` ties the Access id to the MariaDB row with the same `Clsf` when ALL unit files that hold the id agree on its code and exactly ONE row has that code (on the row's unit when it has one; without a unit, among the units whose file holds the id when several rows have the code). The tie is written (the row is no longer blocking) and listed as an INFO line «clasificatie gasita dupa Clsf ... de IDClsf n (m randuri EFT_O)», so the choice is visible.
- The same Access id with different codes in different unit files, or two or more MariaDB rows with the code, or none: still BLOCKING, with the codes of every file / the candidates in the line.
- `EfImportOptions.ClassificationFiles` (new): `AvcEFacturaStep` passes every ticked unit's `baza<year>.accdb`; `LoadAccessClassifications` reads the Access `Clasificatii` of each (id -> code, name, unit of the file). With none given the importer reads `IssuedFile` only, as before.

## Correction 2, same day - a classification that Clasificatii lacks is CREATED from Access (Adelina: «Da, creeaza»)

`PrepareClassifications` runs once, right before the EFT_O rows are copied (so no INSERT happens while rows are being read): it reads the EFT_O rows of the invoices kept (`IDEFT` in `_receivedIds`), takes each distinct (Access id, unit) that neither the pair (IdClsfAcc, IdUnitate) nor the code (`ResolveByCode`) answers, and `CreateClassification` handles it:
- Access must know the id (a `Clasificatii` row in a ticked unit file) with ONE code in every file that holds it.
- Unit = the row's own; without one, the single unit whose file holds the id. Several units = NOT created (it would be a guess), the blocking line says so.
- Not created either: that unit already has a row with the same `Clsf` (no duplicate), the unit is not in `Unitati`, the capitol has no known sector (`ClasificatieDerived.Sector` empty), or MariaDB refuses the INSERT (a key into AVACONT_COMUN is missing: the `MySqlException` is logged and its text goes on the blocking line; the statement fails alone, the transaction stays usable). The reason is added to that id's blocking line («Nu s-a putut crea din Access: ...»).
- The row written: `IdClsfAcc` = the Access id, `IdUnitate`, Capitol/Subcapitol/Articol/Alineat, `Denumire` (max 255), and Sector/Sursa/SS from `ClasificatieDerived` (the same source the Clasificatii transfer uses). The new row is added at once to the pair and code lookups, so the EFT_O rows (with or without a unit) resolve through the normal path.
- Verification only announces it: WARNING «clasificatia Access N lipseste din Clasificatii si SE VA CREA ... unitatea u; n randuri EFT_O», no blocker. The write pass reports WARNING «... a fost creata ... IDClsf n». Both inside the run's transaction: any failure rolls it back.
- Not shown in the "rows written" counts of the step (those list the EF_ tables).

Unverified: the INSERT was never run (no database here); the foreign keys of `Clasificatii` into AVACONT_COMUN (DefaArticol, DefaClsfF, DefaTitlu, DefaSursaSector) may refuse a code the common database does not know: then the id stays blocking with MariaDB's message.

## Correction 3, same day - which EFT_O rows, and the unit deduced from Sursa

Second run: Access 316 is «contrib.asiguratorie de munca» in unit 46's file and «apa canal salubritate» in unit 47's file (Access ids are per unit file), and the EFT_O row has no unit, so nothing says which is meant; the run (rightly) refused. Adelina asked (a) where the row is identified (the lines only gave the Access classification id) and (b) that a missing `IdUnitate` be deduced from the row's `Sursa` against `cale.accdb`:
- **Rows named**: every blocking line now ends with «Randuri EFT_O: IDEFTO n (factura IDEFT m, operatia Access p, Sursa «..», suma ..); ...» (first 10, then «si inca k»). These are the Access keys, so the row can be found in `ef_<an>.accdb`.
- **Unit from Sursa**: `EfImportOptions.UnitBySursa` (filled by `AvcEFacturaStep` from the ticked units' `CaiUnit.Sursa`, i.e. cale.accdb). An EFT_O row with no (or 0) `IdUnitate` gets the unit whose Sursa equals the row's `Sursa` when EXACTLY ONE ticked unit has it. Used for the classification lookup, for the creation pass, and it is what is WRITTEN in `EF_PrimiteOperatii.IdUnitate` (the column `PointLinksAtOperations` joins on). The report says per Sursa how many rows got a unit (INFO). No match / several units = the row stays without a unit as before.
- **Unverified**: that `EFT_O.Sursa` holds the same text as `cale.accdb` Sursa (`02A`, `02E`, ...). If it holds something else (a bare letter, empty), nothing is deduced and the INFO line does not appear.

## Files touched
- `src/KBot.Migrator/EFactura/EfImporter.vb` - `_classificationsByCode`, `_accessClassifications`, `LoadClassificationCodes`, `LoadAccessClassifications`, `ResolveByCode`, `ResolveClassification`, `ReportClassificationsByCode`, `ClassificationProposal`, nested `ClsfCandidate` / `AccessClsf`, `ReportClassificationProblems`.
- `src/KBot.Migrator/EFactura/EfModels.vb` - `EfImportOptions.ClassificationFiles`, `UnitBySursa`.
- `src/AVC.Migrator/IntroducereDate/EFactura/AvcEFacturaStep.vb` - fills it.
- `src/KBot.Migrator/KBot.Migrator.vbproj` - FileVersion 1.22.0.0.

## Unverified / deferred
- Not run: that the Access `Clasificatii` of the first unit file holds the ids that the received-invoices store (`ef_<year>.accdb`) uses in `EFT_O.IdClsf`. The code assumes it (same file family as the budget opening); if a store uses ids of another year's file the code shown would be wrong. Compare the first run's codes with the invoices.
- Rows without a unit: whether to resolve them from the invoice's unit is NOT done (Adelina asked only for the report and the proposal).
- Help: none (owner-side tool, not part of the in-app help).
