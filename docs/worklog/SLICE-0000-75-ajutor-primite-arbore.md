# SLICE-0000-75 - Help for 00EF-29: received invoices, tree by supplier (15.10.2026)

Help sub-slice of 0000 for `SLICE-00EF-29-primite-arbore-furnizori.md` (what changed on the screen is described there).

## What changed and why
`src/KBot.App/HelpContent/contabil/efactura/index.md`:
- «Facturile primite»: the tree is now root > month > supplier (by tax code) > invoice (date, number, total, value on the right);
  the dot of an invoice is neutral for now (no longer orange / green; it does not show whether the invoice was opened); the magnifier in the
  tree header opens the search (3 characters, Esc clears); the button on the right of the header is for filter options (none yet);
  «Continut» has the supplier and the notes on top and the lines below, and the VAT per rate is gone.
- «E-Factura Cumparare: «Facturi primite»»: the search box above the tree is gone, the search is the magnifier of the header;
  the tree has the suppliers level.
- Tags `00EF-29, 0000-75` on both sections; `help-version.txt` = `2026-10-15`.

## Files touched
`src/KBot.App/HelpContent/contabil/efactura/index.md`, `src/KBot.App/HelpContent/help-version.txt`,
`docs/worklog/state/KBOT_STATUS_0000-0009.md`, `docs/worklog/KBOT_STATUS.md`.

## Test results
- `Check-Help.ps1 -Coverage`: no error for `efactura/index.md`; the other errors are old (tours / tutorials).
- `dotnet build src/KBot.App`: 0 warnings, 0 errors. The help was not opened on screen.

## Unverified / deferred
- Captures to redo: `efactura-cumparare` (the tag carries `redo:`).
- The text describes the code, not what was seen on screen.
