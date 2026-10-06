# Form x table matrix

`R` = read, `W` = write, lowercase `r`/`w` = through a query that the form (or its code) runs. Combo/list sources count as `R`.

| Object | BIC | Balanta_Forexe | CALE | CONFIGS | Clasificatii | ClientiEF | Cont8067 | Contracte | DefaNote | DefaNoteA | DefaSub | DefaTitlu2 | Documente | EF | EFS | EFT | EFT_C | EFT_M | EFT_O | EF_ERR | EF_F | EF_TMP | EF_UM | Factura | FacturaC | IMG | Jud | Oper | Parteneri | ParteneriAng | PlatiFacturi | Scheme | UNIT | _TempCredit | tblFiles | tblMSG | tmpConturiIBAN | tmpDoc2017P | tmpEF_TRANS | tmpFacturaC | tmpFacturi | tmpMF_EFactura | tmpNote_EFactura | tmpQEF | tmpXMLPath |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| EFACTURA_2025 |  |  | r |  | R |  | r | R | R | R |  |  | r |  | W | rW |  |  | r |  | R |  |  |  |  |  |  | r | r | R | r |  | r |  |  | R |  | rwW |  |  | rwwW |  |  | RwwwW |  |
| EFACTURA_AA |  |  | r |  |  |  |  |  |  |  |  |  |  | W | W | rW | W |  | r |  |  |  |  |  |  |  |  | r |  |  |  |  |  |  |  |  |  |  | W |  |  |  |  |  |  |
| EFACTURA_ADD |  |  |  |  | W |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  | W | W |  |  |  |  |  |  | R |  |  |  | R | W |  |  | W |  |  |  |  |  |
| EFACTURA_ADD_FACTURI |  |  |  |  |  | R |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  | R | R |  |  |  |  |  |  |  |  |  |  |  |  |  |  | W |  |  |  |  |  |
| EFACTURA_ADD_SUB |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  | R |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  | W |  |  |  |  |  |
| EFACTURA_CLIENTI | R |  |  |  |  | W |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  | R |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
| EFACTURA_MSG |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  | R |  | R |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  | R |  |
| EFACTURA_PDF |  |  |  |  |  |  |  |  |  |  |  |  |  | R |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
| EFACTURA_PickIBAN |  |  |  |  |  |  |  |  |  |  | R | R |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  | R |  |  |  |  |  |  |  |  |
| EFACTURA_TMP |  |  |  |  |  |  |  |  |  |  |  |  |  | W |  |  |  |  |  |  |  | W |  |  |  |  |  |  |  |  |  |  | W |  |  |  |  |  |  |  |  |  |  |  |  |
| EFACTURA_UM |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  | R |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
| EFACTURA_VANZATOR |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  | R |  |  |  |  |  | W |  |  |  |  |  |  |  |  |  |  |  |  |
| EF_F |  |  |  |  |  |  | W |  |  |  |  |  | R |  |  |  |  |  |  |  |  |  |  |  |  |  |  | W |  |  |  |  |  |  |  |  |  |  |  |  | R |  |  |  |  |
| EF_F_inlucru |  |  |  |  |  |  | W |  |  |  |  |  | R |  |  |  |  |  |  |  | R |  |  |  |  |  |  | W |  |  |  |  |  |  |  |  |  |  |  |  | R |  |  |  |  |
| EF_P |  |  |  |  |  |  |  |  |  |  |  |  | R |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  | R |  |  | R |  |  |  |  |
| MF2019_Asoc_EF |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  | rW |  |  | rW |  |  |  |  |  |  | R |  | W | r |  |  |  |  |  |  |  |  |  |  |  |  | rwW | R |  |  |
| MF_PDF_EF |  |  |  |  |  |  |  |  |  |  |  |  |  | R |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
| Mesaj_ANAF |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
| Note_EFactura |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  | r |  |  | r |  |  |  |  |  |  | R |  |  | r |  |  |  |  | R |  |  |  |  |  |  |  | W | RwW |  |  |
| Note_PDF_EF |  |  |  |  |  |  |  |  |  |  |  |  |  | R |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
| _MF2019_Asoc_EF |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  | rW |  |  | rW |  |  |  |  |  |  | R |  | W | r |  |  |  |  | R |  |  |  |  |  |  |  | rwW |  |  |  |
| mdl_EFactura (code) |  | W | R | R |  |  |  |  |  |  |  |  |  | W | W | W | W | W |  | W |  | W |  | W |  | R |  |  | R |  |  | R | W |  | R |  |  |  |  |  |  |  |  |  | W |
| mdl_EFactura_Add (code) | R |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  | R | R |  |  |  |  |  |  | R |  |  |  |  |  |  |  |  |  |  |  |  |  |
| clsEF_element (code) |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
| clsAnaf_Async (code) |  |  | R | R |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |