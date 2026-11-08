# SLICE-AV02-02 — payments, orders of payment (OP, OPME) and the accounting settings

Family AV, stage 2. Asked by the operator (08.11.2026, plan `docs/PLANS/PLAN_04EF_01_PLATI_CONTARI_OP9_OPURI2017.md`) together with `SLICE-04EF-01`. **Code written and compiled (`dotnet build` KBot.App and AVC.Migrator, 0 warnings in the new files); the DDL is NOT run, nothing was run against a database, no test was run (operator rule).**

## What changed and why

**DDL** `sql/AVACONT/IntroducereDate/Operatii/AV02_02_plati_op_setari.sql` (on `AVACONT_SURSA`, then schema sync; safe to run again; after `AV02_01`):

| Table | Role |
|---|---|
| `TZ_Settings` | settings of the accounting at database level, model `VC_Settings`. `IdSubunitate` NULLABLE (NULL = whole unit); own id as key; the generated column `IdSubunitateCheie = IFNULL(IdSubunitate, 0)` carries the unique index `(SettingKey, IdSubunitateCheie)`. Verified by the operator on the server (test table, duplicate refused with `1062`). |
| `TZ_OPME` | header of a multiple order (Access `OPME`); `NrRec` kept as Access has it. |
| `TZ_OPuri` | one order per payment operation (`IdOper` UNIQUE); `IdOPME` optional; `NrOP` given by the server, key not unique on purpose. |
| `TZ_PlatiFacturi` | the allocation of a payment on an invoice (Access `PlatiFacturi`); `IdOP` optional (history). |

**Rules** (operator answers, 08.11.2026): one OP = one payment = one accounting operation (`401 = 770`); several invoices of a supplier in one payment give ONE order. An order is **locked** when it sits in an OPME or a bank statement line confirms it (`FX_Extrase.IDOP`); a locked order and its payment cannot be edited or deleted. Deleting an unlocked payment deletes its order. A second payment on the same invoice is a second, independent order. The number: `MAX(NrOP) + 1`, over all subunits when `PlajaComunaOPuri` is ON, over the subunit only when OFF (default). Before saving the number is PROVISIONAL (tooltip on the cell); the save returns the REAL one.

**Server** (`PYTHON/routes/avacont/introducere_date/operatii/plati.py`, `PYTHON/routes/efactura/primite_plata.py`, `primite_contari.py`, `primite_contari_routes.py`): settings reading (subunit row, then unit row, then default), numbering, lock, rest to pay (evidenced minus allocated; payments never go into `EF_PrimiteOperatii`), reception check, allocation on the evidence operations, deletion. `POST /api/efactura/contari/salveaza` takes `mod` (`evidentiere` / `plata` / `ambele`), `protectie`, `dataOP`, `explicatieOP` and answers `Plata { IdOper, IdOP, NrOP, ... }`; `POST .../calcul` answers the provisional number and `RestPlata`; new `DELETE /api/efactura/contari/plata/<idOP>`; `GET .../existente/<id>` adds `plati` and `restPlata`.

**Migrator** (`src/AVC.Migrator/IntroducereDate/Plati/`, step `Plati`, depends on `Subunitati` and `Operatii`): `OPME`, `OPuri`, `PlatiFacturi` as they are; the order of an allocation is found through the payment operation (`IdPlata = OPuri.Oper`) only when unique; `PlajaComunaOPuri` from `Scheme._OP9.C6` written as ON per subunit (OFF needs no row); a warning when the Access range (the units marked in `LEGUNIT.OP`) is not all the subunits.

## Files touched
`sql/AVACONT/IntroducereDate/Operatii/AV02_02_plati_op_setari.sql` (new); `PYTHON/routes/avacont/introducere_date/operatii/plati.py` (new); `PYTHON/routes/efactura/primite_plata.py` (new), `primite_contari.py`, `primite_contari_routes.py`; `src/AVC.Migrator/IntroducereDate/Plati/AvcPlatiSet.vb`, `AvcPlatiStep.vb` (new), `AvcCatalog.vb`.

## Test results
None run. Python: all four modules parse and import (`python -c import`). VB: `dotnet build` of `KBot.App` and `AVC.Migrator` succeeded.

## Unverified / deferred
- `FX_Extrase.IDOP` as the confirmation of an order is an ASSUMPTION: nothing in K-BOT writes it yet for a new order; for a migrated one it holds the Access `IDOP` (the code matches `IdOPAccess`). A new order can be confirmed only once the statement import writes its id there.
- `ContUnitate` (treasury IBAN) and `bBIC` of the new orders are left empty: the sources (`Unit.SursaCont`, the `CDB` table of banks) are not in the unit database. `pBIC` = `TREZROBU`. They are needed before the order is printed or exported.
- The reception rule (`EF_PrimiteReceptii` joined to `FX_Receptii_R`, not deleted) is my reading of "active"; compare with the receptions view.
- A repeated identical save request is stopped only by the rest check (no request id is stored).
- The tick "Protectie contari Plati/Receptii" is per Windows user (`app_settings.json`) and travels in the request; the server trusts it. A server-side per-user rule is not built. The automatic activation at 2026 -> 2027 is not built (see "Inchidere anuala" in `KBOT_STATUS_AV.md`).
- `OPME`: the form, the lot assembly and the XML `f1129` export are not built; only the table and the migration.
- `NrRec` of `OPME`: the plan reads it as the number of orders in the lot, the older design note as the receipt number; kept as Access has it.
- Migration order check "Operatii" first: the verification warns when `TZ_Oper` is empty and the step runs in the same pass.
