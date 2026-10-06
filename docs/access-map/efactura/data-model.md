# Data model

Tables in scope. "Linked" means the table lives in another .accdb and Avacont.accdb only links to it - the passwords in the connect strings are masked. Relationships come from the Access relationship window (`relationships.md` in the export), plus the join keys used by the queries below.

## Storage

| Database | Tables |
|---|---|
| `C:\AVACONT\ENERGETIC LOCAL\baza2026.accdb` | `ClientiEF`, `Factura`, `FacturaC` |
| `C:\AVACONT\EFACTURA\ef_2026.accdb` | `EF`, `EFS`, `EFT`, `EFT_C`, `EFT_M`, `EFT_O` |
| `Avacont.accdb (local)` | `EF_ERR`, `EF_EXITCODES`, `EF_F`, `EF_TMP`, `EF_UM`, `TEF_F`, `TEF_FF`, `TrecereEFactura`, `tblMSG`, `tmpEF`, `tmpEFS`, `tmpEFT`, `tmpEF_TRANS`, `tmpFacturaC`, `tmpMF_EFactura`, `tmpNoteFacturi`, `tmpNote_EFactura` |

Note: `Surse/RawExport/EF_ACCDB/` is the export of `EF_2025.accdb` (the e-invoice store, owned tables EF, EFS, EFT, EFT_C, EFT_M, EFT_O, Ver and 15 helper queries). Avacont.accdb sees the same tables through links to `ef_2026.accdb`.

## Relationships inside the e-invoice store

- **EFT__IDEFT___EFS__IDEFT**: EFT.IDEFT -> EFS.IDEFT [cascade update, cascade delete]
- **EFT__IDEFT___EFT_C__IDEFT**: EFT.IDEFT -> EFT_C.IDEFT [cascade update, cascade delete]
- **EFT__IDEFT___EFT_M__IDEFT**: EFT.IDEFT -> EFT_M.IDEFT [cascade update, cascade delete]
- **EFT__IDEFT___EFT_O__IDEFT**: EFT.IDEFT -> EFT_O.IDEFT [cascade update, cascade delete]
- **EF__id_sol___EFT__id_sol**: EF.id_sol -> EFT.id_sol [cascade update, cascade delete]

Logical keys the code and queries rely on (no Access relationship defined):

| From | To | Meaning |
|---|---|---|
| EFT.CUI | Parteneri.CodFiscal | supplier/customer match for received invoices (QEF_NOI) |
| EFT_O.IdOperatie | Oper.IdOperatie | accounting operation the invoice was booked on |
| EFT_O.IdClsf | Clasificatii.IDClsf | budget classification of the booked amount |
| EFT.IDEFT_REF | EFT.IDEFT | credit note / correction points to the original invoice |
| EF.cui_unit | UNIT (CUI) | which unit (entity) the downloaded message belongs to |
| Factura.IdClient | ClientiEF.IdClient | customer on an issued invoice |
| FacturaC.IdFactura | Factura.IdFactura | lines of an issued invoice |

## Tables

### `ClientiEF`

Linked table. Source: `ClientiEF`  Connect: `MS Access;PWD=***;DATABASE=C:\AVACONT\ENERGETIC LOCAL\baza2026.accdb`

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | IdClient | AutoNumber | 4 | False |  |  |
| 1 | DenumireClient | Text | 255 | False |  |  |
| 2 | CodFiscal | Text | 255 | False |  |  |
| 3 | Cont | Text | 255 | False |  |  |
| 4 | Banca | Text | 255 | False |  |  |
| 5 | Adresa | Text | 255 | False |  |  |
| 6 | Judetul | Text | 255 | False |  |  |
| 7 | Orasul | Text | 255 | False |  |  |
| 8 | IndFiscal | Text | 255 | False |  |  |
| 9 | Sector | Text | 255 | False |  |  |
| 10 | CNP | Yes/No | 1 | False |  |  |

Indexes: `IdClient: (IdClient)`; `PrimaryKey: (IdClient) PRIMARY UNIQUE`

In scope - written by: `form:EFACTURA_CLIENTI`

In scope - read by: `query:qFacturi_Vanzare`, `form:EFACTURA_ADD_FACTURI`

Also used by 1 object(s) outside the scope: `report:FacturaV`.

### `EF`

