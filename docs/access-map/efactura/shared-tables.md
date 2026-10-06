# Shared tables used by the scope

These belong to the wider Avacont system (accounting, partners, classifications). The e-invoice screens read them; most are NOT part of what has to be rebuilt for e-invoice, but the columns the queries use must exist in K-BOT.

### `BIC`

Local table in Avacont.accdb.

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | Cod | Text | 255 | False |  |  |
| 1 | Banca | Text | 255 | False |  |  |
| 2 | SWIFT | Text | 255 | False |  |  |

In scope - written by: nothing found

In scope - read by: `form:EFACTURA_CLIENTI`, `module:mdl_EFactura_Add`

Also used by 4 object(s) outside the scope: `form:Burse`, `form:OPuri`, `form:Parteneri`, `module:mdl_Popup2022`.

### `Balanta_Forexe`

Local table in Avacont.accdb.

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | Cont | Text | 255 | True |  |  |
| 1 | SIMBOL_CONT | Text | 255 | False |  |  |
| 2 | COD_SURSA | Text | 255 | False |  |  |
| 3 | COD_SECTOR | Text | 255 | False |  |  |
| 4 | COD_FUNCTIONAL | Text | 255 | False |  |  |
| 5 | COD_ECONOMIC | Text | 255 | False |  |  |
| 6 | DENUMIRE_CONT | Text | 255 | False |  |  |
| 7 | SI_DEBIT | Double | 8 | False |  |  |
| 8 | SI_CREDIT | Double | 8 | False |  |  |
| 9 | RLC_DEBIT | Double | 8 | False |  |  |
| 10 | RLC_CREDIT | Double | 8 | False |  |  |
| 11 | TR_DEBIT | Double | 8 | False |  |  |
| 12 | TR_CREDIT | Double | 8 | False |  |  |
| 13 | TS_DEBIT | Double | 8 | False |  |  |
| 14 | TS_CREDIT | Double | 8 | False |  |  |
| 15 | SF_DEBIT | Double | 8 | False |  |  |
| 16 | SF_CREDIT | Double | 8 | False |  |  |
| 17 | DENUMIRE_SURSA | Text | 255 | False |  |  |
| 18 | DENUMIRE_SECTOR | Text | 255 | False |  |  |
| 19 | DENUMIRE_INDICATOR_CF | Text | 255 | False |  |  |
| 20 | DENUMIRE_INDICATOR_CE | Text | 255 | False |  |  |

Indexes: `COD_ECO: (COD_ECONOMIC)`; `COD_FUN: (COD_FUNCTIONAL)`; `COD_SECTOR: (COD_SECTOR)`; `COD_SURSA: (COD_SURSA)`; `CONT: (Cont)`; `PrimaryKey: (Cont) PRIMARY UNIQUE`; `SIMBOL_CONT: (SIMBOL_CONT)`

In scope - written by: `module:mdl_EFactura`

In scope - read by: nothing found

Also used by 2 object(s) outside the scope: `query:Balanta_Compare`, `module:mdl_ExportExcel`.

### `CALE`

Local table in Avacont.accdb.

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | ID | AutoNumber | 4 | False |  |  |
| 1 | IdUnitate | Double | 8 | False | 0 |  |
| 2 | CALE | Text | 255 | False |  |  |
| 3 | CALEEF | Text | 255 | False |  |  |
| 4 | NumeUnitate | Text | 255 | False |  |  |
| 5 | Detalii | Text | 255 | False |  |  |
| 6 | CaleRealaEF | Text | 255 | False |  |  |
| 7 | CaleReala | Text | 255 | False |  |  |
| 8 | CU | Text | 255 | False |  |  |
| 9 | ANL | Long | 4 | False |  |  |
| 10 | DC | Text | 255 | False |  |  |
| 11 | Blocat | Yes/No | 1 | False | 0 |  |
| 12 | CDeschid | Text | 255 | False |  |  |
| 13 | Usr | Text | 255 | False |  |  |
| 14 | FaraUpdate | Yes/No | 1 | False | 0 |  |
| 15 | Expirat | Yes/No | 1 | False | 0 |  |
| 16 | Gest | Yes/No | 1 | False | No |  |
| 17 | CaleRealaGest | Text | 255 | False |  |  |
| 18 | Sursa | Text | 255 | False |  |  |
| 19 | CodFiscal | Text | 255 | False |  |  |
| 20 | SclavBuget | Yes/No | 1 | False | No |  |
| 21 | IdUnitateParinte | Long | 4 | False | 0 |  |
| 22 | MasterBuget | Yes/No | 1 | False | No |  |
| 23 | UnitatiSclav | Text | 255 | False |  |  |

Indexes: `CaleReala: (CaleReala)`; `CDeschid: (CDeschid)`; `CodFiscal: (CodFiscal)`; `IdUnitate: (IdUnitate)`; `IdUnitateParinte: (IdUnitateParinte)`; `IdUnitateSclav: (UnitatiSclav)`; `NumeUnitate: (NumeUnitate)`; `PrimaryKey: (ID) PRIMARY UNIQUE`; `Sursa: (Sursa)`

In scope - written by: nothing found

In scope - read by: `query:EFACTURA_AA`, `query:QEF`, `query:QEF_ARH`, `query:QEF_EV`, `query:QEF_EV_inlucru`, `query:QEF_NOI`, `query:QEF_PL`, `query:QEF_RestDePlata`, `query:QEF_RestDePlata_Data`, `query:QEF_SAV`, `query:QEF_TRECERE`, `form:EFACTURA_AA`, `module:mdl_EFactura`, `class:clsAnaf_Async`

Also used by 49 object(s) outside the scope.

### `CONFIGS`

Local table in Avacont.accdb.

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | ID | AutoNumber | 4 | False |  |  |
| 1 | EXPL | Text | 255 | False |  |  |
| 2 | INFO | Memo/Long Text | 0 | False |  |  |

Indexes: `ID: (ID)`; `PrimaryKey: (ID) PRIMARY UNIQUE`

In scope - written by: nothing found

In scope - read by: `module:mdl_EFactura`, `class:clsAnaf_Async`

Also used by 0 object(s) outside the scope.

### `CT8030`

Linked table. Source: `CT8030`  Connect: `MS Access;PWD=***;DATABASE=C:\AVACONT\ENERGETIC LOCAL\baza2026.accdb`

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | ID | AutoNumber | 4 | False |  |  |
| 1 | NumarDocument | Text | 255 | False |  |  |
| 2 | DataDocument | Date/Time | 8 | False |  |  |
| 3 | DataOperatie | Date/Time | 8 | False |  |  |
| 4 | SumaDebit | Double | 8 | False |  |  |
| 5 | SumaCredit | Double | 8 | False |  |  |
| 6 | DenumireDocument | Text | 255 | False |  |  |
| 7 | Cont | Text | 255 | False |  |  |
| 8 | Sursa | Text | 255 | False |  |  |

Indexes: `NumarDocument: (NumarDocument)`; `PrimaryKey: (ID) PRIMARY UNIQUE`

In scope - written by: nothing found

In scope - read by: `query:Q_7`

Also used by 5 object(s) outside the scope: `query:Cont_8030`, `query:qCT8030`, `form:subCT8030`, `report:FisaCT8030`, `module:basRibbonCallbacks`.

### `Clasificatii`

