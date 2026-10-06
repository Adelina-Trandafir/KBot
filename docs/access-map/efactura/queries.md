# Queries in scope

Every query that is named like an e-invoice object, or is read by an e-invoice form / module, plus the queries those call. SQL is exported as-is from Access (Jet/ACE dialect). Functions such as `Concat_WS`, `ConcatRelated`, `REGEXP`, `Nz`, `IIf` are VBA functions called from SQL.

| Query | Type | Reads | Writes | VBA functions | Used by |
|---|---|---|---|---|---|
| `AddFacturaFF` | Append | `TEMPF` | `TEMPF` |  |  |
| `BalantaListareFP` | Append | `TmpBalantaListare`, `BalantaTemporarFP` | `TmpBalantaListare` |  |  |
| `BalantaSelectFP` | Union | `PlanCont`, `Parteneri`, `ParteneriSI`, `Debit`, `Oper`, `Credit`, `Q_1`, `DefaCont` |  |  | `query:BalantaTemporarFP` |
| `BalantaTemporarFP` | Select | `BalantaSelectFP`, `TmpConturi` |  |  | `query:BalantaListareFP` |
| `DetaliiEvidentiereFactura` | Append | `Parteneri`, `Documente`, `Oper`, `Credit` |  |  |  |
| `DetaliiPlataFactura` | Append | `Parteneri`, `Documente`, `Oper`, `Debit`, `qPlatiParteneri2015` |  |  |  |
| `EFACTURA_AA` | Select | `EFT_O`, `EFT`, `Oper`, `CALE` |  |  | `form:EFACTURA_2025`, `form:EFACTURA_AA` |
| `Find duplicates for DefaCont` | Select | `DefaCont` |  |  |  |
| `QEF` | Select | `EFT`, `Oper`, `Credit`, `PlatiFacturi`, `Parteneri`, `CALE` |  |  | `module:mdl_Popup2022` |
| `QEF_ARH` | Append | `EFT_O`, `tmpQEF`, `EFT`, `Parteneri`, `CALE` | `tmpQEF` |  | `form:EFACTURA_2025` |
| `QEF_DePlata` | Select | `EFT`, `PlatiFacturi`, `Parteneri`, `Oper` |  | `concatrelated` |  |
| `QEF_EV` | Append | `tmpFacturi`, `EFT`, `EFT_O`, `CALE` | `tmpFacturi` | `REGEXP`, `REGEXP_COUNT` | `form:EFACTURA_2025` |
| `QEF_EV_inlucru` | Append | `EF_F`, `EFT`, `EFT_O`, `CALE` | `EF_F` | `REGEXP`, `REGEXP_COUNT` |  |
| `QEF_NOI` | Append | `EFT_O`, `CALE`, `EFT`, `tmpQEF`, `Parteneri` | `tmpQEF` |  | `form:EFACTURA_2025` |
| `QEF_OPER` | Append | `PlatiFacturi`, `tmpFacturi`, `EFT`, `Oper`, `Cont8067`, `Documente`, `Clasificatii`, `Credit` | `tmpFacturi` |  |  |
| `QEF_OPNU` | Append | `DefaNoteA`, `EXPLOP` | `EXPLOP` | `ConcatRelated` |  |
| `QEF_PL` | Append | `EFT_O`, `PlatiFacturi`, `tmpFacturi`, `EFT`, `Oper`, `Clasificatii`, `Documente`, `Cont8067` (+1) | `tmpFacturi` | `Concat_WS` | `form:EFACTURA_2025` |
| `QEF_Plata` | Append | `tmpDoc2017P`, `tmpFacturi` | `tmpDoc2017P` | `ConcatRelated`, `Regexp` | `form:EFACTURA_2025` |
| `QEF_RestDePlata` | Select | `EFT_O`, `PlatiFacturi`, `EFT`, `CALE` |  |  | `module:mdl_Popup2022` |
| `QEF_RestDePlata_Data` | Select | `EFT_O`, `PlatiFacturi`, `EFT`, `CALE` |  |  | `module:mdl_Popup2022` |
| `QEF_SAV` | Append | `EFT`, `PlatiFacturi`, `tmpQEF`, `Clasificatii`, `Parteneri`, `EFT_O`, `Oper`, `UNIT` (+1) | `tmpQEF` | `concat_ws` | `form:EFACTURA_2025` |
| `QEF_TRECERE` | Append | `EFT_O`, `EFT`, `tmpQEF`, `Parteneri`, `CALE` | `tmpQEF` | `TrecereAN` |  |
| `Q_1` | Union | `Q_7`, `Q_2`, `qClsfE`, `Rectificari`, `dispozitii_sub`, `Q_8`, `ClasificatiiV`, `RectificariV` (+2) |  |  | `query:BalantaSelectFP` |
| `Q_13` | Union | `ClasificatiiV` |  |  | `query:Q_1` |
| `Q_2` | Union | `Clasificatii` |  |  | `query:Q_1` |
| `Q_7` | Select | `CT8030` |  |  | `query:Q_1` |
| `Q_8` | Union | `Contracte`, `Contracte_A` |  |  | `query:Q_1` |
| `SAV_OPER_EF` | Append | `EFT_O`, `tmpFacturi` | `EFT_O` |  | `module:mdl_Salvare_NoteContabile` |
| `SAV_OPER_EF_2017` | Append | `EFT_O`, `tmpFacturi` | `EFT_O` |  |  |
| `UEFP` | Union | `Parteneri`, `EFT`, `EFS` |  | `roundcut` |  |
| `_qNote_EFactura_Asociat_Nou` | Append | `tmpNote_EFactura`, `EFT` | `tmpNote_EFactura` | `mmin` |  |
| `_qNote_EFactura_Initial` | Append | `tmpNote_EFactura`, `EFT`, `EFT_O` | `tmpNote_EFactura` |  |  |
| `_qNote_EFacturat_Ramas` | Append | `tmpNote_EFactura`, `EFT`, `EFT_O`, `Parteneri` | `tmpNote_EFactura` |  |  |
| `qCLSFV` | Select | `ClasificatiiV` |  |  | `query:Cont_8090`, `query:Q_1` |
| `qClsfE` | Select | `Clasificatii` |  |  | `query:AddBugetI1133`, `query:AddBugetT1133`, `module:mdl_Balanta2020`, `query:Q_1` |
| `qDefaCont_Bilant` | Select | `DefaCont` |  |  |  |
| `qDefaNote` | Select | `DefaNoteA`, `Parteneri`, `DefaNote`, `Clasificatii` |  |  |  |
| `qFacturi_Vanzare` | Select | `UNIT`, `FacturaC`, `ClientiEF`, `Factura` |  |  | `module:mdl_EFactura_Add` |
| `qMF_EFactura_Initial` | Append | `tmpMF_EFactura`, `EFT`, `EFT_O` | `tmpMF_EFactura` |  | `form:MF2019_1` |
| `qMF_EFactura_Ramas` | Append | `tmpMF_EFactura`, `EFT`, `EFT_O`, `Parteneri` | `tmpMF_EFactura` |  | `form:MF2019_1` |
| `qMF_EFactura_Toate` | Append | `tmpMF_EFactura`, `EFT`, `EFT_O`, `Parteneri` | `tmpMF_EFactura` |  | `form:MF2019_Asoc_EF`, `form:_MF2019_Asoc_EF` |
| `qNote_EFactura_Toate` | Append | `tmpNote_EFactura`, `EFT`, `EFT_O`, `Parteneri` | `tmpNote_EFactura` |  | `form:Note`, `form:Note_EFactura` |
| `qPlatiParteneri2015` | Select | `PlatiFacturi`, `Documente`, `Oper`, `Credit` |  |  | `query:DetaliiPlataFactura` |

## SQL

### `AddFacturaFF` (Append)

```sql
INSERT INTO TEMPF ( Serie, Numar, Data, Continut, UM, Cant, PU, Valoare, IDBeneficiar, IDDelegat )
SELECT Facturi.Serie, Facturi.Numar, Facturi.Data, Facturi.Continut, Facturi.UM, Facturi.Cant, Facturi.PU, Facturi.Valoare, Facturi.IDBeneficiar, Facturi.IDDelegat
FROM Facturi
WHERE (((Facturi.IdFact)=[forms]![fact]![idfact]))
ORDER BY Facturi.NrRand;
```

### `BalantaListareFP` (Append)

```sql
INSERT INTO TmpBalantaListare ( CL, Sintetic, Cont, NumeCont, SIID, SIIC, RULPD, RULPC, RULCD, RULCC, TOTSD, TOTSC, SFD, SFC )
SELECT Mid([ctcup],1,1) AS CL, scoatesintetic([ctcup]) AS Sintetic, BalantaTemporarFP.CTCUP AS Cont, BalantaTemporarFP.NNCT AS NumeCont, BalantaTemporarFP.SIID1 AS SIID, BalantaTemporarFP.SIIC1 AS SIIC, BalantaTemporarFP.SumOfPRCD AS RULPD, BalantaTemporarFP.SumOfPRCC AS RULPC, BalantaTemporarFP.SumOfCRCD AS RULCD, BalantaTemporarFP.SumOfCRCC AS RULCC, [siid]+[rulpd]+[rulcd] AS TOTSD, [siic]+[rulpc]+[rulcc] AS TOTSC, IIf([totsd]>[totsc],[totsd]-[totsc],0) AS SFD, IIf([totsc]>[totsd],[totsc]-[totsd],0) AS SFC
FROM BalantaTemporarFP
ORDER BY scoatesintetic([ctcup]);
```

### `BalantaSelectFP` (Union)

```sql
PARAMETERS TIPP Text ( 255 );
select plancont.simbolcont, plancont.denumirecont as NCT, "" as Part, plancont.simbolcont as CTCUP, plancont.SID as SIID, plancont.SIC as SIIC,  0 as PRCD, 0 as PRCC, 0 as CRCD, 0 as CRCC, ContLung
from plancont

UNION ALL SELECT partenerisi.simbolcont, PlanCont.DenumireCont as NCT,"" as part, partenerisi.simbolcont as ctcup, partenerisi.SID as SIID, partenerisi.SIC as SIIC, 0 as prcd, 0 as prcc, 0 as crcd, 0 as crcc, ContLung FROM (Parteneri INNER JOIN ParteneriSI ON Parteneri.CodPartener = ParteneriSI.CodPartener) INNER JOIN PlanCont ON ParteneriSI.SimbolCont = PlanCont.SimbolCont

union all select debit.simbolcont, PlanCont.DenumireCont as NCT, "" as part,  debit.simbolcont as ctcup,0 as siid, 0 as siic, debit.sumadebit as prcd,  0 as PRCC, 0 as CRCD, 0 as CRCC, ContLung FROM (debit INNER JOIN oper ON debit.IdOperatie = oper.IdOperatie) INNER JOIN PlanCont ON debit.SimbolCont = PlanCont.SimbolCont where oper.dataoperatie < forms!balanta!di

union all select credit.simbolcont,PlanCont.DenumireCont as NCT,""  as part,  credit.simbolcont  as ctcup,0 as siid, 0 as siic, 0 as prcd,  credit.sumacredit as PRCC, 0 as CRCD, 0 as CRCC, ContLung FROM (credit INNER JOIN oper ON credit.IdOperatie = oper.IdOperatie) INNER JOIN PlanCont ON credit.SimbolCont = PlanCont.SimbolCont where oper.dataoperatie < forms!balanta!di

union all select debit.simbolcont, PlanCont.DenumireCont as NCT, "" as part,  debit.simbolcont as ctcup,0 as siid, 0 as siic, 0 as prcd,  0 as PRCC, debit.sumadebit as CRCD, 0 as CRCC, ContLung FROM (debit INNER JOIN oper ON debit.IdOperatie = oper.IdOperatie) INNER JOIN PlanCont ON debit.SimbolCont = PlanCont.SimbolCont where oper.dataoperatie >= forms!balanta!di and oper.dataoperatie <= forms!balanta!dsf

UNION ALL select credit.simbolcont, PlanCont.DenumireCont as NCT, "" as part,  credit.simbolcont as ctcup,0 as siid, 0 as siic,  0 as prcd,  0 as PRCC,  0 as CRCD, credit.sumacredit as CRCC, ContLung FROM (credit INNER JOIN oper ON credit.IdOperatie = oper.IdOperatie) INNER JOIN PlanCont ON credit.SimbolCont = PlanCont.SimbolCont where oper.dataoperatie >= forms!balanta!di and oper.dataoperatie <= forms!balanta!dsf

UNION ALL SELECT x.Cont, DefaCont.Denumire, "" AS Part, x.cont, 0 AS sid, 0 AS sic, 0 AS PRCD, 0 AS PRCC, x.RCD, x.RCC, [x]![cont] & Replace(Space(41-Len([x]![cont]))," ","X") AS contlung
FROM (SELECT Left([cont],7) AS LC, q_1.Cont, q_1.Data, q_1.RCD, q_1.RCC
FROM q_1)  AS x INNER JOIN DefaCont ON x.LC = DefaCont.Cont;
```