Linked table. Source: `EF`  Connect: `MS Access;PWD=***;DATABASE=C:\AVACONT\EFACTURA\ef_2026.accdb`

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | IDEF | AutoNumber | 4 | False |  |  |
| 1 | id_sol | Text | 255 | True |  |  |
| 2 | id | Text | 255 | False |  |  |
| 3 | cif | Text | 255 | False |  |  |
| 4 | data | Date/Time | 8 | False |  |  |
| 5 | cui_unit | Text | 255 | False |  |  |
| 6 | nou | Yes/No | 1 | False | No |  |
| 9 | DTQ | Date/Time | 8 | False | Now() |  |
| 10 | id_solicitare | Text | 255 | False |  |  |
| 11 | nume_fisier | Text | 255 | False |  |  |
| 12 | XML | OLE Object | 0 | False |  |  |

Indexes: `PrimaryKey: (id_sol) PRIMARY UNIQUE`

In scope - written by: nothing found

In scope - read by: nothing found

Also used by 0 object(s) outside the scope.

### `EFS`

Linked table. Source: `EFS`  Connect: `MS Access;PWD=***;DATABASE=C:\AVACONT\EFACTURA\ef_2026.accdb`

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | IDEFS | AutoNumber | 4 | False |  |  |
| 1 | IDEFT | Long | 4 | False | 0 |  |
| 2 | ID | Long | 4 | False | 0 |  |
| 3 | UNIT | Text | 255 | False |  |  |
| 4 | Cant | Double | 8 | False | 0 |  |
| 5 | Valoare | Double | 8 | False | 0 |  |
| 6 | Pret | Double | 8 | False | 0 |  |
| 7 | Denumire | Text | 255 | False |  |  |
| 8 | S | Yes/No | 1 | False | No |  |
| 9 | Rez | Yes/No | 1 | False | No |  |
| 10 | DTQ | Date/Time | 8 | False | Now() |  |
| 11 | Explicatie | Text | 255 | False |  |  |

Indexes: `EFT__IDEFT___EFS__IDEFT: (IDEFT) FOREIGN`; `ID: (ID)`; `IDEFS: (IDEFS)`; `IDEFT: (IDEFT)`; `PrimaryKey: (IDEFS) PRIMARY UNIQUE`

In scope - written by: `form:EFACTURA_2025`, `form:EFACTURA_AA`, `module:mdl_EFactura`

In scope - read by: `query:UEFP`

Also used by 4 object(s) outside the scope: `report:Factura_sub`, `report:NIR`, `module:mdl_Autoexec`, `module:mdl_Popup2022`.

### `EFT`

Linked table. Source: `EFT`  Connect: `MS Access;PWD=***;DATABASE=C:\AVACONT\EFACTURA\ef_2026.accdb`

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | IDEFT | AutoNumber | 4 | False |  |  |
| 1 | id_sol | Text | 255 | False |  |  |
| 2 | NrFact | Text | 255 | False |  |  |
| 3 | DataFact | Date/Time | 8 | False |  |  |
| 4 | DataScad | Date/Time | 8 | False |  |  |
| 5 | CotaTVA | Long | 4 | False | 0 |  |
| 6 | TVA | Double | 8 | False | 0 |  |
| 7 | Valoare | Double | 8 | False | 0 |  |
| 8 | TOTAL | Double | 8 | False | 0 |  |
| 9 | CUI | Text | 255 | False |  |  |
| 10 | DenumireP | Text | 255 | False |  |  |
| 11 | Adresa | Text | 255 | False |  |  |
| 12 | S | Yes/No | 1 | False | No |  |
| 13 | Complet | Yes/No | 1 | False | No |  |
| 14 | Attach | Text | 255 | False |  |  |
| 15 | DTQ | Date/Time | 8 | False | Now() |  |
| 16 | Tip | Text | 255 | False | FC |  |
| 17 | Ref | Text | 255 | False |  |  |
| 18 | IDEFT_REF | Long | 4 | False |  |  |
| 19 | NC | Long | 4 | False |  |  |
| 20 | AA | Yes/No | 1 | False | 0 |  |

Indexes: `EF__id_sol___EFT__id_sol: (id_sol) FOREIGN`; `id_sol: (id_sol)`; `IDF: (IDEFT)`; `PrimaryKey: (IDEFT) PRIMARY UNIQUE`

In scope - written by: `form:EFACTURA_2025`, `form:EFACTURA_AA`, `form:MF2019_Asoc_EF`, `form:_MF2019_Asoc_EF`, `module:mdl_EFactura`

