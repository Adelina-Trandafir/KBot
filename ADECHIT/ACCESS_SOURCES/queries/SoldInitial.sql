-- Query: SoldInitial
-- Type: Select

SELECT X.IDP, Sum([d])-Sum([c]) AS SI
FROM (SELECT Prezenta.IDP, Sum(Prezenta.ValoareTotala) AS D, Sum(0) AS C
FROM LunaD INNER JOIN Prezenta ON LunaD.IDL = Prezenta.IDL
WHERE (((dhLastDayInMonth(CDate("01/" & [luna] & "/" & [anul])))<dhLastDayInMonth(CDate("01/" & [forms]![prezenta]![luni]![luna] & "/" & [forms]![prezenta]![luni]![anul]))))
GROUP BY Prezenta.IDP

UNION ALL SELECT Plati.IDP, Sum(0) AS D, Sum(Nz([plata],0)) AS C
FROM LunaD INNER JOIN Plati ON LunaD.IDL = Plati.IDL
WHERE (((dhLastDayInMonth(CDate("01/" & [luna] & "/" & [anul])))<dhLastDayInMonth(CDate("01/" & [forms]![prezenta]![luni]![luna] & "/" & [forms]![prezenta]![luni]![anul]))) AND ((Plati.Anulata)=False))
GROUP BY Plati.IDP
)  AS X
GROUP BY X.IDP;
