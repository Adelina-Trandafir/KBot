# Form `EFACTURA_2025`

Source: `Forms/EFACTURA_2025.txt` (152,813 chars). Units are twips (1 inch = 1440). `Parent` is the tab page or group a control sits in, or - for a label - the control it is attached to. Positions are relative to the section that holds the control; `LayoutCached*` values are used when present.

## Form properties

| Property | Value |
|---|---|
| DefaultView | 0 |
| AutoCenter | NotDefault |
| BorderStyle | 0 |
| NavigationButtons | NotDefault |
| RecordSelectors | NotDefault |
| DividingLines | NotDefault |
| ScrollBars | 0 |
| Width | 18260 |
| Left | 3195 |
| Top | 870 |
| Right | 21615 |
| Bottom | 10050 |
| GridY | 10 |
| TabularCharSet | 238 |
| DatasheetFontName | Calibri |
| DatasheetFontHeight | 11 |

Form events wired: `OnClose` = VBA, `OnLoad` = VBA

## Sections

| Section | Height | Visible | BackColor | Events |
|---|---|---|---|---|
| FormHeader | 416 |  |  |  |
| PageHeaderSection | 0 |  | 1 |  |
| Detail | 5840 |  | 1 |  |
| PageFooterSection | 315 |  | 1 |  |
| FormFooter | 510 |  | 1 |  |

## Data

- Combo/list sources: `DefaNoteA`, `DefaNote`

## Controls

