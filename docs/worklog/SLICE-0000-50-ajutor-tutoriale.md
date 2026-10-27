# SLICE-0000-50 — help for the interactive tutorials (also 000T-05)

The help sub-slice of slice 000T (rule of 30.09.2026: every change to what the operator sees is recorded in the help, as `0000-NN`, with the slice number in the hidden source tag). Written for the user only (`docs/HELP_SYSTEM.md` §5): what a tutorial is and how it behaves, how to start one. The designer, the picker and the recorder are an operator tool and are NOT described (same line as the capture mode).

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
- Not touched: `NOUTATI.md` — the release script asks an assistant to write a version's section when a version is built (`docs/release-notes/README.md`), so the line is left here for that run (no diacritics there, nothing about the designer): `- «?» are un grup nou «Tutoriale»: ele te duc pas cu pas (de exemplu «cum adaug o revizie pe baza unei rezervari existente») si asteapta sa faci tu fiecare pas.`

## Captures

No new capture tag: the existing picture of the «?» menu (`ajutor-meniu`) now lacks the «Tutoriale» group, so it is stale. **Capturi de refăcut:** `ajutor-meniu` (the prepare text still works). Not marked with `redo:` here, to leave the operator's capture file alone; mark it when the capture run is next done.

## Test results

- `Check-Help.ps1 -Coverage`: **«No errors.»**; the «no topic» list is the same six windows as before.
- `dotnet build src\KBot.App\KBot.App.vbproj`: **0 warnings, 0 errors** (the help files are copied by the build).
- Nothing was opened in the help window; the text was written from the code and from the tutorial runner as built, **not from a run**.

## Left unverified or deferred

- The behaviour the text describes was never seen on screen (see 000T-01..04). If a tutorial turns out to behave differently (for example the dim over a drop-down list), this text must follow the code.
- The text says the tutorial «pornește dintr-o anumită fereastră»: true of `starts:` in the file; a tutorial without `starts:` starts anywhere.
- The first (and only) tutorial is «Adaugă o revizie pe baza unei rezervări existente»; it is named as an example only.

## SLICE-0000-51 — help for the tutorial of the start (000T-07) <a id="slice-0000-51"></a>

Procedure `docs/HELP_SYSTEM.md` §4, for the feature slice 000T-07 (the operator sees a new tutorial at start and a new switch in Setări).

### What changed and why

- `contabil.ajutor`: new section «Tutorialul de început» (what starts, after the tour, the two ways to stop it, how to get it back); keywords «tutorial initial», «tutorialul de inceput», «ce sunt tutorialele», «nu mai arata tutorialul». Tag `000T-07, 0000-51`.
- `contabil.setari`: new bullet «Arată tutorialul de început la pornirea K-BOT» in «Pagina «Aplicație»: pornirea K-BOT» (default on, when it switches itself off); keywords; section tag now `0097-02, 000T-07, 0000-51`.
- The two tutorials themselves are not topics (they live in `tutorials/`, tagged `000T-07`); the popup's «Tutoriale» group lists them.
- `help-version.txt` = `2026-10-05`; watermark moved in `state/KBOT_STATUS_0000-0009.md`.

### Capturi de refăcut

`setari` (a new switch on «Aplicație»): its tag got `redo: 2026-10-05 08:53 | why: 000T-07: ...` (it was already red from 0100-02), so the capture list paints it red until the picture is taken again. `ajutor-meniu` (the «?» popup lists two tutorials now; 0000-50 only noted it as to be redone, with no marker) got the same `redo:` marker.

### Test results

`Check-Help.ps1 -Coverage`: «No errors.» No topic left stale by this slice.

### Left unverified

Nothing seen on screen; the text is written from the designer files and the code (`SetariAplicatieView.Designer.vb`, `HelpService.vb`), not from the running app.

## SLICE-0000-107 — ajutor pentru tutorialul de pornire (000T-13) <a id="slice-0000-107"></a>

### What changed and why

Ajutorul urmează schimbarea din [000T-13](SLICE-000T-07-tutorial-initial.md#slice-000t-13): la pornire K-BOT nu mai arată turul ferestrei principale și tutorialul «Ce sunt tutorialele», ci tutorialul «Primii pași în K-BOT», cu o singură bifă în Setări › Aplicație › Generale.

- `contabil/ajutor.md`: «Tururile ghidate» (turul nu mai pornește singur) și «Tutorialul de început» (rescrisă: cei 11 pași, «Mă opresc», caseta de pe bulă, cum se oprește / pornește).
- `contabil/setari.md`: rândul grupului «Fereastra principală» și bifa «Arată tutorialul de început la pornirea K-BOT» (bifa turului a plecat).
- Etichete `000T-13, 0000-107` pe secțiunile atinse. Fără capturi noi.

### Files touched

`src/KBot.App/HelpContent/contabil/ajutor.md`, `setari.md`.

### Test results

`Check-Help.ps1 -Coverage`: nicio eroare despre aceste fișiere. Nimic văzut pe ecran.

### Left unverified or deferred

Tutorialul `setare-tutorial-initial` (cum pornești / oprești tutorialul de început) a fost șters la cererea operatorului, tot 18.10.2026.
