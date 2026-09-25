# SLICE 0080-03 — the «Extrase de cont» window (24.09.2026)

## Request (operator, 24.09.2026)

A standalone window with the same tree for EVERY statement of the database — headers with or
without an angajament, operations with CodContract, only CodContract, or neither (those stand
for something else). The grids show everything the view shows plus CodAngajament / Indicator and,
at the end, Explicații; the detail adds CodAngajament, Indicator, ReferintaDest, CodProgram (not
CodPartener). Modal (popup) with a maximize button. It opens from the left icon of the main
tree's footer, in place of the direct download, and has a real download button in its footer.
The date search in the tree: ignored for now.

## What changed

- `Views/ExtraseForm` (new, Designer + code): `KBotShellForm` (resizable, 1px outline),
  `KBotCaptionBar` with `ShowMaximize`, `ExtrasePanel` in `Toate` mode (footer icon off), footer
  with «Descarcă extrasele din FOREXE», a status line and «Închide»; Escape closes. Loads
  `GET /api/forexe/extrase/lista` without `cod` through the shell's re-login net; reloads after
  a download that imported something.
- `KbotForm.Tree_FooterLeftIconClicked` now opens the window modal (tooltip changed in the
  designer); after it closes, an open Extrase view reloads. The download itself is
  `DescarcaExtraseAsync(owner)` (0080-02), so its messages sit over the window.
- Columns: the window's own two layouts from «Setări → Extrase» (defaults: headers as in the
  view; operations add Clasificație, Cod angajament, Indicator, Explicații last).

## Files touched
`src/KBot.App/Views/ExtraseForm(.Designer).vb` (new), `src/KBot.App/KbotForm(.Designer).vb`.

## Test results
`dotnet build src\KBot.App\KBot.App.vbproj --no-incremental`: 0 errors, 0 warnings. No test
run. The panel in `Toate` mode was rendered with `DrawToBitmap` (see 0080-02); **the window
itself (caption bar, footer, maximize) was not rendered.**

## Unverified / deferred
- Window never opened for real; download from it never run.
- The tree's built-in search band is there (header search icon); the date-picker search the
  operator mentioned is deferred by the operator.
- The shell's busy bar runs under the modal window during a download; the window shows a wait
  cursor and disables its button instead.
