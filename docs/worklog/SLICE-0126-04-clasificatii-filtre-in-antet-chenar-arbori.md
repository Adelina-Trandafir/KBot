# SLICE-0126-04 - Clasificatii: filtrele in iconita din antetul arborelui, chenar si culori de tema pentru arbori (07.11.2026)

Operator, 07.11.2026, din captura de ecran a ferestrei «Clasificatii cheltuieli». Numar de felie: urmatorul liber din 0126; cererea nu a numit o felie - de confirmat.

## What changed and why
1. **Bifele mutate in iconita din dreapta a antetului arborelui**, intr-un `CustomPopup`, ca la arborele principal (`KbotForm.TreeOptions.vb`). «Arata toate clasificatiile» si «Arata DOAR clasificatiile folosite in FOREXE» sunt doua randuri bifabile ale popup-ului (`tree.HeaderRightIcon` = `settings__1_`, tooltip in designer; `Tree_HeaderRightIconClicked` cu garda `CustomPopup.ClosedJustNow`). Starea sta in doua campuri (`_showAll`, `_onlyForexe`) in loc de doua `CheckBox`; regulile raman (FOREXE are prioritate, «+» din subsolul arborelui pune `_showAll`). Scoase din designer: `tlyBife`, `chkToate`, `chkForexe`; `tlyBody` are un singur rand, arborele ia toata inaltimea.
2. **Arborii din Clasificatii (Cheltuieli si Venituri) fara culori fixate si cu chenar:**
   - Din designere am scos `SearchBackColor = SystemColors.Control`, `SearchSeparatorColor = SystemColors.ActiveBorder`, `SearchSeparatorWidth = 2` (culori puse de mana, care bateau tema: banda de cautare ramanea gri chiar sub alta schema).
   - Chenar: arborele nu are chenar implicit (`BorderColor` = Transparent), pe cand tabelele de alaturi au (`KBotDataView.BorderColorResolved` = `Palette.BorderColor`). Clasa noua `ClasificatiiTreeStyle.ApplyFrame` pune arborelui aceeasi culoare de chenar, la fiecare schimbare de tema, din `ApplyTheme` al ambelor vederi. Nu am schimbat `AdvancedTreeControl` (ar fi atins toti arborii aplicatiei).
3. **Ajutor** (`0126-04`): `clasificatii.md` (filtrele din iconita), tutorialul `clasificatii-adaugare.md` (pasul «Bifele de sub arbore» -> «Filtrele arborelui», `target: ClasificatiiBugetView.tree`, `part: header.right`).

## Files touched
`src/KBot.App/KBotForm/Views/Nomenclatoare/ClasificatiiBugetView.vb`, `.Designer.vb`, `ClasificatiiTreeStyle.vb` (nou); `src/KBot.App/Avacont/Configurari/ClasificatiiVenituri/Views/ClasificatiiVenituriView.vb`, `.Designer.vb`; ajutor: `HelpContent/contabil/nomenclatoare/clasificatii.md`, `HelpContent/tutorials/clasificatii-adaugare.md`; `docs/worklog/state/KBOT_STATUS_0120-0129.md`, `docs/worklog/KBOT_STATUS.md`. FileVersion `KBot.App` nebumpat.

## Test results
Compilare `KBot.App` (Debug, in alt director de iesire, fiindca aplicatia ruleaza si tine fisierele): 0 erori, 0 avertismente. Nimic rulat, nici un test.

## Unverified / deferred
- **Nevazut pe ecran.** Interpretarea punctului 2 e a mea: «nu respecta culorile sistemului» = culorile fixate in designer la banda de cautare; «par ca nu au border» = lipsa chenarului. Daca operatorul se referea la altceva (de ex. culoarea fundalului antetului), de spus.
- Iconita `settings__1_` si cele doua pictograme de rand (`cells`) sunt alese de mine, ca la arborele principal.
- Pasul de tutorial cu `part: header.right` nerulat. Captura `clasificatii` din ajutor nerefacuta.
- Ceilalti arbori ai aplicatiei raman fara chenar (nu au fost ceruti).
