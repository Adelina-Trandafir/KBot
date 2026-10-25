# PDF viewer forms: EFACTURA_PDF, MF_PDF_EF and Note_PDF_EF

Shared properties, sections, WB control, declarations and handler map below apply to all three form records. Their distinct SQL, coordinates, callers and source identities are in Data. [Reading conventions](../README.md#form-page-conventions). Consolidated in D001-07; no Access form or raw export is changed.

## Form properties

| Property | Value |
|---|---|
| RecordSource | [Per-form SQL and geometry](#data) |
| DefaultView | 0 |
| PopUp | NotDefault |
| Modal | NotDefault |
| AutoCenter | NotDefault |
| NavigationButtons | NotDefault |
| RecordSelectors | NotDefault |
| DividingLines | NotDefault |
| ScrollBars | 0 |
| Width | 6349 |
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

All three RecordSource queries read EF. Their paths, form references and export identities remain separate:

<a id="efactura-pdf"></a>

### Form `EFACTURA_PDF`

Source: `Forms/EFACTURA_PDF.txt` (14,813 chars). Raw source: `Surse/RawExport/Forms/EFACTURA_PDF.txt`

Window geometry (twips): Left=5616, Top=3696, Right=18900, Bottom=9108. Opened from: Nothing found by static scan (probably opened by name from a ribbon, a menu string or a variable).

```sql
SELECT [TempVars]![CaleAvacont] & "\PDF\" & [id_sol] & ".pdf" AS Cale FROM EF WHERE (((EF.id_sol)=[forms]![EFACTURA_2025]![id_sol]));
```

<a id="mf-pdf-ef"></a>

### Form `MF_PDF_EF`

Source: `Forms/MF_PDF_EF.txt` (14,831 chars). Raw source: `Surse/RawExport/Forms/MF_PDF_EF.txt`

Window geometry (twips): Left=-2640, Top=3700, Right=16190, Bottom=14640. Opened from: `form:MF2019_Asoc_EF`

```sql
SELECT [TempVars]![CaleAvacont] & "\EFACTURA\PDF\" & [id_sol] & ".pdf" AS Cale FROM EF WHERE (((EF.id_sol)=[forms]![MF2019_Asoc_EF]![id_sol]));
```

<a id="note-pdf-ef"></a>

### Form `Note_PDF_EF`

Source: `Forms/Note_PDF_EF.txt` (14,822 chars). Raw source: `Surse/RawExport/Forms/Note_PDF_EF.txt`

Window geometry (twips): Left=5610, Top=6610, Right=19870, Bottom=13890. Opened from: `form:Note_EFactura`

```sql
SELECT [TempVars]![CaleAvacont] & "\EFACTURA\PDF\" & [id_sol] & ".pdf" AS Cale FROM EF WHERE (((EF.id_sol)=[forms]![Note]![Note_EFactura]![id_sol]));
```

## Controls

| # | Type | Name | Parent | Section | Pos L,T WxH | Caption | ControlSource | RowSource | Tab | Font | Events | Other |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| 1 | WebBrowser | WB |  | Section | 113,113 6123x2664 |  | Cale |  |  |  |  |  |

## Module-level declarations

Only the [common attribute prefix](../README.md#module-declarations); no additional module declarations.

## Event handlers and what they touch

See [static-scan limits](../README.md#how-it-was-made-and-its-limits).

| Procedure | Bound to | Lines | Opens forms/reports | Reads | Writes | Calls (modules) | Classes | Local calls |
|---|---|---|---|---|---|---|---|---|
| Form_Close | Form.OnClose | 9 |  |  |  |  |  | `Form_Load` |
| Form_Load | Form.OnLoad | 3 |  |  |  |  |  |  |

