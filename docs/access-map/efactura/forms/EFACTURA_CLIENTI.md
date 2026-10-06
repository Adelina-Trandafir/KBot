# Form `EFACTURA_CLIENTI`

Source: `Forms/EFACTURA_CLIENTI.txt` (68,955 chars). Units are twips (1 inch = 1440). `Parent` is the tab page or group a control sits in, or - for a label - the control it is attached to. Positions are relative to the section that holds the control; `LayoutCached*` values are used when present.

## Form properties

| Property | Value |
|---|---|
| Caption | Modifică / Adaugă Clienți |
| DefaultView | 0 |
| AutoCenter | NotDefault |
| BorderStyle | 3 |
| NavigationButtons | NotDefault |
| RecordSelectors | NotDefault |
| DividingLines | NotDefault |
| ScrollBars | 0 |
| Width | 5385 |
| Left | 8280 |
| Top | 3350 |
| Right | 13670 |
| Bottom | 6130 |
| FitToScreen | 1 |
| GridY | 10 |
| TabularCharSet | 238 |
| DatasheetFontName | Calibri |
| DatasheetFontHeight | 11 |

Form events wired: `OnLoad` = VBA

## Sections

| Section | Height | Visible | BackColor | Events |
|---|---|---|---|---|
| FormHeader | 0 | NotDefault | 15849926 |  |
| Detail | 2381 |  | 1 |  |
| FormFooter | 405 |  | 1 |  |

## Data

- Combo/list sources: `Jud`, `ClientiEF`

## Controls