In scope - read by: `query:EFACTURA_AA`, `query:QEF`, `query:QEF_ARH`, `query:QEF_DePlata`, `query:QEF_EV`, `query:QEF_EV_inlucru`, `query:QEF_NOI`, `query:QEF_OPER`, `query:QEF_PL`, `query:QEF_RestDePlata`, `query:QEF_RestDePlata_Data`, `query:QEF_SAV`, `query:QEF_TRECERE`, `query:qMF_EFactura_Initial`, `query:qMF_EFactura_Ramas`, `query:qMF_EFactura_Toate`, `query:qNote_EFactura_Toate`, `query:UEFP`, `query:_qNote_EFacturat_Ramas`, `query:_qNote_EFactura_Asociat_Nou`, `query:_qNote_EFactura_Initial`, `form:EFACTURA_MSG`

Also used by 8 object(s) outside the scope: `query:qParteneri2024`, `form:Note`, `form:TrecereAN`, `report:Factura`, `report:NIR`, `module:mdl_Autoexec`, `module:mdl_Popup2022`, `module:mdl_Salvare_NoteContabile`.

### `EFT_C`

Linked table. Source: `EFT_C`  Connect: `MS Access;PWD=***;DATABASE=C:\AVACONT\EFACTURA\ef_2026.accdb`

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | IDEFTC | AutoNumber | 4 | False |  |  |
| 1 | IDEFT | Long | 4 | False | 0 |  |
| 2 | Note | Memo/Long Text | 0 | False |  |  |
| 3 | DTQ | Date/Time | 8 | False | Now() |  |

Indexes: `EFT__IDEFT___EFT_C__IDEFT: (IDEFT) FOREIGN`; `IDEFT: (IDEFT)`; `IDEFTO: (IDEFTC)`; `PrimaryKey: (IDEFTC) PRIMARY UNIQUE`

In scope - written by: `form:EFACTURA_AA`, `module:mdl_EFactura`

In scope - read by: nothing found

Also used by 1 object(s) outside the scope: `report:FacturaC`.

### `EFT_M`

Linked table. Source: `EFT_M`  Connect: `MS Access;PWD=***;DATABASE=C:\AVACONT\EFACTURA\ef_2026.accdb`

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | IDMSG | AutoNumber | 4 | False |  |  |
| 1 | id_sol | Text | 255 | False |  |  |
| 2 | IDEFT | Long | 4 | False |  |  |
| 3 | ID | Text | 255 | False |  |  |
| 4 | Mesaj | Memo/Long Text | 0 | False |  |  |
| 5 | Data | Date/Time | 8 | False |  |  |
| 6 | DTQ | Date/Time | 8 | False | Now() |  |

Indexes: `EFT__IDEFT___EFT_M__IDEFT: (IDEFT) FOREIGN`; `id_sol: (id_sol)`; `ideft: (IDEFT)`; `PrimaryKey: (IDMSG) PRIMARY UNIQUE`

In scope - written by: `module:mdl_EFactura`

In scope - read by: `form:EFACTURA_MSG`

Also used by 0 object(s) outside the scope.

### `EFT_O`

Linked table. Source: `EFT_O`  Connect: `MS Access;PWD=***;DATABASE=C:\AVACONT\EFACTURA\ef_2026.accdb`

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | IDEFTO | AutoNumber | 4 | False |  |  |
| 1 | IDEFT | Long | 4 | False | 0 |  |
| 2 | IdOperatie | Long | 4 | False | 0 |  |
| 3 | IdClsf | Long | 4 | False | 0 |  |
| 4 | Sursa | Text | 255 | False |  |  |
| 5 | Suma | Double | 8 | False | 0 |  |
| 6 | DTQ | Date/Time | 8 | False | Now() |  |
| 7 | IdUnitate | Long | 4 | False |  |  |

Indexes: `EFT__IDEFT___EFT_O__IDEFT: (IDEFT) FOREIGN`; `IdClsf: (IdClsf)`; `IDEFT: (IDEFT)`; `IDEFTO: (IDEFTO)`; `IdOperatie: (IdOperatie)`; `PrimaryKey: (IDEFTO) PRIMARY UNIQUE`

In scope - written by: `query:SAV_OPER_EF`, `query:SAV_OPER_EF_2017`, `form:MF2019_Asoc_EF`, `form:_MF2019_Asoc_EF`

In scope - read by: `query:EFACTURA_AA`, `query:QEF_ARH`, `query:QEF_EV`, `query:QEF_EV_inlucru`, `query:QEF_NOI`, `query:QEF_PL`, `query:QEF_RestDePlata`, `query:QEF_RestDePlata_Data`, `query:QEF_SAV`, `query:QEF_TRECERE`, `query:qMF_EFactura_Initial`, `query:qMF_EFactura_Ramas`, `query:qMF_EFactura_Toate`, `query:qNote_EFactura_Toate`, `query:_qNote_EFacturat_Ramas`, `query:_qNote_EFactura_Initial`

