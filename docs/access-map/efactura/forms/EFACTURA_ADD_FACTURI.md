# Form `EFACTURA_ADD_FACTURI`

Source: `Forms/EFACTURA_ADD_FACTURI.txt` (19,638 chars). Units are twips (1 inch = 1440). `Parent` is the tab page or group a control sits in, or - for a label - the control it is attached to. Positions are relative to the section that holds the control; `LayoutCached*` values are used when present.

## Form properties

| Property | Value |
|---|---|
| RecordSource | `SELECT Factura.IdFactura, Factura.IdClient, Factura.IdOperatie, ClientiEF.DenumireClient, Factura.NumarFactura, Factura.DataFactura, Factura.Comentarii, Factura.Anulata, Factura.TRIMISA, Factura.id_incarcare, Factura.id_descarcare, Factura.ATT, Factura.ContPlata, Factura.DTQ, Factura.SerieFactura, Factura.BT_13, Factura.TipFactura, Factura.IdFacturaA, Factura.SerieFacturaA, Factura.NumarFacturaA, Factura.Corectata FROM Factura INNER JOIN ClientiEF ON Factura.IdClient = ClientiEF.IdClient ORDER BY Factura.IdFactura DESC; ` |
| BorderStyle | 3 |
| NavigationButtons | NotDefault |
| RecordSelectors | NotDefault |
| DividingLines | NotDefault |
| ScrollBars | 2 |
| AllowAdditions | NotDefault |
| AllowEdits | NotDefault |
| AllowDeletions | NotDefault |
| Width | 4219 |
| Left | 2800 |
| Top | 2830 |
| Right | 7770 |
| Bottom | 7220 |
| FitToScreen | 1 |
| GridY | 10 |
| TabularCharSet | 238 |
| DatasheetFontName | Calibri |
| DatasheetFontHeight | 11 |

Form events wired: none

## Sections

| Section | Height | Visible | BackColor | Events |
|---|---|---|---|---|
| PageHeaderSection | 285 |  | 1 |  |
| Detail | 285 |  | 1 |  |
| PageFooterSection | 0 |  | 1 |  |

## Data

- RecordSource: 
```sql
SELECT Factura.IdFactura, Factura.IdClient, Factura.IdOperatie, ClientiEF.DenumireClient, Factura.NumarFactura, Factura.DataFactura, Factura.Comentarii, Factura.Anulata, Factura.TRIMISA, Factura.id_incarcare, Factura.id_descarcare, Factura.ATT, Factura.ContPlata, Factura.DTQ, Factura.SerieFactura, Factura.BT_13, Factura.TipFactura, Factura.IdFacturaA, Factura.SerieFacturaA, Factura.NumarFacturaA, Factura.Corectata FROM Factura INNER JOIN ClientiEF ON Factura.IdClient = ClientiEF.IdClient ORDER BY Factura.IdFactura DESC;
```
- Tables/queries behind the RecordSource: `Factura`, `ClientiEF`

## Controls

| # | Type | Name | Parent | Section | Pos L,T WxH | Caption | ControlSource | RowSource | Tab | Font | Events | Other |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| 1 | TextBox | IDF |  | PageHeader | 0,0 741x285 |  |  |  |  | Consolas 11 |  |  |
| 2 | TextBox | Bck2 |  | Section | 283,0 3916x285 |  |  |  | 5 | Calibri 11 |  |  |
| 3 | TextBox | Bck |  | Section | 0,0 276x285 |  |  |  | 3 | Calibri 11 |  |  |
| 4 | TextBox | NumarFactura |  | Section | 276,0 796x285 |  | NumarFactura |  |  | Consolas 9 |  |  |
| 5 | TextBox | DataFactura |  | Section | 1072,0 1236x285 |  | DataFactura |  | 1 | Consolas 9 |  |  |
| 6 | TextBox | DenumireClient |  | Section | 2308,0 1911x285 |  | DenumireClient |  | 2 | Consolas 9 |  |  |
| 7 | Button | S |  | Section | 276,0 3936x285 | Command7 |  |  | 4 | Calibri 11 | OnClick |  |

## Module-level declarations

Form state kept in variables, constants, types and WithEvents helpers (the Access form class body before the first procedure).

```vb
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = True
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
'Private Sub Detail_Paint()
'Bck.BackColor = Nz(Me!BColor, 0)
'If Me!IdFactura = IDF Then
'    RS.ForeColor = vbBlack
'Else
'    RS.ForeColor = RS.BackColor
'End If
'End Sub
```

## Event handlers and what they touch

Reads/writes come from SQL text and recordset calls found in the procedure body; they are a lead for reading the code, not a proof (dynamic SQL built from variables is missed).

| Procedure | Bound to | Lines | Opens forms/reports | Reads | Writes | Calls (modules) | Classes | Local calls |
|---|---|---|---|---|---|---|---|---|
| S_Click | S.OnClick | 35 | `EFACTURA_ADD` | `Factura`, `ClientiEF`, `FacturaC` | `tmpFacturaC` | `mdl_Functii2023.Concat_WS` |  |  |

## Opened from

`form:EFACTURA_ADD`

Raw source: `Surse/RawExport/Forms/EFACTURA_ADD_FACTURI.txt`