# Form `EF_F`

Source: `Forms/EF_F.txt` (62,535 chars). Units are twips (1 inch = 1440). `Parent` is the tab page or group a control sits in, or - for a label - the control it is attached to. Positions are relative to the section that holds the control; `LayoutCached*` values are used when present.

## Form properties

| Property | Value |
|---|---|
| Caption | tmpDoc2017 |
| RecordSource | `tmpFacturi` |
| OrderBy | DataDocument, NumarDocument |
| AutoCenter | NotDefault |
| NavigationButtons | NotDefault |
| RecordSelectors | NotDefault |
| DividingLines | NotDefault |
| ScrollBars | 2 |
| AllowAdditions | NotDefault |
| AllowDeletions | NotDefault |
| Width | 9081 |
| Left | 9580 |
| Top | 2440 |
| Right | 19260 |
| Bottom | 5220 |
| FitToScreen | 1 |
| Cycle | 1 |
| GridX | 60 |
| GridY | 60 |
| TabularCharSet | 161 |
| DatasheetFontName | Consolas |

Form events wired: `OnCurrent` = VBA, `AfterUpdate` = VBA, `OnClose` = VBA, `OnLoad` = VBA, `OnError` = VBA

## Sections

| Section | Height | Visible | BackColor | Events |
|---|---|---|---|---|
| FormHeader | 464 |  | 15527148 |  |
| Detail | 290 |  |  |  |
| FormFooter | 290 |  | 15527148 |  |

## Data

- RecordSource: `tmpFacturi`
- Tables/queries behind the RecordSource: `tmpFacturi`
- Combo/list sources: `Documente`

## Controls

| # | Type | Name | Parent | Section | Pos L,T WxH | Caption | ControlSource | RowSource | Tab | Font | Events | Other |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| 1 | Label | lDocument |  | FormHeader | 1351,0 1994x464 | Document |  |  |  | Consolas 9 bold |  |  |
| 2 | Label | lData |  | FormHeader | 4548,0 1413x464 | Data |  |  |  | Consolas 9 bold |  |  |
| 3 | Label | lValoare |  | FormHeader | 5961,0 1416x464 | Valoare rămasă de înregistrat |  |  |  | Consolas 8 bold |  |  |
| 4 | Label | lNC |  | FormHeader | 0,0 1348x464 | NC |  |  |  | Consolas 9 bold |  |  |
| 5 | Label | lFEL |  | FormHeader | 3345,0 1203x464 | Fel |  |  |  | Consolas 9 bold |  |  |
| 6 | Label | lPlatita |  | FormHeader | 7377,0 1416x464 | Valoare care se plătește |  |  |  | Consolas 8 bold |  |  |
| 7 | Label | lALG |  | FormHeader | 8793,0 288x464 | FX |  |  |  | Consolas 9 bold |  |  |
| 8 | TextBox | CL |  | FormHeader | 0,0 429x0 |  |  |  |  | Calibri 11 |  |  |
| 9 | TextBox | Bck |  | Section | 0,0 9081x290 |  |  |  | 7 | Consolas 9 | OnEnter |  |
| 10 | TextBox | NumarDocument |  | Section | 1351,0 1994x290 |  | NumarDocument |  | 1 | Consolas 9 | AfterUpdate |  |
| 11 | TextBox | DataDocument |  | Section | 4548,0 1413x290 |  | DataDocument |  | 3 | Consolas 9 | BeforeUpdate |  |
| 12 | TextBox | Suma |  | Section | 5961,0 1416x290 |  | Suma |  | 4 | Consolas 9 | AfterUpdate | fmt=Standard |
| 13 | TextBox | FelDocument |  | Section | 3345,0 1203x290 |  | FelDocument |  | 2 | Consolas 9 |  |  |
| 14 | TextBox | Platit |  | Section | 7377,0 1416x290 |  | Platit |  | 5 | Consolas 9 | BeforeUpdate, AfterUpdate | fmt=Standard; tip=Dacă nu se introduce și pltă, se lasă valoare totală a facturi! |
| 15 | CheckBox | ALG |  | Section | 8850,45 216x204 |  | ALG |  | 6 |  |  | tag=○\|●; tip=Numar / Data Ang. Leg.: 0/ |
| 16 | ComboBox | NC |  | Section | 0,0 1348x290 |  | NC | `SELECT Documente.NumarNC, First(Documente.[datadocument]) AS DI, Last(Documente.[datadocument]) AS DSF FROM Documente GROUP BY Documente.NumarNC HAVING (((Docum...` |  | Consolas 9 | AfterUpdate | rowType=Table/Query; cols=3; colW=567;1419;1419 |
| 17 | Button | BFX |  | Section | 8793,0 288x283 | Command45 |  |  | 8 | Calibri 11 | OnClick | tip=Suma poate fi adăugată în contul 8067 DOAR în cazul în care se plătește integral, sau deloc! |
| 18 | Button | FOC |  | Section | 8551,75 143x136 | Command46 |  |  | 9 | Calibri 11 |  |  |
| 19 | TextBox | Text47 |  | FormFooter | 0,0 9081x290 |  |  |  | 2 | Consolas 9 bold |  | fmt=Standard |
| 20 | TextBox | Text43 |  | FormFooter | 5961,0 1416x290 |  | =Sum([Suma]) |  |  | Consolas 9 bold |  | fmt=Standard |
| 21 | TextBox | TotalSumaPlata |  | FormFooter | 7377,0 1416x290 |  | =Sum([Platit]) |  | 1 | Consolas 9 bold |  | fmt=Standard; tip=Dacă nu se introduce și pltă, se lasă valoare totală a facturi! |