Linked table. Source: `Clasificatii`  Connect: `MS Access;PWD=***;DATABASE=C:\AVACONT\ENERGETIC LOCAL\baza2026.accdb`

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | IDClsf | Long | 4 | True | 0 |  |
| 1 | Capitol | Text | 50 | False |  |  |
| 2 | Subcapitol | Text | 50 | False |  |  |
| 3 | Articol | Text | 50 | False |  |  |
| 4 | Alineat | Text | 50 | False |  |  |
| 6 | TOTAL | Double | 8 | False | 0 |  |
| 7 | Trim1 | Double | 8 | False | 0 |  |
| 8 | Trim2 | Double | 8 | False | 0 |  |
| 9 | Trim3 | Double | 8 | False | 0 |  |
| 10 | Trim4 | Double | 8 | False | 0 |  |
| 11 | DTQ | Date/Time | 8 | False | Now() |  |
| 12 | Document | Text | 255 | False |  |  |
| 13 | Data | Date/Time | 8 | False |  |  |
| 14 | Denumire | Text | 255 | False |  |  |
| 15 | IdLegatura | Long | 4 | False |  |  |
| 16 | Clsf | Text | 243 | False |  |  |
| 17 | Titlu | Text | 243 | False |  |  |
| 18 | Sector | Text | 243 | False |  |  |
| 20 | ClsfSal | Text | 243 | False |  |  |
| 21 | ClsfF | Text | 243 | False |  |  |
| 22 | ClsfE | Text | 243 | False |  |  |
| 24 | ClsfX | Text | 243 | False |  |  |
| 25 | CodAng | Text | 10 | False |  |  |
| 26 | CodInd | Text | 3 | False |  |  |
| 28 | TOTALFX | Double | 8 | False | 0 |  |
| 29 | IdClsfPY | Long | 4 | False | 0 |  |
| 30 | Esinc | Yes/No | 1 | False | 0 |  |
| 31 | Sursa | Text | 243 | False |  |  |
| 32 | SS | Text | 243 | False |  |  |
| 33 | CodSSI | Text | 243 | False |  |  |

Indexes: `C: (Capitol)`; `L: (Alineat)`; `PrimaryKey: (IDClsf) PRIMARY UNIQUE`; `R: (Articol)`; `S: (Subcapitol)`

In scope - written by: `form:EFACTURA_ADD`

In scope - read by: `query:qClsfE`, `query:qDefaNote`, `query:QEF_OPER`, `query:QEF_PL`, `query:QEF_SAV`, `query:Q_2`, `form:EFACTURA_2025`

Also used by 146 object(s) outside the scope.

### `ClasificatiiV`

Linked table. Source: `ClasificatiiV`  Connect: `MS Access;PWD=***;DATABASE=C:\AVACONT\ENERGETIC LOCAL\baza2026.accdb`

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | IdClsfV | Long | 4 | True | 0 |  |
| 1 | Capitol | Text | 255 | False |  |  |
| 2 | SubCapitol | Text | 255 | False |  |  |
| 3 | Paragraf | Text | 255 | False |  |  |
| 4 | Trim1 | Long | 4 | False | 0 |  |
| 5 | Trim2 | Long | 4 | False | 0 |  |
| 6 | Trim3 | Long | 4 | False | 0 |  |
| 7 | Trim4 | Long | 4 | False | 0 |  |
| 8 | Denumire | Text | 255 | False |  |  |

Indexes: `IdClsfV: (IdClsfV)`; `PrimaryKey: (IdClsfV) PRIMARY UNIQUE`

In scope - written by: nothing found

In scope - read by: `query:qCLSFV`, `query:Q_1`, `query:Q_13`

Also used by 7 object(s) outside the scope: `query:BugetVSelect`, `query:Cont_8090`, `query:qClsfV2025`, `form:AddClsf_Venituri`, `form:ClasificatiiBugetareV`, `form:ClasificatiiV`, `form:RectificariV`.

### `Cont8067`

Linked table. Source: `Cont8067`  Connect: `MS Access;PWD=***;DATABASE=C:\AVACONT\ENERGETIC LOCAL\baza2026.accdb`

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | ID8 | AutoNumber | 4 | False |  |  |
| 1 | IDO | Long | 4 | False | 0 |  |
| 2 | DataO | Date/Time | 8 | False |  |  |
| 3 | DataAlg | Date/Time | 8 | False |  |  |
| 4 | SumaAlg | Double | 8 | False | 0 |  |
| 5 | CodP | Text | 255 | False |  |  |
| 6 | Contract | Double | 8 | False |  |  |
| 7 | IdClsf | Long | 4 | False |  |  |
| 8 | ContractA | Long | 4 | False |  |  |
| 9 | DTQ | Date/Time | 8 | False | Now() |  |

Indexes: `Clasificatii__IDClsf___Cont8067__IdClsf: (IdClsf) FOREIGN`; `Contracte__NrCrt___Cont8067__Contract: (Contract) FOREIGN`; `Contracte_A__NrCrtA___Cont8067__ContractA: (ContractA) FOREIGN`; `ID8: (ID8)`; `IDO: (IDO)`; `Oper__IdOperatie___Cont8067__IDO: (IDO) FOREIGN`; `PrimaryKey: (ID8) PRIMARY UNIQUE`

In scope - written by: `form:EF_F`, `form:EF_F_inlucru`

In scope - read by: `query:QEF_OPER`, `query:QEF_PL`

Also used by 51 object(s) outside the scope.

### `Contracte`

Linked table. Source: `Contracte`  Connect: `MS Access;PWD=***;DATABASE=C:\AVACONT\ENERGETIC LOCAL\baza2026.accdb`

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | ID | AutoNumber | 4 | False |  |  |
| 1 | NrCrt | Double | 8 | True |  |  |
| 2 | IdClsf | Long | 4 | False |  |  |
| 3 | Denumire | Text | 255 | False |  |  |
| 4 | NrDoc | Text | 255 | False |  |  |
| 5 | Data | Date/Time | 8 | False |  |  |
| 6 | Explicatie | Text | 255 | False |  |  |
| 7 | Valoare | Double | 8 | False | 0 |  |
| 8 | CodAngajament | Text | 255 | False |  |  |
| 9 | CodIndicator | Text | 255 | False |  |  |
| 10 | CodPartener | Text | 255 | False |  |  |
| 11 | DTQ | Date/Time | 8 | False | Now() |  |

Indexes: `Clasificatii__IDClsf___Contracte__IdClsf: (IdClsf) FOREIGN`; `idclsf: (IdClsf)`; `NrCrt: (NrCrt) PRIMARY UNIQUE`

In scope - written by: nothing found

In scope - read by: `query:Q_8`, `form:EFACTURA_2025`

Also used by 18 object(s) outside the scope.

### `Contracte_A`

Linked table. Source: `Contracte_A`  Connect: `MS Access;PWD=***;DATABASE=C:\AVACONT\ENERGETIC LOCAL\baza2026.accdb`

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | ID | AutoNumber | 4 | False |  |  |
| 1 | NrCrt | Double | 8 | False |  |  |
| 2 | Ajustare | Double | 8 | False |  |  |
| 3 | DataAjustare | Date/Time | 8 | False |  |  |
| 4 | NrCrtA | Long | 4 | False |  |  |

Indexes: `Contracte__NrCrt___Contracte_A__NrCrt: (NrCrt) FOREIGN`; `NrCrtA: (NrCrtA) PRIMARY UNIQUE`

In scope - written by: nothing found

In scope - read by: `query:Q_8`

Also used by 7 object(s) outside the scope: `query:8067`, `query:Cont8067_Q`, `query:Cont8067_X`, `query:Cont8067_Z`, `query:qContracte`, `form:Contracte`, `form:Contracte_a`.

### `Credit`

