# Data model

Field rows use the [table-page conventions](README.md#table-page-conventions): only identical type, size, required flag, default and validation are grouped; each original ordinal and field name remains explicit.

Tables in scope. "Linked" means the table lives in another .accdb and Avacont.accdb only links to it - the passwords in the connect strings are masked. Relationships come from the Access relationship window (`relationships.md` in the export), plus the join keys used by the queries below.

## Storage

Table storage follows the map below. The source table name is exactly the table heading. For linked tables, the original masked Connect string is `MS Access;PWD=***;DATABASE=<Database>`; local tables belong to Avacont.accdb.

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

- AutoNumber · 4 — 0: IdClient
- Text · 255 — 1: DenumireClient<br>2: CodFiscal<br>3: Cont<br>4: Banca<br>5: Adresa<br>6: Judetul<br>7: Orasul<br>8: IndFiscal<br>9: Sector
- Yes/No · 1 — 10: CNP

Indexes: `IdClient: (IdClient)`; `PrimaryKey: (IdClient) PRIMARY UNIQUE`

In scope - written by: `form:EFACTURA_CLIENTI` · In scope - read by: `query:qFacturi_Vanzare`, `form:EFACTURA_ADD_FACTURI` · Also used by 1 object(s) outside the scope: `report:FacturaV`.

### `EF`

- AutoNumber · 4 — 0: IDEF
- Text · 255 — 1: id_sol · Required=True
- Text · 255 — 2: id<br>3: cif<br>5: cui_unit<br>10: id_solicitare<br>11: nume_fisier
- Date/Time · 8 — 4: data
- Yes/No · 1 — 6: nou · Default=No
- Date/Time · 8 — 9: DTQ · Default=Now()
- OLE Object · 0 — 12: XML

Indexes: `PrimaryKey: (id_sol) PRIMARY UNIQUE`

In scope - written by: nothing found · In scope - read by: nothing found · Also used by 0 object(s) outside the scope.

### `EFS`

- AutoNumber · 4 — 0: IDEFS
- Long · 4 — 1: IDEFT<br>2: ID · Default=0
- Text · 255 — 3: UNIT<br>7: Denumire<br>11: Explicatie
- Double · 8 — 4: Cant<br>5: Valoare<br>6: Pret · Default=0
- Yes/No · 1 — 8: S<br>9: Rez · Default=No
- Date/Time · 8 — 10: DTQ · Default=Now()

Indexes: `EFT__IDEFT___EFS__IDEFT: (IDEFT) FOREIGN`; `ID: (ID)`; `IDEFS: (IDEFS)`; `IDEFT: (IDEFT)`; `PrimaryKey: (IDEFS) PRIMARY UNIQUE`

In scope - written by: `form:EFACTURA_2025`, `form:EFACTURA_AA`, `module:mdl_EFactura` · In scope - read by: `query:UEFP` · Also used by 4 object(s) outside the scope: `report:Factura_sub`, `report:NIR`, `module:mdl_Autoexec`, `module:mdl_Popup2022`.

### `EFT`

- AutoNumber · 4 — 0: IDEFT
- Text · 255 — 1: id_sol<br>2: NrFact<br>9: CUI<br>10: DenumireP<br>11: Adresa<br>14: Attach<br>17: Ref
- Date/Time · 8 — 3: DataFact<br>4: DataScad
- Long · 4 — 5: CotaTVA · Default=0
- Double · 8 — 6: TVA<br>7: Valoare<br>8: TOTAL · Default=0
- Yes/No · 1 — 12: S<br>13: Complet · Default=No
- Date/Time · 8 — 15: DTQ · Default=Now()
- Text · 255 — 16: Tip · Default=FC
- Long · 4 — 18: IDEFT_REF<br>19: NC
- Yes/No · 1 — 20: AA · Default=0

Indexes: `EF__id_sol___EFT__id_sol: (id_sol) FOREIGN`; `id_sol: (id_sol)`; `IDF: (IDEFT)`; `PrimaryKey: (IDEFT) PRIMARY UNIQUE`

In scope - written by: `form:EFACTURA_2025`, `form:EFACTURA_AA`, `form:MF2019_Asoc_EF`, `form:_MF2019_Asoc_EF`, `module:mdl_EFactura` · In scope - read by: `query:EFACTURA_AA`, `query:QEF`, `query:QEF_ARH`, `query:QEF_DePlata`, `query:QEF_EV`, `query:QEF_EV_inlucru`, `query:QEF_NOI`, `query:QEF_OPER`, `query:QEF_PL`, `query:QEF_RestDePlata`, `query:QEF_RestDePlata_Data`, `query:QEF_SAV`, `query:QEF_TRECERE`, `query:qMF_EFactura_Initial`, `query:qMF_EFactura_Ramas`, `query:qMF_EFactura_Toate`, `query:qNote_EFactura_Toate`, `query:UEFP`, `query:_qNote_EFacturat_Ramas`, `query:_qNote_EFactura_Asociat_Nou`, `query:_qNote_EFactura_Initial`, `form:EFACTURA_MSG` · Also used by 8 object(s) outside the scope: `query:qParteneri2024`, `form:Note`, `form:TrecereAN`, `report:Factura`, `report:NIR`, `module:mdl_Autoexec`, `module:mdl_Popup2022`, `module:mdl_Salvare_NoteContabile`.

### `EFT_C`

- AutoNumber · 4 — 0: IDEFTC
- Long · 4 — 1: IDEFT · Default=0
- Memo/Long Text · 0 — 2: Note
- Date/Time · 8 — 3: DTQ · Default=Now()

Indexes: `EFT__IDEFT___EFT_C__IDEFT: (IDEFT) FOREIGN`; `IDEFT: (IDEFT)`; `IDEFTO: (IDEFTC)`; `PrimaryKey: (IDEFTC) PRIMARY UNIQUE`

In scope - written by: `form:EFACTURA_AA`, `module:mdl_EFactura` · In scope - read by: nothing found · Also used by 1 object(s) outside the scope: `report:FacturaC`.

### `EFT_M`

- AutoNumber · 4 — 0: IDMSG
- Text · 255 — 1: id_sol<br>3: ID
- Long · 4 — 2: IDEFT
- Memo/Long Text · 0 — 4: Mesaj
- Date/Time · 8 — 5: Data
- Date/Time · 8 — 6: DTQ · Default=Now()

Indexes: `EFT__IDEFT___EFT_M__IDEFT: (IDEFT) FOREIGN`; `id_sol: (id_sol)`; `ideft: (IDEFT)`; `PrimaryKey: (IDMSG) PRIMARY UNIQUE`

In scope - written by: `module:mdl_EFactura` · In scope - read by: `form:EFACTURA_MSG` · Also used by 0 object(s) outside the scope.

### `EFT_O`

- AutoNumber · 4 — 0: IDEFTO
- Long · 4 — 1: IDEFT<br>2: IdOperatie<br>3: IdClsf · Default=0
- Text · 255 — 4: Sursa
- Double · 8 — 5: Suma · Default=0
- Date/Time · 8 — 6: DTQ · Default=Now()
- Long · 4 — 7: IdUnitate

Indexes: `EFT__IDEFT___EFT_O__IDEFT: (IDEFT) FOREIGN`; `IdClsf: (IdClsf)`; `IDEFT: (IDEFT)`; `IDEFTO: (IDEFTO)`; `IdOperatie: (IdOperatie)`; `PrimaryKey: (IDEFTO) PRIMARY UNIQUE`

In scope - written by: `query:SAV_OPER_EF`, `query:SAV_OPER_EF_2017`, `form:MF2019_Asoc_EF`, `form:_MF2019_Asoc_EF` · In scope - read by: `query:EFACTURA_AA`, `query:QEF_ARH`, `query:QEF_EV`, `query:QEF_EV_inlucru`, `query:QEF_NOI`, `query:QEF_PL`, `query:QEF_RestDePlata`, `query:QEF_RestDePlata_Data`, `query:QEF_SAV`, `query:QEF_TRECERE`, `query:qMF_EFactura_Initial`, `query:qMF_EFactura_Ramas`, `query:qMF_EFactura_Toate`, `query:qNote_EFactura_Toate`, `query:_qNote_EFacturat_Ramas`, `query:_qNote_EFactura_Initial` · Also used by 5 object(s) outside the scope: `query:MNotaC1`, `form:MF2019_1`, `form:Note`, `module:mdl_Popup2022`, `module:mdl_Salvare_NoteContabile`.

### `EF_ERR`

- AutoNumber · 4 — 0: IDEROARE
- Memo/Long Text · 0 — 1: EROARE

Indexes: `IDEROARE: (IDEROARE)`; `PrimaryKey: (IDEROARE) PRIMARY UNIQUE`

In scope - written by: `module:mdl_EFactura` · In scope - read by: nothing found · Also used by 2 object(s) outside the scope: `report:EF_ERR`, `module:mdl_Popup2022`.

### `EF_EXITCODES`

- Long · 4 — 0: ExitCode · Required=True
- Text · 255 — 1: Expl

Indexes: `ExitCode: (ExitCode) PRIMARY UNIQUE`

In scope - written by: nothing found · In scope - read by: nothing found · Also used by 1 object(s) outside the scope: `form:Setari implicite`.

### `EF_F`

- AutoNumber · 4 — 0: ID
- Long · 4 — 1: IdDocument<br>2: IdOperatie<br>3: IdClsf<br>4: IdPlata<br>20: NrAng
- Long · 4 — 5: ID8<br>6: IDPF<br>7: IDEFT · Default=0
- Long · 4 — 8: NC · Default=Null
- Text · 255 — 9: Forma<br>10: Clsf<br>14: Explicatie<br>15: Fel
- Text · 50 — 11: NumarDocument<br>13: FelDocument
- Date/Time · 8 — 12: DataDocument<br>22: DataALG
- Double · 8 — 16: TotalFactura<br>18: ValoareRamasa<br>19: Diferenta
- Double · 8 — 17: SalvatAnterior<br>23: SumaALG · Default=0
- Yes/No · 1 — 21: ALG · Default=No
- Double · 8 — 24: ValInit · Default=Null

Indexes: `ID: (ID8)`; `ID1: (ID)`; `IdClsf: (IdClsf)`; `IdDocument: (IdDocument)`; `IDEFT: (IDEFT)`; `IdOperatie: (IdOperatie)`; `IDPF: (IDPF)`; `IdPlata: (IdPlata)`; `NumarDocument: (NumarDocument)`; `PrimaryKey: (ID) PRIMARY UNIQUE`

In scope - written by: `query:QEF_EV_inlucru` · In scope - read by: `form:EFACTURA_2025`, `form:EF_F_inlucru` · Also used by 0 object(s) outside the scope.

### `EF_TMP`

- Long · 4 — 0: ID · Required=True · Default=0
- Text · 255 — 1: IDS<br>2: IDI<br>5: TIP<br>6: CIFE<br>8: FURNIZOR
- Date/Time · 8 — 3: DataC
- Memo/Long Text · 0 — 4: Detalii
- Yes/No · 1 — 7: S · Default=No
- Long · 4 — 9: NC · Default=1

Indexes: `IDI: (IDI)`; `IDS: (IDS)`; `PrimaryKey: (ID) PRIMARY UNIQUE`

In scope - written by: `form:EFACTURA_TMP`, `module:mdl_EFactura` · In scope - read by: nothing found · Also used by 0 object(s) outside the scope.

### `EF_UM`

- Text · 255 — 0: COD · Required=True
- Text · 255 — 1: EXP

Indexes: `EXP: (EXP)`; `PrimaryKey: (COD) PRIMARY UNIQUE`

In scope - written by: nothing found · In scope - read by: `form:EFACTURA_ADD_SUB`, `form:EFACTURA_UM` · Also used by 0 object(s) outside the scope.

### `Factura`

- Long · 4 — 0: IdFactura · Required=True
- Long · 4 — 1: IdClient · Default=0
- Long · 4 — 2: IdOperatie · Default=Null
- Double · 8 — 3: NumarFactura
- Date/Time · 8 — 4: DataFactura
- Text · 255 — 5: Comentarii<br>8: id_incarcare<br>9: id_descarcare<br>11: ContPlata<br>13: SerieFactura
- Yes/No · 1 — 6: Anulata
- Yes/No · 1 — 7: TRIMISA · Default=0
- Yes/No · 1 — 10: ATT · Default=No
- Date/Time · 8 — 12: DTQ · Default=Now()
- Text · 30 — 14: BT_13
- Text · 3 — 15: TipFactura · Default=380
- Long · 4 — 16: IdFacturaA
- Text · 10 — 17: SerieFacturaA<br>18: NumarFacturaA
- Double · 8 — 19: Corectata · Default=0

Indexes: `ClientiEF__IdClient___Factura__IdClient: (IdClient) FOREIGN`; `Id: (IdFactura)`; `id_descarcare: (id_descarcare)`; `IdOperatie: (IdOperatie)`; `IdUnitate: (IdClient)`; `NumarFactura: (NumarFactura) UNIQUE`; `PrimaryKey: (IdFactura) PRIMARY UNIQUE`

In scope - written by: `form:EFACTURA_ADD`, `module:mdl_EFactura` · In scope - read by: `query:qFacturi_Vanzare`, `form:EFACTURA_ADD_FACTURI`, `module:mdl_EFactura_Add` · Also used by 7 object(s) outside the scope: `form:Note`, `form:Setari implicite`, `report:EF_ERR`, `report:FacturaV`, `module:mdl_2024`, `module:mdl_Functii_Generale`, `module:mdl_Popup2022`.

### `FacturaC`

- AutoNumber · 4 — 0: IdContinut
- Long · 4 — 1: IdFactura<br>9: Grup · Default=0
- Memo/Long Text · 0 — 2: Continut
- Text · 50 — 3: Um
- Double · 8 — 4: Cant<br>5: PU<br>6: Valoare · Default=0
- Yes/No · 1 — 7: Platit · Default=0
- Text · 255 — 8: NrCrt

Indexes: `Factura__IdFactura___FacturaC__IdFactura: (IdFactura) FOREIGN`; `IdFactura: (IdContinut)`; `IdFactura1: (IdFactura)`; `PrimaryKey: (IdContinut) PRIMARY UNIQUE`

In scope - written by: `form:EFACTURA_ADD` · In scope - read by: `query:qFacturi_Vanzare`, `form:EFACTURA_ADD_FACTURI`, `module:mdl_EFactura_Add` · Also used by 3 object(s) outside the scope: `report:FacturaV`, `report:FacturaV_sub`, `module:mdl_Popup2022`.

### `TEF_F`

- Text · 255 — 0: CP<br>1: Furnizor
- Text · 255 — 2: CF · Required=True

Indexes: `Nume: (Furnizor)`; `PrimaryKey: (CF) PRIMARY UNIQUE`

In scope - written by: nothing found · In scope - read by: nothing found · Also used by 0 object(s) outside the scope.

### `TEF_FF`

- Long · 4 — 0: IDEFT · Required=True · Default=0
- Text · 255 — 1: CF · Required=True
- Text · 255 — 2: id_sol<br>3: NrFact<br>4: LunaAn<br>10: TIP
- Long · 4 — 5: Luna · Default=0
- Date/Time · 8 — 6: DataFact
- Double · 8 — 7: TotalFactura<br>8: TotalSalvat<br>9: TotalRamas · Default=0

Indexes: `{5B4E5596-2A8F-4FAE-9346-A8E1C017FB1D}: (CF) FOREIGN`; `CF: (CF)`; `id_sol: (id_sol)`; `IDEFT: (IDEFT)`; `PrimaryKey: (IDEFT) PRIMARY UNIQUE`

In scope - written by: nothing found · In scope - read by: nothing found · Also used by 0 object(s) outside the scope.

### `TrecereEFactura`

- Text · 255 — 0: CUI<br>1: ID_SOL · Required=True
- Long · 4 — 2: IDEFT · Required=True · Default=0
- Yes/No · 1 — 3: S · Default=Yes

Indexes: `PrimaryKey: (CUI, ID_SOL, IDEFT) PRIMARY UNIQUE`

In scope - written by: nothing found · In scope - read by: nothing found · Also used by 2 object(s) outside the scope: `form:TrecereAN`, `module:mdl_TRECERE_AN`.

### `tblMSG`

- Long · 4 — 0: ID · Required=True
- Memo/Long Text · 0 — 1: Mesaj<br>7: SQL
- Text · 255 — 2: Titlu<br>6: IB<br>8: Valid<br>9: Defa<br>10: Frm<br>11: ActBtn
- Long · 4 — 3: Fel<br>4: Tip<br>5: Fls
- Long · 4 — 12: IDIMG · Default=0

Indexes: `ID: (ID)`; `IDIMG: (IDIMG)`; `PrimaryKey: (ID) PRIMARY UNIQUE`; `Titlu: (Titlu)`; `Valid: (Valid)`

In scope - written by: nothing found · In scope - read by: `form:EFACTURA_2025`, `form:EFACTURA_ADD` · Also used by 17 object(s) outside the scope.

### `tmpEF`

- AutoNumber · 4 — 0: IDEF
- Text · 255 — 1: id_sol · Required=True
- Text · 255 — 2: ID<br>3: CIF
- Date/Time · 8 — 4: Data

Indexes: `ID: (ID)`; `id_solicitare: (id_sol)`; `IDEF: (IDEF)`; `PrimaryKey: (id_sol) PRIMARY UNIQUE`

In scope - written by: nothing found · In scope - read by: nothing found · Also used by 0 object(s) outside the scope.

### `tmpEFS`

- AutoNumber · 4 — 0: IDEFS
- Long · 4 — 1: IDEFT<br>2: ID · Default=0
- Text · 255 — 3: UNIT<br>7: Denumire
- Double · 8 — 4: Cant<br>5: Valoare<br>6: Pret · Default=0
- Yes/No · 1 — 8: S · Default=No

Indexes: `ID: (ID)`; `IDEFS: (IDEFS)`; `IDEFT: (IDEFT)`; `PrimaryKey: (IDEFS) PRIMARY UNIQUE`

In scope - written by: nothing found · In scope - read by: nothing found · Also used by 0 object(s) outside the scope.

### `tmpEFT`

- AutoNumber · 4 — 0: IDEFT
- Text · 255 — 1: NrFact<br>8: id_sol<br>9: CUI<br>10: DenumireP<br>11: Adresa
- Date/Time · 8 — 2: DataFact<br>3: DataScad
- Long · 4 — 4: CotaTVA · Default=0
- Double · 8 — 5: TVA<br>6: Valoare<br>7: TOTAL · Default=0
- Yes/No · 1 — 12: S · Default=No

Indexes: `id_sol: (id_sol)`; `IDF: (IDEFT)`; `PrimaryKey: (IDEFT) PRIMARY UNIQUE`

In scope - written by: nothing found · In scope - read by: nothing found · Also used by 0 object(s) outside the scope.

### `tmpEF_TRANS`

- AutoNumber · 4 — 0: ID
- Text · 255 — 1: id_sol
- Long · 4 — 2: IDEFT<br>3: IDEFTN · Default=0

Indexes: `ID: (ID)`; `id_sol: (id_sol)`; `IDEFT: (IDEFT)`; `IDEFTN: (IDEFTN)`; `PrimaryKey: (ID) PRIMARY UNIQUE`

In scope - written by: `form:EFACTURA_AA` · In scope - read by: nothing found · Also used by 0 object(s) outside the scope.

### `tmpFacturaC`

- AutoNumber · 4 — 0: IdContinut
- Long · 4 — 1: IdFactura<br>9: Grup · Default=0
- Memo/Long Text · 0 — 2: Continut
- Text · 50 — 3: Um
- Double · 8 — 4: Cant<br>5: PU<br>6: Valoare · Default=0
- Yes/No · 1 — 7: Platit · Default=0
- Text · 255 — 8: NrCrt

Indexes: `IdFactura: (IdContinut)`; `IdFactura1: (IdFactura)`; `PrimaryKey: (IdContinut) PRIMARY UNIQUE`

In scope - written by: `form:EFACTURA_ADD`, `form:EFACTURA_ADD_FACTURI`, `form:EFACTURA_ADD_SUB` · In scope - read by: nothing found · Also used by 1 object(s) outside the scope: `module:mdl_Popup2022`.

### `tmpMF_EFactura`

- AutoNumber · 4 — 0: IDTEF
- Long · 4 — 1: IDEF<br>2: IDEFT<br>8: CotaTVA
- Long · 4 — 3: IdUnitate · Default=0
- Text · 255 — 4: id_sol<br>5: id<br>6: NrFact<br>14: DenumireP<br>19: Tip
- Date/Time · 8 — 7: DataFact
- Double · 8 — 9: TVA<br>10: Valoare<br>11: TOTAL<br>12: Asociat<br>13: DeAsociat
- Integer · 2 — 15: AreAtt<br>16: AreXml
- Yes/No · 1 — 17: S · Default=No
- OLE Object · 0 — 18: Pic
- Double · 8 — 20: AlteAsocieri · Default=0

Indexes: `ID: (IDTEF)`; `id_sol: (id_sol)`; `id1: (id)`; `IDEF: (IDEF)`; `IDEFT: (IDEFT)`; `IdUnitate: (IdUnitate)`; `PrimaryKey: (IDTEF) PRIMARY UNIQUE`

In scope - written by: `query:qMF_EFactura_Initial`, `query:qMF_EFactura_Ramas`, `query:qMF_EFactura_Toate`, `form:MF2019_Asoc_EF`, `form:Note_EFactura`, `form:_MF2019_Asoc_EF` · In scope - read by: nothing found · Also used by 1 object(s) outside the scope: `form:MF2019_1`.

### `tmpNoteFacturi`

- AutoNumber · 4 — 0: ID
- Yes/No · 1 — 1: S · Default=No
- Long · 4 — 2: IdOperatie<br>3: IdDocument
- Text · 255 — 4: NumarDocument<br>8: Partener<br>9: Cont<br>11: CodAng<br>12: CodInd<br>13: CodPrg<br>15: ExplicatieOP<br>18: Fel
- Date/Time · 8 — 5: DataDocument<br>7: DataOP
- Long · 4 — 6: NrOP<br>14: IDOPME<br>16: NrOPE · Default=0
- Double · 8 — 10: Valoare<br>17: ValoarePlata · Default=0

Indexes: `IdFactura: (IdDocument)`; `IDOPME: (IDOPME)`; `IdPlata: (IdOperatie)`; `NumarDocument: (NumarDocument)`; `PrimaryKey: (ID) PRIMARY UNIQUE`

In scope - written by: nothing found · In scope - read by: nothing found · Also used by 3 object(s) outside the scope: `form:Note`, `form:Note_PlatiAsoc`, `module:basRibbonCallbacks`.

### `tmpNote_EFactura`

- AutoNumber · 4 — 0: IDTEF
- Long · 4 — 1: IDEF<br>2: IDEFT<br>7: CotaTVA
- Text · 255 — 3: id_sol<br>4: id<br>5: NrFact<br>14: DenumireP<br>18: Tip
- Date/Time · 8 — 6: DataFact
- Double · 8 — 8: TVA<br>9: Valoare<br>10: TOTAL<br>11: Asociat<br>13: DeAsociat
- Double · 8 — 12: AlteAsocieri · Default=0
- Integer · 2 — 15: AreAtt<br>16: AreXml
- Yes/No · 1 — 17: S · Default=No
- OLE Object · 0 — 19: Pic

Indexes: `ID: (IDTEF)`; `id_sol: (id_sol)`; `id1: (id)`; `IDEF: (IDEF)`; `IDEFT: (IDEFT)`; `PrimaryKey: (IDTEF) PRIMARY UNIQUE`

In scope - written by: `query:qNote_EFactura_Toate`, `query:_qNote_EFacturat_Ramas`, `query:_qNote_EFactura_Asociat_Nou`, `query:_qNote_EFactura_Initial`, `form:Note_EFactura` · In scope - read by: `form:MF2019_Asoc_EF` · Also used by 1 object(s) outside the scope: `form:Note`.