## Module-level declarations

Form state kept in variables, constants, types and WithEvents helpers (the Access form class body before the first procedure).

```vb
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = True
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
Public WithEvents DP As clsDatePicker
Attribute DP.VB_VarHelpID = -1
Private Declare PtrSafe Function SetParent Lib "user32" ( _
                                                ByVal hWndChild As LongPtr, _
                                                ByVal hWndNewParent As LongPtr) As LongPtr
```

## Event handlers and what they touch

Reads/writes come from SQL text and recordset calls found in the procedure body; they are a lead for reading the code, not a proof (dynamic SQL built from variables is missed).

| Procedure | Bound to | Lines | Opens forms/reports | Reads | Writes | Calls (modules) | Classes | Local calls |
|---|---|---|---|---|---|---|---|---|
| Bck_Enter | Bck.OnEnter | 4 |  |  |  |  |  |  |
| BFX_Click | BFX.OnClick | 54 |  |  | `Cont8067`, `Oper` |  |  |  |
| DataDocument_BeforeUpdate | DataDocument.BeforeUpdate | 23 |  |  |  | `mdl_MsBox.ptMS`, `mdl_GLOBALS.globANL`, `mdl_Sendkeys.SendKeysAPI` |  | `BFX_Click` |
| DP_ZiuaClick | WithEvents DP (clsDatePicker).ZiuaClick | 53 |  |  | `Cont8067` |  |  |  |
| Form_AfterUpdate | Form.AfterUpdate | 32 | `EFACTURA_2025` | `tmpFacturi` |  |  |  |  |
| Form_Close | Form.OnClose | 8 |  |  |  |  |  |  |
| Form_Current | Form.OnCurrent | 18 |  |  |  |  |  |  |
| Form_Error | Form.OnError | 5 |  |  |  |  |  |  |
| Form_Load | Form.OnLoad | 16 |  |  |  |  |  |  |
| NC_AfterUpdate | NC.AfterUpdate | 30 |  |  |  |  |  |  |
| NumarDocument_AfterUpdate | NumarDocument.AfterUpdate | 26 |  | `Documente` |  | `mdl_MsBox.MsBox` |  |  |
| Platit_AfterUpdate | Platit.AfterUpdate | 11 |  |  |  |  |  |  |
| Platit_BeforeUpdate | Platit.BeforeUpdate | 30 |  |  |  | `mdl_MsBox.ptMS`, `mdl_Sendkeys.SendKeysAPI` |  |  |
| Suma_AfterUpdate | Suma.AfterUpdate | 4 |  |  |  |  |  |  |

## Opened from

Nothing found by static scan (probably opened by name from a ribbon, a menu string or a variable).

Raw source: `Surse/RawExport/Forms/EF_F.txt`