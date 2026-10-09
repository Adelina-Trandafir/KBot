# SLICE-00EF-22 -- the tab «E-Factura» from a tree flag (no extra requests)

Replaces the client-side check of 00EF-21 (two requests per selection: the DDF read and the list).

## Done
- `PYTHON/routes/forexe/tree.py`: new column `ArePrimite` = EXISTS(partner of the angajament's DDF whose normalised tax code is in `EF_Primite.CuiNormalizat`) OR EXISTS(row of `EF_PrimiteAsocieri` for the DDF). Same normalisation as `routes/efactura/primite.py`. `0` when `EF_Primite` / `EF_PrimiteAsocieri` / `FX_DDF_Parteneri` are not all there (checked in information_schema, like the notes tables).
- Client: `AngajamentTreeInfo.ArePrimite`, wire `ArePrimite`; `ApplyViewGating` shows the tab when `AreDDF AndAlso ArePrimite`; `VerificaFacturiPrimite` and its fields removed.
- Help 0000-65.

## Verified / not
- `py_compile`; the generated SQL read (5 bind markers as before, both shapes). NOT run on MariaDB (REGEXP_REPLACE in the tree query, join cost). Build of `KBot.App`: 0 errors. Nothing seen on screen. Server must be redeployed.
- Like `AreDDF` the flag is filled when the tree is loaded: new invoices or a new link show the tab at the next refresh of the tree.
