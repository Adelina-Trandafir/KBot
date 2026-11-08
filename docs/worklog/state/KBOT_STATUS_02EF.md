# K-BOT - STATUS, slice family 02EF (e-invoices: receptions window)

Family 02EF (operator, 06.11.2026): numbering `02EF-01`, `02EF-02`, ... Worklogs `SLICE-02EF-nn-*.md`. Corrections to the reception window of the received invoices
(the window «Recepții»: «Creeaza receptia» / «Leaga» / «Modifica receptia»; REMOVED in KE21, the shared view `KeReceptiiView` does its work); continues 00EF / KE / 01EF.

## Current focus
- **02EF-10..13 done (07.11.2026), tried and confirmed by the operator, server files registered for deployment:** reception pictures at creation; «Genereaza ordonantare» from the reception (Parteneri data, `IDRP` kept); «Creeaza rezervare» (the «Adauga rezervare» window prefilled, blocking check on this route only, refresh through `KeRobot.DateAngajamentSchimbate`); payments from the statement attached to those ordonantari (automatic rule + by hand).

- **02EF-01 done (06.11.2026):** «Receptii» of the classification table = all receptions of the classification (all angajamente, before the day, `SUM(DIF)`).

- **02EF-02 built (06.11.2026):** «Recepționată» typed in the invoice header; per-angajament columns (Rezervări / Recepții / Disponibil); «Valoare» editable; both grids in step; «Rămas» >= 0 blocks.
- **02EF-08 built (06.11.2026):** a press on an invoice in the tree works like its tick; pressing another invoice takes the former out.

## Registry

| Slice | Name | Status | Worklog | Notes |
|------:|------|--------|---------|-------|
| 02EF-01 | «Receptii» column of the classifications table: all receptions of the classification (every angajament, before the day, SUM(DIF)) | GATA; operatorul a verificat (06.11.2026); `py_compile` OK; server nedeployat de mine | `SLICE-02EF-01-receptii-pe-clasificatie.md` | `primite_ke.py` `disponibil()`; ajutor 0000-153; SUM(DIF) in loc de SUM(Valoare) = decizia mea, de confirmat |
| 02EF-02 | Window «Recepții»: `GroupHeaderEdit` (new control feature) for «Recepționată» in the invoice header; `gridAloc`: «Total recepții», Rezervări / Recepții / Disponibil per angajament, «Valoare» editable, «Rămas» = angajament Disponibil - Valoare (block under 0); both grids update each other | BUILT (0 warnings); NEVER seen on screen, no tests; server `disponibil()` changed (two fields), NOT deployed by me | `SLICE-02EF-02-valori-pe-angajament-si-sincron.md` | ajutor 0000-155; Rezervari = SUM(R_Valoare), Receptii = SUM(DIF) before the day (as Sumar); «la zi» = day of the reception, to confirm |
| 02EF-04 | Reception window: the button of the work («Creează recepția» / «Leagă» / «Modifică recepția») moves to the ribbon group «Recepții» (`ef_receptie_actiune`), the footer bar is gone | BUILT (no BC errors; full build blocked by the running app); never seen on screen | `SLICE-02EF-04-buton-receptie-in-ribbon.md` | ajutor 0000-159 |
| 02EF-06 | Reception window: standard delete button in «Angajamente» (popup menu gone); the tree opens with the invoices without receptions, header-icon menu has «fără recepții» / «asociate» / «toate» | BUILT (`KBot.EFactura` ok); never seen on screen | `SLICE-02EF-06-sterge-standard-si-arbore-fara-receptii.md` | ajutor 0000-161; default list = not associated IN FULL (none or partly linked), by the operator |
| 02EF-07 | «Date recepție» tabs: «Adaugă» / «Asociate» (only while a ticked invoice has receptions linked; grid with «Valoare» = what the invoices take) / «Existente» in line; column «Tip» removed from the receptions grid | BUILT (`KBot.EFactura` ok); never seen on screen; «Adaugă» only for invoices not associated in full | `SLICE-02EF-07-file-adauga-asociate-existente.md` | ajutor 0000-162 |
| 02EF-08 | Reception window: a press on an invoice in the tree works like its tick (the invoice pressed before, not ticked, goes out; ticked ones stay) | BUILT (`KBot.EFactura` ok); never seen on screen; the pressed invoice shows no tick | `SLICE-02EF-08-clic-pe-factura-ca-bifa.md` | ajutor 0000-164 |
| 02EF-10 | Reception created from the page: the two screen pictures of the ordonantare are taken right after the site's «saved» message and filed (`FX_Receptii_IMG`) once the ingest wrote the reception | GATA; operatorul a verificat (07.11.2026) | `SLICE-02EF-10-capturi-la-crearea-receptiei.md` | ajutor 0000-168; pictures only when the site's message was seen |
| 02EF-11 | Ribbon button «Genereaza ordonantare» after a reception is saved: ordonantare built on the RECEPTION (`POST /api/forexe/ord/genereaza-din-receptie`), beneficiary from Parteneri by the invoices' tax code, lines keep `IDRP`, same editor as «Adauga» | GATA; operatorul a verificat (07.11.2026); server file registered (SLICE-02EF-11) | `SLICE-02EF-11-ordonantare-din-receptie.md` | ajutor 0000-168 |
| 02EF-12 | Ribbon button «Creeaza rezervare»: opens the «Adauga rezervare» window prefilled with the reception's sums; save refused while a value cannot make the reception fit (this route only); `KeRobot.DateAngajamentSchimbate` refreshes the pages after a DDF save / an ingest | GATA; operatorul a verificat (07.11.2026) | `SLICE-02EF-12-creeaza-rezervare-din-receptie.md` | ajutor 0000-168; reserved figures change only after the revision is SENT and read back (confirmed intended) |
| 02EF-13 | Payments from the bank statement attach to the lines of an ordonantare made from a reception: automatic rule (angajament + indicator + CUI + sum, a smaller payment splits the row) at the end of the payments ingest and of the statement import; by hand through «Leaga de ordonantare» in the Plati view | GATA; operatorul a verificat (07.11.2026); server files registered (SLICE-02EF-13) | `SLICE-02EF-13-plati-din-extras-la-ordonantari-din-receptie.md` | ajutor 0000-169; OP from the program later |

## Open threads
- 02EF-02: confirm «la zi» = day of the reception and that Rezervari (undated SUM(R_Valoare)) matches the operator's figure on real data; look at the number box over the header and the new columns on screen.
- 02EF-13: the rule uses the tax code only (no IBAN). OPEN, later: payment orders (OP) made from the program.
- `DIF` instead of `Valoare` was not asked for; tell the operator if the old figure was expected.

**Next free number: 02EF-14**
