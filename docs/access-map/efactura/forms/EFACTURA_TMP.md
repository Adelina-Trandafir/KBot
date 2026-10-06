# Form `EFACTURA_TMP`

Source: `Forms/EFACTURA_TMP.txt` (35,587 chars). Units are twips (1 inch = 1440). `Parent` is the tab page or group a control sits in, or - for a label - the control it is attached to. Positions are relative to the section that holds the control; `LayoutCached*` values are used when present.

## Form properties

| Property | Value |
|---|---|
| Caption | FACTURI PRIMITE |
| DefaultView | 0 |
| PopUp | NotDefault |
| AutoCenter | NotDefault |
| BorderStyle | 3 |
| NavigationButtons | NotDefault |
| RecordSelectors | NotDefault |
| DividingLines | NotDefault |
| ScrollBars | 0 |
| Width | 6579 |
| Left | 8115 |
| Top | 2700 |
| Right | 14700 |
| Bottom | 10800 |
| FitToScreen | 1 |
| GridY | 10 |
| TabularCharSet | 238 |
| DatasheetFontName | Calibri |
| DatasheetFontHeight | 11 |

Form events wired: `OnClose` = VBA, `OnLoad` = VBA

## Sections

| Section | Height | Visible | BackColor | Events |
|---|---|---|---|---|
| FormHeader | 850 |  |  |  |
| Detail | 6689 |  | 1 |  |
| FormFooter | 566 |  | 1 |  |

## Data

- Unbound form (data loaded/saved by code).

## Controls

| # | Type | Name | Parent | Section | Pos L,T WxH | Caption | ControlSource | RowSource | Tab | Font | Events | Other |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| 1 | Label | Label0 |  | FormHeader | 0,0 6579x850 | ATENȚIE! Data nu reprezintă ziua în care a fost emisă factura, ci ziua în care a fost încărcată în sistemul E-FACTURĂ! |  |  |  | Consolas 11 bold |  |  |
| 2 | Subform | subTreeView |  | Section | 56,56 6463x6609 |  |  |  |  |  |  |  |
| 3 | Button | bDWN |  | FormFooter | 3160,113 3308x360 |  Descarcă fact. selectate |  |  |  | Consolas 9 bold | OnClick |  |
| 4 | Button | EXIT |  | FormFooter | 56,120 1500x360 |  IEȘIRE |  |  | 1 | Consolas 9 bold | OnClick |  |

## Module-level declarations

Form state kept in variables, constants, types and WithEvents helpers (the Access form class body before the first procedure).

```vb
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = True
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
Public WithEvents mcTree As clsTreeView
Attribute mcTree.VB_VarHelpID = -1
Private Mtr As clsMeter
Public AppName As String
```

## Event handlers and what they touch

Reads/writes come from SQL text and recordset calls found in the procedure body; they are a lead for reading the code, not a proof (dynamic SQL built from variables is missed).

| Procedure | Bound to | Lines | Opens forms/reports | Reads | Writes | Calls (modules) | Classes | Local calls |
|---|---|---|---|---|---|---|---|---|
| bDWN_Click | bDWN.OnClick | 135 |  |  | `EF`, `EF_TMP`, `UNIT` | `mdl_EFactura.DescarcaFactura`, `mdl_EFactura.entUnZip1File`, `mdl_EFactura.ParseXML` |  | `Pop_Tree_Efactura_Tmp` |
| EXIT_Click | EXIT.OnClick | 4 |  |  |  |  |  |  |
| Form_Close | Form.OnClose | 37 |  | `EF_TMP` |  | `mdl_Functii2023.isLoaded` |  |  |
| Form_Load | Form.OnLoad | 19 |  |  |  |  |  | `Init_Tree_EFACTURA_TMP` |
| Init_Tree_EFACTURA_TMP |  | 16 |  |  |  |  |  |  |
| Pop_Tree_Efactura_Tmp (Public) |  | 49 |  | `EF_TMP` |  |  |  |  |
| mcTree_Checked | WithEvents mcTree (clsTreeView).Checked | 22 |  |  | `EF_TMP` |  |  |  |
| mcTree_TreeLoaded | WithEvents mcTree (clsTreeView).TreeLoaded | 3 |  |  |  |  |  | `Pop_Tree_Efactura_Tmp` |

## Opened from

`module:mdl_EFactura`

Raw source: `Surse/RawExport/Forms/EFACTURA_TMP.txt`