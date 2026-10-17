# SLICE-0114-23 - E-Factura: no bottom padding on the sub-views, narrower trees, «Atasamente» folded into «Continut» (17.10.2026)

## What changed and why
Operator, five points.
1. **Bottom padding of the sub-views** (the 2-3 px strip under the XML / content views, in the view of the angajament and in the window «Facturi primite»): the pages `PrimiteLiniiPage`, `PrimiteMesajePage`, `PrimiteReceptiiPage` and the XML view (`pgXml`) had `Padding(3)`; now `Padding(3, 3, 3, 0)` (left, top and right stay, the bottom goes), so the content reaches the bottom of the tree's frame.
2. **«Facturi primite» (the control, so the view and the window): the tree 25 % narrower**: the column split 32 / 68 became 24 / 76.
3. **Vanzare (`FacturiForm`): tree 25 % narrower**: the body split 30 / 70 became 22.5 / 77.5 (the footer buttons' split follows, so the footer stays under the same columns).
4. **Vanzare: the view «Atasamente» is gone** (nav item, `pgAtasamente`, the class `VanzareAtasamentePage` and its two files deleted).
5. **The tick «Ataseaza factura originala»** is now the footer row of the view «Continut» (`VanzareContinutPage`, a third row under the grid); `FacturiForm` binds `chkAtasament` to it, nothing else in its logic changed (saved with the invoice, locked outside edit mode as before).

## Files touched
`src/KBot.EFactura/Views/Primite/PrimiteView.Designer.vb`, `PrimiteLiniiPage.Designer.vb`, `PrimiteMesajePage.Designer.vb`, `PrimiteReceptiiPage.Designer.vb`; `FacturiForm.Designer.vb`, `FacturiForm.vb`, `FacturiForm.Pagini.vb`, `FacturiForm.Pdf.vb`; `Views/Vanzare/VanzareContinutPage.Designer.vb`, `VanzareContinutPage.vb`; deleted `Views/Vanzare/VanzareAtasamentePage.vb` and `.Designer.vb`. Help `contabil/efactura/index.md` (0000-94).

## Test results
`dotnet build KBot.sln`: 0 warnings, 0 errors. Nothing run, nothing seen on screen.

## Unverified / deferred
- Not seen on screen: whether the 3 px strip is really gone in both places (the page padding was the only source found; if a strip stays, it is the card around the view), the tick's place and look in the footer of «Continut», the narrower trees (row text is cut earlier now; the width 0 = dynamic text still shows what fits).
- The other Vanzare pages keep their 6 px padding all round (not asked).
- Captures: `efactura-factura-noua` and the ones showing the «Atasamente» tab (if any) were not marked for redo.