| # | Type | Name | Parent | Section | Pos L,T WxH | Caption | ControlSource | RowSource | Tab | Font | Events | Other |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| 1 | TextBox | Cont |  | Section | 1014,686 4316x275 |  |  |  | 1 | Consolas 9 bold | AfterUpdate |  |
| 2 | Label | Label5 | Cont | Section | 56,686 960x275 | Cont |  |  |  | Calibri 10 |  |  |
| 3 | TextBox | Banca |  | Section | 1014,1001 4316x275 |  |  |  | 2 | Consolas 9 bold |  |  |
| 4 | Label | Label6 | Banca | Section | 56,1001 960x275 | Banca |  |  |  | Calibri 10 |  |  |
| 5 | TextBox | Adresa |  | Section | 1018,1990 4316x275 |  |  |  | 3 | Consolas 9 bold |  |  |
| 6 | Label | Label10 | Adresa | Section | 60,1990 960x275 | Adresa |  |  |  | Calibri 10 |  |  |
| 7 | ComboBox | Judetul |  | Section | 1018,1330 4316x275 |  |  | `SELECT Jud.Cod, Jud.Judet FROM Jud; ` | 6 | Consolas 9 bold | OnChange | rowType=Table/Query; cols=2; colW=0 |
| 8 | Label | Label12 | Judetul | Section | 60,1330 960x275 | Județul |  |  |  | Calibri 10 |  |  |
| 9 | TextBox | Orasul |  | Section | 1018,1660 4316x275 |  |  |  | 5 | Consolas 9 bold |  |  |
| 10 | Label | Label14 | Orasul | Section | 60,1660 960x275 | Orașul |  |  |  | Calibri 10 |  |  |
| 11 | TextBox | RO |  | Section | 3639,56 441x275 |  |  |  | 7 | Consolas 10 bold |  |  |
| 12 | CheckBox | CNP |  | Section | 1039,56 200x195 |  |  |  | 8 |  | OnClick |  |
| 13 | Label | Label24 | CNP | Section | 1239,56 510x285 | CNP? |  |  |  | Arial Narrow 10 |  |  |
| 14 | ComboBox | IdClient |  | Section | 1010,371 4316x275 |  |  | `SELECT ClientiEF.IdClient, ClientiEF.DenumireClient FROM ClientiEF ORDER BY ClientiEF.DenumireClient; ` |  | Consolas 9 bold | AfterUpdate | rowType=Table/Query; cols=2; colW=0 |
| 15 | Label | Label3 | IdClient | Section | 59,371 960x275 | Denumire |  |  |  | Calibri 10 |  |  |
| 16 | TextBox | CodFiscal |  | Section | 1749,56 1896x275 |  |  |  | 9 | Consolas 10 bold | AfterUpdate |  |
| 17 | Label | Label4 | CodFiscal | Section | 56,56 960x275 | Cod Fiscal |  |  |  | Calibri 10 |  |  |
| 18 | ComboBox | Sector |  | Section | 1018,1660 4316x275 |  |  | `SECTOR1;SECTOR2;SECTOR3;SECTOR4;SECTOR5;SECTOR6` | 4 | Consolas 9 bold | AfterUpdate | rowType=Value List |
| 19 | Label | Label20 | Sector | Section | 60,1660 960x275 | Sector |  |  |  | Calibri 10 |  |  |
| 20 | TextBox | DenumireClient |  | Section | 1014,371 4316x275 |  |  |  | 10 | Consolas 9 bold |  |  |
| 21 | Label | Label28 | DenumireClient | Section | 56,371 960x275 | Denumire |  |  |  | Calibri 10 |  |  |
| 22 | CheckBox | E |  | FormFooter | 2211,0 432x288 |  |  |  | 1 |  |  |  |
| 23 | Button | bMod |  | FormFooter | 20,60 1746x335 |  Modifică |  |  | 2 | Consolas 9 bold | OnClick | tip=Close Form |
| 24 | Button | bUnd |  | FormFooter | 20,60 1746x335 |  Renunță |  |  | 3 | Consolas 9 bold | OnClick | tip=Close Form |
| 25 | Button | bAdd |  | FormFooter | 3630,60 1746x335 |  Adaugă client |  |  | 4 | Consolas 9 bold | OnClick | tip=Close Form |
| 26 | Button | bSav |  | FormFooter | 3630,60 1746x335 |  Salvează |  |  | 5 | Consolas 9 bold | OnClick | tip=Close Form |
| 27 | Button | FOC |  | FormFooter | 2834,113 114x113 | Command26 |  |  |  | Calibri 11 |  |  |

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
| bADD_Click | bAdd.OnClick | 39 |  |  |  |  |  |  |
| bMod_Click | bMod.OnClick | 33 |  |  |  |  |  |  |
| bSAV_Click | bSav.OnClick | 72 |  |  | `ClientiEF` | `mdl_MsBox.MsBox` |  |  |
| bUND_Click | bUnd.OnClick | 29 |  |  |  |  |  |  |
| CNP_Click | CNP.OnClick | 12 |  |  |  |  |  |  |
| CodFiscal_AfterUpdate | CodFiscal.AfterUpdate | 60 |  | `Jud` |  | `mdl_VerificaCNP.vercnp`, `mdl_VerificaCF.VER_CF`, `mdl_MsBox.MsBox`, `mdl_EFactura.InformatiiFirmaOnline` |  |  |
| Cont_AfterUpdate | Cont.AfterUpdate | 15 |  | `BIC` |  | `mdl_Functii_Generale.CreeazaIBAN`, `mdl_MsBox.MsBox` |  |  |
| DenumireClient_LostFocus |  | 5 |  |  |  |  |  |  |
| Form_Load | Form.OnLoad | 25 |  |  |  |  |  |  |
| Iesire_Click |  | 4 |  |  |  |  |  |  |
| IdClient_AfterUpdate (Public) | IdClient.AfterUpdate | 26 |  | `ClientiEF` |  |  |  |  |
| Judetul_Change | Judetul.OnChange | 6 |  |  |  |  |  |  |
| Sector_AfterUpdate | Sector.AfterUpdate | 3 |  |  |  |  |  |  |

## Opened from

`form:EFACTURA_ADD`

Raw source: `Surse/RawExport/Forms/EFACTURA_CLIENTI.txt`