Linked table. Source: `Credit`  Connect: `MS Access;PWD=***;DATABASE=C:\AVACONT\ENERGETIC LOCAL\baza2026.accdb`

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | IdOperatie | Long | 4 | False | 0 |  |
| 1 | SimbolCont | Text | 50 | False |  |  |
| 2 | CodPartener | Text | 50 | False |  |  |
| 3 | SumaCredit | Double | 8 | False | 0 |  |

Indexes: `CodPartener: (CodPartener)`; `IdOperatie: (IdOperatie)`; `Oper__IdOperatie___Credit__IdOperatie: (IdOperatie) FOREIGN`; `Parteneri__CodPartener___Credit__CodPartener: (CodPartener) FOREIGN`; `PlanCont__SimbolCont___Credit__SimbolCont: (SimbolCont) FOREIGN`; `SimbolCont: (SimbolCont)`; `SumaCredit: (SumaCredit)`

In scope - written by: nothing found

In scope - read by: `query:BalantaSelectFP`, `query:DetaliiEvidentiereFactura`, `query:QEF`, `query:QEF_OPER`, `query:qPlatiParteneri2015`

Also used by 135 object(s) outside the scope.

### `Debit`

Linked table. Source: `Debit`  Connect: `MS Access;PWD=***;DATABASE=C:\AVACONT\ENERGETIC LOCAL\baza2026.accdb`

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | IdOperatie | Long | 4 | False | 0 |  |
| 1 | SimbolCont | Text | 50 | False |  |  |
| 2 | CodPartener | Text | 50 | False |  |  |
| 3 | SumaDebit | Double | 8 | False | 0 |  |

Indexes: `CodPartener: (CodPartener)`; `IdOperatie: (IdOperatie)`; `Oper__IdOperatie___Debit__IdOperatie: (IdOperatie) FOREIGN`; `Parteneri__CodPartener___Debit__CodPartener: (CodPartener) FOREIGN`; `PlanCont__SimbolCont___Debit__SimbolCont: (SimbolCont) FOREIGN`; `SimbolCont: (SimbolCont)`; `SumaDebit: (SumaDebit)`

In scope - written by: nothing found

In scope - read by: `query:BalantaSelectFP`, `query:DetaliiPlataFactura`

Also used by 110 object(s) outside the scope.

### `DefaCont`

Linked table. Source: `DefaCont`  Connect: `MS Access;PWD=***;DATABASE=C:\AVACONT\ENERGETIC LOCAL\baza2026.accdb`

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | Cont | Text | 255 | True |  |  |
| 1 | Denumire | Text | 255 | False |  |  |
| 2 | Tip | Text | 1 | False |  |  |
| 3 | Sector | Yes/No | 1 | False | 0 |  |
| 4 | Sursa | Yes/No | 1 | False | 0 |  |
| 5 | ClsfF | Yes/No | 1 | False | 0 |  |
| 6 | ClsfE | Yes/No | 1 | False | 0 |  |
| 7 | Vizibil | Yes/No | 1 | False | 0 |  |

Indexes: `PrimaryKey: (Cont) PRIMARY UNIQUE`

In scope - written by: nothing found

In scope - read by: `query:BalantaSelectFP`, `query:Find duplicates for DefaCont`, `query:qDefaCont_Bilant`

Also used by 27 object(s) outside the scope.

### `DefaNote`

Linked table. Source: `DefaNote`  Connect: `MS Access;PWD=***;DATABASE=C:\AVACONT\ENERGETIC LOCAL\baza2026.accdb`

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | ID | Long | 4 | True |  |  |
| 1 | IDA | Long | 4 | False |  |  |
| 2 | Forma | Text | 255 | False |  |  |
| 3 | Tip | Long | 4 | False |  |  |
| 4 | Clsf | Long | 4 | False |  |  |
| 5 | Jurnal | Text | 255 | False |  |  |
| 6 | Explicatie | Text | 255 | False |  |  |
| 7 | ContDebit | Text | 255 | False |  |  |
| 8 | PartDebit | Text | 255 | False |  |  |
| 9 | ContCredit | Text | 255 | False |  |  |
| 10 | PartCredit | Text | 255 | False |  |  |
| 11 | Ordonantare | Yes/No | 1 | False |  |  |
| 12 | PA | Yes/No | 1 | False |  |  |
| 13 | Chelt | Yes/No | 1 | False |  |  |
| 14 | Simpla | Long | 4 | False |  |  |
| 15 | Fel | Text | 255 | False |  |  |
| 16 | DTQ | Date/Time | 8 | False | Now() |  |

Indexes: `Clasificatii__IDClsf___DefaNote__Clsf: (Clsf) FOREIGN`; `Clsf: (Clsf)`; `DefaNoteA__IDA___DefaNote__IDA: (IDA) FOREIGN`; `IDA: (IDA)`; `Jurnale__CodJurnal___DefaNote__Jurnal: (Jurnal) FOREIGN`; `PlanCont__SimbolCont___DefaNote__ContCredit: (ContCredit) FOREIGN`; `PlanCont__SimbolCont___DefaNote__ContDebit: (ContDebit) FOREIGN`; `PrimaryKey: (ID) PRIMARY UNIQUE`

In scope - written by: nothing found

In scope - read by: `query:qDefaNote`, `form:EFACTURA_2025`

Also used by 37 object(s) outside the scope.

### `DefaNoteA`

Linked table. Source: `DefaNoteA`  Connect: `MS Access;PWD=***;DATABASE=C:\AVACONT\ENERGETIC LOCAL\baza2026.accdb`

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | IDA | Long | 4 | True |  |  |
| 1 | Explicatie | Text | 255 | False |  |  |
| 2 | Tip | Text | 255 | False |  |  |
| 3 | Jurnal | Text | 255 | False |  |  |
| 4 | Fel | Text | 255 | False |  |  |
| 5 | Compartiment | Text | 255 | False |  |  |
| 6 | CodPartener | Text | 255 | False |  |  |
| 7 | FelOP | Text | 255 | False |  |  |
| 8 | DTQ | Date/Time | 8 | False | Now() |  |

Indexes: `IDA: (IDA) PRIMARY UNIQUE`

In scope - written by: nothing found

In scope - read by: `query:qDefaNote`, `query:QEF_OPNU`, `form:EFACTURA_2025`

Also used by 36 object(s) outside the scope.

### `DefaSub`

Local table in Avacont.accdb.

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | ID | AutoNumber | 4 | False |  |  |
| 1 | SubCapitol | Text | 255 | False |  |  |
| 2 | Explicatie | Text | 255 | False |  |  |
| 3 | ExplicatieEdusal | Text | 255 | False |  |  |

Indexes: `PrimaryKey: (ID) PRIMARY UNIQUE`

In scope - written by: nothing found

In scope - read by: `form:EFACTURA_PickIBAN`

Also used by 6 object(s) outside the scope: `query:qClasificatii`, `query:qClsf2026`, `form:Burse_PickIBAN`, `form:OPuri_PickIBAN`, `module:mdl_ExportExcel`, `module:mdl_Import_EDUSAL`.

### `DefaTitlu2`

Linked table. Source: `DefaTitlu2`  Connect: `MS Access;PWD=***;DATABASE=C:\AVACONT\ENERGETIC LOCAL\baza2026.accdb`

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | ID | AutoNumber | 4 | False |  |  |
| 1 | Titlu | Text | 255 | False |  |  |
| 2 | Denumire | Text | 255 | False |  |  |

Indexes: `PrimaryKey: (ID) PRIMARY UNIQUE`

In scope - written by: nothing found

In scope - read by: `form:EFACTURA_PickIBAN`

