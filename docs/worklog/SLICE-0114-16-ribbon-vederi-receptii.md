# SLICE-0114-16 - Receptii view: Asociaza receptii / Reface din istoric in the ribbon, only «Actualizeaza» left in the tree (operator items 3, 4) (17.10.2026)

## What changed and why
`ReceptiiView` implements `IViewRibbon`: «Asociaza receptii» (was the header-right icon; there while the view has an angajament) and «Reface din istoric...» (was the footer-left icon; now only when the angajament has errors to repair = a reception whose chain does not close, `AngajamentTreeInfo.LantNeinchis`, the same list that paints its row red in the main tree). The tree keeps only the footer-right «Actualizeaza». The view now keeps the tree info of the angajament (`_info`).

## Files touched
`src/KBot.App/Views/ReceptiiView.vb`, `ReceptiiView.Designer.vb`, `AvacontMain.Designer.vb` (two buttons in the view group).

## Test results
`dotnet build src/KBot.App/KBot.App.vbproj`: 0 warnings, 0 errors. Nothing run, nothing seen on screen.

## Unverified / deferred
- Not seen on screen: the new buttons in the view group, the popup of the document button (opens at the mouse), the fields in the ribbon, the tree rows of the issued invoices.
