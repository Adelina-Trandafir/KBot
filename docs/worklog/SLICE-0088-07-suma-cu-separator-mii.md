# SLICE-0088-07 - amounts in the F1135 data keep the thousands dot (05.11.2026)

## What changed and why
Operator: validating note CAB 3 in Adobe showed «Warning: JavaScript Window - f1135 / TypeError: line: 1215: totalSumaCredit.toFixed is not a function» (twice per attempt; the signing trap clicked OK on both).

- Line 1215 of the script object `csValidareNC` (starts at line 7879 of `Surse/NotaCab.xml`) = file line 9093: `totalSumaCredit = parseFloat(totalSumaCredit.toFixed(2))`. The total is built with `totalSumaCredit + valueSumaCredit[i]`; if one `rawValue` is a string, the sum becomes a string and `.toFixed` does not exist. The debit total passed (line 9092), the credit total did not.
- Note 3's data (the datasets in `Surse/NotaCab.xml`, line 10167): every `sumaDebit` is `0,00`, `sumaCredit` is `-8000,00` and `8000,00`. K-BOT wrote amounts with `"0.00"` in ro-RO = no thousands separator.
- The amount fields bind with `num{zzz,zzz,zzz,zz9.99}` under `ro_RO` («.» groups, «,» decimals). `8000,00` has no group dot, so it does not match the picture and the field keeps the text. `0,00` and `-111,00` (the sample NOTA CAB 23) have no thousands and match.
- Fix: `CabCorrectionNotes.FormAmount` now uses `"#,##0.00"` in ro-RO: `8.000,00`, `-8.000,00`, `0,00`, `-111,00`.

## Files touched
- `src/KBot.Domain/CabCorrectionNotes.vb` — `FormAmount` format string + its summary.

## Test results
- `dotnet build src/KBot.Domain`: 0 errors, 0 warnings.
- No tests written or run (operator's rule). Nothing opened in Adobe.

## Left unverified / deferred
- CONFIRMED by the operator on 05.11.2026: after regenerating note 3 the `toFixed` error no longer appears (the validation now ends with «Validarea s-a terminat cu succes!» in the logs). The cause itself was never seen in Adobe, but the fix is proven by that result.
- Whether Adobe accepts the negative sign in front of the grouped amount (`-8.000,00`) is assumed from the sample's `-111,00`.
- Notes already generated (e.g. note 3) keep the old amounts; they must be generated again.
- No help change: the operator sees nothing different besides the form validating.