### `BalantaTemporarFP` (Select)

```sql
SELECT BalantaSelectFP.NCT AS NNCT, BalantaSelectFP.CTCUP, IIf(Sum([SIID])>Sum([siic]),Sum([siid])-Sum([siic]),0) AS SIID1, IIf(Sum([SiIC])>Sum([SIID]),Sum([siic])-Sum([siid]),0) AS SIIC1, Sum(BalantaSelectFP.PRCD) AS SumOfPRCD, Sum(BalantaSelectFP.PRCC) AS SumOfPRCC, Sum(BalantaSelectFP.CRCD) AS SumOfCRCD, Sum(BalantaSelectFP.CRCC) AS SumOfCRCC
FROM BalantaSelectFP INNER JOIN TmpConturi ON BalantaSelectFP.simbolcont LIKE TmpConturi.CONT
GROUP BY BalantaSelectFP.NCT, BalantaSelectFP.CTCUP;
```

### `DetaliiEvidentiereFactura` (Append)

```sql
INSERT INTO tmpDetaliiEvPart ( CodPartener, FelDocument, NumarDocument, DataDocument, Explicatie, SumaCredit, Platit, NrNC, NrDC )
SELECT Parteneri.CodPartener, Documente.FelDocument, Documente.NumarDocument, Documente.DataDocument, Oper.Explicatie, Sum(Credit.SumaCredit) AS SumaCredit, 0 AS Expr1, Documente.NumarNC AS NrNC, Documente.IdDocument AS NrDC
FROM Parteneri INNER JOIN ((Documente INNER JOIN Oper ON Documente.IdDocument = Oper.IdDocument) INNER JOIN Credit ON Oper.IdOperatie = Credit.IdOperatie) ON Parteneri.CodPartener = Credit.CodPartener
GROUP BY Parteneri.CodPartener, Documente.FelDocument, Documente.NumarDocument, Documente.DataDocument, Oper.Explicatie, 0, Documente.NumarNC, Documente.IdDocument, Mid([SimbolCont],1,1)
HAVING (((Parteneri.CodPartener)=forms!parteneri2015!cp) And ((Mid([SimbolCont],1,1)) Like "4"))
ORDER BY Documente.DataDocument;
```

### `DetaliiPlataFactura` (Append)

```sql
INSERT INTO tmpDetaliiPlPart ( IdOperatie, CodPartener, FelDocument, IDClsf, NumarDocument, DataDocument, Explicatie, Platit, SumaAnt, IdSalarii, OP, SumaDebit, NrNC, NrDC, NrOP, angleg )
SELECT Oper.IdOperatie, Parteneri.CodPartener, Documente.FelDocument, Oper.IDClsf, Documente.NumarDocument, Documente.DataDocument, Oper.Explicatie, Oper.Platit, Oper.SumaAnt AS Expr1, Oper.IdSalarii, Oper.OP, Debit.SumaDebit, Documente.NumarNC AS NrNC, Documente.IdDocument AS NrDC, Oper.IdOperatie AS NrOP, Oper.angleg AS Expr2
FROM Parteneri INNER JOIN ((Documente INNER JOIN Oper ON Documente.IdDocument = Oper.IdDocument) INNER JOIN Debit ON Oper.IdOperatie = Debit.IdOperatie) ON Parteneri.CodPartener = Debit.CodPartener
GROUP BY Parteneri.CodPartener, Documente.FelDocument, Oper.IDClsf, Documente.NumarDocument, Documente.DataDocument, Oper.Explicatie, Oper.Platit, Oper.SumaAnt, Oper.IdSalarii, Oper.OP, Debit.SumaDebit, Documente.NumarNC, Documente.IdDocument, Oper.IdOperatie, Oper.angleg, Oper.IdOperatie
HAVING (((Parteneri.CodPartener)=[forms]![parteneri2015]![cp]) AND ((Documente.IdDocument) In (select iddocument from qPlatiParteneri2015)))
ORDER BY Documente.DataDocument;
```

### `EFACTURA_AA` (Select)

```sql
SELECT *
FROM (SELECT EF.IDEF, EF.id_sol, EF.cif, EF.cui_unit, EFT.IDEFT, EFT.NrFact, EFT.DataFact, EFT.TVA, EFT.Valoare, EFT.TOTAL, EFT.CUI, EFT.DenumireP
FROM (SELECT IDEFT, Sum(SUMA) as S FROM EFT_O IN '' {DB} GROUP BY IDEFT)  AS EFTO RIGHT JOIN (EF INNER JOIN EFT ON EF.id_sol = EFT.id_sol) ON EFTO.IDEFT = EFT.IDEFT IN '' {DB}
WHERE Round([total],2)-Round(Nz([s],0),2)<>0 AND Year(DataFact) IN (SELECT YEAR(Max(DataOperatie)) FROM Oper IN '' {DB2})
)  AS EFT_AA
WHERE (((EFT_AA.EF.id_sol) Not In (SELECT id_sol FROM EF)) AND ((EFT_AA.cui_unit) In (SELECT CodFiscal FROM Cale)))
ORDER BY EFT_AA.DenumireP, EFT_AA.DataFact DESC;
```

### `Find duplicates for DefaCont` (Select)

```sql
SELECT First(DefaCont.[Cont]) AS [Cont Field], Count(DefaCont.[Cont]) AS NumberOfDups
FROM DefaCont
GROUP BY DefaCont.[Cont]
HAVING (((Count([DefaCont].[Cont]))>1));
```

### `QEF` (Select)

```sql
SELECT EFT.id_sol, EFT.IDEFT, [evanterior]![ideft] Is Not Null AS AreOperatii, Nz([codpartener],-1) AS CP, EFT.cui AS CF, EFT.DenumireP AS Furnizor, EFT.NrFact, Format([datafact],'mm\/yyyy') AS LunaAn, Month([datafact]) AS LUNA, EFT.DataFact, EFT.Total AS TotalFactura, CDbl(Nz([totals],0)) AS TotalSalvat, CDbl(Nz([totalp],0)) AS TotalPlatit, [EFT]![total]-CDbl(Nz([planterior]![totalp],0)) AS DePlata, Switch(IsNull([totals]) And [Complet]=True,3,Not IsNull([totals]) And [Total]=[totals],2,True,1) AS F
FROM (((EFT LEFT JOIN (SELECT Oper.IDEFT, Sum(Credit.SumaCredit) AS totals FROM Oper INNER JOIN Credit ON Oper.IdOperatie = Credit.IdOperatie GROUP BY Oper.IDEFT HAVING Oper.IDEFT Is Not Null)  AS EvAnterior ON (EFT.IDEFT = EvAnterior.IDEFT) AND (EFT.IDEFT = EvAnterior.IDEFT)) LEFT JOIN (SELECT Oper.IDEFT, Sum(PlatiFacturi.Platit) AS totalp FROM Oper INNER JOIN PlatiFacturi ON Oper.IdOperatie = PlatiFacturi.IdFactura GROUP BY Oper.IDEFT HAVING Oper.IDEFT Is Not Null)  AS PlAnterior ON EFT.IDEFT = PlAnterior.IDEFT) LEFT JOIN (SELECT First(Parteneri.CodPartener) AS CodPartener, Count(Parteneri.CodPartener) AS C, Parteneri.CodFiscal AS CUI FROM Parteneri GROUP BY Parteneri.CodFiscal HAVING Parteneri.CodFiscal<>"")  AS PART ON EFT.CUI = PART.CUI) INNER JOIN EF ON EFT.id_sol = EF.id_sol
WHERE (((Switch(IsNull([totals]) And [Complet]=True,3,Not IsNull([totals]) And [Total]=[totals],2,True,1))=[forms]![EFACTURA]![FACTURI]) AND ((EF.cui_unit) In (select cstr(codfiscal) from cale)))
ORDER BY EFT.DenumireP, Month([datafact]) DESC , EFT.DataFact DESC;
```

### `QEF_ARH` (Append)

```sql
INSERT INTO tmpqef ( DataIncarcareEF, id_sol, IDEFT, AreOperatii, CP, CF, Furnizor, NrFact, LunaAn, LUNA, DataFact, TotalFactura, TotalSalvat, TotalPlatit, DePlata, F, Tip )
SELECT EF.data, EFT.id_sol, EFT.IDEFT, False AS AreOperatii, Nz([codpartener],-1) AS CP, EFT.cui AS CF, EFT.DenumireP AS Furnizor, EFT.NrFact, Format([datafact],'mm\/yyyy') AS LunaAn, Month([datafact]) AS LUNA, EFT.DataFact, EFT.Total AS TotalFactura, 0 AS TotalSalvat, 0 AS TotalPlatit, EFT.total AS DePlata, EFT.complet AS F, EFT.Tip
FROM (EFT LEFT JOIN (SELECT First(Parteneri.CodPartener) AS CodPartener, Count(Parteneri.CodPartener) AS C, Parteneri.CodFiscal AS CUI FROM Parteneri GROUP BY Parteneri.CodFiscal HAVING Parteneri.CodFiscal<>"")  AS PART ON EFT.CUI = PART.CUI) INNER JOIN EF ON EFT.id_sol = EF.id_sol
WHERE (((EFT.IDEFT) Not In (select [IDEFT] FROM [EFT_O])) AND ((EFT.complet)=True) AND ((EF.cui_unit) In (select cstr(codfiscal) from cale)))
ORDER BY EFT.DenumireP, Month([datafact]) DESC , EFT.DataFact DESC;
```

### `QEF_DePlata` (Select)

```sql
SELECT Nz([CodPartener],"-1") AS CP, Nz([DenumirePartener],[denumirep]) AS Furnizor, EFT.cui AS CF, EFT.adresa AS Adr, concatrelated('NrFact','EFT','IDEFT=' & [eft]![ideft]) AS Expl, EFT.TOTAL AS Factura, Sum(PlatiFacturi.Platit) AS Plati, [total]-Sum([platifacturi].[platit]) AS DePlata, Parteneri.ContIBAN, Parteneri.Banca, EFT.S
FROM ((((SELECT Parteneri.CodPartener, Parteneri.DenumirePartener, Replace(Replace(Replace(Nz([CodFiscal],""),"RO",""),"R",""),Space(1),"") AS CodFisc, Parteneri.ContIBAN, Parteneri.Banca, Parteneri.Adresa FROM Parteneri)  AS Parteneri RIGHT JOIN EF ON Parteneri.CodFisc = EF.CIF) INNER JOIN EFT ON EF.id_sol = EFT.id_sol) LEFT JOIN Oper ON EFT.IDEFT = Oper.IDEFT) LEFT JOIN PlatiFacturi ON Oper.IdOperatie = PlatiFacturi.IdFactura
GROUP BY Nz([CodPartener],"-1"), Nz([DenumirePartener],[denumirep]), EFT.cui, EFT.adresa, concatrelated('NrFact','EFT','IDEFT=' & [eft]![ideft]), EFT.TOTAL, Parteneri.ContIBAN, Parteneri.Banca, EFT.S
HAVING (((Nz([CodPartener],"-1"))=forms!EFACTURA!CODP) And (([total]-Sum(platifacturi.platit))<>0) And ((EFT.S)=True));
```

