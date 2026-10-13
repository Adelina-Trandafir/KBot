# SLICE-0113 - Fereastra principala AvacontMain si machetele ca pagini (13.10.2026)

Construit si probat in copia `KBOT_TEST`, adus in `KBOT\src` la 13.10.2026 (~17:15); istoria rundelor e in
`KBOT_TEST\MERGE_NOTES.md` (sectiunile 1, 2, 3b, 3c, 3d, 3i). Felie inregistrata dupa unire, ca lucru deja facut.
Feliile surori ale aceleiasi arhitecturi: 0114 (ribbon), 0115 (Acasa + panoul lateral), 0116 (registrul de
machete), 0117 (actiuni recente), 0118 (Jurnal / Istoric versiuni ca machete), 0119 (E-Factura pe ribbon),
0120 (randul de jos din editorul DDF).

## Ce s-a schimbat si de ce
1. **Fereastra principala noua, `AvacontMain`** (`KBotShellForm`): sus bara de titlu si bara de ocupat (aceleasi
   instante ale lui `KbotForm`, mutate la rulare prin `KbotForm.TakeHeader`, ca `Handles capBar...` sa mearga in
   continuare), apoi ribbonul (0114), in stanga panoul lateral (0115), restul = zona de pagini `pnlHost`.
   `Program.vb` porneste `AvacontMain` in loc de `KbotForm`.
2. **Lista angajamentelor (`KbotForm`) e gazduita**: `PrepareForHosting` (fara chenar, `TopLevel = False`,
   `Dock = Fill`, `AutoFitToTheme = False`, randul MENIU `pnlHeader` ascuns). Se incarca in spate la pornire
   (`OnShown`: `_kbot.Show()` sub pagina Acasa), deci selectoarele, arborele, banda FOREXE pornesc ca inainte.
   `KbotForm.OnLoad` maximizeaza doar cand nu e gazduit.
3. **Decizie: toate machetele se deschid ca pagini, una cate una** (`AvacontMain.ShowPage`, `ShowPageAndWaitAsync`,
   `ShowMachetaAsync`). Extrase, Grupe, Setari, Clasificatii, Parteneri, editoarele DDF / ORD, legaturile
   receptiilor (`AsociereForm`), operatiunile necorelate (`CabNoteForm`), E-Factura Vanzare / Cumparare, graficul
   rezervarilor, asocierea partenerilor din Sumar, catalogul de mesaje (Debug). Apelurile `ShowDialog` au devenit
   `Await ShowMachetaAsync(...)`: codul de dupa inchidere ruleaza exact ca inainte (citeste `SAuSalvatModificari`).
   Fara fereastra principala (bancul Debug) totul ramane dialog, ca inainte.
   **Raman ferestre separate / dialoguri:** casetele mici (alegeri, confirmari, file pickers, `OrdZiuaForm`,
   `CabNoteReceiptForm`, dialogurile din editoare, «Conturi Unitate», «Date Unitate», tokenul ANAF, sincronizarea
   facturilor primite), ferestrele de monitorizare (consola FOREXE, istoric FOREXE, coada robotului, informatii
   interne), `LoginForm`, `DirectorForm`, fereastra de ajutor deschisa cu F1.
4. **Titlul paginii** trece in bara de titlu a ferestrei principale (bara de titlu proprie a paginii se ascunde,
   `HidePageTitleBar`); la inchiderea paginii revine titlul de dinainte.
5. **Machetele si-au pierdut butoanele proprii de inchidere** (`HideCloseButtons`: `btnInchide`, `btnIesire`,
   `btnClose`; plus `btnRenunta` la `DdfEditForm` / `OrdEditForm` / `AsociereForm`). Se inchid din butonul
   «Inchide» al ribbonului (0114); **Esc** inchide in continuare (ascultat pe forma cand butonul de anulare a fost
   ascuns). «Renunta» al unui nomenclator sau al unei facturi ramane (anuleaza o editare, nu inchide).
   Inchiderea pastreaza intrebarile fiecarei machete despre modificarile nesalvate (`FormClosing` al ei); o
   macheta care refuza inchiderea opreste si deschiderea celei noi.
