# SLICE-01EF-01 — «Contari»: the accounting half of the received e-invoices (design)

Family **01EF** = the accounting side of the e-invoices (operator, 31.10.2026: «01EF-nn» instead of the 00EF-54 first proposed). `00EF` stays what it is: receiving, issuing, linking, sending to Vercon. 01EF starts from the Access forms `EFACTURA_2025`, `EF_F`, `EF_P` (export `AVACONT/EXPORT_VBA/Forms/`; `EF_F_inlucru*` ignored) and `SaveEFactura` (`Modules/mdl_Salvare_NoteContabile_code.txt`, line 870). Earlier assistants read a typed «01EF» as a slip for 00EF (see `SLICE-00EF-18-primite-client.md`); from 31.10.2026 it is a real family.

Design plus one SQL file (`sql/01EF_01_efactura_arhiva.sql`, not run); no code, nothing run, nothing seen on screen. Visual changes arrive with the later sub-slices, each with its help `0000-NN` and the `01EF-nn` tag.

## Decisions of the operator (31.10.2026)

1. Fetching new invoices from ANAF (token check, number of days, `CautaFacturiNoi`) is **already solved** by the 00EF sync. Not part of 01EF.
2. The DDL of `TZ_SablonOperatii` / `TZ_SablonOperatiiLinie` **has been run** (also present in `MariaDB_Schema/AVACONT_SURSA.sql`; `000_DEMO.sql` is an older dump without them).
3. **RULE (operator, 31.10.2026): the FOREXE part of Access is of no interest** - no FX/ALG tick, no ALG date, no `Cont8067`, no `SAV_Cont8067_*`, no message 365. None of it is read or ported, in 01EF or later.
4. «Contari» is **the FIRST item of the horizontal bar `navDetaliu` of `PrimiteView`** (`KBot.EFactura/Views/Primite/`). No new ribbon button, no new window. The tree, its state filters («facturi noi», «fara operatii in AVACONT») and the invoice selection are the existing ones.
5. «Arhiva» (hidden invoices) gets **its own new table** with the archiving date and other information.
6. **Both areas of the page are DGVs** (`KBotDataView`): the evidence grid AND the payment row (a one-row grid).
7. **«Contari» has nothing to do with receptions or FOREXE** (operator, 31.10.2026, correcting an earlier line of the same day about a link in both directions): it neither reads the reception link nor creates one. There is **one exception**, which the operator will explain later (open point 1).
8. **Selection**: for several accounting operations, several invoices **of the same supplier** can be ticked; ticking an invoice of another supplier unticks all the others. The ticks are **tick boxes in the tree** (confirmed by the operator; `AdvancedTreeControl` has `HasCheckBox` per node).
9. **No DDF for now**: the tab exists only in the window with all invoices (`PrimiteForm`), not in the DDF view (`EFacturaView`).
10. **Only templates of `Tip` M are used**, never `Tip` A («Operatii diverse»). Confirmed by the operator: `TZ_SablonOperatii` carries both `Tip` (A/M) and `Fel` (P/E/A), so the evidence list = `Tip` M + `Fel` E and the payment list = `Tip` M + `Fel` P, filtered by the classification key.
11. The confirmation when the invoice year differs from the work year **stays**.
12. Archive table: `DataArhivare` is **NULL** for invoices migrated from Access (nullable column); `UN` (who) is NULL for them too; `Motiv` is optional. The DDL is written: `sql/01EF_01_efactura_arhiva.sql`.

## What Access does (verified by reading the export)

