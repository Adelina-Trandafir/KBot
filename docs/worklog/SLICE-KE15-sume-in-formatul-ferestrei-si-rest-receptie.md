# SLICE-KE15 - Page «Recepții»: amounts typed in the operator's regional format; the rest message says where the rest is (19.10.2026)

Operator (19.10.2026), page «Recepții», two reports:
1. «Leagă» stays disabled with «Recepția aleasă nu are destul rest pe 65.02.04.01.20.01.03: rest 0,00, de alocat 81,54», while the table of receptions shows «Rest nealocat» 81,54 for the chosen reception.
2. The user's Windows has «.» as decimal symbol. Typing `77.49` in «Recepționată» gives «Suma recepționată a liniei nu poate depăși valoarea ei (77,49) …»; ticking / unticking the row leaves «Recepționată» at 0.

## What changed and why
- **Typed amounts (2)**: `ReadNumber` parsed the typed text with `DdfSendInputs.CulturaSite` (ro-RO: «.» is the thousands separator), so `77.49` was read as 7749 and refused as above the line value. The grid paints and edits in `CurrentCulture` (`KBotColumnFormat`, `KBotDataView.Painting`), so the text is now read with `CurrentCulture` too. A comma user still types «77,49», a dot user «77.49».
- **Numbers in the notices**: new `Fmt(Decimal)` (`N2`, `CurrentCulture`) used by the three notices of the window (line above its value, reception without enough rest, availability), so a notice shows the sum as the cells do (81.54, not 81,54, for a dot user). Dates and the other `_ro` uses stay.
- **Rest message (1)**: `ReceptionFitProblem` now adds, when the chosen reception has rest on OTHER classifications, which ones and how much («Recepția are rest pe: X (81.54). Alegeți clasificația liniilor în acord cu ea sau o altă recepție.»). Reason (from the code read, not from the data): the list's «Rest nealocat» is the SUM over all the classifications of the reception (`FillReceptii`), the message and «Rest recepție existentă» are PER classification of the lines. 81.54 in the list with 0.00 on `65.02.04.01.20.01.03` can only mean the rest is on another classification of the reception; the old message did not say so.
- The tick/untick returning 0 is the same cause: with an existing reception, `RecomputeDefaults` cuts the default to what the reception has on that classification (0), as designed since KE11. It is NOT changed.

## Update (19.10.2026, after the operator sent `FX_FisaAngajament` of AAB2KPFMMMN)
Verified from the data: reception 129 (13.02.2026) has 54,493.93 on `...20.01.03` fully allocated (invoice 3584) and 9,359.07 on `...20.01.30` with 9,277.53 allocated (invoice 70) -> rest 81.54 on `.30`. The penalty invoice 941 (81.54) belongs to `.30` (its Access operation and order are on `.30`) but its lines had no classification and the operator had them on `.03`. So the old message was right, the screen did not show it. Nothing was wrong in `primite_ke.py`.
- `FillAlocari` now adds, with an existing reception chosen, one row for every classification where the reception has rest but no line stands («... (recepția are rest aici; nicio linie nu e pe ea)», only the «Rest recepție existentă» cell filled), under the rows of the lines. The operator sees the 81.54 next to the line's 0.00.
- **The real hole (operator's screenshot with `.03`, «Recepționată» 0,00, «Leagă» grey, no message)**: with an existing reception whose rest on the lines' classification is 0, `RecomputeDefaults` cuts every sum to 0; `RefreshOk` ran `ReceptionFitProblem` only when some sum was above 0 (`k_ok` needs `k_any`), so NOTHING was said (the message of the first screenshot only showed while old sums were still there; the bottom bar kept the older «Sunt facturi bifate…» text). Now, when no sum is left (`Not k_any`), an existing reception is chosen and an invoice is ticked, `ReceptionFitProblem(True)` checks with the value of the lines (less what the invoice already has linked) and shows the same warning, naming where the reception has rest.
- Not done (offered): default the classification of the lines from the reception's single classification with rest, or from `EF_PrimiteOperatii` (Access).

## Files touched
`src/KBot.EFactura/Forms/KeLegaForm.vb` (`ReadNumber`, `Fmt`, `GridLinii_CellValidating`, `ReceptionFitProblem`, `AvailabilityProblem`); help `contabil/efactura/index.md` (0000-111).

## Test results
`dotnet build src\KBot.EFactura\KBot.EFactura.vbproj`: 0 warnings, 0 errors. Nothing run, nothing seen on screen, no test written (house rule).

## Unverified / deferred
- Report 1 is read from the code only: the data of that angajament (the reception's classifications) was not looked at. If the reception really has rest 81.54 on `65.02.04.01.20.01.03`, the fault is in `_SQL_VALORI_RECEPTII` / `_SQL_ALOCAT_RECEPTII` (`primite_ke.py`) instead; ask for `FX_FisaAngajament(db, cod)` (KE01-02) of that angajament.
- A dot user who types a comma («77,49») gets 7749 (comma = thousands in his format): the window follows his Windows setting.
- Not seen on screen.