Also used by 5 object(s) outside the scope: `query:MNotaC1`, `form:MF2019_1`, `form:Note`, `module:mdl_Popup2022`, `module:mdl_Salvare_NoteContabile`.

### `EF_ERR`

Local table in Avacont.accdb.

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | IDEROARE | AutoNumber | 4 | False |  |  |
| 1 | EROARE | Memo/Long Text | 0 | False |  |  |

Indexes: `IDEROARE: (IDEROARE)`; `PrimaryKey: (IDEROARE) PRIMARY UNIQUE`

In scope - written by: `module:mdl_EFactura`

In scope - read by: nothing found

Also used by 2 object(s) outside the scope: `report:EF_ERR`, `module:mdl_Popup2022`.

### `EF_EXITCODES`

Local table in Avacont.accdb.

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | ExitCode | Long | 4 | True |  |  |
| 1 | Expl | Text | 255 | False |  |  |

Indexes: `ExitCode: (ExitCode) PRIMARY UNIQUE`

In scope - written by: nothing found

In scope - read by: nothing found

Also used by 1 object(s) outside the scope: `form:Setari implicite`.

### `EF_F`

Local table in Avacont.accdb.

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | ID | AutoNumber | 4 | False |  |  |
| 1 | IdDocument | Long | 4 | False |  |  |
| 2 | IdOperatie | Long | 4 | False |  |  |
| 3 | IdClsf | Long | 4 | False |  |  |
| 4 | IdPlata | Long | 4 | False |  |  |
| 5 | ID8 | Long | 4 | False | 0 |  |
| 6 | IDPF | Long | 4 | False | 0 |  |
| 7 | IDEFT | Long | 4 | False | 0 |  |
| 8 | NC | Long | 4 | False | Null |  |
| 9 | Forma | Text | 255 | False |  |  |
| 10 | Clsf | Text | 255 | False |  |  |
| 11 | NumarDocument | Text | 50 | False |  |  |
| 12 | DataDocument | Date/Time | 8 | False |  |  |
| 13 | FelDocument | Text | 50 | False |  |  |
| 14 | Explicatie | Text | 255 | False |  |  |
| 15 | Fel | Text | 255 | False |  |  |
| 16 | TotalFactura | Double | 8 | False |  |  |
| 17 | SalvatAnterior | Double | 8 | False | 0 |  |
| 18 | ValoareRamasa | Double | 8 | False |  |  |
| 19 | Diferenta | Double | 8 | False |  |  |
| 20 | NrAng | Long | 4 | False |  |  |
| 21 | ALG | Yes/No | 1 | False | No |  |
| 22 | DataALG | Date/Time | 8 | False |  |  |
| 23 | SumaALG | Double | 8 | False | 0 |  |
| 24 | ValInit | Double | 8 | False | Null |  |

Indexes: `ID: (ID8)`; `ID1: (ID)`; `IdClsf: (IdClsf)`; `IdDocument: (IdDocument)`; `IDEFT: (IDEFT)`; `IdOperatie: (IdOperatie)`; `IDPF: (IDPF)`; `IdPlata: (IdPlata)`; `NumarDocument: (NumarDocument)`; `PrimaryKey: (ID) PRIMARY UNIQUE`

In scope - written by: `query:QEF_EV_inlucru`

In scope - read by: `form:EFACTURA_2025`, `form:EF_F_inlucru`

Also used by 0 object(s) outside the scope.

### `EF_TMP`

Local table in Avacont.accdb.

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | ID | Long | 4 | True | 0 |  |
| 1 | IDS | Text | 255 | False |  |  |
| 2 | IDI | Text | 255 | False |  |  |
| 3 | DataC | Date/Time | 8 | False |  |  |
| 4 | Detalii | Memo/Long Text | 0 | False |  |  |
| 5 | TIP | Text | 255 | False |  |  |
| 6 | CIFE | Text | 255 | False |  |  |
| 7 | S | Yes/No | 1 | False | No |  |
| 8 | FURNIZOR | Text | 255 | False |  |  |
| 9 | NC | Long | 4 | False | 1 |  |

Indexes: `IDI: (IDI)`; `IDS: (IDS)`; `PrimaryKey: (ID) PRIMARY UNIQUE`

In scope - written by: `form:EFACTURA_TMP`, `module:mdl_EFactura`

In scope - read by: nothing found

