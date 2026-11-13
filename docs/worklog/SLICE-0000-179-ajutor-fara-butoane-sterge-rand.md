# SLICE-0000-179 — Help: no more «Sterge randul» buttons (ORD_EDIT documents, Setari › Pagina FOREXE), 10.11.2026

## What changed and why
The operator's rule (10.11.2026, «NICIODATA PE NICIUN FORM»): no button that deletes a selected row exists on any form; rows are deleted with the standard DGV delete column (`ButtonKind.Delete`, the ✕). After the page «Facturi si plati» (see `SLICE-AV02-04-facturi-plati.md`) the operator approved taking out the two other «Sterge randul» buttons:
- **`ORD_EDIT/OrdDocumentePage`** (editor of the ordonantare, documents): the button «Sterge randul» is gone; `grdText` got the delete column. The button «Sterge fisierul» STAYS: the operator's order was only about «Sterge randul» («sub nicio forma»), so it was put back exactly as it was. A defect of the old page disappeared with the removed button: the handler of «Sterge randul» was declared `Handles btnAdaugaText.Click`, so «Adauga rand» also deleted the current row right after adding.
- **`Setari/SetariPaginaView`** (Setari › Pagina FOREXE): the button «Sterge» is gone; the grid of rules got the delete column.
Help: `contabil/vederi/ord-editor.md` (section «Documente justificative») and `avansat/pagina.md` (section «Lista regulilor», the bullet «Sterge» removed), tags `0000-179`.

## Files touched
`src/KBot.App/ORD_EDIT/OrdDocumentePage.vb` + `.Designer.vb`; `src/KBot.App/Setari/Views/SetariPaginaView.vb` + `.Designer.vb`; the two help topics above; `docs/worklog/state/KBOT_STATUS_0000-0009.md` (row 0000-179).

## Test results
None run (no tests by operator rule). KBot.App builds Debug with 0 errors and 0 warnings (into a scratch folder). `tools\HelpCheck\Check-Help.ps1` NOT run.

## Unverified / deferred (de citit de operator)
- Nothing seen on screen: both pages were never opened. In the ordonantare editor the delete column sits at the end of a read-only grid on the «< TOTI BENEFICIARII >» row; the `Button` column is active in a read-only grid by design of the control (`KBotDataView.Input.vb`), not checked live.
- The captures `ord-editor` and `avansat-pagina` show the old buttons: to re-shoot.
- NOT touched and NOT to be touched without a new order (operator: only «Sterge randul» was meant): the other delete buttons, e.g. `Forexe/Helpers/RecorderForm` («Sterge pasul»), `ORD_EDIT/OrdDocumentePage` («Sterge fisierul»), `DDF_EDIT/DdfEditFisierePage` («Sterge fisierul»), `ORD_EDIT/OrdAtasamentePage` («Sterge»), `Tutorial/TutorialDesignerForm` («Sterge», «Sterge tutorialul»), `Avacont/Configurari/PlanCont/Views/PlanContConturiView` («Stergere»), `EFactura/Views/Vanzare/VanzareCumparatorPage` («Sterge clientul»), `Views/Comune/Ddf/DdfPartnersView` («Scoate din asociere»), `Setari/Views/SetariForexeView` («Sterge ultima luna din istoric...»), `Views/Comune/LogClearDialog`; ribbon commands in `AvacontMain` («Sterge plata», «Sterge operatia», «Sterge documentul», «Ștergere» de parteneri, «Sterge clasificatia») are ribbon commands, not form buttons, and were not counted.
