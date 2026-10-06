# Code in scope

Procedures of the standard modules/classes that are specific to e-invoice, with what each touches. Line numbers are inside the exported `.txt` file.

## module `mdl_EFactura`

Source: `Modules/mdl_EFactura.bas.txt` (96,535 chars, 33 procedures)

| Line | Procedure | Scope | Length | Comment above | Tables/queries | Writes | Opens forms | Calls outside | Local calls |
|---|---|---|---|---|---|---|---|---|---|
| 28 | `CautaFacturiNoi` | Public | 207 | ====== preluare facturi din sistemul E-Factura ========= | `CALE`, `EF_TMP`, `Scheme`, `UNIT` | `EF_TMP`, `UNIT`, `EF` | `EFACTURA_2025`, `Mesaj_ANAF`, `EFACTURA_TMP` | `mdl_2025.CALE_EF`, `mdl_Functii2023.isLoaded`, `mdl_2025.Token_Key`, `mdl_JSON.ParseJson`, `mdl_MsBox.MsBox` | `InformatiiFirmaOnline`, `DescarcaFacturi` |
| 235 | `DescarcaFacturi` | Private | 99 |  | `EF_TMP`, `UNIT` | `EF`, `EF_TMP`, `UNIT` |  | `mdl_2025.CALE_EF` | `DescarcaFactura`, `entUnZip1File`, `ParseXML` |
| 334 | `DescarcaFactura` | Public | 64 |  |  |  |  | `mdl_2025.CALE_EF`, `mdl_CheckTVW.FileExists`, `mdl_2025.Token_Key` |  |
| 398 | `RestabilesteXML` | Public | 49 |  |  |  |  | `mdl_2025.CALE_EF`, `mdl_CheckTVW.FileExists` | `entUnZip1File` |
| 447 | `DescarcaFacturaPDF` | Public | 54 |  | `EFT` |  |  | `mdl_2025.CALE_EF`, `mdl_CheckTVW.FileExists`, `mdl_JSON.ParseJson` | `RestabilesteXML` |
| 501 | `EncodeStringToBytes` | Public | 16 | Helper function to encode a string to a byte array with specified charset |  |  |  |  |  |
| 517 | `entUnZip1File` | Public | 70 |  |  |  |  | `mdl_Functii_Generale.Folder`, `mdl_CheckTVW.FileExists`, `mdl_Functii_Generale.folder` |  |
| 587 | `PrelucreazaZipLocal` | Public | 88 |  | `EF_TMP` | `EF_TMP`, `EF` | `EFACTURA_TMP` | `mdl_2025.CALE_EF`, `mdl_CheckTVW.FileExists` | `ValidateSignatureANAF` |
| 675 | `ParseXML` | Public | 261 |  | `UNIT`, `EFT`, `EF_TMP`, `EFT_C`, `EFS` | `EFT_C`, `EF`, `EFT`, `EF_TMP`, `EFS` |  | `mdl_2025.CALE_EF`, `mdl_GLOBALS.init_globals`, `mdl_CheckTVW.FileExists` | `StrToBytes` |
| 936 | `Export_XML_TO_PDF` | Public | 124 |  |  |  |  |  |  |
| 1060 | `IncarcaFacturaXML` | Public | 80 |  | `Factura`, `UNIT`, `Scheme` | `Factura` |  | `mdl_2025.CALE_EF`, `mdl_CheckTVW.FileExists`, `mdl_EFactura_Add.GENEREAZA_XML_EFACTURA`, `mdl_2025.Token_Key` |  |
| 1140 | `StatusFactura` | Public | 54 |  | `CALE`, `Factura` | `Factura` |  | `mdl_2025.Token_Key` | `stareMesaj` |
| 1194 | `ListaMesaje` | Public | 112 |  | `CALE`, `UNIT` |  |  | `mdl_2025.Token_Key`, `mdl_JSON.ParseJson` | `InformatiiFirmaOnline` |
| 1306 | `DescarcaMesaje` | Public | 47 |  |  |  |  | `mdl_2025.CALE_EF`, `mdl_2025.Token_Key` |  |
| 1353 | `StareMesaj` | Public | 55 |  | `CALE`, `Factura` | `Factura` |  | `mdl_2025.Token_Key` |  |
| 1408 | `InformatiiFirmaOnline` | Public | 103 | ======== descarcare informatii pentru CUI din ANAF =========== | `CONFIGS` |  | `Mesaj_ANAF` | `mdl_JSON.ParseJson` |  |
| 1511 | `InformatiiFirmaOnline2` | Public | 44 |  | `CONFIGS` |  |  | `mdl_JSON.ParseJson` |  |
| 1555 | `StrToBytes` | Private | 20 | ============ Functii ajutatoare =========== |  |  |  |  |  |
| 1575 | `EncodeBase64` | Private | 29 |  |  |  |  |  |  |
| 1604 | `PrintXMLStructure` | Public | 56 |  |  |  |  |  |  |
| 1660 | `ParseXML_ERR` | Public | 34 |  | `EF_ERR` | `EF_ERR` |  |  |  |
| 1694 | `TrimiteMesajFactura` | Public | 71 |  | `CALE`, `UNIT`, `EFT_M`, `EFT` | `EFT_M` |  | `mdl_2025.Token_Key` | `GenereazaXMLMesaj` |
| 1765 | `GenereazaXMLMesaj` | Public | 23 |  |  |  |  |  |  |
| 1788 | `ParseXML_Manual` | Public | 297 |  | `tmpXMLPath`, `Parteneri`, `EF_TMP`, `UNIT`, `CALE` | `EF_TMP`, `tmpXMLPath`, `UNIT` | `XMLPath`, `EFACTURA_2025`, `EFACTURA_TMP` | `mdl_MsBox.MsBox`, `mdl_JSON.ParseJson`, `mdl_CheckTVW.FileExists` | `InformatiiFirmaOnline` |
| 2085 | `isAnafOK` | Public | 54 |  | `UNIT`, `CONFIGS` |  |  | `mdl_2025.Token_Key` |  |
| 2139 | `VerificaAnaf` | Public | 105 |  | `CALE`, `UNIT`, `IMG` | `UNIT` |  | `mdl_Functii2023.isLoaded`, `mdl_GLOBALS.init_globals`, `mdl_2025.Token_Key`, `mdl_2025.Token_RefreshKey`, `mdl_2025.Token_Expiry`, `mdl_Functii2024.imgDic` | `isAnafOK` |
| 2244 | `ParseXML_Balanta` | Public | 63 |  | `Balanta_Forexe` | `Balanta_Forexe` |  | `mdl_GLOBALS.init_globals`, `mdl_CheckTVW.FileExists`, `mdl_Functii2023.Concat_WS` |  |
| 2307 | `ValideazaXML_Local` | Public | 47 |  |  |  |  | `mdl_MsBox.MsBox` |  |
| 2354 | `ValidateSignatureANAF` | Public | 76 |  |  |  |  | `mdl_2025.Token_Key` | `ReadTextFile` |
| 2430 | `ReadTextFile` | Public | 111 | ------------------------------------------------------ | `tblFiles` |  |  | `mdl_2026.RefreshAccessToken`, `mdl_2025.Token_RefreshKey`, `mdl_CheckTVW.FileExists`, `mdl_DISK_RW.SaveByteArrayToFile`, `mdl_JSON.ParseJson`, `mdl_2025.Token_NewLocation` |  |
| 2541 | `ValidateSignature` | Public | 76 |  |  |  |  | `mdl_2025.token_key` | `ReadFileAsByteArray`, `StringToByteArray` |
| 2617 | `ReadFileAsByteArray` | Public | 11 | Func?ie pentru a citi un fi?ier ca matrice de octe?i (byte array) |  |  |  |  |  |
| 2628 | `StringToByteArray` | Public | 3 | Func?ie pentru a converti un string �ntr-o matrice de octe?i (folosind conversia ANSI) |  |  |  |  |  |