Also used by 10 object(s) outside the scope.

### `Documente`

Linked table. Source: `Documente`  Connect: `MS Access;PWD=***;DATABASE=C:\AVACONT\ENERGETIC LOCAL\baza2026.accdb`

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | IdDocument | Long | 4 | True | 0 |  |
| 1 | IDSalarii | Long | 4 | False |  |  |
| 2 | NumarNC | Double | 8 | False |  |  |
| 3 | NumarDocument | Text | 255 | False |  |  |
| 4 | DataDocument | Date/Time | 8 | False |  |  |
| 6 | Auto | Yes/No | 1 | False |  |  |
| 7 | Fel | Text | 255 | False |  |  |
| 8 | Platit | Long | 4 | False |  |  |
| 9 | NRNIR | Text | 255 | False |  |  |
| 10 | EditManual | Yes/No | 1 | False |  |  |
| 11 | DTQ | Date/Time | 8 | False | Now() |  |
| 12 | FelDocument | Text | 255 | False |  |  |

Indexes: `Documente_FelDocument_847142: (FelDocument)`; `IdDocument: (IdDocument)`; `NoteCDocumente: (NumarNC) FOREIGN`; `NumarDocument: (NumarDocument)`; `NumarNC: (NumarNC)`; `PrimaryKey: (IdDocument) PRIMARY UNIQUE`

In scope - written by: nothing found

In scope - read by: `query:DetaliiEvidentiereFactura`, `query:DetaliiPlataFactura`, `query:QEF_OPER`, `query:QEF_PL`, `query:qPlatiParteneri2015`, `form:EFACTURA_2025`, `form:EF_F`, `form:EF_F_inlucru`, `form:EF_P`

Also used by 125 object(s) outside the scope.

### `EXPLOP`

Local table in Avacont.accdb.

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | ID | AutoNumber | 4 | False |  |  |
| 1 | CLSF | Text | 255 | False |  |  |
| 2 | EXPC | Text | 255 | False |  |  |
| 3 | DOCF | Text | 255 | False |  |  |
| 4 | DOCD | Text | 255 | False |  |  |
| 5 | DOCN | Text | 255 | False |  |  |
| 6 | EXPL | Text | 255 | False |  |  |
| 7 | FORMA | Text | 255 | False |  |  |

Indexes: `PrimaryKey: (ID) PRIMARY UNIQUE`

In scope - written by: `query:QEF_OPNU`

In scope - read by: nothing found

Also used by 1 object(s) outside the scope: `form:OPuri2017`.

### `IMG`

Local table in Avacont.accdb.

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | ID | AutoNumber | 4 | False |  |  |
| 1 | Pic | OLE Object | 0 | True |  |  |
| 2 | Expl | Text | 255 | False |  |  |
| 3 | S | Yes/No | 1 | False | No |  |
| 4 | Res | Long | 4 | False | 0 |  |
| 5 | FRM | Text | 255 | False |  |  |

Indexes: `Expl: (Expl)`; `ID1: (ID)`; `PrimaryKey: (ID) PRIMARY UNIQUE`

In scope - written by: nothing found

In scope - read by: `form:MF2019_Asoc_EF`, `form:Note_EFactura`, `form:_MF2019_Asoc_EF`, `module:mdl_EFactura`

Also used by 17 object(s) outside the scope.

### `Jud`

Local table in Avacont.accdb.

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | Nr | Long | 4 | False | 0 |  |
| 1 | Cod | Text | 255 | False |  |  |
| 2 | Judet | Text | 255 | False |  |  |

In scope - written by: nothing found

In scope - read by: `form:EFACTURA_CLIENTI`, `form:EFACTURA_VANZATOR`

Also used by 1 object(s) outside the scope: `form:UNIT subform`.

### `Oper`

Linked table. Source: `Oper`  Connect: `MS Access;PWD=***;DATABASE=C:\AVACONT\ENERGETIC LOCAL\baza2026.accdb`

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | IdOperatie | Long | 4 | True | 0 |  |
| 1 | IdDocument | Long | 4 | False | 0 |  |
| 2 | IDClsf | Long | 4 | False | 0 |  |
| 3 | IdSalarii | Long | 4 | False |  |  |
| 4 | IDX | Long | 4 | False |  |  |
| 5 | IDEFT | Long | 4 | False |  |  |
| 6 | IDA | Long | 4 | False | 0 |  |
| 7 | IDSS | Long | 4 | False | 0 |  |
| 8 | IDC | Long | 4 | False | 0 |  |
| 9 | DataOperatie | Date/Time | 8 | False |  |  |
| 10 | Explicatie | Text | 255 | False |  |  |
| 11 | CFPP | Double | 8 | False | 0 |  |
| 12 | CodJurnal | Text | 50 | True |  |  |
| 13 | Simpla | Double | 8 | False | 0 |  |
| 14 | Chelt | Yes/No | 1 | False |  |  |
| 15 | Ordonantare | Yes/No | 1 | False |  |  |
| 16 | Angajament | Yes/No | 1 | False |  |  |
| 17 | Propunere | Yes/No | 1 | False |  |  |
| 18 | NrANG | Integer | 2 | False |  |  |
| 19 | NrOPE | Integer | 2 | False |  |  |
| 20 | Comp | Text | 50 | False |  |  |
| 21 | Auto | Yes/No | 1 | False |  |  |
| 22 | Fel | Text | 255 | False |  |  |
| 23 | OP | Text | 255 | False |  |  |
| 24 | Platit | Yes/No | 1 | False |  |  |
| 25 | Tip | Text | 255 | False |  |  |
| 26 | AngajamentLegal | Yes/No | 1 | False |  |  |
| 27 | DataAngLeg | Date/Time | 8 | False |  |  |
| 28 | DTQ | Date/Time | 8 | False | Now() |  |
| 29 | IdBursa | Long | 4 | False |  |  |

Indexes: `Clasificatii__IDClsf___Oper__IDClsf: (IDClsf) FOREIGN`; `CodJurnal: (CodJurnal)`; `Documente__IdDocument___Oper__IdDocument: (IdDocument) FOREIGN`; `Explicatie: (Explicatie)`; `IDC: (IDC)`; `IDClsf: (IDClsf)`; `IdDocument: (IdDocument)`; `IDEFT: (IDEFT)`; `Jurnale__CodJurnal___Oper__CodJurnal: (CodJurnal) FOREIGN`; `Oper_IDA_659760: (IDA)`; `Oper_IDSS_84828: (IDSS)`; `PrimaryKey: (IdOperatie) PRIMARY UNIQUE`

In scope - written by: `form:EF_F`, `form:EF_F_inlucru`, `form:MF2019_Asoc_EF`, `form:_MF2019_Asoc_EF`

In scope - read by: `query:BalantaSelectFP`, `query:DetaliiEvidentiereFactura`, `query:DetaliiPlataFactura`, `query:EFACTURA_AA`, `query:QEF`, `query:QEF_DePlata`, `query:QEF_OPER`, `query:QEF_PL`, `query:QEF_SAV`, `query:qPlatiParteneri2015`

Also used by 186 object(s) outside the scope.

### `Parteneri`

