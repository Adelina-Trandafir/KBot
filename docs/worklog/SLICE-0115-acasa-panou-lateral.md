# SLICE-0115 — Acasă și evoluția panoului lateral

Istoric 13–18.10.2026. 01 mută butonul jos și restrânge acțiunile la ziua curentă, cu temporizator de miezul nopții; 02 înlocuiește vizibilitatea permanentă cu panoul în spatele machetelor în mod implicit, păstrând starea salvată de pliere. Proba din KBOT_TEST privește baza inițială; amendamentele nu au fost văzute pe ecran.

[0115](#slice-0115) · [0115-01](#slice-0115-01) · [0115-02](#slice-0115-02)

## SLICE-0115 - Pagina «Acasa» si panoul lateral al ferestrei principale (13.10.2026) <a id="slice-0115"></a>

Construit si probat in `KBOT_TEST`, adus in `KBOT\src` la 13.10.2026. Istoria: `KBOT_TEST\MERGE_NOTES.md` (sectiunile 1, 3b, 3d, 3g). Felie inregistrata dupa unire, ca lucru deja facut. Fereastra: 0113; randurile «Machete deschise»: 0116; randurile «Actiuni recente»: 0117.

### Ce s-a schimbat si de ce
1. **Pagina de start `AcasaPage`** (o pagina a lui `AvacontMain`, tinuta incarcata, doar aratata / ascunsa):
   - cardul **«Ce este nou»**, in coltul din dreapta-sus: ultimele versiuni din `NOUTATI.md` (`ReleaseNotesText.RecentVersions`), doar de citit (fara cursor, selectie, focus; rotita deruleaza). Butonul cu sageata il strange / desface (`AppSettings.HomeNewsCollapsed`). **Dupa fiecare actualizare** cardul se desface singur o data (`AppSettings.NewsSeenVersion`), apoi decide butonul;
   - **logo-ul nou** K-BOT in coltul din dreapta-jos (`Resources/kbot_logo.png`, 256 px; tot el e iconita butonului «Lista angajamentelor» din ribbon);
   - bannerul albastru cu unitatea (din primele runde) a fost scos.
   - Fara timere: pagina se citeste din nou la fiecare aducere pe ecran (`RefreshState`).
2. **Panoul lateral** (`AvacontMain.Sidebar.vb`, controlul nou `KBotSidePanel`): sta langa orice pagina, nu doar langa «Acasa». Sectiuni pliabile (clic pe titlu), declarate in Designer:
   - **Machete deschise** (0116): istoricul registrului de machete, doar din sesiunea curenta; clic pe un rand o deschide din nou; randurile nu au ora;
   - **Actiuni recente** (0117): scrierile pe server ale operatorului;
   - **Informatii** (randuri pe doua linii): Operator, Rol, Baza si perioada, FOREXE (Conectat / Neconectat, cu certificatul; verde / portocaliu), Activitate robot, Token E-Factura (starea tokenului ANAF, cu culoare dupa zilele ramase), Versiunea K-BOT;
   - **Erori din sesiune**: ultimele 10 erori ale rularii curente (`GlobalErrorLog.RecentErrors`, evenimentul `Written`, clasa `SessionErrorEntry`); clic pe una arata detaliul intr-o caseta. Panoul se strange la o banda ingusta cu pictogramele sectiunilor (butonul din bara lui de sus; clic pe o pictograma il desface; `AppSettings.HomeSidebarCollapsed`); latimea desfacuta 203 (Designer). Rotita deruleaza cand sectiunile nu incap. Nimic nu se interogheaza periodic: robotul prin evenimentele controlerului, erorile prin `Written`, machetele prin `History.Changed`, tokenul la fiecare reimprospatare a panoului.
3. `ButtonStyles.ApplyTransparent` (KBot.Theming): buton fara corp, pentru o bara (butonul cardului «Ce este nou»). Singura schimbare adusa in `ButtonStyles.vb`; varianta portocalie de test NU (vezi 0114).

### Fisiere
NOU: `src/KBot.App/AcasaPage.vb` + `.Designer.vb`, `src/KBot.App/AvacontMain.Sidebar.vb`, `src/KBot.Controls/SidePanel/KBotSidePanel.vb` + `KBotSidePanelItems.vb`, `src/KBot.App/Resources/kbot_logo.png`. Modificate: `KBot.Common/GlobalErrorLog.vb` (buffer in memorie; `Write` cheama `Remember`, nu arunca), `KBot.Common/AppSettings.vb` (`HomeSidebarCollapsed`, `HomeNewsCollapsed`, `NewsSeenVersion` + DTO nullabil, format compatibil cu cel vechi), `KBot.Theming/ButtonStyles.vb` (`ApplyTransparent`), `My Project/Resources.resx` + `Resources.Designer.vb` (`kbot_logo`), `AvacontMain.Designer.vb` (sectiunile panoului).

### Teste / verificare
- Build-ul unirii: 0 erori, 0 avertismente (operator). Nimic rulat in aceasta sesiune. Probat pe ecran in `KBOT_TEST`.

### Neverificat / amanat
- Comentariul din `AcasaPage.vb` spune inca «the same text as Setari -> Istoric versiuni» (istoricul e acum macheta, 0118). Doar comentariu, neschimbat.
- `%AppData%\AVACONT` (setari) e comun cu versiunea veche; aceasta ignora cheile noi.
- FileVersion `KBot.Controls`, `KBot.Common`, `KBot.Theming`, `KBot.App`: nebumpate.
- Ajutorul: 0000-71 (impreuna cu 0116 si 0117).

## SLICE-0115-01 - Side panel: fold button at the bottom, tooltips when folded, picture of open machete, today-only actions (17.10.2026) <a id="slice-0115-01"></a>

Operator's request of 17.10.2026 on the panel of the main window (`KBotSidePanel` in `AvacontMain`).

### What changed and why
1. **«Actiuni recente» start over every day.** `AvacontMain.LoadActionsAsync` keeps only the actions whose day is today (`Moment.Date = Date.Today`), so every row shows just `HH:mm`. A one-shot timer (`_midnightTimer`, re-armed each time) reads the list again a few seconds after midnight, so an idle window empties itself too.
2. **Fold button moved to the bottom**, under «Erori din sesiune», bottom left, with the same icons and size as the button of `navViews` in the shell (`collapse_24` / `expand_24`, 14 logical px, 6 px air). The top bar of the panel is gone (the sections start at the very top); a band at the bottom (line + button) is reserved. New properties on the control: `CollapseButtonSize`, `CollapseExpandedImage`, `CollapseCollapsedImage` (same ShouldSerialize/Reset convention as `KBotNavList`); drawn with `KBotNavList.DrawItemImage`, a chevron when no icon is given. The button has its own tooltip.
3. **Folded / unfolded state remembered between runs**: this already existed (`AppSettings.HomeSidebarCollapsed`, read in `InitSidebar`, saved in `Sidebar_CollapsedChanged`); nothing to change, only documented in the help.
4. **Tooltip over the icons of the folded strip**: `KBotToolTip` (header = section title + its icon, body = one line per row; stacked rows as `caption: value`, closed machete in italics, tone colours kept, at most 12 lines, then «... și încă N mai jos»; an empty section shows its `EmptyText`). Markup in row texts is neutralised (`EscapeMarkup`).
5. **Picture of an open macheta** in the tooltip of its row in «Machete deschise»; a closed one has NO tooltip.
   - `KBotToolTipContent.BodyImage` (new): the tooltip window draws it under the body, fitted into 300 x 220 logical px, never enlarged, never disposed by the tooltip. `IsEmpty` and the fingerprint know it.
   - `KBotSidePanelRow.ToolTipHeader` and `ToolTipImageProvider` (new); the panel asks the provider each time the tooltip opens.
   - `AvacontMain`: `CaptureFront()` draws the macheta that is on screen into a bitmap (`Control.DrawToBitmap`, reduced to at most 640 x 420) right after `PutAwayFront()` and before it is hidden (`PutFrontAway`, `SwitchBase`); the macheta that is on screen when its row is hovered is drawn again. Bitmaps are kept by `MachetaRef.Id` and disposed when the macheta closes (`Page_FormClosed`, shell back to the opening page, reference change) and when the window closes.

### Files touched
`src/KBot.App/AvacontMain.vb`, `AvacontMain.Sidebar.vb`, `AvacontMain.Designer.vb` (button size + two icons); `src/KBot.Controls/SidePanel/KBotSidePanel.vb`, `KBotSidePanelItems.vb`; `src/KBot.Controls/ToolTip/KBotToolTip.vb` (`BodyImage`), `KBotToolTipWindow.vb` (picture); help `HelpContent/contabil/panou-lateral.md` + `help-version.txt` (0000-87).

### Test results
`dotnet build src\KBot.App\KBot.App.vbproj -c Debug`: 0 warnings, 0 errors. No tests written or run, nothing run on screen (project rule). `Check-Help.ps1 -Coverage`: no error in `panou-lateral.md` (20 old errors in other files remain).

### Unverified / deferred
- Never seen on screen: the placement of the button and of the strip tooltip, and above all the picture - `DrawToBitmap` draws a Windows control faithfully, but a part that is a window of another process or ActiveX/WebView (Adobe, the FOREXE browser panel) can come out blank. The picture is taken after `PutAwayFront()` (the tab without a PDF), which is what the operator finds on return.
- Every open macheta has a picture after its first hover (if it is on screen) or after the first time it is covered.
- The midnight timer relies on `System.Windows.Forms.Timer` surviving sleep; after a long sleep the list refreshes at the next save, page change or midnight.
- Help captures `panou-lateral` and `panou-lateral-strans` are marked `redo:` (the button moved).

### Follow-up (17.10.2026)
The fold band is explicitly outside the scrolled area (opaque, pinned to the bottom edge; the sections scroll with the wheel in the space above it) and, while unfolded, carries the text «Ascunde meniul lateral» beside the button (`KBotSidePanel.CollapseButtonText`, set in `AvacontMain.Designer.vb`); the whole button + text strip is the click and hover area. Build 0/0; not seen on screen.

## SLICE-0115-02 - Side panel behind the pages, switch in Setari (18.10.2026) <a id="slice-0115-02"></a>

Operator's request of 18.10.2026: an application setting that keeps the sidebar of the main window either visible or behind the forms; default behind.

### What changed and why
- New setting `AppSettings.SidebarBehindPages` (default `True`), with its DTO field, `ToDto` and `FromDto` lines.
- `AvacontMain.ApplySidebarPlacement` (`AvacontMain.Sidebar.vb`) hides the panel when `SidebarBehindPages` is on AND a page or shell/list is shown (`_page` set or `_baseIsShell`); `pnlHost` (`Dock.Fill`) takes the freed width. On the opening page it stays visible; with the switch off it stays visible everywhere. Called by the constructor, `UpdateCloseButton` (page open/put away/close and base switch) and `AppSettings_Changed`. Historical v1 (`SidebarAlwaysVisible`: opening page + shell) and v2 (same rule everywhere) misread the request; this is the operator's 18.10.2026 rule.
- Behind the machete the fold button («Ascunde meniul lateral») is not shown: new `KBotSidePanel.CollapseButtonVisible` (default True; hides the button and its band, no hit area, no tooltip). `ApplySidebarPlacement` sets it to `Not SidebarBehindPages` and unfolds the panel meanwhile (`Collapsed = False`) so a panel saved as folded cannot be stuck without its button; `_applyingPlacement` keeps `Sidebar_CollapsedChanged` from saving that, and the saved `HomeSidebarCollapsed` comes back when the setting is turned off.
- `SetariAplicatieView`: new tick `chkSidebarInSpate` («Meniul lateral stă în spatele machetelor», checked = behind) in the «Fereastra principala» group (Designer row 7, the rows below shifted by one, `RowCount` 17), saved by `SalveazaComutator`, loaded in `IncarcaComutatoarele`.

### Files touched
`src/KBot.Common/AppSettings.vb`; `src/KBot.Controls/SidePanel/KBotSidePanel.vb`; `src/KBot.App/AvacontMain.vb`, `AvacontMain.Sidebar.vb`; `src/KBot.App/Setari/SetariAplicatieView.vb`, `SetariAplicatieView.Designer.vb`; help in SLICE-0000-106.

### Test results
`dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors. No tests written or run, nothing run on screen (project rule).

### Unverified / deferred
- Never seen on screen: that open pages (Adobe-hosting ones included) re-fit correctly when the panel appears / disappears under them, and the look of the new row in Setari.
- The panel keeps following its events while hidden; when the tick is turned on it is refreshed (`RefreshSidebar`).
