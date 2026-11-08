# SLICE-0130-01 - Borduri linistite in Classic si Modern (08.11.2026)

## Request
Operator, 08.11.2026 (Dark ramane neschimbat):
1. Toate controalele, Classic si Modern: chenarul exterior = ActiveBorder, daca nu e fixat manual (de verificat ca nu e fixat nicaieri).
2. DGV, arbore: separatoarele de HEADER si FOOTER = ActiveBorder in Classic, albastrul actual in Modern, daca nu sunt fixate manual.
3. DGV: marginile interioare (celule, antete) = InactiveBorder in Classic si Modern.
4. NavList: linia de subsol = ActiveBorder in Classic, albastrul actual in Modern.

## What changed and why
- `ThemeScheme` (KBot.Theming): `UsesQuietBorders()` (Classic sau Modern, nu Dark), `OuterBorder(fallback)` = ActiveBorder, `InnerLine(fallback)` = InactiveBorder; in rest iau `fallback` (culoarea de pana acum).
- Chenar exterior din tema: `KBotTextBox`, `KBotTextField`, `KBotComboBox`, `KBotDatePicker`, `KBotCalendar`, `KBotLabel`, `KBotTableLayoutPanel`, editorul din `KBotRichTextEditor`, `KBotDataView` (camp nou `_cOuterBorder`; inainte chenarul lua culoarea antetului), `AdvancedTreeControl` (inainte fara chenar din tema; acum ActiveBorder in Classic / Modern; bara de derulare se reaseaza cand chenarul apare).
- Separatoare antet / subsol: `KBotDataView` si `AdvancedTreeControl` Classic = ActiveBorder (inainte Gainsboro); Modern = accentul, neschimbat. `KBotNavList` linia de subsol Classic = ActiveBorder (Modern = accent, neschimbat).
- DGV, linii interioare (`_cGridLine`, separatoarele verticale de antet / subsol): `InnerLine` = InactiveBorder (inainte `Border` din paleta; in Modern era #E2E2E2).
- Pinuri manuale scoase: `tree.BorderColor = InactiveBorder` (`PrimiteView`), `Header/FooterColumnSeparatorColor = Gainsboro` (9 designere de grile: Director x2, Istoric, OrdVizualizare, Plati, Receptii, Rezervari, Sumar, DdfValori), `tree.HeaderSeparatorColor = Gainsboro` (`XmlViz`). `ClasificatiiTreeStyle` si `ParteneriForm` pun acum chenarul arborelui prin `OuterBorder` (nu paleta).
- `CLAUDE.md`: regula benzilor DGV / arbore actualizata.

## Files touched
`KBot.Theming/ThemeScheme.vb`; `KBot.Controls`: `TextField/KBotTextBox.vb`, `TextField/KBotTextField.vb`, `Combo/KBotComboBox.vb`, `Calendar/KBotDatePicker.vb`, `Calendar/KBotCalendar.vb`, `Label/KBotLabel.vb`, `Table/KBotTableLayoutPanel.vb`, `RichText/KBotRichTextEditor.Theming.vb`, `DataView/KBotDataView.Theming.vb`, `Tree/AdvancedTreeControl.Theming.vb`, `NavList/KBotNavList.vb`, `Xml/XmlViz.Designer.vb`; `KBot.App`: 8 designere de grile, `ClasificatiiTreeStyle.vb`, `ParteneriForm.vb`; `KBot.EFactura/Views/Primite/PrimiteView.Designer.vb`; `CLAUDE.md`.

## Test results
Niciun test scris sau rulat. `dotnet build KBot.sln`: 0 erori; singurul avertisment (BC42108 in `AVC.Migrator/AvcOperatiiStep.vb`) nu e din aceasta felie.

## Unverified / deferred
- Nimic vazut pe ecran, in nicio tema. ActiveBorder (#F4F7FC) e foarte deschis: chenarele pot parea aproape invizibile.
- Nefacute: `KBotButton` / `ModernRenderer` si `KBotCheckBox` (butoane si bife, au lucrare proprie), alte controale cu chenar (`KBotRibbon`, `KBotSidePanel`, popup-uri, tooltip-uri).
- Raman fixate intentionat: `SearchSeparatorColor = InactiveBorder` pe arbori, `edtLunga.Header/FooterSeparatorColor` (`DdfEditDescrierePage`), `benzi.HeaderSeparatorColor` (`KBotLaneView`), `tree.ColumnHeaderSeparatorColor = Gainsboro` (`KbotForm`), `CellBorderColor = Transparent` pe tabele, `lblInfo.BorderColor = Transparent`.
- Ajutor: capturile cu grile / campuri / arbori pot diferi usor (nota in «Ajutor de actualizat», 0000-0009).

## Revizie (08.11.2026)
Operatorul: InactiveBorder (#F4F7FC) e mai deschis decat ActiveBorder (#B4B4B4), iar chenarele exterioare si separatoarele de antet / subsol trebuie sa fie mai inchise decat liniile interioare. Culorile din aceasta felie s-au inversat: exterior + separatoare antet / subsol (Classic) + subsol NavList (Classic) = ActiveBorder; linii interioare DGV = InactiveBorder. Textul de mai sus e deja dat pe varianta finala.

## Revizie 2 (08.11.2026)
InactiveBorder (#F4F7FC) prea deschis pentru liniile interioare ale grilelor. Inlocuit cu `ThemeScheme.SoftBorder` = #D2D2D2 (ActiveBorder 40% spre alb). Liniile interioare DGV (`InnerLine`) il folosesc; restul regulii neschimbat.