### `QEF_EV` (Append)

```sql
INSERT INTO tmpFacturi ( IDEFT, Valoarefactura, Suma, Platit, SalvatAnterior, Clsf, IdClsf, Forma, NumarDocument, DataDocument, FelDocument, Fel, Nou, S )
SELECT EFT.IDEFT, EFT.TOTAL, ([total]-Nz([totalsalvat],0))*IIf([tip]="NC",-1,1) AS Suma, ([total]-Nz([totalsalvat],0))*IIf([tip]="NC",-1,1) AS Platit, Nz([totalsalvat],0)*IIf([tip]="NC",-1,1) AS ValoareRamasa, [forms]![efactura_2025]![CLSF] AS Clsf, CLng([forms]![efactura_2025]![idclsf]) AS IdClsf, [forms]![efactura_2025]![forma] AS Forma, IIf(REGEXP(REGEXP([NrFact],"\w+",REGEXP_COUNT([NrFact],"\w+")),"\D+")="",[nrfact],REGEXP(REGEXP([NrFact],"\w+",REGEXP_COUNT([NrFact],"\w+")),"\D+")) AS NrDoc, EFT.DataFact, IIf([Tip]="FC","Factură","Notă credit") AS FelDocument, "E" AS Fel, True AS NOU, EFT.S
FROM (EFT INNER JOIN EF ON EFT.id_sol = EF.id_sol) LEFT JOIN (SELECT IDEFT, Sum(Suma) AS TotalSalvat FROM EFT_O WHERE IdUnitate IN (SELECT IdUnitate FROM Cale) GROUP BY IDEFT)  AS EFT_O ON EFT.IDEFT = EFT_O.IDEFT
WHERE (((EFT.IDEFT) Not In (SELECT IDEFT FROM [tmpFacturi])) AND ((EFT.S)=True) AND ((EFT.CUI)=[Forms]![efactura_2025]![CUIF]) AND ((EF.cui_unit) In (SELECT Cstr(CodFiscal) FROM Cale)) AND ((EFT.Complet)=False));
```

### `QEF_EV_inlucru` (Append)

```sql
INSERT INTO EF_F ( IDEFT, TotalFactura, SalvatAnterior, ValoareRamasa, Clsf, IdClsf, Forma, NumarDocument, DataDocument, FelDocument, Fel, Nou, S )
SELECT EFT.IDEFT, EFT.TOTAL*EFT.NC AS TotalFactura, Nz([totalsalvat],0) AS SalvatAnterior, ([total]*EFT.NC-Nz([totalsalvat],0)) AS ValoareRamasa, [forms]![efactura]![CLSF] AS Clsf, CLng([forms]![efactura]![idclsf]) AS IdClsf, [forms]![efactura]![forma] AS Forma, REGEXP(REGEXP([NrFact],"\w+",REGEXP_COUNT([NrFact],"\w+")),"\D+") AS NrDoc, EFT.DataFact, IIf([Tip]="FC","Factură","Notă credit") AS FelDocument, "E" AS Fel, True AS NOU, EFT.S
FROM (EFT INNER JOIN EF ON EFT.id_sol = EF.id_sol) LEFT JOIN (SELECT IDEFT, Sum(Suma) AS TotalSalvat FROM EFT_O WHERE IdUnitate IN (SELECT IdUnitate FROM Cale) GROUP BY IDEFT)  AS EFT_O ON EFT.IDEFT = EFT_O.IDEFT
WHERE EFT.IDEFT Not In (                 SELECT                     IDEFT                 FROM                     EF_F             )                  AND EFT.S = True         AND EFT.CUI = [Forms]![EFACTURA]![CUIF]         AND              EF.cui_unit = DLOOKUP("CUI","UNIT");
```

### `QEF_NOI` (Append)

```sql
INSERT INTO tmpQEF ( DataIncarcareEF, id_sol, IDEFT, AreOperatii, CP, CF, Furnizor, NrFact, LunaAn, LUNA, DataFact, TotalFactura, TotalSalvat, TotalRamas, Tip )
SELECT EF.data, EFT.id_sol, EFT.IDEFT, [idefto] Is Not Null AS AreOperatii, Nz([codpartener],-1) AS CP, EFT.cui AS CF, DenP.DenumirePT AS Furnizor, EFT.NrFact, Format([datafact],'mm\/yyyy') AS LunaAn, Month([datafact]) AS LUNA, EFT.DataFact, [Total]*IIf([tip]="NC",-1,1) AS TotalFactura, Sum(CDbl(Nz([eft_o]![suma],0))) AS TotalSalvat, Round([total]*IIf([tip]="NC",-1,1)-Sum(CDbl(Nz([eft_o]![suma],0)))) AS TotalRamas, EFT.Tip
FROM (((EFT LEFT JOIN (SELECT First(Parteneri.CodPartener) AS CodPartener, Count(Parteneri.CodPartener) AS C, Parteneri.CodFiscal AS CUI FROM Parteneri GROUP BY Parteneri.CodFiscal HAVING Parteneri.CodFiscal<>"")  AS PART ON EFT.CUI = PART.CUI) INNER JOIN EF ON EFT.id_sol = EF.id_sol) LEFT JOIN EFT_O ON EFT.IDEFT = EFT_O.IDEFT) INNER JOIN (SELECT EFT.CUI, Max(EFT.DenumireP) AS DenumirePT FROM EFT GROUP BY EFT.CUI)  AS DenP ON EFT.CUI = DenP.CUI
WHERE (((Nz([IdUnitate],0)) In (SELECT IdUnitate FROM [CALE]) Or (Nz([IdUnitate],0))=0))
GROUP BY EF.data, EFT.id_sol, EFT.IDEFT, [idefto] Is Not Null, Nz([codpartener],-1), EFT.cui, DenP.DenumirePT, EFT.NrFact, Format([datafact],'mm\/yyyy'), Month([datafact]), EFT.DataFact, [Total]*IIf([tip]="NC",-1,1), EFT.Tip, EF.cui_unit, ([Complet]=True Or (Round([EFT]![total]-CDbl(Nz([eft_o]![suma],0)),2)=0 And [Total]<>0))
HAVING (((EF.cui_unit) In (select cstr(codfiscal) from cale)) AND ((([Complet]=True Or (Round([EFT]![total]-CDbl(Nz([eft_o]![suma],0)),2)=0 And [Total]<>0)))=False))
ORDER BY DenP.DenumirePT, Month([datafact]) DESC , EFT.DataFact DESC;
```

### `QEF_OPER` (Append)

```sql
INSERT INTO tmpFacturi ( IdDocument, IdOperatie, IDClsf, IdPlata, ID8, IDPF, NC, Forma, NumarDocument, DataDocument, FelDocument, Explicatie, NrANG, ALG, DataAlg, SumaAlg, Suma, Platit, Diferenta, IDEFT, Fel )
SELECT Oper.IdDocument, Oper.IdOperatie, Oper.IDClsf, PlatiFacturi.IdPlata, Cont8067.ID8, PlatiFacturi.ID AS IDPF, Documente.NumarNC, Clasificatii.subcapitol AS Forma, Documente.NumarDocument, Documente.DataDocument, Documente.FelDocument, Oper.Explicatie, Oper.NrANG, Not IsNull([id8]) AS ALG, Cont8067.DataAlg, Cont8067.SumaAlg, Credit.SumaCredit AS Suma, [sumacredit]-IIf(Nz([platifacturi].[Platit],0)=0,[sumacredit],[platifacturi].[Platit]) AS PL, [sumacredit]-IIf(Nz([platifacturi].[Platit],0)=0,[sumacredit],[platifacturi].[Platit]) AS DIF, EFT.IDEFT, Documente.FelDocument
FROM (((((EFT INNER JOIN Oper ON EFT.IDEFT = Oper.IDEFT) LEFT JOIN PlatiFacturi ON Oper.IdOperatie = PlatiFacturi.IdFactura) LEFT JOIN Cont8067 ON Oper.IdOperatie = Cont8067.IDO) INNER JOIN Documente ON Oper.IdDocument = Documente.IdDocument) INNER JOIN Clasificatii ON Oper.IDClsf = Clasificatii.IDClsf) INNER JOIN Credit ON Oper.IdOperatie = Credit.IdOperatie
WHERE ((([sumacredit]-IIf(Nz(platifacturi.Platit,0)=0,[sumacredit],platifacturi.Platit))<>0) And ((EFT.S)=True) And ((PlatiFacturi.CodP)=forms!EFACTURA!CODP))
ORDER BY Oper.IdOperatie;
```

### `QEF_OPNU` (Append)

```sql
INSERT INTO EXPLOP ( CLSF, FORMA, DOCF, DOCD, DOCN, EXPL )
SELECT Replace([forms]![efactura]![CLSF],"XX.XX",[forms]![efactura]![Forma]) AS Clsf, [forms]![efactura]![forma] AS FORMA, DMax("Fel","tmpFacturi","Forma='" & [forms]![efactura]![Forma] & "' AND (S=True OR Nou=True)") AS Fel, ConcatRelated("[NumarDocument] & '/' & Replace([DataDocument],'/','.')","tmpFacturi","Forma='" & [forms]![efactura]![Forma] & "' AND (S=True OR Nou=True)","[DataDocument]",";") AS Doc, ConcatRelated("NumarDocument","tmpFacturi","(S=True OR Nou=True) AND Forma='" & [forms]![efactura]![Forma] & "'","NumarDocument",";") AS NR, DefaNoteA.Explicatie AS Expr1
FROM DefaNoteA
WHERE ((([DefaNoteA].[IDA])=[forms]![EFACTURA]![LstPl]));
```

### `QEF_PL` (Append)

```sql
INSERT INTO tmpFacturi ( IDEFT, ValoareFactura, Suma, Platit, Diferenta, SalvatAnterior, Clsf, IdClsf, Forma, NumarDocument, DataDocument, FelDocument, Fel, Nou, S, NC, ALG, SumaAlg, IdOperatie, NrANG, IDA )
SELECT EFT.IDEFT, EFT.TOTAL, [eft_o]![suma]-Sum(Nz([platifacturi]![platit],0)) AS Suma, [eft_o]![suma]-Sum(Nz([platifacturi]![platit],0)) AS Platit, [eft_o]![suma]-Sum(Nz([platifacturi]![platit],0)) AS Diferenta, [eft_o]![suma] AS SalvatAnterior, Concat_WS(".",[Capitol],[Subcapitol],[Articol],[Alineat]) AS Clsf, EFT_O.idclsf AS IdClsf, Clasificatii.subcapitol AS Forma, Documente.numardocument, Documente.DataDocument, Documente.FelDocument, Documente.Fel, False AS NOU, EFT.S, Documente.NumarNC, Oper.AngajamentLegal, CDbl(Nz([SumaAlg],0)) AS SALG, Oper.IdOperatie, Oper.NrANG, Oper.IDA
FROM ((((((EFT INNER JOIN EF ON EFT.id_sol = EF.id_sol) INNER JOIN EFT_O ON EFT.IDEFT = EFT_O.IDEFT) INNER JOIN Oper ON EFT_O.IdOperatie = Oper.IdOperatie) LEFT JOIN PlatiFacturi ON Oper.IdOperatie = PlatiFacturi.IdFactura) INNER JOIN Clasificatii ON EFT_O.IdClsf = Clasificatii.IDClsf) INNER JOIN Documente ON Oper.IdDocument = Documente.IdDocument) LEFT JOIN Cont8067 ON Oper.IdOperatie = Cont8067.IDO
WHERE (((EFT_O.IdUnitate) In (SELECT IdUnitate FROM Cale)))
GROUP BY EFT.IDEFT, EFT.TOTAL, [eft_o]![suma], Concat_WS(".",[Capitol],[Subcapitol],[Articol],[Alineat]), EFT_O.idclsf, Clasificatii.subcapitol, Documente.numardocument, Documente.DataDocument, Documente.FelDocument, Documente.Fel, False, EFT.S, Documente.NumarNC, Oper.AngajamentLegal, CDbl(Nz([SumaAlg],0)), Oper.IdOperatie, Oper.NrANG, Oper.IDA, EFT.CUI, EF.cui_unit
HAVING ((([eft_o]![suma]-Sum(Nz([platifacturi]![platit],0)))<>0) AND ((EFT_O.idclsf) Like [Forms]![EFACTURA_2025]![IdClsf]) AND ((EFT.S)=True) AND ((EFT.CUI)=[Forms]![EFACTURA_2025]![CUIF]) AND ((EF.cui_unit) In (SELECT Cstr(CodFiscal) FROM Cale)));
```