## module `mdl_EFactura_Add`

Source: `Modules/mdl_EFactura_Add.bas.txt` (14,927 chars, 10 procedures)

| Line | Procedure | Scope | Length | Comment above | Tables/queries | Writes | Opens forms | Calls outside | Local calls |
|---|---|---|---|---|---|---|---|---|---|
| 7 | `GENEREAZA_XML_EFACTURA` | Public | 138 | ==== GENERARE XML EFACTURA ==== | `FacturaC`, `qFacturi_Vanzare`, `BIC`, `Factura`, `Scheme` |  |  | `mdl_Functii2023.Concat_WS`, `mdl_2025.CALE_EF` | `FormatNumber`, `AddAttribute`, `AddElement`, `AddSupplierParty`, `AddCustomerParty`, `AddPaymentMeans`, `AddTaxTotal`, `AddLegalMonetaryTotal`, `AddInvoiceLine` |
| 145 | `AddSupplierParty` | Public | 57 |  |  |  |  |  | `AddElement` |
| 202 | `AddCustomerParty` | Public | 33 |  |  |  |  |  | `AddElement` |
| 235 | `AddPaymentMeans` | Public | 15 |  | `BIC` |  |  |  | `AddElement` |
| 250 | `AddTaxTotal` | Public | 31 |  |  |  |  |  | `AddElement`, `AddAttribute` |
| 281 | `AddLegalMonetaryTotal` | Public | 19 |  |  |  |  |  | `AddElement`, `AddAttribute` |
| 300 | `AddInvoiceLine` | Public | 47 |  |  |  |  |  | `AddElement`, `FormatNumber`, `AddAttribute` |
| 347 | `AddAttribute` | Private | 9 |  |  |  |  |  |  |
| 356 | `AddElement` | Private | 9 |  |  |  |  |  |  |
| 365 | `FormatNumber` | Private | 7 |  |  |  |  |  |  |