Linked table. Source: `Parteneri`  Connect: `MS Access;PWD=***;DATABASE=C:\AVACONT\ENERGETIC LOCAL\baza2026.accdb`

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | CodPartener | Text | 50 | True |  |  |
| 1 | DenumirePartener | Text | 255 | False |  |  |
| 2 | CodFiscal | Text | 255 | False |  |  |
| 3 | ContIBAN | Text | 255 | False |  |  |
| 4 | Banca | Text | 255 | False |  |  |
| 5 | Adresa | Text | 255 | False |  |  |
| 6 | ContPl | Text | 255 | False |  |  |
| 7 | F8 | Text | 255 | False |  |  |
| 8 | F9 | Text | 255 | False |  |  |
| 9 | IDPART | Long | 4 | False |  |  |
| 10 | Tip | Text | 255 | False |  |  |
| 11 | CodClient | Text | 255 | False |  |  |
| 12 | DTQ | Date/Time | 8 | False | Now() |  |
| 13 | Ascuns | Yes/No | 1 | False | 0 |  |
| 14 | IdPartener | Long | 4 | False | 0 |  |
| 15 | Esinc | Yes/No | 1 | False | 0 |  |
| 16 | NumePartener | Text | 255 | False |  |  |

Indexes: `DenumirePartener: (DenumirePartener)`; `PrimaryKey: (CodPartener) PRIMARY UNIQUE`

In scope - written by: nothing found

In scope - read by: `query:BalantaSelectFP`, `query:DetaliiEvidentiereFactura`, `query:DetaliiPlataFactura`, `query:qDefaNote`, `query:QEF`, `query:QEF_ARH`, `query:QEF_DePlata`, `query:QEF_NOI`, `query:QEF_SAV`, `query:QEF_TRECERE`, `query:qMF_EFactura_Ramas`, `query:qMF_EFactura_Toate`, `query:qNote_EFactura_Toate`, `query:UEFP`, `query:_qNote_EFacturat_Ramas`, `form:EFACTURA_2025`, `module:mdl_EFactura`

Also used by 117 object(s) outside the scope.

### `ParteneriAng`

Linked table. Source: `ParteneriAng`  Connect: `MS Access;PWD=***;DATABASE=C:\AVACONT\ENERGETIC LOCAL\baza2026.accdb`

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | Id | AutoNumber | 4 | False |  |  |
| 1 | IdClsf | Long | 4 | False |  |  |
| 2 | CodPartener | Text | 255 | False |  |  |
| 3 | CodAng | Text | 11 | False |  |  |
| 4 | CodInd | Text | 4 | False |  |  |
| 5 | Clsf | Text | 255 | False |  |  |
| 6 | ContBanca | Text | 255 | False |  |  |
| 7 | DTQ | Date/Time | 8 | False | Now() |  |

Indexes: `Clasificatii__IDClsf___ParteneriAng__IdClsf: (IdClsf) FOREIGN`; `CodP: (CodPartener)`; `IdClsf: (IdClsf)`; `Parteneri__CodPartener___ParteneriAng__CodPartener: (CodPartener) FOREIGN`; `PrimaryKey: (Id) PRIMARY UNIQUE`

In scope - written by: nothing found

In scope - read by: `form:EFACTURA_2025`

Also used by 9 object(s) outside the scope.

### `ParteneriSI`

Linked table. Source: `ParteneriSI`  Connect: `MS Access;PWD=***;DATABASE=C:\AVACONT\ENERGETIC LOCAL\baza2026.accdb`

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | SimbolCont | Text | 50 | False |  |  |
| 1 | CodPartener | Text | 50 | False |  |  |
| 2 | SID | Double | 8 | False | 0 |  |
| 3 | SIC | Double | 8 | False | 0 |  |
| 4 | RPC | Double | 8 | False | 0 |  |
| 5 | RPD | Double | 8 | False | 0 |  |
| 6 | DTQ | Date/Time | 8 | False | Now() |  |
| 7 | Esinc | Yes/No | 1 | False | 0 |  |

Indexes: `CodPartener: (CodPartener)`; `Parteneri__CodPartener___ParteneriSI__CodPartener: (CodPartener) FOREIGN`; `PlanCont__SimbolCont___ParteneriSI__SimbolCont: (SimbolCont) FOREIGN`; `SID: (SID)`; `SimbolCont: (SimbolCont)`

In scope - written by: nothing found

In scope - read by: `query:BalantaSelectFP`

Also used by 18 object(s) outside the scope.

### `PlanCont`

Linked table. Source: `PlanCont`  Connect: `MS Access;PWD=***;DATABASE=C:\AVACONT\ENERGETIC LOCAL\baza2026.accdb`

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | SimbolCont | Text | 50 | True |  |  |
| 1 | Analitic | Text | 10 | False |  |  |
| 2 | DenumireCont | Text | 255 | False |  |  |
| 3 | FelCont | Text | 50 | False |  |  |
| 6 | RPC | Double | 8 | False | 0 |  |
| 7 | RPD | Double | 8 | False | 0 |  |
| 8 | Cont | Text | 50 | False |  |  |
| 9 | CodSector | Text | 255 | False |  |  |
| 10 | Sursa | Text | 255 | False |  |  |
| 11 | ClsfF | Text | 255 | False |  |  |
| 12 | ClsfE | Text | 255 | False |  |  |
| 13 | ContLung | Text | 50 | False |  |  |
| 14 | DTQ | Date/Time | 8 | False | Now() |  |
| 15 | Expr1 | Long | 4 | False | 0 |  |
| 16 | Extra | Yes/No | 1 | False |  |  |
| 17 | SIC | Currency | 8 | False |  |  |
| 18 | SID | Currency | 8 | False |  |  |
| 19 | TipCont | Text | 255 | False |  |  |

Indexes: `Denumirecont: (DenumireCont)`; `PlanCont_Expr1_283484: (Expr1)`; `PlanCont_SID_999021: (SID)`; `PrimaryKey: (SimbolCont) PRIMARY UNIQUE`

In scope - written by: nothing found

In scope - read by: `query:BalantaSelectFP`

Also used by 53 object(s) outside the scope.

### `PlatiFacturi`

Linked table. Source: `PlatiFacturi`  Connect: `MS Access;PWD=***;DATABASE=C:\AVACONT\ENERGETIC LOCAL\baza2026.accdb`

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | ID | AutoNumber | 4 | False |  |  |
| 1 | IdPlata | Long | 4 | False |  |  |
| 2 | IdFactura | Long | 4 | False |  |  |
| 3 | IdFacturaAA | Long | 4 | False |  |  |
| 4 | IDOP | Long | 4 | False | 0 |  |
| 5 | ValoarePlata | Double | 8 | False |  |  |
| 6 | ValoareFactura | Double | 8 | False |  |  |
| 7 | ValoareOP | Double | 8 | False |  |  |
| 8 | DataPlata | Date/Time | 8 | False |  |  |
| 9 | AlteDetalii | Memo/Long Text | 0 | False |  |  |
| 10 | OP | Long | 4 | False |  |  |
| 11 | Diferenta | Double | 8 | False |  |  |
| 12 | Numerar | Yes/No | 1 | False |  |  |
| 13 | Platit | Double | 8 | False |  |  |
| 14 | CodP | Text | 255 | False |  |  |
| 15 | DtDoc | Date/Time | 8 | False |  |  |
| 16 | NrAL | Double | 8 | False |  |  |
| 17 | NrDoc | Text | 255 | False |  |  |
| 18 | DTQ | Date/Time | 8 | False | Now() |  |
| 19 | IDEFT | Long | 4 | False | 0 |  |

Indexes: `IdFactura: (IdFactura)`; `IDOP: (IDOP)`; `IdPlata: (IdPlata)`; `OP: (OP)`; `Oper__IdOperatie___PlatiFacturi__IdPlata: (IdPlata) FOREIGN`; `Parteneri__CodPartener___PlatiFacturi__CodP: (CodP) FOREIGN`; `PlatiFacturi_IDEFT_115220: (IDEFT)`; `PrimaryKey: (ID) PRIMARY UNIQUE`

