# SLICE-KE20-04 - An invoice is covered on its TOTAL, split into allocations (classification + sum); no more link per invoice line (06.11.2026)

## What changed and why
Operator (06.11.2026): the reception must use the WHOLE value of the invoice (with or without VAT, whatever the invoice states) or a value typed by the user; no more link per line. The table of invoices loses the content of the invoice
and the logic behind it. Her rules for the new table, agreed in chat:

1. One group per ticked invoice. Its rows are the invoice's **allocations**: a classification and a sum. The header of the group holds the classification combo (it shows the classification of the FIRST row).
2. A classification picked in the header = the classification of the first row; its sum is the whole of what is left of the invoice (total less its links), cut for an existing reception to what that reception still has.
3. Under it, **one empty row**, shown ONLY while the allocations add up to less than what is left of the invoice. In it the accountant picks another classification and a sum (the first pick takes what the others left; the sum is then typed).
   No classification picked in the header: expanding the group shows only the empty row.
4. Per-row validation stays: a sum has the sign of the invoice and, with the other rows, may not pass what is left of the invoice.
5. The header of the group is no longer typed over (it shows the sum of its rows). The ticks per line and the group tick are gone (`CheckBoxes = None`).
6. «Valoare» of the grid of classifications (KE20-01, surplus above the invoices) is unchanged for a new / modified reception; for «Leagă» it is read-only now (the reception's value does not change; the sums are those of the table of invoices).
7. An invoice already covered, looked at (KE14), shows the allocations of its links (one row per classification) instead of its products.
8. The classification is written on the lines of the invoice (SetKeClasificatii, so the rest of K-BOT keeps reading it there) only when the invoice goes on ONE classification.

Server (`primite_ke.py` `_verifica`): the check is on the invoice TOTAL (`EF_Primite.Total * Semn`): the sum of all the allocations has the sign of the invoice and does not pass the total less ALL the links the invoice already has (any classification). It no longer needs a line with that
classification, nor the VAT rounding of the lines (KE20-03 stays for `starea` and the proposals). `leaga` uses the same `_verifica`.

Control (`KBot.Controls`): `KBotDataColumn.GroupHeaderComboFirstRow` (default False): the header combo shows the value of the first row of the group, not the value all rows share. Doc in `KBotDataView.md`.

## Files touched
`PYTHON/routes/efactura/primite_ke.py` (`_verifica`); `src/KBot.Controls/DataView/KBotDataColumn.vb`, `KBotDataView.GroupEditor.vb`, `KBotDataView.md`;
the former reception window (`LineTag`, `ApplyChecked`, `StoredClass`, `LabelFor`, `AddAllocRow`, `InvoiceRest`, `AllocatedOf`, `ReshapeInvoices`, `QueueReshape`, `AssignGroupKeys`, `ChosenClass`, `GridLinii_*`, `RecomputeDefaults`,
`FillAlocari`, `GridAloc_*`, `ReceptionFitProblem`, `BuildRequests`, `FillViewedLines`, titles; removed: `SpreadOnClassification`, `FillDown`, `GridLinii_RowChecked`, `GridLinii_GroupHeaderValueEdited`, `InitialLabel`), the former reception window (`gridLinii` columns);
help `contabil/efactura/index.md` (0000-163); this worklog, `state/KBOT_STATUS_KE.md`, `KBOT_STATUS.md`, `release-notes/NOUTATI.md`.

## Test results
`dotnet build src/KBot.EFactura/KBot.EFactura.vbproj`: succeeded, 0 warnings, 0 errors; `py_compile` OK. No tests written or run; nothing run on screen or on data (the operator runs it); `Check-Help.ps1` not run. Server NOT deployed by me.

## Unverified / deferred
- **Never seen on screen**: the new table (group header combo showing the first row, the empty row appearing and disappearing, the «Rest de alocat» note, the group staying folded while rows change). Rows are added / removed a moment after the edit that caused it (`QueueReshape`), never inside the cell event.
- The empty row's sum cell is blank (Nothing) and not typeable until it has a classification; `GroupHeaderComboFirstRow` assumes the rows of a group stay in the order they were added (no sort is set on the grid).
- Typed sums of a row that is later changed to another classification keep their sum; a classified row set to 0 stays a row (it does not disappear).
- The proposals in bulk (KE16, `primite_ke_propuneri.py`) and the «Conținut» view of an invoice still read the lines: untouched.
- A reception **made before this change with several classifications per invoice** keeps its links; they show in the viewing, but cannot be made the same way again except by hand allocations of the same shape (now possible: several rows).
- `GroupHeaderEdit` (slice 02EF-02) stays in the control; this page no longer uses it.
- **Concurrent edit of the former reception window**: while this slice was built the file changed on disk several times (other slices 02EF-04..07). One of my scripted removals hit wrong lines and was repaired by hand (the end of `GridAloc_CellValueChanged`); the file build is clean, but the whole `GridAloc_*` block is worth one look.
