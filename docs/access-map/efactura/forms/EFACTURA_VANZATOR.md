# Form `EFACTURA_VANZATOR`

Source: `Forms/EFACTURA_VANZATOR.txt` (70,328 chars). Units are twips (1 inch = 1440). `Parent` is the tab page or group a control sits in, or - for a label - the control it is attached to. Positions are relative to the section that holds the control; `LayoutCached*` values are used when present.

## Form properties

| Property | Value |
|---|---|
| DefaultView | 0 |
| NavigationButtons | NotDefault |
| RecordSelectors | NotDefault |
| DividingLines | NotDefault |
| ScrollBars | 0 |
| Width | 5840 |
| Left | 8280 |
| Top | 3756 |
| Right | 15228 |
| Bottom | 7224 |
| FitToScreen | 1 |
| GridY | 10 |
| TabularCharSet | 238 |
| DatasheetFontName | Calibri |
| DatasheetFontHeight | 11 |

Form events wired: `OnLoad` = VBA

## Sections

| Section | Height | Visible | BackColor | Events |
|---|---|---|---|---|
| FormHeader | 0 |  | 15849926 |  |
| Detail | 2324 |  | 1 |  |
| FormFooter | 845 |  | 1 |  |

## Data

- Combo/list sources: `Jud`

## Controls

| # | Type | Name | Parent | Section | Pos L,T WxH | Caption | ControlSource | RowSource | Tab | Font | Events | Other |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| 1 | TextBox | NumeUnitate |  | Section | 1326,56 4451x300 |  |  |  |  | Consolas 9 |  |  |
| 2 | Label | Label0 | NumeUnitate | Section | 56,56 1270x300 | Nume Unitate |  |  |  | Consolas 9 bold |  |  |
| 3 | ComboBox | Judetul |  | Section | 1330,410 4451x300 |  |  | `SELECT Jud.Cod, Jud.Judet FROM Jud ORDER BY Jud.Judet; ` | 1 | Consolas 9 |  | rowType=Table/Query; cols=2; colW=0; limit |
| 4 | Label | Label2 | Judetul | Section | 60,410 1270x300 | Județul |  |  |  | Consolas 9 bold |  |  |
| 5 | TextBox | Orasul |  | Section | 1330,710 4451x300 |  |  |  | 2 | Consolas 9 |  |  |
| 6 | Label | Label3 | Orasul | Section | 60,710 1270x300 | Orașul |  |  |  | Consolas 9 bold |  |  |
| 7 | TextBox | Adresa |  | Section | 1330,1010 4451x300 |  |  |  | 4 | Consolas 9 |  |  |
| 8 | Label | Label4 | Adresa | Section | 60,1010 1270x300 | Adresa |  |  |  | Consolas 9 bold |  |  |
| 9 | TextBox | Director |  | Section | 1334,1364 4451x300 |  |  |  | 3 | Consolas 9 |  |  |
| 10 | Label | Label5 | Director | Section | 64,1364 1270x300 | Contact |  |  |  | Consolas 9 bold |  |  |
| 11 | TextBox | TelefonContact |  | Section | 1334,1664 4451x300 |  |  |  | 5 | Consolas 9 |  |  |
| 12 | Label | Label6 | TelefonContact | Section | 64,1664 1270x300 | Telefon |  |  |  | Consolas 9 bold |  |  |
| 13 | TextBox | AdresaMail |  | Section | 1334,1964 4451x300 |  |  |  | 6 | Consolas 9 |  |  |
| 14 | Label | Label7 | AdresaMail | Section | 64,1964 1270x300 | Adresa Mail |  |  |  | Consolas 9 bold |  |  |
| 15 | Label | Label8 |  | FormFooter | 0,0 5840x510 | ATENȚIE! Orice modificare de aici are repercursiuni asupra tuturor documentelor generate în Avacont! |  |  |  | Arial Narrow 10 bold |  |  |
| 16 | Button | bMod |  | FormFooter | 0,510 1296x335 |  Modifică |  |  |  | Consolas 9 bold | OnClick | tip=Close Form |
| 17 | Button | bUnd |  | FormFooter | 0,510 1296x335 |  Renunță |  |  | 1 | Consolas 9 bold | OnClick | tip=Close Form |
| 18 | Button | bSav |  | FormFooter | 4614,510 1226x335 |  Salvează |  |  | 2 | Consolas 9 bold | OnClick | tip=Close Form |
| 19 | Button | FOC |  | FormFooter | 0,0 114x113 | Command26 |  |  | 3 | Calibri 11 |  |  |

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
| bMod_Click | bMod.OnClick | 20 |  |  |  |  |  |  |
| bSAV_Click | bSav.OnClick | 26 |  |  | `UNIT` | `mdl_MsBox.MsBox` |  | `bUND_Click` |
| bUND_Click | bUnd.OnClick | 20 |  |  |  |  |  |  |
| Form_Load | Form.OnLoad | 17 |  | `UNIT` |  |  |  |  |

## Opened from

`form:EFACTURA_ADD`

Raw source: `Surse/RawExport/Forms/EFACTURA_VANZATOR.txt`