| Part | Behaviour |
|---|---|
| `EFACTURA_2025` | Three views: FN new, FS saved, FA archive (state from `EFT_O` sums and `EFT.Complet`). Invoices of ONE supplier are ticked (the supplier node itself is refused). Classification: the only classification of article 20 that has a template, else the partner's (`ParteneriAng` / `Contracte`), else the picker `Note_PickClsf`. From the partner: IBAN account, client code, kind of O.P. Two lists: evidence templates (`DefaNoteA.Fel='E'`) and payment templates (`Fel='P'`), filtered by the classification key. |
| `EF_F` over `tmpFacturi` | Per invoice: NC (accounting note number), document number (last numeric group of `NrFact`), date, kind (Factura / Nota credit), **Suma** = total minus already booked (sign -1 for a credit note), **Platit** <= Suma (at a saved invoice <= the difference). |
| `EF_P` over `tmpDoc2017P` | One row: NC, date, O.P. number (next free, checked against used ones on leaving the field), value = sum of `Platit`, explanation `F <invoice numbers> C <client code> <FelOP>` (yellow above 69 characters, locked at 254). |
| Checks | Closed month (document `CCV`): the date moves to the 1st of the next month. Work year vs invoice year: confirmation. Duplicates (same `NrFact` + date + total). Credit note: confirmation. Partner without angajament/contract: warning. |
| `SaveEFactura` | One transaction: `NoteC`; evidence: `Documente`(E), `Oper`, `Debit`, `Credit`, `Actiuni`, `EFT_O`; payment: the same for `Fel=P`, then `OPuri` and `PlatiFacturi`; at the end `EFT.Complet`. Debit/credit lines come from the template line of the invoice's form; in partner columns `XXX` is replaced by the supplier code. |
| FS (saved) | Only a payment on already booked operations (`QEF_PL`: `EFT_O.Suma` minus `PlatiFacturi`). |

Defects seen and **not ported**: the closed-month check compares month numbers without the year; the new numbers (`IDD`, `IDO`, `NrAng`, `IDOP`, `NrOpe`) are computed in the client as `MAX + ID - MIN` (two users can collide).

## What K-BOT already has (verified)

| Access | K-BOT |
|---|---|
| Fetching, tree by supplier, state filters | 00EF-17..33 (`PrimiteView`) |
| Link to the FOREXE reception, classification per line | the former reception window, KE11, 00EF-39 (not used by «Contari») |
| Send to Vercon | 00EF-45..49 |
| `EFT_O` | `EF_PrimiteOperatii` (`IdPrimita`, `IdOperatie` without foreign key, `IdClsf`, `IdUnitate`, `Sursa`, `Suma`), filled only by the Migrator |
| `DefaNote` / `DefaNoteA` | `TZ_SablonOperatii` (`Tip` M/A, **`Fel` E/P/A**, `FelOP`, `Cheie` with `XX.XX`, `Compartiment`, `IdJurnal`) + `TZ_SablonOperatiiLinie` (`Forma`, `IdClsf`, debit/credit base account + analytic, partner mode NIMIC/OPERATIE/FIX, `PA`, `Ordonantare`, `Chelt`) |
| Accounts, journals, classifications | `TZ_PlanCont`, `TZ_Jurnale`, `Clasificatii` (AV01) |
| `Documente`, `Oper`, `Debit`, `Credit`, `NoteC`, `OPuri`, `PlatiFacturi` | **do not exist** (AV stage 2 not started; no such table in `AVACONT_SURSA.sql`) |
| `EFT.Complet`, archive | `EF_Primite` has no `Complet` and no hidden flag |
| `ParteneriAng` (partner x classification -> angajament, indicator, bank account) | `Parteneri_Coduri` (`IdPartener`, `IdClsf`, `CodAng`, `CodInd`, `ContBancar`; unique per partner + classification) and, keyed by the supplier's normalized tax code, `Parteneri_Angajamente` (`CodFiscal`, `CodAng`) + `Parteneri_Angajamente_Indicatori` (`CodAI`, `CodInd`, `ContBancar`) from slice 0121-02; the partner's own account is `Parteneri.ContIBAN` / `Banca`. A table `Contracte` is not in the dump. |

## Design

**Where.** New page `PrimiteContarePage` (same pattern as `PrimiteLiniiPage`), first item of `navDetaliu`, key `contare`, shown through `ShowPage`. Writes only for role `Contabil` (rule of `primite_ke_routes`: 403 `ROL_INTERZIS`, checked on the server).

**Page.** Top: classification button, IBAN account, client code, kind of O.P., the two template lists (evidence / payment). Middle: evidence DGV: NC, number, date, kind, Suma, Plati. Bottom: payment DGV (one row): date, O.P. number, value, explanation with the 70 / 254 counter. Footer: «Salveaza», «Renunta». The tab is not in the DDF view. Ticks: tick boxes in the tree, invoices of one supplier at a time (decision 8); the tree of `PrimiteView` does not show them today, they appear when the «Contari» tab is the active one.
Classification: as in Access, without any reception: the only classification of article 20 that has a template, else the partner's (`Parteneri_Coduri`), else chosen by the operator in a picker of classifications (`CautaInfoClasificatie` / `Note_PickClsf` logic). Warnings of Access that stay: partner without bank account (10382, 10990), angajament / indicator codes not configured (368). Codes and bank account for the O.P. are looked up in `Parteneri_Coduri` by partner + classification, then in `Parteneri_Angajamente(_Indicatori)` by tax code, then `Parteneri.ContIBAN`.