## class `clsAnaf_Async`

Source: `Classes/clsAnaf_Async.cls.txt` (8,519 chars, 11 procedures)

| Line | Procedure | Scope | Length | Comment above | Tables/queries | Writes | Opens forms | Calls outside | Local calls |
|---|---|---|---|---|---|---|---|---|---|
| 50 | `NumarZile` (Property Let) | Public | 10 |  |  |  |  |  |  |
| 60 | `cHTTP_HttpError` | Private | 5 |  |  |  |  |  |  |
| 65 | `cHTTP_HttpWorking` | Private | 5 |  |  |  |  |  |  |
| 70 | `Class_Initialize` | Private | 6 |  | `CALE` |  |  |  |  |
| 76 | `CautaMesajeNoi` | Public | 15 |  |  |  |  | `mdl_2025.Token_Key` | `MesajeNoi` |
| 91 | `MesajeNoi` | Private | 95 |  | `CONFIGS` |  |  | `mdl_JSON.ParseJson` | `InfoFirme` |
| 186 | `InfoFirme` | Private | 69 |  |  |  |  | `mdl_JSON.ParseJson` | `AsociazaFirmeCuFacturi` |
| 255 | `AsociazaFirmeCuFacturi` | Private | 11 |  |  |  |  |  |  |
| 266 | `Class_Terminate` | Private | 10 |  |  |  |  |  |  |
| 276 | `cHTTP_HttpResolved` | Private | 8 |  |  |  |  |  | `MesajeNoi`, `InfoFirme` |
| 284 | `cHTTP_HttpTimeOut` | Private | 4 |  |  |  |  |  |  |

## class `clsEF_element`

Source: `Classes/clsEF_element.cls.txt` (3,758 chars, 20 procedures)

| Line | Procedure | Scope | Length | Comment above | Tables/queries | Writes | Opens forms | Calls outside | Local calls |
|---|---|---|---|---|---|---|---|---|---|
| 20 | `Detalii` (Property Let) | Public | 4 | Property for Detalii |  |  |  |  |  |
| 24 | `Detalii` (Property Get) | Public | 5 |  |  |  |  |  | `data_creare` |
| 29 | `Id_Solicitare` (Property Let) | Public | 4 | Property for data_creare |  |  |  |  |  |
| 33 | `Id_Solicitare` (Property Get) | Public | 5 |  |  |  |  |  | `data_creare` |
| 38 | `data_creare` (Property Let) | Public | 4 | Property for data_creare |  |  |  |  |  |
| 42 | `data_creare` (Property Get) | Public | 5 |  |  |  |  |  | `id_incarcare` |
| 47 | `id_incarcare` (Property Let) | Public | 4 | Property for id_incarcare |  |  |  |  |  |
| 51 | `id_incarcare` (Property Get) | Public | 5 |  |  |  |  |  | `cif_emitent` |
| 56 | `cif_emitent` (Property Let) | Public | 4 | Property for cif_emitent |  |  |  |  |  |
| 60 | `cif_emitent` (Property Get) | Public | 5 |  |  |  |  |  | `cif_beneficiar` |
| 65 | `cif_beneficiar` (Property Let) | Public | 4 | Property for cif_beneficiar |  |  |  |  |  |
| 69 | `cif_beneficiar` (Property Get) | Public | 5 |  |  |  |  |  |  |
| 74 | `Tip` (Property Let) | Public | 4 | Property for tip |  |  |  |  |  |
| 78 | `Tip` (Property Get) | Public | 5 |  |  |  |  |  |  |
| 83 | `ID` (Property Let) | Public | 4 | Property for id |  |  |  |  |  |
| 87 | `ID` (Property Get) | Public | 5 |  |  |  |  |  | `denumire` |
| 92 | `Denumire` (Property Let) | Public | 4 | Property for denumire |  |  |  |  |  |
| 96 | `Denumire` (Property Get) | Public | 5 |  |  |  |  |  |  |
| 101 | `Add` | Public | 29 | Method to set property values dynamically |  |  |  |  | `data_creare`, `id_incarcare`, `id_solicitare`, `cif_emitent`, `cif_beneficiar`, `denumire`, `detalii` |
| 130 | `item` | Public | 16 |  |  |  |  |  | `data_creare`, `id_incarcare`, `cif_emitent`, `cif_beneficiar`, `denumire` |

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