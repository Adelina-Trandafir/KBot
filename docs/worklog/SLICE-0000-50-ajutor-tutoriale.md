# SLICE-0000-50 — help for the interactive tutorials (also 000T-05)

The help sub-slice of slice 000T (rule of 30.09.2026: every change to what the operator sees is recorded in the help,
as `0000-NN`, with the slice number in the hidden source tag). Written for the user only (`docs/HELP_SYSTEM.md` §5):
what a tutorial is and how it behaves, how to start one. The designer, the picker and the recorder are an operator
tool and are NOT described (same line as the capture mode).

## What changed and why

- `src/KBot.App/HelpContent/contabil/ajutor.md` (every touched section carries `000T` in its source tag):
  - the opening list and **«Meniul «?»»**: a new bullet **Tutoriale** (all tutorials, from any window; the ones that
    start in the window of the «?» come first);
  - **«Cum pui o întrebare»**: a tutorial that matches the question comes first in the results;
  - new sections **«Tutorialele»** (what it is: ring + the rest dimmed + bubble, waits for the user and never acts for
    them, crosses windows, optional steps with «Sari peste» and the reason, steps that do not apply are skipped, a
    manual step's «Înainte», the exit question «Vrei să ieși din tutorial?» with Da / Nu and «Mă opresc») and
    **«Cum pornești un tutorial»** (the «?» menu group, a typed question, the window it starts from);
  - keywords for the typed question: tutorial, pas cu pas, sari peste, pas opțional, iesi din tutorial, mă opresc...
- `src/KBot.App/HelpContent/help-version.txt`: `2026-10-04`.
- `src/KBot.App/HelpContent/README.md` (maintainer): the format of a tutorial file (done in 000T-01/02).
- Not touched: `NOUTATI.md` — the release script asks an assistant to write a version's section when a version is built
  (`docs/release-notes/README.md`), so the line is left here for that run (no diacritics there, nothing about the
  designer): `- «?» are un grup nou «Tutoriale»: ele te duc pas cu pas (de exemplu «cum adaug o revizie pe baza unei rezervari existente») si asteapta sa faci tu fiecare pas.`

## Captures

No new capture tag: the existing picture of the «?» menu (`ajutor-meniu`) now lacks the «Tutoriale» group, so it is stale.
**Capturi de refăcut:** `ajutor-meniu` (the prepare text still works). Not marked with `redo:` here, to leave the operator's
capture file alone; mark it when the capture run is next done.

## Test results

- `Check-Help.ps1 -Coverage`: **«No errors.»**; the «no topic» list is the same six windows as before.
- `dotnet build src\KBot.App\KBot.App.vbproj`: **0 warnings, 0 errors** (the help files are copied by the build).
- Nothing was opened in the help window; the text was written from the code and from the tutorial runner as built, **not
  from a run**.

## Left unverified or deferred

- The behaviour the text describes was never seen on screen (see 000T-01..04). If a tutorial turns out to behave differently
  (for example the dim over a drop-down list), this text must follow the code.
- The text says the tutorial «pornește dintr-o anumită fereastră»: true of `starts:` in the file; a tutorial without `starts:`
  starts anywhere.
- The first (and only) tutorial is «Adaugă o revizie pe baza unei rezervări existente»; it is named as an example only.