### `QEF_Plata` (Append)

```sql
INSERT INTO tmpDoc2017P ( IdClsf, Document, Valoare, ClsfBug, FORMA, ExplicatieOP, CodPartener )
SELECT CLng([forms]![efactura_2025]![idclsf]) AS IdClsf, [forms]![efactura_2025]![ultimulop] AS Document, Sum(tmpFacturi.Platit) AS Valoare, [forms]![efactura_2025]![CLSF] AS Clsf, [forms]![efactura_2025]![forma] AS FORMA, "F " & Trim(ConcatRelated("Regexp([NumarDocument],'\D+')","tmpFacturi","Forma='" & [forms]![efactura_2025]![Forma] & "' AND (Platit<>0)","[DataDocument]"," ") & " " & IIf(Nz([forms]![efactura_2025]![codclient],"")<>"","C " & Trim([forms]![efactura_2025]![codclient]),"") & " " & [forms]![efactura_2025]![felop]) AS ExplicatieOP, [forms]![efactura_2025]![codp] AS Expr1
FROM tmpFacturi
GROUP BY CLng([forms]![efactura_2025]![idclsf]), [forms]![efactura_2025]![ultimulop], [forms]![efactura_2025]![CLSF], [forms]![efactura_2025]![forma], [forms]![efactura_2025]![codp];
```

### `QEF_RestDePlata` (Select)

```sql
SELECT Sum([suma]-Nz([TotalPlati],0)) AS C
FROM (EFT_O LEFT JOIN (SELECT PlatiFacturi.IdFactura, Sum(PlatiFacturi.ValoareOP) AS TotalPlati FROM PlatiFacturi WHERE (((PlatiFacturi.CodP)=[Forms]![EFACTURA_2025]![CodP])) GROUP BY PlatiFacturi.IdFactura)  AS PlatiFacturi ON EFT_O.IdOperatie = PlatiFacturi.IdFactura) INNER JOIN EFT ON EFT_O.IDEFT = EFT.IDEFT
WHERE (((EFT_O.IdUnitate) In (SELECT IdUnitate FROM Cale)))
GROUP BY EFT.CUI, EFT_O.IdClsf
HAVING (((EFT.CUI)=Forms!EFACTURA_2025!CUIF) And ((EFT_O.IdClsf)=Forms!EFACTURA_2025!IdClsf));
```

### `QEF_RestDePlata_Data` (Select)

```sql
SELECT Sum([suma]-Nz([TotalPlati],0)) AS C
FROM (EFT_O LEFT JOIN (SELECT PlatiFacturi.IdFactura, Sum(PlatiFacturi.ValoareOP) AS TotalPlati FROM PlatiFacturi WHERE (((PlatiFacturi.CodP)=[Forms]![EFACTURA_2025]![CodP])) GROUP BY PlatiFacturi.IdFactura)  AS PlatiFacturi ON EFT_O.IdOperatie = PlatiFacturi.IdFactura) INNER JOIN EFT ON EFT_O.IDEFT = EFT.IDEFT
WHERE (((EFT.CUI)=[Forms]![EFACTURA_2025]![CUIF]) AND ((EFT_O.IdClsf)=[Forms]![EFACTURA_2025]![IdClsf]) AND ((Format([DataFact],"mm\/yyyy"))=[Forms]![EFACTURA_2025]![LunaAn]) AND ((EFT_O.IdUnitate) In (SELECT IdUnitate FROM Cale)));
```

### `QEF_SAV` (Append)

```sql
INSERT INTO tmpQEF ( DataIncarcareEF, id_sol, IDEFT, IdOperatie, IDA, IDClsf, CP, CF, Furnizor, Forma, ClsfBug, NrFact, LunaAn, LUNA, DataFact, TotalFactura, TotalSalvat, TotalPlatit, DePlata, DenClsf, Articol, Alineat, Tip )
SELECT EF.data, EFT.id_sol, EFT.IDEFT, EFT_O.IdOperatie, Oper.IDA, Clasificatii.IDClsf, Nz([codpartener],-1) AS CP, EFT.cui AS CF, PART.dp AS Furnizor, Clasificatii.subcapitol AS Forma, concat_ws(".",[capitol],[subcapitol],[articol],[alineat]) AS ClsfBug, EFT.NrFact, Format([datafact],'mm\/yyyy') AS LunaAn, Month([datafact]) AS LUNA, EFT.DataFact, [EFT].[Total]*Nz([EFT].[nc],1) AS TotalFactura, Sum(EFT_O.suma) AS TotalSalvat, Sum(Nz([platifacturi]![platit],0)) AS TotalPlatit, ([EFT]![total]*Nz([eft]![nc],1))-CDbl(Nz(Sum([platifacturi]![platit]),0)) AS DePlata, Clasificatii.Denumire AS DenClsf, Clasificatii.Articol, Clasificatii.Alineat, EFT.Tip
FROM ((Clasificatii INNER JOIN (((EFT INNER JOIN (SELECT First(Parteneri.CodPartener) AS CodPartener, Count(Parteneri.CodPartener) AS C, Parteneri.CodFiscal AS CUI, Max(DenumirePartener) AS DP FROM Parteneri GROUP BY Parteneri.CodFiscal HAVING Parteneri.CodFiscal<>"")  AS PART ON EFT.CUI = PART.CUI) INNER JOIN EF ON EFT.id_sol = EF.id_sol) INNER JOIN EFT_O ON EFT.IDEFT = EFT_O.IDEFT) ON Clasificatii.IDClsf = EFT_O.IdClsf) LEFT JOIN PlatiFacturi ON EFT_O.IdOperatie = PlatiFacturi.IdFactura) INNER JOIN Oper ON EFT_O.IdOperatie = Oper.IdOperatie
WHERE (((EFT_O.Sursa) In (SELECT SectorSursa FROM UNIT)) AND ((EFT_O.IdUnitate) In (SELECT IdUnitate FROM Cale)))
GROUP BY EF.data, EFT.id_sol, EFT.IDEFT, EFT_O.IdOperatie, Oper.IDA, Clasificatii.IDClsf, Nz([codpartener],-1), EFT.cui, PART.dp, Clasificatii.subcapitol, concat_ws(".",[capitol],[subcapitol],[articol],[alineat]), EFT.NrFact, Format([datafact],'mm\/yyyy'), Month([datafact]), EFT.DataFact, [EFT].[Total]*Nz([EFT].[nc],1), Clasificatii.Denumire, Clasificatii.Articol, Clasificatii.Alineat, EFT.Tip, EF.cui_unit, Month([datafact])
HAVING (((EF.cui_unit) In (select cstr(codfiscal) from cale)))
ORDER BY PART.dp, Clasificatii.subcapitol, Clasificatii.Articol, Clasificatii.Alineat, Month([datafact]) DESC , EFT.DataFact DESC;
```

### `QEF_TRECERE` (Append)

```sql
INSERT INTO tmpQEF ( id_sol, IDEFT, AreOperatii, CP, CF, Furnizor, NrFact, LunaAn, LUNA, DataFact, TotalFactura, TotalSalvat, TotalRamas, Tip )
SELECT EFT.id_sol, EFT.IDEFT, [idefto] Is Not Null AS AreOperatii, Nz([codpartener],-1) AS CP, EFT.cui AS CF, DenP.DenumirePT AS Furnizor, EFT.NrFact, Format([datafact],'mm\/yyyy') AS LunaAn, Month([datafact]) AS LUNA, EFT.DataFact, [Total]*IIf([tip]="NC",-1,1) AS TotalFactura, Sum(CDbl(Nz([eft_o]![suma],0))) AS TotalSalvat, Round([total]*IIf([tip]="NC",-1,1)-Sum(CDbl(Nz([eft_o]![suma],0)))) AS TotalRamas, EFT.Tip
FROM (((EFT LEFT JOIN (SELECT First(Parteneri.CodPartener) AS CodPartener, Count(Parteneri.CodPartener) AS C, Parteneri.CodFiscal AS CUI FROM Parteneri GROUP BY Parteneri.CodFiscal HAVING Parteneri.CodFiscal<>"")  AS PART ON EFT.CUI = PART.CUI) INNER JOIN EF ON EFT.id_sol = EF.id_sol) LEFT JOIN EFT_O ON EFT.IDEFT = EFT_O.IDEFT) INNER JOIN (SELECT EFT.CUI, Max(EFT.DenumireP) AS DenumirePT FROM EFT GROUP BY EFT.CUI)  AS DenP ON EFT.CUI = DenP.CUI
WHERE (((Year([DataFact]))=[Forms]![TrecereAN]![AnNou]) AND ((EFT_O.IdUnitate) In (SELECT IdUnitate FROM Cale)))
GROUP BY EFT.id_sol, EFT.IDEFT, [idefto] Is Not Null, Nz([codpartener],-1), EFT.cui, DenP.DenumirePT, EFT.NrFact, Format([datafact],'mm\/yyyy'), Month([datafact]), EFT.DataFact, [Total]*IIf([tip]="NC",-1,1), EFT.Tip, EF.cui_unit, ([Complet]=True Or (Round([EFT]![total]-CDbl(Nz([eft_o]![suma],0)),2)=0 And [Total]<>0))
HAVING (((EF.cui_unit) In (select cstr(codfiscal) from cale)) AND ((([Complet]=True Or (Round([EFT]![total]-CDbl(Nz([eft_o]![suma],0)),2)=0 And [Total]<>0)))=False))
ORDER BY DenP.DenumirePT, Month([datafact]) DESC , EFT.DataFact DESC;
```

### `Q_1` (Union)

