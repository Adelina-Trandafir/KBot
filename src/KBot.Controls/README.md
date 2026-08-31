# KBot.Controls

**Toate controalele K-BOT trăiesc aici.** Nu există control K-BOT în alt proiect: nici în
`KBot.Theming` (acolo a rămas doar motorul de teme plus form-urile de bază), nici în
`KBot.App`.

## Regula de organizare

Fiecare **familie de control** are folderul ei, iar în folder intră controlul ÎMPREUNĂ cu
tot ce ține de el — enum-uri, colecții, `EventArgs`, modele, form-uri ajutătoare. Gruparea
e logică (după control), nu după felul fișierului.

| Folder | Conține |
|---|---|
| `Tree/` | `AdvancedTreeControl` (+ partial-urile `.API`/`.Painting`/`.Popup*`/…), `ColumnDef`, `TreeLogger`, `TooltipTableModel`, `NodeDebugInfo`, `FrmNodeDebug` |
| `DataView/` | `KBotDataView` (+ partial-urile `.Layout`/`.Painting`/`.Editing`/…), `KBotDataColumn`, `KBotDataColumnCollection`, `KBotDataRow`, enum-urile `KBotAggregate`/`KBotAutoSizeMode`/`KBotColumnType`/`KBotFillMode`; `DataView/Events/` ține `KBotCell*`/`KBotRow*`/`KBotButtonClickEventArgs` |
| `Adobe/` | vizualizatorul Adobe: `AcroPdfHost`, `AcroPdfSurface`, găzduirea nativă (`AdobeReaderHost`, `AdobeWindow*`, `Adobe*Watcher`, registry, `IHostSurface`) |
| `NavList/` | `KBotNavList`, `KBotNavItem`, `KBotNavItemCollection`, `KBotNavFlyout` + `KBotNavFlyoutStyle` (eticheta plutitoare a barei strânse), enum-urile `KBotNavOrientation`/`KBotNavAlign`/`KBotNavCorner`/`KBotNavCollapseState` |
| `Popup/` | `CustomPopup` (+ partialele `.Painting`/`.Input`), `CustomPopupItem`, `CustomPopupItemCollection`, `CustomPopupItemEventArgs`, `PopupMnemonic` (litera de acces, funcție pură), `IPopupAnchor` (controlul care desfășoară meniul rămâne aprins cât e deschis) |
| `CaptionBar/` | `KBotCaptionBar` |
| `BusyBar/` | `KBotBusyBar` |
| `Notice/` | `KBotNotice`, `NoticeKind` |
| `TextField/` | `KBotTextField` (câmpul de o linie de pe `LoginForm`) și `KBotTextBox` (caseta generală: chenar reglabil ca CULOARE și GROSIME, multilinie, cu bare `KBotScrollBar` proprii în locul celor native) |
| `Scroll/` | `KBotScrollBar` — bara de derulare desenată de noi. Semantica intervalului e a lui `System.Windows.Forms.ScrollBar` (`Minimum .. Maximum - LargeChange + 1`), dar fața e a paletei: barele native sunt ferestre pictate de Windows, deci nicio culoare a schemei nu ajunge pe ele |
| `Label/` | `KBotLabel` — eticheta cu chenar propriu (culoare + grosime + rază). `BorderStyle` moștenit e ascuns și refuzat: cele trei valori native se desenează în culorile SISTEMULUI |
| `ToolTip/` | `KBotToolTip` (componenta `IExtenderProvider`), `KBotToolTipContent`, `KBotToolTipStyle` + `KBotToolTipBand`/`KBotToolTipSeparator`, `KBotToolTipWindow` (fereastra) și `KBotRichText` (motorul de text îmbogățit: analiză, așezare, desen — pur, măsurabil fără ecran) |
| `Chart/` | `KBotChartView` (+ partial-ul `.Painting`) — graficul de timp desenat de noi, cu banda SINGLE-SELECT de butoane pe post de tabcontrol; `KBotChartSeries`/`KBotChartSeriesCollection`, `KBotChartPoint`/`KBotChartPointCollection`, `KBotChartTab`/`KBotChartTabCollection`, enum-urile `KBotChartMarkerStyle`/`KBotChartTabAlign`/`KBotChartValueAxisMode`. Singurul folder scris integral în engleză, inclusiv numele categoriilor din grila de proprietăți (felia 0048-05) |

Un control nou => un folder nou. Nu se adaugă fișiere de control în rădăcina proiectului
(acolo rămân doar `AssemblyInfo.vb`, `.vbproj` și acest README).

## Referințe

Sensul e `KBot.Controls → KBot.Theming`, niciodată invers — `KBot.Theming` NU referă
proiectul ăsta, altfel s-ar face ciclu. De aceea ce-i trebuie unui control din motor e
expus `Public` acolo: `ThemeManager`/`ThemePalette`, `IThemedControl`, `ThemeShapes`
(`ScaleDpi`/`RoundedRect`/`Blend`), `KBotDesignTime` și `NativeMethods.DragMove`.

`KBot.Common` e referit doar pentru `GlobalErrorLog` (sink-ul global de erori).

## Ce NU intră aici

- **`KBotThemedForm` / `KBotShellForm`** — sunt form-uri de bază, nu controale; stau în
  `KBot.Theming`, lângă motorul de teme. Nu se confundă cu ferestrele-ajutor ale unui control
  (`KBotNavFlyout`, `TreeNodeFlyout`, `CustomPopup`): acelea moștenesc `Form` fiindcă au nevoie
  de HWND propriu, dar aparțin controlului lor și stau în folderul familiei.
- **Vederile din `KBot.App\Views\`** (`SumarView`, `IstoricView`, `DdfView`,
  `DdfFileBrowser`, `XfaXmlPreview`, `ReaderHostPreview`, `PlaceholderView`) — sunt ecrane
  ale aplicației: implementează `IAngajamentView` și cheamă API-ul. Mutate aici, ar trage
  `KBot.Api`/`KBot.Forexe`/`KBot.Xfa` într-o bibliotecă de UI care trebuie să rămână frunză.

## Note istorice

`AdvancedTreeControl` e importat din TREEVIEW_VBA cu podul Access tăiat (forma-gazdă
`Tree.vb`, `TrimiteMesajAccess`, `ProcesareComandaAccess`, argumentele `/frm /acc /idt /log`,
API-ul pe string-uri `SET_CHECKBOX||NodeID||State`). A rămas controlul reutilizabil cu
evenimentele native (`NodeChecked`, `NodeRadioSelected`, `RightIconClicked`,
`SearchFinished`) consumate direct. Rămâne **ne-tematizat** — shell-ul îi împinge culorile
prin proprietăți publice; convenția `IThemedControl` se aplică restului controalelor.
