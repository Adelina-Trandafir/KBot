# SLICE-0122-09 - DDF edit: the «Recepții» of a revision add DIF, not Valoare (29.10.2026)

Sub-slice of [SLICE-0122](SLICE-0122-parteneri-fara-idpartener.md) (numbered there; the work is not about partners).

**Status: WRITTEN, NOT RUN (no test, by the operator's rule); py_compile clean. Server file not deployed.**

Filed under 0122 only because it is the latest slice whose worklog names `forexe/ddf_edit.py`; nothing to do with partners. 0122-04…07 are reserved by the plan above.

- **What and why.** In «adăugare revizie din rezervare» the receptions came out too large (operator, 29.10.2026; AAB2FH3R3FF, IDRZ 425). `FX_Receptii` holds a row per snapshot: `Valoare` is its full value, `DIF` the delta, so `SUM(Valoare)` counts the same money several times. Sumar and the ordonantare already add `DIF`; `ddf_edit.py` now does too, in both queries: `_SQL_RECEPTII_PANA_LA` (before the revision date, at generation) and the `val_rec` query (reopening a saved revision). AAB before 12.10: `SUM(Valoare)` 1.311.793, `SUM(DIF)` 919.463 = `R_Anterioara` of 425.
- **Files.** `PYTHON/routes/forexe/ddf_edit.py` (it also carries an unrelated uncommitted edit, `antet_gol`, left as found); tag `0122-09` on «Secțiunea A» in `HelpContent/contabil/ddf/editor.md` (text unchanged, no `0000-NN` opened).
- **Unverified.** Not checked on a live database (the sums are from the log). Check: `SELECT CodIndicator, SUM(DIF), SUM(Valoare) FROM FX_Receptii WHERE CodAngajament = '...' AND DATE(Data) < '2026-10-12' GROUP BY CodIndicator`.
