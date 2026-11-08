# SLICE-0126-03 - Clasificatii: butoanele in ribbon, angajamente fara bara, rectificari mai joase (07.11.2026)

Operator, 07.11.2026, fereastra «Clasificatii cheltuieli» (pagina Cheltuieli), din captura de ecran. Numar de felie: urmatorul liber din 0126 (fereastra e din 0087 / 0126-01); cererea nu a numit o felie - de confirmat.

## What changed and why
1. **Butoanele in ribbon-right.** «Salveaza», «Verifica bugetul» si «Trimite in Access» nu mai sunt in banda de jos a paginii; sunt butoane ale grupului din dreapta al ribbonului din `AvacontMain` (`ribbon.RightGroup`, grupul «Clasificatii»), ca «Plan de conturi» (`plan_cont_*`). Cheile: `clasificatii_salveaza`, `clasificatii_verifica`, `clasificatii_access` (facute in designerul `AvacontMain`, `Visible = False`, tooltipurile mutate acolo; adaugate in `ViewGroupKeys` si in `Case` din `Ribbon_ItemClicked`).
   - `ClasificatiiForm` implementeaza acum `IViewRibbon` (titlul grupului «Clasificatii»; cheile paginii din fata, vezi punctul 5) si trece mai departe evenimentul de stare.
   - `ClasificatiiBugetView` nu mai are cele trei `Button`-uri: `RibbonButtonState` (Salveaza activ cand sunt modificari nesalvate si nimeni nu lucreaza; Verifica activ cand nu e ocupat; Access ascuns fara componenta Access, activ doar fara modificari nesalvate si cu o clasificatie aleasa) si `RunRibbonButton`; `SetDirty` / `SetBusy` ridica `RibbonStateChanged` in loc sa stinga/aprinda butoane. Munca (`RunSave` / `RunCheck` / `RunSendAccess`) e cea de pana acum, doar redenumita.
   - Banda de jos a paginii a ramas doar cu linia de stare (`lblStare`), 78 -> 45 la 144 dpi.
2. **Tabelul «Angajamente»** isi pastreaza 3 randuri + antet + subsol fara bara de derulare: marea parte vine din (3) si din banda de jos mai mica.
3. **Rectificari**: `CorrectionsRowsFit` 6 -> 2 (plus randul gol = 3 linii vizibile, era 7). Inaltimea din designer a randului (330 -> 204) urmeaza.
4. **Ajutor** (`0126-03`): `clasificatii.md`, `verificare-buget.md`, `tur-clasificatii.md`, `tutorials/clasificatii-buget.md`, `tutorials/clasificatii-rectificare.md` - butoanele sunt «din ribbon», pasii cu `target: ClasificatiiBugetView.btn*` au devenit `anchor: ribbon.clasificatii_*`; la rectificari pasul «Salveaza» a devenit manual (fara `guard`, care cere `wait: click`, deci un control real), cu textul «aici nu salvam nimic».

5. **Pagina Venituri** (la cererea operatorului, dupa prima parte): «Salveaza» foloseste acelasi buton `clasificatii_salveaza`, iar «Sterge clasificatia» e butonul nou `clasificatii_sterge` (`minus_red`). `ClasificatiiVenituriView` are acum `RibbonStateChanged` / `RibbonButtonState` / `RunRibbonButton`; fara butoane jos, banda ei de jos 78 -> 45. `ClasificatiiForm` raspunde cu pagina din fata pentru ambele pagini. Ajutorul `clasificatii.md` (sectiunile Venituri si Stergere) actualizat.

## Files touched
`src/KBot.App/Avacont/AvacontMain.Designer.vb`, `AvacontMain.vb`; `src/KBot.App/KBotForm/Views/Nomenclatoare/ClasificatiiBugetView.vb`, `.Designer.vb`, `ClasificatiiForm.vb`, `Avacont/Configurari/ClasificatiiVenituri/Views/ClasificatiiVenituriView.vb`, `.Designer.vb`; ajutor: `HelpContent/contabil/nomenclatoare/clasificatii.md`, `verificare-buget.md`, `HelpContent/tours/tur-clasificatii.md`, `HelpContent/tutorials/clasificatii-buget.md`, `clasificatii-rectificare.md`; `docs/worklog/state/KBOT_STATUS_0120-0129.md`, `docs/worklog/KBOT_STATUS.md`. FileVersion al `KBot.App` NEbumpat (se alege la `push-update.ps1`).

## Test results
`dotnet build src\KBot.App\KBot.App.vbproj -c Debug`: 0 avertismente, 0 erori. `Check-Help.ps1`: nimic nou pe fisierele atinse (avertismentele ramase sunt mai vechi: `0000-47`, `0000-20`, `IKBotTutorialHost`). Nimic rulat, nici un test.

## Unverified / deferred
- Pagina Venituri: butoanele din ribbon (si icoana `minus_red` aleasa de mine) nevazute pe ecran.
- **Nevazut pe ecran.** Nu s-a verificat: ca butoanele apar in ribbon cu pictogramele alese (`save_32`, `Fatcow_Farm_Fresh_Check_boxes_32`, `export` - alese de mine), ca grupul isi ia titlul «Clasificatii», si ca tabelul «Angajamente» incape 3 randuri fara bara la 100/125/150% si la inaltimea minima a ferestrei (calcul pe hartie la 144 dpi: ramane o rezerva de ~1,5 randuri).
- Pasii de tutorial `wait: signal:ribbon:clasificatii_salveaza` si `wait: opens:BudgetCheckForm` nu au fost rulati.
- Captura `clasificatii` din ajutor nerefacuta (fereastra s-a schimbat din nou). Nimic in `NOUTATI.md`.
