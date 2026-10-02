# SLICE-0000-37 — Help for slice 0101 (receptions that do not close: red; month «+» without a success box) (operator request, 02.10.2026)

Help work for feature slice **0101** (`SLICE-0101-lanturi-neinchise-si-ord-lot.md`), branch `SLICE-0100-Multithreading`.
Tags: `<!-- slice: 0101 -->` (added to the ids already there).

## What changed and why
- `contabil.fereastra` («Lista angajamentelor»): new bullet — an angajament written in **red** has receptions whose chain
  does not close; the hover text lists each reception (date, last snapshot value, reception value); a chain ending in the
  deletion row does not count; link to the cases topic.
- `contabil.asocieri.fereastra` («Stânga»): the «⚠ Lanțul nu se închide…» paragraph now also says the reception and its last
  snapshot are written in red over any other colour, that it goes away once the chain closes, and that the same red marks
  the angajament in the main list.
- `contabil.asocieri.cazuri` («Lanțul nu se închide» oprește descărcările…): new paragraph «Cum îl găsești dinainte».
- `contabil.vederi.ord-generare` («Generare în lot») and `contabil.vederi.plati` («+»): after confirming, the batch ends with
  no final message when all days were generated (an error box still shows the day and the reason).
- `tours/tur-asocieri.md`, step «Recepțiile și lanțurile lor»: one clause about the red.
- `help-version.txt` → `2026-10-02`. Nothing in the help names the server, the query or any file (help rule: users only).

## Captures
None taken, none marked for redo: the screens look the same unless a unit has a chain that does not close, and the capture
list's pictures were shot on a unit where it was fine.

## Check
`Check-Help.ps1 -Coverage`: one error, an OLD one — slice `0077-3` in `contabil\fereastra.md` (already listed in the 0000
Open threads under 0000-32); coverage lists the same four windows without a topic as before. `dotnet build src\KBot.App`:
0 warnings, 0 errors. The app was not run.

## Files touched
`src/KBot.App/HelpContent/` `contabil/fereastra.md`, `contabil/asocieri/fereastra.md`, `contabil/asocieri/cazuri.md`,
`contabil/vederi/ord-generare.md`, `contabil/vederi/plati.md`, `tours/tur-asocieri.md`, `help-version.txt`;
`docs/release-notes/NOUTATI.md`; status files.

## For the operator to read
The sentence «Roșul dispare când lanțul se închide, după o mutare» describes the form's behaviour from the code (the colouring
is recomputed after every move); it was not seen on screen.
