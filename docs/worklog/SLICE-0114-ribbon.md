# SLICE-0114 - Ribbonul ferestrei principale (KBotRibbon) si butoanele fixate «Ajutor» / «Inchide» (13.10.2026)

Construit si probat in `KBOT_TEST`, adus in `KBOT\src` la 13.10.2026. Istoria: `KBOT_TEST\MERGE_NOTES.md`
(sectiunile 1, 3b, 3d, 3h, 3i). Felie inregistrata dupa unire, ca lucru deja facut. Fereastra care il gazduieste: 0113.

## Ce s-a schimbat si de ce
1. **Controlul nou `KBotRibbon`** (`KBot.Controls/Ribbon/`): o banda de file deasupra unui panou cu grupuri de
   butoane mari / mici, ca in Office; totul desenat (fara controale copil), tematizat prin `IThemedControl`.
   Modelul `KBotRibbonTab` / `KBotRibbonGroup` / `KBotRibbonButton` + colectii + `KBotRibbonItemClickedEventArgs`
   (`KBotRibbonItems.vb`), autorat in Designer. Un buton / grup / fila fara nimic vizibil nu se deseneaza.
2. **Strangerea**: dublu clic pe o fila sau sageata din dreapta strange ribbonul la numele filelor; un clic pe o
   fila il desface. Starea se tine minte (`AppSettings.RibbonCollapsed`). Fara navigare cu tastatura, fara panou
   plutitor in starea stransa, fara butoane cu lista (deliberat, prima versiune).
3. **Ribbonul ia locul meniului MENIU** (`AvacontMain.Designer.vb`). Filele si grupurile:
   - **Angajamente**: Navigare (Lista angajamentelor), Adaugare (Angajament nou, Creeaza in FOREXE), Verificari
     (Operatiuni necorelate, vizibil doar cand exista);
   - **Date**: Nomenclatoare (Clasificatii bugetare, Parteneri), Documente (Extrase);
   - **E-Factura**: vezi 0119;
   - **Configurari**: Configurare (Jurnal activitate, Configurare K-BOT, Istoric versiuni), Unitate (0119),
     Administrare (Mesaje (catalog), Capturi pentru ajutor, Designer tutoriale; ascunse in afara Debug / modului capturi).
   Un clic ajunge la acelasi handler pe care il folosea meniul (`KbotForm.RunMenuEntry` -> `MenuNou_ItemClicked`).
   Ce se vede doar uneori urmeaza regulile meniului vechi: `KbotForm.MirrorMenuTo` aplica `MenuNou_Opening` si
   copiaza vizibilitatea fiecarui rand cu aceeasi cheie (plus textul si culoarea randului necorelatelor);
   `MenuStateChanged` (ridicat si la sfarsitul `MarkUncorrelated`) si `AppSettings.Changed` declanseaza copierea.
   Intrarile care lucreaza pe lista angajamentelor (angajament_nou, angajament_forexe, operatiuni_necorelate) aduc
   intai lista pe ecran (`ShowShell`).
4. **Butoane fixate** (`PinnedButtons`): in dreapta benzii de file, vizibile pe orice fila si cu ribbonul strans.
   - **«Inchide»** (`pagina_inchide`, `AvacontMain.CloseCurrent`): inchide macheta de pe ecran; din lista
     angajamentelor revine la «Acasa» (lista nu se distruge, doar se ascunde). Nu exista pe pagina «Acasa»;
     aplicatia se inchide doar din X-ul ferestrei. Butonul e mare, in dreapta panoului; cu ribbonul strans se
     micsoreaza in banda de file (`KBotRibbonButton.DrawSmall`).
   - **«Ajutor»** (`ajutor`, resursa `ajutor_128.png`): fara macheta deschisa -> fereastra de ajutor ca pagina
     (`HelpService.ShowHelpPage`, macheta `ajutor` in registru); cu o macheta sau lista angajamentelor pe ecran ->
     meniul «?» complet al ei (cautare, ecranul acesta, tururi, tutoriale), ancorat sub buton
     (`KBotRibbon.ButtonScreenBounds`). `KBotHelp.RequestMenu` / `IKBotHelpProvider.ShowHelpMenu` primesc
     `k_withTours` (False = fara tururi si tutoriale); ribbonul nu il mai foloseste, parametrul ramane.
   - Butonul fixat «Acasa» / «Inapoi» (runda 3h) a fost scos in runda 3i: machetele nu raman deschise in fundal;
     la «Acasa» se ajunge prin «Inchide».
5. In dreapta benzii de file: numele operatorului (`RightText`, urmeaza `lblOperator`).
6. **Decizie: portocaliul a fost doar pentru test.** Accentele portocalii din `KBOT_TEST`
   (`BuiltInSchemes.vb`, `ButtonStyles.ApplyPrimary`) NU au fost aduse in `KBOT`; temele raman cele albastre.

## Fisiere
- NOU: `src/KBot.Controls/Ribbon/KBotRibbon.vb`, `KBotRibbonItems.vb`, `src/KBot.App/Resources/ajutor_128.png`.
- Modificate: `src/KBot.App/AvacontMain.vb` + `.Designer.vb` (ribbonul, `Ribbon_ItemClicked`, `SyncRibbon`,
  `CloseCurrent`, `UpdateCloseButton`, `ShowRibbonHelp`), `KbotForm.Host.vb` (`RunMenuEntry`, `MirrorMenuTo`,
  `MenuStateChanged`), `KbotForm.CabNotes.vb` (o linie), `KBot.Theming/KBotHelp.vb`, `Help/HelpService.vb`
  (`k_withTours`, `ShowHelpPage`, `PopupHomeRows`), `My Project/Resources.resx` + `Resources.Designer.vb`,
  `KBot.Common/AppSettings.vb` (`RibbonCollapsed`).

## Teste / verificare
- Build-ul unirii: 0 erori, 0 avertismente (operator, MERGE_NOTES). Nimic rulat in aceasta sesiune.
- Probat pe ecran de operator in `KBOT_TEST`, cu accentele portocalii de test.

## Neverificat / amanat
- `KBotRibbon.md` (documentatia controlului) si linia din `CONTROLS.md`: nescrise.
- Ribbonul in temele Classic / Dark / Modern cu accentele albastre reale: nevazut (proba a fost pe portocaliu).
- Captiunea comutatorului din Setari › Aplicatie spune inca «Randul «Jurnal activitate» in meniul MENIU al
  ferestrei principale» (`SetariAplicatieView.Designer.vb`, `chkLogViewer`): acum comanda butonul din ribbon.
  Text de pe ecran invechit; nu s-a schimbat (cod).
- FileVersion `KBot.Controls`, `KBot.Theming`, `KBot.App`: nebumpate.
- Ajutorul: 0000-70.
