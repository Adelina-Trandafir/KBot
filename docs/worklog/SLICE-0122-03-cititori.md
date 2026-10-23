# SLICE-0122-03 - Readers: activity, partner name and dump by CodFiscal (23.10.2026)

**Status: BUILT (py_compile OK, the SQL strings printed and read), NOT RUN, no test (operator's rule).**

## What changed
- `PYTHON/routes/forexe/partner_links.py`: new `fiscal_sql(expression)`, the SQL form of `normalize_cui` (letters and digits, upper case, a leading RO before a digit dropped).
- `PYTHON/routes/forexe/parteneri_edit.py`: the activity test (`_ACTIVE`, the «activ» flag of the list) is the OR of two tests: `_ACTIVE_BY_ID` (the old one, by IdPartener) and `_ACTIVE_BY_CF` (the normalized CodFiscal of the partner is a CodPartener of a DDF section A / B row or an ORD table row, or the CodFiscal is on a DDF header). The delete of a partner is refused when document rows still name it BY ID (the cascade would delete them) or when it is the LAST Parteneri row of a CodFiscal that documents use; if another row has the same CodFiscal the delete is allowed. Docstring updated.
- `PYTHON/routes/forexe/sumar.py`: the «Partener» name of the DDF revisions is looked up by the normalized CodFiscal (lowest IdPartener), with the old lookup by unit code + IdUnitate as a fallback.
- `PYTHON/routes/forexe/angajament_dump.py`: the partners of the dump are matched by CodPartener OR by the normalized CodFiscal.
- Client: no change (no wire change). No visible text changed, so no help entry.

## Why the old tests stay
Until 0122-04 drops the three foreign keys, deleting a `Parteneri` row still deletes the document rows that name it by IdPartener. A database the one-time query 0122_01 has not reached still has such rows, and on it the by-CodFiscal test sees nothing. So the by-id test stays and is a hard refusal. Remove it with 0122-04 / 0122-07.

## Left unverified
Nothing was run. The `IN (SELECT ... UNION ...)` of the activity test and the correlated lookups were composed and read, not executed against MariaDB. `VC_NIR` is NOT part of the activity test (deleting a partner with receipts still fails on the RESTRICT key, as before).

## Next
0122-04 (the operator drops the three foreign keys - DDL by hand, only after 0122-02 and 0122-03 are on the server and the one-time queries ran on every database), then Vercon (0122-05).