**Server (two calls).**
1. *Calculation, no write* (templates: `Tip` = 'M' only): from invoice(s), classification, templates -> the evidence rows, the payment row, the next O.P. number and the warnings (closed month, year, duplicates, credit note). Replaces `tmpFacturi` / `tmpDoc2017P`; the client keeps the result in memory.
2. *Save*: one transaction; the server assigns every number under lock; calls the posting service of AVACONT (decision of AV: one posting service, the windows only call it); writes `EF_PrimiteOperatii` with a real `IdOperatie`. Evidence lines follow `TZ_SablonOperatiiLinie` of the form (`Forma`) of the classification: `PA` -> commitment + proposal; `Ordonantare` only at payment; `Chelt`; partner by `PartDebitMod` / `PartCreditMod`.

**State.** «Complet» is computed: sum of `EF_PrimiteOperatii.Suma` (sign by `Semn`) equals `EF_Primite.Total`. Not stored.

**Archive table: `EF_PrimiteArhiva`, DDL written in `sql/01EF_01_efactura_arhiva.sql` (NOT RUN; the operator runs it on `AVACONT_SURSA` and syncs the schema).** Columns: `IdArhiva`, `IdPrimita` (UNIQUE, foreign key to `EF_Primite`, cascade), `DataArhivare`, `UN` (e-mail of the user, as in `EF_PrimiteAsocieri.UN`), `Motiv`. `DataArhivare` and `UN` are filled by the server when the operator archives; both are NULL for invoices migrated from Access (neither who nor when is known; no default on the date so a migrated row never gets an invented one). Un-archiving deletes the row. Migrator: Access invoices with `Complet = True` and no `EFT_O` row enter the table with NULL date and NULL `UN`. How an invoice reaches that state in Access is **not verified** (probably the right-click menu `Make_Popup_EFactura_Raport`).

## Sub-slices

| Slice | Content | Needs |
|---|---|---|
| 01EF-01 | This design | nothing |
| 01EF-02 | Page «Contari» (first item of the bar) + server calculation, no write; the operator compares the result with `Oper/Debit/Credit` of invoices already booked in Access (migrated into `EF_PrimiteOperatii`) | the E/P templates filled |
| 01EF-03 | `EF_PrimiteArhiva` (DDL already written in 01EF-01): Migrator step, «Arhiveaza» / «Scoate din arhiva» in the invoice menu, filter in the tree | 01EF-01 |
| 01EF-04 | Save of the evidence through the posting service | AV stage 2 (documents, operations, debit, credit, notes) |
| 01EF-05 | Payment: O.P. and the payment-invoice link | AV stage 2, OP9 read |
| 01EF-06 | Carry-over of open invoices to the next year (`EFACTURA_AA`) | 01EF-04, 01EF-05 |

## Resolved / open

Resolved by the operator (31.10.2026): the DGV shape; ticking several invoices of ONE supplier, with tick boxes in the tree; no DDF for now; only `Tip` M templates, `Tip` and `Fel` both on `TZ_SablonOperatii`; the year confirmation stays; `DataArhivare` NULL for migrated invoices; the FOREXE part of Access is out (rule); «Contari» has nothing to do with receptions. Resolved by reading: the partner / classification / angajament / indicator / bank account sit in `Parteneri_Coduri`, `Parteneri_Angajamente`, `Parteneri_Angajamente_Indicatori` and `Parteneri` (see the table above); `PrimiteView` is hosted by `PrimiteForm` and `EFacturaView`.

Open:
1. **The exception** to «no relation with receptions / FOREXE», which the operator will explain later. Until then nothing about receptions is designed or built in 01EF.

Assumptions to be corrected if wrong:
1. Where the old Access lookup used `Contracte` (no such table in the dump), K-BOT uses only the three `Parteneri_*` tables above.

## Files touched

This file, `sql/01EF_01_efactura_arhiva.sql` (new, not run), `docs/worklog/state/KBOT_STATUS_01EF.md` and the index row in `docs/worklog/KBOT_STATUS.md`.

## Tests

None (nothing to run).

## Unverified / deferred

Everything above marked «not verified»; the way an invoice becomes `Complet` without operations in Access; the shape of the AV stage 2 tables (not designed yet); help topics (none needed for a document).
