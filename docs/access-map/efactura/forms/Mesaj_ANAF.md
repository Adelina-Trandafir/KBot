# Form `Mesaj_ANAF`

Source: `Forms/Mesaj_ANAF.txt` (13,532 chars). Units are twips (1 inch = 1440). `Parent` is the tab page or group a control sits in, or - for a label - the control it is attached to. Positions are relative to the section that holds the control; `LayoutCached*` values are used when present.

## Form properties

| Property | Value |
|---|---|
| Caption | Mesaj ANAF |
| DefaultView | 0 |
| PopUp | NotDefault |
| Modal | NotDefault |
| AutoCenter | NotDefault |
| NavigationButtons | NotDefault |
| RecordSelectors | NotDefault |
| DividingLines | NotDefault |
| ScrollBars | 0 |
| Width | 8820 |
| Left | 6168 |
| Top | 3780 |
| Right | 14988 |
| Bottom | 8508 |
| GridX | 24 |
| GridY | 24 |
| TabularCharSet | 238 |
| DatasheetFontName | Calibri |
| DatasheetFontHeight | 11 |

Form events wired: none

## Sections

| Section | Height | Visible | BackColor | Events |
|---|---|---|---|---|
| Detail | 4740 |  | 1 |  |

## Data

- Unbound form (data loaded/saved by code).

## Controls

| # | Type | Name | Parent | Section | Pos L,T WxH | Caption | ControlSource | RowSource | Tab | Font | Events | Other |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| 1 | WebBrowser | wb1 |  | Section | 60,60 8700x4620 |  | ="C:\Avacont\efactura\mesaj.html" |  |  |  |  |  |

## Event handlers and what they touch

Reads/writes come from SQL text and recordset calls found in the procedure body; they are a lead for reading the code, not a proof (dynamic SQL built from variables is missed).

| Procedure | Bound to | Lines | Opens forms/reports | Reads | Writes | Calls (modules) | Classes | Local calls |
|---|---|---|---|---|---|---|---|---|

## Opened from

`module:mdl_EFactura`

Raw source: `Surse/RawExport/Forms/Mesaj_ANAF.txt`