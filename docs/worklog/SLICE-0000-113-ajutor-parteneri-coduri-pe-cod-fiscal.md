# SLICE-0000-113 - Help for 0121-03: «Coduri angajament» of the Parteneri window (19.10.2026)

## What changed and why
The «Coduri angajament» table of the «Parteneri» window now works on the supplier's CodFiscal (slice 0121-03), so the help says what the operator sees:
- the table holds the supplier's angajamente (linked to its fiscal code, seen again whenever a partner has the same code);
- «Clasificație» is read-only (it comes from FOREXE); «Cod ang.» is required; an empty «Cod ind.» keeps only the angajament;
- the same «Cod ang.» + «Cod ind.» cannot appear twice;
- the table is disabled (with a hint in its title) until a fiscal code is typed, and for a saved partner whose code was changed but not saved yet;
- deleting a partner leaves the codes in place.

## Files touched
- `src/KBot.App/HelpContent/contabil/nomenclatoare/parteneri.md` (section «Lista și tabelul «Coduri angajament»», tag `0121-03`)
- `src/KBot.App/HelpContent/tours/tur-parteneri.md` (step «Coduri angajament», tag `0121-03`)
- `src/KBot.App/HelpContent/tutorials/parteneri.md` (step «Codurile de angajament», tag `0121-03`)
- `src/KBot.App/HelpContent/help-version.txt` (2026-10-19)
- `docs/worklog/state/KBOT_STATUS_0000-0009.md` (registry row)

## Test results
`tools\HelpCheck\Check-Help.ps1 -Coverage`: no error or warning on the three partner files; its 22 errors are the existing «no slice tag» ones in `tutorials\ordonantare-din-plata.md` and others, not touched here. The app was not run.

## Left unverified or deferred
- No capture carries the «Coduri angajament» table (no `capture:` tag in these topics), so nothing is marked `redo`.
- The «Ajutorul e la zi până la» paragraph of the status was not rewritten (other help sub-slices 0000-107..112 are not in the registry either).