```sql
SELECT '8030000' & [ss1] AS Cont, Q_7.dataoperatie AS Data, 0 as SID, 0 as SIC, Q_7.sumadebit AS RCD, Q_7.sumacredit AS RCC
FROM Q_7
GROUP BY '8030000' & [ss1], Q_7.dataoperatie, 0, 0, Q_7.sumadebit, Q_7.sumacredit
HAVING (((Q_7.dataoperatie) Between [forms]![balanta]![di] And [forms]![balanta]![dsf]) AND ((Q_7.sumadebit)<>0)) OR (((Q_7.dataoperatie) Between [forms]![balanta]![di] And [forms]![balanta]![dsf]) AND ((Q_7.sumacredit)<>0))
ORDER BY Q_7.dataoperatie;
 UNION ALL SELECT '8060000' & [ss1] & [clsffsal] & [clsfesal] AS Cont, q_2.Data, 0 AS SID, 0 AS SIC, IIf(CDbl([Suma])>0,CDbl([suma]),0) AS RCD, IIf(CDbl([Suma])<0,Abs(CDbl([suma])),0) AS RCC
FROM q_2 INNER JOIN qCLSFe ON q_2.IDClsf = qCLSFe.IDClsf
GROUP BY '8060000' & [ss1] & [clsffsal] & [clsfesal], q_2.Data, 0, 0, IIf(CDbl([Suma])>0,CDbl([suma]),0), IIf(CDbl([Suma])<0,Abs(CDbl([suma])),0), q_2.IDClsf
HAVING (((IIf(CDbl([Suma])>0,CDbl([suma]),0))<>0)) OR (((IIf(CDbl([Suma])<0,Abs(CDbl([suma])),0))<>0))

UNION ALL SELECT '8060000' & [ss1] & [clsffsal] & [clsfesal] AS Cont, Rectificari.Data, 0 AS SID, 0 AS SIC, Abs(IIf(Nz([Trim1],0)+Nz([Trim2],0)+Nz([Trim3],0)+Nz([Trim4],0)>0,CDbl(Nz([Trim1],0)+Nz([Trim2],0)+Nz([Trim3],0)+Nz([Trim4],0)),0)) AS RCD, Abs(IIf(Nz([Trim1],0)+Nz([Trim2],0)+Nz([Trim3],0)+Nz([Trim4],0)<0,CDbl(Nz([Trim1],0)+Nz([Trim2],0)+Nz([Trim3],0)+Nz([Trim4],0)),0)) AS RCC
FROM Rectificari INNER JOIN qCLSF ON Rectificari.IdClsf = qCLSF.IDClsf
IN "" [MS Access;DATABASE=C:\Avacont\Gradinita_23_5004\baza2020.mdb;Pwd=andreI]
GROUP BY '8060000' & [ss1] & [clsffsal] & [clsfesal], Rectificari.Data, 0, 0, Abs(IIf(Nz([Trim1],0)+Nz([Trim2],0)+Nz([Trim3],0)+Nz([Trim4],0)>0,CDbl(Nz([Trim1],0)+Nz([Trim2],0)+Nz([Trim3],0)+Nz([Trim4],0)),0)), Abs(IIf(Nz([Trim1],0)+Nz([Trim2],0)+Nz([Trim3],0)+Nz([Trim4],0)<0,CDbl(Nz([Trim1],0)+Nz([Trim2],0)+Nz([Trim3],0)+Nz([Trim4],0)),0)) HAVING (((Rectificari.Data)<=[forms]![balanta]![dsf]))
 UNION ALL SELECT '8062000' & [ss1] & [clsffsal] & [clsfesal] AS Cont, Dispozitii.Data, 0 as SID, 0 as SIC, Abs(IIf(CDbl([Suma])>0,CDbl([suma]),0)) AS RCD, Abs(IIf(CDbl([Suma])<0,CDbl([suma]),0)) AS RCC
FROM (Dispozitii INNER JOIN dispozitii_sub ON (Dispozitii.CAP = dispozitii_sub.CAP) AND (Dispozitii.CCB = dispozitii_sub.CCB) AND (Dispozitii.Numar = dispozitii_sub.Numar)) INNER JOIN qCLSF ON dispozitii_sub.Clsf = qCLSF.ClsfBug
IN "" [MS Access;DATABASE=C:\Avacont\Gradinita_23_5004\baza2020.mdb;Pwd=andreI]
GROUP BY '8062000' & [ss1] & [clsffsal] & [clsfesal], Dispozitii.Data, 0,0, IIf(CDbl([Suma])>0,CDbl([suma]),0), IIf(CDbl([Suma])<0,CDbl([suma]),0)
HAVING (((Dispozitii.Data) Between [forms]![balanta]![di] And [forms]![balanta]![dsf]) AND ((IIf(CDbl([Suma])>0,CDbl([suma]),0))<>0)) OR (((Dispozitii.Data) Between [forms]![balanta]![di] And [forms]![balanta]![dsf]) AND ((IIf(CDbl([Suma])<0,CDbl([suma]),0))<>0))
 UNION ALL SELECT '8066000' & [ss1] & [clsffsal] & [clsfesal] AS Cont, TmpFCEBalanta.Data, 0 AS SID, 0 AS SIC, TmpFCEBalanta.rcd AS RCD, 0 AS RCC
FROM qCLSFe INNER JOIN TmpFCEBalanta ON qCLSFe.IDClsf = TmpFCEBalanta.IDClsf
WHERE (((TmpFCEBalanta.OD2)=2)) OR (((TmpFCEBalanta.OD2)=2))
GROUP BY '8066000' & [ss1] & [clsffsal] & [clsfesal], TmpFCEBalanta.Data, 0, 0, TmpFCEBalanta.rcd, 0, TmpFCEBalanta.IDClsf
HAVING (((TmpFCEBalanta.Data) Between [forms]![balanta]![di] And [forms]![balanta]![dsf]) AND ((TmpFCEBalanta.rcd)<>0)) OR (((TmpFCEBalanta.Data) Between [forms]![balanta]![di] And [forms]![balanta]![dsf]) AND ((0)<>0));
 UNION ALL SELECT X.Cont, X.Data, Sum(X.SIDx) AS SumOfSIDx, Sum(X.SICx) AS SumOfSICx, Sum(X.RCD) AS SumOfRCD, Sum(X.RCC) AS SumOfRCC
FROM (
SELECT '8067000' & [ss1] & [clsffsal] & [clsfesal] AS Cont, q_8.Data, 0 AS SIDx, 0 AS SICx, q_8.valoare AS RCD, 0 AS RCC
FROM q_8 INNER JOIN qCLSFe ON q_8.IdClsf = qCLSFe.IDClsf
GROUP BY '8067000' & [ss1] & [clsffsal] & [clsfesal], q_8.Data, 0, 0, q_8.valoare, 0, q_8.ID
HAVING (((q_8.Data) Between [forms]![balanta]![di] And [forms]![balanta]![dsf]))

UNION ALL SELECT '8067000' & [ss1] & [clsffsal] & [clsfesal] AS Cont, tmp_q9.LunaDeclaratie, 0 AS SIDx, 0 AS SICx, tmp_q9.valoarecab AS RCD, 0 AS RCC
FROM tmp_q9 INNER JOIN qCLSFe ON tmp_q9.IdClsf = qCLSFe.IDClsf
WHERE ((('8067000' & [ss1] & [clsffsal] & [clsfesal] & "_" & [codpartener]) Not In (SELECT '8067000' & [ss1] & [clsffsal] & [clsfesal] & "_" & [CodPartener] AS Cont
FROM q_8 INNER JOIN qClsfE ON q_8.IdClsf = qClsfE.IDClsf
WHERE (((q_8.Data) Between [forms]![balanta]![di] And [forms]![balanta]![dsf])))))
GROUP BY '8067000' & [ss1] & [clsffsal] & [clsfesal], tmp_q9.LunaDeclaratie, 0, 0, tmp_q9.valoarecab, 0, tmp_q9.ID
HAVING (((tmp_q9.LunaDeclaratie) Between [forms]![balanta]![di] And [forms]![balanta]![dsf]))

UNION ALL SELECT '8067000' & [ss1] & [clsffsal] & [clsfesal] AS Cont, TmpFCEBalanta.Data, sid AS SIDx, sic AS SICx, 0 AS RCD, 0 AS RCC
FROM TmpFCEBalanta INNER JOIN qCLSFe ON TmpFCEBalanta.IDClsf = qCLSFe.IDClsf
WHERE (((TmpFCEBalanta.OD2)=1) AND (([sid]+[sic]+[rcd]+[rcc])<>0))
GROUP BY '8067000' & [ss1] & [clsffsal] & [clsfesal], TmpFCEBalanta.Data, sid, sic, 0, 0, TmpFCEBalanta.ID
HAVING (((TmpFCEBalanta.Data) Between [forms]![balanta]![di] And [forms]![balanta]![dsf]))

UNION ALL SELECT '8067000' & [ss1] & [clsffsal] & [clsfesal] AS Cont, #12/31/2017# AS Expr1, 0 AS SIDx, 0 AS SICx, qCLSFe.rf AS RCD, 0 AS RCC
FROM qCLSFe
GROUP BY '8067000' & [ss1] & [clsffsal] & [clsfesal], #12/31/2017#, 0, 0, qCLSFe.rf, 0
HAVING (((qCLSFe.rf)<>0))
)  AS X
GROUP BY X.Cont, X.Data;
 UNION ALL SELECT '8090000' & "02E" & [clsf] AS Cont, Q_13.Data, 0 AS SID, 0 AS SIC, IIf(CDbl([Suma])>0,CDbl([suma]),0) AS RCD, IIf(CDbl([Suma])<0,Abs(CDbl([suma])),0) AS RCC
FROM (SELECT ClasificatiiV.IdClsfV, nz([Trim1],0) AS Suma, DateValue("01/01/" & Year(forms!balanta!dsf)) AS Data, capitol & subcapitol & paragraf as CLSF
FROM ClasificatiiV
IN "" [MS Access;DATABASE=C:\Avacont\Gradinita_23_5004\baza2020.mdb;Pwd=andreI]
GROUP BY ClasificatiiV.IdClsfV, nz([Trim1],0), DateValue("01/01/" & Year(forms!balanta!dsf)) , capitol & subcapitol & paragraf

UNION ALL SELECT ClasificatiiV.IdClsfV, nz([Trim2],0) AS Suma, DateValue("01/04/" & Year(forms!balanta!dsf)) AS Data, capitol & subcapitol & paragraf
FROM ClasificatiiV
IN "" [MS Access;DATABASE=C:\Avacont\Gradinita_23_5004\baza2020.mdb;Pwd=andreI]
GROUP BY ClasificatiiV.IdClsfV, nz([Trim2],0), DateValue("01/04/" & Year(forms!balanta!dsf)) , capitol & subcapitol & paragraf

UNION ALL SELECT ClasificatiiV.IdClsfV, nz([Trim3],0) AS Suma, DateValue("01/07/" & Year(forms!balanta!dsf)) AS Data, capitol & subcapitol & paragraf
FROM ClasificatiiV
IN "" [MS Access;DATABASE=C:\Avacont\Gradinita_23_5004\baza2020.mdb;Pwd=andreI]
GROUP BY ClasificatiiV.IdClsfV, nz([Trim3],0), DateValue("01/07/" & Year(forms!balanta!dsf)) , capitol & subcapitol & paragraf

UNION ALL SELECT ClasificatiiV.IdClsfV, nz([Trim4],0) AS Suma, DateValue("01/10/" & Year(forms!balanta!dsf)) AS Data, capitol & subcapitol & paragraf
FROM ClasificatiiV
IN "" [MS Access;DATABASE=C:\Avacont\Gradinita_23_5004\baza2020.mdb;Pwd=andreI]
GROUP BY ClasificatiiV.IdClsfV, nz([Trim4],0), DateValue("01/10/" & Year(forms!balanta!dsf)), capitol & subcapitol & paragraf
)  AS Q_13
GROUP BY '8090000' & "02E" & [clsf], Q_13.Data, 0, IIf(CDbl([Suma])>0,CDbl([suma]),0), IIf(CDbl([Suma])<0,Abs(CDbl([suma])),0), 0, Q_13.IDClsfV
HAVING (((IIf(CDbl([Suma])>0,CDbl([suma]),0))<>0)) OR (((IIf(CDbl([Suma])<0,Abs(CDbl([suma])),0))<>0))


UNION ALL SELECT '8090000' & [ss1] & [ShortClsfBug] AS Cont, RectificariV.Data, 0 AS SID, 0 AS SIC, Abs(IIf(Nz([Trim1],0)+Nz([Trim2],0)+Nz([Trim3],0)+Nz([Trim4],0)>0,CDbl(Nz([Trim1],0)+Nz([Trim2],0)+Nz([Trim3],0)+Nz([Trim4],0)),0)) AS RCD, Abs(IIf(Nz([Trim1],0)+Nz([Trim2],0)+Nz([Trim3],0)+Nz([Trim4],0)<0,CDbl(Nz([Trim1],0)+Nz([Trim2],0)+Nz([Trim3],0)+Nz([Trim4],0)),0)) AS RCC
FROM RectificariV INNER JOIN qCLSFV ON RectificariV.IdClsfV = qCLSFV.IDClsfV
IN "" [MS Access;DATABASE=C:\Avacont\Gradinita_23_5004\baza2020.mdb;Pwd=andreI]
GROUP BY '8090000' & [ss1] & [ShortClsfBug], RectificariV.Data, 0, 0, Abs(IIf(Nz([Trim1],0)+Nz([Trim2],0)+Nz([Trim3],0)+Nz([Trim4],0)>0,CDbl(Nz([Trim1],0)+Nz([Trim2],0)+Nz([Trim3],0)+Nz([Trim4],0)),0)), Abs(IIf(Nz([Trim1],0)+Nz([Trim2],0)+Nz([Trim3],0)+Nz([Trim4],0)<0,CDbl(Nz([Trim1],0)+Nz([Trim2],0)+Nz([Trim3],0)+Nz([Trim4],0)),0)) HAVING (((RectificariV.Data)<=[forms]![balanta]![dsf]));
```

