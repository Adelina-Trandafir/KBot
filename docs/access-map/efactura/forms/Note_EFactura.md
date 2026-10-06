# Form `Note_EFactura`

Source: `Forms/Note_EFactura.txt` (42,108 chars). Units are twips (1 inch = 1440). `Parent` is the tab page or group a control sits in, or - for a label - the control it is attached to. Positions are relative to the section that holds the control; `LayoutCached*` values are used when present.

## Form properties

| Property | Value |
|---|---|
| RecordSource | `tmpNote_EFactura` |
| AutoCenter | NotDefault |
| NavigationButtons | NotDefault |
| RecordSelectors | NotDefault |
| DividingLines | NotDefault |
| ScrollBars | 2 |
| AllowAdditions | NotDefault |
| AllowDeletions | NotDefault |
| Width | 11687 |
| Left | 3470 |
| Top | 7520 |
| Right | 16500 |
| Bottom | 9300 |
| FitToScreen | 1 |
| GridX | 60 |
| GridY | 60 |
| TabularCharSet | 161 |
| DatasheetFontName | Consolas |

Form events wired: `OnLoad` = VBA

## Sections

| Section | Height | Visible | BackColor | Events |
|---|---|---|---|---|
| FormHeader | 300 |  | 15527148 |  |
| Detail | 300 |  |  |  |
| FormFooter | 0 | NotDefault | 15527148 |  |

## Data

- RecordSource: `tmpNote_EFactura`
- Tables/queries behind the RecordSource: `tmpNote_EFactura`

## Controls

| # | Type | Name | Parent | Section | Pos L,T WxH | Caption | ControlSource | RowSource | Tab | Font | Events | Other |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| 1 | Label | NumarDocument_Label |  | FormHeader | 292,0 1425x300 | Solicitare |  |  |  | Arial Narrow 9 bold |  |  |
| 2 | Label | Suma_Label |  | FormHeader | 3142,0 1425x300 | Nr. Factură |  |  |  | Arial Narrow 9 bold |  |  |
| 3 | Label | Label34 |  | FormHeader | 1717,0 1425x300 | Încărcare |  |  |  | Arial Narrow 9 bold |  |  |
| 4 | Label | Label55 |  | FormHeader | 4567,0 1479x300 | Dată Factură |  |  |  | Arial Narrow 9 bold |  |  |
| 5 | Label | Label57 |  | FormHeader | 6046,0 1263x300 | Total |  |  |  | Arial Narrow 9 bold |  |  |
| 6 | Label | Label66 |  | FormHeader | 9828,0 1263x300 | Rămas |  |  |  | Arial Narrow 9 bold |  |  |
| 7 | Label | Label68 |  | FormHeader | 8565,0 1263x300 | Alte Asocieri |  |  |  | Arial Narrow 9 bold |  |  |
| 8 | Label | Label70 |  | FormHeader | 7309,0 1263x300 | Asociere Curentă |  |  |  | Arial Narrow 9 bold |  |  |
| 9 | TextBox | Bck |  | Section | 0,0 11683x300 |  |  |  |  | Calibri 9 | OnEnter |  |
| 10 | TextBox | id_sol |  | Section | 292,0 1425x300 |  | id_sol |  | 1 | Consolas 9 |  |  |
| 11 | TextBox | id |  | Section | 1717,0 1425x300 |  | id |  | 2 | Consolas 9 |  |  |
| 12 | TextBox | NrFact |  | Section | 3142,0 1425x300 |  | NrFact |  | 3 | Consolas 9 |  |  |
| 13 | TextBox | DataFact |  | Section | 4567,0 1479x300 |  | DataFact |  | 4 | Consolas 9 |  |  |
| 14 | TextBox | TOTAL |  | Section | 6046,0 1263x300 |  | TOTAL |  | 5 | Consolas 9 |  | fmt=Standard |
| 15 | Button | bXML |  | Section | 11091,0 298x300 | Deschide XML    |  |  | 6 | Consolas 9 bold |  | picture=Hopstarter-Adobe-Cs4-File-Adobe-Dreamweaver-XML-01.16.png; tip=Deschide fișierul sursă (XML) |
| 16 | Button | bFact |  | Section | 11389,0 298x300 |  Listează Factură |  |  | 7 | Consolas 9 bold | OnClick | tip=Listează factura în formatul clasic |
| 17 | Rectangle | Box63 |  | Section | 0,0 278x300 |  |  |  |  |  |  |  |
| 18 | CheckBox | S |  | Section | 40,40 208x199 |  | S |  | 8 |  |  |  |
| 19 | TextBox | DeAsociat |  | Section | 9828,0 1263x300 |  | DeAsociat |  | 9 | Consolas 9 |  | fmt=Standard |
| 20 | OleFrame(bound) | bndPic |  | Section | 40,40 199x199 |  | Pic |  | 10 |  |  |  |
| 21 | TextBox | AlteAsocieri |  | Section | 8565,0 1263x300 |  | AlteAsocieri |  | 11 | Consolas 9 |  | fmt=Standard |
| 22 | TextBox | Asociat |  | Section | 7309,0 1263x300 |  | Asociat |  | 13 | Consolas 9 |  | fmt=Standard |
| 23 | Button | Btn |  | Section | 0,0 11103x300 | Command32 |  |  | 12 | Calibri 9 | OnClick |  |

## Module-level declarations

Form state kept in variables, constants, types and WithEvents helpers (the Access form class body before the first procedure).

```vb
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = True
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
Public initIDEFT As Variant
```

## Event handlers and what they touch

Reads/writes come from SQL text and recordset calls found in the procedure body; they are a lead for reading the code, not a proof (dynamic SQL built from variables is missed).

| Procedure | Bound to | Lines | Opens forms/reports | Reads | Writes | Calls (modules) | Classes | Local calls |
|---|---|---|---|---|---|---|---|---|
| Bck_Enter | Bck.OnEnter | 4 |  |  |  |  |  |  |
| bFact_Click | bFact.OnClick | 9 | `Note_PDF_EF` |  |  | `mdl_CheckTVW.FileExists`, `mdl_EFactura.DescarcaFacturaPDF` |  |  |
| Btn_Click | Btn.OnClick | 86 | `Note` |  | `tmpNote_EFactura` | `mdl_Functii2023.mMin`, `mdl_MsBox.MsBox` |  |  |
| Form_Load | Form.OnLoad | 88 |  | `qNote_EFactura_Toate`, `_TempCredit`, `IMG` | `tmpNote_EFactura`, `tmpMF_EFactura` | `mdl_MsBox.MsBox`, `mdl_Functii2023.mmin` |  |  |

## Opened from

Nothing found by static scan (probably opened by name from a ribbon, a menu string or a variable).

Raw source: `Surse/RawExport/Forms/Note_EFactura.txt`