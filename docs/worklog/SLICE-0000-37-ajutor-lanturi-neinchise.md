# SLICE-0000-37 — Help for slice 0101 (receptions that do not close: red; month «+» without a success box) (operator request, 02.10.2026)

Help work for feature slice **0101** (`SLICE-0101-lanturi-neinchise-si-ord-lot.md`), branch `SLICE-0100-Multithreading`. Tags: `<!-- slice: 0101 -->` (added to the ids already there).

## What changed and why
- `contabil.fereastra` («Lista angajamentelor»): new bullet: **red** angajamente have unclosed reception chains (except chains ending in deletion); hover lists each reception's date, last snapshot value and reception value; link to cases.
- `contabil.asocieri.fereastra` («Stânga», «⚠ Lanțul nu se închide…»): reception / last snapshot in red over any other colour until the chain closes; the same red marks the main-list angajament.
- `contabil.asocieri.cazuri` («Lanțul nu se închide» oprește descărcările…): new paragraph «Cum îl găsești dinainte».
- `contabil.vederi.ord-generare` («Generare în lot») and `contabil.vederi.plati` («+»): after confirmation, successful generation of all days ends silently; errors still show the day and reason.
- `tours/tur-asocieri.md`, step «Recepțiile și lanțurile lor»: one clause about the red.
- `help-version.txt` → `2026-10-02`. Nothing in the help names the server, the query or any file (help rule: users only).

## Captures
None taken, none marked for redo: the screens look the same unless a unit has a chain that does not close, and the capture list's pictures were shot on a unit where it was fine.

## Check
`Check-Help.ps1 -Coverage`: one error, an OLD one — slice `0077-3` in `contabil\fereastra.md` (already listed in the 0000 Open threads under 0000-32); coverage lists the same four windows without a topic as before. `dotnet build src\KBot.App`: 0 warnings, 0 errors. The app was not run.

## Files touched
`src/KBot.App/HelpContent/` `contabil/fereastra.md`, `contabil/asocieri/fereastra.md`, `contabil/asocieri/cazuri.md`, `contabil/vederi/ord-generare.md`, `contabil/vederi/plati.md`, `tours/tur-asocieri.md`, `help-version.txt`; `docs/release-notes/NOUTATI.md`; status files.

## For the operator to read
The sentence «Roșul dispare când lanțul se închide, după o mutare» describes the form's behaviour from the code (the colouring is recomputed after every move); it was not seen on screen.
