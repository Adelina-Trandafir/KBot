# Forms in scope

Form details are in `forms/`; the three PDF viewers share one page with per-form records. Captions and bindings summarize the source; read each form page for its controls and handlers. Entry-point rows group only identical Object/Opens or identical form-control targets; `#: name` preserves each original row position.

| Form | Caption | Bound to | Subforms | Opens | Controls | Procs |
|---|---|---|---|---|---|---|
| [`EFACTURA_2025`](forms/EFACTURA_2025.md) |  | unbound |  | `EFACTURA_AA`, `Note_PickClsf` | 43 | 41 |
| [`EFACTURA_AA`](forms/EFACTURA_AA.md) | Transfer E-Facturi din anul anterior | unbound |  |  | 4 | 7 |
| [`EFACTURA_ADD`](forms/EFACTURA_ADD.md) | Facturi vânzare prin sistemul E-Factură | unbound | `EFACTURA_ADD_FACTURI`, `EFACTURA_CLIENTI`, `EFACTURA_VANZATOR`, `EFACTURA_ADD_SUB` | `EFACTURA_CLIENTI`, `Setari implicite` | 40 | 17 |
| [`EFACTURA_ADD_FACTURI`](forms/EFACTURA_ADD_FACTURI.md) |  | `Factura`, `ClientiEF` (SQL) |  | `EFACTURA_ADD` | 7 | 1 |
| [`EFACTURA_ADD_SUB`](forms/EFACTURA_ADD_SUB.md) |  | `tmpFacturaC` (SQL) |  | `EFACTURA_UM` | 21 | 9 |
| [`EFACTURA_CLIENTI`](forms/EFACTURA_CLIENTI.md) | Modifică / Adaugă Clienți | unbound |  |  | 27 | 13 |
| [`EFACTURA_MSG`](forms/EFACTURA_MSG.md) | Mesaje Factură | unbound |  |  | 8 | 4 |
| [`EFACTURA_PDF`](forms/EFACTURA_PDF.md) |  | `EF` (SQL) |  |  | 1 | 2 |
| [`EFACTURA_PickIBAN`](forms/EFACTURA_PickIBAN.md) | PickIBAN | unbound |  | `EFACTURA_ADD` | 1 | 6 |
| [`EFACTURA_TMP`](forms/EFACTURA_TMP.md) | FACTURI PRIMITE | unbound |  |  | 4 | 8 |
| [`EFACTURA_UM`](forms/EFACTURA_UM.md) | FACTURI PRIMITE | `TC`, `EF_UM` (SQL) |  | `EFACTURA_ADD` | 9 | 3 |
| [`EFACTURA_VANZATOR`](forms/EFACTURA_VANZATOR.md) |  | unbound |  |  | 19 | 4 |
| [`EF_F`](forms/EF_F.md) | tmpDoc2017 | `tmpFacturi` |  | `EFACTURA_2025` | 21 | 14 |
| [`EF_F_inlucru`](forms/EF_F_inlucru.md) | tmpDoc2017 | `EF_F` |  | `EFACTURA_2025` | 21 | 14 |
| [`EF_P`](forms/EF_P.md) | tmpDoc2017 subform | `tmpDoc2017P` |  | `EFACTURA_2025` | 15 | 9 |
| [`MF2019_Asoc_EF`](forms/MF2019_Asoc_EF.md) |  | `tmpNote_EFactura` |  | `MF_PDF_EF`, `MF2019`, `Note`, `_MF2019_Asoc_EF` | 25 | 5 |
| [`MF_PDF_EF`](forms/EFACTURA_PDF.md#mf-pdf-ef) |  | `EF` (SQL) |  |  | 1 | 2 |
| [`Mesaj_ANAF`](forms/Mesaj_ANAF.md) | Mesaj ANAF | unbound |  |  | 1 | 0 |
| [`Note_EFactura`](forms/Note_EFactura.md) |  | `tmpNote_EFactura` |  | `Note_PDF_EF`, `Note` | 23 | 4 |
| [`Note_PDF_EF`](forms/EFACTURA_PDF.md#note-pdf-ef) |  | `EF` (SQL) |  |  | 1 | 2 |
| [`_MF2019_Asoc_EF`](forms/_MF2019_Asoc_EF.md) | Asociere E-Factură pentru `Part` | unbound |  | `MF2019` | 17 | 4 |

## Forms outside the scope that these forms open

| Form | Opened by | Bound to |
|---|---|---|
| `Balanta` | `query:BalantaSelectFP`, `query:Q_1`, `query:Q_13`, `query:Q_2`, `query:Q_8` |  |
| `MF2019` | `query:qMF_EFactura_Initial`, `query:qMF_EFactura_Ramas`, `query:qMF_EFactura_Toate`, `form:MF2019_Asoc_EF`, `form:_MF2019_Asoc_EF` |  |
| `Note` | `query:qNote_EFactura_Toate`, `query:_qNote_EFacturat_Ramas`, `query:_qNote_EFactura_Asociat_Nou`, `query:_qNote_EFactura_Initial`, `form:MF2019_Asoc_EF`, `form:Note_EFactura` |  |
| `Note_PickClsf` | `form:EFACTURA_2025` |  |
| `Setari implicite` | `form:EFACTURA_ADD` |  |
| `TrecereAN` | `query:QEF_TRECERE` |  |
| `XMLPath` |  |  |
## Navigation: who opens whom

## Form graph

Solid arrow = opens (DoCmd.OpenForm / Forms! reference in code or query); thick = hosts as subform.

The catalog columns **Opens** and **Subforms** above contain these same 22 edges; the former graph used solid arrows for opens/references and thick arrows for hosted subforms.

## Entry points: code outside the scope that opens these forms

| Object | Procedures (#: name) | Opens |
|---|---|---|
| module:basRibbonCallbacks | 1: OnActionButton | `EFACTURA_2025`, `EFACTURA_ADD` |
| module:mdl_Popup2022 | 2: Popup_Efactura_2022<br>3: EF_OP<br>7: Popup_EFactura_IncarcaXML<br>8: EF_ZILE<br>10: Popup_EFactura_Raport | `EFACTURA_2025` |
| module:mdl_Popup2022 | 4: Popup_EFactura_Optiuni<br>5: PP_EF_OPT<br>6: OPT_EF<br>9: EFV | `EFACTURA_ADD` |
| module:mdl_Popup2022 | 11: PP_EF_Raport | `EFACTURA_2025`, `EFACTURA_MSG` |
| module:mdl_Salvare_NoteContabile | 12: SaveEFactura<br>13: SaveBurse | `EFACTURA_2025` |
| form:MF2019_1 | 14: EF_MouseUp | `MF2019_Asoc_EF` |
| form:Note | 15: Form_Load | `EFACTURA_2025` |

## Entry points: queries that read these forms (parameters)

| Queries (#: name) | Reads form controls of |
|---|---|
| 1: QEF_EV<br>2: QEF_PL<br>3: QEF_Plata<br>4: QEF_RestDePlata<br>5: QEF_RestDePlata_Data<br>6: SAV_ACTIUNE_EV<br>7: SAV_ACTIUNE_PL<br>8: SAV_Cont8067_EV<br>9: SAV_Cont8067_PL<br>10: SAV_Credit_EV<br>11: SAV_Credit_PL<br>12: SAV_Debit_EV<br>13: SAV_Debit_PL<br>14: SAV_Document_EV<br>15: SAV_OP<br>16: SAV_OPER_EV<br>17: SAV_OPER_PL<br>18: SAV_PlatiFacturi | `EFACTURA_2025` |
## Form x table matrix

`R` = read, `W` = write, lowercase `r`/`w` = through a query that the form (or its code) runs. Combo/list sources count as `R`. Empty source cells are omitted below; — means no marker in any column. Compound markers (for example rwwW) are preserved literally, including repeated letters.

Source column order: `BIC`, `Balanta_Forexe`, `CALE`, `CONFIGS`, `Clasificatii`, `ClientiEF`, `Cont8067`, `Contracte`, `DefaNote`, `DefaNoteA`, `DefaSub`, `DefaTitlu2`, `Documente`, `EF`, `EFS`, `EFT`, `EFT_C`, `EFT_M`, `EFT_O`, `EF_ERR`, `EF_F`, `EF_TMP`, `EF_UM`, `Factura`, `FacturaC`, `IMG`, `Jud`, `Oper`, `Parteneri`, `ParteneriAng`, `PlatiFacturi`, `Scheme`, `UNIT`, `_TempCredit`, `tblFiles`, `tblMSG`, `tmpConturiIBAN`, `tmpDoc2017P`, `tmpEF_TRANS`, `tmpFacturaC`, `tmpFacturi`, `tmpMF_EFactura`, `tmpNote_EFactura`, `tmpQEF`, `tmpXMLPath`.

| Object | Table: original marker |
|---|---|
| EFACTURA_2025 | `CALE`: r; `Clasificatii`: R; `Cont8067`: r; `Contracte`: R; `DefaNote`: R; `DefaNoteA`: R; `Documente`: r; `EFS`: W; `EFT`: rW; `EFT_O`: r; `EF_F`: R; `Oper`: r; `Parteneri`: r; `ParteneriAng`: R; `PlatiFacturi`: r; `UNIT`: r; `tblMSG`: R; `tmpDoc2017P`: rwW; `tmpFacturi`: rwwW; `tmpQEF`: RwwwW |
| EFACTURA_AA | `CALE`: r; `EF`: W; `EFS`: W; `EFT`: rW; `EFT_C`: W; `EFT_O`: r; `Oper`: r; `tmpEF_TRANS`: W |
| EFACTURA_ADD | `Clasificatii`: W; `Factura`: W; `FacturaC`: W; `Scheme`: R; `tblMSG`: R; `tmpConturiIBAN`: W; `tmpFacturaC`: W |
| EFACTURA_ADD_FACTURI | `ClientiEF`: R; `Factura`: R; `FacturaC`: R; `tmpFacturaC`: W |
| EFACTURA_ADD_SUB | `EF_UM`: R; `tmpFacturaC`: W |
| EFACTURA_CLIENTI | `BIC`: R; `ClientiEF`: W; `Jud`: R |
| EFACTURA_MSG | `EFT`: R; `EFT_M`: R; `tmpQEF`: R |
| EFACTURA_PDF | `EF`: R |
| EFACTURA_PickIBAN | `DefaSub`: R; `DefaTitlu2`: R; `tmpConturiIBAN`: R |
| EFACTURA_TMP | `EF`: W; `EF_TMP`: W; `UNIT`: W |
| EFACTURA_UM | `EF_UM`: R |
| EFACTURA_VANZATOR | `Jud`: R; `UNIT`: W |
| EF_F | `Cont8067`: W; `Documente`: R; `Oper`: W; `tmpFacturi`: R |
| EF_F_inlucru | `Cont8067`: W; `Documente`: R; `EF_F`: R; `Oper`: W; `tmpFacturi`: R |
| EF_P | `Documente`: R; `tmpDoc2017P`: R; `tmpFacturi`: R |
| MF2019_Asoc_EF | `EFT`: rW; `EFT_O`: rW; `IMG`: R; `Oper`: W; `Parteneri`: r; `tmpMF_EFactura`: rwW; `tmpNote_EFactura`: R |
| MF_PDF_EF | `EF`: R |
| Mesaj_ANAF | — |
| Note_EFactura | `EFT`: r; `EFT_O`: r; `IMG`: R; `Parteneri`: r; `_TempCredit`: R; `tmpMF_EFactura`: W; `tmpNote_EFactura`: RwW |
| Note_PDF_EF | `EF`: R |
| _MF2019_Asoc_EF | `EFT`: rW; `EFT_O`: rW; `IMG`: R; `Oper`: W; `Parteneri`: r; `_TempCredit`: R; `tmpMF_EFactura`: rwW |
| mdl_EFactura (code) | `Balanta_Forexe`: W; `CALE`: R; `CONFIGS`: R; `EF`: W; `EFS`: W; `EFT`: W; `EFT_C`: W; `EFT_M`: W; `EF_ERR`: W; `EF_TMP`: W; `Factura`: W; `IMG`: R; `Parteneri`: R; `Scheme`: R; `UNIT`: W; `tblFiles`: R; `tmpXMLPath`: W |
| mdl_EFactura_Add (code) | `BIC`: R; `Factura`: R; `FacturaC`: R; `Scheme`: R |
| clsEF_element (code) | — |
| clsAnaf_Async (code) | `CALE`: R; `CONFIGS`: R |
