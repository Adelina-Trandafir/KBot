# SLICE-0122-10 - One month on automatic placement; receptions checked against reservations (29.10.2026)

Sub-slice of [SLICE-0122](SLICE-0122-parteneri-fara-idpartener.md) (numbered there; the work is not about partners). Follow-up of [0122-09](SLICE-0122-09-ddf-receptii-dif.md).

**Status: WRITTEN, BUILT (KBot.App 0 warnings, 0 errors; Python py_compile). NOT RUN, no test (by the operator's rule). Server files not deployed.**

Follow-up of 0122-09, same filing reason. The case, the rule and the decisions are in `docs/FUNDAMENT_Asociere_Receptii.md`, Part 7 (F36–F39); this section only lists what was changed.

### What changed
1. **One month on automatic placement** (F37). Server: `prelucrare_asociere.pas4c_automat`, `PRAG_ZILE_SUGESTIE` = 31, the candidate query now reads `DataR`, the journal says why a candidate was refused. Client: `AsociereForm.AsazaPerechileUnice`, `PRAG_ZILE_ASEZARE` = 31. No exemption by sum. Manual placement unchanged.
2. **Detector** (F39): `routes/forexe/receptii_cumulat.py` (new). `GET /api/forexe/tree` row key `ReceptiiNepotrivite` (`yyyy-MM-dd~indicator~rezervat~receptionat|…`, null = fine); a warning after a saved download (`prelucrare.py`, after step 4d).
3. **Tree, KBotMain:** tooltip block «⚠ Recepții nepotrivite cu rezervările» (day, indicator, reserved, received; up to 6 + «și încă N»; hint to check in «Asocieri») and the red of the broken-chain row. Client: `ReceptieNepotrivita`, `AngajamentTreeInfo.ReceptiiNepotrivite`, DTO field, `ApiClient.ParseReceptiiNepotrivite`.
4. `sql/0122_10_receptii_vs_rezervari_PREVIEW.sql`: the rule as one read-only SELECT.
5. **Help:** «Lista angajamentelor» (`contabil/fereastra.md`) and «Așezările automate» (`contabil/asocieri/pasi.md`), tagged `0122-10`.

### Files touched
`PYTHON/routes/forexe/` `receptii_cumulat.py` (new), `tree.py`, `prelucrare.py`, `prelucrare_asociere.py`; `src/KBot.Domain/ReceptieNepotrivita.vb` (new), `AngajamentTreeInfo.vb`; `src/KBot.Api/UpsertAngajamenteRequest.vb`, `ApiClient.vb`; `src/KBot.App/KbotForm.Tree.vb`, `Forexe/AsociereForm.vb`; `sql/0122_10_receptii_vs_rezervari_PREVIEW.sql` (new); the two help files.

### Test results
Build of KBot.App clean; py_compile of the touched Python files clean. No test written or run. The rule, applied by hand to the log of AAB2FH3R3FF, gives exactly the five wrong days of 12.10.2026 (AAB 99.764,00; AA2 4.834,00; AA3 27.466,00; AA4 2.767,00; AA5 3.034,00) and none of the 55 other checks.

### Left unverified or deferred
- Tried on one payroll angajament only; contracts and other kinds may give false alarms (the detector only warns).
- Not run on MariaDB, neither the Python queries nor the SQL file (CTEs); they must give the same rows.
- No test covers the limit; the existing `pas4c_automat` tests use rows without `DataR` (no verdict, so unchanged). The Python suite and the other test projects were not run.
- Snapshot 245 stays on reception 58 (operator's decision); the detector shows it once the new server and client are out. Decided and not built: see Part 7.

### Corectie 31.10.2026 — alarma falsa «Recepții nepotrivite» cand receptia vine mai tarziu
**Simptom (fisa AAB2MAACHXB, 000_DEMO):** arborele ramanea rosu la 07.02.2026, indicatorul AAB (rezervat 308.780,59, receptionat 172.622,01), desi operatorul a verificat si nu era nimic gresit.
**Cauza:** rezervarea din 07.02 a urcat cu 136.514,44 = receptia 179 (136.158,58) + penalitati (355,86). Penalitatile s-au inregistrat pe 07.02, dar receptia 179 a primit prima versiune pe 15.02, cu exact 136.158,58; de pe 15.02 toate cele 20 de zile verificate se potrivesc. Detectorul presupune ca receptia se inregistreaza in ziua rezervarii si nu deosebeste «receptia vine mai tarziu» de «versiune pusa pe antetul gresit».
**Schimbat:** `PYTHON/routes/forexe/receptii_cumulat.py` — noua `_vine_mai_tarziu`: o zi cu diferenta nu se arata daca suma lipsa (rezervat - receptionat, pozitiva) o aduce EXACT, intr-o singura zi cu receptii, urmatoarea zi cu receptii a indicatorului, iar aceea cade strict INAINTE de urmatoarea zi cu rezervare. Ajutorul `contabil/fereastra.md` (randul «Același roșu...») are propozitia noua; eticheta `0122-10` era deja pe sectiune.
**Verificat (pe fisele existente, cu o oglinda manuala in node, NU codul Python):** AAB2MAACHXB — ziua 07.02 devine «excusata»; AAB2FH3R3FF — cele 5 zile din 12.10 raman aratate (urmatoarea zi cu receptii aduce 65.476 fata de 99.764 lipsa, si cade pe 18.10, zi cu rezervare). `py_compile` curat. Nimic rulat pe MariaDB, codul Python nerulat, teste nerulate.
**Neverificat / amanat:**
- O versiune pusa gresit a carei suma lipsa ajunge totusi exact, singura, inainte de urmatoarea rezervare, nu va mai fi aratata (alarma lipsa). Nu se poate deosebi din cifre.
- `sql/0122_10_receptii_vs_rezervari_PREVIEW.sql` (previzualizare, doar citire) NU are regula noua; ramane cum era.
- Fara numar nou de felie: corectia sta sub 0122-10. Serverul trebuie actualizat de operator, de mana.
