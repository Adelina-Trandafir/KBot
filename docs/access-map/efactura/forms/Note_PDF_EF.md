# Form `Note_PDF_EF`

Source: `Forms/Note_PDF_EF.txt` (14,822 chars). Units are twips (1 inch = 1440). `Parent` is the tab page or group a control sits in, or - for a label - the control it is attached to. Positions are relative to the section that holds the control; `LayoutCached*` values are used when present.

## Form properties

| Property | Value |
|---|---|
| RecordSource | `SELECT [TempVars]![CaleAvacont] & "\EFACTURA\PDF\" & [id_sol] & ".pdf" AS Cale FROM EF WHERE (((EF.id_sol)=[forms]![Note]![Note_EFactura]![id_sol])); ` |
| DefaultView | 0 |
| PopUp | NotDefault |
| Modal | NotDefault |
| AutoCenter | NotDefault |
| NavigationButtons | NotDefault |
| RecordSelectors | NotDefault |
| DividingLines | NotDefault |
| ScrollBars | 0 |
| Width | 6349 |
| Left | 5610 |
| Top | 6610 |
| Right | 19870 |
| Bottom | 13890 |
| FitToScreen | 1 |
| GridY | 10 |
| TabularCharSet | 238 |
| DatasheetFontName | Calibri |
| DatasheetFontHeight | 11 |

Form events wired: `OnClose` = VBA, `OnLoad` = VBA

## Sections

| Section | Height | Visible | BackColor | Events |
|---|---|---|---|---|
| Detail | 2891 |  | 1 |  |

## Data

- RecordSource: 
```sql
SELECT [TempVars]![CaleAvacont] & "\EFACTURA\PDF\" & [id_sol] & ".pdf" AS Cale FROM EF WHERE (((EF.id_sol)=[forms]![Note]![Note_EFactura]![id_sol]));
```
- Tables/queries behind the RecordSource: `EF`

## Controls

| # | Type | Name | Parent | Section | Pos L,T WxH | Caption | ControlSource | RowSource | Tab | Font | Events | Other |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| 1 | WebBrowser | WB |  | Section | 113,113 6123x2664 |  | Cale |  |  |  |  |  |

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
| Form_Close | Form.OnClose | 9 |  |  |  |  |  | `Form_Load` |
| Form_Load | Form.OnLoad | 3 |  |  |  |  |  |  |

## Opened from

`form:Note_EFactura`

Raw source: `Surse/RawExport/Forms/Note_PDF_EF.txt`