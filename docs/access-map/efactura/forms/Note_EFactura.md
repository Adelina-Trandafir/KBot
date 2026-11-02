# Form `Note_EFactura`

Source: `Surse/RawExport/Forms/Note_EFactura.txt` (42,108 chars). [Reading conventions](../README.md#form-page-conventions).

## Form properties

Inherit the [MF2019_Asoc_EF properties](MF2019_Asoc_EF.md#form-properties), except `PopUp` is absent from this export (no Boolean value inferred) and the following geometry replaces its rows:

| Property | Value |
|---|---|
| Left | 3470 |
| Top | 7520 |
| Right | 16500 |
| Bottom | 9300 |

Form events wired: `OnLoad` = VBA

## Sections

Inherit [MF2019_Asoc_EF sections](MF2019_Asoc_EF.md#sections), replacing only FormFooter:

| Section | Height | Visible | BackColor | Events |
|---|---|---|---|---|
| FormFooter | 0 | NotDefault | 15527148 |  |

## Data

- RecordSource: `tmpNote_EFactura`
- Tables/queries behind the RecordSource: `tmpNote_EFactura`

## Controls

Inherit controls 1…23 from [MF2019_Asoc_EF](MF2019_Asoc_EF.md#controls), replacing rows 6 and 23 below. Controls 24 (`SavFact`) and 25 (`Iesire`) are absent here; handler dependencies remain separate below.

| # | Type | Name | Parent | Section | Pos L,T WxH | Caption | ControlSource | RowSource | Tab | Font | Events | Other |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| 6 | Label | Label66 |  | FormHeader | 9828,0 1263x300 | Rămas |  |  |  | Arial Narrow 9 bold |  |  |
| 23 | Button | Btn |  | Section | 0,0 11103x300 | Command32 |  |  | 12 | Calibri 9 | OnClick |  |

## Module-level declarations

The [common attribute prefix](../README.md#module-declarations) precedes the declarations below.

```vb
Public initIDEFT As Variant
```

## Event handlers and what they touch

See [static-scan limits](../README.md#how-it-was-made-and-its-limits).

| Procedure | Bound to | Lines | Opens forms/reports | Reads | Writes | Calls (modules) | Classes | Local calls |
|---|---|---|---|---|---|---|---|---|
| Bck_Enter | Bck.OnEnter | 4 |  |  |  |  |  |  |
| bFact_Click | bFact.OnClick | 9 | `Note_PDF_EF` |  |  | `mdl_CheckTVW.FileExists`, `mdl_EFactura.DescarcaFacturaPDF` |  |  |
| Btn_Click | Btn.OnClick | 86 | `Note` |  | `tmpNote_EFactura` | `mdl_Functii2023.mMin`, `mdl_MsBox.MsBox` |  |  |
| Form_Load | Form.OnLoad | 88 |  | `qNote_EFactura_Toate`, `_TempCredit`, `IMG` | `tmpNote_EFactura`, `tmpMF_EFactura` | `mdl_MsBox.MsBox`, `mdl_Functii2023.mmin` |  |  |

## Opened from

Nothing found by static scan (probably opened by name from a ribbon, a menu string or a variable).
