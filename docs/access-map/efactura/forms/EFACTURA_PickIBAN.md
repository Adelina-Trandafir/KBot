# Form `EFACTURA_PickIBAN`

Source: `Forms/EFACTURA_PickIBAN.txt` (10,146 chars). [Reading conventions](../README.md#form-page-conventions).

## Form properties

| Property | Value |
|---|---|
| Caption | PickIBAN |
| DefaultView | 0 |
| PopUp | NotDefault |
| Modal | NotDefault |
| BorderStyle | 0 |
| ControlBox | NotDefault |
| NavigationButtons | NotDefault |
| RecordSelectors | NotDefault |
| DividingLines | NotDefault |
| ScrollBars | 0 |
| Width | 6723 |
| Left | 4610 |
| Top | 4330 |
| Right | 11330 |
| Bottom | 8000 |
| FitToScreen | 1 |
| GridX | 24 |
| GridY | 24 |
| TabularCharSet | 238 |
| DatasheetFontName | Calibri |
| DatasheetFontHeight | 11 |

Form events wired: `OnUnload` = VBA, `OnLoad` = VBA

## Sections

| Section | Height | Visible | BackColor | Events |
|---|---|---|---|---|
| Detail | 3675 |  | 1 |  |

## Data

- Unbound form (data loaded/saved by code).

## Controls

| # | Type | Name | Parent | Section | Pos L,T WxH | Caption | ControlSource | RowSource | Tab | Font | Events | Other |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| 1 | Subform | subTreeView |  | Section | 15,15 6663x3615 |  |  |  |  |  |  |  |

## Module-level declarations

The [common attribute prefix](../README.md#module-declarations) precedes the declarations below.

```vb
Public OA As String
Public CloseOk As Boolean
Public WithEvents mcTree As clsTreeView
Attribute mcTree.VB_VarHelpID = -1
Public Event IBANClick(IdClsf As Long, IBAN As String, Denumire As String, ClsfBug As String)
Public Event isClosing(Cancel As Integer)
```

## Event handlers and what they touch

See [static-scan limits](../README.md#how-it-was-made-and-its-limits).

| Procedure | Bound to | Lines | Opens forms/reports | Reads | Writes | Calls (modules) | Classes | Local calls |
|---|---|---|---|---|---|---|---|---|
| Form_Load | Form.OnLoad | 8 |  |  |  |  |  | `Init_Pop_EFACTURA_IBAN` |
| Init_Pop_EFACTURA_IBAN (Public) |  | 32 |  |  |  | `mdl_2026.GenerateUniqueSequence` |  | `PopTree_EFACTURA_IBAN` |
| PopTree_EFACTURA_IBAN |  | 88 |  | `tmpConturiIBAN`, `DefaTitlu2`, `DefaSub` |  |  |  | `PosClsf` |
| mcTree_Click | WithEvents mcTree (clsTreeView).Click | 16 | `EFACTURA_ADD` |  |  | `mdl_Functii2023.isLoaded` |  |  |
| PosClsf |  | 13 |  |  |  |  |  |  |
| Form_Unload | Form.OnUnload | 17 |  |  |  |  |  |  |

## Opened from

Nothing found by static scan (probably opened by name from a ribbon, a menu string or a variable).

Raw source: `Surse/RawExport/Forms/EFACTURA_PickIBAN.txt`