| # | Type | Name | Parent | Section | Pos L,T WxH | Caption | ControlSource | RowSource | Tab | Font | Events | Other |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| 1 | Button | bAA |  | FormHeader | 5102,110 260x300 |  Preluare an anterior  |  |  |  | Arial Narrow 9 bold | OnClick | picture=File-Code-XML-icon.png |
| 2 | Label | lblClsf |  | FormHeader | 5440,110 2993x300 | Clasificație bugetară |  |  |  | Consolas 10 bold |  |  |
| 3 | Label | lblCodclient |  | FormHeader | 8530,110 2562x300 | Cod Client |  |  |  | Consolas 10 bold |  |  |
| 4 | Label | lblContIBAN |  | FormHeader | 11200,110 3452x300 | Cont IBAN |  |  |  | Consolas 10 bold |  |  |
| 5 | Label | lblFelOp |  | FormHeader | 14740,110 3473x300 | Fel factură (pentru O.P.uri) |  |  |  | Consolas 10 bold |  |  |
| 6 | Button | FOC |  | FormHeader | 0,0 109x110 | Command49 |  |  | 1 | Calibri 11 |  |  |
| 7 | Button | FN |  | FormHeader | 402,110 1610x300 |  Facturi noi |  |  | 2 | Arial 8 bold | OnClick | tag=16249583 |
| 8 | Button | FS |  | FormHeader | 2012,110 1790x300 |  Facturi salvate |  |  | 3 | Arial 8 bold | OnClick | tag=16249583 |
| 9 | Button | FA |  | FormHeader | 3802,110 1560x300 |  Arhivă |  |  | 4 | Arial 8 bold | OnClick | tag=16249583 |
| 10 | Button | bXML |  | FormHeader | 22,110 380x300 |  |  |  | 5 | Arial 9 bold | OnClick==Popup_EFactura_IncarcaXML() | picture=C009.png; tip=Opțiuni diverse pentru E-FACTURĂ |
| 11 | Subform | subTreeView |  | Section | 40,60 5340x5740 |  |  |  |  |  |  |  |
| 12 | ListBox | LstEv |  | Section | 5463,703 2993x2522 |  |  | `SELECT DefaNoteA.IDA, DefaNoteA.Explicatie FROM DefaNoteA INNER JOIN DefaNote ON DefaNoteA.IDA = DefaNote.IDA GROUP BY DefaNoteA.IDA, DefaNoteA.Explicatie, Defa...` | 1 | Consolas 9 | AfterUpdate, OnMouseDown | rowType=Table/Query; cols=2; colW=0;4536; tag=SELECT DefaNoteA.IDA, DefaNoteA.Explicatie, DefaNoteA.Fel, DefaNoteA.Jurnal, DefaNoteA.Compartiment, DefaNoteA.CodPartener FROM DefaNoteA INNER JOIN DefaNote ON DefaNoteA.IDA = DefaNote.IDA GROUP BY DefaNoteA.IDA, DefaNoteA.Explicatie, DefaNoteA.Fel, DefaNoteA.Jurnal, DefaNoteA.Compartiment, DefaNoteA.CodPartener HAVING (((DefaNoteA.Fel)='E') AND ((DefaNoteA.CodPartener) Like [forms]![opuri2017]![clsf])) ORDER BY DefaNoteA.IDA; |
| 13 | Label | Label2 | LstEv | Section | 5463,437 2993x266 | Documente evidențiere |  |  |  | Consolas 10 bold |  |  |
| 14 | ListBox | LstPl |  | Section | 5463,3554 2993x2252 |  |  | `SELECT DefaNoteA.IDA, DefaNoteA.Explicatie FROM DefaNoteA INNER JOIN DefaNote ON DefaNoteA.IDA = DefaNote.IDA GROUP BY DefaNoteA.IDA, DefaNoteA.Explicatie, Defa...` | 2 | Consolas 9 | AfterUpdate, OnMouseDown | rowType=Table/Query; cols=2; colW=0;4536; tag=SELECT DefaNoteA.IDA, DefaNoteA.Explicatie, DefaNoteA.Fel, DefaNoteA.Jurnal, DefaNoteA.FelOP, DefaNoteA.CodPartener, DefaNoteA.Compartiment FROM DefaNoteA INNER JOIN DefaNote ON DefaNoteA.IDA = DefaNote.IDA GROUP BY DefaNoteA.IDA, DefaNoteA.Explicatie, DefaNoteA.Fel, DefaNoteA.Jurnal, DefaNoteA.FelOP, DefaNoteA.CodPartener, DefaNoteA.Compartiment HAVING (((DefaNoteA.Fel)='P') AND ((DefaNoteA.CodPartener) Like [forms]![opuri2017]![clsf])) ORDER BY DefaNoteA.IDA; |
| 15 | Label | Label1 | LstPl | Section | 5463,3288 2993x266 | Documente plată |  |  |  | Consolas 10 bold |  | tag= -> |
| 16 | Button | bClsf |  | Section | 5456,47 2993x300 | 123456789_123456789_12345678 |  |  | 3 | Consolas 10 bold | OnClick | tag=Alege clasificația |
| 17 | Rectangle | anchorBox |  | Section | 0,0 18260x5840 |  |  |  |  |  |  |  |
| 18 | Subform | tmpEV |  | Section | 8527,439 9683x2790 |  |  |  | 4 |  | OnExit |  |
| 19 | Subform | tmpPl |  | Section | 8532,3288 9683x2525 |  |  |  | 5 |  | OnExit |  |
| 20 | TextBox | CodClient |  | Section | 8527,45 2562x300 |  |  |  | 6 | Consolas 10 |  |  |
| 21 | TextBox | ContIBAN |  | Section | 11197,45 3452x300 |  |  |  | 7 | Consolas 10 |  |  |
| 22 | TextBox | FelOp |  | Section | 14737,45 3473x300 |  |  |  | 8 | Consolas 10 |  |  |
| 23 | TextBox | EFT |  | PageFooter | 0,0 996x315 |  |  |  | 3 | Calibri 11 |  |  |
| 24 | TextBox | CODP |  | PageFooter | 996,0 996x315 |  |  |  |  | Calibri 11 |  |  |
| 25 | TextBox | NUMP |  | PageFooter | 1992,0 996x315 |  |  |  | 1 | Calibri 11 |  |  |
| 26 | TextBox | cui_unit |  | PageFooter | 2988,0 996x315 |  |  |  | 2 | Calibri 11 |  |  |
| 27 | TextBox | Zile |  | PageFooter | 3984,0 996x315 |  |  |  | 4 | Calibri 11 |  |  |
| 28 | TextBox | FORMA |  | PageFooter | 4960,0 996x315 |  |  |  | 14 | Calibri 11 |  |  |
| 29 | TextBox | Lvl |  | PageFooter | 5976,0 996x315 |  |  |  | 5 | Calibri 11 |  |  |
| 30 | TextBox | id_Sol |  | PageFooter | 6972,0 996x315 |  |  |  | 6 | Calibri 11 |  |  |
| 31 | TextBox | CUIF |  | PageFooter | 7984,0 996x315 |  |  |  | 8 | Calibri 11 |  |  |
| 32 | TextBox | DataF |  | PageFooter | 8976,0 996x315 |  |  |  | 9 | Calibri 11 |  |  |
| 33 | TextBox | IdClsf |  | PageFooter | 10015,0 996x315 |  |  |  | 10 | Calibri 11 |  |  |
| 34 | TextBox | Clsf |  | PageFooter | 11102,0 996x315 |  |  |  | 12 | Calibri 11 |  |  |
| 35 | TextBox | UltimulOP |  | PageFooter | 12118,0 996x315 |  |  |  | 11 | Calibri 11 |  |  |
| 36 | TextBox | IdOperatie |  | PageFooter | 16629,0 996x315 |  |  |  | 13 | Calibri 11 |  |  |
| 37 | CheckBox | CHK |  | PageFooter | 15519,30 260x240 |  |  |  | 7 |  |  |  |
| 38 | Label | Label30 | CHK | PageFooter | 15749,0 840x315 | Check29 |  |  |  | Calibri 11 |  |  |
| 39 | TextBox | LunaAn |  | PageFooter | 13152,0 996x315 |  |  |  | 15 | Calibri 11 |  |  |
| 40 | Button | bSAV |  | FormFooter | 16343,15 1872x450 | Salvează  |  |  |  | Consolas 10 bold | OnClick |  |
| 41 | Button | bRap |  | FormFooter | 3786,15 1872x450 | Rapoarte  |  |  | 1 | Consolas 10 bold | OnClick==Make_Popup_Efactura() |  |
| 42 | Button | bUND |  | FormFooter | 8532,15 1872x450 |  Renunță |  |  | 2 | Consolas 10 bold | OnClick |  |
| 43 | Button | IESIRE |  | FormFooter | 45,15 1872x450 |  IEȘIRE |  |  | 3 | Consolas 10 bold | OnClick |  |

