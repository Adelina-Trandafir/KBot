# Code in scope

Procedures of the standard modules/classes that are specific to e-invoice, with what each touches. Line numbers are inside the exported `.txt` file.

SLICE-D003-20: procedures with identical Scope and effect columns share a row within their original module/class. Each entry is `Line · Procedure · Length · Comment above`; an empty last part means the original comment was empty. Numeric Line restores the original order. The static-scan limits remain unchanged.

## module `mdl_EFactura`

Source: `Modules/mdl_EFactura.bas.txt` (96,535 chars, 33 procedures)

| Procedures (Line · name · length · comment) | Scope | Tables/queries | Writes | Opens forms | Calls outside | Local calls |
|---|---|---|---|---|---|---|
| 28 · `CautaFacturiNoi` · 207 · ====== preluare facturi din sistemul E-Factura ========= | Public | `CALE`, `EF_TMP`, `Scheme`, `UNIT` | `EF_TMP`, `UNIT`, `EF` | `EFACTURA_2025`, `Mesaj_ANAF`, `EFACTURA_TMP` | `mdl_2025.CALE_EF`, `mdl_Functii2023.isLoaded`, `mdl_2025.Token_Key`, `mdl_JSON.ParseJson`, `mdl_MsBox.MsBox` | `InformatiiFirmaOnline`, `DescarcaFacturi` |
| 235 · `DescarcaFacturi` · 99 ·  | Private | `EF_TMP`, `UNIT` | `EF`, `EF_TMP`, `UNIT` |  | `mdl_2025.CALE_EF` | `DescarcaFactura`, `entUnZip1File`, `ParseXML` |
| 334 · `DescarcaFactura` · 64 ·  | Public |  |  |  | `mdl_2025.CALE_EF`, `mdl_CheckTVW.FileExists`, `mdl_2025.Token_Key` |  |
| 398 · `RestabilesteXML` · 49 ·  | Public |  |  |  | `mdl_2025.CALE_EF`, `mdl_CheckTVW.FileExists` | `entUnZip1File` |
| 447 · `DescarcaFacturaPDF` · 54 ·  | Public | `EFT` |  |  | `mdl_2025.CALE_EF`, `mdl_CheckTVW.FileExists`, `mdl_JSON.ParseJson` | `RestabilesteXML` |
| 501 · `EncodeStringToBytes` · 16 · Helper function to encode a string to a byte array with specified charset<br>936 · `Export_XML_TO_PDF` · 124 · <br>1604 · `PrintXMLStructure` · 56 · <br>1765 · `GenereazaXMLMesaj` · 23 · <br>2617 · `ReadFileAsByteArray` · 11 · Func?ie pentru a citi un fi?ier ca matrice de octe?i (byte array)<br>2628 · `StringToByteArray` · 3 · Func?ie pentru a converti un string �ntr-o matrice de octe?i (folosind conversia ANSI) | Public |  |  |  |  |  |
| 517 · `entUnZip1File` · 70 ·  | Public |  |  |  | `mdl_Functii_Generale.Folder`, `mdl_CheckTVW.FileExists`, `mdl_Functii_Generale.folder` |  |
| 587 · `PrelucreazaZipLocal` · 88 ·  | Public | `EF_TMP` | `EF_TMP`, `EF` | `EFACTURA_TMP` | `mdl_2025.CALE_EF`, `mdl_CheckTVW.FileExists` | `ValidateSignatureANAF` |
| 675 · `ParseXML` · 261 ·  | Public | `UNIT`, `EFT`, `EF_TMP`, `EFT_C`, `EFS` | `EFT_C`, `EF`, `EFT`, `EF_TMP`, `EFS` |  | `mdl_2025.CALE_EF`, `mdl_GLOBALS.init_globals`, `mdl_CheckTVW.FileExists` | `StrToBytes` |
| 1060 · `IncarcaFacturaXML` · 80 ·  | Public | `Factura`, `UNIT`, `Scheme` | `Factura` |  | `mdl_2025.CALE_EF`, `mdl_CheckTVW.FileExists`, `mdl_EFactura_Add.GENEREAZA_XML_EFACTURA`, `mdl_2025.Token_Key` |  |
| 1140 · `StatusFactura` · 54 ·  | Public | `CALE`, `Factura` | `Factura` |  | `mdl_2025.Token_Key` | `stareMesaj` |
| 1194 · `ListaMesaje` · 112 ·  | Public | `CALE`, `UNIT` |  |  | `mdl_2025.Token_Key`, `mdl_JSON.ParseJson` | `InformatiiFirmaOnline` |
| 1306 · `DescarcaMesaje` · 47 ·  | Public |  |  |  | `mdl_2025.CALE_EF`, `mdl_2025.Token_Key` |  |
| 1353 · `StareMesaj` · 55 ·  | Public | `CALE`, `Factura` | `Factura` |  | `mdl_2025.Token_Key` |  |
| 1408 · `InformatiiFirmaOnline` · 103 · ======== descarcare informatii pentru CUI din ANAF =========== | Public | `CONFIGS` |  | `Mesaj_ANAF` | `mdl_JSON.ParseJson` |  |
| 1511 · `InformatiiFirmaOnline2` · 44 ·  | Public | `CONFIGS` |  |  | `mdl_JSON.ParseJson` |  |
| 1555 · `StrToBytes` · 20 · ============ Functii ajutatoare ===========<br>1575 · `EncodeBase64` · 29 ·  | Private |  |  |  |  |  |
| 1660 · `ParseXML_ERR` · 34 ·  | Public | `EF_ERR` | `EF_ERR` |  |  |  |
| 1694 · `TrimiteMesajFactura` · 71 ·  | Public | `CALE`, `UNIT`, `EFT_M`, `EFT` | `EFT_M` |  | `mdl_2025.Token_Key` | `GenereazaXMLMesaj` |
| 1788 · `ParseXML_Manual` · 297 ·  | Public | `tmpXMLPath`, `Parteneri`, `EF_TMP`, `UNIT`, `CALE` | `EF_TMP`, `tmpXMLPath`, `UNIT` | `XMLPath`, `EFACTURA_2025`, `EFACTURA_TMP` | `mdl_MsBox.MsBox`, `mdl_JSON.ParseJson`, `mdl_CheckTVW.FileExists` | `InformatiiFirmaOnline` |
| 2085 · `isAnafOK` · 54 ·  | Public | `UNIT`, `CONFIGS` |  |  | `mdl_2025.Token_Key` |  |
| 2139 · `VerificaAnaf` · 105 ·  | Public | `CALE`, `UNIT`, `IMG` | `UNIT` |  | `mdl_Functii2023.isLoaded`, `mdl_GLOBALS.init_globals`, `mdl_2025.Token_Key`, `mdl_2025.Token_RefreshKey`, `mdl_2025.Token_Expiry`, `mdl_Functii2024.imgDic` | `isAnafOK` |
| 2244 · `ParseXML_Balanta` · 63 ·  | Public | `Balanta_Forexe` | `Balanta_Forexe` |  | `mdl_GLOBALS.init_globals`, `mdl_CheckTVW.FileExists`, `mdl_Functii2023.Concat_WS` |  |
| 2307 · `ValideazaXML_Local` · 47 ·  | Public |  |  |  | `mdl_MsBox.MsBox` |  |
| 2354 · `ValidateSignatureANAF` · 76 ·  | Public |  |  |  | `mdl_2025.Token_Key` | `ReadTextFile` |
| 2430 · `ReadTextFile` · 111 · ------------------------------------------------------ | Public | `tblFiles` |  |  | `mdl_2026.RefreshAccessToken`, `mdl_2025.Token_RefreshKey`, `mdl_CheckTVW.FileExists`, `mdl_DISK_RW.SaveByteArrayToFile`, `mdl_JSON.ParseJson`, `mdl_2025.Token_NewLocation` |  |
| 2541 · `ValidateSignature` · 76 ·  | Public |  |  |  | `mdl_2025.token_key` | `ReadFileAsByteArray`, `StringToByteArray` |

