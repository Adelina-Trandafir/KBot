# K-BOT — STATUS, slice family 01EF (accounting side of the e-invoices)

Family 01EF (operator, 31.10.2026): numbering `01EF-01`, `01EF-02`, ... Worklogs `SLICE-01EF-nn-*.md`. It continues 00EF (receive, issue, link, send to Vercon; see `KBOT_STATUS_0000-0009.md`) with the part 00EF left out on purpose: booking a received invoice («Contari») and paying it. Starting point: Access `EFACTURA_2025`, `EF_F`, `EF_P`, `SaveEFactura`.

## Current focus
- **01EF-01..03 written (31.10.2026), nothing run or seen.** 01EF-02 (page + calculation) and 01EF-03 (archive) are in code and build clean. Blocked: 01EF-04/05/06 (the posting tables of AV stage 2 do not exist) and the AVC.Migrator step for the existing operations (`NoteC`, `Documente`, `Oper`, `Debit`, `Credit`).

## Registry

| Slice | Name | Status | Worklog | Notes |
|------:|------|--------|---------|-------|
| 01EF-01 | «Contari»: design of the accounting half of the received e-invoices (first item of the bar of `PrimiteView`; archive table; sub-slices) | GATA ca document + `sql/01EF_01_efactura_arhiva.sql` (**DDL nerulat**); fara cod, nimic rulat | `SLICE-01EF-01-contari-design.md` | FOREXE part of Access out of scope (rule); fetching from ANAF is 00EF; the `TZ_SablonOperatii` DDL is run. |
| 01EF-02 | «Contari»: first view of `PrimiteView` (tick boxes, one supplier, evidence + payment DGVs, bookings preview) + `POST /api/efactura/contari/calcul`; no write | GATA pe cod (builds curate); **nimic rulat, nevazut, server nedeployat** | `SLICE-01EF-02-contari-pagina-calcul.md` | Help 0000-144. Lipsa: nr. OP, nr. NC, luna inchisa (tabele inexistente). |
| 01EF-03 | Archive: `EF_PrimiteArhiva` (DDL rulat de operator), routes, menu «Arhiveaza» / «Scoate din arhiva», filter «Afisare facturi din arhiva» | GATA pe cod (builds curate); **nimic rulat, nevazut, server nedeployat** | `SLICE-01EF-03-arhiva.md` | Migrarea arhivei din Access NU e facuta (AVC.Migrator nu migreaza facturi). Help 0000-144. |
| 01EF-04 | «Contari»: «Salveaza evidentierea» prin serviciul de postare (nota, document, operatie, debit, credit, `EF_PrimiteOperatii`), cu confirmarile avertizarilor, luna inchisa, anul | GATA pe cod (builds curate, py_compile); **nimic rulat pe MariaDB, nevazut, server nedeployat** | `SLICE-01EF-04-salvare-evidenta.md` | Help 0000-145. Plata = 01EF-05 + AV02-02. |
| 01EF-07 | «Contari»: pagina refacuta ca formularul Access (liste de sabloane, coloana NC, ordin de plata activ doar cu sablon de plata, bara lungimii explicatiei 1..75, fara diacritice si virgule), pornita din meniul facturii «Adauga operatii contabile»; bife doar la facturile aceluiasi furnizor, minim una bifata | GATA pe cod (builds curate, py_compile); **nimic rulat, nevazut, server nedeployat** | `SLICE-01EF-07-contari-macheta-access.md` | Help 0000-149. **Numarul OP nu se poate completa singur** (nu exista tabela OP, vine cu AV02-02); Cod Client scris de operator; coloana FX neportata. |

## Open threads
- **DDL de rulat de operator:** `sql/01EF_01_efactura_arhiva.sql` pe `AVACONT_SURSA`, apoi sincronizarea schemei.
- AV02-01 tables RUN by the operator (02.11.2026). The AVC.Migrator step «Operatii» (existing operations; no invoice) is written, builds clean, never run: see `src/AVC.Migrator/README.md`.
- Posting tables designed in AV02-01 (`TZ_NoteC`, `TZ_Documente`, `TZ_Oper`, `TZ_Debit`, `TZ_Credit`): **DDL `sql/AVACONT/IntroducereDate/Operatii/AV02_01_operatii.sql` to be run by the operator**, then 01EF-04 (save of the evidence) can be written. 01EF-05 (payment) also waits for AV02-02 (`OPuri`, `PlatiFacturi`).
- One open point: the exception to «Contari has nothing to do with receptions / FOREXE», to be explained by the operator; one assumption in `SLICE-01EF-01-contari-design.md` (no `Contracte` table in the dump). 31.10.2026 answers: tab only in the window with all invoices; only `Tip` M templates (`Tip` and `Fel` both on `TZ_SablonOperatii`); tick boxes in the tree, invoices of ONE supplier; year confirmation stays; `DataArhivare` NULL for migrated invoices. RULE: the FOREXE part of Access is out of scope.

**Next free number: 01EF-08** (01EF-05 payment waits for AV02-02: `OPuri`, `PlatiFacturi`, the link with the ORD, and will fill the next OP number; 01EF-06 year carry-over; both stay reserved).
