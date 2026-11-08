# SLICE-00EF-65 — Primite* devine FacturiCumparare* in KBot.EFactura (08.11.2026)

Continuare a mutarii FacturiForm -> FacturiVanzareForm / PrimiteForm -> FacturiCumparareForm (cererea operatorului: «tot ce e Primite trebuie redenumit in FacturiCumparare»).

## What changed and why
Names only, no behaviour. Folder `Views/Primite` -> `Views/FacturiCumparare`; every `PrimiteX` file/class -> `FacturiCumparareX` (View with all its partials, DetaliiView, the *Page views, Receptii*View); `SincronizarePrimiteForm` -> `SincronizareFacturiCumparareForm`; `PrimiteAngajamentView` (KbotForm.EFactura.vb) -> `FacturiCumparareAngajamentView`; control `primite` -> `facturiCumparare` and `Name` fixed in FacturiCumparareForm; `OpenPrimite` -> `OpenFacturiCumparare`; `_primiteForm` -> `_facturiCumparareForm`; menu key `listaprimite` -> `listafacturicumparare`; `.vbproj` entries and `DependentUpon` paths.
Left on purpose: operator-visible texts («Facturi primite»), DB tables `EF_Primite*`, server routes, and the API/Domain contract names (`SyncPrimiteAsync`, `GetPrimiteAsync`, `ArePrimite`, `IdPrimite`, `AfiseazaPrimiteNoi`), App-side `SyncPrimite` / `RunPrimiteAccess` / `ef_primite_*` keys, `FacturaPrimitaPdf`.

## Files touched
`src/KBot.EFactura/` (Forms/, Views/FacturiCumparare/, Views/Receptii/KeReceptiiView*, KBot.EFactura.vbproj); `src/KBot.App/KBotForm/Integrations/KbotForm.EFactura.vb`, `src/KBot.App/KBotForm/Navigation/KbotForm.Views.vb`; CLAUDE.md (example file name).

## Test results
`dotnet build KBot.sln -c Debug`: 0 errors, 0 warnings. No tests written or run; app not started.

## Unverified / deferred
Old worklogs and `docs/deployment/pending/*.json` still name the old paths. Help `contabil/efactura/index.md` still lists `PrimiteForm` in its screens line (also stale: `FacturiForm`); no visual change, so not edited. API/Domain/App renames need the server side too, not done.
