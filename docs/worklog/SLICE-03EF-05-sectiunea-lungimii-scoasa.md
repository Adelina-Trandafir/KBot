# SLICE-03EF-05 - «Contari»: the section with the length of the explanation is out of the view (08.11.2026)

## What changed and why
Operator (08.11.2026): remove the section with the length of the explanation (the bar and the counter under the payment grid).
- `tlyBara`, `barExplicatie` (`KBotProgressBar`), `lblLungime` and the intermediate table `tlyPlataDr` removed; `gridPlata` now sits straight in the right cell of `tlyPlata` (full height).
- Its tooltip text (the 1-75 rule, no diacritics, no commas) moved to the header tooltip of the new column (03EF-06); the orphan entry was removed from the page `.resx`.

## Files touched
`FacturiCumparareContarePage.Designer.vb`, `.vb`, `.resx`.

## Test results
Nothing run, nothing seen on screen: the build is clean (KBot.App, Debug, 0 warnings, 0 errors), no test was run (rule), the designer was never opened in Visual Studio.

## Unverified / deferred
The red / amber / green colour of the old bar is gone (see 03EF-06).
