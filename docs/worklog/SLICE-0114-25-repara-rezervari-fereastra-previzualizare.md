# SLICE-0114-25 - Repara rezervari: modal preview of what was detected and what will be done (19.10.2026)

## What changed and why
Operator: on the «Repara...» button of the Rezervari view, show a modal popup with what was actually detected (the mistakes) and what the repair is about to do. Until now the dry run ended in a message box with a sentence and eight examples.

- New window `RezervariReparaForm` (`KBot.App/Views/`, Designer + code behind, inherits `KBotThemedForm`, same card / caption bar / bottom bar shape as `CorectieValoareForm`). Two read-only `KBotDataView` grids and texts:
  - **Ce s-a detectat**: the broken reservations the main tree already knows (`AngajamentTreeInfo.RezervariGresite`: day, indicator, «anterioara acum», «ar trebui sa fie»), plus the dry-run counts (history rows with wrong order / type / difference, reservations with a wrong value).
  - **Ce se va face**: every reservation the repair rewrites (`RezervariReanalizaResult.Details`: day, indicator, value old -> new, previous old -> new), a sentence listing the three steps (correct history rows, rewrite the reservations, reload the tree), and a read-only box with «nothing is downloaded from FOREXE», the single-transaction note and the server's warnings (incl. «first 60 of N»).
  - Buttons: «Renunta» left (`DialogResult.Cancel`), «Repara» right (`DialogResult.OK`) -- the button-sides rule. No default button on «Repara».
- `RezervariView`: the `repara` callback is now `Action(Of String, IReadOnlyList(Of RezervareGresita))`; the view passes the list of its current `_info`.
- `KbotForm.Ddf.vb` (`ReparaRezervari` / `ReanalizeazaRezervariAsync`): after the dry run, opens the window with `ShowDialog(Me)` instead of the Yes/No message box; `OK` goes on to the real write (unchanged), anything else returns with nothing written. The «nothing to do» message box is unchanged. Server change only for the revision-months case (below).

- **Revision months (operator, 19.10.2026): the window opens anyway.** Before, the server refused the dry run too when the repair would touch a month with DDF revisions (`RevizieBlocata`, 409), so the window never appeared. Now `reanalizeaza_rezervari` raises it only when `aplica` is true: the dry run returns the full preview, and the refusal arrives when the operator presses «Repara» (the real call), shown by the existing `ReparaRezervari` error path (`ApiException` message box), same text as before. Server file: `PYTHON/routes/forexe/rezervari_reanaliza.py` (docstring + one condition). **Needs a push to the server** (`py_compile` ok; no tests exist for the route; not run against MariaDB).

## Files touched
`src/KBot.App/Views/RezervariReparaForm.vb`, `RezervariReparaForm.Designer.vb` (new); `src/KBot.App/Views/RezervariView.vb`; `src/KBot.App/KbotForm.Ddf.vb`; `PYTHON/routes/forexe/rezervari_reanaliza.py`. Help `contabil/vederi/rezervari.md`, `tours/tur-rezervari.md` (0000-110).

## Test results
`dotnet build src/KBot.App/KBot.App.vbproj`: 0 warnings, 0 errors. Not run, not seen on screen (no tests asked for, no live server here).

## Unverified / deferred
- The window was never opened: grid column widths, the 40/60 row split and the text lengths are drawn from numbers, not seen.
- The dry run is capped by the server at 60 detail rows (`_MAX_DETALII`); beyond that the grid shows the first 60 and the warning says so. History rows to correct are shown as a count only (the route does not itemise them).
- The window is not a «macheta» (not recorded in the history); it is a plain dialog like `CorectieValoareForm`.
