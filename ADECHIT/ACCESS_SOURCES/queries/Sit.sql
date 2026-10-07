-- Query: Sit
-- Type: Select

SELECT Grupe.IDG, Platitori.IDP, LunaD.IDL, Prezenta.IDZ, LunaD.Luna, LunaD.Anul, Grupe.Grupa, Grupe.Educator, Platitori.Nume, Platitori.CNP, Prezenta.ZilePrezenta, Prezenta.ValoareContract, Prezenta.ValoareTotala, Sum(Plati.Plata) AS Plata, Sum(Plati.Restanta) AS Restanta, IIf([prezenta]![Restanta]<0,Abs([prezenta]![restanta]),0) AS Compensare, Sum(Plati.Anticipat) AS Anticipat
FROM ((Grupe INNER JOIN Platitori ON Grupe.IDG = Platitori.IDG) INNER JOIN (LunaD INNER JOIN Prezenta ON LunaD.IDL = Prezenta.IDL) ON Platitori.IDP = Prezenta.IDP) LEFT JOIN Plati ON Prezenta.IDZ = Plati.IDZ
GROUP BY Grupe.IDG, Platitori.IDP, LunaD.IDL, Prezenta.IDZ, LunaD.Luna, LunaD.Anul, Grupe.Grupa, Grupe.Educator, Platitori.Nume, Platitori.CNP, Prezenta.ZilePrezenta, Prezenta.ValoareContract, Prezenta.ValoareTotala, IIf([prezenta]![Restanta]<0,Abs([prezenta]![restanta]),0)
HAVING (((LunaD.IDL) Like [forms]![Prezenta]![IDL]))
ORDER BY Platitori.Nume, Platitori.CNP;
