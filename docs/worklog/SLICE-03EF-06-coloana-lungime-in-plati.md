# SLICE-03EF-06 - «Contari», payment grid: a small «Lungime» column with a progress bar (08.11.2026)

## What changed and why
Operator (08.11.2026): a new small column next to «Explicație O.P.» (after it) with a progress bar, updated while typing, as the removed section did.
- `gridPlata`: new column `lungime` (`KBotColumnType.ProgressBar`, `ProgressMax` 75, read-only, 90 px) between «Explicație O.P.» and «Valoare». The value is the length of the explanation (at most 75).
- Live: `GridPlata_EditorTextChanged` → `UpdateBar` writes the row cell under `_filling`; also on a recomposition (client code, kind of invoice) and on a fill.
- `UpdateSave` is also called while typing, so «Salvează» (ribbon) follows the length (1 to 75).

## Files touched
`FacturiCumparareContarePage.Designer.vb`, `.vb`. Help 0000-171.

## Test results
Nothing run, nothing seen on screen: the build is clean (KBot.App, Debug, 0 warnings, 0 errors), no test was run (rule), the designer was never opened in Visual Studio.

## Unverified / deferred
- The progress cell of `KBotDataView` has ONE theme colour: the old bar turned red (empty) / amber (near 75) / green. That is not reproduced; making the cell colour follow a threshold needs a change in the control, not asked for.
- Whether the cell repaints on each key while the editor is open was not seen.
