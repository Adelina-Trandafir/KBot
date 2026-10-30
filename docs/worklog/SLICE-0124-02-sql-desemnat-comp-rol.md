# SLICE-0124-02 — SQL: `FX_DDF_REV.DeSemnat` and `FX_DDF_COMP_ROL`

**Date:** 30.10.2026. Plan: [SLICE-0124-01](SLICE-0124-01-flux-ddf-kbot-plan-si-banca.md).

## What changed and why

`sql/0124_ddf_desemnat_comp_rol.sql` (new), structure only:

- `FX_DDF_REV.DeSemnat VARCHAR(32) NULL` + index `IX_FX_DDF_REV_DeSemnat`: the role that has to sign the revision next. NULL = not in the
  K-BOT flow (every revision that exists today, every DDF that comes from FOREXE).
- `FX_DDF_COMP_ROL (Comp VARCHAR(255) PK, Rol VARCHAR(32))`: compartment → the role that signs section A. One role per compartment.
  Filled from a «Setări» window (0124-05); the script inserts nothing.

**Role names (operator, 30.10.2026):** `Secretar` and `Imputernicit` (the FOREXE proxy) join `Administrator`, `Contabil`, `Director`.
`Unitati_Utilizatori.Rol` is `VARCHAR(32)` with no list in the database, so the new names need no SQL.

**How it is run:** the admin app, «Interogare pe SURSA» (Vezi, then Rulează pe SURSA), then «Sincronizează unitățile». The script has no data
statements and no `USE`, so the sync can carry it to the units and mark it (the `0078_05` script had `USE 000_DEMO` and a data
statement and landed in the wrong database). Nothing was run by me.

## Files touched

- `sql/0124_ddf_desemnat_comp_rol.sql` (new).

## Test results

Run by the operator on SURSA (30.10.2026). Checked by reading the refreshed `MariaDB_Schema/AVACONT_SURSA.sql` (17:36): `FX_DDF_REV.DeSemnat`, the index
`IX_FX_DDF_REV_DeSemnat` and the table `FX_DDF_COMP_ROL` are there. NOT checked: that the sync carried them to the units (the dumps of the units are older).

## Left unverified / deferred

- **CORRECTION (30.10.2026, later):** the first version of this note said K-BOT refuses `Secretar` / `Imputernicit`, citing `routes/portal/access.py:41`.
  That file is the **web portal's** K-BOT visibility (`policy.py`, slice WB01, other work in the tree), not the desktop login. The desktop login
  (`routes/auth/auth.py`) reads `Rol` from `Unitati_Utilizatori` and returns it with no list, so the new roles CAN sign in on the desktop. What
  they get is the shell of a Contabil: only `Director` is narrowed (`DirectorMode`). Whether `Secretar` / `Administrator` get a narrowed shell (a list
  of documents to sign, nothing else) is a design question for 0124-03/06.
- The admin app's account window (`KBotAdmin/AccountForm`, `cmbRol`) must offer the two new roles — a 0124-03/05 item.
- The match of `FX_DDF.Comp` to `FX_DDF_COMP_ROL.Comp` is by value; a compartment spelled differently (trailing space, other
  diacritics) will not match. The `Setări` window should pick from the compartments that exist, not take free text.