Also used by 0 object(s) outside the scope.

### `EF_UM`

Local table in Avacont.accdb.

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | COD | Text | 255 | True |  |  |
| 1 | EXP | Text | 255 | False |  |  |

Indexes: `EXP: (EXP)`; `PrimaryKey: (COD) PRIMARY UNIQUE`

In scope - written by: nothing found

In scope - read by: `form:EFACTURA_ADD_SUB`, `form:EFACTURA_UM`

Also used by 0 object(s) outside the scope.

### `Factura`

Linked table. Source: `Factura`  Connect: `MS Access;PWD=***;DATABASE=C:\AVACONT\ENERGETIC LOCAL\baza2026.accdb`

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | IdFactura | Long | 4 | True |  |  |
| 1 | IdClient | Long | 4 | False | 0 |  |
| 2 | IdOperatie | Long | 4 | False | Null |  |
| 3 | NumarFactura | Double | 8 | False |  |  |
| 4 | DataFactura | Date/Time | 8 | False |  |  |
| 5 | Comentarii | Text | 255 | False |  |  |
| 6 | Anulata | Yes/No | 1 | False |  |  |
| 7 | TRIMISA | Yes/No | 1 | False | 0 |  |
| 8 | id_incarcare | Text | 255 | False |  |  |
| 9 | id_descarcare | Text | 255 | False |  |  |
| 10 | ATT | Yes/No | 1 | False | No |  |
| 11 | ContPlata | Text | 255 | False |  |  |
| 12 | DTQ | Date/Time | 8 | False | Now() |  |
| 13 | SerieFactura | Text | 255 | False |  |  |
| 14 | BT_13 | Text | 30 | False |  |  |
| 15 | TipFactura | Text | 3 | False | 380 |  |
| 16 | IdFacturaA | Long | 4 | False |  |  |
| 17 | SerieFacturaA | Text | 10 | False |  |  |
| 18 | NumarFacturaA | Text | 10 | False |  |  |
| 19 | Corectata | Double | 8 | False | 0 |  |

Indexes: `ClientiEF__IdClient___Factura__IdClient: (IdClient) FOREIGN`; `Id: (IdFactura)`; `id_descarcare: (id_descarcare)`; `IdOperatie: (IdOperatie)`; `IdUnitate: (IdClient)`; `NumarFactura: (NumarFactura) UNIQUE`; `PrimaryKey: (IdFactura) PRIMARY UNIQUE`

In scope - written by: `form:EFACTURA_ADD`, `module:mdl_EFactura`

In scope - read by: `query:qFacturi_Vanzare`, `form:EFACTURA_ADD_FACTURI`, `module:mdl_EFactura_Add`

Also used by 7 object(s) outside the scope: `form:Note`, `form:Setari implicite`, `report:EF_ERR`, `report:FacturaV`, `module:mdl_2024`, `module:mdl_Functii_Generale`, `module:mdl_Popup2022`.

### `FacturaC`

Linked table. Source: `FacturaC`  Connect: `MS Access;PWD=***;DATABASE=C:\AVACONT\ENERGETIC LOCAL\baza2026.accdb`

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | IdContinut | AutoNumber | 4 | False |  |  |
| 1 | IdFactura | Long | 4 | False | 0 |  |
| 2 | Continut | Memo/Long Text | 0 | False |  |  |
| 3 | Um | Text | 50 | False |  |  |
| 4 | Cant | Double | 8 | False | 0 |  |
| 5 | PU | Double | 8 | False | 0 |  |
| 6 | Valoare | Double | 8 | False | 0 |  |
| 7 | Platit | Yes/No | 1 | False | 0 |  |
| 8 | NrCrt | Text | 255 | False |  |  |
| 9 | Grup | Long | 4 | False | 0 |  |

Indexes: `Factura__IdFactura___FacturaC__IdFactura: (IdFactura) FOREIGN`; `IdFactura: (IdContinut)`; `IdFactura1: (IdFactura)`; `PrimaryKey: (IdContinut) PRIMARY UNIQUE`

In scope - written by: `form:EFACTURA_ADD`

In scope - read by: `query:qFacturi_Vanzare`, `form:EFACTURA_ADD_FACTURI`, `module:mdl_EFactura_Add`

Also used by 3 object(s) outside the scope: `report:FacturaV`, `report:FacturaV_sub`, `module:mdl_Popup2022`.

### `TEF_F`

Local table in Avacont.accdb.

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | CP | Text | 255 | False |  |  |
| 1 | Furnizor | Text | 255 | False |  |  |
| 2 | CF | Text | 255 | True |  |  |

