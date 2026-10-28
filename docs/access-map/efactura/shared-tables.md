# Shared tables used by the scope

Field rows use the [table-page conventions](README.md#table-page-conventions): only identical type, size, required flag, default and validation are grouped; each original ordinal and field name remains explicit.

These belong to the wider Avacont system (accounting, partners, classifications). The e-invoice screens read them; most are NOT part of what has to be rebuilt for e-invoice, but the columns the queries use must exist in K-BOT.

## Storage

Table storage follows the map below. The source table name is exactly the table heading. For linked tables, the original masked Connect string is `MS Access;PWD=***;DATABASE=<Database>`; local tables belong to Avacont.accdb.

| Kind | Database | Tables |
|---|---|---|
| Local | `Avacont.accdb` | `BIC`, `Balanta_Forexe`, `CALE`, `CONFIGS`, `DefaSub`, `EXPLOP`, `IMG`, `Jud`, `TEMPF`, `TmpBalantaListare`, `TmpConturi`, `_TempCredit`, `tblFiles`, `tmpConturiIBAN`, `tmpDoc2017P`, `tmpFacturi`, `tmpQEF`, `tmpXMLPath` |
| Linked | `C:\AVACONT\ENERGETIC LOCAL\baza2026.accdb` | `CT8030`, `Clasificatii`, `ClasificatiiV`, `Cont8067`, `Contracte`, `Contracte_A`, `Credit`, `Debit`, `DefaCont`, `DefaNote`, `DefaNoteA`, `DefaTitlu2`, `Documente`, `Oper`, `Parteneri`, `ParteneriAng`, `ParteneriSI`, `PlanCont`, `PlatiFacturi`, `Rectificari`, `RectificariV`, `Scheme`, `UNIT`, `dispozitii_sub` |

### `BIC`

- Text · 255 — 0: Cod<br>1: Banca<br>2: SWIFT

In scope - written by: nothing found · In scope - read by: `form:EFACTURA_CLIENTI`, `module:mdl_EFactura_Add` · Also used by 4 object(s) outside the scope: `form:Burse`, `form:OPuri`, `form:Parteneri`, `module:mdl_Popup2022`.

### `Balanta_Forexe`

- Text · 255 — 0: Cont · Required=True
- Text · 255 — 1: SIMBOL_CONT<br>2: COD_SURSA<br>3: COD_SECTOR<br>4: COD_FUNCTIONAL<br>5: COD_ECONOMIC<br>6: DENUMIRE_CONT<br>17: DENUMIRE_SURSA<br>18: DENUMIRE_SECTOR<br>19: DENUMIRE_INDICATOR_CF<br>20: DENUMIRE_INDICATOR_CE
- Double · 8 — 7: SI_DEBIT<br>8: SI_CREDIT<br>9: RLC_DEBIT<br>10: RLC_CREDIT<br>11: TR_DEBIT<br>12: TR_CREDIT<br>13: TS_DEBIT<br>14: TS_CREDIT<br>15: SF_DEBIT<br>16: SF_CREDIT

Indexes: `COD_ECO: (COD_ECONOMIC)`; `COD_FUN: (COD_FUNCTIONAL)`; `COD_SECTOR: (COD_SECTOR)`; `COD_SURSA: (COD_SURSA)`; `CONT: (Cont)`; `PrimaryKey: (Cont) PRIMARY UNIQUE`; `SIMBOL_CONT: (SIMBOL_CONT)`

In scope - written by: `module:mdl_EFactura` · In scope - read by: nothing found · Also used by 2 object(s) outside the scope: `query:Balanta_Compare`, `module:mdl_ExportExcel`.

### `CALE`

- AutoNumber · 4 — 0: ID
- Double · 8 — 1: IdUnitate · Default=0
- Text · 255 — 2: CALE<br>3: CALEEF<br>4: NumeUnitate<br>5: Detalii<br>6: CaleRealaEF<br>7: CaleReala<br>8: CU<br>10: DC<br>12: CDeschid<br>13: Usr<br>17: CaleRealaGest<br>18: Sursa<br>19: CodFiscal<br>23: UnitatiSclav
- Long · 4 — 9: ANL
- Yes/No · 1 — 11: Blocat<br>14: FaraUpdate<br>15: Expirat · Default=0
- Yes/No · 1 — 16: Gest<br>20: SclavBuget<br>22: MasterBuget · Default=No
- Long · 4 — 21: IdUnitateParinte · Default=0

Indexes: `CaleReala: (CaleReala)`; `CDeschid: (CDeschid)`; `CodFiscal: (CodFiscal)`; `IdUnitate: (IdUnitate)`; `IdUnitateParinte: (IdUnitateParinte)`; `IdUnitateSclav: (UnitatiSclav)`; `NumeUnitate: (NumeUnitate)`; `PrimaryKey: (ID) PRIMARY UNIQUE`; `Sursa: (Sursa)`

In scope - written by: nothing found · In scope - read by: `query:EFACTURA_AA`, `query:QEF`, `query:QEF_ARH`, `query:QEF_EV`, `query:QEF_EV_inlucru`, `query:QEF_NOI`, `query:QEF_PL`, `query:QEF_RestDePlata`, `query:QEF_RestDePlata_Data`, `query:QEF_SAV`, `query:QEF_TRECERE`, `form:EFACTURA_AA`, `module:mdl_EFactura`, `class:clsAnaf_Async` · Also used by 49 object(s) outside the scope.

### `CONFIGS`

- AutoNumber · 4 — 0: ID
- Text · 255 — 1: EXPL
- Memo/Long Text · 0 — 2: INFO

Indexes: `ID: (ID)`; `PrimaryKey: (ID) PRIMARY UNIQUE`

In scope - written by: nothing found · In scope - read by: `module:mdl_EFactura`, `class:clsAnaf_Async` · Also used by 0 object(s) outside the scope.

### `CT8030`

- AutoNumber · 4 — 0: ID
- Text · 255 — 1: NumarDocument<br>6: DenumireDocument<br>7: Cont<br>8: Sursa
- Date/Time · 8 — 2: DataDocument<br>3: DataOperatie
- Double · 8 — 4: SumaDebit<br>5: SumaCredit

Indexes: `NumarDocument: (NumarDocument)`; `PrimaryKey: (ID) PRIMARY UNIQUE`

In scope - written by: nothing found · In scope - read by: `query:Q_7` · Also used by 5 object(s) outside the scope: `query:Cont_8030`, `query:qCT8030`, `form:subCT8030`, `report:FisaCT8030`, `module:basRibbonCallbacks`.

### `Clasificatii`

- Long · 4 — 0: IDClsf · Required=True · Default=0
- Text · 50 — 1: Capitol<br>2: Subcapitol<br>3: Articol<br>4: Alineat
- Double · 8 — 6: TOTAL<br>7: Trim1<br>8: Trim2<br>9: Trim3<br>10: Trim4<br>28: TOTALFX · Default=0
- Date/Time · 8 — 11: DTQ · Default=Now()
- Text · 255 — 12: Document<br>14: Denumire
- Date/Time · 8 — 13: Data
- Long · 4 — 15: IdLegatura
- Text · 243 — 16: Clsf<br>17: Titlu<br>18: Sector<br>20: ClsfSal<br>21: ClsfF<br>22: ClsfE<br>24: ClsfX<br>31: Sursa<br>32: SS<br>33: CodSSI
- Text · 10 — 25: CodAng
- Text · 3 — 26: CodInd
- Long · 4 — 29: IdClsfPY · Default=0
- Yes/No · 1 — 30: Esinc · Default=0

Indexes: `C: (Capitol)`; `L: (Alineat)`; `PrimaryKey: (IDClsf) PRIMARY UNIQUE`; `R: (Articol)`; `S: (Subcapitol)`

In scope - written by: `form:EFACTURA_ADD` · In scope - read by: `query:qClsfE`, `query:qDefaNote`, `query:QEF_OPER`, `query:QEF_PL`, `query:QEF_SAV`, `query:Q_2`, `form:EFACTURA_2025` · Also used by 146 object(s) outside the scope.

### `ClasificatiiV`

- Long · 4 — 0: IdClsfV · Required=True · Default=0
- Text · 255 — 1: Capitol<br>2: SubCapitol<br>3: Paragraf<br>8: Denumire
- Long · 4 — 4: Trim1<br>5: Trim2<br>6: Trim3<br>7: Trim4 · Default=0

Indexes: `IdClsfV: (IdClsfV)`; `PrimaryKey: (IdClsfV) PRIMARY UNIQUE`

In scope - written by: nothing found · In scope - read by: `query:qCLSFV`, `query:Q_1`, `query:Q_13` · Also used by 7 object(s) outside the scope: `query:BugetVSelect`, `query:Cont_8090`, `query:qClsfV2025`, `form:AddClsf_Venituri`, `form:ClasificatiiBugetareV`, `form:ClasificatiiV`, `form:RectificariV`.

### `Cont8067`

- AutoNumber · 4 — 0: ID8
- Long · 4 — 1: IDO · Default=0
- Date/Time · 8 — 2: DataO<br>3: DataAlg
- Double · 8 — 4: SumaAlg · Default=0
- Text · 255 — 5: CodP
- Double · 8 — 6: Contract
- Long · 4 — 7: IdClsf<br>8: ContractA
- Date/Time · 8 — 9: DTQ · Default=Now()

Indexes: `Clasificatii__IDClsf___Cont8067__IdClsf: (IdClsf) FOREIGN`; `Contracte__NrCrt___Cont8067__Contract: (Contract) FOREIGN`; `Contracte_A__NrCrtA___Cont8067__ContractA: (ContractA) FOREIGN`; `ID8: (ID8)`; `IDO: (IDO)`; `Oper__IdOperatie___Cont8067__IDO: (IDO) FOREIGN`; `PrimaryKey: (ID8) PRIMARY UNIQUE`

In scope - written by: `form:EF_F`, `form:EF_F_inlucru` · In scope - read by: `query:QEF_OPER`, `query:QEF_PL` · Also used by 51 object(s) outside the scope.

### `Contracte`

- AutoNumber · 4 — 0: ID
- Double · 8 — 1: NrCrt · Required=True
- Long · 4 — 2: IdClsf
- Text · 255 — 3: Denumire<br>4: NrDoc<br>6: Explicatie<br>8: CodAngajament<br>9: CodIndicator<br>10: CodPartener
- Date/Time · 8 — 5: Data
- Double · 8 — 7: Valoare · Default=0
- Date/Time · 8 — 11: DTQ · Default=Now()

Indexes: `Clasificatii__IDClsf___Contracte__IdClsf: (IdClsf) FOREIGN`; `idclsf: (IdClsf)`; `NrCrt: (NrCrt) PRIMARY UNIQUE`

In scope - written by: nothing found · In scope - read by: `query:Q_8`, `form:EFACTURA_2025` · Also used by 18 object(s) outside the scope.

### `Contracte_A`

- AutoNumber · 4 — 0: ID
- Double · 8 — 1: NrCrt<br>2: Ajustare
- Date/Time · 8 — 3: DataAjustare
- Long · 4 — 4: NrCrtA

Indexes: `Contracte__NrCrt___Contracte_A__NrCrt: (NrCrt) FOREIGN`; `NrCrtA: (NrCrtA) PRIMARY UNIQUE`

In scope - written by: nothing found · In scope - read by: `query:Q_8` · Also used by 7 object(s) outside the scope: `query:8067`, `query:Cont8067_Q`, `query:Cont8067_X`, `query:Cont8067_Z`, `query:qContracte`, `form:Contracte`, `form:Contracte_a`.

### `Credit`

- Long · 4 — 0: IdOperatie · Default=0
- Text · 50 — 1: SimbolCont<br>2: CodPartener
- Double · 8 — 3: SumaCredit · Default=0

Indexes: `CodPartener: (CodPartener)`; `IdOperatie: (IdOperatie)`; `Oper__IdOperatie___Credit__IdOperatie: (IdOperatie) FOREIGN`; `Parteneri__CodPartener___Credit__CodPartener: (CodPartener) FOREIGN`; `PlanCont__SimbolCont___Credit__SimbolCont: (SimbolCont) FOREIGN`; `SimbolCont: (SimbolCont)`; `SumaCredit: (SumaCredit)`

In scope - written by: nothing found · In scope - read by: `query:BalantaSelectFP`, `query:DetaliiEvidentiereFactura`, `query:QEF`, `query:QEF_OPER`, `query:qPlatiParteneri2015` · Also used by 135 object(s) outside the scope.

### `Debit`

- Long · 4 — 0: IdOperatie · Default=0
- Text · 50 — 1: SimbolCont<br>2: CodPartener
- Double · 8 — 3: SumaDebit · Default=0

Indexes: `CodPartener: (CodPartener)`; `IdOperatie: (IdOperatie)`; `Oper__IdOperatie___Debit__IdOperatie: (IdOperatie) FOREIGN`; `Parteneri__CodPartener___Debit__CodPartener: (CodPartener) FOREIGN`; `PlanCont__SimbolCont___Debit__SimbolCont: (SimbolCont) FOREIGN`; `SimbolCont: (SimbolCont)`; `SumaDebit: (SumaDebit)`

In scope - written by: nothing found · In scope - read by: `query:BalantaSelectFP`, `query:DetaliiPlataFactura` · Also used by 110 object(s) outside the scope.

### `DefaCont`

- Text · 255 — 0: Cont · Required=True
- Text · 255 — 1: Denumire
- Text · 1 — 2: Tip
- Yes/No · 1 — 3: Sector<br>4: Sursa<br>5: ClsfF<br>6: ClsfE<br>7: Vizibil · Default=0

Indexes: `PrimaryKey: (Cont) PRIMARY UNIQUE`

In scope - written by: nothing found · In scope - read by: `query:BalantaSelectFP`, `query:Find duplicates for DefaCont`, `query:qDefaCont_Bilant` · Also used by 27 object(s) outside the scope.

### `DefaNote`

- Long · 4 — 0: ID · Required=True
- Long · 4 — 1: IDA<br>3: Tip<br>4: Clsf<br>14: Simpla
- Text · 255 — 2: Forma<br>5: Jurnal<br>6: Explicatie<br>7: ContDebit<br>8: PartDebit<br>9: ContCredit<br>10: PartCredit<br>15: Fel
- Yes/No · 1 — 11: Ordonantare<br>12: PA<br>13: Chelt
- Date/Time · 8 — 16: DTQ · Default=Now()

Indexes: `Clasificatii__IDClsf___DefaNote__Clsf: (Clsf) FOREIGN`; `Clsf: (Clsf)`; `DefaNoteA__IDA___DefaNote__IDA: (IDA) FOREIGN`; `IDA: (IDA)`; `Jurnale__CodJurnal___DefaNote__Jurnal: (Jurnal) FOREIGN`; `PlanCont__SimbolCont___DefaNote__ContCredit: (ContCredit) FOREIGN`; `PlanCont__SimbolCont___DefaNote__ContDebit: (ContDebit) FOREIGN`; `PrimaryKey: (ID) PRIMARY UNIQUE`

In scope - written by: nothing found · In scope - read by: `query:qDefaNote`, `form:EFACTURA_2025` · Also used by 37 object(s) outside the scope.

### `DefaNoteA`

- Long · 4 — 0: IDA · Required=True
- Text · 255 — 1: Explicatie<br>2: Tip<br>3: Jurnal<br>4: Fel<br>5: Compartiment<br>6: CodPartener<br>7: FelOP
- Date/Time · 8 — 8: DTQ · Default=Now()

Indexes: `IDA: (IDA) PRIMARY UNIQUE`

In scope - written by: nothing found · In scope - read by: `query:qDefaNote`, `query:QEF_OPNU`, `form:EFACTURA_2025` · Also used by 36 object(s) outside the scope.

### `DefaSub`

- AutoNumber · 4 — 0: ID
- Text · 255 — 1: SubCapitol<br>2: Explicatie<br>3: ExplicatieEdusal

Indexes: `PrimaryKey: (ID) PRIMARY UNIQUE`

In scope - written by: nothing found · In scope - read by: `form:EFACTURA_PickIBAN` · Also used by 6 object(s) outside the scope: `query:qClasificatii`, `query:qClsf2026`, `form:Burse_PickIBAN`, `form:OPuri_PickIBAN`, `module:mdl_ExportExcel`, `module:mdl_Import_EDUSAL`.

### `DefaTitlu2`

- AutoNumber · 4 — 0: ID
- Text · 255 — 1: Titlu<br>2: Denumire

Indexes: `PrimaryKey: (ID) PRIMARY UNIQUE`

In scope - written by: nothing found · In scope - read by: `form:EFACTURA_PickIBAN` · Also used by 10 object(s) outside the scope.

### `Documente`

- Long · 4 — 0: IdDocument · Required=True · Default=0
- Long · 4 — 1: IDSalarii<br>8: Platit
- Double · 8 — 2: NumarNC
- Text · 255 — 3: NumarDocument<br>7: Fel<br>9: NRNIR<br>12: FelDocument
- Date/Time · 8 — 4: DataDocument
- Yes/No · 1 — 6: Auto<br>10: EditManual
- Date/Time · 8 — 11: DTQ · Default=Now()

Indexes: `Documente_FelDocument_847142: (FelDocument)`; `IdDocument: (IdDocument)`; `NoteCDocumente: (NumarNC) FOREIGN`; `NumarDocument: (NumarDocument)`; `NumarNC: (NumarNC)`; `PrimaryKey: (IdDocument) PRIMARY UNIQUE`

In scope - written by: nothing found · In scope - read by: `query:DetaliiEvidentiereFactura`, `query:DetaliiPlataFactura`, `query:QEF_OPER`, `query:QEF_PL`, `query:qPlatiParteneri2015`, `form:EFACTURA_2025`, `form:EF_F`, `form:EF_F_inlucru`, `form:EF_P` · Also used by 125 object(s) outside the scope.

### `EXPLOP`

- AutoNumber · 4 — 0: ID
- Text · 255 — 1: CLSF<br>2: EXPC<br>3: DOCF<br>4: DOCD<br>5: DOCN<br>6: EXPL<br>7: FORMA

Indexes: `PrimaryKey: (ID) PRIMARY UNIQUE`

In scope - written by: `query:QEF_OPNU` · In scope - read by: nothing found · Also used by 1 object(s) outside the scope: `form:OPuri2017`.

### `IMG`

- AutoNumber · 4 — 0: ID
- OLE Object · 0 — 1: Pic · Required=True
- Text · 255 — 2: Expl<br>5: FRM
- Yes/No · 1 — 3: S · Default=No
- Long · 4 — 4: Res · Default=0

Indexes: `Expl: (Expl)`; `ID1: (ID)`; `PrimaryKey: (ID) PRIMARY UNIQUE`

In scope - written by: nothing found · In scope - read by: `form:MF2019_Asoc_EF`, `form:Note_EFactura`, `form:_MF2019_Asoc_EF`, `module:mdl_EFactura` · Also used by 17 object(s) outside the scope.

### `Jud`

- Long · 4 — 0: Nr · Default=0
- Text · 255 — 1: Cod<br>2: Judet

In scope - written by: nothing found · In scope - read by: `form:EFACTURA_CLIENTI`, `form:EFACTURA_VANZATOR` · Also used by 1 object(s) outside the scope: `form:UNIT subform`.

### `Oper`

- Long · 4 — 0: IdOperatie · Required=True · Default=0
- Long · 4 — 1: IdDocument<br>2: IDClsf<br>6: IDA<br>7: IDSS<br>8: IDC · Default=0
- Long · 4 — 3: IdSalarii<br>4: IDX<br>5: IDEFT<br>29: IdBursa
- Date/Time · 8 — 9: DataOperatie<br>27: DataAngLeg
- Text · 255 — 10: Explicatie<br>22: Fel<br>23: OP<br>25: Tip
- Double · 8 — 11: CFPP<br>13: Simpla · Default=0
- Text · 50 — 12: CodJurnal · Required=True
- Yes/No · 1 — 14: Chelt<br>15: Ordonantare<br>16: Angajament<br>17: Propunere<br>21: Auto<br>24: Platit<br>26: AngajamentLegal
- Integer · 2 — 18: NrANG<br>19: NrOPE
- Text · 50 — 20: Comp
- Date/Time · 8 — 28: DTQ · Default=Now()

Indexes: `Clasificatii__IDClsf___Oper__IDClsf: (IDClsf) FOREIGN`; `CodJurnal: (CodJurnal)`; `Documente__IdDocument___Oper__IdDocument: (IdDocument) FOREIGN`; `Explicatie: (Explicatie)`; `IDC: (IDC)`; `IDClsf: (IDClsf)`; `IdDocument: (IdDocument)`; `IDEFT: (IDEFT)`; `Jurnale__CodJurnal___Oper__CodJurnal: (CodJurnal) FOREIGN`; `Oper_IDA_659760: (IDA)`; `Oper_IDSS_84828: (IDSS)`; `PrimaryKey: (IdOperatie) PRIMARY UNIQUE`

In scope - written by: `form:EF_F`, `form:EF_F_inlucru`, `form:MF2019_Asoc_EF`, `form:_MF2019_Asoc_EF` · In scope - read by: `query:BalantaSelectFP`, `query:DetaliiEvidentiereFactura`, `query:DetaliiPlataFactura`, `query:EFACTURA_AA`, `query:QEF`, `query:QEF_DePlata`, `query:QEF_OPER`, `query:QEF_PL`, `query:QEF_SAV`, `query:qPlatiParteneri2015` · Also used by 186 object(s) outside the scope.

### `Parteneri`

- Text · 50 — 0: CodPartener · Required=True
- Text · 255 — 1: DenumirePartener<br>2: CodFiscal<br>3: ContIBAN<br>4: Banca<br>5: Adresa<br>6: ContPl<br>7: F8<br>8: F9<br>10: Tip<br>11: CodClient<br>16: NumePartener
- Long · 4 — 9: IDPART
- Date/Time · 8 — 12: DTQ · Default=Now()
- Yes/No · 1 — 13: Ascuns<br>15: Esinc · Default=0
- Long · 4 — 14: IdPartener · Default=0

Indexes: `DenumirePartener: (DenumirePartener)`; `PrimaryKey: (CodPartener) PRIMARY UNIQUE`

In scope - written by: nothing found · In scope - read by: `query:BalantaSelectFP`, `query:DetaliiEvidentiereFactura`, `query:DetaliiPlataFactura`, `query:qDefaNote`, `query:QEF`, `query:QEF_ARH`, `query:QEF_DePlata`, `query:QEF_NOI`, `query:QEF_SAV`, `query:QEF_TRECERE`, `query:qMF_EFactura_Ramas`, `query:qMF_EFactura_Toate`, `query:qNote_EFactura_Toate`, `query:UEFP`, `query:_qNote_EFacturat_Ramas`, `form:EFACTURA_2025`, `module:mdl_EFactura` · Also used by 117 object(s) outside the scope.

### `ParteneriAng`

- AutoNumber · 4 — 0: Id
- Long · 4 — 1: IdClsf
- Text · 255 — 2: CodPartener<br>5: Clsf<br>6: ContBanca
- Text · 11 — 3: CodAng
- Text · 4 — 4: CodInd
- Date/Time · 8 — 7: DTQ · Default=Now()

Indexes: `Clasificatii__IDClsf___ParteneriAng__IdClsf: (IdClsf) FOREIGN`; `CodP: (CodPartener)`; `IdClsf: (IdClsf)`; `Parteneri__CodPartener___ParteneriAng__CodPartener: (CodPartener) FOREIGN`; `PrimaryKey: (Id) PRIMARY UNIQUE`

In scope - written by: nothing found · In scope - read by: `form:EFACTURA_2025` · Also used by 9 object(s) outside the scope.

### `ParteneriSI`

- Text · 50 — 0: SimbolCont<br>1: CodPartener
- Double · 8 — 2: SID<br>3: SIC<br>4: RPC<br>5: RPD · Default=0
- Date/Time · 8 — 6: DTQ · Default=Now()
- Yes/No · 1 — 7: Esinc · Default=0

Indexes: `CodPartener: (CodPartener)`; `Parteneri__CodPartener___ParteneriSI__CodPartener: (CodPartener) FOREIGN`; `PlanCont__SimbolCont___ParteneriSI__SimbolCont: (SimbolCont) FOREIGN`; `SID: (SID)`; `SimbolCont: (SimbolCont)`

In scope - written by: nothing found · In scope - read by: `query:BalantaSelectFP` · Also used by 18 object(s) outside the scope.

### `PlanCont`

- Text · 50 — 0: SimbolCont · Required=True
- Text · 10 — 1: Analitic
- Text · 255 — 2: DenumireCont<br>9: CodSector<br>10: Sursa<br>11: ClsfF<br>12: ClsfE<br>19: TipCont
- Text · 50 — 3: FelCont<br>8: Cont<br>13: ContLung
- Double · 8 — 6: RPC<br>7: RPD · Default=0
- Date/Time · 8 — 14: DTQ · Default=Now()
- Long · 4 — 15: Expr1 · Default=0
- Yes/No · 1 — 16: Extra
- Currency · 8 — 17: SIC<br>18: SID

Indexes: `Denumirecont: (DenumireCont)`; `PlanCont_Expr1_283484: (Expr1)`; `PlanCont_SID_999021: (SID)`; `PrimaryKey: (SimbolCont) PRIMARY UNIQUE`

In scope - written by: nothing found · In scope - read by: `query:BalantaSelectFP` · Also used by 53 object(s) outside the scope.

### `PlatiFacturi`

- AutoNumber · 4 — 0: ID
- Long · 4 — 1: IdPlata<br>2: IdFactura<br>3: IdFacturaAA<br>10: OP
- Long · 4 — 4: IDOP<br>19: IDEFT · Default=0
- Double · 8 — 5: ValoarePlata<br>6: ValoareFactura<br>7: ValoareOP<br>11: Diferenta<br>13: Platit<br>16: NrAL
- Date/Time · 8 — 8: DataPlata<br>15: DtDoc
- Memo/Long Text · 0 — 9: AlteDetalii
- Yes/No · 1 — 12: Numerar
- Text · 255 — 14: CodP<br>17: NrDoc
- Date/Time · 8 — 18: DTQ · Default=Now()

Indexes: `IdFactura: (IdFactura)`; `IDOP: (IDOP)`; `IdPlata: (IdPlata)`; `OP: (OP)`; `Oper__IdOperatie___PlatiFacturi__IdPlata: (IdPlata) FOREIGN`; `Parteneri__CodPartener___PlatiFacturi__CodP: (CodP) FOREIGN`; `PlatiFacturi_IDEFT_115220: (IDEFT)`; `PrimaryKey: (ID) PRIMARY UNIQUE`

In scope - written by: nothing found · In scope - read by: `query:QEF`, `query:QEF_DePlata`, `query:QEF_OPER`, `query:QEF_PL`, `query:QEF_RestDePlata`, `query:QEF_RestDePlata_Data`, `query:QEF_SAV`, `query:qPlatiParteneri2015` · Also used by 50 object(s) outside the scope.

### `Rectificari`

- AutoNumber · 4 — 0: ID
- Long · 4 — 1: IdClsf
- Text · 255 — 2: Capitol<br>3: Subcapitol<br>4: Articol<br>5: Alineat<br>7: Document
- Date/Time · 8 — 6: Data
- Double · 8 — 8: Trim1<br>9: Trim2<br>10: Trim3<br>11: Trim4 · Default=0
- Date/Time · 8 — 12: DTQ · Default=Now()
- Yes/No · 1 — 13: Esinc · Default=0

Indexes: `Clasificatii__IDClsf___Rectificari__IdClsf: (IdClsf) FOREIGN`; `IdClsf: (IdClsf)`; `PrimaryKey: (ID) PRIMARY UNIQUE`

In scope - written by: nothing found · In scope - read by: `query:Q_1` · Also used by 20 object(s) outside the scope.

### `RectificariV`

- Long · 4 — 0: IdClsfV · Default=0
- AutoNumber · 4 — 1: ID
- Double · 8 — 2: Trim1<br>3: Trim2<br>4: Trim3<br>5: Trim4 · Default=0
- Text · 255 — 6: Document
- Date/Time · 8 — 7: Data
- Date/Time · 8 — 8: DTQ · Default=Now()

Indexes: `ClasificatiiV__IdClsfV___RectificariV__IdClsfV: (IdClsfV) FOREIGN`; `ID: (ID) UNIQUE`; `IdClsfV: (IdClsfV)`; `PrimaryKey: (ID) PRIMARY UNIQUE`

In scope - written by: nothing found · In scope - read by: `query:Q_1` · Also used by 3 object(s) outside the scope: `query:BugetVSelect`, `query:Cont_8090`, `form:RectificariV`.

### `Scheme`

- AutoNumber · 4 — 0: ID
- Text · 50 — 1: NumeForm
- Long · 4 — 2: C0<br>3: C1<br>4: C2<br>5: C3<br>6: C4<br>7: C5<br>8: C6<br>9: C7<br>10: C8<br>11: C9 · Default=0
- Text · 255 — 12: T0<br>13: T1<br>14: T2<br>15: T3

Indexes: `NumeForm: (NumeForm)`; `PrimaryKey: (ID) PRIMARY UNIQUE`

In scope - written by: nothing found · In scope - read by: `form:EFACTURA_ADD`, `module:mdl_EFactura`, `module:mdl_EFactura_Add` · Also used by 18 object(s) outside the scope.

### `TEMPF`

- Text · 50 — 0: Serie<br>2: Data<br>3: Continut<br>4: UM
- Long · 4 — 1: Numar<br>8: IDBeneficiar<br>9: IDDelegat · Default=0
- Double · 8 — 5: Cant<br>6: PU<br>7: Valoare · Default=0
- AutoNumber · 4 — 10: ID

Indexes: `ID: (ID)`; `IDBeneficiar: (IDBeneficiar)`; `IDDelegat: (IDDelegat)`; `Numar: (Numar)`

In scope - written by: `query:AddFacturaFF` · In scope - read by: nothing found · Also used by 0 object(s) outside the scope.

### `TmpBalantaListare`

- Double · 8 — 0: Lvl · Default=0
- Long · 4 — 1: IdUnitate
- Text · 255 — 2: CL<br>3: Sintetic<br>4: ContS<br>5: Sursa<br>6: CodSector<br>7: SS<br>8: ClsfF<br>9: Titlu<br>10: ClsfE<br>11: Analitic<br>12: CODP<br>13: Cont<br>14: Part<br>15: NumePart<br>16: NumeCont<br>17: Nume<br>30: ContLung<br>32: Sort
- Double · 8 — 18: SIID<br>19: SIIC<br>20: RULPD<br>21: RULPC<br>22: RULCD<br>23: RULCC<br>24: RULTD<br>25: RULTC<br>26: TOTSD<br>27: TOTSC<br>28: SFD<br>29: SFC
- Long · 4 — 31: CSint<br>33: BackColor · Default=0
- Yes/No · 1 — 34: Bold · Default=No

Indexes: `1: (CL)`; `2: (Sintetic)`; `3: (Cont)`; `IdUnitate: (IdUnitate)`; `Nume: (Nume)`; `NumeCont: (NumeCont)`; `NumePart: (NumePart)`; `SIID: (SIID)`

In scope - written by: `query:BalantaListareFP` · In scope - read by: nothing found · Also used by 59 object(s) outside the scope.

### `TmpConturi`

- AutoNumber · 4 — 0: ID
- Text · 50 — 1: CONT

Indexes: `CONT: (CONT) UNIQUE`; `ID: (ID)`; `PrimaryKey: (ID) PRIMARY UNIQUE`

In scope - written by: nothing found · In scope - read by: `query:BalantaTemporarFP` · Also used by 6 object(s) outside the scope: `query:Ext_BAT`, `query:Jurnale_Centralizator`, `query:Q_BalantaListare_2017`, `form:LRJ`, `report:Jurnale Separat`, `module:mdl_Balanta2020`.

### `UNIT`

- Text · 255 — 0: NumeUnitate<br>1: Detalii<br>3: Adresa<br>4: Director<br>5: Contabil<br>8: Culoare<br>10: Secretar<br>11: SursaCont<br>12: AdresaMail<br>13: Orasul<br>14: TelefonContact<br>15: SectorSursa<br>17: AlteDetalii<br>18: DC<br>20: Alte<br>21: Gestionar<br>22: Judetul
- Text · 50 — 2: CUI
- Yes/No · 1 — 9: Expirat
- Long · 4 — 16: AnDate · Default=0
- Text · 255 — 19: CodProgram · Default=0000000000

Indexes: `NumeUnitate: (NumeUnitate)`

In scope - written by: `form:EFACTURA_TMP`, `form:EFACTURA_VANZATOR`, `module:mdl_EFactura` · In scope - read by: `query:QEF_SAV`, `query:qFacturi_Vanzare`, `class:clsAnaf_Async` · Also used by 30 object(s) outside the scope.

### `_TempCredit`

- AutoNumber · 4 — 0: ID
- Long · 4 — 1: IDOperatie
- Text · 100 — 2: SimbolCont · Validation=Is Not Null
- Text · 20 — 3: CodPartener
- Currency · 8 — 4: SumaCredit
- Text · 255 — 5: DenumireCont<br>6: ContLung

Indexes: `ID: (ID)`; `IDOperatie: (IDOperatie)`; `PrimaryKey: (ID) PRIMARY UNIQUE`

In scope - written by: nothing found · In scope - read by: `form:Note_EFactura`, `form:_MF2019_Asoc_EF` · Also used by 6 object(s) outside the scope: `query:Note2020`, `query:SAV_NOTE_OP`, `form:Note`, `form:Note_Credit`, `form:Note_FactAsoc`, `module:mdl_Salvare_NoteContabile`.

### `dispozitii_sub`

- AutoNumber · 4 — 0: IDSubDisp
- Text · 255 — 1: Numar<br>2: CCB<br>3: CAP<br>4: TIT<br>5: Clsf
- Double · 8 — 6: Suma
- Long · 4 — 7: IDD

Indexes: `IDD: (IDD)`; `IDSubDisp: (IDSubDisp)`; `PrimaryKey: (IDSubDisp) PRIMARY UNIQUE`

In scope - written by: nothing found · In scope - read by: `query:Q_1` · Also used by 2 object(s) outside the scope: `query:q8XX1`, `form:Disp2020`.

### `tblFiles`

- AutoNumber · 4 — 0: ID
- OLE Object · 0 — 1: fileData
- Text · 255 — 2: filePath<br>3: fileName
- Double · 8 — 4: fileVersion · Default=0
- Long · 4 — 5: locVersion · Default=0
- Yes/No · 1 — 6: chk · Default=Yes

Indexes: `ID: (ID)`; `PrimaryKey: (ID) PRIMARY UNIQUE`

In scope - written by: nothing found · In scope - read by: `module:mdl_EFactura` · Also used by 3 object(s) outside the scope: `form:frmFiles`, `module:mdl_Autoexec`, `module:mdl_DISK_RW`.

### `tmpConturiIBAN`

- AutoNumber · 4 — 0: ID
- Long · 4 — 1: IdClsf · Default=0
- Text · 255 — 2: Cont<br>3: Capitol<br>4: Subcapitol<br>5: Articol<br>6: Alineat<br>7: SursaCont<br>8: Denumire

Indexes: `ID: (ID)`; `IdClsf: (IdClsf)`; `PrimaryKey: (ID) PRIMARY UNIQUE`

In scope - written by: `form:EFACTURA_ADD` · In scope - read by: `form:EFACTURA_PickIBAN` · Also used by 4 object(s) outside the scope: `form:Burse`, `form:Burse_PickIBAN`, `form:OPuri`, `form:OPuri_PickIBAN`.

### `tmpDoc2017P`

- AutoNumber · 4 — 0: ID
- Long · 4 — 1: IDO<br>2: IDD<br>7: NC<br>12: NrOpe
- Long · 4 — 3: IDOP<br>4: IdClsf · Default=0
- Text · 255 — 5: ClsfBug<br>6: Forma<br>9: Document<br>10: ExplicatieOP<br>14: DataALG<br>16: NrAng<br>19: CodPartener
- Date/Time · 8 — 8: Data
- Double · 8 — 11: Valoare
- Yes/No · 1 — 13: ALG · Default=No
- Double · 8 — 15: SumaAlg<br>17: AngLeg · Default=0
- Yes/No · 1 — 18: SV · Default=Yes

Indexes: `ID: (ID)`; `IdClsf: (Forma)`; `IdClsf1: (IdClsf)`; `IDD: (IDD)`; `IDO: (IDO)`; `IDOP: (IDOP)`; `PrimaryKey: (ID) PRIMARY UNIQUE`

In scope - written by: `query:QEF_Plata`, `form:EFACTURA_2025` · In scope - read by: `form:EF_P` · Also used by 26 object(s) outside the scope.

### `tmpFacturi`

- AutoNumber · 4 — 0: ID
- Yes/No · 1 — 1: S · Default=0
- Yes/No · 1 — 2: SV<br>3: S_Vechi<br>4: Nou<br>5: AnAnterior<br>30: ALG · Default=No
- Long · 4 — 6: IdDocument<br>7: IdOperatie<br>8: IdClsf<br>9: IdPlata<br>29: NrAng
- Long · 4 — 10: IDA<br>11: ID8<br>12: IDPF<br>13: IDEFT · Default=0
- Long · 4 — 14: NC · Default=Null
- Text · 255 — 15: Forma<br>16: Clsf<br>20: Explicatie<br>21: Fel
- Text · 50 — 17: NumarDocument<br>19: FelDocument
- Date/Time · 8 — 18: DataDocument<br>31: DataALG
- Double · 8 — 22: Suma<br>23: Platit<br>24: PlatitAnterior<br>27: Diferenta<br>28: Diferenta_temp
- Double · 8 — 25: SalvatAnterior<br>26: ValoareFactura<br>32: SumaALG · Default=0
- Double · 8 — 33: ValInit · Default=Null

Indexes: `ID: (ID8)`; `ID1: (ID)`; `IDA: (IDA)`; `IdClsf: (IdClsf)`; `IdDocument: (IdDocument)`; `IDEFT: (IDEFT)`; `IdOperatie: (IdOperatie)`; `IDPF: (IDPF)`; `IdPlata: (IdPlata)`; `NumarDocument: (NumarDocument)`; `PrimaryKey: (ID) PRIMARY UNIQUE`

In scope - written by: `query:QEF_EV`, `query:QEF_OPER`, `query:QEF_PL`, `form:EFACTURA_2025` · In scope - read by: `query:QEF_Plata`, `query:SAV_OPER_EF`, `query:SAV_OPER_EF_2017`, `form:EF_F`, `form:EF_F_inlucru`, `form:EF_P` · Also used by 47 object(s) outside the scope.

### `tmpQEF`

- AutoNumber · 4 — 0: K
- Long · 4 — 1: IDEFT<br>3: IdOperatie<br>4: IDA<br>18: TotalRamas · Default=0
- Text · 255 — 2: id_sol<br>6: CP<br>7: CF<br>8: Furnizor<br>10: ClsfBug<br>11: NrFact<br>12: LunaAn<br>20: DenClsf<br>25: Tip
- Long · 4 — 5: IDClsf
- Text · 50 — 9: Forma<br>21: Articol<br>22: Alineat
- Integer · 2 — 13: LUNA
- Date/Time · 8 — 14: DataFact<br>27: DataIncarcareEF
- Double · 8 — 15: TotalFactura<br>16: TotalSalvat<br>17: TotalPlatit<br>19: DePlata
- Yes/No · 1 — 23: AreOperatii<br>24: F<br>26: S · Default=No

Indexes: `id_sol: (id_sol)`; `IDA: (IDA)`; `IDClsf: (IDClsf)`; `IDEFT: (IDEFT)`; `IdOperatie: (IdOperatie)`; `PrimaryKey: (K) PRIMARY UNIQUE`

In scope - written by: `query:QEF_ARH`, `query:QEF_NOI`, `query:QEF_SAV`, `query:QEF_TRECERE`, `form:EFACTURA_2025` · In scope - read by: `form:EFACTURA_MSG` · Also used by 4 object(s) outside the scope: `form:Parteneri`, `form:TrecereAN`, `report:EFACTURA_T`, `module:mdl_Popup2022`.

### `tmpXMLPath`

- AutoNumber · 4 — 0: ID
- Text · 255 — 1: CodPartener<br>2: NType<br>4: NValue
- Memo/Long Text · 0 — 3: NPath
- Yes/No · 1 — 5: S · Default=No

Indexes: `ID: (ID)`; `PrimaryKey: (ID) PRIMARY UNIQUE`

In scope - written by: `module:mdl_EFactura` · In scope - read by: nothing found · Also used by 2 object(s) outside the scope: `form:XMLPath`, `form:XMLPath_sub`.
