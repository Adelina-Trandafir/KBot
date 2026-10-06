# What the e-invoice scope needs from the rest of Avacont

Code and objects outside the scope that scope objects call. Everything in the table must either be rebuilt in K-BOT, replaced by a K-BOT service/control, or deliberately dropped. Noise (tracing, `Compare`, `Terminate`, ...) is filtered out.

## Procedures from other modules

| Procedure | Used by (count) | Signature | Examples |
|---|---|---|---|
| `mdl_MsBox.MsBox` | 11 | Public Function MsBox(Optional ID As Long, Optional Mesaj As String, Optional Titlu As String, Optional Fel As | `form:EFACTURA_2025`, `form:EFACTURA_ADD`, `form:EFACTURA_ADD_SUB`, `form:EFACTURA_CLIENTI` (+7) |
| `mdl_Functii2023.isLoaded` | 6 | Function isLoaded(ByVal strFormName As String) As Boolean | `form:EFACTURA_2025`, `form:EFACTURA_MSG`, `form:EFACTURA_PickIBAN`, `form:EFACTURA_TMP` (+2) |
| `mdl_Functii2023.Concat_WS` | 6 | Public Function Concat_WS(ParamArray values() As Variant) As String | `form:EFACTURA_2025`, `form:EFACTURA_ADD_FACTURI`, `form:EFACTURA_MSG`, `module:mdl_EFactura` (+2) |
| `mdl_MsBox.ptMS` | 5 | Public Function ptMS(Optional TXT As String, Optional ID As Long = 0, Optional Prms As String) As String | `form:EFACTURA_2025`, `form:EFACTURA_UM`, `form:EF_F`, `form:EF_F_inlucru` (+1) |
| `mdl_GLOBALS.globANL` | 4 | Public Function globANL(Optional NANL As Variant) As Long | `form:EFACTURA_AA`, `form:EF_F`, `form:EF_F_inlucru`, `form:EF_P` |
| `mdl_2025.Token_Key` | 3 | Public Function Token_Key() As Variant | `form:EFACTURA_2025`, `module:mdl_EFactura`, `class:clsAnaf_Async` |
| `mdl_Sendkeys.SendKeysAPI` | 3 | Public Function SendKeysAPI(ByVal strText As String) | `form:EF_F`, `form:EF_F_inlucru`, `form:EF_P` |
| `mdl_CheckTVW.FileExists` | 3 | Public Function FileExists(ByVal sFile As String) As Boolean | `form:MF2019_Asoc_EF`, `form:Note_EFactura`, `module:mdl_EFactura` |
| `mdl_Functii2023.mmin` | 3 | Public Function mMin(ParamArray values() As Variant) As Double | `form:Note_EFactura`, `form:_MF2019_Asoc_EF`, `query:_qNote_EFactura_Asociat_Nou` |
| `mdl_2025.Token_Expiry` | 2 | Public Function Token_Expiry() As Variant | `form:EFACTURA_2025`, `module:mdl_EFactura` |
| `mdl_Functii2024.imgDic` | 2 | Public Function imgDic(Optional item As Variant) As Variant | `form:EFACTURA_2025`, `module:mdl_EFactura` |
| `mdl_Functii2023.fLuna` | 2 | Function fLuna(L As String) As String | `form:EFACTURA_2025`, `form:EFACTURA_AA` |
| `mdl_Functii_Generale.CreeazaIBAN` | 2 | Public Function CreeazaIBAN(IBAN As String) As String | `form:EFACTURA_ADD`, `form:EFACTURA_CLIENTI` |
| `mdl_Functii2023.mMin` | 2 | Public Function mMin(ParamArray values() As Variant) As Double | `form:MF2019_Asoc_EF`, `form:Note_EFactura` |
| `mdl_2025.CALE_EF` | 2 | Public Function CALE_EF(Optional withTrSlash As Boolean = False) As String | `module:mdl_EFactura`, `module:mdl_EFactura_Add` |
| `mdl_JSON.ParseJson` | 2 | Public Function ParseJson(ByVal jsonString As String, Optional JsonSep As String = "") As Object | `module:mdl_EFactura`, `class:clsAnaf_Async` |
| `mdl_Functii2023.REGEXP` | 2 | Function REGEXP(S As Variant, R As Variant, Optional P As Variant = -1) As String | `query:QEF_EV`, `query:QEF_EV_inlucru` |
| `mdl_Functii2023.REGEXP_COUNT` | 2 | Function REGEXP_COUNT(S As String, R As String) As Integer | `query:QEF_EV`, `query:QEF_EV_inlucru` |
| `mdl_2025.ConcatRelated` | 2 | Public Function ConcatRelated(strField As String, _ | `query:QEF_OPNU`, `query:QEF_Plata` |
| `mdl_Popup2022.Popup_EFactura_IncarcaXML` | 1 | Public Function Popup_EFactura_IncarcaXML() | `form:EFACTURA_2025` |
| `mdl_Popup2022.Make_Popup_Efactura` | 1 | Public Function Make_Popup_Efactura() | `form:EFACTURA_2025` |
| `mdl_2025.ObtineIDURI` | 1 | Public Function ObtineIDURI() As Boolean | `form:EFACTURA_2025` |
| `mdl_2025.ObtineIDUri` | 1 | Public Function ObtineIDURI() As Boolean | `form:EFACTURA_2025` |
| `mdl_Popup2022.Make_Popup_EFactura_NumarZile` | 1 | Public Function Make_Popup_EFactura_NumarZile() | `form:EFACTURA_2025` |
| `mdl_Salvare_NoteContabile.SaveEFactura` | 1 | Public Function SaveEFactura() As Boolean | `form:EFACTURA_2025` |
| `mdl_Popup2022.Make_Popup_EFactura_Raport` | 1 | Public Function Make_Popup_EFactura_Raport(ByVal cNode As clsTreeNode) | `form:EFACTURA_2025` |
| `mdl_Functii_Generale.FormatN` | 1 | Public Function FormatN(Optional ByVal Suma_Text As String = "", Optional Lungime As Integer = 13, Optional Ro | `form:EFACTURA_2025` |
| `mdl_Functii_Generale.UltimulNumarOP` | 1 | Public Function UltimulNumarOP() As Integer | `form:EFACTURA_2025` |
| `mdl_Popup2022.Popup_EFactura_Optiuni` | 1 | Public Function Popup_EFactura_Optiuni() | `form:EFACTURA_ADD` |
| `mdl_Functii_Generale.DMAX_EFACTURA` | 1 | Public Function DMAX_EFACTURA(COL As String, Tbl As String, Optional Cond As String = "") As Variant | `form:EFACTURA_ADD` |
| `mdl_Popup2022.Make_Popup_EFV` | 1 | Public Function Make_Popup_EFV() | `form:EFACTURA_ADD` |
| `mdl_Messagebox_hook.MsgBoxCB` | 1 | Function MsgBoxCB(MsgBox_Text As String, Button1 As String, Optional Button2 As String, Optional Button3 As St | `form:EFACTURA_ADD` |
| `mdl_PositionRelativeToControl.PositionFormRelativeToControl` | 1 | Public Function PositionFormRelativeToControl(frmName As String, Ctl As Access.control, SizeToControl As Boole | `form:EFACTURA_ADD` |
| `mdl_VerificaCNP.vercnp` | 1 | Function vercnp(CNP As String) As Integer | `form:EFACTURA_CLIENTI` |
| `mdl_VerificaCF.VER_CF` | 1 | Function VER_CF(CF As String) As Boolean | `form:EFACTURA_CLIENTI` |
| `mdl_2026.GenerateUniqueSequence` | 1 | Public Function GenerateUniqueSequence(numberOfCharacters As Integer) As String | `form:EFACTURA_PickIBAN` |
| `mdl_Functii_Generale.NumarOPFolosit` | 1 | Public Function NumarOPFolosit(Numar As Long) As String | `form:EF_P` |
| `mdl_ThemeManager.ThemeManager` | 1 | Public Function ThemeManager() As clsThemeManager | `form:MF2019_Asoc_EF` |
| `mdl_Functii2023.mmax` | 1 | Public Function mMax(ParamArray values() As Variant) | `form:MF2019_Asoc_EF` |
| `mdl_GLOBALS.globSectorSursa` | 1 | Public Function globSectorSursa(Optional nSS As Variant) As String | `form:_MF2019_Asoc_EF` |
| `mdl_GLOBALS.globIdUnitate` | 1 | Public Function globIdUnitate(Optional nIDU As Variant) As Long | `form:_MF2019_Asoc_EF` |
| `mdl_Functii_Generale.Folder` | 1 | Function Folder(Fld As String) As String | `module:mdl_EFactura` |
| `mdl_Functii_Generale.folder` | 1 | Function Folder(Fld As String) As String | `module:mdl_EFactura` |
| `mdl_GLOBALS.init_globals` | 1 | Function init_globals() | `module:mdl_EFactura` |
| `mdl_2025.Token_RefreshKey` | 1 | Public Function Token_RefreshKey() As Variant | `module:mdl_EFactura` |
| `mdl_2026.RefreshAccessToken` | 1 | Public Function RefreshAccessToken() As Boolean | `module:mdl_EFactura` |
| `mdl_DISK_RW.SaveByteArrayToFile` | 1 | Sub SaveByteArrayToFile(ByVal filePath As String, byteArray() As Byte) | `module:mdl_EFactura` |
| `mdl_2025.Token_NewLocation` | 1 | Public Function Token_NewLocation() As Boolean | `module:mdl_EFactura` |
| `mdl_2025.token_key` | 1 | Public Function Token_Key() As Variant | `module:mdl_EFactura` |
| `mdl_Functii2022.Productie` | 1 | Public Function Productie() As Boolean | `class:clsAnaf_Async` |
| `mdl_2025.concatrelated` | 1 | Public Function ConcatRelated(strField As String, _ | `query:QEF_DePlata` |
| `mdl_Functii2023.Regexp` | 1 | Function REGEXP(S As Variant, R As Variant, Optional P As Variant = -1) As String | `query:QEF_Plata` |
| `mdl_Functii2023.concat_ws` | 1 | Public Function Concat_WS(ParamArray values() As Variant) As String | `query:QEF_SAV` |
| `mdl_TRECERE_AN.TrecereAN` | 1 | Function TrecereAN() As Boolean | `query:QEF_TRECERE` |
| `mdl_Functii2023.roundcut` | 1 | Public Function RoundCut(dblValue As Double) As Double | `query:UEFP` |

## Classes instantiated or declared

| Class | Used by (count) | Examples |
|---|---|---|

## Shared forms

`Note_PickClsf`, `Setari implicite`, `MF2019`, `Note`, `XMLPath`, `TrecereAN`, `Balanta`

## Shared tables

`BIC`, `Balanta_Forexe`, `CALE`, `CONFIGS`, `CT8030`, `Clasificatii`, `ClasificatiiV`, `Cont8067`, `Contracte`, `Contracte_A`, `Credit`, `Debit`, `DefaCont`, `DefaNote`, `DefaNoteA`, `DefaSub`, `DefaTitlu2`, `Documente`, `EXPLOP`, `IMG`, `Jud`, `Oper`, `Parteneri`, `ParteneriAng`, `ParteneriSI`, `PlanCont`, `PlatiFacturi`, `Rectificari`, `RectificariV`, `Scheme`, `TEMPF`, `TmpBalantaListare`, `TmpConturi`, `UNIT`, `_TempCredit`, `dispozitii_sub`, `tblFiles`, `tmpConturiIBAN`, `tmpDoc2017P`, `tmpFacturi`, `tmpQEF`, `tmpXMLPath`

Field lists in `shared-tables.md`.