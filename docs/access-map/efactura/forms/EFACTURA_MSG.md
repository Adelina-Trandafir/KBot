# Form `EFACTURA_MSG`

Source: `Forms/EFACTURA_MSG.txt` (26,968 chars). Units are twips (1 inch = 1440). `Parent` is the tab page or group a control sits in, or - for a label - the control it is attached to. Positions are relative to the section that holds the control; `LayoutCached*` values are used when present.

## Form properties

| Property | Value |
|---|---|
| Caption | Mesaje Factură |
| DefaultView | 0 |
| AutoCenter | NotDefault |
| BorderStyle | 3 |
| NavigationButtons | NotDefault |
| RecordSelectors | NotDefault |
| DividingLines | NotDefault |
| ScrollBars | 0 |
| Width | 6994 |
| Left | 11748 |
| Top | 3900 |
| Right | 18744 |
| Bottom | 8088 |
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
| Detail | 3686 |  | 1 |  |
| FormFooter | 510 |  | 1 |  |

## Data

- Combo/list sources: `EFT`, `EFT_M`

## Controls

| # | Type | Name | Parent | Section | Pos L,T WxH | Caption | ControlSource | RowSource | Tab | Font | Events | Other |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| 1 | ListBox | MSGT |  | Section | 56,311 2438x1500 |  |  | `SELECT EFT_M.IDMSG, EFT_M.Data FROM EFT INNER JOIN EFT_M ON EFT.IDEFT = EFT_M.IDEFT WHERE (((EFT.id_sol)=[Forms]![EFACTURA_2025]![id_Sol])) ORDER BY EFT_M.Data ...` |  | Consolas 9 | OnClick | rowType=Table/Query; cols=2; colW=0 |
| 2 | Label | Label1 | MSGT | Section | 56,56 2438x255 | Mesaje trimise |  |  |  | Consolas 9 bold |  |  |
| 3 | TextBox | MSG |  | Section | 2551,311 4373x3300 |  |  |  | 2 | Consolas 9 bold | OnKeyPress |  |
| 4 | Label | Label3 | MSG | Section | 2551,56 4373x255 | Compune mesaj |  |  |  | Consolas 9 bold |  |  |
| 5 | ListBox | RSP |  | Section | 56,2115 2438x1500 |  |  |  | 1 | Consolas 9 |  | rowType=Table/Query; cols=2; colW=0 |
| 6 | Label | Label6 | RSP | Section | 56,1860 2438x255 | Răspunsuri |  |  |  | Consolas 9 bold |  |  |
| 7 | Button | bSend |  | FormFooter | 4700,56 2217x360 |   Trimite mesaj |  |  |  | Consolas 9 bold | OnClick | picture=Send-message-2-icon.png |
| 8 | Button | FOC |  | FormFooter | 2891,226 113x114 | Command4 |  |  | 1 | Calibri 11 |  |  |

## Module-level declarations

Form state kept in variables, constants, types and WithEvents helpers (the Access form class body before the first procedure).

```vb
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = True
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
Private RC As DAO.Recordset
```

## Event handlers and what they touch

Reads/writes come from SQL text and recordset calls found in the procedure body; they are a lead for reading the code, not a proof (dynamic SQL built from variables is missed).

| Procedure | Bound to | Lines | Opens forms/reports | Reads | Writes | Calls (modules) | Classes | Local calls |
|---|---|---|---|---|---|---|---|---|
| bSend_Click | bSend.OnClick | 15 |  |  |  | `mdl_EFactura.TrimiteMesajFactura` |  |  |
| Form_Load | Form.OnLoad | 14 |  | `tmpQEF` |  | `mdl_Functii2023.isLoaded`, `mdl_Functii2023.Concat_WS` |  |  |
| MSG_KeyPress | MSG.OnKeyPress | 4 |  |  |  |  |  |  |
| MSGT_Click | MSGT.OnClick | 5 |  | `EFT_M` |  |  |  |  |

## Opened from

`module:mdl_Popup2022`

Raw source: `Surse/RawExport/Forms/EFACTURA_MSG.txt`