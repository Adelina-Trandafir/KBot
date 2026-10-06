# Form `EFACTURA_ADD`

Source: `Forms/EFACTURA_ADD.txt` (133,048 chars). Units are twips (1 inch = 1440). `Parent` is the tab page or group a control sits in, or - for a label - the control it is attached to. Positions are relative to the section that holds the control; `LayoutCached*` values are used when present.

## Form properties

| Property | Value |
|---|---|
| Caption | Facturi vânzare prin sistemul E-Factură |
| DefaultView | 0 |
| AutoCenter | NotDefault |
| BorderStyle | 1 |
| NavigationButtons | NotDefault |
| RecordSelectors | NotDefault |
| DividingLines | NotDefault |
| ScrollBars | 0 |
| Width | 12869 |
| Left | 2450 |
| Top | 1170 |
| Right | 15320 |
| Bottom | 6760 |
| FitToScreen | 255 |
| GridY | 10 |
| TabularCharSet | 238 |
| DatasheetFontName | Calibri |
| DatasheetFontHeight | 11 |

Form events wired: `OnClose` = VBA, `OnLoad` = VBA

## Sections

| Section | Height | Visible | BackColor | Events |
|---|---|---|---|---|
| FormHeader | 330 |  | 15527148 |  |
| Detail | 4762 |  | 1 |  |
| FormFooter | 510 |  | 1 |  |

## Data

- Combo/list sources: `Factura`

## Subforms

| Control | Subform | Link child | Link master |
|---|---|---|---|
| FACTURI | `EFACTURA_ADD_FACTURI` |  |  |
| subClienti | `EFACTURA_CLIENTI` |  |  |
| subVanzator | `EFACTURA_VANZATOR` |  |  |
| EFACTURA_ADD_SUB | `EFACTURA_ADD_SUB` |  |  |

## Controls

