# Form `EFACTURA_ADD_SUB`

Source: `Forms/EFACTURA_ADD_SUB.txt` (44,031 chars). Units are twips (1 inch = 1440). `Parent` is the tab page or group a control sits in, or - for a label - the control it is attached to. Positions are relative to the section that holds the control; `LayoutCached*` values are used when present.

## Form properties

| Property | Value |
|---|---|
| RecordSource | `SELECT tmpFacturaC.IdContinut, tmpFacturaC.IdFactura, tmpFacturaC.Continut, tmpFacturaC.Um, tmpFacturaC.Cant, tmpFacturaC.PU, tmpFacturaC.Valoare, tmpFacturaC.NrCrt, tmpFacturaC.Grup FROM tmpFacturaC; ` |
| NavigationButtons | NotDefault |
| RecordSelectors | NotDefault |
| DividingLines | NotDefault |
| ScrollBars | 2 |
| Width | 6971 |
| Left | 11880 |
| Top | 5210 |
| Right | 18860 |
| Bottom | 9120 |
| FitToScreen | 1 |
| GridY | 10 |
| TabularCharSet | 238 |
| DatasheetFontName | Calibri |
| DatasheetFontHeight | 11 |

Form events wired: `BeforeUpdate` = VBA, `OnLoad` = VBA

## Sections

| Section | Height | Visible | BackColor | Events |
|---|---|---|---|---|
| FormHeader | 285 |  | 15527148 |  |
| Detail | 505 |  | 1 |  |
| FormFooter | 285 |  | 15527148 |  |

## Data

- RecordSource: 
```sql
SELECT tmpFacturaC.IdContinut, tmpFacturaC.IdFactura, tmpFacturaC.Continut, tmpFacturaC.Um, tmpFacturaC.Cant, tmpFacturaC.PU, tmpFacturaC.Valoare, tmpFacturaC.NrCrt, tmpFacturaC.Grup FROM tmpFacturaC;
```
- Tables/queries behind the RecordSource: `tmpFacturaC`

## Controls

| # | Type | Name | Parent | Section | Pos L,T WxH | Caption | ControlSource | RowSource | Tab | Font | Events | Other |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| 1 | Label | Label2 |  | FormHeader | 276,0 3005x285 | Continut |  |  |  | Consolas 9 bold |  |  |
| 2 | Label | Label3 |  | FormHeader | 3281,0 915x285 | Um |  |  |  | Consolas 9 bold |  |  |
| 3 | Label | Label4 |  | FormHeader | 4203,0 900x285 | Cant |  |  |  | Consolas 9 bold |  |  |
| 4 | Label | Label5 |  | FormHeader | 5102,0 915x285 | PU |  |  |  | Consolas 9 bold |  |  |
| 5 | Label | Label6 |  | FormHeader | 6009,0 960x285 | Valoare |  |  |  | Consolas 9 bold |  |  |
| 6 | Button | Command13 |  | FormHeader | 0,0 114x113 | Command26 |  |  |  | Calibri 11 |  |  |
| 7 | TextBox | IdContinut |  | Section | 566,0 741x285 |  | IdContinut |  | 1 | Consolas 9 |  |  |
| 8 | TextBox | Continut |  | Section | 276,0 3005x505 |  | Continut |  |  | Arial Narrow 10 | BeforeUpdate, AfterUpdate |  |
| 9 | TextBox | IdFactura |  | Section | 2121,0 290x285 |  | IdFactura |  | 6 | Consolas 9 |  |  |
| 10 | TextBox | Cant |  | Section | 4203,0 900x505 |  | Cant |  | 3 | Consolas 9 |  | fmt=Standard |
| 11 | TextBox | PU |  | Section | 5102,0 915x505 |  | PU |  | 4 | Consolas 9 | OnLostFocus | fmt=Standard |
| 12 | TextBox | Valoare |  | Section | 6009,0 960x505 |  | Valoare |  | 5 | Consolas 9 |  | fmt=Standard |
| 13 | TextBox | NrCrt |  | Section | 2701,0 290x285 |  | NrCrt |  | 7 | Consolas 9 bold |  |  |
| 14 | TextBox | Grup |  | Section | 2411,0 290x285 |  | Grup |  | 8 | Consolas 9 bold |  |  |
| 15 | TextBox | Um |  | Section | 3281,0 915x505 |  | Um |  | 2 | Consolas 9 | BeforeUpdate | def="XPP" |
| 16 | Button | bCaut |  | Section | 3972,0 224x505 |  |  |  | 9 | Consolas 9 | OnClick |  |
| 17 | Button | FOC |  | Section | 0,0 224x285 |  |  |  | 10 | Consolas 9 |  |  |
| 18 | Button | bDel |  | Section | 0,0 276x505 | Command42 |  |  | 11 | Calibri 11 | OnClick | tip=Șterge înregistrare curentă |
| 19 | Label | Label7 |  | Section | 2991,0 290x285 | Nr |  |  |  | Consolas 9 bold |  |  |
| 20 | Label | Label8 |  | FormFooter | 0,0 5100x285 | TOTAL FACTURĂ |  |  |  | Consolas 9 bold |  |  |
| 21 | TextBox | TotalFactura |  | FormFooter | 5094,0 1875x285 |  | =Sum([Valoare]) |  |  | Consolas 9 bold |  | fmt=Standard |

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
| bCaut_Click | bCaut.OnClick | 10 | `EFACTURA_UM` |  |  |  |  |  |
| bDEL_Click | bDel.OnClick | 6 |  |  | `tmpFacturaC` |  |  |  |
| Continut_AfterUpdate | Continut.AfterUpdate | 7 |  |  |  |  |  |  |
| Continut_BeforeUpdate | Continut.BeforeUpdate | 9 |  |  |  | `mdl_MsBox.MsBox` |  |  |
| Form_BeforeInsert |  | 6 |  | `tmpFacturaC` |  |  |  |  |
| Form_BeforeUpdate | Form.BeforeUpdate | 6 |  | `tmpFacturaC` |  |  |  |  |
| Form_Load | Form.OnLoad | 4 |  |  |  |  |  |  |
| PU_LostFocus | PU.OnLostFocus | 6 |  |  |  |  |  |  |
| Um_BeforeUpdate | Um.BeforeUpdate | 10 |  | `EF_UM` |  |  |  |  |

## Opened from

`form:EFACTURA_ADD`

Raw source: `Surse/RawExport/Forms/EFACTURA_ADD_SUB.txt`