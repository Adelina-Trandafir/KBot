# SLICE-0000-133 - Help: «Clasificatii cheltuieli» / «Clasificatii venituri», deleting a revenue classification (26.10.2026, slice AV01-13)

## What changed and why
- The expenses side is «Clasificatii cheltuieli» everywhere except the ribbon, whose button is «Clasificatii» (it opens both pages); its page in the window is «Cheltuieli», next to «Venituri». Pages touched: `clasificatii.md` (title and the
  page names), `nomenclatoare/index.md`, `verificare-buget.md`, `fereastra.md`, `panou-lateral.md`, `acasa.md`, `liste.md`, `ribbon.md` (the ribbon button keeps «Clasificatii»; the menu path says «Clasificatii cheltuieli»),
  `ddf/editor.md`, `tours/tur-clasificatii.md` and the three `tutorials/clasificatii-*.md` (step titles and `why:` lines; the anchors are keys and did not change). Tag `AV01-13` on every section touched.
- `clasificatii.md`: new section «Stergerea unei clasificatii de venituri» (the button, when it is enabled, a used classification is not deleted and the message says where, editing does not exist).

## Files touched
The pages listed above, under `src/KBot.App/HelpContent/`.

## Test results
`tools\HelpCheck\Check-Help.ps1`: no new finding for these pages (the old ones elsewhere stay).

## Unverified / deferred
Nothing seen on screen. The captures that show the old names (`clasificatii`, the menu ones) are not retaken; `clasificatii` stays marked `redo`.