### `Q_13` (Union)

```sql
SELECT ClasificatiiV.IdClsfV, nz([Trim1],0) AS Suma, DateValue("01/01/" & Year(forms!balanta!dsf)) AS Data
FROM ClasificatiiV
IN "" [MS Access;DATABASE=C:\Avacont\Gradinita_23_5004\baza2020.mdb;Pwd=andreI]
GROUP BY ClasificatiiV.IdClsfV, nz([Trim1],0), DateValue("01/01/" & Year(forms!balanta!dsf)) 

UNION ALL SELECT ClasificatiiV.IdClsfV, nz([Trim2],0) AS Suma, DateValue("01/04/" & Year(forms!balanta!dsf)) AS Data
FROM ClasificatiiV
IN "" [MS Access;DATABASE=C:\Avacont\Gradinita_23_5004\baza2020.mdb;Pwd=andreI]
GROUP BY ClasificatiiV.IdClsfV, nz([Trim2],0), DateValue("01/04/" & Year(forms!balanta!dsf)) 

UNION ALL SELECT ClasificatiiV.IdClsfV, nz([Trim3],0) AS Suma, DateValue("01/07/" & Year(forms!balanta!dsf)) AS Data
FROM ClasificatiiV
IN "" [MS Access;DATABASE=C:\Avacont\Gradinita_23_5004\baza2020.mdb;Pwd=andreI]
GROUP BY ClasificatiiV.IdClsfV, nz([Trim3],0), DateValue("01/07/" & Year(forms!balanta!dsf)) 

UNION ALL SELECT ClasificatiiV.IdClsfV, nz([Trim4],0) AS Suma, DateValue("01/10/" & Year(forms!balanta!dsf)) AS Data
FROM ClasificatiiV
IN "" [MS Access;DATABASE=C:\Avacont\Gradinita_23_5004\baza2020.mdb;Pwd=andreI]
GROUP BY ClasificatiiV.IdClsfV, nz([Trim4],0), DateValue("01/10/" & Year(forms!balanta!dsf));
```

### `Q_2` (Union)

```sql
SELECT Clasificatii.IDClsf, nz([Trim1],0) AS Suma, DateValue("01/01/" & Year(forms!balanta!dsf)) AS Data
FROM Clasificatii
IN "" [MS Access;DATABASE=C:\Avacont\Gradinita_23_5004\baza2020.mdb;Pwd=andreI]
GROUP BY Clasificatii.IDClsf, nz([Trim1],0), DateValue("01/01/" & Year(forms!balanta!dsf)) 

UNION ALL SELECT Clasificatii.IDClsf, nz([Trim2],0) AS Suma, DateValue("01/04/" & Year(forms!balanta!dsf)) AS Data
FROM Clasificatii
IN "" [MS Access;DATABASE=C:\Avacont\Gradinita_23_5004\baza2020.mdb;Pwd=andreI]
GROUP BY Clasificatii.IDClsf, nz([Trim2],0), DateValue("01/04/" & Year(forms!balanta!dsf)) 

UNION ALL SELECT Clasificatii.IDClsf, nz([Trim3],0) AS Suma, DateValue("01/07/" & Year(forms!balanta!dsf)) AS Data
FROM Clasificatii
IN "" [MS Access;DATABASE=C:\Avacont\Gradinita_23_5004\baza2020.mdb;Pwd=andreI]
GROUP BY Clasificatii.IDClsf, nz([Trim3],0), DateValue("01/07/" & Year(forms!balanta!dsf)) 

UNION ALL SELECT Clasificatii.IDClsf, nz([Trim4],0) AS Suma, DateValue("01/10/" & Year(forms!balanta!dsf)) AS Data
FROM Clasificatii
IN "" [MS Access;DATABASE=C:\Avacont\Gradinita_23_5004\baza2020.mdb;Pwd=andreI]
GROUP BY Clasificatii.IDClsf, nz([Trim4],0), DateValue("01/10/" & Year(forms!balanta!dsf));
```

### `Q_7` (Select)

```sql
SELECT CT8030.*, Proiecte.SursaFinantare AS SS1
FROM CT8030, Proiecte IN '' [MS Access;DATABASE=C:\Avacont\Gradinita_23_5004\baza2020.mdb;Pwd=andreI];
```

### `Q_8` (Union)

```sql
SELECT Contracte.ID, Contracte.NrCrt, Contracte.Denumire, Contracte.NrDoc, Contracte.Data, Contracte.Explicatie, Contracte.Valoare, Contracte.CodAngajament, Contracte.CodPartener, Contracte.IdClsf, Contracte.CodIndicator
FROM Contracte IN '' [MS Access;DATABASE=C:\Avacont\Gradinita_23_5004\baza2020.mdb;Pwd=andreI]
WHERE (((nz([codpartener],''))<>'') AND ((nz([idclsf],''))<>''));

UNION ALL SELECT Contracte.ID, Contracte.NrCrt, Contracte.Denumire, Contracte.NrDoc, Contracte_A.DataAjustare, Contracte.Explicatie, Contracte_A.ajustare, Contracte.CodAngajament, Contracte.CodPartener, Contracte.IdClsf, Contracte.CodIndicator
FROM Contracte INNER JOIN Contracte_A ON Contracte.NrCrt = Contracte_A.NrCrt IN '' [MS Access;DATABASE=C:\Avacont\Gradinita_23_5004\baza2020.mdb;Pwd=andreI]
WHERE (((Contracte_A.DataAjustare) Between [forms]![balanta]![di] And [forms]![balanta]![dsf]) AND ((nz([codpartener],''))<>'') AND ((nz([idclsf],''))<>''));
```

### `SAV_OPER_EF` (Append)

```sql
INSERT INTO EFT_O ( IdOperatie, IDEFT, IdClsf, Sursa, Suma, IdUnitate )
SELECT tmpFacturi.IdOperatie, tmpFacturi.IDEFT, tmpFacturi.IdClsf, DLookUp("SectorSursa","Unit") AS Sursa, tmpFacturi.Suma, DLookUp("IdUnitate","Cale")
FROM tmpFacturi
WHERE (((tmpFacturi.IDEFT) Is Not Null));
```

### `SAV_OPER_EF_2017` (Append)

```sql
INSERT INTO EFT_O ( IdOperatie, IDEFT, IdClsf, Sursa, Suma )
SELECT tmpFacturi.IdOperatie, tmpFacturi.IDEFT, tmpFacturi.IdClsf, DLookUp("SectorSursa","Unit") AS Sursa, tmpFacturi.Suma
FROM tmpFacturi
WHERE (((tmpFacturi.IDEFT) Is Not Null));
```

### `UEFP` (Union)

```sql
SELECT Nz([CodPartener],"-1") AS CP, Nz([DenumirePartener],[denumirep]) AS Furnizor, IIf([CodFisc]<>'',[CodFisc],[cui]) AS CF, Nz([parteneri]![Adresa],[eft]![adresa]) AS Adr, EFT.NrFact AS Expl, TOTAL, Parteneri.ContIBAN, Parteneri.Banca, EFT.S
FROM ((SELECT Parteneri.CodPartener, Parteneri.DenumirePartener, Replace(Replace(Replace(Nz([CodFiscal],""),"RO",""),"R",""),Space(1),"") AS CodFisc, Parteneri.ContIBAN, Parteneri.Banca, Parteneri.Adresa FROM Parteneri)  AS Parteneri RIGHT JOIN EF ON Parteneri.CodFisc = EF.CIF) INNER JOIN EFT ON EF.id_sol = EFT.id_sol
GROUP BY Nz([CodPartener],"-1"), Nz([DenumirePartener],[denumirep]), IIf([CodFisc]<>'',[CodFisc],[cui]), Nz([parteneri]![Adresa],[eft]![adresa]), EFT.NrFact, TOTAL, Parteneri.ContIBAN, Parteneri.Banca, EFT.S
HAVING (((EFT.S)=True));

UNION ALL SELECT Nz([CodPartener],"-1") AS CP, Nz([DenumirePartener],[denumirep]) AS Furnizor, IIf([CodFisc]<>'',[CodFisc],[cui]) AS CF, Nz([parteneri]![Adresa],[eft]![adresa]) AS Adr, EFT.NrFact, roundcut(Sum(([efs]![Valoare])*((100+[cotatva])/100))) AS TOTAL, Parteneri.ContIBAN, Parteneri.Banca, EFS.S
FROM (((SELECT Parteneri.CodPartener, Parteneri.DenumirePartener, Replace(Replace(Replace(Nz([CodFiscal],""),"RO",""),"R",""),Space(1),"") AS CodFisc, Parteneri.ContIBAN, Parteneri.Banca, Parteneri.Adresa FROM Parteneri)  AS Parteneri RIGHT JOIN EF ON Parteneri.CodFisc = EF.CIF) INNER JOIN EFT ON EF.id_sol = EFT.id_sol) INNER JOIN EFS ON EFT.IDEFT = EFS.IDEFT
WHERE (((EFT.S)=False))
GROUP BY Nz([CodPartener],"-1"), Nz([DenumirePartener],[denumirep]), IIf([CodFisc]<>'',[CodFisc],[cui]), Nz([parteneri]![Adresa],[eft]![adresa]), EFT.NrFact, Parteneri.ContIBAN, Parteneri.Banca, EFS.S
HAVING (((EFS.S)=True));
```

### `_qNote_EFactura_Asociat_Nou` (Append)

```sql
INSERT INTO tmpnote_efactura ( IDEF, IDEFT, id_sol, id, NrFact, DataFact, CotaTVA, TVA, Valoare, TOTAL, DenumireP, AreAtt, AreXml, S, Asociat, DeAsociat )
SELECT EF.IDEF, EFT.IDEFT, EF.id_sol, EF.id, EFT.NrFact, EFT.DataFact, EFT.CotaTVA, [TVA]*[nc] AS xTVA, [Valoare]*[nc] AS xValoare, [TOTAL]*[nc] AS xTOTAL, EFT.DenumireP, Len([Attach])<>0 AS AreAtt, Len([xml])<>0 AS AreXml, True AS S, Sum(mmin([total]*[nc],[forms]![note]![note_credit]![sumacredit])) AS Asociat, ([total]*[nc])-mmin(([total]*[nc]),[forms]![note]![note_credit]![sumacredit]) AS DeAsociat
FROM EF INNER JOIN EFT ON EF.id_sol = EFT.id_sol
GROUP BY EF.IDEF, EFT.IDEFT, EF.id_sol, EF.id, EFT.NrFact, EFT.DataFact, EFT.CotaTVA, [TVA]*[nc], [Valoare]*[nc], [TOTAL]*[nc], EFT.DenumireP, Len([Attach])<>0, Len([xml])<>0, True, EFT.Tip
HAVING (((EFT.IDEFT)=[forms]![Note]![Note_EFactura]![IDEFT]));
```

### `_qNote_EFactura_Initial` (Append)

