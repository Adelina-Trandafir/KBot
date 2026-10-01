# SLICE-0000-25 — the help for slice 0097-02

The second pass of the corrective slice (`SLICE-0097-02-corective.md`, operator, 01.10.2026)
changed what the operator sees in seven places. This is the help for them, written in the same
task (rule of 30.09.2026), every section tagged `0097-02`.

## What changed and why

| Topic / tour | What |
|--------------|------|
| `contabil.fereastra` | «MENIU»: the folder «Adăugare angajamente...» with its two rows; new section «Cum pornește fereastra» (maximized start, the tour at the first starts); keywords |
| `contabil.ddf.nou` | the path is now «MENIU › Adăugare angajamente... › Angajament nou»; a note pointing to the FOREXE row |
| `contabil.forexe.browser` | four new sections: «Un angajament nou făcut direct în FOREXE», «Întrebările «Sunteți sigur...?» din pagină», «Două recepții pe aceeași dată» (the box's text quoted from `ForexeWatch.js`), «Mini-meniul K-BOT din pagină»; keywords |
| `contabil.setari` | new sections «Pagina «Aplicație»: pornirea K-BOT» (the two new boxes) and «Pagina «FOREXE»: mini-meniul din pagină»; the pages table; keywords |
| `contabil.ajutor` | the login window has no «?»; the tour of the main window starts by itself, until seen to the end or «Nu mai arăta turul inițial»; keywords |
| `contabil.autentificare` | the login window has no «?»; its page opens with F1 |
| `tur-fereastra` | step «Bine ai venit» (the tour starts by itself), step «Butonul MENIU» (the folder) |
| `tur-ddf` | step «De unde pornește» (the new menu path) |
| `tur-setari` | steps «Aplicație» and «FOREXE» (the new switches) |

Also: `HelpContent/README.md` (the menu key `angajament_forexe` exists but starts the robot — not
for `goto:` / `open:`), `docs/HELP_SYSTEM.md` §1 (no «?» on the login window; the initial tour and
what depends on the id `tur-fereastra`), `help-version.txt` = `2026-10-01`.

Engine changes (recorded in 0097-02, listed here because they are in `src/KBot.App/Help/`):
`HelpService.StartInitialTour` / `InitialTourSeen` / `InitialTourId`, `HelpTourRunner` (`initial`,
`_completed`), `HelpTourBubble` (`chkNuMaiArata`, `ShowNeverAgain`, `NeverAgain`).

Left out on purpose (HELP_SYSTEM §5): how K-BOT recognises a confirmation window, how the dates
are read from the page, the manual «▶ Începe / ■ Gata / ✕» buttons of the mini menu.

## Files touched

- `src/KBot.App/HelpContent/contabil/fereastra.md`, `ajutor.md`, `setari.md`, `autentificare.md`,
  `ddf/nou.md`, `forexe/browser.md`
- `src/KBot.App/HelpContent/tours/tur-fereastra.md`, `tur-ddf.md`, `tur-setari.md`
- `src/KBot.App/HelpContent/README.md`, `help-version.txt`
- `docs/HELP_SYSTEM.md`

## Test results

- `tools\HelpCheck\Check-Help.ps1 -Coverage`: «No errors.», 48 topics, 17 tours, 57 capture tags.
  Coverage names only `RobotQueueForm` (0098, already in the open threads).
- `dotnet build src/KBot.App/KBot.App.vbproj`: **0 warnings, 0 errors**.
- Nothing seen on screen.

## Capturi de refăcut / noi

- To re-shoot when they exist: `setari` (the page «Aplicație» has two more boxes),
  `conectare-fereastra` (the bar has no «?» now).
- No new capture tags. The FOREXE-page sections (the same-date box, the mini menu) have none: the
  capture tool does not read the FOREXE page, and the box appears only in a real situation.

## De citit de operator

- `contabil.forexe.browser`, «Întrebările «Sunteți sigur...?» din pagină»: written from what the
  code does, but **the FOREXE windows themselves were not seen** — the example («la «Renunță»»)
  comes from the comments in `ForexeWatch.js`. If on a client PC a question stays on screen, the
  sentence must be narrowed (and the rule in the script widened).
- `contabil.forexe.browser`, «Două recepții pe aceeași dată»: «după ce ieși din câmpul datei» is
  what the script asks for; whether FOREXE's date picker lets it happen there was not seen. The
  question at «Salvează» is certain.
- `contabil.forexe.browser`, «Mini-meniul K-BOT din pagină»: the first description of the mini
  menu in the help; it names only the zoom buttons and the state line.

## Left unverified or deferred

- The 0098 note in the open threads («Coada robotului» in four topics + a capture) is still open —
  not part of this pass.
