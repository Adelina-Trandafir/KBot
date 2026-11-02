# SLICE-00EF-53 - A saved DDF refreshes the «efactura» entry of the navigation (31.10.2026, operator request)

## What changed and why
- Operator: when a DDF with an associated partner is saved, the navigation list of the main window must refresh so the associated e-invoices show up, if there are any.
- Cause (read in code): the «efactura» entry is visible when `AreDDF AndAlso ArePrimite` (`ApplyViewGating`). `ArePrimite` is computed by the server in the tree query (`routes/forexe/tree.py`, `_ARE_PRIMITE`: the document's partners matched on tax code against `EF_Primite`, or an `EF_PrimiteAsocieri` row) and is read only when the tree loads. `DupaScriereaDdf` refreshed `AreDDF` locally but never `ArePrimite`, so a partner saved on the document did not show the entry until the next tree reload.
- Fix: `DupaScriereaDdf` now calls the new `ReimprospateazaEFacturiDdf(cod)` (`KbotForm.DdfDelete.vb`). It asks the server for the tree again (same year / SS / hidden flag as `LoadTreeAsync`), copies ONLY `ArePrimite` of that angajament onto the selected node and re-applies `ApplyViewGating`. Skipped when the selected node is another angajament than the one written, when the document is gone (`AreDDF` false) or when the operator moved to another node while the tree was coming.
- Why not `LoadTreeAsync(pastreazaSelectia:=True)`: its `PopulateTree` runs `SetContext` on the open DDF view, which starts a second load racing the `Reincarca(idrevSalvat)` that lands the view on the revision just saved.
- No server change: the existing `GET /api/forexe/tree` is used. Also runs after the three DDF deletes (same `DupaScriereaDdf`), where a removed partner can make the entry go away.
- Help: `contabil/efactura/index.md`, «Facturile primite» (tags 00EF-53, 0000-143).

## Files touched
`src/KBot.App/KbotForm.DdfDelete.vb`; `src/KBot.App/HelpContent/contabil/efactura/index.md`; this worklog; `state/KBOT_STATUS_0000-0009.md`; `KBOT_STATUS.md`.

## Test results
`dotnet build src/KBot.App/KBot.App.vbproj`: succeeded, 0 warnings, 0 errors. No tests run (project rule).

## Unverified / deferred
Not run, not seen on screen; the server was not called. Assumption: «DDF with associated partner» = a partner saved in the document's partners (`FX_DDF_Parteneri`) or an association in `EF_PrimiteAsocieri`, as `_ARE_PRIMITE` reads them. The refresh costs one extra tree request per DDF save/delete. `Check-Help.ps1` not run.