| # | Type | Name | Parent | Section | Pos L,T WxH | Caption | ControlSource | RowSource | Tab | Font | Events | Other |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| 1 | Label | Label0 |  | FormHeader | 5392,0 7335x330 | Detalii factură |  |  |  | Consolas 12 bold |  |  |
| 2 | Label | Label1 |  | FormHeader | 56,0 5229x330 | Facturi salvate |  |  |  | Consolas 12 bold |  |  |
| 3 | TextBox | IdFactura |  | FormHeader | 0,0 741x285 |  |  |  |  | Consolas 11 |  |  |
| 4 | TextBox | id_incarcare |  | FormHeader | 737,0 741x285 |  |  |  | 1 | Consolas 11 |  |  |
| 5 | TextBox | IdFacturaA |  | FormHeader | 1474,0 741x285 |  |  |  | 2 | Consolas 11 |  |  |
| 6 | Subform | FACTURI |  | Section | 0,0 5229x4651 |  |  |  |  |  |  |  |
| 7 | Tab | Tb |  | Section | 5328,60 7492x4660 |  |  |  | 1 | Consolas 9 bold |  |  |
| 8 | TabPage | pgGenerale | Tb | Section | 5380,420 7390x4250 | [Generale] |  |  |  |  |  |  |
| 9 | TextBox | NumarFactura | pgGenerale | Section | 7091,536 1701x275 |  |  |  |  | Consolas 10 bold |  |  |
| 10 | Label | Label5 | NumarFactura | Section | 5441,536 1650x275 | Număr factură |  |  |  | Consolas 10 |  |  |
| 11 | TextBox | DataFactura | pgGenerale | Section | 11001,536 1701x275 |  |  |  | 1 | Consolas 10 bold |  | fmt=Short Date; tag=1986 |
| 12 | Label | Label7 | DataFactura | Section | 9351,536 1650x275 | Dată factură |  |  |  | Consolas 10 |  |  |
| 13 | ComboBox | TipFactura | pgGenerale | Section | 7311,1036 5391x275 |  |  | `380;Factură;384;Factură corectată` | 2 | Consolas 10 bold |  | fmt=Short Date; rowType=Value List; cols=2; colW=0;1134; limit; tag=1986 |
| 14 | Label | Label62 | TipFactura | Section | 5441,1036 1870x275 | Tip factură |  |  |  | Consolas 10 |  |  |
| 15 | TextBox | Comentarii | pgGenerale | Section | 7306,1381 5391x1339 |  |  |  | 3 | Consolas 10 bold |  |  |
| 16 | Label | Label11 | Comentarii | Section | 5439,1381 1870x270 | Comentarii factură |  |  |  | Arial Narrow 10 bold |  |  |
| 17 | TextBox | BT_13 | pgGenerale | Section | 7306,2800 5391x315 |  |  |  | 4 | Consolas 10 bold |  |  |
| 18 | Label | Label53 | BT_13 | Section | 5439,2800 1870x315 | Ref. comandă (BT-13) |  |  |  | Arial Narrow 10 bold |  |  |
| 19 | Label | Label8 | pgGenerale | Section | 5440,4370 5390x285 | TOTAL FACTURĂ |  |  |  | Consolas 9 bold |  |  |
| 20 | TextBox | TotalFactura | pgGenerale | Section | 10820,4370 1875x285 |  | =DSum("Valoare","tmpFacturaC") |  | 5 | Consolas 9 bold |  | fmt=Standard |
| 21 | TextBox | txtStorno | pgGenerale | Section | 5442,4025 7260x285 |  |  |  | 6 | Consolas 9 bold |  |  |
| 22 | TabPage | pgCumparator | Tb | Section | 5380,420 7390x4250 | [Cumpărător] |  |  |  |  |  |  |
| 23 | Subform | subClienti | pgCumparator | Section | 5442,456 7310x4180 |  |  |  |  |  |  |  |
| 24 | TabPage | pgVanzator | Tb | Section | 5380,420 7390x4250 | [Vânzător] |  |  |  |  |  |  |
| 25 | Subform | subVanzator | pgVanzator | Section | 5442,903 7200x3721 |  |  |  |  |  |  |  |
| 26 | ComboBox | IBANPlat | pgVanzator | Section | 7312,536 5331x285 |  |  | `SELECT Factura.ContPlata FROM Factura GROUP BY Factura.ContPlata; ` |  | Consolas 10 bold |  | rowType=Table/Query |
| 27 | Label | Label22 | IBANPlat | Section | 5442,536 1870x285 | Cont emitent |  |  |  | Consolas 10 |  |  |
| 28 | TabPage | pgAtt | Tb | Section | 5380,420 7390x4250 | [Atașamente] |  |  |  |  |  |  |
| 29 | CheckBox | XML | pgAtt | Section | 5459,556 260x240 |  |  |  |  |  | OnClick |  |
| 30 | Label | Label41 | XML | Section | 5719,496 2729x315 | Atașează factura originală |  |  |  | Consolas 9 bold |  |  |
| 31 | TabPage | pgContinut | Tb | Section | 5380,420 7390x4250 | [Conținut] |  |  |  |  |  |  |
| 32 | Subform | EFACTURA_ADD_SUB | pgContinut | Section | 5442,456 7245x4180 |  |  |  |  |  |  |  |
| 33 | Button | FOC |  | Section | 0,0 114x113 | Command26 |  |  | 2 | Calibri 11 |  |  |
| 34 | Button | IESIRE |  | FormFooter | 56,56 1701x436 |  Ieșire |  |  |  | Consolas 9 bold | OnClick | tip=Close Form |
| 35 | Button | bAdd |  | FormFooter | 10624,56 2016x436 |  Adaugă factură |  |  | 1 | Consolas 9 bold | OnClick | tip=Close Form |
| 36 | Button | bSav |  | FormFooter | 10624,56 2016x436 |  Salvează |  |  | 2 | Consolas 9 bold | OnClick | tip=Close Form |
| 37 | Button | bMod |  | FormFooter | 5392,56 2016x436 |  Modifică |  |  | 3 | Consolas 9 bold | OnClick | tip=Close Form |
| 38 | Button | bUnd |  | FormFooter | 5392,56 2016x436 |  Renunță |  |  | 4 | Consolas 9 bold | OnClick | tip=Close Form |
| 39 | CheckBox | NOU |  | FormFooter | 8680,113 260x240 |  |  |  | 5 |  |  |  |
| 40 | Button | bOpt |  | FormFooter | 3213,56 2016x436 |   Opțiuni |  |  | 6 | Consolas 9 bold | OnClick==Popup_EFactura_Optiuni() | tip=Close Form |