Indexes: `Nume: (Furnizor)`; `PrimaryKey: (CF) PRIMARY UNIQUE`

In scope - written by: nothing found

In scope - read by: nothing found

Also used by 0 object(s) outside the scope.

### `TEF_FF`

Local table in Avacont.accdb.

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | IDEFT | Long | 4 | True | 0 |  |
| 1 | CF | Text | 255 | True |  |  |
| 2 | id_sol | Text | 255 | False |  |  |
| 3 | NrFact | Text | 255 | False |  |  |
| 4 | LunaAn | Text | 255 | False |  |  |
| 5 | Luna | Long | 4 | False | 0 |  |
| 6 | DataFact | Date/Time | 8 | False |  |  |
| 7 | TotalFactura | Double | 8 | False | 0 |  |
| 8 | TotalSalvat | Double | 8 | False | 0 |  |
| 9 | TotalRamas | Double | 8 | False | 0 |  |
| 10 | TIP | Text | 255 | False |  |  |

Indexes: `{5B4E5596-2A8F-4FAE-9346-A8E1C017FB1D}: (CF) FOREIGN`; `CF: (CF)`; `id_sol: (id_sol)`; `IDEFT: (IDEFT)`; `PrimaryKey: (IDEFT) PRIMARY UNIQUE`

In scope - written by: nothing found

In scope - read by: nothing found

Also used by 0 object(s) outside the scope.

### `TrecereEFactura`

Local table in Avacont.accdb.

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | CUI | Text | 255 | True |  |  |
| 1 | ID_SOL | Text | 255 | True |  |  |
| 2 | IDEFT | Long | 4 | True | 0 |  |
| 3 | S | Yes/No | 1 | False | Yes |  |

Indexes: `PrimaryKey: (CUI, ID_SOL, IDEFT) PRIMARY UNIQUE`

In scope - written by: nothing found

In scope - read by: nothing found

Also used by 2 object(s) outside the scope: `form:TrecereAN`, `module:mdl_TRECERE_AN`.

### `tblMSG`

Local table in Avacont.accdb.

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | ID | Long | 4 | True |  |  |
| 1 | Mesaj | Memo/Long Text | 0 | False |  |  |
| 2 | Titlu | Text | 255 | False |  |  |
| 3 | Fel | Long | 4 | False |  |  |
| 4 | Tip | Long | 4 | False |  |  |
| 5 | Fls | Long | 4 | False |  |  |
| 6 | IB | Text | 255 | False |  |  |
| 7 | SQL | Memo/Long Text | 0 | False |  |  |
| 8 | Valid | Text | 255 | False |  |  |
| 9 | Defa | Text | 255 | False |  |  |
| 10 | Frm | Text | 255 | False |  |  |
| 11 | ActBtn | Text | 255 | False |  |  |
| 12 | IDIMG | Long | 4 | False | 0 |  |

Indexes: `ID: (ID)`; `IDIMG: (IDIMG)`; `PrimaryKey: (ID) PRIMARY UNIQUE`; `Titlu: (Titlu)`; `Valid: (Valid)`

In scope - written by: nothing found

In scope - read by: `form:EFACTURA_2025`, `form:EFACTURA_ADD`

Also used by 17 object(s) outside the scope.

### `tmpEF`

Local table in Avacont.accdb.

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | IDEF | AutoNumber | 4 | False |  |  |
| 1 | id_sol | Text | 255 | True |  |  |
| 2 | ID | Text | 255 | False |  |  |
| 3 | CIF | Text | 255 | False |  |  |
| 4 | Data | Date/Time | 8 | False |  |  |

Indexes: `ID: (ID)`; `id_solicitare: (id_sol)`; `IDEF: (IDEF)`; `PrimaryKey: (id_sol) PRIMARY UNIQUE`

In scope - written by: nothing found

In scope - read by: nothing found

Also used by 0 object(s) outside the scope.

### `tmpEFS`

Local table in Avacont.accdb.

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | IDEFS | AutoNumber | 4 | False |  |  |
| 1 | IDEFT | Long | 4 | False | 0 |  |
| 2 | ID | Long | 4 | False | 0 |  |
| 3 | UNIT | Text | 255 | False |  |  |
| 4 | Cant | Double | 8 | False | 0 |  |
| 5 | Valoare | Double | 8 | False | 0 |  |
| 6 | Pret | Double | 8 | False | 0 |  |
| 7 | Denumire | Text | 255 | False |  |  |
| 8 | S | Yes/No | 1 | False | No |  |

