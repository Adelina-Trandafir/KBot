# Form `EFACTURA_UM`

Source: `Forms/EFACTURA_UM.txt` (29,005 chars). Units are twips (1 inch = 1440). `Parent` is the tab page or group a control sits in, or - for a label - the control it is attached to. Positions are relative to the section that holds the control; `LayoutCached*` values are used when present.

## Form properties

| Property | Value |
|---|---|
| Caption | FACTURI PRIMITE |
| RecordSource | `SELECT EF_UM.COD, EF_UM.Exp FROM EF_UM WHERE (((EF_UM.Exp) Like "*" & [forms]![EFACTURA_UM]![TC] & "*")) ORDER BY EF_UM.Exp; ` |
| AutoCenter | NotDefault |
| NavigationButtons | NotDefault |
| RecordSelectors | NotDefault |
| DividingLines | NotDefault |
| ScrollBars | 2 |
| AllowAdditions | NotDefault |
| AllowDeletions | NotDefault |
| Width | 6576 |
| Left | 11832 |
| Top | 2784 |
| Right | 18408 |
| Bottom | 10308 |
| FitToScreen | 1 |
| GridY | 10 |
| TabularCharSet | 238 |
| DatasheetFontName | Calibri |
| DatasheetFontHeight | 11 |

Form events wired: none

## Sections

| Section | Height | Visible | BackColor | Events |
|---|---|---|---|---|
| FormHeader | 396 |  |  |  |
| Detail | 285 |  | 1 |  |
| FormFooter | 228 |  | 1 |  |

## Data

- RecordSource: 
```sql
SELECT EF_UM.COD, EF_UM.Exp FROM EF_UM WHERE (((EF_UM.Exp) Like "*" & [forms]![EFACTURA_UM]![TC] & "*")) ORDER BY EF_UM.Exp;
```
- Tables/queries behind the RecordSource: `TC`, `EF_UM`

## Controls

| # | Type | Name | Parent | Section | Pos L,T WxH | Caption | ControlSource | RowSource | Tab | Font | Events | Other |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| 1 | TextBox | TC |  | FormHeader | 1545,60 4581x283 |  |  |  |  | Consolas 10 | AfterUpdate |  |
| 2 | Label | Label11 | TC | FormHeader | 0,60 1545x285 | CĂUTARE |  |  |  | Consolas 11 |  |  |
| 3 | Button | C |  | FormHeader | 6126,56 336x283 | Command12 |  |  | 1 | Calibri 11 |  |  |
| 4 | TextBox | Bck |  | Section | 0,0 6576x285 |  |  |  | 1 | Calibri 11 |  |  |
| 5 | TextBox | EXP |  | Section | 627,0 5946x285 |  | EXP |  |  | Consolas 9 bold |  |  |
| 6 | TextBox | COD |  | Section | 0,0 627x285 |  | COD |  | 2 | Consolas 9 bold |  |  |
| 7 | Button | B |  | Section | 0,0 6576x283 | Command7 |  |  | 3 | Calibri 11 | OnClick, OnDblClick |  |
| 8 | TextBox | CL |  | FormFooter | 0,0 627x228 |  |  |  |  | Consolas 9 bold |  |  |
| 9 | Label | Label13 |  | FormFooter | 0,0 6576x228 | Dublu-click pe rândul dorit pentru selecție! |  |  |  | Consolas 8 bold |  |  |

## Module-level declarations

Form state kept in variables, constants, types and WithEvents helpers (the Access form class body before the first procedure).

```vb
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = True
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
```

## Event handlers and what they touch

Reads/writes come from SQL text and recordset calls found in the procedure body; they are a lead for reading the code, not a proof (dynamic SQL built from variables is missed).

| Procedure | Bound to | Lines | Opens forms/reports | Reads | Writes | Calls (modules) | Classes | Local calls |
|---|---|---|---|---|---|---|---|---|
| B_Click | B.OnClick | 5 |  |  |  |  |  |  |
| B_DblClick | B.OnDblClick | 26 | `EFACTURA_ADD` |  |  | `mdl_Functii2023.isLoaded`, `mdl_MsBox.ptMS` |  |  |
| TC_AfterUpdate | TC.AfterUpdate | 5 |  |  |  |  |  |  |

## Opened from

`form:EFACTURA_ADD_SUB`

Raw source: `Surse/RawExport/Forms/EFACTURA_UM.txt`