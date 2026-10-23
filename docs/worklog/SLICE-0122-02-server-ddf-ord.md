# SLICE-0122-02 - Server DDF + ORD: the supplier of a row is its CodFiscal (23.10.2026)

**Status: BUILT (py_compile OK), NOT RUN, no test (operator's rule).**

## Data state before this (operator, 23.10.2026)
On one database the one-time queries `0122_01` + `0122_01b` were run and `sql/0122_01_verificari.sql` came back clean: no row left with an IdPartener on SA / SB / ORD_TBL, `VC_NIR` without CodPartener = 0.

## What changed
- `PYTHON/routes/forexe/ddf_edit.py`: the section A / B rows are saved with `CodPartener = partner_links.normalize_cui(cod_partener)` and `IdPartener = NULL` whatever the client sends. They are read with the normalized CodFiscal in `cod_partener` and `id_partener` = 0 (the wire field stays). The unrelated uncommitted edits of this file (program of the DDF, slice 0114-27) were left exactly as they were.
- `PYTHON/routes/forexe/ord_edit.py`: the dictionary that copies the section-A partner onto the payment lines (`incarca_dic_part_ind`) returns the normalized CodFiscal and no id; the payment lines are read with the normalized `cod_partener` and `id_partener` null; saved with `CodPartener` = normalized CodFiscal and `IdPartener` = NULL; the save no longer checks that an `IdPartener` exists in `Parteneri`.
- Client: NO change. `id_partener` still travels (0 / null) and `ApiClient` already reads both; the editors already put the header's CodFiscal in the lines' partner.

## Known differences
- The header (`FX_DDF.CodFiscal`) keeps the code as picked from `Parteneri` (for example with RO), while the lines now come back normalized (no RO). Nothing on the client compares the two (checked by reading: the cell is only displayed and edited).
- Rows of other databases that were NOT yet through the one-time queries still hold their old values; they are read normalized, written normalized when saved.

## Left unverified
Nothing was run: no database, no editor. The save paths (SA / SB inserts and updates, the ORD dictionary) are changed by reading, not exercised.

## Next
0122-03 (readers: `parteneri_edit._ACTIVE`, `sumar.py`, `angajament_dump.py`), then 0122-04 (drop the three foreign keys) once 0122-02 and 0122-03 are on the server.