## Module-level declarations

Form state kept in variables, constants, types and WithEvents helpers (the Access form class body before the first procedure).

```vb
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = True
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
Public IdFacturaStorno As Long
Public NrFacturaStorno As Long
Public IdFacturaDeStornat As Long
```

## Event handlers and what they touch

Reads/writes come from SQL text and recordset calls found in the procedure body; they are a lead for reading the code, not a proof (dynamic SQL built from variables is missed).

| Procedure | Bound to | Lines | Opens forms/reports | Reads | Writes | Calls (modules) | Classes | Local calls |
|---|---|---|---|---|---|---|---|---|
| bADD_Click | bAdd.OnClick | 70 |  | `Factura` | `tmpFacturaC` | `mdl_Functii_Generale.DMAX_EFACTURA` |  |  |
| bAddC_Click |  | 4 | `EFACTURA_CLIENTI` |  |  |  |  |  |
| bMod_Click (Public) | bMod.OnClick | 56 |  |  |  | `mdl_MsBox.MsBox`, `mdl_Popup2022.Make_Popup_EFV` |  |  |
| bSAV_Click | bSav.OnClick | 185 |  | `tblMSG`, `tmpFacturaC`, `Scheme` | `FacturaC`, `Factura` | `mdl_MsBox.MsBox`, `mdl_Messagebox_hook.MsgBoxCB` |  | `TrimiteFactura`, `bUND_Click` |
| TrimiteFactura |  | 27 |  |  | `Factura` | `mdl_EFactura_Add.GENEREAZA_XML_EFACTURA`, `mdl_EFactura.ValideazaXML_Local`, `mdl_EFactura.IncarcaFacturaXML`, `mdl_EFactura.StatusFactura` |  |  |
| BT_13_Change |  | 11 |  |  |  |  |  |  |
| bUND_Click | bUnd.OnClick | 33 |  |  |  |  |  |  |
| DataFactura_Enter |  | 5 |  |  |  |  |  |  |
| DataFactura_Exit |  | 4 |  |  |  |  |  |  |
| EFACTURA_ADD_SUB_Exit |  | 5 |  |  |  |  |  |  |
| Form_Load | Form.OnLoad | 73 | `Setari implicite` | `Scheme` | `tmpFacturaC` | `mdl_MsBox.MsBox`, `mdl_EFactura.VerificaAnaf` |  |  |
| IBANPlat_LostFocus |  | 12 |  |  |  | `mdl_Functii_Generale.CreeazaIBAN`, `mdl_MsBox.MsBox` |  |  |
| Iesire_Click | IESIRE.OnClick | 4 |  |  |  |  |  |  |
| PickIBAN_Click |  | 9 |  |  |  | `mdl_PositionRelativeToControl.PositionFormRelativeToControl` |  |  |
| XML_Click | XML.OnClick | 5 |  |  |  |  |  |  |
| CreeazaConturiIBAN (Public) |  | 48 |  |  | `tmpConturiIBAN`, `Clasificatii` | `mdl_Functii_Generale.CreeazaIBAN` |  | `fSursaCont` |
| fSursaCont |  | 14 |  |  |  |  |  |  |

## Opened from

`module:basRibbonCallbacks`, `module:mdl_Popup2022`, `form:EFACTURA_ADD_FACTURI`, `form:EFACTURA_PickIBAN`, `form:EFACTURA_UM`

Raw source: `Surse/RawExport/Forms/EFACTURA_ADD.txt`