## module `mdl_EFactura_Add`

Source: `Modules/mdl_EFactura_Add.bas.txt` (14,927 chars, 10 procedures)

| Procedures (Line · name · length · comment) | Scope | Tables/queries | Writes | Opens forms | Calls outside | Local calls |
|---|---|---|---|---|---|---|
| 7 · `GENEREAZA_XML_EFACTURA` · 138 · ==== GENERARE XML EFACTURA ==== | Public | `FacturaC`, `qFacturi_Vanzare`, `BIC`, `Factura`, `Scheme` |  |  | `mdl_Functii2023.Concat_WS`, `mdl_2025.CALE_EF` | `FormatNumber`, `AddAttribute`, `AddElement`, `AddSupplierParty`, `AddCustomerParty`, `AddPaymentMeans`, `AddTaxTotal`, `AddLegalMonetaryTotal`, `AddInvoiceLine` |
| 145 · `AddSupplierParty` · 57 · <br>202 · `AddCustomerParty` · 33 ·  | Public |  |  |  |  | `AddElement` |
| 235 · `AddPaymentMeans` · 15 ·  | Public | `BIC` |  |  |  | `AddElement` |
| 250 · `AddTaxTotal` · 31 · <br>281 · `AddLegalMonetaryTotal` · 19 ·  | Public |  |  |  |  | `AddElement`, `AddAttribute` |
| 300 · `AddInvoiceLine` · 47 ·  | Public |  |  |  |  | `AddElement`, `FormatNumber`, `AddAttribute` |
| 347 · `AddAttribute` · 9 · <br>356 · `AddElement` · 9 · <br>365 · `FormatNumber` · 7 ·  | Private |  |  |  |  |  |

## class `clsAnaf_Async`

Source: `Classes/clsAnaf_Async.cls.txt` (8,519 chars, 11 procedures)

