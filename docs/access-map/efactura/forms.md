# Forms in scope

One page per form in `forms/`. Role column is derived from the caption and the data it binds to; read the per-form page for the details.

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
| [`MF_PDF_EF`](forms/MF_PDF_EF.md) |  | `EF` (SQL) |  |  | 1 | 2 |
| [`Mesaj_ANAF`](forms/Mesaj_ANAF.md) | Mesaj ANAF | unbound |  |  | 1 | 0 |
| [`Note_EFactura`](forms/Note_EFactura.md) |  | `tmpNote_EFactura` |  | `Note_PDF_EF`, `Note` | 23 | 4 |
| [`Note_PDF_EF`](forms/Note_PDF_EF.md) |  | `EF` (SQL) |  |  | 1 | 2 |
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