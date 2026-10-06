# Form `EF_P`

Source: `Forms/EF_P.txt` (26,816 chars). Units are twips (1 inch = 1440). `Parent` is the tab page or group a control sits in, or - for a label - the control it is attached to. Positions are relative to the section that holds the control; `LayoutCached*` values are used when present.

## Form properties

| Property | Value |
|---|---|
| Caption | tmpDoc2017 subform |
| RecordSource | `tmpDoc2017P` |
| DefaultView | 0 |
| AutoCenter | NotDefault |
| NavigationButtons | NotDefault |
| RecordSelectors | NotDefault |
| DividingLines | NotDefault |
| ScrollBars | 0 |
| AllowAdditions | NotDefault |
| Width | 9097 |
| Left | 3490 |
| Top | 1040 |
| Right | 11490 |
| Bottom | 3540 |
| FitToScreen | 1 |
| GridX | 60 |
| GridY | 60 |
| TabularCharSet | 204 |
| DatasheetFontName | Calibri |
| DatasheetFontHeight | 11 |

Form events wired: `OnClose` = VBA, `OnLoad` = VBA, `OnError` = VBA

## Sections

| Section | Height | Visible | BackColor | Events |
|---|---|---|---|---|
| FormHeader | 295 |  | 15527148 |  |
| Detail | 664 |  |  |  |
| FormFooter | 286 |  | 15527148 |  |

## Data

- RecordSource: `tmpDoc2017P`
- Tables/queries behind the RecordSource: `tmpDoc2017P`
- Combo/list sources: `Documente`

## Controls

| # | Type | Name | Parent | Section | Pos L,T WxH | Caption | ControlSource | RowSource | Tab | Font | Events | Other |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| 1 | Label | lDOCument |  | FormHeader | 2751,0 1235x295 | Număr OP |  |  |  | Consolas 9 bold |  |  |
| 2 | Label | lDATA |  | FormHeader | 1348,0 1403x295 | Dată |  |  |  | Consolas 9 bold |  |  |
| 3 | Label | lValoare |  | FormHeader | 7378,0 1416x295 | Valoare |  |  |  | Consolas 9 bold |  |  |
| 4 | Label | lNC |  | FormHeader | 0,0 1348x295 | NC |  |  |  | Consolas 9 bold |  |  |
| 5 | Label | lALG |  | FormHeader | 8794,0 303x295 | FX |  |  |  | Consolas 9 bold |  |  |
| 6 | Label | lExplicatieOP |  | FormHeader | 3985,0 3393x295 | Explicație O.P. |  |  |  | Consolas 9 bold |  |  |
| 7 | TextBox | Document |  | Section | 2751,0 1235x289 |  | Document |  | 2 | Consolas 9 | OnGotFocus, OnLostFocus | valid=IsNumeric([document])=True Or Is Null |
| 8 | TextBox | Data |  | Section | 1348,0 1403x289 |  | Data |  | 1 | Consolas 9 | BeforeUpdate |  |
| 9 | TextBox | Valoare |  | Section | 7378,0 1416x289 |  | Valoare |  | 4 | Consolas 9 |  | fmt=Standard |
| 10 | TextBox | ExplicatieOP |  | Section | 3985,0 3393x664 |  | ExplicatieOP |  | 3 | Consolas 9 | OnKeyPress |  |
| 11 | CheckBox | ALG |  | Section | 8850,45 216x204 |  | ALG |  | 5 |  |  | tag=○\|●; tip=Numar / Data Ang. Leg.: / |
| 12 | Button | BFX |  | Section | 8809,0 288x283 | Command45 |  |  | 6 | Calibri 11 | OnClick |  |
| 13 | ComboBox | NC |  | Section | 0,0 1348x290 |  | NC | `SELECT Documente.NumarNC, First(Documente.[datadocument]) AS DI, Last(Documente.[datadocument]) AS DSF FROM Documente GROUP BY Documente.NumarNC HAVING (((Docum...` |  | Consolas 9 |  | rowType=Table/Query; cols=3; colW=567;1420;1420 |
| 14 | Button | FOC |  | Section | 0,0 143x136 | Command46 |  |  | 7 | Calibri 11 |  |  |
| 15 | Label | Ex |  | FormFooter | 0,0 9075x286 | Explicația depășește limita de 70 de caractere! |  |  |  | Consolas 10 |  |  |

## Module-level declarations

Form state kept in variables, constants, types and WithEvents helpers (the Access form class body before the first procedure).

```vb
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = True
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
Public WithEvents DP As clsDatePicker
Attribute DP.VB_VarHelpID = -1
```

## Event handlers and what they touch

Reads/writes come from SQL text and recordset calls found in the procedure body; they are a lead for reading the code, not a proof (dynamic SQL built from variables is missed).

| Procedure | Bound to | Lines | Opens forms/reports | Reads | Writes | Calls (modules) | Classes | Local calls |
|---|---|---|---|---|---|---|---|---|
| BFX_Click | BFX.OnClick | 47 |  | `tmpFacturi` |  | `mdl_MsBox.MsBox` |  |  |
| Data_BeforeUpdate | Data.BeforeUpdate | 29 |  | `tmpFacturi` |  | `mdl_MsBox.ptMS`, `mdl_GLOBALS.globANL`, `mdl_Sendkeys.SendKeysAPI` |  |  |
| Document_GotFocus | Document.OnGotFocus | 8 | `EFACTURA_2025` |  |  |  |  |  |
| Document_LostFocus | Document.OnLostFocus | 17 | `EFACTURA_2025` |  |  | `mdl_Functii_Generale.NumarOPFolosit`, `mdl_MsBox.MsBox`, `mdl_MsBox.ptMS` |  |  |
| ExplicatieOP_KeyPress | ExplicatieOP.OnKeyPress | 18 |  |  |  |  |  |  |
| Form_Close | Form.OnClose | 8 |  |  |  |  |  |  |
| Form_Error | Form.OnError | 4 |  |  |  |  |  |  |
| Form_Load | Form.OnLoad | 16 |  |  |  |  |  |  |
| DP_ZiuaClick | WithEvents DP (clsDatePicker).ZiuaClick | 23 |  | `tmpFacturi` |  |  |  |  |

## Opened from

Nothing found by static scan (probably opened by name from a ribbon, a menu string or a variable).

Raw source: `Surse/RawExport/Forms/EF_P.txt`