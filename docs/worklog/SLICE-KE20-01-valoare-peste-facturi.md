# SLICE-KE20-01 - A new / modified reception may take MORE than the ticked invoices (06.11.2026)

## What changed and why
Operator (06.11.2026): the value of a reception created from K-BOT may be higher than the total of the ticked invoices (the value typed in FOREXE is hers; the invoice for part of it may be missing).
No invoice is invented: the extra stays on the reception only. Rules agreed in chat:

1. **Table of invoices** (lines and invoice header): per-row validation stays (a line never above its own value). Whatever is typed there moves «Valoare» of the grid of classifications.
   Header sum LOWER than the lines' sum: taken off the first line until 0, then the second, and so on (the lines add up to the typed sum). HIGHER: the lines are filled from the first one down, each up to its value (as before).
2. **Grid of classifications, «Valoare»** (new / modified reception): may be higher than the lines (within the angajament's Disponibil), never lower than the lines of the ticked invoices on that classification
   (a credit note, negative lines, takes no surplus). What is above the lines is kept apart per classification (`_extra`) and is NOT written back on the lines. «Valoare» = lines + surplus, so when a line goes down by 50 the value goes down by 50 and the surplus stays.
3. The grid lists **every indicator of the angajament** (not only the classifications of the ticked lines): the surplus may go on one no ticked invoice stands on (its invoice may be missing).
4. **«Leagă facturile de recepție»** (existing reception, its value does not change): unchanged (typing «Valoare» still spreads on the lines, both ways); no surplus there, and it is dropped on switching to this mode.
5. The surplus is **never a link**: `EF_PrimiteReceptii` keeps only what the invoices cover; the rest is the reception's rest (computed already: value - allocated). The robot writes lines + surplus on each indicator (`SumRows`).

Server: `verifica` takes an optional `surplus: [{IdClsf, Suma > 0}]` (sent once per reception, on the first invoice's request); every answer row has `Surplus`; a classification only the surplus stands on gets a row with `Suma = 0`;
the legal-commitment warning counts it; `existenta` + surplus is a problem; for `modificata`, `ValoareNoua` = old + Suma + Surplus. `leaga` is unchanged. A request without surplus is identical to before (an older server still works while no surplus is typed).

## Files touched
`PYTHON/routes/efactura/primite_ke.py` (`_surplus`, `_verifica`, `verifica`), `primite_ke_routes.py`; `src/KBot.Domain/EFacturaKe.vb` (`EFacturaKeCerere.Surplus`, `EFacturaKeRand.Surplus`);
`src/KBot.Api/ApiClient.EFactura.Ke.vb` (`VerifyKeAsync`, `KeRandWire.Surplus`); the former reception window (`_extra`, `AllowsExtra`, `ExtraOf`, `ReceptionTotals`, `FillAlocari`, `UpdateAlocValues`,
`GridAloc_CellValidating` / `CellValueChanged`, `GridLinii_GroupHeaderValueEdited`, `AngajamentProblem`, `AvailabilityProblem`, `BuildRequests`, `SumRows`). Help: `contabil/efactura/index.md` (0000-157). State: `state/KBOT_STATUS_KE.md`, `state/KBOT_STATUS_0000-0009.md`, `KBOT_STATUS.md`.

## Test results
`dotnet build src/KBot.EFactura/KBot.EFactura.vbproj`: succeeded, 0 warnings, 0 errors; `py_compile` OK on the two Python files. No tests written or run, nothing run on screen (the operator runs it); `Check-Help.ps1` not run. Server NOT deployed by me.

## Unverified / deferred
- **Never seen on screen**: the longer grid of classifications (all indicators), the notices, the header rule. The robot's workflows (KE05/07/09) never ran on the real site: a value above the invoices is untested there too.
- The **Disponibil** limit is checked on the client only (`AngajamentProblem` / `AvailabilityProblem`, now on lines + surplus); the server only warns about the legal commitment, FOREXE itself checks on save.
- A reception made only of surplus (no invoice with an amount) is not possible: the button needs at least one invoice amount above 0 (my assumption, to confirm).
- A credit note (negative lines) takes no surplus on its classification (my choice).
- Next in the family: KE20-02 (done, see its worklog) = «Recepții existente» shows the reception's value ON THE INVOICE DATE, not today's.
