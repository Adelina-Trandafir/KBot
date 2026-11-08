# SLICE-KE20-03 - An invoice without VAT rows is rounded on its header VAT, so its lines add up to its total (06.11.2026)

## What changed and why
Operator (06.11.2026, with the unit's journal `jurnale_alti_utilizatori/AAB2MAACHXB.txt`): she linked an invoice to a reception, the reception's rest went down, but the invoice stayed un-ticked in the tree.
The journal shows the link `EF_PrimiteReceptii.IdLegatura = 131`: invoice `IdPrimita 46` (TPLEN2026 3582), `Suma = 136158.55`, while the invoice's `Total = 136158.58`.

Cause (read in the journal, then reproduced with the invoice's own numbers): the tree ticks and locks an invoice only when its links add up to its total within 0.01 (the former reception window). The link is written on the amount of the invoice's LINES
with VAT; `valori_cu_tva` puts the cents of rounding between the lines and the VAT the invoice states on the last line of the rate, but it needs the VAT per rate from `EF_PrimiteTVA`, and this invoice has NO rows there
(the journal: `EF_PrimiteTVA {"randuri": 0}`; the invoices brought from the old system have none). Without a rate to correct against, each line's VAT stays rounded alone: 13 lines at 11% give 13493.18 instead of the invoice's 13493.21, so the lines came to 136158.55 (0.03 short).

Fix: `_cote` (`PYTHON/routes/efactura/primite_ke.py`) reads the header of the invoice (`EF_Primite.CotaTVA`, `Valoare`, `TVA`) as ONE rate when `EF_PrimiteTVA` has no rows. With it the same 13 lines give 136158.58 (checked by calling `valori_cu_tva` on the invoice's numbers: 136158.55 before, 136158.58 after).
This is the amount of every window that reads the lines (the state of the invoice, the check, the proposals), so «Recepționată» is now the exact total. No change on screen.

## Files touched
`PYTHON/routes/efactura/primite_ke.py` (`_cote`); this worklog; `state/KBOT_STATUS_KE.md`, `KBOT_STATUS.md`.

## Test results
`py_compile` OK. No test file written; the rounding was checked by one call of `valori_cu_tva` on the numbers of invoice 46 (see above). Not run against the database. Server NOT deployed by me.

## Unverified / deferred
- **The link already written (131) stays 0.03 short**: the fix changes what is proposed from now on, not what is stored. To close it: remove the link and make it again, or raise its `Suma` by 0.03 by hand (the operator's decision).
- Other migrated invoices with the same lack of VAT rows and a link written earlier are in the same state (a few cents partial); not listed here.
- An invoice whose header has several rates but no `EF_PrimiteTVA` rows is corrected only on the lines of the header's rate.
- **Found in the same journal, NOT changed**: reception 179 was 136158.58 on 15.02.2026, then lowered to 128730.58 on 08.04.2026 (snapshot 318, final). The invoice (31.01.2026) came before the reception, so KE20-02 read the first value (136158.58) and let the link take 136158.55; the reception is now worth 128730.58, so the link takes ~7427.97 more than it is worth today.
  Proposal waiting for the operator: the rest of a reception = the smaller of its value on the invoice's day and its value now, minus the links.
