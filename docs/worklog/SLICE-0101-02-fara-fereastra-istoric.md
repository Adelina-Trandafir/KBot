# SLICE-0101-02 — No «Istoric angajament» window after a save in the FOREXE page (operator request, 02.10.2026)

Branch: `SLICE-0100-Multithreading` (nothing committed).

## What changed and why

Taking over a FOREXE save opened «Istoric angajament» (history rows FOREXE wrote in the operator's minutes). Reception saves stopped opening it on 28.09.2026; the operator now asks removal for all operations: no added value, interrupted flow.

- `KbotForm.ForexeWatch.vb`: removed the post-takeover `DeschideIstoricInterval(...)` call, the method and unused `deLa`/`panaLa` locals.
- **`IstoricIntervalForm` (+ Designer) is deliberately KEPT** in `src/KBot.App/Forexe/`, unreferenced - the operator asked to remove only the references, not the form.
- Help: `contabil/forexe/browser.md` - the paragraph about the window and its capture `istoric-interval` are gone, as is «și istoricul» from the receptions row and `IstoricIntervalForm` from `screens:`. No picture file existed for it.

## Test results

No tests written or run (operator: no tests). `dotnet build src/KBot.App` -> 0 warnings, 0 errors. The app was not run.
