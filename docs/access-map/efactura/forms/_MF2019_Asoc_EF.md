# Form `_MF2019_Asoc_EF`

Source: `Forms/_MF2019_Asoc_EF.txt` (58,344 chars). Units are twips (1 inch = 1440). `Parent` is the tab page or group a control sits in, or - for a label - the control it is attached to. Positions are relative to the section that holds the control; `LayoutCached*` values are used when present.

## Form properties

| Property | Value |
|---|---|
| Caption | Asociere E-Factură pentru `Part` |
| AutoCenter | NotDefault |
| BorderStyle | 3 |
| NavigationButtons | NotDefault |
| RecordSelectors | NotDefault |
| DividingLines | NotDefault |
| ScrollBars | 2 |
| AllowAdditions | NotDefault |
| AllowDeletions | NotDefault |
| Width | 7781 |
| Left | 1920 |
| Top | 2625 |
| Right | 9735 |
| Bottom | 7575 |
| FitToScreen | 1 |
| GridX | 24 |
| GridY | 24 |
| TabularCharSet | 238 |
| DatasheetFontName | Calibri |
| DatasheetFontHeight | 11 |

Form events wired: `OnLoad` = VBA

## Sections

| Section | Height | Visible | BackColor | Events |
|---|---|---|---|---|
| FormHeader | 300 |  | 15527148 |  |
| PageHeaderSection | 300 |  | 1 |  |
| Detail | 300 |  | 1 |  |
| PageFooterSection | 0 |  | 1 |  |
| FormFooter | 420 |  | 1 |  |

## Data

- Unbound form (data loaded/saved by code).

## Controls

| # | Type | Name | Parent | Section | Pos L,T WxH | Caption | ControlSource | RowSource | Tab | Font | Events | Other |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| 1 | Label | Text14 |  | FormHeader | 0,0 2595x300 | Număr factură |  |  |  | Consolas 9 bold |  |  |
| 2 | Label | Text16 |  | FormHeader | 2598,0 1620x300 | Dată Factură |  |  |  | Consolas 9 bold |  |  |
| 3 | Label | Text17 |  | FormHeader | 4218,0 1485x300 | Total factură |  |  |  | Consolas 9 bold |  |  |
| 4 | Label | Text18 |  | FormHeader | 5703,0 2075x300 | Alte asocieri |  |  |  | Consolas 9 bold |  |  |
| 5 | TextBox | CL |  | PageHeader | 0,0 660x300 |  |  |  |  | Calibri 11 |  |  |
| 6 | TextBox | Bck |  | Section | 0,0 7185x300 |  |  |  | 3 | Calibri 11 |  |  |
| 7 | TextBox | NrFact |  | Section | 0,0 2595x300 |  | NrFact |  | 1 | Consolas 9 bold |  |  |
| 8 | TextBox | DataFact |  | Section | 2595,0 1620x300 |  | DataFact |  | 2 | Consolas 9 bold |  |  |
| 9 | TextBox | TOTAL |  | Section | 4215,0 1485x300 |  | TOTAL |  | 4 | Consolas 9 bold |  | fmt=Standard |
| 10 | TextBox | Asociat |  | Section | 5700,0 1485x300 |  | Asociat |  | 5 | Consolas 9 bold |  | fmt=Standard |
| 11 | OleFrame(bound) | bndPic |  | Section | 60,50 199x199 |  | Pic |  | 6 |  |  |  |
| 12 | Button | Btn |  | Section | 0,0 7185x300 | Command8 |  |  |  | Calibri 11 | OnClick, OnDblClick |  |
| 13 | CheckBox | S |  | Section | 50,50 213x236 |  | S |  | 7 |  |  |  |
| 14 | Button | bXML |  | Section | 7185,0 298x300 | Deschide XML    |  |  | 8 | Consolas 9 bold |  | picture=Hopstarter-Adobe-Cs4-File-Adobe-Dreamweaver-XML-01.16.png; tip=Deschide fișierul sursă (XML) |
| 15 | Button | bFact |  | Section | 7483,0 298x300 |  Listează Factură |  |  | 9 | Consolas 9 bold |  | tip=Listează factura în formatul clasic |
| 16 | Button | SavFact |  | FormFooter | 5716,0 2065x420 | Salvează date   |  |  |  | Consolas 9 bold | OnClick |  |
| 17 | Button | Iesire |  | FormFooter | 0,0 2150x420 |       IEȘIRE |  |  | 1 | Consolas 9 bold |  | tip=Close Form |

## Module-level declarations

Form state kept in variables, constants, types and WithEvents helpers (the Access form class body before the first procedure).

```vb
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = True
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
Private initIDEFT As Long
```

## Event handlers and what they touch

Reads/writes come from SQL text and recordset calls found in the procedure body; they are a lead for reading the code, not a proof (dynamic SQL built from variables is missed).

| Procedure | Bound to | Lines | Opens forms/reports | Reads | Writes | Calls (modules) | Classes | Local calls |
|---|---|---|---|---|---|---|---|---|
| Btn_Click | Btn.OnClick | 38 | `MF2019` |  | `tmpMF_EFactura` |  |  |  |
| Btn_DblClick | Btn.OnDblClick | 6 |  |  |  |  |  | `SavFact_Click` |
| Form_Load | Form.OnLoad | 103 | `MF2019` | `qMF_EFactura_Toate`, `_TempCredit`, `IMG` | `tmpMF_EFactura` | `mdl_MsBox.MsBox`, `mdl_Functii2023.mmin` |  |  |
| SavFact_Click | SavFact.OnClick | 42 | `MF2019` |  | `Oper`, `EFT`, `EFT_O` | `mdl_MsBox.MsBox`, `mdl_GLOBALS.globSectorSursa`, `mdl_GLOBALS.globIdUnitate` |  |  |

## Opened from

`form:MF2019_Asoc_EF`

Raw source: `Surse/RawExport/Forms/_MF2019_Asoc_EF.txt`