In scope - written by: nothing found

In scope - read by: `query:QEF`, `query:QEF_DePlata`, `query:QEF_OPER`, `query:QEF_PL`, `query:QEF_RestDePlata`, `query:QEF_RestDePlata_Data`, `query:QEF_SAV`, `query:qPlatiParteneri2015`

Also used by 50 object(s) outside the scope.

### `Rectificari`

Linked table. Source: `Rectificari`  Connect: `MS Access;PWD=***;DATABASE=C:\AVACONT\ENERGETIC LOCAL\baza2026.accdb`

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | ID | AutoNumber | 4 | False |  |  |
| 1 | IdClsf | Long | 4 | False |  |  |
| 2 | Capitol | Text | 255 | False |  |  |
| 3 | Subcapitol | Text | 255 | False |  |  |
| 4 | Articol | Text | 255 | False |  |  |
| 5 | Alineat | Text | 255 | False |  |  |
| 6 | Data | Date/Time | 8 | False |  |  |
| 7 | Document | Text | 255 | False |  |  |
| 8 | Trim1 | Double | 8 | False | 0 |  |
| 9 | Trim2 | Double | 8 | False | 0 |  |
| 10 | Trim3 | Double | 8 | False | 0 |  |
| 11 | Trim4 | Double | 8 | False | 0 |  |
| 12 | DTQ | Date/Time | 8 | False | Now() |  |
| 13 | Esinc | Yes/No | 1 | False | 0 |  |

Indexes: `Clasificatii__IDClsf___Rectificari__IdClsf: (IdClsf) FOREIGN`; `IdClsf: (IdClsf)`; `PrimaryKey: (ID) PRIMARY UNIQUE`

In scope - written by: nothing found

In scope - read by: `query:Q_1`

Also used by 20 object(s) outside the scope.

### `RectificariV`

Linked table. Source: `RectificariV`  Connect: `MS Access;PWD=***;DATABASE=C:\AVACONT\ENERGETIC LOCAL\baza2026.accdb`

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | IdClsfV | Long | 4 | False | 0 |  |
| 1 | ID | AutoNumber | 4 | False |  |  |
| 2 | Trim1 | Double | 8 | False | 0 |  |
| 3 | Trim2 | Double | 8 | False | 0 |  |
| 4 | Trim3 | Double | 8 | False | 0 |  |
| 5 | Trim4 | Double | 8 | False | 0 |  |
| 6 | Document | Text | 255 | False |  |  |
| 7 | Data | Date/Time | 8 | False |  |  |
| 8 | DTQ | Date/Time | 8 | False | Now() |  |

Indexes: `ClasificatiiV__IdClsfV___RectificariV__IdClsfV: (IdClsfV) FOREIGN`; `ID: (ID) UNIQUE`; `IdClsfV: (IdClsfV)`; `PrimaryKey: (ID) PRIMARY UNIQUE`

In scope - written by: nothing found

In scope - read by: `query:Q_1`

Also used by 3 object(s) outside the scope: `query:BugetVSelect`, `query:Cont_8090`, `form:RectificariV`.

### `Scheme`

Linked table. Source: `Scheme`  Connect: `MS Access;PWD=***;DATABASE=C:\AVACONT\ENERGETIC LOCAL\baza2026.accdb`

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | ID | AutoNumber | 4 | False |  |  |
| 1 | NumeForm | Text | 50 | False |  |  |
| 2 | C0 | Long | 4 | False | 0 |  |
| 3 | C1 | Long | 4 | False | 0 |  |
| 4 | C2 | Long | 4 | False | 0 |  |
| 5 | C3 | Long | 4 | False | 0 |  |
| 6 | C4 | Long | 4 | False | 0 |  |
| 7 | C5 | Long | 4 | False | 0 |  |
| 8 | C6 | Long | 4 | False | 0 |  |
| 9 | C7 | Long | 4 | False | 0 |  |
| 10 | C8 | Long | 4 | False | 0 |  |
| 11 | C9 | Long | 4 | False | 0 |  |
| 12 | T0 | Text | 255 | False |  |  |
| 13 | T1 | Text | 255 | False |  |  |
| 14 | T2 | Text | 255 | False |  |  |
| 15 | T3 | Text | 255 | False |  |  |

Indexes: `NumeForm: (NumeForm)`; `PrimaryKey: (ID) PRIMARY UNIQUE`

In scope - written by: nothing found

In scope - read by: `form:EFACTURA_ADD`, `module:mdl_EFactura`, `module:mdl_EFactura_Add`

Also used by 18 object(s) outside the scope.

### `TEMPF`

Local table in Avacont.accdb.

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | Serie | Text | 50 | False |  |  |
| 1 | Numar | Long | 4 | False | 0 |  |
| 2 | Data | Text | 50 | False |  |  |
| 3 | Continut | Text | 50 | False |  |  |
| 4 | UM | Text | 50 | False |  |  |
| 5 | Cant | Double | 8 | False | 0 |  |
| 6 | PU | Double | 8 | False | 0 |  |
| 7 | Valoare | Double | 8 | False | 0 |  |
| 8 | IDBeneficiar | Long | 4 | False | 0 |  |
| 9 | IDDelegat | Long | 4 | False | 0 |  |
| 10 | ID | AutoNumber | 4 | False |  |  |

Indexes: `ID: (ID)`; `IDBeneficiar: (IDBeneficiar)`; `IDDelegat: (IDDelegat)`; `Numar: (Numar)`

In scope - written by: `query:AddFacturaFF`

In scope - read by: nothing found

Also used by 0 object(s) outside the scope.

### `TmpBalantaListare`

Local table in Avacont.accdb.

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | Lvl | Double | 8 | False | 0 |  |
| 1 | IdUnitate | Long | 4 | False |  |  |
| 2 | CL | Text | 255 | False |  |  |
| 3 | Sintetic | Text | 255 | False |  |  |
| 4 | ContS | Text | 255 | False |  |  |
| 5 | Sursa | Text | 255 | False |  |  |
| 6 | CodSector | Text | 255 | False |  |  |
| 7 | SS | Text | 255 | False |  |  |
| 8 | ClsfF | Text | 255 | False |  |  |
| 9 | Titlu | Text | 255 | False |  |  |
| 10 | ClsfE | Text | 255 | False |  |  |
| 11 | Analitic | Text | 255 | False |  |  |
| 12 | CODP | Text | 255 | False |  |  |
| 13 | Cont | Text | 255 | False |  |  |
| 14 | Part | Text | 255 | False |  |  |
| 15 | NumePart | Text | 255 | False |  |  |
| 16 | NumeCont | Text | 255 | False |  |  |
| 17 | Nume | Text | 255 | False |  |  |
| 18 | SIID | Double | 8 | False |  |  |
| 19 | SIIC | Double | 8 | False |  |  |
| 20 | RULPD | Double | 8 | False |  |  |
| 21 | RULPC | Double | 8 | False |  |  |
| 22 | RULCD | Double | 8 | False |  |  |
| 23 | RULCC | Double | 8 | False |  |  |
| 24 | RULTD | Double | 8 | False |  |  |
| 25 | RULTC | Double | 8 | False |  |  |
| 26 | TOTSD | Double | 8 | False |  |  |
| 27 | TOTSC | Double | 8 | False |  |  |
| 28 | SFD | Double | 8 | False |  |  |
| 29 | SFC | Double | 8 | False |  |  |
| 30 | ContLung | Text | 255 | False |  |  |
| 31 | CSint | Long | 4 | False | 0 |  |
| 32 | Sort | Text | 255 | False |  |  |
| 33 | BackColor | Long | 4 | False | 0 |  |
| 34 | Bold | Yes/No | 1 | False | No |  |