| Procedures (Line · name · length · comment) | Scope | Tables/queries | Writes | Opens forms | Calls outside | Local calls |
|---|---|---|---|---|---|---|
| 50 · `NumarZile` (Property Let) · 10 ·  | Public |  |  |  |  |  |
| 60 · `cHTTP_HttpError` · 5 · <br>65 · `cHTTP_HttpWorking` · 5 · <br>255 · `AsociazaFirmeCuFacturi` · 11 · <br>266 · `Class_Terminate` · 10 · <br>284 · `cHTTP_HttpTimeOut` · 4 ·  | Private |  |  |  |  |  |
| 70 · `Class_Initialize` · 6 ·  | Private | `CALE` |  |  |  |  |
| 76 · `CautaMesajeNoi` · 15 ·  | Public |  |  |  | `mdl_2025.Token_Key` | `MesajeNoi` |
| 91 · `MesajeNoi` · 95 ·  | Private | `CONFIGS` |  |  | `mdl_JSON.ParseJson` | `InfoFirme` |
| 186 · `InfoFirme` · 69 ·  | Private |  |  |  | `mdl_JSON.ParseJson` | `AsociazaFirmeCuFacturi` |
| 276 · `cHTTP_HttpResolved` · 8 ·  | Private |  |  |  |  | `MesajeNoi`, `InfoFirme` |

## class `clsEF_element`

Source: `Classes/clsEF_element.cls.txt` (3,758 chars, 20 procedures)

| Procedures (Line · name · length · comment) | Scope | Tables/queries | Writes | Opens forms | Calls outside | Local calls |
|---|---|---|---|---|---|---|
| 20 · `Detalii` (Property Let) · 4 · Property for Detalii<br>29 · `Id_Solicitare` (Property Let) · 4 · Property for data_creare<br>38 · `data_creare` (Property Let) · 4 · Property for data_creare<br>47 · `id_incarcare` (Property Let) · 4 · Property for id_incarcare<br>56 · `cif_emitent` (Property Let) · 4 · Property for cif_emitent<br>65 · `cif_beneficiar` (Property Let) · 4 · Property for cif_beneficiar<br>69 · `cif_beneficiar` (Property Get) · 5 · <br>74 · `Tip` (Property Let) · 4 · Property for tip<br>78 · `Tip` (Property Get) · 5 · <br>83 · `ID` (Property Let) · 4 · Property for id<br>92 · `Denumire` (Property Let) · 4 · Property for denumire<br>96 · `Denumire` (Property Get) · 5 ·  | Public |  |  |  |  |  |
| 24 · `Detalii` (Property Get) · 5 · <br>33 · `Id_Solicitare` (Property Get) · 5 ·  | Public |  |  |  |  | `data_creare` |
| 42 · `data_creare` (Property Get) · 5 ·  | Public |  |  |  |  | `id_incarcare` |
| 51 · `id_incarcare` (Property Get) · 5 ·  | Public |  |  |  |  | `cif_emitent` |
| 60 · `cif_emitent` (Property Get) · 5 ·  | Public |  |  |  |  | `cif_beneficiar` |
| 87 · `ID` (Property Get) · 5 ·  | Public |  |  |  |  | `denumire` |
| 101 · `Add` · 29 · Method to set property values dynamically | Public |  |  |  |  | `data_creare`, `id_incarcare`, `id_solicitare`, `cif_emitent`, `cif_beneficiar`, `denumire`, `detalii` |
| 130 · `item` · 16 ·  | Public |  |  |  |  | `data_creare`, `id_incarcare`, `cif_emitent`, `cif_beneficiar`, `denumire` |

## Call tree starting from the UI entry points

Which module procedure is reached from which form handler (one level), taken from the per-form pages:

| Form | Handler | Bound to | Calls |
|---|---|---|---|
| `EFACTURA_2025` | FN_Click | FN.OnClick | `mdl_2025.Token_Key`, `mdl_2025.Token_Expiry`, `mdl_EFactura.CautaFacturiNoi` |
| `EFACTURA_2025` | bSAV_Click | bSAV.OnClick | `mdl_Salvare_NoteContabile.SaveEFactura` |
| `EFACTURA_ADD` | TrimiteFactura |  | `mdl_EFactura_Add.GENEREAZA_XML_EFACTURA`, `mdl_EFactura.ValideazaXML_Local`, `mdl_EFactura.IncarcaFacturaXML`, `mdl_EFactura.StatusFactura` |
| `EFACTURA_ADD` | Form_Load | Form.OnLoad | `mdl_EFactura.VerificaAnaf` |
| `EFACTURA_CLIENTI` | CodFiscal_AfterUpdate | CodFiscal.AfterUpdate | `mdl_EFactura.InformatiiFirmaOnline` |
| `EFACTURA_MSG` | bSend_Click | bSend.OnClick | `mdl_EFactura.TrimiteMesajFactura` |
| `EFACTURA_TMP` | bDWN_Click | bDWN.OnClick | `mdl_EFactura.DescarcaFactura`, `mdl_EFactura.entUnZip1File`, `mdl_EFactura.ParseXML` |
| `MF2019_Asoc_EF` | bFact_Click | bFact.OnClick | `mdl_EFactura.DescarcaFacturaPDF` |
| `Note_EFactura` | bFact_Click | bFact.OnClick | `mdl_EFactura.DescarcaFacturaPDF` |