6. **Pornire marita o singura data** (`AppSettings.FullScreenOffered`): la prima pornire dupa schimbare,
   `StartMaximized` se pune pe True si fereastra porneste marita; apoi bifa e a operatorului.
7. **Adobe / Office gazduite**: `FindForm()` -> `TopLevelForm()` (`KBot.Controls/Adobe/AdobeTopForm.vb`, 13 locuri in
   6 fisiere Adobe + `OfficeDocumentHost.vb`): fereastra de sus, nu primul `Form` din lant. Remediaza Ctrl+H,
   activarea si inchiderea cand lista angajamentelor e gazduita.
8. `HelpService.ShowHelpMenu`: proprietarul popup-ului «?» = fereastra de sus (`TopLevelControl`), fiindca o pagina
   gazduita nu e fereastra de sus.

## Fisiere
- NOU: `src/KBot.App/AvacontMain.vb` + `.Designer.vb` (partea de pagini; ribbonul si panoul sunt in 0114 / 0115),
  `src/KBot.App/KbotForm.Host.vb` (`HostForm`, `TakeHeader`, `PrepareForHosting`), `src/KBot.Controls/Adobe/AdobeTopForm.vb`.
- Modificate: `KBot.App/Program.vb`, `KbotForm.vb`, `KbotForm.Chrome.vb`, `KbotForm.HelpCapture.vb`,
  `KbotForm.Extrase.vb`, `KbotForm.Grupe.vb`, `KbotForm.Ddf.vb`, `KbotForm.Ord.vb`, `KbotForm.Receptii.vb`,
  `KbotForm.Ingest.vb`, `KbotForm.MessageCatalog.vb`, `Views/SumarView.vb`, `Views/RezervariView.vb`,
  `Setari/SetariForm.vb` (`ShowFor`: pagina cand e gazduit), `Help/HelpService.vb` (proprietarul popup-ului);
  `KBot.Common/AppSettings.vb` (`FullScreenOffered` + DTO); `KBot.Controls/Adobe/AcroPdfViewer.vb`,
  `AcroPdfViewer.CheckLog.vb`, `.LightWatch.vb`, `.ReadMode.vb`, `.SaveTrap.vb`, `.Watch.vb`, `AdobeReaderHost.vb`,
  `KBot.Controls/Office/OfficeDocumentHost.vb`.

## Teste / verificare
- Dupa unire (MERGE_NOTES, «UNIT in KBOT»): `dotnet build KBot.sln` 0 erori, 0 avertismente. `dotnet test`: 99 teste
  picate, aceleasi ca pe codul de dinainte (mediu / cai), deci niciunul nou. Rulate de operator, nu in aceasta sesiune.
- Probat pe ecran de operator in `KBOT_TEST`; in `KBOT` nimic rulat in aceasta sesiune.

## Neverificat / amanat
- **Turul initial de ajutor nu mai porneste**: `PornesteTurulInitial` verifica `Visible`, iar shell-ul e ascuns la
  pornire (Acasa e pe ecran); turul e si ancorat pe `btnMeniu`, ascuns. De reparat separat (Open threads 0113).
- `menuNou` / `btnMeniu` raman in `KbotForm`, doar ascunse: ribbonul le copiaza starea si ruleaza aceleasi handlere.
  Sterse doar la cererea operatorului.
- FileVersion pentru `KBot.App`, `KBot.Common`, `KBot.Controls` nebumpate (de facut separat).
- `KbotForm` gazduit nu mai creste fereastra cand tema cere fonturi mai mari (`AutoFitToTheme = False`).
- Ajutorul: 0000-69 (machete ca pagini, fara butoane de inchidere).