Indexes: `1: (CL)`; `2: (Sintetic)`; `3: (Cont)`; `IdUnitate: (IdUnitate)`; `Nume: (Nume)`; `NumeCont: (NumeCont)`; `NumePart: (NumePart)`; `SIID: (SIID)`

In scope - written by: `query:BalantaListareFP`

In scope - read by: nothing found

Also used by 59 object(s) outside the scope.

### `TmpConturi`

Local table in Avacont.accdb.

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | ID | AutoNumber | 4 | False |  |  |
| 1 | CONT | Text | 50 | False |  |  |

Indexes: `CONT: (CONT) UNIQUE`; `ID: (ID)`; `PrimaryKey: (ID) PRIMARY UNIQUE`

In scope - written by: nothing found

In scope - read by: `query:BalantaTemporarFP`

Also used by 6 object(s) outside the scope: `query:Ext_BAT`, `query:Jurnale_Centralizator`, `query:Q_BalantaListare_2017`, `form:LRJ`, `report:Jurnale Separat`, `module:mdl_Balanta2020`.

### `UNIT`

Linked table. Source: `UNIT`  Connect: `MS Access;PWD=***;DATABASE=C:\AVACONT\ENERGETIC LOCAL\baza2026.accdb`

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | NumeUnitate | Text | 255 | False |  |  |
| 1 | Detalii | Text | 255 | False |  |  |
| 2 | CUI | Text | 50 | False |  |  |
| 3 | Adresa | Text | 255 | False |  |  |
| 4 | Director | Text | 255 | False |  |  |
| 5 | Contabil | Text | 255 | False |  |  |
| 8 | Culoare | Text | 255 | False |  |  |
| 9 | Expirat | Yes/No | 1 | False |  |  |
| 10 | Secretar | Text | 255 | False |  |  |
| 11 | SursaCont | Text | 255 | False |  |  |
| 12 | AdresaMail | Text | 255 | False |  |  |
| 13 | Orasul | Text | 255 | False |  |  |
| 14 | TelefonContact | Text | 255 | False |  |  |
| 15 | SectorSursa | Text | 255 | False |  |  |
| 16 | AnDate | Long | 4 | False | 0 |  |
| 17 | AlteDetalii | Text | 255 | False |  |  |
| 18 | DC | Text | 255 | False |  |  |
| 19 | CodProgram | Text | 255 | False | 0000000000 |  |
| 20 | Alte | Text | 255 | False |  |  |
| 21 | Gestionar | Text | 255 | False |  |  |
| 22 | Judetul | Text | 255 | False |  |  |

Indexes: `NumeUnitate: (NumeUnitate)`

In scope - written by: `form:EFACTURA_TMP`, `form:EFACTURA_VANZATOR`, `module:mdl_EFactura`

In scope - read by: `query:QEF_SAV`, `query:qFacturi_Vanzare`, `class:clsAnaf_Async`

Also used by 30 object(s) outside the scope.

### `_TempCredit`

Local table in Avacont.accdb.

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | ID | AutoNumber | 4 | False |  |  |
| 1 | IDOperatie | Long | 4 | False |  |  |
| 2 | SimbolCont | Text | 100 | False |  | Is Not Null |
| 3 | CodPartener | Text | 20 | False |  |  |
| 4 | SumaCredit | Currency | 8 | False |  |  |
| 5 | DenumireCont | Text | 255 | False |  |  |
| 6 | ContLung | Text | 255 | False |  |  |

Indexes: `ID: (ID)`; `IDOperatie: (IDOperatie)`; `PrimaryKey: (ID) PRIMARY UNIQUE`

In scope - written by: nothing found

In scope - read by: `form:Note_EFactura`, `form:_MF2019_Asoc_EF`

Also used by 6 object(s) outside the scope: `query:Note2020`, `query:SAV_NOTE_OP`, `form:Note`, `form:Note_Credit`, `form:Note_FactAsoc`, `module:mdl_Salvare_NoteContabile`.

### `dispozitii_sub`

Linked table. Source: `dispozitii_sub`  Connect: `MS Access;PWD=***;DATABASE=C:\AVACONT\ENERGETIC LOCAL\baza2026.accdb`

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | IDSubDisp | AutoNumber | 4 | False |  |  |
| 1 | Numar | Text | 255 | False |  |  |
| 2 | CCB | Text | 255 | False |  |  |
| 3 | CAP | Text | 255 | False |  |  |
| 4 | TIT | Text | 255 | False |  |  |
| 5 | Clsf | Text | 255 | False |  |  |
| 6 | Suma | Double | 8 | False |  |  |
| 7 | IDD | Long | 4 | False |  |  |

Indexes: `IDD: (IDD)`; `IDSubDisp: (IDSubDisp)`; `PrimaryKey: (IDSubDisp) PRIMARY UNIQUE`

In scope - written by: nothing found

In scope - read by: `query:Q_1`

Also used by 2 object(s) outside the scope: `query:q8XX1`, `form:Disp2020`.

### `tblFiles`

Local table in Avacont.accdb.

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | ID | AutoNumber | 4 | False |  |  |
| 1 | fileData | OLE Object | 0 | False |  |  |
| 2 | filePath | Text | 255 | False |  |  |
| 3 | fileName | Text | 255 | False |  |  |
| 4 | fileVersion | Double | 8 | False | 0 |  |
| 5 | locVersion | Long | 4 | False | 0 |  |
| 6 | chk | Yes/No | 1 | False | Yes |  |

Indexes: `ID: (ID)`; `PrimaryKey: (ID) PRIMARY UNIQUE`

In scope - written by: nothing found

In scope - read by: `module:mdl_EFactura`

Also used by 3 object(s) outside the scope: `form:frmFiles`, `module:mdl_Autoexec`, `module:mdl_DISK_RW`.

### `tmpConturiIBAN`

Local table in Avacont.accdb.

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | ID | AutoNumber | 4 | False |  |  |
| 1 | IdClsf | Long | 4 | False | 0 |  |
| 2 | Cont | Text | 255 | False |  |  |
| 3 | Capitol | Text | 255 | False |  |  |
| 4 | Subcapitol | Text | 255 | False |  |  |
| 5 | Articol | Text | 255 | False |  |  |
| 6 | Alineat | Text | 255 | False |  |  |
| 7 | SursaCont | Text | 255 | False |  |  |
| 8 | Denumire | Text | 255 | False |  |  |

Indexes: `ID: (ID)`; `IdClsf: (IdClsf)`; `PrimaryKey: (ID) PRIMARY UNIQUE`

In scope - written by: `form:EFACTURA_ADD`

In scope - read by: `form:EFACTURA_PickIBAN`

Also used by 4 object(s) outside the scope: `form:Burse`, `form:Burse_PickIBAN`, `form:OPuri`, `form:OPuri_PickIBAN`.

### `tmpDoc2017P`