```sql
INSERT INTO tmpnote_efactura ( IDEF, IDEFT, id_sol, id, NrFact, DataFact, CotaTVA, TVA, Valoare, TOTAL, DenumireP, AreAtt, AreXml, S, Asociat, DeAsociat )
SELECT EF.IDEF, EFT.IDEFT, EF.id_sol, EF.id, EFT.NrFact, EFT.DataFact, EFT.CotaTVA, [tva]*[nc] AS xTVA, [Valoare]*[nc] AS xValoare, [TOTAL]*[nc] AS xTotal, EFT.DenumireP, Len([Attach])<>0 AS AreAtt, Len([xml])<>0 AS AreXml, True AS S, Sum(EFTO.Suma) AS Asociat, ([total]*[nc])-Sum(Nz([suma],0)) AS DeAsociat
FROM (EF INNER JOIN EFT ON EF.id_sol = EFT.id_sol) INNER JOIN (SELECT * FROM EFT_O WHERE IdOperatie=[forms]![Note]![IDO])  AS EFTO ON EFT.IDEFT = EFTO.IDEFT
GROUP BY EF.IDEF, EFT.IDEFT, EF.id_sol, EF.id, EFT.NrFact, EFT.DataFact, EFT.CotaTVA, [tva]*[nc], [Valoare]*[nc], [TOTAL]*[nc], EFT.DenumireP, Len([Attach])<>0, Len([xml])<>0, True, EFT.Tip;
```

### `_qNote_EFacturat_Ramas` (Append)

```sql
INSERT INTO tmpNote_EFactura ( IDEF, IDEFT, id_sol, id, NrFact, DataFact, CotaTVA, TVA, Valoare, TOTAL, DenumireP, AreAtt, AreXml, S, Asociat, DeAsociat )
SELECT EF.IDEF, EFT.IDEFT, EF.id_sol, EF.id, EFT.NrFact, EFT.DataFact, EFT.CotaTVA, [TVA]*[nc] AS xTVA, [Valoare]*[nc] AS xValoare, [TOTAL]*[nc] AS xTOTAL, EFT.DenumireP, Len([Attach])<>0 AS AreAtt, Len([xml])<>0 AS AreXml, False AS S, Sum(EFT_O.Suma) AS Asociat, ([total]*[nc])-Sum(Nz([suma],0)) AS DeAsociat
FROM (EF INNER JOIN EFT ON EF.id_sol = EFT.id_sol) LEFT JOIN EFT_O ON EFT.IDEFT = EFT_O.IDEFT
GROUP BY EF.IDEF, EFT.IDEFT, EF.id_sol, EF.id, EFT.NrFact, EFT.DataFact, EFT.CotaTVA, [TVA]*[nc], [Valoare]*[nc], [TOTAL]*[nc], EFT.DenumireP, Len([Attach])<>0, Len([xml])<>0, False, EFT_O.IdOperatie, EFT.CUI, EFT.Tip
HAVING (((([total]*[nc])-Sum(Nz([suma],0)))<>0) AND ((EFT_O.IdOperatie)<>[forms]![Note]![IDO]) AND ((EFT.CUI) In (SELECT CodFiscal FROM Parteneri WHERE CodPartener=forms!note!Part)));
```

### `qCLSFV` (Select)

```sql
SELECT ClasificatiiV.IDClsfv AS Expr1, [capitol] & '.' & [subcapitol] & '.' & [paragraf] AS ClsfBug, ClasificatiiV.Capitol AS Expr2, ClasificatiiV.SubCapitol AS Expr3, ClasificatiiV.Paragraf AS Expr4, [capitol] & [subcapitol] & [paragraf] AS ShortClsfBug, '02' AS Sector, 'E' AS Sursa, '02E' AS ss1
FROM ClasificatiiV
GROUP BY ClasificatiiV.IDClsfv, [capitol] & '.' & [subcapitol] & '.' & [paragraf], ClasificatiiV.Capitol, ClasificatiiV.SubCapitol, ClasificatiiV.Paragraf, [capitol] & [subcapitol] & [paragraf], '02', 'E', '02E';
```

### `qClsfE` (Select)

```sql
SELECT [capitol] & '.' & [subcapitol] & '.' & [articol] & '.' & [alineat] AS ClsfBug, Clasificatii.IDClsf, Clasificatii.Capitol, Clasificatii.Subcapitol, Clasificatii.Articol, Clasificatii.Alineat, First(Clasificatii.Denumire) AS Denumire, Left([capitol],2) & '.' & [subcapitol] & '.' & [articol] & '.' & [alineat] AS ShortClsfBug, [capitol] & '.XX.XX.' & [articol] & '.' & [alineat] AS xClsfBug, Left([capitol],2) & Replace([subcapitol] & [articol] & [alineat],'.','') AS ClsfSal, Left([capitol],2) & Replace([subcapitol],'.','') AS ClsfFSal, Replace([articol] & [alineat],'.','') AS ClsfESal, Switch(Right([capitol],2)='02','02',Right([capitol],2)='01','01',Right([capitol],2)='10','02') AS Sector, Switch(Right([capitol],2)='02','A',Right([capitol],2)='01','A',Right([capitol],2)='10','E') AS Sursa, CStr([idclsf]) AS Str_IdClsf, Left([articol],2) AS Titlu, Switch(Right([capitol],2)='02','02',Right([capitol],2)='01','01',Right([capitol],2)='10','02',CInt(Right([Capitol],2))>10,'01') & Switch(Right([capitol],2)='02','A',Right([capitol],2)='01','A',Right([capitol],2)='10','E',Right([capitol],2)='13','E',CInt(Right([Capitol],2))>10,'A') AS ss1
FROM Clasificatii IN '' [MS Access;DATABASE=C:\AVACONT\Local\baza2026.accdb;Pwd=andreI]
GROUP BY [capitol] & '.' & [subcapitol] & '.' & [articol] & '.' & [alineat], Clasificatii.IDClsf, Clasificatii.Capitol, Clasificatii.Subcapitol, Clasificatii.Articol, Clasificatii.Alineat, Left([capitol],2) & '.' & [subcapitol] & '.' & [articol] & '.' & [alineat], [capitol] & '.XX.XX.' & [articol] & '.' & [alineat], Left([capitol],2) & Replace([subcapitol] & [articol] & [alineat],'.',''), Left([capitol],2) & Replace([subcapitol],'.',''), Replace([articol] & [alineat],'.',''), Switch(Right([capitol],2)='02','02',Right([capitol],2)='01','01',Right([capitol],2)='10','02'), Switch(Right([capitol],2)='02','A',Right([capitol],2)='01','A',Right([capitol],2)='10','E'), CStr([idclsf]), Left([articol],2), Switch(Right([capitol],2)='02','02',Right([capitol],2)='01','01',Right([capitol],2)='10','02',CInt(Right([Capitol],2))>10,'01') & Switch(Right([capitol],2)='02','A',Right([capitol],2)='01','A',Right([capitol],2)='10','E',Right([capitol],2)='13','E',CInt(Right([Capitol],2))>10,'A');
```

### `qDefaCont_Bilant` (Select)

```sql
SELECT DefaCont.Cont AS DC_Cont, DefaCont.Tip AS DC_Tip
FROM DefaCont
WHERE (((Len([cont]))=7));
```

### `qDefaNote` (Select)

```sql
SELECT Parteneri.DenumirePartener, DefaNoteA.Explicatie, DefaNoteA.Jurnal, DefaNoteA.Fel, DefaNoteA.Compartiment, DefaNote.Clsf, DefaNote.ContDebit, DefaNote.PartDebit, DefaNote.ContCredit, DefaNote.PartCredit, DefaNote.Ordonantare, DefaNote.PA, DefaNote.Chelt, [capitol] & "." & [subcapitol] & "." & [articol] & "." & [alineat] AS ClsfBug
FROM ((DefaNoteA INNER JOIN Parteneri ON DefaNoteA.CodPartener = Parteneri.CodPartener) INNER JOIN DefaNote ON DefaNoteA.IDA = DefaNote.IDA) LEFT JOIN Clasificatii ON DefaNote.Clsf = Clasificatii.IDClsf
WHERE (((DefaNoteA.IDA)=forms!oci!ida) And ((DefaNoteA.CodPartener)=forms!oci!cp));
```

### `qFacturi_Vanzare` (Select)

```sql
SELECT Factura.IdFactura, Factura.SerieFactura, Factura.NumarFactura, Factura.DataFactura, Factura.Comentarii, Factura.BT_13, Factura.ContPlata, ClientiEF.IdClient, ClientiEF.DenumireClient AS Client, ClientiEF.IndFiscal AS Client_RO, ClientiEF.CodFiscal AS Client_CUI, ClientiEF.Cont AS Client_Cont, ClientiEF.Banca AS Client_Banca, ClientiEF.Adresa AS Client_Adresa, ClientiEF.Judetul AS Client_Judetul, ClientiEF.Orasul AS Client_Orasul, ClientiEF.Sector AS Client_Sector, FacturaC.IdContinut, FacturaC.Continut, FacturaC.Um, FacturaC.Cant, FacturaC.PU, FacturaC.Valoare, FacturaC.Platit, FacturaC.NrCrt, Factura.TipFactura, FacturaC.Grup, UNIT.NumeUnitate AS Furnizor, UNIT.cui AS Furnizor_CUI, UNIT.Orasul AS Furnizor_Orasul, UNIT.Adresa AS Furnizor_Adresa, UNIT.Director AS Furnizor_Reprezentant, UNIT.AdresaMail AS Furnizor_Mail, UNIT.TelefonContact AS Furnizor_Telefon, UNIT.Judetul AS Furnizor_Judetul
FROM UNIT, (Factura INNER JOIN FacturaC ON Factura.IdFactura = FacturaC.IdFactura) INNER JOIN ClientiEF ON Factura.IdClient = ClientiEF.IdClient;
```

### `qMF_EFactura_Initial` (Append)

```sql
INSERT INTO tmpMF_EFactura ( IDEF, IDEFT, id_sol, id, NrFact, DataFact, CotaTVA, TVA, Valoare, TOTAL, DenumireP, AreAtt, AreXml, S, Asociat, DeAsociat, Tip )
SELECT EF.IDEF, EFT.IDEFT, EF.id_sol, EF.id, EFT.NrFact, EFT.DataFact, EFT.CotaTVA, [tva]*[nc] AS xTVA, [Valoare]*[nc] AS xValoare, [TOTAL]*[nc] AS xTotal, EFT.DenumireP, Len([Attach])<>0 AS AreAtt, Len([xml])<>0 AS AreXml, True AS S, Sum(EFTO.Suma) AS Asociat, ([total]*[nc])-Sum(Nz([suma],0)) AS DeAsociat, EFT.Tip
FROM (EF INNER JOIN EFT ON EF.id_sol = EFT.id_sol) INNER JOIN (SELECT * FROM EFT_O WHERE IdOperatie=[forms]![MF2019]!MF2019_1![Rd])  AS EFTO ON EFT.IDEFT = EFTO.IDEFT
GROUP BY EF.IDEF, EFT.IDEFT, EF.id_sol, EF.id, EFT.NrFact, EFT.DataFact, EFT.CotaTVA, [tva]*[nc], [Valoare]*[nc], [TOTAL]*[nc], EFT.DenumireP, Len([Attach])<>0, Len([xml])<>0, True, EFT.Tip;
```

### `qMF_EFactura_Ramas` (Append)

```sql
INSERT INTO tmpMF_EFactura ( IDEF, IDEFT, id_sol, id, NrFact, DataFact, CotaTVA, TVA, Valoare, TOTAL, DenumireP, AreAtt, AreXml, S, Asociat, DeAsociat, Tip )
SELECT EF.IDEF, EFT.IDEFT, EF.id_sol, EF.id, EFT.NrFact, EFT.DataFact, EFT.CotaTVA, [TVA]*[nc] AS xTVA, [Valoare]*[nc] AS xValoare, [TOTAL]*[nc] AS xTOTAL, EFT.DenumireP, Len([Attach])<>0 AS AreAtt, Len([xml])<>0 AS AreXml, False AS S, Sum(EFT_O.Suma) AS Asociat, ([total]*[nc])-Sum(Nz([suma],0)) AS DeAsociat, EFT.Tip
FROM (EF INNER JOIN EFT ON EF.id_sol = EFT.id_sol) LEFT JOIN EFT_O ON EFT.IDEFT = EFT_O.IDEFT
GROUP BY EF.IDEF, EFT.IDEFT, EF.id_sol, EF.id, EFT.NrFact, EFT.DataFact, EFT.CotaTVA, [TVA]*[nc], [Valoare]*[nc], [TOTAL]*[nc], EFT.DenumireP, Len([Attach])<>0, Len([xml])<>0, False, EFT_O.IdOperatie, EFT.CUI, EFT.Tip
HAVING (((([total]*[nc])-Sum(Nz([suma],0)))<>0) AND ((EFT_O.IdOperatie)<>[forms]![MF2019]![MF2019_1]![idoperatie]) AND ((EFT.CUI) In (SELECT CodFiscal FROM Parteneri WHERE CodPartener=forms![MF2019]![MF2019_0]!CodPartener)));
```