Indexes: `ID: (ID)`; `IDEFS: (IDEFS)`; `IDEFT: (IDEFT)`; `PrimaryKey: (IDEFS) PRIMARY UNIQUE`

In scope - written by: nothing found

In scope - read by: nothing found

Also used by 0 object(s) outside the scope.

### `tmpEFT`

Local table in Avacont.accdb.

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | IDEFT | AutoNumber | 4 | False |  |  |
| 1 | NrFact | Text | 255 | False |  |  |
| 2 | DataFact | Date/Time | 8 | False |  |  |
| 3 | DataScad | Date/Time | 8 | False |  |  |
| 4 | CotaTVA | Long | 4 | False | 0 |  |
| 5 | TVA | Double | 8 | False | 0 |  |
| 6 | Valoare | Double | 8 | False | 0 |  |
| 7 | TOTAL | Double | 8 | False | 0 |  |
| 8 | id_sol | Text | 255 | False |  |  |
| 9 | CUI | Text | 255 | False |  |  |
| 10 | DenumireP | Text | 255 | False |  |  |
| 11 | Adresa | Text | 255 | False |  |  |
| 12 | S | Yes/No | 1 | False | No |  |

Indexes: `id_sol: (id_sol)`; `IDF: (IDEFT)`; `PrimaryKey: (IDEFT) PRIMARY UNIQUE`

In scope - written by: nothing found

In scope - read by: nothing found

Also used by 0 object(s) outside the scope.

### `tmpEF_TRANS`

Local table in Avacont.accdb.

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | ID | AutoNumber | 4 | False |  |  |
| 1 | id_sol | Text | 255 | False |  |  |
| 2 | IDEFT | Long | 4 | False | 0 |  |
| 3 | IDEFTN | Long | 4 | False | 0 |  |

Indexes: `ID: (ID)`; `id_sol: (id_sol)`; `IDEFT: (IDEFT)`; `IDEFTN: (IDEFTN)`; `PrimaryKey: (ID) PRIMARY UNIQUE`

In scope - written by: `form:EFACTURA_AA`

In scope - read by: nothing found

Also used by 0 object(s) outside the scope.

### `tmpFacturaC`

Local table in Avacont.accdb.

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | IdContinut | AutoNumber | 4 | False |  |  |
| 1 | IdFactura | Long | 4 | False | 0 |  |
| 2 | Continut | Memo/Long Text | 0 | False |  |  |
| 3 | Um | Text | 50 | False |  |  |
| 4 | Cant | Double | 8 | False | 0 |  |
| 5 | PU | Double | 8 | False | 0 |  |
| 6 | Valoare | Double | 8 | False | 0 |  |
| 7 | Platit | Yes/No | 1 | False | 0 |  |
| 8 | NrCrt | Text | 255 | False |  |  |
| 9 | Grup | Long | 4 | False | 0 |  |

Indexes: `IdFactura: (IdContinut)`; `IdFactura1: (IdFactura)`; `PrimaryKey: (IdContinut) PRIMARY UNIQUE`

In scope - written by: `form:EFACTURA_ADD`, `form:EFACTURA_ADD_FACTURI`, `form:EFACTURA_ADD_SUB`

In scope - read by: nothing found

Also used by 1 object(s) outside the scope: `module:mdl_Popup2022`.

### `tmpMF_EFactura`

Local table in Avacont.accdb.

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | IDTEF | AutoNumber | 4 | False |  |  |
| 1 | IDEF | Long | 4 | False |  |  |
| 2 | IDEFT | Long | 4 | False |  |  |
| 3 | IdUnitate | Long | 4 | False | 0 |  |
| 4 | id_sol | Text | 255 | False |  |  |
| 5 | id | Text | 255 | False |  |  |
| 6 | NrFact | Text | 255 | False |  |  |
| 7 | DataFact | Date/Time | 8 | False |  |  |
| 8 | CotaTVA | Long | 4 | False |  |  |
| 9 | TVA | Double | 8 | False |  |  |
| 10 | Valoare | Double | 8 | False |  |  |
| 11 | TOTAL | Double | 8 | False |  |  |
| 12 | Asociat | Double | 8 | False |  |  |
| 13 | DeAsociat | Double | 8 | False |  |  |
| 14 | DenumireP | Text | 255 | False |  |  |
| 15 | AreAtt | Integer | 2 | False |  |  |
| 16 | AreXml | Integer | 2 | False |  |  |
| 17 | S | Yes/No | 1 | False | No |  |
| 18 | Pic | OLE Object | 0 | False |  |  |
| 19 | Tip | Text | 255 | False |  |  |
| 20 | AlteAsocieri | Double | 8 | False | 0 |  |

