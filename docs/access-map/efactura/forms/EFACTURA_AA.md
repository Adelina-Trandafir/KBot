# Form `EFACTURA_AA`

Source: `Forms/EFACTURA_AA.txt` (32,800 chars). Units are twips (1 inch = 1440). `Parent` is the tab page or group a control sits in, or - for a label - the control it is attached to. Positions are relative to the section that holds the control; `LayoutCached*` values are used when present.

## Form properties

| Property | Value |
|---|---|
| Caption | Transfer E-Facturi din anul anterior |
| DefaultView | 0 |
| PopUp | NotDefault |
| AutoCenter | NotDefault |
| BorderStyle | 3 |
| NavigationButtons | NotDefault |
| RecordSelectors | NotDefault |
| DividingLines | NotDefault |
| ScrollBars | 0 |
| Width | 7334 |
| Left | 9730 |
| Top | 6270 |
| Right | 17060 |
| Bottom | 13570 |
| FitToScreen | 1 |
| GridY | 10 |
| TabularCharSet | 238 |
| DatasheetFontName | Calibri |
| DatasheetFontHeight | 11 |

Form events wired: `OnClose` = VBA, `OnLoad` = VBA

## Sections

| Section | Height | Visible | BackColor | Events |
|---|---|---|---|---|
| FormHeader | 740 |  | 15849926 |  |
| Detail | 6122 |  | 1 |  |
| FormFooter | 453 |  | 1 |  |

## Data

- Unbound form (data loaded/saved by code).

## Controls

| # | Type | Name | Parent | Section | Pos L,T WxH | Caption | ControlSource | RowSource | Tab | Font | Events | Other |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| 1 | Label | Label0 |  | FormHeader | 0,0 7310x740 | ATENȚIE! În copacul de mai jos vei regăsi DOAR facturile neînregistrate în anul din care fac parte.\015\012Cele înregistrare vor putea fi achitate din macheta [Plăți Facturi]. |  |  |  | Consolas 9 |  |  |
| 2 | Subform | subTreeView |  | Section | 0,0 7334x6017 |  |  |  |  |  |  |  |
| 3 | Button | IESIRE |  | FormFooter | 60,0 1872x403 |  IEȘIRE |  |  |  | Consolas 10 bold | OnClick |  |
| 4 | Button | bTra |  | FormFooter | 5550,0 1722x403 | Transferă  |  |  | 1 | Consolas 9 bold | OnClick |  |

## Module-level declarations

Form state kept in variables, constants, types and WithEvents helpers (the Access form class body before the first procedure).

```vb
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = True
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
Public WithEvents mcTree As clsTreeView
Attribute mcTree.VB_VarHelpID = -1
Private AnAnterior As Long
Private txtCale As String
Private txtCale2 As String
```

## Event handlers and what they touch

Reads/writes come from SQL text and recordset calls found in the procedure body; they are a lead for reading the code, not a proof (dynamic SQL built from variables is missed).

| Procedure | Bound to | Lines | Opens forms/reports | Reads | Writes | Calls (modules) | Classes | Local calls |
|---|---|---|---|---|---|---|---|---|
| bTra_Click | bTra.OnClick | 49 |  |  | `tmpEF_TRANS`, `EF`, `EFT`, `EFS`, `EFT_C` |  |  | `Pop_EFACTURA_AA` |
| Form_Close | Form.OnClose | 18 |  |  |  |  |  |  |
| Form_Load | Form.OnLoad | 18 |  |  |  | `mdl_GLOBALS.globANL` |  | `Init_Pop_EFACTURA_AA` |
| Init_Pop_EFACTURA_AA |  | 17 |  | `EFACTURA_AA` |  |  |  |  |
| Pop_EFACTURA_AA (Public) |  | 72 |  | `CALE`, `EFACTURA_AA` |  | `mdl_Functii2023.fLuna` |  |  |
| Iesire_Click | IESIRE.OnClick | 4 |  |  |  |  |  |  |
| mcTree_TreeLoaded | WithEvents mcTree (clsTreeView).TreeLoaded | 3 |  |  |  |  |  | `Pop_EFACTURA_AA` |

## Opened from

`form:EFACTURA_2025`

Raw source: `Surse/RawExport/Forms/EFACTURA_AA.txt`