## Module-level declarations

Form state kept in variables, constants, types and WithEvents helpers (the Access form class body before the first procedure).

```vb
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = True
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
Public WithEvents mcTree As clsTreeView
Attribute mcTree.VB_VarHelpID = -1
Public WithEvents cAnchor As clsAnchorManager
Attribute cAnchor.VB_VarHelpID = -1
Private WithEvents mPick As Form_Note_PickClsf
Attribute mPick.VB_VarHelpID = -1
Public FACTURI As Single 'decide tipul de copac: 1) Facturi noi, 2) Facturi Salvate, 3) Facturi Ascunse
Private skipNodeCheckEvent As Boolean 'il folosesc cand bifez noduri din cod, nu cu mausul
Private skipNodeClickEvent As Boolean
Private lngEvOld As Long 'pastreaza ultima valoare apasata in lstEv. o folosesc pentru a dezactiva
Private lngPlOld As Long 'pastreaza ultima valoare apasata in lstEv. o folosesc pentru a dezactiva
Private colChkNodes As Collection 'pastrez nodurile bifate.
Public isTreeReady As Boolean
Private timerVAR As Integer 'pentru a decide ce rulez in timer. Valori:
                            '0 - la deschiderea form-ului, se ruleaza functiile initiale
                            '1 - s-a bifat/debifat nod manual (cu mausul)
'============ FORM =======================
```

## Event handlers and what they touch

Reads/writes come from SQL text and recordset calls found in the procedure body; they are a lead for reading the code, not a proof (dynamic SQL built from variables is missed).