### `qMF_EFactura_Toate` (Append)

```sql
INSERT INTO tmpMF_EFactura ( IDEF, IDEFT, id_sol, id, NrFact, DataFact, CotaTVA, TVA, Valoare, TOTAL, DenumireP, AreAtt, AreXml, S, Asociat, DeAsociat, Tip, IdUnitate )
SELECT IDEF, IDEFT, id_sol, id, NrFact, DataFact, CotaTVA, xTVA, xValoare, xTOTAL, DenumireP, AreAtt, AreXml, S, Asociat, DeAsociat, Tip, DLookUp("IdUnitate","Cale") AS IdUnitate
FROM (SELECT
        EF.IDEF,
        EFT.IDEFT,
        EF.id_sol,
        EF.id,
        EFT.NrFact,
        EFT.DataFact,
        EFT.CotaTVA,
        [tva] * [nc] AS xTVA,
        [Valoare] * [nc] AS xValoare,
        [TOTAL] * [nc] AS xTOTAL,
        EFT.DenumireP,
        Len([Attach]) <> 0 AS AreAtt,
        Len([xml]) <> 0 AS AreXml,
        True AS S,
        Sum(EFT_O.Suma) AS Asociat,
        Round(
            Nz(
                DSum(
                    'Suma',
                    'EFT_O',
                    'IDEFT=' & Nz([Forms]![MF2019]![MF2019_1]![IDEFT],0) &
                    ' AND IdOperatie<>' & [Forms]![MF2019]![MF2019_1]![IdOperatie]
                ),
                0
            ),
            2
        ) AS AlteAsocieri,
        ([total] * [nc]) - Sum(Nz([suma], 0)) -
        Round(
            Nz(
                DSum(
                    'Suma',
                    'EFT_O',
                    'IDEFT=' & Nz([Forms]![MF2019]![MF2019_1]![IDEFT],0) &
                    ' AND IdOperatie<>' & [Forms]![MF2019]![MF2019_1]![IdOperatie]
                ),
                0
            ),
            2
        ) AS DeAsociat,
        EFT.Tip,
        DLookUp("IdUnitate","Cale") AS IdUnitateSelect,
        EFT_O.IdUnitate AS IdUnitateOperatie
    FROM
        (EF INNER JOIN EFT ON EF.id_sol = EFT.id_sol)
        LEFT JOIN EFT_O ON EFT.IDEFT = EFT_O.IDEFT
    WHERE
        Nz([IdOperatie], 0) = [Forms]![MF2019]![MF2019_1]![IdOperatie]
    GROUP BY
        EF.IDEF,
        EFT.IDEFT,
        EF.id_sol,
        EF.id,
        EFT.NrFact,
        EFT.DataFact,
        EFT.CotaTVA,
        [tva] * [nc],
        [Valoare] * [nc],
        [TOTAL] * [nc],
        EFT.DenumireP,
        Len([Attach]) <> 0,
        Len([xml]) <> 0,
        EFT.Tip,
        EFT.CUI,
        EF.cui_unit,
        DLookUp("IdUnitate","Cale"),
        EFT_O.IdUnitate
    HAVING
        EFT.CUI In (
            SELECT CodFiscal FROM Parteneri
            WHERE CodPartener = [Forms]![MF2019]![MF2019_0]![CodPartener]
        )
        AND EF.cui_unit = DLookUp("CUI","Unit")
        AND EFT_O.IdUnitate = DLookUp("IdUnitate","Cale")

    UNION ALL

    SELECT
        EF.IDEF,
        EFT.IDEFT,
        EF.id_sol,
        EF.id,
        EFT.NrFact,
        EFT.DataFact,
        EFT.CotaTVA,
        [tva] * [nc] AS xTVA,
        [Valoare] * [nc] AS xValoare,
        [TOTAL] * [nc] AS xTOTAL,
        EFT.DenumireP,
        Len([Attach]) <> 0 AS AreAtt,
        Len([xml]) <> 0 AS AreXml,
        False AS S,
        Sum(Nz([Suma], 0)) AS Asociat,
        0 AS AlteAsocieri,
        ([total] * [nc]) - Sum(Nz([suma], 0)) AS DeAsociat,
        EFT.Tip,
        DLookUp("IdUnitate","Cale") AS IdUnitateSelect,
        EFT_O.IdUnitate AS IdUnitateOperatie
    FROM
        (EF INNER JOIN EFT ON EF.id_sol = EFT.id_sol)
        LEFT JOIN EFT_O ON EFT.IDEFT = EFT_O.IDEFT
    GROUP BY
        EF.IDEF,
        EFT.IDEFT,
        EF.id_sol,
        EF.id,
        EFT.NrFact,
        EFT.DataFact,
        EFT.CotaTVA,
        [tva] * [nc],
        [Valoare] * [nc],
        [TOTAL] * [nc],
        EFT.DenumireP,
        Len([Attach]) <> 0,
        Len([xml]) <> 0,
        0,
        EFT.Tip,
        EFT.CUI,
        EF.cui_unit,
        DLookUp("IdUnitate","Cale"),
        EFT_O.IdUnitate
    HAVING
        EFT.IDEFT <> Nz([Forms]![MF2019]![MF2019_1]![IDEFT],0)
        AND (([total] * [nc]) - Sum(Nz([suma], 0))) <> 0
        AND EFT.CUI In (
            SELECT CodFiscal FROM Parteneri
            WHERE CodPartener = [Forms]![MF2019]![MF2019_0]![CodPartener]
        )
        AND EF.cui_unit = DLookUp("CUI","Unit")
)  AS T;
```

### `qNote_EFactura_Toate` (Append)

```sql
INSERT INTO tmpNote_EFactura ( IDEF, IDEFT, id_sol, id, NrFact, DataFact, CotaTVA, TVA, Valoare, TOTAL, DenumireP, AreAtt, AreXml, S, Asociat, DeAsociat, Tip )
SELECT IDEF, IDEFT, id_sol, id, NrFact, DataFact, CotaTVA, xTVA, xValoare, xTOTAL, DenumireP, AreAtt, AreXml, S, Asociat, DeAsociat, Tip
FROM (SELECT
            EF.IDEF,
            EFT.IDEFT,
            EF.id_sol,
            EF.id,
            EFT.NrFact,
            EFT.DataFact,
            EFT.CotaTVA,
            [tva] * [nc] AS xTVA,
            [Valoare] * [nc] AS xValoare,
            [TOTAL] * [nc] AS xTOTAL,
            EFT.DenumireP,
            Len([Attach]) <> 0 AS AreAtt,
            Len([xml]) <> 0 AS AreXml,
            True AS S,
            Sum(EFT_O.Suma) AS Asociat,
            Round(
                Nz(
                    DSum(
                        'Suma',
                        'EFT_O',
                        'IDEFT=' & Nz([Forms]![Note]![IDEFT],0) & ' AND IdOperatie<>' & [Forms]![Note]![IDO] & ''
                    ),
                    0
                ),
                2
            ) AS AlteAsocieri,
            ([total] * [nc]) - Sum(Nz([suma], 0)) - Round(
                Nz(
                    DSum(
                        'Suma',
                        'EFT_O',
                        'IDEFT=' & Nz([Forms]![Note]![IDEFT],0) & ' AND IdOperatie<>' & [Forms]![Note]![IDO] & ''
                    ),
                    0
                ),
                2
            ) AS DeAsociat,
            EFT.Tip
        FROM
            (
                EF
                INNER JOIN EFT ON EF.id_sol = EFT.id_sol
            )
            LEFT JOIN EFT_O ON EFT.IDEFT = EFT_O.IDEFT
        WHERE
            (((Nz([IdOperatie], 0)) = [Forms]![Note]![IDO]))
        GROUP BY
            EF.IDEF,
            EFT.IDEFT,
            EF.id_sol,
            EF.id,
            EFT.NrFact,
            EFT.DataFact,
            EFT.CotaTVA,
            [tva] * [nc],
            [Valoare] * [nc],
            [TOTAL] * [nc],
            EFT.DenumireP,
            Len([Attach]) <> 0,
            Len([xml]) <> 0,
            Round(
                Nz(
                    DSum(
                        'Suma',
                        'EFT_O',
                        'IDEFT=' & Nz([Forms]![Note]![IDEFT],0) & ' AND IdOperatie<>' & [Forms]![Note]![IDO] & ''
                    ),
                    0
                ),
                2
            ),
            EFT.Tip,
            False,
            EFT.CUI,
            EF.cui_unit
        HAVING
            (
                (
                    (EFT.CUI) In (
                        SELECT
                            CodFiscal
                        FROM
                            Parteneri
                        WHERE
                            CodPartener = forms!note!Part
                    )
                )
                AND ((EF.cui_unit) = DLookUp("CUI", "Unit"))
            )
        UNION
        ALL
        SELECT
            EF.IDEF,
            EFT.IDEFT,
            EF.id_sol,
            EF.id,
            EFT.NrFact,
            EFT.DataFact,
            EFT.CotaTVA,
            [tva] * [nc] AS xTVA,
            [Valoare] * [nc] AS xValoare,
            [TOTAL] * [nc] AS xTOTAL,
            EFT.DenumireP,
            Len([Attach]) <> 0 AS AreAtt,
            Len([xml]) <> 0 AS AreXml,
            False AS S,
            Sum(Nz([Suma], 0)) AS Asociat,
            0 AS AlteAsocieri,
            ([total] * [nc]) - Sum(Nz([suma], 0)) AS DeAsociat,
            EFT.Tip
        FROM
            (
                EF
                INNER JOIN EFT ON EF.id_sol = EFT.id_sol
            )
            LEFT JOIN EFT_O ON EFT.IDEFT = EFT_O.IDEFT
        GROUP BY
            EF.IDEF,
            EFT.IDEFT,
            EF.id_sol,
            EF.id,
            EFT.NrFact,
            EFT.DataFact,
            EFT.CotaTVA,
            [tva] * [nc],
            [Valoare] * [nc],
            [TOTAL] * [nc],
            EFT.DenumireP,
            Len([Attach]) <> 0,
            Len([xml]) <> 0,
            False,
            0,
            EFT.Tip,
            EFT.CUI,
            EF.cui_unit
        HAVING
            (
                ((EFT.IDEFT) <> Nz([Forms]![Note]![IDEFT],0))
                AND ((([total] * [nc]) - Sum(Nz([suma], 0))) <> 0)
                AND (
                    (EFT.CUI) In (
                        SELECT
                            CodFiscal
                        FROM
                            Parteneri
                        WHERE
                            CodPartener = forms!note!Part
                    )
                )
                AND ((EF.cui_unit) = DLookUp("CUI", "Unit"))
            )
    )  AS T;
```

### `qPlatiParteneri2015` (Select)

```sql
SELECT Documente_1.IdDocument
FROM (((PlatiFacturi INNER JOIN (Documente INNER JOIN Oper ON Documente.IdDocument = Oper.IdDocument) ON PlatiFacturi.IdFactura = Oper.IdOperatie) INNER JOIN Credit ON Oper.IdOperatie = Credit.IdOperatie) INNER JOIN Oper AS Oper_1 ON PlatiFacturi.IdPlata = Oper_1.IdOperatie) INNER JOIN Documente AS Documente_1 ON Oper_1.IdDocument = Documente_1.IdDocument
WHERE (((Credit.CodPartener) Like forms!parteneri2015!cp) And ((Documente.IdDocument)=forms!parteneri2015!nrdc))
GROUP BY Documente_1.IdDocument;
```
