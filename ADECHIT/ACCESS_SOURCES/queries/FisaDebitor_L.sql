-- Query: FisaDebitor_L
-- Type: Select

SELECT Prezenta.IDZ, Prezenta.IDP, [lunat] & "/" & [anul] AS Luna, Prezenta.ZilePrezenta, Prezenta.ZileAbsenta, Prezenta.MZCPrezenta, Prezenta.MZCAbsenta, Prezenta.ValoareContract, Prezenta.ValoareMancare, Prezenta.ValoareTotala, Prezenta.ReducereFrate, Prezenta.Avans, Prezenta.Detalii, Sum(Nz([plata],0)) AS Platit, IIf(Len([lunad]![luna])=1,"0","") & [lunad]![luna] & [anul] AS LA
FROM (LunaD INNER JOIN Prezenta ON LunaD.IDL = Prezenta.IDL) LEFT JOIN Plati ON Prezenta.IDZ = Plati.IDZ
GROUP BY Prezenta.IDZ, Prezenta.IDP, [lunat] & "/" & [anul], Prezenta.ZilePrezenta, Prezenta.ZileAbsenta, Prezenta.MZCPrezenta, Prezenta.MZCAbsenta, Prezenta.ValoareContract, Prezenta.ValoareMancare, Prezenta.ValoareTotala, Prezenta.ReducereFrate, Prezenta.Avans, Prezenta.Detalii, dhLastDayInMonth(CDate("01/" & [luna] & "/" & [anul])), IIf(Len([lunad]![luna])=1,"0","") & [lunad]![luna] & [anul]
HAVING (((dhLastDayInMonth(CDate("01/" & [luna] & "/" & [anul])))<=dhLastDayInMonth(CDate("01/" & [forms]![prezenta]![luni]![luna] & "/" & [forms]![prezenta]![luni]![anul]))));
