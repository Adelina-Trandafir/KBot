# SLICE-KE20-02 - «Recepții existente» shows the reception's value ON THE INVOICE DATE (06.11.2026)

## What changed and why
Operator (06.11.2026): for past invoices, the grid «Recepții existente» (page «Recepții <> Efactura», mode «Leagă facturile de recepție») must show the value the reception had on the date of the invoice, not today's.

Rules (1 confirmed by the operator 06.11.2026, 2 corrected by her the same day):
1. **Day** = the date of the OLDEST ticked invoice (confirmed); only when it is before today (an invoice of today keeps the old, current behaviour). Only the mode «Leagă» (existing reception); «Modifică» works on the current value (the robot raises what is there now).
2. Not `DataR` (operator: receptions are usually made AFTER the invoice appears, so the date typed on the reception must not decide). The value is read **as the payment orders read it** (operator: "use FX_Receptii"; `ord_edit.py` `_SQL_REC_ANT`):
   `SUM(FX_Receptii.DIF)` of the lines dated (`FX_Receptii.Data`) up to the day, per reception and classification. A reception made after the day has no line up to it: it is read up to the date of its FIRST line, so it shows the value it was made with, not 0 (my addition, see below). Receptions deleted now stay out, as before.
3. **Alocat / Rest**: the links of ALL invoices count, whatever their date (corrected 06.11.2026 after the operator's screenshot: my first rule counted only invoices dated up to the day, so a reception a NEWER invoice had already taken looked free for an older one -
   and `leaga` would have let it be taken twice). Rest = value on the day - all links; with a day, a rest that would be below 0 (the reception was raised after the day for other invoices) is shown as 0.
4. While the answer for the day is on its way the grid is empty (nothing can be chosen on today's figures by mistake); the title of the section says «— valori la dd.MM.yyyy».
5. The day travels with «Leagă» (`la_data`), and the server measures the rest the same way (`REST_RECEPTIE` would otherwise disagree with the screen).

Server (`primite_ke.py`): `_valori_receptii` / `_alocat_receptii` take an optional day (two new queries `_SQL_VALORI_RECEPTII_LA_ZI`, `_SQL_ALOCAT_RECEPTII_LA_ZI`; the queries without a day are untouched); `receptii_angajament(cod, la_data)`
(`GET .../receptii?data=aaaa-ll-zz`, answers `la_data`); `leaga(..., la_data)` for `existenta`. The bulk proposals (KE16, `primite_ke_propuneri.py`) are unchanged: they still use the current values.

## Files touched
`PYTHON/routes/efactura/primite_ke.py`, `primite_ke_routes.py`; `src/KBot.Api/IEFacturaApi.vb` (`GetKeReceptiiLaDataAsync`), `ApiClient.EFactura.Ke.vb`; `src/KBot.Domain/EFacturaKe.vb` (`EFacturaKeCerere.LaData`);
the former reception window (`ReceptionDay`, `CurrentRecKey`, `ReceptiiList`, `LoadReceptiiAtAsync`, `FillReceptii`, `ApplyChecked`, `UseAngajamentAsync`, `ChangeMode`, `BuildRequests`, `UpdateTitles`).
Help: `contabil/efactura/index.md` (0000-158). State: `state/KBOT_STATUS_KE.md`, `state/KBOT_STATUS_0000-0009.md`, `KBOT_STATUS.md`.

## Test results
`dotnet build src/KBot.EFactura/KBot.EFactura.vbproj`: succeeded, 0 warnings, 0 errors; `py_compile` OK. No tests, nothing run on screen or against data (the operator runs it); `Check-Help.ps1` not run. Server NOT deployed by me.

## Unverified / deferred
- **Never run on data**: the new query (`_SQL_VALORI_RECEPTII_LA_ZI`, `GREATEST(day, date of the first line)`) was only compiled as Python, not run on MariaDB; `SUM(DIF)` is the rule of Sumar / payment orders, but a reception whose lines have no `DIF` or `Data` would show nothing.
- The "first line" rule for a reception made after the invoice is mine: exactly as the payment orders do, such a reception would get 0 and disappear from the list.
- The viewing of an invoice a reception already covers (`FillViewedReceptii`) and the bulk proposals keep showing current values.
- Never seen on screen: the empty grid while loading, the title suffix.
