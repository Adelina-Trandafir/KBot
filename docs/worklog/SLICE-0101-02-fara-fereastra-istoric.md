# SLICE-0101-02 — No «Istoric angajament» window after a save in the FOREXE page (operator request, 02.10.2026)

Branch: `SLICE-0100-Multithreading` (nothing committed).

## What changed and why

After K-BOT took over a save made in the FOREXE page, it opened the «Istoric angajament» window (the history rows FOREXE
wrote in the operator's minutes). Since 28.09.2026 it no longer opened after a reception; the operator now says it adds
nothing and only breaks the flow, for any operation.

- `KbotForm.ForexeWatch.vb`: the call `DeschideIstoricInterval(...)` after each take-over is gone, and so is the
  `DeschideIstoricInterval` method and the now-unused `deLa` / `panaLa` locals.
- **`IstoricIntervalForm` (+ Designer) is deliberately KEPT** in `src/KBot.App/Forexe/`, unreferenced - the operator asked to
  remove only the references, not the form.
- Help: `contabil/forexe/browser.md` - the paragraph about the window and its capture `istoric-interval` are gone, as is
  «și istoricul» from the receptions row and `IstoricIntervalForm` from `screens:`. No picture file existed for it.

## Test results

No tests written or run (operator: no tests). `dotnet build src/KBot.App` -> 0 warnings, 0 errors. The app was not run.
