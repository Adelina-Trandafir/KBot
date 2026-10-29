# SLICE-0114 — Ribbonul: control, grupuri și comenzi contextuale

Istoric 13–17.10.2026: etapa inițială și 01…18. 02 scoate «Ajutor», 03 înlocuiește butonul fixat contextual cu RightGroup, 06/07 adaugă FormGroup și separă comenzile machetei de cele ale vederii. Descărcarea Extrase trece prin etapele 04/05/08/09, iar 15/16 mută comenzile vederilor în ribbon. 18 înlocuiește crearea imediată a câmpurilor din 01 cu crearea la primul layout. Etapele E-Factura 19/21…24 sunt în [istoricul lor](SLICE-0114-19-efactura-vanzare-fara-luna-si-randuri-taiate.md), 20 în [istoricul ferestrelor](SLICE-0113-fereastra-principala-machete-pagini.md#slice-0114-20); 25…27 rămân distincte. 0114-24 — ribbon, mai jos, înlocuiește lipsa panoului temporar și gestul inițial de desfacere. Stările sunt istorice. Probe identice și limite copiate între subetape au mai jos o singură definiție, fără extinderea vreunei confirmări vizuale.

[0114](#slice-0114) · [0114-01](#slice-0114-01) · [0114-02](#slice-0114-02) · [0114-03](#slice-0114-03) · [0114-04](#slice-0114-04) · [0114-05](#slice-0114-05) · [0114-06](#slice-0114-06) · [0114-07](#slice-0114-07) · [0114-08](#slice-0114-08) · [0114-09](#slice-0114-09) · [0114-10](#slice-0114-10) · [0114-11](#slice-0114-11) · [0114-12](#slice-0114-12) · [0114-13](#slice-0114-13) · [0114-14](#slice-0114-14) · [0114-15](#slice-0114-15) · [0114-16](#slice-0114-16) · [0114-17](#slice-0114-17) · [0114-18](#slice-0114-18)

## SLICE-0114 - Ribbonul ferestrei principale (KBotRibbon) si butoanele fixate «Ajutor» / «Inchide» (13.10.2026) <a id="slice-0114"></a>

Construit si probat in `KBOT_TEST`, adus in `KBOT\src` la 13.10.2026. Istoria: `KBOT_TEST\MERGE_NOTES.md` (sectiunile 1, 3b, 3d, 3h, 3i). Felie inregistrata dupa unire, ca lucru deja facut. Fereastra care il gazduieste: 0113.

### Ce s-a schimbat si de ce
1. **Controlul nou `KBotRibbon`** (`KBot.Controls/Ribbon/`): o banda de file deasupra unui panou cu grupuri de butoane mari / mici, ca in Office; totul desenat (fara controale copil), tematizat prin `IThemedControl`. Modelul `KBotRibbonTab` / `KBotRibbonGroup` / `KBotRibbonButton` + colectii + `KBotRibbonItemClickedEventArgs` (`KBotRibbonItems.vb`), autorat in Designer. Un buton / grup / fila fara nimic vizibil nu se deseneaza.
2. **Strangerea**: dublu clic pe o fila sau sageata din dreapta strange ribbonul la numele filelor; un clic pe o fila il desface. Starea se tine minte (`AppSettings.RibbonCollapsed`). Fara navigare cu tastatura, fara panou plutitor in starea stransa, fara butoane cu lista (deliberat, prima versiune).
3. **Ribbonul ia locul meniului MENIU** (`AvacontMain.Designer.vb`). Filele si grupurile:
   - **Angajamente**: Navigare (Lista angajamentelor), Adaugare (Angajament nou, Creeaza in FOREXE), Verificari (Operatiuni necorelate, vizibil doar cand exista);
   - **Date**: Nomenclatoare (Clasificatii bugetare, Parteneri), Documente (Extrase);
   - **E-Factura**: vezi 0119;
   - **Configurari**: Configurare (Jurnal activitate, Configurare K-BOT, Istoric versiuni), Unitate (0119), Administrare (Mesaje (catalog), Capturi pentru ajutor, Designer tutoriale; ascunse in afara Debug / modului capturi).
   Un clic ajunge la acelasi handler pe care il folosea meniul (`KbotForm.RunMenuEntry` -> `MenuNou_ItemClicked`).
   Ce se vede doar uneori urmeaza regulile meniului vechi: `KbotForm.MirrorMenuTo` aplica `MenuNou_Opening` si copiaza vizibilitatea fiecarui rand cu aceeasi cheie (plus textul si culoarea randului necorelatelor); `MenuStateChanged` (ridicat si la sfarsitul `MarkUncorrelated`) si `AppSettings.Changed` declanseaza copierea.
   Intrarile care lucreaza pe lista angajamentelor (angajament_nou, angajament_forexe, operatiuni_necorelate) aduc intai lista pe ecran (`ShowShell`).
4. **Butoane fixate** (`PinnedButtons`): in dreapta benzii de file, vizibile pe orice fila si cu ribbonul strans.
   - **«Inchide»** (`pagina_inchide`, `AvacontMain.CloseCurrent`): inchide macheta de pe ecran; din lista angajamentelor revine la «Acasa» (lista nu se distruge, doar se ascunde). Nu exista pe pagina «Acasa»; aplicatia se inchide doar din X-ul ferestrei. Butonul e mare, in dreapta panoului; cu ribbonul strans se micsoreaza in banda de file (`KBotRibbonButton.DrawSmall`).
   - **«Ajutor»** (`ajutor`, resursa `ajutor_128.png`): fara macheta deschisa -> fereastra de ajutor ca pagina (`HelpService.ShowHelpPage`, macheta `ajutor` in registru); cu o macheta sau lista angajamentelor pe ecran -> meniul «?» complet al ei (cautare, ecranul acesta, tururi, tutoriale), ancorat sub buton (`KBotRibbon.ButtonScreenBounds`). `KBotHelp.RequestMenu` / `IKBotHelpProvider.ShowHelpMenu` primesc `k_withTours` (False = fara tururi si tutoriale); ribbonul nu il mai foloseste, parametrul ramane.
   - Butonul fixat «Acasa» / «Inapoi» (runda 3h) a fost scos in runda 3i: machetele nu raman deschise in fundal; la «Acasa» se ajunge prin «Inchide».
5. In dreapta benzii de file: numele operatorului (`RightText`, urmeaza `lblOperator`).
6. **Decizie: portocaliul a fost doar pentru test.** Accentele portocalii din `KBOT_TEST` (`BuiltInSchemes.vb`, `ButtonStyles.ApplyPrimary`) NU au fost aduse in `KBOT`; temele raman cele albastre.

### Fisiere
NOU: `src/KBot.Controls/Ribbon/KBotRibbon.vb`, `KBotRibbonItems.vb`, `src/KBot.App/Resources/ajutor_128.png`. Modificate: `src/KBot.App/AvacontMain.vb` + `.Designer.vb` (ribbonul, `Ribbon_ItemClicked`, `SyncRibbon`, `CloseCurrent`, `UpdateCloseButton`, `ShowRibbonHelp`), `KbotForm.Host.vb` (`RunMenuEntry`, `MirrorMenuTo`, `MenuStateChanged`), `KbotForm.CabNotes.vb` (o linie), `KBot.Theming/KBotHelp.vb`, `Help/HelpService.vb` (`k_withTours`, `ShowHelpPage`, `PopupHomeRows`), `My Project/Resources.resx` + `Resources.Designer.vb`, `KBot.Common/AppSettings.vb` (`RibbonCollapsed`).

### Teste / verificare
- Build-ul unirii: 0 erori, 0 avertismente (operator, MERGE_NOTES). Nimic rulat in aceasta sesiune.
- Probat pe ecran de operator in `KBOT_TEST`, cu accentele portocalii de test.

### Neverificat / amanat
- `KBotRibbon.md` (documentatia controlului) si linia din `CONTROLS.md`: nescrise.
- Ribbonul in temele Classic / Dark / Modern cu accentele albastre reale: nevazut (proba a fost pe portocaliu).
- Captiunea comutatorului din Setari › Aplicatie spune inca «Randul «Jurnal activitate» in meniul MENIU al ferestrei principale» (`SetariAplicatieView.Designer.vb`, `chkLogViewer`): acum comanda butonul din ribbon. Text de pe ecran invechit; nu s-a schimbat (cod).
- FileVersion `KBot.Controls`, `KBot.Theming`, `KBot.App`: nebumpate.
- Ajutorul: 0000-70.

## SLICE-0114-01 - Ribbon: fields (combo / text box) in a group, conditional buttons, order (15.10.2026) <a id="slice-0114-01"></a>

Sub-slice of 0114 (the slice that made `KBotRibbon`), asked by the operator: changes to the menu go in the slice that made it. The E-Factura side of the same request is [00EF-30](SLICE-00EF-18-primite-client.md#slice-00EF-30).

### What changed and why
1. **New `KBotRibbonField`** (`KBot.Controls/Ribbon/KBotRibbonItems.vb`), designer Group → Fields: **Combo** (`KBotComboBox`, `Editable = False`, selection only) or **Text** (`KBotTextBox`, `PlaceholderText`, `MaxLength`, `DigitsOnly`). Properties: `Key`, logical `Width` 150, `Visible`, `Enabled`, tooltip. Code reads `Text`/`SelectedIndex`; no event. `KBotRibbonFieldCollection` creates/removes the real control.
2. **`KBotRibbon`:** `AttachField`/`DetachField` manage runtime child controls. Fields occupy a column after group buttons, maximum three rows. `ApplyFieldControls` positions/shows/hides for tab/collapse/field visibility; a visible field alone keeps its group visible. `FindField`, field keys in `ValidateKeys`, tooltips and theme are supported; «no child controls» is removed from the summary.
3. **E-Factura tab** (`AvacontMain.Designer.vb`): «Cumparare» now stands BEFORE «Vanzare»; the button «Sincronizeaza facturi primite» and its two fields start hidden (`Visible = False`) and `AvacontMain.UpdatePrimiteSyncControls` shows them only while the page on screen is the received-invoices window (`PrimiteForm`); called from `UpdateCloseButton`, i.e. on every page change.

### Files touched
`src/KBot.Controls/Ribbon/KBotRibbonItems.vb`, `KBotRibbon.vb`, `src/KBot.App/AvacontMain.Designer.vb`, `AvacontMain.vb`.

### Test results
`dotnet build src/KBot.App` (builds Controls and EFactura): 0 warnings, 0 errors. No tests run (operator rule). Not seen on screen.

### Unverified / deferred
- The look of the fields in the panel (row height 22 px logical, 20 px control) and whether the KBot combo / text box keep their themed look inside the ribbon in the three themes; the first paint of a field after the page opens.
- `KBotRibbon.md` / `CONTROLS.md` still unwritten (open thread of 0114).
- Fields are not hit by the ribbon's own tooltip (the KBotToolTip component is applied to the controls, through `ToolTipHeader` / `ToolTipText`).

### Follow-up (15.10.2026, after the first look)
The fields overlapped: the KBot combo forces its own height (`PreferredHeight`, taller than a 22 px small-button row), so the text box under it was covered. `KBotRibbon` now takes the tallest control height as the row height (`FieldHeight`), stacks the rows with a 3 px gap and centres the block above the group caption. Build KBot.App: 0 warnings, 0 errors; not seen on screen (look at 100% and 150%).

## SLICE-0114-02 - Ribbon: separator before a pinned button, page buttons, «Ajutor» removed (16.10.2026) <a id="slice-0114-02"></a>

Sub-slice of 0114 (the slice that made `KBotRibbon`), asked by the operator: (1) remove the «Ajutor» button from the ribbon; (2) while the «Sumar» view is on screen a new button «Asociaza parteneri» and a separator appear before «Inchide», and go away when the view is no longer on screen; the system will be reused, so the control changes if it has to. The Sumar side is [SLICE-0084-04](SLICE-0084-02-asociere-parteneri-sumar.md#slice-0084-04).

### What changed and why
1. **Control** (`KBot.Controls/Ribbon`): `KBotRibbonButton.SeparatorBefore` (designer property, default False) + derived `SeparatorX`. `KBotRibbon.ComputeLayout` gives a pinned button that asked for it a 6 px gap and a thin line to its left, **only while another pinned button to its left is visible** (so «Inchide» alone shows no line); both layouts (the panel and the folded tab strip). `OnPaint` draws the line in the border colour. A pinned button already supported `Visible`, so no other change: a page button is a pinned button with `Visible = False` at design time that the host shows and hides.
2. **`AvacontMain`**: the pinned «Ajutor» (key `ajutor`) is gone, with `ShowRibbonHelp` and its click case. The new pinned button `sumar_parteneri` («Asociaza parteneri», icon `binvoice`, hidden at design time) comes first; «Inchide» has `SeparatorBefore = True`. `UpdateSumarPartnersButton` (called from `UpdateCloseButton`, i.e. on every page change, and from `KbotForm.ActiveViewChanged`) shows it only when no page is open, the shell is the base and the active view is «sumar»; it is enabled only when the angajament has a DDF. A click runs `KbotForm.OpenSumarPartners`.
3. Kept on purpose (house rule: old paths switched off, not deleted): `HelpService.ShowHelpPage`, the registry kind `ajutor`, `KBotRibbon.ButtonScreenBounds`, the resource `ajutor_128`. F1 and the caption bar «?» still open the help.

> Superseded on the App side by [0114-03](SLICE-0114-ribbon.md#slice-0114-03) (same day): the button now lives in the ribbon's right group; `SeparatorBefore` stays in the control but «Inchide» no longer sets it.

### Files touched
`src/KBot.Controls/Ribbon/KBotRibbonItems.vb`, `KBotRibbon.vb`; `src/KBot.App/AvacontMain.vb`, `AvacontMain.Designer.vb`, `KbotForm.Views.vb`; help: `contabil/ajutor.md`, `contabil/ribbon.md`, `contabil/ajutor-buton.md` (deleted), `docs/HELP_SYSTEM.md` (+ `sumar.md`, `tur-sumar.md`, see 0084-04). Status rows in `KBOT_STATUS.md`, `state/KBOT_STATUS_0110-0119.md`.

### Test results
`dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors. No tests run (house rule). Not run, not seen on screen. `Check-Help.ps1`: see the help status row 0000-85 (the errors that remain are old ones, in tutorials).

### Unverified / deferred
- The look: the 6 px gap and the line (panel: from 10 px below the tab strip to 12 px above the bottom; folded: 6 px in the tab strip) in the three themes, at 100% and 150%. The «Asociaza parteneri» caption width in the panel.
- The icon is `binvoice` (the one of «Parteneri»): there is no partners-specific picture in the resources. Say if another is wanted.
- Captures `ribbon-file` / `ribbon-strans` of the help still show «Ajutor»; to redo.
- `KBotRibbon.md` / `CONTROLS.md` still unwritten (open thread of 0114).
- Not committed (git writes are the operator's).

## SLICE-0114-03 - Ribbon: the right group (16.10.2026) <a id="slice-0114-03"></a>

Sub-slice of 0114, asked by the operator: more than one button, and other kinds of elements, for what a view offers; a right group, apart from the «Inchide» button. Replaces the one-button mechanism of 0114-02 on the App side.

### What changed and why
1. **`KBotRibbon.RightGroup`** (a `KBotRibbonGroup`, designer-authored: Buttons, Fields, Text, Visible; expandable in the property grid). It stands at the right end of the panel, before the large pinned buttons («Inchide»), the same on every tab. It is shown only while it has a visible button or field, and not while the ribbon is folded (its field controls are hidden then, like the tab groups'). Its right edge line (every group draws one) is the separator before «Inchide».
2. Everything that walked «the groups of the tabs» now walks `AllGroups()` (tab groups + the right group): `FindButton`, `FindField`, `ValidateKeys`, theme, field tooltips, field controls. Layout: the group placement became `PlaceGroup` (used by the tab groups and the right group; the right group is placed at 0 and shifted with `ShiftGroup`); paint: `PaintGroup`; hit test: `ButtonInGroup`. No change of look for the tab groups.
3. **App**: the button `sumar_parteneri` moved from `PinnedButtons` to `ribbon.RightGroup.Buttons` (designer); `SeparatorBefore` is no longer set on «Inchide» (the group's own line does it). `UpdateSumarPartnersButton` also sets the group's caption («Sumar») while it shows the button. Another button or field for a view = one more control in the right group with `Visible = False`, shown and hidden by the host.
4. `KBotRibbonButton.SeparatorBefore` (0114-02) stays in the control, unused for now (it still works between two pinned buttons).

### Files touched
`src/KBot.Controls/Ribbon/KBotRibbon.vb`; `src/KBot.App/AvacontMain.vb`, `AvacontMain.Designer.vb`; help `contabil/ribbon.md`, `contabil/vederi/sumar.md`, `tours/tur-sumar.md` (help 0000-85, extended). Status rows in `KBOT_STATUS.md`, `state/KBOT_STATUS_0110-0119.md`.

### Test results
`dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors. No tests run (house rule). Not run, not seen on screen.

### Unverified / deferred
- Not seen: the group at the right of the panel (width, caption «Sumar», the line before «Inchide»), the three themes, 100% / 150%.
- With a narrow window the right group can overlap the groups of a tab (no clipping or wrapping yet); a hidden-by-default field of the right group has never been shown (the tab groups' fields were seen only in `KBOT_TEST`).
- The right group is not shown while the ribbon is folded (decision, not asked): small versions of its buttons in the tab strip would be the alternative.
- With no tab shown at all the layout stops before the right group (as it does for the pinned large buttons).
- `KBotRibbon.md` / `CONTROLS.md` still unwritten (open thread of 0114).
- Not committed.

## SLICE-0114-04 - Ribbon: tab order, "Date" folded into Configurari, new Extrase tab (17.10.2026) <a id="slice-0114-04"></a>

### What changed and why
Operator request. Ribbon tabs are now: Configurari, Angajamente (selected at start), E-Factura, Extrase.
- "Date" tab is gone; its group (Clasificatii bugetare, Parteneri) is the first group of Configurari, now captioned "Date".
- "Extrase" button left the old "Documente" group's tab and sits in the new "Extrase" tab (group "Documente"); a second group "Descarcare" holds the new button `extrase_descarca` ("Descarca extrase") = the same download + import as the tree-footer icon, then the open Extrase view reloads.
- `ribbon.SelectedTabIndex = 1` (Angajamente) in the designer.

### Files touched
`src/KBot.App/AvacontMain.Designer.vb`, `AvacontMain.vb` (new `Case "extrase_descarca"`), `KbotForm.Extrase.vb` (`DescarcaExtraseDinRibbon`).

### Test results
`dotnet build src/KBot.App/KBot.App.vbproj`: 0 warnings, 0 errors. No test run, nothing run on screen.

### Unverified / deferred
Not seen on screen: tab order, the preselected tab, the new button (its icon reuses the FOREXE browser icon). Help done as 0000-88.

## SLICE-0114-05 - «Descarca extrase»: only the ribbon button, tied to the Extrase window / the shell (17.10.2026) <a id="slice-0114-05"></a>

### What changed and why
Operator: the download of the statements belongs to the ribbon button «Descarca extrase» (0114-04), which must be associated with the Extrase window or with the main shell (KbotMain); the icons in the trees are to be hidden.
1. **Ribbon button** `extrase_descarca` (`AvacontMain`): `Visible = False` in the designer; new `UpdateExtraseDownloadButton` (called from `UpdateCloseButton`, so on every page / shell change) shows it only while `CurrentPage Is ExtraseForm` or the shell is on screen (`CurrentPage Is Nothing AndAlso _baseIsShell`). The ribbon skips a group with nothing visible, so the «Descarcare» group goes with it.
2. **Tree icons gone**: the main tree lost its left icon (credit card + tooltip, `KbotForm.Designer.vb`) and its handler `Tree_FooterLeftIconClicked` (`KbotForm.Extrase.vb`); `ExtraseView` no longer takes the `descarca` delegate, sets `panel.ShowDownloadIcon = False` and drops `Panel_DescarcaCerut`; `KbotForm.Views.vb` builds it without the delegate. With the icon out, the footer text of the Extrase tree («Actualizeaza») goes too (see the 00EF-33 follow-up: the caption needs a visible icon).
3. Kept: the window's own button `ExtraseForm.btnDescarca`, `ExtrasePanel.ShowDownloadIcon` / `DescarcaCerut` (unused now, harmless), the resource `credit_card`.
4. Help: 0000-89. Captures marked for redo (36, see its worklog).

### Files touched
`src/KBot.App/AvacontMain.vb`, `AvacontMain.Designer.vb`, `KbotForm.Designer.vb`, `KbotForm.Extrase.vb`, `KbotForm.Views.vb`, `Views/ExtraseView.vb`.

**Probe:** [B1](#probe-b1).

### Unverified / deferred
- Read of «associated with the Extrase form or KbotMain»: visible on the Extrase window and on the shell (any view). If only the shell's Extrase view was meant, narrow `UpdateExtraseDownloadButton` with `_kbot.ActiveViewKey = "extrase"`.
- Not seen: the button appearing / disappearing when pages change, nor the empty-group rule of the ribbon.
- `ExtraseForm.btnDescarca` left in place (not mentioned).

## SLICE-0114-06 - Ribbon: a group for the FORM, next to the view group (operator item 1) (17.10.2026) <a id="slice-0114-06"></a>

### What changed and why
New control property `KBotRibbon.FormGroup` (`KBotRibbonGroup`, serialized like `RightGroup`): placed at the very right of the panel, against the large pinned buttons («Inchide»). `RightGroup` stays the group of the VIEW and now stands to its LEFT. Order, right to left: X («Inchide»), group of the form, group of the view. Layout, painting, hit-test, `AllGroups` (FindButton / FindField / ValidateKeys) cover both. Both groups show only while they have a visible button or field and not while the ribbon is folded.

### Files touched
`src/KBot.Controls/Ribbon/KBotRibbon.vb`.

**Probe:** [B1](#probe-b1).

### Unverified / deferred
- [U2 — limita vizuală comună](#limita-u2).

## SLICE-0114-07 - Form group tied to the whole form (KbotMain), not to a view (operator item 2) (17.10.2026) <a id="slice-0114-07"></a>

### What changed and why
`AvacontMain.UpdateFormGroup` (replaces `UpdatePrimiteSyncControls` and the `UpdateExtraseDownloadButton` of 0114-05; called from `UpdateCloseButton` on every page / shell change) decides what the form group holds from the FORM on screen: the shell (KbotMain, every view of it) -> «Editeaza grupe», «Descarca extrase»; the «Extrase de cont» window -> «Descarca extrase»; the received invoices window -> «Sincronizeaza facturi primite» + its two inputs. The caption is the form («Lista angajamentelor» / «Extrase de cont» / «Facturi primite»). The view group («Sumar» ...) is untouched. The tab «Descarcare» group of the Extrase tab is gone.

### Files touched
`src/KBot.App/AvacontMain.vb`, `AvacontMain.Designer.vb`.

**Probe:** [B1](#probe-b1).

### Unverified / deferred
- [U2 — limita vizuală comună](#limita-u2).
- Reading of «KbotMain»: the whole shell (any view). If only some views were meant, narrow `UpdateFormGroup`.

## SLICE-0114-08 - «Extrase de cont» window: its own download button removed (operator item 3) (17.10.2026) <a id="slice-0114-08"></a>

### What changed and why
`btnDescarca` is out of `ExtraseForm` (designer, handler, busy / theme lines, the `descarca` constructor argument); the footer table has two columns now. New `ExtraseForm.Reincarca`; `KbotForm.ReloadExtraseView` reloads the window too, so the ribbon download refreshes it. The empty text says the ribbon button brings the statements.

### Files touched
`src/KBot.App/Views/ExtraseForm.vb`, `ExtraseForm.Designer.vb`, `KbotForm.Extrase.vb`.

**Probe:** [B1](#probe-b1).

### Unverified / deferred
- [U2 — limita vizuală comună](#limita-u2).
- The window's own «Inchide» button was kept (not mentioned).

## SLICE-0114-09 - «Descarca extrase» in the right group of the ribbon (operator item 4) (17.10.2026) <a id="slice-0114-09"></a>

### What changed and why
The button `extrase_descarca` moved from the Extrase tab (group «Descarcare», removed) to `ribbon.FormGroup`; visible for the shell and the Extrase window (see 0114-07). It keeps the key and the handler (`DescarcaExtraseDinRibbon`).

### Files touched
`src/KBot.App/AvacontMain.Designer.vb`, `AvacontMain.vb`.

**Probe:** [B1](#probe-b1).

### Unverified / deferred
- [U2 — limita vizuală comună](#limita-u2).

## SLICE-0114-10 - E-Factura: «Sincronizeaza facturi primite» and its inputs in the form group (operator item 5) (17.10.2026) <a id="slice-0114-10"></a>

### What changed and why
`ef_primite_sync` and the two fields (days list, days box) moved from the «ANAF» group of the E-Factura tab to `ribbon.FormGroup`; shown while the received invoices window is on screen (caption «Facturi primite»). «ANAF» keeps only «Token ANAF».

### Files touched
`src/KBot.App/AvacontMain.Designer.vb`, `AvacontMain.vb`.

**Probe:** [B1](#probe-b1).

### Unverified / deferred
- [U2 — limita vizuală comună](#limita-u2).

## SLICE-0114-11 - KbotMain: «Editeaza grupe» out of the popup menu, into the ribbon form group (operator item 6.1) (17.10.2026) <a id="slice-0114-11"></a>

### What changed and why
The row «Editeaza grupe...» and its separator are gone from `menuGrupe` (`KbotForm.Grupe.vb`); new ribbon button `grupe_editeaza` (`FormGroup`, shell only) calls `KbotForm.DeschideEditorulGrupe` (now `Friend`). Tutorial anchor doc updated.

### Files touched
`src/KBot.App/KbotForm.Grupe.vb`, `AvacontMain.vb`, `AvacontMain.Designer.vb`, `KbotForm.Tutorial.vb` (comment).

**Probe:** [B1](#probe-b1).

### Unverified / deferred
- [U2 — limita vizuală comună](#limita-u2).

## SLICE-0114-12 - KbotMain: a popup on the footer «Actualizeaza» button (operator item 6.2) (17.10.2026) <a id="slice-0114-12"></a>

### What changed and why
The right footer icon AND the footer text (`FooterCaptionClicked`) open a `CustomPopup` above the footer (icon bar on the right, as in the received invoices tree): «Descarca angajamente NOI» (what the icon did alone: the list refresh, now `DescarcaAngajamenteNoi`) and «Actualizeaza angajamente VRAC» (`DeschideActualizareaMultipla`). The row «Actualizeaza angajamente...» left the tree options menu. Tutorial signal `footer-menu:<key>` + anchors `popup.noi` / `popup.vrac`. Footer icon tooltip reworded.

### Files touched
`src/KBot.App/KbotForm.Download.vb`, `KbotForm.TreeOptions.vb`, `KbotForm.Tutorial.vb`, `KbotForm.Designer.vb`.

**Probe:** [B1](#probe-b1).

### Unverified / deferred
- [U2 — limita vizuală comună](#limita-u2).

## SLICE-0114-13 - KbotMain: no «Grupe» row when there is no custom group (operator item 6.3) (17.10.2026) <a id="slice-0114-13"></a>

### What changed and why
`ShowTreeOptionsMenu` is async: it asks the server for the groups first (`ExistaGrupeAsync`; no unit, no API or a failed read = no row, logged) and adds «Grupe» only if at least one group exists (or one is active, so «Toate angajamentele» stays reachable). Groups are made with the ribbon button (0114-11).

### Files touched
`src/KBot.App/KbotForm.TreeOptions.vb`, `KbotForm.Grupe.vb`.

**Probe:** [B1](#probe-b1).

### Unverified / deferred
- [U2 — limita vizuală comună](#limita-u2).
- The groups are read from the server every time the options menu opens (one extra call); the menu appears after it answers.

## SLICE-0114-14 - KbotMain: the E-Factura view button under Extrase, aligned top (operator item 6.4) (17.10.2026) <a id="slice-0114-14"></a>

### What changed and why
In `navViews` the `efactura` item is no longer `Align = Far` (the bottom block with Fundamentare / Ordonantare / Note corectie): it is a near (top) item placed right after «Extrase».

### Files touched
`src/KBot.App/KbotForm.Designer.vb`.

**Probe:** [B1](#probe-b1).

### Unverified / deferred
- [U2 — limita vizuală comună](#limita-u2).

## SLICE-0114-15 - Rezervari view: Grafice / document / Repara in the ribbon, tree header and footer emptied (operator items 1, 2) (17.10.2026) <a id="slice-0114-15"></a>

### What changed and why
New interface `IViewRibbon` (+ `ViewButtonState`: Hidden / Dimmed / Enabled) in `Views`: a view of the shell that wants buttons in the ribbon's VIEW group says which keys are its, their state and text, and what a press does; `RibbonStateChanged` tells the window to ask again. `KbotForm` hooks the event of every view that implements it (same path as «Asociaza parteneri») and exposes `ActiveViewRibbon` / `RunViewRibbonButton`; `AvacontMain.UpdateViewGroup` (replaces `UpdateSumarPartnersButton`) shows / dims / hides the buttons, sets their text and the group caption («Sumar», «Rezervari», «Receptii»). `RezervariView` implements it: «Grafice» (the chart, was the header-right icon; dimmed with no reservations), the document button (was the footer-left menu; takes the name of the option, Adauga rezervare / Definitiveaza / Deruleaza / Genereaza PDF final; hidden while a leaf still has the «+» or there is no option), «Repara...» (was the footer-right icon; only when the angajament has broken reservations). The tree of the view has no header or footer icon left (designer: header-right, footer-left, footer-right icons and their tooltips, and the footer caption, removed; the leaf «+» stays).

### Files touched
`src/KBot.App/Views/IViewRibbon.vb` (new), `Views/RezervariView.vb`, `RezervariView.Designer.vb`, `KbotForm.Views.vb`, `AvacontMain.vb`, `AvacontMain.Designer.vb` (three buttons in the view group).

**Probe:** [B1](#probe-b1).

### Unverified / deferred
- [U3 — limita vizuală comună](#limita-u3).

## SLICE-0114-16 - Receptii view: Asociaza receptii / Reface din istoric in the ribbon, only «Actualizeaza» left in the tree (operator items 3, 4) (17.10.2026) <a id="slice-0114-16"></a>

### What changed and why
`ReceptiiView` implements `IViewRibbon`: «Asociaza receptii» (was the header-right icon; there while the view has an angajament) and «Reface din istoric...» (was the footer-left icon; now only when the angajament has errors to repair = a reception whose chain does not close, `AngajamentTreeInfo.LantNeinchis`, the same list that paints its row red in the main tree). The tree keeps only the footer-right «Actualizeaza». The view now keeps the tree info of the angajament (`_info`).

### Files touched
`src/KBot.App/Views/ReceptiiView.vb`, `ReceptiiView.Designer.vb`, `AvacontMain.Designer.vb` (two buttons in the view group).

**Probe:** [B1](#probe-b1).

### Unverified / deferred
- [U3 — limita vizuală comună](#limita-u3).

## SLICE-0114-17 - E-Factura view: no «show all» tick, only the «without operations» filter, footer «Actualizeaza» with the list's icon (operator item 5) (17.10.2026) <a id="slice-0114-17"></a>

### What changed and why
`PrimiteView` with a DDF lists ONLY the invoices linked to it: the tick «Arata toate facturile primite» (control, handler, theme lines, second server read) is gone. The filter menu of the tree header no longer offers «Afisare facturi noi» when the view has a DDF (it stays in the window with all the received invoices); the «fara operatii» filter and the grouping stay. The footer of the received-invoices tree says «Actualizeaza» (was «Actualizare...») and uses the icon of the angajament list's footer (the picture was copied into the resources of KBot.EFactura); the idle text after a sync follows. The change is in the shared control, so the «Facturi primite» window gets the same footer.

### Files touched
`src/KBot.EFactura/Views/Primite/PrimiteView.vb`, `PrimiteView.Designer.vb`, `Resources/Jonas-Rask-Danish-Royalty-Free-Refresh.32.png`, `My Project/Resources.resx`, `Resources.Designer.vb`.

**Probe:** [B1](#probe-b1).

### Unverified / deferred
- [U3 — limita vizuală comună](#limita-u3).
- The grouping options stay in the filter menu of the view (only the «noi» filter was removed): the operator said «only the without-operations option», read as only the filter.

## SLICE-0114-18 - Ribbon fields of «Sincronizeaza facturi primite»: the list opens and has the rows, the second one is a text box (operator item 6) (17.10.2026) <a id="slice-0114-18"></a>

### What changed and why
Cause: the generated designer code adds a field to the group BEFORE it sets the field's kind, rows and hint, and the ribbon made the real control the moment the field was added: both fields became an empty combo (so the first had nothing to open and the second was not a text box). `KBotRibbon` now makes the controls of the fields on the first layout (`EnsureFieldControls`), when every property is in. The look follows the popup of the invoices tree: the list shows the pale text «Perioada de adus» (no row picked at start; `KBotRibbonField.PlaceholderText` now also works for a combo) and the box «Intre 1 si 60 de zile». Nothing picked and nothing typed = 7 days (said in the tooltip). The icon bar of the popup (the ANAF picture) has no ribbon counterpart.

### Files touched
`src/KBot.Controls/Ribbon/KBotRibbon.vb`, `KBotRibbonItems.vb`, `src/KBot.App/AvacontMain.vb`, `AvacontMain.Designer.vb`.

**Probe:** [B1](#probe-b1).

### Unverified / deferred
- [U3 — limita vizuală comună](#limita-u3).

## Probe și limite comune — numai pentru etapele care trimit aici

**B1.** `dotnet build src/KBot.App/KBot.App.vbproj`: 0 warnings, 0 errors. Nothing run, nothing seen on screen. <a id="probe-b1"></a>

**U2.** Not seen on screen: layout of the two ribbon groups side by side, the footer popup position, the nav order. <a id="limita-u2"></a>

**U3.** Not seen on screen: the new buttons in the view group, the popup of the document button (opens at the mouse), the fields in the ribbon, the tree rows of the issued invoices. <a id="limita-u3"></a>

## Trasabilitate ajutor păstrată din registru

| Etape | Ajutor |
|---|---|
| 0114 | 0000-70 |
| 0114-02/03 | 0000-85 |
| 0114-04 | 0000-88 |
| 0114-05 | 0000-89 |
| 0114-06…14 | 0000-90 |
| 0114-15…18 | 0000-91 |

## SLICE-0114-24 - Ribbon: compact mode and a temporary panel while it is folded (18.10.2026) <a id="slice-0114-24-ribbon"></a>

### What changed and why
Operator, two points. On low resolutions the ribbon takes too much of the screen; and a folded ribbon gave no way to use a button without unfolding it for good.

1. **Compact ribbon.** New setting `AppSettings.RibbonCompact` (default False) and a tick in Setari > Aplicatie > group «Fereastra principala» (`chkRibbonCompact`, declared in `SetariAplicatieView.Designer.vb`; the rows below it moved down one, `RowCount` 16). `KBotRibbon.Compact` (new property) swaps the measures: tab strip 28 -> 22, large icon 32 -> 24, small icon 16 -> 14, small row 22 -> 18, group caption 18 -> 14 (logical px, still scaled with the DPI), and the text uses `BodyFont()` = the control's font x 0.88 (min 6.5 pt); the caption / bold / right-text fonts derive from it. The input fields of the ribbon (combo, text box) take the same smaller font, so their height follows. `AvacontMain` sets `ribbon.Compact` at start and again on `AppSettings.Changed`, so the tick works at once, no restart.
2. **Fly-out while folded.** A single click on a tab of a folded ribbon opens that tab's panel as a temporary overlay (`KBotRibbonFlyout`, new file): a child of the form, brought to front, outside the docking, so the open page does not move. The panel is laid out by the ribbon itself in the fly-out's own coordinates (panel top 0 while folded), shows the groups of the tab plus the form group and the view group (they were not reachable at all while folded), and the fields stand in it while it is open (reparented, and put back on close). It closes: after a button was used (closed BEFORE `ItemClicked` is raised, so a window the command opens is not under it), on a press anywhere else in the form (`OutsideClickFilter`, a message filter that never eats the press; a press in the list window of a combo is ignored; a press on the tab strip is left to the ribbon), on **Esc** (eaten only when it closed the fly-out and no combo list is open), when the form is deactivated / moved / resized, when the ribbon is hidden or resized. Clicking the open tab again closes it; clicking another tab shows that tab. **Double click** on a tab, or the chevron, unfolds for good (the old «a click on a tab unfolds» is gone: it is the fly-out now).

### Files touched
`src/KBot.Controls/Ribbon/KBotRibbon.vb`, new `src/KBot.Controls/Ribbon/KBotRibbonFlyout.vb`; `src/KBot.Common/AppSettings.vb`; `src/KBot.App/AvacontMain.vb`; `src/KBot.App/Setari/SetariAplicatieView.vb`, `.Designer.vb`. Help `contabil/ribbon.md`, `contabil/setari.md` (0000-100).

### Test results
`dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors. No tests run, nothing run or seen on screen.

### Unverified / deferred
- Never run: the fly-out position (just under the folded ribbon), that the page below really does not move, the look of the compact ribbon at 100 / 125 / 150 %, the height of the fields in compact (they use the smaller font; their own padding may still make them taller than a small row), the fly-out over a hosted Adobe / Excel window (a press inside a foreign window never reaches the message filter, so the fly-out then stays until another click / Esc / the form loses focus).
- The panel height (`PanelPx`) does not include the fields (as before 0114-24); in compact a field row is the height its smaller font asks for.
- Captures: those showing the ribbon were not marked for redo (the normal size is unchanged).
