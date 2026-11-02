# SLICE-0000-38 — Help for slice 0101-01 (ORD / DDF: nav bar stays on a month; «Documente» tab holds the print list) (operator request, 02.10.2026)

Help work for feature slice **0101-01** (`SLICE-0101-01-documente-pe-luna-ord-ddf.md`), branch `SLICE-0100-Multithreading`. Tags: `<!-- slice: 0101 -->` (added to the ids already there).

## What changed and why
- `contabil.vederi.ord` («Ordonanțare»): the top bar chooses the two pages. On a month / «Toate ordonanțările», «Vizualizare» shows all ordonanțări lines below it; «Document» becomes **«Documente»**, with the print list. «Lista de tipărire» explains the persistent bar and list access.
- `contabil.ddf` («Vederea «Fundamentare»» and «Lista de tipărire»): the same for «Toate reviziile» / a month, with «Document PDF» becoming «Documente».
- `tours/tur-ord.md`, `tours/tur-ddf.md`: the steps «Lista de tipărire» (the condition: a month or the root chosen, then the «Documente» page), «Paginile…», «Vizualizare» and «Document» / «Document PDF» say what the page does on a month.
- `help-version.txt` unchanged (`2026-10-02`, same day).

## Captures
None taken or to redo: `ord-lista-tiparire`, `ord-lista-meniu`, `ddf-lista-tiparire`, `ddf-lista-meniu` never existed in `img/`. Their `prepare:` adds «apoi pe pagina «Documente»» so the list capture will show that bar selection. Existing `ord`, `ddf-vedere`, `ddf-document`, `ord-document` show an unchanged leaf.

## Check
`Check-Help.ps1 -Coverage`: the same single OLD error (slice `0077-3` in `contabil\fereastra.md`) and the same four windows without a topic as before. `dotnet build src\KBot.App`: 0 warnings, 0 errors. The app was not run.

## Files touched
`src/KBot.App/HelpContent/contabil/vederi/ord.md`, `contabil/ddf/index.md`, `tours/tur-ord.md`, `tours/tur-ddf.md`; status files; `docs/release-notes/NOUTATI.md`.

## For the operator to read
- The «Fișiere» page of the DDF view is still hidden in the designer while `contabil.ddf` describes it (open thread since 0000-23); the new text keeps listing it, untouched.