Indexes: `ID: (IDTEF)`; `id_sol: (id_sol)`; `id1: (id)`; `IDEF: (IDEF)`; `IDEFT: (IDEFT)`; `IdUnitate: (IdUnitate)`; `PrimaryKey: (IDTEF) PRIMARY UNIQUE`

In scope - written by: `query:qMF_EFactura_Initial`, `query:qMF_EFactura_Ramas`, `query:qMF_EFactura_Toate`, `form:MF2019_Asoc_EF`, `form:Note_EFactura`, `form:_MF2019_Asoc_EF`

In scope - read by: nothing found

Also used by 1 object(s) outside the scope: `form:MF2019_1`.

### `tmpNoteFacturi`

Local table in Avacont.accdb.

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | ID | AutoNumber | 4 | False |  |  |
| 1 | S | Yes/No | 1 | False | No |  |
| 2 | IdOperatie | Long | 4 | False |  |  |
| 3 | IdDocument | Long | 4 | False |  |  |
| 4 | NumarDocument | Text | 255 | False |  |  |
| 5 | DataDocument | Date/Time | 8 | False |  |  |
| 6 | NrOP | Long | 4 | False | 0 |  |
| 7 | DataOP | Date/Time | 8 | False |  |  |
| 8 | Partener | Text | 255 | False |  |  |
| 9 | Cont | Text | 255 | False |  |  |
| 10 | Valoare | Double | 8 | False | 0 |  |
| 11 | CodAng | Text | 255 | False |  |  |
| 12 | CodInd | Text | 255 | False |  |  |
| 13 | CodPrg | Text | 255 | False |  |  |
| 14 | IDOPME | Long | 4 | False | 0 |  |
| 15 | ExplicatieOP | Text | 255 | False |  |  |
| 16 | NrOPE | Long | 4 | False | 0 |  |
| 17 | ValoarePlata | Double | 8 | False | 0 |  |
| 18 | Fel | Text | 255 | False |  |  |

Indexes: `IdFactura: (IdDocument)`; `IDOPME: (IDOPME)`; `IdPlata: (IdOperatie)`; `NumarDocument: (NumarDocument)`; `PrimaryKey: (ID) PRIMARY UNIQUE`

In scope - written by: nothing found

In scope - read by: nothing found

Also used by 3 object(s) outside the scope: `form:Note`, `form:Note_PlatiAsoc`, `module:basRibbonCallbacks`.

### `tmpNote_EFactura`

Local table in Avacont.accdb.

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | IDTEF | AutoNumber | 4 | False |  |  |
| 1 | IDEF | Long | 4 | False |  |  |
| 2 | IDEFT | Long | 4 | False |  |  |
| 3 | id_sol | Text | 255 | False |  |  |
| 4 | id | Text | 255 | False |  |  |
| 5 | NrFact | Text | 255 | False |  |  |
| 6 | DataFact | Date/Time | 8 | False |  |  |
| 7 | CotaTVA | Long | 4 | False |  |  |
| 8 | TVA | Double | 8 | False |  |  |
| 9 | Valoare | Double | 8 | False |  |  |
| 10 | TOTAL | Double | 8 | False |  |  |
| 11 | Asociat | Double | 8 | False |  |  |
| 12 | AlteAsocieri | Double | 8 | False | 0 |  |
| 13 | DeAsociat | Double | 8 | False |  |  |
| 14 | DenumireP | Text | 255 | False |  |  |
| 15 | AreAtt | Integer | 2 | False |  |  |
| 16 | AreXml | Integer | 2 | False |  |  |
| 17 | S | Yes/No | 1 | False | No |  |
| 18 | Tip | Text | 255 | False |  |  |
| 19 | Pic | OLE Object | 0 | False |  |  |

Indexes: `ID: (IDTEF)`; `id_sol: (id_sol)`; `id1: (id)`; `IDEF: (IDEF)`; `IDEFT: (IDEFT)`; `PrimaryKey: (IDTEF) PRIMARY UNIQUE`

In scope - written by: `query:qNote_EFactura_Toate`, `query:_qNote_EFacturat_Ramas`, `query:_qNote_EFactura_Asociat_Nou`, `query:_qNote_EFactura_Initial`, `form:Note_EFactura`

In scope - read by: `form:MF2019_Asoc_EF`

Also used by 1 object(s) outside the scope: `form:Note`.
