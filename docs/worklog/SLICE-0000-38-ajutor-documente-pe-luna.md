# SLICE-0000-38 — Help for slice 0101-01 (ORD / DDF: nav bar stays on a month; «Documente» tab holds the print list) (operator request, 02.10.2026)

Help work for feature slice **0101-01** (`SLICE-0101-01-documente-pe-luna-ord-ddf.md`), branch `SLICE-0100-Multithreading`.
Tags: `<!-- slice: 0101 -->` (added to the ids already there).

## What changed and why
- `contabil.vederi.ord` («Ordonanțare»): the two pages are chosen from the bar on top; on a month / «Toate ordonanțările»
  «Vizualizare» shows the lines of all the ordonanțări under it and the «Document» page is called **«Documente»** and shows the
  print list. «Lista de tipărire»: the intro now says the bar stays and where to open the list.
- `contabil.ddf` («Vederea «Fundamentare»» and «Lista de tipărire»): the same for «Toate reviziile» / a month, with «Document PDF»
  becoming «Documente».
- `tours/tur-ord.md`, `tours/tur-ddf.md`: the steps «Lista de tipărire» (the condition: a month or the root chosen, then the
  «Documente» page), «Paginile…», «Vizualizare» and «Document» / «Document PDF» say what the page does on a month.
- `help-version.txt` unchanged (`2026-10-02`, same day).

## Captures
None taken. None to redo: the pictures that show these screens (`ord-lista-tiparire`, `ord-lista-meniu`, `ddf-lista-tiparire`,
`ddf-lista-meniu`) were never taken (no file in `img/`), so there is nothing out of date. Their `prepare:` now ends with the
extra click — «apoi pe pagina «Documente»» — and a picture of the list will show the bar with «Documente» selected. The pictures
that exist (`ord`, `ddf-vedere`, `ddf-document`, `ord-document`) show a leaf, which looks as before.

## Check
`Check-Help.ps1 -Coverage`: the same single OLD error (slice `0077-3` in `contabil\fereastra.md`) and the same four windows
without a topic as before. `dotnet build src\KBot.App`: 0 warnings, 0 errors. The app was not run.

## Files touched
`src/KBot.App/HelpContent/contabil/vederi/ord.md`, `contabil/ddf/index.md`, `tours/tur-ord.md`, `tours/tur-ddf.md`;
status files; `docs/release-notes/NOUTATI.md`.

## For the operator to read
- The «Fișiere» page of the DDF view is still hidden in the designer while `contabil.ddf` describes it (open thread since 0000-23);
  the new text keeps listing it, untouched.
