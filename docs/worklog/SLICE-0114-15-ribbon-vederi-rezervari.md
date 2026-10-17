# SLICE-0114-15 - Rezervari view: Grafice / document / Repara in the ribbon, tree header and footer emptied (operator items 1, 2) (17.10.2026)

## What changed and why
New interface `IViewRibbon` (+ `ViewButtonState`: Hidden / Dimmed / Enabled) in `Views`: a view of the shell that wants buttons in the ribbon's VIEW group says which keys are its, their state and text, and what a press does; `RibbonStateChanged` tells the window to ask again. `KbotForm` hooks the event of every view that implements it (same path as «Asociaza parteneri») and exposes `ActiveViewRibbon` / `RunViewRibbonButton`; `AvacontMain.UpdateViewGroup` (replaces `UpdateSumarPartnersButton`) shows / dims / hides the buttons, sets their text and the group caption («Sumar», «Rezervari», «Receptii»). `RezervariView` implements it: «Grafice» (the chart, was the header-right icon; dimmed with no reservations), the document button (was the footer-left menu; takes the name of the option, Adauga rezervare / Definitiveaza / Deruleaza / Genereaza PDF final; hidden while a leaf still has the «+» or there is no option), «Repara...» (was the footer-right icon; only when the angajament has broken reservations). The tree of the view has no header or footer icon left (designer: header-right, footer-left, footer-right icons and their tooltips, and the footer caption, removed; the leaf «+» stays).

## Files touched
`src/KBot.App/Views/IViewRibbon.vb` (new), `Views/RezervariView.vb`, `RezervariView.Designer.vb`, `KbotForm.Views.vb`, `AvacontMain.vb`, `AvacontMain.Designer.vb` (three buttons in the view group).

## Test results
`dotnet build src/KBot.App/KBot.App.vbproj`: 0 warnings, 0 errors. Nothing run, nothing seen on screen.

## Unverified / deferred
- Not seen on screen: the new buttons in the view group, the popup of the document button (opens at the mouse), the fields in the ribbon, the tree rows of the issued invoices.
