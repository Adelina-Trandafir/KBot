# SLICE-0128-03 - Butonul «Creeaza receptia» in grupul din dreapta al ribbon-ului (05.11.2026)

Operator: butonul «Creeaza receptia» sa fie in ribbon-right.

## Starea finala
Butonul `ef_receptie_actiune` («Creeaza receptia» / «Leaga» / «Modifica receptia», pictograma `plus_green`) sta in `ribbon.RightGroup`, ascuns implicit; `AvacontMain.UpdateViewGroup` il arata cand pagina pe ecran spune ca vederea «Receptii» e deschisa, aprins sau stins dupa starea ei, cu textul ei, iar legenda grupului e «Receptii». Clicul ruleaza actiunea vederii. Dupa KE21 lantul e `PrimiteForm` -> `PrimiteView` -> `KeReceptiiView`; in fereastra principala butonul vine prin `IViewRibbon` din `PrimiteAngajamentView` (`KBot.EFactura` nu poate implementa `IViewRibbon`, care sta in `KBot.App`, de aceea in formular cablajul e direct, ca la «Sincronizeaza facturi primite»).

Fisiere: `AvacontMain.vb`, `AvacontMain.Designer.vb`, `PrimiteForm.vb`, `PrimiteView.Receptii.vb`. Fara teste. Rolul «Director»: butonul nu e in `RibbonKeys`, deci nu apare; neverificat.