Local table in Avacont.accdb.

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | ID | AutoNumber | 4 | False |  |  |
| 1 | IDO | Long | 4 | False |  |  |
| 2 | IDD | Long | 4 | False |  |  |
| 3 | IDOP | Long | 4 | False | 0 |  |
| 4 | IdClsf | Long | 4 | False | 0 |  |
| 5 | ClsfBug | Text | 255 | False |  |  |
| 6 | Forma | Text | 255 | False |  |  |
| 7 | NC | Long | 4 | False |  |  |
| 8 | Data | Date/Time | 8 | False |  |  |
| 9 | Document | Text | 255 | False |  |  |
| 10 | ExplicatieOP | Text | 255 | False |  |  |
| 11 | Valoare | Double | 8 | False |  |  |
| 12 | NrOpe | Long | 4 | False |  |  |
| 13 | ALG | Yes/No | 1 | False | No |  |
| 14 | DataALG | Text | 255 | False |  |  |
| 15 | SumaAlg | Double | 8 | False | 0 |  |
| 16 | NrAng | Text | 255 | False |  |  |
| 17 | AngLeg | Double | 8 | False | 0 |  |
| 18 | SV | Yes/No | 1 | False | Yes |  |
| 19 | CodPartener | Text | 255 | False |  |  |

Indexes: `ID: (ID)`; `IdClsf: (Forma)`; `IdClsf1: (IdClsf)`; `IDD: (IDD)`; `IDO: (IDO)`; `IDOP: (IDOP)`; `PrimaryKey: (ID) PRIMARY UNIQUE`

In scope - written by: `query:QEF_Plata`, `form:EFACTURA_2025`

In scope - read by: `form:EF_P`

Also used by 26 object(s) outside the scope.

### `tmpFacturi`

Local table in Avacont.accdb.

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | ID | AutoNumber | 4 | False |  |  |
| 1 | S | Yes/No | 1 | False | 0 |  |
| 2 | SV | Yes/No | 1 | False | No |  |
| 3 | S_Vechi | Yes/No | 1 | False | No |  |
| 4 | Nou | Yes/No | 1 | False | No |  |
| 5 | AnAnterior | Yes/No | 1 | False | No |  |
| 6 | IdDocument | Long | 4 | False |  |  |
| 7 | IdOperatie | Long | 4 | False |  |  |
| 8 | IdClsf | Long | 4 | False |  |  |
| 9 | IdPlata | Long | 4 | False |  |  |
| 10 | IDA | Long | 4 | False | 0 |  |
| 11 | ID8 | Long | 4 | False | 0 |  |
| 12 | IDPF | Long | 4 | False | 0 |  |
| 13 | IDEFT | Long | 4 | False | 0 |  |
| 14 | NC | Long | 4 | False | Null |  |
| 15 | Forma | Text | 255 | False |  |  |
| 16 | Clsf | Text | 255 | False |  |  |
| 17 | NumarDocument | Text | 50 | False |  |  |
| 18 | DataDocument | Date/Time | 8 | False |  |  |
| 19 | FelDocument | Text | 50 | False |  |  |
| 20 | Explicatie | Text | 255 | False |  |  |
| 21 | Fel | Text | 255 | False |  |  |
| 22 | Suma | Double | 8 | False |  |  |
| 23 | Platit | Double | 8 | False |  |  |
| 24 | PlatitAnterior | Double | 8 | False |  |  |
| 25 | SalvatAnterior | Double | 8 | False | 0 |  |
| 26 | ValoareFactura | Double | 8 | False | 0 |  |
| 27 | Diferenta | Double | 8 | False |  |  |
| 28 | Diferenta_temp | Double | 8 | False |  |  |
| 29 | NrAng | Long | 4 | False |  |  |
| 30 | ALG | Yes/No | 1 | False | No |  |
| 31 | DataALG | Date/Time | 8 | False |  |  |
| 32 | SumaALG | Double | 8 | False | 0 |  |
| 33 | ValInit | Double | 8 | False | Null |  |

Indexes: `ID: (ID8)`; `ID1: (ID)`; `IDA: (IDA)`; `IdClsf: (IdClsf)`; `IdDocument: (IdDocument)`; `IDEFT: (IDEFT)`; `IdOperatie: (IdOperatie)`; `IDPF: (IDPF)`; `IdPlata: (IdPlata)`; `NumarDocument: (NumarDocument)`; `PrimaryKey: (ID) PRIMARY UNIQUE`

In scope - written by: `query:QEF_EV`, `query:QEF_OPER`, `query:QEF_PL`, `form:EFACTURA_2025`

In scope - read by: `query:QEF_Plata`, `query:SAV_OPER_EF`, `query:SAV_OPER_EF_2017`, `form:EF_F`, `form:EF_F_inlucru`, `form:EF_P`

Also used by 47 object(s) outside the scope.

### `tmpQEF`

Local table in Avacont.accdb.

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | K | AutoNumber | 4 | False |  |  |
| 1 | IDEFT | Long | 4 | False | 0 |  |
| 2 | id_sol | Text | 255 | False |  |  |
| 3 | IdOperatie | Long | 4 | False | 0 |  |
| 4 | IDA | Long | 4 | False | 0 |  |
| 5 | IDClsf | Long | 4 | False |  |  |
| 6 | CP | Text | 255 | False |  |  |
| 7 | CF | Text | 255 | False |  |  |
| 8 | Furnizor | Text | 255 | False |  |  |
| 9 | Forma | Text | 50 | False |  |  |
| 10 | ClsfBug | Text | 255 | False |  |  |
| 11 | NrFact | Text | 255 | False |  |  |
| 12 | LunaAn | Text | 255 | False |  |  |
| 13 | LUNA | Integer | 2 | False |  |  |
| 14 | DataFact | Date/Time | 8 | False |  |  |
| 15 | TotalFactura | Double | 8 | False |  |  |
| 16 | TotalSalvat | Double | 8 | False |  |  |
| 17 | TotalPlatit | Double | 8 | False |  |  |
| 18 | TotalRamas | Long | 4 | False | 0 |  |
| 19 | DePlata | Double | 8 | False |  |  |
| 20 | DenClsf | Text | 255 | False |  |  |
| 21 | Articol | Text | 50 | False |  |  |
| 22 | Alineat | Text | 50 | False |  |  |
| 23 | AreOperatii | Yes/No | 1 | False | No |  |
| 24 | F | Yes/No | 1 | False | No |  |
| 25 | Tip | Text | 255 | False |  |  |
| 26 | S | Yes/No | 1 | False | No |  |
| 27 | DataIncarcareEF | Date/Time | 8 | False |  |  |

Indexes: `id_sol: (id_sol)`; `IDA: (IDA)`; `IDClsf: (IDClsf)`; `IDEFT: (IDEFT)`; `IdOperatie: (IdOperatie)`; `PrimaryKey: (K) PRIMARY UNIQUE`

In scope - written by: `query:QEF_ARH`, `query:QEF_NOI`, `query:QEF_SAV`, `query:QEF_TRECERE`, `form:EFACTURA_2025`

In scope - read by: `form:EFACTURA_MSG`

Also used by 4 object(s) outside the scope: `form:Parteneri`, `form:TrecereAN`, `report:EFACTURA_T`, `module:mdl_Popup2022`.

### `tmpXMLPath`

Local table in Avacont.accdb.

| # | Field | Type | Size | Required | Default | Validation |
|---|---|---|---|---|---|---|
| 0 | ID | AutoNumber | 4 | False |  |  |
| 1 | CodPartener | Text | 255 | False |  |  |
| 2 | NType | Text | 255 | False |  |  |
| 3 | NPath | Memo/Long Text | 0 | False |  |  |
| 4 | NValue | Text | 255 | False |  |  |
| 5 | S | Yes/No | 1 | False | No |  |

Indexes: `ID: (ID)`; `PrimaryKey: (ID) PRIMARY UNIQUE`

In scope - written by: `module:mdl_EFactura`

In scope - read by: nothing found

Also used by 2 object(s) outside the scope: `form:XMLPath`, `form:XMLPath_sub`.