| Procedure | Bound to | Lines | Opens forms/reports | Reads | Writes | Calls (modules) | Classes | Local calls |
|---|---|---|---|---|---|---|---|---|
| Form_Close | Form.OnClose | 33 |  |  |  | `mdl_Functii2023.isLoaded` |  |  |
| Form_Load | Form.OnLoad | 39 |  |  |  | `mdl_2025.ObtineIDURI`, `mdl_2025.ObtineIDUri` |  | `InitVars`, `CurataTabele`, `UltimaLunaInchisa` |
| bAA_Click (Public) | bAA.OnClick | 5 | `EFACTURA_AA` | `EFACTURA_AA` |  |  |  | `Pop_EFACTURA_25` |
| FA_Click | FA.OnClick | 20 |  |  |  |  |  | `Init_Pop_EFACTURA_25`, `Pop_EFACTURA_25` |
| FN_Click | FN.OnClick | 55 |  |  |  | `mdl_2025.Token_Key`, `mdl_2025.Token_Expiry`, `mdl_Popup2022.Make_Popup_EFactura_NumarZile`, `mdl_EFactura.CautaFacturiNoi` |  | `Pop_EFACTURA_25`, `Init_Pop_EFACTURA_25` |
| FS_Click | FS.OnClick | 20 |  |  |  |  |  | `Init_Pop_EFACTURA_25`, `Pop_EFACTURA_25` |
| bSAV_Click | bSAV.OnClick | 97 |  | `tblMSG`, `Contracte`, `ParteneriAng` | `tmpFacturi`, `tmpDoc2017P`, `EFT`, `tmpQEF` | `mdl_MsBox.ptMS`, `mdl_MsBox.MsBox`, `mdl_2025.ObtineIDURI`, `mdl_Salvare_NoteContabile.SaveEFactura` |  | `Pop_EFACTURA_25` |
| bClsf_Click | bClsf.OnClick | 10 | `Note_PickClsf` |  |  | `mdl_Functii2023.isLoaded` |  |  |
| bUND_Click | bUND.OnClick | 22 |  |  |  |  |  | `InitVars` |
| Iesire_Click | IESIRE.OnClick | 5 |  |  |  |  |  |  |
| LstEv_AfterUpdate | LstEv.AfterUpdate | 14 |  |  |  |  |  | `AdaugaDate_Evidentiere`, `Formateaza_TmpEv`, `EliminaDubluri`, `AjusteazaDataFacturi`, `Formateaza_bSAV` |
| LstEv_MouseDown | LstEv.OnMouseDown | 5 |  |  |  |  |  |  |
| LstPl_AfterUpdate | LstPl.AfterUpdate | 6 |  |  |  |  |  | `AdaugaDate_Plata`, `Formateaza_TmpPl`, `Formateaza_bSAV` |
| LstPl_MouseDown | LstPl.OnMouseDown | 4 |  |  |  |  |  |  |
| tmpEV_Exit | tmpEV.OnExit | 5 |  |  |  |  |  |  |
| tmpPl_Exit | tmpPl.OnExit | 11 |  |  |  |  |  |  |
| cAnchor_AnchorInitFinished | WithEvents cAnchor (clsAnchorManager).AnchorInitFinished | 9 |  |  |  |  |  | `FN_Click`, `Init_Pop_EFACTURA_25`, `InitVarsTrue` |
| mcTree_Click | WithEvents mcTree (clsTreeView).Click | 46 |  | `tmpQEF`, `Clasificatii` |  |  |  |  |
| mcTree_Checked | WithEvents mcTree (clsTreeView).Checked | 39 |  |  |  |  |  | `Actualizeaza_EFT_tmpQEF`, `AdaugaDate_Evidentiere`, `EliminaDubluri`, `AjusteazaDataFacturi`, `AdaugaDate_Plata`, `Formateaza_TmpEv`, `Formateaza_TmpPl` |
| mcTree_RightClick | WithEvents mcTree (clsTreeView).RightClick | 4 |  |  |  | `mdl_Popup2022.Make_Popup_EFactura_Raport` |  |  |
| mcTree_TreeLoaded | WithEvents mcTree (clsTreeView).TreeLoaded | 4 |  |  |  |  |  | `Pop_EFACTURA_25` |
| mcTree_TreeRefreshed | WithEvents mcTree (clsTreeView).TreeRefreshed | 31 |  |  |  |  |  |  |
| mPick_ClsfClick | WithEvents mPick (Form_Note_PickClsf).ClsfClick | 37 |  |  |  |  |  | `CautaInfoPartener`, `LstEv_AfterUpdate` |
| Init_Pop_EFACTURA_25 (Public) |  | 35 |  |  |  | `mdl_Functii2024.imgDic` |  |  |
| Pop_EFACTURA_25 (Public) |  | 135 |  | `QEF_NOI`, `QEF_SAV`, `QEF_ARH` | `tmpQEF` | `mdl_Functii2023.fLuna`, `mdl_Functii_Generale.FormatN` |  |  |
| Actualizeaza_EFT_tmpQEF |  | 34 |  |  | `tmpQEF`, `EFT` |  |  |  |
| AdaugaDate_Evidentiere (Public) |  | 52 |  | `QEF_EV`, `QEF_PL` | `tmpFacturi` |  |  |  |
| AdaugaDate_Plata (Public) |  | 66 |  | `DefaNoteA`, `QEF_Plata` | `tmpDoc2017P` | `mdl_MsBox.MsBox`, `mdl_Functii_Generale.UltimulNumarOP` |  |  |
| AdaugareFacturiBifate (Public) |  | 99 |  | `tmpQEF` |  | `mdl_MsBox.MsBox` |  | `InitVarsTrue`, `VerificaAnFactura`, `CautaInfoClasificatie`, `CautaInfoPartener`, `Actualizeaza_EFT_tmpQEF`, `LstEv_AfterUpdate`, `AdaugaDate_Evidentiere`, `AdaugaDate_Plata`, `Formateaza_TmpEv`, `Formateaza_TmpPl` |
| AjusteazaDataFacturi |  | 49 |  |  | `tmpFacturi` |  |  |  |
| CautaInfoClasificatie |  | 50 |  | `Clasificatii`, `DefaNote`, `Parteneri`, `ParteneriAng` |  | `mdl_Functii2023.Concat_WS` |  |  |
| CautaInfoPartener |  | 44 |  | `tmpQEF`, `Parteneri`, `ParteneriAng`, `Contracte` |  | `mdl_MsBox.MsBox` |  |  |
| CurataTabele |  | 8 |  |  | `EFT`, `EFS`, `tmpDoc2017P`, `tmpFacturi` |  |  |  |
| EliminaDubluri |  | 36 |  | `EFT`, `tblMSG` | `tmpFacturi` | `mdl_MsBox.MsBox` |  |  |
| Formateaza_bSAV |  | 10 |  |  |  |  |  |  |
| Formateaza_TmpEv |  | 35 |  | `EF_F` |  |  |  |  |
| Formateaza_TmpPl |  | 27 |  |  |  |  |  |  |
| InitVars |  | 54 |  |  |  |  |  |  |
| InitVarsTrue |  | 34 |  |  | `tmpFacturi`, `tmpDoc2017P` |  |  |  |
| VerificaAnFactura (Public) |  | 53 |  | `Documente` |  | `mdl_MsBox.MsBox` |  | `InitVars` |
| UltimaLunaInchisa |  | 9 |  | `Documente` |  |  |  |  |

## Opened from

`query:SAV_ACTIUNE_EV`, `query:SAV_ACTIUNE_PL`, `query:SAV_Cont8067_EV`, `query:SAV_Cont8067_PL`, `query:SAV_Credit_EV`, `query:SAV_Credit_PL`, `query:SAV_Debit_EV`, `query:SAV_Debit_PL`, `query:SAV_Document_EV`, `query:SAV_OP`, `query:SAV_OPER_EV`, `query:SAV_OPER_PL`, `query:SAV_PlatiFacturi`, `form:Note`, `module:basRibbonCallbacks`, `module:mdl_Popup2022`, `module:mdl_Salvare_NoteContabile`, `query:QEF_EV`, `query:QEF_PL`, `query:QEF_Plata`, `query:QEF_RestDePlata`, `query:QEF_RestDePlata_Data`, `form:EF_F`, `form:EF_F_inlucru`, `form:EF_P`, `module:mdl_EFactura`

Raw source: `Surse/RawExport/Forms/EFACTURA_2025.txt`