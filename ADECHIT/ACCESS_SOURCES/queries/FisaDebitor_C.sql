-- Query: FisaDebitor_C
-- Type: Select

SELECT X.IDP, X.IDZ, X.Data, X.Plata, X.TIP, X.NrDoc, X.DataDoc, X.Detalii, X.Listat, X.Corect
FROM (SELECT Prezenta.IDP, Prezenta.IDZ, Plati.Data, Plati.Plata, Plati.TIP, BonuriF.NrBF AS NrDoc, BonuriF.DataBF AS DataDoc, "Bon fiscal" AS Detalii, BonuriF.Listat, BonuriF.Corect
FROM ((LunaD INNER JOIN Prezenta ON LunaD.IDL = Prezenta.IDL) INNER JOIN Plati ON Prezenta.IDZ = Plati.IDZ) INNER JOIN BonuriF ON Plati.IDPL = BonuriF.IDPL
WHERE dhLastDayInMonth(CDate("01/" & [luna] & "/" & [anul]))<=dhLastDayInMonth(CDate("01/" & [forms]![prezenta]![luni]![luna] & "/" & [forms]![prezenta]![luni]![anul]))

UNION ALL SELECT Prezenta.IDP, Prezenta.IDZ, Plati.Data, Plati.Plata, Plati.TIP, AlteDoc.NrDoc, AlteDoc.DataDoc, AlteDoc.[feldoc] AS Detalii, -1 AS Expr1, -1 AS Expr2
FROM ((LunaD INNER JOIN Prezenta ON LunaD.IDL = Prezenta.IDL) INNER JOIN Plati ON Prezenta.IDZ = Plati.IDZ) INNER JOIN AlteDoc ON Plati.IDPL = AlteDoc.IDPL
WHERE dhLastDayInMonth(CDate("01/" & [luna] & "/" & [anul]))<=dhLastDayInMonth(CDate("01/" & [forms]![prezenta]![luni]![luna] & "/" & [forms]![prezenta]![luni]![anul]))

)  AS X
ORDER BY X.Data;
