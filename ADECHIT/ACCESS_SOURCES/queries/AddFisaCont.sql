-- Query: AddFisaCont
-- Type: Append

INSERT INTO FisaCont ( IDP, Data, Expl, Zile, DEBIT, CREDIT, Tip )
SELECT X.IDP, X.Data, X.Expl, X.Zile, Sum(X.DEBIT) AS SumOfDEBIT, Sum(X.CREDIT) AS SumOfCREDIT, X.P
FROM (SELECT Prezenta.IDP, dhlastdayinmonth(CDate("01/" & [luna] & "/" & [anul])) AS Data, "Prezență " & lunat & "/" & anul AS Expl, Prezenta.ZilePrezenta AS Zile, Prezenta.Valoarecontract AS DEBIT, 0 AS CREDIT, "Z" as P
FROM LunaD INNER JOIN Prezenta ON LunaD.IDL = Prezenta.IDL
WHERE Prezenta.IDP Like [tempvars]![idp]

UNION ALL 

SELECT Platitori.IDP, #1/1/2001# AS Data, "Sold Inițial" AS Expl, 0 AS Zile, CDbl(IIf([si]>0,[si],0)) AS DEBIT, CDbl(IIf([si]<0,Abs([si]),0)) AS CREDIT, "S" AS P
FROM Platitori
WHERE Platitori.IDP Like [tempvars]![idp]

UNION ALL 

SELECT Plati.IDP, Plati.Data, Switch([tip]="0",[feldoc] & " " & [nrdoc],[tip]="1","Bon Fiscal " & [nrbf],[tip]="2","Chitanță " & [serie] & "/" & [numar]) AS Expl, 0 AS Expr1, 0 AS DEBIT, Plati.PLATA AS CREDIT, "P" AS P
FROM (((LunaD INNER JOIN Plati ON LunaD.IDL = Plati.IDL) LEFT JOIN AlteDoc ON Plati.IDPL = AlteDoc.IDPL) LEFT JOIN BonuriF ON Plati.IDPL = BonuriF.IDPL) LEFT JOIN Chitante ON Plati.IDPL = Chitante.IDPL
WHERE Plati.IDP Like [tempvars]![idp] AND Plati.Anulata=False

UNION ALL 

SELECT Retur.IDP, Retur.Data, "Restituire: " & [nrdoc] & "/" & [explicatie] AS Expl, 0 AS Expr1, Retur.suma AS DEBIT, 0 AS CREDIT, "R" AS P
FROM LunaD INNER JOIN Retur ON LunaD.IDL = Retur.IDL
WHERE Retur.IDP Like [tempvars]![idp] AND Retur.Anulat=False

UNION ALL 

SELECT MutaCopil.IDP, MutaCopil.data AS Data, "Transfer de la " & [grupe_1]![grupa] & " la " & [grupe]![grupa] AS Expl, 0 AS Zile, 0 AS DEBIT, 0 AS CREDIT, "T" AS P
FROM (MutaCopil INNER JOIN Grupe ON MutaCopil.IDG_N = Grupe.IDG) INNER JOIN Grupe AS Grupe_1 ON MutaCopil.IDG_V = Grupe_1.IDG
WHERE MutaCopil.IDP Like [tempvars]![idp]

)  AS X
GROUP BY X.IDP, X.Data, X.Expl, X.Zile, X.P
HAVING (((X.Data)<=dhLastDayInMonth(CDate("01/" & [forms]![prezenta]![luna] & "/" & [forms]![prezenta]![anul]))))
ORDER BY X.IDP, X.Data;
